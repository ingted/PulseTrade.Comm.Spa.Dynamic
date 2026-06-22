import Doc from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.Doc"
import { FSharpOption } from "../../../Content/WebSharper/WebSharper.StdLib/Microsoft.FSharp.Core.FSharpOption`1"
import { Attr_T } from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.Attr"
import IEnumerable from "../../../Content/WebSharper/WebSharper.StdLib/System.Collections.Generic.IEnumerable`1"
export function TryRender(rawContent:string):FSharpOption<Doc>
export function createSduiCanvas(jsonStr:string):Doc
export function renderNode(obj):Doc
export function tryGetSchema(jsonStr:string):FSharpOption<string>
export function V(name:string, attrs:IEnumerable<Attr_T>):Doc
export function E<T0>(name:string, attrs:IEnumerable<Attr_T>, children:IEnumerable<T0>):Doc
