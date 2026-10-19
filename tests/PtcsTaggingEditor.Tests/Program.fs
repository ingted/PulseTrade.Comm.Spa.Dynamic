open Expecto
open PulseTrade.Comm.Spa.Dynamic.Tagging.Client

let target space kind targetId label =
    { SpaceId=space;TargetKind=kind;TargetId=targetId;ContextLabel=label }

let tests =
    testList "PTCS Dynamic tagging editor Plug-In" [
        testCase "length-prefixed target key does not collide on separators" (fun _ ->
            let first = target "a/" "b" "c" "first"
            let second = target "a" "/b" "c" "second"
            Expect.notEqual (TaggingEditorPlugin.targetKey first) (TaggingEditorPlugin.targetKey second) "Target tuple boundaries must be authoritative.")

        testCase "target label is excluded from identity" (fun _ ->
            let first = target "space" "kind" "42" "old label"
            let second = target "space" "kind" "42" "new label"
            Expect.equal (TaggingEditorPlugin.targetKey first) (TaggingEditorPlugin.targetKey second) "Display context must not create a second logical editor.")

        testCase "default SDUI schema exposes generic tagging fields" (fun _ ->
            let fields = TaggingEditorPlugin.defaultSchema.Fields |> Array.map _.Key
            Expect.sequenceEqual fields [|"tags";"summary";"timeFrames"|] "The default schema must stay domain-neutral."
            Expect.isOk (TaggingEditorPlugin.validateOptions TaggingEditorPlugin.defaultOptions) "Default options must validate.")

        testCase "open editor bound is fail closed" (fun _ ->
            let invalid = { TaggingEditorPlugin.defaultOptions with MaxOpenEditors=0 }
            Expect.equal (TaggingEditorPlugin.validateOptions invalid) (Error "tagging-editor-max-open-invalid") "Zero open editors is not a useful host contract.")

        testCase "distinct domain adapters share the same binding contract" (fun _ ->
            let requests = ResizeArray<string*TaggingEditorRequest>()
            let bindingFor name targetValue =
                { Target=targetValue
                  Dispatch=fun request complete ->
                      requests.Add((name,request))
                      complete (Ok(TaggingEditorReply.Listed [||])) }
            let messaging = bindingFor "messaging" (target "message-space" "message" "27" "sender")
            let market = bindingFor "market" (target "market-space" "bar" "ES:1m:202610191230" "ES 1m")
            messaging.Dispatch TaggingEditorRequest.List ignore
            market.Dispatch TaggingEditorRequest.List ignore
            Expect.equal (requests |> Seq.map fst |> Seq.toList) ["messaging";"market"] "The public callback must not depend on one target domain.")
    ]

[<EntryPoint>]
let main argv = runTestsWithCLIArgs [] argv tests
