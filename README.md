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

The proxy key UI only builds the binding. The selected key still routes the command to `proxyActorAddress` because it remains the first key segment. The actual PTCS durable proxy actor and RN Host target are deployment/runtime concerns owned outside this package.

## ActorsPage Renderer

`RFC-PTCS-DYNAMIC-0005.actors-page-renderer.md` defines the Dynamic-side contract for PTCS `/actors`.

This is not the generic Canvas message renderer. When PTCS provides an `ActorsPage` / `ActorTopologyPage` DSL document and the extension supports that renderer, Dynamic must render the whole Actors page: node blocks, actor hierarchy tree, grid, cards, reload/report controls, and status UI. The page must group actors by `actorSystem@host:port` and order node blocks as PTCS Host, GW Host, RN Host, then Unknown.

If Dynamic does not support `ActorsPage`, it must return not-supported and let PTCS use its fallback tree/grid/table. It must not display a Canvas summary card, raw JSON preview, or `Expand Canvas` button as a substitute for ActorsPage support.

First implementation slice keeps the renderer inside `Client/ActorDynamicTab.fs` instead of a new client compile unit. In this checkout, WebSharper `10.1.5.674` crashes `wsfsc.exe` when a new `[<JavaScript>]` client file is added, and also crashes on `String.Contains`; the current classifier uses a single `IndexOf("ActorTopologyPage")` gate. This is sufficient for PTCS page renderer dispatch because PTCS only sends ActorsPage documents to page renderers.

The current renderer already builds a page-level Actors UI with action shell, count cards, host/port node blocks, hierarchy rows, grid rows, and boxed `+` / `-` tree toggles. A PTCS source-host Playwright MCP gate verified that Dynamic owns the `/actors` DOM when loaded: fallback rows are `0`, blocks are ordered PTCS Host -> GW Host -> RN Host, full `akka.tcp://...` addresses are visible, and tree toggle clicks actually collapse and expand rows. `0.1.3-beta24` restores the early hierarchy visual language inside each host block: virtual `/user` and `/system` ancestors stay in the relevant PTCS/GW/RN block, rows expose status dots and connector lines, and virtual parents no longer create a synthetic Unknown block. Public 81 is redeployed as `live81-ptcs-beta39-dynamic-beta24-hierarchy-restore-202606282340` with PTCS `0.2.5-beta39`; evidence is `G:\PulseTrade.fs\log\20260628\20260628233906.actors-page-dynamic-beta24-hierarchy.png` and `G:\PulseTrade.fs\log\20260628\20260628234106.actors-public81-beta24-hierarchy.png`. NuGet push for `0.1.3-beta24` was accepted; immediate flat-container lookup was still propagation-pending. The reusable accepted-path gate is `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Scripts\verify.actorsPageDynamic.playwright.fsx`. Full strict schema parsing, production report actions, restart/failover visual states, and Dynamic absent/unsupported renderer failure-injection remain tracked by `DYN-WBS-519`.

## Verification

`PulseTrade.Comm.Spa.Dynamic` is developed under a long Windows path. WebSharper `wsfsc.exe` can crash without diagnostics when both intermediate and output paths stay under this repo path. Use the short-path build command recorded in `doc/Verification.md` for package verification:

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
