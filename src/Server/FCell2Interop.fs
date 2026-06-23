namespace PulseTrade.Comm.Spa.Dynamic.Server

open System.Globalization
open PersistedConcurrentSortedList.Type

module FCell2Interop =
    
    /// 將 fCell2 AST 遞迴轉換為 JSON 字串
    let rec toJsonString (cell: fCell2<string>) : string =
        match cell with
        | fCell2.N() -> "null"
        | fCell2.B b -> if b then "true" else "false"
        | fCell2.D d -> d.ToString(CultureInfo.InvariantCulture)
        | fCell2.S s -> 
            if isNull s then "null"
            else
                // 處理跳脫字元
                let escaped = s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t")
                "\"" + escaped + "\""
        | fCell2.A arr ->
            if isNull arr then "null"
            else
                let elements = arr |> Array.map toJsonString
                "[" + String.concat "," elements + "]"
        | fCell2.T map ->
            if isNull (box map) then "null"
            else
                let props = 
                    map 
                    |> Map.toSeq 
                    |> Seq.map (fun (k, v) -> 
                        let kEscaped = k.Replace("\\", "\\\\").Replace("\"", "\\\"")
                        "\"" + kEscaped + "\":" + toJsonString v)
                "{" + String.concat "," props + "}"

    /// 將一個 fCell2 根節點打包成包含 fskynet-sdui schema 的 Payload
    let toMessagePayload (cell: fCell2<string>) : string =
        let innerJson = toJsonString cell
        $"{{\"schema\":\"fskynet-sdui\",\"ui\":{innerJson}}}"

    let rec private fromJsonElement (el: System.Text.Json.JsonElement) : fCell2<string> =
        match el.ValueKind with
        | System.Text.Json.JsonValueKind.Null -> fCell2.N()
        | System.Text.Json.JsonValueKind.True -> fCell2.B true
        | System.Text.Json.JsonValueKind.False -> fCell2.B false
        | System.Text.Json.JsonValueKind.Number -> fCell2.D (el.GetDecimal())
        | System.Text.Json.JsonValueKind.String -> fCell2.S (el.GetString())
        | System.Text.Json.JsonValueKind.Array ->
            let elements = 
                el.EnumerateArray()
                |> Seq.map fromJsonElement
                |> Seq.toArray
            fCell2.A elements
        | System.Text.Json.JsonValueKind.Object ->
            let props =
                el.EnumerateObject()
                |> Seq.map (fun prop -> prop.Name, fromJsonElement prop.Value)
                |> Map.ofSeq
            fCell2.T props
        | _ -> fCell2.N()

    /// 解析 JSON 字串並轉換為 fCell2 AST
    let fromJsonString (json: string) : fCell2<string> =
        try
            use doc = System.Text.Json.JsonDocument.Parse(json)
            fromJsonElement doc.RootElement
        with _ ->
            fCell2.N()
