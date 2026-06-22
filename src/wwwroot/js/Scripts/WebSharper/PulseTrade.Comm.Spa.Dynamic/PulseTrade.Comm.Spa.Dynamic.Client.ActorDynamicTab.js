import { TryRender } from "./PulseTrade.Comm.Spa.Dynamic.Client.DynamicRenderer.js"
import { LoadLocalTemplates } from "../WebSharper.UI/WebSharper.UI.Client.Templates.js"
import Doc from "../WebSharper.UI/WebSharper.UI.Doc.js"
import { Some } from "../WebSharper.StdLib/Microsoft.FSharp.Core.FSharpOption`1.js"
import Attr from "../WebSharper.UI/WebSharper.UI.Attr.js"
export function Main(){
  globalThis.console.log("EVALUATING SPAEntryPoint Main in ActorDynamicTab!");
  return _registerRenderer();
}
export function _registerRenderer(){
  globalThis.PulseTradeRegisterRenderer("fskynet-sdui", (text) => {
    try {
      globalThis.console.log(["Inside fskynet-sdui renderer wrapper! Text length:", text.length]);
      const docOpt=TryRender(text);
      if(docOpt==null){
        globalThis.console.log("Got None from TryRender");
        return null;
      }
      else {
        const doc=docOpt.$0;
        globalThis.console.log("Got Some doc! Creating container...");
        const container=globalThis.document.createElement("div");
        LoadLocalTemplates("");
        Doc.Run(container, doc);
        globalThis.console.log("Rendered doc to container!");
        return Some(container);
      }
    }
    catch(e){
      globalThis.console.error(["Extension renderer threw an exception:", e]);
      return null;
    }
  });
  return globalThis.console.log("PulseTrade.Comm.Spa.Dynamic Client Extension Started and registered fskynet-sdui!");
}
export function renderActorDynamicPage(){
  return Doc.Element("div", [Attr.Create("class", "actor-dynamic-container"), Attr.Create("style", "padding: 16px;")], [Doc.Element("h2", [Attr.Create("style", "color: #333; margin-bottom: 16px;")], [Doc.TextNode("Actor Dynamic")]), Doc.Element("p", [Attr.Create("style", "color: #666; margin-bottom: 24px;")], [Doc.TextNode("Actor Dynamic POC")]), Doc.Element("div", [Attr.Create("class", "sdui-canvas-area"), Attr.Create("style", "display: grid; grid-template-columns: 1fr 1fr; gap: 16px;")], [Doc.Element("div", [Attr.Create("style", "border: 1px solid #ccc; border-radius: 8px; padding: 16px; background-color: #fff;")], [Doc.Element("h3", [Attr.Create("style", "margin-top: 0;")], [Doc.TextNode("Canvas")]), Doc.Element("div", [Attr.Create("id", "sdui-canvas-mount"), Attr.Create("style", "min-height: 300px; border: 1px dashed #aaa; display: flex; align-items: center; justify-content: center; color: #888;")], [Doc.TextNode("Loading... (WebSocket fskynet-sdui Payload)")])]), Doc.Element("div", [Attr.Create("style", "border: 1px solid #ccc; border-radius: 8px; padding: 16px; background-color: #f9f9f9;")], [Doc.Element("h3", [Attr.Create("style", "margin-top: 0;")], [Doc.TextNode("PropertyGrid")]), Doc.Element("p", [], [Doc.TextNode("Select element")])])])]);
}
