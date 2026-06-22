open System
open System.Text.Json
open System.Text.Json.Serialization

[<CLIMutable>]
type CommSpaWebSocketStreamKeyDto =
    { [<JsonPropertyName("pageId")>]
      PageId: string
      [<JsonPropertyName("mode")>]
      Mode: string
      [<JsonPropertyName("setName")>]
      SetName: string
      [<JsonPropertyName("keys")>]
      Keys: string array }

[<CLIMutable>]
type CommSpaWebSocketRequest =
    { [<JsonPropertyName("type")>]
      Type: string
      [<JsonPropertyName("renderMode")>]
      RenderMode: string }

let options = JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true)
let json = """{"type":"append-page","renderMode":"actor-dynamic"}"""
let request = JsonSerializer.Deserialize<CommSpaWebSocketRequest>(json, options)
printfn "Type: %s, RenderMode: %s" request.Type request.RenderMode
