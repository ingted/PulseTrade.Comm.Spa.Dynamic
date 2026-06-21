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
