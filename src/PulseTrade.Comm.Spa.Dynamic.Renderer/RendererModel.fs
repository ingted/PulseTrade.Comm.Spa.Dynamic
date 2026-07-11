namespace PulseTrade.Comm.Spa.Dynamic.Renderer

open PulseTrade.Comm.Spa.Dynamic.Contracts
open WebSharper

type TaCandlePoint =
    { Timestamp: string
      Open: float
      High: float
      Low: float
      Close: float
      Volume: float }

type TaLinePoint =
    { Timestamp: string
      Value: float }

type TaVisibleWindow =
    { StartIndex: int
      Count: int }

[<JavaScript; RequireQualifiedAccess>]
module RendererModel =
    let tryObject = function
        | SduiValue.Object value -> Some value
        | _ -> None

    let tryText = function
        | SduiValue.Text value -> Some value
        | _ -> None

    let tryNumber = function
        | SduiValue.Number value -> Some value
        | _ -> None

    let objectField name value =
        value |> Map.tryFind name

    let objectText name value =
        objectField name value |> Option.bind tryText

    let objectNumber name value =
        objectField name value |> Option.bind tryNumber

    let parseCandle value =
        value
        |> tryObject
        |> Option.bind (fun item ->
            match
                objectText "t" item,
                objectNumber "o" item,
                objectNumber "h" item,
                objectNumber "l" item,
                objectNumber "c" item,
                objectNumber "v" item
            with
            | Some timestamp, Some openValue, Some high, Some low, Some close, Some volume ->
                Some
                    { Timestamp = timestamp
                      Open = openValue
                      High = high
                      Low = low
                      Close = close
                      Volume = volume }
            | _ -> None)

    let parseLine value =
        value
        |> tryObject
        |> Option.bind (fun item ->
            match objectText "t" item, objectNumber "v" item with
            | Some timestamp, Some lineValue -> Some { Timestamp = timestamp; Value = lineValue }
            | _ -> None)

    let seriesValues dataRef data =
        match Map.tryFind dataRef data with
        | Some(SduiValue.Array values) -> values
        | _ -> [||]

    let candleSeries dataRef data =
        seriesValues dataRef data |> Array.choose parseCandle

    let lineSeries dataRef data =
        seriesValues dataRef data |> Array.choose parseLine

    let clampWindow minimumCount maximumCount total requested =
        if total <= 0 then
            { StartIndex = 0; Count = 0 }
        else
            let upper = min maximumCount total
            let lower = min minimumCount upper
            let count = max lower (min requested.Count upper)
            let startIndex = max 0 (min requested.StartIndex (total - count))
            { StartIndex = startIndex; Count = count }

    let selectWindow window (values: 'T array) =
        if window.Count <= 0 || values.Length = 0 then [||]
        else values |> Array.skip window.StartIndex |> Array.truncate window.Count

    let paddedRange fallbackLow fallbackHigh values =
        if Array.isEmpty values then fallbackLow, fallbackHigh
        else
            let low = Array.min values
            let high = Array.max values

            if low = high then low - 1.0, high + 1.0
            else
                let padding = max ((high - low) * 0.08) 0.0001
                low - padding, high + padding

    let normalize low high top height value =
        if low = high then top + height / 2.0
        else top + height - ((value - low) / (high - low)) * height
