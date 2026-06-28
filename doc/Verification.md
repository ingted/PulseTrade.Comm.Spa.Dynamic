---
status: active
updated: 2026-06-28
scope: PulseTrade.Comm.Spa.Dynamic package / browser-extension gates
---

# PulseTrade.Comm.Spa.Dynamic Verification Registry

本文件記錄 `PulseTrade.Comm.Spa.Dynamic` 的可重複驗證方式。若後續新增 verifier，先更新本表，再同步 `doc/TEST.md`、`doc/WBS.md` 與 `doc/Traceability.md`。

## Gate 清單

| Gate | 類型 | 命令 / 腳本 | 用途與摘要 | Revision |
|---|---|---|---|---|
| DYN-VFY-001 | Package build / WebSharper bundle | `dotnet build .\src\PulseTrade.Comm.Spa.Dynamic.fsproj -c Debug -p:BaseIntermediateOutputPath=C:\ptcsdyn-build\obj\ -p:OutputPath=C:\ptcsdyn-build\bin\` | 以短 intermediate/output path 建置 Dynamic package 與 WebSharper bundle。長 repo path 下 `wsfsc.exe` 可能直接 crash 且只回 `MSB6006`，因此 package verification 使用短 path。 | 本輪新增：2026-06-28 actor-dynamic-action-modes |
| PTC-VFY-007 | Cross-repo package bundle | `G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\verify-ptcs-dynamic-nuget-bundle.fsx` | 驗證 PTCS / Dynamic NuGet package、bundle assets、Dynamic JS markers 與 exact package version。 | Cross-repo |
| PTC-VFY-008 | Cross-repo live host | `G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\run-ptcs-dynamic-nuget-live-host.fsx` | 啟動 PTCS + Dynamic in-process live host，供 browser/manual/Playwright 驗證 Actor Dynamic / Actor Argu flows。 | Cross-repo |

## WebSharper 長路徑注意事項

`C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic` 路徑較長。若直接執行 `dotnet build .\src\PulseTrade.Comm.Spa.Dynamic.fsproj -c Debug`，WebSharper `wsfsc.exe` 可能在產生 bundled JS 時直接終止，表面錯誤為：

```text
error MSB6006: "wsfsc.exe" exited with code -532462766.
```

同一份 source 以短 path 設定 `BaseIntermediateOutputPath` 與 `OutputPath` 可通過，因此這不是 F# 語法錯誤。後續 package build、NuGet local verification 與 CI wrapper 應使用 DYN-VFY-001 的短 path 命令，或提供等價的短 build root。
