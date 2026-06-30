---
status: active
updated: 2026-06-30
scope: PulseTrade.Comm.Spa.Dynamic package / browser-extension gates
---

# PulseTrade.Comm.Spa.Dynamic Verification Registry

本文件記錄 `PulseTrade.Comm.Spa.Dynamic` 的可重複驗證方式。若後續新增 verifier，先更新本表，再同步 `doc/TEST.md`、`doc/WBS.md` 與 `doc/Traceability.md`。

## Gate 清單

### 2026-06-29 Current Package Override

- Current package pair: `PulseTrade.Comm.Spa 0.2.5-beta48` + `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta38`.
- `DYN-VFY-001` short-path build currently uses `C:\ptcsdyn-release-beta38\bin`.
- `DYN-VFY-004` package tests passed `18/18` against beta38.
- `DYN-VFY-007` current command: `dotnet fsi --exec .\src\poc.full.nuget.journal.fsx -- --pcsl-root "G:/PulseTrade.fs.Comm.Log/manual/ptcsDynamicNugetJournal/pcsl_echo_reuse_after_stop_20260630" --cluster-port 9808 --no-wait`. It verifies `PingPong actor projected status: Some "terminated"`, `pingPongFiltered=true`, `ensureEchoActorRegistered()` reuses an already-live `nuget-journal-echo`, and `recreateEchoActor()` stops then reuses the same fixed actor name without duplicate spawn failure.
- PTC cross-repo `PTC-VFY-007` resolves beta48/beta38 from NuGet/library-packs; public 81 deployment is `live81-ptcs-beta48-dynamic-beta38-pingpong-registry-20260629161443`.

| Gate | 類型 | 命令 / 腳本 | 用途與摘要 | Revision |
|---|---|---|---|---|
| DYN-VFY-001 | Package build / WebSharper bundle | `dotnet build .\src\PulseTrade.Comm.Spa.Dynamic.fsproj -c Release -p:BaseIntermediateOutputPath=C:\ptcsdyn-release-beta38\obj\ -p:OutputPath=C:\ptcsdyn-release-beta38\bin\` | 以短 intermediate/output path 建置 Dynamic package 與 WebSharper bundle。長 repo path 下 `wsfsc.exe` 可能直接 crash 且只回 `MSB6006`，因此 package verification 使用短 path。本輪 RFC-0005 也確認新增 client `[<JavaScript>]` compile unit、`String.Contains`、多段 `IndexOf` predicate 都可能觸發 crash；first slice 使用既有 `ActorDynamicTab.fs` 與單一 `IndexOf("ActorTopologyPage")`。2026-06-29 beta38 gate 直接檢查 nupkg dependency closure 指向 `PulseTrade.Comm.Spa [0.2.5-beta48]`，並保留 ActorsPage console logging marker `[PTCS.Dynamic ActorTree DSL]`。 | 本輪修訂：2026-06-29 beta38-pingpong-stop |
| DYN-VFY-004 | Package tests | `dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release -p:WebSharperRunCompiler=false -p:GeneratePackageOnBuild=false -- --summary --no-spinner` | 在 DYN-VFY-001 full WebSharper build 通過後，關閉 WebSharper compiler 跑 Expecto package tests，避免 test build 重跑 long/default path `wsfsc.exe`。本輪覆蓋 DYN-T-526/DYN-T-527/DYN-T-528，總計 18/18 pass。2026-06-29 beta30 仍以同 gate 驗證 package semantics。 | 本輪修訂：2026-06-29 actors-page-beta30-offline-cleanup |
| PTCS-VFY-ActorsPageDynamic | Cross-repo source host / Playwright MCP | `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Scripts\run.actorsPageDynamic.localHost.fsx` with `--dynamic-bin-dir C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\src\bin\Release\net10.0` | 啟動 PTCS source host 載入 Dynamic source Release bundle，使用 Playwright MCP 驗證 `/actors` 由 Dynamic page renderer 接管：page renderer registered, fallback rows `0`, blocks ordered PTCS Host -> GW Host -> RN Host -> Unknown, full actor addresses visible, boxed `+` / `-` toggles functional。Evidence: `G:\PulseTrade.fs\log\20260628\20260628220000.actors-page-toggle-check.json`; screenshot: `G:\PulseTrade.fs\log\20260628\20260628220000.actors-page-toggle-fixed.png`。 | 本輪修訂：2026-06-28 actors-page-grouping-toggle |
| PTCS-VFY-ActorsPageDynamic-FSharp | Cross-repo reusable F# Playwright verifier | `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Scripts\verify.actorsPageDynamic.playwright.fsx -- --dynamic-bin-dir C:\ptcsdyn-release-beta30d\bin` | 啟動 PTCS + Dynamic source/short-path Release bundle，使用 F# Playwright locator API 驗證 Dynamic page-level `/actors` renderer accepted path：fallback DOM absent、core `actor-node` / `actor-card` absent、PTCS/GW/RN blocks、full `akka.tcp://...` addresses、`/user` 與 `/system` virtual ancestors、reload/report controls、report generate、browser-local schedule start/stop、status dots、connector lines、depth rows、boxed toggle `aria-expanded`/text 與 visible row collapse/expand，並使用 deterministic loopback probe listeners 避免 online fixtures 被 offline filter 誤清除。Evidence: `G:\PulseTrade.fs\log\20260629\public81-actors-beta41-dyn30.png`。 | 本輪修訂：2026-06-29 actors-page-beta30-offline-cleanup |
| PTC-VFY-007 | Cross-repo package bundle | `G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\verify-ptcs-dynamic-nuget-bundle.fsx` | 驗證 PTCS / Dynamic NuGet package、bundle assets、Dynamic JS markers 與 exact package version。2026-06-29 beta43/beta33 gate 檢查實際 `#r` assembly path、`ActorTopologyPage`、`[PTCS.Dynamic ActorTree DSL]`、`dynamic-actors-page`、`virtual-path`、`data-parent-id`、`dynamic-actor-tree-toggle`、`dynamic-actor-tree-status-dot`、`dynamic-actor-tree-connector`、report marker、schedule start/stop markers、offline marker、PTCS/GW/RN block markers，並排除 beta28 stale schedule disabled marker。 | Cross-repo |
| PTC-VFY-008 | Cross-repo live host | `G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\run-ptcs-dynamic-nuget-live-host.fsx` | 啟動 PTCS + Dynamic in-process live host，供 browser/manual/Playwright 驗證 Actor Dynamic / Actor Argu flows。 | Cross-repo |
| DYN-VFY-005 | Full NuGet POC script | `dotnet fsi --exec .\src\poc.full.nuget.fsx -- --no-wait` | 直接 `#r` PTCS `0.2.5-beta40` 與 Dynamic `0.1.3-beta29`，啟動 PTCS + Dynamic extension full POC，建立 Durable Echo / Dynamic Echo / Showcase actors，送 ActorArgu durable command 與 Canvas JSON DSL，列印 Chat/Sets/Actors/ActorArgu/Dynamic page URL、PCSL root、完整 `akka.tcp://...` actor address、tickets 與 ingress health，最後自動停止。2026-06-29 修訂：補 `ActorArguSendArgs.HistoryKeys`、保留 `defaultArgumentsText` 並讓外部 argv 只覆蓋指定值、`--cluster-port 0` 自動挑可用 Akka port、人工 FSI 模式改用 `stopPocFullNuget()` 而非 `Console.ReadLine()`。 | 本輪修訂：2026-06-29 poc-full-nuget |
| DYN-VFY-006 | Full NuGet POC 2 script | `dotnet fsi --exec .\src\poc.full.nuget.2.fsx -- --no-wait` | 新增、但不取代 `poc.full.nuget.fsx`。直接 `#r` PTCS `0.2.5-beta43` 與 Dynamic `0.1.3-beta33`，啟動 PTCS + Dynamic extension POC，註冊 host-local Argu DU/template 與 Actor Argu default target key，並覆寫 Dynamic extension manifest 使 `+ Page` 不提供 Actor Dynamic tab page type。POC2 會把真實 echo actor 以 `hub.RegisterActor` 投影到 PTCS actor registry；`--no-wait` 驗證 health、Actors/ActorArgu/Dynamic JS 可達、`/actors/api/snapshot` 有非零 node/actor 且含 `nuget2-echo` actor address、ActorArgu durable send completed、bundle 含 Actors/Form markers、chat HTML 不含 `option value="actor-dynamic"`，並確認 target-key alias 在 server-side send/probe 後仍保留。 | 本輪修訂：2026-06-29 poc-full-nuget-2-beta33 |
| DYN-VFY-007 | Full NuGet journal POC script | `dotnet fsi --exec .\src\poc.full.nuget.journal.fsx -- --pcsl-root "G:/PulseTrade.fs.Comm.Log/manual/ptcsDynamicNugetJournal/pcsl_echo_reuse_after_stop_20260630" --cluster-port 9808 --no-wait` | 直接 `#r` PTCS `0.2.5-beta48` 與 Dynamic `0.1.3-beta38`，用 SQL Server Akka.Persistence journal 作 canonical event source，PCSL root 只作 projection/cache。腳本以 `pcslRoot` hash 派生 SQL DB 名、固定 default cluster port `9787` 與 stable template key `poc-full-nuget-journal-argu`，並把 UI hub backend 包成 `PcslActorProxyCommSpaPersistenceBackend(remoteWire=true)`，避免 `/pages/api/*` direct append 繞過 journal。Echo actor 與 reload-test PingPong actor 都以 `PulseTrade.Comm.Actor.Registry.ActorOfRegistered` 建立，sink 指向 `hub.ActorRegistrySink()`，不再直接呼叫 `CommHub.RegisterActor`。人工 FSI 模式會列印 `ensureEchoActorRegistered()`、`stopEchoActor()`、`recreateEchoActor()` 與 `stopPingPongActor()`；live fixed-name Echo actor 不可 duplicate-spawn，但 `recreateEchoActor()` 會先 stop、等待 path release，再以同名 strict `ActorOfRegistered` 重建，證明 stopped/terminated 後固定 actor name 可重用。可先 reload `/actors` 看到 PingPong，再呼叫 `stopPingPongActor()` 後 reload 觀察 actor lifecycle 更新。Dynamic beta33+ 在 ActorsPage RENDER 與 RELOAD 都會於 browser console 輸出 `[PTCS.Dynamic ActorTree DSL]` group，包含 raw DSL、parsed DSL object 與 nodes array。 | 本輪修訂：2026-06-30 echo-reuse-after-stop |

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

## PackageReference-only PTCS Boundary

2026-06-29 起，`src\PulseTrade.Comm.Spa.Dynamic.fsproj` 不再以 ProjectReference 消費 PTCS source，改用 exact `PackageReference Include="PulseTrade.Comm.Spa" Version="[0.2.5-beta48]"`。驗證重點：

- `rg ProjectReference src\PulseTrade.Comm.Spa.Dynamic.fsproj` 應無命中。
- Release build 需通過；若出現 `MSB6006 wsfsc.exe -532462766` 且存在舊 `wsfscservice.exe`，先停用 stale WebSharper compiler service 再重試。
- 新 nupkg nuspec 必須包含 `PulseTrade.Comm.Spa [0.2.5-beta48]` dependency。
- 完成後複製 nupkg 到目前 SDK `FSharp\library-packs`，供 `poc.full.nuget*.fsx` 與 PTC bundle verifier 使用。
