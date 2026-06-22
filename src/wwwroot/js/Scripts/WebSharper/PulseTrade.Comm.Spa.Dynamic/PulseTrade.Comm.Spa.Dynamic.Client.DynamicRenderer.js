import { Trim, Substring, IsNullOrEmpty } from "../WebSharper.StdLib/Microsoft.FSharp.Core.StringModule.js"
import { Some } from "../WebSharper.StdLib/Microsoft.FSharp.Core.FSharpOption`1.js"
import Var from "../WebSharper.UI/WebSharper.UI.Var.js"
import { Equals } from "../WebSharper.StdLib/Microsoft.FSharp.Core.Operators.Unchecked.js"
import Attr from "../WebSharper.UI/WebSharper.UI.Attr.js"
import Doc from "../WebSharper.UI/WebSharper.UI.Doc.js"
import { Dynamic, Handler, OnAfterRender } from "../WebSharper.UI/WebSharper.UI.Client.Attr.js"
import { Map } from "../WebSharper.UI/WebSharper.UI.View.js"
import { ofSeq, ofArray, append } from "../WebSharper.StdLib/Microsoft.FSharp.Collections.ListModule.js"
import { delay } from "../WebSharper.StdLib/Microsoft.FSharp.Collections.SeqModule.js"
import { enumTryWith } from "../WebSharper.StdLib/Microsoft.FSharp.Core.CompilerServices.RuntimeHelpers.js"
import { map } from "../WebSharper.StdLib/Microsoft.FSharp.Collections.ArrayModule.js"
import FSharpList from "../WebSharper.StdLib/Microsoft.FSharp.Collections.FSharpList`1.js"
export function TryRender(rawContent){
  globalThis.console.log(["DynamicRenderer.TryRender called with:", rawContent]);
  const idx=rawContent.indexOf("replied msg:");
  const content=idx>=0?Trim(rawContent.substring(idx+"replied msg:".length)):rawContent;
  globalThis.console.log(["Content after strip:", content]);
  const m=tryGetSchema(content);
  return m!=null&&m.$==1&&m.$0=="fskynet-sdui"?(globalThis.console.log("Schema is fskynet-sdui, rendering canvas!"),Some(createSduiCanvas(content))):(globalThis.console.log(["Schema not matched:", tryGetSchema(content)]),null);
}
export function createSduiCanvas(jsonStr){
  let _1;
  const isExpanded=Var.Create_1(false);
  if(!Equals(globalThis.document.head, null)){
    const styleId="sdui-dynamic-styles";
    if(Equals(globalThis.document.getElementById(styleId), null)){
      const style=globalThis.document.createElement("style");
      _1=(style.setAttribute("id", styleId),style.textContent="\r\n                        @keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }\r\n                        .sdui-json-snippet {\r\n                            background: #222; color: #aaa; padding: 10px; border-radius: 4px; font-size: 0.85em;\r\n                            cursor: pointer; margin-bottom: 12px; white-space: pre-wrap; word-break: break-all;\r\n                            max-height: 80px; overflow: hidden; width: 100%; box-sizing: border-box;\r\n                        }\r\n                        .sdui-json-snippet.expanded {\r\n                            max-height: 400px; overflow-y: auto;\r\n                        }\r\n                    ",void globalThis.document.head.appendChild(style));
    }
    else _1=null;
  }
  else _1=null;
  const jsonSnippet=jsonStr.length>100?Substring(jsonStr, 0, 100)+"...":jsonStr;
  const isCodeExpanded=Var.Create_1(false);
  return E("div", [Attr.Create("class", "sdui-summary-card"), Attr.Create("style", "border: 1px solid #5bc0de; padding: 15px; border-radius: 6px; background: rgba(91, 192, 222, 0.1); margin-top: 10px; display: flex; flex-direction: column; align-items: flex-start;")], [E("strong", [Attr.Create("style", "display: block; margin-bottom: 5px; color: #5bc0de; font-size: 1.1em;")], [Doc.TextNode("\ud83d\udcc8 FSkynet \u52d5\u614b\u756b\u5e03 (Canvas)")]), E("span", [Attr.Create("class", "muted"), Attr.Create("style", "display: block; font-size: 0.9em; margin-bottom: 12px; color: #aaa;")], [Doc.TextNode("\u9ede\u64ca\u5c55\u958b\u4ee5\u986f\u793a\u5177\u5099\u6392\u5e8f\u3001\u7be9\u9078\u53ca\u4e0b\u55ae\u529f\u80fd\u7684\u4e92\u52d5\u5f0f\u7db2\u683c\u8207\u5716\u8868\u3002")]), E("pre", [Dynamic("class", Map((e) => e?"sdui-json-snippet expanded":"sdui-json-snippet", isCodeExpanded.View)), Attr.Create("title", "\u9ede\u64ca\u6aa2\u8996\u5b8c\u6574 JSON"), Handler("click", () =>() => isCodeExpanded.Set(!isCodeExpanded.Get()))], [Doc.TextView(Map((e) => e?jsonStr:jsonSnippet, isCodeExpanded.View))]), E("button", [Attr.Create("class", "btn btn-info"), Attr.Create("style", "background: #5bc0de; color: #111; font-weight: bold; border: none; padding: 6px 12px; border-radius: 4px; cursor: pointer; margin-bottom: 10px;"), Handler("click", () =>() => isExpanded.Set(!isExpanded.Get()))], [Doc.TextView(Map((e) => e?"\u6536\u5408 Canvas":"\u5c55\u958b Canvas", isExpanded.View))]), Doc.EmbedView(Map((expanded) => expanded?E("div", [Attr.Create("style", "margin-top: 10px; width: 100%; border-top: 1px dashed #5bc0de; padding-top: 15px;")], ofSeq(delay(() => enumTryWith(delay(() => {
    let r;
    let payloadObj=JSON.parse(jsonStr);
    let sduiNode=payloadObj.ui||payloadObj.sdui;
    if(!sduiNode)r=[];
    let unwrapped=globalThis.unwrapFCell?globalThis.unwrapFCell(sduiNode):sduiNode;
    r=Array.isArray(unwrapped)?unwrapped:[unwrapped];
    let _2=map(renderNode, r);
    let _3=ofArray(_2);
    let _4=[Doc.Concat(_3)];
    return[E("div", [], _4)];
  }), () => 1, (ex) =>[E("pre", [Attr.Create("style", "color: #d9534f;")], [Doc.TextNode("Error parsing SDUI Canvas: "+ex.message)])])))):Doc.Empty, isExpanded.View))]);
}
export function renderNode(obj){
  if(Equals(typeof obj, "undefined")||Equals(obj, null))return Doc.Empty;
  else {
    const t=obj.type;
    switch(t){
      case"Heading":
        const textStr=obj.text||"";
        return E("h2", [Attr.Create("style", "color: #5bc0de; margin-bottom: 15px;")], [Doc.TextNode(textStr)]);
      case"Label":
        const textStr_1=obj.text||"";
        return E("span", [Attr.Create("style", "margin-right: 10px; color: #ccc;")], [Doc.TextNode(textStr_1)]);
      case"TextInput":
        const placeholderStr=obj.placeholder||"";
        const idStr=obj.id||"";
        return V("input", append(ofArray([Attr.Create("type", "text"), Attr.Create("placeholder", placeholderStr), Attr.Create("style", "padding: 8px; background: #333; color: white; border: 1px solid #555; border-radius: 4px; display: block; width: 100%; box-sizing: border-box; margin: 5px 0;")]), !IsNullOrEmpty(idStr)?ofArray([OnAfterRender((el) => {
          el.setAttribute("id", idStr);
        })]):FSharpList.Empty));
      case"Row":
        const childrenDocs=ofArray(map(renderNode, obj.children||[]));
        return E("div", [Attr.Create("style", "display: flex; flex-direction: row; gap: 15px; margin-bottom: 10px; align-items: center;")], childrenDocs);
      case"Column":
        const childrenDocs_1=ofArray(map(renderNode, obj.children||[]));
        return E("div", [Attr.Create("style", "display: flex; flex-direction: column; gap: 10px;")], childrenDocs_1);
      case"Divider":
        return V("hr", [Attr.Create("style", "border: 0; border-top: 1px solid #444; margin: 15px 0; width: 100%;")]);
      case"SelectBox":
      case"Dropdown":
        const isMultiple=!(!obj.multiple);
        const optionDocs=ofArray(map((opt) => E("option", [], [Doc.TextNode(opt)]), obj.options||[]));
        return E("select", append(ofArray([Attr.Create("style", "padding: 8px; margin: 5px 0; background: #333; color: white; border: 1px solid #555; border-radius: 4px; font-size: 1rem; display: block; width: 200px;")]), isMultiple?ofArray([OnAfterRender((el) => {
          el.setAttribute("multiple", "multiple");
        })]):FSharpList.Empty), optionDocs);
      case"DataGrid":
        return E("div", [Attr.Create("style", "background: #1e1e1e; border-radius: 8px; overflow: hidden; border: 1px solid #444; margin: 20px 0;")], [Doc.TextNode("DataGrid rendered (Requires DataRef bindings)")]);
      case"Button":
        const btnText=obj.text||"Button";
        return E("button", [Attr.Create("class", "btn btn-success canvas-btn"), Attr.Create("style", "margin-top: 15px; padding: 10px 20px; font-weight: bold; background: #5cb85c; color: white; border: none; border-radius: 4px; cursor: pointer; font-size: 1rem;"), Handler("click", () =>() => globalThis.alert("Dispatcher: Sending command..."))], [Doc.TextNode(btnText)]);
      case"AppLoader":
        const textStr_2=obj.text||"Loading...";
        return E("div", [Attr.Create("style", "display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 20px; color: #5bc0de;")], [V("div", [Attr.Create("style", "border: 4px solid rgba(255,255,255,0.1); border-top: 4px solid #5bc0de; border-radius: 50%; width: 40px; height: 40px; animation: spin 1s linear infinite;")]), E("span", [Attr.Create("style", "margin-top: 10px;")], [Doc.TextNode(textStr_2)])]);
      case"ColorPicker":
        const defaultColor=obj.defaultColor||"#000000";
        const idStr_1=obj.id||"";
        return V("input", append(ofArray([Attr.Create("type", "color"), Attr.Create("value", defaultColor), Attr.Create("style", "padding: 0; margin: 5px 0; background: none; border: 1px solid #555; border-radius: 4px; cursor: pointer; height: 40px; width: 60px;")]), !IsNullOrEmpty(idStr_1)?ofArray([OnAfterRender((el) => {
          el.setAttribute("id", idStr_1);
        })]):FSharpList.Empty));
      case"DatePicker":
        return V("input", [Attr.Create("type", "date"), Attr.Create("style", "padding: 8px; margin: 5px 0; background: #333; color: white; border: 1px solid #555; border-radius: 4px; display: inline-block;")]);
      case"TimePicker":
        return V("input", [Attr.Create("type", "time"), Attr.Create("style", "padding: 8px; margin: 5px 0; background: #333; color: white; border: 1px solid #555; border-radius: 4px; display: inline-block;")]);
      case"Pagination":
        return E("div", [Attr.Create("style", "display: flex; gap: 5px; margin: 15px 0; justify-content: center;")], [E("button", [Attr.Create("style", "padding: 5px 10px; background: #444; color: white; border: 1px solid #555; cursor: pointer;")], [Doc.TextNode("Prev")]), E("button", [Attr.Create("style", "padding: 5px 10px; background: #444; color: white; border: 1px solid #555; cursor: pointer;")], [Doc.TextNode("Next")])]);
      case"AutoComplete":
        return E("div", [Attr.Create("style", "position: relative; display: inline-block; width: 100%; margin: 5px 0;")], [V("input", [Attr.Create("type", "text"), Attr.Create("placeholder", "Search..."), Attr.Create("style", "padding: 8px; background: #333; color: white; border: 1px solid #555; border-radius: 4px; display: block; width: 100%; box-sizing: border-box;")])]);
      case"Rolling":
        return E("div", [Attr.Create("style", "padding: 10px; background: #222; color: #5bc0de; border-radius: 4px; border: 1px solid #444; margin: 10px 0;")], [Doc.TextNode("Rolling...")]);
      case"Tree":
        const dataRefStr=obj.dataRef||"";
        return E("ul", [Attr.Create("style", "list-style-type: none; padding-left: 20px; color: #ccc;")], [E("li", [Attr.Create("style", "padding: 5px 0; cursor: pointer;")], [Doc.TextNode("Tree Node bound to: "+dataRefStr)])]);
      case"ContextMenu":
        return V("div", [Attr.Create("style", "display: none; position: absolute; background: #333; border: 1px solid #555; box-shadow: 2px 2px 5px rgba(0,0,0,0.5); z-index: 1000;")]);
      default:
        return Doc.Empty;
    }
  }
}
export function tryGetSchema(jsonStr){
  try {
    const obj=globalThis.JSON.parse(jsonStr);
    return"schema"in obj?Some(obj.schema):null;
  }
  catch(m){
    return null;
  }
}
export function V(name, attrs){
  return Doc.Element(name, attrs, FSharpList.Empty);
}
export function E(name, attrs, children){
  return Doc.Element(name, attrs, children);
}
