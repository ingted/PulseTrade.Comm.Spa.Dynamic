namespace PulseTrade.Comm.Spa.Dynamic.Server

open System
open System.Text.Json

type SduiPayloadDiscriminator =
    { schema: string
      surface: string
      documentType: string }

type SduiPayloadKind =
    | NonSdui
    | StaticCanvas
    | FormInput
    | ActorsPage
    | Runtime
    | InvalidSdui of reasonCode: string

[<RequireQualifiedAccess>]
module SduiPayloadClassifier =
    let expectedSchema = "fskynet-sdui"
    let actorsPageSurface = "ActorsPage"
    let actorTopologyDocumentType = "ActorTopologyPage"

    let tryStringProperty (name: string) (root: JsonElement) =
        let mutable value = Unchecked.defaultof<JsonElement>

        if root.TryGetProperty(name, &value) && value.ValueKind = JsonValueKind.String then
            Some(value.GetString())
        else
            None

    let hasArrayProperty (name: string) (root: JsonElement) =
        let mutable value = Unchecked.defaultof<JsonElement>
        root.TryGetProperty(name, &value) && value.ValueKind = JsonValueKind.Array

    let classify (rawContent: string) =
        if String.IsNullOrWhiteSpace rawContent then
            SduiPayloadKind.NonSdui
        else
            try
                use document = JsonDocument.Parse(rawContent)
                let root = document.RootElement

                if root.ValueKind <> JsonValueKind.Object then
                    SduiPayloadKind.NonSdui
                else
                    match tryStringProperty "schema" root with
                    | Some schema when String.Equals(schema, expectedSchema, StringComparison.Ordinal) ->
                        match tryStringProperty "protocol" root with
                        | Some protocol when String.Equals(protocol, "sdui-runtime.v1", StringComparison.Ordinal) ->
                            SduiPayloadKind.Runtime
                        | Some _ ->
                            SduiPayloadKind.InvalidSdui "unsupported-protocol"
                        | None ->
                            match tryStringProperty "surface" root with
                            | Some surface when String.Equals(surface, "Canvas", StringComparison.Ordinal) ->
                                SduiPayloadKind.StaticCanvas
                            | Some surface when String.Equals(surface, "FormInput", StringComparison.Ordinal) ->
                                SduiPayloadKind.FormInput
                            | Some surface when String.Equals(surface, actorsPageSurface, StringComparison.Ordinal) ->
                                match tryStringProperty "documentType" root with
                                | Some documentType when String.Equals(documentType, actorTopologyDocumentType, StringComparison.Ordinal) ->
                                    SduiPayloadKind.ActorsPage
                                | _ ->
                                    SduiPayloadKind.InvalidSdui "invalid-actors-page-document-type"
                            | Some _ ->
                                SduiPayloadKind.InvalidSdui "unsupported-surface"
                            | None when hasArrayProperty "ui" root ->
                                SduiPayloadKind.StaticCanvas
                            | None ->
                                match tryStringProperty "formMode" root with
                                | Some formMode when String.Equals(formMode, "argu-form", StringComparison.Ordinal) ->
                                    SduiPayloadKind.FormInput
                                | _ ->
                                    SduiPayloadKind.InvalidSdui "missing-surface"
                    | Some _ ->
                        SduiPayloadKind.NonSdui
                    | None ->
                        SduiPayloadKind.NonSdui
            with
            | :? JsonException -> SduiPayloadKind.NonSdui

    let isActorsPagePayload (rawContent: string) =
        classify rawContent = SduiPayloadKind.ActorsPage
