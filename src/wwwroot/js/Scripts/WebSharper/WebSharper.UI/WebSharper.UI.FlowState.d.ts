import Var from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.Var`1"
import FlowPage from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.FlowPage"
import { FSharpOption } from "../../../Content/WebSharper/WebSharper.StdLib/Microsoft.FSharp.Core.FSharpOption`1"
import Doc from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.Doc"
export default class FlowState {
  Id:number;
  Index:Var<number>;
  Pages:(FlowPage)[];
  EndedOn:FSharpOption<number>;
  FirstRender:boolean;
  RenderFirst:(() => void);
  Embed():Doc
  get Navigator():{Get:(() => number),Set:((a:number) => void)}
  End(page:Doc):void
  Restart():void
  Cancel(page:Doc):void
  Add(page:FlowPage):void
  UpdatePage(f:((a:number) => number)):void
  static New(Id:number, Index:Var<number>, Pages:(FlowPage)[], EndedOn:FSharpOption<number>, FirstRender:boolean, RenderFirst:(() => void)):FlowState
}
