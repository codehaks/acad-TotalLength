# TotalLength — Developer Documentation

This folder contains the engineering documentation for **CodeHaks TotalLength**. The user-facing entry point lives in the repository [README.md](../README.md); this index is for contributors and maintainers.

## Index

| Document | Audience | Purpose |
| :--- | :--- | :--- |
| [architecture.md](architecture.md) | New contributors | Layered design, namespaces, AutoCAD interop notes. |
| [build-and-load.md](build-and-load.md) | Anyone building locally | Per-version build matrix, `build.bat`, `NETLOAD` workflow. |
| [developer-guide.md](developer-guide.md) | Active contributors | Daily workflow, code map, debugging tips. |
| [contributing.md](contributing.md) | External contributors | Branching, commit style, review expectations, PR checklist. |
| [testing.md](testing.md) | Contributors adding tests | How to introduce unit tests and AutoCAD-integration tests. |
| [roadmap.md](roadmap.md) | Maintainers, planners | Future ideas, suggested improvements, technical debt to address. |

## Conventions

- All paths in these docs are written relative to the repository root.
- "the shared project" = [src/Shared/](../src/Shared/).
- "per-version csprojs" = [src/TotalLength 2024/](../src/TotalLength%202024/), [src/TotalLength 2025/](../src/TotalLength%202025/), [src/TotalLength 2026/](../src/TotalLength%202026/).
- The C# root namespace is `MyApp` (the assembly is `codehaks.TotalLength.dll`).
