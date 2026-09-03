namespace PulseTrade.Comm.Spa.Dynamic.Contracts

open System
open WebSharper

[<RequireQualifiedAccess>]
type RuntimePollState =
    | Unmounted
    | MountedIdle
    | Ready
    | PollInFlight
    | Backoff of retryAtUtc: DateTimeOffset
    | Suspended
    | PausedForResync
    | Disposed

type RuntimeIdentity =
    { DocumentId: DocumentId
      CanvasInstanceId: CanvasInstanceId }

type RuntimeViewState =
    { Values: Map<string, SduiValue> }

type RuntimeState =
    { Identity: RuntimeIdentity
      Document: TaWorkspaceDocument option
      Data: Map<string, SduiValue>
      DocumentRevision: int64
      DataRevision: int64
      LastTransportSequence: int64
      View: RuntimeViewState
      Poll: RuntimePollState
      LastError: RuntimeError option }

[<RequireQualifiedAccess>]
type RuntimeEffect =
    | NoEffect
    | RequestResync of CanvasInstanceId * lastDataRevision: int64
    | SubmitAction of SduiAction
    | SchedulePoll of TimeSpan
    | CancelPoll
    | ReportDiagnostic of DynamicDiagnostic

[<JavaScript; RequireQualifiedAccess>]
module RuntimeReducer =
    [<Inline "($left < $right ? -1 : ($left > $right ? 1 : 0))">]
    let compareOrdinalText (left: string) (right: string) =
        StringComparer.Ordinal.Compare(left, right)

    let initial identity =
        { Identity = identity
          Document = None
          Data = Map.empty
          DocumentRevision = 0L
          DataRevision = 0L
          LastTransportSequence = 0L
          View = { Values = Map.empty }
          Poll = RuntimePollState.Unmounted
          LastError = None }

    let knownDataRefs state =
        match state.Document with
        | None -> Set.empty
        | Some document ->
            [ yield document.RowsRef
              yield document.StatusRef
              for row in document.Rows do
                  yield! TaRowSpec.dataRefs row ]
            |> Set.ofList

    let knownTargetIds state =
        match state.Document with
        | None -> Set.empty
        | Some document ->
            [ yield document.WorkspaceId
              yield! document.Rows |> Array.map _.RowId ]
            |> Set.ofList

    let patchRuntimeError state (patch: RuntimePatch) =
        let refs = knownDataRefs state
        let targets = knownTargetIds state

        patch.Operations
        |> Array.tryPick (function
            | PatchOperation.ReplaceDataRef(dataRef, _)
            | PatchOperation.UpsertSeriesPoints(dataRef, _, _)
            | PatchOperation.RemoveSeriesBefore(dataRef, _, _)
            | PatchOperation.SetStatus(dataRef, _) when not (Set.contains dataRef refs) ->
                Some("unknown-data-ref", $"Patch dataRef `{dataRef}` is not registered by the document.")
            | PatchOperation.SetOptions(targetId, _) when not (Set.contains targetId targets) ->
                Some("unknown-target-id", $"Patch target `{targetId}` is not registered by the document.")
            | PatchOperation.UpsertSeriesPoints(dataRef, _, items) ->
                let existingCount =
                    match Map.tryFind dataRef state.Data with
                    | Some(SduiValue.Array values) -> values.Length
                    | _ -> 0

                if existingCount + items.Length > DynamicRuntimeDefaults.limits.MaxRetainedBarsPerSeries then
                    Some("limit-retained-bars", $"Series `{dataRef}` would exceed retained hard limit {DynamicRuntimeDefaults.limits.MaxRetainedBarsPerSeries}.")
                else
                    None
            | _ -> None)

    let upsertPoints keyField existing items =
        let existingItems =
            match existing with
            | Some(SduiValue.Array values) ->
                values
                |> Array.choose (function SduiValue.Object item when Map.containsKey keyField item -> Some item | _ -> None)
            | _ -> [||]

        Array.append existingItems items
        |> Array.fold (fun state item -> Map.add item[keyField] item state) Map.empty
        |> Map.toArray
        |> Array.map (snd >> SduiValue.Object)
        |> SduiValue.Array

    let compareValue left right =
        match left, right with
        | SduiValue.Number a, SduiValue.Number b -> compare a b
        | SduiValue.Text a, SduiValue.Text b -> compareOrdinalText a b
        | _ -> 0

    let applyPatch (state: RuntimeState) (patch: RuntimePatch) =
        let refs = knownDataRefs state

        patch.Operations
        |> Array.fold (fun (current: RuntimeState) operation ->
            match operation with
            | PatchOperation.ReplaceDataRef(dataRef, value) when Set.contains dataRef refs ->
                { current with Data = Map.add dataRef value current.Data }
            | PatchOperation.UpsertSeriesPoints(dataRef, keyField, items) when Set.contains dataRef refs ->
                let value = upsertPoints keyField (Map.tryFind dataRef current.Data) items
                { current with Data = Map.add dataRef value current.Data }
            | PatchOperation.RemoveSeriesBefore(dataRef, keyField, key) when Set.contains dataRef refs ->
                let next =
                    match Map.tryFind dataRef current.Data with
                    | Some(SduiValue.Array values) ->
                        values
                        |> Array.filter (function
                            | SduiValue.Object item -> item |> Map.tryFind keyField |> Option.map (fun value -> compareValue value key >= 0) |> Option.defaultValue false
                            | _ -> false)
                        |> SduiValue.Array
                    | value -> value |> Option.defaultValue SduiValue.Null

                { current with Data = Map.add dataRef next current.Data }
            | PatchOperation.SetStatus(dataRef, value) when Set.contains dataRef refs ->
                { current with Data = Map.add dataRef (SduiValue.Object value) current.Data }
            | PatchOperation.SetOptions(targetId, value) ->
                { current with View = { Values = Map.add targetId (SduiValue.Object value) current.View.Values } }
            | _ -> current) state

    let applyValidatedFrame (state: RuntimeState) (frame: RuntimeFrame) =
        match frame.Payload with
        | RuntimePayload.Document document ->
            { state with
                Document = Some document
                DocumentRevision = frame.DocumentRevision
                DataRevision = frame.DataRevision
                LastTransportSequence = frame.TransportSequence
                View = { Values = document.DefaultView }
                Poll = RuntimePollState.Ready
                LastError = None }, RuntimeEffect.SchedulePoll DynamicRuntimeDefaults.limits.MinimumPollInterval
        | RuntimePayload.Snapshot snapshot ->
            { state with
                Data = snapshot.Data
                DocumentRevision = frame.DocumentRevision
                DataRevision = frame.DataRevision
                LastTransportSequence = frame.TransportSequence
                Poll = RuntimePollState.Ready
                LastError = None }, RuntimeEffect.NoEffect
        | RuntimePayload.Patch patch ->
            match patchRuntimeError state patch with
            | Some(reasonCode, message) ->
                { state with
                    Poll = RuntimePollState.PausedForResync
                    LastError = Some { ReasonCode = reasonCode; Message = message; Recoverable = true } },
                RuntimeEffect.RequestResync(frame.CanvasInstanceId, state.DataRevision)
            | None ->
                let next = applyPatch state patch
                { next with
                    DocumentRevision = frame.DocumentRevision
                    DataRevision = frame.DataRevision
                    LastTransportSequence = frame.TransportSequence
                    Poll = RuntimePollState.Ready
                    LastError = None }, RuntimeEffect.NoEffect
        | RuntimePayload.Error runtimeError ->
            { state with
                LastTransportSequence = frame.TransportSequence
                LastError = Some runtimeError
                Poll = if runtimeError.Recoverable then state.Poll else RuntimePollState.Suspended }, RuntimeEffect.NoEffect
        | RuntimePayload.Heartbeat _ ->
            { state with LastTransportSequence = frame.TransportSequence }, RuntimeEffect.NoEffect

    let reduce (state: RuntimeState) (frame: RuntimeFrame) =
        if frame.CanvasInstanceId <> state.Identity.CanvasInstanceId || frame.DocumentId <> state.Identity.DocumentId then
            state, RuntimeEffect.RequestResync(state.Identity.CanvasInstanceId, state.DataRevision)
        elif frame.TransportSequence <= state.LastTransportSequence then
            state, RuntimeEffect.NoEffect
        elif frame.TransportSequence <> state.LastTransportSequence + 1L then
            { state with Poll = RuntimePollState.PausedForResync }, RuntimeEffect.RequestResync(frame.CanvasInstanceId, state.DataRevision)
        elif frame.Kind = RuntimeFrameKind.Patch && frame.BaseDataRevision <> Some state.DataRevision then
            { state with Poll = RuntimePollState.PausedForResync }, RuntimeEffect.RequestResync(frame.CanvasInstanceId, state.DataRevision)
        elif frame.Kind <> RuntimeFrameKind.Document && state.Document.IsNone then
            state, RuntimeEffect.RequestResync(frame.CanvasInstanceId, state.DataRevision)
        else
            applyValidatedFrame state frame

    let resetView state =
        match state.Document with
        | None -> state, RuntimeEffect.NoEffect
        | Some document -> { state with View = { Values = document.DefaultView } }, RuntimeEffect.NoEffect

    let resetCanvas state =
        state, RuntimeEffect.SubmitAction(SduiAction.ResetCanvas state.Identity.CanvasInstanceId)

[<RequireQualifiedAccess>]
module RuntimePoll =
    let mount state =
        match state with
        | RuntimePollState.Unmounted -> RuntimePollState.MountedIdle
        | value -> value

    let ready isVisible isExpanded isConnected state =
        if not isVisible || not isExpanded || not isConnected then RuntimePollState.Suspended
        else
            match state with
            | RuntimePollState.MountedIdle
            | RuntimePollState.Suspended
            | RuntimePollState.Backoff _ -> RuntimePollState.Ready
            | value -> value

    let beginPoll state =
        match state with
        | RuntimePollState.Ready -> RuntimePollState.PollInFlight, true
        | value -> value, false

    let complete state =
        match state with
        | RuntimePollState.PollInFlight -> RuntimePollState.Ready
        | value -> value

    let timeout retryAtUtc state =
        match state with
        | RuntimePollState.PollInFlight -> RuntimePollState.Backoff retryAtUtc
        | value -> value

    let suspend state =
        match state with
        | RuntimePollState.Disposed
        | RuntimePollState.Unmounted -> state
        | _ -> RuntimePollState.Suspended

    let dispose _ = RuntimePollState.Disposed

type RuntimeRegistryState =
    { Instances: Map<CanvasInstanceId, RuntimeState> }

[<RequireQualifiedAccess>]
module RuntimeRegistry =
    let empty = { Instances = Map.empty }

    let mount (identity: RuntimeIdentity) (registry: RuntimeRegistryState) =
        match Map.tryFind identity.CanvasInstanceId registry.Instances with
        | Some existing -> registry, existing, RuntimeEffect.NoEffect
        | None ->
            let state =
                { RuntimeReducer.initial identity with
                    Poll = RuntimePollState.MountedIdle }

            { registry with Instances = Map.add identity.CanvasInstanceId state registry.Instances },
            state,
            RuntimeEffect.NoEffect

    let tryFind (canvasInstanceId: CanvasInstanceId) (registry: RuntimeRegistryState) =
        Map.tryFind canvasInstanceId registry.Instances

    let applyFrame (frame: RuntimeFrame) (registry: RuntimeRegistryState) =
        match tryFind frame.CanvasInstanceId registry with
        | None ->
            registry,
            RuntimeEffect.ReportDiagnostic
                { CanvasInstanceId = Some frame.CanvasInstanceId
                  DocumentRevision = Some frame.DocumentRevision
                  DataRevision = Some frame.DataRevision
                  TransportSequence = Some frame.TransportSequence
                  ReasonCode = "canvas-not-mounted"
                  LimitName = None }
        | Some state ->
            let next, effect = RuntimeReducer.reduce state frame
            { registry with Instances = Map.add frame.CanvasInstanceId next registry.Instances }, effect

    let dispose (canvasInstanceId: CanvasInstanceId) (registry: RuntimeRegistryState) =
        if Map.containsKey canvasInstanceId registry.Instances then
            { registry with Instances = Map.remove canvasInstanceId registry.Instances }, RuntimeEffect.CancelPoll
        else
            registry, RuntimeEffect.NoEffect
