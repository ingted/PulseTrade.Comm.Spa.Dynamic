# PulseTrade.Comm.Spa.Dynamic

This project is a dynamic SDUI (Server-Driven UI) and Actor extension for `PulseTrade.Comm.Spa`, built with WebSharper.

## Actor Dynamic / Actor Argu modes

`PulseTrade.Comm.Spa.Dynamic` owns Dynamic rendering and Dynamic target binding; `PulseTrade.Comm.Spa` owns the append page shell, key registry, pending replay, and actor-argu command path.

Current mode split:

| PTCS page | Dynamic support |
| --- | --- |
| Actor Argu | FormInput only. No canvas rendering and no Add proxy key. |
| Actor Dynamic | Direct actor key, DU/FormInput target key, and Dynamic proxy key. Canvas is used only when the actor reply payload is `schema=fskynet-sdui` JSON DSL. |

Key shapes:

```text
Direct actor key:
[ actorAddress ]

DU/FormInput target:
[ actorAddress; duTypeOrTemplateKey; canonicalArgString ]

Dynamic proxy key:
[ proxyActorAddress; "proxy-v1"; rnActorAddress; targetKind ]
```

When an Actor Dynamic selected key is only `[ actorAddress ]`, the extension intentionally returns no FormInput renderer. PTCS fallback input accepts arbitrary text or JSON DSL. If the actor replies with Canvas JSON DSL, the Dynamic message renderer draws the canvas; otherwise PTCS normal message rendering is used.

The proxy key UI only builds the binding. The selected key still routes the command to `proxyActorAddress` because it remains the first key segment. PTCS core sends `ActorArguTargetCommand.ActorAddress = proxyActorAddress` and `RawArgu = <input text>`; it does not include the remaining selected-key segments in the command envelope. Therefore `rnActorAddress` is binding/diagnostic data for the Dynamic/RN deployment path, not a PTCS core route selector. Current no-core-change deployments use one target key -> one proxy actor/spec. A future shared-proxy design needs a PTCS route-envelope/resolver RFC before it can choose many native targets at send time.

## ActorsPage Renderer

`RFC-PTCS-DYNAMIC-0005.actors-page-renderer.md` defines the Dynamic-side contract for PTCS `/actors`.

This is not the generic Canvas message renderer. When PTCS provides an `ActorsPage` / `ActorTopologyPage` DSL document and the extension supports that renderer, Dynamic must render the whole Actors page: node blocks, actor hierarchy tree, grid, cards, reload/report controls, and status UI. The page must group actors by `actorSystem@host:port` and order node blocks as PTCS Host, GW Host, RN Host, then Unknown.

If Dynamic does not support `ActorsPage`, it must return not-supported and let PTCS use its fallback tree/grid/table. It must not display a Canvas summary card, raw JSON preview, or `Expand Canvas` button as a substitute for ActorsPage support.

First implementation slice keeps the renderer inside `Client/ActorDynamicTab.fs` instead of a new client compile unit. In this checkout, WebSharper `10.1.5.674` crashes `wsfsc.exe` when a new `[<JavaScript>]` client file is added, and also crashes on `String.Contains`; the current classifier uses a single `IndexOf("ActorTopologyPage")` gate. This is sufficient for PTCS page renderer dispatch because PTCS only sends ActorsPage documents to page renderers.

Current package slice: `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta56`, paired with `PulseTrade.Comm.Spa 0.2.5-beta66`. `src\poc.full.nuget.journal.ACL.fsx -- --no-wait` verifies the ACL/Login dual-auth path: 81-style GitHub OAuth host, 82-style PTCS.Login local host, DamnWZ/AssTerry pages, Echo/PingPong target keys, ACL snapshot, Dynamic bundle, durable ActorArgu echo, PingPong stop filtering, fixed actor name reuse after stop, PTCS WebSocket ACL gate availability, PTCS.Login beta53 session-store injection package compatibility, PTCS beta54 JSONL ACL audit sink compatibility, PTCS beta55 WebSocket principal revalidation compatibility, PTCS beta56 WebSocket proxy cleanup compatibility, PTCS beta57 HTTP ACL canonical resource compatibility, PTCS beta58 TLS-offload same-origin compatibility, PTCS beta63 protected API fetch credentials, PTCS beta64 SQL audit sink compatibility, PTCS beta65 ACL policy hot-reload compatibility, and PTCS beta66 protected ACL policy reload endpoint compatibility. Playwright MCP verified public 81 `/actors`, admin/Terry ACL FormInput behavior, and `/page/assterry` WebSocket send/reply through deployed release `live81-ptcs-beta66-dynamic-beta56-acl-reload-202607010805`.

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

Instead, they are packed into the `contentFiles` directory, specifically:
`contentFiles/any/<tfm>/wwwroot/js/`

If your server-side F# code (e.g., an extension registration mechanism using `typeof<SomeActor>.Assembly.Location`) attempts to locate these static assets at runtime by looking relative to the DLL's path, it will fail when the library is consumed via a NuGet reference (like `#r "nuget: MyPackage"` or `<PackageReference>`). The DLL will be running from `~/.nuget/packages/.../lib/<tfm>/`, where no `wwwroot` folder exists.

### The Solution
To successfully resolve these static assets when running from a NuGet cache, you must instruct your server code to traverse up the directory structure and look inside the `contentFiles` folder.

Example of how to locate the `wwwroot/js` folder robustly:

```fsharp
let assembly = typeof<ShowcaseDemoActor>.Assembly
let dllPath = assembly.Location
let dir = System.IO.Path.GetDirectoryName(dllPath)

// Local development path (e.g., bin/Debug/net10.0/wwwroot/js)
let localJsDir = System.IO.Path.Combine(dir, "wwwroot", "js")

// NuGet cache path (e.g., ~/.nuget/packages/.../contentFiles/any/net10.0/wwwroot/js)
let nugetJsDir = System.IO.Path.Combine(dir, "..", "..", "contentFiles", "any", "net10.0", "wwwroot", "js")

// Fallback logic
let jsDir = if System.IO.Directory.Exists(localJsDir) then localJsDir else nugetJsDir

if System.IO.Directory.Exists(jsDir) then
    // Successfully found the assets, proceed to serve them
    let allJsFiles = System.IO.Directory.GetFiles(jsDir, "*.js", System.IO.SearchOption.AllDirectories)
    // ...
```

By incorporating this fallback, your extension can seamlessly transition between local FSI script testing (`#I "bin/Release/..."`) and remote NuGet consumption (`#r "nuget: ..."`).
