namespace PulseTrade.Comm.Spa.Dynamic.Contracts

open System
open System.Globalization

[<RequireQualifiedAccess>]
module TemporalPointCodec =
    [<Literal>]
    let TypeKey = "_type"

    [<Literal>]
    let TypeValue = "temporal-point.v1"

    let finalityText = function
        | PointFinality.Preview -> "preview"
        | PointFinality.Final -> "final"

    let projectionText = function
        | TemporalProjection.CandleSpan -> "candle-span"
        | TemporalProjection.RepeatAcrossBaseBuckets -> "repeat-across-base-buckets"
        | TemporalProjection.StepAfterClose -> "step-after-close"

    let timestampText (value: DateTimeOffset) =
        value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)

    let encode (point: TemporalPoint) =
        SduiValue.Object(
            Map [
                TypeKey, SduiValue.Text TypeValue
                "sourceIntervalId", SduiValue.Text point.SourceIntervalId
                "scaleKey", SduiValue.Text point.ScaleKey
                "intervalStartUtc", SduiValue.Text(timestampText point.IntervalStartUtc)
                "intervalEndUtc", SduiValue.Text(timestampText point.IntervalEndUtc)
                "observedThroughUtc", SduiValue.Text(timestampText point.ObservedThroughUtc)
                "finality", SduiValue.Text(finalityText point.Finality)
                "projection", SduiValue.Text(projectionText point.Projection)
                match point.AvailableAtUtc with
                | Some value -> "availableAtUtc", SduiValue.Text(timestampText value)
                | None -> ()
                match point.Quality with
                | Some value -> "quality", SduiValue.Text value
                | None -> ()
                match point.Value with
                | Some value -> "value", value
                | None -> "value", SduiValue.Null
            ])

    let objectText field values =
        match Map.tryFind field values with
        | Some(SduiValue.Text value) -> Some value
        | _ -> None

    let timestamp key field values =
        match objectText key values with
        | None -> Error(RuntimeValidation.error "required" field $"{field} is required.")
        | Some value ->
            match DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) with
            | true, parsed -> Ok parsed
            | _ -> Error(RuntimeValidation.error "invalid-timestamp" field $"{field} must be an ISO-8601 timestamp.")

    let optionalTimestamp key field values =
        match objectText key values with
        | None -> Ok None
        | Some value ->
            match DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) with
            | true, parsed -> Ok(Some parsed)
            | _ -> Error(RuntimeValidation.error "invalid-timestamp" field $"{field} must be an ISO-8601 timestamp.")

    let requiredText key field values =
        match objectText key values with
        | Some value when not (String.IsNullOrWhiteSpace value) -> Ok value
        | _ -> Error(RuntimeValidation.error "required" field $"{field} is required.")

    let pointFinality key field values =
        match objectText key values |> Option.map _.Trim().ToLowerInvariant() with
        | Some "preview" -> Ok PointFinality.Preview
        | Some "final" -> Ok PointFinality.Final
        | _ -> Error(RuntimeValidation.error "invalid-finality" field $"{field} must be preview or final.")

    let temporalProjection key field values =
        match objectText key values |> Option.map _.Trim().ToLowerInvariant() with
        | Some "candle-span" -> Ok TemporalProjection.CandleSpan
        | Some "repeat-across-base-buckets" -> Ok TemporalProjection.RepeatAcrossBaseBuckets
        | Some "step-after-close" -> Ok TemporalProjection.StepAfterClose
        | _ -> Error(RuntimeValidation.error "invalid-projection" field $"{field} is not a supported temporal projection.")

    let sequenceResults values =
        let errors = values |> List.choose (function Error error -> Some error | _ -> None)
        if List.isEmpty errors then Ok () else Error errors

    let validate (point: TemporalPoint) =
        let errors =
            [ yield! RuntimeValidation.identifier "temporalPoint.sourceIntervalId" point.SourceIntervalId
              yield! RuntimeValidation.identifier "temporalPoint.scaleKey" point.ScaleKey
              yield! RuntimeValidation.identifier "temporalPoint.quality" (defaultArg point.Quality "unknown")

              for field, value in
                  [ "temporalPoint.intervalStartUtc", point.IntervalStartUtc
                    "temporalPoint.intervalEndUtc", point.IntervalEndUtc
                    "temporalPoint.observedThroughUtc", point.ObservedThroughUtc ] do
                  if value.Offset <> TimeSpan.Zero then
                      yield RuntimeValidation.error "utc-required" field $"{field} must use UTC offset zero."

              match point.AvailableAtUtc with
              | Some value when value.Offset <> TimeSpan.Zero ->
                  yield RuntimeValidation.error "utc-required" "temporalPoint.availableAtUtc" "temporalPoint.availableAtUtc must use UTC offset zero."
              | _ -> ()

              if point.IntervalStartUtc >= point.IntervalEndUtc then
                  yield RuntimeValidation.error "invalid-interval" "temporalPoint.intervalEndUtc" "Interval end must be later than interval start."

              if point.ObservedThroughUtc < point.IntervalStartUtc || point.ObservedThroughUtc > point.IntervalEndUtc then
                  yield RuntimeValidation.error "invalid-frontier" "temporalPoint.observedThroughUtc" "Observed frontier must remain inside the source interval."

              match point.Finality with
              | PointFinality.Final when point.ObservedThroughUtc <> point.IntervalEndUtc ->
                  yield RuntimeValidation.error "invalid-final-frontier" "temporalPoint.observedThroughUtc" "A final point must observe through the interval end."
              | _ -> ()

              match point.AvailableAtUtc with
              | Some value when value < point.ObservedThroughUtc ->
                  yield RuntimeValidation.error "invalid-availability" "temporalPoint.availableAtUtc" "Availability cannot precede the observed frontier."
              | _ -> ()

              match point.Value with
              | Some value -> yield! RuntimeValidation.unsafeValue "temporalPoint.value" value
              | None -> () ]

        match errors with
        | [] -> Ok point
        | values -> Error values

    let decode value =
        match value with
        | SduiValue.Object values when objectText TypeKey values = Some TypeValue ->
            let sourceIntervalId = requiredText "sourceIntervalId" "temporalPoint.sourceIntervalId" values
            let scaleKey = requiredText "scaleKey" "temporalPoint.scaleKey" values
            let intervalStart = timestamp "intervalStartUtc" "temporalPoint.intervalStartUtc" values
            let intervalEnd = timestamp "intervalEndUtc" "temporalPoint.intervalEndUtc" values
            let observedThrough = timestamp "observedThroughUtc" "temporalPoint.observedThroughUtc" values
            let availableAt = optionalTimestamp "availableAtUtc" "temporalPoint.availableAtUtc" values
            let finality = pointFinality "finality" "temporalPoint.finality" values
            let projection = temporalProjection "projection" "temporalPoint.projection" values

            match sequenceResults [ sourceIntervalId |> Result.map ignore; scaleKey |> Result.map ignore; intervalStart |> Result.map ignore; intervalEnd |> Result.map ignore; observedThrough |> Result.map ignore; availableAt |> Result.map ignore; finality |> Result.map ignore; projection |> Result.map ignore ] with
            | Error errors -> Error errors
            | Ok () ->
                { SourceIntervalId = Result.defaultValue "" sourceIntervalId
                  ScaleKey = Result.defaultValue "" scaleKey
                  IntervalStartUtc = Result.defaultValue DateTimeOffset.MinValue intervalStart
                  IntervalEndUtc = Result.defaultValue DateTimeOffset.MinValue intervalEnd
                  ObservedThroughUtc = Result.defaultValue DateTimeOffset.MinValue observedThrough
                  AvailableAtUtc = Result.defaultValue None availableAt
                  Finality = Result.defaultValue PointFinality.Preview finality
                  Projection = Result.defaultValue TemporalProjection.RepeatAcrossBaseBuckets projection
                  Quality = objectText "quality" values
                  Value =
                    match Map.tryFind "value" values with
                    | Some SduiValue.Null
                    | None -> None
                    | Some item -> Some item }
                |> validate
        | _ ->
            Error [ RuntimeValidation.error "temporal-point-required" "temporalPoint" "Expected temporal-point.v1." ]

[<RequireQualifiedAccess>]
module TemporalAxisCodec =
    [<Literal>]
    let TypeValue = "temporal-axis.v1"

    let position field values =
        match Map.tryFind "position" values with
        | Some(SduiValue.Number value)
            when not (Double.IsNaN value)
                 && not (Double.IsInfinity value)
                 && value >= 0.0
                 && value = Math.Truncate value ->
            Ok(int64 value)
        | _ -> Error(RuntimeValidation.error "invalid-position" field $"{field} must be a non-negative integer.")

    let revision key field values =
        match Map.tryFind key values with
        | Some(SduiValue.Number value)
            when not (Double.IsNaN value)
                 && not (Double.IsInfinity value)
                 && value >= 0.0
                 && value = Math.Truncate value ->
            Ok(int64 value)
        | _ -> Error(RuntimeValidation.error "invalid-revision" field $"{field} must be a non-negative integer.")

    let encodePointFields (point: TemporalAxisPoint) =
        Map [
            "position", SduiValue.Number(float point.Position)
            "sourceIntervalId", SduiValue.Text point.SourceIntervalId
            "scaleKey", SduiValue.Text point.ScaleKey
            "intervalStartUtc", SduiValue.Text(TemporalPointCodec.timestampText point.IntervalStartUtc)
            "intervalEndUtc", SduiValue.Text(TemporalPointCodec.timestampText point.IntervalEndUtc)
            "observedThroughUtc", SduiValue.Text(TemporalPointCodec.timestampText point.ObservedThroughUtc)
            "finality", SduiValue.Text(TemporalPointCodec.finalityText point.Finality)
            "projection", SduiValue.Text(TemporalPointCodec.projectionText point.Projection)
            match point.AvailableAtUtc with
            | Some value -> "availableAtUtc", SduiValue.Text(TemporalPointCodec.timestampText value)
            | None -> ()
            match point.Quality with
            | Some value -> "quality", SduiValue.Text value
            | None -> ()
        ]

    let encodePoint point = SduiValue.Object(encodePointFields point)

    let validatePoint (point: TemporalAxisPoint) =
        { SourceIntervalId = point.SourceIntervalId
          ScaleKey = point.ScaleKey
          IntervalStartUtc = point.IntervalStartUtc
          IntervalEndUtc = point.IntervalEndUtc
          ObservedThroughUtc = point.ObservedThroughUtc
          AvailableAtUtc = point.AvailableAtUtc
          Finality = point.Finality
          Projection = point.Projection
          Quality = point.Quality
          Value = None }
        |> TemporalPointCodec.validate
        |> Result.map (fun _ -> point)

    let decodePointFields field values =
        let positionValue = position (field + ".position") values
        let sourceIntervalId = TemporalPointCodec.requiredText "sourceIntervalId" (field + ".sourceIntervalId") values
        let scaleKey = TemporalPointCodec.requiredText "scaleKey" (field + ".scaleKey") values
        let intervalStart = TemporalPointCodec.timestamp "intervalStartUtc" (field + ".intervalStartUtc") values
        let intervalEnd = TemporalPointCodec.timestamp "intervalEndUtc" (field + ".intervalEndUtc") values
        let observedThrough = TemporalPointCodec.timestamp "observedThroughUtc" (field + ".observedThroughUtc") values
        let availableAt = TemporalPointCodec.optionalTimestamp "availableAtUtc" (field + ".availableAtUtc") values
        let finality = TemporalPointCodec.pointFinality "finality" (field + ".finality") values
        let projection = TemporalPointCodec.temporalProjection "projection" (field + ".projection") values

        match
            TemporalPointCodec.sequenceResults
                [ positionValue |> Result.map ignore
                  sourceIntervalId |> Result.map ignore
                  scaleKey |> Result.map ignore
                  intervalStart |> Result.map ignore
                  intervalEnd |> Result.map ignore
                  observedThrough |> Result.map ignore
                  availableAt |> Result.map ignore
                  finality |> Result.map ignore
                  projection |> Result.map ignore ]
        with
        | Error errors -> Error errors
        | Ok () ->
            { Position = Result.defaultValue 0L positionValue
              SourceIntervalId = Result.defaultValue "" sourceIntervalId
              ScaleKey = Result.defaultValue "" scaleKey
              IntervalStartUtc = Result.defaultValue DateTimeOffset.MinValue intervalStart
              IntervalEndUtc = Result.defaultValue DateTimeOffset.MinValue intervalEnd
              ObservedThroughUtc = Result.defaultValue DateTimeOffset.MinValue observedThrough
              AvailableAtUtc = Result.defaultValue None availableAt
              Finality = Result.defaultValue PointFinality.Preview finality
              Projection = Result.defaultValue TemporalProjection.RepeatAcrossBaseBuckets projection
              Quality = TemporalPointCodec.objectText "quality" values }
            |> validatePoint

    let decodePoint field = function
        | SduiValue.Object values -> decodePointFields field values
        | _ -> Error [ RuntimeValidation.error "temporal-axis-point-required" field "Expected a temporal axis point object." ]

    let orderedPointErrors field (points: TemporalAxisPoint array) =
        points
        |> Array.pairwise
        |> Array.indexed
        |> Array.choose (fun (index, (left, right)) ->
            if left.Position < right.Position then None
            else Some(RuntimeValidation.error "unordered-position" $"{field}[{index + 1}].position" "Temporal axis positions must be strictly increasing."))
        |> Array.toList

    let validate (axis: TemporalAxis) =
        let points = if isNull axis.Points then [||] else axis.Points
        let errors =
            [ yield! RuntimeValidation.identifier "temporalAxis.axisRef" axis.AxisRef
              if axis.Revision < 0L then
                  yield RuntimeValidation.error "invalid-revision" "temporalAxis.revision" "Temporal axis revision must be non-negative."
              for index, point in points |> Array.indexed do
                  match validatePoint point with
                  | Ok _ -> ()
                  | Error values ->
                      for value in values do
                          yield { value with Field = $"temporalAxis.points[{index}]." + value.Field.Replace("temporalPoint.", "") }
              yield! orderedPointErrors "temporalAxis.points" points ]

        match errors with
        | [] -> Ok { axis with Points = points }
        | values -> Error values

    let encode (axis: TemporalAxis) =
        SduiValue.Object(
            Map [
                TemporalPointCodec.TypeKey, SduiValue.Text TypeValue
                "axisRef", SduiValue.Text axis.AxisRef
                "revision", SduiValue.Number(float axis.Revision)
                "points", SduiValue.Array(axis.Points |> Array.map encodePoint)
            ])

    let decode = function
        | SduiValue.Object values when TemporalPointCodec.objectText TemporalPointCodec.TypeKey values = Some TypeValue ->
            let axisRef = TemporalPointCodec.requiredText "axisRef" "temporalAxis.axisRef" values
            let revisionValue = revision "revision" "temporalAxis.revision" values
            let points =
                match Map.tryFind "points" values with
                | Some(SduiValue.Array items) ->
                    items
                    |> Array.indexed
                    |> Array.map (fun (index, item) -> decodePoint $"temporalAxis.points[{index}]" item)
                | _ -> [| Error [ RuntimeValidation.error "temporal-axis-points-required" "temporalAxis.points" "Temporal axis points are required." ] |]
            let pointErrors = points |> Array.choose (function Error errors -> Some errors | _ -> None) |> Array.toList |> List.concat

            match axisRef, revisionValue, pointErrors with
            | Ok axisRef, Ok revision, [] ->
                { AxisRef = axisRef
                  Revision = revision
                  Points = points |> Array.choose (function Ok point -> Some point | _ -> None) }
                |> validate
            | _ ->
                [ match axisRef with Error error -> yield error | _ -> ()
                  match revisionValue with Error error -> yield error | _ -> ()
                  yield! pointErrors ]
                |> Error
        | _ -> Error [ RuntimeValidation.error "temporal-axis-required" "temporalAxis" "Expected temporal-axis.v1." ]

[<RequireQualifiedAccess>]
module TemporalSeriesCodec =
    [<Literal>]
    let TypeValue = "temporal-series.v1"

    let encodePointFields (point: TemporalSeriesPoint) =
        Map [ "position", SduiValue.Number(float point.Position); "value", point.Value ]

    let encodePoint point = SduiValue.Object(encodePointFields point)

    let decodePoint field = function
        | SduiValue.Object values ->
            let positionValue = TemporalAxisCodec.position (field + ".position") values
            match positionValue, Map.tryFind "value" values with
            | Ok position, Some value -> Ok { Position = position; Value = value }
            | Error error, _ -> Error [ error ]
            | _, None -> Error [ RuntimeValidation.error "required" (field + ".value") "Temporal series point value is required." ]
        | _ -> Error [ RuntimeValidation.error "temporal-series-point-required" field "Expected a temporal series point object." ]

    let orderedPointErrors field (points: TemporalSeriesPoint array) =
        points
        |> Array.pairwise
        |> Array.indexed
        |> Array.choose (fun (index, (left, right)) ->
            if left.Position < right.Position then None
            else Some(RuntimeValidation.error "unordered-position" $"{field}[{index + 1}].position" "Temporal series positions must be strictly increasing."))
        |> Array.toList

    let validate (series: TemporalSeries) =
        let points = if isNull series.Points then [||] else series.Points
        let errors =
            [ yield! RuntimeValidation.identifier "temporalSeries.axisRef" series.AxisRef
              if series.AxisRevision < 0L then
                  yield RuntimeValidation.error "invalid-revision" "temporalSeries.axisRevision" "Temporal series axis revision must be non-negative."
              for index, point in points |> Array.indexed do
                  if point.Position < 0L then
                      yield RuntimeValidation.error "invalid-position" $"temporalSeries.points[{index}].position" "Temporal series position must be non-negative."
                  yield! RuntimeValidation.unsafeValue $"temporalSeries.points[{index}].value" point.Value
              yield! orderedPointErrors "temporalSeries.points" points ]

        match errors with
        | [] -> Ok { series with Points = points }
        | values -> Error values

    let encode (series: TemporalSeries) =
        SduiValue.Object(
            Map [
                TemporalPointCodec.TypeKey, SduiValue.Text TypeValue
                "axisRef", SduiValue.Text series.AxisRef
                "axisRevision", SduiValue.Number(float series.AxisRevision)
                "points", SduiValue.Array(series.Points |> Array.map encodePoint)
            ])

    let decode = function
        | SduiValue.Object values when TemporalPointCodec.objectText TemporalPointCodec.TypeKey values = Some TypeValue ->
            let axisRef = TemporalPointCodec.requiredText "axisRef" "temporalSeries.axisRef" values
            let revisionValue = TemporalAxisCodec.revision "axisRevision" "temporalSeries.axisRevision" values
            let points =
                match Map.tryFind "points" values with
                | Some(SduiValue.Array items) ->
                    items
                    |> Array.indexed
                    |> Array.map (fun (index, item) -> decodePoint $"temporalSeries.points[{index}]" item)
                | _ -> [| Error [ RuntimeValidation.error "temporal-series-points-required" "temporalSeries.points" "Temporal series points are required." ] |]
            let pointErrors = points |> Array.choose (function Error errors -> Some errors | _ -> None) |> Array.toList |> List.concat

            match axisRef, revisionValue, pointErrors with
            | Ok axisRef, Ok revision, [] ->
                { AxisRef = axisRef
                  AxisRevision = revision
                  Points = points |> Array.choose (function Ok point -> Some point | _ -> None) }
                |> validate
            | _ ->
                [ match axisRef with Error error -> yield error | _ -> ()
                  match revisionValue with Error error -> yield error | _ -> ()
                  yield! pointErrors ]
                |> Error
        | _ -> Error [ RuntimeValidation.error "temporal-series-required" "temporalSeries" "Expected temporal-series.v1." ]
