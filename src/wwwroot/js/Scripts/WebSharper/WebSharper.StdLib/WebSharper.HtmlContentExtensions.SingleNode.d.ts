import Object from "../../../Content/WebSharper/WebSharper.StdLib/System.Object"
import IControlBody from "../../../Content/WebSharper/WebSharper.StdLib/WebSharper.IControlBody"
export default class SingleNode extends Object implements IControlBody {
  node:Node;
  ReplaceInDom(old:Node):void
  constructor(node:Node)
}
