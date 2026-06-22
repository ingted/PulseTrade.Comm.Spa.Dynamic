import Doc from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.Doc"
import { FSharpOption } from "../../../Content/WebSharper/WebSharper.StdLib/Microsoft.FSharp.Core.FSharpOption`1"
import TemplateHole from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.TemplateHole"
import Dictionary from "../../../Content/WebSharper/WebSharper.StdLib/System.Collections.Generic.Dictionary`2"
export default class $StartupCode_Templates {
  static RenderedFullDocTemplate:FSharpOption<Doc>;
  static TextHoleRE:string;
  static GlobalHoles:Dictionary<string, TemplateHole>;
  static LocalTemplatesLoaded:boolean;
  static LoadedTemplates:Dictionary<string, Dictionary<string, Element>>;
}
