# DYN-TA-008 Package and release closure

Status: Active
Progress: 88%
Updated: 2026-07-12

## Completed

- `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta74` full WebSharper build/pack succeeded after releasing the stale 10.1.5 compiler service handle；nupkg includes the repository README as `PackageReadmeFile`。
- beta74 nupkg copied to SDK 10.0.301 library-packs；PTC bundle verifier loaded exact PTCS beta80 / Dynamic beta74 assemblies from NuGet cache and passed asset/classifier gates。
- Root PTC canonical bundle verifier、NuGet live host and RN JSON SDUI demo now exact-reference beta74。No `ProjectReference` fallback or `nuget.config` was introduced。
- Package README、Verification、WBS/Test/DevLog current-state references aligned。
- Public packages now include Dynamic `0.1.3-beta76`、Dynamic.Ptcs `0.1.0-alpha6-win6`、Renderer `0.1.0-alpha9` and Ptcs.Client `0.1.0-alpha7-win10`；NuGet registration dependency readback confirms exact PTCS beta87 / Contracts alpha4 / Renderer alpha9 edges。
- Downstream Host client `0.1.0-alpha8` exact-pins Ptcs.Client win10；formal 81/82 service consumes the same package chain。

## Remaining

- Complete browser absent/present-invalid visual gate、E2EQ isolated host bundle and cross-host matrix before final release status。
- Resolve canonical generated `websharper.log` owner/ACL and rerun non-mirror package builds；retained mirror packages were built from current fsproj/README/source and separately accepted by package tests/deployed Playwright。

## 2026-07-12 package chain release

- NuGet push returned `Created` for Renderer `0.1.0-alpha7`、Dynamic.Ptcs `0.1.0-alpha6-win4`、Ptcs.Client `0.1.0-alpha7-win8`；downstream Host client `0.1.0-alpha6` exact-pins Ptcs.Client win8。
- nupkg dependency inspection confirms exact Dynamic package chain；NuGet public registration/flat-container indexing was still pending immediately after push，so website dependency readback remains an open release check rather than silently marked complete。
- WebSharper generated `websharper.log` on the canonical checkout became OS access-denied after an orphan MSBuild node；packages were built from hash-recorded canonical source staging under `G:\PulseTrade.fs.Comm.Log\package-staging` with `--disable-build-servers`。This is package-build evidence, not a second source checkout/worktree。
- Docs-aligned final repack/push returned `Created` for Renderer `0.1.0-alpha8`、Dynamic.Ptcs `0.1.0-alpha6-win5`、Ptcs.Client `0.1.0-alpha7-win9` and Host client `0.1.0-alpha7`；these are the current exact versions。
- Current release supersedes that chain with Dynamic beta76、Dynamic.Ptcs win6、Renderer alpha9、Ptcs.Client win10 and Host client alpha8；all pushes returned `Created` and public registration exposes the expected exact dependencies。

## Evidence

- Package：`src/bin/Release/PulseTrade.Comm.Spa.Dynamic.0.1.3-beta74.nupkg`（generated/ignored）。
- Verifier：`G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\verify-ptcs-dynamic-nuget-bundle.fsx` revision 10。
- Operation log：`log/20260712/20260712045600.dyn-ta-static-compat.op_log`。
