# PulseTrade.Comm.Spa.Dynamic

This project is a dynamic SDUI (Server-Driven UI) and Actor extension for `PulseTrade.Comm.Spa`, built with WebSharper.

## Actor Dynamic / Actor Argu modes

`PulseTrade.Comm.Spa.Dynamic` owns Dynamic rendering and Dynamic target binding; `PulseTrade.Comm.Spa` owns the append page shell, key registry, pending replay, and actor-argu command path.

Current mode split:

| PTCS page | Dynamic support |
| --- | --- |
| Actor Argu | FormInput only. No canvas rendering. `Add Target Key` explicitly collects proxy actor + target actor. |
| Actor Dynamic | Direct actor key, DU/FormInput target key, and Dynamic proxy key. Canvas is used only when the actor reply payload is `schema=fskynet-sdui` JSON DSL. |

Key shapes:

```text
Direct actor key:
[ actorAddress ]

DU/FormInput target:
[ actorAddress; duTypeOrTemplateKey; canonicalArgString ]

Explicit ActorArgu target through proxy:
[ proxyActorAddress; "target-v1"; targetActorAddress; duTypeOrTemplateKey; canonicalArgString ]

Dynamic proxy key:
[ proxyActorAddress; "proxy-v1"; rnActorAddress; targetKind ]
```

When an Actor Dynamic selected key is only `[ actorAddress ]`, the extension intentionally returns no FormInput renderer. PTCS fallback input accepts arbitrary text or JSON DSL. If the actor replies with Canvas JSON DSL, the Dynamic message renderer draws the canvas; otherwise PTCS normal message rendering is used.

For ActorArgu `Add Target Key`, the selected key routes the command to `proxyActorAddress` because it remains the first key segment. PTCS core sends `ActorArguTargetCommand.ActorAddress = proxyActorAddress`, `TargetActorAddress = Some targetActorAddress`, and `RawArgu = <input text>`. The proxy actor owns native invocation and durability. Dynamic must not hide this route by creating or rewriting proxy actors in an add-key hook.

## ActorsPage Renderer

`RFC-PTCS-DYNAMIC-0005.actors-page-renderer.md` defines the Dynamic-side contract for PTCS `/actors`.

This is not the generic Canvas message renderer. When PTCS provides an `ActorsPage` / `ActorTopologyPage` DSL document and the extension supports that renderer, Dynamic must render the whole Actors page: node blocks, actor hierarchy tree, grid, cards, reload/report controls, and status UI. The page must group actors by `actorSystem@host:port` and order node blocks as PTCS Host, GW Host, RN Host, then Unknown.

If Dynamic does not support `ActorsPage`, it must return not-supported and let PTCS use its fallback tree/grid/table. It must not display a Canvas summary card, raw JSON preview, or `Expand Canvas` button as a substitute for ActorsPage support.

First implementation slice keeps the renderer inside `Client/ActorDynamicTab.fs` instead of a new client compile unit. In this checkout, WebSharper `10.1.5.674` crashes `wsfsc.exe` when a new `[<JavaScript>]` client file is added, and also crashes on `String.Contains`; the current classifier uses a single `IndexOf("ActorTopologyPage")` gate. This is sufficient for PTCS page renderer dispatch because PTCS only sends ActorsPage documents to page renderers.

Current package slice: `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta74`, paired with `PulseTrade.Comm.Spa 0.2.5-beta80`. The current Actor Argu Add Target Key schema is `[proxyActorAddress; "target-v1"; targetActorAddress; duTypeOrTemplateKey; canonicalArgString]`; the invalidated beta64 `actor-argu-proxy` hidden rewrite path is historical only. Add Actor Key keeps the native actor address as its key and relies on the PTCS fabric's installed platform provider for low-cognitive `string -> fCell2<string> -> native actor -> fCell2<string> -> reply` dispatch. Beta74 adds the canonical payload classifier that distinguishes unrelated/absent payloads from valid static Canvas/FormInput/ActorsPage/runtime documents and present-invalid SDUI reason codes；browser absence/present-invalid visual proof remains tracked separately。

Realtime TA新路徑使用`PulseTrade.Comm.Spa.Dynamic.Ptcs 0.1.0-alpha6-win1` server adapter、`PulseTrade.Comm.Spa.Dynamic.Ptcs.Client 0.1.0-alpha7-win4` pure WebSharper client adapter與`Renderer 0.1.0-alpha5`。server端exact PTCS beta85 + Contracts alpha4，持有authenticated transient context與canonical reducer；browser只走same-origin `/sync/ws`、bounded `ta-browser.v1` wire與Renderer，不接受任意URL或credential。真PTCS desktop/mobile gate位於`tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.LiveDemo`與`scripts/verify-ptcs-ta-live-playwright.fsx`。Legacy Dynamic bundle暫不reference新adapters；禁止raw JavaScript或HTTP polling。

`poc.full.nuget.journal.ACL.fsx` quiet startup mode must not capture `Console.Out` with a disposable `StringWriter`: Suave can retain the writer after `startWithSharing` returns, and later listener output can crash with `ObjectDisposedException`. The script now uses `TextWriter.Null` while suppressed. Latest quiet no-wait proof: `dotnet fsi --exec .\src\poc.full.nuget.journal.ACL.fsx -- --no-wait --local-port 18102 --github-port 18101 --cluster-port 18801 --pcsl-root .\.pcsl\verify.acl.beta56.dual-host.quiet`.

`0.1.3-beta56` keeps the normal Release WebSharper compile/pack path against PTCS `0.2.5-beta66`. If `wsfsc.exe` fails with `UnauthorizedAccessException` deleting `src\websharper.log`, stop stale `wsfscservice.exe`, remove the generated `src\websharper.log`, and rebuild; this is a compiler-service file-lock issue, not a Dynamic source failure.

The current renderer already builds a page-level Actors UI with action shell, count cards, host/port node blocks, hierarchy rows, grid rows, and boxed `+` / `-` tree toggles. A PTCS source-host Playwright MCP gate verified that Dynamic owns the `/actors` DOM when loaded: fallback rows are `0`, blocks are ordered PTCS Host -> GW Host -> RN Host, full `akka.tcp://...` addresses are visible, and tree toggle clicks actually collapse and expand rows. `0.1.3-beta24` restored the early hierarchy visual language inside each host block: virtual `/user` and `/system` ancestors stay in the relevant PTCS/GW/RN block, rows expose status dots and connector lines, and virtual parents no longer create a synthetic Unknown block. `0.1.3-beta27` keeps that hierarchy, adds stable row metadata (`data-node-kind`, `data-display-address`, `data-parent-id`) and virtual-path grouping, and is paired with PTCS `0.2.5-beta40` so accepted Dynamic pages do not mix PTCS core fallback `actor-node` / `actor-card` DOM underneath the Dynamic page. `0.1.3-beta29` adds the browser-local Actors report schedule start/stop control and supersedes `0.1.3-beta28`, whose pushed package used a stale source Release JS bundle where schedule stayed disabled. `0.1.3-beta30` adds offline-like status display and ordering support for the PTCS beta41 backend cleanup path; default public `/actors` reload now removes stale/offline node blocks before Dynamic renders the page. Latest public 81 health/alignment release is `live81-ptcs-beta66-dynamic-beta56-acl-reload-202607010805`; authenticated ActorsPage evidence is `G:\PulseTrade.fs\log\20260701\ptcs81-beta66-actors-snapshot.md`, `G:\PulseTrade.fs\log\20260701\ptcs81-beta66-actors.png`, and `G:\PulseTrade.fs\log\20260701\ptcs81-beta66-actors-console.txt`. The reusable accepted-path gate is `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Scripts\verify.actorsPageDynamic.playwright.fsx`; the PTCS fallback gate is `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Scripts\verify.actorsActorTree.playwright.fsx -- --with-unsupported-client-extension`, with Playwright MCP evidence at `G:\PulseTrade.fs\log\20260629\20260629001159.actors-unsupported-fallback-playwright-mcp.png`. `src\poc.full.nuget.2.fsx` is the beta41/beta30 POC that keeps Actor Argu Add target key available while disabling `+ Page` Actor Dynamic page creation; it also registers its real `nuget2-echo` actor into PTCS actor registry so `/actors` is non-empty. POC2 browser evidence is `G:\PulseTrade.fs\log\20260629\poc2-actors-page-fixed-deep-snapshot.md` and `.png`. `src\poc.full.nuget.fsx` remains the original beta40/beta29 full POC. Full strict schema parsing, server-side persisted report scheduling, restart/cache sync, and failover visual states remain tracked by `DYN-WBS-519`.

## Verification

Current beta44 package verification uses the normal Release build from this repo path:

```powershell
dotnet build .\src\PulseTrade.Comm.Spa.Dynamic.fsproj -c Release
dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-restore
```

`PulseTrade.Comm.Spa.Dynamic` is developed under a long Windows path. If WebSharper `wsfsc.exe` crashes without diagnostics, first inspect whether stale `wsfscservice.exe` has locked generated `src\websharper.log`; stop that service and remove the generated log before retrying. If the problem is genuinely path-length related, use the short-path fallback command recorded in `doc/Verification.md`:

```powershell
dotnet build .\src\PulseTrade.Comm.Spa.Dynamic.fsproj -c Debug `
  -p:BaseIntermediateOutputPath=C:\ptcsdyn-build\obj\ `
  -p:OutputPath=C:\ptcsdyn-build\bin\
```

## WebSharper Bundle Project & NuGet Packaging Quirks

When developing a WebSharper `Bundle` project (or `spa` project) that is intended to be distributed as a NuGet package, there is an important behavior to note regarding how static assets (`wwwroot`) are packaged and consumed.

### The Problem
By default, when you run `dotnet pack` on a WebSharper `Bundle` project, the compiled JavaScript files and static assets (located in `wwwroot/js/`) are **NOT** placed alongside the compiled `.dll` inside the `lib/<tfm>/` directory of the resulting `.nupkg`.

Depending on the WebSharper/MSBuild pack path, they can be packed under either:

- `content/wwwroot/js/`
- `contentFiles/any/<tfm>/wwwroot/js/`

If your server-side F# code (e.g., an extension registration mechanism using `typeof<SomeActor>.Assembly.Location`) attempts to locate these static assets at runtime by looking relative to the DLL's path, it will fail when the library is consumed via a NuGet reference (like `#r "nuget: MyPackage"` or `<PackageReference>`). The DLL will be running from `~/.nuget/packages/.../lib/<tfm>/`, where no `wwwroot` folder exists.

## Dynamic demo actor contracts

- showcase-dynamic-actor：收到合法 fskynet-sdui payload時回顯該 payload；其他輸入回 simple showcase。
- sdui-echo-actor：回顯 ActorArguTargetCommand.RawArgu，供任意 Canvas DSL round-trip。
- showcase-dynamic-actor2：固定回 built-in complex showcase2；輸入 payload不會改變 marqueeData，這是 fixed showcase by design。

以上 actor都使用 ActorArguTargetCommand -> ActorArguTargetReply contract。string-only/fCell2-only legacy actor需透過 explicit Add Target Key指定 proxy與 native target，不可直接當作 Add actor key target。

### The Solution
To resolve these static assets from a NuGet cache, probe the local build path first and then both NuGet layouts. Do not assume that every generated package uses `contentFiles`.

Example of how to locate the `wwwroot/js` folder robustly:

```fsharp
let assembly = typeof<ShowcaseDemoActor>.Assembly
let dllPath = assembly.Location
let dir = System.IO.Path.GetDirectoryName(dllPath)

// Local development path (e.g., bin/Debug/net10.0/wwwroot/js)
let localJsDir = System.IO.Path.Combine(dir, "wwwroot", "js")

// NuGet package layouts seen in supported WebSharper pack flows.
let contentJsDir = System.IO.Path.Combine(dir, "..", "..", "content", "wwwroot", "js")
let contentFilesJsDir = System.IO.Path.Combine(dir, "..", "..", "contentFiles", "any", "net10.0", "wwwroot", "js")

// Fallback logic
let jsDir =
    [ localJsDir; contentJsDir; contentFilesJsDir ]
    |> List.tryFind System.IO.Directory.Exists

match jsDir with
| Some jsDir ->
    // Successfully found the assets, proceed to serve them
    let allJsFiles = System.IO.Directory.GetFiles(jsDir, "*.js", System.IO.SearchOption.AllDirectories)
    // ...
| None ->
    // Register no script URL and report the missing extension asset explicitly.
    ()
```

The beta72 server extension implements this ordered discovery. Host/demo deployment may also copy the discovered files into its extension directory, but it must copy from the package's actual layout rather than hard-code one path.
