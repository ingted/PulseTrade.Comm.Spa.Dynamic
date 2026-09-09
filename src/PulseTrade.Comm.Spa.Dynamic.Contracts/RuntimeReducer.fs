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
              if not (isNull document.TemporalAxisRefs) then
                  yield! document.TemporalAxisRefs
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

    let temporalType value =
        match value with
        | SduiValue.Object fields ->
            match Map.tryFind "_type" fields with
            | Some(SduiValue.Text kind) -> Some kind
            | _ -> None
        | _ -> None

    let temporalObject expectedKind value =
        match value with
        | SduiValue.Object fields when temporalType value = Some expectedKind -> Some fields
        | _ -> None

    let temporalNumber key fields =
        match Map.tryFind key fields with
        | Some(SduiValue.Number value) when value >= 0.0 && value = Math.Truncate value -> Some value
        | _ -> None

    let temporalText key fields =
        match Map.tryFind key fields with
        | Some(SduiValue.Text value) when not (String.IsNullOrWhiteSpace value) -> Some value
        | _ -> None

    let temporalPointMaps fields =
        match Map.tryFind "points" fields with
        | Some(SduiValue.Array values) ->
            values |> Array.choose (function SduiValue.Object point -> Some point | _ -> None)
        | _ -> [||]

    let temporalPointValues fields =
        match Map.tryFind "points" fields with
        | Some(SduiValue.Array values) -> values
        | _ -> [||]

    let temporalPosition fields = temporalNumber "position" fields

    let temporalUtcText key fields =
        temporalText key fields
        |> Option.filter (fun value ->
            value.EndsWith("Z")
            || value.EndsWith("+00:00"))

    let temporalAxisPointShapeValid fields =
        match
            temporalPosition fields,
            temporalText "sourceIntervalId" fields,
            temporalText "scaleKey" fields,
            temporalUtcText "intervalStartUtc" fields,
            temporalUtcText "intervalEndUtc" fields,
            temporalUtcText "observedThroughUtc" fields,
            temporalText "finality" fields,
            temporalText "projection" fields
        with
        | Some _, Some _, Some _, Some intervalStart, Some intervalEnd, Some observedThrough, Some finality, Some projection ->
            let availableAtValid =
                match Map.tryFind "availableAtUtc" fields with
                | None -> true
                | Some _ ->
                    temporalUtcText "availableAtUtc" fields
                    |> Option.exists (fun availableAt -> compareOrdinalText observedThrough availableAt <= 0)
            let causalInterval =
                compareOrdinalText intervalStart intervalEnd < 0
                && compareOrdinalText intervalStart observedThrough <= 0
                && compareOrdinalText observedThrough intervalEnd <= 0
                && (finality <> "final" || observedThrough = intervalEnd)
            let knownFinality = finality = "preview" || finality = "final"
            let knownProjection =
                projection = "candle-span"
                || projection = "repeat-across-base-buckets"
                || projection = "step-after-close"
            availableAtValid && causalInterval && knownFinality && knownProjection
        | _ -> false

    let upsertTemporalPointValues existing items =
        let fallback () =
            Array.append
                (existing |> Array.choose (function SduiValue.Object point -> Some point | _ -> None))
                items
            |> Array.choose (fun item -> temporalPosition item |> Option.map (fun position -> position, item))
            |> Array.fold (fun values (position, item) -> Map.add position item values) Map.empty
            |> Map.toArray
            |> Array.map (snd >> SduiValue.Object)

        match items with
        | [||] -> existing
        | [| item |] ->
            match temporalPosition item with
            | None -> fallback ()
            | Some position when existing.Length = 0 -> [| SduiValue.Object item |]
            | Some position ->
                match existing[existing.Length - 1] with
                | SduiValue.Object last ->
                    match temporalPosition last with
                    | Some lastPosition when position = lastPosition ->
                        let next = Array.copy existing
                        next[next.Length - 1] <- SduiValue.Object item
                        next
                    | Some lastPosition when position > lastPosition ->
                        Array.append existing [| SduiValue.Object item |]
                    | _ -> fallback ()
                | _ -> fallback ()
        | _ -> fallback ()

    let updateTemporalObject kind refKey refValue revisionKey revision existing items =
        let fields =
            existing
            |> Option.bind (temporalObject kind)
            |> Option.defaultValue Map.empty
        let points = upsertTemporalPointValues (temporalPointValues fields) items

        fields
        |> Map.add "_type" (SduiValue.Text kind)
        |> Map.add refKey (SduiValue.Text refValue)
        |> Map.add revisionKey (SduiValue.Number(float revision))
        |> Map.add "points" (SduiValue.Array points)
        |> SduiValue.Object

    let trimTemporalObject kind revisionKey revision position existing =
        match existing |> Option.bind (temporalObject kind) with
        | None -> existing |> Option.defaultValue SduiValue.Null
        | Some fields ->
            let points =
                temporalPointMaps fields
                |> Array.filter (fun item -> temporalPosition item |> Option.exists (fun value -> value >= float position))
                |> Array.map SduiValue.Object

            fields
            |> Map.add revisionKey (SduiValue.Number(float revision))
            |> Map.add "points" (SduiValue.Array points)
            |> SduiValue.Object

    let applyDataOperation refs data operation =
        match operation with
        | PatchOperation.ReplaceDataRef(dataRef, value) when Set.contains dataRef refs ->
            Map.add dataRef value data
        | PatchOperation.UpsertSeriesPoints(dataRef, keyField, items) when Set.contains dataRef refs ->
            Map.add dataRef (upsertPoints keyField (Map.tryFind dataRef data) items) data
        | PatchOperation.RemoveSeriesBefore(dataRef, keyField, key) when Set.contains dataRef refs ->
            let next =
                match Map.tryFind dataRef data with
                | Some(SduiValue.Array values) ->
                    values
                    |> Array.filter (function
                        | SduiValue.Object item ->
                            item
                            |> Map.tryFind keyField
                            |> Option.map (fun value -> compareValue value key >= 0)
                            |> Option.defaultValue false
                        | _ -> false)
                    |> SduiValue.Array
                | value -> value |> Option.defaultValue SduiValue.Null

            Map.add dataRef next data
        | PatchOperation.UpsertTemporalAxisPoints(axisRef, _, newRevision, items) when Set.contains axisRef refs ->
            let value = updateTemporalObject "temporal-axis.v1" "axisRef" axisRef "revision" newRevision (Map.tryFind axisRef data) items
            Map.add axisRef value data
        | PatchOperation.RemoveTemporalAxisBefore(axisRef, _, newRevision, position) when Set.contains axisRef refs ->
            let value = trimTemporalObject "temporal-axis.v1" "revision" newRevision position (Map.tryFind axisRef data)
            Map.add axisRef value data
        | PatchOperation.UpsertTemporalSeriesPoints(dataRef, axisRef, axisRevision, items) when Set.contains dataRef refs ->
            let value = updateTemporalObject "temporal-series.v1" "axisRef" axisRef "axisRevision" axisRevision (Map.tryFind dataRef data) items
            Map.add dataRef value data
        | PatchOperation.RemoveTemporalSeriesBefore(dataRef, _, axisRevision, position) when Set.contains dataRef refs ->
            let value = trimTemporalObject "temporal-series.v1" "axisRevision" axisRevision position (Map.tryFind dataRef data)
            Map.add dataRef value data
        | PatchOperation.SetStatus(dataRef, value) when Set.contains dataRef refs ->
            Map.add dataRef (SduiValue.Object value) data
        | _ -> data

    let documentAxisRefs state =
        state.Document
        |> Option.map (fun document -> if isNull document.TemporalAxisRefs then [||] else document.TemporalAxisRefs)
        |> Option.defaultValue [||]
        |> Set.ofArray

    let rawTemporalPoints fields =
        match Map.tryFind "points" fields with
        | Some(SduiValue.Array points) -> Some points
        | _ -> None

    let temporalPositions fields =
        rawTemporalPoints fields
        |> Option.map (Array.choose (function SduiValue.Object point -> temporalPosition point | _ -> None))

    let positionsStrictlyIncrease positions =
        positions
        |> Array.pairwise
        |> Array.forall (fun (left, right) -> left < right)

    let temporalPositionExists fields position =
        match rawTemporalPoints fields with
        | None -> false
        | Some points ->
            let rec search low high =
                if low > high then false
                else
                    let middle = low + ((high - low) / 2)
                    match points[middle] with
                    | SduiValue.Object point ->
                        match temporalPosition point with
                        | Some candidate when candidate = position -> true
                        | Some candidate when candidate < position -> search (middle + 1) high
                        | Some _ -> search low (middle - 1)
                        | None -> false
                    | _ -> false
            search 0 (points.Length - 1)

    let firstTemporalPosition fields =
        rawTemporalPoints fields
        |> Option.bind Array.tryHead
        |> Option.bind (function SduiValue.Object point -> temporalPosition point | _ -> None)

    let temporalAxisError axisRef value =
        match temporalObject "temporal-axis.v1" value with
        | None -> Some("temporal-axis-required", $"Temporal axis `{axisRef}` must contain temporal-axis.v1 data.")
        | Some fields ->
            match temporalText "axisRef" fields, temporalNumber "revision" fields, rawTemporalPoints fields, temporalPositions fields with
            | Some encodedRef, Some _, Some rawPoints, Some positions
                when encodedRef = axisRef
                     && rawPoints.Length = positions.Length
                     && rawPoints
                        |> Array.forall (function
                            | SduiValue.Object point -> temporalAxisPointShapeValid point
                            | _ -> false)
                     && positionsStrictlyIncrease positions
                     && positions.Length <= DynamicRuntimeDefaults.limits.MaxRetainedBarsPerSeries -> None
            | Some encodedRef, _, _, _ when encodedRef <> axisRef ->
                Some("temporal-axis-ref-mismatch", $"Temporal axis key `{axisRef}` contains axisRef `{encodedRef}`.")
            | _, _, Some rawPoints, Some positions when rawPoints.Length = positions.Length && not (positionsStrictlyIncrease positions) ->
                Some("unordered-position", $"Temporal axis `{axisRef}` positions must be strictly increasing.")
            | _, _, _, Some positions when positions.Length > DynamicRuntimeDefaults.limits.MaxRetainedBarsPerSeries ->
                Some("limit-retained-bars", $"Temporal axis `{axisRef}` exceeds retained hard limit {DynamicRuntimeDefaults.limits.MaxRetainedBarsPerSeries}.")
            | _ -> Some("invalid-temporal-axis", $"Temporal axis `{axisRef}` is malformed.")

    let temporalDataError state data =
        let axisRefs = documentAxisRefs state
        let axisError =
            axisRefs
            |> Seq.tryPick (fun axisRef ->
                Map.tryFind axisRef data
                |> Option.map (temporalAxisError axisRef)
                |> Option.defaultValue (Some("missing-temporal-axis", $"Temporal axis `{axisRef}` is missing.")))

        match axisError with
        | Some error -> Some error
        | None ->
            data
            |> Map.toSeq
            |> Seq.tryPick (fun (dataRef, value) ->
                match temporalObject "temporal-series.v1" value with
                | None -> None
                | Some fields ->
                    match temporalText "axisRef" fields, temporalNumber "axisRevision" fields, rawTemporalPoints fields, temporalPositions fields with
                    | Some axisRef, Some axisRevision, Some rawPoints, Some positions when not (Set.contains axisRef axisRefs) ->
                        Some("unknown-temporal-axis", $"Temporal series `{dataRef}` references undeclared axis `{axisRef}`.")
                    | Some axisRef, Some axisRevision, Some rawPoints, Some positions ->
                        match Map.tryFind axisRef data |> Option.bind (temporalObject "temporal-axis.v1") with
                        | None -> Some("missing-temporal-axis", $"Temporal series `{dataRef}` references missing axis `{axisRef}`.")
                        | Some axisFields ->
                            let axisPositions = temporalPositions axisFields |> Option.defaultValue [||] |> Set.ofArray
                            let unknownPosition = positions |> Array.tryFind (fun position -> not (Set.contains position axisPositions))
                            match temporalNumber "revision" axisFields, unknownPosition with
                            | Some revision, _ when revision <> axisRevision ->
                                Some("temporal-axis-revision-mismatch", $"Temporal series `{dataRef}` expects axis revision {axisRevision}, but `{axisRef}` is {revision}.")
                            | _, Some position ->
                                Some("unknown-temporal-position", $"Temporal series `{dataRef}` position {position} is absent from axis `{axisRef}`.")
                            | _ when rawPoints.Length <> positions.Length || not (positionsStrictlyIncrease positions) ->
                                Some("invalid-temporal-series", $"Temporal series `{dataRef}` positions must be complete and strictly increasing.")
                            | _ when positions.Length > DynamicRuntimeDefaults.limits.MaxRetainedBarsPerSeries ->
                                Some("limit-retained-bars", $"Temporal series `{dataRef}` exceeds retained hard limit {DynamicRuntimeDefaults.limits.MaxRetainedBarsPerSeries}.")
                            | _ -> None
                    | _ -> Some("invalid-temporal-series", $"Temporal series `{dataRef}` is malformed."))

    let temporalOperationError refs axisRefs targets data operation =
        let unknownDataRef dataRef =
            if Set.contains dataRef refs then None
            else Some("unknown-data-ref", $"Patch dataRef `{dataRef}` is not registered by the document.")

        match operation with
        | PatchOperation.ReplaceDataRef(dataRef, _)
        | PatchOperation.UpsertSeriesPoints(dataRef, _, _)
        | PatchOperation.RemoveSeriesBefore(dataRef, _, _)
        | PatchOperation.SetStatus(dataRef, _) -> unknownDataRef dataRef
        | PatchOperation.UpsertTemporalAxisPoints(axisRef, expectedRevision, _, _)
        | PatchOperation.RemoveTemporalAxisBefore(axisRef, expectedRevision, _, _) ->
            match unknownDataRef axisRef with
            | Some error -> Some error
            | None when not (Set.contains axisRef axisRefs) ->
                Some("unknown-temporal-axis", $"Patch axis `{axisRef}` is not declared by the document.")
            | None ->
                match Map.tryFind axisRef data |> Option.bind (temporalObject "temporal-axis.v1") |> Option.bind (temporalNumber "revision") with
                | Some revision when revision = float expectedRevision -> None
                | Some revision -> Some("temporal-axis-revision-mismatch", $"Patch expected axis `{axisRef}` revision {expectedRevision}, but current revision is {revision}.")
                | None -> Some("missing-temporal-axis", $"Patch axis `{axisRef}` has no current temporal-axis.v1 state.")
        | PatchOperation.UpsertTemporalSeriesPoints(dataRef, axisRef, _, _) ->
            match unknownDataRef dataRef with
            | Some error -> Some error
            | None when not (Set.contains axisRef axisRefs) -> Some("unknown-temporal-axis", $"Patch series `{dataRef}` references undeclared axis `{axisRef}`.")
            | None ->
                match Map.tryFind dataRef data with
                | None -> None
                | Some value ->
                    match temporalObject "temporal-series.v1" value |> Option.bind (temporalText "axisRef") with
                    | Some current when current = axisRef -> None
                    | Some current -> Some("temporal-axis-ref-mismatch", $"Patch series `{dataRef}` uses axis `{axisRef}`, but current axis is `{current}`.")
                    | None -> Some("temporal-series-required", $"Patch series `{dataRef}` does not contain temporal-series.v1 data.")
        | PatchOperation.RemoveTemporalSeriesBefore(dataRef, axisRef, _, _) ->
            match unknownDataRef dataRef with
            | Some error -> Some error
            | None ->
                match Map.tryFind dataRef data |> Option.bind (temporalObject "temporal-series.v1") |> Option.bind (temporalText "axisRef") with
                | Some current when current = axisRef -> None
                | Some current -> Some("temporal-axis-ref-mismatch", $"Patch series `{dataRef}` uses axis `{axisRef}`, but current axis is `{current}`.")
                | None -> Some("temporal-series-required", $"Patch series `{dataRef}` has no current temporal-series.v1 state.")
        | PatchOperation.SetOptions(targetId, _) when not (Set.contains targetId targets) ->
            Some("unknown-target-id", $"Patch target `{targetId}` is not registered by the document.")
        | _ -> None

    let patchCandidate state (patch: RuntimePatch) =
        let refs = knownDataRefs state
        let axisRefs = documentAxisRefs state
        let targets = knownTargetIds state
        let candidate =
            patch.Operations
            |> Array.fold (fun result operation ->
                match result with
                | Error error -> Error error
                | Ok data ->
                    match temporalOperationError refs axisRefs targets data operation with
                    | Some error -> Error error
                    | None -> Ok(applyDataOperation refs data operation)) (Ok state.Data)

        let changedRefs =
            patch.Operations
            |> Array.choose (function
                | PatchOperation.ReplaceDataRef(dataRef, _)
                | PatchOperation.UpsertSeriesPoints(dataRef, _, _)
                | PatchOperation.RemoveSeriesBefore(dataRef, _, _)
                | PatchOperation.UpsertTemporalAxisPoints(dataRef, _, _, _)
                | PatchOperation.RemoveTemporalAxisBefore(dataRef, _, _, _)
                | PatchOperation.UpsertTemporalSeriesPoints(dataRef, _, _, _)
                | PatchOperation.RemoveTemporalSeriesBefore(dataRef, _, _, _)
                | PatchOperation.SetStatus(dataRef, _) -> Some dataRef
                | PatchOperation.SetOptions _ -> None)
            |> Set.ofArray

        let changedAxisRefs =
            patch.Operations
            |> Array.choose (function
                | PatchOperation.UpsertTemporalAxisPoints(axisRef, _, _, _)
                | PatchOperation.RemoveTemporalAxisBefore(axisRef, _, _, _) -> Some axisRef
                | _ -> None)
            |> Set.ofArray

        let requiresFullValidation =
            patch.Operations
            |> Array.exists (function PatchOperation.ReplaceDataRef _ -> true | _ -> false)

        let operationLocalError candidateData =
            patch.Operations
            |> Array.tryPick (function
                | PatchOperation.UpsertTemporalAxisPoints(axisRef, _, newRevision, items) ->
                    match Map.tryFind axisRef candidateData |> Option.bind (temporalObject "temporal-axis.v1") with
                    | None -> Some("missing-temporal-axis", $"Temporal axis `{axisRef}` is missing after patch application.")
                    | Some fields ->
                        let malformed = items |> Array.exists (temporalAxisPointShapeValid >> not)
                        match temporalText "axisRef" fields, temporalNumber "revision" fields, rawTemporalPoints fields with
                        | Some encodedRef, _, _ when encodedRef <> axisRef ->
                            Some("temporal-axis-ref-mismatch", $"Temporal axis key `{axisRef}` contains axisRef `{encodedRef}`.")
                        | _, Some revision, _ when revision < float newRevision ->
                            Some("temporal-axis-revision-mismatch", $"Temporal axis `{axisRef}` did not reach revision {newRevision}.")
                        | _, _, Some points when points.Length > DynamicRuntimeDefaults.limits.MaxRetainedBarsPerSeries ->
                            Some("limit-retained-bars", $"Temporal axis `{axisRef}` exceeds retained hard limit {DynamicRuntimeDefaults.limits.MaxRetainedBarsPerSeries}.")
                        | _ when malformed -> Some("invalid-temporal-axis", $"Temporal axis `{axisRef}` patch contains malformed point metadata.")
                        | _ -> None
                | PatchOperation.UpsertTemporalSeriesPoints(dataRef, axisRef, axisRevision, items) ->
                    match
                        Map.tryFind dataRef candidateData |> Option.bind (temporalObject "temporal-series.v1"),
                        Map.tryFind axisRef candidateData |> Option.bind (temporalObject "temporal-axis.v1")
                    with
                    | None, _ -> Some("temporal-series-required", $"Temporal series `{dataRef}` is missing after patch application.")
                    | _, None -> Some("missing-temporal-axis", $"Temporal series `{dataRef}` references missing axis `{axisRef}`.")
                    | Some seriesFields, Some axisFields ->
                        let unknownPosition =
                            items
                            |> Array.choose temporalPosition
                            |> Array.tryFind (temporalPositionExists axisFields >> not)
                        match temporalText "axisRef" seriesFields, temporalNumber "axisRevision" seriesFields, temporalNumber "revision" axisFields, rawTemporalPoints seriesFields, unknownPosition with
                        | Some encodedRef, _, _, _, _ when encodedRef <> axisRef ->
                            Some("temporal-axis-ref-mismatch", $"Temporal series `{dataRef}` uses axis `{encodedRef}`, not `{axisRef}`.")
                        | _, Some seriesRevision, Some axisCurrentRevision, _, _ when seriesRevision <> axisCurrentRevision || seriesRevision < float axisRevision ->
                            Some("temporal-axis-revision-mismatch", $"Temporal series `{dataRef}` expects axis revision {seriesRevision}, but `{axisRef}` is {axisCurrentRevision}.")
                        | _, _, _, Some points, _ when points.Length > DynamicRuntimeDefaults.limits.MaxRetainedBarsPerSeries ->
                            Some("limit-retained-bars", $"Temporal series `{dataRef}` exceeds retained hard limit {DynamicRuntimeDefaults.limits.MaxRetainedBarsPerSeries}.")
                        | _, _, _, _, Some position ->
                            Some("unknown-temporal-position", $"Temporal series `{dataRef}` position {position} is absent from axis `{axisRef}`.")
                        | _ -> None
                | _ -> None)

        let dependencyError candidateData =
            candidateData
            |> Map.toSeq
            |> Seq.tryPick (fun (dataRef, value) ->
                match temporalObject "temporal-series.v1" value with
                | None -> None
                | Some fields ->
                    match temporalText "axisRef" fields, temporalNumber "axisRevision" fields, rawTemporalPoints fields with
                    | Some axisRef, Some seriesRevision, Some points
                        when Set.contains dataRef changedRefs || Set.contains axisRef changedAxisRefs ->
                        match Map.tryFind axisRef candidateData |> Option.bind (temporalObject "temporal-axis.v1") with
                        | None -> Some("missing-temporal-axis", $"Temporal series `{dataRef}` references missing axis `{axisRef}`.")
                        | Some axisFields ->
                            match temporalNumber "revision" axisFields with
                            | Some axisRevision when axisRevision <> seriesRevision ->
                                Some("temporal-axis-revision-mismatch", $"Temporal series `{dataRef}` expects axis revision {seriesRevision}, but `{axisRef}` is {axisRevision}.")
                            | _ when points.Length > DynamicRuntimeDefaults.limits.MaxRetainedBarsPerSeries ->
                                Some("limit-retained-bars", $"Temporal series `{dataRef}` exceeds retained hard limit {DynamicRuntimeDefaults.limits.MaxRetainedBarsPerSeries}.")
                            | _ ->
                                match firstTemporalPosition fields, firstTemporalPosition axisFields with
                                | Some seriesFirst, Some axisFirst when seriesFirst < axisFirst ->
                                    Some("unknown-temporal-position", $"Temporal series `{dataRef}` retains position {seriesFirst}, which is absent from axis `{axisRef}`.")
                                | _ -> None
                    | _ when Set.contains dataRef changedRefs ->
                        Some("invalid-temporal-series", $"Temporal series `{dataRef}` is malformed.")
                    | _ -> None)

        match candidate with
        | Error error -> Error error
        | Ok candidateData ->
            let retainedError =
                changedRefs
                |> Seq.tryPick (fun dataRef ->
                    match Map.tryFind dataRef candidateData with
                    | Some(SduiValue.Array values) when values.Length > DynamicRuntimeDefaults.limits.MaxRetainedBarsPerSeries ->
                        Some("limit-retained-bars", $"Series `{dataRef}` would exceed retained hard limit {DynamicRuntimeDefaults.limits.MaxRetainedBarsPerSeries}.")
                    | Some _ -> None
                    | None -> None)

            let error =
                retainedError
                |> Option.orElseWith (fun () -> operationLocalError candidateData)
                |> Option.orElseWith (fun () -> dependencyError candidateData)
                |> Option.orElseWith (fun () -> if requiresFullValidation then temporalDataError state candidateData else None)

            match error with
            | Some value -> Error value
            | None -> Ok candidateData

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
            | PatchOperation.UpsertTemporalAxisPoints(axisRef, _, newRevision, items) when Set.contains axisRef refs ->
                let value = updateTemporalObject "temporal-axis.v1" "axisRef" axisRef "revision" newRevision (Map.tryFind axisRef current.Data) items
                { current with Data = Map.add axisRef value current.Data }
            | PatchOperation.RemoveTemporalAxisBefore(axisRef, _, newRevision, position) when Set.contains axisRef refs ->
                let value = trimTemporalObject "temporal-axis.v1" "revision" newRevision position (Map.tryFind axisRef current.Data)
                { current with Data = Map.add axisRef value current.Data }
            | PatchOperation.UpsertTemporalSeriesPoints(dataRef, axisRef, axisRevision, items) when Set.contains dataRef refs ->
                let value = updateTemporalObject "temporal-series.v1" "axisRef" axisRef "axisRevision" axisRevision (Map.tryFind dataRef current.Data) items
                { current with Data = Map.add dataRef value current.Data }
            | PatchOperation.RemoveTemporalSeriesBefore(dataRef, _, axisRevision, position) when Set.contains dataRef refs ->
                let value = trimTemporalObject "temporal-series.v1" "axisRevision" axisRevision position (Map.tryFind dataRef current.Data)
                { current with Data = Map.add dataRef value current.Data }
            | PatchOperation.SetStatus(dataRef, value) when Set.contains dataRef refs ->
                { current with Data = Map.add dataRef (SduiValue.Object value) current.Data }
            | PatchOperation.SetOptions(targetId, value) ->
                { current with View = { Values = Map.add targetId (SduiValue.Object value) current.View.Values } }
            | _ -> current) state

    let snapshotRuntimeError state (snapshot: RuntimeSnapshot) =
        let refs = knownDataRefs state
        let unknownRef =
            snapshot.Data
            |> Map.toSeq
            |> Seq.map fst
            |> Seq.tryFind (fun dataRef -> not (Set.contains dataRef refs))

        match unknownRef with
        | Some dataRef -> Some("unknown-data-ref", $"Snapshot dataRef `{dataRef}` is not registered by the document.")
        | None -> temporalDataError state snapshot.Data

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
            match snapshotRuntimeError state snapshot with
            | Some(reasonCode, message) ->
                { state with
                    Poll = RuntimePollState.PausedForResync
                    LastError = Some { ReasonCode = reasonCode; Message = message; Recoverable = true } },
                RuntimeEffect.RequestResync(frame.CanvasInstanceId, state.DataRevision)
            | None ->
                { state with
                    Data = snapshot.Data
                    DocumentRevision = frame.DocumentRevision
                    DataRevision = frame.DataRevision
                    LastTransportSequence = frame.TransportSequence
                    Poll = RuntimePollState.Ready
                    LastError = None }, RuntimeEffect.NoEffect
        | RuntimePayload.Patch patch ->
            match patchCandidate state patch with
            | Error(reasonCode, message) ->
                { state with
                    Poll = RuntimePollState.PausedForResync
                    LastError = Some { ReasonCode = reasonCode; Message = message; Recoverable = true } },
                RuntimeEffect.RequestResync(frame.CanvasInstanceId, state.DataRevision)
            | Ok data ->
                { state with
                    Data = data
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
            match RuntimeValidation.validateFrame DynamicRuntimeDefaults.limits frame with
            | Ok _ -> applyValidatedFrame state frame
            | Error (error :: _) ->
                { state with
                    Poll = RuntimePollState.PausedForResync
                    LastError =
                        Some
                            { ReasonCode = error.Code
                              Message = error.Message
                              Recoverable = true } },
                RuntimeEffect.RequestResync(frame.CanvasInstanceId, state.DataRevision)
            | Error [] ->
                state, RuntimeEffect.RequestResync(frame.CanvasInstanceId, state.DataRevision)

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
