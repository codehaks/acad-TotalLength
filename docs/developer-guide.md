# Developer Guide

Everything you need to be productive day-to-day on **CodeHaks TotalLength**. Pair this with [architecture.md](architecture.md) for the design rationale and [build-and-load.md](build-and-load.md) for the build matrix.

## Getting set up

1. Clone the repo and open `Acad-TotalLength.sln` in Visual Studio 2022 (17.8+).
2. Visual Studio will prompt to install missing components (.NET 8 SDK, .NET Framework 4.8 targeting pack) — accept.
3. Install at least one supported AutoCAD release (2024 / 2025 / 2026) at the standard path. You can target only the version(s) you have — unload the others from the solution if needed.
4. From the **Build** menu, run *Build Solution*. All three projects should build, modulo any AutoCAD versions you don't have installed.

## Daily loop

```text
edit Shared/* → build target csproj → close AutoCAD → start AutoCAD → NETLOAD → ZLen → debug → repeat
```

Two productivity-killers and how to avoid them:

- **AutoCAD locks the loaded DLL.** You must close *all* AutoCAD instances before MSBuild can overwrite `codehaks.TotalLength.dll`. Use Task Manager to confirm `acad.exe` has fully exited.
- **`NETLOAD` is per-session.** AutoCAD does not reload your DLL automatically. After a rebuild, restart AutoCAD and re-`NETLOAD`. (Or wire the *Startup Suite* via `APPLOAD` — see [build-and-load.md](build-and-load.md#auto-load-on-autocad-startup-optional).)

## Debugging in AutoCAD

To set breakpoints in `MainViewModel`, `SelectService`, etc.:

1. Build the matching csproj in **Debug** configuration.
2. Set the per-version csproj as the *Startup Project*.
3. In *Project Properties → Debug → Open debug launch profiles UI*, configure:
   - **Launch:** Executable
   - **Executable:** `C:\Program Files\Autodesk\AutoCAD <year>\acad.exe`
   - **Working directory:** `C:\Program Files\Autodesk\AutoCAD <year>\`
4. Press <kbd>F5</kbd>. Visual Studio launches AutoCAD and attaches its debugger.
5. Inside AutoCAD, run `NETLOAD` against `src\TotalLength <year>\bin\Debug\…\codehaks.TotalLength.dll`. Breakpoints become active once the DLL loads.

If you didn't pre-configure a launch profile, use **Debug → Attach to Process…** and pick `acad.exe` after AutoCAD is already running.

### Debug-only commands and UI

- The `DrawCircle` command is gated by `#if DEBUG` and only registers in Debug builds. Use it as a smoke-test that interop is working.
- The `MainWindow` title appends an `HH-mm-ss` timestamp in Debug, which is useful for confirming you're testing the freshly-built DLL and not a cached one.

## Code map

| You want to change… | Edit… |
| :--- | :--- |
| The `ZLen` command body | [src/Shared/AcadCommand.cs](../src/Shared/AcadCommand.cs) |
| What entity types can be selected | [src/Shared/Core/SelectOptions.cs](../src/Shared/Core/SelectOptions.cs) and the checkboxes in [MainWindow.xaml](../src/Shared/Presentation/Views/MainWindow.xaml) |
| Length math / selection flow | [src/Shared/Application/SelectService.cs](../src/Shared/Application/SelectService.cs) |
| The dialog layout | [src/Shared/Presentation/Views/MainWindow.xaml](../src/Shared/Presentation/Views/MainWindow.xaml) |
| Buttons, status, observable state | [src/Shared/Presentation/ViewModels/MainViewModel.cs](../src/Shared/Presentation/ViewModels/MainViewModel.cs) |
| Ribbon tab / panel / button | [src/Shared/Presentation/Ribbons/RibbonManager.cs](../src/Shared/Presentation/Ribbons/RibbonManager.cs) |
| What the ribbon button does | [src/Shared/Presentation/Ribbons/RibbonMainCommand.cs](../src/Shared/Presentation/Ribbons/RibbonMainCommand.cs) |
| AutoCAD startup behavior | [src/Shared/AcadApplication.cs](../src/Shared/AcadApplication.cs) |
| Add a brand-new command | New file under `src/Shared/Commands/`, then verify it's listed in [Shared.projitems](../src/Shared/Shared.projitems) |
| Add a new resource (icon, etc.) | Drop into `src/Shared/Resources/` and add a `<Resource Include="…">` line to `Shared.projitems` |

## Adding a new file

The shared project requires every file to be listed in [Shared.projitems](../src/Shared/Shared.projitems). Visual Studio normally adds it automatically when you *Add → New Item* with the Shared project active in the solution explorer. If you create the file manually (or via another IDE), add the appropriate `<Compile Include="…">`, `<Page Include="…">`, or `<Resource Include="…">` line yourself, using `$(MSBuildThisFileDirectory)` as the prefix.

## Working with the AutoCAD API safely

A short list of pitfalls that catch every new AutoCAD developer:

- **Always be in a transaction.** Reading `ObjectId.Open` without a transaction (or outside it) throws `eNotOpenForRead` at runtime. Use `using (var trans = doc.TransactionManager.StartTransaction()) { … trans.Commit(); }`.
- **Open mode matters.** `OpenMode.ForRead` for reads, `OpenMode.ForWrite` for mutations. Don't escalate read → write — close and reopen for write.
- **The active document can change.** Cache `Application.DocumentManager.MdiActiveDocument` once at the top of an operation; don't rely on it being stable across UI yields.
- **No background threads touching `Database` / `Editor`.** AutoCAD is single-threaded for its API. WPF `async`/`await` continuations land on the UI thread, which is fine; explicit `Task.Run` is not.
- **`SendStringToExecute` is async.** It enqueues a command; control returns immediately. Don't write code that assumes the command finished before the next line runs.
- **Pack URIs reference the assembly name.** `pack://application:,,,/codehaks.TotalLength;component/Resources/large.png` — if you ever rename the assembly, update every pack URI.

## When you're stuck

- Check the AutoCAD command line — `Editor.WriteMessage` is your friend for tracing.
- Use **Tools → Options → System → DBX → Diagnostic Information** in AutoCAD for runtime errors that AutoCAD swallows.
- The Autodesk ObjectARX docs (`acdbmgd.chm`) ship with AutoCAD under `C:\Program Files\Autodesk\AutoCAD <year>\Help\`.
