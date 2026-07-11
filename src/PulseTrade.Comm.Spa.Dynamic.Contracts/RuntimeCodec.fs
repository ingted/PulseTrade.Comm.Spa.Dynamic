namespace PulseTrade.Comm.Spa.Dynamic.Contracts

open System
open System.Text
open System.Text.Json
open System.Text.Json.Serialization
open FSharp.SystemTextJson

[<RequireQualifiedAccess>]
module RuntimeCodec =
    let options =
        let value = JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
        value.Converters.Add(JsonFSharpConverter())
        value

    let encode (frame: RuntimeFrame) =
        JsonSerializer.Serialize(frame, options)

    let decode limits (text: string) =
        if isNull text then
            Error [ RuntimeValidation.error "frame-required" "frame" "Runtime frame is required." ]
        elif Encoding.UTF8.GetByteCount text > limits.MaxFrameBytes then
            Error [ RuntimeValidation.error "limit-frame-bytes" "frame" $"Frame exceeds hard limit {limits.MaxFrameBytes} bytes." ]
        else
            try
                let frame = JsonSerializer.Deserialize<RuntimeFrame>(text, options)

                if isNull (box frame) then
                    Error [ RuntimeValidation.error "frame-invalid" "frame" "Runtime frame decoded to null." ]
                else
                    RuntimeValidation.validateFrame limits frame
            with :? JsonException as error ->
                Error [ RuntimeValidation.error "frame-json-invalid" "frame" error.Message ]
