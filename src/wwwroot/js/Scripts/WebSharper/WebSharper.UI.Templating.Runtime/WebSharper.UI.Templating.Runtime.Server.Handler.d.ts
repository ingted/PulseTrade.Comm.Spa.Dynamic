import TemplateHole from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.TemplateHole"
import IEnumerable from "../../../Content/WebSharper/WebSharper.StdLib/System.Collections.Generic.IEnumerable`1"
import { FSharpOption } from "../../../Content/WebSharper/WebSharper.StdLib/Microsoft.FSharp.Core.FSharpOption`1"
import { CompletedHoles } from "../../../Content/WebSharper/WebSharper.UI.Templating.Runtime/WebSharper.UI.Templating.Runtime.Server.CompletedHoles"
import TemplateInstance from "../../../Content/WebSharper/WebSharper.UI.Templating.Runtime/WebSharper.UI.Templating.Runtime.Server.TemplateInstance"
export function CompleteHoles(key:string, filledHoles:IEnumerable<TemplateHole>, vars:([string, number, FSharpOption<any>])[]):[IEnumerable<TemplateHole>, CompletedHoles]
export function AfterRenderQ2(key:string, holeName:string, ti:(() => TemplateInstance), f):TemplateHole
export function EventQ2<T0>(key:string, holeName:string, ti:(() => TemplateInstance), f):TemplateHole
