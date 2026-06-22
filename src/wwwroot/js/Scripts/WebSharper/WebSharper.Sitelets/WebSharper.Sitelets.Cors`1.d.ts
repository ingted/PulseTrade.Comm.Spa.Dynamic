import CorsAllows from "../../../Content/WebSharper/WebSharper.Sitelets/WebSharper.Sitelets.CorsAllows"
import { FSharpOption } from "../../../Content/WebSharper/WebSharper.StdLib/Microsoft.FSharp.Core.FSharpOption`1"
export default interface Cors<T0>{
  DefaultAllows?:FSharpOption<CorsAllows>;
  EndPoint?:FSharpOption<T0>;
}
