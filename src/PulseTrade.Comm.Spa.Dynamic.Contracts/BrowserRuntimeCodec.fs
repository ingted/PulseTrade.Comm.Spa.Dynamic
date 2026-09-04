namespace PulseTrade.Comm.Spa.Dynamic.Contracts

open WebSharper

/// Browser/server 共用的 WebSharper typed-JSON codec。
///
/// RuntimeFrame 走 authoritative state stream；DynamicAction request/result 走
/// 獨立的 correlated command stream。兩者只用於 Interactive Extension 的
/// loopback WebSocket。既有 RuntimeCodec 及其 JSON 格式維持不變。
[<JavaScript; RequireQualifiedAccess>]
module BrowserRuntimeCodec =
    let required field value =
        if System.String.IsNullOrWhiteSpace value then Some(field + " is required.")
        elif value.Length > 128 then Some(field + " exceeds 128 characters.")
        else None

    let validationMessage errors = errors |> List.choose id |> String.concat "; "

    let requestFrameError (frame: DynamicActionClientFrame) =
        validationMessage
            [ if frame.Protocol = DynamicActionWireDefaults.Protocol then None else Some "Unsupported dynamic action protocol."
              if frame.Kind = DynamicActionWireDefaults.RequestKind then None else Some "Dynamic action client frame must be an action request."
              if isNull (box frame.Request) then Some "Dynamic action request is required."
              else required "action.requestId" frame.Request.RequestId
              if isNull (box frame.Request) || frame.Request.ExpectedDocumentRevision |> Option.forall (fun revision -> revision >= 0L) then None
              else Some "Expected document revision must be non-negative."
              if isNull (box frame.Request) || not (isNull (box frame.Request.Action)) then None
              else Some "Dynamic action is required." ]

    let resultFrameError (frame: DynamicActionServerFrame) =
        let resultErrors =
            if isNull (box frame.Result) then
                [ Some "Dynamic action result is required." ]
            else
                match frame.Result with
                | DynamicActionResult.Accepted(requestId, revision)
                | DynamicActionResult.RevisionConflict(requestId, revision) ->
                    [ required "actionResult.requestId" requestId
                      if revision >= 0L then None else Some "Result revision must be non-negative." ]
                | DynamicActionResult.Rejected(requestId, code, message) ->
                    [ required "actionResult.requestId" requestId
                      required "actionResult.code" code
                      if not (isNull message) && message.Length <= 512 then None else Some "Action result message must not exceed 512 characters." ]

        [ if frame.Protocol = DynamicActionWireDefaults.Protocol then None else Some "Unsupported dynamic action protocol."
          if frame.Kind = DynamicActionWireDefaults.ResultKind then None else Some "Dynamic action server frame must be an action result."
          yield! resultErrors ]
        |> validationMessage

    let encode (frame: RuntimeFrame) =
        Json.Serialize frame

    let decode (text: string) =
        try
            Ok(Json.Deserialize<RuntimeFrame> text)
        with error ->
            Error error.Message

    let encodeClient (frame: RuntimeClientFrame) =
        Json.Serialize frame

    let decodeClient (text: string) =
        try
            Ok(Json.Deserialize<RuntimeClientFrame> text)
        with error ->
            Error error.Message

    let encodeActionRequest request =
        Json.Serialize
            { Protocol = DynamicActionWireDefaults.Protocol
              Kind = DynamicActionWireDefaults.RequestKind
              Request = request }

    let decodeActionRequest (text: string) =
        try
            let frame = Json.Deserialize<DynamicActionClientFrame> text
            match requestFrameError frame with
            | "" -> Ok frame.Request
            | message -> Error message
        with error ->
            Error error.Message

    let encodeActionResult result =
        Json.Serialize
            { Protocol = DynamicActionWireDefaults.Protocol
              Kind = DynamicActionWireDefaults.ResultKind
              Result = result }

    let decodeActionResult (text: string) =
        try
            let frame = Json.Deserialize<DynamicActionServerFrame> text
            match resultFrameError frame with
            | "" -> Ok frame.Result
            | message -> Error message
        with error ->
            Error error.Message
