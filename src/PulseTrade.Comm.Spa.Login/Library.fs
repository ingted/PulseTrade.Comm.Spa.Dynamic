namespace PulseTrade.Comm.Spa.Login

open System
open System.IO
open System.Reflection
open System.Text
open PulseTrade.Comm.Login.Core
open PulseTrade.Comm.Spa
open WebSharper
open WebSharper.JavaScript
open WebSharper.JavaScript.Dom

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
              MetadataJson = Some """{"kind":"ptcs-login","package":"PulseTrade.Comm.Spa.Login","version":"0.1.0-alpha10"}"""
              ScriptUrls = [ scriptUrl ]
              AppendPageShapes = [] })
        |> ignore

        options

    let usePtcsLogin (login: PtcsLoginOptions) (options: ServerOptions) =
        let login =
            PtcsLogin.withLoginPageClientExtensionScript scriptUrl login

        options
        |> registerClientBundle
        |> Server.withPtcsLogin login

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
