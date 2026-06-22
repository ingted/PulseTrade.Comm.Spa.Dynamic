import { View_T } from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.View`1"
import { FSharpChoice } from "../../../Content/WebSharper/WebSharper.StdLib/Microsoft.FSharp.Core.FSharpChoice`2"
import { FSharpOption } from "../../../Content/WebSharper/WebSharper.StdLib/Microsoft.FSharp.Core.FSharpOption`1"
import { Attr_T } from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.Attr"
import Object from "../../../Content/WebSharper/WebSharper.StdLib/System.Object"
export default class TemplateHole extends Object {
  static Value(th:TemplateHole)
  get AsChoiceView():FSharpChoice<string, View_T<string>>
  ForTextView():FSharpOption<View_T<string>>
  ApplyVarHole(el:Element):void
  AddAttribute(a:((a:Element) => ((a:Attr_T) => void)), a_1:Element):void
}
