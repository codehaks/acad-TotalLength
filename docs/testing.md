# Testing Guide

The repo currently ships **no automated tests**. This document is the recipe for adding them — both pure-.NET unit tests and AutoCAD-integration tests — without disturbing the existing multi-target shared-project pattern.

## Strategy

The codebase has three categories of code, each with a different testing strategy:

| Category | Examples | How to test |
| :--- | :--- | :--- |
| **Pure .NET** — no AutoCAD types | `SelectOptions`, `RelayCommand<T>`, view-model logic that doesn't call services | xUnit unit tests in a separate test project. Fast, run on every commit. |
| **AutoCAD interop wrappers** | `SelectService` | Hide `Editor` / `Document` / `Database` behind an interface; inject a fake in unit tests; run a thin smoke test against real AutoCAD. |
| **AutoCAD-hosted commands** | `[CommandMethod("ZLen")]`, `IExtensionApplication` | In-process integration tests loaded via `NETLOAD` against a real AutoCAD using the `accoreconsole.exe` headless runner. |

We'll set up infrastructure for all three.

---

## Step 1 — Add a unit-test project (xUnit)

Tests target **`net8.0-windows`** (matches AutoCAD 2025/2026; lets us test WPF VM code if needed). Path: `tests/TotalLength.Tests/`.

### Create the project

```bash
dotnet new xunit -n TotalLength.Tests -o tests/TotalLength.Tests --framework net8.0
```

Edit `tests/TotalLength.Tests/TotalLength.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <UseWPF>true</UseWPF>
    <IsPackable>false</IsPackable>
    <RootNamespace>TotalLength.Tests</RootNamespace>
  </PropertyGroup>

  <Import Project="..\..\src\Shared\Shared.projitems" Label="Shared" />

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="FluentAssertions" Version="6.12.1" />
    <PackageReference Include="NSubstitute" Version="5.1.0" />
  </ItemGroup>

  <!-- AutoCAD references — needed because Shared.projitems uses Autodesk types.
       Pull from the 2025 install path (any .NET 8-compatible release works). -->
  <ItemGroup>
    <Reference Include="accoremgd"><HintPath>C:\Program Files\Autodesk\AutoCAD 2025\accoremgd.dll</HintPath><Private>False</Private></Reference>
    <Reference Include="Acdbmgd"><HintPath>C:\Program Files\Autodesk\AutoCAD 2025\acdbmgd.dll</HintPath><Private>False</Private></Reference>
    <Reference Include="Acmgd"><HintPath>C:\Program Files\Autodesk\AutoCAD 2025\acmgd.dll</HintPath><Private>False</Private></Reference>
    <Reference Include="AcWindows"><HintPath>C:\Program Files\Autodesk\AutoCAD 2025\AcWindows.dll</HintPath><Private>False</Private></Reference>
    <Reference Include="AdWindows"><HintPath>C:\Program Files\Autodesk\AutoCAD 2025\AdWindows.dll</HintPath><Private>False</Private></Reference>
  </ItemGroup>
</Project>
```

> **Why import `Shared.projitems` instead of referencing a built DLL?** The shared project compiles into the test assembly directly. Tests get to reach into `internal` types, and there's no DLL-version mismatch.

> **Why still link AutoCAD assemblies?** The Shared sources reference `Autodesk.AutoCAD.*` types (e.g. in `SelectService`). Even pure tests need those types resolvable at compile time. They never load at test-runtime as long as the test code doesn't `new` any AutoCAD type.

Add the test project to the solution:

```bash
dotnet sln Acad-TotalLength.sln add tests/TotalLength.Tests/TotalLength.Tests.csproj
```

### Write your first test

`tests/TotalLength.Tests/Core/SelectOptionsTests.cs`:

```csharp
using FluentAssertions;
using MyApp.Core;
using Xunit;

namespace TotalLength.Tests.Core;

public class SelectOptionsTests
{
    [Fact]
    public void ToString_AllTypesSelected_BuildsCsvFilter()
    {
        var options = new SelectOptions
        {
            SelectLines = true,
            SelectPolyLines = true,
            SelectArcs = true,
        };

        options.ToString().Should().Be("LINE,LWPOLYLINE,ARC");
    }

    [Fact]
    public void ToString_NoTypesSelected_ReturnsEmpty()
    {
        new SelectOptions().ToString().Should().BeEmpty();
    }

    [Theory]
    [InlineData(true, false, false, "LINE")]
    [InlineData(false, true, false, "LWPOLYLINE")]
    [InlineData(false, false, true, "ARC")]
    [InlineData(true, false, true, "LINE,ARC")]
    public void ToString_SubsetSelected_OnlyIncludesSelected(
        bool lines, bool polys, bool arcs, string expected)
    {
        var options = new SelectOptions
        {
            SelectLines = lines,
            SelectPolyLines = polys,
            SelectArcs = arcs,
        };

        options.ToString().Should().Be(expected);
    }
}
```

Run:

```bash
dotnet test tests/TotalLength.Tests/TotalLength.Tests.csproj
```

---

## Step 2 — Make `SelectService` testable

`SelectService` currently calls `Application.DocumentManager.MdiActiveDocument` directly, which makes it untestable in isolation. Refactor in two passes:

### 2a. Introduce a thin abstraction

`src/Shared/Application/IAcadEditor.cs`:

```csharp
namespace MyApp.Services;

public interface IAcadEditor
{
    PromptSelectionResult GetSelection(SelectionFilter filter);
    Transaction StartTransaction();
}
```

Provide a real implementation `AcadEditor` that wraps the live document, and use it in `MainViewModel`:

```csharp
public MainViewModel() : this(new AcadEditor()) { }
public MainViewModel(IAcadEditor editor) { _selectService = new SelectService(editor); ... }
```

### 2b. Test the math, not AutoCAD

`SelectService.SelectObjects` boils down to: *given a list of `Curve`s and the result of a selection prompt, sum lengths*. Extract that math:

```csharp
internal static double SumCurveLengths(IEnumerable<Curve> curves) =>
    curves.Sum(c => c.GetDistanceAtParameter(c.EndParam) - c.GetDistanceAtParameter(c.StartParam));
```

Tests can construct AutoCAD `Line` / `Arc` objects in memory (they don't need a `Database` for simple geometry) and verify the sum. For more elaborate cases — polylines with bulges, splines — drop down to the integration test layer.

> If extracting the math gets too entangled with `Transaction` semantics, prefer skipping unit tests for `SelectService` and rely on integration tests instead. **Don't fight the framework.**

---

## Step 3 — Integration tests against AutoCAD

AutoCAD ships a console host: `accoreconsole.exe`. It's a stripped-down AutoCAD that runs LISP / .NET commands without showing the UI — perfect for CI smoke tests.

### Project layout

`tests/TotalLength.IntegrationTests/` — a class library that:

1. Imports `Shared.projitems` (so commands are available).
2. Defines a parallel set of `[CommandMethod]` test commands that drive the production code and write results to a sentinel file.
3. Is `NETLOAD`-ed by an `accoreconsole.exe` invocation, which then runs the test commands via a script.

Skeleton csproj — same shape as the per-version csproj, target `net8.0-windows`, AutoCAD 2025/2026 references.

### Sample test command

```csharp
using System.IO;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace MyApp.IntegrationTests;

public class LengthMathTests
{
    [CommandMethod("TL_TEST_ARC_LENGTH")]
    public void Arc_LengthMatchesGeometricFormula()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        using var trans = doc.TransactionManager.StartTransaction();
        var ms = (BlockTableRecord)trans.GetObject(
            ((BlockTable)trans.GetObject(doc.Database.BlockTableId, OpenMode.ForRead))
                [BlockTableRecord.ModelSpace],
            OpenMode.ForWrite);

        // Quarter circle, radius 10. Expected arc length: π/2 * 10 ≈ 15.708.
        var arc = new Arc(Point3d.Origin, 10.0, 0.0, System.Math.PI / 2);
        ms.AppendEntity(arc);
        trans.AddNewlyCreatedDBObject(arc, true);

        var length = arc.GetDistanceAtParameter(arc.EndParam)
                   - arc.GetDistanceAtParameter(arc.StartParam);

        File.WriteAllText(
            Path.Combine(Path.GetTempPath(), "tl_test_arc_length.txt"),
            length.ToString("F6"));

        trans.Commit();
    }
}
```

### Driver script

`tests/run-integration.scr` (an AutoCAD script file):

```text
NETLOAD
"D:\path\to\TotalLength.IntegrationTests.dll"
TL_TEST_ARC_LENGTH
QUIT
Y
```

Driver `tests/run-integration.ps1`:

```powershell
$acad = "C:\Program Files\Autodesk\AutoCAD 2026\accoreconsole.exe"
$drawing = "$PSScriptRoot\fixtures\empty.dwg"
$script = "$PSScriptRoot\run-integration.scr"

& $acad /i $drawing /s $script

$expected = [math]::PI / 2 * 10
$actual   = [double](Get-Content "$env:TEMP\tl_test_arc_length.txt")
if ([math]::Abs($expected - $actual) -gt 0.001) {
    throw "Arc length mismatch. Expected $expected, got $actual."
}
Write-Host "Integration tests passed." -ForegroundColor Green
```

`accoreconsole.exe` exits with the result of the final command. The script writes results to disk; the PowerShell driver verifies them and surfaces non-zero exit codes for CI.

### What to integration-test

- **Length math on real entities** with known geometry (a unit-length line, a quarter-circle arc, a polyline with a bulge).
- **Filter behavior** — selecting in a drawing populated with mixed entity types and verifying only the requested ones contribute.
- **Empty-selection paths** — confirm the error message and zero totals.
- **Transaction safety** — run two commands back-to-back; verify no entities leak into the drawing.

### What *not* to integration-test

- The WPF window's keyboard shortcuts, layout, or visual tree. That's a UI test, not an AutoCAD test, and it doesn't need AutoCAD running. Use a UI test framework like FlaUI if it ever becomes worth it.
- Ribbon registration. It's a one-liner and the failure mode is "tab doesn't appear" — caught by manual smoke testing.

---

## CI considerations

Integration tests need an AutoCAD installation and so cannot run on stock GitHub-hosted runners. Options:

1. **Self-hosted Windows runner** with one or more AutoCAD versions installed. Tag the runner with `autocad-2026` and gate the integration job on the tag.
2. **Skip integration tests in CI**, run only unit tests, and run integration tests as a manual checklist before each release. Pragmatic for a small team.

Unit tests run anywhere as long as the AutoCAD assemblies referenced in the test csproj are accessible — for true CI portability, copy `accoremgd.dll`, `acdbmgd.dll`, `acmgd.dll`, `AcWindows.dll`, `AdWindows.dll` into a `lib/` folder and rewrite the test csproj's hint paths to point at it. Note: redistributing AutoCAD's managed assemblies requires an Autodesk redistribution agreement. Until that exists, run unit tests on a developer box or self-hosted runner.

## Suggested progression

1. Add the unit-test project and write tests for `SelectOptions` (zero AutoCAD coupling — easy win).
2. Tests for `RelayCommand<T>` — one-line classes with one-line tests.
3. Tests for `MainViewModel` startup state, defaults, command wiring (use NSubstitute for `IAcadEditor`).
4. Once `SelectService` is refactored against `IAcadEditor`, add tests with a fake editor.
5. Bring up the integration project and start with the arc-length sanity check above.

The goal is **a tight unit-test loop for the parts that change often (VMs, options, math)** plus **a thin integration smoke layer** that catches AutoCAD interop regressions before release.
