import IAttrNode from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.Client.IAttrNode"
import { FSharpOption } from "../../../Content/WebSharper/WebSharper.StdLib/Microsoft.FSharp.Core.FSharpOption`1"
export function New(DynElem, DynFlags, DynNodes, OnAfterRender)
export default interface Dyn {
  DynElem:Element;
  DynFlags:number;
  DynNodes:(IAttrNode)[];
  OnAfterRender?:FSharpOption<((a:Element) => void)>;
}
