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
