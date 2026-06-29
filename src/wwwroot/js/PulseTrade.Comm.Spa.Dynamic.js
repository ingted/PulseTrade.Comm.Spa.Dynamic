import Runtime from "./WebSharper.Core.JavaScript/Runtime.js"
Runtime.ScriptBasePath="/Scripts/";
import { MarkResizable, Lazy, Create as Create_1, GetOptional, SetOptional } from "./WebSharper.Core.JavaScript/Runtime.js"
function isIDisposable(x){
  return"Dispose"in x;
}
function Register(){
  loadSchemasFromManifest();
  registerAddKeyRenderer("dynamic-argu-add-key", 100, renderAddKey);
  registerAppendInputRenderer("dynamic-argu-append-input", 100, renderAppendInput);
}
function loadSchemasFromManifest(){
  const node=doc().getElementById("ptc-comm-client-extensions");
  if(!(node==null)&&!isBlank(node.textContent)){
    const m=tryDecodeJson(node.textContent);
    if(m!=null&&m.$==1)iter((extension) => {
      if(!(extension==null)&&!isBlank(extension.metadataJson)){
        const m_1=tryDecodeJson(extension.metadataJson);
        if(m_1==null)void 0;
        else {
          const metadata=m_1.$0;
          iter((s) => {
            upsertSchema(s);
          }, arrayOrEmpty(metadata.dynamicArguSchemas));
          iter((d) => {
            upsertDocument(d);
          }, arrayOrEmpty(metadata.dynamicFormDocuments));
        }
      }
    }, arrayOrEmpty(m.$0));
  }
  else void 0;
}
function registerAddKeyRenderer(name, priority, renderer){
  if(Equals(typeof globalThis.PulseTradeRegisterAddKeyRenderer, "function"))globalThis.PulseTradeRegisterAddKeyRenderer(name, priority, renderer);
}
function renderAddKey(ctx){
  const shape=asText(ctx.shape).toLowerCase();
  const supportsProxy=shape=="actor-dynamic-proxy";
  if(!(shape=="actor-dynamic-target"||shape=="actor-argu-target"||shape=="actor-argu")&&!supportsProxy)return null;
  else if(supportsProxy){
    const defaultKeyParts=keyPartsFromJson(ctx.defaultKey);
    const o=tryHead(defaultKeyParts);
    const defaultProxyAddress=o==null?"":o.$0;
    const defaultRnAddress=length(defaultKeyParts)>2&&get(defaultKeyParts, 1)=="proxy-v1"?get(defaultKeyParts, 2):"";
    const defaultTargetKind=length(defaultKeyParts)>3&&get(defaultKeyParts, 1)=="proxy-v1"?get(defaultKeyParts, 3):"raw";
    const root=setTestId("dynamic-argu-proxy-key", element("div", "dynamic-argu-add-key dynamic-argu-proxy-key", null));
    const proxyLabel=element("label", "dynamic-argu-label", "Proxy actor address");
    proxyLabel.setAttribute("for", "dynamic-argu-proxy-actor");
    const proxyActor=input("text", "dynamic-argu-actor-address", "dynamic-argu-proxy-actor");
    proxyActor.setAttribute("id", "dynamic-argu-proxy-actor");
    proxyActor.setAttribute("placeholder", "akka.tcp://PtcsHost@127.0.0.1:9779/user/durable-proxy");
    proxyActor.value=defaultProxyAddress;
    const rnLabel=element("label", "dynamic-argu-label", "RN actor address");
    rnLabel.setAttribute("for", "dynamic-argu-rn-actor");
    const rnActor=input("text", "dynamic-argu-actor-address", "dynamic-argu-rn-actor");
    rnActor.setAttribute("id", "dynamic-argu-rn-actor");
    rnActor.setAttribute("placeholder", "akka.tcp://ResourceNode@127.0.0.1:9791/user/echo");
    rnActor.value=defaultRnAddress;
    const kindLabel=element("label", "dynamic-argu-label", "Target kind");
    kindLabel.setAttribute("for", "dynamic-argu-proxy-kind");
    const targetKind=input("text", "dynamic-argu-proxy-kind", "dynamic-argu-proxy-kind");
    targetKind.setAttribute("id", "dynamic-argu-proxy-kind");
    targetKind.setAttribute("placeholder", "raw | canvas-json | argu");
    targetKind.value=isBlank(defaultTargetKind)?"raw":defaultTargetKind;
    const aliasLabel=element("label", "dynamic-argu-label", "Target alias");
    aliasLabel.setAttribute("for", "dynamic-argu-proxy-alias");
    const aliasInput=input("text", "dynamic-argu-key-alias", "dynamic-argu-proxy-alias");
    aliasInput.setAttribute("id", "dynamic-argu-proxy-alias");
    aliasInput.setAttribute("placeholder", "Display name (optional)");
    const actions=setTestId("dynamic-argu-proxy-key-actions", element("div", "dynamic-argu-key-actions", null));
    const clean=button("dynamic-argu-key-clean", "dynamic-argu-proxy-key-clean", "Clean");
    const cancel_1=button("dynamic-argu-key-cancel", "dynamic-argu-proxy-key-cancel", "Cancel");
    const submit=button("dynamic-argu-key-ok primary", "dynamic-argu-proxy-key-submit", "OK");
    clean.addEventListener("click", () => {
      proxyActor.value="";
      rnActor.value="";
      targetKind.value="raw";
      aliasInput.value="";
      return proxyActor.focus();
    });
    cancel_1.addEventListener("click", ctx.cancelKey);
    submit.addEventListener("click", () => {
      const proxyAddress=Trim(proxyActor.value);
      const rnAddress=Trim(rnActor.value);
      const kind=Trim(targetKind.value);
      return isBlank(proxyAddress)?proxyActor.focus():isBlank(rnAddress)?rnActor.focus():ctx.submitKey(New_3([proxyAddress, "proxy-v1", rnAddress, isBlank(kind)?"raw":kind], Trim(aliasInput.value)));
    });
    append(actions, [clean, cancel_1, submit]);
    append(root, [proxyLabel, proxyActor, rnLabel, rnActor, kindLabel, targetKind, aliasLabel, aliasInput, actions]);
    return Some(root);
  }
  else {
    const keys=sort(distinct(concat([documentKeys(), schemaKeys()])));
    const defaultKeyParts_1=keyPartsFromJson(ctx.defaultKey);
    const o_1=tryHead(defaultKeyParts_1);
    const defaultActorAddress=o_1==null?"":o_1.$0;
    const defaultTypeName=length(defaultKeyParts_1)>1?get(defaultKeyParts_1, 1):"";
    if(length(keys)===0)return Some(errorNode("No Dynamic Argu schemas are registered."));
    else {
      const root_1=setTestId("dynamic-argu-add-key", element("div", "dynamic-argu-add-key", null));
      const actorLabel=element("label", "dynamic-argu-label", "Actor address");
      actorLabel.setAttribute("for", "dynamic-argu-key-actor");
      const actor=input("text", "dynamic-argu-actor-address", "dynamic-argu-key-actor");
      actor.setAttribute("id", "dynamic-argu-key-actor");
      actor.setAttribute("placeholder", "/user/durable-proxy");
      actor.value=defaultActorAddress;
      const typeLabel=element("label", "dynamic-argu-label", "DU type or template key");
      typeLabel.setAttribute("for", "dynamic-argu-key-du-type");
      const typeInput=input("text", "dynamic-argu-du-type", "dynamic-argu-key-du-type");
      typeInput.setAttribute("id", "dynamic-argu-key-du-type");
      typeInput.setAttribute("placeholder", "Full DU type name or template key");
      typeInput.value=defaultTypeName;
      const aliasLabel_1=element("label", "dynamic-argu-label", "Target alias");
      aliasLabel_1.setAttribute("for", "dynamic-argu-key-alias");
      const aliasInput_1=input("text", "dynamic-argu-key-alias", "dynamic-argu-key-alias");
      aliasInput_1.setAttribute("id", "dynamic-argu-key-alias");
      aliasInput_1.setAttribute("placeholder", "Display name (optional)");
      const targetConfig=setTestId("dynamic-argu-key-target-config", element("div", "dynamic-argu-target-config", null));
      const argInput=doc().createElement("textarea");
      argInput.className="dynamic-argu-canonical-arg-string";
      argInput.setAttribute("rows", "3");
      argInput.setAttribute("placeholder", "--say \"hello\"");
      setTestId("dynamic-argu-key-canonical-arg-string", argInput);
      argInput.value=length(defaultKeyParts_1)>2?get(defaultKeyParts_1, 2):"";
      const renderTargetConfig=() => {
        targetConfig.textContent="";
        const typeName=Trim(typeInput.value);
        if(isBlank(typeName))targetConfig.appendChild(element("div", "dynamic-argu-target-note", "Enter a full DU type name or registered template key."));
        else if(tryFindDocument(typeName)==null){
          const _1=tryFindSchema(typeName);
          if(_1!=null&&_1.$==1){
            const label_1=element("label", "dynamic-argu-label", "Canonical Argu string");
            label_1.setAttribute("for", "dynamic-argu-key-canonical-arg-string");
            targetConfig.appendChild(label_1);
            targetConfig.appendChild(argInput);
          }
          else targetConfig.appendChild(errorNode("Dynamic Argu schema not found for DU type: "+typeName));
        }
        else targetConfig.appendChild(element("div", "dynamic-argu-target-note", "Direct DSL document target; no canonical Argu string required."));
      };
      typeInput.addEventListener("input", renderTargetConfig);
      typeInput.addEventListener("change", renderTargetConfig);
      renderTargetConfig();
      const actions_1=setTestId("dynamic-argu-key-actions", element("div", "dynamic-argu-key-actions", null));
      const clean_1=button("dynamic-argu-key-clean", "dynamic-argu-key-clean", "Clean");
      const cancel_2=button("dynamic-argu-key-cancel", "dynamic-argu-key-cancel", "Cancel");
      const submit_1=button("dynamic-argu-key-ok primary", "dynamic-argu-key-submit", "OK");
      clean_1.addEventListener("click", () => {
        actor.value="";
        typeInput.value="";
        aliasInput_1.value="";
        argInput.value="";
        renderTargetConfig();
        return actor.focus();
      });
      cancel_2.addEventListener("click", ctx.cancelKey);
      submit_1.addEventListener("click", () => {
        let keyTail;
        const actorAddress=Trim(actor.value);
        const selectedTypeName=Trim(typeInput.value);
        const displayName=Trim(aliasInput_1.value);
        if(tryFindDocument(selectedTypeName)==null){
          const canonicalArgString=Trim(argInput.value);
          keyTail=isBlank(canonicalArgString)?(argInput.focus(),[]):[canonicalArgString];
        }
        else keyTail=[];
        return isBlank(actorAddress)?actor.focus():isBlank(selectedTypeName)?typeInput.focus():tryFindDocument(selectedTypeName)!=null||length(keyTail)>0?ctx.submitKey(New_3(ofSeq(delay(() => append_2([actorAddress], delay(() => append_2([selectedTypeName], delay(() => keyTail)))))), displayName)):null;
      });
      append(actions_1, [clean_1, cancel_2, submit_1]);
      append(root_1, [actorLabel, actor, typeLabel, typeInput, aliasLabel_1, aliasInput_1, targetConfig, actions_1]);
      return Some(root_1);
    }
  }
}
function registerAppendInputRenderer(name, priority, renderer){
  if(Equals(typeof globalThis.PulseTradeRegisterAppendInputRenderer, "function"))globalThis.PulseTradeRegisterAppendInputRenderer(name, priority, renderer);
}
function renderAppendInput(ctx){
  let _1;
  const keyParts=normalizeDynamicTargetKeyParts(ctx.keyParts);
  const typeName=length(keyParts)>1?asText(get(keyParts, 1)):asText(ctx.duTypeName);
  const isBackendTarget=length(keyParts)===3&&!isBlank(get(keyParts, 2));
  if(isBlank(typeName)||typeName=="proxy-v1")return null;
  else {
    const root=setTestId("dynamic-argu-form", element("div", "dynamic-argu-form", "Loading Dynamic Argu form..."));
    if(isBackendTarget){
      postJson("/client-extensions/dynamic/argu/resolve-target", New_4(keyParts), (reply) => {
        if(reply.ok&&!(reply.document==null)&&!(reply.document.arguFormSchema==null))renderSchemaIntoRoot(root, ctx, asText(reply.templateKey), Some(reply.document), reply.document.arguFormSchema);
        else {
          root.textContent="";
          root.appendChild(errorNode(isBlank(reply.error)?"Dynamic Argu target resolution failed.":reply.error));
        }
      }, (error) => {
        root.textContent="";
        root.appendChild(errorNode(error));
      });
      return Some(root);
    }
    else {
      const document=tryFindDocument(typeName);
      const schema=document!=null&&document.$==1&&(!(document.$0.arguFormSchema==null)&&(_1=document.$0,true))?Some(_1.arguFormSchema):tryFindSchema(typeName);
      return schema!=null&&schema.$==1?(renderSchemaIntoRoot(root, ctx, typeName, document, schema.$0),Some(root)):Some(errorNode("Dynamic Form document or Argu schema not found for target: "+typeName));
    }
  }
}
function doc(){
  return _c_2.doc;
}
function isBlank(value){
  return IsNullOrWhiteSpace(asText(value));
}
function tryDecodeJson(text){
  try {
    return Some(decodeJson(text));
  }
  catch(m){
    return null;
  }
}
function arrayOrEmpty(values){
  return values==null||Equals(typeof values, "undefined")?[]:values;
}
function upsertSchema(schema){
  if(!(schema==null)&&!isBlank(schema.duTypeName))set_schemas(filter((existing) => asText(existing.duTypeName)!=asText(schema.duTypeName), schemas()).concat([schema]));
}
function upsertDocument(document){
  if(!(document==null)&&!isBlank(document.documentId)){
    set_documents(filter((existing) => asText(existing.documentId)!=asText(document.documentId), documents()).concat([document]));
    !(document.arguFormSchema==null)?upsertSchema(document.arguFormSchema):void 0;
  }
}
function asText(value){
  return value==null||Equals(typeof value, "undefined")?"":value;
}
function keyPartsFromJson(text){
  let _1;
  const m=tryDecodeJson(text);
  if(m==null){
    const m_1=tryDecodeJson(text);
    return m_1!=null&&m_1.$==1&&(!isBlank(m_1.$0)&&(_1=m_1.$0,true))?[_1]:[];
  }
  else return map(asText, arrayOrEmpty(m.$0));
}
function element(tag, className, textValue){
  const node=doc().createElement(tag);
  if(!isBlank(className))node.className=className;
  if(!(textValue==null))node.textContent=textValue;
  return node;
}
function setTestId(id, node){
  !isBlank(id)?node.setAttribute("data-testid", id):void 0;
  return node;
}
function input(inputType, className, testId){
  const node=doc().createElement("input");
  node.setAttribute("type", inputType);
  node.className=className;
  setTestId(testId, node);
  return node;
}
function button(className, testId, label_1){
  const node=element("button", className, label_1);
  node.setAttribute("type", "button");
  setTestId(testId, node);
  return node;
}
function append(parent, children){
  iter((child) => {
    parent.appendChild(child);
  }, children);
  return parent;
}
function errorNode(message){
  const root=setTestId("dynamic-argu-error", element("div", "dynamic-argu-error", message));
  root.setAttribute("role", "alert");
  return root;
}
function tryFindSchema(duTypeName){
  return tryFind((schema) => asText(schema.duTypeName)==asText(duTypeName), schemas());
}
function tryFindDocument(documentId){
  return tryFind((document) => asText(document.documentId)==asText(documentId), documents());
}
function schemaKeys(){
  return sort(filter((x) =>!isBlank(x), map((a) => a.duTypeName, schemas())));
}
function documentKeys(){
  return sort(filter((x) =>!isBlank(x), map((a) => a.documentId, documents())));
}
function normalizeDynamicTargetKeyParts(keyParts){
  const keyParts_1=map(asText, arrayOrEmpty(keyParts));
  if(length(keyParts_1)===3){
    const first=get(keyParts_1, 0);
    const second=get(keyParts_1, 1);
    const third=get(keyParts_1, 2);
    return!isActorAddress(first)&&isActorAddress(second)&&isRegisteredArguSchema(third)?[second, third, first]:keyParts_1;
  }
  else return keyParts_1;
}
function postJson(url, body, onOk, onError){
  const headers=new Headers();
  headers.set("Content-Type", "application/json");
  (globalThis.fetch(url, {
    method:"POST", 
    headers:headers, 
    body:JSON.stringify(body)
  }).then((response) => response.text().then((responseBody) => response.ok?onOk(decodeJson(isBlank(responseBody)?"{}":responseBody)):onError(isBlank(responseBody)?"POST "+String(url)+" "+String(response.status):responseBody))))["catch"]((error) => onError(errorMessage(error)));
}
function renderSchemaIntoRoot(root, context, typeName, document, schema){
  root.textContent="";
  const defaultMap=defaultsFromDocument(document);
  const isDocumentBacked=document!=null;
  const allowed=document==null?unionCaseNamesFromContext(context, schema):map((a) => a.name, arrayOrEmpty(schema.unionCases));
  const unionCases=filter((item) => exists((name) => name==asText(item.name), allowed), arrayOrEmpty(schema.unionCases));
  root.setAttribute("data-dynamic-argu-du-type", typeName);
  root.setAttribute("data-dynamic-form-document-id", document==null?"":asText(document.$0.documentId));
  root.setAttribute("data-dynamic-argu-union-cases", concat_2(",", allowed));
  if(length(unionCases)===0)root.appendChild(errorNode("Dynamic Argu schema has no requested union cases for DU type: "+typeName));
  else {
    const fullPreviewOpt=isDocumentBacked?Some(setTestId("dynamic-argu-raw-preview-full", element("pre", "dynamic-argu-raw-preview full", ""))):null;
    const fullSendOpt=isDocumentBacked?Some(button("dynamic-argu-send full", "dynamic-argu-send-full", "Send")):null;
    const fullRaw=() => {
      const parts=MarkResizable([]);
      const previews=root.querySelectorAll(".dynamic-argu-case-row .dynamic-argu-raw-preview");
      for(let index=0, _1=previews.length-1;index<=_1;index++){
        const text=Trim(asText(previews[index].textContent));
        if(!isBlank(text))parts.push(text);
      }
      return Join_1(" ", parts.slice());
    };
    const refreshFullPreview=() => {
      if(fullPreviewOpt!=null&&fullPreviewOpt.$==1)fullPreviewOpt.$0.textContent=fullRaw();
    };
    iter((unionCase) => {
      let fieldGetters;
      const caseName=asText(unionCase.name);
      const caseRow=setTestId("dynamic-argu-case-"+caseName, element("section", "dynamic-argu-case-row", null));
      caseRow.setAttribute("data-dynamic-argu-case", caseName);
      const heading=setTestId("dynamic-argu-case-title-"+caseName, element("div", "dynamic-argu-case-title", isBlank(unionCase.label)?caseName:asText(unionCase.label)));
      const fields=setTestId("dynamic-argu-fields-"+caseName, element("div", "dynamic-argu-fields", null));
      const rawPreview=setTestId("dynamic-argu-raw-preview-"+caseName, element("pre", "dynamic-argu-raw-preview", ""));
      const send=button("dynamic-argu-send", "dynamic-argu-send-"+caseName, "Send");
      fieldGetters=[];
      const caseRaw=() => buildRawArgu(unionCase, fieldGetters);
      const refreshPreview=() => {
        rawPreview.textContent=caseRaw();
        refreshFullPreview();
      };
      fieldGetters=map((field) => {
        const p=renderField(refreshPreview, defaultMap, caseName, field);
        fields.appendChild(p[0]);
        return[field, p[1]];
      }, arrayOrEmpty(unionCase.fields));
      send.addEventListener("click", () => {
        const raw=caseRaw();
        rawPreview.textContent=raw;
        return context.submit(New_31(raw, typeName, caseName, keyJsonForSubmit(normalizeDynamicTargetKeyParts(context.keyParts))));
      });
      append(caseRow, isDocumentBacked?[heading, fields, rawPreview]:[heading, fields, rawPreview, send]);
      root.appendChild(caseRow);
      refreshPreview();
    }, unionCases);
    if(isDocumentBacked){
      if(fullPreviewOpt!=null&&fullPreviewOpt.$==1){
        if(fullSendOpt!=null&&fullSendOpt.$==1){
          const fullPreview=fullPreviewOpt.$0;
          const fullSend=fullSendOpt.$0;
          fullSend.addEventListener("click", () => {
            const raw=fullRaw();
            fullPreview.textContent=raw;
            return context.submit(New_31(raw, typeName, "__document", keyJsonForSubmit(normalizeDynamicTargetKeyParts(context.keyParts))));
          });
          append(root, [fullPreview, fullSend]);
          refreshFullPreview();
        }
        else void 0;
      }
      else void 0;
    }
    else void 0;
  }
}
function decodeJson(text){
  return JSON.parse(asText(text));
}
function set_schemas(_1){
  _c_2.schemas=_1;
}
function schemas(){
  return _c_2.schemas;
}
function set_documents(_1){
  _c_2.documents=_1;
}
function documents(){
  return _c_2.documents;
}
function isActorAddress(value){
  const text=Trim(asText(value));
  const lower_1=text.toLowerCase();
  return StartsWith(text, "/")||StartsWith(lower_1, "akka.")||StartsWith(lower_1, "akka://");
}
function isRegisteredArguSchema(value){
  return tryFindSchema(value)!=null;
}
function errorMessage(error){
  return error==null?"unknown error":String(error);
}
function unionCaseNamesFromContext(ctx, schema){
  const allowed=filter((x) =>!isBlank(x), map(asText, arrayOrEmpty(ctx.unionCaseNames)));
  return length(allowed)===0?map((a) => a.name, arrayOrEmpty(schema.unionCases)):allowed;
}
function buildRawArgu(unionCase, fieldGetters){
  const raw=buildRawArguFromValues(map((_1) =>[_1[0], _1[1]()], fieldGetters));
  const prefix=Trim(asText(unionCase.arguName));
  const isOptionPrefix=prefix.length>=2&&Substring(prefix, 0, 2)=="--";
  return isBlank(prefix)||isOptionPrefix?raw:isBlank(raw)?prefix:prefix+" "+raw;
}
function renderField(refresh, defaultMap, caseName, field){
  let getter, _1;
  const x=element("div", "dynamic-argu-field", null);
  const row=setTestId("dynamic-argu-field-"+asText(field.name), x);
  row.setAttribute("data-dynamic-argu-field", asText(field.name));
  row.setAttribute("data-dynamic-argu-kind", asText(field.kind));
  row.appendChild(label(isBlank(field.label)?field.name:field.label));
  const fieldDefaults=defaultValuesFor(defaultMap, caseName, field.name);
  getter=() =>[];
  const wireInputEvents=(node_4) => {
    node_4.addEventListener("input", refresh);
    node_4.addEventListener("change", refresh);
  };
  const m=asText(field.kind);
  switch(m){
    case"number":
      const node=input("number", "dynamic-argu-input", "dynamic-argu-number-"+asText(field.name));
      const o=tryHead(fieldDefaults);
      let _2=o==null?"":o.$0;
      node.value=_2;
      node.setAttribute("data-dynamic-argu-input", "true");
      wireInputEvents(node);
      row.appendChild(node);
      _1=void(getter=() =>[elementValue(node)]);
      break;
    case"enum":
      const node_1=doc().createElement("select");
      node_1.className="dynamic-argu-select";
      setTestId("dynamic-argu-enum-"+asText(field.name), node_1);
      node_1.setAttribute("data-dynamic-argu-input", "true");
      iter((value_1) => {
        const option=doc().createElement("option");
        option.setAttribute("value", asText(value_1));
        option.textContent=asText(value_1);
        node_1.appendChild(option);
      }, arrayOrEmpty(field.options));
      const x_1=tryHead(fieldDefaults);
      const v=elementValue(node_1);
      let _3=x_1==null?v:x_1.$0;
      setElementValue(node_1, _3);
      wireInputEvents(node_1);
      row.appendChild(node_1);
      _1=void(getter=() =>[elementValue(node_1)]);
      break;
    case"tuple":
      const x_2=element("div", "dynamic-argu-tuple", null);
      const tuple=setTestId("dynamic-argu-tuple-"+asText(field.name), x_2);
      const itemGetters=MarkResizable([]);
      _1=(iteri((_5, _6) => {
        let p;
        const x_3=element("div", "dynamic-argu-tuple-item", null);
        const itemRow=setTestId("dynamic-argu-tuple-item-"+String(asText(field.name))+"-"+String(_5+1), x_3);
        itemRow.appendChild(label(String(_5+1)+". "+String(isBlank(_6.label)?_6.name:_6.label)));
        const testId="";
        const defaults=defaultValuesFor(defaultMap, caseName, _6.name);
        const m_1=asText(_6.kind);
        switch(m_1){
          case"number":
            const node_4=input("number", "dynamic-argu-input", testId);
            const o_3=tryHead(defaults);
            let _7=o_3==null?"":o_3.$0;
            node_4.value=_7;
            node_4.setAttribute("data-dynamic-argu-input", "true");
            wireInputEvents(node_4);
            p=[node_4, () =>[elementValue(node_4)]];
            break;
          case"enum":
            const node_5=doc().createElement("select");
            node_5.className="dynamic-argu-select";
            setTestId(testId, node_5);
            node_5.setAttribute("data-dynamic-argu-input", "true");
            iter((value_2) => {
              const option=doc().createElement("option");
              option.setAttribute("value", asText(value_2));
              option.textContent=asText(value_2);
              node_5.appendChild(option);
            }, arrayOrEmpty(_6.options));
            const x_4=tryHead(defaults);
            const v_1=elementValue(node_5);
            let _8=x_4==null?v_1:x_4.$0;
            setElementValue(node_5, _8);
            wireInputEvents(node_5);
            p=[node_5, () =>[elementValue(node_5)]];
            break;
          case"bool-value":
          case"bool":
            const node_6=input("checkbox", "dynamic-argu-input", testId);
            const o_4=tryHead(defaults);
            const value_1=o_4==null?"":o_4.$0;
            p=(node_6.checked=value_1.toLowerCase()=="true"||value_1=="1"||value_1.toLowerCase()=="yes",node_6.setAttribute("data-dynamic-argu-input", "true"),wireInputEvents(node_6),[node_6, () => ofSeq(delay(() => node_6.checked?["true"]:["false"]))]);
            break;
          default:
            const node_7=input("text", "dynamic-argu-input", testId);
            const o_5=tryHead(defaults);
            let _9=o_5==null?"":o_5.$0;
            node_7.value=_9;
            node_7.setAttribute("data-dynamic-argu-input", "true");
            wireInputEvents(node_7);
            p=[node_7, () =>[node_7.value]];
            break;
        }
        const node_8=p[0];
        node_8.setAttribute("data-dynamic-argu-tuple-item", String(_5+1));
        itemGetters.push(p[1]);
        itemRow.appendChild(node_8);
        tuple.appendChild(itemRow);
      }, arrayOrEmpty(field.items)),row.appendChild(tuple),void(getter=() => ofSeq(collect_1((getter_1) => getter_1(), itemGetters))));
      break;
    case"list":
      const listTestKey=asText(caseName)+"-"+asText(field.name);
      const list=setTestId("dynamic-argu-list-"+listTestKey, element("div", "dynamic-argu-list", null));
      const add=button("dynamic-argu-add-list-item", "dynamic-argu-list-add-"+listTestKey, "Add value");
      const addInput=(defaults) => {
        const itemRow=setTestId("dynamic-argu-list-row-"+listTestKey, element("div", "dynamic-argu-list-item-row", null));
        const node_4=input("text", "dynamic-argu-input dynamic-argu-list-item-input", "dynamic-argu-list-item-"+listTestKey);
        const o_3=tryHead(defaults);
        let _5=o_3==null?"":o_3.$0;
        node_4.value=_5;
        node_4.setAttribute("data-dynamic-argu-input", "true");
        node_4.setAttribute("data-dynamic-argu-list-item", "true");
        wireInputEvents(node_4);
        const remove=button("dynamic-argu-remove-list-item", "dynamic-argu-list-remove-"+listTestKey, "Remove");
        remove.addEventListener("click", () => {
          list.removeChild(itemRow);
          return refresh();
        });
        append(itemRow, [remove, node_4]);
        list.insertBefore(itemRow, add);
      };
      _1=(add.addEventListener("click", () => {
        addInput([]);
        return refresh();
      }),list.appendChild(add),length(fieldDefaults)===0?addInput([]):iter((value_1) => {
        addInput([value_1]);
      }, fieldDefaults),row.appendChild(list),void(getter=() => map(elementValue, queryInputs(list, "input[data-dynamic-argu-list-item='true']"))));
      break;
    case"bool-value":
    case"bool":
      const node_2=input("checkbox", "dynamic-argu-input", "dynamic-argu-bool-"+asText(field.name));
      const o_1=tryHead(fieldDefaults);
      const value=o_1==null?"":o_1.$0;
      _1=(node_2.checked=value.toLowerCase()=="true"||value=="1"||value.toLowerCase()=="yes",node_2.setAttribute("data-dynamic-argu-input", "true"),wireInputEvents(node_2),row.appendChild(node_2),void(getter=() => ofSeq(delay(() => node_2.checked?["true"]:["false"]))));
      break;
    default:
      const node_3=input("text", "dynamic-argu-input", "dynamic-argu-text-"+asText(field.name));
      const o_2=tryHead(fieldDefaults);
      let _4=o_2==null?"":o_2.$0;
      node_3.value=_4;
      node_3.setAttribute("data-dynamic-argu-input", "true");
      wireInputEvents(node_3);
      row.appendChild(node_3);
      _1=void(getter=() =>[node_3.value]);
      break;
  }
  return[row, getter];
}
function keyJsonForSubmit(keyParts){
  return JSON.stringify(keyParts);
}
function defaultsFromDocument(document){
  return document!=null&&document.$==1?document.$0==null?(document.$0,new FSharpMap("New", [])):OfArray(ofSeq(choose_1((node) => {
    const binding=asText(node.binding);
    const values=map(asText, arrayOrEmpty(node.defaultValues));
    return isBlank(binding)||length(values)===0?null:Some([binding, values]);
  }, collect_1(flattenNodeDefaults, arrayOrEmpty(document.$0.nodes))))):new FSharpMap("New", []);
}
function label(text){
  return element("label", "dynamic-argu-label", text);
}
function defaultValuesFor(defaultMap, caseName, fieldName){
  const o=defaultMap.TryFind(asText(caseName)+"."+asText(fieldName));
  return o==null?[]:o.$0;
}
function elementValue(node){
  return node.value;
}
function setElementValue(node, value){
  node.value=value;
}
function queryInputs(root, selector){
  const nodes=root.querySelectorAll(selector);
  return ofSeq(delay(() => map_1((index) => nodes[index], range(0, toInt(nodes.length)-1))));
}
function flattenNodeDefaults(node){
  return delay(() =>!(node==null)?append_2([node], delay(() => append_2(collect_1(flattenNodeDefaults, arrayOrEmpty(node.children)), delay(() => collect_1(flattenNodeDefaults, arrayOrEmpty(node.items)))))):[]);
}
function _registerRenderer(){
  globalThis.PulseTradeRegisterRenderer("fskynet-sdui", (text) => {
    try {
      globalThis.console.log(["Inside fskynet-sdui renderer wrapper! Text length:", text.length]);
      const docOpt=IsActorsPagePayload(text)?Some(createActorsPageDocument(text)):TryRender(text);
      if(docOpt==null){
        globalThis.console.log("Got None from TryRender");
        return null;
      }
      else {
        const doc_2=docOpt.$0;
        globalThis.console.log("Got Some doc! Creating container...");
        const container=globalThis.document.createElement("div");
        LoadLocalTemplates("");
        Doc.Run(container, doc_2);
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
function Main(){
  globalThis.console.log("EVALUATING SPAEntryPoint Main in ActorDynamicTab!");
  _registerRenderer();
  registerActorsPageRenderer();
  Register();
}
function IsActorsPagePayload(rawContent){
  return rawContent.indexOf("ActorTopologyPage")>=0;
}
function createActorsPageDocument(rawContent){
  let reportScheduleHandle;
  const nodes=actorNodes(rawContent);
  const projectionId=projectionText(rawContent, "projectionId", "ptcs-actors");
  const projectionVersion=projectionText(rawContent, "projectionVersion", "0");
  const groups=createNodeGroups(nodes);
  const reportOutputDirectory=_c.Create_1("");
  const reportStatus=_c.Create_1("");
  const reportScheduleRunning=_c.Create_1(false);
  reportScheduleHandle=null;
  const stopReportSchedule=() => {
    reportScheduleHandle==null?void 0:(globalThis.clearInterval(reportScheduleHandle.$0),reportScheduleHandle=null);
    reportScheduleRunning.Set(false);
  };
  const activeCount=filter((node) => {
    const status=lower(nodeStatus(node));
    return status.indexOf("active")>=0||status.indexOf("running")>=0;
  }, nodes).length;
  const offlineCount=filter((node) => statusLooksOffline(nodeStatus(node)), nodes).length;
  return Doc.Element("div", [Attr.Create("class", "ptcs-dynamic-actors-page"), OnAfterRender((node) => {
    node.setAttribute("data-testid", "dynamic-actors-page");
    logActorsTreeDsl("RENDER", rawContent);
  }), Attr.Create("style", "display:flex; flex-direction:column; gap:12px; color:#142033; min-width:0;")], ofSeq_1(delay(() => append_2([Doc.Element("div", [Attr.Create("style", "display:flex; justify-content:space-between; gap:12px; align-items:flex-start; border-bottom:1px solid #d8e1ee; padding-bottom:10px; flex-wrap:wrap;")], [Doc.Element("div", [], [Doc.Element("h2", [Attr.Create("style", "margin:0; font-size:18px; font-weight:700;")], [Doc.TextNode("Actors / Dynamic")]), Doc.Element("div", [Attr.Create("style", "color:#50627a; font-size:12px;")], [Doc.TextNode("projection "+projectionId+" / v"+projectionVersion)])]), Doc.Element("div", [Attr.Create("style", "display:grid; grid-template-columns:minmax(260px,460px) auto auto auto; gap:6px; align-items:start;")], [Doc.Element("div", [Attr.Create("style", "display:flex; flex-direction:column; gap:4px; min-width:260px;")], [V("input", [Attr.Create("type", "text"), Attr.Create("placeholder", "Server-local report output directory"), Attr.Create("style", "border:1px solid #b8c7dc; border-radius:5px; padding:5px 8px; font-size:12px; min-width:260px; width:100%; box-sizing:border-box;"), OnAfterRender((node) => {
    node.setAttribute("data-testid", "dynamic-actors-report-output-directory");
    const input_2=node;
    input_2.addEventListener("input", () => reportOutputDirectory.Set(input_2.value));
    input_2.addEventListener("change", () => reportOutputDirectory.Set(input_2.value));
  })]), Doc.Element("div", [Attr.Create("style", "min-height:16px; color:#50627a; font-size:11px; line-height:1.35; overflow-wrap:anywhere;"), OnAfterRender((node) => {
    node.setAttribute("data-testid", "dynamic-actors-report-status");
  })], [Doc.EmbedView(Map(Doc.TextNode, reportStatus.View))])]), Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("style", "border:1px solid #b8c7dc; background:#fff; color:#22344d; border-radius:5px; padding:5px 9px; font-size:12px; cursor:pointer;"), OnAfterRender((node) => {
    node.setAttribute("data-testid", "dynamic-actors-reload");
  }), Handler("click", () =>() => {
    logActorsTreeDsl("RELOAD", rawContent);
    return globalThis.location.reload();
  })], [Doc.TextNode("Reload")]), Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("style", "border:1px solid #2563eb; background:#2563eb; color:#fff; border-radius:5px; padding:5px 9px; font-size:12px; cursor:pointer;"), OnAfterRender((node) => {
    node.setAttribute("data-testid", "dynamic-actors-generate-report");
  }), Handler("click", () =>() => generateActorReport(reportOutputDirectory.Get(), reportStatus))], [Doc.TextNode("Generate report")]), Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("title", "Run the same report endpoint immediately and then every 60 seconds while this browser page is open."), Attr.Create("style", "border:1px solid #0f766e; background:#f7fffd; color:#0f4f49; border-radius:5px; padding:5px 9px; font-size:12px; cursor:pointer;"), OnAfterRender((node) => {
    node.setAttribute("data-testid", "dynamic-actors-schedule-report");
  }), Handler("click", () =>() => {
    if(reportScheduleRunning.Get()){
      stopReportSchedule();
      return reportStatus.Set("Report schedule stopped.");
    }
    else {
      const outputDirectory=reportOutputDirectory.Get();
      return isBlank_1(outputDirectory)?reportStatus.Set("Report output directory is required."):(stopReportSchedule(),reportScheduleRunning.Set(true),reportStatus.Set("Report schedule started; generating first report..."),generateActorReport(outputDirectory, reportStatus),void(reportScheduleHandle=Some(globalThis.setInterval(() => generateActorReport(reportOutputDirectory.Get(), reportStatus), 60000))));
    }
  })], [Doc.EmbedView(Map((running) => Doc.TextNode(running?"Stop schedule":"Schedule"), reportScheduleRunning.View))])])])], delay(() => append_2([Doc.Element("div", [Attr.Create("style", "display:grid; grid-template-columns:repeat(auto-fit,minmax(180px,1fr)); gap:10px;")], [renderCountCard("Renderer", "ActorsPage"), renderCountCard("Node groups", String(length(groups))), renderCountCard("Actor tree rows", String(length(nodes))), renderCountCard("Active", String(activeCount)), renderCountCard("Offline", String(offlineCount))])], delay(() => length(nodes)===0?[Doc.Element("div", [Attr.Create("style", "border:1px solid #c9d7e8; border-radius:6px; background:#fff; padding:12px; color:#4b5e76; font-size:12px;")], [Doc.TextNode("No actor topology rows are available in this projection.")])]:[Doc.Element("div", [Attr.Create("style", "display:flex; flex-direction:column; gap:12px;"), OnAfterRender((node) => {
    node.setAttribute("data-testid", "dynamic-actor-node-blocks");
  })], [Doc.Concat(ofArray(map((_1) => renderNodeBlock(_1[1], _1[2], _1[3]), groups)))])])))))));
}
function registerActorsPageRenderer(){
  const renderer=(rawContent) => {
    try {
      if(IsActorsPagePayload(rawContent)){
        const container=globalThis.document.createElement("div");
        const doc_2=createActorsPageDocument(rawContent);
        LoadLocalTemplates("");
        Doc.Run(container, doc_2);
        return Some(container);
      }
      else return null;
    }
    catch(e){
      globalThis.console.error(["ActorsPage renderer failed:", e]);
      return null;
    }
  };
  function ensure(attempts){
    "PulseTradeRegisterPageRenderer"in globalThis&&!(!(!(globalThis.PulseTrade&&globalThis.PulseTrade.PageRenderers&&globalThis.PulseTrade.PageRenderers.some((r) => r&&r.name==="ptcs-actors-page"))))?(globalThis.PulseTradeRegisterPageRenderer("ptcs-actors-page", 100, renderer),globalThis.console.log("PulseTrade.Comm.Spa.Dynamic registered ActorsPage renderer.")):void 0;
    attempts>0?setTimeout(() => {
      ensure(attempts-1);
    }, 200):void 0;
  }
  ensure(20);
}
function actorNodes(rawContent){
  try {
    const data=globalThis.JSON.parse(rawContent).data;
    if(Equals(data, null)||Equals(typeof data, "undefined"))return[];
    else {
      const nodes=data.actorTreeNodes;
      return Equals(nodes, null)||Equals(typeof nodes, "undefined")?[]:nodes;
    }
  }
  catch(m){
    return[];
  }
}
function createNodeGroups(nodes){
  let _1;
  const groupKey=(node) => actorSystemAddress(nodeRawAddress(node));
  const knownGroups=map((_3) => {
    const key_1=_3[0];
    const augmentedNodes_1=normalizeGroupTreeNodes(key_1, _3[1]);
    const p_1=classifyNodeBlock(key_1, augmentedNodes_1);
    return[p_1[0], key_1, p_1[1], augmentedNodes_1];
  }, groupBy(groupKey, filter((node) => groupKey(node)!="unknown", nodes)));
  const unknownSeeds=filter((node) => {
    const id=nodeId(node);
    return!exists((_3) => exists((known) => nodeId(known)==id, _3[3]), knownGroups)&&!(nodeKind(node)=="virtual-path"||isBlank_1(nodeRawAddress(node))&&!isBlank_1(actorTreePath(nodeFullPath(node))))&&groupKey(node)=="unknown";
  }, nodes);
  if(length(unknownSeeds)===0)_1=[];
  else {
    const key="unknown";
    const augmentedNodes=withAncestors(nodes, unknownSeeds);
    const p=classifyNodeBlock(key, augmentedNodes);
    _1=[[p[0], key, p[1], augmentedNodes]];
  }
  let _2=knownGroups.concat(_1);
  return sortBy((_3) => String(groupOfflineRank(_3[3]))+":"+String(_3[0])+":"+_3[1], _2);
}
function logActorsTreeDsl(phase, rawContent){
  try {
    const nodes=actorNodes(rawContent);
    const projectionId=projectionText(rawContent, "projectionId", "ptcs-actors");
    const projectionVersion=projectionText(rawContent, "projectionVersion", "0");
    const payload=globalThis.JSON.parse(rawContent);
    const title="[PTCS.Dynamic ActorTree DSL] "+phase+" projection="+projectionId+" version="+projectionVersion+" nodes="+String(length(nodes));
    globalThis.console.groupCollapsed(title);
    globalThis.console.log(["phase", phase]);
    globalThis.console.log(["raw", rawContent]);
    globalThis.console.log(["dsl", payload]);
    globalThis.console.log(["nodes", nodes]);
    return globalThis.console.groupEnd();
  }
  catch(e){
    globalThis.console.log(["[PTCS.Dynamic ActorTree DSL] "+phase+" raw", rawContent]);
    return globalThis.console.error(["[PTCS.Dynamic ActorTree DSL] log failed", e]);
  }
}
function V(name, attrs){
  return Doc.Element(name, attrs, FSharpList.Empty);
}
function generateActorReport(outputDirectory, status){
  const trimmed=Trim(asText_1(outputDirectory));
  if(isBlank_1(trimmed))status.Set("Report output directory is required.");
  else {
    status.Set("Generating actor state report...");
    postJson_1("/actors/api/report", New_32(trimmed), (reply) => {
      status.Set("Report written: "+(isBlank_1(reply.filePath)?reply.fileName:reply.filePath));
    }, (message) => {
      status.Set("Report failed: "+asText_1(message));
    });
  }
}
function renderCountCard(title, value){
  return Doc.Element("div", [Attr.Create("style", "border:1px solid #d9e3f0; border-radius:6px; background:#fff; padding:10px 12px;")], [Doc.Element("div", [Attr.Create("style", "font-size:11px; color:#667891;")], [Doc.TextNode(title)]), Doc.Element("div", [Attr.Create("style", "margin-top:4px; font-size:18px; font-weight:700;")], [Doc.TextNode(value)])]);
}
function renderNodeBlock(key, roleLabel, groupNodes){
  const statuses=concat_2(", ", distinctValues(map(displayStatus, distinctValues(map(nodeStatus, groupNodes)))));
  return Doc.Element("section", [Attr.Create("style", "display:flex; flex-direction:column; gap:10px; border:1px solid #cfdcec; background:#fff; border-radius:7px; padding:12px;"), OnAfterRender((node) => {
    node.setAttribute("data-testid", "dynamic-actor-node-block");
    node.setAttribute("data-offline-rank", String(groupOfflineRank(groupNodes)));
  })], [Doc.Element("div", [Attr.Create("style", "display:flex; align-items:flex-start; justify-content:space-between; gap:12px; flex-wrap:wrap;")], [Doc.Element("div", [Attr.Create("style", "min-width:0;")], [Doc.Element("div", [Attr.Create("style", "font-size:11px; color:#667891;")], [Doc.TextNode(roleLabel)]), Doc.Element("h3", [Attr.Create("style", "margin:2px 0 0 0; font-size:15px; font-weight:700; color:#16263c; font-family:Consolas, 'Cascadia Mono', monospace; white-space:nowrap; overflow-x:auto;")], [Doc.TextNode(key)])]), Doc.Element("div", [Attr.Create("style", "font-size:12px; color:#53677f; white-space:nowrap;")], ofSeq_1(delay(() =>[Doc.TextNode(String(filter((node) =>!isBlank_1(nodeRawAddress(node)), groupNodes).length)+" actor node(s)")])))]), Doc.Element("div", [Attr.Create("style", "display:flex; gap:8px; align-items:center; flex-wrap:wrap; font-size:12px; color:#53677f;")], [Doc.Element("span", [Attr.Create("style", "font-weight:650;")], [Doc.TextNode("Status")]), Doc.Element("span", [], [Doc.TextNode(isBlank_1(statuses)?"unknown":statuses)])]), renderTree(groupNodes), renderGrid(groupNodes)]);
}
function statusLooksOffline(status){
  return hasToken("offline", status)||hasToken("unreachable", status)||hasToken("stale", status)||hasToken("terminated", status)||hasToken("stopped", status)||hasToken("dead", status)||hasToken("failed", status);
}
function nodeStatus(node){
  try {
    return node.status||"";
  }
  catch(m){
    return"";
  }
}
function lower(value){
  return value==null?"":value.toLowerCase();
}
function isBlank_1(value){
  return value==null||Trim(value)=="";
}
function projectionText(rawContent, fieldName, fallback){
  try {
    const payload=globalThis.JSON.parse(rawContent);
    const value=fieldName=="projectionId"?payload.projectionId:fieldName=="projectionVersion"?payload.projectionVersion:null;
    return Equals(value, null)||Equals(typeof value, "undefined")?fallback:String(value);
  }
  catch(m){
    return fallback;
  }
}
function actorSystemAddress(address){
  if(isBlank_1(address))return"unknown";
  else {
    const userIndex=address.indexOf("/user");
    const systemIndex=address.indexOf("/system");
    const cutIndex=userIndex>0&&systemIndex>0?userIndex<systemIndex?userIndex:systemIndex:userIndex>0?userIndex:systemIndex>0?systemIndex:-1;
    return address.indexOf("://")>=0&&cutIndex>0?Substring(address, 0, cutIndex):address.indexOf("://")>=0?address:"unknown";
  }
}
function nodeRawAddress(node){
  try {
    return node.address||"";
  }
  catch(m){
    return"";
  }
}
function normalizeGroupTreeNodes(groupKey, groupNodes){
  let result;
  result=FSharpList.Empty;
  const add=(node_1) => {
    const id=nodeId(node_1);
    if(!isBlank_1(id)&&!exists_2((node_2) => nodeId(node_2)==id, result))result=append_3(result, ofArray([node_1]));
  };
  for(let i=0, _4=groupNodes.length-1;i<=_4;i++){
    const node=get(groupNodes, i);
    const rawAddress=nodeRawAddress(node);
    const sourcePath=isBlank_1(rawAddress)?nodeFullPath(node):rawAddress;
    const treePath=actorTreePath(sourcePath);
    const segments=pathSegments(treePath);
    if(length(segments)<=1)add(node);
    else {
      for(let index=1, _5=length(segments)-1;index<=_5;index++){
        const path=joinPath(segments, index);
        add(makeActorTreeNode(groupKey+path, index===1?"":groupKey+joinPath(segments, index-1), index===1?path:get(segments, index-1), groupKey+path, "", "virtual-path", "active"));
      }
      const parentId=groupKey+joinPath(segments, length(segments)-1);
      const existingId=nodeId(node);
      let _1=isBlank_1(existingId)?groupKey+treePath:existingId;
      const existingLabel=nodeLabel(node);
      let _2=isBlank_1(existingLabel)?nodeAddress(node):existingLabel;
      let _3=makeActorTreeNode(_1, parentId, _2, nodeFullPath(node), nodeRawAddress(node), nodeKind(node), nodeStatus(node));
      add(_3);
    }
  }
  return ofList(result);
}
function classifyNodeBlock(key, nodes){
  const sample=key+" "+concat_2(" ", map((node) => nodeLabel(node)+" "+nodeKind(node)+" "+nodeAddress(node), nodes));
  return key=="unknown"||isBlank_1(key)?[3, "Unknown"]:hasToken("ptcshost", key)||hasToken("ptcs-host", key)||hasToken("ptcs", key)||hasToken("commspa", key)?[0, "PTCS Host"]:hasToken("gwhost", key)||hasToken("gw-host", key)||hasToken("gateway", key)?[1, "GW Host"]:hasToken("rnhost", key)||hasToken("rn-host", key)||hasToken("resourcenode", key)||hasToken("resource-node", key)?[2, "RN Host"]:hasToken("ptcs", sample)||hasToken("spa", sample)||hasToken("commspa", sample)?[0, "PTCS Host"]:hasToken("gw", sample)||hasToken("gateway", sample)?[1, "GW Host"]:hasToken("rn", sample)||hasToken("resource", sample)?[2, "RN Host"]:[3, "Unknown"];
}
function groupOfflineRank(nodes){
  let hasConcrete, hasOnline;
  hasConcrete=false;
  hasOnline=false;
  for(let i=0, _1=nodes.length-1;i<=_1;i++){
    const node=get(nodes, i);
    if(!isBlank_1(nodeRawAddress(node))&&nodeKind(node)!="virtual-path"){
      hasConcrete=true;
      !statusLooksOffline(nodeStatus(node))?hasOnline=true:void 0;
    }
  }
  return hasConcrete&&!hasOnline?1:0;
}
function withAncestors(allNodes, seedNodes){
  let result;
  result=FSharpList.Empty;
  const add=(node_1) => {
    const id=nodeId(node_1);
    if(!isBlank_1(id)&&!exists_2((item) => nodeId(item)==id, result))result=append_3(result, ofArray([node_1]));
  };
  function addAncestors(node_1){
    const parentId=nodeParentId(node_1);
    if(!isBlank_1(parentId)){
      const m=tryFind((node_2) => nodeId(node_2)==parentId, allNodes);
      if(m==null){ }
      else {
        const parent=m.$0;
        addAncestors(parent);
        add(parent);
      }
    }
  }
  for(let i=0, _1=seedNodes.length-1;i<=_1;i++){
    const node=get(seedNodes, i);
    addAncestors(node);
    add(node);
  }
  return ofList(result);
}
function nodeKind(node){
  try {
    return node.kind||"";
  }
  catch(m){
    return"";
  }
}
function actorTreePath(address){
  const value=asText_1(address);
  const userIndex=value.indexOf("/user");
  const systemIndex=value.indexOf("/system");
  const cutIndex=userIndex>=0&&systemIndex>=0?userIndex<systemIndex?userIndex:systemIndex:userIndex>=0?userIndex:systemIndex>=0?systemIndex:-1;
  return cutIndex<0?"":value.substring(cutIndex);
}
function nodeFullPath(node){
  try {
    return node.fullPath||"";
  }
  catch(m){
    return"";
  }
}
function nodeId(node){
  try {
    return node.id||"";
  }
  catch(m){
    return"";
  }
}
function asText_1(value){
  return value==null||Equals(typeof value, "undefined")?"":value;
}
function postJson_1(url, body, onOk, onError){
  const headers=new Headers();
  headers.set("Content-Type", "application/json");
  (globalThis.fetch(url, {
    method:"POST", 
    headers:headers, 
    body:JSON.stringify(body)
  }).then((response) => response.text().then((responseBody) => response.ok?onOk(decodeJson_1(isBlank_1(responseBody)?"{}":responseBody)):onError(isBlank_1(responseBody)?"POST "+String(url)+" "+String(response.status):responseBody))))["catch"]((error) => onError(errorMessage_1(error)));
}
function distinctValues(values){
  let known;
  known=FSharpList.Empty;
  for(let i=0, _1=values.length-1;i<=_1;i++)((() => {
    const value=get(values, i);
    const normalized=value==null?"":Trim(value);
    return normalized!=""&&!exists_2((current) => current==normalized, known)?void(known=append_3(known, ofArray([normalized]))):null;
  })());
  return ofList(known);
}
function displayStatus(status){
  return statusLooksOffline(status)?"OFFLINE":isBlank_1(status)?"unknown":status;
}
function renderTree(groupNodes){
  const collapsedIds=_c.Create_1([]);
  const containsId=(id, ids) => exists((current) => current==id, ids);
  const roots=sortBy(nodeLabel, filter((node) => {
    const parentId=nodeParentId(node);
    return isBlank_1(parentId)||!exists((node_1) => nodeId(node_1)==parentId, groupNodes);
  }, groupNodes));
  function renderNode_1(collapsed){
    return(depth) =>(node) => {
      let _1;
      const id=nodeId(node);
      const children=sortBy(nodeLabel, filter((node_1) => nodeParentId(node_1)==id, groupNodes));
      const a=12;
      const a_1=0;
      const b=Compare(a_1, depth)===1?a_1:depth;
      const depthValue=Compare(a, b)===-1?a:b;
      const address=nodeAddress(node);
      const fullPath=nodeFullPath(node);
      const kind=nodeKind(node);
      const parentId=nodeParentId(node);
      const displayAddress=kind=="virtual-path"?nodeLabel(node):isBlank_1(address)?fullPath:address;
      const isCollapsed=containsId(id, collapsed);
      let _2=Doc.Element("div", [Attr.Create("class", "dynamic-actor-tree-row"), Attr.Create("style", "position:relative; display:grid; grid-template-columns:20px 10px max-content max-content max-content; gap:8px; align-items:center; width:max-content; min-width:100%; padding:4px 10px 4px 6px; border-radius:5px; font-size:12px; line-height:1.35; margin-left:"+String(depthValue*18)+"px;"), OnAfterRender((node_1) => {
        node_1.setAttribute("data-testid", "dynamic-actor-tree-row");
        node_1.setAttribute("data-node-id", id);
        node_1.setAttribute("data-parent-id", parentId);
        node_1.setAttribute("data-depth", String(depthValue));
        node_1.setAttribute("data-node-kind", kind);
        node_1.setAttribute("data-display-address", displayAddress);
      })], ofSeq_1(delay(() => append_2(depthValue>0?append_2([Doc.Element("span", [Attr.Create("class", "dynamic-actor-tree-connector-h"), Attr.Create("style", "position:absolute; left:-12px; top:50%; width:12px; border-top:1px solid #aeb8c8;"), OnAfterRender((node_1) => {
        node_1.setAttribute("data-testid", "dynamic-actor-tree-connector");
      })], [])], delay(() =>[Doc.Element("span", [Attr.Create("class", "dynamic-actor-tree-connector-v"), Attr.Create("style", "position:absolute; left:-12px; top:-5px; height:calc(100% + 5px); border-left:1px solid #aeb8c8;")], [])])):[], delay(() => append_2(length(children)>0?[Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("title", isCollapsed?"Expand actor node":"Collapse actor node"), Attr.Create("style", "display:inline-flex; align-items:center; justify-content:center; width:18px; height:18px; border:1px solid #7d92ad; background:#fff; color:#21354f; font-size:12px; line-height:16px; padding:0; margin-top:3px; cursor:pointer; font-family:Consolas, 'Cascadia Mono', monospace;"), Handler("click", () =>() => {
        const current=collapsedIds.Get();
        return collapsedIds.Set(containsId(id, current)?filter((value) => value!=id, current):current.concat([id]));
      }), OnAfterRender((node_1) => {
        node_1.setAttribute("data-testid", "dynamic-actor-tree-toggle");
        node_1.setAttribute("aria-expanded", isCollapsed?"false":"true");
      })], [Doc.TextNode(isCollapsed?"+":"-")])]:[Doc.Element("span", [Attr.Create("style", "display:inline-flex; width:18px; height:18px; margin-top:3px;"), OnAfterRender((node_1) => {
        node_1.setAttribute("data-testid", "dynamic-actor-tree-toggle-placeholder");
      })], [])], delay(() => append_2([renderStatusDot(nodeStatus(node))], delay(() => append_2([Doc.Element("span", [Attr.Create("class", "dynamic-actor-tree-label"), Attr.Create("title", isBlank_1(fullPath)?displayAddress:fullPath), Attr.Create("style", "white-space:nowrap; color:#172033; font-weight:600; overflow:visible; text-overflow:clip; font-family:Consolas, 'Cascadia Mono', monospace;")], [Doc.TextNode(displayAddress)])], delay(() => append_2([renderSmallPill(kind)], delay(() =>[renderStatusChip(nodeStatus(node))])))))))))))));
      if(isCollapsed||depth>=24)_1=FSharpList.Empty;
      else {
        const x=ofArray(children);
        _1=collect_2((renderNode_1(collapsed))(depth+1), x);
      }
      return FSharpList.Cons(_2, _1);
    };
  }
  return Doc.Element("div", [Attr.Create("style", "border:1px solid #d8e2ef; background:#f8fafc; border-radius:6px; padding:8px 10px; overflow-x:scroll; overflow-y:auto; scrollbar-gutter:stable; max-height:430px;"), OnAfterRender((node) => {
    node.setAttribute("data-testid", "dynamic-actor-tree-viewport");
  })], [Doc.EmbedView(Map((collapsed) => {
    const x=ofArray(roots);
    const rows=collect_2((renderNode_1(collapsed))(0), x);
    return rows.$==0?Doc.Element("div", [Attr.Create("style", "color:#667891; font-size:12px;")], [Doc.TextNode("No actor tree rows.")]):Doc.Concat(rows);
  }, collapsedIds.View))]);
}
function renderGrid(groupNodes){
  const headerCell=(label_1) => E("th", [Attr.Create("style", "text-align:left; padding:8px 10px; border-bottom:1px solid #d7e2ef; color:#53677f; font-size:11px; white-space:nowrap;")], [Doc.TextNode(label_1)]);
  return Doc.Element("div", [Attr.Create("style", "overflow-x:auto; border:1px solid #d8e2ef; border-radius:6px; background:#fff;"), OnAfterRender((node) => {
    node.setAttribute("data-testid", "dynamic-actor-grid");
  })], [E("table", [Attr.Create("style", "border-collapse:collapse; min-width:980px; width:100%;")], [E("thead", [], [E("tr", [], [headerCell("Kind"), headerCell("Status"), headerCell("Address"), headerCell("Full path")])]), E("tbody", [], ofArray(map((node) => E("tr", [], [E("td", [Attr.Create("style", "padding:8px 10px; border-bottom:1px solid #edf2f8; white-space:nowrap;")], [Doc.TextNode(nodeKind(node))]), E("td", [Attr.Create("style", "padding:8px 10px; border-bottom:1px solid #edf2f8; white-space:nowrap;")], [renderStatusChip(nodeStatus(node))]), E("td", [Attr.Create("style", "padding:8px 10px; border-bottom:1px solid #edf2f8; font-family:Consolas, 'Cascadia Mono', monospace; font-size:12px; white-space:nowrap;")], [Doc.TextNode(nodeAddress(node))]), E("td", [Attr.Create("style", "padding:8px 10px; border-bottom:1px solid #edf2f8; font-family:Consolas, 'Cascadia Mono', monospace; font-size:12px; white-space:nowrap;")], [Doc.TextNode(nodeFullPath(node))])]), groupNodes)))])]);
}
function hasToken(token, value){
  return lower(value).indexOf(token)>=0;
}
function pathSegments(path){
  return isBlank_1(path)?[]:SplitChars(path, ["/"], 1);
}
function joinPath(segments, count){
  return count<=0?"":"/"+Join_1("/", take(count, segments));
}
function makeActorTreeNode(id, parentId, label_1, fullPath, address, kind, status){
  return{
    id:id, 
    parentId:parentId, 
    label:label_1, 
    fullPath:fullPath, 
    address:address, 
    kind:kind, 
    status:status
  };
}
function nodeLabel(node){
  let label_1;
  try {
    label_1=node.label||"";
  }
  catch(m){
    label_1="";
  }
  return isBlank_1(label_1)?nodeId(node):label_1;
}
function nodeAddress(node){
  const address=nodeRawAddress(node);
  return isBlank_1(address)?nodeFullPath(node):address;
}
function nodeParentId(node){
  try {
    return node.parentId||"";
  }
  catch(m){
    return"";
  }
}
function errorMessage_1(error){
  return error==null?"unknown error":String(error);
}
function decodeJson_1(text){
  return JSON.parse(asText_1(text));
}
function renderStatusDot(status){
  const normalized=lower(status);
  const p=normalized.indexOf("active")>=0||normalized.indexOf("running")>=0?["#16a34a", "#dcfce7"]:normalized.indexOf("passivated")>=0?["#d97706", "#fef3c7"]:normalized.indexOf("stale")>=0||normalized.indexOf("changed")>=0?["#d97706", "#fef3c7"]:statusLooksOffline(status)||normalized.indexOf("terminated")>=0||normalized.indexOf("dead")>=0?["#dc2626", "#fee2e2"]:["#64748b", "#e2e8f0"];
  return Doc.Element("span", [Attr.Create("style", "width:8px; height:8px; border-radius:999px; background:"+p[0]+"; box-shadow:0 0 0 2px "+p[1]+"; display:inline-block;"), OnAfterRender((node) => {
    node.setAttribute("data-testid", "dynamic-actor-tree-status-dot");
    node.setAttribute("data-status", isBlank_1(status)?"unknown":status);
  })], []);
}
function renderSmallPill(value){
  return Doc.Element("span", [Attr.Create("style", "white-space:nowrap; border:1px solid #d4ddec; border-radius:999px; background:#fff; color:#44546d; padding:1px 7px; font-size:11px; line-height:1.45;")], [Doc.TextNode(isBlank_1(value)?"unknown":value)]);
}
function renderStatusChip(status){
  const normalized=lower(status);
  const color=normalized.indexOf("active")>=0||normalized.indexOf("running")>=0?"#0b6b3a":normalized.indexOf("stale")>=0||normalized.indexOf("changed")>=0?"#8a5a00":statusLooksOffline(status)||normalized.indexOf("terminated")>=0||normalized.indexOf("dead")>=0?"#8b1e2d":"#46566b";
  return Doc.Element("span", [Attr.Create("style", "display:inline-block; border:1px solid "+color+"; color:"+color+"; border-radius:999px; padding:2px 7px; font-size:11px; line-height:16px; white-space:nowrap;")], [Doc.TextNode(displayStatus(status))]);
}
function E(name, attrs, children){
  return Doc.Element(name, attrs, children);
}
function Main_1(){
  let mountedPageElement, mountedAppendPageResolved, mounted, appendRegistryWsState, appendRegistryPageCount, appendRegistryMaxSequence, appendRegistrySocket, queuedAppendRegistryFrames, appendRegistrySubscribed, appendRegistryTailRequested;
  if(!(doc_1().body==null))doc_1().body.setAttribute("data-server-reality-id", currentServerRealityId());
  const trimmed=TrimEnd(asText_2(globalThis.location.pathname), ["/"]);
  const path=isBlank_2(trimmed)?"/chat":trimmed;
  mountedPageElement=null;
  mountedAppendPageResolved=false;
  const cacheKey_1=appendPagesDefinitionsCacheKey();
  mounted=false;
  appendRegistryWsState="idle";
  appendRegistryPageCount=0;
  appendRegistryMaxSequence=0n;
  const mountOnce=(pages) => {
    if(!mounted){
      let _1;
      mounted=true;
      const pages_1=arrayOrEmpty_1(pages);
      const p=shell(path, pages_1);
      const page=p[1];
      mountedPageElement=Some(page);
      mountedAppendPageResolved=false;
      setMain(p[0]);
      if(path=="/sets")_1=mountSets(page);
      else if(path=="/actors")_1=mountActors(page);
      else if(path=="/chat")_1=mountChat(page);
      else {
        const m=findAppendPage(path, pages_1);
        if(m==null)_1=mountUnknownPage(page, path);
        else {
          const definition=m.$0;
          _1=(mountedAppendPageResolved=true,mountAppendPage(page, definition));
        }
      }
      globalThis.setInterval(() => refreshAppendNav(path), 5000);
    }
  };
  const renderAppendRegistryHealth=() => {
    if(!(doc_1().body==null))doc_1().body.setAttribute("data-append-registry-ws-state", appendRegistryWsState);
    const node=doc_1().querySelector("[data-testid='append-registry-health']");
    if(!(node==null)){
      const x=setData("ws-state", appendRegistryWsState, node);
      const x_1=setData("page-count", String(appendRegistryPageCount), x);
      let _1=setData("max-sequence", String(appendRegistryMaxSequence), x_1);
      setData("cache-key", cacheKey_1, _1);
      node.setAttribute("title", "cacheKey="+String(cacheKey_1)+"\nwsState="+String(appendRegistryWsState)+"\npageCount="+String(appendRegistryPageCount)+"\nmaxSequence="+String(appendRegistryMaxSequence));
      node.textContent="append registry ws "+String(appendRegistryWsState)+" | pages "+String(appendRegistryPageCount)+" | seq "+String(appendRegistryMaxSequence);
    }
    else void 0;
  };
  const applyDefinitionsFromReply=(data) => {
    let _1;
    const data_1=data==null?emptyAppendPagesReply():data;
    appendRegistryPageCount=arrayOrEmpty_1(data_1.pages).length;
    const b=data_1.maxSequence;
    appendRegistryMaxSequence=Compare(appendRegistryMaxSequence, b)===1?appendRegistryMaxSequence:b;
    if(mounted){
      const nav=doc_1().getElementById("ptc-nav");
      if(!(nav==null))renderNav(nav, path, arrayOrEmpty_1(data_1.pages));
      if(path!="/sets"&&path!="/actors"&&path!="/chat"&&!mountedAppendPageResolved){
        const _2=findAppendPage(path, arrayOrEmpty_1(data_1.pages));
        if(mountedPageElement!=null&&mountedPageElement.$==1){
          if(_2!=null&&_2.$==1){
            const definition=_2.$0;
            const page=mountedPageElement.$0;
            _1=(clear(page),mountedAppendPageResolved=true,mountAppendPage(page, definition));
          }
          else _1=void 0;
        }
        else _1=void 0;
      }
      else _1=void 0;
    }
    else _1=mountOnce(data_1.pages);
    renderAppendRegistryHealth();
  };
  const setAppendRegistryWsState=(value) => {
    appendRegistryWsState=asText_2(value);
    renderAppendRegistryHealth();
  };
  appendRegistrySocket=null;
  queuedAppendRegistryFrames=[];
  appendRegistrySubscribed=false;
  appendRegistryTailRequested=false;
  const handleAppendRegistryEvents=(events) => {
    if(length(arrayOrEmpty_1(events))>0)readJson(cacheKey_1, (cached) => {
      const merged=mergeAppendPageRegistryEvents(cached==null?emptyAppendPagesReply():cached.$0, events);
      writeAppendPagesDefinitions(merged);
      applyDefinitionsFromReply(merged);
    });
  };
  function flushAppendRegistryFrames(socket){
    if(Equals(socket.readyState, 1)){
      const frames=queuedAppendRegistryFrames;
      queuedAppendRegistryFrames=[];
      iter((frame) => {
        socket.send(frame);
      }, frames);
    }
  }
  function ensureAppendRegistrySocket(){
    let _1, _2;
    if(appendRegistrySocket!=null&&appendRegistrySocket.$==1){
      const socket=appendRegistrySocket.$0;
      _1=(Equals(socket.readyState, 1)||Equals(socket.readyState, 0))&&(_2=appendRegistrySocket.$0,true);
    }
    else _1=false;
    if(_1)return _2;
    else {
      setAppendRegistryWsState("connecting");
      const socket_1=new WebSocket(syncWebSocketUrl());
      appendRegistrySocket=Some(socket_1);
      socket_1.onopen=() => {
        setAppendRegistryWsState("open");
        return flushAppendRegistryFrames(socket_1);
      };
      socket_1.onmessage=(event) => {
        try {
          const response=json(String(event.data));
          const responseType=asText_2(response.type).toLowerCase();
          const responseStatus=asText_2(response.status).toLowerCase();
          switch(responseStatus=="ok"?responseType=="subscribe"?0:responseType=="stream-event"?1:responseType=="read-tail"?2:responseType=="read"?2:responseType=="tail"?2:4:responseStatus=="error"?3:4){
            case 0:
              return setAppendRegistryWsState("subscribed");
            case 1:
              return handleAppendRegistryEvents([response.event]);
            case 2:
              return handleAppendRegistryEvents(response.events);
            case 3:
              return setAppendRegistryWsState("error");
            case 4:
              return null;
          }
        }
        catch(m){
          return setAppendRegistryWsState("parse-error");
        }
      };
      socket_1.onerror=() => setAppendRegistryWsState("error");
      socket_1.onclose=() => {
        appendRegistrySocket=null;
        appendRegistrySubscribed=false;
        appendRegistryTailRequested=false;
        return setAppendRegistryWsState("closed");
      };
      return socket_1;
    }
  }
  function sendAppendRegistryFrame(frame){
    while(true)
      {
        const socket=ensureAppendRegistrySocket();
        return Equals(socket.readyState, 1)?socket.send(frame):void(queuedAppendRegistryFrames=queuedAppendRegistryFrames.concat([frame]));
      }
  }
  function subscribeAppendPageRegistry(){
    const streamKey=appendPageRegistryStreamKey();
    if(!appendRegistrySubscribed){
      appendRegistrySubscribed=true;
      sendAppendRegistryFrame(JSON.stringify(New_1("subscribe", newRequestId("append-pages-subscribe"), streamKey)));
    }
    if(!appendRegistryTailRequested){
      appendRegistryTailRequested=true;
      sendAppendRegistryFrame(JSON.stringify(New_2("read-tail", newRequestId("append-pages-read-tail"), streamKey, defaultCacheLimit())));
    }
  }
  readJson(cacheKey_1, (a) => {
    if(a==null){ }
    else applyDefinitionsFromReply(a.$0);
  });
  getJson("/pages/api/definitions", (data) => {
    writeAppendPagesDefinitions(data);
    applyDefinitionsFromReply(data);
  }, () => {
    mountOnce([]);
    renderAppendRegistryHealth();
  });
  subscribeAppendPageRegistry();
  renderAppendRegistryHealth();
}
function doc_1(){
  return _c_1.doc;
}
function currentServerRealityId(){
  const node=doc_1().getElementById("ptc-comm-reality");
  if(node==null||isBlank_2(node.textContent))return"legacy";
  else try {
    return textOr("legacy", json(node.textContent).serverRealityId);
  }
  catch(m){
    return"legacy";
  }
}
function isBlank_2(value){
  return value==null||Trim(value)=="";
}
function asText_2(value){
  return value==null||Equals(typeof value, "undefined")?"":value;
}
function appendPagesDefinitionsCacheKey(){
  return cacheKey("append-pages-definitions", FSharpList.Empty);
}
function setData(name, value, node){
  !isBlank_2(name)?node.setAttribute("data-"+name, asText_2(value)):void 0;
  return node;
}
function emptyAppendPagesReply(){
  return New("ok", 0, 0n, []);
}
function arrayOrEmpty_1(values){
  return values==null?[]:values;
}
function mergeAppendPageRegistryEvents(baseline, events){
  let pages, maxSequence;
  const baseline_1=baseline==null?emptyAppendPagesReply():baseline;
  pages=arrayOrEmpty_1(baseline_1.pages);
  maxSequence=baseline_1.maxSequence;
  iter((event) => {
    if(!(event==null)&&event.sequence>0n){
      const m=asText_2(event.sourceKind).toLowerCase();
      if(m=="append-page.definition"){
        const b=event.sequence;
        maxSequence=Compare(maxSequence, b)===1?maxSequence:b;
        try {
          const o=pageDefinitionFromWire(json(event.payload));
          if(o==null)null;
          else {
            const page=o.$0;
            pages=sortAppendPages(filter((existing) =>!sameTextInvariant(existing.pageId, page.pageId), pages).concat([page]));
          }
        }
        catch(m_1){
          null;
        }
      }
      else if(m=="append-page.hidden"){
        const b_1=event.sequence;
        maxSequence=Compare(maxSequence, b_1)===1?maxSequence:b_1;
        try {
          const o_1=hiddenPageFromWire(json(event.payload));
          if(o_1==null)null;
          else {
            const _1=o_1.$0[0];
            const _2=o_1.$0[1];
            pages=sortAppendPages(filter((page_1) =>!(sameTextInvariant(page_1.pageId, _1)||sameTextInvariant(page_1.tabId, _2)||sameTextInvariant(page_1.pageId, _2)||sameTextInvariant(page_1.tabId, _1)), pages));
          }
        }
        catch(m_2){
          null;
        }
      }
      else void 0;
    }
  }, arrayOrEmpty_1(events));
  return New("ok", length(pages), maxSequence, pages);
}
function writeAppendPagesDefinitions(data){
  writeSnapshotWithWatermark(appendPagesDefinitionsCacheKey(), data, data.maxSequence, length(arrayOrEmpty_1(data.pages)), "append-pages-definitions");
}
function syncWebSocketUrl(){
  const location=globalThis.location;
  return(location.protocol=="https:"?"wss:":"ws:")+"//"+location.host+"/sync/ws";
}
function appendPageRegistryStreamKey(){
  return New_6("__append-page-registry", "append-page-registry", "__append-pages", ["__append-pages"]);
}
function newRequestId(prefix){
  set_requestSeq(requestSeq()+1);
  return prefix+"-"+String(requestSeq())+"-"+String(Math.floor(Math.random()*1000000000));
}
function defaultCacheLimit(){
  return _c_1.defaultCacheLimit;
}
function getJson(url, onOk, onError){
  (globalThis.fetch(url, {cache:"no-store"}).then((response) => response.text().then((body) => response.ok?onOk(json(isBlank_2(body)?"{}":body)):onError(isBlank_2(body)?"GET "+String(url)+" "+String(response.status):body))))["catch"]((error) => onError(errorMessage_2(error)));
}
function findAppendPage(path, pages){
  return tryFind((page) => isCurrentPage(path, pagePath(page))||isCurrentPage(path, "/page/"+asText_2(page.pageId))||isCurrentPage(path, "/"+asText_2(page.pageId)), arrayOrEmpty_1(pages));
}
function clear(node){
  node.textContent="";
}
function mountAppendPage(page, definition){
  let currentLineageHealth, selected, selectedKeyJson, buckets, locallyHiddenKeyIds, pendingSelectKeyId, loadGeneration, visibleValueLimit, scrollValuesToBottomAfterNextRender, addKeyEditorOpen, addKeyMode, ensureSelectedSubscription, replayPendingCommands, deleteAcceptedPendingAppends, rerenderAppendForm, rerenderAddKeyBuilder, currentKeyMaxSequence, keyRegistryWsState, syncSocket, queuedSyncFrames, subscribedValueStream, keyRegistrySubscribed, keyRegistryTailRequested, pendingWsAppendIds, syncRepairScheduled, repairSyncAfterClose, replayingPending;
  page.className="page append-page";
  setData("tab-id", definition.tabId, setData("page-id", definition.pageId, setTestId_1("append-page-"+asText_2(definition.pageId), page)));
  const sameText=(left, right) => asText_2(left).toLowerCase()==asText_2(right).toLowerCase();
  const readsLegacy=sameText(definition.tabId, definition.pageId);
  let currentLineage=New_7(definition.tabId, readsLegacy?"default":"fresh", readsLegacy?definition.pageId:"", readsLegacy, readsLegacy?"read-current-tab-and-legacy-page-streams":"read-current-tab-stream-only");
  const applyLineage=(lineage) => {
    const lineage_1=lineage==null?currentLineage:lineage;
    currentLineage=lineage_1;
    setData("lineage-read-repair-policy", lineage_1.readRepairPolicy, setData("lineage-reads-legacy", lineage_1.readsLegacyPageStreams?"true":"false", setData("lineage-legacy-page-id-alias", lineage_1.legacyPageIdAlias, setData("lineage-kind", lineage_1.lineageKind, setData("lineage-stream-page-id", lineage_1.streamPageId, page)))));
  };
  applyLineage(currentLineage);
  const defaultLineageHealth=() => New_8(currentLineage.streamPageId, currentLineage.lineageKind, currentLineage.legacyPageIdAlias, currentLineage.readsLegacyPageStreams, currentLineage.readRepairPolicy, [], 0, [], 0);
  currentLineageHealth=defaultLineageHealth();
  selected="";
  selectedKeyJson="";
  buckets=[];
  locallyHiddenKeyIds=[];
  pendingSelectKeyId="";
  loadGeneration=0;
  visibleValueLimit=defaultRenderLimit();
  scrollValuesToBottomAfterNextRender=false;
  addKeyEditorOpen=false;
  addKeyMode="target";
  const isLocallyHiddenKeyId=(keyId) =>!isBlank_2(keyId)&&exists((hidden) => sameText(hidden, keyId), locallyHiddenKeyIds);
  const rememberLocallyHiddenKeyId=(keyId) => {
    if(!isBlank_2(keyId)&&!isLocallyHiddenKeyId(keyId))locallyHiddenKeyIds=locallyHiddenKeyIds.concat([keyId]);
  };
  const side=element_1("aside", "sidebar append-sidebar", null);
  const sideHead=element_1("div", "panel-head", null);
  const sideActions=element_1("div", "head-actions", null);
  const addActorKeyButton=setTestId_1("append-add-actor-key", button_1("", "Add actor key"));
  const addKeyButton=setTestId_1("append-add-key", button_1("", "Add target key"));
  const addProxyKeyButton=setTestId_1("append-add-proxy-key", button_1("", "Add proxy key"));
  const removeKeyButton=setTestId_1("append-remove-key", button_1("", "Remove"));
  const removePageButton=setTestId_1("append-remove-page", button_1("", "Remove page"));
  const reload=setTestId_1("append-reload", button_1("", "Reload"));
  const actionPool=setTestId_1("append-page-actions", element_1("details", "append-page-actions", null));
  const actionSummary=setTestId_1("append-page-actions-summary", element_1("summary", "append-page-actions-summary", "Actions"));
  const actionMenu=setTestId_1("append-page-actions-menu", element_1("div", "append-page-actions-menu", null));
  const filters=element_1("div", "filters", null);
  const keyFilter=setTestId_1("append-key-filter", input_1("key contains"));
  const newKeyInput=setTestId_1("append-key-input", input_1(textOr("\"Aster\"", definition.keyPlaceholder)));
  const newKeyAliasInput=setTestId_1("append-key-alias-input", input_1("target alias (optional)"));
  const addKeyPanel=setTestId_1("append-add-key-panel", element_1("div", "append-add-key-panel", null));
  const fallbackAddKeyPanel=setTestId_1("append-add-key-fallback", element_1("div", "append-add-key-fallback", null));
  const fallbackAddKeyActions=setTestId_1("append-add-key-actions", element_1("div", "append-add-key-actions", null));
  const cleanKeyButton=setTestId_1("append-key-clean", button_1("", "Clean"));
  const cancelKeyButton=setTestId_1("append-key-cancel", button_1("", "Cancel"));
  const okKeyButton=setTestId_1("append-key-ok", button_1("primary", "OK"));
  const addKeyRendererHost=setData("renderer-state", "not-rendered", setTestId_1("append-add-key-renderer-host", element_1("div", "append-add-key-renderer-host", null)));
  const status=setTestId_1("append-key-status", element_1("div", "state", "Loading"));
  const list=setTestId_1("append-key-list", element_1("div", "list", null));
  const work=setTestId_1("append-work", element_1("section", "append-work", null));
  const values=setTestId_1("append-values", element_1("div", "append-values", null));
  const form=setTestId_1("append-form", element_1("div", "append-form", null));
  const valueInput=setTestId_1("append-value-input", textarea("append-value-input", textOr("JSON value", definition.valuePlaceholder)));
  const directionInput=setTestId_1("append-direction", input_1("outbound-message"));
  const appendButton=setTestId_1("append-submit", button_1("primary", "Append"));
  const head_2=element_1("div", "work-head", null);
  const titleBox=element_1("div", "", null);
  const workState=setTestId_1("append-work-status", element_1("div", "state", "Loading"));
  const pendingState=setTestId_1("append-pending-state", element_1("div", "state pending-state", ""));
  const lineageHealthBox=setTestId_1("append-lineage-health", element_1("div", "meta wrap lineage-health", null));
  const lineageDetailBox=setTestId_1("append-lineage-detail", element_1("div", "lineage-detail", null));
  const lineageDetailPolicy=setTestId_1("append-lineage-detail-policy", element_1("span", "lineage-detail-value", ""));
  const lineageDetailStream=setTestId_1("append-lineage-detail-stream", element_1("span", "lineage-detail-value", ""));
  const lineageDetailLegacy=setTestId_1("append-lineage-detail-legacy", element_1("span", "lineage-detail-value", ""));
  const lineageDetailValueCount=setTestId_1("append-lineage-detail-value-count", element_1("span", "lineage-detail-value", ""));
  const lineageDetailValueStreams=setTestId_1("append-lineage-detail-value-streams", element_1("pre", "lineage-detail-value lineage-streams", ""));
  const lineageDetailKeyCount=setTestId_1("append-lineage-detail-key-count", element_1("span", "lineage-detail-value", ""));
  const lineageDetailKeyStreams=setTestId_1("append-lineage-detail-key-streams", element_1("pre", "lineage-detail-value lineage-streams", ""));
  const keyRegistryHealthBox=setTestId_1("append-key-registry-health", element_1("div", "meta wrap key-registry-health", null));
  const browserCacheHealthBox=setTestId_1("append-browser-cache-health", element_1("div", "meta wrap browser-cache-health", null));
  const lineageInfo=setTestId_1("append-lineage-info", element_1("details", "lineage-info", null));
  const lineageSummary=setTestId_1("append-lineage-toggle", element_1("summary", "lineage-summary", "Tab info"));
  const lineageInfoContent=setTestId_1("append-lineage-info-content", element_1("div", "lineage-info-content", null));
  const identityBox=setTestId_1("append-page-identity", element_1("div", "page-identity", null));
  const pageIdChip=setTestId_1("append-page-id", element_1("span", "identity-chip", "page "+asText_2(definition.pageId)));
  const tabIdChip=setTestId_1("append-tab-id", element_1("span", "identity-chip", "tab "+asText_2(definition.tabId)));
  const sideTitle=element_1("div", "panel-title", null);
  const lineageDetailRow=(label_1, valueNode) => {
    const row=element_1("div", "lineage-detail-row", null);
    append_1(row, [element_1("span", "lineage-detail-label", label_1), valueNode]);
    return row;
  };
  append_1(lineageDetailBox, [lineageDetailRow("policy", lineageDetailPolicy), lineageDetailRow("stream", lineageDetailStream), lineageDetailRow("legacy", lineageDetailLegacy), lineageDetailRow("value count", lineageDetailValueCount), lineageDetailRow("value streams", lineageDetailValueStreams), lineageDetailRow("key count", lineageDetailKeyCount), lineageDetailRow("key streams", lineageDetailKeyStreams)]);
  append_1(lineageInfoContent, [lineageHealthBox, lineageDetailBox, keyRegistryHealthBox, browserCacheHealthBox]);
  append_1(lineageInfo, [lineageSummary, lineageInfoContent]);
  newKeyInput.value=asText_2(definition.defaultKey);
  directionInput.value="outbound-message";
  directionInput.className="append-direction";
  appendButton.textContent=actorArguButtonLabel(definition);
  append_1(identityBox, [pageIdChip, tabIdChip]);
  append_1(sideTitle, [element_1("h1", "", pageTitle(definition)), identityBox]);
  append_1(actionMenu, isActorDynamicPage(definition)?[addActorKeyButton, addKeyButton, addProxyKeyButton, removeKeyButton, reload, removePageButton]:isActorArguPage(definition)?[addKeyButton, removeKeyButton, reload, removePageButton]:(addKeyButton.textContent="Add key",[addKeyButton, removeKeyButton, reload, removePageButton]));
  append_1(actionPool, [actionSummary, actionMenu]);
  append_1(sideActions, [actionPool]);
  append_1(sideHead, [sideTitle]);
  append_1(fallbackAddKeyActions, [cleanKeyButton, cancelKeyButton, okKeyButton]);
  append_1(fallbackAddKeyPanel, [newKeyInput, newKeyAliasInput, fallbackAddKeyActions]);
  append_1(addKeyPanel, [fallbackAddKeyPanel, addKeyRendererHost]);
  append_1(filters, [addKeyPanel, keyFilter, status]);
  append_1(side, [sideHead, sideActions, filters, list]);
  append_1(titleBox, [setTestId_1("append-page-type-label", element_1("label", "", pageTypeLabel(definition)+" / "+asText_2(definition.setName))), element_1("h2", "", pageTitle(definition)), element_1("div", "meta wrap", asText_2(definition.description)), lineageInfo]);
  append_1(head_2, [titleBox, workState]);
  const applyLineageHealth=(health) => {
    const health_1=health==null?defaultLineageHealth():health;
    currentLineageHealth=health_1;
    const valueStreamKeys=health_1.candidateValueStreamKeys==null?"":concat_2("\n", map(asText_2, health_1.candidateValueStreamKeys));
    const keyRegistryStreamKeys=health_1.candidateKeyRegistryStreamKeys==null?"":concat_2("\n", map(asText_2, health_1.candidateKeyRegistryStreamKeys));
    const visibleStreamKeys=(streamKeys) => isBlank_2(streamKeys)?"none":streamKeys;
    setData("lineage-health-policy", health_1.readRepairPolicy, setData("lineage-candidate-key-registry-stream-keys", keyRegistryStreamKeys, setData("lineage-candidate-value-stream-keys", valueStreamKeys, setData("lineage-candidate-key-registry-stream-count", String(health_1.candidateKeyRegistryStreamCount), setData("lineage-candidate-value-stream-count", String(health_1.candidateValueStreamCount), page)))));
    setData("read-repair-policy", health_1.readRepairPolicy, setData("candidate-key-registry-stream-keys", keyRegistryStreamKeys, setData("candidate-value-stream-keys", valueStreamKeys, setData("candidate-key-registry-stream-count", String(health_1.candidateKeyRegistryStreamCount), setData("candidate-value-stream-count", String(health_1.candidateValueStreamCount), setData("lineage-kind", health_1.lineageKind, setData("stream-page-id", health_1.streamPageId, lineageHealthBox)))))));
    lineageHealthBox.setAttribute("title", "value streams:\n"+valueStreamKeys+"\nkey registry streams:\n"+keyRegistryStreamKeys);
    lineageHealthBox.textContent="lineage "+String(asText_2(health_1.lineageKind))+" | stream "+String(asText_2(health_1.streamPageId))+" | value streams "+String(health_1.candidateValueStreamCount)+" | key streams "+String(health_1.candidateKeyRegistryStreamCount)+" | "+String(asText_2(health_1.readRepairPolicy));
    setData("read-repair-policy", health_1.readRepairPolicy, setData("candidate-key-registry-stream-keys", keyRegistryStreamKeys, setData("candidate-value-stream-keys", valueStreamKeys, setData("candidate-key-registry-stream-count", String(health_1.candidateKeyRegistryStreamCount), setData("candidate-value-stream-count", String(health_1.candidateValueStreamCount), setData("reads-legacy", health_1.readsLegacyPageStreams?"true":"false", setData("legacy-page-id-alias", health_1.legacyPageIdAlias, setData("lineage-kind", health_1.lineageKind, setData("stream-page-id", health_1.streamPageId, lineageDetailBox)))))))));
    lineageDetailPolicy.textContent=asText_2(health_1.readRepairPolicy);
    lineageDetailStream.textContent=asText_2(health_1.streamPageId);
    lineageDetailLegacy.textContent=isBlank_2(health_1.legacyPageIdAlias)?"none":asText_2(health_1.legacyPageIdAlias);
    lineageDetailValueCount.textContent=String(health_1.candidateValueStreamCount);
    lineageDetailValueStreams.textContent=visibleStreamKeys(valueStreamKeys);
    lineageDetailKeyCount.textContent=String(health_1.candidateKeyRegistryStreamCount);
    lineageDetailKeyStreams.textContent=visibleStreamKeys(keyRegistryStreamKeys);
  };
  applyLineageHealth(currentLineageHealth);
  if(isActorArguPage(definition)){
    form.className="append-form actor-argu-form";
    append_1(form, [valueInput, appendButton]);
  }
  else asText_2(definition.shape).toLowerCase()=="fcell-chat"?(form.className="append-form chat-form",append_1(form, [directionInput, valueInput, appendButton])):append_1(form, [valueInput, appendButton]);
  append_1(work, [head_2, pendingState, values, form]);
  append_1(page, [side, work]);
  const browserId=currentUserId();
  ensureSelectedSubscription=() => { };
  replayPendingCommands=() => { };
  deleteAcceptedPendingAppends=() =>() => null;
  rerenderAppendForm=() => { };
  rerenderAddKeyBuilder=() => { };
  const refreshPendingState=() => {
    readPendingRealitySplit((_3, _4) => renderPendingInspection(pendingState, filter((command) =>!(command==null)&&(sameText(command.target, definition.pageId)||!isBlank_2(command.payloadJson)&&command.payloadJson.indexOf("\"pageId\":\""+asText_2(definition.pageId)+"\"")!=-1), _3), filter((command) =>!(command==null)&&(sameText(command.target, definition.pageId)||!isBlank_2(command.payloadJson)&&command.payloadJson.indexOf("\"pageId\":\""+asText_2(definition.pageId)+"\"")!=-1), _4)));
  };
  const isPendingForThisPage=(command) =>!(command==null)&&(sameText(command.target, definition.pageId)||!isBlank_2(command.payloadJson)&&command.payloadJson.indexOf("\"pageId\":\""+asText_2(definition.pageId)+"\"")!=-1);
  const currentFilterText=() => isBlank_2(keyFilter.value)?"":Trim(keyFilter.value);
  const requestValuesScrollToBottom=() => {
    scrollValuesToBottomAfterNextRender=true;
  };
  const stateCacheKey=() => cacheKey("append-page-state", ofArray([definition.pageId, definition.tabId, currentFilterText()]));
  const keyRegistryCacheKey=() => cacheKey("append-page-keys", ofArray([definition.pageId, definition.tabId]));
  currentKeyMaxSequence=0n;
  keyRegistryWsState="idle";
  const updateBrowserCacheHealth=(renderedCount, cachedCount, minSequence, maxSequence, snapshotSeqId, backendGap) => {
    const gapText=backendGap?"true":"false";
    const cacheKey_1=stateCacheKey();
    const selectedText=isBlank_2(selected)?"(none)":selected;
    const n=setData("cache-key", cacheKey_1, browserCacheHealthBox);
    let _3=setData("selected-key-id", selected, n);
    let _4=setData("rendered-count", String(renderedCount), _3);
    let _5=setData("cached-count", String(cachedCount), _4);
    let _6=setData("min-sequence", String(minSequence), _5);
    let _7=setData("max-sequence", String(maxSequence), _6);
    let _8=setData("snapshot-seqid", String(snapshotSeqId), _7);
    setData("backend-gap", gapText, _8);
    browserCacheHealthBox.setAttribute("title", "cacheKey="+String(cacheKey_1)+"\nselectedKey="+String(selectedText)+"\nrendered="+String(renderedCount)+"\ncached="+String(cachedCount)+"\nrange="+String(minSequence)+".."+String(maxSequence)+"\nsnapshotSeqId="+String(snapshotSeqId)+"\nbackendGap="+String(gapText));
    browserCacheHealthBox.textContent="browser cache "+String(cacheKey_1)+" | rendered "+String(renderedCount)+" | cached "+String(cachedCount)+" | seq "+String(minSequence)+".."+String(maxSequence)+" | snapshot "+String(snapshotSeqId)+" | gap "+String(gapText);
  };
  const updateKeyRegistryHealth=() => {
    const cacheKey_1=keyRegistryCacheKey();
    const x=setData("ws-state", keyRegistryWsState, keyRegistryHealthBox);
    const x_1=setData("key-count", String(length(buckets)), x);
    let _3=setData("max-sequence", String(currentKeyMaxSequence), x_1);
    setData("cache-key", cacheKey_1, _3);
    keyRegistryHealthBox.setAttribute("title", "cacheKey="+String(cacheKey_1)+"\nwsState="+String(keyRegistryWsState)+"\nvisibleKeyCount="+String(length(buckets))+"\nmaxSequence="+String(currentKeyMaxSequence));
    keyRegistryHealthBox.textContent="key registry ws "+String(keyRegistryWsState)+" | visible keys "+String(length(buckets))+" | seq "+String(currentKeyMaxSequence);
  };
  const writeAppendPageKeyWatermark=(snapshot) => {
    const b=snapshot.keyMaxSequence;
    currentKeyMaxSequence=Compare(currentKeyMaxSequence, b)===1?currentKeyMaxSequence:b;
    writeWatermark(keyRegistryCacheKey(), currentKeyMaxSequence, snapshot.bucketCount, "append-page-keys");
    updateKeyRegistryHealth();
  };
  const writeCurrentSnapshot=() => {
    const snapshot=New_10("ok", definition, length(buckets), fold((_3, _4) => Compare(_3, _4)===1?_3:_4, 0n, map((bucket) => bucket.maxSequence, buckets)), currentKeyMaxSequence, currentLineage, currentLineageHealth, buckets);
    writeSnapshotWithWatermark(stateCacheKey(), snapshot, snapshot.maxSequence, appendPageValueCount(snapshot), "append-page-state");
    writeAppendPageKeyWatermark(snapshot);
  };
  const appendPageKeyId=(keys) => asText_2(definition.setName)+"::"+concat_2(" + ", sortBy((key) => key.toLowerCase(), distinctBy((key) => key.toLowerCase(), choose((key) => {
    const text=Trim(asText_2(key));
    return isBlank_2(text)?null:Some(text);
  }, arrayOrEmpty_1(keys)))));
  const selectBucketKeys=(keys) => {
    const keys_1=arrayOrEmpty_1(keys);
    return length(keys_1)>0&&(selected=appendPageKeyId(keys_1),selectedKeyJson=keysAsJson(keys_1),newKeyInput.value=selectedKeyJson,true);
  };
  const sortAppendPageBuckets=(items) => sortBy((bucket) =>[asText_2(bucket.setName), asText_2(bucket.keyId)], arrayOrEmpty_1(items));
  const sequenceBounds=(items) => {
    let oldest, newest;
    oldest=0n;
    newest=0n;
    iter((value) => {
      !(value==null)&&value.sequence>0n&&(oldest===0n||value.sequence<oldest)?oldest=value.sequence:void 0;
      !(value==null)&&value.sequence>newest?newest=value.sequence:void 0;
    }, arrayOrEmpty_1(items));
    return[oldest, newest];
  };
  const mergeAppendValues=(existing, incoming) => {
    let merged;
    merged=[];
    const add=(value) => {
      if(!(value==null)&&!isBlank_2(value.valueId)&&!exists((row) => row.valueId==value.valueId, merged))merged=merged.concat([value]);
    };
    iter(add, arrayOrEmpty_1(incoming));
    iter(add, arrayOrEmpty_1(existing));
    return sortBy((value) => asText_2(value.createdAtUtc), merged);
  };
  function renderList(){
    clear(list);
    iter((bucket) => {
      const item=button_1(bucket.keyId==selected?"list-card active":"list-card", null);
      const x=setData("key-id", bucket.keyId, setTestId_1("append-key-card", item));
      const x_1=setData("key-display-name", asText_2(bucket.displayName), x);
      let _3=setData("key-json", keysAsJson(bucket.keys), x_1);
      let _4=setData("min-sequence", String(bucket.minSequence), _3);
      setData("max-sequence", String(bucket.maxSequence), _4);
      item.setAttribute("title", joinValues(bucket.keys));
      let _5=item;
      const displayName=Trim(asText_2(bucket.displayName));
      let _6=isBlank_2(displayName)?joinValues(bucket.keys):displayName;
      let _7=element_1("div", "strong wrap", _6);
      let _8=[_7, element_1("div", "muted wrap", asText_2(bucket.setName)), element_1("div", "meta", "values="+String(bucket.valueCount)+" seq="+String(bucket.maxSequence)+" updated="+String(asText_2(bucket.updatedAtUtc)))];
      append_1(_5, _8);
      item.addEventListener("click", () => {
        selected=bucket.keyId;
        selectedKeyJson=keysAsJson(bucket.keys);
        newKeyInput.value=selectedKeyJson;
        visibleValueLimit=defaultRenderLimit();
        renderList();
        requestValuesScrollToBottom();
        renderValues();
        rerenderAppendForm();
        return ensureSelectedSubscription();
      });
      list.appendChild(item);
    }, buckets);
  }
  function renderValues(){
    while(true)
      {
        let _3, _4, _5;
        clear(values);
        const x=(((n) =>(n_1) => setData(n, selected, n_1))("selected-key-id"))(work);
        ((((n) =>(n_1) => setData(n, selectedKeyJson, n_1))("selected-key-json"))(x));
        const bucket=(((p) =>(a_3) => tryFind(p, a_3))((bucket_2) => bucket_2.keyId==selected))(buckets);
        if(bucket!=null&&bucket.$==1){
          const bucket_1=bucket.$0;
          const allValues=arrayOrEmpty_1(bucket_1.values);
          const visible=latestArray(visibleValueLimit, allValues);
          const a=0;
          const b=length(allValues)-length(visible);
          const hiddenCached=Compare(a, b)===1?a:b;
          const a_1=bucket_1.valueCount;
          const b_1=length(allValues);
          const reportedCount=Compare(a_1, b_1)===1?a_1:b_1;
          const fromValues=(sequenceBounds(allValues))[0];
          const oldestSequence=bucket_1.minSequence>0n?bucket_1.minSequence:fromValues;
          const a_2=bucket_1.maxSequence;
          const b_2=(sequenceBounds(allValues))[1];
          const newestSequence=Compare(a_2, b_2)===1?a_2:b_2;
          const backendGapAvailable=oldestSequence>1n&&hiddenCached===0;
          const x_1=[asText_2(definition.tabId), asText_2(definition.shape), asText_2(definition.setName), concat_2("\u001f", arrayOrEmpty_1(bucket_1.keys))];
          const selectedValueStreamKey=(((s) =>(s_1) => concat_2(s, s_1))("\n"))(x_1);
          const x_2=(((n, v) =>(n_1) => setData(n, v, n_1))("lineage-candidate-value-stream-count", "1"))(page);
          ((((n, selectedValueStreamKey_1) =>(n_1) => setData(n, selectedValueStreamKey_1, n_1))("lineage-candidate-value-stream-keys", selectedValueStreamKey))(x_2));
          const x_3=(((n, v) =>(n_1) => setData(n, v, n_1))("candidate-value-stream-count", "1"))(lineageHealthBox);
          ((((n, selectedValueStreamKey_1) =>(n_1) => setData(n, selectedValueStreamKey_1, n_1))("candidate-value-stream-keys", selectedValueStreamKey))(x_3));
          const x_4=(((n, v) =>(n_1) => setData(n, v, n_1))("candidate-value-stream-count", "1"))(lineageDetailBox);
          ((((n, selectedValueStreamKey_1) =>(n_1) => setData(n, selectedValueStreamKey_1, n_1))("candidate-value-stream-keys", selectedValueStreamKey))(x_4));
          lineageDetailValueCount.textContent="1";
          lineageDetailValueStreams.textContent=selectedValueStreamKey;
          const x_5=(((n, v) =>(n_1) => setData(n, v, n_1))("rendered-count", String(length(visible))))(values);
          const x_6=(((n, v) =>(n_1) => setData(n, v, n_1))("cached-count", String(length(allValues))))(x_5);
          const x_7=(((n, v) =>(n_1) => setData(n, v, n_1))("oldest-sequence", String(oldestSequence)))(x_6);
          const x_8=(((n, v) =>(n_1) => setData(n, v, n_1))("min-sequence", String(oldestSequence)))(x_7);
          const x_9=(((n, v) =>(n_1) => setData(n, v, n_1))("max-sequence", String(newestSequence)))(x_8);
          const x_10=(((n, v) =>(n_1) => setData(n, v, n_1))("snapshot-seqid", String(newestSequence)))(x_9);
          ((((n, v) =>(n_1) => setData(n, v, n_1))("backend-gap", backendGapAvailable?"true":"false"))(x_10));
          updateBrowserCacheHealth(length(visible), length(allValues), oldestSequence, newestSequence, newestSequence, backendGapAvailable);
          if(length(visible)===0)_3=void values.appendChild(element_1("div", "empty", "No values appended yet."));
          else {
            if(hiddenCached>0){
              const x_11=button_1("", "Load older ("+String(hiddenCached)+")");
              const loadOlder=(((i) =>(n) => setTestId_1(i, n))("append-load-older"))(x_11);
              _4=(loadOlder.addEventListener("click", ((allValues_1) =>() => {
                const a_3=length(allValues_1);
                const b_3=visibleValueLimit+defaultRenderLimit();
                visibleValueLimit=Compare(a_3, b_3)===-1?a_3:b_3;
                return renderValues();
              })(allValues)),void values.appendChild(loadOlder));
            }
            else if(backendGapAvailable){
              const x_12=button_1("", "Load older (backend)");
              const loadOlder_1=(((i) =>(n) => setTestId_1(i, n))("append-load-older"))(x_12);
              _4=(loadOlder_1.addEventListener("click", ((bucket_2, oldestSequence_1) =>() => readOlderFromBackend(bucket_2, oldestSequence_1))(bucket_1, oldestSequence)),void values.appendChild(loadOlder_1));
            }
            else _4=null;
            _3=(((a_3) =>(a_4) => {
              iter(a_3, a_4);
            })((value) => {
              values.appendChild(renderAppendValue(value));
            }))(visible);
          }
          _5=length(visible)<reportedCount?setStatus(workState, "Showing "+String(length(visible))+"/"+String(reportedCount)+" value(s)"):setStatus(workState, String(reportedCount)+" value(s)");
        }
        else {
          const x_13=(((n, v) =>(n_1) => setData(n, v, n_1))("rendered-count", "0"))(values);
          const x_14=(((n, v) =>(n_1) => setData(n, v, n_1))("cached-count", "0"))(x_13);
          const x_15=(((n, v) =>(n_1) => setData(n, v, n_1))("oldest-sequence", "0"))(x_14);
          const x_16=(((n, v) =>(n_1) => setData(n, v, n_1))("min-sequence", "0"))(x_15);
          const x_17=(((n, v) =>(n_1) => setData(n, v, n_1))("max-sequence", "0"))(x_16);
          const x_18=(((n, v) =>(n_1) => setData(n, v, n_1))("snapshot-seqid", "0"))(x_17);
          ((((n, v) =>(n_1) => setData(n, v, n_1))("backend-gap", "false"))(x_18));
          updateBrowserCacheHealth(0, 0, 0n, 0n, 0n, false);
          const x_19=(((n, v) =>(n_1) => setData(n, v, n_1))("lineage-candidate-value-stream-count", "0"))(page);
          ((((n, v) =>(n_1) => setData(n, v, n_1))("lineage-candidate-value-stream-keys", ""))(x_19));
          const x_20=(((n, v) =>(n_1) => setData(n, v, n_1))("candidate-value-stream-count", "0"))(lineageHealthBox);
          ((((n, v) =>(n_1) => setData(n, v, n_1))("candidate-value-stream-keys", ""))(x_20));
          const x_21=(((n, v) =>(n_1) => setData(n, v, n_1))("candidate-value-stream-count", "0"))(lineageDetailBox);
          ((((n, v) =>(n_1) => setData(n, v, n_1))("candidate-value-stream-keys", ""))(x_21));
          lineageDetailValueCount.textContent="0";
          lineageDetailValueStreams.textContent="none";
          values.appendChild(element_1("div", "empty", "No key selected."));
          _5=setStatus(workState, "No key selected");
        }
        if(scrollValuesToBottomAfterNextRender){
          scrollValuesToBottomAfterNextRender=false;
          scrollToBottomAfterRender(values);
        }
        return rerenderAppendForm();
      }
  }
  function readOlderFromBackend(bucket, beforeSequence){
    const keyJson=isBlank_2(selectedKeyJson)?keysAsJson(bucket.keys):selectedKeyJson;
    const url="/pages/api/read-before?pageId="+encodeURIComponent(asText_2(definition.pageId))+"&keyJson="+encodeURIComponent(keyJson)+"&beforeSequence="+String(beforeSequence)+"&count="+String(defaultRenderLimit());
    setStatus(workState, "Loading older values before "+String(beforeSequence));
    return getJson(url, (reply) => {
      applyLineage(reply.lineage);
      applyLineageHealth(reply.lineageHealth);
      const incoming=arrayOrEmpty_1(reply.values);
      if(length(incoming)===0)setStatus(workState, "No older backend values");
      else {
        updateSelectedBucketWithOlder(incoming);
        setStatus(workState, "Loaded "+String(length(incoming))+" older backend value(s)");
      }
    }, (t) => {
      setStatus(workState, t);
    });
  }
  function updateSelectedBucketWithOlder(incoming){
    let mergedLength;
    mergedLength=0;
    buckets=map((bucket) => {
      if(bucket.keyId==selected){
        const merged=mergeAppendValues(bucket.values, incoming);
        const p=sequenceBounds(merged);
        mergedLength=length(merged);
        const a=bucket.valueCount;
        const b_2=length(merged);
        let _3=Compare(a, b_2)===1?a:b_2;
        const a_1=bucket.maxSequence;
        const b_3=p[1];
        let _4=Compare(a_1, b_3)===1?a_1:b_3;
        return New_11(bucket.keyId, bucket.keys, bucket.displayName, bucket.setName, _3, p[0], _4, bucket.updatedAtUtc, merged);
      }
      else return bucket;
    }, buckets);
    writeCurrentSnapshot();
    const b=visibleValueLimit+length(arrayOrEmpty_1(incoming));
    const b_1=Compare(mergedLength, b)===-1?mergedLength:b;
    visibleValueLimit=Compare(visibleValueLimit, b_1)===1?visibleValueLimit:b_1;
    renderList();
    renderValues();
  }
  const readNewerFromBackend=(generation, bucket) => {
    const keyJson=keysAsJson(bucket.keys);
    return getJson("/pages/api/read-after?pageId="+encodeURIComponent(asText_2(definition.pageId))+"&keyJson="+encodeURIComponent(keyJson)+"&afterSequence="+String(bucket.maxSequence)+"&count="+String(defaultCacheLimit()), (reply) => {
      if(generation===loadGeneration){
        applyLineage(reply.lineage);
        applyLineageHealth(reply.lineageHealth);
        const incoming=arrayOrEmpty_1(reply.values);
        if(length(incoming)>0){
          (deleteAcceptedPendingAppends(bucket))(incoming);
          const keyId=reply.keyId;
          buckets=map((bucket_1) => {
            if(bucket_1.keyId==keyId){
              const merged=mergeAppendValues(bucket_1.values, incoming);
              const p=sequenceBounds(merged);
              const minSequence=p[0];
              const a=bucket_1.valueCount;
              const b=length(merged);
              let _3=Compare(a, b)===1?a:b;
              const a_1=bucket_1.maxSequence;
              const b_1=p[1];
              let _4=Compare(a_1, b_1)===1?a_1:b_1;
              return New_11(bucket_1.keyId, bucket_1.keys, bucket_1.displayName, bucket_1.setName, _3, minSequence>0n?minSequence:bucket_1.minSequence, _4, bucket_1.updatedAtUtc, merged);
            }
            else return bucket_1;
          }, buckets);
          writeCurrentSnapshot();
          renderList();
          requestValuesScrollToBottom();
          renderValues();
          setStatus(status, "Synced "+String(length(incoming))+" newer value(s) from backend");
        }
        else setStatus(status, "Cached data is current");
      }
    }, (error) => {
      if(generation===loadGeneration)setStatus(status, "Cached data loaded; tail sync failed: "+error);
    });
  };
  const applySnapshot=(source, data) => {
    let _3;
    applyLineage(data.lineage);
    applyLineageHealth(data.lineageHealth);
    const b=data.keyMaxSequence;
    currentKeyMaxSequence=Compare(currentKeyMaxSequence, b)===1?currentKeyMaxSequence:b;
    buckets=filter((bucket_1) =>!isLocallyHiddenKeyId(bucket_1.keyId), arrayOrEmpty_1(data.buckets));
    visibleValueLimit=defaultRenderLimit();
    if(isBlank_2(pendingSelectKeyId))_3=false;
    else {
      const m=tryFind((bucket_1) => sameText(bucket_1.keyId, pendingSelectKeyId), buckets);
      if(m==null)_3=false;
      else {
        const bucket=m.$0;
        const selectedPending=bucket==null?false:selectBucketKeys(bucket.keys);
        _3=(selectedPending?pendingSelectKeyId="":void 0,selectedPending);
      }
    }
    if(_3)null;
    else(isBlank_2(selected)||!exists((bucket_1) => bucket_1.keyId==selected, buckets))&&length(buckets)>0?(selected=get(buckets, 0).keyId,selectedKeyJson=keysAsJson(get(buckets, 0).keys),void(newKeyInput.value=selectedKeyJson)):length(buckets)===0?(selected="",void(selectedKeyJson="")):null;
    setStatus(status, "Loaded "+String(length(buckets))+" "+String(source)+" bucket(s)");
    renderList();
    requestValuesScrollToBottom();
    renderValues();
    ensureSelectedSubscription();
    iter((bucket_1) => {
      (deleteAcceptedPendingAppends(bucket_1))(bucket_1.values);
    }, buckets);
    refreshPendingState();
    return sameText(source, "backend")?void setTimeout(() => {
      replayPendingCommands();
    }, 100):null;
  };
  const load=() => {
    let url;
    loadGeneration=loadGeneration+1;
    const generation=loadGeneration;
    const filterText=currentFilterText();
    url="/pages/api/state?pageId="+encodeURIComponent(asText_2(definition.pageId))+"&limit="+String(defaultCacheLimit());
    if(!isBlank_2(filterText))url=url+"&key="+encodeURIComponent(filterText);
    const cacheKey_1=stateCacheKey();
    const fetchFullState=() => {
      getJson(url, (data) => {
        if(generation===loadGeneration){
          writeSnapshotWithWatermark(cacheKey_1, data, data.maxSequence, appendPageValueCount(data), "append-page-state");
          writeAppendPageKeyWatermark(data);
          applySnapshot("backend", data);
        }
      }, (error) => {
        if(generation===loadGeneration){
          setStatus(status, error);
          setStatus(workState, error);
        }
      });
    };
    readJson(cacheKey_1, (a) => {
      if(a==null){
        if(generation===loadGeneration)fetchFullState();
      }
      else if(a.$0,generation===loadGeneration){
        const cached=a.$0;
        applySnapshot("cached", cached);
        const sequenceBuckets=filter((bucket) => bucket.maxSequence>0n, arrayOrEmpty_1(cached.buckets));
        if(length(sequenceBuckets)===0)fetchFullState();
        else {
          iter((_3) => readNewerFromBackend(generation, _3), sequenceBuckets);
          fetchFullState();
        }
      }
    });
  };
  syncSocket=null;
  queuedSyncFrames=[];
  subscribedValueStream="";
  keyRegistrySubscribed=false;
  keyRegistryTailRequested=false;
  pendingWsAppendIds=[];
  syncRepairScheduled=false;
  repairSyncAfterClose=() => { };
  const setWsState=(value) => {
    setData("ws-state", value, work);
  };
  const setKeyRegistryWsState=(value) => {
    keyRegistryWsState=asText_2(value);
    setData("key-registry-ws-state", value, work);
    updateKeyRegistryHealth();
  };
  const effectiveSelectedKeys=() => {
    const selectedJsonKeys=keysFromJson(selectedKeyJson);
    const m=tryFind((bucket_1) => bucket_1.keyId==selected, buckets);
    if(m==null)return keysFromJson(selectedKeyJson);
    else {
      const bucket=m.$0;
      return length(selectedJsonKeys)>0&&sameText(appendPageKeyId(selectedJsonKeys), bucket.keyId)?(m.$0,selectedJsonKeys):arrayOrEmpty_1(m.$0.keys);
    }
  };
  const effectiveSelectedKeyJson=() => {
    const selectedJsonKeys=keysFromJson(selectedKeyJson);
    const m=tryFind((bucket_1) => bucket_1.keyId==selected, buckets);
    if(m==null)return selectedKeyJson;
    else {
      const bucket=m.$0;
      return length(selectedJsonKeys)>0&&sameText(appendPageKeyId(selectedJsonKeys), bucket.keyId)?(m.$0,selectedKeyJson):keysAsJson(m.$0.keys);
    }
  };
  const selectedBucket=() => {
    const m=tryFind((bucket_1) => bucket_1.keyId==selected, buckets);
    if(m==null){
      const keys=effectiveSelectedKeys();
      return length(keys)===0?null:Some(New_11(appendPageKeyId(keys), keys, "", definition.setName, 0, 0n, 0n, "", []));
    }
    else {
      const bucket=m.$0;
      const keys_1=effectiveSelectedKeys();
      return Some(New_11(bucket.keyId, length(keys_1)>0?keys_1:bucket.keys, bucket.displayName, bucket.setName, bucket.valueCount, bucket.minSequence, bucket.maxSequence, bucket.updatedAtUtc, bucket.values));
    }
  };
  deleteAcceptedPendingAppends=(bucket) =>(acceptedValues) => {
    const acceptedValues_1=arrayOrEmpty_1(acceptedValues);
    if(length(acceptedValues_1)>0){
      const keyJson=keysAsJson(bucket.keys);
      const commandMatches=(command) => {
        if(sameText(command.kind, "append-page-append-value")&&sameText(command.url, "/pages/api/append")&&isPendingForThisPage(command)&&!isBlank_2(command.payloadJson))try {
          const x=json(command.payloadJson);
          const _3=command.commandId;
          return sameText(x.pageId, definition.pageId)&&sameText(x.keyJson, keyJson)&&exists((value) => sameText(value.valueId, _3), acceptedValues_1);
        }
        catch(m){
          return false;
        }
        else return false;
      };
      return readAllPending((commands) => {
        let remaining;
        const accepted=filter(commandMatches, commands);
        if(length(accepted)>0){
          remaining=length(accepted);
          const finishOne=() => {
            remaining=remaining-1;
            remaining===0?refreshPendingState():void 0;
          };
          iter((command) => {
            deletePendingThen(command.commandId, finishOne);
          }, accepted);
        }
      });
    }
    else return null;
  };
  const streamKeyFor=(bucket) => New_6(definition.tabId, definition.shape, definition.setName, arrayOrEmpty_1(bucket.keys));
  const handleSyncEvent=(source, event) => {
    let o, updated, _3, o_1;
    if(!(event==null)){
      const m=asText_2(event.sourceKind).toLowerCase();
      if(m=="append-page.key"||m=="append-page.key-hidden"){
        if(!(event==null)&&event.sequence>0n){
          const m_1=asText_2(event.sourceKind).toLowerCase();
          if(m_1=="append-page.key"){
            if(event==null||isBlank_2(event.payload))o=null;
            else try {
              const wire=json(event.payload);
              if(wire==null||asText_2(wire.schema)!="ptc.comm.spa.append-page.key.v1"||!sameText(wire.pageId, definition.pageId))o=null;
              else {
                const keys=filter((key) =>!isBlank_2(key), map(asText_2, arrayOrEmpty_1(wire.keys)));
                o=length(keys)===0?null:Some([keys, Trim(asText_2(wire.displayName))]);
              }
            }
            catch(m_3){
              o=null;
            }
            if(o==null)return null;
            else {
              const _4=o.$0[0];
              const _5=o.$0[1];
              const b=event.sequence;
              currentKeyMaxSequence=Compare(currentKeyMaxSequence, b)===1?currentKeyMaxSequence:b;
              const keyId=appendPageKeyId(_4);
              const filterText=currentFilterText();
              if((isBlank_2(filterText)||exists((key) => asText_2(key).toLowerCase().indexOf(filterText.toLowerCase())!=-1, arrayOrEmpty_1(_4)))&&!isLocallyHiddenKeyId(keyId)){
                const m_2=tryFind((bucket_1) => sameText(bucket_1.keyId, keyId), buckets);
                if(m_2==null)updated=New_11(keyId, _4, _5, definition.setName, 0, 0n, 0n, asText_2(event.createdAtUtc), []);
                else {
                  const existing=m_2.$0;
                  updated=New_11(existing.keyId, _4, textOr(existing.displayName, _5), definition.setName, existing.valueCount, existing.minSequence, existing.maxSequence, textOr(existing.updatedAtUtc, event.createdAtUtc), existing.values);
                }
                _3=(buckets=sortAppendPageBuckets(filter((bucket_1) =>!sameText(bucket_1.keyId, keyId), buckets).concat([updated])),sameText(pendingSelectKeyId, keyId)?selectBucketKeys(_4)?void(pendingSelectKeyId=""):null:isBlank_2(selected)||!exists((bucket_1) => sameText(bucket_1.keyId, selected), buckets)?(selected=keyId,selectedKeyJson=keysAsJson(_4),void(newKeyInput.value=selectedKeyJson)):null);
              }
              else _3=null;
              writeCurrentSnapshot();
              renderList();
              renderValues();
              ensureSelectedSubscription();
              return setStatus(status, "Synced "+String(source)+" key registry");
            }
          }
          else if(m_1=="append-page.key-hidden"){
            if(event==null||isBlank_2(event.payload))o_1=null;
            else try {
              const wire_1=json(event.payload);
              o_1=wire_1==null||asText_2(wire_1.schema)!="ptc.comm.spa.append-page.key-hidden.v1"||!sameText(wire_1.pageId, definition.pageId)||isBlank_2(wire_1.keyId)?null:Some(Trim(wire_1.keyId));
            }
            catch(m_4){
              o_1=null;
            }
            if(o_1==null)return null;
            else {
              const keyId_1=o_1.$0;
              const b_1=event.sequence;
              currentKeyMaxSequence=Compare(currentKeyMaxSequence, b_1)===1?currentKeyMaxSequence:b_1;
              rememberLocallyHiddenKeyId(keyId_1);
              buckets=sortAppendPageBuckets(filter((bucket_1) =>!sameText(bucket_1.keyId, keyId_1), buckets));
              if(sameText(selected, keyId_1))if(length(buckets)>0){
                selected=get(buckets, 0).keyId;
                selectedKeyJson=keysAsJson(get(buckets, 0).keys);
                newKeyInput.value=selectedKeyJson;
              }
              else {
                selected="";
                selectedKeyJson="";
              }
              writeCurrentSnapshot();
              renderList();
              renderValues();
              ensureSelectedSubscription();
              return setStatus(status, "Synced "+String(source)+" key removal");
            }
          }
          else return null;
        }
        else return null;
      }
      else if(!(event==null)&&event.sequence>0n&&!(event.streamKey==null)){
        const eventKeys=arrayOrEmpty_1(event.streamKey.keys);
        const o_2=tryFind((bucket_1) => {
          const left=arrayOrEmpty_1(bucket_1.keys);
          const right=arrayOrEmpty_1(eventKeys);
          return length(left)===length(right)&&forall2(sameText, left, right);
        }, buckets);
        if(o_2==null)return null;
        else {
          const bucket=o_2.$0;
          return bucket.maxSequence>0n?readNewerFromBackend(loadGeneration, bucket):load();
        }
      }
      else return null;
    }
    else return null;
  };
  function flushSyncFrames(socket){
    if(Equals(socket.readyState, 1)){
      const frames=queuedSyncFrames;
      queuedSyncFrames=[];
      iter((frame) => {
        socket.send(frame);
      }, frames);
    }
  }
  function ensureSyncSocket(){
    let _3, _4;
    if(syncSocket!=null&&syncSocket.$==1){
      const socket=syncSocket.$0;
      _3=(Equals(socket.readyState, 1)||Equals(socket.readyState, 0))&&(_4=syncSocket.$0,true);
    }
    else _3=false;
    if(_3)return _4;
    else {
      setWsState("connecting");
      const socket_1=new WebSocket(syncWebSocketUrl());
      syncSocket=Some(socket_1);
      socket_1.onopen=() => {
        setWsState("open");
        return flushSyncFrames(socket_1);
      };
      socket_1.onmessage=(event) => {
        const text=String(event.data);
        try {
          const response=json(text);
          const responseType=asText_2(response.type).toLowerCase();
          const responseStatus=asText_2(response.status).toLowerCase();
          const requestId=asText_2(response.requestId);
          switch(responseStatus=="ok"?responseType=="subscribe"?0:responseType=="append"?1:responseType=="append-page"?1:responseType=="actor-argu"?1:responseType=="stream-event"?2:responseType=="read-tail"?3:responseType=="read"?3:responseType=="tail"?3:5:responseStatus=="error"?4:5){
            case 0:
              return asText_2(response.streamKey).indexOf("append-page-key-registry")!=-1?setKeyRegistryWsState("subscribed"):setWsState("subscribed");
            case 1:
              if(exists((id) => id==requestId, pendingWsAppendIds)){
                pendingWsAppendIds=filter((id) => id!=requestId, pendingWsAppendIds);
                deletePendingThen(requestId, () => {
                  valueInput.value="";
                  refreshPendingState();
                  setStatus(workState, "Appended through WebSocket");
                });
              }
              if(sameText(responseType, "actor-argu"))(((event_1, value) => {
                let keys, matched, _5;
                if(!(value==null)&&!isBlank_2(value.valueId)){
                  const eventKeys=event_1==null||event_1.streamKey==null?[]:arrayOrEmpty_1(event_1.streamKey.keys);
                  if(length(eventKeys)>0)keys=eventKeys;
                  else {
                    const m=tryFind((bucket_1) => bucket_1.keyId==selected, buckets);
                    keys=m==null?keysFromJson(selectedKeyJson):arrayOrEmpty_1(m.$0.keys);
                  }
                  if(length(keys)>0){
                    const keyId=appendPageKeyId(keys);
                    const incoming=[value];
                    matched=false;
                    buckets=map((bucket_1) => {
                      if(sameText(bucket_1.keyId, keyId)){
                        matched=true;
                        const merged=mergeAppendValues(bucket_1.values, incoming);
                        const p_1=sequenceBounds(merged);
                        const minSequence=p_1[0];
                        const a=bucket_1.valueCount;
                        const b=length(merged);
                        let _6=Compare(a, b)===1?a:b;
                        const a_1=bucket_1.maxSequence;
                        const b_1=p_1[1];
                        let _7=Compare(a_1, b_1)===1?a_1:b_1;
                        return New_11(bucket_1.keyId, keys, bucket_1.displayName, bucket_1.setName, _6, minSequence>0n?minSequence:bucket_1.minSequence, _7, textOr(bucket_1.updatedAtUtc, value.createdAtUtc), merged);
                      }
                      else return bucket_1;
                    }, buckets);
                    if(!matched){
                      const p=sequenceBounds(incoming);
                      const bucket=New_11(keyId, keys, "", definition.setName, length(incoming), p[0], p[1], asText_2(value.createdAtUtc), incoming);
                      _5=void(buckets=sortAppendPageBuckets(buckets.concat([bucket])));
                    }
                    else _5=null;
                    selected=keyId;
                    selectedKeyJson=keysAsJson(keys);
                    newKeyInput.value=selectedKeyJson;
                    writeCurrentSnapshot();
                    renderList();
                    requestValuesScrollToBottom();
                    return renderValues();
                  }
                  else return null;
                }
                else return null;
              })(response.event, response.value));
              return handleSyncEvent("live", response.event);
            case 2:
              return handleSyncEvent("live", response.event);
            case 3:
              return iter((_5) => handleSyncEvent("tail", _5), arrayOrEmpty_1(response.events));
            case 4:
              return exists((id) => id==requestId, pendingWsAppendIds)?setStatus(workState, pendingFailure("WebSocket append", asText_2(response.error))):setStatus(status, "WebSocket sync error: "+asText_2(response.error));
            case 5:
              return null;
          }
        }
        catch(error){
          return setStatus(status, "WebSocket sync parse failed: "+errorMessage_2(error));
        }
      };
      socket_1.onerror=() => {
        setWsState("error");
        return setStatus(status, "WebSocket sync error; pending command remains replayable");
      };
      socket_1.onclose=() => {
        syncSocket=null;
        subscribedValueStream="";
        keyRegistrySubscribed=false;
        keyRegistryTailRequested=false;
        setWsState("closed");
        setKeyRegistryWsState("closed");
        return!syncRepairScheduled?(syncRepairScheduled=true,void setTimeout(() => {
          syncRepairScheduled=false;
          repairSyncAfterClose();
        }, 500)):null;
      };
      return socket_1;
    }
  }
  function sendSyncFrame(frame){
    const socket=ensureSyncSocket();
    if(Equals(socket.readyState, 1))socket.send(frame);
    else queuedSyncFrames=queuedSyncFrames.concat([frame]);
  }
  const subscribeKeyRegistry=() => {
    const streamPageId=textOr(definition.pageId, definition.tabId);
    const streamKey=New_6(streamPageId, "append-page-key-registry", definition.setName, ["__append-page-keys", streamPageId]);
    if(!keyRegistrySubscribed){
      keyRegistrySubscribed=true;
      setKeyRegistryWsState("subscribing");
      sendSyncFrame(JSON.stringify(New_1("subscribe", newRequestId("append-page-keys-subscribe"), streamKey)));
    }
    if(!keyRegistryTailRequested){
      keyRegistryTailRequested=true;
      sendSyncFrame(JSON.stringify(New_2("read-tail", newRequestId("append-page-keys-read-tail"), streamKey, defaultCacheLimit())));
    }
  };
  ensureSelectedSubscription=() => {
    const o=selectedBucket();
    if(o==null)void 0;
    else {
      const streamKey=streamKeyFor(o.$0);
      const identity=concat_2("\n", [asText_2(streamKey.pageId), asText_2(streamKey.mode), asText_2(streamKey.setName), concat_2("\u001f", arrayOrEmpty_1(streamKey.keys))]);
      if(!isBlank_2(identity)&&identity!=subscribedValueStream){
        subscribedValueStream=identity;
        setWsState("subscribing");
        sendSyncFrame(JSON.stringify(New_1("subscribe", newRequestId("subscribe"), streamKey)));
      }
    }
  };
  repairSyncAfterClose=() => {
    setWsState("repairing");
    refreshPendingState();
    subscribeKeyRegistry();
    const m=selectedBucket();
    if(m==null)load();
    else {
      const bucket=m.$0;
      ensureSelectedSubscription();
      if(bucket.maxSequence>0n)readNewerFromBackend(loadGeneration, bucket);
      else load();
    }
  };
  const closeAddKeyEditor=() => {
    addKeyEditorOpen=false;
  };
  const cancelAddKeyEditor=() => {
    closeAddKeyEditor();
    rerenderAddKeyBuilder();
  };
  const addKeyWithKeyJson=(keyJson, displayName) => {
    if(isBlank_2(keyJson))return setStatus(status, "Key JSON is required");
    else {
      const submittedKeys=keysFromJson(keyJson);
      if(length(submittedKeys)>0)pendingSelectKeyId=appendPageKeyId(submittedKeys);
      else null;
      const request=New_13(definition.pageId, keyJson, Trim(asText_2(displayName)));
      const pendingId=rememberPending("append-page-add-key", definition.pageId, "/pages/api/add-key", request);
      refreshPendingState();
      setStatus(status, "Adding key; pending command saved in browser DB");
      return postAppendPageKey("/pages/api/add-key", request, (reply) => {
        deletePendingThen(pendingId, () => {
          let _3;
          if(!(reply.key==null)){
            const keyId=reply.key.keyId;
            if(!isBlank_2(keyId))locallyHiddenKeyIds=filter((hidden) =>!sameText(hidden, keyId), locallyHiddenKeyIds);
            pendingSelectKeyId=reply.key.keyId;
            _3=selectBucketKeys(reply.key.keys);
          }
          else _3=length(submittedKeys)>0?selectBucketKeys(submittedKeys):void 0;
          newKeyAliasInput.value="";
          closeAddKeyEditor();
          setStatus(status, "Key added");
          rerenderAddKeyBuilder();
          rerenderAppendForm();
          refreshPendingState();
          load();
        });
      }, (error) => {
        setStatus(status, pendingFailure("Add key", error));
        refreshPendingState();
      });
    }
  };
  const appendValue=() => {
    const request=New_12(definition.pageId, selectedKeyJson, Trim(valueInput.value), Trim(directionInput.value), ["web-append"]);
    if(isBlank_2(request.keyJson))setStatus(workState, "Select or add a key first");
    else if(isBlank_2(request.valueText))setStatus(workState, "Value text is required");
    else if(isActorArguPage(definition)){
      const request_1=New_14(definition.pageId, request.keyJson, request.valueText, ["web-append", "actor-argu"]);
      const m=selectedBucket();
      if(m!=null&&m.$==1){
        const bucket=m.$0;
        const o=tryHead(arrayOrEmpty_1(bucket.keys));
        const actorAddress=o==null?"":o.$0;
        if(isBlank_2(actorAddress))setStatus(workState, "Actor address key is required");
        else {
          const pendingId=rememberPending("actor-argu-send", definition.pageId, "/pages/api/actor-argu/send", request_1);
          const wsRequest=New_17("actor-argu", pendingId, definition.pageId, definition.title, definition.setName, streamKeyFor(bucket), actorAddress, request_1.rawArgu, definition.shape, ofSeq(delay(() => append_2(arrayOrEmpty_1(definition.tags), delay(() => append_2(arrayOrEmpty_1(request_1.tags), delay(() => append_2(["page:"+asText_2(definition.pageId)], delay(() => append_2(["tab:"+asText_2(definition.tabId)], delay(() =>["shape:"+asText_2(definition.shape)])))))))))), browserId, definition.tabId);
          pendingWsAppendIds=pendingWsAppendIds.concat([pendingId]);
          refreshPendingState();
          setStatus(workState, "Sending through WebSocket; pending command saved in browser DB");
          ensureSelectedSubscription();
          sendSyncFrame(JSON.stringify(wsRequest));
          scrollToBottomAfterRender(values);
        }
      }
      else setStatus(workState, "Select or add a key first");
    }
    else if(sameText(definition.shape, "raw")){
      const m_1=selectedBucket();
      if(m_1!=null&&m_1.$==1){
        const bucket_1=m_1.$0;
        const pendingId_1=rememberPending("append-page-append-value", definition.pageId, "/pages/api/append", request);
        const wsRequest_1=New_19("append", pendingId_1, streamKeyFor(bucket_1), request.valueText, "append-page.value", definition.shape, pendingId_1, ofSeq(delay(() => append_2(arrayOrEmpty_1(definition.tags), delay(() => append_2(arrayOrEmpty_1(request.tags), delay(() => append_2(["page:"+asText_2(definition.pageId)], delay(() => append_2(["tab:"+asText_2(definition.tabId)], delay(() =>["shape:"+asText_2(definition.shape)])))))))))), browserId, definition.tabId);
        pendingWsAppendIds=pendingWsAppendIds.concat([pendingId_1]);
        refreshPendingState();
        setStatus(workState, "Appending through WebSocket; pending command saved in browser DB");
        ensureSelectedSubscription();
        sendSyncFrame(JSON.stringify(wsRequest_1));
        scrollToBottomAfterRender(values);
      }
      else setStatus(workState, "Select or add a key first");
    }
    else {
      const m_2=selectedBucket();
      if(m_2!=null&&m_2.$==1){
        const bucket_2=m_2.$0;
        const pendingId_2=rememberPending("append-page-append-value", definition.pageId, "/pages/api/append", request);
        const wsRequest_2=New_18("append-page", pendingId_2, definition.pageId, definition.title, definition.setName, streamKeyFor(bucket_2), request.keyJson, request.valueText, request.direction, definition.shape, pendingId_2, ofSeq(delay(() => append_2(arrayOrEmpty_1(definition.tags), delay(() => append_2(arrayOrEmpty_1(request.tags), delay(() => append_2(["page:"+asText_2(definition.pageId)], delay(() => append_2(["tab:"+asText_2(definition.tabId)], delay(() =>["shape:"+asText_2(definition.shape)])))))))))), browserId, definition.tabId);
        pendingWsAppendIds=pendingWsAppendIds.concat([pendingId_2]);
        refreshPendingState();
        setStatus(workState, "Appending through WebSocket; pending command saved in browser DB");
        ensureSelectedSubscription();
        sendSyncFrame(JSON.stringify(wsRequest_2));
        scrollToBottomAfterRender(values);
      }
      else setStatus(workState, "Select or add a key first");
    }
  };
  rerenderAddKeyBuilder=() => {
    const baseRendererShape=isActorDynamicPage(definition)?"actor-dynamic":isActorArguPage(definition)?"actor-argu":definition.shape;
    const _3=asText_2(addKeyMode).toLowerCase();
    const rendererShape=_3=="target"?baseRendererShape=="actor-dynamic"?"actor-dynamic-target":baseRendererShape=="actor-argu"?"actor-argu-target":baseRendererShape:_3=="proxy"?baseRendererShape=="actor-dynamic"?"actor-dynamic-proxy":baseRendererShape:baseRendererShape;
    const forceFallback=sameText(addKeyMode, "actor");
    clear(addKeyRendererHost);
    const n=setData("shape", rendererShape, setData("renderer-state", "fallback", addKeyRendererHost));
    setData("mode", addKeyMode, n);
    setHidden(!addKeyEditorOpen, addKeyPanel);
    setHidden(true, fallbackAddKeyPanel);
    setHidden(true, addKeyRendererHost);
    if(sameText(addKeyMode, "actor"))newKeyInput.setAttribute("placeholder", "\"akka.tcp://system@127.0.0.1:9779/user/actor\"");
    else newKeyInput.setAttribute("placeholder", textOr("\"Aster\"", definition.keyPlaceholder));
    if(addKeyEditorOpen&&!forceFallback){
      const m=tryRenderAddKeyWithRegisteredRenderers(definition.pageId, rendererShape, definition.title, definition.setName, definition.keyPlaceholder, definition.defaultKey, (payload) => {
        const keyJson=rendererSubmittedKeyJson(payload);
        const displayName=rendererSubmittedDisplayName(payload);
        if(isBlank_2(keyJson))setStatus(status, "Renderer key is required");
        else {
          newKeyInput.value=keyJson;
          setData("last-key-json", keyJson, addKeyRendererHost);
          addKeyWithKeyJson(keyJson, displayName);
        }
      }, cancelAddKeyEditor, (payload) => {
        const keyJson=rendererSubmittedKeyJson(payload);
        const displayName=rendererSubmittedDisplayName(payload);
        if(!isBlank_2(keyJson)){
          newKeyInput.value=keyJson;
          setData("last-key-json", keyJson, addKeyRendererHost);
        }
        if(!isBlank_2(displayName))newKeyAliasInput.value=displayName;
      });
      if(m==null){
        setHidden(false, fallbackAddKeyPanel);
        addKeyRendererHost.textContent="";
      }
      else {
        const node=m.$0;
        setData("renderer-state", "custom", addKeyRendererHost);
        setHidden(false, addKeyRendererHost);
        addKeyRendererHost.appendChild(node);
      }
    }
    else addKeyEditorOpen?(setHidden(false, fallbackAddKeyPanel),addKeyRendererHost.textContent=""):setData("renderer-state", "closed", addKeyRendererHost);
  };
  rerenderAppendForm=() => {
    let effectiveKeyId;
    const rendererShape=isActorArguPage(definition)?"actor-argu":definition.shape;
    clear(form);
    const effectiveKeyJson=effectiveSelectedKeyJson();
    const m=tryFind((bucket) => bucket.keyId==selected, buckets);
    if(m==null){
      const keys=keysFromJson(selectedKeyJson);
      effectiveKeyId=length(keys)===0?"":appendPageKeyId(keys);
    }
    else effectiveKeyId=m.$0.keyId;
    const selectedKeys=effectiveSelectedKeys();
    const x=setData("selected-key-json", effectiveKeyJson, setData("selected-key-id", effectiveKeyId, setData("shape", rendererShape, setData("renderer-state", "fallback", form))));
    setData("selected-key-source", isBlank_2(effectiveKeyJson)?"none":"selected", x);
    const customNode=isBlank_2(effectiveKeyJson)?null:tryRenderAppendInputWithRegisteredRenderers(definition.pageId, rendererShape, definition.title, definition.setName, effectiveKeyId, effectiveKeyJson, selectedKeys, valueInput.placeholder, valueInput.value, (payload) => {
      let _3;
      const submitted=rendererSubmittedText(payload);
      const submittedKeyJson=rendererSubmittedKeyJson(payload);
      if(isBlank_2(submitted))setStatus(workState, "Renderer value text is required");
      else {
        if(!isBlank_2(submittedKeyJson)){
          const submittedKeys=keysFromJson(submittedKeyJson);
          _3=length(submittedKeys)>0?(selectedKeyJson=submittedKeyJson,selected=appendPageKeyId(submittedKeys),newKeyInput.value=submittedKeyJson):void 0;
        }
        else _3=void 0;
        const keyJson=effectiveSelectedKeyJson();
        const keys_1=effectiveSelectedKeys();
        if(isBlank_2(selectedKeyJson)&&!isBlank_2(keyJson)){
          selectedKeyJson=keyJson;
          newKeyInput.value=keyJson;
        }
        if(isBlank_2(selected)&&length(keys_1)>0)selected=appendPageKeyId(keys_1);
        valueInput.value=submitted;
        setData("last-raw-argu", submitted, form);
        appendValue();
      }
    }, (payload) => {
      const submitted=rendererSubmittedText(payload);
      if(!isBlank_2(submitted)){
        valueInput.value=submitted;
        setData("last-raw-argu", submitted, form);
      }
    });
    if(customNode==null)isActorArguPage(definition)?(form.className="append-form actor-argu-form",append_1(form, [valueInput, appendButton])):asText_2(definition.shape).toLowerCase()=="fcell-chat"?(form.className="append-form chat-form",append_1(form, [directionInput, valueInput, appendButton])):(form.className="append-form",append_1(form, [valueInput, appendButton]));
    else {
      const node=customNode.$0;
      form.className="append-form custom-append-input-form";
      setData("renderer-state", "custom", form);
      form.appendChild(node);
    }
  };
  rerenderAddKeyBuilder();
  rerenderAppendForm();
  replayingPending=false;
  replayPendingCommands=() => {
    if(!replayingPending){
      replayingPending=true;
      readAllPending((commands) => {
        let remaining, accepted;
        const mine=filter((command) => sameText(command.method, "POST")&&!isBlank_2(command.url)&&!isBlank_2(command.payloadJson), filter(isPendingForThisPage, commands));
        if(length(mine)===0){
          replayingPending=false;
          refreshPendingState();
        }
        else {
          remaining=length(mine);
          accepted=0;
          setStatus(pendingState, "Replaying "+String(length(mine))+" pending command(s)");
          const finishOne=() => {
            remaining=remaining-1;
            remaining===0?(replayingPending=false,refreshPendingState(),accepted>0?(setStatus(workState, "Replayed "+String(accepted)+" pending command(s)"),load()):void 0):void 0;
          };
          iter((command) => {
            postJsonText(command.url, command.payloadJson, (body) => {
              deletePendingThen(command.commandId, () => {
                let _3, _4;
                accepted=accepted+1;
                if(sameText(command.kind, "append-page-remove-page")){
                  try {
                    const reply=json(isBlank_2(body)?"{}":body);
                    _3=!(reply==null)?writeAppendPagesDefinitions(reply):null;
                  }
                  catch(m){
                    _3=null;
                  }
                  _4=globalThis.location.assign("/chat");
                }
                else _4=void 0;
                finishOne();
              });
            }, () => {
              finishOne();
            });
          }, mine);
        }
      });
    }
  };
  const openAddKeyEditor=(mode) => {
    const normalizedMode=asText_2(mode).toLowerCase();
    if(addKeyEditorOpen&&sameText(addKeyMode, normalizedMode))addKeyEditorOpen=false;
    else {
      addKeyMode=normalizedMode;
      addKeyEditorOpen=true;
    }
    actionPool.removeAttribute("open");
    rerenderAddKeyBuilder();
  };
  let _1=(addActorKeyButton.addEventListener("click", () => openAddKeyEditor("actor")),addKeyButton.addEventListener("click", () => openAddKeyEditor("target")),addProxyKeyButton.addEventListener("click", () => openAddKeyEditor("proxy")),cleanKeyButton.addEventListener("click", () => {
    newKeyInput.value="";
    newKeyAliasInput.value="";
  }),cancelKeyButton.addEventListener("click", cancelAddKeyEditor),okKeyButton.addEventListener("click", () => addKeyWithKeyJson(isBlank_2(newKeyInput.value)?asText_2(definition.defaultKey):Trim(newKeyInput.value), newKeyAliasInput.value)),removeKeyButton.addEventListener("click", () => {
    if(isBlank_2(selected))setStatus(status, "Select a key first");
    else {
      const removedKeyId=selected;
      const request=New_16(definition.pageId, removedKeyId);
      const pendingId=rememberPending("append-page-remove-key", definition.pageId, "/pages/api/remove-key", request);
      refreshPendingState();
      setStatus(status, "Removing key; pending command saved in browser DB");
      postRemoveAppendPageKey("/pages/api/remove-key", request, () => {
        deletePendingThen(pendingId, () => {
          rememberLocallyHiddenKeyId(removedKeyId);
          buckets=filter((bucket) => bucket.keyId!=removedKeyId, buckets);
          selected="";
          selectedKeyJson="";
          writeCurrentSnapshot();
          renderList();
          renderValues();
          setStatus(status, "Key removed");
          refreshPendingState();
        });
      }, (error) => {
        setStatus(status, pendingFailure("Remove key", error));
        refreshPendingState();
      });
    }
  }),removePageButton.addEventListener("click", () => {
    const request=New_15(definition.pageId);
    const pendingId=rememberPending("append-page-remove-page", definition.pageId, "/pages/api/remove-page", request);
    refreshPendingState();
    setStatus(status, "Removing page; pending command saved in browser DB");
    return postJson_2("/pages/api/remove-page", request, (reply) => {
      deletePendingThen(pendingId, () => {
        writeAppendPagesDefinitions(reply);
        setStatus(status, "Page removed");
        globalThis.location.assign("/chat");
      });
    }, (error) => {
      setStatus(status, pendingFailure("Remove page", error));
      refreshPendingState();
    });
  }),reload.addEventListener("click", load),keyFilter.addEventListener("input", load),appendButton.addEventListener("click", appendValue),load(),subscribeKeyRegistry(),refreshPendingState());
  let _2=_1;
  _2;
}
function renderNav(nav, activePath, pages){
  clear(nav);
  iter((_1) => {
    const href=_1[0];
    const label_1=_1[1];
    const x=setHref(href, element_1("a", isCurrentPage(activePath, href)?"nav-link active":"nav-link", label_1));
    let _2=setTestId_1("nav-"+label_1.toLowerCase(), x);
    nav.appendChild(_2);
  }, [["/chat", "Chat"], ["/sets", "Sets"], ["/actors", "Actors"]]);
  iter((page) => {
    const href=pagePath(page);
    const x=setHref(href, element_1("a", isCurrentPage(activePath, href)?"nav-link active":"nav-link", null));
    let _1=setTestId_1("nav-append-page-"+asText_2(page.pageId), x);
    let _2=setData("page-id", page.pageId, _1);
    const link=setData("shape", page.shape, _2);
    const x_1=element_1("span", "nav-type-badge "+pageTypeClass(page), pageTypeBadge(page));
    const badge=setTestId_1("nav-type-badge-append-page-"+asText_2(page.pageId), x_1);
    badge.setAttribute("title", pageTypeLabel(page));
    badge.setAttribute("aria-label", pageTypeLabel(page));
    const x_2=button_1("nav-close", "x");
    const closeButton=setTestId_1("nav-close-append-page-"+asText_2(page.pageId), x_2);
    closeButton.setAttribute("aria-label", "Remove page "+pageTitle(page));
    closeButton.setAttribute("title", "Remove page");
    closeButton.addEventListener("click", (event) => {
      event.preventDefault();
      event.stopPropagation();
      closeButton.setAttribute("disabled", "disabled");
      return postJson_2("/pages/api/remove-page", New_15(page.pageId), (reply) => {
        writeAppendPagesDefinitions(reply);
        isCurrentPage(activePath, href)?globalThis.location.assign("/chat"):renderNav(nav, activePath, reply.pages);
      }, (error) => {
        closeButton.removeAttribute("disabled");
        closeButton.textContent="!";
        closeButton.setAttribute("title", "Remove page failed: "+error);
      });
    });
    append_1(link, [badge, element_1("span", "nav-title", pageTitle(page)), closeButton]);
    nav.appendChild(link);
  }, arrayOrEmpty_1(pages));
}
function shell(activePath, pages){
  const app=element_1("div", "app", null);
  const top=element_1("header", "topbar", null);
  const topRow=element_1("div", "topbar-main", null);
  const brandCluster=element_1("div", "brand-cluster", null);
  const navShell=element_1("div", "nav-shell", null);
  const navViewport=setTestId_1("nav-viewport", element_1("div", "nav-viewport", null));
  const nav=setId("ptc-nav", element_1("nav", "nav", null));
  const navBack=setTestId_1("nav-scroll-left", button_1("nav-scroll", "<"));
  const navForward=setTestId_1("nav-scroll-right", button_1("nav-scroll", ">"));
  const create_1=renderPageCreator(nav, activePath, pages);
  const registryHealth=setTestId_1("append-registry-health", element_1("div", "state registry-health", "append registry ws pending"));
  const scrollTabs=(delta) => {
    navViewport.scrollLeft=navViewport.scrollLeft+delta;
  };
  navBack.setAttribute("aria-label", "Scroll tabs left");
  navForward.setAttribute("aria-label", "Scroll tabs right");
  navBack.addEventListener("click", () => scrollTabs(-260));
  navForward.addEventListener("click", () => scrollTabs(260));
  append_1(brandCluster, [element_1("div", "brand", "PTC.Comm SPA"), registryHealth]);
  renderNav(nav, activePath, pages);
  const logout=setHref("/chat/logout", element_1("a", "logout", "Logout"));
  const page=element_1("main", "page", null);
  append_1(navViewport, [nav]);
  append_1(navShell, [navBack, navViewport, navForward]);
  append_1(topRow, [brandCluster, navShell, logout]);
  append_1(top, [topRow, create_1]);
  append_1(app, [top, page]);
  return[app, page];
}
function setMain(node){
  const main=doc_1().getElementById("main");
  if(!(main==null)){
    clear(main);
    main.appendChild(node);
  }
}
function mountSets(page){
  let selected, buckets, syncSocket, queuedSyncFrames, subscribedStreams, tailRequestedStreams, registryTailRequested, ensureSetsSubscriptions, loadGeneration;
  page.className="page sets-grid";
  selected="";
  buckets=[];
  const side=element_1("aside", "sidebar", null);
  const sideHead=element_1("div", "panel-head", null);
  const reload=button_1("", "Reload");
  const filters=element_1("div", "filters", null);
  const keyFilter=input_1("key contains");
  const setFilter=input_1("set name");
  const status=element_1("div", "state", "Loading sets");
  const list=element_1("div", "list", null);
  const work=element_1("section", "work", null);
  append_1(sideHead, [element_1("h1", "", "Sets"), reload]);
  append_1(filters, [keyFilter, setFilter, status]);
  append_1(side, [sideHead, filters, list]);
  append_1(page, [side, work]);
  syncSocket=null;
  queuedSyncFrames=[];
  subscribedStreams=[];
  tailRequestedStreams=[];
  registryTailRequested=false;
  ensureSetsSubscriptions=() => { };
  loadGeneration=0;
  const sameText=(left, right) => asText_2(left).toLowerCase()==asText_2(right).toLowerCase();
  const streamIdentity=(streamKey) => concat_2("\n", [asText_2(streamKey.pageId), asText_2(streamKey.mode), asText_2(streamKey.setName), concat_2("\u001f", arrayOrEmpty_1(streamKey.keys))]);
  const setValueStreamKey=(pageId, mode, setName, keys) => New_6(asText_2(pageId), textOr("set", mode), asText_2(setName), arrayOrEmpty_1(keys));
  const currentFilterTexts=() =>[isBlank_2(keyFilter.value)?"":Trim(keyFilter.value), isBlank_2(setFilter.value)?"":Trim(setFilter.value)];
  const currentCacheKey=() => {
    const p=currentFilterTexts();
    return cacheKey("sets-state", ofArray([p[0], p[1]]));
  };
  function renderList(){
    clear(list);
    iter((bucket) => {
      const item=button_1(bucket.keyId==selected?"list-card active":"list-card", null);
      setData("key-id", bucket.keyId, setTestId_1("sets-bucket", item));
      append_1(item, [element_1("div", "strong wrap", asText_2(bucket.setName)), element_1("div", "muted wrap", joinValues(bucket.keys)), element_1("div", "meta", "values="+String(bucket.valueCount)+" seq="+String(bucket.maxSequence)+" updated="+String(asText_2(bucket.updatedAtUtc)))]);
      item.addEventListener("click", () => {
        selected=bucket.keyId;
        renderList();
        return renderDetail();
      });
      list.appendChild(item);
    }, buckets);
  }
  function renderDetail(){
    clear(work);
    const bucket=tryFind((bucket_2) => bucket_2.keyId==selected, buckets);
    if(bucket!=null&&bucket.$==1){
      const bucket_1=bucket.$0;
      const detail=element_1("div", "detail", null);
      const head_2=element_1("div", "work-head", null);
      const title=element_1("div", "", null);
      append_1(title, [element_1("label", "", "Key set"), element_1("h2", "", bucket_1.keyId)]);
      append_1(head_2, [title, element_1("div", "state", String(bucket_1.valueCount)+" value(s)")]);
      detail.appendChild(head_2);
      const table=element_1("table", "data-table", null);
      const thead=element_1("thead", "", null);
      const headerRow=element_1("tr", "", null);
      iter((label_1) => {
        headerRow.appendChild(element_1("th", "", label_1));
      }, ["Value", "Keys", "Created", "Body", "Tags"]);
      thead.appendChild(headerRow);
      const tbody=element_1("tbody", "", null);
      iter((value) => {
        const row=element_1("tr", "", null);
        iter((_1) => {
          row.appendChild(element_1("td", _1[1], _1[0]));
        }, [[value.valueId, "wrap"], [joinValues(value.keys), "wrap"], [asText_2(value.createdAtUtc), "wrap"], [asText_2(value.value), "preview"], [joinValues(value.tags), "wrap"]]);
        tbody.appendChild(row);
      }, arrayOrEmpty_1(bucket_1.values));
      append_1(table, [thead, tbody]);
      append_1(detail, [table]);
      work.appendChild(detail);
    }
    else work.appendChild(element_1("div", "empty", "No set selected."));
  }
  const applySnapshot=(source, data) => {
    buckets=arrayOrEmpty_1(data.buckets);
    (isBlank_2(selected)||!exists((bucket) => bucket.keyId==selected, buckets))&&length(buckets)>0?selected=get(buckets, 0).keyId:length(buckets)===0?selected="":void 0;
    setStatus(status, "Loaded "+String(length(buckets))+" "+String(source)+" bucket(s)");
    renderList();
    renderDetail();
    return ensureSetsSubscriptions();
  };
  const load=() => {
    loadGeneration=loadGeneration+1;
    const generation=loadGeneration;
    tailRequestedStreams=[];
    const parts=MarkResizable([]);
    const p=currentFilterTexts();
    const setText=p[1];
    const keyText=p[0];
    if(!isBlank_2(keyText))parts.push("participantId="+encodeURIComponent(keyText));
    if(!isBlank_2(setText))parts.push("setName="+encodeURIComponent(setText));
    parts.push("limit="+String(defaultRenderLimit()));
    const cacheKey_1=currentCacheKey();
    readJson(cacheKey_1, (a) => {
      if(a==null){
        if(generation===loadGeneration){
          buckets=[];
          selected="";
          renderList();
          renderDetail();
          ensureSetsSubscriptions();
        }
      }
      else if(a.$0,generation===loadGeneration)applySnapshot("cached", a.$0);
    });
    getJson("/sets/api/state?"+concat_2("&", ofSeq(parts)), (data) => {
      if(generation===loadGeneration){
        writeSnapshotWithWatermark(cacheKey_1, data, data.maxSequence, setValueCount(data.buckets), "sets-state");
        applySnapshot("backend", data);
      }
    }, (error) => {
      if(generation===loadGeneration)setStatus(status, error);
    });
  };
  const setWsState=(value) => {
    setData("ws-state", value, page);
  };
  const setWsStreamCount=() => {
    setData("ws-stream-count", String(length(subscribedStreams)), page);
  };
  function recF(recI, _1){
    while(true)
      switch(recI){
        case 0:
          const socket=ensureSyncSocket();
          return Equals(socket.readyState, 1)?socket.send(_1):void(queuedSyncFrames=queuedSyncFrames.concat([_1]));
        case 1:
          const request=New_2("read-tail", newRequestId("sets-read-tail"), _1, defaultRenderLimit());
          _1=JSON.stringify(request);
          recI=0;
          break;
        case 2:
          const identity=streamIdentity(_1);
          if(!isBlank_2(identity)&&!(((p) =>(a) => exists(p, a))(((identity_1) =>(existing) => existing==identity_1)(identity)))(tailRequestedStreams)){
            tailRequestedStreams=tailRequestedStreams.concat([identity]);
            _1=_1;
            recI=1;
          }
          else return null;
          break;
      }
  }
  function flushSyncFrames(socket){
    if(Equals(socket.readyState, 1)){
      const frames=queuedSyncFrames;
      queuedSyncFrames=[];
      iter((frame) => {
        socket.send(frame);
      }, frames);
    }
  }
  function ensureSyncSocket(){
    let _1, _2;
    if(syncSocket!=null&&syncSocket.$==1){
      const socket=syncSocket.$0;
      _1=(Equals(socket.readyState, 1)||Equals(socket.readyState, 0))&&(_2=syncSocket.$0,true);
    }
    else _1=false;
    if(_1)return _2;
    else {
      setWsState("connecting");
      const socket_1=new WebSocket(syncWebSocketUrl());
      syncSocket=Some(socket_1);
      socket_1.onopen=() => {
        setWsState("open");
        return flushSyncFrames(socket_1);
      };
      socket_1.onmessage=(event) => handleSyncMessage(String(event.data));
      socket_1.onerror=() => {
        setWsState("error");
        return setStatus(status, "WebSocket sets sync error");
      };
      socket_1.onclose=() => {
        syncSocket=null;
        subscribedStreams=[];
        tailRequestedStreams=[];
        registryTailRequested=false;
        setWsStreamCount();
        return setWsState("closed");
      };
      return socket_1;
    }
  }
  function sendSyncFrame(frame){
    return recF(0, frame);
  }
  function subscribeStream(streamKey){
    const identity=streamIdentity(streamKey);
    if(!isBlank_2(identity)&&!exists((existing) => existing==identity, subscribedStreams)){
      subscribedStreams=subscribedStreams.concat([identity]);
      setWsStreamCount();
      setWsState("subscribing");
      sendSyncFrame(JSON.stringify(New_1("subscribe", newRequestId("sets-subscribe"), streamKey)));
    }
  }
  function requestReadTail(streamKey){
    return recF(1, streamKey);
  }
  function requestReadTailOnce(streamKey){
    return recF(2, streamKey);
  }
  function handleSyncEvent(event){
    if(!(event==null)&&!(event.streamKey==null)){
      let m, updated, _1;
      const m_1=asText_2(event.sourceKind).toLowerCase();
      if(m_1=="set.stream"){
        if(event==null||isBlank_2(event.payload))m=null;
        else try {
          const wire=json(event.payload);
          m=wire==null||asText_2(wire.schema)!="ptc.comm.spa.set.stream.v1"?null:Some(setValueStreamKey(wire.pageId, wire.mode, wire.setName, wire.keys));
        }
        catch(m_3){
          m=null;
        }
        if(m==null)void 0;
        else {
          const streamKey=m.$0;
          subscribeStream(streamKey);
          requestReadTailOnce(streamKey);
        }
      }
      else if(m_1=="set"){
        if(!(event==null)&&event.sequence>0n&&!(event.streamKey==null)){
          const setName=asText_2(event.streamKey.setName);
          const keys=arrayOrEmpty_1(event.streamKey.keys);
          const p=currentFilterTexts();
          const setText=p[1];
          const keyText=p[0];
          if((isBlank_2(setText)||sameText(setName, setText))&&(isBlank_2(keyText)||exists((key) => asText_2(key).toLowerCase().indexOf(keyText.toLowerCase())!=-1, arrayOrEmpty_1(keys)))){
            const value=New_21(textOr(event.eventId, event.sourceId), arrayOrEmpty_1(event.streamKey.keys), asText_2(event.createdAtUtc), asText_2(event.payload), arrayOrEmpty_1(event.tags));
            const keyId=asText_2(setName)+"::"+concat_2(" + ", arrayOrEmpty_1(keys));
            const m_2=tryFind((bucket) => sameText(bucket.keyId, keyId), buckets);
            if(m_2==null)updated=New_20(keyId, setName, keys, 1, event.sequence, asText_2(event.createdAtUtc), [value]);
            else {
              const existing=m_2.$0;
              const existingValues=arrayOrEmpty_1(existing.values);
              const alreadyVisible=exists((row) => sameText(row.valueId, value.valueId), existingValues);
              const v=filter((row) =>!sameText(row.valueId, value.valueId), existingValues).concat([value]);
              const mergedValues=latestArray(defaultRenderLimit(), v);
              if(alreadyVisible)_1=existing.valueCount;
              else {
                const a=existing.valueCount;
                const b=length(existingValues);
                let _2=Compare(a, b)===1?a:b;
                _1=_2+1;
              }
              const a_1=existing.maxSequence;
              const b_1=event.sequence;
              let _3=Compare(a_1, b_1)===1?a_1:b_1;
              updated=New_20(existing.keyId, existing.setName, existing.keys, _1, _3, textOr(existing.updatedAtUtc, event.createdAtUtc), mergedValues);
            }
            buckets=sortBy((bucket) =>[asText_2(bucket.setName), asText_2(bucket.keyId)], arrayOrEmpty_1(filter((bucket) =>!sameText(bucket.keyId, keyId), buckets).concat([updated])));
            selected=keyId;
            renderList();
            renderDetail();
            const snapshot=New_22(fold((_4, _5) => Compare(_4, _5)===1?_4:_5, 0n, map((bucket) => bucket==null?0n:bucket.maxSequence, buckets)), buckets);
            writeSnapshotWithWatermark(currentCacheKey(), snapshot, snapshot.maxSequence, setValueCount(snapshot.buckets), "sets-state");
            ensureSetsSubscriptions();
            setStatus(status, "Synced set event "+value.valueId);
          }
          else void 0;
        }
        else void 0;
      }
      else void 0;
    }
  }
  function handleSyncMessage(text){
    try {
      const response=json(text);
      const responseType=asText_2(response.type).toLowerCase();
      const responseStatus=asText_2(response.status).toLowerCase();
      switch(responseStatus=="ok"?responseType=="subscribe"?0:responseType=="stream-event"?1:responseType=="read-tail"?2:responseType=="read"?2:responseType=="tail"?2:4:responseStatus=="error"?3:4){
        case 0:
          setData("ws-last-stream", response.streamKey, page);
          setWsState("subscribed");
          break;
        case 1:
          handleSyncEvent(response.event);
          break;
        case 2:
          iter(handleSyncEvent, arrayOrEmpty_1(response.events));
          break;
        case 3:
          setStatus(status, "WebSocket sets sync error: "+asText_2(response.error));
          break;
        case 4:
          null;
          break;
      }
    }
    catch(error){
      setStatus(status, "WebSocket sets sync parse failed: "+errorMessage_2(error));
    }
  }
  ensureSetsSubscriptions=() => {
    const registryKey=New_6("__set-registry", "set-registry", "__sets", ["__sets"]);
    subscribeStream(registryKey);
    if(!registryTailRequested){
      registryTailRequested=true;
      requestReadTail(registryKey);
    }
    iter((bucket) => {
      const streamKey=setValueStreamKey("", "set", bucket.setName, bucket.keys);
      subscribeStream(streamKey);
      requestReadTailOnce(streamKey);
    }, buckets);
  };
  reload.addEventListener("click", load);
  keyFilter.addEventListener("input", load);
  setFilter.addEventListener("input", load);
  load();
}
function mountActors(page){
  let actorSnapshot, syncSocket, queuedSyncFrames, subscribedRegistry, registryTailRequested, dynamicActorsPageAccepted;
  page.className="page actors-page";
  const head_2=element_1("div", "work-head actors-head", null);
  const title=element_1("div", "", null);
  const actions=element_1("div", "head-actions", null);
  const status=element_1("div", "state", "Loading actors");
  const reload=button_1("", "Reload");
  const nodes=element_1("div", "nodes", null);
  const treePanel=setTestId_1("actor-tree-panel", element_1("section", "actor-tree-panel", null));
  append_1(title, [element_1("label", "", "Actor / Participant Management"), element_1("h1", "", "Actors")]);
  append_1(actions, [status, reload]);
  append_1(head_2, [title, actions]);
  append_1(page, [head_2, treePanel, nodes]);
  const emptySnapshot=New_23(0, 0, 0n, []);
  actorSnapshot=emptySnapshot;
  syncSocket=null;
  queuedSyncFrames=[];
  subscribedRegistry=false;
  registryTailRequested=false;
  dynamicActorsPageAccepted=false;
  const collapsedTreeNodes=new HashSet("New_3");
  const cacheKey_1=cacheKey("actors-snapshot", FSharpList.Empty);
  const sameText=(left, right) => asText_2(left).toLowerCase()==asText_2(right).toLowerCase();
  const actorRegistryStreamKey=() => New_6("__actor-registry", "actor-registry", "__actors", ["__actors"]);
  const isAkkaAddress=(value) => {
    const text=asText_2(value).toLowerCase();
    return StartsWith(text, "akka://")||StartsWith(text, "akka.tcp://")||StartsWith(text, "akka.ssl.tcp://");
  };
  function renderActorTree(source, tree){
    clear(treePanel);
    const safeNodes=arrayOrEmpty_1(tree.nodes);
    const title_1=element_1("div", "actor-tree-title", null);
    const content=setTestId_1("actor-tree-content", element_1("div", "actor-tree-content", null));
    const treeViewport=setTestId_1("actor-tree-viewport", element_1("div", "actor-tree-viewport", null));
    const treeBody=setTestId_1("actor-tree-body", element_1("div", "actor-tree-body", null));
    const tableViewport=setTestId_1("actor-tree-table-viewport", element_1("div", "actor-tree-table-viewport", null));
    const table=setTestId_1("actor-tree-table", element_1("table", "actor-tree-table", null));
    const thead=element_1("thead", "", null);
    const tbody=element_1("tbody", "", null);
    append_1(title_1, [element_1("label", "", "ActorTree"), element_1("h2", "", String(asText_2(tree.projectionId))+" / v"+String(tree.projectionVersion)), element_1("div", "state", String(source)+"; "+String(length(safeNodes))+" node(s); "+String(arrayOrEmpty_1(tree.edges).length)+" edge(s)")]);
    const safeNodes_1=arrayOrEmpty_1(tree.nodes);
    const jsonString=(value) =>"\""+Replace(Replace(Replace(Replace(Replace(asText_2(value), "\\", "\\\\"), "\"", "\\\""), "\r", "\\r"), "\n", "\\n"), "\u0009", "\\t")+"\"";
    const jsonArray=(values) =>"["+concat_2(",", map(jsonString, arrayOrEmpty_1(values)))+"]";
    const nodesJson=concat_2(",", map((node) => {
      const tags=jsonArray(arrayOrEmpty_1(node.tags));
      return"{\"id\":"+jsonString(node.id)+","+"\"parentId\":"+jsonString(node.parentId)+","+"\"label\":"+jsonString(node.label)+","+"\"fullPath\":"+jsonString(node.fullPath)+","+"\"kind\":"+jsonString(node.kind)+","+"\"status\":"+jsonString(node.status)+","+"\"address\":"+jsonString(node.address)+","+"\"tags\":"+tags+"}";
    }, safeNodes_1));
    const rootIdsJson=jsonArray(map(asText_2, arrayOrEmpty_1(tree.rootNodeIds)));
    let _1="{\"schema\":\"fskynet-sdui\",\"version\":\"1\",\"documentId\":"+jsonString("ptcs.actors."+textOr("actor-tree", tree.projectionId))+","+"\"surface\":\"ActorsPage\","+"\"documentType\":\"ActorTopologyPage\","+"\"projectionId\":"+jsonString(tree.projectionId)+","+"\"projectionVersion\":"+String(tree.projectionVersion)+","+"\"ui\":[{\"type\":\"ActorsPage\",\"id\":\"ptcs-actors-page\",\"dataRef\":\"actorTreeNodes\",\"rootNodeIds\":"+rootIdsJson+",\"nodeIdField\":\"id\",\"parentIdField\":\"parentId\",\"labelField\":\"label\",\"statusField\":\"status\",\"columns\":[\"kind\",\"status\",\"address\",\"fullPath\"],\"groupBy\":\"actorSystemHostPort\",\"roleOrder\":[\"ptcs-host\",\"gw-host\",\"rn-host\",\"unknown\"]}],"+"\"actions\":[{\"kind\":\"reload\"},{\"kind\":\"generate-report\"},{\"kind\":\"schedule-report\"}],"+"\"data\":{\"actorTreeNodes\":["+nodesJson+"]}"+"}";
    const m=tryRenderWithRegisteredPageRenderers(_1);
    if(m==null){
      dynamicActorsPageAccepted=false;
      nodes.removeAttribute("style");
      setData("renderer", "fallback", treePanel);
      const childMap=OfArray(groupBy((node) => asText_2(node.parentId), safeNodes));
      const nodeMap=OfArray(map((node) =>[asText_2(node.id), node], safeNodes));
      function renderNode_1(depth, node){
        let toggle;
        const id=asText_2(node.id);
        const o=childMap.TryFind(id);
        let _6=o==null?[]:o.$0;
        const children=sortBy((node_1) => asText_2(node_1.label), _6);
        const hasChildren=length(children)>0;
        const row=setData("node-id", id, setTestId_1("actor-tree-row", element_1("div", "actor-tree-row", null)));
        setData("parent-id", asText_2(node.parentId), row);
        const a=12;
        const a_1=0;
        const b=Compare(a_1, depth)===1?a_1:depth;
        let _7=Compare(a, b)===-1?a:b;
        let _8=String(_7);
        setData("depth", _8, row);
        const toggleText=!hasChildren?"":collapsedTreeNodes.Contains(id)?"+":"-";
        if(hasChildren){
          const value=setTestId_1("actor-tree-toggle", button_1("actor-tree-toggle", toggleText));
          toggle=(value.setAttribute("aria-expanded", collapsedTreeNodes.Contains(id)?"false":"true"),value.setAttribute("title", collapsedTreeNodes.Contains(id)?"Expand":"Collapse"),value);
        }
        else toggle=element_1("span", "actor-tree-toggle actor-tree-toggle-placeholder", "");
        if(hasChildren)toggle.addEventListener("click", () => {
          collapsedTreeNodes.Contains(id)?collapsedTreeNodes.Remove(id):collapsedTreeNodes.SAdd(id);
          return renderActorTree("toggle", tree);
        });
        else null;
        const labelText=asText_2(node.label);
        const kindText=asText_2(node.kind);
        const statusText=asText_2(node.status);
        const fullPathText=asText_2(node.fullPath);
        const addressText=asText_2(node.address);
        const displayText=!isBlank_2(addressText)?addressText:!isBlank_2(fullPathText)?fullPathText:!isBlank_2(labelText)?labelText:id;
        const label_1=element_1("span", "actor-tree-label", displayText);
        const statusDot_1=setData("status", statusText, element_1("span", "actor-tree-status-dot", ""));
        const kindPill=element_1("span", "actor-tree-kind-pill", kindText);
        const statusPill=setData("status", statusText, element_1("span", "actor-tree-status-pill", statusText));
        label_1.setAttribute("title", displayText);
        kindPill.setAttribute("title", "kind: "+kindText);
        statusPill.setAttribute("title", "status: "+statusText);
        append_1(row, [toggle, statusDot_1, label_1, kindPill, statusPill]);
        treeBody.appendChild(row);
        if(!collapsedTreeNodes.Contains(id)){
          const _9=depth+1;
          return iter((_10) => renderNode_1(_9, _10), children);
        }
        else return null;
      }
      const roots=arrayOrEmpty_1(tree.rootNodeIds);
      let _2=length(roots)===0?map((a) => a.id, filter((node) => isBlank_2(node.parentId), safeNodes)):roots;
      let _3=choose((id) => nodeMap.TryFind(asText_2(id)), _2);
      let _4=sortBy((node) => asText_2(node.label), _3);
      iter((_6) => renderNode_1(0, _6), _4);
      const headerRow=element_1("tr", "", null);
      let _5=(iter((text) => {
        headerRow.appendChild(element_1("th", "", text));
      }, ["parentId", "id", "kind", "status", "address", "fullPath"]),thead.appendChild(headerRow),iter((node) => {
        const x=setTestId_1("actor-tree-table-row", element_1("tr", "", null));
        const row=setData("node-id", asText_2(node.id), x);
        iter((text) => {
          row.appendChild(element_1("td", "", text));
        }, [asText_2(node.parentId), asText_2(node.id), asText_2(node.kind), asText_2(node.status), asText_2(node.address), asText_2(node.fullPath)]);
        tbody.appendChild(row);
      }, sortBy((node) => asText_2(node.fullPath), safeNodes)),table.appendChild(thead),table.appendChild(tbody),treeViewport.appendChild(treeBody),tableViewport.appendChild(table),append_1(content, [treeViewport, tableViewport]),void append_1(treePanel, [title_1, content]));
      return _5;
    }
    else {
      const dynamicNode=m.$0;
      dynamicActorsPageAccepted=true;
      clear(nodes);
      nodes.setAttribute("style", "display:none;");
      const host=setTestId_1("actor-tree-dynamic-page", element_1("div", "actor-tree-dynamic-page", null));
      setData("renderer", "dynamic-actors-page", treePanel);
      host.appendChild(dynamicNode);
      treePanel.appendChild(host);
      return;
    }
  }
  const applySnapshot=(source, data) => {
    actorSnapshot=data==null?emptySnapshot:data;
    clear(nodes);
    dynamicActorsPageAccepted?nodes.setAttribute("style", "display:none;"):(nodes.removeAttribute("style"),iter((node) => {
      const block=setData("node-id", node.nodeId, setTestId_1("actor-node", element_1("section", "node-block", null)));
      const blockHead=element_1("div", "work-head", null);
      const title_1=element_1("div", "", null);
      const grid=element_1("div", "actor-grid", null);
      let _1=(append_1(title_1, [element_1("label", "", "Node"), element_1("h2", "", asText_2(node.nodeId))]),append_1(blockHead, [title_1, element_1("div", "state", asText_2(node.status)+" / "+joinValues(node.roles))]),iter((actor) => {
        const card=setData("actor-id", actor.actorId, setTestId_1("actor-card", element_1("div", "actor-card", null)));
        const line=asText_2(actor.kind)+" / "+joinValues(actor.keys);
        const routees=element_1("div", "routees", null);
        const address=TrimEnd(Trim(asText_2(node.nodeAddress)), ["/"]);
        const logicalNode=TrimEnd(Trim(asText_2(node.nodeId)), ["/"]);
        const node_1=isBlank_2(address)?logicalNode:address;
        const actor_1=Trim(asText_2(actor.actorId));
        const fullAddress=isBlank_2(actor_1)?node_1:isAkkaAddress(actor_1)?actor_1:isBlank_2(node_1)?actor_1:StartsWith(actor_1, "/")?node_1+actor_1:isAkkaAddress(node_1)?node_1+"/user/"+TrimStart(actor_1, ["/"]):node_1+"/"+TrimStart(actor_1, ["/"]);
        const addressRow=setData("actor-address", fullAddress, setTestId_1("actor-address", element_1("div", "meta wrap actor-address", "address "+fullAddress)));
        let _2=(card.appendChild(cardTitle(textOr(actor.actorId, actor.displayName), actor.actorId, actor.status, line)),card.appendChild(addressRow),iter((routee) => {
          const row=element_1("div", "routee", null);
          let _3=(append_1(row, [statusDot(routee.status), element_1("span", "strong", asText_2(routee.routeeId)), element_1("span", "muted wrap", joinValues(routee.tags))]),row);
          routees.appendChild(_3);
        }, arrayOrEmpty_1(actor.routees)),card.appendChild(routees),card);
        grid.appendChild(_2);
      }, arrayOrEmpty_1(node.actors)),append_1(block, [blockHead, grid]),block);
      nodes.appendChild(_1);
    }, arrayOrEmpty_1(actorSnapshot.nodes)));
    return setStatus(status, "Loaded "+String(actorSnapshot.nodeCount)+" "+String(source)+" node(s), "+String(actorSnapshot.actorCount)+" actor(s)");
  };
  const load=() => {
    getJson("/actors/api/snapshot", (data) => {
      writeSnapshotWithWatermark(cacheKey_1, data, data.maxSequence, actorValueCount(data), "actors-snapshot");
      applySnapshot("backend", data);
    }, (t) => {
      setStatus(status, t);
    });
    getJson("/actors/api/tree", (data) => {
      renderActorTree("backend", data);
    }, (error) => {
      clear(treePanel);
      treePanel.appendChild(element_1("div", "empty", "ActorTree unavailable: "+error));
    });
  };
  const setWsState=(value) => {
    setData("ws-state", value, page);
  };
  function flushSyncFrames(socket){
    if(Equals(socket.readyState, 1)){
      const frames=queuedSyncFrames;
      queuedSyncFrames=[];
      iter((frame) => {
        socket.send(frame);
      }, frames);
    }
  }
  function ensureSyncSocket(){
    let _1, _2;
    if(syncSocket!=null&&syncSocket.$==1){
      const socket=syncSocket.$0;
      _1=(Equals(socket.readyState, 1)||Equals(socket.readyState, 0))&&(_2=syncSocket.$0,true);
    }
    else _1=false;
    if(_1)return _2;
    else {
      setWsState("connecting");
      const socket_1=new WebSocket(syncWebSocketUrl());
      syncSocket=Some(socket_1);
      socket_1.onopen=() => {
        setWsState("open");
        return flushSyncFrames(socket_1);
      };
      socket_1.onmessage=(event) => handleSyncMessage(String(event.data));
      socket_1.onerror=() => {
        setWsState("error");
        return setStatus(status, "WebSocket actors sync error");
      };
      socket_1.onclose=() => {
        syncSocket=null;
        subscribedRegistry=false;
        registryTailRequested=false;
        return setWsState("closed");
      };
      return socket_1;
    }
  }
  function sendSyncFrame(frame){
    while(true)
      {
        const socket=ensureSyncSocket();
        return Equals(socket.readyState, 1)?socket.send(frame):void(queuedSyncFrames=queuedSyncFrames.concat([frame]));
      }
  }
  function subscribeRegistry(){
    if(!subscribedRegistry){
      subscribedRegistry=true;
      setWsState("subscribing");
      const streamKey=actorRegistryStreamKey();
      sendSyncFrame(JSON.stringify(New_1("subscribe", newRequestId("actors-subscribe"), streamKey)));
    }
  }
  function requestRegistryTail(){
    if(!registryTailRequested){
      registryTailRequested=true;
      sendSyncFrame(JSON.stringify(New_2("read-tail", newRequestId("actors-read-tail"), actorRegistryStreamKey(), defaultRenderLimit())));
    }
  }
  function handleSyncEvent(event){
    if(!(event==null)&&asText_2(event.sourceKind).toLowerCase()=="actor.registered"){
      let x, updatedNode;
      if(event==null||isBlank_2(event.payload))x=null;
      else try {
        const wire=json(event.payload);
        x=wire==null||asText_2(wire.schema)!="ptc.comm.spa.actor.registration.v1"?null:Some(wire);
      }
      catch(m_1){
        x=null;
      }
      if(x==null)void 0;
      else {
        const _1=x.$0;
        const nodeId_1=asText_2(_1.nodeId);
        const nodeAddress_1=asText_2(_1.nodeAddress);
        const actorId=asText_2(_1.actorId);
        if(!isBlank_2(nodeId_1)&&!isBlank_2(actorId)){
          const tags=arrayOrEmpty_1(_1.tags);
          const roles=arrayOrEmpty_1(_1.roles);
          const actor=New_24(actorId, textOr(actorId, _1.displayName), textOr("actor", _1.kind), [nodeId_1, actorId].concat(tags), textOr("running", _1.status), arrayOrEmpty_1(_1.routees));
          const m=tryFind((node) => sameText(node.nodeId, nodeId_1), arrayOrEmpty_1(actorSnapshot.nodes));
          if(m==null)updatedNode=New_25(nodeId_1, nodeAddress_1, "up", roles, [actor]);
          else {
            const existing=m.$0;
            const actors=sortBy((row) => asText_2(row.actorId), filter((row) =>!sameText(row.actorId, actorId), arrayOrEmpty_1(existing.actors)).concat([actor]));
            updatedNode=New_25(existing.nodeId, isBlank_2(nodeAddress_1)?asText_2(existing.nodeAddress):nodeAddress_1, textOr("up", existing.status), length(roles)===0?arrayOrEmpty_1(existing.roles):roles, actors);
          }
          const nodes_1=sortBy((node) => asText_2(node.nodeId), filter((node) =>!sameText(node.nodeId, nodeId_1), arrayOrEmpty_1(actorSnapshot.nodes)).concat([updatedNode]));
          let _2=length(nodes_1);
          let _3=fold((_5, _6) => _5+_6, 0, map((node) => arrayOrEmpty_1(node.actors).length, nodes_1));
          const a=actorSnapshot.maxSequence;
          const b=event.sequence;
          let _4=Compare(a, b)===1?a:b;
          actorSnapshot=New_23(_2, _3, _4, nodes_1);
          writeSnapshotWithWatermark(cacheKey_1, actorSnapshot, actorSnapshot.maxSequence, actorValueCount(actorSnapshot), "actors-snapshot");
          applySnapshot("synced", actorSnapshot);
          setStatus(status, "Synced actor "+actorId);
        }
        else void 0;
      }
    }
  }
  function handleSyncMessage(text){
    try {
      const response=json(text);
      const responseType=asText_2(response.type).toLowerCase();
      const responseStatus=asText_2(response.status).toLowerCase();
      switch(responseStatus=="ok"?responseType=="subscribe"?0:responseType=="stream-event"?1:responseType=="read-tail"?2:responseType=="read"?2:responseType=="tail"?2:4:responseStatus=="error"?3:4){
        case 0:
          setWsState("subscribed");
          break;
        case 1:
          handleSyncEvent(response.event);
          break;
        case 2:
          iter(handleSyncEvent, arrayOrEmpty_1(response.events));
          break;
        case 3:
          setStatus(status, "WebSocket actors sync error: "+asText_2(response.error));
          break;
        case 4:
          null;
          break;
      }
    }
    catch(error){
      setStatus(status, "WebSocket actors sync parse failed: "+errorMessage_2(error));
    }
  }
  reload.addEventListener("click", load);
  load();
  subscribeRegistry();
}
function mountChat(page){
  let selected, cursor, polling, participants, replayingPending, chatSocket, queuedChatSyncFrames, subscribedChatStream, pendingWsChatIds;
  selected="";
  cursor="";
  polling=false;
  participants=[];
  const participantId=currentUserId();
  page.className="page chat-grid";
  const side=element_1("aside", "sidebar", null);
  const sideHead=element_1("div", "panel-head", null);
  const reload=button_1("", "Reload");
  const list=element_1("div", "list", null);
  append_1(sideHead, [element_1("h1", "", "Chat"), reload]);
  append_1(side, [sideHead, element_1("div", "", null), list]);
  const work=setTestId_1("chat-work", element_1("section", "work", null));
  const workHead=element_1("div", "work-head", null);
  const titleBox=element_1("div", "", null);
  const toTitle=element_1("h2", "", "No participant selected");
  const state=element_1("div", "state", "Loading participants");
  const pendingState=setTestId_1("chat-pending-state", element_1("div", "state pending-state", ""));
  const thread=setTestId_1("thread-list", setId("thread-list", element_1("div", "thread-list", null)));
  const composer=setTestId_1("chat-composer", element_1("div", "chat-composer", null));
  const draft=setTestId_1("chat-draft", textarea("draft", "Type a message"));
  const actions=element_1("div", "actions", null);
  const send=setTestId_1("chat-send", button_1("primary", "Send"));
  const participantsCacheKey=cacheKey("chat-agents", ofArray([participantId]));
  const threadCacheKey=(peerId) => cacheKey("chat-thread", ofArray([participantId, peerId]));
  append_1(titleBox, [element_1("label", "", "To"), toTitle]);
  append_1(workHead, [titleBox, state]);
  append_1(actions, [send]);
  append_1(composer, [draft, actions]);
  append_1(work, [workHead, pendingState, thread, composer]);
  append_1(page, [side, work]);
  const sameText=(left, right) => asText_2(left).toLowerCase()==asText_2(right).toLowerCase();
  const isPendingForThisChat=(command) =>!(command==null)&&sameText(command.kind, "chat-send")&&StartsWith(asText_2(command.target), participantId+"->");
  replayingPending=false;
  chatSocket=null;
  queuedChatSyncFrames=[];
  subscribedChatStream="";
  pendingWsChatIds=[];
  const setChatWsState=(value) => {
    setData("ws-state", value, work);
  };
  const chatStreamKey=(peerId) => New_6("", "set", "chat", sameText(peerId, "channel.public")?["channel:public"]:[participantId, peerId]);
  const streamIdentity=(streamKey) => concat_2("\n", [asText_2(streamKey.pageId), asText_2(streamKey.mode), asText_2(streamKey.setName), concat_2("\u001f", arrayOrEmpty_1(streamKey.keys))]);
  function renderParticipants(){
    let _1;
    clear(list);
    iter((p_1) => {
      const className=p_1.participantId==selected?"list-card active":"list-card";
      const name=textOr(p_1.participantId, p_1.displayName);
      const line=asText_2(p_1.kind)+" / "+joinValues(p_1.labels);
      const item=button_1(className, null);
      setData("participant-id", p_1.participantId, setTestId_1("chat-participant", item));
      item.appendChild(cardTitle(name, p_1.participantId, p_1.status, line));
      item.addEventListener("click", () => {
        selected=p_1.participantId;
        cursor="";
        clear(thread);
        renderParticipants();
        refreshChatPendingState();
        pollThread(true);
        return ensureSelectedChatSubscription();
      });
      list.appendChild(item);
    }, participants);
    const current=tryFind((p_1) => p_1.participantId==selected, participants);
    if(current==null)_1="No participant selected";
    else {
      const p=current.$0;
      _1=textOr(p.participantId, p.displayName)+" ("+p.participantId+")";
    }
    toTitle.textContent=_1;
  }
  function appendMessages(messages){
    iter((message) => {
      if(!(message==null)&&!isBlank_2(message.messageId)&&doc_1().getElementById("thread-"+message.messageId)==null){
        const outbound=message.fromId==participantId;
        const wrap=setId("thread-"+message.messageId, element_1("div", outbound?"message outbound":"message inbound", null));
        setData("message-id", message.messageId, setTestId_1("chat-message", wrap));
        const meta=element_1("div", "message-meta", null);
        const route=message.scope=="public"?outbound?"You -> Public":asText_2(message.fromId)+" -> Public":outbound?"You -> "+asText_2(message.toId):asText_2(message.fromId)+" -> You";
        const idNode=setData("full-message-id", message.messageId, element_1("span", "message-id", compactMessageId(message.messageId)+"  "+asText_2(message.createdAtUtc)));
        idNode.setAttribute("title", message.messageId+"  "+asText_2(message.createdAtUtc));
        append_1(meta, [element_1("span", "", route), idNode]);
        append_1(wrap, [meta, element_1("pre", "message-body", asText_2(message.body))]);
        thread.appendChild(wrap);
      }
    }, arrayOrEmpty_1(messages));
    scrollToBottomAfterRender(thread);
  }
  function loadParticipants(){
    setStatus(state, "Loading participants");
    readJson(participantsCacheKey, (a) => {
      if(a!=null&&a.$==1)if(a.$0,length(participants)===0){
        participants=arrayOrEmpty_1(a.$0.participants);
        isBlank_2(selected)&&length(participants)>0?selected=get(participants, 0).participantId:void 0;
        renderParticipants();
        setStatus(state, "Loaded "+String(length(participants))+" cached participant(s)");
        pollThread(true);
        ensureSelectedChatSubscription();
        replayPendingChatCommands();
      }
    });
    getJson("/chat/api/agents", (data) => {
      participants=arrayOrEmpty_1(data.participants);
      writeSnapshotWithWatermark(participantsCacheKey, data, 0n, length(participants), "chat-agents");
      isBlank_2(selected)&&length(participants)>0?selected=get(participants, 0).participantId:void 0;
      renderParticipants();
      setStatus(state, "Loaded "+String(length(participants))+" participant(s)");
      pollThread(true);
      ensureSelectedChatSubscription();
      replayPendingChatCommands();
    }, (t) => {
      setStatus(state, t);
    });
  }
  function pollThread(force){
    if(!isBlank_2(selected)&&!polling){
      polling=true;
      const cacheKey_1=threadCacheKey(selected);
      const fetchThread=(useCursor) => {
        let url;
        url="/chat/api/thread?participantId="+encodeURIComponent(participantId)+"&peerId="+encodeURIComponent(selected);
        if(useCursor&&!isBlank_2(cursor))url=url+"&afterMessageId="+encodeURIComponent(cursor);
        getJson(url, (data) => {
          const messages=force&&!useCursor?latestArray(defaultRenderLimit(), data.messages):arrayOrEmpty_1(data.messages);
          appendMessages(messages);
          if(!isBlank_2(data.nextAfterMessageId))cursor=data.nextAfterMessageId;
          readJson(cacheKey_1, (cached) => {
            let _1, _2;
            switch(cached!=null&&cached.$==1?(cached.$0,useCursor?(_1=cached.$0,0):(cached.$0,!force?(_1=cached.$0,1):2)):2){
              case 0:
                _2=_1.messages;
                break;
              case 1:
                _2=_1.messages;
                break;
              case 2:
                _2=[];
                break;
            }
            const merged=mergeThreadMessages(_2, messages);
            const nextAfterMessageId=textOr(cursor, data.nextAfterMessageId);
            readWatermark(cacheKey_1, (watermark) => {
              const a=watermark==null?0n:int64OrZero(watermark.$0.newestSequence);
              const b=maxMessageSequence(merged);
              let _3=Compare(a, b)===1?a:b;
              writeSnapshotWithWatermark(cacheKey_1, New_27(merged, nextAfterMessageId), _3, length(merged), "chat-thread");
            });
          });
          setStatus(state, String(useCursor?"Synced":"Loaded")+" "+String(length(messages))+" backend message(s)");
          polling=false;
        }, (error) => {
          setStatus(state, error);
          polling=false;
        });
      };
      if(force)readJson(cacheKey_1, (a) => {
        if(a==null)fetchThread(false);
        else {
          const cached=a.$0;
          const messages=latestArray(defaultRenderLimit(), cached.messages);
          appendMessages(messages);
          if(!isBlank_2(cached.nextAfterMessageId))cursor=cached.nextAfterMessageId;
          setStatus(state, "Loaded "+String(length(messages))+" cached message(s); syncing missing tail");
          fetchThread(!isBlank_2(cursor));
        }
      });
      else fetchThread(!isBlank_2(cursor));
    }
  }
  function refreshChatPendingState(){
    readPendingRealitySplit((_1, _2) => renderPendingInspection(pendingState, filter(isPendingForThisChat, _1), filter(isPendingForThisChat, _2)));
  }
  function replayPendingChatCommands(){
    if(!replayingPending){
      replayingPending=true;
      readAllPending((commands) => {
        let remaining, accepted;
        const mine=filter((command) => sameText(command.method, "POST")&&!isBlank_2(command.url)&&!isBlank_2(command.payloadJson), filter(isPendingForThisChat, commands));
        if(length(mine)===0){
          replayingPending=false;
          refreshChatPendingState();
        }
        else {
          remaining=length(mine);
          accepted=0;
          setStatus(pendingState, "Replaying "+String(length(mine))+" pending command(s)");
          const finishOne=() => {
            remaining=remaining-1;
            remaining===0?(replayingPending=false,refreshChatPendingState(),accepted>0?(setStatus(state, "Replayed "+String(accepted)+" pending chat command(s)"),cursor="",polling=false,pollThread(true)):void 0):void 0;
          };
          iter((command) => {
            postJsonText(command.url, command.payloadJson, (responseBody) => {
              try {
                const reply=json(isBlank_2(responseBody)?"{}":responseBody);
                if(!(reply.message==null)&&!isBlank_2(reply.message.messageId))deletePendingThen(command.commandId, () => {
                  accepted=accepted+1;
                  appendMessages([reply.message]);
                  cacheAcceptedChatMessage(int64OrZero(reply.streamSequence), reply.message);
                  finishOne();
                });
                else finishOne();
              }
              catch(m){
                finishOne();
              }
            }, () => {
              finishOne();
            });
          }, mine);
        }
      });
    }
  }
  function cacheAcceptedChatMessage(sequence, message){
    if(!(message==null)&&!isBlank_2(message.messageId)&&!isBlank_2(selected)){
      const cacheKey_1=threadCacheKey(selected);
      return readJson(cacheKey_1, (cached) => {
        const merged=mergeThreadMessages(cached==null?[]:cached.$0.messages, [message]);
        writeSnapshotWithWatermark(cacheKey_1, New_27(merged, message.messageId), sequence>0n?sequence:maxMessageSequence(merged), length(merged), "chat-thread");
      });
    }
    else return null;
  }
  function handleChatSyncMessage(text){
    try {
      let o;
      const response=json(text);
      const responseType=asText_2(response.type).toLowerCase();
      const responseStatus=asText_2(response.status).toLowerCase();
      const requestId=asText_2(response.requestId);
      if(responseStatus=="ok"){
        if(responseType=="subscribe")setChatWsState("subscribed");
        else if(responseType=="chat-send"){
          exists((id) => id==requestId, pendingWsChatIds)?(pendingWsChatIds=filter((id) => id!=requestId, pendingWsChatIds),deletePendingThen(requestId, () => {
            refreshChatPendingState();
            draft.value="";
          })):void 0;
          !(response.message==null)&&!isBlank_2(response.message.messageId)?(appendMessages([response.message]),cacheAcceptedChatMessage(response.event==null?0n:response.event.sequence, response.message),cursor=response.message.messageId):void 0;
          setStatus(state, "Sent "+textOr("message", response.message==null?"":response.message.messageId)+" "+asText_2(response.deliveryHint));
        }
        else if(responseType=="stream-event"){
          const event=response.event;
          if(!isBlank_2(selected)&&!(event==null)&&!(event.streamKey==null)&&streamIdentity(event.streamKey)==streamIdentity(chatStreamKey(selected))){
            const event_1=response.event;
            if(event_1==null||isBlank_2(event_1.payload))o=null;
            else try {
              const message=json(event_1.payload);
              o=message==null||isBlank_2(message.messageId)?null:Some(message);
            }
            catch(m){
              o=Some(New_26(textOr(event_1.eventId, event_1.sourceId), "", participantId, "direct", asText_2(event_1.payload), asText_2(event_1.createdAtUtc)));
            }
            if(o==null)null;
            else {
              const message_1=o.$0;
              appendMessages([message_1]);
              cacheAcceptedChatMessage(response.event.sequence, message_1);
              cursor=message_1.messageId;
              setStatus(state, "Synced chat event "+message_1.messageId);
            }
          }
          else null;
        }
        else null;
      }
      else responseStatus=="error"?exists((id) => id==requestId, pendingWsChatIds)?(setStatus(state, pendingFailure("WebSocket chat send", asText_2(response.error))),refreshChatPendingState()):setStatus(state, "WebSocket chat error: "+asText_2(response.error)):null;
    }
    catch(error){
      setStatus(state, "WebSocket chat parse failed: "+errorMessage_2(error));
    }
  }
  function flushChatSyncFrames(socket){
    if(Equals(socket.readyState, 1)){
      const frames=queuedChatSyncFrames;
      queuedChatSyncFrames=[];
      iter((frame) => {
        socket.send(frame);
      }, frames);
    }
  }
  function ensureChatSyncSocket(){
    let _1, _2;
    if(chatSocket!=null&&chatSocket.$==1){
      const socket=chatSocket.$0;
      _1=(Equals(socket.readyState, 1)||Equals(socket.readyState, 0))&&(_2=chatSocket.$0,true);
    }
    else _1=false;
    if(_1)return _2;
    else {
      setChatWsState("connecting");
      const socket_1=new WebSocket(syncWebSocketUrl());
      chatSocket=Some(socket_1);
      socket_1.onopen=() => {
        setChatWsState("open");
        return flushChatSyncFrames(socket_1);
      };
      socket_1.onmessage=(event) => handleChatSyncMessage(String(event.data));
      socket_1.onerror=() => {
        setChatWsState("error");
        return setStatus(state, "WebSocket chat error; pending command remains replayable");
      };
      socket_1.onclose=() => {
        chatSocket=null;
        subscribedChatStream="";
        return setChatWsState("closed");
      };
      return socket_1;
    }
  }
  function sendChatSyncFrame(frame){
    while(true)
      {
        const socket=ensureChatSyncSocket();
        return Equals(socket.readyState, 1)?socket.send(frame):void(queuedChatSyncFrames=queuedChatSyncFrames.concat([frame]));
      }
  }
  function ensureSelectedChatSubscription(){
    if(!isBlank_2(selected)){
      const streamKey=chatStreamKey(selected);
      const identity=streamIdentity(streamKey);
      if(!isBlank_2(identity)&&identity!=subscribedChatStream){
        subscribedChatStream=identity;
        setChatWsState("subscribing");
        sendChatSyncFrame(JSON.stringify(New_1("subscribe", newRequestId("chat-subscribe"), streamKey)));
      }
    }
  }
  function sendMessage(){
    const body=Trim(draft.value);
    if(isBlank_2(selected))setStatus(state, "Select a participant first");
    else if(isBlank_2(body))setStatus(state, "Message is empty");
    else {
      const request=New_30(participantId, selected, body, ["web-chat"]);
      const pendingId=rememberPending("chat-send", participantId+"->"+selected, "/chat/api/send", request);
      const wsRequest=New_29("chat-send", pendingId, participantId, selected, body, ["web-chat"], participantId, "chat");
      pendingWsChatIds=pendingWsChatIds.concat([pendingId]);
      refreshChatPendingState();
      setStatus(state, "Sending through WebSocket; pending command saved in browser DB");
      sendChatSyncFrame(JSON.stringify(wsRequest));
      scrollToBottomAfterRender(thread);
    }
  }
  reload.addEventListener("click", loadParticipants);
  send.addEventListener("click", sendMessage);
  draft.addEventListener("keydown", (event) => event.key=="Enter"&&!event.shiftKey?(event.preventDefault(),sendMessage()):null);
  globalThis.setInterval(() => pollThread(false), 2500);
  refreshChatPendingState();
  loadParticipants();
}
function mountUnknownPage(page, path){
  page.className="page actors-page";
  page.appendChild(element_1("div", "empty", "No append page is registered for "+String(path)+"."));
}
function refreshAppendNav(activePath){
  const applyDefinitions=(data) => {
    const nav=doc_1().getElementById("ptc-nav");
    if(!(nav==null))renderNav(nav, activePath, arrayOrEmpty_1(data.pages));
  };
  readJson(appendPagesDefinitionsCacheKey(), (a) => {
    if(a==null){ }
    else applyDefinitions(a.$0);
  });
  getJson("/pages/api/definitions", (data) => {
    writeAppendPagesDefinitions(data);
    applyDefinitions(data);
  }, () => { });
}
function textOr(fallback, value){
  return isBlank_2(value)?fallback:value;
}
function pageDefinitionFromWire(wire){
  if(wire==null||asText_2(wire.schema)!="ptc.comm.spa.append-page.definition.v1"||isBlank_2(wire.pageId))return null;
  else {
    const pageId=asText_2(wire.pageId);
    return Some(New_5(pageId, textOr(pageId, wire.tabId), textOr("/page/"+pageId, wire.path), textOr(pageId, wire.title), textOr(pageId, wire.setName), textOr("raw", wire.shape), asText_2(wire.description), textOr("\"Aster\"", wire.keyPlaceholder), textOr("JSON value", wire.valuePlaceholder), asText_2(wire.defaultKey), arrayOrEmpty_1(wire.tags)));
  }
}
function hiddenPageFromWire(wire){
  if(wire==null||asText_2(wire.schema)!="ptc.comm.spa.append-page.hidden.v1"||isBlank_2(wire.pageId))return null;
  else {
    const pageId=asText_2(wire.pageId);
    return Some([pageId, textOr(pageId, wire.tabId)]);
  }
}
function sameTextInvariant(left, right){
  return asText_2(left).toLowerCase()==asText_2(right).toLowerCase();
}
function sortAppendPages(pages){
  return sortBy((page) =>[asText_2(page.title).toLowerCase(), asText_2(page.pageId).toLowerCase()], arrayOrEmpty_1(pages));
}
function writeSnapshotWithWatermark(cacheKey_1, value, newestSequence, cachedCount, source){
  writeJson(cacheKey_1, value);
  writeWatermark(cacheKey_1, newestSequence, cachedCount, source);
}
function set_requestSeq(_1){
  _c_1.requestSeq=_1;
}
function requestSeq(){
  return _c_1.requestSeq;
}
function errorMessage_2(error){
  return error==null?"request failed":String(error);
}
function isCurrentPage(activePath, href){
  return TrimEnd(activePath, ["/"])==TrimEnd(href, ["/"]);
}
function pagePath(page){
  const pageId=asText_2(page.pageId);
  const path=asText_2(page.path);
  return exists((alias) => sameTextInvariant(path, alias), ["/fcell-chat", "/fcell-list", "/fcell-grid"])?path:"/page/"+pageId;
}
function setTestId_1(id, node){
  !isBlank_2(id)?node.setAttribute("data-testid", id):void 0;
  return node;
}
function defaultRenderLimit(){
  return _c_1.defaultRenderLimit;
}
function element_1(tag, className, textValue){
  const node=doc_1().createElement(tag);
  if(!isBlank_2(className))node.className=className;
  if(!(textValue==null))node.textContent=textValue;
  return node;
}
function button_1(className, text){
  const node=element_1("button", className, text);
  node.setAttribute("type", "button");
  return node;
}
function input_1(placeholder){
  const node=doc_1().createElement("input");
  node.placeholder=placeholder;
  return node;
}
function textarea(className, placeholder){
  const node=doc_1().createElement("textarea");
  node.className=className;
  node.placeholder=placeholder;
  return node;
}
function append_1(parent, children){
  for(let i=0, _1=children.length-1;i<=_1;i++)parent.appendChild(get(children, i));
  return parent;
}
function actorArguButtonLabel(page){
  return isActorArguPage(page)?"Tell":"Append";
}
function pageTitle(page){
  return textOr(asText_2(page.pageId), asText_2(page.title));
}
function pageTypeLabel(page){
  const shapeText=asText_2(page.shape).toLowerCase();
  if(isActorArguPage(page)){
    if(shapeText=="fcell-chat")return"Actor Argu";
    else if(shapeText=="actor-argu")return"Actor Argu";
    else if(shapeText=="raw")return"Raw Actor Argu";
    else {
      const m=findAppendPageShape(page.shape);
      return m==null?"Actor Argu":textOr("Actor Argu", m.$0.label);
    }
  }
  else {
    const m_1=findAppendPageShape(page.shape);
    if(m_1==null)return"Raw";
    else {
      const shape=m_1.$0;
      return textOr(normalizeShapeText(page.shape), shape.label);
    }
  }
}
function isActorArguPage(page){
  return hasTag("actor-argu", page.tags);
}
function currentUserId(){
  const userNode=doc_1().getElementById("ptc-comm-user");
  if(userNode==null||isBlank_2(userNode.textContent))return"user.web";
  else {
    const user=json(userNode.textContent);
    return user==null||isBlank_2(user.participantId)?"user.web":user.participantId;
  }
}
function renderPendingInspection(node, commands, foreignCommands){
  let _1, shown, shown_1;
  const commands_1=arrayOrEmpty_1(commands);
  const foreignCommands_1=arrayOrEmpty_1(foreignCommands);
  node.setAttribute("data-pending-count", String(length(commands_1)));
  node.setAttribute("data-foreign-pending-count", String(length(foreignCommands_1)));
  node.setAttribute("data-foreign-pending-realities", concat_2(",", distinct(map((command) => asText_2(command.serverRealityId), foreignCommands_1))));
  node.setAttribute("data-pending-kinds", concat_2(",", map((a) => a.kind, commands_1)));
  node.setAttribute("data-pending-targets", concat_2("\n", map((a) => a.target, commands_1)));
  node.setAttribute("data-pending-urls", concat_2("\n", map((a) => a.url, commands_1)));
  node.setAttribute("data-pending-statuses", concat_2(",", map((a) => a.status, commands_1)));
  clear(node);
  if(length(commands_1)>0){
    node.appendChild(element_1("div", "strong", "Pending commands: "+String(length(commands_1))));
    const list=setTestId_1("pending-command-list", element_1("div", "pending-inspection-list", null));
    _1=(shown=0,iter((command) => {
      if(shown<4){
        shown=shown+1;
        const row=setData("pending-status", command.status, setData("pending-url", command.url, setData("pending-target", command.target, setData("pending-kind", command.kind, setTestId_1("pending-command-row", element_1("div", "pending-command-row wrap", null))))));
        append_1(row, [element_1("span", "strong pending-command-kind", asText_2(command.kind)), element_1("span", "muted wrap pending-command-target", asText_2(command.target)), element_1("span", "meta wrap pending-command-status", String(asText_2(command.method))+" "+String(asText_2(command.url))+" / "+String(asText_2(command.status)))]);
        list.appendChild(row);
      }
    }, commands_1),length(commands_1)>shown?list.appendChild(element_1("div", "meta", "+"+String(length(commands_1)-shown)+" more pending command(s)")):void 0,node.appendChild(list));
  }
  else _1=void 0;
  if(length(foreignCommands_1)>0){
    node.appendChild(setData("foreign-pending-count", String(length(foreignCommands_1)), setTestId_1("foreign-pending-summary", element_1("div", "pending-foreign-summary meta", "Foreign pending blocked/stale: "+String(length(foreignCommands_1))))));
    const list_1=setTestId_1("foreign-pending-list", element_1("div", "pending-foreign-list", null));
    shown_1=0;
    iter((command) => {
      if(shown_1<3){
        shown_1=shown_1+1;
        const x=setTestId_1("foreign-pending-row", element_1("div", "pending-command-row pending-command-foreign wrap", null));
        let _2=setData("pending-reality", asText_2(command.serverRealityId), x);
        let _3=setData("pending-kind", command.kind, _2);
        const row=setData("pending-target", command.target, _3);
        append_1(row, [element_1("span", "strong pending-command-kind", asText_2(command.kind)), element_1("span", "muted wrap pending-command-target", asText_2(command.target)), element_1("span", "meta wrap pending-command-status", "blocked/stale / "+asText_2(command.serverRealityId))]);
        list_1.appendChild(row);
      }
    }, foreignCommands_1);
    if(length(foreignCommands_1)>shown_1)list_1.appendChild(element_1("div", "meta", "+"+String(length(foreignCommands_1)-shown_1)+" more foreign pending command(s)"));
    node.appendChild(list_1);
  }
  else void 0;
}
function appendPageValueCount(snapshot){
  return snapshot==null?0:fold((_1, _2) => _1+_2, 0, map((bucket) => {
    if(bucket==null)return 0;
    else {
      const a=bucket.valueCount;
      const b=length(arrayOrEmpty_1(bucket.values));
      return Compare(a, b)===1?a:b;
    }
  }, arrayOrEmpty_1(snapshot.buckets)));
}
function keysAsJson(keys){
  const keys_1=arrayOrEmpty_1(keys);
  return length(keys_1)===1?JSON.stringify(get(keys_1, 0)):JSON.stringify(keys_1);
}
function joinValues(values){
  const values_1=arrayOrEmpty_1(values);
  return length(values_1)===0?"":concat_2(" / ", values_1);
}
function latestArray(limit, values){
  const values_1=arrayOrEmpty_1(values);
  return length(values_1)<=limit?values_1:skip(length(values_1)-limit, values_1);
}
function renderAppendValue(value){
  let _1;
  const mode=asText_2(value.mode);
  const m=mode.toLowerCase();
  const className=m=="inbound-message"?"fcell-card fcell-chat inbound":m=="outbound-message"?"fcell-card fcell-chat outbound":m=="list"?"fcell-card fcell-list":m=="grid"?"fcell-card fcell-grid":"fcell-card";
  const card=setData("mode", mode, setTestId_1("append-value-card", element_1("div", className, null)));
  const head_2=element_1("div", "fcell-head", null);
  append_1(head_2, [element_1("span", "fcell-pill", fcellValueModeLabel(mode, value.tags)), element_1("span", "muted wrap", asText_2(value.valueId)+" / "+asText_2(value.createdAtUtc))]);
  card.appendChild(head_2);
  const m_1=mode.toLowerCase();
  switch(m_1){
    case"outbound-message":
    case"inbound-message":
      _1=iter((row) => {
        card.appendChild(renderTextBlock("fcell-message-body", row));
      }, arrayOrEmpty_1(value.rows));
      break;
    case"list":
      const list=element_1("ul", "fcell-list-items", null);
      _1=(iter((row) => {
        list.appendChild(element_1("li", "", asText_2(row)));
      }, arrayOrEmpty_1(value.rows)),void card.appendChild(list));
      break;
    case"grid":
      let _2;
      const table=element_1("table", "fcell-grid-table", null);
      const columns=arrayOrEmpty_1(value.columns);
      if(length(columns)>0){
        const thead=element_1("thead", "", null);
        const header=element_1("tr", "", null);
        _2=(iter((column) => {
          header.appendChild(element_1("th", "wrap", asText_2(column)));
        }, columns),thead.appendChild(header),void table.appendChild(thead));
      }
      else _2=null;
      const tbody=element_1("tbody", "", null);
      _1=(iter((cells) => {
        const tr=element_1("tr", "", null);
        iter((cell) => {
          tr.appendChild(element_1("td", "wrap", asText_2(cell)));
        }, arrayOrEmpty_1(cells));
        tbody.appendChild(tr);
      }, arrayOrEmpty_1(value.tableRows)),table.appendChild(tbody),void card.appendChild(table));
      break;
    default:
      _1=void card.appendChild(renderTextBlock("fcell-source", value.rawValue));
      break;
  }
  if(!isBlank_2(value.source)&&mode.toLowerCase()!="inbound-message"&&mode.toLowerCase()!="outbound-message")card.appendChild(renderTextBlock("fcell-source", value.source));
  return card;
}
function setStatus(node, text){
  node.textContent=text;
}
function scrollToBottomAfterRender(node){
  scrollToBottomNow(node);
  setTimeout(() => {
    scrollToBottomNow(node);
  }, 0);
  setTimeout(() => {
    scrollToBottomNow(node);
  }, 50);
  setTimeout(() => {
    scrollToBottomNow(node);
  }, 150);
  setTimeout(() => {
    scrollToBottomNow(node);
  }, 300);
}
function keysFromJson(keyJson){
  let r;
  const _1=keyJson;
  if(typeof _1!=="string"||_1.trim().length===0)return[];
  try {
    let parsed=JSON.parse(_1);
    let keys=Array.isArray(parsed)?parsed:parsed==null?[]:[parsed];
    return keys.map((value) => value==null?"":String(value).trim()).filter((value) => value.length>0);
  }
  catch(_ignoreKeyJsonParse){
    return[];
  }
}
function postAppendPageKey(url, body, onOk, onError){
  const headers=new Headers();
  headers.set("Content-Type", "application/json");
  (globalThis.fetch(url, {
    method:"POST", 
    headers:headers, 
    body:JSON.stringify(body)
  }).then((response) => response.text().then((responseBody) => response.ok?onOk(json(isBlank_2(responseBody)?"{}":responseBody)):onError(isBlank_2(responseBody)?"POST "+String(url)+" "+String(response.status):responseBody))))["catch"]((error) => onError(errorMessage_2(error)));
}
function pendingFailure(action, error){
  return String(action)+" failed; pending command kept in browser DB: "+String(asText_2(error));
}
function rememberPending(kind, target, url, body){
  const payloadJson=JSON.stringify(body);
  const commandId=newPendingCommandId(kind, target, url, payloadJson);
  writePending(New_9(commandId, currentServerRealityId(), kind, target, url, "POST", payloadJson, "pending"));
  return commandId;
}
function isActorDynamicPage(page){
  return sameTextInvariant(page.shape, "actor-dynamic");
}
function setHidden(hidden, node){
  hidden?node.setAttribute("hidden", "hidden"):node.removeAttribute("hidden");
  return node;
}
function tryRenderAddKeyWithRegisteredRenderers(pageId, shape, title, setName, keyPlaceholder, defaultKey, submitKey, cancelKey, setKeyJson){
  let r;
  const _1=pageId;
  const _2=shape;
  const _3=title;
  const _4=setName;
  const _5=keyPlaceholder;
  const _6=defaultKey;
  const _7=submitKey;
  const _8=cancelKey;
  const _9=setKeyJson;
  if(!(globalThis.PulseTrade&&globalThis.PulseTrade.AddKeyRenderers))return null;
  let renderers=globalThis.PulseTrade.AddKeyRenderers;
  let context={
    pageId:String(_1||""), 
    shape:String(_2||""), 
    title:String(_3||""), 
    setName:String(_4||""), 
    keyPlaceholder:String(_5||""), 
    defaultKey:String(_6||""), 
    submitKey:(payload) => {
      _7(payload);
    }, 
    cancelKey:() => {
      _8();
    }, 
    setKeyJson:(payload) => {
      _9(payload);
    }
  };
  for(let i=0;i<renderers.length;i++){
    let r_1=renderers[i];
    try {
      let value=(r_1.render||r_1[1])(context);
      let nodeOpt=((value_1) => {
        if(value_1==null)return null;
        if(value_1.$===1)return value_1;
        if(value_1.nodeType)return{$:1, $0:value_1};
        if(value_1.element&&value_1.element.nodeType)return{$:1, $0:value_1.element};
        if(value_1.node&&value_1.node.nodeType)return{$:1, $0:value_1.node};
        return null;
      })(value);
      if(nodeOpt!=null)return nodeOpt;
    }
    catch(e){
      console.error("Add-key renderer exception:", e);
    }
  }
  return null;
}
function rendererSubmittedKeyJson(payload){
  let r;
  if(payload==null)return"";
  if(typeof payload==="string")return payload;
  if(typeof payload.keyJson==="string")return payload.keyJson;
  let keys=[];
  if(Array.isArray(payload))keys=payload;
  else if(payload&&Array.isArray(payload.keys))keys=payload.keys;
  else if(payload&&typeof payload.actorAddress==="string"){
    keys=[payload.actorAddress];
    if(typeof payload.duTypeName==="string"&&payload.duTypeName.trim().length>0)keys.push(payload.duTypeName);
    if(Array.isArray(payload.unionCaseNames))keys=keys.concat(payload.unionCaseNames);
  }
  keys=keys.map((value) => value==null?"":String(value).trim()).filter((value) => value.length>0);
  if(keys.length===0)return"";
  return JSON.stringify(keys.length===1?keys[0]:keys);
}
function rendererSubmittedDisplayName(payload){
  let r;
  if(payload==null||typeof payload==="string")return"";
  let value="";
  if(typeof payload.displayName==="string")value=payload.displayName;
  else if(typeof payload.keyAlias==="string")value=payload.keyAlias;
  else if(typeof payload.alias==="string")value=payload.alias;
  else if(typeof payload.targetAlias==="string")value=payload.targetAlias;
  value=String(value||"").trim();
  return value;
}
function tryRenderAppendInputWithRegisteredRenderers(pageId, shape, title, setName, selectedKeyId, selectedKeyJson, selectedKeys, valuePlaceholder, valueText, submit, setValue){
  let r;
  const _1=pageId;
  const _2=shape;
  const _3=title;
  const _4=setName;
  const _5=selectedKeyId;
  const _6=selectedKeyJson;
  const _7=selectedKeys;
  const _8=valuePlaceholder;
  const _9=valueText;
  const _10=submit;
  const _11=setValue;
  if(!(globalThis.PulseTrade&&globalThis.PulseTrade.AppendInputRenderers))return null;
  let renderers=globalThis.PulseTrade.AppendInputRenderers;
  let keyParts=Array.isArray(_7)?_7.slice().map(String):[];
  if(keyParts.length===0&&typeof _6==="string"&&_6.trim().length>0)try {
    let parsedKeyJson=JSON.parse(_6);
    if(Array.isArray(parsedKeyJson))keyParts=parsedKeyJson.slice().map(String);
    else if(parsedKeyJson!=null)keyParts=[String(parsedKeyJson)];
  }
  catch(_ignoreKeyJsonParse){
    keyParts=[];
  }
  let duTypeName=keyParts.length>1?String(keyParts[1]||""):"";
  if(duTypeName.indexOf("1:duType:")===0)duTypeName=duTypeName.substring("1:duType:".length);
  let unionCaseNames=keyParts.length>2?keyParts.slice(2).map(String):[];
  unionCaseNames=unionCaseNames.length===1&&unionCaseNames[0].indexOf("2:unionCases:")===0?unionCaseNames[0].substring("2:unionCases:".length).split("|").map((value_1) => String(value_1||"").trim()).filter((value_1) => value_1.length>0):unionCaseNames.map((value_1) => value_1.indexOf("2:unionCase:")===0?value_1.substring("2:unionCase:".length):value_1).map((value_1) => String(value_1||"").trim()).filter((value_1) => value_1.length>0);
  let context={
    pageId:String(_1||""), 
    shape:String(_2||""), 
    title:String(_3||""), 
    setName:String(_4||""), 
    selectedKeyId:String(_5||""), 
    selectedKeyJson:String(_6||""), 
    selectedKeys:keyParts.slice(), 
    keyParts:keyParts.slice(), 
    actorAddress:keyParts.length>0?String(keyParts[0]||""):"", 
    duTypeName:duTypeName, 
    unionCaseNames:unionCaseNames, 
    valuePlaceholder:String(_8||""), 
    valueText:String(_9||""), 
    submit:(payload) => {
      _10(payload);
    }, 
    setValue:(payload) => {
      _11(payload);
    }
  };
  for(let i=0;i<renderers.length;i++){
    let r_1=renderers[i];
    try {
      let value=(r_1.render||r_1[1])(context);
      let nodeOpt=((value_1) => {
        if(value_1==null)return null;
        if(value_1.$===1)return value_1;
        if(value_1.nodeType)return{$:1, $0:value_1};
        if(value_1.element&&value_1.element.nodeType)return{$:1, $0:value_1.element};
        if(value_1.node&&value_1.node.nodeType)return{$:1, $0:value_1.node};
        return null;
      })(value);
      if(nodeOpt!=null)return nodeOpt;
    }
    catch(e){
      console.error("Append input renderer exception:", e);
    }
  }
  return null;
}
function rendererSubmittedText(payload){
  let r;
  if(payload==null)return"";
  if(typeof payload==="string")return payload;
  if(typeof payload.rawArgu==="string")return payload.rawArgu;
  if(typeof payload.valueText==="string")return payload.valueText;
  if(typeof payload.argu==="string")return payload.argu;
  if(typeof payload.commandLine==="string")return payload.commandLine;
  return String(payload);
}
function postJsonText(url, payloadJson, onOk, onError){
  const headers=new Headers();
  headers.set("Content-Type", "application/json");
  (globalThis.fetch(url, {
    method:"POST", 
    headers:headers, 
    body:textOr("{}", payloadJson)
  }).then((response) => response.text().then((responseBody) => response.ok?onOk(responseBody):onError(isBlank_2(responseBody)?"POST "+String(url)+" "+String(response.status):responseBody))))["catch"]((error) => onError(errorMessage_2(error)));
}
function postJson_2(url, body, onOk, onError){
  const headers=new Headers();
  headers.set("Content-Type", "application/json");
  (globalThis.fetch(url, {
    method:"POST", 
    headers:headers, 
    body:JSON.stringify(body)
  }).then((response) => response.text().then((responseBody) => response.ok?onOk(json(isBlank_2(responseBody)?"{}":responseBody)):onError(isBlank_2(responseBody)?"POST "+String(url)+" "+String(response.status):responseBody))))["catch"]((error) => onError(errorMessage_2(error)));
}
function postRemoveAppendPageKey(url, body, onOk, onError){
  const headers=new Headers();
  headers.set("Content-Type", "application/json");
  (globalThis.fetch(url, {
    method:"POST", 
    headers:headers, 
    body:JSON.stringify(body)
  }).then((response) => response.text().then((responseBody) => response.ok?onOk(json(isBlank_2(responseBody)?"{}":responseBody)):onError(isBlank_2(responseBody)?"POST "+String(url)+" "+String(response.status):responseBody))))["catch"]((error) => onError(errorMessage_2(error)));
}
function setHref(href, node){
  node.setAttribute("href", href);
  return node;
}
function pageTypeClass(page){
  const shapeText=asText_2(page.shape).toLowerCase();
  if(isActorArguPage(page)){
    if(shapeText=="fcell-chat")return"actor-argu";
    else if(shapeText=="actor-argu")return"actor-argu";
    else if(shapeText=="raw")return"raw actor-argu";
    else {
      const m=findAppendPageShape(page.shape);
      if(m==null)return"actor-argu";
      else {
        const shape=m.$0;
        return textOr(normalizeShapeText(page.shape), shape.className);
      }
    }
  }
  else {
    const m_1=findAppendPageShape(page.shape);
    if(m_1==null)return"raw";
    else {
      const shape_1=m_1.$0;
      return textOr(normalizeShapeText(page.shape), shape_1.className);
    }
  }
}
function pageTypeBadge(page){
  const shapeText=asText_2(page.shape).toLowerCase();
  if(isActorArguPage(page)){
    if(shapeText=="fcell-chat")return"aa";
    else if(shapeText=="actor-argu")return"aa";
    else if(shapeText=="raw")return"ra";
    else {
      const m=findAppendPageShape(page.shape);
      return m==null?"aa":textOr("aa", m.$0.badge);
    }
  }
  else {
    const m_1=findAppendPageShape(page.shape);
    return m_1==null?"R":textOr("?", m_1.$0.badge);
  }
}
function setId(id, node){
  node.setAttribute("id", id);
  return node;
}
function renderPageCreator(nav, activePath, pages){
  let candidatePageId, candidatesLoaded, replayingPendingPageRegistration;
  const wrap=setTestId_1("page-create", element_1("div", "page-create", null));
  const shape=setTestId_1("page-create-shape", select(appendPageShapeOptions()));
  const pageId=setTestId_1("page-create-id", input_1("page id"));
  const title=setTestId_1("page-create-title", input_1("title"));
  const binding=setTestId_1("page-create-binding", select([]));
  const add=setTestId_1("page-create-submit", button_1("", "+ Page"));
  const status=setTestId_1("page-create-status", element_1("span", "state page-create-status", ""));
  candidatePageId="";
  candidatesLoaded=false;
  const sameText=(left, right) => asText_2(left).toLowerCase()==asText_2(right).toLowerCase();
  const appendOption=(value, label_1, target) => {
    const option=doc_1().createElement("option");
    option.setAttribute("value", value);
    option.textContent=label_1;
    target.appendChild(option);
  };
  const resetBinding=() => {
    clear(binding);
    appendOption("", "Default", binding);
    binding.value="";
    binding.setAttribute("data-candidate-count", "0");
    candidatesLoaded=false;
    candidatePageId="";
  };
  const refresh=(pages_1) => {
    renderNav(nav, activePath, arrayOrEmpty_1(pages_1));
  };
  const loadCandidates=(pageIdText, onDone) => {
    if(isBlank_2(pageIdText)){
      resetBinding();
      return onDone();
    }
    else {
      const normalizedInput=Trim(pageIdText);
      return candidatesLoaded&&candidatePageId.toLowerCase()==normalizedInput.toLowerCase()?onDone():(setStatus(status, "Checking history"),getJson("/pages/api/tab-candidates?pageId="+encodeURIComponent(normalizedInput), (reply) => {
        const candidates=arrayOrEmpty_1(reply.candidates);
        clear(binding);
        if(length(candidates)===0){
          appendOption("", "Default", binding);
          binding.value="";
          setStatus(status, "Ready");
        }
        else {
          iter((candidate) => {
            appendOption("reuse:"+asText_2(candidate.tabId), "Reuse "+textOr(asText_2(candidate.pageId), asText_2(candidate.tabId))+" ("+(candidate.visible?"visible":"hidden")+")", binding);
          }, candidates);
          appendOption("new", "New history", binding);
          binding.value="reuse:"+asText_2(get(candidates, 0).tabId);
          setStatus(status, "Existing history found");
        }
        candidatePageId=normalizedInput;
        candidatesLoaded=true;
        binding.setAttribute("data-candidate-count", String(length(candidates)));
        onDone();
      }, (error) => {
        resetBinding();
        setStatus(status, error);
        onDone();
      }));
    }
  };
  const addPageAfterCandidates=() => {
    const pageIdText=Trim(pageId.value);
    const titleText=Trim(title.value);
    if(isBlank_2(pageIdText)&&isBlank_2(titleText))setStatus(status, "Page id or title is required");
    else {
      const bindingValue=asText_2(binding.value);
      const p=StartsWith(bindingValue, "reuse:")?[bindingValue.substring("reuse:".length), "reuse"]:bindingValue=="new"?["", "new"]:["", ""];
      const request=New_34(pageIdText, titleText, "", shape.value, p[0], p[1], "", "");
      const pendingId=rememberPending("append-page-register", textOr(titleText, pageIdText), "/pages/api/register-page", request);
      setStatus(status, "Saving");
      postJson_2("/pages/api/register-page", request, (reply) => {
        deletePendingThen(pendingId, () => {
          writeAppendPagesDefinitions(New(reply.status, length(arrayOrEmpty_1(reply.pages)), reply.maxSequence, reply.pages));
          refresh(reply.pages);
          reply.page==null?setStatus(status, "Saved"):(setStatus(status, "Saved "+pageTitle(reply.page)),globalThis.location.assign(navigationPathForCreatedPage(reply.page)));
        });
      }, (error) => {
        setStatus(status, pendingFailure("Create page", error));
      });
    }
  };
  replayingPendingPageRegistration=false;
  const addPage=() => {
    loadCandidates(Trim(pageId.value), addPageAfterCandidates);
  };
  pageId.addEventListener("keydown", (event) => event.key=="Enter"?addPage():null);
  pageId.addEventListener("input", () => {
    const pageIdText=Trim(pageId.value);
    return isBlank_2(pageIdText)?resetBinding():loadCandidates(pageIdText, () => { });
  });
  title.addEventListener("keydown", (event) => event.key=="Enter"?addPage():null);
  add.addEventListener("click", addPage);
  resetBinding();
  refresh(pages);
  append_1(wrap, [shape, pageId, title, binding, add, status]);
  if(!replayingPendingPageRegistration){
    replayingPendingPageRegistration=true;
    readAllPending((commands) => {
      let remaining, accepted;
      const mine=filter((command) =>!(command==null)&&sameText(command.kind, "append-page-register")&&sameText(command.method, "POST")&&!isBlank_2(command.url)&&!isBlank_2(command.payloadJson), commands);
      if(length(mine)===0)replayingPendingPageRegistration=false;
      else {
        remaining=length(mine);
        accepted=0;
        setStatus(status, "Replaying "+String(length(mine))+" pending page command(s)");
        const finishOne=() => {
          remaining=remaining-1;
          remaining===0?(replayingPendingPageRegistration=false,accepted>0?setStatus(status, "Replayed "+String(accepted)+" pending page command(s)"):void 0):void 0;
        };
        iter((command) => {
          postJsonText(command.url, command.payloadJson, (body) => {
            try {
              const reply=json(isBlank_2(body)?"{}":body);
              deletePendingThen(command.commandId, () => {
                accepted=accepted+1;
                !(reply==null)?(writeAppendPagesDefinitions(New(reply.status, length(arrayOrEmpty_1(reply.pages)), reply.maxSequence, reply.pages)),refresh(reply.pages)):void 0;
                finishOne();
              });
            }
            catch(error){
              setStatus(status, "Replay create page parse failed: "+errorMessage_2(error));
              finishOne();
            }
          }, (error) => {
            setStatus(status, pendingFailure("Replay create page", error));
            finishOne();
          });
        }, mine);
      }
    });
  }
  return wrap;
}
function setValueCount(buckets){
  return fold((_1, _2) => _1+_2, 0, map((bucket) => bucket==null?0:bucket.valueCount, arrayOrEmpty_1(buckets)));
}
function tryRenderWithRegisteredPageRenderers(text){
  let r;
  const content=asText_2(text);
  if(isBlank_2(content))return null;
  else {
    const _1=content;
    if(globalThis.PulseTrade){
      let rendererGroups=[];
      if(globalThis.PulseTrade.PageRenderers)rendererGroups.push(globalThis.PulseTrade.PageRenderers);
      if(globalThis.PulseTrade.MessageRenderers)rendererGroups.push(globalThis.PulseTrade.MessageRenderers);
      for(let g=0;g<rendererGroups.length;g++){
        let renderers=rendererGroups[g];
        for(let i=0;i<renderers.length;i++){
          let r_1=renderers[i];
          try {
            let value=(r_1.render||r_1[1])(_1);
            let nodeOpt=((value_1) => {
              if(value_1==null)return null;
              if(value_1.$===1)return value_1;
              if(value_1.nodeType)return{$:1, $0:value_1};
              if(value_1.element&&value_1.element.nodeType)return{$:1, $0:value_1.element};
              if(value_1.node&&value_1.node.nodeType)return{$:1, $0:value_1.node};
              return null;
            })(value);
            if(nodeOpt!=null)return nodeOpt;
          }
          catch(e){
            console.error("Page renderer exception:", e);
          }
        }
      }
    }
    return null;
  }
}
function actorValueCount(data){
  if(data==null)return 0;
  else {
    const a=data.actorCount;
    const b=data.nodeCount;
    return Compare(a, b)===1?a:b;
  }
}
function cardTitle(title, id, status, line){
  const wrap=doc_1().createDocumentFragment();
  const row=element_1("div", "name-row", null);
  append_1(row, [statusDot(status), element_1("span", "strong wrap", title)]);
  wrap.appendChild(row);
  if(!isBlank_2(id))wrap.appendChild(element_1("div", "muted wrap", id));
  if(!isBlank_2(line))wrap.appendChild(element_1("div", "meta wrap", line));
  return wrap;
}
function statusDot(status){
  const node=element_1("span", isLive(status)?"status-dot online":"status-dot offline", null);
  node.setAttribute("title", asText_2(status));
  return node;
}
function compactMessageId(value){
  const text=asText_2(value);
  return text.length<=32?text:StartsWith(text.toLowerCase(), "pending-command")?"pending-command:"+String(text.length):Substring(text, 0, 24)+"..."+text.substring(text.length-6);
}
function mergeThreadMessages(existing, incoming){
  const v=distinctMessages(arrayOrEmpty_1(existing).concat(arrayOrEmpty_1(incoming)));
  return latestArray(defaultRenderLimit(), v);
}
function maxMessageSequence(messages){
  return fold((_1, _2) => Compare(_1, _2)===1?_1:_2, 0n, map((message) => message==null?0n:tryParseSequence("msg-", message.messageId), arrayOrEmpty_1(messages)));
}
function int64OrZero(value){
  const parsed=parseInt(asText_2(value), globalThis.$radix);
  return isNaN(parsed)||parsed<0?0n:BigInt(parsed);
}
function initializeClientExtensionGlobals(){
  if(!globalThis.PulseTrade)globalThis.PulseTrade={};
  if(!globalThis.PulseTrade.MessageRenderers)globalThis.PulseTrade.MessageRenderers=[];
  if(!globalThis.PulseTrade.PageRenderers)globalThis.PulseTrade.PageRenderers=[];
  if(!globalThis.PulseTrade.AppendInputRenderers)globalThis.PulseTrade.AppendInputRenderers=[];
  if(!globalThis.PulseTrade.AddKeyRenderers)globalThis.PulseTrade.AddKeyRenderers=[];
  if(!globalThis.PulseTrade.Renderers)globalThis.PulseTrade.Renderers=globalThis.PulseTrade.MessageRenderers;
  let register=(collection, name, priority, func) => {
    if(typeof priority==="function"){
      func=priority;
      priority=0;
    }
    if(typeof func!=="function")return;
    collection.push({
      name:String(name||"unnamed"), 
      priority:Number(priority||0), 
      render:func
    });
    collection.sort((left, right) =>(right.priority||0)-(left.priority||0));
  };
  globalThis.PulseTradeRegisterRenderer=(name, priority, func) => {
    register(globalThis.PulseTrade.MessageRenderers, name, priority, func);
  };
  globalThis.PulseTradeRegisterPageRenderer=(name, priority, func) => {
    register(globalThis.PulseTrade.PageRenderers, name, priority, func);
  };
  globalThis.PulseTradeRegisterAppendInputRenderer=(name, priority, func) => {
    register(globalThis.PulseTrade.AppendInputRenderers, name, priority, func);
  };
  globalThis.PulseTradeRegisterAddKeyRenderer=(name, priority, func) => {
    register(globalThis.PulseTrade.AddKeyRenderers, name, priority, func);
  };
}
function findAppendPageShape(shape){
  const normalized=normalizeShapeText(shape);
  return tryFind((candidate) => normalizeShapeText(candidate.shape)==normalized, appendPageShapeRegistry());
}
function normalizeShapeText(value){
  const text=Trim(asText_2(value)).toLowerCase();
  return text.length>0&&text.length<=64&&forall_1((ch) => ch>="a"&&ch<="z"||ch>="0"&&ch<="9"||ch==="-"||ch==="_"||ch===".", text)?text:"raw";
}
function hasTag(tag, tags){
  return exists((value) => asText_2(value).toLowerCase()==tag, arrayOrEmpty_1(tags));
}
function fcellValueModeLabel(mode, tags){
  return hasTag("actor-argu-command", tags)?"Actor Argu Outbound":hasTag("actor-argu-reply", tags)?"Actor Argu Reply":hasTag("actor-argu-error", tags)?"Actor Argu Error":fcellModeLabel(mode);
}
function renderTextBlock(className, text){
  const m=tryRenderWithRegisteredRenderers(text);
  return m==null?element_1("pre", className, asText_2(text)):m.$0;
}
function scrollToBottomNow(node){
  if(!(node==null)){
    try {
      node.scrollTop=node.scrollHeight;
    }
    catch(m){
      null;
    }
  }
}
function newPendingCommandId(kind, target, url, payloadJson){
  set_pendingCommandSeq(pendingCommandSeq()+1);
  return cacheKey("pending-command", ofArray([kind, target, url, payloadJson, "attempt-"+String(pendingCommandSeq()), "rand-"+String(Math.floor(Math.random()*1000000000))]));
}
function select(options){
  const node=doc_1().createElement("select");
  iter((_1) => {
    const option=doc_1().createElement("option");
    option.setAttribute("value", _1[0]);
    option.textContent=_1[1];
    node.appendChild(option);
  }, options);
  return node;
}
function appendPageShapeOptions(){
  return map((shape) =>[normalizeShapeText(shape.shape), textOr(normalizeShapeText(shape.shape), shape.label)], appendPageShapeRegistry());
}
function navigationPathForCreatedPage(page){
  const pageId=asText_2(page.pageId);
  const path=asText_2(page.path);
  return exists((alias) => sameTextInvariant(path, alias), ["/fcell-chat", "/fcell-list", "/fcell-grid"])?path:"/page/"+pageId;
}
function isLive(status){
  const m=asText_2(status).toLowerCase();
  return m=="online"||(m=="running"||(m=="up"||m=="available"));
}
function distinctMessages(messages){
  let kept;
  kept=[];
  iter((message) => {
    if(!(message==null)&&!isBlank_2(message.messageId)&&!exists((row) => row.messageId==message.messageId, kept))kept=kept.concat([message]);
  }, arrayOrEmpty_1(messages));
  return kept;
}
function tryParseSequence(prefix, value){
  const text=asText_2(value);
  if(isBlank_2(text)||!StartsWith(text, prefix))return 0n;
  else try {
    return BigInt(text.substring(prefix.length));
  }
  catch(m){
    return 0n;
  }
}
function appendPageShapeRegistry(){
  return distinctBy((shape) => normalizeShapeText(shape.shape), concat([builtInAppendPageShapes(), manifestAppendPageShapes(), runtimeAppendPageShapes()]));
}
function fcellModeLabel(mode){
  const m=asText_2(mode).toLowerCase();
  return m=="inbound-message"?"FCell Chat":m=="outbound-message"?"FCell Chat":m=="list"?"FCell List":m=="table"?"FCell Grid":m=="grid"?"FCell Grid":"FCell Value";
}
function tryRenderWithRegisteredRenderers(text){
  let r;
  const content=asText_2(text);
  if(isBlank_2(content))return null;
  else {
    const local=tryPick((_2) => {
      try {
        return _2[1](content);
      }
      catch(m){
        return null;
      }
    }, registeredRenderers());
    if(local==null){
      const _1=content;
      if(globalThis.PulseTrade&&globalThis.PulseTrade.MessageRenderers){
        let renderers=globalThis.PulseTrade.MessageRenderers;
        for(let i=0;i<renderers.length;i++){
          let r_1=renderers[i];
          try {
            let value=(r_1.render||r_1[1])(_1);
            let nodeOpt=((value_1) => {
              if(value_1==null)return null;
              if(value_1.$===1)return value_1;
              if(value_1.nodeType)return{$:1, $0:value_1};
              if(value_1.element&&value_1.element.nodeType)return{$:1, $0:value_1.element};
              if(value_1.node&&value_1.node.nodeType)return{$:1, $0:value_1.node};
              return null;
            })(value);
            if(nodeOpt!=null)return nodeOpt;
          }
          catch(e){
            console.error("Renderer exception:", e);
          }
        }
      }
      return null;
    }
    else return Some(local.$0);
  }
}
function set_pendingCommandSeq(_1){
  _c_1.pendingCommandSeq=_1;
}
function pendingCommandSeq(){
  return _c_1.pendingCommandSeq;
}
function builtInAppendPageShapes(){
  return[shapeRegistration("fcell-chat", "FCell Chat", "C", "fcell-chat"), shapeRegistration("fcell-list", "FCell List", "L", "fcell-list"), shapeRegistration("fcell-grid", "FCell Grid", "G", "fcell-grid"), shapeRegistration("actor-argu", "Actor Argu", "aa", "actor-argu"), shapeRegistration("raw", "Raw", "R", "raw")];
}
function manifestAppendPageShapes(){
  return filter((shape) => shape.shape!="raw", map((shape) => shape==null?shapeRegistration("raw", "Raw", "R", "raw"):shapeRegistration(shape.shape, shape.label, shape.badge, shape.className), collect((extension) => extension==null?[]:arrayOrEmpty_1(extension.appendPageShapes), serverClientExtensions())));
}
function runtimeAppendPageShapes(){
  return _c_1.runtimeAppendPageShapes;
}
function registeredRenderers(){
  return _c_1.registeredRenderers;
}
function shapeRegistration(shape, label_1, badge, className){
  return New_33(normalizeShapeText(shape), textOr(normalizeShapeText(shape), label_1), textOr("?", badge), textOr(normalizeShapeText(shape), className));
}
function serverClientExtensions(){
  const node=doc_1().getElementById("ptc-comm-client-extensions");
  if(node==null||isBlank_2(node.textContent))return[];
  else {
    const o=tryJson(node.textContent);
    return o==null?[]:o.$0;
  }
}
function GetFieldValues(o){
  let r=[];
  let k;
  for(var k_1 in o)r.push(o[k_1]);
  return r;
}
function NewFromSeq(fields){
  let _1;
  const r={};
  const e=Get(fields);
  try {
    while(e.MoveNext())
      {
        const f=e.Current;
        r[f[0]]=f[1];
      }
    _1=void 0;
  }
  finally {
    const _2=e;
    if(typeof _2=="object"&&isIDisposable(_2))e.Dispose();
  }
  return r;
}
function Some(Value){
  return{$:1, $0:Value};
}
function TryRender(rawContent){
  globalThis.console.log(["DynamicRenderer.TryRender called with:", rawContent]);
  const idx=rawContent.indexOf("replied msg:");
  const content=idx>=0?Trim(rawContent.substring(idx+"replied msg:".length)):rawContent;
  globalThis.console.log(["Content after strip:", content]);
  const m=tryGetSchema(content);
  return m!=null&&m.$==1&&m.$0=="fskynet-sdui"?(globalThis.console.log("Schema is fskynet-sdui, rendering canvas!"),Some(createSduiCanvas(content))):(globalThis.console.log(["Schema not matched:", tryGetSchema(content)]),null);
}
function tryGetSchema(jsonStr){
  try {
    const obj=globalThis.JSON.parse(jsonStr);
    return"schema"in obj?Some(obj.schema):null;
  }
  catch(m){
    return null;
  }
}
function createSduiCanvas(jsonStr){
  let _1;
  const isExpanded=_c.Create_1(false);
  if(!Equals(globalThis.document.head, null)){
    const styleId="sdui-dynamic-styles";
    if(Equals(globalThis.document.getElementById(styleId), null)){
      const style=globalThis.document.createElement("style");
      _1=(style.setAttribute("id", styleId),style.textContent="\r\n                        @keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }\r\n                        .sdui-json-snippet {\r\n                            background: #222; color: #aaa; padding: 10px; border-radius: 4px; font-size: 0.85em;\r\n                            cursor: pointer; margin-bottom: 12px; white-space: pre-wrap; word-break: break-all;\r\n                            max-height: 80px; overflow: hidden; width: 100%; box-sizing: border-box;\r\n                        }\r\n                        .sdui-json-snippet.expanded {\n                            max-height: 400px; overflow-y: auto;\n                        }\n                    ",void globalThis.document.head.appendChild(style));
    }
    else _1=null;
  }
  else _1=null;
  const jsonSnippet=jsonStr.length>100?Substring(jsonStr, 0, 100)+"...":jsonStr;
  const isCodeExpanded=_c.Create_1(false);
  return E_1("div", [Attr.Create("class", "sdui-summary-card"), Attr.Create("style", "border: 1px solid #5bc0de; padding: 15px; border-radius: 6px; background: rgba(91, 192, 222, 0.1); margin-top: 10px; display: flex; flex-direction: column; align-items: flex-start;")], [E_1("strong", [Attr.Create("style", "display: block; margin-bottom: 5px; color: #5bc0de; font-size: 1.1em;")], [Doc.TextNode("\ud83d\udcc8 FSkynet \u52d5\u614b\u756b\u5e03 (Canvas)")]), E_1("span", [Attr.Create("class", "muted"), Attr.Create("style", "display: block; font-size: 0.9em; margin-bottom: 12px; color: #aaa;")], [Doc.TextNode("\u9ede\u64ca\u5c55\u958b\u4ee5\u986f\u793a\u5177\u5099\u6392\u5e8f\u3001\u7be9\u9078\u53ca\u4e0b\u55ae\u529f\u80fd\u7684\u4e92\u52d5\u5f0f\u7db2\u683c\u8207\u5716\u8868\u3002")]), E_1("pre", [Dynamic("class", Map((e) => e?"sdui-json-snippet expanded":"sdui-json-snippet", isCodeExpanded.View)), Attr.Create("title", "\u9ede\u64ca\u6aa2\u8996\u5b8c\u6574 JSON"), Handler("click", () =>() => isCodeExpanded.Set(!isCodeExpanded.Get()))], [Doc.TextView(Map((e) => e?jsonStr:jsonSnippet, isCodeExpanded.View))]), E_1("button", [Attr.Create("class", "btn btn-info"), Attr.Create("style", "background: #5bc0de; color: #111; font-weight: bold; border: none; padding: 6px 12px; border-radius: 4px; cursor: pointer; margin-bottom: 10px;"), Handler("click", () =>() => isExpanded.Set(!isExpanded.Get()))], [Doc.TextView(Map((e) => e?"\u6536\u5408 Canvas":"\u5c55\u958b Canvas", isExpanded.View))]), Doc.EmbedView(Map((expanded) => expanded?E_1("div", [Attr.Create("style", "position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; background: rgba(0,0,0,0.8); z-index: 9999; display: flex; flex-direction: column; padding: 40px; box-sizing: border-box;")], [E_1("div", [Attr.Create("style", "display: flex; justify-content: space-between; align-items: center; background: #1e1e1e; padding: 15px 25px; border-radius: 8px 8px 0 0; color: #fff; font-family: sans-serif; box-shadow: 0 4px 6px rgba(0,0,0,0.3);")], [E_1("h2", [Attr.Create("style", "margin: 0; font-size: 1.5rem; font-weight: normal;")], [Doc.TextNode("FSkynet SDUI Canvas")]), E_1("button", [Attr.Create("class", "btn btn-danger"), Attr.Create("style", "background: #d9534f; color: white; border: none; padding: 8px 16px; border-radius: 4px; cursor: pointer;"), Handler("click", () =>() => isExpanded.Set(false))], [Doc.TextNode("\u95dc\u9589 Canvas")])]), E_1("div", [Attr.Create("style", "flex: 1; background: #2b2b2b; padding: 30px; overflow-y: auto; border-radius: 0 0 8px 8px; color: #eee; font-family: sans-serif;")], ofSeq_1(delay(() => enumTryWith(delay(() => {
    let r, items;
    let payloadObj=JSON.parse(jsonStr);
    let sduiNode=payloadObj.ui||payloadObj.sdui;
    if(!sduiNode)items=[];
    let unwrapped=globalThis.unwrapFCell?globalThis.unwrapFCell(sduiNode):sduiNode;
    items=Array.isArray(unwrapped)?unwrapped:[unwrapped];
    const payloadObj_1=JSON.parse(jsonStr);
    return[E_1("div", [], [Doc.Concat(ofArray(map((i) => renderNode(i, payloadObj_1), items)))])];
  }), () => 1, (ex) =>[E_1("pre", [Attr.Create("style", "color: #d9534f;")], [Doc.TextNode("Error parsing SDUI Canvas: "+ex.message)])]))))]):Doc.Empty, isExpanded.View))]);
}
function E_1(name, attrs, children){
  return Doc.Element(name, attrs, children);
}
function renderNode(obj, payloadObj){
  if(Equals(typeof obj, "undefined")||Equals(obj, null))return Doc.Empty;
  else {
    const t=obj.type;
    switch(t){
      case"Heading":
        const textStr=obj.text||"";
        return E_1("h2", [Attr.Create("style", "color: #5bc0de; margin-bottom: 15px;")], [Doc.TextNode(textStr)]);
      case"Label":
        const textStr_1=obj.text||"";
        return E_1("span", [Attr.Create("style", "margin-right: 10px; color: #ccc;")], [Doc.TextNode(textStr_1)]);
      case"TextInput":
        const placeholderStr=obj.placeholder||"";
        const idStr=obj.id||"";
        return V_1("input", append_3(ofArray([Attr.Create("type", "text"), Attr.Create("placeholder", placeholderStr), Attr.Create("style", "padding: 8px; background: #333; color: white; border: 1px solid #555; border-radius: 4px; display: block; width: 100%; box-sizing: border-box; margin: 5px 0;")]), !IsNullOrEmpty(idStr)?ofArray([OnAfterRender((el) => {
          el.setAttribute("id", idStr);
        })]):FSharpList.Empty));
      case"Row":
        const childrenDocs=ofArray(map((c) => renderNode(c, payloadObj), obj.children||[]));
        return E_1("div", [Attr.Create("style", "display: flex; flex-direction: row; gap: 15px; margin-bottom: 10px; align-items: center;")], childrenDocs);
      case"Column":
        const childrenDocs_1=ofArray(map((c) => renderNode(c, payloadObj), obj.children||[]));
        return E_1("div", [Attr.Create("style", "display: flex; flex-direction: column; gap: 10px;")], childrenDocs_1);
      case"Divider":
        return V_1("hr", [Attr.Create("style", "border: 0; border-top: 1px solid #444; margin: 15px 0; width: 100%;")]);
      case"SelectBox":
      case"Dropdown":
        const isMultiple=!(!obj.multiple);
        const optionDocs=ofArray(map((opt) => E_1("option", [], [Doc.TextNode(opt)]), obj.options||[]));
        return E_1("select", append_3(ofArray([Attr.Create("style", "padding: 8px; margin: 5px 0; background: #333; color: white; border: 1px solid #555; border-radius: 4px; font-size: 1rem; display: block; width: 200px;")]), isMultiple?ofArray([OnAfterRender((el) => {
          el.setAttribute("multiple", "multiple");
        })]):FSharpList.Empty), optionDocs);
      case"DataGrid":
        let r, rows;
        const dataRefStr=obj.dataRef||"";
        const _1=dataRefStr;
        let unwrappedData=globalThis.unwrapFCell?globalThis.unwrapFCell(payloadObj.data):payloadObj.data;
        let arr=unwrappedData?unwrappedData[_1]:null;
        rows=Array.isArray(arr)?arr:[];
        return E_1("div", [Attr.Create("style", "background: #1e1e1e; border-radius: 8px; overflow: hidden; border: 1px solid #444; margin: 20px 0;")], ofSeq_1(delay(() => {
          if(length(rows)>0){
            const keys=Object.keys(get(rows, 0));
            const thead=E_1("thead", [], [E_1("tr", [Attr.Create("style", "background: #333; color: #aaa;")], ofArray(map((k) => E_1("th", [Attr.Create("style", "padding: 12px 15px; border-bottom: 1px solid #555;")], [Doc.TextNode(k)]), keys)))]);
            const tbody=E_1("tbody", [], ofArray(map((rowObj) => E_1("tr", [Attr.Create("style", "border-bottom: 1px solid #444;")], ofArray(map((k) => {
              const cellVal=String(rowObj[k]||"");
              return E_1("td", [Attr.Create("style", "padding: 12px 15px;")], [Doc.TextNode(cellVal)]);
            }, keys))), rows)));
            return[E_1("table", [Attr.Create("style", "width: 100%; border-collapse: collapse; text-align: left;")], [thead, tbody])];
          }
          else return[E_1("div", [Attr.Create("style", "padding: 20px; color: #ccc;")], [Doc.TextNode("No data found for dataRef: "+dataRefStr)])];
        })));
      case"Button":
        const btnText=obj.text||"Button";
        return E_1("button", [Attr.Create("class", "btn btn-success canvas-btn"), Attr.Create("style", "margin-top: 15px; padding: 10px 20px; font-weight: bold; background: #5cb85c; color: white; border: none; border-radius: 4px; cursor: pointer; font-size: 1rem;"), Handler("click", () =>() => globalThis.alert("Dispatcher: Sending command..."))], [Doc.TextNode(btnText)]);
      case"AppLoader":
        const textStr_2=obj.text||"Loading...";
        return E_1("div", [Attr.Create("style", "display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 20px; color: #5bc0de;")], [V_1("div", [Attr.Create("style", "border: 4px solid rgba(255,255,255,0.1); border-top: 4px solid #5bc0de; border-radius: 50%; width: 40px; height: 40px; animation: spin 1s linear infinite;")]), E_1("span", [Attr.Create("style", "margin-top: 10px;")], [Doc.TextNode(textStr_2)])]);
      case"ColorPicker":
        const defaultColor=obj.defaultColor||"#000000";
        const idStr_1=obj.id||"";
        return V_1("input", append_3(ofArray([Attr.Create("type", "color"), Attr.Create("value", defaultColor), Attr.Create("style", "padding: 0; margin: 5px 0; background: none; border: 1px solid #555; border-radius: 4px; cursor: pointer; height: 40px; width: 60px;")]), !IsNullOrEmpty(idStr_1)?ofArray([OnAfterRender((el) => {
          el.setAttribute("id", idStr_1);
        })]):FSharpList.Empty));
      case"DatePicker":
        return V_1("input", [Attr.Create("type", "date"), Attr.Create("style", "padding: 8px; margin: 5px 0; background: #333; color: white; border: 1px solid #555; border-radius: 4px; display: inline-block;")]);
      case"TimePicker":
        return V_1("input", [Attr.Create("type", "time"), Attr.Create("style", "padding: 8px; margin: 5px 0; background: #333; color: white; border: 1px solid #555; border-radius: 4px; display: inline-block;")]);
      case"Pagination":
        return E_1("div", [Attr.Create("style", "display: flex; gap: 5px; margin: 15px 0; justify-content: center;")], [E_1("button", [Attr.Create("style", "padding: 5px 10px; background: #444; color: white; border: 1px solid #555; cursor: pointer;")], [Doc.TextNode("Prev")]), E_1("button", [Attr.Create("style", "padding: 5px 10px; background: #444; color: white; border: 1px solid #555; cursor: pointer;")], [Doc.TextNode("Next")])]);
      case"AutoComplete":
        return E_1("div", [Attr.Create("style", "position: relative; display: inline-block; width: 100%; margin: 5px 0;")], [V_1("input", [Attr.Create("type", "text"), Attr.Create("placeholder", "Search..."), Attr.Create("style", "padding: 8px; background: #333; color: white; border: 1px solid #555; border-radius: 4px; display: block; width: 100%; box-sizing: border-box;")])]);
      case"Rolling":
        let r_1, items;
        const direction=obj.direction||"left";
        const _2=obj.dataRef||"";
        let unwrappedData_1=globalThis.unwrapFCell?globalThis.unwrapFCell(payloadObj.data):payloadObj.data;
        let arr_1=unwrappedData_1?unwrappedData_1[_2]:null;
        items=Array.isArray(arr_1)?arr_1.map(String):[];
        const contentText=length(items)>0?concat_2(" | ", items):"No data for Rolling.";
        return V_1("marquee", [Attr.Create("style", "padding: 10px; background: #222; color: #5bc0de; border-radius: 4px; border: 1px solid #444; margin: 10px 0;"), OnAfterRender((el) => {
          el.setAttribute("direction", direction);
          el.textContent=contentText;
        })]);
      case"Tree":
        const dataRefStr_1=obj.dataRef||"";
        return E_1("ul", [Attr.Create("style", "list-style-type: none; padding-left: 20px; color: #ccc;")], [E_1("li", [Attr.Create("style", "padding: 5px 0; cursor: pointer;")], [Doc.TextNode("Tree Node bound to: "+dataRefStr_1)])]);
      case"ContextMenu":
        return V_1("div", [Attr.Create("style", "display: none; position: absolute; background: #333; border: 1px solid #555; box-shadow: 2px 2px 5px rgba(0,0,0,0.5); z-index: 1000;")]);
      default:
        return Doc.Empty;
    }
  }
}
function V_1(name, attrs){
  return Doc.Element(name, attrs, FSharpList.Empty);
}
function FailWith(msg){
  throw new Error(msg);
}
function range(min, max_1){
  const count=1+max_1-min;
  return count<=0?[]:init_1(count, (x) => x+min);
}
function toInt(x){
  const u=toUInt(x);
  return u>2147483647?u-4294967296:u;
}
function KeyValue(kvp){
  return[kvp.K, kvp.V];
}
function toUInt(x){
  return(x<0?Math.ceil(x):Math.floor(x))>>>0;
}
function New(status, count, maxSequence, pages){
  return{
    status:status, 
    count:count, 
    maxSequence:maxSequence, 
    pages:pages
  };
}
function iter(f, arr){
  for(let i=0, _1=arr.length-1;i<=_1;i++)f(arr[i]);
}
function tryHead(arr){
  return arr.length===0?null:Some(arr[0]);
}
function concat(xs){
  return Array.prototype.concat.apply([], ofSeq(xs));
}
function distinct(l){
  return ofSeq(distinct_1(l));
}
function sort(arr){
  return map((t) => t[0], mapi((_1, _2) =>[_2, _1], arr).sort(Compare));
}
function map(f, arr){
  const r=new Array(arr.length);
  for(let i=0, _1=arr.length-1;i<=_1;i++)r[i]=f(arr[i]);
  return r;
}
function filter(f, arr){
  const r=[];
  for(let i=0, _1=arr.length-1;i<=_1;i++)if(f(arr[i]))r.push(arr[i]);
  return r;
}
function tryFind(f, arr){
  let res, i;
  res=null;
  i=0;
  while(i<arr.length&&res==null)
    {
      f(arr[i])?res=Some(arr[i]):void 0;
      i=i+1;
    }
  return res;
}
function exists(f, x){
  let e, i;
  e=false;
  i=0;
  const l=length(x);
  while(!e&&i<l)
    if(f(x[i]))e=true;
    else i=i+1;
  return e;
}
function sortBy(f, arr){
  return map((t) => t[0], mapi((_1, _2) =>[_2, [f(_2), _1]], arr).sort((_1, _2) => Compare(_1[1], _2[1])));
}
function forall2(f, x1, x2){
  let a, i;
  checkLength(x1, x2);
  a=true;
  i=0;
  const l=length(x1);
  while(a&&i<l)
    if(f(x1[i], x2[i]))i=i+1;
    else a=false;
  return a;
}
function choose(f, arr){
  const q=[];
  for(let i=0, _1=arr.length-1;i<=_1;i++){
    const m=f(arr[i]);
    if(m==null){ }
    else q.push(m.$0);
  }
  return q;
}
function distinctBy(f, a){
  return ofSeq(distinctBy_1(f, a));
}
function fold(f, zero, arr){
  let acc;
  acc=zero;
  for(let i=0, _1=arr.length-1;i<=_1;i++)acc=f(acc, arr[i]);
  return acc;
}
function ofSeq(xs){
  if(xs instanceof Array)return xs.slice();
  else if(xs instanceof FSharpList)return ofList(xs);
  else {
    const q=[];
    const o=Get(xs);
    try {
      while(o.MoveNext())
        q.push(o.Current);
      return q;
    }
    finally {
      const _1=o;
      if(typeof _1=="object"&&isIDisposable(_1))o.Dispose();
    }
  }
}
function mapi(f, arr){
  const y=new Array(arr.length);
  for(let i=0, _1=arr.length-1;i<=_1;i++)y[i]=f(i, arr[i]);
  return y;
}
function skip(i, ar){
  return i<0?nonNegative():i>ar.length?insufficient():ar.slice(i);
}
function checkLength(arr1, arr2){
  if(arr1.length!==arr2.length)FailWith("The arrays have different lengths.");
}
function ofList(xs){
  let l;
  const q=[];
  l=xs;
  while(!(l.$==0))
    {
      q.push(head_1(l));
      l=tail(l);
    }
  return q;
}
function iteri(f, arr){
  for(let i=0, _1=arr.length-1;i<=_1;i++)f(i, arr[i]);
}
function sortInPlace(arr){
  mapInPlace((t) => t[0], mapiInPlace((_1, _2) =>[_2, _1], arr).sort(Compare));
}
function take(n, ar){
  return n<0?nonNegative():n>ar.length?insufficient():ar.slice(0, n);
}
function tryPick(f, arr){
  let res, i;
  res=null;
  i=0;
  while(i<arr.length&&res==null)
    {
      const m=f(arr[i]);
      if(m!=null&&m.$==1)res=m;
      i=i+1;
    }
  return res;
}
function tryFindIndex(f, arr){
  let res, i;
  res=null;
  i=0;
  while(i<arr.length&&res==null)
    {
      f(arr[i])?res=Some(i):void 0;
      i=i+1;
    }
  return res;
}
function foldBack(f, arr, zero){
  let acc;
  acc=zero;
  const len=arr.length;
  for(let i=1, _1=len;i<=_1;i++)acc=f(arr[len-i], acc);
  return acc;
}
function collect(f, x){
  return Array.prototype.concat.apply([], map(f, x));
}
function pick(f, arr){
  const m=tryPick(f, arr);
  return m==null?FailWith("KeyNotFoundException"):m.$0;
}
function create(size, value){
  const r=new Array(size);
  for(let i=0, _1=size-1;i<=_1;i++)r[i]=value;
  return r;
}
function init(size, f){
  if(size<0)FailWith("Negative size given.");
  else null;
  const r=new Array(size);
  for(let i=0, _1=size-1;i<=_1;i++)r[i]=f(i);
  return r;
}
function forall(f, x){
  let a, i;
  a=true;
  i=0;
  const l=length(x);
  while(a&&i<l)
    if(f(x[i]))i=i+1;
    else a=false;
  return a;
}
function readJson(key, onRead){
  if(isBlank_2(key))onRead(null);
  else withStore(snapshotStore(), "readonly", (store) => {
    try {
      const request=store.get(key);
      request.onsuccess=(event) => {
        const value=eventResult(event);
        if(isMissing(value))return onRead(null);
        else try {
          const text=String(value);
          return isBlank_2(text)?onRead(null):onRead(tryJson(text));
        }
        catch(m){
          return onRead(null);
        }
      };
      request.onerror=() => onRead(null);
    }
    catch(m){
      onRead(null);
    }
  }, () => {
    onRead(null);
  });
}
function cacheKey(scope, parts){
  return currentServerRealityId()+":"+scope+":"+concat_2(":", map_2((part) => encodeURIComponent(asText_2(part)), parts));
}
function withStore(storeName, mode, onStore, onUnavailable){
  openDb((db) => {
    try {
      onStore(db.transaction([storeName], mode).objectStore(storeName));
    }
    catch(m){
      onUnavailable();
    }
  }, onUnavailable);
}
function snapshotStore(){
  return _c_1.snapshotStore;
}
function eventResult(event){
  const target=event.target;
  return isMissing(target)?null:target.result;
}
function isMissing(value){
  return value==null||Equals(typeof value, "undefined");
}
function readPendingRealitySplit(onRead){
  readAllPendingRaw((commands) => {
    const reality=currentServerRealityId();
    onRead(filter((command) =>!(command==null)&&textOr("legacy", command.serverRealityId)==reality, commands), filter((command) =>!(command==null)&&textOr("legacy", command.serverRealityId)!=reality, commands));
  });
}
function writeWatermark(streamId, newestSequence, cachedCount, source){
  if(!isBlank_2(streamId)){
    let _1=watermarkStore();
    const a=0n;
    let _2=Compare(a, newestSequence)===1?a:newestSequence;
    let _3=String(_2);
    const a_1=0;
    let _4=Compare(a_1, cachedCount)===1?a_1:cachedCount;
    let _5=New_28(streamId, _3, _4, asText_2(source), nowTicks());
    writeJsonTo(_1, streamId, _5);
    compactSnapshots();
  }
}
function readAllPending(onRead){
  readAllPendingRaw((commands) => {
    const reality=currentServerRealityId();
    onRead(filter((command) =>!(command==null)&&textOr("legacy", command.serverRealityId)==reality, commands));
  });
}
function deletePendingThen(commandId, onDeleted){
  deleteFromThen(pendingStore(), commandId, onDeleted);
}
function readWatermark(key, onRead){
  if(isBlank_2(key))onRead(null);
  else withStore(watermarkStore(), "readonly", (store) => {
    try {
      const request=store.get(key);
      request.onsuccess=(event) => {
        const value=eventResult(event);
        if(isMissing(value))return onRead(null);
        else try {
          const text=String(value);
          return isBlank_2(text)?onRead(null):onRead(tryJson(text));
        }
        catch(m){
          return onRead(null);
        }
      };
      request.onerror=() => onRead(null);
    }
    catch(m){
      onRead(null);
    }
  }, () => {
    onRead(null);
  });
}
function openDb(onReady, onUnavailable){
  try {
    const indexedDb=globalThis.indexedDB;
    if(isMissing(indexedDb))onUnavailable();
    else {
      const a=[databaseName(), databaseVersion()];
      const request=indexedDb.open.apply(indexedDb, a);
      request.onupgradeneeded=(event) => {
        const db=eventResult(event);
        return!isMissing(db)?ensureStores(db):null;
      };
      request.onsuccess=(event) => {
        const db=eventResult(event);
        return!isMissing(db)?onReady(db):onUnavailable();
      };
      request.onerror=() => onUnavailable();
    }
  }
  catch(m){
    onUnavailable();
  }
}
function writeJson(key, value){
  writeJsonTo(snapshotStore(), key, value);
}
function readAllPendingRaw(onRead){
  withStore(pendingStore(), "readonly", (store) => {
    try {
      const request=store.getAll();
      request.onsuccess=(event) => {
        const value=eventResult(event);
        if(isMissing(value))return onRead([]);
        else try {
          return onRead(choose((text) => {
            try {
              return isBlank_2(text)?null:tryJson(text);
            }
            catch(m){
              return null;
            }
          }, value));
        }
        catch(m){
          return onRead([]);
        }
      };
      request.onerror=() => onRead([]);
    }
    catch(m){
      onRead([]);
    }
  }, () => {
    onRead([]);
  });
}
function writeJsonTo(storeName, key, value){
  if(!isBlank_2(key))withStore(storeName, "readwrite", (store) => {
    try {
      const a=[JSON.stringify(value), key];
      store.put.apply(store, a);
    }
    catch(m){
      null;
    }
  }, () => { });
}
function watermarkStore(){
  return _c_1.watermarkStore;
}
function nowTicks(){
  try {
    const this_1=Date.now();
    let _1=BigInt(Math.trunc(this_1))*BigInt(1E4)+BigInt((this_1-Math.trunc(this_1))*1E4);
    return String(_1);
  }
  catch(m){
    return"0";
  }
}
function compactSnapshots(){
  readAllWatermarks((watermarks) => {
    const watermarks_1=arrayOrEmpty_1(watermarks);
    const overflow=length(watermarks_1)-maxSnapshotRecords();
    if(overflow>0)iter((watermark) => {
      deleteSnapshotAndWatermark(watermark.streamId);
    }, sortBy(watermarkTouchedAt, filter((watermark) =>!(watermark==null)&&!isBlank_2(watermark.streamId)&&!protectedSnapshotKey(watermark.streamId), watermarks_1)).slice(0, overflow));
    readAllSnapshotKeys((snapshotKeys) => {
      iter((key) => {
        deleteFrom(snapshotStore(), key);
      }, filter((key) =>!isBlank_2(key)&&!protectedSnapshotKey(key)&&!exists((watermark) =>!(watermark==null)&&watermark.streamId==key, watermarks_1), snapshotKeys));
    });
  });
}
function deleteFromThen(storeName, key, onDeleted){
  if(isBlank_2(key))onDeleted();
  else withTransactionStore(storeName, "readwrite", (_1, _2) =>(((tx) =>(store) => {
    let finished;
    finished=false;
    const finish=() => {
      if(!finished){
        finished=true;
        onDeleted();
      }
    };
    tx.oncomplete=() => finish();
    tx.onabort=() => finish();
    tx.onerror=() => finish();
    try {
      store["delete"](key);
      return;
    }
    catch(m){
      return finish();
    }
  })(_1))(_2), onDeleted);
}
function pendingStore(){
  return _c_1.pendingStore;
}
function writePending(command){
  writeJsonTo(pendingStore(), command.commandId, command);
}
function databaseName(){
  return _c_1.databaseName;
}
function databaseVersion(){
  return _c_1.databaseVersion;
}
function ensureStores(db){
  ensureStore(snapshotStore(), db);
  ensureStore(pendingStore(), db);
  ensureStore(watermarkStore(), db);
}
function readAllWatermarks(onRead){
  withStore(watermarkStore(), "readonly", (store) => {
    try {
      const request=store.getAll();
      request.onsuccess=(event) => {
        const value=eventResult(event);
        if(isMissing(value))return onRead([]);
        else try {
          return onRead(choose((text) => {
            try {
              return isBlank_2(text)?null:tryJson(text);
            }
            catch(m){
              return null;
            }
          }, value));
        }
        catch(m){
          return onRead([]);
        }
      };
      request.onerror=() => onRead([]);
    }
    catch(m){
      onRead([]);
    }
  }, () => {
    onRead([]);
  });
}
function maxSnapshotRecords(){
  return _c_1.maxSnapshotRecords;
}
function protectedSnapshotKey(key){
  const key_1=asText_2(key);
  return key_1=="append-pages-definitions:"||key_1.indexOf(":append-pages-definitions:")!=-1||StartsWith(key_1, "chat-agents:")||key_1.indexOf(":chat-agents:")!=-1||StartsWith(key_1, "actors-snapshot:")||key_1.indexOf(":actors-snapshot:")!=-1;
}
function watermarkTouchedAt(watermark){
  let o;
  if(watermark==null)return 0n;
  else {
    const m=(o=0n,[TryParse_1(asText_2(watermark.touchedAt), {get:() => o, set:(v) => {
      o=v;
    }}), o]);
    return m[0]?m[1]:0n;
  }
}
function deleteSnapshotAndWatermark(key){
  if(!isBlank_2(key))withSnapshotWatermarkStores("readwrite", (_1, _2, _3) => {
    try {
      _2["delete"](key);
      _3["delete"](key);
      return;
    }
    catch(m){
      return null;
    }
  }, () => { });
}
function readAllSnapshotKeys(onRead){
  withStore(snapshotStore(), "readonly", (store) => {
    try {
      const request=store.getAllKeys();
      request.onsuccess=(event) => {
        const value=eventResult(event);
        if(isMissing(value))return onRead([]);
        else try {
          return onRead(value);
        }
        catch(m){
          return onRead([]);
        }
      };
      request.onerror=() => onRead([]);
    }
    catch(m){
      onRead([]);
    }
  }, () => {
    onRead([]);
  });
}
function deleteFrom(storeName, key){
  if(!isBlank_2(key))withStore(storeName, "readwrite", (store) => {
    try {
      store["delete"](key);
    }
    catch(m){
      null;
    }
  }, () => { });
}
function withTransactionStore(storeName, mode, onStore, onUnavailable){
  openDb((db) => {
    try {
      const tx=db.transaction([storeName], mode);
      onStore(tx, tx.objectStore(storeName));
    }
    catch(m){
      onUnavailable();
    }
  }, onUnavailable);
}
function ensureStore(storeName, db){
  let _1;
  const names=db.objectStoreNames;
  if(isMissing(names))_1=false;
  else try {
    _1=names.contains(storeName);
  }
  catch(m){
    _1=false;
  }
  if(!_1)db.createObjectStore(storeName);
}
function withSnapshotWatermarkStores(mode, onStores, onUnavailable){
  openDb((db) => {
    try {
      const a=[[snapshotStore(), watermarkStore()], mode];
      const tx=db.transaction.apply(db, a);
      const a_1=[snapshotStore()];
      let _1=tx.objectStore.apply(tx, a_1);
      const a_2=[watermarkStore()];
      let _2=tx.objectStore.apply(tx, a_2);
      onStores(tx, _1, _2);
    }
    catch(m){
      onUnavailable();
    }
  }, onUnavailable);
}
function Equals(a, b){
  let _1;
  if(a===b)return true;
  else {
    const m=typeof a;
    if(m=="object"){
      if(a===null||a===void 0||b===null||b===void 0||!Equals(typeof b, "object"))return false;
      else if("Equals"in a)return a.Equals(b);
      else if("Equals"in b)return false;
      else if(a instanceof Array&&b instanceof Array)return arrayEquals(a, b);
      else if(a instanceof Date&&b instanceof Date)return dateEquals(a, b);
      else {
        const a_1=a;
        const b_1=b;
        const eqR=[true];
        let k;
        for(var k_2 in a_1)if(((k_3) => {
          eqR[0]=!a_1.hasOwnProperty(k_3)||b_1.hasOwnProperty(k_3)&&Equals(a_1[k_3], b_1[k_3]);
          return!eqR[0];
        })(k_2))break;
        if(eqR[0]){
          let k_1;
          for(var k_3 in b_1)if(((k_4) => {
            eqR[0]=!b_1.hasOwnProperty(k_4)||a_1.hasOwnProperty(k_4);
            return!eqR[0];
          })(k_3))break;
          _1=void 0;
        }
        else _1=null;
        return eqR[0];
      }
    }
    else return m=="function"&&("$Func"in a?a.$Func===b.$Func&&a.$Target===b.$Target:"$Invokes"in a&&"$Invokes"in b&&arrayEquals(a.$Invokes, b.$Invokes));
  }
}
function Compare(a, b){
  if(a===b)return 0;
  else {
    const m=typeof a;
    switch(m=="boolean"?1:m=="number"?1:m=="bigint"?1:m=="string"?1:m=="object"?2:m=="function"?3:m=="symbol"?4:0){
      case 0:
        return typeof b=="undefined"?0:-1;
      case 1:
        return a<b?-1:1;
      case 2:
        let _1;
        if(a===null)return -1;
        else if(b===null)return 1;
        else if("CompareTo"in a)return a.CompareTo(b);
        else if("CompareTo0"in a)return a.CompareTo0(b);
        else if(a instanceof Array&&b instanceof Array)return compareArrays(a, b);
        else if(a instanceof Date&&b instanceof Date)return compareDates(a, b);
        else {
          const a_1=a;
          const b_1=b;
          const cmp=[0];
          let k;
          for(var k_2 in a_1)if(((k_3) =>!a_1.hasOwnProperty(k_3)?false:!b_1.hasOwnProperty(k_3)?(cmp[0]=1,true):(cmp[0]=Compare(a_1[k_3], b_1[k_3]),cmp[0]!==0))(k_2))break;
          if(cmp[0]===0){
            let k_1;
            for(var k_3 in b_1)if(((k_4) =>!b_1.hasOwnProperty(k_4)?false:!a_1.hasOwnProperty(k_4)&&(cmp[0]=-1,true))(k_3))break;
            _1=void 0;
          }
          else _1=null;
          return cmp[0];
        }
        break;
      case 3:
        return FailWith("Cannot compare function values.");
      case 4:
        return FailWith("Cannot compare symbol values.");
    }
  }
}
function arrayEquals(a, b){
  let eq, i;
  if(length(a)===length(b)){
    eq=true;
    i=0;
    while(eq&&i<length(a))
      {
        !Equals(get(a, i), get(b, i))?eq=false:void 0;
        i=i+1;
      }
    return eq;
  }
  else return false;
}
function dateEquals(a, b){
  return a.getTime()===b.getTime();
}
function compareArrays(a, b){
  let cmp, i;
  if(length(a)<length(b))return -1;
  else if(length(a)>length(b))return 1;
  else {
    cmp=0;
    i=0;
    while(cmp===0&&i<length(a))
      {
        cmp=Compare(get(a, i), get(b, i));
        i=i+1;
      }
    return cmp;
  }
}
function compareDates(a, b){
  return Compare(a.getTime(), b.getTime());
}
function Hash(o){
  const m=typeof o;
  return m=="function"?0:m=="boolean"?o?1:0:m=="number"?o:m=="string"?hashString(o):m=="object"?o==null?0:o instanceof Array?hashArray(o):hashObject(o):m=="bigint"?hashString(String(o)):m=="symbol"?hashString(o.description):0;
}
function hashString(s){
  let hash;
  if(s===null)return 0;
  else {
    hash=5381;
    for(let i=0, _1=s.length-1;i<=_1;i++)hash=hashMix(hash, s[i].charCodeAt());
    return hash;
  }
}
function hashArray(o){
  let h;
  h=-34948909;
  for(let i=0, _1=length(o)-1;i<=_1;i++)h=hashMix(h, Hash(get(o, i)));
  return h;
}
function hashObject(o){
  if("GetHashCode"in o)return o.GetHashCode();
  else {
    const ____=hashMix;
    const h=[0];
    let k;
    for(var k_1 in o)if(((key) => {
      h[0]=____(____(h[0], hashString(key)), Hash(o[key]));
      return false;
    })(k_1))break;
    return h[0];
  }
}
function hashMix(x, y){
  return(x<<5)+x+y;
}
function json(text){
  return JSON.parse(asText_2(text));
}
function tryJson(text){
  try {
    return isBlank_2(text)?null:Some(json(text));
  }
  catch(m){
    return null;
  }
}
function New_1(type, requestId, streamKey){
  return{
    type:type, 
    requestId:requestId, 
    streamKey:streamKey
  };
}
function New_2(type, requestId, streamKey, count){
  return{
    type:type, 
    requestId:requestId, 
    streamKey:streamKey, 
    count:count
  };
}
function get(arr, n){
  checkBounds(arr, n);
  return arr[n];
}
function length(arr){
  return arr.dims===2?arr.length*arr.length:arr.length;
}
function checkBounds(arr, n){
  if(n<0||n>=arr.length)FailWith("Index was outside the bounds of the array.");
}
function set(arr, n, x){
  checkBounds(arr, n);
  arr[n]=x;
}
function New_3(keys, displayName){
  return{keys:keys, displayName:displayName};
}
function delay(f){
  return{GetEnumerator:() => Get(f())};
}
function append_2(s1, s2){
  return{GetEnumerator:() => {
    const e1=Get(s1);
    const first=[true];
    return new T(e1, null, (x) => {
      if(x.s.MoveNext()){
        x.c=x.s.Current;
        return true;
      }
      else {
        const x_1=x.s;
        if(!Equals(x_1, null))x_1.Dispose();
        else null;
        x.s=null;
        return first[0]&&(first[0]=false,x.s=Get(s2),x.s.MoveNext()?(x.c=x.s.Current,true):(x.s.Dispose(),x.s=null,false));
      }
    }, (x) => {
      const x_1=x.s;
      if(!Equals(x_1, null))x_1.Dispose();
    });
  }};
}
function distinct_1(s){
  return distinctBy_1((x) => x, s);
}
function distinctBy_1(f, s){
  return{GetEnumerator:() => {
    const o=Get(s);
    const seen=new HashSet("New_3");
    return new T(null, null, (e) => {
      let cur, has;
      if(o.MoveNext()){
        cur=o.Current;
        has=seen.SAdd(f(cur));
        while(!has&&o.MoveNext())
          {
            cur=o.Current;
            has=seen.SAdd(f(cur));
          }
        return has&&(e.c=cur,true);
      }
      else return false;
    }, () => {
      o.Dispose();
    });
  }};
}
function map_1(f, s){
  return{GetEnumerator:() => {
    const en=Get(s);
    return new T(null, null, (e) => en.MoveNext()&&(e.c=f(en.Current),true), () => {
      en.Dispose();
    });
  }};
}
function collect_1(f, s){
  return concat_1(map_1(f, s));
}
function choose_1(f, s){
  return collect_1((x) => {
    const m=f(x);
    return m==null?FSharpList.Empty:ofArray([m.$0]);
  }, s);
}
function head(s){
  const e=Get(s);
  try {
    return e.MoveNext()?e.Current:insufficient();
  }
  finally {
    const _1=e;
    if(typeof _1=="object"&&isIDisposable(_1))e.Dispose();
  }
}
function forall_1(p, s){
  return!exists_1((x) =>!p(x), s);
}
function concat_1(ss){
  return{GetEnumerator:() => {
    const outerE=Get(ss);
    function next(st){
      while(true)
        {
          const m=st.s;
          if(Equals(m, null)){
            if(outerE.MoveNext()){
              st.s=Get(outerE.Current);
              st=st;
            }
            else {
              outerE.Dispose();
              return false;
            }
          }
          else if(m.MoveNext()){
            st.c=m.Current;
            return true;
          }
          else {
            st.Dispose();
            st.s=null;
            st=st;
          }
        }
    }
    return new T(null, null, next, (st) => {
      const x=st.s;
      if(!Equals(x, null))x.Dispose();
      const x_1=outerE;
      if(!Equals(x_1, null))x_1.Dispose();
    });
  }};
}
function exists_1(p, s){
  const e=Get(s);
  try {
    let r;
    r=false;
    while(!r&&e.MoveNext())
      r=p(e.Current);
    return r;
  }
  finally {
    const _1=e;
    if(typeof _1=="object"&&isIDisposable(_1))e.Dispose();
  }
}
function init_1(n, f){
  return take_1(n, initInfinite(f));
}
function fold_1(f, x, s){
  let r;
  r=x;
  const e=Get(s);
  try {
    while(e.MoveNext())
      r=f(r, e.Current);
    return r;
  }
  finally {
    const _1=e;
    if(typeof _1=="object"&&isIDisposable(_1))e.Dispose();
  }
}
function compareWith(f, s1, s2){
  const e1=Get(s1);
  try {
    const e2=Get(s2);
    try {
      let r, loop;
      r=0;
      loop=true;
      while(loop&&r===0)
        if(e1.MoveNext())r=e2.MoveNext()?f(e1.Current, e2.Current):1;
        else if(e2.MoveNext())r=-1;
        else loop=false;
      return r;
    }
    finally {
      const _1=e2;
      if(typeof _1=="object"&&isIDisposable(_1))e2.Dispose();
    }
  }
  finally {
    const _2=e1;
    if(typeof _2=="object"&&isIDisposable(_2))e1.Dispose();
  }
}
function forall2_1(p, s1, s2){
  return!exists2((_1, _2) =>!p(_1, _2), s1, s2);
}
function take_1(n, s){
  n<0?nonNegative():void 0;
  return{GetEnumerator:() => {
    const e=[Get(s)];
    return new T(0, null, (o) => {
      o.s=o.s+1;
      if(o.s>n)return false;
      else {
        const en=e[0];
        return Equals(en, null)?insufficient():en.MoveNext()?(o.c=en.Current,o.s===n?(en.Dispose(),e[0]=null):void 0,true):(en.Dispose(),e[0]=null,insufficient());
      }
    }, () => {
      const x=e[0];
      if(!Equals(x, null))x.Dispose();
    });
  }};
}
function initInfinite(f){
  return{GetEnumerator:() => new T(0, null, (e) => {
    e.c=f(e.s);
    e.s=e.s+1;
    return true;
  }, void 0)};
}
function rev(s){
  return delay(() => ofSeq(s).slice().reverse());
}
function iter_1(p, s){
  const e=Get(s);
  try {
    while(e.MoveNext())
      p(e.Current);
  }
  finally {
    const _1=e;
    if(typeof _1=="object"&&isIDisposable(_1))e.Dispose();
  }
}
function exists2(p, s1, s2){
  const e1=Get(s1);
  try {
    const e2=Get(s2);
    try {
      let r;
      r=false;
      while(!r&&e1.MoveNext()&&e2.MoveNext())
        r=p(e1.Current, e2.Current);
      return r;
    }
    finally {
      const _1=e2;
      if(typeof _1=="object"&&isIDisposable(_1))e2.Dispose();
    }
  }
  finally {
    const _2=e1;
    if(typeof _2=="object"&&isIDisposable(_2))e1.Dispose();
  }
}
function max(s){
  const e=Get(s);
  try {
    let m;
    if(!e.MoveNext())seqEmpty();
    else null;
    m=e.Current;
    while(e.MoveNext())
      {
        const x=e.Current;
        if(Compare(x, m)===1)m=x;
      }
    return m;
  }
  finally {
    const _1=e;
    if(typeof _1=="object"&&isIDisposable(_1))e.Dispose();
  }
}
function seqEmpty(){
  return FailWith("The input sequence was empty.");
}
function unfold(f, s){
  return{GetEnumerator:() => new T(s, null, (e) => {
    const m=f(e.s);
    if(m==null)return false;
    else {
      const t=m.$0[0];
      const s_1=m.$0[1];
      e.c=t;
      e.s=s_1;
      return true;
    }
  }, void 0)};
}
function New_4(keys){
  return{keys:keys};
}
class Object_1 {
  Equals(obj){
    return this===obj;
  }
  GetHashCode(){
    return -1;
  }
}
let _c=Lazy((_i) => class Var_1 extends Object_1 {
  static {
    _c=_i(this);
  }
  static Create_1(v){
    return new ConcreteVar(false, {s:Ready(v, [])}, v);
  }
  static { }
});
class Var extends Object_1 { }
class attr extends Object_1 { }
function Map(fn, a){
  return CreateLazy(() => Map_1(fn, a()));
}
function CreateLazy(observe){
  const lv={c:null, o:observe};
  return() => {
    let c;
    c=lv.c;
    if(c===null){
      c=lv.o();
      lv.c=c;
      const _1=c.s;
      if(_1!=null&&_1.$==0)lv.o=null;
      else WhenObsoleteRun(c, () => {
        lv.c=null;
      });
      return c;
    }
    else return c;
  };
}
function Const(x){
  const o={s:Forever(x)};
  return() => o;
}
function Bind(fn, view){
  return Join(Map(fn, view));
}
function Map2Unit(a, a_1){
  return CreateLazy(() => Map2Unit_1(a(), a_1()));
}
function Sink(act, a){
  function loop(){
    WhenRun(a(), act, () => {
      scheduler().Fork(loop);
    });
  }
  scheduler().Fork(loop);
}
function Join(a){
  return CreateLazy(() => Join_2(a()));
}
class Doc extends Object_1 {
  docNode;
  updates;
  static Concat(xs){
    return TreeReduce(Doc.Empty, Doc.Append, ofSeqNonCopying(xs));
  }
  static Run(parent, doc_2){
    LinkElement(parent, doc_2.docNode);
    Doc.RunInPlace(false, parent, doc_2);
  }
  static TextNode(v){
    return Doc.Mk(TextNodeDoc(globalThis.document.createTextNode(v)), Const());
  }
  static Element(name, attr_1, children){
    const a=Attr.Concat(attr_1);
    const c=Doc.Concat(children);
    return Elt.New(globalThis.document.createElement(name), a, c);
  }
  static EmbedView(view){
    const node=CreateEmbedNode();
    return Doc.Mk(EmbedDoc(node), Map(() => { }, Bind((doc_2) => {
      UpdateEmbedNode(node, doc_2.docNode);
      return doc_2.updates;
    }, view)));
  }
  static Append(a, b){
    return Doc.Mk(AppendDoc(a.docNode, b.docNode), Map2Unit(a.updates, b.updates));
  }
  static get Empty(){
    return Doc.Mk(null, Const());
  }
  static RunInPlace(childrenOnly, parent, doc_2){
    const st=CreateRunState(parent, doc_2.docNode);
    Sink(get_UseAnimations()||BatchUpdatesEnabled()?StartProcessor(PerformAnimatedUpdate(childrenOnly, st, doc_2.docNode)):() => {
      PerformSyncUpdate(childrenOnly, st, doc_2.docNode);
    }, doc_2.updates);
  }
  static Mk(node, updates){
    return new Doc(node, updates);
  }
  static TextView(txt){
    const node=CreateTextNode();
    return Doc.Mk(TextDoc(node), Map((t) => {
      UpdateTextNode(node, t);
    }, txt));
  }
  constructor(docNode, updates){
    super();
    this.docNode=docNode;
    this.updates=updates;
  }
}
function LoadLocalTemplates(baseName){
  !LocalTemplatesLoaded()?(set_LocalTemplatesLoaded(true),LoadNestedTemplates(globalThis.document.body, "")):void 0;
  LoadedTemplates().set_Item(baseName, LoadedTemplateFile(""));
}
function LocalTemplatesLoaded(){
  return _c_3.LocalTemplatesLoaded;
}
function set_LocalTemplatesLoaded(_1){
  _c_3.LocalTemplatesLoaded=_1;
}
function LoadNestedTemplates(root, baseName){
  const loadedTpls=LoadedTemplateFile(baseName);
  const rawTpls=new Dictionary("New_5");
  const wsTemplates=root.querySelectorAll("[ws-template]");
  for(let i=0, _1=wsTemplates.length-1;i<=_1;i++){
    const node=wsTemplates[i];
    const name=node.getAttribute("ws-template").toLowerCase();
    node.removeAttribute("ws-template");
    rawTpls.set_Item(name, FakeRootSingle(node));
  }
  const wsChildrenTemplates=root.querySelectorAll("[ws-children-template]");
  for(let i_1=0, _2=wsChildrenTemplates.length-1;i_1<=_2;i_1++){
    const node_1=wsChildrenTemplates[i_1];
    const name_1=node_1.getAttribute("ws-children-template").toLowerCase();
    node_1.removeAttribute("ws-children-template");
    rawTpls.set_Item(name_1, FakeRoot(node_1));
  }
  const html5TemplateBasedTemplates=root.querySelectorAll("template[id]");
  for(let i_2=0, _3=html5TemplateBasedTemplates.length-1;i_2<=_3;i_2++){
    const node_2=html5TemplateBasedTemplates[i_2];
    rawTpls.set_Item(node_2.getAttribute("id").toLowerCase(), FakeRootFromHTMLTemplate(node_2));
  }
  const html5TemplateBasedTemplates_1=root.querySelectorAll("template[name]");
  for(let i_3=0, _4=html5TemplateBasedTemplates_1.length-1;i_3<=_4;i_3++){
    const node_3=html5TemplateBasedTemplates_1[i_3];
    rawTpls.set_Item(node_3.getAttribute("name").toLowerCase(), FakeRootFromHTMLTemplate(node_3));
  }
  const instantiated=new HashSet("New_3");
  function prepareTemplate(name_2){
    if(!loadedTpls.ContainsKey(name_2)){
      let o;
      const m=(o=null,[rawTpls.TryGetValue(name_2, {get:() => o, set:(v) => {
        o=v;
      }}), o]);
      if(m[0]){
        instantiated.SAdd(name_2);
        rawTpls.RemoveKey(name_2);
        PrepareTemplateStrict(baseName, Some(name_2), m[1], Some(prepareTemplate));
      }
      else console.warn(instantiated.Contains(name_2)?"Encountered loop when instantiating "+name_2:"Local template does not exist: "+name_2);
    }
  }
  while(rawTpls.count>0)
    prepareTemplate(head(rawTpls.Keys));
}
function LoadedTemplates(){
  return _c_3.LoadedTemplates;
}
function LoadedTemplateFile(name){
  let o;
  const m=(o=null,[LoadedTemplates().TryGetValue(name, {get:() => o, set:(v) => {
    o=v;
  }}), o]);
  if(m[0])return m[1];
  else {
    const d=new Dictionary("New_5");
    LoadedTemplates().set_Item(name, d);
    return d;
  }
}
function FakeRootSingle(el){
  let _1;
  el.removeAttribute("ws-template");
  const m=el.getAttribute("ws-replace");
  if(m==null)_1=null;
  else {
    el.removeAttribute("ws-replace");
    const m_1=el.parentNode;
    if(Equals(m_1, null))_1=null;
    else {
      const n=globalThis.document.createElement(el.tagName);
      _1=(n.setAttribute("ws-replace", m),void m_1.replaceChild(n, el));
    }
  }
  const fakeroot=globalThis.document.createElement("div");
  fakeroot.appendChild(el);
  return fakeroot;
}
function FakeRoot(parent){
  const fakeroot=globalThis.document.createElement("div");
  while(parent.hasChildNodes())
    fakeroot.appendChild(parent.firstChild);
  return fakeroot;
}
function FakeRootFromHTMLTemplate(parent){
  const fakeroot=globalThis.document.createElement("div");
  const content=parent.content;
  for(let i=0, _1=content.childNodes.length-1;i<=_1;i++)fakeroot.appendChild(content.childNodes[i].cloneNode(true));
  return fakeroot;
}
function PrepareTemplateStrict(baseName, name, fakeroot, prepareLocalTemplate){
  const processedHTML5Templates=new HashSet("New_3");
  function recF(recI, _1){
    while(true)
      switch(recI){
        case 0:
          if(_1!==null){
            const next=_1.nextSibling;
            if(Equals(_1.nodeType, Node.TEXT_NODE))convertTextNode(_1);
            else Equals(_1.nodeType, Node.ELEMENT_NODE)?convertElement(_1):null;
            _1=next;
          }
          else return null;
          break;
        case 1:
          let _2;
          let _3;
          const name_2=string(_1.nodeName, Some(3), null).toLowerCase();
          const m=name_2.indexOf(".");
          const p=m===-1?[baseName, name_2]:[string(name_2, null, Some(m-1)), string(name_2, Some(m+1), null)];
          const instName=p[1];
          const instBaseName=p[0];
          if(instBaseName!=""&&!LoadedTemplates().ContainsKey(instBaseName))return failNotLoaded(instName);
          else {
            if(instBaseName==""&&prepareLocalTemplate!=null)prepareLocalTemplate.$0(instName);
            else null;
            const d=LoadedTemplates().Item(instBaseName);
            if(!d.ContainsKey(instName))return failNotLoaded(instName);
            else {
              const t=d.Item(instName);
              const instance=t.cloneNode(true);
              const usedHoles=new HashSet("New_3");
              const mappings=new Dictionary("New_5");
              const attrs=_1.attributes;
              for(let i=0, _6=attrs.length-1;i<=_6;i++){
                const name_3=attrs.item(i).name.toLowerCase();
                const m_1=attrs.item(i).nodeValue;
                let _4=m_1!=null&&m_1.length===0?name_3:m_1.toLowerCase();
                mappings.set_Item(name_3, _4);
                if(!usedHoles.SAdd(name_3))console.warn("Hole mapped twice", name_3);
              }
              for(let i_1=0, _7=_1.childNodes.length-1;i_1<=_7;i_1++){
                const n=_1.childNodes[i_1];
                if(Equals(n.nodeType, Node.ELEMENT_NODE))if(!usedHoles.SAdd(n.nodeName.toLowerCase()))console.warn("Hole filled twice", instName);
              }
              const singleTextFill=_1.childNodes.length===1&&Equals(_1.firstChild.nodeType, Node.TEXT_NODE);
              if(singleTextFill){
                const x=fillTextHole(instance, _1.firstChild.textContent, instName);
                const f=((usedHoles_1) =>(i_2) => usedHoles_1.SAdd(i_2))(usedHoles);
                let _5=((a) =>(o) => {
                  if(o!=null)a(o.$0);
                })((x_1) => {
                  f(x_1);
                });
                _2=_5(x);
              }
              else _2=null;
              removeHolesExcept(instance, usedHoles);
              if(!singleTextFill){
                for(let i_2=0, _8=_1.childNodes.length-1;i_2<=_8;i_2++){
                  const n_1=_1.childNodes[i_2];
                  if(Equals(n_1.nodeType, Node.ELEMENT_NODE))if(n_1.hasAttributes())fillInstanceAttrs(instance, n_1);
                  else fillDocHole(instance, n_1);
                }
                _3=void 0;
              }
              else _3=null;
              mapHoles(instance, mappings);
              fill(instance, _1.parentNode, _1);
              _1.parentNode.removeChild(_1);
              return;
            }
          }
          break;
      }
  }
  function fillDocHole(instance, fillWith){
    const name_2=fillWith.nodeName.toLowerCase();
    const fillHole=(p, n) => {
      let _1;
      if(name_2=="title"&&fillWith.hasChildNodes()){
        const parsed=ParseHTMLIntoFakeRoot(fillWith.textContent);
        fillWith.removeChild(fillWith.firstChild);
        while(parsed.hasChildNodes())
          fillWith.appendChild(parsed.firstChild);
        _1=void 0;
      }
      else _1=null;
      convertElement(fillWith);
      return fill(fillWith, p, n);
    };
    foreachNotPreserved(instance, "[ws-attr-holes]", (e) => {
      const holeAttrs=SplitChars(e.getAttribute("ws-attr-holes"), [" "], 1);
      for(let i=0, _2=holeAttrs.length-1;i<=_2;i++){
        const attrName=get(holeAttrs, i);
        let this_1=new RegExp("\\${"+name_2+"}", "ig");
        let str=e.getAttribute(attrName);
        let newSubStr=fillWith.textContent;
        let _1=str.replace(this_1, newSubStr);
        e.setAttribute(attrName, _1);
      }
    });
    const m=instance.querySelector("[ws-hole="+name_2+"]");
    if(Equals(m, null)){
      const m_1=instance.querySelector("[ws-replace="+name_2+"]");
      if(Equals(m_1, null)){
        const m_2=instance.querySelector("slot[name="+name_2+"]");
        return instance.tagName.toLowerCase()=="template"?(fillHole(m_2.parentNode, m_2),void m_2.parentNode.removeChild(m_2)):null;
      }
      else {
        fillHole(m_1.parentNode, m_1);
        m_1.parentNode.removeChild(m_1);
        return;
      }
    }
    else {
      while(m.hasChildNodes())
        m.removeChild(m.lastChild);
      m.removeAttribute("ws-hole");
      return(((a) => {
        const _1=a;
        return(_2) => fillHole(_1, _2);
      })(m))(null);
    }
  }
  function convertElement(el){
    if(!el.hasAttribute("ws-preserve"))if(StartsWith(el.nodeName.toLowerCase(), "ws-"))convertInstantiation(el);
    else {
      convertAttrs(el);
      convertNodeAndSiblings(el.firstChild);
    }
  }
  function convertNodeAndSiblings(n){
    return recF(0, n);
  }
  function convertInstantiation(el){
    return recF(1, el);
  }
  function convertNestedTemplates(el){
    while(true)
      {
        const m=el.querySelector("[ws-template]");
        if(Equals(m, null)){
          const m_1=el.querySelector("[ws-children-template]");
          if(Equals(m_1, null)){
            const idTemplates=el.querySelectorAll("template[id]");
            for(let i=1, _1=idTemplates.length-1;i<=_1;i++){
              const n=idTemplates[i];
              if(processedHTML5Templates.Contains(n)){ }
              else {
                PrepareTemplateStrict(baseName, Some(n.getAttribute("id")), n, null);
                processedHTML5Templates.SAdd(n);
              }
            }
            const nameTemplates=el.querySelectorAll("template[name]");
            for(let i_1=1, _2=nameTemplates.length-1;i_1<=_2;i_1++){
              const n_1=nameTemplates[i_1];
              if(processedHTML5Templates.Contains(n_1)){ }
              else {
                PrepareTemplateStrict(baseName, Some(n_1.getAttribute("name")), n_1, null);
                processedHTML5Templates.SAdd(n_1);
              }
            }
            return null;
          }
          else {
            const name_2=m_1.getAttribute("ws-children-template");
            m_1.removeAttribute("ws-children-template");
            PrepareTemplateStrict(baseName, Some(name_2), m_1, null);
            el=el;
          }
        }
        else {
          const name_3=m.getAttribute("ws-template");
          (PrepareSingleTemplate(baseName, Some(name_3), m))(null);
          el=el;
        }
      }
  }
  const name_1=(name==null?"":name.$0).toLowerCase();
  LoadedTemplateFile(baseName).set_Item(name_1, fakeroot);
  if(fakeroot.hasChildNodes()){
    convertNestedTemplates(fakeroot);
    convertNodeAndSiblings(fakeroot.firstChild);
  }
}
function foreachNotPreserved(root, selector, f){
  IterSelector(root, selector, (p) => {
    if(p.closest("[ws-preserve]")==null)f(p);
  });
}
function PrepareSingleTemplate(baseName, name, el){
  const root=FakeRootSingle(el);
  return(p) => {
    PrepareTemplateStrict(baseName, name, root, p);
  };
}
function TextHoleRE(){
  return _c_3.TextHoleRE;
}
let _c_1=Lazy((_i) => class $StartupCode_Client {
  static {
    _c_1=_i(this);
  }
  static requestSeq;
  static pendingCommandSeq;
  static maxSnapshotRecords;
  static watermarkStore;
  static pendingStore;
  static snapshotStore;
  static databaseVersion;
  static databaseName;
  static initializeClientExtensionGlobalsOnce;
  static runtimeAppendPageShapes;
  static registeredRenderers;
  static defaultCacheLimit;
  static defaultRenderLimit;
  static doc;
  static {
    this.doc=globalThis.document;
    this.defaultRenderLimit=200;
    this.defaultCacheLimit=1000;
    this.registeredRenderers=[];
    this.runtimeAppendPageShapes=[];
    this.initializeClientExtensionGlobalsOnce=(initializeClientExtensionGlobals(),0);
    this.databaseName="PulseTrade.Comm.Spa.BrowserDb";
    this.databaseVersion=3;
    this.snapshotStore="uiSnapshots";
    this.pendingStore="pendingCommands";
    this.watermarkStore="streamWatermarks";
    this.maxSnapshotRecords=256;
    this.pendingCommandSeq=0;
    this.requestSeq=0;
  }
});
function TrimEnd(s, t){
  let i, go;
  if(Equals(t, null)||t.length==0)return TrimEndWS(s);
  else {
    i=s.length-1;
    go=true;
    while(i>=0&&go)
      ((() => {
        const c=s[i];
        return exists((y) => c===y, t)?void(i=i-1):void(go=false);
      })());
    return Substring(s, 0, i+1);
  }
}
function concat_2(separator, strings){
  return ofSeq(strings).join(separator);
}
function Trim(s){
  return s.replace(new RegExp("^\\s+"), "").replace(new RegExp("\\s+$"), "");
}
function TrimEndWS(s){
  return s.replace(new RegExp("\\s+$"), "");
}
function StartsWith(t, s){
  return t.substring(0, s.length)==s;
}
function Replace(subject, search, replace){
  function replaceLoop(subj){
    const index=subj.indexOf(search);
    if(index!==-1){
      const replaced=ReplaceOnce(subj, search, replace);
      const nextStartIndex=index+replace.length;
      return Substring(replaced, 0, index+replace.length)+replaceLoop(replaced.substring(nextStartIndex));
    }
    else return subj;
  }
  return replaceLoop(subject);
}
function TrimStart(s, t){
  let i, go;
  if(Equals(t, null)||t.length==0)return TrimStartWS(s);
  else {
    i=0;
    go=true;
    while(i<s.length&&go)
      ((() => {
        const c=s[i];
        return exists((y) => c===y, t)?void(i=i+1):void(go=false);
      })());
    return s.substring(i);
  }
}
function IsNullOrWhiteSpace(x){
  return x==null||(new RegExp("^\\s*$")).test(x);
}
function Join_1(sep, values){
  return values.join(sep);
}
function Substring(s, ix, ct){
  return s.substr(ix, ct);
}
function ReplaceOnce(string_1, search, replace){
  return string_1.replace(search, replace);
}
function TrimStartWS(s){
  return s.replace(new RegExp("^\\s+"), "");
}
function IsNullOrEmpty(x){
  return x==null||x=="";
}
function SplitChars(s, sep, opts){
  return Split(s, new RegExp("["+RegexEscape(sep.join(""))+"]"), opts);
}
function Split(s, pat, opts){
  return opts===1?filter((x) => x!=="", SplitWith(s, pat)):SplitWith(s, pat);
}
function RegexEscape(s){
  return s.replace(new RegExp("[-\\/\\\\^$*+?.()|[\\]{}]", "g"), "\\$&");
}
function SplitWith(str, pat){
  return str.split(pat);
}
function forall_2(f, s){
  return forall_1(f, protect(s));
}
function protect(s){
  return s==null?"":s;
}
class FSharpList {
  static Empty=Create_1(FSharpList, {$:0});
  static Cons(Head, Tail){
    return Create_1(FSharpList, {
      $:1, 
      $0:Head, 
      $1:Tail
    });
  }
  GetEnumerator(){
    return new T(this, null, (e) => {
      const m=e.s;
      if(m.$==0)return false;
      else {
        const xs=m.$1;
        e.c=m.$0;
        e.s=xs;
        return true;
      }
    }, void 0);
  }
  $;
  $0;
  $1;
}
function TryParse(s, r){
  return TryParse_2(s, -2147483648, 2147483647, r);
}
function TryParse_1(s, r){
  return TryParseBigInt(s, -9223372036854775808n, 9223372036854775807n, r);
}
function New_5(pageId, tabId, path, title, setName, shape, description, keyPlaceholder, valuePlaceholder, defaultKey, tags){
  return{
    pageId:pageId, 
    tabId:tabId, 
    path:path, 
    title:title, 
    setName:setName, 
    shape:shape, 
    description:description, 
    keyPlaceholder:keyPlaceholder, 
    valuePlaceholder:valuePlaceholder, 
    defaultKey:defaultKey, 
    tags:tags
  };
}
function New_6(pageId, mode, setName, keys){
  return{
    pageId:pageId, 
    mode:mode, 
    setName:setName, 
    keys:keys
  };
}
function New_7(streamPageId, lineageKind, legacyPageIdAlias, readsLegacyPageStreams, readRepairPolicy){
  return{
    streamPageId:streamPageId, 
    lineageKind:lineageKind, 
    legacyPageIdAlias:legacyPageIdAlias, 
    readsLegacyPageStreams:readsLegacyPageStreams, 
    readRepairPolicy:readRepairPolicy
  };
}
function New_8(streamPageId, lineageKind, legacyPageIdAlias, readsLegacyPageStreams, readRepairPolicy, candidateValueStreamKeys, candidateValueStreamCount, candidateKeyRegistryStreamKeys, candidateKeyRegistryStreamCount){
  return{
    streamPageId:streamPageId, 
    lineageKind:lineageKind, 
    legacyPageIdAlias:legacyPageIdAlias, 
    readsLegacyPageStreams:readsLegacyPageStreams, 
    readRepairPolicy:readRepairPolicy, 
    candidateValueStreamKeys:candidateValueStreamKeys, 
    candidateValueStreamCount:candidateValueStreamCount, 
    candidateKeyRegistryStreamKeys:candidateKeyRegistryStreamKeys, 
    candidateKeyRegistryStreamCount:candidateKeyRegistryStreamCount
  };
}
function New_9(commandId, serverRealityId, kind, target, url, method, payloadJson, status){
  return{
    commandId:commandId, 
    serverRealityId:serverRealityId, 
    kind:kind, 
    target:target, 
    url:url, 
    method:method, 
    payloadJson:payloadJson, 
    status:status
  };
}
function ofArray(arr){
  let r;
  r=FSharpList.Empty;
  for(let i=length(arr)-1, _1=0;i>=_1;i--)r=FSharpList.Cons(get(arr, i), r);
  return r;
}
function ofSeq_1(s){
  if(s instanceof FSharpList)return s;
  else if(s instanceof Array)return ofArray(s);
  else {
    const e=Get(s);
    try {
      let go, r;
      go=e.MoveNext();
      if(!go)return FSharpList.Empty;
      else {
        const res=Create_1(FSharpList, {$:1});
        r=res;
        while(go)
          {
            r.$0=e.Current;
            if(e.MoveNext()){
              const t=Create_1(FSharpList, {$:1});
              r=(r.$1=t,t);
            }
            else go=false;
          }
        r.$1=FSharpList.Empty;
        return res;
      }
    }
    finally {
      const _1=e;
      if(typeof _1=="object"&&isIDisposable(_1))e.Dispose();
    }
  }
}
function map_2(f, x){
  let r, l, go;
  if(x.$==0)return x;
  else {
    const res=Create_1(FSharpList, {$:1});
    r=res;
    l=x;
    go=true;
    while(go)
      {
        r.$0=f(l.$0);
        l=l.$1;
        if(l.$==0)go=false;
        else {
          const t=Create_1(FSharpList, {$:1});
          r=(r.$1=t,t);
        }
      }
    r.$1=FSharpList.Empty;
    return res;
  }
}
function exists_2(p, x){
  let e, l;
  e=false;
  l=x;
  while(!e&&l.$==1)
    {
      e=p(l.$0);
      l=l.$1;
    }
  return e;
}
function collect_2(f, l){
  return ofSeq_1(collect_1(f, l));
}
function append_3(x, y){
  let r, l, go;
  if(x.$==0)return y;
  else if(y.$==0)return x;
  else {
    const res=Create_1(FSharpList, {$:1});
    r=res;
    l=x;
    go=true;
    while(go)
      {
        r.$0=l.$0;
        l=l.$1;
        if(l.$==0)go=false;
        else {
          const t=Create_1(FSharpList, {$:1});
          r=(r.$1=t,t);
        }
      }
    r.$1=y;
    return res;
  }
}
function head_1(l){
  return l.$==1?l.$0:listEmpty();
}
function tail(l){
  return l.$==1?l.$1:listEmpty();
}
function listEmpty(){
  return FailWith("The input list was empty.");
}
function New_10(status, page, bucketCount, maxSequence, keyMaxSequence, lineage, lineageHealth, buckets){
  return{
    status:status, 
    page:page, 
    bucketCount:bucketCount, 
    maxSequence:maxSequence, 
    keyMaxSequence:keyMaxSequence, 
    lineage:lineage, 
    lineageHealth:lineageHealth, 
    buckets:buckets
  };
}
function New_11(keyId, keys, displayName, setName, valueCount, minSequence, maxSequence, updatedAtUtc, values){
  return{
    keyId:keyId, 
    keys:keys, 
    displayName:displayName, 
    setName:setName, 
    valueCount:valueCount, 
    minSequence:minSequence, 
    maxSequence:maxSequence, 
    updatedAtUtc:updatedAtUtc, 
    values:values
  };
}
function New_12(pageId, keyJson, valueText, direction, tags){
  return{
    pageId:pageId, 
    keyJson:keyJson, 
    valueText:valueText, 
    direction:direction, 
    tags:tags
  };
}
function New_13(pageId, keyJson, displayName){
  return{
    pageId:pageId, 
    keyJson:keyJson, 
    displayName:displayName
  };
}
function New_14(pageId, keyJson, rawArgu, tags){
  return{
    pageId:pageId, 
    keyJson:keyJson, 
    rawArgu:rawArgu, 
    tags:tags
  };
}
function New_15(pageId){
  return{pageId:pageId};
}
function New_16(pageId, keyId){
  return{pageId:pageId, keyId:keyId};
}
function New_17(type, requestId, pageId, title, setName, streamKey, actorAddress, rawArgu, renderMode, tags, browserId, tabId){
  return{
    type:type, 
    requestId:requestId, 
    pageId:pageId, 
    title:title, 
    setName:setName, 
    streamKey:streamKey, 
    actorAddress:actorAddress, 
    rawArgu:rawArgu, 
    renderMode:renderMode, 
    tags:tags, 
    browserId:browserId, 
    tabId:tabId
  };
}
function New_18(type, requestId, pageId, title, setName, streamKey, keyJson, valueText, direction, renderMode, idempotencyKey, tags, browserId, tabId){
  return{
    type:type, 
    requestId:requestId, 
    pageId:pageId, 
    title:title, 
    setName:setName, 
    streamKey:streamKey, 
    keyJson:keyJson, 
    valueText:valueText, 
    direction:direction, 
    renderMode:renderMode, 
    idempotencyKey:idempotencyKey, 
    tags:tags, 
    browserId:browserId, 
    tabId:tabId
  };
}
function New_19(type, requestId, streamKey, payload, sourceKind, renderMode, idempotencyKey, tags, browserId, tabId){
  return{
    type:type, 
    requestId:requestId, 
    streamKey:streamKey, 
    payload:payload, 
    sourceKind:sourceKind, 
    renderMode:renderMode, 
    idempotencyKey:idempotencyKey, 
    tags:tags, 
    browserId:browserId, 
    tabId:tabId
  };
}
function New_20(keyId, setName, keys, valueCount, maxSequence, updatedAtUtc, values){
  return{
    keyId:keyId, 
    setName:setName, 
    keys:keys, 
    valueCount:valueCount, 
    maxSequence:maxSequence, 
    updatedAtUtc:updatedAtUtc, 
    values:values
  };
}
function New_21(valueId, keys, createdAtUtc, value, tags){
  return{
    valueId:valueId, 
    keys:keys, 
    createdAtUtc:createdAtUtc, 
    value:value, 
    tags:tags
  };
}
function New_22(maxSequence, buckets){
  return{maxSequence:maxSequence, buckets:buckets};
}
function New_23(nodeCount, actorCount, maxSequence, nodes){
  return{
    nodeCount:nodeCount, 
    actorCount:actorCount, 
    maxSequence:maxSequence, 
    nodes:nodes
  };
}
class HashSet extends Object_1 {
  equals;
  hash;
  data;
  count;
  Contains(item){
    const arr=this.data[this.hash(item)];
    return arr==null?false:this.arrContains(item, arr);
  }
  Remove(item){
    const arr=this.data[this.hash(item)];
    return arr==null?false:this.arrRemove(item, arr)&&(this.count=this.count-1,true);
  }
  SAdd(item){
    return this.add(item);
  }
  arrContains(item, arr){
    let c, i;
    c=true;
    i=0;
    const l=arr.length;
    while(c&&i<l)
      if(this.equals.apply(null, [arr[i], item]))c=false;
      else i=i+1;
    return!c;
  }
  arrRemove(item, arr){
    let c, i;
    c=true;
    i=0;
    const l=arr.length;
    while(c&&i<l)
      if(this.equals.apply(null, [arr[i], item])){
        arr.splice(i, 1);
        c=false;
      }
      else i=i+1;
    return!c;
  }
  add(item){
    const h=this.hash(item);
    const arr=this.data[h];
    return arr==null?(this.data[h]=[item],this.count=this.count+1,true):this.arrContains(item, arr)?false:(arr.push(item),this.count=this.count+1,true);
  }
  GetEnumerator(){
    return Get(concat_3(this.data));
  }
  ExceptWith(xs){
    const e=Get(xs);
    try {
      while(e.MoveNext())
        this.Remove(e.Current);
    }
    finally {
      const _1=e;
      if(typeof _1=="object"&&isIDisposable(_1))e.Dispose();
    }
  }
  get Count(){
    return this.count;
  }
  IntersectWith(xs){
    const other=new HashSet("New_4", xs, this.equals, this.hash);
    const all=concat_3(this.data);
    for(let i=0, _1=all.length-1;i<=_1;i++){
      const item=all[i];
      if(!other.Contains(item))this.Remove(item);
    }
  }
  CopyTo(arr, index){
    const all=concat_3(this.data);
    for(let i=0, _1=all.length-1;i<=_1;i++)set(arr, i+index, all[i]);
  }
  constructor(i, _1, _2, _3){
    if(i=="New_3"){
      i="New_4";
      _1=[];
      _2=Equals;
      _3=Hash;
    }
    let init_2;
    if(i=="New_2"){
      init_2=_1;
      i="New_4";
      _1=init_2;
      _2=Equals;
      _3=Hash;
    }
    if(i=="New_4"){
      const init_3=_1;
      const equals=_2;
      const hash=_3;
      super();
      this.equals=equals;
      this.hash=hash;
      this.data=[];
      this.count=0;
      const e=Get(init_3);
      try {
        while(e.MoveNext())
          this.add(e.Current);
      }
      finally {
        const _4=e;
        if(typeof _4=="object"&&isIDisposable(_4))e.Dispose();
      }
    }
  }
}
function OfArray(a){
  return new FSharpMap("New_1", OfSeq(map_1((_1) => Pair.New(_1[0], _1[1]), a)));
}
function New_24(actorId, displayName, kind, keys, status, routees){
  return{
    actorId:actorId, 
    displayName:displayName, 
    kind:kind, 
    keys:keys, 
    status:status, 
    routees:routees
  };
}
function New_25(nodeId_1, nodeAddress_1, status, roles, actors){
  return{
    nodeId:nodeId_1, 
    nodeAddress:nodeAddress_1, 
    status:status, 
    roles:roles, 
    actors:actors
  };
}
function New_26(messageId, fromId, toId, scope, body, createdAtUtc){
  return{
    messageId:messageId, 
    fromId:fromId, 
    toId:toId, 
    scope:scope, 
    body:body, 
    createdAtUtc:createdAtUtc
  };
}
function New_27(messages, nextAfterMessageId){
  return{messages:messages, nextAfterMessageId:nextAfterMessageId};
}
function New_28(streamId, newestSequence, cachedCount, source, touchedAt){
  return{
    streamId:streamId, 
    newestSequence:newestSequence, 
    cachedCount:cachedCount, 
    source:source, 
    touchedAt:touchedAt
  };
}
function New_29(type, requestId, fromId, toId, body, tags, browserId, tabId){
  return{
    type:type, 
    requestId:requestId, 
    fromId:fromId, 
    toId:toId, 
    body:body, 
    tags:tags, 
    browserId:browserId, 
    tabId:tabId
  };
}
function New_30(fromId, toId, body, tags){
  return{
    fromId:fromId, 
    toId:toId, 
    body:body, 
    tags:tags
  };
}
let _c_2=Lazy((_i) => class $StartupCode_ArguFormRenderer {
  static {
    _c_2=_i(this);
  }
  static documents;
  static schemas;
  static doc;
  static {
    this.doc=globalThis.document;
    this.schemas=[];
    this.documents=[];
  }
});
function Get(x){
  return x instanceof Array?ArrayEnumerator(x):Equals(typeof x, "string")?StringEnumerator(x):x.GetEnumerator();
}
function ArrayEnumerator(s){
  return new T(0, null, (e) => {
    const i=e.s;
    return i<length(s)&&(e.c=get(s, i),e.s=i+1,true);
  }, void 0);
}
function StringEnumerator(s){
  return new T(0, null, (e) => {
    const i=e.s;
    return i<s.length&&(e.c=s[i],e.s=i+1,true);
  }, void 0);
}
function Get0(x){
  return x instanceof Array?ArrayEnumerator(x):Equals(typeof x, "string")?StringEnumerator(x):"GetEnumerator0"in x?x.GetEnumerator0():x.GetEnumerator();
}
class T extends Object_1 {
  s;
  c;
  n;
  d;
  e;
  MoveNext(){
    const m=this.n(this);
    this.e=m?1:2;
    return m;
  }
  get Current(){
    return this.e===1?this.c:this.e===0?FailWith("Enumeration has not started. Call MoveNext."):FailWith("Enumeration already finished.");
  }
  Dispose(){
    if(this.d)this.d(this);
  }
  constructor(s, c, n, d){
    super();
    this.s=s;
    this.c=c;
    this.n=n;
    this.d=d;
    this.e=0;
  }
}
function New_31(rawArgu, duTypeName, unionCaseName, keyJson){
  return{
    rawArgu:rawArgu, 
    duTypeName:duTypeName, 
    unionCaseName:unionCaseName, 
    keyJson:keyJson
  };
}
class ConcreteVar extends Var {
  isConst;
  current;
  snap;
  view;
  id;
  get View(){
    return this.view;
  }
  Set(v){
    if(this.isConst)(((_1) => _1("WebSharper.UI: invalid attempt to change value of a Var after calling SetFinal"))((s) => {
      console.log(s);
    }));
    else {
      Obsolete(this.snap);
      this.current=v;
      this.snap={s:Ready(v, [])};
    }
  }
  Get(){
    return this.current;
  }
  UpdateMaybe(f){
    const m=f(this.Get());
    if(m!=null&&m.$==1)this.Set(m.$0);
  }
  constructor(isConst, initSnap, initValue){
    super();
    this.isConst=isConst;
    this.current=initValue;
    this.snap=initSnap;
    this.view=() => this.snap;
    this.id=Int();
  }
}
function Map_1(fn, sn){
  const m=sn.s;
  if(m!=null&&m.$==0)return{s:Forever(fn(m.$0))};
  else {
    const res={s:Waiting([], [])};
    When(sn, (a) => {
      MarkDone(res, sn, fn(a));
    }, res);
    return res;
  }
}
function WhenObsoleteRun(snap, obs){
  const m=snap.s;
  if(m==null)obs();
  else m!=null&&m.$==2?(m.$0,m.$1.push(obs)):m!=null&&m.$==3?(m.$0,m.$1.push(obs)):m.$0;
}
function When(snap, avail, obs){
  const m=snap.s;
  if(m==null)Obsolete(obs);
  else if(m!=null&&m.$==2){
    const v=m.$0;
    EnqueueSafe(m.$1, obs);
    avail(v);
  }
  else if(m!=null&&m.$==3){
    const q2=m.$1;
    m.$0.push(avail);
    EnqueueSafe(q2, obs);
  }
  else avail(m.$0);
}
function MarkDone(res, sn, v){
  const _1=sn.s;
  if(_1!=null&&_1.$==0)MarkForever(res, v);
  else MarkReady(res, v);
}
function EnqueueSafe(q, x){
  q.push(x);
  if(q.length%20===0){
    const qcopy=q.slice(0);
    Clear(q);
    for(let i=0, _1=length(qcopy)-1;i<=_1;i++){
      const o=get(qcopy, i);
      if(typeof o=="object")(((sn) => {
        if(sn.s)q.push(sn);
      })(o));
      else(((f) => {
        q.push(f);
      })(o));
    }
  }
  else void 0;
}
function MarkForever(sn, v){
  const m=sn.s;
  if(m!=null&&m.$==3){
    const q=m.$0;
    sn.s=Forever(v);
    for(let i=0, _1=length(q)-1;i<=_1;i++)(get(q, i))(v);
  }
  else void 0;
}
function MarkReady(sn, v){
  const m=sn.s;
  if(m!=null&&m.$==3){
    const q2=m.$1;
    const q1=m.$0;
    sn.s=Ready(v, q2);
    for(let i=0, _1=length(q1)-1;i<=_1;i++)(get(q1, i))(v);
  }
  else void 0;
}
function Map2Unit_1(sn1, sn2){
  const _1=sn1.s;
  const _2=sn2.s;
  if(_1!=null&&_1.$==0)return _2!=null&&_2.$==0?{s:Forever(null)}:sn2;
  else if(_2!=null&&_2.$==0)return sn1;
  else {
    const res={s:Waiting([], [])};
    const cont=() => {
      const m=res.s;
      if(!(m!=null&&m.$==0||m!=null&&m.$==2)){
        const _3=ValueAndForever(sn1);
        const _4=ValueAndForever(sn2);
        if(_3!=null&&_3.$==1)if(_4!=null&&_4.$==1)if(_3.$0[1]&&_4.$0[1])MarkForever(res, null);
        else MarkReady(res, null);
      }
    };
    When(sn1, cont, res);
    When(sn2, cont, res);
    return res;
  }
}
function WhenRun(snap, avail, obs){
  const m=snap.s;
  if(m==null)obs();
  else if(m!=null&&m.$==2){
    const v=m.$0;
    m.$1.push(obs);
    avail(v);
  }
  else if(m!=null&&m.$==3){
    const q2=m.$1;
    m.$0.push(avail);
    q2.push(obs);
  }
  else avail(m.$0);
}
function Join_2(snap){
  const res={s:Waiting([], [])};
  When(snap, (x) => {
    const y=x();
    When(y, (v) => {
      let _1;
      const _2=y.s;
      if(_2!=null&&_2.$==0){
        const _3=snap.s;
        _1=_3!=null&&_3.$==0;
      }
      else _1=false;
      if(_1)MarkForever(res, v);
      else MarkReady(res, v);
    }, res);
  }, res);
  return res;
}
function ValueAndForever(snap){
  const m=snap.s;
  return m!=null&&m.$==0?Some([m.$0, true]):m!=null&&m.$==2?Some([m.$0, false]):null;
}
function Copy(sn){
  const m=sn.s;
  if(m==null)return sn;
  else if(m!=null&&m.$==2){
    const res={s:Ready(m.$0, [])};
    WhenObsolete(sn, res);
    return res;
  }
  else if(m!=null&&m.$==3){
    const res_1={s:Waiting([], [])};
    When(sn, (v) => {
      MarkDone(res_1, sn, v);
    }, res_1);
    return res_1;
  }
  else return sn;
}
function WhenObsolete(snap, obs){
  const m=snap.s;
  if(m==null)Obsolete(obs);
  else m!=null&&m.$==2?(m.$0,EnqueueSafe(m.$1, obs)):m!=null&&m.$==3?(m.$0,EnqueueSafe(m.$1, obs)):m.$0;
}
class Attr {
  static Create(name, value){
    return Attr.A3((el) => {
      el.setAttribute(name, value);
    });
  }
  static A4(onAfterRender){
    return Create_1(Attr, {$:4, $0:onAfterRender});
  }
  static Concat(xs){
    const x=ofSeqNonCopying(xs);
    return TreeReduce(EmptyAttr(), (_1, _2) => AppendTree(_1, _2), x);
  }
  static A3(init_2){
    return Create_1(Attr, {$:3, $0:init_2});
  }
  static A1(Item){
    return Create_1(Attr, {$:1, $0:Item});
  }
  static A2(Item1, Item2){
    return Create_1(Attr, {
      $:2, 
      $0:Item1, 
      $1:Item2
    });
  }
  $;
  $0;
  $1;
}
function OnAfterRender(callback){
  return Attr.A4(callback);
}
function Handler(name, callback){
  return Attr.A3((el) => {
    el.addEventListener(name, (d) =>(callback(el))(d), false);
  });
}
function Dynamic(name, view){
  return Dynamic_1(view, (el) =>(v) => el.setAttribute(name, v));
}
function New_32(outputDirectory){
  return{outputDirectory:outputDirectory};
}
function ofSeqNonCopying(xs){
  if(xs instanceof Array)return xs;
  else if(xs instanceof FSharpList)return ofList(xs);
  else if(xs===null)return[];
  else {
    const q=[];
    const o=Get(xs);
    try {
      while(o.MoveNext())
        q.push(o.Current);
      return q;
    }
    finally {
      const _1=o;
      if(typeof _1=="object"&&isIDisposable(_1))o.Dispose();
    }
  }
}
function TreeReduce(defaultValue, reduction, array){
  const l=length(array);
  function loop(off){
    return(len) => {
      let _1;
      switch(len<=0?0:len===1?off>=0&&off<l?1:(_1=len,2):(_1=len,2)){
        case 0:
          return defaultValue;
        case 1:
          return get(array, off);
        case 2:
          const l2=len/2>>0;
          return reduction((loop(off))(l2), (loop(off+l2))(len-l2));
      }
    };
  }
  return(loop(0))(l);
}
function MapTreeReduce(mapping, defaultValue, reduction, array){
  const l=length(array);
  function loop(off){
    return(len) => {
      let _1;
      switch(len<=0?0:len===1?off>=0&&off<l?1:(_1=len,2):(_1=len,2)){
        case 0:
          return defaultValue;
        case 1:
          return mapping(get(array, off));
        case 2:
          const l2=len/2>>0;
          return reduction((loop(off))(l2), (loop(off+l2))(len-l2));
      }
    };
  }
  return(loop(0))(l);
}
function enumTryWith(s, f, h){
  return{GetEnumerator:() => {
    let enum_1, orig;
    enum_1=null;
    orig=true;
    return new T(null, null, (e) => {
      try {
        Equals(enum_1, null)?enum_1=Get(s):void 0;
        return enum_1.MoveNext()&&(e.c=enum_1.Current,true);
      }
      catch(m){
        if(orig&&f(m)===1){
          orig=false;
          const x=enum_1;
          if(!Equals(x, null))x.Dispose();
          else null;
          enum_1=Get(h(m));
          return enum_1.MoveNext()&&(e.c=enum_1.Current,true);
        }
        else throw m;
      }
    }, () => {
      const x=enum_1;
      if(!Equals(x, null))x.Dispose();
    });
  }};
}
class Exception extends Object_1 { }
class Dictionary extends Object_1 {
  equals;
  hash;
  count;
  data;
  set_Item(k, v){
    this.set(k, v);
  }
  ContainsKey(k){
    const d=this.data[this.hash(k)];
    return d==null?false:exists((a) => this.equals.apply(null, [(KeyValue(a))[0], k]), d);
  }
  TryGetValue(k, res){
    const d=this.data[this.hash(k)];
    if(d==null)return false;
    else {
      const v=tryPick((a) => {
        const a_1=KeyValue(a);
        return this.equals.apply(null, [a_1[0], k])?Some(a_1[1]):null;
      }, d);
      return v!=null&&v.$==1&&(res.set(v.$0),true);
    }
  }
  RemoveKey(k){
    return this.remove(k);
  }
  get Keys(){
    return new KeyCollection(this);
  }
  set(k, v){
    const h=this.hash(k);
    const d=this.data[h];
    if(d==null){
      this.count=this.count+1;
      this.data[h]=new Array({K:k, V:v});
    }
    else {
      const m=tryFindIndex((a) => this.equals.apply(null, [(KeyValue(a))[0], k]), d);
      if(m==null){
        this.count=this.count+1;
        d.push({K:k, V:v});
      }
      else d[m.$0]={K:k, V:v};
    }
  }
  Item(k){
    return this.get(k);
  }
  DAdd(k, v){
    this.add(k, v);
  }
  remove(k){
    const h=this.hash(k);
    const d=this.data[h];
    if(d==null)return false;
    else {
      const r=filter((a) =>!this.equals.apply(null, [(KeyValue(a))[0], k]), d);
      return length(r)<d.length&&(this.count=this.count-1,this.data[h]=r,true);
    }
  }
  GetEnumerator(){
    return Get0(concat(GetFieldValues(this.data)));
  }
  get(k){
    const d=this.data[this.hash(k)];
    return d==null?notPresent():pick((a) => {
      const a_1=KeyValue(a);
      return this.equals.apply(null, [a_1[0], k])?Some(a_1[1]):null;
    }, d);
  }
  add(k, v){
    const h=this.hash(k);
    const d=this.data[h];
    if(d==null){
      this.count=this.count+1;
      this.data[h]=new Array({K:k, V:v});
    }
    else {
      exists((a) => this.equals.apply(null, [(KeyValue(a))[0], k]), d)?alreadyAdded():void 0;
      this.count=this.count+1;
      d.push({K:k, V:v});
    }
  }
  constructor(i, _1, _2, _3){
    if(i=="New_5"){
      i="New_6";
      _1=[];
      _2=Equals;
      _3=Hash;
    }
    if(i=="New_6"){
      const init_2=_1;
      const equals=_2;
      const hash=_3;
      super();
      this.equals=equals;
      this.hash=hash;
      this.count=0;
      this.data=[];
      const e=Get(init_2);
      try {
        while(e.MoveNext())
          {
            const x=e.Current;
            this.set(x.K, x.V);
          }
      }
      finally {
        const _4=e;
        if(typeof _4=="object"&&isIDisposable(_4))e.Dispose();
      }
    }
  }
}
function LinkElement(el, children){
  InsertDoc(el, children, null);
}
function CreateEmbedNode(){
  return{Current:null, Dirty:false};
}
function UpdateEmbedNode(node, upd){
  node.Current=upd;
  node.Dirty=true;
}
function InsertDoc(parent, doc_2, pos){
  while(true)
    {
      if(doc_2!=null&&doc_2.$==1){
        const e=doc_2.$0;
        return InsertNode(parent, e.El, pos);
      }
      else if(doc_2!=null&&doc_2.$==2){
        const d=doc_2.$0;
        d.Dirty=false;
        doc_2=d.Current;
      }
      else if(doc_2==null)return pos;
      else if(doc_2!=null&&doc_2.$==4){
        const t=doc_2.$0;
        return InsertNode(parent, t.Text, pos);
      }
      else if(doc_2!=null&&doc_2.$==5){
        const t_1=doc_2.$0;
        return InsertNode(parent, t_1, pos);
      }
      else if(doc_2!=null&&doc_2.$==6)return foldBack((_1, _2) =>((((parent_1) =>(el) =>(pos_1) => el==null||el.constructor===Object?InsertDoc(parent_1, el, pos_1):InsertNode(parent_1, el, pos_1))(parent))(_1))(_2), doc_2.$0.Els, pos);
      else {
        const b=doc_2.$1;
        const a=doc_2.$0;
        doc_2=a;
        pos=InsertDoc(parent, b, pos);
      }
    }
}
function CreateRunState(parent, doc_2){
  return New_35(get_Empty_1(), CreateElemNode(parent, EmptyAttr(), doc_2));
}
function PerformAnimatedUpdate(childrenOnly, st, doc_2){
  return get_UseAnimations()?Delay(() => {
    const cur=FindAll(doc_2);
    const change=ComputeChangeAnim(st, cur);
    const enter=ComputeEnterAnim(st, cur);
    return Bind_1(Play(Append(change, ComputeExitAnim(st, cur))), () => Bind_1(SyncElemNodesNextFrame(childrenOnly, st), () => Bind_1(Play(enter), () => {
      st.PreviousNodes=cur;
      return Return(null);
    })));
  }):SyncElemNodesNextFrame(childrenOnly, st);
}
function PerformSyncUpdate(childrenOnly, st, doc_2){
  const cur=FindAll(doc_2);
  SyncElemNode(childrenOnly, st.Top);
  st.PreviousNodes=cur;
}
function InsertNode(parent, node, pos){
  InsertAt(parent, pos, node);
  return node;
}
function CreateElemNode(el, attr_1, children){
  LinkElement(el, children);
  const attr_2=Insert(el, attr_1);
  return DocElemNode.New(attr_2, children, null, el, Int(), GetOptional(attr_2.OnAfterRender));
}
function SyncElemNodesNextFrame(childrenOnly, st){
  if(BatchUpdatesEnabled()){
    const c=(ok) => {
      requestAnimationFrame(() => {
        SyncElemNode(childrenOnly, st.Top);
        ok();
      });
    };
    return FromContinuations((_1, _2, _3) => c.apply(null, [_1, _2, _3]));
  }
  else {
    SyncElemNode(childrenOnly, st.Top);
    return Return(null);
  }
}
function ComputeExitAnim(st, cur){
  return Concat(map((n) => GetExitAnim(n.Attr), ToArray(Except(cur, Filter((n) => HasExitAnim(n.Attr), st.PreviousNodes)))));
}
function ComputeEnterAnim(st, cur){
  return Concat(map((n) => GetEnterAnim(n.Attr), ToArray(Except(st.PreviousNodes, Filter((n) => HasEnterAnim(n.Attr), cur)))));
}
function ComputeChangeAnim(st, cur){
  const f=(n) => HasChangeAnim(n.Attr);
  const relevant=(a) => Filter(f, a);
  return Concat(map((n) => GetChangeAnim(n.Attr), ToArray(Intersect(relevant(st.PreviousNodes), relevant(cur)))));
}
function SyncElemNode(childrenOnly, el){
  !childrenOnly?SyncElement(el):void 0;
  Sync(el.Children);
  AfterRender(el);
}
function CreateTextNode(){
  return{
    Text:globalThis.document.createTextNode(""), 
    Dirty:false, 
    Value:""
  };
}
function UpdateTextNode(n, t){
  n.Value=t;
  n.Dirty=true;
}
function SyncElement(el){
  function hasDirtyChildren(el_1){
    function dirty(doc_2){
      while(true)
        {
          if(doc_2!=null&&doc_2.$==0){
            const b=doc_2.$1;
            const a=doc_2.$0;
            if(dirty(a))return true;
            else doc_2=b;
          }
          else if(doc_2!=null&&doc_2.$==2){
            const d=doc_2.$0;
            if(d.Dirty)return true;
            else doc_2=d.Current;
          }
          else if(doc_2!=null&&doc_2.$==6){
            const t=doc_2.$0;
            return t.Dirty||exists(hasDirtyChildren, t.Holes);
          }
          else return false;
        }
    }
    return dirty(el_1.Children);
  }
  Sync_1(el.El, el.Attr);
  if(hasDirtyChildren(el))DoSyncElement(el);
}
function Sync(doc_2){
  while(true)
    {
      if(doc_2!=null&&doc_2.$==1)return SyncElemNode(false, doc_2.$0);
      else if(doc_2!=null&&doc_2.$==2){
        const n=doc_2.$0;
        doc_2=n.Current;
      }
      else if(doc_2==null)return null;
      else if(doc_2!=null&&doc_2.$==5)return null;
      else if(doc_2!=null&&doc_2.$==4){
        const d=doc_2.$0;
        return d.Dirty?(d.Text.nodeValue=d.Value,d.Dirty=false):null;
      }
      else if(doc_2!=null&&doc_2.$==6){
        const t=doc_2.$0;
        iter((h) => {
          SyncElemNode(false, h);
        }, t.Holes);
        iter((t_1) => {
          Sync_1(t_1[0], t_1[1]);
        }, t.Attrs);
        return AfterRender(t);
      }
      else {
        const b=doc_2.$1;
        const a=doc_2.$0;
        Sync(a);
        doc_2=b;
      }
    }
}
function AfterRender(el){
  const m=GetOptional(el.Render);
  if(m!=null&&m.$==1){
    m.$0(el.El);
    SetOptional(el, "Render", null);
  }
}
function DoSyncElement(el){
  const parent=el.El;
  function ins(doc_2, pos){
    while(true)
      {
        if(doc_2!=null&&doc_2.$==1)return doc_2.$0.El;
        else if(doc_2!=null&&doc_2.$==2){
          const d=doc_2.$0;
          if(d.Dirty){
            d.Dirty=false;
            return InsertDoc(parent, d.Current, pos);
          }
          else doc_2=d.Current;
        }
        else if(doc_2==null)return pos;
        else if(doc_2!=null&&doc_2.$==4)return doc_2.$0.Text;
        else if(doc_2!=null&&doc_2.$==5)return doc_2.$0;
        else if(doc_2!=null&&doc_2.$==6){
          const t=doc_2.$0;
          if(t.Dirty)t.Dirty=false;
          return foldBack((_3, _4) => _3==null||_3.constructor===Object?ins(_3, _4):_3, t.Els, pos);
        }
        else {
          const b=doc_2.$1;
          const a=doc_2.$0;
          doc_2=a;
          pos=ins(b, pos);
        }
      }
  }
  const p=el.El;
  Iter((e) => {
    RemoveNode(p, e);
  }, Except_2(DocChildren(el), Children(el.El, GetOptional(el.Delimiters))));
  let _1=el.Children;
  const m=GetOptional(el.Delimiters);
  let _2=m!=null&&m.$==1?m.$0[1]:null;
  ins(_1, _2);
}
function New_33(shape, label_1, badge, className){
  return{
    shape:shape, 
    label:label_1, 
    badge:badge, 
    className:className
  };
}
function New_34(pageId, title, setName, shape, tabId, tabMode, path, description){
  return{
    pageId:pageId, 
    title:title, 
    setName:setName, 
    shape:shape, 
    tabId:tabId, 
    tabMode:tabMode, 
    path:path, 
    description:description
  };
}
function notPresent(){
  throw new KeyNotFoundException("New");
}
function alreadyAdded(){
  throw new ArgumentException("New_2", "An item with the same key has already been added.");
}
class FSharpMap extends Object_1 {
  tree;
  TryFind(k){
    const o=TryFind(Pair.New(k, void 0), this.tree);
    return o==null?null:Some(o.$0.Value);
  }
  Equals(other){
    return this.Count===other.Count&&forall2_1(Equals, this, other);
  }
  get Count(){
    const tree=this.tree;
    return tree==null?0:tree.Count;
  }
  GetEnumerator(){
    return Get(map_1((kv) =>({K:kv.Key, V:kv.Value}), Enumerate(false, this.tree)));
  }
  GetHashCode(){
    return Hash(ofSeq(this));
  }
  CompareTo0(other){
    return compareWith((_1, _2) => Compare(_1, _2), this, other);
  }
  constructor(i, _1){
    let s;
    if(i=="New"){
      s=_1;
      i="New_1";
      _1=fromSeq(s);
    }
    if(i=="New_1"){
      const tree=_1;
      super();
      this.tree=tree;
    }
  }
}
class Pair {
  Key;
  Value;
  Equals(other){
    return Equals(this.Key, other.Key);
  }
  GetHashCode(){
    return Hash(this.Key);
  }
  CompareTo0(other){
    return Compare(this.Key, other.Key);
  }
  static New(Key, Value){
    return Create_1(Pair, {Key:Key, Value:Value});
  }
}
function OfSeq(data){
  const a=ofSeq(distinct_1(data));
  sortInPlace(a);
  return Build(a, 0, a.length-1);
}
function TryFind(v, t){
  const x=(Lookup(v, t))[0];
  return x==null?null:Some(x.Node);
}
function Lookup(k, t){
  let spine, t_1, loop;
  spine=[];
  t_1=t;
  loop=true;
  while(loop)
    if(t_1==null)loop=false;
    else {
      const m=Compare(k, t_1.Node);
      if(m===0)loop=false;
      else m===1?(spine.unshift([true, t_1.Node, t_1.Left]),t_1=t_1.Right):(spine.unshift([false, t_1.Node, t_1.Right]),t_1=t_1.Left);
    }
  return[t_1, spine];
}
function Build(data, min, max_1){
  if(max_1-min+1<=0)return null;
  else {
    const center=(min+max_1)/2>>0;
    return Branch(get(data, center), Build(data, min, center-1), Build(data, center+1, max_1));
  }
}
function Branch(node, left, right){
  const a=left==null?0:left.Height;
  const b=right==null?0:right.Height;
  let _1=Compare(a, b)===1?a:b;
  let _2=1+_1;
  return New_36(node, left, right, _2, 1+(left==null?0:left.Count)+(right==null?0:right.Count));
}
function Enumerate(flip, t){
  function gen(t_1, spine){
    let t_2;
    while(true)
      {
        if(t_1==null){
          if(spine.$==1){
            const t_3=spine.$0[0];
            const spine_1=spine.$1;
            return Some([t_3, [spine.$0[1], spine_1]]);
          }
          else return null;
        }
        else if(flip){
          t_2=t_1;
          t_1=t_2.Right;
          spine=FSharpList.Cons([t_2.Node, t_2.Left], spine);
        }
        else {
          t_2=t_1;
          t_1=t_2.Left;
          spine=FSharpList.Cons([t_2.Node, t_2.Right], spine);
        }
      }
  }
  return unfold((_1) => gen(_1[0], _1[1]), [t, FSharpList.Empty]);
}
function groupBy(f, a){
  const d=new Dictionary("New_5");
  const keys=[];
  for(let i=0, _1=length(a)-1;i<=_1;i++){
    const c=a[i];
    const k=f(c);
    if(d.ContainsKey(k))d.Item(k).push(c);
    else {
      keys.push(k);
      d.DAdd(k, [c]);
    }
  }
  mapInPlace((k_1) =>[k_1, d.Item(k_1)], keys);
  return keys;
}
function nonNegative(){
  return FailWith("The input must be non-negative.");
}
function insufficient(){
  return FailWith("The input sequence has an insufficient number of elements.");
}
function mapInPlace(f, arr){
  for(let i=0, _1=arr.length-1;i<=_1;i++)arr[i]=f(arr[i]);
}
function mapiInPlace(f, arr){
  for(let i=0, _1=arr.length-1;i<=_1;i++)arr[i]=f(i, arr[i]);
  return arr;
}
function arrContains(item, arr){
  let c, i;
  c=true;
  i=0;
  const l=length(arr);
  while(c&&i<l)
    if(Equals(arr[i], item))c=false;
    else i=i+1;
  return!c;
}
function buildRawArguFromValues(fields){
  const parts=MarkResizable([]);
  iter((_1) => appendFieldParts(parts, _1[0], _1[1]), arrayOrEmpty_2(fields));
  return Join_1(" ", ofSeq(parts));
}
function arrayOrEmpty_2(values){
  return values==null?[]:values;
}
function appendFieldParts(parts, field, values){
  const values_1=filter((value_3) => value_3.length>0, map((a) => Trim(a), map(asText_3, arrayOrEmpty_2(values))));
  const m=asText_3(field.kind);
  if(m=="bool"){
    const o=tryHead(values_1);
    const o_1=o==null?null:Some(o.$0.toLowerCase());
    if(o_1==null)void 0;
    else {
      const value=o_1.$0;
      if(value=="true"||value=="1"||value=="yes")parts.push(asText_3(field.arguName));
    }
  }
  else if(m=="bool-value"){
    const o_2=tryHead(values_1);
    if(o_2==null)void 0;
    else {
      const value_1=o_2.$0;
      parts.push(asText_3(field.arguName));
      parts.push(quoteArg(value_1));
    }
  }
  else if(m=="list")length(values_1)>0?(parts.push(asText_3(field.arguName)),iter((x) => {
    parts.push(quoteArg(x));
  }, values_1)):void 0;
  else if(m=="tuple")length(values_1)>0?(parts.push(asText_3(field.arguName)),iter((x) => {
    parts.push(quoteArg(x));
  }, values_1)):void 0;
  else {
    const o_3=tryHead(values_1);
    if(o_3==null)void 0;
    else {
      const value_2=o_3.$0;
      parts.push(asText_3(field.arguName));
      parts.push(quoteArg(value_2));
    }
  }
}
function asText_3(value){
  return value==null?"":value;
}
function quoteArg(value){
  const text=asText_3(value);
  return text.length===0?"\"\"":exists_1(IsWhiteSpace, text)||text.indexOf("\"")!=-1?"\""+Replace(Replace(text, "\\", "\\\\"), "\"", "\\\"")+"\"":text;
}
function Int(){
  set_counter(counter()+1);
  return counter();
}
function set_counter(_1){
  _c_5.counter=_1;
}
function counter(){
  return _c_5.counter;
}
function Ready(Item1, Item2){
  return{
    $:2, 
    $0:Item1, 
    $1:Item2
  };
}
function Forever(Item){
  return{$:0, $0:Item};
}
function Waiting(Item1, Item2){
  return{
    $:3, 
    $0:Item1, 
    $1:Item2
  };
}
function Dynamic_1(view, set_1){
  return Attr.A1(new DynamicAttrNode(view, set_1));
}
function Updates(dyn){
  return MapTreeReduce((x) => x.NChanged, Const(), Map2Unit, dyn.DynNodes);
}
function AppendTree(a, b){
  if(a===null)return b;
  else if(b===null)return a;
  else {
    const x=Attr.A2(a, b);
    SetFlags(x, Flags(a)|Flags(b));
    return x;
  }
}
function EmptyAttr(){
  return _c_7.EmptyAttr;
}
function Insert(elem, tree){
  const nodes=[];
  const oar=[];
  function loop(node){
    while(true)
      {
        if(!(node===null)){
          if(node!=null&&node.$==1)return nodes.push(node.$0);
          else if(node!=null&&node.$==2){
            const b=node.$1;
            const a=node.$0;
            loop(a);
            node=b;
          }
          else return node!=null&&node.$==3?node.$0(elem):node!=null&&node.$==4?oar.push(node.$0):null;
        }
        else return null;
      }
  }
  loop(tree);
  const arr=nodes.slice(0);
  let _1=New_37(elem, Flags(tree), arr, oar.length===0?null:Some((el) => {
    iter_1((f) => {
      f(el);
    }, oar);
  }));
  return _1;
}
function HasExitAnim(attr_1){
  const flag=2;
  return(attr_1.DynFlags&flag)===flag;
}
function GetExitAnim(dyn){
  return GetAnim(dyn, (_1, _2) => _1.NGetExitAnim(_2));
}
function HasEnterAnim(attr_1){
  const flag=1;
  return(attr_1.DynFlags&flag)===flag;
}
function GetEnterAnim(dyn){
  return GetAnim(dyn, (_1, _2) => _1.NGetEnterAnim(_2));
}
function HasChangeAnim(attr_1){
  const flag=4;
  return(attr_1.DynFlags&flag)===flag;
}
function GetChangeAnim(dyn){
  return GetAnim(dyn, (_1, _2) => _1.NGetChangeAnim(_2));
}
function SetFlags(a, f){
  a.flags=f;
}
function Flags(a){
  return a!==null&&a.hasOwnProperty("flags")?a.flags:0;
}
function GetAnim(dyn, f){
  return Concat(map((n) => f(n, dyn.DynElem), dyn.DynNodes));
}
function Sync_1(elem, dyn){
  iter((d) => {
    d.NSync(elem);
  }, dyn.DynNodes);
}
function ParseHTMLIntoFakeRoot(elem){
  const root=globalThis.document.createElement("div");
  if(!rhtml().test(elem)){
    root.appendChild(globalThis.document.createTextNode(elem));
    return root;
  }
  else {
    const m=rtagName().exec(elem);
    const tag=Equals(m, null)?"":get(m, 1).toLowerCase();
    const w=(wrapMap())[tag];
    const p=w?w:defaultWrap();
    root.innerHTML=p[1]+elem.replace(rxhtmlTag(), "<$1></$2>")+p[2];
    function unwrap(elt, a){
      while(true)
        {
          if(a===0)return elt;
          else {
            const i=a;
            elt=elt.lastChild;
            a=i-1;
          }
        }
    }
    return(((a) => {
      const _1=a;
      return(_2) => unwrap(_1, _2);
    })(root))(p[0]);
  }
}
function rhtml(){
  return _c_8.rhtml;
}
function wrapMap(){
  return _c_8.wrapMap;
}
function defaultWrap(){
  return _c_8.defaultWrap;
}
function rxhtmlTag(){
  return _c_8.rxhtmlTag;
}
function rtagName(){
  return _c_8.rtagName;
}
function IterSelector(el, selector, f){
  const l=el.querySelectorAll(selector);
  for(let i=0, _1=l.length-1;i<=_1;i++)f(l[i]);
}
function InsertAt(parent, pos, node){
  let _1;
  if(node.parentNode===parent){
    const m=node.nextSibling;
    let _2=Equals(m, null)?null:m;
    _1=pos===_2;
  }
  else _1=false;
  if(!_1)parent.insertBefore(node, pos);
}
function RemoveNode(parent, el){
  if(el.parentNode===parent)parent.removeChild(el);
}
function TextNodeDoc(Item){
  return{$:5, $0:Item};
}
function EmbedDoc(Item){
  return{$:2, $0:Item};
}
function AppendDoc(Item1, Item2){
  return{
    $:0, 
    $0:Item1, 
    $1:Item2
  };
}
function ElemDoc(Item){
  return{$:1, $0:Item};
}
function TextDoc(Item){
  return{$:4, $0:Item};
}
function Obsolete(sn){
  let _1;
  const m=sn.s;
  if(m==null||(m!=null&&m.$==2?(_1=m.$1,false):m!=null&&m.$==3?(_1=m.$1,false):true))void 0;
  else {
    sn.s=null;
    for(let i=0, _2=length(_1)-1;i<=_2;i++){
      const o=get(_1, i);
      if(typeof o=="object")(((sn_1) => {
        Obsolete(sn_1);
      })(o));
      else o();
    }
  }
}
class View { }
let _c_3=Lazy((_i) => class $StartupCode_Templates {
  static {
    _c_3=_i(this);
  }
  static RenderedFullDocTemplate;
  static TextHoleRE;
  static GlobalHoles;
  static LocalTemplatesLoaded;
  static LoadedTemplates;
  static {
    this.LoadedTemplates=new Dictionary("New_5");
    this.LocalTemplatesLoaded=false;
    this.GlobalHoles=new Dictionary("New_5");
    this.TextHoleRE="\\${([^}]+)}";
    this.RenderedFullDocTemplate=null;
  }
});
function get_UseAnimations(){
  return UseAnimations();
}
function Play(anim){
  return Delay(() => Bind_1(Run(() => { }, Actions(anim)), () => {
    Finalize(anim);
    return Return(null);
  }));
}
function Append(a, a_1){
  return Anim(Append_1(a.$0, a_1.$0));
}
function Run(k, anim){
  const dur=anim.Duration;
  if(dur===0)return Zero();
  else {
    const c=(ok) => {
      function loop(start){
        return(now) => {
          const t=now-start;
          anim.Compute(t);
          k();
          return t<=dur?void requestAnimationFrame((t_1) => {
            (loop(start))(t_1);
          }):ok();
        };
      }
      requestAnimationFrame((t) => {
        (loop(t))(t);
      });
    };
    return FromContinuations((_1, _2, _3) => c.apply(null, [_1, _2, _3]));
  }
}
function Anim(Item){
  return{$:0, $0:Item};
}
function Concat(xs){
  return Anim(Concat_1(map_1(List, xs)));
}
function get_Empty(){
  return Anim(Empty());
}
function BatchUpdatesEnabled(){
  return _c_4.BatchUpdatesEnabled;
}
function StartProcessor(procAsync){
  const st=[0];
  function work(){
    return Delay(() => Bind_1(procAsync, () => {
      const m=st[0];
      return Equals(m, 1)?(st[0]=0,Zero()):Equals(m, 2)?(st[0]=1,work()):Zero();
    }));
  }
  return() => {
    const m=st[0];
    if(Equals(m, 0)){
      st[0]=1;
      Start(work(), null);
    }
    else Equals(m, 1)?st[0]=2:void 0;
  };
}
class Elt extends Doc {
  docNode_1;
  updates_1;
  elt;
  rvUpdates;
  static New(el, attr_1, children){
    const node=CreateElemNode(el, attr_1, children.docNode);
    const rvUpdates=Updates_1.Create(children.updates);
    return new Elt(ElemDoc(node), Map2Unit(Updates(node.Attr), rvUpdates.v), el, rvUpdates);
  }
  constructor(docNode, updates, elt, rvUpdates){
    super(docNode, updates);
    this.docNode_1=docNode;
    this.updates_1=updates;
    this.elt=elt;
    this.rvUpdates=rvUpdates;
  }
}
class DocElemNode {
  Attr;
  Children;
  Delimiters;
  El;
  ElKey;
  Render;
  Equals(o){
    return this.ElKey===o.ElKey;
  }
  GetHashCode(){
    return this.ElKey;
  }
  static New(Attr_1, Children_1, Delimiters, El, ElKey, Render){
    const _1={
      Attr:Attr_1, 
      Children:Children_1, 
      El:El, 
      ElKey:ElKey
    };
    let _2=(SetOptional(_1, "Delimiters", Delimiters),SetOptional(_1, "Render", Render),_1);
    return Create_1(DocElemNode, _2);
  }
}
class TemplateHole extends Object_1 { }
function convertTextNode(n){
  let m, li;
  m=null;
  li=0;
  const s=n.textContent;
  const strRE=new RegExp(TextHoleRE(), "g");
  while(m=strRE.exec(s),m!==null)
    {
      n.parentNode.insertBefore(globalThis.document.createTextNode(string(s, Some(li), Some(strRE.lastIndex-get(m, 0).length-1))), n);
      li=strRE.lastIndex;
      const hole=globalThis.document.createElement("span");
      hole.setAttribute("ws-replace", get(m, 1).toLowerCase());
      n.parentNode.insertBefore(hole, n);
    }
  strRE.lastIndex=0;
  n.textContent=string(s, Some(li), null);
}
function failNotLoaded(name){
  console.warn("Instantiating non-loaded template", name);
}
function fillTextHole(instance, fillWith, templateName){
  const m=instance.querySelector("[ws-replace]");
  return Equals(m, null)?(console.warn("Filling non-existent text hole", templateName),null):(m.parentNode.replaceChild(globalThis.document.createTextNode(fillWith), m),Some(m.getAttribute("ws-replace")));
}
function removeHolesExcept(instance, dontRemove){
  const run=(attrName) => {
    foreachNotPreserved(instance, "["+attrName+"]", (e) => {
      if(!dontRemove.Contains(e.getAttribute(attrName)))e.removeAttribute(attrName);
    });
  };
  run("ws-attr");
  run("ws-onafterrender");
  run("ws-var");
  foreachNotPreserved(instance, "[ws-hole]", (e) => {
    if(!dontRemove.Contains(e.getAttribute("ws-hole"))){
      e.removeAttribute("ws-hole");
      while(e.hasChildNodes())
        e.removeChild(e.lastChild);
    }
  });
  foreachNotPreserved(instance, "[ws-replace]", (e) => {
    if(!dontRemove.Contains(e.getAttribute("ws-replace")))e.parentNode.removeChild(e);
  });
  foreachNotPreserved(instance, "[ws-on]", (e) => {
    e.setAttribute("ws-on", concat_2(" ", filter((x) => dontRemove.Contains(get(SplitChars(x, [":"], 1), 1)), SplitChars(e.getAttribute("ws-on"), [" "], 1))));
  });
  foreachNotPreserved(instance, "[ws-attr-holes]", (e) => {
    const holeAttrs=SplitChars(e.getAttribute("ws-attr-holes"), [" "], 1);
    for(let i=0, _2=holeAttrs.length-1;i<=_2;i++){
      const attrName=get(holeAttrs, i);
      let this_1=new RegExp(TextHoleRE(), "g");
      let str=e.getAttribute(attrName);
      let replaceFn=(_3, _4) => dontRemove.Contains(_4)?_3:"";
      let _1=str.replace(this_1, replaceFn);
      e.setAttribute(attrName, _1);
    }
  });
}
function fillInstanceAttrs(instance, fillWith){
  convertAttrs(fillWith);
  const name=fillWith.nodeName.toLowerCase();
  const m=instance.querySelector("[ws-attr="+name+"]");
  if(Equals(m, null))console.warn("Filling non-existent attr hole", name);
  else {
    m.removeAttribute("ws-attr");
    for(let i=0, _1=fillWith.attributes.length-1;i<=_1;i++){
      const a=fillWith.attributes.item(i);
      if(a.name=="class"&&m.hasAttribute("class"))m.setAttribute("class", m.getAttribute("class")+" "+a.nodeValue);
      else m.setAttribute(a.name, a.nodeValue);
    }
  }
}
function mapHoles(t, mappings){
  const run=(attrName) => {
    foreachNotPreserved(t, "["+attrName+"]", (e) => {
      let o;
      const m=(o=null,[mappings.TryGetValue(e.getAttribute(attrName).toLowerCase(), {get:() => o, set:(v) => {
        o=v;
      }}), o]);
      if(m[0])e.setAttribute(attrName, m[1]);
    });
  };
  run("ws-hole");
  run("ws-replace");
  run("ws-attr");
  run("ws-onafterrender");
  run("ws-var");
  foreachNotPreserved(t, "[ws-on]", (e) => {
    e.setAttribute("ws-on", concat_2(" ", map((x) => {
      let o;
      const a=SplitChars(x, [":"], 1);
      const m=(o=null,[mappings.TryGetValue(get(a, 1), {get:() => o, set:(v) => {
        o=v;
      }}), o]);
      return m[0]?get(a, 0)+":"+m[1]:x;
    }, SplitChars(e.getAttribute("ws-on"), [" "], 1))));
  });
  foreachNotPreserved(t, "[ws-attr-holes]", (e) => {
    const holeAttrs=SplitChars(e.getAttribute("ws-attr-holes"), [" "], 1);
    for(let i=0, _1=holeAttrs.length-1;i<=_1;i++)((() => {
      const attrName=get(holeAttrs, i);
      return e.setAttribute(attrName, fold_1((_2, _3) => {
        const a=KeyValue(_3);
        return _2.replace(new RegExp("\\${"+a[0]+"}", "ig"), "${"+a[1]+"}");
      }, e.getAttribute(attrName), mappings));
    })());
  });
}
function fill(fillWith, p, n){
  while(true)
    {
      if(fillWith.hasChildNodes())n=p.insertBefore(fillWith.lastChild, n);
      else return null;
    }
}
function convertAttrs(el){
  const attrs=el.attributes;
  const toRemove=[];
  const events=[];
  const holedAttrs=[];
  for(let i=0, _2=attrs.length-1;i<=_2;i++){
    const a=attrs.item(i);
    if(StartsWith(a.nodeName, "ws-on")&&a.nodeName!="ws-onafterrender"&&a.nodeName!="ws-on"){
      toRemove.push(a.nodeName);
      events.push(string(a.nodeName, Some("ws-on".length), null)+":"+a.nodeValue.toLowerCase());
    }
    else if(!StartsWith(a.nodeName, "ws-")&&(new RegExp(TextHoleRE())).test(a.nodeValue)){
      let this_1=new RegExp(TextHoleRE(), "g");
      let str=a.nodeValue;
      let replaceFn=(_3, _4) =>"${"+_4.toLowerCase()+"}";
      let _1=str.replace(this_1, replaceFn);
      a.nodeValue=_1;
      holedAttrs.push(a.nodeName);
    }
    else void 0;
  }
  if(!(events.length==0))el.setAttribute("ws-on", concat_2(" ", events));
  if(!(holedAttrs.length==0))el.setAttribute("ws-attr-holes", concat_2(" ", holedAttrs));
  const lowercaseAttr=(name) => {
    const m=el.getAttribute(name);
    if(m==null){ }
    else el.setAttribute(name, m.toLowerCase());
  };
  lowercaseAttr("ws-hole");
  lowercaseAttr("ws-replace");
  lowercaseAttr("ws-attr");
  lowercaseAttr("ws-onafterrender");
  lowercaseAttr("ws-var");
  iter((a_1) => {
    el.removeAttribute(a_1);
  }, toRemove);
}
function string(source, start, finish){
  if(start==null){
    if(finish!=null&&finish.$==1){
      const f=finish.$0;
      return f<0?"":source.slice(0, f+1);
    }
    else return"";
  }
  else if(finish==null)return source.slice(start.$0);
  else {
    const f_1=finish.$0;
    const s=start.$0;
    return f_1<0?"":source.slice(s, f_1+1);
  }
}
class KeyCollection extends Object_1 {
  d;
  GetEnumerator(){
    return Get(map_1((kvp) => kvp.K, this.d));
  }
  constructor(d){
    super();
    this.d=d;
  }
}
function New_35(PreviousNodes, Top){
  return{PreviousNodes:PreviousNodes, Top:Top};
}
function get_Empty_1(){
  return NodeSet(new HashSet("New_3"));
}
function FindAll(doc_2){
  const q=[];
  function recF(recI, _1){
    while(true)
      switch(recI){
        case 0:
          if(_1!=null&&_1.$==0){
            const b=_1.$1;
            const a=_1.$0;
            recF(0, a);
            _1=b;
          }
          else if(_1!=null&&_1.$==1){
            const el=_1.$0;
            _1=el;
            recI=1;
          }
          else if(_1!=null&&_1.$==2){
            const em=_1.$0;
            _1=em.Current;
          }
          else if(_1!=null&&_1.$==6){
            const x=_1.$0.Holes;
            return(((a_1) =>(a_2) => {
              iter(a_1, a_2);
            })(loopEN))(x);
          }
          else return null;
          break;
        case 1:
          q.push(_1);
          _1=_1.Children;
          recI=0;
          break;
      }
  }
  function loop(node){
    return recF(0, node);
  }
  function loopEN(el){
    return recF(1, el);
  }
  loop(doc_2);
  return NodeSet(new HashSet("New_2", q));
}
function NodeSet(Item){
  return{$:0, $0:Item};
}
function Filter(f, a){
  return NodeSet(Filter_1(f, a.$0));
}
function Except(a, a_1){
  return NodeSet(Except_1(a.$0, a_1.$0));
}
function ToArray(a){
  return ToArray_2(a.$0);
}
function Intersect(a, a_1){
  return NodeSet(Intersect_1(a.$0, a_1.$0));
}
function Delay(mk){
  return(c) => {
    try {
      (mk())(c);
    }
    catch(e){
      c.k(No(e));
    }
  };
}
function Bind_1(r, f){
  return checkCancel((c) => {
    r(New_38((a) => {
      if(a.$==0){
        const x=a.$0;
        scheduler().Fork(() => {
          try {
            (f(x))(c);
          }
          catch(e){
            c.k(No(e));
          }
        });
      }
      else scheduler().Fork(() => {
        c.k(a);
      });
    }, c.ct));
  });
}
function Zero(){
  return _c_9.Zero;
}
function Start(c, ctOpt){
  const d=(defCTS())[0];
  const ct=ctOpt==null?d:ctOpt.$0;
  scheduler().Fork(() => {
    if(!ct.c)c(New_38((a) => {
      if(a.$==1)UncaughtAsyncError(a.$0);
    }, ct));
  });
}
function Return(x){
  return(c) => {
    c.k(Ok(x));
  };
}
function scheduler(){
  return _c_9.scheduler;
}
function checkCancel(r){
  return(c) => {
    if(c.ct.c)cancel(c);
    else r(c);
  };
}
function defCTS(){
  return _c_9.defCTS;
}
function UncaughtAsyncError(e){
  console.log("WebSharper: Uncaught asynchronous exception", e);
}
function FromContinuations(subscribe){
  return(c) => {
    const continued=[false];
    const once=(cont) => {
      if(continued[0])FailWith("A continuation provided by Async.FromContinuations was invoked multiple times");
      else {
        continued[0]=true;
        scheduler().Fork(cont);
      }
    };
    subscribe((a) => {
      once(() => {
        c.k(Ok(a));
      });
    }, (e) => {
      once(() => {
        c.k(No(e));
      });
    }, (e) => {
      once(() => {
        c.k(Cc(e));
      });
    });
  };
}
function cancel(c){
  c.k(Cc(new OperationCanceledException("New", c.ct)));
}
function UseAnimations(){
  return _c_6.UseAnimations;
}
function Actions(a){
  return ConcatActions(choose((a_1) => a_1.$==1?Some(a_1.$0):null, ToArray_1(a.$0)));
}
function Finalize(a){
  iter((a_1) => {
    if(a_1.$==0)a_1.$0();
  }, ToArray_1(a.$0));
}
function ConcatActions(xs){
  const xs_1=ofSeqNonCopying(xs);
  const m=length(xs_1);
  if(m===0)return Const_1();
  else if(m===1)return get(xs_1, 0);
  else {
    const dur=max(map_1((anim) => anim.Duration, xs_1));
    const xs_2=map((x) => Prolong(dur, x), xs_1);
    return Def(dur, (t) => {
      iter((anim) => {
        anim.Compute(t);
      }, xs_2);
    });
  }
}
function List(a){
  return a.$0;
}
function Const_1(v){
  return Def(0, () => v);
}
function Def(d, f){
  return{Compute:f, Duration:d};
}
function Prolong(nextDuration, anim){
  const comp=anim.Compute;
  const dur=anim.Duration;
  const last=Create(() => anim.Compute(anim.Duration));
  return{Compute:(t) => t>=dur?last.f():comp(t), Duration:nextDuration};
}
let _c_4=Lazy((_i) => class Proxy {
  static {
    _c_4=_i(this);
  }
  static BatchUpdatesEnabled;
  static {
    this.BatchUpdatesEnabled=true;
  }
});
function New_36(Node_1, Left, Right, Height, Count){
  return{
    Node:Node_1, 
    Left:Left, 
    Right:Right, 
    Height:Height, 
    Count:Count
  };
}
function fromSeq(s){
  const a=ofSeq(map_1((_1) => Pair.New(_1[0], _1[1]), distinctBy_1((t) => t[0], rev(s))));
  sortInPlace(a);
  return Build(a, 0, a.length-1);
}
let _c_5=Lazy((_i) => class $StartupCode_Abbrev {
  static {
    _c_5=_i(this);
  }
  static counter;
  static {
    this.counter=0;
  }
});
class Updates_1 {
  c;
  s;
  v;
  static Create(v){
    let var_1;
    var_1=null;
    var_1=Updates_1.New(v, null, () => {
      let c;
      c=var_1.s;
      return c===null?(c=Copy(var_1.c()),var_1.s=c,WhenObsoleteRun(c, () => {
        var_1.s=null;
      }),c):c;
    });
    return var_1;
  }
  static New(Current, Snap, VarView){
    return Create_1(Updates_1, {
      c:Current, 
      s:Snap, 
      v:VarView
    });
  }
}
function New_37(DynElem, DynFlags, DynNodes, OnAfterRender_1){
  const _1={
    DynElem:DynElem, 
    DynFlags:DynFlags, 
    DynNodes:DynNodes
  };
  SetOptional(_1, "OnAfterRender", OnAfterRender_1);
  return _1;
}
class DynamicAttrNode extends Object_1 {
  push;
  value;
  dirty;
  updates;
  get NChanged(){
    return this.updates;
  }
  NGetExitAnim(parent){
    return get_Empty();
  }
  NGetEnterAnim(parent){
    return get_Empty();
  }
  NGetChangeAnim(parent){
    return get_Empty();
  }
  NSync(parent){
    if(this.dirty){
      (this.push(parent))(this.value);
      this.dirty=false;
    }
  }
  constructor(view, push){
    super();
    this.push=push;
    this.value=void 0;
    this.dirty=false;
    this.updates=Map((x) => {
      this.value=x;
      this.dirty=true;
    }, view);
  }
}
let _c_6=Lazy((_i) => class $StartupCode_Animation {
  static {
    _c_6=_i(this);
  }
  static UseAnimations;
  static CubicInOut;
  static {
    this.CubicInOut=Easing.Custom((t) => {
      const t2=t*t;
      return 3*t2-2*(t2*t);
    });
    this.UseAnimations=true;
  }
});
function Append_1(x, y){
  return x.$==0?y:y.$==0?x:{
    $:2, 
    $0:x, 
    $1:y
  };
}
function ToArray_1(xs){
  const out=[];
  function loop(xs_1){
    while(true)
      {
        if(xs_1.$==1)return out.push(xs_1.$0);
        else if(xs_1.$==2){
          const y=xs_1.$1;
          const x=xs_1.$0;
          loop(x);
          xs_1=y;
        }
        else return xs_1.$==3?iter((v) => {
          out.push(v);
        }, xs_1.$0):null;
      }
  }
  loop(xs);
  return out.slice(0);
}
function Concat_1(xs){
  const x=ofSeqNonCopying(xs);
  return TreeReduce(Empty(), Append_1, x);
}
function Empty(){
  return _c_10.Empty;
}
function concat_3(o){
  let r=[];
  let k;
  for(var k_1 in o)r.push.apply(r, o[k_1]);
  return r;
}
function TryParseBigInt(s, min, max_1, r){
  let o, _1;
  o=0n;
  try {
    _1=(o=BigInt(s),true);
  }
  catch(m_1){
    _1=false;
  }
  const m=[_1, o];
  if(m[0]){
    const x=m[1];
    const ok=x===x-x%1n&&x>=min&&x<=max_1;
    if(ok)r.set(x);
    return ok;
  }
  else return false;
}
function TryParse_2(s, min, max_1, r){
  const x=+s;
  const ok=x===x-x%1&&x>=min&&x<=max_1;
  if(ok)r.set(x);
  return ok;
}
function IsWhiteSpace(c){
  return c.match(new RegExp("\\s"))!==null;
}
let _c_7=Lazy((_i) => class Client {
  static {
    _c_7=_i(this);
  }
  static FloatApplyChecked;
  static FloatGetChecked;
  static FloatSetChecked;
  static FloatApplyUnchecked;
  static FloatGetUnchecked;
  static FloatSetUnchecked;
  static IntApplyChecked;
  static IntGetChecked;
  static IntSetChecked;
  static IntApplyUnchecked;
  static IntGetUnchecked;
  static IntSetUnchecked;
  static FileApplyUnchecked;
  static FileGetUnchecked;
  static FileSetUnchecked;
  static DateTimeApplyUnchecked;
  static DateTimeGetUnchecked;
  static DateTimeSetUnchecked;
  static StringListApply;
  static StringListGet;
  static StringListSet;
  static StringApply;
  static StringGet;
  static StringSet;
  static BoolCheckedApply;
  static EmptyAttr;
  static {
    this.EmptyAttr=null;
    this.BoolCheckedApply=(var_1) =>[(el) => {
      el.addEventListener("change", () => var_1.Get()!=el.checked?var_1.Set(el.checked):null);
    }, (_1) =>(_2) => _2!=null&&_2.$==1?void(_1.checked=_2.$0):null, Map((V_2) => Some(V_2), var_1.View)];
    this.StringSet=(el) =>(s_8) => {
      el.value=s_8;
    };
    this.StringGet=(el) => Some(el.value);
    const g=StringGet();
    const s=StringSet();
    this.StringApply=(v) => ApplyValue(g, s, v);
    this.StringListSet=(el) =>(s_8) => {
      const options_=el.options;
      for(let i=0, _1=options_.length-1;i<=_1;i++)((() => {
        const option=options_.item(i);
        option.selected=arrContains(option.value, s_8);
      })());
    };
    this.StringListGet=(el) => {
      const selectedOptions=el.selectedOptions;
      return Some(ofSeq(delay(() => collect_1((i) =>[selectedOptions.item(i).value], range(0, selectedOptions.length-1)))));
    };
    const g_1=StringListGet();
    const s_1=StringListSet();
    this.StringListApply=(v) => ApplyValue(g_1, s_1, v);
    this.DateTimeSetUnchecked=(el) =>(i) => {
      el.value=(new Date(i)).toLocaleString();
    };
    this.DateTimeGetUnchecked=(el) => {
      let o, m;
      const s_8=el.value;
      if(isBlank_3(s_8))return Some(-8640000000000000);
      else {
        o=0;
        const m_1=TryParse_3(s_8);
        let _1=m_1!=null&&m_1.$==1&&(o=m_1.$0,true);
        m=[_1, o];
        return m[0]?Some(m[1]):null;
      }
    };
    const g_2=DateTimeGetUnchecked();
    const s_2=DateTimeSetUnchecked();
    this.DateTimeApplyUnchecked=(v) => ApplyValue(g_2, s_2, v);
    this.FileSetUnchecked=() =>() => null;
    this.FileGetUnchecked=(el) => {
      const files=el.files;
      return Some(ofSeq(delay(() => map_1((i) => files.item(i), range(0, files.length-1)))));
    };
    const g_3=FileGetUnchecked();
    const s_3=FileSetUnchecked();
    this.FileApplyUnchecked=(v) => FileApplyValue(g_3, s_3, v);
    this.IntSetUnchecked=(el) =>(i) => {
      el.value=String(i);
    };
    this.IntGetUnchecked=(el) => {
      const s_8=el.value;
      if(isBlank_3(s_8))return Some(0);
      else {
        const pd=+s_8;
        return pd!==pd>>0?null:Some(pd);
      }
    };
    const g_4=IntGetUnchecked();
    const s_4=IntSetUnchecked();
    this.IntApplyUnchecked=(v) => ApplyValue(g_4, s_4, v);
    this.IntSetChecked=(el) =>(i) => {
      const i_1=i.Input;
      return el.value!=i_1?void(el.value=i_1):null;
    };
    this.IntGetChecked=(el) => {
      let _1, o;
      const s_8=el.value;
      if(isBlank_3(s_8))_1=(el.checkValidity?el.checkValidity():true)?CheckedInput.Blank(s_8):CheckedInput.Invalid(s_8);
      else {
        const m=(o=0,[TryParse(s_8, {get:() => o, set:(v) => {
          o=v;
        }}), o]);
        _1=m[0]?CheckedInput.Valid(m[1], s_8):CheckedInput.Invalid(s_8);
      }
      return Some(_1);
    };
    const g_5=IntGetChecked();
    const s_5=IntSetChecked();
    this.IntApplyChecked=(v) => ApplyValue(g_5, s_5, v);
    this.FloatSetUnchecked=(el) =>(i) => {
      el.value=String(i);
    };
    this.FloatGetUnchecked=(el) => {
      const s_8=el.value;
      if(isBlank_3(s_8))return Some(0);
      else {
        const pd=+s_8;
        return isNaN(pd)?null:Some(pd);
      }
    };
    const g_6=FloatGetUnchecked();
    const s_6=FloatSetUnchecked();
    this.FloatApplyUnchecked=(v) => ApplyValue(g_6, s_6, v);
    this.FloatSetChecked=(el) =>(i) => {
      const i_1=i.Input;
      return el.value!=i_1?void(el.value=i_1):null;
    };
    this.FloatGetChecked=(el) => {
      let _1;
      const s_8=el.value;
      if(isBlank_3(s_8))_1=(el.checkValidity?el.checkValidity():true)?CheckedInput.Blank(s_8):CheckedInput.Invalid(s_8);
      else {
        const i=+s_8;
        _1=isNaN(i)?CheckedInput.Invalid(s_8):CheckedInput.Valid(i, s_8);
      }
      return Some(_1);
    };
    const g_7=FloatGetChecked();
    const s_7=FloatSetChecked();
    this.FloatApplyChecked=(v) => ApplyValue(g_7, s_7, v);
  }
});
function Clear(a){
  a.splice(0, length(a));
}
let _c_8=Lazy((_i) => class $StartupCode_DomUtility {
  static {
    _c_8=_i(this);
  }
  static defaultWrap;
  static wrapMap;
  static rhtml;
  static rtagName;
  static rxhtmlTag;
  static {
    this.rxhtmlTag=new RegExp("<(?!area|br|col|embed|hr|img|input|link|meta|param)(([\\w:]+)[^>]*)\\/>", "gi");
    this.rtagName=new RegExp("<([\\w:]+)");
    this.rhtml=new RegExp("<|&#?\\w+;");
    const table=[1, "<table>", "</table>"];
    let _1=Object.fromEntries([["option", [1, "<select multiple='multiple'>", "</select>"]], ["legend", [1, "<fieldset>", "</fieldset>"]], ["area", [1, "<map>", "</map>"]], ["param", [1, "<object>", "</object>"]], ["thead", table], ["tbody", table], ["tfoot", table], ["tr", [2, "<table><tbody>", "</tbody></table>"]], ["col", [2, "<table><colgroup>", "</colgoup></table>"]], ["td", [3, "<table><tbody><tr>", "</tr></tbody></table>"]]]);
    this.wrapMap=_1;
    this.defaultWrap=[0, "", ""];
  }
});
class Scheduler extends Object_1 {
  idle;
  robin;
  Fork(action){
    this.robin.push(action);
    this.idle?(this.idle=false,setTimeout(() => {
      this.tick();
    }, 0)):void 0;
  }
  tick(){
    let loop;
    const t=Date.now();
    loop=true;
    while(loop)
      if(this.robin.length===0){
        this.idle=true;
        loop=false;
      }
      else {
        (this.robin.shift())();
        Date.now()-t>40?(setTimeout(() => {
          this.tick();
        }, 0),loop=false):void 0;
      }
  }
  constructor(){
    super();
    this.idle=true;
    this.robin=[];
  }
}
class Easing extends Object_1 {
  transformTime;
  static Custom(f){
    return new Easing(f);
  }
  constructor(transformTime){
    super();
    this.transformTime=transformTime;
  }
}
function New_38(k, ct){
  return{k:k, ct:ct};
}
function No(Item){
  return{$:1, $0:Item};
}
function Ok(Item){
  return{$:0, $0:Item};
}
function Cc(Item){
  return{$:2, $0:Item};
}
let _c_9=Lazy((_i) => class $StartupCode_Concurrency {
  static {
    _c_9=_i(this);
  }
  static GetCT;
  static Zero;
  static defCTS;
  static scheduler;
  static noneCT;
  static {
    this.noneCT=New_39(false, []);
    this.scheduler=new Scheduler();
    this.defCTS=[new CancellationTokenSource()];
    this.Zero=Return();
    this.GetCT=(c) => {
      c.k(Ok(c.ct));
    };
  }
});
function New_39(IsCancellationRequested, Registrations){
  return{c:IsCancellationRequested, r:Registrations};
}
function Filter_1(ok, set_1){
  return new HashSet("New_2", filter(ok, ToArray_2(set_1)));
}
function Except_1(excluded, included){
  const set_1=new HashSet("New_2", ToArray_2(included));
  set_1.ExceptWith(ToArray_2(excluded));
  return set_1;
}
function ToArray_2(set_1){
  const arr=create(set_1.Count, void 0);
  set_1.CopyTo(arr, 0);
  return arr;
}
function Intersect_1(a, b){
  const set_1=new HashSet("New_2", ToArray_2(a));
  set_1.IntersectWith(ToArray_2(b));
  return set_1;
}
class KeyNotFoundException extends Error {
  constructor(i, _1){
    if(i=="New"){
      i="New_1";
      _1="The given key was not present in the dictionary.";
    }
    if(i=="New_1"){
      const message=_1;
      super(message);
    }
  }
}
class ArgumentException extends Error {
  constructor(i, _1){
    if(i=="New_2"){
      const message=_1;
      super(message);
    }
  }
}
function ApplyValue(get_1, set_1, var_1){
  let expectedValue;
  expectedValue=null;
  return[(el) => {
    const onChange=() => {
      var_1.UpdateMaybe((v) => {
        let _1;
        expectedValue=get_1(el);
        return expectedValue!=null&&expectedValue.$==1&&(!Equals(expectedValue.$0, v)&&(_1=[expectedValue, expectedValue.$0],true))?_1[0]:null;
      });
    };
    el.addEventListener("change", onChange);
    el.addEventListener("input", onChange);
    el.addEventListener("keypress", onChange);
  }, (x) => {
    const _1=set_1(x);
    return(_2) => _2==null?null:_1(_2.$0);
  }, Map((v) => {
    let _1;
    return expectedValue!=null&&expectedValue.$==1&&(Equals(expectedValue.$0, v)&&(_1=expectedValue.$0,true))?null:Some(v);
  }, var_1.View)];
}
function StringSet(){
  return _c_7.StringSet;
}
function StringGet(){
  return _c_7.StringGet;
}
function StringListSet(){
  return _c_7.StringListSet;
}
function StringListGet(){
  return _c_7.StringListGet;
}
function DateTimeSetUnchecked(){
  return _c_7.DateTimeSetUnchecked;
}
function DateTimeGetUnchecked(){
  return _c_7.DateTimeGetUnchecked;
}
function FileApplyValue(get_1, set_1, var_1){
  let expectedValue;
  expectedValue=null;
  return[(el) => {
    el.addEventListener("change", () => {
      var_1.UpdateMaybe((v) => {
        let _1;
        expectedValue=get_1(el);
        return expectedValue!=null&&expectedValue.$==1&&(expectedValue.$0!==v&&(_1=[expectedValue, expectedValue.$0],true))?_1[0]:null;
      });
    });
  }, (x) => {
    const _1=set_1(x);
    return(_2) => _2==null?null:_1(_2.$0);
  }, Map((v) => {
    let _1;
    return expectedValue!=null&&expectedValue.$==1&&(Equals(expectedValue.$0, v)&&(_1=expectedValue.$0,true))?null:Some(v);
  }, var_1.View)];
}
function FileSetUnchecked(){
  return _c_7.FileSetUnchecked;
}
function FileGetUnchecked(){
  return _c_7.FileGetUnchecked;
}
function IntSetUnchecked(){
  return _c_7.IntSetUnchecked;
}
function IntGetUnchecked(){
  return _c_7.IntGetUnchecked;
}
function IntSetChecked(){
  return _c_7.IntSetChecked;
}
function IntGetChecked(){
  return _c_7.IntGetChecked;
}
function FloatSetUnchecked(){
  return _c_7.FloatSetUnchecked;
}
function FloatGetUnchecked(){
  return _c_7.FloatGetUnchecked;
}
function FloatSetChecked(){
  return _c_7.FloatSetChecked;
}
function FloatGetChecked(){
  return _c_7.FloatGetChecked;
}
function isBlank_3(s){
  return forall_2(IsWhiteSpace, s);
}
class CheckedInput {
  get Input(){
    return this.$==1?this.$0:this.$==2?this.$0:this.$1;
  }
  static Blank(inputText){
    return Create_1(CheckedInput, {$:2, $0:inputText});
  }
  static Invalid(inputText){
    return Create_1(CheckedInput, {$:1, $0:inputText});
  }
  static Valid(value, inputText){
    return Create_1(CheckedInput, {
      $:0, 
      $0:value, 
      $1:inputText
    });
  }
  $;
  $0;
  $1;
}
class CancellationTokenSource extends Object_1 {
  init;
  c;
  pending;
  r;
  constructor(){
    super();
    this.c=false;
    this.pending=null;
    this.r=[];
    this.init=1;
  }
}
function Children(elem, delims){
  let n;
  if(delims!=null&&delims.$==1){
    const rdelim=delims.$0[1];
    const ldelim=delims.$0[0];
    const a=[];
    n=ldelim.nextSibling;
    while(n!==rdelim)
      {
        a.push(n);
        n=n.nextSibling;
      }
    return DomNodes(a);
  }
  else {
    let _1=elem.childNodes.length;
    const o=elem.childNodes;
    let _2=init(_1, (i) => o[i]);
    return DomNodes(_2);
  }
}
function Except_2(a, a_1){
  const excluded=a.$0;
  return DomNodes(filter((n) => forall((k) =>!(n===k), excluded), a_1.$0));
}
function Iter(f, a){
  iter(f, a.$0);
}
function DocChildren(node){
  const q=[];
  function loop(doc_2){
    while(true)
      {
        if(doc_2!=null&&doc_2.$==2){
          const d=doc_2.$0;
          doc_2=d.Current;
        }
        else if(doc_2!=null&&doc_2.$==1)return q.push(doc_2.$0.El);
        else if(doc_2==null)return null;
        else if(doc_2!=null&&doc_2.$==5)return q.push(doc_2.$0);
        else if(doc_2!=null&&doc_2.$==4)return q.push(doc_2.$0.Text);
        else if(doc_2!=null&&doc_2.$==6){
          const x=doc_2.$0.Els;
          return(((a_1) =>(a_2) => {
            iter(a_1, a_2);
          })((a_1) => {
            if(a_1==null||a_1.constructor===Object)loop(a_1);
            else q.push(a_1);
          }))(x);
        }
        else {
          const b=doc_2.$1;
          const a=doc_2.$0;
          loop(a);
          doc_2=b;
        }
      }
  }
  loop(node.Children);
  return DomNodes(ofSeqNonCopying(q));
}
function DomNodes(Item){
  return{$:0, $0:Item};
}
function TryParse_3(s){
  const d=Date.parse(s);
  return isNaN(d)?null:Some(d);
}
class OperationCanceledException extends Error {
  ct;
  constructor(i, _1, _2, _3){
    let ct;
    if(i=="New"){
      ct=_1;
      i="New_1";
      _1="The operation was canceled.";
      _2=null;
      _3=ct;
    }
    if(i=="New_1"){
      const message=_1;
      const inner=_2;
      const ct_1=_3;
      super(message);
      this.inner=inner;
      this.ct=ct_1;
    }
  }
}
function Create(f){
  return New_40(false, f, forceLazy);
}
function forceLazy(){
  const v=this.v();
  this.c=true;
  this.v=v;
  this.f=cachedLazy;
  return v;
}
function cachedLazy(){
  return this.v;
}
let _c_10=Lazy((_i) => class $StartupCode_AppendList {
  static {
    _c_10=_i(this);
  }
  static Empty;
  static {
    this.Empty={$:0};
  }
});
function New_40(created, evalOrVal, force){
  return{
    c:created, 
    v:evalOrVal, 
    f:force
  };
}
Main();

