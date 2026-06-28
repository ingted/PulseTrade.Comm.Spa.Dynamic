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
      return isBlank(proxyAddress)?proxyActor.focus():isBlank(rnAddress)?rnActor.focus():ctx.submitKey(New([proxyAddress, "proxy-v1", rnAddress, isBlank(kind)?"raw":kind], Trim(aliasInput.value)));
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
        return isBlank(actorAddress)?actor.focus():isBlank(selectedTypeName)?typeInput.focus():tryFindDocument(selectedTypeName)!=null||length(keyTail)>0?ctx.submitKey(New(ofSeq(delay(() => append_1([actorAddress], delay(() => append_1([selectedTypeName], delay(() => keyTail)))))), displayName)):null;
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
      postJson("/client-extensions/dynamic/argu/resolve-target", New_1(keyParts), (reply) => {
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
  return _c_1.doc;
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
        return context.submit(New_2(raw, typeName, caseName, keyJsonForSubmit(normalizeDynamicTargetKeyParts(context.keyParts))));
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
            return context.submit(New_2(raw, typeName, "__document", keyJsonForSubmit(normalizeDynamicTargetKeyParts(context.keyParts))));
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
  _c_1.schemas=_1;
}
function schemas(){
  return _c_1.schemas;
}
function set_documents(_1){
  _c_1.documents=_1;
}
function documents(){
  return _c_1.documents;
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
      }, arrayOrEmpty(field.items)),row.appendChild(tuple),void(getter=() => ofSeq(collect((getter_1) => getter_1(), itemGetters))));
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
  }, collect(flattenNodeDefaults, arrayOrEmpty(document.$0.nodes))))):new FSharpMap("New", []);
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
  return delay(() =>!(node==null)?append_1([node], delay(() => append_1(collect(flattenNodeDefaults, arrayOrEmpty(node.children)), delay(() => collect(flattenNodeDefaults, arrayOrEmpty(node.items)))))):[]);
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
        const doc_1=docOpt.$0;
        globalThis.console.log("Got Some doc! Creating container...");
        const container=globalThis.document.createElement("div");
        LoadLocalTemplates("");
        Doc.Run(container, doc_1);
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
  const nodes=actorNodes(rawContent);
  const projectionId=projectionText(rawContent, "projectionId", "ptcs-actors");
  const projectionVersion=projectionText(rawContent, "projectionVersion", "0");
  const groups=createNodeGroups(nodes);
  const reportOutputDirectory=_c.Create_1("");
  const reportStatus=_c.Create_1("");
  const activeCount=filter((node) => {
    const status=lower(nodeStatus(node));
    return status.indexOf("active")>=0||status.indexOf("running")>=0;
  }, nodes).length;
  return Doc.Element("div", [Attr.Create("class", "ptcs-dynamic-actors-page"), OnAfterRender((node) => {
    node.setAttribute("data-testid", "dynamic-actors-page");
  }), Attr.Create("style", "display:flex; flex-direction:column; gap:12px; color:#142033; min-width:0;")], ofSeq_1(delay(() => append_1([Doc.Element("div", [Attr.Create("style", "display:flex; justify-content:space-between; gap:12px; align-items:flex-start; border-bottom:1px solid #d8e1ee; padding-bottom:10px; flex-wrap:wrap;")], [Doc.Element("div", [], [Doc.Element("h2", [Attr.Create("style", "margin:0; font-size:18px; font-weight:700;")], [Doc.TextNode("Actors / Dynamic")]), Doc.Element("div", [Attr.Create("style", "color:#50627a; font-size:12px;")], [Doc.TextNode("projection "+projectionId+" / v"+projectionVersion)])]), Doc.Element("div", [Attr.Create("style", "display:grid; grid-template-columns:minmax(260px,460px) auto auto auto; gap:6px; align-items:start;")], [Doc.Element("div", [Attr.Create("style", "display:flex; flex-direction:column; gap:4px; min-width:260px;")], [V("input", [Attr.Create("type", "text"), Attr.Create("placeholder", "Server-local report output directory"), Attr.Create("style", "border:1px solid #b8c7dc; border-radius:5px; padding:5px 8px; font-size:12px; min-width:260px; width:100%; box-sizing:border-box;"), OnAfterRender((node) => {
    node.setAttribute("data-testid", "dynamic-actors-report-output-directory");
    const input_1=node;
    input_1.addEventListener("input", () => reportOutputDirectory.Set(input_1.value));
    input_1.addEventListener("change", () => reportOutputDirectory.Set(input_1.value));
  })]), Doc.Element("div", [Attr.Create("style", "min-height:16px; color:#50627a; font-size:11px; line-height:1.35; overflow-wrap:anywhere;"), OnAfterRender((node) => {
    node.setAttribute("data-testid", "dynamic-actors-report-status");
  })], [Doc.EmbedView(Map(Doc.TextNode, reportStatus.View))])]), Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("style", "border:1px solid #b8c7dc; background:#fff; color:#22344d; border-radius:5px; padding:5px 9px; font-size:12px; cursor:pointer;"), OnAfterRender((node) => {
    node.setAttribute("data-testid", "dynamic-actors-reload");
  }), Handler("click", () =>() => globalThis.location.reload())], [Doc.TextNode("Reload")]), Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("style", "border:1px solid #2563eb; background:#2563eb; color:#fff; border-radius:5px; padding:5px 9px; font-size:12px; cursor:pointer;"), OnAfterRender((node) => {
    node.setAttribute("data-testid", "dynamic-actors-generate-report");
  }), Handler("click", () =>() => generateActorReport(reportOutputDirectory.Get(), reportStatus))], [Doc.TextNode("Generate report")]), Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("title", "Report scheduling is tracked separately from one-shot generation."), Attr.Create("style", "border:1px solid #cfd8e6; background:#f4f7fb; color:#738299; border-radius:5px; padding:5px 9px; font-size:12px;"), OnAfterRender((node) => {
    node.setAttribute("data-testid", "dynamic-actors-schedule-report");
    node.setAttribute("disabled", "disabled");
  })], [Doc.TextNode("Schedule")])])])], delay(() => append_1([Doc.Element("div", [Attr.Create("style", "display:grid; grid-template-columns:repeat(auto-fit,minmax(180px,1fr)); gap:10px;")], [renderCountCard("Renderer", "ActorsPage"), renderCountCard("Node groups", String(length(groups))), renderCountCard("Actor tree rows", String(length(nodes))), renderCountCard("Active", String(activeCount))])], delay(() => length(nodes)===0?[Doc.Element("div", [Attr.Create("style", "border:1px solid #c9d7e8; border-radius:6px; background:#fff; padding:12px; color:#4b5e76; font-size:12px;")], [Doc.TextNode("No actor topology rows are available in this projection.")])]:[Doc.Element("div", [Attr.Create("style", "display:flex; flex-direction:column; gap:12px;"), OnAfterRender((node) => {
    node.setAttribute("data-testid", "dynamic-actor-node-blocks");
  })], [Doc.Concat(ofArray(map((_1) => renderNodeBlock(_1[1], _1[2], _1[3]), groups)))])])))))));
}
function registerActorsPageRenderer(){
  const renderer=(rawContent) => {
    try {
      if(IsActorsPagePayload(rawContent)){
        const container=globalThis.document.createElement("div");
        const doc_1=createActorsPageDocument(rawContent);
        LoadLocalTemplates("");
        Doc.Run(container, doc_1);
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
  return sortBy((_3) =>[_3[0], _3[1]], _2);
}
function V(name, attrs){
  return Doc.Element(name, attrs, FSharpList.Empty);
}
function generateActorReport(outputDirectory, status){
  const trimmed=Trim(asText_1(outputDirectory));
  if(isBlank_1(trimmed))status.Set("Report output directory is required.");
  else {
    status.Set("Generating actor state report...");
    postJson_1("/actors/api/report", New_3(trimmed), (reply) => {
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
  const statuses=concat_2(", ", distinctValues(map(nodeStatus, groupNodes)));
  return Doc.Element("section", [Attr.Create("style", "display:flex; flex-direction:column; gap:10px; border:1px solid #cfdcec; background:#fff; border-radius:7px; padding:12px;"), OnAfterRender((node) => {
    node.setAttribute("data-testid", "dynamic-actor-node-block");
  })], [Doc.Element("div", [Attr.Create("style", "display:flex; align-items:flex-start; justify-content:space-between; gap:12px; flex-wrap:wrap;")], [Doc.Element("div", [Attr.Create("style", "min-width:0;")], [Doc.Element("div", [Attr.Create("style", "font-size:11px; color:#667891;")], [Doc.TextNode(roleLabel)]), Doc.Element("h3", [Attr.Create("style", "margin:2px 0 0 0; font-size:15px; font-weight:700; color:#16263c; font-family:Consolas, 'Cascadia Mono', monospace; white-space:nowrap; overflow-x:auto;")], [Doc.TextNode(key)])]), Doc.Element("div", [Attr.Create("style", "font-size:12px; color:#53677f; white-space:nowrap;")], ofSeq_1(delay(() =>[Doc.TextNode(String(filter((node) =>!isBlank_1(nodeRawAddress(node)), groupNodes).length)+" actor node(s)")])))]), Doc.Element("div", [Attr.Create("style", "display:flex; gap:8px; align-items:center; flex-wrap:wrap; font-size:12px; color:#53677f;")], [Doc.Element("span", [Attr.Create("style", "font-weight:650;")], [Doc.TextNode("Status")]), Doc.Element("span", [], [Doc.TextNode(isBlank_1(statuses)?"unknown":statuses)])]), renderTree(groupNodes), renderGrid(groupNodes)]);
}
function lower(value){
  return value==null?"":value.toLowerCase();
}
function nodeStatus(node){
  try {
    return node.status||"";
  }
  catch(m){
    return"";
  }
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
    if(!isBlank_1(id)&&!exists_2((node_2) => nodeId(node_2)==id, result))result=append_2(result, ofArray([node_1]));
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
function withAncestors(allNodes, seedNodes){
  let result;
  result=FSharpList.Empty;
  const add=(node_1) => {
    const id=nodeId(node_1);
    if(!isBlank_1(id)&&!exists_2((item) => nodeId(item)==id, result))result=append_2(result, ofArray([node_1]));
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
function isBlank_1(value){
  return value==null||Trim(value)=="";
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
    return normalized!=""&&!exists_2((current) => current==normalized, known)?void(known=append_2(known, ofArray([normalized]))):null;
  })());
  return ofList(known);
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
      })], ofSeq_1(delay(() => append_1(depthValue>0?append_1([Doc.Element("span", [Attr.Create("class", "dynamic-actor-tree-connector-h"), Attr.Create("style", "position:absolute; left:-12px; top:50%; width:12px; border-top:1px solid #aeb8c8;"), OnAfterRender((node_1) => {
        node_1.setAttribute("data-testid", "dynamic-actor-tree-connector");
      })], [])], delay(() =>[Doc.Element("span", [Attr.Create("class", "dynamic-actor-tree-connector-v"), Attr.Create("style", "position:absolute; left:-12px; top:-5px; height:calc(100% + 5px); border-left:1px solid #aeb8c8;")], [])])):[], delay(() => append_1(length(children)>0?[Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("title", isCollapsed?"Expand actor node":"Collapse actor node"), Attr.Create("style", "display:inline-flex; align-items:center; justify-content:center; width:18px; height:18px; border:1px solid #7d92ad; background:#fff; color:#21354f; font-size:12px; line-height:16px; padding:0; margin-top:3px; cursor:pointer; font-family:Consolas, 'Cascadia Mono', monospace;"), Handler("click", () =>() => {
        const current=collapsedIds.Get();
        return collapsedIds.Set(containsId(id, current)?filter((value) => value!=id, current):current.concat([id]));
      }), OnAfterRender((node_1) => {
        node_1.setAttribute("data-testid", "dynamic-actor-tree-toggle");
        node_1.setAttribute("aria-expanded", isCollapsed?"false":"true");
      })], [Doc.TextNode(isCollapsed?"+":"-")])]:[Doc.Element("span", [Attr.Create("style", "display:inline-flex; width:18px; height:18px; margin-top:3px;"), OnAfterRender((node_1) => {
        node_1.setAttribute("data-testid", "dynamic-actor-tree-toggle-placeholder");
      })], [])], delay(() => append_1([renderStatusDot(nodeStatus(node))], delay(() => append_1([Doc.Element("span", [Attr.Create("class", "dynamic-actor-tree-label"), Attr.Create("title", isBlank_1(fullPath)?displayAddress:fullPath), Attr.Create("style", "white-space:nowrap; color:#172033; font-weight:600; overflow:visible; text-overflow:clip; font-family:Consolas, 'Cascadia Mono', monospace;")], [Doc.TextNode(displayAddress)])], delay(() => append_1([renderSmallPill(kind)], delay(() =>[renderStatusChip(nodeStatus(node))])))))))))))));
      if(isCollapsed||depth>=24)_1=FSharpList.Empty;
      else {
        const x=ofArray(children);
        _1=collect_1((renderNode_1(collapsed))(depth+1), x);
      }
      return FSharpList.Cons(_2, _1);
    };
  }
  return Doc.Element("div", [Attr.Create("style", "border:1px solid #d8e2ef; background:#f8fafc; border-radius:6px; padding:8px 10px; overflow-x:scroll; overflow-y:auto; scrollbar-gutter:stable; max-height:430px;"), OnAfterRender((node) => {
    node.setAttribute("data-testid", "dynamic-actor-tree-viewport");
  })], [Doc.EmbedView(Map((collapsed) => {
    const x=ofArray(roots);
    const rows=collect_1((renderNode_1(collapsed))(0), x);
    return rows.$==0?Doc.Element("div", [Attr.Create("style", "color:#667891; font-size:12px;")], [Doc.TextNode("No actor tree rows.")]):Doc.Concat(rows);
  }, collapsedIds.View))]);
}
function renderGrid(groupNodes){
  const headerCell=(label_1) => E("th", [Attr.Create("style", "text-align:left; padding:8px 10px; border-bottom:1px solid #d7e2ef; color:#53677f; font-size:11px; white-space:nowrap;")], [Doc.TextNode(label_1)]);
  return Doc.Element("div", [Attr.Create("style", "overflow-x:auto; border:1px solid #d8e2ef; border-radius:6px; background:#fff;"), OnAfterRender((node) => {
    node.setAttribute("data-testid", "dynamic-actor-grid");
  })], [E("table", [Attr.Create("style", "border-collapse:collapse; min-width:980px; width:100%;")], [E("thead", [], [E("tr", [], [headerCell("Kind"), headerCell("Status"), headerCell("Address"), headerCell("Full path")])]), E("tbody", [], ofArray(map((node) => E("tr", [], [E("td", [Attr.Create("style", "padding:8px 10px; border-bottom:1px solid #edf2f8; white-space:nowrap;")], [Doc.TextNode(nodeKind(node))]), E("td", [Attr.Create("style", "padding:8px 10px; border-bottom:1px solid #edf2f8; white-space:nowrap;")], [renderStatusChip(nodeStatus(node))]), E("td", [Attr.Create("style", "padding:8px 10px; border-bottom:1px solid #edf2f8; font-family:Consolas, 'Cascadia Mono', monospace; font-size:12px; white-space:nowrap;")], [Doc.TextNode(nodeAddress(node))]), E("td", [Attr.Create("style", "padding:8px 10px; border-bottom:1px solid #edf2f8; font-family:Consolas, 'Cascadia Mono', monospace; font-size:12px; white-space:nowrap;")], [Doc.TextNode(nodeFullPath(node))])]), groupNodes)))])]);
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
function hasToken(token, value){
  return lower(value).indexOf(token)>=0;
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
  const p=normalized.indexOf("active")>=0||normalized.indexOf("running")>=0?["#16a34a", "#dcfce7"]:normalized.indexOf("passivated")>=0?["#d97706", "#fef3c7"]:normalized.indexOf("stale")>=0||normalized.indexOf("changed")>=0?["#d97706", "#fef3c7"]:normalized.indexOf("terminated")>=0||normalized.indexOf("dead")>=0?["#dc2626", "#fee2e2"]:["#64748b", "#e2e8f0"];
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
  const color=normalized.indexOf("active")>=0||normalized.indexOf("running")>=0?"#0b6b3a":normalized.indexOf("stale")>=0||normalized.indexOf("changed")>=0?"#8a5a00":normalized.indexOf("terminated")>=0||normalized.indexOf("dead")>=0?"#8b1e2d":"#46566b";
  return Doc.Element("span", [Attr.Create("style", "display:inline-block; border:1px solid "+color+"; color:"+color+"; border-radius:999px; padding:2px 7px; font-size:11px; line-height:16px; white-space:nowrap;")], [Doc.TextNode(isBlank_1(status)?"unknown":status)]);
}
function E(name, attrs, children){
  return Doc.Element(name, attrs, children);
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
        return V_1("input", append_2(ofArray([Attr.Create("type", "text"), Attr.Create("placeholder", placeholderStr), Attr.Create("style", "padding: 8px; background: #333; color: white; border: 1px solid #555; border-radius: 4px; display: block; width: 100%; box-sizing: border-box; margin: 5px 0;")]), !IsNullOrEmpty(idStr)?ofArray([OnAfterRender((el) => {
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
        return E_1("select", append_2(ofArray([Attr.Create("style", "padding: 8px; margin: 5px 0; background: #333; color: white; border: 1px solid #555; border-radius: 4px; font-size: 1rem; display: block; width: 200px;")]), isMultiple?ofArray([OnAfterRender((el) => {
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
        return V_1("input", append_2(ofArray([Attr.Create("type", "color"), Attr.Create("value", defaultColor), Attr.Create("style", "padding: 0; margin: 5px 0; background: none; border: 1px solid #555; border-radius: 4px; cursor: pointer; height: 40px; width: 60px;")]), !IsNullOrEmpty(idStr_1)?ofArray([OnAfterRender((el) => {
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
function sortInPlace(arr){
  mapInPlace((t) => t[0], mapiInPlace((_1, _2) =>[_2, _1], arr).sort(Compare));
}
function pick(f, arr){
  const m=tryPick(f, arr);
  return m==null?FailWith("KeyNotFoundException"):m.$0;
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
function New(keys, displayName){
  return{keys:keys, displayName:displayName};
}
function delay(f){
  return{GetEnumerator:() => Get(f())};
}
function append_1(s1, s2){
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
  return distinctBy((x) => x, s);
}
function distinctBy(f, s){
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
function collect(f, s){
  return concat_1(map_1(f, s));
}
function choose_1(f, s){
  return collect((x) => {
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
function map_1(f, s){
  return{GetEnumerator:() => {
    const en=Get(s);
    return new T(null, null, (e) => en.MoveNext()&&(e.c=f(en.Current),true), () => {
      en.Dispose();
    });
  }};
}
function init_1(n, f){
  return take_1(n, initInfinite(f));
}
function fold(f, x, s){
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
function forall2(p, s1, s2){
  return!exists2((_1, _2) =>!p(_1, _2), s1, s2);
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
function seqEmpty(){
  return FailWith("The input sequence was empty.");
}
function forall_1(p, s){
  return!exists_1((x) =>!p(x), s);
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
function New_1(keys){
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
class attr extends Object_1 { }
class Var extends Object_1 { }
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
  static Run(parent, doc_1){
    LinkElement(parent, doc_1.docNode);
    Doc.RunInPlace(false, parent, doc_1);
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
    return Doc.Mk(EmbedDoc(node), Map(() => { }, Bind((doc_1) => {
      UpdateEmbedNode(node, doc_1.docNode);
      return doc_1.updates;
    }, view)));
  }
  static Append(a, b){
    return Doc.Mk(AppendDoc(a.docNode, b.docNode), Map2Unit(a.updates, b.updates));
  }
  static get Empty(){
    return Doc.Mk(null, Const());
  }
  static RunInPlace(childrenOnly, parent, doc_1){
    const st=CreateRunState(parent, doc_1.docNode);
    Sink(get_UseAnimations()||BatchUpdatesEnabled()?StartProcessor(PerformAnimatedUpdate(childrenOnly, st, doc_1.docNode)):() => {
      PerformSyncUpdate(childrenOnly, st, doc_1.docNode);
    }, doc_1.updates);
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
  return _c_2.LocalTemplatesLoaded;
}
function set_LocalTemplatesLoaded(_1){
  _c_2.LocalTemplatesLoaded=_1;
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
  return _c_2.LoadedTemplates;
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
  return _c_2.TextHoleRE;
}
let _c_1=Lazy((_i) => class $StartupCode_ArguFormRenderer {
  static {
    _c_1=_i(this);
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
function Trim(s){
  return s.replace(new RegExp("^\\s+"), "").replace(new RegExp("\\s+$"), "");
}
function concat_2(separator, strings){
  return ofSeq(strings).join(separator);
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
function StartsWith(t, s){
  return t.substring(0, s.length)==s;
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
function SplitWith(str, pat){
  return str.split(pat);
}
function ReplaceOnce(string_1, search, replace){
  return string_1.replace(search, replace);
}
function forall_2(f, s){
  return forall_1(f, protect(s));
}
function protect(s){
  return s==null?"":s;
}
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
function New_2(rawArgu, duTypeName, unionCaseName, keyJson){
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
function ofArray(arr){
  let r;
  r=FSharpList.Empty;
  for(let i=length(arr)-1, _1=0;i>=_1;i--)r=FSharpList.Cons(get(arr, i), r);
  return r;
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
function collect_1(f, l){
  return ofSeq_1(collect(f, l));
}
function append_2(x, y){
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
function New_3(outputDirectory){
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
function InsertDoc(parent, doc_1, pos){
  while(true)
    {
      if(doc_1!=null&&doc_1.$==1){
        const e=doc_1.$0;
        return InsertNode(parent, e.El, pos);
      }
      else if(doc_1!=null&&doc_1.$==2){
        const d=doc_1.$0;
        d.Dirty=false;
        doc_1=d.Current;
      }
      else if(doc_1==null)return pos;
      else if(doc_1!=null&&doc_1.$==4){
        const t=doc_1.$0;
        return InsertNode(parent, t.Text, pos);
      }
      else if(doc_1!=null&&doc_1.$==5){
        const t_1=doc_1.$0;
        return InsertNode(parent, t_1, pos);
      }
      else if(doc_1!=null&&doc_1.$==6)return foldBack((_1, _2) =>((((parent_1) =>(el) =>(pos_1) => el==null||el.constructor===Object?InsertDoc(parent_1, el, pos_1):InsertNode(parent_1, el, pos_1))(parent))(_1))(_2), doc_1.$0.Els, pos);
      else {
        const b=doc_1.$1;
        const a=doc_1.$0;
        doc_1=a;
        pos=InsertDoc(parent, b, pos);
      }
    }
}
function CreateRunState(parent, doc_1){
  return New_4(get_Empty_1(), CreateElemNode(parent, EmptyAttr(), doc_1));
}
function PerformAnimatedUpdate(childrenOnly, st, doc_1){
  return get_UseAnimations()?Delay(() => {
    const cur=FindAll(doc_1);
    const change=ComputeChangeAnim(st, cur);
    const enter=ComputeEnterAnim(st, cur);
    return Bind_1(Play(Append(change, ComputeExitAnim(st, cur))), () => Bind_1(SyncElemNodesNextFrame(childrenOnly, st), () => Bind_1(Play(enter), () => {
      st.PreviousNodes=cur;
      return Return(null);
    })));
  }):SyncElemNodesNextFrame(childrenOnly, st);
}
function PerformSyncUpdate(childrenOnly, st, doc_1){
  const cur=FindAll(doc_1);
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
    function dirty(doc_1){
      while(true)
        {
          if(doc_1!=null&&doc_1.$==0){
            const b=doc_1.$1;
            const a=doc_1.$0;
            if(dirty(a))return true;
            else doc_1=b;
          }
          else if(doc_1!=null&&doc_1.$==2){
            const d=doc_1.$0;
            if(d.Dirty)return true;
            else doc_1=d.Current;
          }
          else if(doc_1!=null&&doc_1.$==6){
            const t=doc_1.$0;
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
function Sync(doc_1){
  while(true)
    {
      if(doc_1!=null&&doc_1.$==1)return SyncElemNode(false, doc_1.$0);
      else if(doc_1!=null&&doc_1.$==2){
        const n=doc_1.$0;
        doc_1=n.Current;
      }
      else if(doc_1==null)return null;
      else if(doc_1!=null&&doc_1.$==5)return null;
      else if(doc_1!=null&&doc_1.$==4){
        const d=doc_1.$0;
        return d.Dirty?(d.Text.nodeValue=d.Value,d.Dirty=false):null;
      }
      else if(doc_1!=null&&doc_1.$==6){
        const t=doc_1.$0;
        iter((h) => {
          SyncElemNode(false, h);
        }, t.Holes);
        iter((t_1) => {
          Sync_1(t_1[0], t_1[1]);
        }, t.Attrs);
        return AfterRender(t);
      }
      else {
        const b=doc_1.$1;
        const a=doc_1.$0;
        Sync(a);
        doc_1=b;
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
  function ins(doc_1, pos){
    while(true)
      {
        if(doc_1!=null&&doc_1.$==1)return doc_1.$0.El;
        else if(doc_1!=null&&doc_1.$==2){
          const d=doc_1.$0;
          if(d.Dirty){
            d.Dirty=false;
            return InsertDoc(parent, d.Current, pos);
          }
          else doc_1=d.Current;
        }
        else if(doc_1==null)return pos;
        else if(doc_1!=null&&doc_1.$==4)return doc_1.$0.Text;
        else if(doc_1!=null&&doc_1.$==5)return doc_1.$0;
        else if(doc_1!=null&&doc_1.$==6){
          const t=doc_1.$0;
          if(t.Dirty)t.Dirty=false;
          return foldBack((_3, _4) => _3==null||_3.constructor===Object?ins(_3, _4):_3, t.Els, pos);
        }
        else {
          const b=doc_1.$1;
          const a=doc_1.$0;
          doc_1=a;
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
function buildRawArguFromValues(fields){
  const parts=MarkResizable([]);
  iter((_1) => appendFieldParts(parts, _1[0], _1[1]), arrayOrEmpty_1(fields));
  return Join_1(" ", ofSeq(parts));
}
function arrayOrEmpty_1(values){
  return values==null?[]:values;
}
function appendFieldParts(parts, field, values){
  const values_1=filter((value_3) => value_3.length>0, map((a) => Trim(a), map(asText_2, arrayOrEmpty_1(values))));
  const m=asText_2(field.kind);
  if(m=="bool"){
    const o=tryHead(values_1);
    const o_1=o==null?null:Some(o.$0.toLowerCase());
    if(o_1==null)void 0;
    else {
      const value=o_1.$0;
      if(value=="true"||value=="1"||value=="yes")parts.push(asText_2(field.arguName));
    }
  }
  else if(m=="bool-value"){
    const o_2=tryHead(values_1);
    if(o_2==null)void 0;
    else {
      const value_1=o_2.$0;
      parts.push(asText_2(field.arguName));
      parts.push(quoteArg(value_1));
    }
  }
  else if(m=="list")length(values_1)>0?(parts.push(asText_2(field.arguName)),iter((x) => {
    parts.push(quoteArg(x));
  }, values_1)):void 0;
  else if(m=="tuple")length(values_1)>0?(parts.push(asText_2(field.arguName)),iter((x) => {
    parts.push(quoteArg(x));
  }, values_1)):void 0;
  else {
    const o_3=tryHead(values_1);
    if(o_3==null)void 0;
    else {
      const value_2=o_3.$0;
      parts.push(asText_2(field.arguName));
      parts.push(quoteArg(value_2));
    }
  }
}
function asText_2(value){
  return value==null?"":value;
}
function quoteArg(value){
  const text=asText_2(value);
  return text.length===0?"\"\"":exists_1(IsWhiteSpace, text)||text.indexOf("\"")!=-1?"\""+Replace(Replace(text, "\\", "\\\\"), "\"", "\\\"")+"\"":text;
}
function OfArray(a){
  return new FSharpMap("New_1", OfSeq(map_1((_1) => Pair.New(_1[0], _1[1]), a)));
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
function mapInPlace(f, arr){
  for(let i=0, _1=arr.length-1;i<=_1;i++)arr[i]=f(arr[i]);
}
function insufficient(){
  return FailWith("The input sequence has an insufficient number of elements.");
}
function nonNegative(){
  return FailWith("The input must be non-negative.");
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
function Int(){
  set_counter(counter()+1);
  return counter();
}
function set_counter(_1){
  _c_4.counter=_1;
}
function counter(){
  return _c_4.counter;
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
  return _c_6.EmptyAttr;
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
  let _1=New_5(elem, Flags(tree), arr, oar.length===0?null:Some((el) => {
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
  return _c_7.rhtml;
}
function wrapMap(){
  return _c_7.wrapMap;
}
function defaultWrap(){
  return _c_7.defaultWrap;
}
function rxhtmlTag(){
  return _c_7.rxhtmlTag;
}
function rtagName(){
  return _c_7.rtagName;
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
function TryParse(s, r){
  return TryParse_2(s, -2147483648, 2147483647, r);
}
class View { }
let _c_2=Lazy((_i) => class $StartupCode_Templates {
  static {
    _c_2=_i(this);
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
class HashSet extends Object_1 {
  equals;
  hash;
  data;
  count;
  SAdd(item){
    return this.add(item);
  }
  Contains(item){
    const arr=this.data[this.hash(item)];
    return arr==null?false:this.arrContains(item, arr);
  }
  add(item){
    const h=this.hash(item);
    const arr=this.data[h];
    return arr==null?(this.data[h]=[item],this.count=this.count+1,true):this.arrContains(item, arr)?false:(arr.push(item),this.count=this.count+1,true);
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
  Remove(item){
    const arr=this.data[this.hash(item)];
    return arr==null?false:this.arrRemove(item, arr)&&(this.count=this.count-1,true);
  }
  CopyTo(arr, index){
    const all=concat_3(this.data);
    for(let i=0, _1=all.length-1;i<=_1;i++)set(arr, i+index, all[i]);
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
  return _c_3.BatchUpdatesEnabled;
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
class FSharpMap extends Object_1 {
  tree;
  TryFind(k){
    const o=TryFind(Pair.New(k, void 0), this.tree);
    return o==null?null:Some(o.$0.Value);
  }
  Equals(other){
    return this.Count===other.Count&&forall2(Equals, this, other);
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
function notPresent(){
  throw new KeyNotFoundException("New");
}
function alreadyAdded(){
  throw new ArgumentException("New_2", "An item with the same key has already been added.");
}
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
      return e.setAttribute(attrName, fold((_2, _3) => {
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
function New_4(PreviousNodes, Top){
  return{PreviousNodes:PreviousNodes, Top:Top};
}
function get_Empty_1(){
  return NodeSet(new HashSet("New_3"));
}
function FindAll(doc_1){
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
  loop(doc_1);
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
    r(New_7((a) => {
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
  return _c_8.Zero;
}
function Start(c, ctOpt){
  const d=(defCTS())[0];
  const ct=ctOpt==null?d:ctOpt.$0;
  scheduler().Fork(() => {
    if(!ct.c)c(New_7((a) => {
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
  return _c_8.scheduler;
}
function checkCancel(r){
  return(c) => {
    if(c.ct.c)cancel(c);
    else r(c);
  };
}
function defCTS(){
  return _c_8.defCTS;
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
  return _c_5.UseAnimations;
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
let _c_3=Lazy((_i) => class Proxy {
  static {
    _c_3=_i(this);
  }
  static BatchUpdatesEnabled;
  static {
    this.BatchUpdatesEnabled=true;
  }
});
function fromSeq(s){
  const a=ofSeq(map_1((_1) => Pair.New(_1[0], _1[1]), distinctBy((t) => t[0], rev(s))));
  sortInPlace(a);
  return Build(a, 0, a.length-1);
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
  return New_6(node, left, right, _2, 1+(left==null?0:left.Count)+(right==null?0:right.Count));
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
let _c_4=Lazy((_i) => class $StartupCode_Abbrev {
  static {
    _c_4=_i(this);
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
function New_5(DynElem, DynFlags, DynNodes, OnAfterRender_1){
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
let _c_5=Lazy((_i) => class $StartupCode_Animation {
  static {
    _c_5=_i(this);
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
  return _c_9.Empty;
}
function IsWhiteSpace(c){
  return c.match(new RegExp("\\s"))!==null;
}
function New_6(Node_1, Left, Right, Height, Count){
  return{
    Node:Node_1,
    Left:Left,
    Right:Right,
    Height:Height,
    Count:Count
  };
}
let _c_6=Lazy((_i) => class Client {
  static {
    _c_6=_i(this);
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
      return Some(ofSeq(delay(() => collect((i) =>[selectedOptions.item(i).value], range(0, selectedOptions.length-1)))));
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
      if(isBlank_2(s_8))return Some(-8640000000000000);
      else {
        o=0;
        const m_1=TryParse_1(s_8);
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
      if(isBlank_2(s_8))return Some(0);
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
      if(isBlank_2(s_8))_1=(el.checkValidity?el.checkValidity():true)?CheckedInput.Blank(s_8):CheckedInput.Invalid(s_8);
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
      if(isBlank_2(s_8))return Some(0);
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
      if(isBlank_2(s_8))_1=(el.checkValidity?el.checkValidity():true)?CheckedInput.Blank(s_8):CheckedInput.Invalid(s_8);
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
let _c_7=Lazy((_i) => class $StartupCode_DomUtility {
  static {
    _c_7=_i(this);
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
function New_7(k, ct){
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
let _c_8=Lazy((_i) => class $StartupCode_Concurrency {
  static {
    _c_8=_i(this);
  }
  static GetCT;
  static Zero;
  static defCTS;
  static scheduler;
  static noneCT;
  static {
    this.noneCT=New_8(false, []);
    this.scheduler=new Scheduler();
    this.defCTS=[new CancellationTokenSource()];
    this.Zero=Return();
    this.GetCT=(c) => {
      c.k(Ok(c.ct));
    };
  }
});
function New_8(IsCancellationRequested, Registrations){
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
function concat_3(o){
  let r=[];
  let k;
  for(var k_1 in o)r.push.apply(r, o[k_1]);
  return r;
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
  return _c_6.StringSet;
}
function StringGet(){
  return _c_6.StringGet;
}
function StringListSet(){
  return _c_6.StringListSet;
}
function StringListGet(){
  return _c_6.StringListGet;
}
function DateTimeSetUnchecked(){
  return _c_6.DateTimeSetUnchecked;
}
function DateTimeGetUnchecked(){
  return _c_6.DateTimeGetUnchecked;
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
  return _c_6.FileSetUnchecked;
}
function FileGetUnchecked(){
  return _c_6.FileGetUnchecked;
}
function IntSetUnchecked(){
  return _c_6.IntSetUnchecked;
}
function IntGetUnchecked(){
  return _c_6.IntGetUnchecked;
}
function IntSetChecked(){
  return _c_6.IntSetChecked;
}
function IntGetChecked(){
  return _c_6.IntGetChecked;
}
function FloatSetUnchecked(){
  return _c_6.FloatSetUnchecked;
}
function FloatGetUnchecked(){
  return _c_6.FloatGetUnchecked;
}
function FloatSetChecked(){
  return _c_6.FloatSetChecked;
}
function FloatGetChecked(){
  return _c_6.FloatGetChecked;
}
function isBlank_2(s){
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
  function loop(doc_1){
    while(true)
      {
        if(doc_1!=null&&doc_1.$==2){
          const d=doc_1.$0;
          doc_1=d.Current;
        }
        else if(doc_1!=null&&doc_1.$==1)return q.push(doc_1.$0.El);
        else if(doc_1==null)return null;
        else if(doc_1!=null&&doc_1.$==5)return q.push(doc_1.$0);
        else if(doc_1!=null&&doc_1.$==4)return q.push(doc_1.$0.Text);
        else if(doc_1!=null&&doc_1.$==6){
          const x=doc_1.$0.Els;
          return(((a_1) =>(a_2) => {
            iter(a_1, a_2);
          })((a_1) => {
            if(a_1==null||a_1.constructor===Object)loop(a_1);
            else q.push(a_1);
          }))(x);
        }
        else {
          const b=doc_1.$1;
          const a=doc_1.$0;
          loop(a);
          doc_1=b;
        }
      }
  }
  loop(node.Children);
  return DomNodes(ofSeqNonCopying(q));
}
function DomNodes(Item){
  return{$:0, $0:Item};
}
function TryParse_1(s){
  const d=Date.parse(s);
  return isNaN(d)?null:Some(d);
}
function TryParse_2(s, min, max_1, r){
  const x=+s;
  const ok=x===x-x%1&&x>=min&&x<=max_1;
  if(ok)r.set(x);
  return ok;
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
  return New_9(false, f, forceLazy);
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
let _c_9=Lazy((_i) => class $StartupCode_AppendList {
  static {
    _c_9=_i(this);
  }
  static Empty;
  static {
    this.Empty={$:0};
  }
});
function New_9(created, evalOrVal, force){
  return{
    c:created,
    v:evalOrVal,
    f:force
  };
}
Main();
