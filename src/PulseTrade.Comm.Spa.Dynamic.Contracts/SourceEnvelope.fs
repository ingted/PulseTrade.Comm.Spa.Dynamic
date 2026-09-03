namespace PulseTrade.Comm.Spa.Dynamic.Contracts

open System
open System.Text
open System.Text.Json

type SourceStreamIdentity =
    { SourceId: string
      SchemaKey: string
      Epoch: string }

type SourceSnapshotEnvelope =
    { Stream: SourceStreamIdentity
      SourceRevision: int64
      LastSequence: int64
      CapturedAtUtc: DateTimeOffset
      Payload: SduiValue }

type SourceEventEnvelope =
    { Stream: SourceStreamIdentity
      BaseSourceRevision: int64
      NewSourceRevision: int64
      Sequence: int64
      EventTimeUtc: DateTimeOffset
      Payload: SduiValue }

type SourceProjectionState =
    { Stream: SourceStreamIdentity
      SourceRevision: int64
      LastSequence: int64
      CapturedAtUtc: DateTimeOffset
      Payload: SduiValue }

[<RequireQualifiedAccess>]
type SourceResyncReason =
    | StreamChanged
    | SequenceGap
    | SequenceConflict
    | RevisionMismatch
    | SnapshotOrderConflict
    | DomainReducerRejected of reasonCode: string

type SourceSnapshotRequest =
    { RequestedStream: SourceStreamIdentity
      LastGoodStream: SourceStreamIdentity option
      LastAcceptedSequence: int64 option
      LastAcceptedRevision: int64 option
      Reason: SourceResyncReason }

[<RequireQualifiedAccess>]
type SourceApplyResult =
    | Applied of SourceProjectionState
    | Duplicate of SourceProjectionState
    | ResyncRequired of lastGood: SourceProjectionState * request: SourceSnapshotRequest

[<RequireQualifiedAccess>]
module SourceEnvelopeValidation =
    let utcTimestamp field (value: DateTimeOffset) =
        if value.Offset = TimeSpan.Zero then
            []
        else
            [ RuntimeValidation.error "utc-required" field $"{field} must use UTC offset zero." ]

    let streamErrors field (stream: SourceStreamIdentity) =
        [ yield! RuntimeValidation.identifier $"{field}.sourceId" stream.SourceId
          yield! RuntimeValidation.identifier $"{field}.schemaKey" stream.SchemaKey
          yield! RuntimeValidation.identifier $"{field}.epoch" stream.Epoch ]

    let snapshotErrors (snapshot: SourceSnapshotEnvelope) =
        [ yield! streamErrors "snapshot.stream" snapshot.Stream

          if snapshot.SourceRevision < 0L then
              yield RuntimeValidation.error "invalid-source-revision" "snapshot.sourceRevision" "Source revision must be non-negative."

          if snapshot.LastSequence < 0L then
              yield RuntimeValidation.error "invalid-source-sequence" "snapshot.lastSequence" "Last sequence must be non-negative."

          yield! utcTimestamp "snapshot.capturedAtUtc" snapshot.CapturedAtUtc
          yield! RuntimeValidation.unsafeValue "snapshot.payload" snapshot.Payload ]

    let eventErrors (event: SourceEventEnvelope) =
        [ yield! streamErrors "event.stream" event.Stream

          if event.BaseSourceRevision < 0L then
              yield RuntimeValidation.error "invalid-base-source-revision" "event.baseSourceRevision" "Base source revision must be non-negative."

          if event.NewSourceRevision <= event.BaseSourceRevision then
              yield RuntimeValidation.error "invalid-new-source-revision" "event.newSourceRevision" "New source revision must be greater than base source revision."

          if event.Sequence < 1L then
              yield RuntimeValidation.error "invalid-source-sequence" "event.sequence" "Event sequence must start at one."

          yield! utcTimestamp "event.eventTimeUtc" event.EventTimeUtc
          yield! RuntimeValidation.unsafeValue "event.payload" event.Payload ]

    let validateSnapshot snapshot =
        match snapshotErrors snapshot with
        | [] -> Ok snapshot
        | errors -> Error errors

    let validateEvent event =
        match eventErrors event with
        | [] -> Ok event
        | errors -> Error errors

[<RequireQualifiedAccess>]
module SourceEnvelopeCodec =
    let encodeSnapshot snapshot =
        JsonSerializer.Serialize(snapshot, RuntimeCodec.options)

    let encodeEvent event =
        JsonSerializer.Serialize(event, RuntimeCodec.options)

    let decodeBounded limits field validate (text: string) =
        if isNull text then
            Error [ RuntimeValidation.error "source-envelope-required" field "Source envelope is required." ]
        elif Encoding.UTF8.GetByteCount text > limits.MaxFrameBytes then
            Error [ RuntimeValidation.error "limit-source-envelope-bytes" field $"Source envelope exceeds hard limit {limits.MaxFrameBytes} bytes." ]
        else
            try
                let value = JsonSerializer.Deserialize<'T>(text, RuntimeCodec.options)

                if isNull (box value) then
                    Error [ RuntimeValidation.error "source-envelope-invalid" field "Source envelope decoded to null." ]
                else
                    validate value
            with :? JsonException as error ->
                Error [ RuntimeValidation.error "source-envelope-json-invalid" field error.Message ]

    let decodeSnapshot limits text =
        decodeBounded limits "sourceSnapshot" SourceEnvelopeValidation.validateSnapshot text

    let decodeEvent limits text =
        decodeBounded limits "sourceEvent" SourceEnvelopeValidation.validateEvent text

[<RequireQualifiedAccess>]
module SourceProjection =
    let stateOfSnapshot (snapshot: SourceSnapshotEnvelope) : SourceProjectionState =
        { Stream = snapshot.Stream
          SourceRevision = snapshot.SourceRevision
          LastSequence = snapshot.LastSequence
          CapturedAtUtc = snapshot.CapturedAtUtc
          Payload = snapshot.Payload }

    let snapshotRequest requestedStream reason (lastGood: SourceProjectionState) =
        { RequestedStream = requestedStream
          LastGoodStream = Some lastGood.Stream
          LastAcceptedSequence = Some lastGood.LastSequence
          LastAcceptedRevision = Some lastGood.SourceRevision
          Reason = reason }

    let normalizeReasonCode reasonCode =
        match RuntimeValidation.identifier "reasonCode" reasonCode with
        | [] -> reasonCode
        | _ -> "domain-reducer-rejected"

    let applySnapshot (current: SourceProjectionState option) (snapshot: SourceSnapshotEnvelope) =
        SourceEnvelopeValidation.validateSnapshot snapshot
        |> Result.map (fun valid ->
            let next = stateOfSnapshot valid

            match current with
            | None -> SourceApplyResult.Applied next
            | Some previous when previous.Stream <> valid.Stream -> SourceApplyResult.Applied next
            | Some previous when previous.SourceRevision = valid.SourceRevision && previous.LastSequence = valid.LastSequence ->
                SourceApplyResult.Duplicate previous
            | Some previous when valid.SourceRevision >= previous.SourceRevision && valid.LastSequence >= previous.LastSequence ->
                SourceApplyResult.Applied next
            | Some previous when valid.SourceRevision <= previous.SourceRevision && valid.LastSequence <= previous.LastSequence ->
                SourceApplyResult.Duplicate previous
            | Some previous ->
                SourceApplyResult.ResyncRequired(
                    previous,
                    snapshotRequest valid.Stream SourceResyncReason.SnapshotOrderConflict previous))

    let applyEvent reducePayload (current: SourceProjectionState) (event: SourceEventEnvelope) =
        SourceEnvelopeValidation.validateEvent event
        |> Result.map (fun valid ->
            if current.Stream <> valid.Stream then
                SourceApplyResult.ResyncRequired(
                    current,
                    snapshotRequest valid.Stream SourceResyncReason.StreamChanged current)
            elif valid.Sequence = current.LastSequence && valid.NewSourceRevision = current.SourceRevision then
                SourceApplyResult.Duplicate current
            elif valid.Sequence < current.LastSequence && valid.NewSourceRevision <= current.SourceRevision then
                SourceApplyResult.Duplicate current
            elif valid.Sequence <= current.LastSequence then
                SourceApplyResult.ResyncRequired(
                    current,
                    snapshotRequest valid.Stream SourceResyncReason.SequenceConflict current)
            elif valid.Sequence <> current.LastSequence + 1L then
                SourceApplyResult.ResyncRequired(
                    current,
                    snapshotRequest valid.Stream SourceResyncReason.SequenceGap current)
            elif valid.BaseSourceRevision <> current.SourceRevision then
                SourceApplyResult.ResyncRequired(
                    current,
                    snapshotRequest valid.Stream SourceResyncReason.RevisionMismatch current)
            else
                match reducePayload current.Payload valid.Payload with
                | Ok payload ->
                    SourceApplyResult.Applied
                        { Stream = current.Stream
                          SourceRevision = valid.NewSourceRevision
                          LastSequence = valid.Sequence
                          CapturedAtUtc = valid.EventTimeUtc
                          Payload = payload }
                | Error reasonCode ->
                    let reason = SourceResyncReason.DomainReducerRejected(normalizeReasonCode reasonCode)
                    SourceApplyResult.ResyncRequired(current, snapshotRequest valid.Stream reason current))
