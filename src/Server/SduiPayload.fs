namespace PulseTrade.Comm.Spa.Dynamic.Server

open System
open System.Text.Json

type SduiPayloadDiscriminator =
    { schema: string
      surface: string
      documentType: string }

[<RequireQualifiedAccess>]
module SduiPayloadClassifier =
    let expectedSchema = "fskynet-sdui"
    let actorsPageSurface = "ActorsPage"
    let actorTopologyDocumentType = "ActorTopologyPage"

    let isActorsPagePayload (rawContent: string) =
        if String.IsNullOrWhiteSpace rawContent then
            false
        else
            try
                use document = JsonDocument.Parse(rawContent)
                let root = document.RootElement
                let mutable schema = Unchecked.defaultof<JsonElement>
                let mutable surface = Unchecked.defaultof<JsonElement>
                let mutable documentType = Unchecked.defaultof<JsonElement>

                root.ValueKind = JsonValueKind.Object
                && root.TryGetProperty("schema", &schema)
                && schema.ValueKind = JsonValueKind.String
                && String.Equals(schema.GetString(), expectedSchema, StringComparison.Ordinal)
                && root.TryGetProperty("surface", &surface)
                && surface.ValueKind = JsonValueKind.String
                && String.Equals(surface.GetString(), actorsPageSurface, StringComparison.Ordinal)
                && root.TryGetProperty("documentType", &documentType)
                && documentType.ValueKind = JsonValueKind.String
                && String.Equals(documentType.GetString(), actorTopologyDocumentType, StringComparison.Ordinal)
            with
            | :? JsonException -> false
