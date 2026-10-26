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

        testCase "semantic display label never replaces target identity" (fun _ ->
            let labelled = target "space" "bar" "109" "202607102000-30K-kbar"
            let fallback = target "space" "bar" "109" "  "
            Expect.equal (TaggingEditorPlugin.displayLabel labelled) "202607102000-30K-kbar" "The editor title uses the producer-authored semantic label."
            Expect.equal (TaggingEditorPlugin.displayLabel fallback) "109" "A missing display label falls back to the stable target identity."
            Expect.equal (TaggingEditorPlugin.targetKey labelled) (TaggingEditorPlugin.targetKey fallback) "Changing presentation metadata must not change dispatch identity.")

        testCase "draft rows precede persisted records and allocate identity only on save" (fun _ ->
            let value =
                { Tags=[|"draft"|]
                  Summary="pending"
                  TimeFrames=[||] }
            let persisted =
                { ExInfoId="918"
                  Target=target "space" "bar" "109" "202607102000-30K-kbar"
                  Draft={ value with Summary="saved" }
                  Revision=7.0
                  CreatedAtUtc="2026-07-10T20:01:00Z"
                  UpdatedAtUtc="2026-07-10T20:02:00Z" }
            match TaggingEditorPlugin.orderedRowSeeds [|value|] [|persisted,persisted.Draft|] with
            | [| TaggingEditorRowSeed.DraftRow draft; TaggingEditorRowSeed.PersistedRow(row,_) |] ->
                Expect.equal draft value "The unsaved draft is the first record."
                Expect.equal row.ExInfoId "918" "The persisted DB identity remains unchanged."
            | actual -> failtestf "Unexpected row order: %A" actual
            Expect.equal (TaggingEditorPlugin.saveRequest None value) (TaggingEditorRequest.Create value) "An unsaved draft sends Create without allocating a DB identity."
            Expect.isNone (TaggingEditorPlugin.deleteRequest None) "Deleting a draft is a local discard."
            Expect.equal
                (TaggingEditorPlugin.deleteRequest (Some persisted))
                (Some(TaggingEditorRequest.Delete("918",7.0)))
                "Deleting a persisted record carries its DB identity and revision.")

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
                      complete (Ok(TaggingEditorReply.Listed [||]))
                  Changed=ignore
                  InitialDraft=None }
            let messaging = bindingFor "messaging" (target "message-space" "message" "27" "sender")
            let market = bindingFor "market" (target "market-space" "bar" "ES:1m:202610191230" "ES 1m")
            messaging.Dispatch TaggingEditorRequest.List ignore
            market.Dispatch TaggingEditorRequest.List ignore
            Expect.equal (requests |> Seq.map fst |> Seq.toList) ["messaging";"market"] "The public callback must not depend on one target domain.")

        testCase "0.1.9 editor suggestion API remains source compatible" (fun _ ->
            let suggestionRequest = TaggingEditorRequest.SuggestTags("ord",8)
            let suggestionReply = TaggingEditorReply.Suggested [|"order";"ordered"|]
            match suggestionRequest,suggestionReply with
            | TaggingEditorRequest.SuggestTags(prefix,limit),TaggingEditorReply.Suggested values ->
                Expect.equal prefix "ord" "The suggestion prefix remains public."
                Expect.equal limit 8 "The suggestion limit remains public."
                Expect.sequenceEqual values [|"order";"ordered"|] "Suggested values remain public."
            | _ -> failtest "The 0.1.9 suggestion union cases must remain available.")

        testCase "0.1.9 search API remains source compatible" (fun _ ->
            let fields =
                [| { Field=TaggingSearchField.TagText;Label="Tags";InitiallySelected=true }
                   { Field=TaggingSearchField.SummaryText;Label="Summary";InitiallySelected=false } |]
            let query =
                { Text="order*"
                  Matcher=TaggingSearchMatcher.Wildcard
                  CaseSensitive=false
                  Fields=fields |> Array.map _.Field
                  Limit=25
                  Cursor="" }
            let page = { Items=[||];NextCursor="next";ScanLimited=false }
            let binding : TaggingSearchBinding =
                { ScopeLabel="research"
                  Fields=fields
                  Dispatch=fun actual complete ->
                      Expect.equal actual query "The public search query reaches the binding unchanged."
                      complete (Ok page)
                  Activate=fun _ complete -> complete (Ok()) }
            let mutable observed = None
            binding.Dispatch query (fun result -> observed <- Some result)
            Expect.equal observed (Some(Ok page)) "The public search binding and page remain usable."
            Expect.equal TaggingSearchPlugin.defaultOptions.MaxOpenWindows 8 "The search plug-in defaults remain available.")
    ]

[<EntryPoint>]
let main argv = runTestsWithCLIArgs [] argv tests
