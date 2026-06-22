[<JavaScript>]
module DynamicRenderer =
    open WebSharper
    open WebSharper.UI
    open WebSharper.JavaScript

    let tryGetSchema (jsonStr: string) =
        try
            if WebSharper.UI.Client.IsClient then
                let obj = JS.Global?JSON?parse(jsonStr)
                if JS.In "schema" obj then Some (obj?schema : string) else None
            else
                if jsonStr.Contains(""schema":"fskynet-sdui") then Some "fskynet-sdui" else None
        with _ -> None
