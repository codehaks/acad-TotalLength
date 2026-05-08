# Roadmap & Ideas

Future improvements for **CodeHaks TotalLength**. Items are grouped by theme and tagged with a rough effort estimate (S = a day or less, M = a few days, L = a week or more) and priority (P1 = next-up, P2 = nice-to-have, P3 = speculative).

This list is **not a commitment** — it's a backlog of ideas to evaluate. PRs that pick one of these up are welcome; please open an issue first to align on scope.

---

## Quality &amp; Testing

| ID | Item | Effort | Priority |
| :-- | :-- | :-- | :-- |
| Q-1 | Add a `tests/TotalLength.UnitTests/` xUnit project covering `SelectOptions`, `RelayCommand<T>`, and `MainViewModel` (after Q-2). | S | **P1** |
| Q-2 | Extract `ISelectService` from `SelectService` and inject it into `MainViewModel`. Unblocks unit-testing the view model. | S | **P1** |
| Q-3 | Split `Shared.projitems` into `Shared.Core.projitems` (no AutoCAD deps) and `Shared.Acad.projitems` (AutoCAD-bound). Lets the unit-test project import only Core. | M | **P1** |
| Q-4 | Add a `tests/TotalLength.IntegrationTests <year>/` project with `[CommandMethod]`-based integration tests driven by `accoreconsole.exe`. | M | P2 |
| Q-5 | Set up GitHub Actions for unit tests on every push (Windows runner; no AutoCAD needed once Q-3 is done). | S | **P1** |
| Q-6 | Self-hosted Windows runner with AutoCAD 2026 for integration tests on `main`/tags. | L | P2 |
| Q-7 | Coverage badge via `coverlet.collector` + `ReportGenerator`. | S | P3 |

## Distribution &amp; Packaging

| ID | Item | Effort | Priority |
| :-- | :-- | :-- | :-- |
| D-1 | Produce an Autodesk **`.bundle`** package (PackageContents.xml + per-version DLLs) so users get auto-load on AutoCAD startup without `NETLOAD`. | M | **P1** |
| D-2 | Sign the assemblies (Authenticode). AutoCAD's Trusted Locations dialog gets noisier every release for unsigned add-ons. | S | P2 |
| D-3 | Publish a [Autodesk App Store](https://apps.autodesk.com/) listing. | L | P3 |
| D-4 | Cut versioned releases via `gh release create`; attach a per-version ZIP. Document in [build-and-load.md](build-and-load.md). | S | **P1** |
| D-5 | Generate a `winget` manifest so users can `winget install codehaks.TotalLength`. | S | P3 |

## Product features

| ID | Item | Effort | Priority |
| :-- | :-- | :-- | :-- |
| F-1 | Support more entity types: `CIRCLE` (circumference), `SPLINE`, `ELLIPSE`, `XLINE`/`RAY` (skip), 3D polylines. | S | **P1** |
| F-2 | **Live preview** — re-run the length calculation as the user changes filter checkboxes, without re-prompting for selection. Requires keeping the last `ObjectIdCollection`. | M | P2 |
| F-3 | **Per-layer / per-color breakdown** — show length grouped by layer or color in the dialog, not just a single total. | M | P2 |
| F-4 | **Unit awareness** — read drawing units (`MEASUREMENT`, `INSUNITS`) and label results (`mm`, `m`, `in`, `ft`). | S | **P1** |
| F-5 | **Copy to clipboard** + **Export to CSV** of the per-entity breakdown. | S | P2 |
| F-6 | Modeless palette (instead of modal dialog) using `Autodesk.AutoCAD.Windows.PaletteSet`. Lets users keep the panel open while drawing. | M | P2 |
| F-7 | **Persistent settings** — remember last-used filter checkboxes and window position across sessions (per-user via `%AppData%`). | S | P2 |
| F-8 | **Localization** — extract strings into resx, ship `de-DE`, `fr-FR`, `es-ES` (AutoCAD's biggest non-English markets). | M | P3 |

## Engineering / code health

| ID | Item | Effort | Priority |
| :-- | :-- | :-- | :-- |
| E-1 | Rename C# root namespace from `MyApp.*` to `CodeHaks.TotalLength.*` to match the assembly name. Blocks/affects pack URIs (assembly name doesn't change, so URIs stay). | S | **P1** |
| E-2 | Add a logging abstraction. Choices: `Microsoft.Extensions.Logging` with a small AutoCAD `Editor.WriteMessage` provider, or Serilog with a sink that writes to `%AppData%\CodeHaks\TotalLength\logs\`. | S | P2 |
| E-3 | Replace ad-hoc `try/catch` in `SelectService` with a `Result<T>` (or use [`OneOf`](https://github.com/mcintyre321/OneOf)). | S | P2 |
| E-4 | Promote `RelayCommand<T>` to a proper `RelayCommand`/`RelayCommand<T>` pair, or adopt `CommunityToolkit.Mvvm`'s source-generated commands. | S | P3 |
| E-5 | Standardize folder casing — currently you'll see both `Application/` and `application/` and `Core/` and `core/` paths. Pick one. Affects `Shared.projitems`. | S | **P1** |
| E-6 | Move the `RibbonManager` class into the `MyApp.Presentation.Ribbons` namespace (currently global namespace). | S | **P1** |
| E-7 | Migrate the 2024 csproj to SDK-style with `<TargetFramework>net48</TargetFramework>`. Aligns the three csprojs syntactically. | M | P2 |

## AutoCAD support

| ID | Item | Effort | Priority |
| :-- | :-- | :-- | :-- |
| A-1 | Add an **AutoCAD 2027** target (`net10.0-windows`). Per Autodesk, 2027 will require .NET 10. | S | **P1** |
| A-2 | **Drop AutoCAD 2024** when usage telemetry / customer demand justifies it. Removes the .NET Framework 4.8 leg of the build. | S | P3 |
| A-3 | **BricsCAD support** (community ask — BricsCAD's API is closely modeled on AutoCAD's). Likely a fork of `SelectService` behind a host-detection abstraction. | L | P3 |

## Documentation

| ID | Item | Effort | Priority |
| :-- | :-- | :-- | :-- |
| Doc-1 | Add a screencast / animated GIF of `ZLen` in action to the README. | S | P2 |
| Doc-2 | Document the **release process** end-to-end (tag → build.bat → ZIP → gh release create). | S | **P1** |
| Doc-3 | Add a `CHANGELOG.md` following [Keep a Changelog](https://keepachangelog.com/). | S | **P1** |
| Doc-4 | Add an `ARCHITECTURE_DECISION_RECORDS/` folder for major design choices (shared-project pattern, modal vs. modeless, etc.). | S | P3 |

## Tooling / DX

| ID | Item | Effort | Priority |
| :-- | :-- | :-- | :-- |
| T-1 | `.editorconfig` enforcing the conventions documented in [contributing.md](contributing.md). | S | **P1** |
| T-2 | A pre-commit hook (or `dotnet format --verify-no-changes` in CI) that fails on style drift. | S | P2 |
| T-3 | A `Stop-Acad.ps1` helper script to kill all `acad.exe` instances before rebuilding. Reference it in [developer-guide.md](developer-guide.md). | S | P2 |
| T-4 | A `Run-Acad.ps1` that builds a target, kills any AutoCAD, and launches the matching AutoCAD with debugger arguments — one-keystroke inner loop. | S | P2 |

## Out-of-scope (won't do)

These come up but are deliberately *not* on the roadmap:

- **Cross-platform support.** AutoCAD on macOS does not load .NET add-ons.
- **Cloud / web export.** Out of scope for an in-AutoCAD measurement tool.
- **Replacing the WPF UI with WinUI3 / MAUI.** AutoCAD's hosted environment is best served by classic WPF. WinUI3 in particular is brittle to host inside `acad.exe`.

---

## Suggested first sprint

If you have a week and want to deliver visible value:

1. **Q-2 + Q-1 + Q-5** (a day): seam + first unit tests + CI green badge.
2. **F-4** (half day): unit-aware totals — biggest user-visible win for the smallest effort.
3. **D-1 + D-4** (one to two days): produce an installable `.bundle`, cut a tagged release.
4. **A-1** (half day): add AutoCAD 2027 support so the matrix is current when AutoCAD 2027 ships.
5. **E-5 + T-1** (half day): folder-casing cleanup + `.editorconfig`.

That's a believable v1.1 release with material improvements in distribution, quality, and product surface.
