namespace PulseTrade.Comm.Spa.Login

open System
open System.IO
open System.Net
open System.Reflection
open System.Text
open System.Text.Json
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.Writers
open PulseTrade.Comm.ACL.Core
open PulseTrade.Comm.Login.Core
open PulseTrade.Comm.Spa
open WebSharper
open WebSharper.JavaScript
open WebSharper.JavaScript.Dom

type PtcsLoginSubmitRequest =
    { UserName: string
      Password: string
      ReturnUrl: string
      KeepSession: bool }

type PtcsLoginSubmitReply =
    { Status: string
      ReturnUrl: string
      UserId: string
      DisplayName: string
      ReasonCode: string }

type PtcsLoginPageConfig =
    { SubmitPath: string
      SessionPath: string
      LogoutPath: string
      ReturnUrl: string
      ProtectedRoute: string
      SessionCookieName: string
      Title: string
      Lead: string
      ProviderLabel: string
      AclLabel: string }

type PtcsLoginOptions =
    { Core: LoginCoreOptions
      Cookie: LoginCookiePolicy
      LoginPath: string
      SubmitPath: string
      SessionPath: string
      LogoutPath: string
      DefaultReturnUrl: string
      Title: string
      Lead: string
      ProviderLabel: string
      AclLabel: string
      ClientExtensionScriptUrls: string list }

type PtcsLoginCoreDependencies =
    { TokenIssuer: ILoginTokenIssuer
      CredentialVerifier: LoginConfigSnapshot -> ILoginCredentialVerifier
      SessionStore: ILoginSessionStore
      AttemptGate: ILoginAttemptGate option
      AuditSink: ILoginAuditSink option
      Clock: unit -> DateTimeOffset
      RateLimitKey: LoginRequest -> string }

module PtcsLogin =
    let jsonOptions =
        JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true)

    let serialize value =
        JsonSerializer.Serialize(value, jsonOptions)

    let deserialize<'T> (text: string) =
        JsonSerializer.Deserialize<'T>(text, jsonOptions)

    let bodyText (ctx: HttpContext) =
        Encoding.UTF8.GetString ctx.request.rawForm

    let query name (ctx: HttpContext) =
        match ctx.request.queryParam name with
        | Choice1Of2 value when not (String.IsNullOrWhiteSpace value) -> Some(value.Trim())
        | _ -> None

    let clientIp (ctx: HttpContext) =
        match ctx.request.headers |> Seq.tryFind (fun (key, _) -> System.String.Equals(key, "x-forwarded-for", StringComparison.OrdinalIgnoreCase)) with
        | Some(_, value) when not (String.IsNullOrWhiteSpace value) ->
            value.Split(',').[0].Trim()
            |> Some
        | _ -> Some(ctx.connection.ipAddr.ToString())

    let userAgent (ctx: HttpContext) =
        ctx.request.headers
        |> Seq.tryFind (fun (key, _) -> System.String.Equals(key, "user-agent", StringComparison.OrdinalIgnoreCase))
        |> Option.map snd

    let httpsRequest (ctx: HttpContext) =
        ctx.request.url.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)
        || (ctx.request.headers
            |> Seq.exists (fun (key, value) ->
                System.String.Equals(key, "x-forwarded-proto", StringComparison.OrdinalIgnoreCase)
                && System.String.Equals(value, "https", StringComparison.OrdinalIgnoreCase)))

    let secureCookieSuffix options ctx =
        if options.Cookie.Secure || httpsRequest ctx then "; Secure" else ""

    let sessionCookie options ctx sessionId =
        let maxAge = int options.Core.Security.AccessTokenTtl.TotalSeconds
        $"{options.Cookie.Name}={WebUtility.UrlEncode sessionId}; Path=/; HttpOnly; SameSite={options.Cookie.SameSite}; Max-Age={maxAge}{secureCookieSuffix options ctx}"

    let clearCookie options =
        $"{options.Cookie.Name}=; Path=/; HttpOnly; SameSite={options.Cookie.SameSite}; Max-Age=0"

    let safeReturn options value =
        let fallback = BrowserAuth.safeReturnUrl options.DefaultReturnUrl
        let candidate = BrowserAuth.safeReturnUrl value

        let allowed =
            options.Core.Security.ReturnUrlWhitelist
            |> List.exists (fun prefix -> BrowserAuth.isPathOrChild prefix candidate || System.String.Equals(prefix, candidate, StringComparison.OrdinalIgnoreCase))

        if allowed then candidate else fallback

    let loginCss =
        """
    :root {
      --bg: #f5f7fb;
      --panel: #ffffff;
      --ink: #142033;
      --muted: #60708a;
      --line: #d7dfeb;
      --line-strong: #b8c5d8;
      --primary: #2857c7;
      --primary-strong: #1f45a0;
      --ok: #0d7f62;
      --warn: #9a5b00;
      --danger: #b42318;
      --shadow: 0 18px 42px rgba(37, 53, 84, 0.14);
      font-family: "Segoe UI", "Noto Sans TC", Arial, sans-serif;
    }
    * { box-sizing: border-box; }
    body {
      margin: 0;
      min-height: 100vh;
      color: var(--ink);
      background:
        linear-gradient(90deg, rgba(20, 32, 51, 0.04) 1px, transparent 1px),
        linear-gradient(180deg, rgba(20, 32, 51, 0.04) 1px, transparent 1px),
        var(--bg);
      background-size: 32px 32px;
    }
    .shell { min-height: 100vh; display: grid; place-items: center; padding: 32px; }
    .login-frame {
      width: min(1040px, 100%);
      min-height: 620px;
      display: grid;
      grid-template-columns: minmax(320px, 0.92fr) minmax(360px, 1.08fr);
      border: 1px solid var(--line);
      border-radius: 8px;
      background: var(--panel);
      box-shadow: var(--shadow);
      overflow: hidden;
    }
    .system-panel {
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      gap: 32px;
      padding: 34px;
      border-right: 1px solid var(--line);
      background: #eef3f9;
    }
    .brand-mark {
      width: 46px;
      height: 46px;
      display: grid;
      place-items: center;
      border: 1px solid #9fb0c8;
      border-radius: 8px;
      background: #fff;
      color: var(--primary);
      font-weight: 800;
      letter-spacing: 0;
    }
    .brand-title { margin: 18px 0 0; font-size: 28px; line-height: 1.15; font-weight: 700; letter-spacing: 0; }
    .brand-subtitle { margin: 10px 0 0; max-width: 360px; color: var(--muted); font-size: 15px; line-height: 1.55; }
    .route-list { display: grid; gap: 10px; margin: 0; padding: 0; list-style: none; }
    .route-item {
      display: grid;
      grid-template-columns: 26px 1fr;
      gap: 10px;
      align-items: start;
      padding: 12px;
      border: 1px solid #cbd6e6;
      border-radius: 8px;
      background: rgba(255, 255, 255, 0.7);
    }
    .route-icon {
      width: 26px;
      height: 26px;
      display: grid;
      place-items: center;
      border: 1px solid #9fb0c8;
      border-radius: 6px;
      background: #fff;
      color: var(--primary);
      font-size: 13px;
      font-weight: 700;
    }
    .route-name { margin: 0; font-size: 13px; font-weight: 700; }
    .route-value { margin: 3px 0 0; color: var(--muted); font-family: Consolas, "Cascadia Mono", monospace; font-size: 12px; overflow-wrap: anywhere; }
    .form-panel { display: flex; align-items: center; padding: 42px; }
    .form-card { width: 100%; max-width: 460px; margin: 0 auto; }
    .status-row { display: flex; flex-wrap: wrap; gap: 8px; margin-bottom: 22px; }
    .pill { display: inline-flex; align-items: center; gap: 6px; min-height: 28px; padding: 5px 9px; border: 1px solid var(--line); border-radius: 999px; color: var(--muted); background: #fff; font-size: 12px; line-height: 1.2; }
    .dot { width: 8px; height: 8px; border-radius: 50%; background: var(--ok); }
    .dot.warn { background: var(--warn); }
    h1 { margin: 0; font-size: 32px; line-height: 1.15; font-weight: 750; letter-spacing: 0; }
    .lead { margin: 12px 0 30px; color: var(--muted); font-size: 15px; line-height: 1.55; }
    form { display: grid; gap: 16px; }
    .field { display: grid; gap: 7px; }
    label { font-size: 13px; font-weight: 650; color: #26364f; }
    input { width: 100%; min-height: 44px; padding: 10px 12px; border: 1px solid var(--line-strong); border-radius: 7px; color: var(--ink); background: #fff; font: inherit; letter-spacing: 0; outline: none; }
    input:focus { border-color: var(--primary); box-shadow: 0 0 0 3px rgba(40, 87, 199, 0.16); }
    .inline-row { display: flex; align-items: center; justify-content: space-between; gap: 16px; margin-top: 2px; }
    .checkbox-row { display: inline-flex; align-items: center; gap: 8px; color: var(--muted); font-size: 13px; }
    .checkbox-row input { width: 16px; min-height: 16px; height: 16px; padding: 0; accent-color: var(--primary); }
    .link { color: var(--primary); font-size: 13px; font-weight: 650; text-decoration: none; }
    .link:hover { text-decoration: underline; }
    button { width: 100%; min-height: 46px; margin-top: 8px; border: 1px solid var(--primary); border-radius: 7px; background: var(--primary); color: #fff; font: inherit; font-weight: 720; letter-spacing: 0; cursor: pointer; }
    button:hover { background: var(--primary-strong); }
    button:disabled { opacity: 0.62; cursor: wait; }
    .error-box { display: none; margin: 0 0 18px; padding: 10px 12px; border: 1px solid #f0b4ae; border-radius: 7px; color: var(--danger); background: #fff4f2; font-size: 13px; line-height: 1.45; }
    .error-box.visible { display: block; }
    .footer-note { margin-top: 22px; color: var(--muted); font-size: 12px; line-height: 1.5; }
    .footer-note code { color: #34445d; font-family: Consolas, "Cascadia Mono", monospace; font-size: 12px; }
    @media (max-width: 820px) {
      .shell { padding: 16px; align-items: stretch; }
      .login-frame { min-height: auto; grid-template-columns: 1fr; }
      .system-panel { border-right: 0; border-bottom: 1px solid var(--line); padding: 24px; }
      .form-panel { padding: 26px 22px 30px; }
      h1 { font-size: 27px; }
      .brand-title { font-size: 24px; }
    }
"""

    let safeClientExtensionScriptUrl (value: string) =
        if String.IsNullOrWhiteSpace value then
            None
        else
            let text = value.Trim()

            if text.Contains("://", StringComparison.Ordinal)
               || text.Contains("\\", StringComparison.Ordinal)
               || text.StartsWith("//", StringComparison.Ordinal)
               || text.StartsWith("../", StringComparison.Ordinal)
               || text.Contains("/../", StringComparison.Ordinal) then
                None
            elif text.StartsWith("/", StringComparison.Ordinal) || text.StartsWith("./", StringComparison.Ordinal) then
                Some text
            else
                Some("/" + text.TrimStart('/'))

    let loginClientExtensionScriptTags (scriptUrls: string list) =
        scriptUrls
        |> List.choose safeClientExtensionScriptUrl
        |> List.distinctBy _.ToLowerInvariant()
        |> List.map (fun url -> $"  <script type=\"module\" src=\"{WebUtility.HtmlEncode url}\"></script>")
        |> String.concat "\n"

    let loginClientExtensionRegistryBootstrap =
        """  <script>
    window.PulseTrade = window.PulseTrade || {};
    window.PulseTrade.MessageRenderers = window.PulseTrade.MessageRenderers || [];
    window.PulseTrade.PageRenderers = window.PulseTrade.PageRenderers || [];
    window.PulseTrade.AppendInputRenderers = window.PulseTrade.AppendInputRenderers || [];
    window.PulseTrade.AddKeyRenderers = window.PulseTrade.AddKeyRenderers || [];
    window.PulseTrade.LoginRenderers = window.PulseTrade.LoginRenderers || [];
    window.PulseTrade.AclSnapshotObservers = window.PulseTrade.AclSnapshotObservers || [];
    window.PulseTrade.AclCapabilityProviders = window.PulseTrade.AclCapabilityProviders || [];
    window.PulseTrade.Renderers = window.PulseTrade.Renderers || window.PulseTrade.MessageRenderers;
    (function () {
      var register = function (collection, name, priority, func) {
        if (typeof priority === "function") {
          func = priority;
          priority = 0;
        }
        if (typeof func !== "function") return;
        collection.push({ name: String(name || "unnamed"), priority: Number(priority || 0), render: func });
        collection.sort(function (left, right) { return (right.priority || 0) - (left.priority || 0); });
      };
      window.PulseTradeRegisterRenderer = function (name, priority, func) {
        register(window.PulseTrade.MessageRenderers, name, priority, func);
      };
      window.PulseTradeRegisterPageRenderer = function (name, priority, func) {
        register(window.PulseTrade.PageRenderers, name, priority, func);
      };
      window.PulseTradeRegisterAppendInputRenderer = function (name, priority, func) {
        register(window.PulseTrade.AppendInputRenderers, name, priority, func);
      };
      window.PulseTradeRegisterAddKeyRenderer = function (name, priority, func) {
        register(window.PulseTrade.AddKeyRenderers, name, priority, func);
      };
      window.PulseTradeRegisterLoginRenderer = function (name, priority, func) {
        register(window.PulseTrade.LoginRenderers, name, priority, func);
      };
      window.PulseTradeRegisterAclSnapshotObserver = function (name, priority, func) {
        register(window.PulseTrade.AclSnapshotObservers, name, priority, func);
      };
      window.PulseTradeRegisterAclCapabilityProvider = function (name, priority, func) {
        register(window.PulseTrade.AclCapabilityProviders, name, priority, func);
      };
    })();
  </script>"""

    let withLoginPageClientExtensionScript scriptUrl options =
        let safeScripts =
            scriptUrl :: options.ClientExtensionScriptUrls
            |> List.choose safeClientExtensionScriptUrl
            |> List.distinctBy _.ToLowerInvariant()

        { options with ClientExtensionScriptUrls = safeScripts }

    let loginPageHtml options returnUrl =
        let config =
            { SubmitPath = options.SubmitPath
              SessionPath = options.SessionPath
              LogoutPath = options.LogoutPath
              ReturnUrl = safeReturn options returnUrl
              ProtectedRoute = safeReturn options returnUrl
              SessionCookieName = options.Cookie.Name
              Title = options.Title
              Lead = options.Lead
              ProviderLabel = options.ProviderLabel
              AclLabel = options.AclLabel }
            |> serialize
            |> fun text -> text.Replace("</", "<\\/")

        let clientExtensionScripts = loginClientExtensionScriptTags options.ClientExtensionScriptUrls

        $"""<!doctype html>
<html lang="zh-Hant">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>{WebUtility.HtmlEncode options.Title}</title>
  <style>{loginCss}</style>
</head>
<body>
  <main id="ptcs-login-root" class="shell"></main>
  <script id="ptcs-login-config" type="application/json">{config}</script>
{loginClientExtensionRegistryBootstrap}
{clientExtensionScripts}
  <script type="module" src="/build/PulseTrade.Comm.Spa.js"></script>
</body>
</html>"""

    let loginPage options : WebPart =
        fun ctx ->
            async {
                let returnUrl =
                    query "returnUrl" ctx
                    |> Option.defaultValue options.DefaultReturnUrl

                return! (BrowserAuth.noStore >=> setMimeType "text/html; charset=utf-8" >=> OK(loginPageHtml options returnUrl)) ctx
            }

    let submit options : WebPart =
        fun ctx ->
            async {
                try
                    if not (BrowserAuth.sameOriginOrMissing ctx) then
                        return! BrowserAuth.text HTTP_403 "Cross-origin login rejected.\n" ctx
                    else
                        let request = deserialize<PtcsLoginSubmitRequest>(bodyText ctx)

                        if isNull (box request) then
                            return! BrowserAuth.text HTTP_400 "Invalid JSON body\n" ctx
                        elif String.IsNullOrWhiteSpace request.UserName || String.IsNullOrWhiteSpace request.Password then
                            return! BrowserAuth.text HTTP_400 "username and password are required\n" ctx
                        else
                            let loginRequest =
                                { Credential = UserPassword(request.UserName.Trim(), request.Password)
                                  RequestedAudience = Some options.Core.Security.Audience
                                  ClientIp = clientIp ctx
                                  UserAgent = userAgent ctx
                                  ReturnUrl = Some(safeReturn options request.ReturnUrl) }

                            let! result = LoginCore.authenticateAsync options.Core loginRequest

                            match result with
                            | Microsoft.FSharp.Core.Error failure ->
                                let reply =
                                    { Status = "denied"
                                      ReturnUrl = ""
                                      UserId = ""
                                      DisplayName = ""
                                      ReasonCode = failure.ReasonCode }

                                return! (BrowserAuth.noStore >=> setMimeType "application/json; charset=utf-8" >=> OK(serialize reply) >=> setStatus HTTP_401) ctx
                            | Microsoft.FSharp.Core.Ok envelope ->
                                let sessionId = envelope.BrowserSessionId |> Option.defaultValue ""
                                let returnUrl = safeReturn options request.ReturnUrl
                                let reply =
                                    { Status = "ok"
                                      ReturnUrl = returnUrl
                                      UserId = envelope.Principal.UserId
                                      DisplayName = envelope.Principal.DisplayName |> Option.defaultValue envelope.Principal.Subject
                                      ReasonCode = "" }

                                return!
                                    (setHeader "Set-Cookie" (sessionCookie options ctx sessionId)
                                     >=> BrowserAuth.noStore
                                     >=> setMimeType "application/json; charset=utf-8"
                                     >=> OK(serialize reply))
                                        ctx
                with ex ->
                    return! BrowserAuth.text HTTP_400 (ex.Message + "\n") ctx
            }

    let session options : WebPart =
        fun ctx ->
            async {
                match BrowserAuth.cookieValue options.Cookie.Name ctx with
                | None -> return! BrowserAuth.text HTTP_401 "Login required.\n" ctx
                | Some sessionId ->
                    let! result = LoginCore.resolveSessionAsync options.Core sessionId

                    match result with
                    | Microsoft.FSharp.Core.Error failure -> return! BrowserAuth.text HTTP_401 (failure.ReasonCode + "\n") ctx
                    | Microsoft.FSharp.Core.Ok principal ->
                        return!
                            (BrowserAuth.noStore
                             >=> setMimeType "application/json; charset=utf-8"
                             >=> OK(
                                 serialize
                                     {| status = "ok"
                                        userId = principal.UserId
                                        displayName = principal.DisplayName |> Option.defaultValue principal.Subject
                                        provider = principal.Provider |}))
                                ctx
            }

    let logout options : WebPart =
        fun ctx ->
            async {
                match BrowserAuth.cookieValue options.Cookie.Name ctx with
                | None -> ()
                | Some sessionId ->
                    let! _ = LoginCore.revokeAsync options.Core sessionId
                    ()

                return!
                    (setHeader "Set-Cookie" (clearCookie options)
                     >=> BrowserAuth.redirect (options.LoginPath + "?returnUrl=" + WebUtility.UrlEncode options.DefaultReturnUrl))
                        ctx
            }

    let redirectToLogin options : WebPart =
        fun ctx ->
            async {
                let returnUrl =
                    query "returnUrl" ctx
                    |> Option.defaultValue options.DefaultReturnUrl
                    |> safeReturn options
                    |> WebUtility.UrlEncode

                return! BrowserAuth.redirect (options.LoginPath + "?returnUrl=" + returnUrl) ctx
            }

    let routes options =
        choose
            [ path options.LoginPath >=> loginPage options
              POST >=> path options.SubmitPath >=> submit options
              path options.SessionPath >=> session options
              path options.LogoutPath >=> logout options
              path "/chat/login" >=> redirectToLogin options
              path "/chat/logout" >=> logout options ]

    let tryUser options ctx =
        BrowserAuth.cookieValue options.Cookie.Name ctx
        |> Option.bind (fun sessionId ->
            match LoginCore.resolveSessionAsync options.Core sessionId |> Async.RunSynchronously with
            | Microsoft.FSharp.Core.Error _ -> None
            | Microsoft.FSharp.Core.Ok principal ->
                let subject =
                    if String.IsNullOrWhiteSpace principal.Subject then principal.UserId else principal.Subject

                let displayName =
                    principal.DisplayName
                    |> Option.defaultValue subject

                Some
                    { ParticipantId = "user.login." + principal.UserId.Replace(":", ".")
                      DisplayName = displayName
                      Login = subject
                      Authenticated = true
                      Provider = principal.Provider
                      Principal = Some principal
                      Labels =
                        [ yield "web"
                          yield "login"
                          yield principal.Provider
                          yield "subject:" + subject
                          for group in principal.Groups do
                              yield "group:" + group
                          for role in principal.Roles do
                              yield "role:" + role ] })

    let provider options =
        { ProviderId = "ptcs-login"
          LoginPath = options.LoginPath
          LogoutPath = options.LogoutPath
          BypassPaths =
            [ options.LoginPath
              options.SubmitPath
              options.SessionPath
              options.LogoutPath
              "/chat/login"
              "/chat/logout"
              "/assets"
              "/build"
              "/favicon.ico"
              "/healthz" ]
          Routes = routes options
          TryUser = tryUser options }

    let fromLoginCore core =
        { Core = core
          Cookie = LoginCookiePolicy.create core.Security.DeploymentProfile "ptc_login_session"
          LoginPath = "/login"
          SubmitPath = "/login/api/submit"
          SessionPath = "/login/api/session"
          LogoutPath = "/login/logout"
          DefaultReturnUrl = "/actors"
          Title = "登入 PTCS"
          Lead = "使用 host 提供的帳號登入。權限由登入後取得的 principal 與 ACL policy 決定。"
          ProviderLabel = "PTCS.Login"
          AclLabel = "ACL mode"
          ClientExtensionScriptUrls = [] }

    let coreFromConfigWithDependencies dependencies config =
        match LoginConfig.decode dependencies.Clock dependencies.RateLimitKey config with
        | Microsoft.FSharp.Core.Error error -> Microsoft.FSharp.Core.Error error
        | Microsoft.FSharp.Core.Ok snapshot ->
            let security =
                { snapshot.Security with
                    ReturnUrlWhitelist = [ "/"; "/chat"; "/sets"; "/actors"; "/fcell-chat"; "/fcell-list"; "/fcell-grid"; "/page" ] }

            Microsoft.FSharp.Core.Ok
                { TokenIssuer = dependencies.TokenIssuer
                  CredentialVerifier = dependencies.CredentialVerifier snapshot
                  SessionStore = dependencies.SessionStore
                  AttemptGate = dependencies.AttemptGate
                  AuditSink = dependencies.AuditSink
                  Clock = dependencies.Clock
                  Security = security }

    let localDevDependenciesWithSessionStore sessionStore =
        let clock () = DateTimeOffset.UtcNow
        let rateLimitKey (request: LoginRequest) = request.ClientIp |> Option.defaultValue "local"

        { TokenIssuer = OpaqueTokenIssuer()
          CredentialVerifier = fun snapshot -> InMemoryLoginCredentialVerifier(snapshot.Users, clock, "ptcs-login") :> ILoginCredentialVerifier
          SessionStore = sessionStore
          AttemptGate = None
          AuditSink = None
          Clock = clock
          RateLimitKey = rateLimitKey }

    let demoLocalDevWithSessionStore sessionStore =
        let config = LoginConfig.demo()
        let dependencies = localDevDependenciesWithSessionStore sessionStore

        coreFromConfigWithDependencies dependencies config
        |> Result.map fromLoginCore

    let demoLocalDev () =
        demoLocalDevWithSessionStore (InMemoryLoginSessionStore("memory"))

[<JavaScript>]
type LoginPageConfigDto =
    { submitPath: string
      sessionPath: string
      logoutPath: string
      returnUrl: string
      protectedRoute: string
      sessionCookieName: string
      title: string
      lead: string
      providerLabel: string
      aclLabel: string }

[<JavaScript>]
type LoginSubmitRequestDto =
    { userName: string
      password: string
      returnUrl: string
      keepSession: bool }

[<JavaScript>]
type LoginSubmitReplyDto =
    { status: string
      returnUrl: string
      userId: string
      displayName: string
      reasonCode: string }

[<JavaScript>]
module ClientBundle =
    let extensionId = "pulse-trade-comm-spa-login"

    let doc () = JS.Document

    let isBlank (value: string) =
        value = null || value.Trim() = ""

    let asText (value: string) =
        if isNull value || JS.TypeOf(box value) = JS.Kind.Undefined then "" else value

    let textOr fallback value =
        let text = asText value
        if isBlank text then fallback else text

    let element name className text =
        let node = (doc()).CreateElement(name)
        if not (isBlank className) then
            node.ClassName <- className
        if not (isNull text) then
            node.TextContent <- text
        node

    let setId value (node: #Element) =
        node.Id <- value
        node

    let setTestId value (node: #Element) =
        node.SetAttribute("data-testid", value)
        node

    let setHref value (node: #Element) =
        node.SetAttribute("href", value)
        node

    let append (parent: Element) (children: Node[]) =
        children |> Array.iter (fun child -> parent.AppendChild child |> ignore)
        parent

    let clear (node: Element) =
        while not (isNull node.FirstChild) do
            node.RemoveChild node.FirstChild |> ignore

    let input placeholder =
        let node = (doc()).CreateElement("input") :?> HTMLInputElement
        node.SetAttribute("placeholder", placeholder)
        node

    let button className text =
        let node = (doc()).CreateElement("button") :?> HTMLButtonElement
        if not (isBlank className) then
            node.ClassName <- className
        node.TextContent <- text
        node.SetAttribute("type", "button")
        node

    let decodeJson<'T> text =
        JSON.Parse(asText text) |> As<'T>

    let defaultConfig () =
        { submitPath = "/login/api/submit"
          sessionPath = "/login/api/session"
          logoutPath = "/login/logout"
          returnUrl = "/actors"
          protectedRoute = "/actors"
          sessionCookieName = "ptc_login_session"
          title = "登入 PTCS"
          lead = "使用 host 提供的帳號登入。權限由登入後取得的 principal 與 ACL policy 決定。"
          providerLabel = "PTCS.Login"
          aclLabel = "ACL mode" }

    let configFromJson configJson =
        if isBlank configJson then defaultConfig () else decodeJson<LoginPageConfigDto> configJson

    let errorMessage (error: obj) =
        if isNull error then "unknown error" else string error

    let postJson url payloadJson onOk onError =
        let headers = Headers()
        headers.Set("Content-Type", "application/json")

        let options = RequestOptions()
        options.Method <- "POST"
        options.Headers <- headers
        options.Body <- payloadJson

        let promise =
            JS.Window.Fetch(url, options)
                .Then<unit>(Func<Response, Promise<unit>>(fun response ->
                    response.Text()
                        .Then<unit>(Func<string, unit>(fun responseBody ->
                            if response.Ok then
                                onOk (if isBlank responseBody then "{}" else responseBody)
                            else
                                onError (if isBlank responseBody then $"POST {url} {response.Status}" else responseBody)))))

        promise.Catch<unit>(Func<obj, unit>(fun error -> onError (errorMessage error))) |> ignore

    let routeItem icon name value =
        let item = element "li" "route-item" null
        let content = element "div" "" null
        append
            content
            [| element "p" "route-name" name :> Node
               element "p" "route-value" value :> Node |]
        |> ignore
        append item [| element "span" "route-icon" icon :> Node; content :> Node |] |> ignore
        item

    let field labelText inputId (control: Element) =
        let wrap = element "div" "field" null
        let label = element "label" "" labelText
        label.SetAttribute("for", inputId)
        append wrap [| label :> Node; control :> Node |] |> ignore
        wrap

    let mountLogin (root: Element) configJson =
        let config = configFromJson configJson
        root.SetAttribute("data-ptcs-login-renderer", extensionId)
        if not (isNull (box (doc()).Body)) then
            (doc()).Body.SetAttribute("data-ptcs-login-renderer", extensionId)

        let frame = element "section" "login-frame" null
        frame.SetAttribute("aria-label", "PTCS Login")

        let systemPanel = element "aside" "system-panel" null
        let brand = element "div" "" null
        append
            brand
            [| element "div" "brand-mark" "PT" :> Node
               element "p" "brand-title" "PulseTrade Comm Spa" :> Node
               element
                   "p"
                   "brand-subtitle"
                   "本頁由 PulseTrade.Comm.Spa.Login extension bundle 呈現。登入成功後由 server 設定 HttpOnly session cookie，再回到受保護的 PTCS 頁面。"
                   :> Node |]
        |> ignore

        let routes = element "ul" "route-list" null
        routes.SetAttribute("aria-label", "Login context")
        append
            routes
            [| routeItem "P" "Protected route" (textOr "/actors" config.protectedRoute) :> Node
               routeItem "S" "Session cookie" (textOr "ptc_login_session" config.sessionCookieName) :> Node
               routeItem "A" "ACL mode" (textOr "enabled or authenticated-only" config.aclLabel) :> Node |]
        |> ignore
        append systemPanel [| brand :> Node; routes :> Node |] |> ignore

        let formPanel = element "section" "form-panel" null
        let card = element "div" "form-card" null
        let statusRow = element "div" "status-row" null
        let providerPill = element "span" "pill" null
        let bypassPill = element "span" "pill" null
        append providerPill [| element "span" "dot" "" :> Node; (doc()).CreateTextNode(textOr "PTCS.Login" config.providerLabel) :> Node |] |> ignore
        append bypassPill [| element "span" "dot warn" "" :> Node; (doc()).CreateTextNode("OAuth bypass") :> Node |] |> ignore
        append statusRow [| providerPill :> Node; bypassPill :> Node |] |> ignore

        let errorBox = element "p" "error-box" "登入失敗。請確認帳號或密碼。" |> setTestId "ptcs-login-error"
        errorBox.SetAttribute("role", "alert")

        let userName = input "admin" |> setId "username" |> setTestId "ptcs-login-username"
        userName.SetAttribute("name", "username")
        userName.SetAttribute("type", "text")
        userName.SetAttribute("autocomplete", "username")

        let password = input "輸入密碼" |> setId "password" |> setTestId "ptcs-login-password"
        password.SetAttribute("name", "password")
        password.SetAttribute("type", "password")
        password.SetAttribute("autocomplete", "current-password")

        let keepSession = (doc()).CreateElement("input") :?> HTMLInputElement
        keepSession.SetAttribute("name", "keepSession")
        keepSession.SetAttribute("type", "checkbox")
        keepSession.SetAttribute("value", "true")

        let inlineRow = element "div" "inline-row" null
        let checkboxLabel = element "label" "checkbox-row" null
        append checkboxLabel [| keepSession :> Node; (doc()).CreateTextNode("保持此瀏覽器登入") :> Node |] |> ignore
        let help = element "a" "link" "需要協助?" |> setHref "/login/help"
        append inlineRow [| checkboxLabel :> Node; help :> Node |] |> ignore

        let form = element "form" "" null
        form.SetAttribute("method", "post")
        form.SetAttribute("action", config.submitPath)
        let submit = button "primary" "登入並返回 PTCS" |> setTestId "ptcs-login-submit"

        let setError text =
            errorBox.TextContent <- textOr "登入失敗。請確認帳號或密碼。" text
            errorBox.ClassName <- "error-box visible"

        let clearError () =
            errorBox.ClassName <- "error-box"

        let submitLogin () =
            let request: LoginSubmitRequestDto =
                { userName = userName.Value.Trim()
                  password = password.Value
                  returnUrl = config.returnUrl
                  keepSession = keepSession.Checked }

            if isBlank request.userName || isBlank request.password then
                setError "請輸入帳號與密碼。"
            else
                clearError ()
                submit.SetAttribute("disabled", "disabled")
                submit.TextContent <- "登入中"

                postJson
                    config.submitPath
                    (JSON.Stringify request)
                    (fun responseBody ->
                        let reply = decodeJson<LoginSubmitReplyDto> responseBody
                        let target = textOr config.returnUrl reply.returnUrl
                        JS.Window.Location.Assign(target))
                    (fun error ->
                        submit.RemoveAttribute("disabled")
                        submit.TextContent <- "登入並返回 PTCS"
                        setError (if isBlank error then "登入失敗。請確認帳號或密碼。" else error))

        form.AddEventListener(
            "submit",
            Action<Event>(fun event ->
                event.PreventDefault()
                submitLogin ()))

        submit.AddEventListener("click", fun () -> submitLogin ())
        append
            form
            [| field "帳號" "username" userName :> Node
               field "密碼" "password" password :> Node
               inlineRow :> Node
               submit :> Node |]
        |> ignore

        append
            card
            [| statusRow :> Node
               element "h1" "" (textOr "登入 PTCS" config.title) :> Node
               element "p" "lead" (textOr "使用 host 提供的帳號登入。權限由登入後取得的 principal 與 ACL policy 決定。" config.lead) :> Node
               errorBox :> Node
               form :> Node
               element "p" "footer-note" "Browser flow 應只回 HttpOnly cookie；headless/API/WS 才使用 bearer token。提交端點示意為 /login/api/submit。" :> Node |]
        |> ignore
        append formPanel [| card :> Node |] |> ignore
        append frame [| systemPanel :> Node; formPanel :> Node |] |> ignore
        clear root
        root.AppendChild frame |> ignore
        true

    let registerLoginRenderer () =
        if JS.In "PulseTradeRegisterLoginRenderer" JS.Window then
            let renderer = Func<Element, string, bool>(fun root configJson -> mountLogin root configJson)
            JS.Inline("window.PulseTradeRegisterLoginRenderer('ptcs-login-page', 100, $0)", renderer)

    [<SPAEntryPoint>]
    let Main () =
        registerLoginRenderer ()
        JS.Global?console?log("PulseTrade.Comm.Spa.Login bundle loaded and registered login renderer")

[<RequireQualifiedAccess>]
module PtcsLoginExtension =
    let extensionId = "pulse-trade-comm-spa-login"
    let scriptUrl = "/client-extensions/login/PulseTrade.Comm.Spa.Login.js"
    let scriptBaseUrl = "/client-extensions/login/"
    let scriptFileName = "PulseTrade.Comm.Spa.Login.js"
    let runtimeScriptRelativePath = "WebSharper.Core.JavaScript/Runtime.js"
    let runtimeScriptUrl = scriptBaseUrl + runtimeScriptRelativePath

    let assetFileCandidates (relativePath: string) =
        let assembly = Assembly.GetExecutingAssembly()
        let assemblyDir = assembly.Location |> Path.GetDirectoryName
        let relativeParts = relativePath.Split([| '/'; '\\' |], StringSplitOptions.RemoveEmptyEntries)
        let combine (root: string) =
            relativeParts |> Array.fold (fun path part -> Path.Combine(path, part)) root

        [ combine (Path.Combine(assemblyDir, "wwwroot", "js"))
          Path.GetFullPath(combine (Path.Combine(assemblyDir, "..", "..", "contentFiles", "any", "net10.0", "wwwroot", "js"))) ]

    let readScriptAsset (relativePath: string) =
        let candidates =
            assetFileCandidates relativePath

        candidates
        |> List.tryFind File.Exists
        |> function
            | Some path -> File.ReadAllText(path, Encoding.UTF8)
            | None ->
                let locations = String.Join("; ", candidates)
                invalidOp $"PTCS.Login script asset {relativePath} not found. Tried: {locations}"

    let readScriptFile () =
        readScriptAsset scriptFileName

    let scriptContent () =
        readScriptFile ()

    let registerScriptAsset (url: string) (relativePath: string) (options: ServerOptions) =
        options.Hub.RegisterClientExtensionScriptAsset(
            { Url = url
              ContentType = "application/javascript"
              Content = readScriptAsset relativePath })
        |> ignore

    let registerClientBundle (options: ServerOptions) =
        options |> registerScriptAsset scriptUrl scriptFileName
        options |> registerScriptAsset runtimeScriptUrl runtimeScriptRelativePath

        options.Hub.RegisterClientExtension(
            { ExtensionId = extensionId
              DisplayName = Some "PTCS.Login"
              MetadataJson = Some """{"kind":"ptcs-login","package":"PulseTrade.Comm.Spa.Login","version":"0.1.0-alpha13"}"""
              ScriptUrls = [ scriptUrl ]
              AppendPageShapes = [] })
        |> ignore

        options

    let usePtcsLogin (login: PtcsLoginOptions) (options: ServerOptions) =
        let login =
            PtcsLogin.withLoginPageClientExtensionScript scriptUrl login

        options
        |> registerClientBundle
        |> Server.withBrowserAuth (PtcsLogin.provider login)

    let provider login =
        PtcsLogin.provider login

    let fromLoginCore core =
        PtcsLogin.fromLoginCore core

    let coreFromConfigWithDependencies dependencies config =
        PtcsLogin.coreFromConfigWithDependencies dependencies config

    let localDevDependenciesWithSessionStore sessionStore =
        PtcsLogin.localDevDependenciesWithSessionStore sessionStore

    let demoLocalDevWithSessionStore sessionStore =
        PtcsLogin.demoLocalDevWithSessionStore sessionStore

    let demoLocalDev () =
        PtcsLogin.demoLocalDev ()

[<AutoOpen>]
module ServerOptionsLoginExtensions =
    type ServerOptions with
        member this.usePtcsLogin(login: PtcsLoginOptions) =
            PtcsLoginExtension.usePtcsLogin login this
