# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

CodeHaks **TotalLength** — an AutoCAD .NET add-on that computes the total length of selected lines, polylines, and arcs. The single in-AutoCAD command is `ZLen`, which opens a WPF window for selection and length reporting.

## Build

The solution produces a `codehaks.TotalLength.dll` per AutoCAD target, loaded into AutoCAD via `NETLOAD`. Each target links against AutoCAD's managed APIs (`accoremgd`, `acdbmgd`, `acmgd`, `AcWindows`, `AdWindows`).

Target frameworks differ by AutoCAD release:

- **TotalLength 2024** → `.NET Framework 4.8` (old-style csproj).
- **TotalLength 2025** → `net8.0-windows` (SDK-style csproj, `<UseWPF>true</UseWPF>`).
- **TotalLength 2026** → `net8.0-windows` (SDK-style csproj, `<UseWPF>true</UseWPF>`).

```sh
# Build all AutoCAD versions and collect DLLs into .\dist\AutoCAD <year>\
build.bat                # Release (default)
build.bat Debug          # Debug

# Or invoke msbuild directly
msbuild Acad-TotalLength.sln /p:Configuration=Release

# Build a single AutoCAD target
msbuild "src/TotalLength 2024/TotalLength 2024.csproj" /p:Configuration=Release
msbuild "src/TotalLength 2025/TotalLength 2025.csproj" /p:Configuration=Release
msbuild "src/TotalLength 2026/TotalLength 2026.csproj" /p:Configuration=Release
```

References use `HintPath`s pointing to `C:\Program Files\Autodesk\AutoCAD 20XX\*.dll`. The 2024 csproj uses a relative path, while the 2025/2026 csprojs use absolute `C:\Program Files\Autodesk\AutoCAD 20XX\` paths. Builds will fail unless the matching AutoCAD version is installed at the standard location, or hint paths are adjusted.

There is no test project, no linter configuration, and no package manager — all dependencies are GAC/AutoCAD assemblies referenced directly.

## Architecture

The solution uses a **shared-project multi-targeting** pattern (not multi-targeting via TFM, but via separate csproj files):

- `src/Shared/` — `Shared.shproj` containing all source code (commands, view models, views, services, ribbon registration). Shared projects compile their files into each referencing project's assembly.
- `src/TotalLength 2024/` — thin csproj that imports `Shared.projitems` and references AutoCAD 2024 assemblies (`.NET Framework 4.8`, old-style csproj).
- `src/TotalLength 2025/` — same, for AutoCAD 2025 (`net8.0-windows`, SDK-style csproj).
- `src/TotalLength 2026/` — same, for AutoCAD 2026 (`net8.0-windows`, SDK-style csproj).

All output assemblies are named `codehaks.TotalLength.dll`. When changing functionality, edit files under `src/Shared/`; the per-version csprojs only differ in their target framework, AutoCAD `Reference` hint paths, and `Properties/`.

### Code layout (in `src/Shared/`)

- `AcadApplication.cs` — `IExtensionApplication` entry point; registers ribbons on AutoCAD startup.
- `AcadCommand.cs` — declares the `[CommandMethod("ZLen")]` that launches `MainWindow` as a modal dialog parented to AutoCAD's main window.
- `Application/SelectService.cs` — wraps AutoCAD selection APIs.
- `Core/SelectOptions.cs` — selection filter / object-type options.
- `Commands/`, `Common/RelayCommand.cs` — WPF MVVM command plumbing.
- `Presentation/Ribbons/` — programmatic ribbon tab/panel construction.
- `Presentation/ViewModels/MainViewModel.cs`, `Presentation/Views/MainWindow.xaml(.cs)` — WPF UI.

The root namespace is `MyApp` (note: assembly name is `codehaks.TotalLength`, but the C# namespace is `MyApp.*`).

### WPF + AutoCAD interop notes

- `MainWindow` is shown via `ShowDialog()` with its owner set through `WindowInteropHelper` to `Application.MainWindow.Handle` so it stays modal to AutoCAD.
- Ribbon registration happens in `RibbonManager.AddRibbons()` from `IExtensionApplication.Initialize`, which runs once per AutoCAD session after `NETLOAD`.

## Conventions

- C# `<LangVersion>latest</LangVersion>` with `<Nullable>enable</Nullable>` and file-scoped namespaces.
- Platform target is `x64` (Debug); AutoCAD only loads 64-bit assemblies.
