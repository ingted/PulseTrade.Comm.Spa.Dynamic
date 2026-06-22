import { FSharpOption } from "../../Content/WebSharper/WebSharper.StdLib/Microsoft.FSharp.Core.FSharpOption`1"
export function tryJson<T0>(text:string):FSharpOption<T0>
export function jsonOrDefault<T0>(fallback:T0, text:string):T0
export function json<T0>(text:string):T0
