# Build & Load

This document covers building the per-version assemblies and loading them into AutoCAD.

## AutoCAD version matrix

| AutoCAD | Target framework | csproj | Output DLL path |
| :--- | :--- | :--- | :--- |
| 2024 | `.NET Framework 4.8` | [src/TotalLength 2024/TotalLength 2024.csproj](../src/TotalLength%202024/TotalLength%202024.csproj) | `src\TotalLength 2024\bin\<Config>\codehaks.TotalLength.dll` |
| 2025 | `net8.0-windows` | [src/TotalLength 2025/TotalLength 2025.csproj](../src/TotalLength%202025/TotalLength%202025.csproj) | `src\TotalLength 2025\bin\<Config>\net8.0-windows\codehaks.TotalLength.dll` |
| 2026 | `net8.0-windows` | [src/TotalLength 2026/TotalLength 2026.csproj](../src/TotalLength%202026/TotalLength%202026.csproj) | `src\TotalLength 2026\bin\<Config>\net8.0-windows\codehaks.TotalLength.dll` |

`<Config>` is `Debug` or `Release`. All three build outputs share the assembly name **`codehaks.TotalLength.dll`**.

## Prerequisites

- Windows 10 / 11, x64.
- **Visual Studio 2022 17.8+** *or* MSBuild 17+ on PATH. .NET Framework 4.8 build tools must be present (installed via the "Desktop development with C++/.NET" workload, or the .NET Framework 4.8 targeting pack).
- **.NET 8 SDK** (`dotnet --list-sdks` should show 8.x).
- The AutoCAD versions you want to target installed at the standard locations:
  - `C:\Program Files\Autodesk\AutoCAD 2024\`
  - `C:\Program Files\Autodesk\AutoCAD 2025\`
  - `C:\Program Files\Autodesk\AutoCAD 2026\`

If a referenced AutoCAD version isn't installed, that csproj will fail to resolve `accoremgd.dll` / `acdbmgd.dll` / `acmgd.dll` / `AcWindows.dll` / `AdWindows.dll`. Either install the matching AutoCAD release, or unload that project from the solution.

## Build

### One-shot (recommended)

```bat
build.bat              :: Release (default)
build.bat Debug        :: Debug
```

[build.bat](../build.bat) builds all three csprojs sequentially and copies each `codehaks.TotalLength.dll` (and `.pdb` if present) into `dist\AutoCAD <year>\` so you don't have to chase three different output paths.

Layout after a successful run:

```text
dist/
├── AutoCAD 2024/
│   └── codehaks.TotalLength.dll
├── AutoCAD 2025/
│   └── codehaks.TotalLength.dll
└── AutoCAD 2026/
    └── codehaks.TotalLength.dll
```

### Whole solution

```bat
msbuild Acad-TotalLength.sln /p:Configuration=Release
```

Builds all three projects but leaves outputs in their respective `bin\<Config>\…\` folders.

### Single target

```bat
msbuild "src\TotalLength 2024\TotalLength 2024.csproj" /p:Configuration=Release
msbuild "src\TotalLength 2025\TotalLength 2025.csproj" /p:Configuration=Release
msbuild "src\TotalLength 2026\TotalLength 2026.csproj" /p:Configuration=Release
```

The 2025 / 2026 projects can also be built with `dotnet build`:

```bat
dotnet build "src\TotalLength 2026\TotalLength 2026.csproj" -c Release
```

(`dotnet build` will *not* build the 2024 project — `.NET Framework 4.8` requires `msbuild`.)

## Load into AutoCAD

1. Start the AutoCAD release that matches the DLL you built (`2024` ↔ `dist\AutoCAD 2024\`, etc.).
2. At the AutoCAD command prompt, type `NETLOAD` and press <kbd>Enter</kbd>.
3. Browse to `dist\AutoCAD <year>\codehaks.TotalLength.dll` and click **Open**.
4. Verify load success — a **Codehaks** tab should appear on the ribbon with a *Total Length* button.
5. Trigger the add-on:
   - Click **Codehaks ▸ Total Length** on the ribbon, **or**
   - Type `ZLen` and press <kbd>Enter</kbd>.
6. In the dialog: pick object types (lines / polylines / arcs), click **Select Objects**, drag a selection in the drawing, and read the total length and count.

## Auto-load on AutoCAD startup (optional)

`NETLOAD` only loads for the current session. To auto-load:

- **Quick & dirty:** drop the DLL into AutoCAD's `Support` path and use the `APPLOAD` *Startup Suite*.
- **Proper:** ship a `.bundle` (PackageContents.xml) installed under `%AppData%\Autodesk\ApplicationPlugins\`. This is currently **not** part of the build; see [roadmap.md](roadmap.md) for the planned packaging work.

## Troubleshooting

| Symptom | Likely cause |
| :--- | :--- |
| `error MSB3245: Could not resolve this reference … accoremgd` | Matching AutoCAD version not installed at the standard path. |
| `NETLOAD` succeeds but `ZLen` says *Unknown command* | The 2025 DLL was loaded into AutoCAD 2024 (or vice-versa). DLL must match host's .NET runtime. |
| Ribbon tab not appearing | `IExtensionApplication.Initialize` only runs on the first `NETLOAD` of the session. Restart AutoCAD or reload the DLL. |
| WPF dialog opens behind AutoCAD | Window owner not set; verify `WindowInteropHelper.Owner = Application.MainWindow.Handle`. |
| `FileLoadException` for `codehaks.TotalLength.dll` after a code change | Old DLL is locked by a running AutoCAD instance. Close all AutoCAD windows before rebuilding. |
