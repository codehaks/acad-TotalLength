# Contributing

## Quick start

1. Clone and open `Acad-TotalLength.sln` in Visual Studio 2022.
2. Make sure the AutoCAD release(s) you want to target are installed.
3. Edit code under [src/Shared/](../src/Shared/) — almost nothing should need to change in the per-version csprojs.
4. Build with [build.bat](../build.bat) or `msbuild Acad-TotalLength.sln`.
5. Smoke-test with `NETLOAD` + `ZLen` in AutoCAD.

## Where to put code

| Layer | Folder | Rules |
| --- | --- | --- |
| AutoCAD entry points | `src/Shared/` (root) | Classes with `IExtensionApplication` or `[CommandMethod]`. Keep thin. |
| AutoCAD interaction | `src/Shared/Application/` | Anything that touches `Editor`, `Database`, `Transaction`. |
| Domain types | `src/Shared/Core/` | POCOs / value objects. No AutoCAD dependencies. |
| WPF | `src/Shared/Presentation/` | Views, view-models, value converters. No `[CommandMethod]` types here. |
| MVVM glue | `src/Shared/Common/` | Cross-cutting helpers (`RelayCommand`, etc.). |

## Coding conventions

- File-scoped namespaces.
- `<LangVersion>latest</LangVersion>` and `<Nullable>enable</Nullable>` are on; new files should be nullable-clean.
- Prefer `using` declarations over `using` blocks where the lifetime is obvious.
- Wrap any code that reads/writes `DBObject`s in `using (var trans = ... StartTransaction())`. Always commit at the end.
- AutoCAD APIs are not thread-safe — never touch them off the main thread.

## Adding support for a new AutoCAD version

1. **Create the csproj.** Copy `src\TotalLength 2026\TotalLength 2026.csproj` (or the 2024 csproj if the new release still uses .NET Framework) to `src\TotalLength <YYYY>\TotalLength <YYYY>.csproj`. Update:
   - `<TargetFramework>` (e.g. `net10.0-windows` for 2027).
   - All `HintPath`s to point to `C:\Program Files\Autodesk\AutoCAD <YYYY>\`.
2. **Register in the .sln.** Add three sections to `Acad-TotalLength.sln`:
   - A `Project(...) = "TotalLength <YYYY>", "src\TotalLength <YYYY>\TotalLength <YYYY>.csproj", "{NEW-GUID}"` block. Use a freshly generated GUID (`[guid]::NewGuid()` in PowerShell).
   - Four `ProjectConfigurationPlatforms` entries for the new GUID (Debug/Release × ActiveCfg/Build.0), mirroring an existing target.
   - One `SharedMSBuildProjectFiles` entry: `src\Shared\Shared.projitems*{new-guid-lowercase}*SharedItemsImports = 5` (`= 5` for SDK-style; `= 4` for old-style).
3. **Update [build.bat](../build.bat).** Add a `call :build <YYYY> ...` line.
4. **Update [docs/build-and-load.md](build-and-load.md)** and the README badge.

## Adding a new AutoCAD command

1. Add the class under `src/Shared/Commands/` (or another logical folder).
2. Decorate the public method with `[CommandMethod("YourCommandName")]`.
3. Make sure the file is included by `Shared.projitems` — this happens automatically if you add the file via Visual Studio with the Shared project active.
4. Rebuild and test with `NETLOAD`.

## Adding a new ribbon button

Edit [RibbonManager.cs](../src/Shared/Presentation/Ribbons/RibbonManager.cs). Use existing buttons as a template — the gotcha is that pack URIs reference the **assembly name** (`codehaks.TotalLength`), not the namespace.

## Pull request checklist

- [ ] Builds clean for all three AutoCAD versions (`build.bat Release` succeeds).
- [ ] Smoke-tested in at least one AutoCAD release.
- [ ] No new `#if DEBUG`-only code on user-visible paths.
- [ ] `MainViewModel` properties raise `OnPropertyChanged()`.
- [ ] AutoCAD work happens inside a transaction.
- [ ] Tests added/updated for new logic (see [testing.md](testing.md)).
