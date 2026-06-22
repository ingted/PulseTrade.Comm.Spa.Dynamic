import { DocNode } from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.Client.DocNode"
import DocElemNode from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.Client.DocElemNode"
import Dyn from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.Client.Attrs.Dyn"
import { FSharpOption } from "../../../Content/WebSharper/WebSharper.StdLib/Microsoft.FSharp.Core.FSharpOption`1"
export default interface DocTreeNode {
  Els:((Node | DocNode))[];
  Dirty:boolean;
  Holes:(DocElemNode)[];
  Attrs:([Element, Dyn])[];
  Render?:FSharpOption<((a:Element) => void)>;
  El?:FSharpOption<Element>;
}
