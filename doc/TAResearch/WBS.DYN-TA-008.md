# DYN-TA-008 Package and release closure

Status: Active
Progress: 35%
Updated: 2026-07-12

## Completed

- `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta74` full WebSharper build/pack succeeded after releasing the stale 10.1.5 compiler service handle；nupkg includes the repository README as `PackageReadmeFile`。
- beta74 nupkg copied to SDK 10.0.301 library-packs；PTC bundle verifier loaded exact PTCS beta80 / Dynamic beta74 assemblies from NuGet cache and passed asset/classifier gates。
- Root PTC canonical bundle verifier、NuGet live host and RN JSON SDUI demo now exact-reference beta74。No `ProjectReference` fallback or `nuget.config` was introduced。
- Package README、Verification、WBS/Test/DevLog current-state references aligned。

## Remaining

- Public NuGet push/index/dependency page verification for beta74 and TA adapter packages。
- Complete browser absent/present-invalid visual gate、E2EQ isolated host bundle and cross-host matrix before final release status。
- Align formal PTCS.Host deployment only after dependent WBS reaches release gate；local library-packs is development evidence, not production deployment。

## Evidence

- Package：`src/bin/Release/PulseTrade.Comm.Spa.Dynamic.0.1.3-beta74.nupkg`（generated/ignored）。
- Verifier：`G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\verify-ptcs-dynamic-nuget-bundle.fsx` revision 10。
- Operation log：`log/20260712/20260712045600.dyn-ta-static-compat.op_log`。
