import { FSharpList_T } from "../../../Content/WebSharper/WebSharper.StdLib/Microsoft.FSharp.Collections.FSharpList`1"
import Var from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.Var`1"
export function New(KeysPressed, KeyListenerActive, LastPressed)
export default interface KeyListenerSt {
  KeysPressed:Var<FSharpList_T<number>>;
  KeyListenerActive:boolean;
  LastPressed:Var<number>;
}
