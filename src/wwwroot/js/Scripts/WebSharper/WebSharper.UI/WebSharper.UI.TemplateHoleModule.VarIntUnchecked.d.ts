import Var from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.Var`1"
import { View_T } from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.View`1"
import { FSharpOption } from "../../../Content/WebSharper/WebSharper.StdLib/Microsoft.FSharp.Core.FSharpOption`1"
import { FSharpChoice } from "../../../Content/WebSharper/WebSharper.StdLib/Microsoft.FSharp.Core.FSharpChoice`2"
import { Attr_T } from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.Attr"
import TemplateHole from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.TemplateHole"
export default class VarIntUnchecked extends TemplateHole {
  name:string;
  fillWith:Var<number>;
  get Value():Var<number>
  ForTextView():FSharpOption<View_T<string>>
  get AsChoiceView():FSharpChoice<string, View_T<string>>
  AddAttribute(addAttr:((a:Element) => ((a:Attr_T) => void)), el:Element):void
  ApplyVarHole(el:Element):void
  WithName(n:string):TemplateHole
  get ValueObj()
  get Name():string
  constructor(name:string, fillWith:Var<number>)
}
