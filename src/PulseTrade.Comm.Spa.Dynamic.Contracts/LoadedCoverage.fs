namespace PulseTrade.Comm.Spa.Dynamic.Contracts

open System
open WebSharper

[<JavaScript; RequireQualifiedAccess>]
module TaLoadedCoverageCodec =
    [<Literal>]
    let Schema = "ta-loaded-coverage.v1"

    [<Literal>]
    let DefaultViewKey = "viewport.loadedCoverage"

    [<Literal>]
    let MaximumVisibleBarsKey = "viewport.maximumVisibleBars"

    [<Literal>]
    let MaximumActiveDetailBars = 4000

    [<Literal>]
    let MaximumOverviewAnchors = 1024

    [<Literal>]
    let WindowIntentSchema = "ta-coverage-window.v1"

    let error code field message =
        { Code = code
          Field = field
          Message = message }

    let invariantInt64 (value: int64) = SduiValue.Text(value.ToString())

    let completenessText = function
        | TaCoverageCompleteness.Complete -> "complete"
        | TaCoverageCompleteness.Partial -> "partial"

    let directionText = function
        | TaCoverageDirection.Earlier -> "earlier"
        | TaCoverageDirection.Later -> "later"

    let encodeSegment segment =
        SduiValue.Object(
            Map [
                "segmentId", SduiValue.Text segment.SegmentId
                "startEventTimeUtc", SduiValue.Text segment.StartEventTimeUtc
                "endEventTimeExclusiveUtc", SduiValue.Text segment.EndEventTimeExclusiveUtc
                "startObservationOrdinal", invariantInt64 segment.StartObservationOrdinal
                "observationCount", invariantInt64 segment.ObservationCount
            ])

    let encodeAnchor anchor =
        SduiValue.Object(
            Map [
                "observationOrdinal", invariantInt64 anchor.ObservationOrdinal
                "eventTimeUtc", SduiValue.Text anchor.EventTimeUtc
                "value", anchor.Value
            ])

    let encode projection =
        SduiValue.Object(
            Map [
                "schema", SduiValue.Text Schema
                "coverageIdentity", SduiValue.Text projection.CoverageIdentity
                "coverageRevision", invariantInt64 projection.CoverageRevision
                "queryGeneration", invariantInt64 projection.QueryGeneration
                "completeness", SduiValue.Text(completenessText projection.Completeness)
                "totalObservationCount",
                    projection.TotalObservationCount
                    |> Option.map invariantInt64
                    |> Option.defaultValue SduiValue.Null
                "segments", projection.Segments |> Array.map encodeSegment |> SduiValue.Array
                "overviewAnchors", projection.OverviewAnchors |> Array.map encodeAnchor |> SduiValue.Array
                "activeDetail",
                    SduiValue.Object(
                        Map [
                            "startObservationOrdinal", invariantInt64 projection.ActiveDetail.StartObservationOrdinal
                            "observationCount", SduiValue.Number(float projection.ActiveDetail.ObservationCount)
                            "baseAxisRef", SduiValue.Text projection.ActiveDetail.BaseAxisRef
                        ])
            ])

    let apply projection values = values |> Map.add DefaultViewKey (encode projection)

    let applyMaximumVisibleBars maximumVisibleBars values =
        values |> Map.add MaximumVisibleBarsKey (SduiValue.Number(float maximumVisibleBars))

    let tryMaximumVisibleBars values =
        match Map.tryFind MaximumVisibleBarsKey values with
        | Some(SduiValue.Number value)
            when not (Double.IsNaN value)
                 && not (Double.IsInfinity value)
                 && value = Math.Floor value
                 && value >= 1.0
                 && value <= float MaximumActiveDetailBars -> Some(int value)
        | _ -> None

    let requiredText field fields =
        match Map.tryFind field fields with
        | Some(SduiValue.Text value) when not (String.IsNullOrWhiteSpace value) -> Ok value
        | _ -> Error(error "invalid-loaded-coverage" field "A non-empty text value is required.")

    let requiredInt64 field fields =
        match Map.tryFind field fields with
        | Some(SduiValue.Text value) ->
            match Int64.TryParse value with
            | true, parsed -> Ok parsed
            | _ -> Error(error "invalid-loaded-coverage" field "An invariant Int64 text value is required.")
        | _ -> Error(error "invalid-loaded-coverage" field "An invariant Int64 text value is required.")

    let requiredInt field fields =
        match Map.tryFind field fields with
        | Some(SduiValue.Number value)
            when not (Double.IsNaN value)
                 && not (Double.IsInfinity value)
                 && value = Math.Floor value
                 && value >= float Int32.MinValue
                 && value <= float Int32.MaxValue -> Ok(int value)
        | _ -> Error(error "invalid-loaded-coverage" field "An integer number is required.")

    let requiredTime field fields =
        requiredText field fields
        |> Result.bind (fun value ->
            if value.EndsWith("Z") || value.EndsWith("+00:00") then Ok value
            else Error(error "invalid-loaded-coverage" field "A UTC timestamp is required."))

    let sequenceResults values =
        values
        |> Array.fold
            (fun state value ->
                match state, value with
                | Ok accepted, Ok item -> Ok(item :: accepted)
                | Error errors, Error nextErrors -> Error(List.append errors nextErrors)
                | Error errors, _ -> Error errors
                | _, Error errors -> Error errors)
            (Ok [])
        |> Result.map (List.rev >> List.toArray)

    let decodeSegment index = function
        | SduiValue.Object fields ->
            match
                requiredText "segmentId" fields,
                requiredTime "startEventTimeUtc" fields,
                requiredTime "endEventTimeExclusiveUtc" fields,
                requiredInt64 "startObservationOrdinal" fields,
                requiredInt64 "observationCount" fields
            with
            | Ok segmentId, Ok startTime, Ok endTime, Ok startOrdinal, Ok count ->
                Ok
                    { SegmentId = segmentId
                      StartEventTimeUtc = startTime
                      EndEventTimeExclusiveUtc = endTime
                      StartObservationOrdinal = startOrdinal
                      ObservationCount = count }
            | values ->
                let errors =
                    [ match values with
                      | Error value, _, _, _, _ -> yield value
                      | _ -> ()
                      match values with
                      | _, Error value, _, _, _ -> yield value
                      | _ -> ()
                      match values with
                      | _, _, Error value, _, _ -> yield value
                      | _ -> ()
                      match values with
                      | _, _, _, Error value, _ -> yield value
                      | _ -> ()
                      match values with
                      | _, _, _, _, Error value -> yield value
                      | _ -> () ]
                Error errors
        | _ -> Error [ error "invalid-loaded-coverage" $"segments[{index}]" "A segment object is required." ]

    let decodeAnchor index = function
        | SduiValue.Object fields ->
            match requiredInt64 "observationOrdinal" fields, requiredTime "eventTimeUtc" fields, Map.tryFind "value" fields with
            | Ok ordinal, Ok eventTime, Some value ->
                Ok
                    { ObservationOrdinal = ordinal
                      EventTimeUtc = eventTime
                      Value = value }
            | _ -> Error [ error "invalid-loaded-coverage" $"overviewAnchors[{index}]" "A valid ordinal, UTC event time and value are required." ]
        | _ -> Error [ error "invalid-loaded-coverage" $"overviewAnchors[{index}]" "An overview anchor object is required." ]

    let decodeArray field decoder fields =
        match Map.tryFind field fields with
        | Some(SduiValue.Array values) -> values |> Array.mapi decoder |> sequenceResults
        | _ -> Error [ error "invalid-loaded-coverage" field "An array is required." ]

    let validationErrors projection =
        [ if String.IsNullOrWhiteSpace projection.CoverageIdentity then
              yield error "invalid-loaded-coverage" "coverageIdentity" "CoverageIdentity is required."
          if projection.CoverageRevision < 0L then
              yield error "invalid-loaded-coverage" "coverageRevision" "CoverageRevision cannot be negative."
          if projection.QueryGeneration < 0L then
              yield error "invalid-loaded-coverage" "queryGeneration" "QueryGeneration cannot be negative."
          if projection.OverviewAnchors.Length > MaximumOverviewAnchors then
              yield error "limit-loaded-coverage" "overviewAnchors" $"At most {MaximumOverviewAnchors} anchors are allowed."
          if projection.ActiveDetail.StartObservationOrdinal < 0L then
              yield error "invalid-loaded-coverage" "activeDetail.startObservationOrdinal" "The active-detail ordinal cannot be negative."
          if projection.ActiveDetail.ObservationCount <= 0 || projection.ActiveDetail.ObservationCount > MaximumActiveDetailBars then
              yield error "invalid-loaded-coverage" "activeDetail.observationCount" $"Active detail must contain 1..{MaximumActiveDetailBars} observations."
          if String.IsNullOrWhiteSpace projection.ActiveDetail.BaseAxisRef then
              yield error "invalid-loaded-coverage" "activeDetail.baseAxisRef" "BaseAxisRef is required."
          match projection.Completeness, projection.TotalObservationCount with
          | TaCoverageCompleteness.Complete, Some total when total > 0L -> ()
          | TaCoverageCompleteness.Complete, _ ->
              yield error "invalid-loaded-coverage" "totalObservationCount" "Complete coverage requires a positive total observation count."
          | TaCoverageCompleteness.Partial, Some _ ->
              yield error "invalid-loaded-coverage" "totalObservationCount" "Partial coverage must not claim a total observation count."
          | TaCoverageCompleteness.Partial, None -> ()
          for index, segment in projection.Segments |> Array.indexed do
              if String.IsNullOrWhiteSpace segment.SegmentId then
                  yield error "invalid-loaded-coverage" $"segments[{index}].segmentId" "SegmentId is required."
              if String.IsNullOrWhiteSpace segment.StartEventTimeUtc || String.IsNullOrWhiteSpace segment.EndEventTimeExclusiveUtc then
                  yield error "invalid-loaded-coverage" $"segments[{index}].eventTime" "Segment UTC bounds are required."
              if segment.StartObservationOrdinal < 0L || segment.ObservationCount < 0L then
                  yield error "invalid-loaded-coverage" $"segments[{index}].ordinal" "Segment ordinals and counts cannot be negative."
              if segment.StartEventTimeUtc.CompareTo(segment.EndEventTimeExclusiveUtc) >= 0 then
                  yield error "invalid-loaded-coverage" $"segments[{index}].eventTime" "Segment end must be later than its start."
          for index in 1 .. projection.Segments.Length - 1 do
              let previous = projection.Segments[index - 1]
              let current = projection.Segments[index]
              if current.StartObservationOrdinal < previous.StartObservationOrdinal + previous.ObservationCount then
                  yield error "invalid-loaded-coverage" $"segments[{index}]" "Coverage segments cannot overlap or move backward."
          for index, anchor in projection.OverviewAnchors |> Array.indexed do
              if anchor.ObservationOrdinal < 0L || String.IsNullOrWhiteSpace anchor.EventTimeUtc then
                  yield error "invalid-loaded-coverage" $"overviewAnchors[{index}]" "Overview anchors require a non-negative ordinal and UTC event time."
          for index in 1 .. projection.OverviewAnchors.Length - 1 do
              if projection.OverviewAnchors[index].ObservationOrdinal <= projection.OverviewAnchors[index - 1].ObservationOrdinal then
                  yield error "invalid-loaded-coverage" $"overviewAnchors[{index}]" "Overview anchor ordinals must increase."
          match projection.TotalObservationCount with
          | Some total when projection.ActiveDetail.StartObservationOrdinal + int64 projection.ActiveDetail.ObservationCount > total ->
              yield error "invalid-loaded-coverage" "activeDetail" "Active detail exceeds the complete observation domain."
          | Some total ->
              for index, segment in projection.Segments |> Array.indexed do
                  if segment.StartObservationOrdinal + segment.ObservationCount > total then
                      yield error "invalid-loaded-coverage" $"segments[{index}]" "Segment exceeds the complete observation domain."
              for index, anchor in projection.OverviewAnchors |> Array.indexed do
                  if anchor.ObservationOrdinal >= total then
                      yield error "invalid-loaded-coverage" $"overviewAnchors[{index}]" "Overview anchor exceeds the complete observation domain."
          | None -> () ]

    let tryDecode values =
        match Map.tryFind DefaultViewKey values with
        | None -> Ok None
        | Some(SduiValue.Object fields) ->
            match
                requiredText "schema" fields,
                requiredText "coverageIdentity" fields,
                requiredInt64 "coverageRevision" fields,
                requiredInt64 "queryGeneration" fields,
                requiredText "completeness" fields,
                decodeArray "segments" decodeSegment fields,
                decodeArray "overviewAnchors" decodeAnchor fields,
                Map.tryFind "activeDetail" fields
            with
            | Ok schema, Ok identity, Ok revision, Ok generation, Ok completeness, Ok segments, Ok anchors, Some(SduiValue.Object active) when schema = Schema ->
                match requiredInt64 "startObservationOrdinal" active, requiredInt "observationCount" active, requiredText "baseAxisRef" active with
                | Ok startOrdinal, Ok count, Ok baseAxisRef ->
                    let completenessValue =
                        match completeness.Trim().ToLower() with
                        | "complete" -> Ok TaCoverageCompleteness.Complete
                        | "partial" -> Ok TaCoverageCompleteness.Partial
                        | _ -> Error(error "invalid-loaded-coverage" "completeness" "Completeness must be complete or partial.")
                    let totalValue =
                        match Map.tryFind "totalObservationCount" fields with
                        | None
                        | Some SduiValue.Null -> Ok None
                        | Some(SduiValue.Text value) ->
                            match Int64.TryParse value with
                            | true, parsed -> Ok(Some parsed)
                            | _ -> Error(error "invalid-loaded-coverage" "totalObservationCount" "An invariant Int64 text value is required.")
                        | _ -> Error(error "invalid-loaded-coverage" "totalObservationCount" "An invariant Int64 text value or null is required.")
                    match completenessValue, totalValue with
                    | Ok completenessKind, Ok total ->
                        let projection =
                            { CoverageIdentity = identity
                              CoverageRevision = revision
                              QueryGeneration = generation
                              Completeness = completenessKind
                              TotalObservationCount = total
                              Segments = segments
                              OverviewAnchors = anchors
                              ActiveDetail =
                                { StartObservationOrdinal = startOrdinal
                                  ObservationCount = count
                                  BaseAxisRef = baseAxisRef } }
                        match validationErrors projection with
                        | [] -> Ok(Some projection)
                        | errors -> Error errors
                    | Error completenessError, _ -> Error [ completenessError ]
                    | _, Error totalError -> Error [ totalError ]
                | _ -> Error [ error "invalid-loaded-coverage" "activeDetail" "A valid active-detail object is required." ]
            | Ok schema, _, _, _, _, _, _, _ when schema <> Schema ->
                Error [ error "unsupported-loaded-coverage" "schema" $"Unsupported loaded coverage schema `{schema}`." ]
            | _ -> Error [ error "invalid-loaded-coverage" DefaultViewKey "The loaded coverage projection is malformed." ]
        | Some _ -> Error [ error "invalid-loaded-coverage" DefaultViewKey "The loaded coverage projection must be an object." ]

    let observationDomainCount projection =
        match projection.TotalObservationCount with
        | Some total -> total
        | None ->
            let segmentEnd = projection.Segments |> Array.fold (fun value segment -> max value (segment.StartObservationOrdinal + segment.ObservationCount)) 0L
            let anchorEnd = projection.OverviewAnchors |> Array.fold (fun value anchor -> max value (anchor.ObservationOrdinal + 1L)) 0L
            max segmentEnd (projection.ActiveDetail.StartObservationOrdinal + int64 projection.ActiveDetail.ObservationCount)

    let tryWindowIntent direction expectedRevision queryGeneration startOrdinal observationCount =
        if queryGeneration < 0L
           || (expectedRevision |> Option.exists (fun value -> value < 0L))
           || (startOrdinal |> Option.exists (fun value -> value < 0L))
           || observationCount <= 0
           || observationCount > MaximumActiveDetailBars then
            None
        else
            Some
                { ExpectedCoverageRevision = expectedRevision
                  QueryGeneration = queryGeneration
                  StartObservationOrdinal = startOrdinal
                  ObservationCount = observationCount
                  Direction = direction }
