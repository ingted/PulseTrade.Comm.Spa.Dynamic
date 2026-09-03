namespace PulseTrade.Comm.Spa.Dynamic.Contracts

open WebSharper

/// Browser/server 共用的 WebSharper typed-JSON codec。
///
/// 這個 codec 保留既有 RuntimeFrame 型別，不建立第二套 wire DTO；它只用於
/// Interactive Extension 的 loopback WebSocket。既有 RuntimeCodec 及其 JSON
/// 格式維持不變，避免影響既有 server consumer。
[<JavaScript; RequireQualifiedAccess>]
module BrowserRuntimeCodec =
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
