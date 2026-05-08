# Architecture

This document explains how **TotalLength** is laid out and how the pieces fit together. Reading this before making changes will save you time.

## High-level overview

TotalLength is a single AutoCAD .NET add-on that ships as `codehaks.TotalLength.dll`. The same source code is compiled three times — once per supported AutoCAD release — using a **shared project** (`.shproj`) imported by three thin per-version csprojs.

```text
                  ┌────────────────────────────┐
                  │   src/Shared/Shared.shproj │
                  │   (all source + XAML)      │
                  └─────────────┬──────────────┘
                                │ projitems import
        ┌───────────────────────┼───────────────────────┐
        ▼                       ▼                       ▼
┌────────────────┐     ┌────────────────┐     ┌────────────────┐
│ TotalLength    │     │ TotalLength    │     │ TotalLength    │
│ 2024.csproj    │     │ 2025.csproj    │     │ 2026.csproj    │
│ net48          │     │ net8.0-windows │     │ net8.0-windows │
│ AutoCAD 2024   │     │ AutoCAD 2025   │     │ AutoCAD 2026   │
└────────────────┘     └────────────────┘     └────────────────┘
        │                       │                       │
        └────────► codehaks.TotalLength.dll ◄───────────┘
                  (one binary per version)
```

## Target frameworks

| AutoCAD | Project | Target framework | Project format |
| :--- | :--- | :--- | :--- |
| 2024 | [src/TotalLength 2024/](../src/TotalLength%202024/) | `.NET Framework 4.8` | Old-style `.csproj` with `Microsoft.CSharp.targets` |
| 2025 | [src/TotalLength 2025/](../src/TotalLength%202025/) | `net8.0-windows` | SDK-style (`Microsoft.NET.Sdk`, `<UseWPF>true</UseWPF>`) |
| 2026 | [src/TotalLength 2026/](../src/TotalLength%202026/) | `net8.0-windows` | SDK-style (`Microsoft.NET.Sdk`, `<UseWPF>true</UseWPF>`) |

The 2024 / 2025 split mirrors the AutoCAD .NET runtime change: AutoCAD 2024 still hosts .NET Framework 4.8; AutoCAD 2025+ hosts .NET 8 (Core).

> **Looking ahead:** AutoCAD 2027 will require .NET 10. When that target is added, the pattern repeats — clone the 2025/2026 csproj, change `<TargetFramework>` to `net10.0-windows`, point hint paths at `AutoCAD 2027`.

## Layering

All shared source lives under [src/Shared/](../src/Shared/). The codebase follows a light layered structure:

```text
src/Shared/
├── AcadApplication.cs          ← IExtensionApplication entry (registers ribbon)
├── AcadCommand.cs              ← [CommandMethod("ZLen")] — opens the WPF window
│
├── Application/
│   └── SelectService.cs        ← AutoCAD selection + length math
│
├── Core/
│   └── SelectOptions.cs        ← user-toggled object-type filter (DTO)
│
├── Common/
│   └── RelayCommand.cs         ← MVVM ICommand<T> implementation
│
├── Commands/
│   └── DrawCircleCommand.cs    ← DEBUG-only smoke test command
│
└── Presentation/
    ├── Ribbons/
    │   ├── RibbonManager.cs        ← builds the "Codehaks" tab + button
    │   └── RibbonMainCommand.cs    ← ribbon button → SendStringToExecute("ZLen")
    ├── ViewModels/
    │   └── MainViewModel.cs        ← orchestrates SelectService, exposes commands
    └── Views/
        ├── MainWindow.xaml
        └── MainWindow.xaml.cs
```

### Responsibilities

- **`AcadApplication`** — implements `IExtensionApplication`. Runs on every AutoCAD session start (after `NETLOAD`); calls `RibbonManager.AddRibbons()`.
- **`AcadCommand.ShowMainWindow`** — the `ZLen` command. Constructs `MainWindow`, parents it to the AutoCAD main window via `WindowInteropHelper`, calls `ShowDialog()`.
- **`SelectService.SelectObjects`** — wraps `editor.GetSelection(filter)`, walks the resulting `ObjectIdCollection` inside a transaction, and sums each `Curve`'s length using `GetDistanceAtParameter(EndParam) - GetDistanceAtParameter(StartParam)`. Returns `(SelectedCount, TotalLength, Message)`.
- **`SelectOptions`** — POCO carrying three booleans (lines / polylines / arcs). Its `ToString()` produces a comma-separated DXF type filter (e.g. `"LINE,LWPOLYLINE,ARC"`).
- **`MainViewModel`** — wires `SelectCommand`, exposes `TotalLength`, `SelectedObjectCount`, `StatusMessage`, `Title`.
- **`RelayCommand<T>`** — generic MVVM command; `CanExecuteChanged` is hooked to `CommandManager.RequerySuggested`.
- **`RibbonManager` / `RibbonMainCommand`** — programmatically build a `RibbonTab` + `RibbonPanel` + `RibbonButton`. The button's command issues `SendStringToExecute("Zlen ", true, false, false)` which routes through AutoCAD's command queue back into `AcadCommand.ShowMainWindow`.

### Namespace note

The C# root namespace is `MyApp.*` (e.g. `MyApp.Services`, `MyApp.Presentation.ViewModels`). The assembly name is `codehaks.TotalLength`. Don't rely on the namespace matching the assembly name — the WPF `pack://` URIs intentionally reference the assembly name (`pack://application:,,,/codehaks.TotalLength;component/Resources/large.png`).

## AutoCAD interop notes

- **Window ownership.** `MainWindow` is always parented to `Application.MainWindow.Handle` via `WindowInteropHelper`. This keeps the dialog modal to AutoCAD, gives it correct z-order, and prevents focus-stealing oddities.
- **Modal vs modeless.** The current dialog is modal (`ShowDialog`). If you switch to `Show()` for a modeless palette, you must also use `Application.ShowModelessWindow` or the AutoCAD `PaletteSet` API, not raw WPF.
- **Transactions.** Every read of an `ObjectId` must happen inside a `Transaction`. `SelectService` uses a single short-lived transaction over the whole selection.
- **Ribbon assemblies.** Ribbon types come from `AcWindows.dll` and `AdWindows.dll` (note: `Autodesk.Windows.RibbonControl`, **not** `System.Windows.Controls.Ribbon`). Both versions of the assemblies are referenced from `C:\Program Files\Autodesk\AutoCAD <year>\`.

## Build-time mechanics

- All three csprojs `<Import Project="..\Shared\Shared.projitems" Label="Shared" />`. The shared project compiles its files *into* each consumer assembly — there is no separate `Shared.dll`.
- `Shared.projitems` lists every file once with `$(MSBuildThisFileDirectory)…` so it's neutral to the consumer's location.
- The 2024 csproj uses old-style `<Reference Include="…">` items with `<HintPath>` for both AutoCAD assemblies *and* WPF (`PresentationCore`, `PresentationFramework`, `WindowsBase`, `System.Xaml`). The 2025/2026 csprojs replace those WPF references with a single `<UseWPF>true</UseWPF>` property.
- `<Private>False</Private>` on every AutoCAD reference prevents AutoCAD's own DLLs from being copied into the output. They're loaded by AutoCAD itself at runtime.

## Conditional compilation

The codebase deliberately avoids `ACAD20XX` symbols. The only `#if` directives are:

- `#if DEBUG` in [DrawCircleCommand.cs](../src/Shared/Commands/DrawCircleCommand.cs) to register the debug-only `DrawCircle` command.
- `#if DEBUG` in [MainViewModel.cs](../src/Shared/Presentation/ViewModels/MainViewModel.cs) to suffix the title bar with a timestamp.

If you find yourself adding `#if ACAD2024`, stop and ask whether the AutoCAD API you're calling is actually different across versions — most of the managed surface used here is stable.
