---
status: active
updated: 2026-06-29
scope: PulseTrade.Comm.Spa.Dynamic package / browser-extension gates
---

# PulseTrade.Comm.Spa.Dynamic Verification Registry

本文件記錄 `PulseTrade.Comm.Spa.Dynamic` 的可重複驗證方式。若後續新增 verifier，先更新本表，再同步 `doc/TEST.md`、`doc/WBS.md` 與 `doc/Traceability.md`。

## Gate 清單

| Gate | 類型 | 命令 / 腳本 | 用途與摘要 | Revision |
|---|---|---|---|---|
| DYN-VFY-001 | Package build / WebSharper bundle | `dotnet build .\src\PulseTrade.Comm.Spa.Dynamic.fsproj -c Release -p:BaseIntermediateOutputPath=C:\ptcsdyn-release-beta29b\obj\ -p:OutputPath=C:\ptcsdyn-release-beta29b\bin\` | 以短 intermediate/output path 建置 Dynamic package 與 WebSharper bundle。長 repo path 下 `wsfsc.exe` 可能直接 crash 且只回 `MSB6006`，因此 package verification 使用短 path。本輪 RFC-0005 也確認新增 client `[<JavaScript>]` compile unit、`String.Contains`、多段 `IndexOf` predicate 都可能觸發 crash；first slice 使用既有 `ActorDynamicTab.fs` 與單一 `IndexOf("ActorTopologyPage")`。2026-06-29 beta29 gate 會直接檢查 nupkg 內 `PulseTrade.Comm.Spa.Dynamic.js` 含 `Stop schedule` / `Report schedule started`，且不含 stale schedule disabled 文案。 | 本輪修訂：2026-06-29 actors-page-beta29-schedule |
| DYN-VFY-004 | Package tests | `dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release -p:WebSharperRunCompiler=false -p:GeneratePackageOnBuild=false -- --summary --no-spinner` | 在 DYN-VFY-001 full WebSharper build 通過後，關閉 WebSharper compiler 跑 Expecto package tests，避免 test build 重跑 long/default path `wsfsc.exe`。本輪覆蓋 DYN-T-526/DYN-T-527/DYN-T-528，總計 18/18 pass。2026-06-29 beta29 仍以同 gate 驗證 package semantics。 | 本輪修訂：2026-06-29 actors-page-beta29-schedule |
| PTCS-VFY-ActorsPageDynamic | Cross-repo source host / Playwright MCP | `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Scripts\run.actorsPageDynamic.localHost.fsx` with `--dynamic-bin-dir C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\src\bin\Release\net10.0` | 啟動 PTCS source host 載入 Dynamic source Release bundle，使用 Playwright MCP 驗證 `/actors` 由 Dynamic page renderer 接管：page renderer registered, fallback rows `0`, blocks ordered PTCS Host -> GW Host -> RN Host -> Unknown, full actor addresses visible, boxed `+` / `-` toggles functional。Evidence: `G:\PulseTrade.fs\log\20260628\20260628220000.actors-page-toggle-check.json`; screenshot: `G:\PulseTrade.fs\log\20260628\20260628220000.actors-page-toggle-fixed.png`。 | 本輪修訂：2026-06-28 actors-page-grouping-toggle |
| PTCS-VFY-ActorsPageDynamic-FSharp | Cross-repo reusable F# Playwright verifier | `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Scripts\verify.actorsPageDynamic.playwright.fsx -- --dynamic-bin-dir C:\ptcsdyn-release-beta29b\bin` | 啟動 PTCS + Dynamic source/short-path Release bundle，使用 F# Playwright locator API 驗證 Dynamic page-level `/actors` renderer accepted path：fallback DOM absent、core `actor-node` / `actor-card` absent、PTCS/GW/RN blocks、full `akka.tcp://...` addresses、`/user` 與 `/system` virtual ancestors、reload/report controls、report generate、browser-local schedule start/stop、status dots、connector lines、depth rows、boxed toggle `aria-expanded`/text 與 visible row collapse/expand，並確認 virtual ancestors 不再產生 synthetic Unknown block。Evidence: `G:\PulseTrade.fs\log\20260629\actors-public81-beta40-dyn29.png`。 | 本輪修訂：2026-06-29 actors-page-beta29-schedule |
| PTC-VFY-007 | Cross-repo package bundle | `G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\verify-ptcs-dynamic-nuget-bundle.fsx` | 驗證 PTCS / Dynamic NuGet package、bundle assets、Dynamic JS markers 與 exact package version。2026-06-29 beta40/beta29 gate 檢查 `ActorTopologyPage`、`dynamic-actors-page`、`virtual-path`、`data-parent-id`、`dynamic-actor-tree-toggle`、`dynamic-actor-tree-status-dot`、`dynamic-actor-tree-connector`、report marker、schedule start/stop markers、PTCS/GW/RN block markers，並排除 beta28 stale schedule disabled marker。 | Cross-repo |
| PTC-VFY-008 | Cross-repo live host | `G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\run-ptcs-dynamic-nuget-live-host.fsx` | 啟動 PTCS + Dynamic in-process live host，供 browser/manual/Playwright 驗證 Actor Dynamic / Actor Argu flows。 | Cross-repo |

## WebSharper 長路徑注意事項

`C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic` 路徑較長。若直接執行 `dotnet build .\src\PulseTrade.Comm.Spa.Dynamic.fsproj -c Debug`，WebSharper `wsfsc.exe` 可能在產生 bundled JS 時直接終止，表面錯誤為：

```text
error MSB6006: "wsfsc.exe" exited with code -532462766.
```

同一份 source 以短 path 設定 `BaseIntermediateOutputPath` 與 `OutputPath` 可通過，因此這不是 F# 語法錯誤。後續 package build、NuGet local verification 與 CI wrapper 應使用 DYN-VFY-001 的短 path 命令，或提供等價的短 build root。

RFC-PTCS-DYNAMIC-0005 first slice 另外確認：

- 新增 `Client/ActorsPageRenderer.fs` 這類 `[<JavaScript>]` compile unit，即使內容 no-op，也可能讓 `wsfsc.exe` crash；本輪改放在既有 `Client/ActorDynamicTab.fs`。
- `String.Contains` 在 `[<JavaScript>]` code 會 crash；使用單一 `IndexOf` 可通過。
- 多個 `IndexOf` chained predicate 仍會 crash；first slice 暫以 `ActorTopologyPage` 單 token gate。
