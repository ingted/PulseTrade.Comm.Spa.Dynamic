import { FSharpOption } from "../../../Content/WebSharper/WebSharper.StdLib/Microsoft.FSharp.Core.FSharpOption`1"
import Var from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.Var`1"
import TemplateHole from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.TemplateHole"
export default class VarDomElement extends TemplateHole {
  name:string;
  fillWith:Var<FSharpOption<Element>>;
  get Value():Var<FSharpOption<Element>>
  ApplyVarHole(el:Element):void
  WithName(n:string):TemplateHole
  get ValueObj()
  get Name():string
  constructor(name:string, fillWith:Var<FSharpOption<Element>>)
}
