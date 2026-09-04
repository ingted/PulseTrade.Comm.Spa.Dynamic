import Runtime from "./WebSharper.Core.JavaScript/Runtime.js"
Runtime.ScriptBasePath="/Scripts/";
import { MarkResizable, Create as Create_1, Lazy, GetOptional, SetOptional } from "./WebSharper.Core.JavaScript/Runtime.js"
function isIDisposable(x){
  return"Dispose"in x;
}
function Main(){
  let handle;
  const shellId="ta-ptcs-live-shell";
  const appId="ta-ptcs-live-app";
  handle=null;
  if(Equals(globalThis.document.getElementById(shellId), null)){
    const container=globalThis.document.createElement("section");
    container.id=shellId;
    const m=globalThis.document.body.firstChild;
    if(Equals(m, null))globalThis.document.body.appendChild(container);
    else globalThis.document.body.insertBefore(container, m);
    const doc_1=Doc.Element("div", [Attr.Create("style", "margin:12px; border:1px solid #ccd7e5; background:#fff; min-width:0; max-height:calc(100vh - 24px); overflow:auto;")], [Doc.Element("div", [Attr.Create("style", "display:flex; align-items:center; gap:6px; padding:6px 8px; border-bottom:1px solid #dce4ef; font-size:11px;")], [Doc.Element("strong", [Attr.Create("data-testid", "ta-ptcs-live-marker")], [Doc.TextNode("PTCS transient TA live")]), Doc.Element("button", [Attr.Create("data-testid", "ta-ptcs-deactivate"), Handler("click", () =>() => handle==null?null:handle.$0.SetActive(false))], [Doc.TextNode("Deactivate")]), Doc.Element("button", [Attr.Create("data-testid", "ta-ptcs-activate"), Handler("click", () =>() => handle==null?null:handle.$0.SetActive(true))], [Doc.TextNode("Activate")]), Doc.Element("button", [Attr.Create("data-testid", "ta-ptcs-dispose"), Handler("click", () =>() => handle==null?null:handle.$0.Dispose())], [Doc.TextNode("Dispose")])]), Doc.Element("div", [Attr.Create("id", appId)], [])]);
    LoadLocalTemplates("");
    Doc.Run(container, doc_1);
    handle=Some(mountByIdWithOptions(appId, "ta-research", "ta-live-main", "ta-live-canvas", New(150, 2000, 250, 250, 2000)));
  }
  else void 0;
}
function Main_1(){
  let mountedPageElement, mountedAppendPageResolved, mountedAppendPageDefinitionFingerprint, mounted, appendRegistryWsState, appendRegistryPageCount, appendRegistryMaxSequence, appendRegistrySocket, queuedAppendRegistryFrames, appendRegistrySubscribed, appendRegistryTailRequested;
  const loginRoot=doc().getElementById("ptcs-login-root");
  if(!(loginRoot==null))mountLogin(loginRoot);
  else {
    if(!(doc().body==null))doc().body.setAttribute("data-server-reality-id", currentServerRealityId());
    const trimmed=TrimEnd(asText(globalThis.location.pathname), ["/"]);
    const path=isBlank(trimmed)?"/chat":trimmed;
    mountedPageElement=null;
    mountedAppendPageResolved=false;
    mountedAppendPageDefinitionFingerprint=null;
    const cacheKey_1=appendPagesDefinitionsCacheKey();
    mounted=false;
    appendRegistryWsState="idle";
    appendRegistryPageCount=0;
    appendRegistryMaxSequence=0n;
    const mountOnce=(pages) => {
      if(!mounted){
        let _1;
        mounted=true;
        const pages_1=arrayOrEmpty(pages);
        const p=shell(path, pages_1);
        const page=p[1];
        mountedPageElement=Some(page);
        mountedAppendPageResolved=false;
        mountedAppendPageDefinitionFingerprint=null;
        setMain(p[0]);
        if(path=="/sets")_1=mountSets(page);
        else if(path=="/actors")_1=mountActors(page);
        else if(path=="/chat")_1=mountChat(page);
        else {
          const m=findAppendPage(path, pages_1);
          if(m==null)_1=mountUnknownPage(page, path);
          else {
            const definition=m.$0;
            _1=(mountedAppendPageResolved=true,mountedAppendPageDefinitionFingerprint=Some(appendPageDefinitionFingerprint(definition)),mountAppendPage(page, definition));
          }
        }
        globalThis.setInterval(() => refreshAppendNav(path), 5000);
      }
    };
    const renderAppendRegistryHealth=() => {
      if(!(doc().body==null))doc().body.setAttribute("data-append-registry-ws-state", appendRegistryWsState);
      const node=doc().querySelector("[data-testid='append-registry-health']");
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
      appendRegistryPageCount=arrayOrEmpty(data_1.pages).length;
      const b=data_1.maxSequence;
      appendRegistryMaxSequence=Compare(appendRegistryMaxSequence, b)===1?appendRegistryMaxSequence:b;
      if(mounted){
        const nav=doc().getElementById("ptc-nav");
        if(!(nav==null))renderNav(nav, path, arrayOrEmpty(data_1.pages));
        if(path!="/sets"&&path!="/actors"&&path!="/chat"){
          const _2=findAppendPage(path, arrayOrEmpty(data_1.pages));
          if(mountedPageElement!=null&&mountedPageElement.$==1){
            if(_2==null){
              mountedPageElement.$0;
              if(mountedAppendPageResolved){
                const page=mountedPageElement.$0;
                _1=(clear(page),mountedAppendPageResolved=false,mountedAppendPageDefinitionFingerprint=null,mountUnknownPage(page, path));
              }
              else _1=void 0;
            }
            else {
              const definition=_2.$0;
              const page_1=mountedPageElement.$0;
              const nextFingerprint=appendPageDefinitionFingerprint(definition);
              _1=!mountedAppendPageResolved||!Equals(mountedAppendPageDefinitionFingerprint, Some(nextFingerprint))?(clear(page_1),mountedAppendPageResolved=true,mountedAppendPageDefinitionFingerprint=Some(nextFingerprint),mountAppendPage(page_1, definition)):void 0;
            }
          }
          else _1=void 0;
        }
        else _1=void 0;
      }
      else _1=mountOnce(data_1.pages);
      renderAppendRegistryHealth();
    };
    const setAppendRegistryWsState=(value) => {
      appendRegistryWsState=asText(value);
      renderAppendRegistryHealth();
    };
    appendRegistrySocket=null;
    queuedAppendRegistryFrames=[];
    appendRegistrySubscribed=false;
    appendRegistryTailRequested=false;
    const handleAppendRegistryEvents=(events) => {
      if(length(arrayOrEmpty(events))>0)readJson(cacheKey_1, (cached) => {
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
            const responseType=asText(response.type).toLowerCase();
            const responseStatus=asText(response.status).toLowerCase();
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
        sendAppendRegistryFrame(JSON.stringify(New_3("subscribe", newRequestId("append-pages-subscribe"), streamKey)));
      }
      if(!appendRegistryTailRequested){
        appendRegistryTailRequested=true;
        sendAppendRegistryFrame(JSON.stringify(New_4("read-tail", newRequestId("append-pages-read-tail"), streamKey, defaultCacheLimit())));
      }
    }
    const startAfterAclSnapshot=() => {
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
    };
    getJson("/acl/api/snapshot", (snapshot) => {
      set_currentAclSnapshotJson(JSON.stringify(snapshot));
      set_currentAclSnapshot(Some(snapshot));
      notifyAclSnapshotObservers(currentAclSnapshotJson());
      startAfterAclSnapshot();
    }, () => {
      startAfterAclSnapshot();
    });
  }
}
function doc(){
  return _c_1.doc;
}
function mountLogin(root){
  if(!tryMountLoginWithRegisteredRenderers(root, JSON.stringify(loginConfig())))mountLoginFallback(root);
}
function currentServerRealityId(){
  const node=doc().getElementById("ptc-comm-reality");
  if(node==null||isBlank(node.textContent))return"legacy";
  else try {
    return textOr("legacy", json(node.textContent).serverRealityId);
  }
  catch(m){
    return"legacy";
  }
}
function isBlank(value){
  return value==null||Trim(value)=="";
}
function asText(value){
  return value==null||Equals(typeof value, "undefined")?"":value;
}
function appendPagesDefinitionsCacheKey(){
  return cacheKey("append-pages-definitions", FSharpList.Empty);
}
function setData(name, value, node){
  !isBlank(name)?node.setAttribute("data-"+name, asText(value)):void 0;
  return node;
}
function emptyAppendPagesReply(){
  return New_2("ok", 0, 0n, []);
}
function arrayOrEmpty(values){
  return values==null?[]:values;
}
function mergeAppendPageRegistryEvents(baseline, events){
  let pages, maxSequence;
  const baseline_1=baseline==null?emptyAppendPagesReply():baseline;
  pages=arrayOrEmpty(baseline_1.pages);
  maxSequence=baseline_1.maxSequence;
  iter((event) => {
    if(!(event==null)&&event.sequence>0n){
      const m=asText(event.sourceKind).toLowerCase();
      if(m=="append-page.definition"){
        const b=event.sequence;
        maxSequence=Compare(maxSequence, b)===1?maxSequence:b;
        try {
          const o=pageDefinitionFromWire(json(event.payload));
          if(o==null)null;
          else {
            const page=o.$0;
            pages=sortAppendPages(filter_1((existing) =>!sameTextInvariant(existing.pageId, page.pageId), pages).concat([page]));
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
            pages=sortAppendPages(filter_1((page_1) =>!(sameTextInvariant(page_1.pageId, _1)||sameTextInvariant(page_1.tabId, _2)||sameTextInvariant(page_1.pageId, _2)||sameTextInvariant(page_1.tabId, _1)), pages));
          }
        }
        catch(m_2){
          null;
        }
      }
      else void 0;
    }
  }, arrayOrEmpty(events));
  return New_2("ok", length(pages), maxSequence, pages);
}
function writeAppendPagesDefinitions(data){
  writeSnapshotWithWatermark(appendPagesDefinitionsCacheKey(), data, data.maxSequence, length(arrayOrEmpty(data.pages)), "append-pages-definitions");
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
  const options=requestOptions();
  options.cache="no-store";
  (globalThis.fetch(url, options).then((response) => response.text().then((body) => response.ok?onOk(json(isBlank(body)?"{}":body)):onError(isBlank(body)?"GET "+String(url)+" "+String(response.status):body))))["catch"]((error) => onError(errorMessage(error)));
}
function set_currentAclSnapshotJson(_1){
  _c_1.currentAclSnapshotJson=_1;
}
function set_currentAclSnapshot(_1){
  _c_1.currentAclSnapshot=_1;
}
function notifyAclSnapshotObservers(snapshotJson){
  let r;
  const _1=snapshotJson;
  if(!(globalThis.PulseTrade&&globalThis.PulseTrade.AclSnapshotObservers))void 0;
  let observers=globalThis.PulseTrade.AclSnapshotObservers;
  for(let i=0;i<observers.length;i++){
    let r_1=observers[i];
    try {
      (r_1.render||r_1[1])(_1);
    }
    catch(e){
      console.error("ACL snapshot observer exception:", e);
    }
  }
}
function currentAclSnapshotJson(){
  return _c_1.currentAclSnapshotJson;
}
function findAppendPage(path, pages){
  return tryFind((page) => isCurrentPage(path, pagePath(page))||isCurrentPage(path, "/page/"+asText(page.pageId))||isCurrentPage(path, "/"+asText(page.pageId)), arrayOrEmpty(pages));
}
function clear(node){
  node.textContent="";
}
function mountUnknownPage(page, path){
  page.className="page actors-page";
  page.appendChild(element("div", "empty", "No append page is registered for "+String(path)+"."));
}
function appendPageDefinitionFingerprint(page){
  return concat_1("\u001e", map(asText, [page.pageId, page.tabId, page.path, page.title, page.setName, page.shape, page.description, page.keyPlaceholder, page.valuePlaceholder, page.defaultKey, concat_1("\u001f", arrayOrEmpty(page.tags))]));
}
function mountAppendPage(page, definition){
  let currentLineageHealth, selected, selectedKeyJson, buckets, acceptedLiveValueIds, locallyHiddenKeyIds, pendingSelectKeyId, loadGeneration, visibleValueLimit, scrollValuesToBottomAfterNextRender, addKeyEditorOpen, addKeyMode, composerMode, ensureSelectedSubscription, replayPendingCommands, deleteAcceptedPendingAppends, rerenderAppendForm, rerenderAddKeyBuilder, renderedValueCardKeys, renderedValueCardValueIds, renderedValueCardElements, currentKeyMaxSequence, keyRegistryWsState, syncSocket, queuedSyncFrames, subscribedValueStream, keyRegistrySubscribed, keyRegistryTailRequested, pendingWsAppendIds, syncRepairScheduled, repairSyncAfterClose, replayingPending;
  page.className="page append-page";
  setData("tab-id", definition.tabId, setData("page-id", definition.pageId, setTestId("append-page-"+asText(definition.pageId), page)));
  const sameText=(left, right) => asText(left).toLowerCase()==asText(right).toLowerCase();
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
  acceptedLiveValueIds=[];
  locallyHiddenKeyIds=[];
  pendingSelectKeyId="";
  loadGeneration=0;
  visibleValueLimit=defaultRenderLimit();
  scrollValuesToBottomAfterNextRender=false;
  addKeyEditorOpen=false;
  addKeyMode="target";
  composerMode="plain";
  const isLocallyHiddenKeyId=(keyId) =>!isBlank(keyId)&&exists((hidden) => sameText(hidden, keyId), locallyHiddenKeyIds);
  const rememberLocallyHiddenKeyId=(keyId) => {
    if(!isBlank(keyId)&&!isLocallyHiddenKeyId(keyId))locallyHiddenKeyIds=locallyHiddenKeyIds.concat([keyId]);
  };
  const isAcceptedLiveValueId=(valueId) =>!isBlank(valueId)&&exists((accepted) => sameText(accepted, valueId), acceptedLiveValueIds);
  const side=element("aside", "sidebar append-sidebar", null);
  const sideHead=element("div", "panel-head", null);
  const sideActions=element("div", "head-actions", null);
  const addActorKeyButton=setTestId("append-add-actor-key", button("", "Add actor key"));
  const addKeyButton=setTestId("append-add-key", button("", "Add target key"));
  const addProxyKeyButton=setTestId("append-add-proxy-key", button("", "Add proxy key"));
  const removeKeyButton=setTestId("append-remove-key", button("", "Remove"));
  const removePageButton=setTestId("append-remove-page", button("", "Remove page"));
  const reload=setTestId("append-reload", button("", "Reload"));
  const actionPool=setTestId("append-page-actions", element("details", "append-page-actions", null));
  const actionSummary=setTestId("append-page-actions-summary", element("summary", "append-page-actions-summary", "Actions"));
  const actionMenu=setTestId("append-page-actions-menu", element("div", "append-page-actions-menu", null));
  const filters=element("div", "filters", null);
  const keyFilter=setTestId("append-key-filter", input("key contains"));
  const newKeyInput=setTestId("append-key-input", input(textOr("\"Aster\"", definition.keyPlaceholder)));
  const newKeyAliasInput=setTestId("append-key-alias-input", input("target alias (optional)"));
  const addKeyPanel=setTestId("append-add-key-panel", element("div", "append-add-key-panel", null));
  const fallbackAddKeyPanel=setTestId("append-add-key-fallback", element("div", "append-add-key-fallback", null));
  const fallbackAddKeyActions=setTestId("append-add-key-actions", element("div", "append-add-key-actions", null));
  const cleanKeyButton=setTestId("append-key-clean", button("", "Clean"));
  const cancelKeyButton=setTestId("append-key-cancel", button("", "Cancel"));
  const okKeyButton=setTestId("append-key-ok", button("primary", "OK"));
  const addKeyRendererHost=setData("renderer-state", "not-rendered", setTestId("append-add-key-renderer-host", element("div", "append-add-key-renderer-host", null)));
  const status=setTestId("append-key-status", element("div", "state", "Loading"));
  const list=setTestId("append-key-list", element("div", "list", null));
  const work=setTestId("append-work", element("section", "append-work", null));
  const values=setTestId("append-values", element("div", "append-values", null));
  const valuesControl=setTestId("append-values-control", element("div", "append-values-control", null));
  values.appendChild(valuesControl);
  const form=setTestId("append-form", element("div", "append-form", null));
  const valueInput=setTestId("append-value-input", textarea("append-value-input", textOr("JSON value", definition.valuePlaceholder)));
  const directionInput=setTestId("append-direction", input("outbound-message"));
  const appendButton=setTestId("append-submit", button("primary", "Append"));
  const canAddKey=pageAclAllows(definition.pageId, "ptcs.target-key.add");
  const canRemoveKey=pageAclAllows(definition.pageId, "ptcs.target-key.remove");
  const canRemovePage=pageAclAllows(definition.pageId, "ptcs.page.remove");
  const canAppendValue=isActorArguPage(definition)?pageAclAllows(definition.pageId, "ptcs.actor-argu.send"):pageAclAllows(definition.pageId, "ptcs.append.write");
  setHidden(!canAddKey, addActorKeyButton);
  setHidden(!canAddKey, addKeyButton);
  setHidden(true, addProxyKeyButton);
  setHidden(!canRemoveKey, removeKeyButton);
  setHidden(!canRemovePage, removePageButton);
  setHidden(!canAppendValue, appendButton);
  const head_2=element("div", "work-head", null);
  const titleBox=element("div", "", null);
  const workState=setTestId("append-work-status", element("div", "state", "Loading"));
  const pendingState=setTestId("append-pending-state", element("div", "state pending-state", ""));
  const lineageHealthBox=setTestId("append-lineage-health", element("div", "meta wrap lineage-health", null));
  const lineageDetailBox=setTestId("append-lineage-detail", element("div", "lineage-detail", null));
  const lineageDetailPolicy=setTestId("append-lineage-detail-policy", element("span", "lineage-detail-value", ""));
  const lineageDetailStream=setTestId("append-lineage-detail-stream", element("span", "lineage-detail-value", ""));
  const lineageDetailLegacy=setTestId("append-lineage-detail-legacy", element("span", "lineage-detail-value", ""));
  const lineageDetailValueCount=setTestId("append-lineage-detail-value-count", element("span", "lineage-detail-value", ""));
  const lineageDetailValueStreams=setTestId("append-lineage-detail-value-streams", element("pre", "lineage-detail-value lineage-streams", ""));
  const lineageDetailKeyCount=setTestId("append-lineage-detail-key-count", element("span", "lineage-detail-value", ""));
  const lineageDetailKeyStreams=setTestId("append-lineage-detail-key-streams", element("pre", "lineage-detail-value lineage-streams", ""));
  const keyRegistryHealthBox=setTestId("append-key-registry-health", element("div", "meta wrap key-registry-health", null));
  const browserCacheHealthBox=setTestId("append-browser-cache-health", element("div", "meta wrap browser-cache-health", null));
  const lineageInfo=setTestId("append-lineage-info", element("details", "lineage-info", null));
  const lineageSummary=setTestId("append-lineage-toggle", element("summary", "lineage-summary", "Tab info"));
  const lineageInfoContent=setTestId("append-lineage-info-content", element("div", "lineage-info-content", null));
  const identityBox=setTestId("append-page-identity", element("div", "page-identity", null));
  const pageIdChip=setTestId("append-page-id", element("span", "identity-chip", "page "+asText(definition.pageId)));
  const tabIdChip=setTestId("append-tab-id", element("span", "identity-chip", "tab "+asText(definition.tabId)));
  const sideTitle=element("div", "panel-title", null);
  const lineageDetailRow=(label, valueNode) => {
    const row=element("div", "lineage-detail-row", null);
    append(row, [element("span", "lineage-detail-label", label), valueNode]);
    return row;
  };
  append(lineageDetailBox, [lineageDetailRow("policy", lineageDetailPolicy), lineageDetailRow("stream", lineageDetailStream), lineageDetailRow("legacy", lineageDetailLegacy), lineageDetailRow("value count", lineageDetailValueCount), lineageDetailRow("value streams", lineageDetailValueStreams), lineageDetailRow("key count", lineageDetailKeyCount), lineageDetailRow("key streams", lineageDetailKeyStreams)]);
  append(lineageInfoContent, [lineageHealthBox, lineageDetailBox, keyRegistryHealthBox, browserCacheHealthBox]);
  append(lineageInfo, [lineageSummary, lineageInfoContent]);
  newKeyInput.value=asText(definition.defaultKey);
  directionInput.value="outbound-message";
  directionInput.className="append-direction";
  appendButton.textContent=actorArguButtonLabel(definition);
  append(identityBox, [pageIdChip, tabIdChip]);
  append(sideTitle, [element("h1", "", pageTitle(definition)), identityBox]);
  append(actionMenu, isActorDynamicPage(definition)?[addActorKeyButton, addKeyButton, removeKeyButton, reload, removePageButton]:isActorArguPage(definition)?[addActorKeyButton, addKeyButton, removeKeyButton, reload, removePageButton]:(addKeyButton.textContent="Add key",[addKeyButton, removeKeyButton, reload, removePageButton]));
  append(actionPool, [actionSummary, actionMenu]);
  append(sideActions, [actionPool]);
  append(sideHead, [sideTitle]);
  append(fallbackAddKeyActions, [cleanKeyButton, cancelKeyButton, okKeyButton]);
  append(fallbackAddKeyPanel, [newKeyInput, newKeyAliasInput, fallbackAddKeyActions]);
  append(addKeyPanel, [fallbackAddKeyPanel, addKeyRendererHost]);
  append(filters, [addKeyPanel, keyFilter, status]);
  append(side, [sideHead, sideActions, filters, list]);
  append(titleBox, [setTestId("append-page-type-label", element("label", "", pageTypeLabel(definition)+" / "+asText(definition.setName))), element("h2", "", pageTitle(definition)), element("div", "meta wrap", asText(definition.description)), lineageInfo]);
  append(head_2, [titleBox, workState]);
  const applyLineageHealth=(health) => {
    const health_1=health==null?defaultLineageHealth():health;
    currentLineageHealth=health_1;
    const valueStreamKeys=health_1.candidateValueStreamKeys==null?"":concat_1("\n", map(asText, health_1.candidateValueStreamKeys));
    const keyRegistryStreamKeys=health_1.candidateKeyRegistryStreamKeys==null?"":concat_1("\n", map(asText, health_1.candidateKeyRegistryStreamKeys));
    const visibleStreamKeys=(streamKeys) => isBlank(streamKeys)?"none":streamKeys;
    setData("lineage-health-policy", health_1.readRepairPolicy, setData("lineage-candidate-key-registry-stream-keys", keyRegistryStreamKeys, setData("lineage-candidate-value-stream-keys", valueStreamKeys, setData("lineage-candidate-key-registry-stream-count", String(health_1.candidateKeyRegistryStreamCount), setData("lineage-candidate-value-stream-count", String(health_1.candidateValueStreamCount), page)))));
    setData("read-repair-policy", health_1.readRepairPolicy, setData("candidate-key-registry-stream-keys", keyRegistryStreamKeys, setData("candidate-value-stream-keys", valueStreamKeys, setData("candidate-key-registry-stream-count", String(health_1.candidateKeyRegistryStreamCount), setData("candidate-value-stream-count", String(health_1.candidateValueStreamCount), setData("lineage-kind", health_1.lineageKind, setData("stream-page-id", health_1.streamPageId, lineageHealthBox)))))));
    lineageHealthBox.setAttribute("title", "value streams:\n"+valueStreamKeys+"\nkey registry streams:\n"+keyRegistryStreamKeys);
    lineageHealthBox.textContent="lineage "+String(asText(health_1.lineageKind))+" | stream "+String(asText(health_1.streamPageId))+" | value streams "+String(health_1.candidateValueStreamCount)+" | key streams "+String(health_1.candidateKeyRegistryStreamCount)+" | "+String(asText(health_1.readRepairPolicy));
    setData("read-repair-policy", health_1.readRepairPolicy, setData("candidate-key-registry-stream-keys", keyRegistryStreamKeys, setData("candidate-value-stream-keys", valueStreamKeys, setData("candidate-key-registry-stream-count", String(health_1.candidateKeyRegistryStreamCount), setData("candidate-value-stream-count", String(health_1.candidateValueStreamCount), setData("reads-legacy", health_1.readsLegacyPageStreams?"true":"false", setData("legacy-page-id-alias", health_1.legacyPageIdAlias, setData("lineage-kind", health_1.lineageKind, setData("stream-page-id", health_1.streamPageId, lineageDetailBox)))))))));
    lineageDetailPolicy.textContent=asText(health_1.readRepairPolicy);
    lineageDetailStream.textContent=asText(health_1.streamPageId);
    lineageDetailLegacy.textContent=isBlank(health_1.legacyPageIdAlias)?"none":asText(health_1.legacyPageIdAlias);
    lineageDetailValueCount.textContent=String(health_1.candidateValueStreamCount);
    lineageDetailValueStreams.textContent=visibleStreamKeys(valueStreamKeys);
    lineageDetailKeyCount.textContent=String(health_1.candidateKeyRegistryStreamCount);
    lineageDetailKeyStreams.textContent=visibleStreamKeys(keyRegistryStreamKeys);
  };
  applyLineageHealth(currentLineageHealth);
  if(isActorArguPage(definition)){
    form.className="append-form actor-argu-form";
    append(form, [valueInput, appendButton]);
  }
  else asText(definition.shape).toLowerCase()=="fcell-chat"?(form.className="append-form chat-form",append(form, [directionInput, valueInput, appendButton])):append(form, [valueInput, appendButton]);
  append(work, [head_2, pendingState, values, form]);
  append(page, [side, work]);
  const browserId=currentUserId();
  ensureSelectedSubscription=() => { };
  replayPendingCommands=() => { };
  deleteAcceptedPendingAppends=() =>() => null;
  rerenderAppendForm=() => { };
  rerenderAddKeyBuilder=() => { };
  renderedValueCardKeys=[];
  renderedValueCardValueIds=[];
  renderedValueCardElements=[];
  const refreshPendingState=() => {
    readPendingRealitySplit((_3, _4) => renderPendingInspection(pendingState, filter_1((command) =>!(command==null)&&(sameText(command.target, definition.pageId)||!isBlank(command.payloadJson)&&command.payloadJson.indexOf("\"pageId\":\""+asText(definition.pageId)+"\"")!=-1), _3), filter_1((command) =>!(command==null)&&(sameText(command.target, definition.pageId)||!isBlank(command.payloadJson)&&command.payloadJson.indexOf("\"pageId\":\""+asText(definition.pageId)+"\"")!=-1), _4)));
  };
  const isPendingForThisPage=(command) =>!(command==null)&&(sameText(command.target, definition.pageId)||!isBlank(command.payloadJson)&&command.payloadJson.indexOf("\"pageId\":\""+asText(definition.pageId)+"\"")!=-1);
  const currentFilterText=() => isBlank(keyFilter.value)?"":Trim(keyFilter.value);
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
    const selectedText=isBlank(selected)?"(none)":selected;
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
  const appendPageKeyId=(keys) => asText(definition.setName)+"::"+concat_1(" + ", sortBy((key) => key.toLowerCase(), distinctBy((key) => key.toLowerCase(), choose((key) => {
    const text_1=Trim(asText(key));
    return isBlank(text_1)?null:Some(text_1);
  }, arrayOrEmpty(keys)))));
  const selectBucketKeys=(keys) => {
    const keys_1=arrayOrEmpty(keys);
    return length(keys_1)>0&&(selected=appendPageKeyId(keys_1),selectedKeyJson=keysAsJson(keys_1),newKeyInput.value=selectedKeyJson,true);
  };
  const sortAppendPageBuckets=(items) => sortBy((bucket) =>[asText(bucket.setName), asText(bucket.keyId)], arrayOrEmpty(items));
  const sequenceBounds=(items) => {
    let oldest, newest;
    oldest=0n;
    newest=0n;
    iter((value) => {
      !(value==null)&&value.sequence>0n&&(oldest===0n||value.sequence<oldest)?oldest=value.sequence:void 0;
      !(value==null)&&value.sequence>newest?newest=value.sequence:void 0;
    }, arrayOrEmpty(items));
    return[oldest, newest];
  };
  const mergeAppendValues=(existing, incoming) => {
    let merged;
    merged=[];
    const add=(value) => {
      if(!(value==null)&&!isBlank(value.valueId)&&!exists((row) => row.valueId==value.valueId, merged))merged=merged.concat([value]);
    };
    iter(add, arrayOrEmpty(incoming));
    iter(add, arrayOrEmpty(existing));
    return sortBy((value) => asText(value.createdAtUtc), merged);
  };
  const replyCardKey=(value) => concat_1("\u001f", [selected, asText(value.valueId)]);
  const disposeReplyCardsExcept=(retainedKeys) => {
    const retainedIndexes=map((t) => t[0], filter_1((_3) => {
      const key=_3[1];
      return exists((y) => key==y, retainedKeys);
    }, mapi((_3, _4) =>[_3, _4], renderedValueCardKeys)));
    iteri((_3, _4) => {
      if(!exists((y) => _4==y, retainedKeys)){
        disposeReplyPresentation(concat_1("\u001f", [asText(definition.pageId), asText(definition.tabId), get(renderedValueCardValueIds, _3)]));
        const card=get(renderedValueCardElements, _3);
        return!(card.parentNode==null)?void card.parentNode.removeChild(card):null;
      }
      else return null;
    }, renderedValueCardKeys);
    renderedValueCardKeys=map((index) => get(renderedValueCardKeys, index), retainedIndexes);
    renderedValueCardValueIds=map((index) => get(renderedValueCardValueIds, index), retainedIndexes);
    renderedValueCardElements=map((index) => get(renderedValueCardElements, index), retainedIndexes);
  };
  const cardForValue=(value) => {
    const key=replyCardKey(value);
    const m=tryFindIndex((y) => key==y, renderedValueCardKeys);
    if(m==null){
      const card=renderAppendValue(definition, value);
      renderedValueCardKeys=renderedValueCardKeys.concat([key]);
      renderedValueCardValueIds=renderedValueCardValueIds.concat([asText(value.valueId)]);
      renderedValueCardElements=renderedValueCardElements.concat([card]);
      return card;
    }
    else {
      const index=m.$0;
      return get(renderedValueCardElements, index);
    }
  };
  function renderList(){
    clear(list);
    iter((bucket) => {
      const item=button(bucket.keyId==selected?"list-card active":"list-card", null);
      const x=setData("key-id", bucket.keyId, setTestId("append-key-card", item));
      const x_1=setData("key-display-name", asText(bucket.displayName), x);
      let _3=setData("key-json", keysAsJson(bucket.keys), x_1);
      let _4=setData("min-sequence", String(bucket.minSequence), _3);
      setData("max-sequence", String(bucket.maxSequence), _4);
      item.setAttribute("title", joinValues(bucket.keys));
      let _5=item;
      const displayName=Trim(asText(bucket.displayName));
      let _6=isBlank(displayName)?joinValues(bucket.keys):displayName;
      let _7=element("div", "strong wrap", _6);
      let _8=[_7, element("div", "muted wrap", asText(bucket.setName)), element("div", "meta", "values="+String(bucket.valueCount)+" seq="+String(bucket.maxSequence)+" updated="+String(asText(bucket.updatedAtUtc)))];
      append(_5, _8);
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
        const x=(((n) =>(n_1) => setData(n, selected, n_1))("selected-key-id"))(work);
        ((((n) =>(n_1) => setData(n, selectedKeyJson, n_1))("selected-key-json"))(x));
        const bucket=(((p) =>(a_3) => tryFind(p, a_3))((bucket_2) => bucket_2.keyId==selected))(buckets);
        if(bucket!=null&&bucket.$==1){
          const bucket_1=bucket.$0;
          const allValues=arrayOrEmpty(bucket_1.values);
          const visible=latestArray(visibleValueLimit, allValues);
          disposeReplyCardsExcept(map(replyCardKey, visible));
          clear(valuesControl);
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
          const x_1=[asText(definition.tabId), asText(definition.shape), asText(definition.setName), concat_1("\u001f", arrayOrEmpty(bucket_1.keys))];
          const selectedValueStreamKey=(((s) =>(s_1) => concat_1(s, s_1))("\n"))(x_1);
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
          if(length(visible)===0)_3=void valuesControl.appendChild(element("div", "empty", "No values appended yet."));
          else {
            if(hiddenCached>0){
              const x_11=button("", "Load older ("+String(hiddenCached)+")");
              const loadOlder=(((i) =>(n) => setTestId(i, n))("append-load-older"))(x_11);
              _4=(loadOlder.addEventListener("click", ((allValues_1) =>() => {
                const a_3=length(allValues_1);
                const b_3=visibleValueLimit+defaultRenderLimit();
                visibleValueLimit=Compare(a_3, b_3)===-1?a_3:b_3;
                return renderValues();
              })(allValues)),void valuesControl.appendChild(loadOlder));
            }
            else if(backendGapAvailable){
              const x_12=button("", "Load older (backend)");
              const loadOlder_1=(((i) =>(n) => setTestId(i, n))("append-load-older"))(x_12);
              _4=(loadOlder_1.addEventListener("click", ((bucket_2, oldestSequence_1) =>() => readOlderFromBackend(bucket_2, oldestSequence_1))(bucket_1, oldestSequence)),void valuesControl.appendChild(loadOlder_1));
            }
            else _4=null;
            const desiredCards=map(cardForValue, visible);
            _3=(((a_3) =>(a_4) => {
              iteri((_6, _7) =>(a_3(_6))(_7), a_4);
            })(((isAttachedToTimeline, desiredCards_1) =>(index) =>(card) => {
              if(!isAttachedToTimeline(card)){
                const nextAttached=tryFind(isAttachedToTimeline, skip(index+1, desiredCards_1));
                return nextAttached==null?void values.appendChild(card):void values.insertBefore(card, nextAttached.$0);
              }
              else return null;
            })((card) => card.parentNode===values, desiredCards)))(desiredCards);
          }
          _5=length(visible)<reportedCount?setStatus(workState, "Showing "+String(length(visible))+"/"+String(reportedCount)+" value(s)"):setStatus(workState, String(reportedCount)+" value(s)");
        }
        else {
          disposeReplyCardsExcept([]);
          clear(valuesControl);
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
          valuesControl.appendChild(element("div", "empty", "No key selected."));
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
    const keyJson=isBlank(selectedKeyJson)?keysAsJson(bucket.keys):selectedKeyJson;
    const url="/pages/api/read-before?pageId="+encodeURIComponent(asText(definition.pageId))+"&keyJson="+encodeURIComponent(keyJson)+"&beforeSequence="+String(beforeSequence)+"&count="+String(defaultRenderLimit());
    setStatus(workState, "Loading older values before "+String(beforeSequence));
    return getJson(url, (reply) => {
      applyLineage(reply.lineage);
      applyLineageHealth(reply.lineageHealth);
      const incoming=arrayOrEmpty(reply.values);
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
    const b=visibleValueLimit+length(arrayOrEmpty(incoming));
    const b_1=Compare(mergedLength, b)===-1?mergedLength:b;
    visibleValueLimit=Compare(visibleValueLimit, b_1)===1?visibleValueLimit:b_1;
    renderList();
    renderValues();
  }
  const readNewerFromBackend=(generation, bucket) => {
    const keyJson=keysAsJson(bucket.keys);
    return getJson("/pages/api/read-after?pageId="+encodeURIComponent(asText(definition.pageId))+"&keyJson="+encodeURIComponent(keyJson)+"&afterSequence="+String(bucket.maxSequence)+"&count="+String(defaultCacheLimit()), (reply) => {
      if(generation===loadGeneration){
        applyLineage(reply.lineage);
        applyLineageHealth(reply.lineageHealth);
        const incoming=arrayOrEmpty(reply.values);
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
    let _3, _4;
    applyLineage(data.lineage);
    applyLineageHealth(data.lineageHealth);
    const b=data.keyMaxSequence;
    currentKeyMaxSequence=Compare(currentKeyMaxSequence, b)===1?currentKeyMaxSequence:b;
    const backendBuckets=filter_1((bucket_1) =>!isLocallyHiddenKeyId(bucket_1.keyId), arrayOrEmpty(data.buckets));
    if(sameText(source, "backend")){
      const projectedValueIds=map((a) => a.valueId, collect((bucket_1) => arrayOrEmpty(bucket_1.values), backendBuckets));
      _3=void(acceptedLiveValueIds=filter_1((accepted) =>!exists((projected) => sameText(projected, accepted), projectedValueIds), acceptedLiveValueIds));
    }
    else _3=null;
    buckets=sortAppendPageBuckets(map((backendBucket) => {
      const m_1=tryFind((existing_1) => sameText(existing_1.keyId, backendBucket.keyId), buckets);
      if(m_1!=null&&m_1.$==1){
        const existing=m_1.$0;
        const v=mergeAppendValues(filter_1((value) => isAcceptedLiveValueId(value.valueId), arrayOrEmpty(existing.values)), backendBucket.values);
        const merged=latestArray(defaultCacheLimit(), v);
        const p=sequenceBounds(merged);
        const minSequence=p[0];
        const a=backendBucket.valueCount;
        const b_1=length(merged);
        let _5=Compare(a, b_1)===1?a:b_1;
        const a_1=backendBucket.maxSequence;
        const b_2=p[1];
        let _6=Compare(a_1, b_2)===1?a_1:b_2;
        return New_11(backendBucket.keyId, backendBucket.keys, backendBucket.displayName, backendBucket.setName, _5, minSequence>0n?minSequence:backendBucket.minSequence, _6, textOr(backendBucket.updatedAtUtc, existing.updatedAtUtc), merged);
      }
      else return backendBucket;
    }, backendBuckets).concat(choose((existing) => {
      const v=filter_1((value) => isAcceptedLiveValueId(value.valueId), arrayOrEmpty(existing.values));
      const pendingAcceptedValues=latestArray(defaultCacheLimit(), v);
      if(length(pendingAcceptedValues)===0)return null;
      else {
        const p=sequenceBounds(pendingAcceptedValues);
        const a=existing.valueCount;
        const b_1=length(pendingAcceptedValues);
        let _5=Compare(a, b_1)===1?a:b_1;
        let _6=New_11(existing.keyId, existing.keys, existing.displayName, existing.setName, _5, p[0], p[1], existing.updatedAtUtc, pendingAcceptedValues);
        return Some(_6);
      }
    }, filter_1((existing) =>!isLocallyHiddenKeyId(existing.keyId)&&!exists((backend) => sameText(backend.keyId, existing.keyId), backendBuckets), buckets))));
    visibleValueLimit=defaultRenderLimit();
    if(isBlank(pendingSelectKeyId))_4=false;
    else {
      const m=tryFind((bucket_1) => sameText(bucket_1.keyId, pendingSelectKeyId), buckets);
      if(m==null)_4=false;
      else {
        const bucket=m.$0;
        const selectedPending=bucket==null?false:selectBucketKeys(bucket.keys);
        _4=(selectedPending?pendingSelectKeyId="":void 0,selectedPending);
      }
    }
    if(_4)null;
    else(isBlank(selected)||!exists((bucket_1) => bucket_1.keyId==selected, buckets))&&length(buckets)>0?(selected=get(buckets, 0).keyId,selectedKeyJson=keysAsJson(get(buckets, 0).keys),void(newKeyInput.value=selectedKeyJson)):length(buckets)===0?(selected="",void(selectedKeyJson="")):null;
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
    url="/pages/api/state?pageId="+encodeURIComponent(asText(definition.pageId))+"&limit="+String(defaultCacheLimit());
    if(!isBlank(filterText))url=url+"&key="+encodeURIComponent(filterText);
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
        const sequenceBuckets=filter_1((bucket) => bucket.maxSequence>0n, arrayOrEmpty(cached.buckets));
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
    keyRegistryWsState=asText(value);
    setData("key-registry-ws-state", value, work);
    updateKeyRegistryHealth();
  };
  const effectiveSelectedKeys=() => {
    const selectedJsonKeys=keysFromJson(selectedKeyJson);
    const m=tryFind((bucket_1) => bucket_1.keyId==selected, buckets);
    if(m==null)return keysFromJson(selectedKeyJson);
    else {
      const bucket=m.$0;
      return length(selectedJsonKeys)>0&&sameText(appendPageKeyId(selectedJsonKeys), bucket.keyId)?(m.$0,selectedJsonKeys):arrayOrEmpty(m.$0.keys);
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
  const effectiveSelectedKeyId=() => {
    const m=tryFind((bucket) => bucket.keyId==selected, buckets);
    if(m==null){
      const keys=keysFromJson(selectedKeyJson);
      return length(keys)===0?"":appendPageKeyId(keys);
    }
    else return m.$0.keyId;
  };
  const applyEffectiveKeySelection=() => {
    const keyJson=effectiveSelectedKeyJson();
    const keys=effectiveSelectedKeys();
    if(isBlank(selectedKeyJson)&&!isBlank(keyJson)){
      selectedKeyJson=keyJson;
      newKeyInput.value=keyJson;
    }
    if(isBlank(selected)&&length(keys)>0)selected=appendPageKeyId(keys);
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
    const acceptedValues_1=arrayOrEmpty(acceptedValues);
    if(length(acceptedValues_1)>0){
      const keyJson=keysAsJson(bucket.keys);
      const commandMatches=(command) => {
        if(sameText(command.kind, "append-page-append-value")&&sameText(command.url, "/pages/api/append")&&isPendingForThisPage(command)&&!isBlank(command.payloadJson))try {
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
        const accepted=filter_1(commandMatches, commands);
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
  const streamKeyFor=(bucket) => New_6(definition.tabId, definition.shape, definition.setName, arrayOrEmpty(bucket.keys));
  const handleSyncEvent=(source, event) => {
    let o, updated, _3, o_1;
    if(!(event==null)){
      const m=asText(event.sourceKind).toLowerCase();
      if(m=="append-page.key"||m=="append-page.key-hidden"){
        if(!(event==null)&&event.sequence>0n){
          const m_1=asText(event.sourceKind).toLowerCase();
          if(m_1=="append-page.key"){
            if(event==null||isBlank(event.payload))o=null;
            else try {
              const wire=json(event.payload);
              if(wire==null||asText(wire.schema)!="ptc.comm.spa.append-page.key.v1"||!sameText(wire.pageId, definition.pageId))o=null;
              else {
                const keys=filter_1((key) =>!isBlank(key), map(asText, arrayOrEmpty(wire.keys)));
                o=length(keys)===0?null:Some([keys, Trim(asText(wire.displayName))]);
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
              if((isBlank(filterText)||exists((key) => asText(key).toLowerCase().indexOf(filterText.toLowerCase())!=-1, arrayOrEmpty(_4)))&&!isLocallyHiddenKeyId(keyId)){
                const m_2=tryFind((bucket_1) => sameText(bucket_1.keyId, keyId), buckets);
                if(m_2==null)updated=New_11(keyId, _4, _5, definition.setName, 0, 0n, 0n, asText(event.createdAtUtc), []);
                else {
                  const existing=m_2.$0;
                  updated=New_11(existing.keyId, _4, textOr(existing.displayName, _5), definition.setName, existing.valueCount, existing.minSequence, existing.maxSequence, textOr(existing.updatedAtUtc, event.createdAtUtc), existing.values);
                }
                _3=(buckets=sortAppendPageBuckets(filter_1((bucket_1) =>!sameText(bucket_1.keyId, keyId), buckets).concat([updated])),sameText(pendingSelectKeyId, keyId)?selectBucketKeys(_4)?void(pendingSelectKeyId=""):null:isBlank(selected)||!exists((bucket_1) => sameText(bucket_1.keyId, selected), buckets)?(selected=keyId,selectedKeyJson=keysAsJson(_4),void(newKeyInput.value=selectedKeyJson)):null);
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
            if(event==null||isBlank(event.payload))o_1=null;
            else try {
              const wire_1=json(event.payload);
              o_1=wire_1==null||asText(wire_1.schema)!="ptc.comm.spa.append-page.key-hidden.v1"||!sameText(wire_1.pageId, definition.pageId)||isBlank(wire_1.keyId)?null:Some(Trim(wire_1.keyId));
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
              buckets=sortAppendPageBuckets(filter_1((bucket_1) =>!sameText(bucket_1.keyId, keyId_1), buckets));
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
        const eventKeys=arrayOrEmpty(event.streamKey.keys);
        const o_2=tryFind((bucket_1) => {
          const left=arrayOrEmpty(bucket_1.keys);
          const right=arrayOrEmpty(eventKeys);
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
        const text_1=String(event.data);
        try {
          const response=json(text_1);
          const responseType=asText(response.type).toLowerCase();
          const responseStatus=asText(response.status).toLowerCase();
          const requestId=asText(response.requestId);
          switch(responseStatus=="ok"?responseType=="subscribe"?0:responseType=="append"?1:responseType=="append-page"?1:responseType=="actor-argu"?1:responseType=="stream-event"?2:responseType=="read-tail"?3:responseType=="read"?3:responseType=="tail"?3:5:responseStatus=="error"?4:5){
            case 0:
              return asText(response.streamKey).indexOf("append-page-key-registry")!=-1?setKeyRegistryWsState("subscribed"):setWsState("subscribed");
            case 1:
              if(exists((id) => id==requestId, pendingWsAppendIds)){
                pendingWsAppendIds=filter_1((id) => id!=requestId, pendingWsAppendIds);
                deletePendingThen(requestId, () => {
                  valueInput.value="";
                  refreshPendingState();
                  setStatus(workState, "Appended through WebSocket");
                });
              }
              if(sameText(responseType, "actor-argu"))(((event_1, value) => {
                let keys, matched, _5;
                if(!(value==null)&&!isBlank(value.valueId)){
                  const valueId=value.valueId;
                  if(!isBlank(valueId)&&!isAcceptedLiveValueId(valueId))acceptedLiveValueIds=acceptedLiveValueIds.concat([valueId]);
                  else null;
                  const eventKeys=event_1==null||event_1.streamKey==null?[]:arrayOrEmpty(event_1.streamKey.keys);
                  if(length(eventKeys)>0)keys=eventKeys;
                  else {
                    const m=tryFind((bucket_1) => bucket_1.keyId==selected, buckets);
                    keys=m==null?keysFromJson(selectedKeyJson):arrayOrEmpty(m.$0.keys);
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
                      const bucket=New_11(keyId, keys, "", definition.setName, length(incoming), p[0], p[1], asText(value.createdAtUtc), incoming);
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
              return iter((_5) => handleSyncEvent("tail", _5), arrayOrEmpty(response.events));
            case 4:
              return exists((id) => id==requestId, pendingWsAppendIds)?(pendingWsAppendIds=filter_1((id) => id!=requestId, pendingWsAppendIds),deletePendingThen(requestId, () => {
                refreshPendingState();
                setStatus(workState, pendingFailure("WebSocket command", asText(response.error)));
              })):setStatus(status, "WebSocket sync error: "+asText(response.error));
            case 5:
              return null;
          }
        }
        catch(error){
          return setStatus(status, "WebSocket sync parse failed: "+errorMessage(error));
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
      sendSyncFrame(JSON.stringify(New_3("subscribe", newRequestId("append-page-keys-subscribe"), streamKey)));
    }
    if(!keyRegistryTailRequested){
      keyRegistryTailRequested=true;
      sendSyncFrame(JSON.stringify(New_4("read-tail", newRequestId("append-page-keys-read-tail"), streamKey, defaultCacheLimit())));
    }
  };
  ensureSelectedSubscription=() => {
    const o=selectedBucket();
    if(o==null)void 0;
    else {
      const streamKey=streamKeyFor(o.$0);
      const identity=concat_1("\n", [asText(streamKey.pageId), asText(streamKey.mode), asText(streamKey.setName), concat_1("\u001f", arrayOrEmpty(streamKey.keys))]);
      if(!isBlank(identity)&&identity!=subscribedValueStream){
        subscribedValueStream=identity;
        setWsState("subscribing");
        sendSyncFrame(JSON.stringify(New_3("subscribe", newRequestId("subscribe"), streamKey)));
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
    if(isBlank(keyJson))return setStatus(status, "Key JSON is required");
    else {
      const submittedKeys=keysFromJson(keyJson);
      if(length(submittedKeys)>0)pendingSelectKeyId=appendPageKeyId(submittedKeys);
      else null;
      const displayName_1=Trim(asText(displayName));
      const request=New_13(definition.pageId, keyJson, addKeyMode, displayName_1);
      const pendingId=rememberPending("append-page-add-key", definition.pageId, "/pages/api/add-key", request);
      refreshPendingState();
      setStatus(status, "Adding key; pending command saved in browser DB");
      return postAppendPageKey("/pages/api/add-key", request, (reply) => {
        deletePendingThen(pendingId, () => {
          let _3;
          if(!(reply.key==null)){
            const keyId=reply.key.keyId;
            if(!isBlank(keyId))locallyHiddenKeyIds=filter_1((hidden) =>!sameText(hidden, keyId), locallyHiddenKeyIds);
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
    if(isBlank(request.keyJson))setStatus(workState, "Select or add a key first");
    else if(isBlank(request.valueText))setStatus(workState, "Value text is required");
    else if(isActorArguPage(definition)){
      const request_1=New_14(definition.pageId, request.keyJson, request.valueText, ["web-append", "actor-argu"]);
      const m=selectedBucket();
      if(m!=null&&m.$==1){
        const bucket=m.$0;
        const o=tryHead(arrayOrEmpty(bucket.keys));
        const actorAddress=o==null?"":o.$0;
        if(isBlank(actorAddress))setStatus(workState, "Actor address key is required");
        else {
          const pendingId=rememberPending("actor-argu-send", definition.pageId, "/pages/api/actor-argu/send", request_1);
          const wsRequest=New_17("actor-argu", pendingId, definition.pageId, definition.title, definition.setName, streamKeyFor(bucket), actorAddress, request_1.rawArgu, definition.shape, ofSeq(delay(() => append_1(arrayOrEmpty(definition.tags), delay(() => append_1(arrayOrEmpty(request_1.tags), delay(() => append_1(["page:"+asText(definition.pageId)], delay(() => append_1(["tab:"+asText(definition.tabId)], delay(() =>["shape:"+asText(definition.shape)])))))))))), browserId, definition.tabId);
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
        const wsRequest_1=New_19("append", pendingId_1, streamKeyFor(bucket_1), request.valueText, "append-page.value", definition.shape, pendingId_1, ofSeq(delay(() => append_1(arrayOrEmpty(definition.tags), delay(() => append_1(arrayOrEmpty(request.tags), delay(() => append_1(["page:"+asText(definition.pageId)], delay(() => append_1(["tab:"+asText(definition.tabId)], delay(() =>["shape:"+asText(definition.shape)])))))))))), browserId, definition.tabId);
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
        const wsRequest_2=New_18("append-page", pendingId_2, definition.pageId, definition.title, definition.setName, streamKeyFor(bucket_2), request.keyJson, request.valueText, request.direction, definition.shape, pendingId_2, ofSeq(delay(() => append_1(arrayOrEmpty(definition.tags), delay(() => append_1(arrayOrEmpty(request.tags), delay(() => append_1(["page:"+asText(definition.pageId)], delay(() => append_1(["tab:"+asText(definition.tabId)], delay(() =>["shape:"+asText(definition.shape)])))))))))), browserId, definition.tabId);
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
    const rendererShape=asText(addKeyMode).toLowerCase()=="target"?baseRendererShape=="actor-dynamic"?"actor-dynamic-target":baseRendererShape=="actor-argu"?"actor-argu-target":baseRendererShape:baseRendererShape;
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
        if(isBlank(keyJson))setStatus(status, "Renderer key is required");
        else {
          newKeyInput.value=keyJson;
          setData("last-key-json", keyJson, addKeyRendererHost);
          addKeyWithKeyJson(keyJson, displayName);
        }
      }, cancelAddKeyEditor, (payload) => {
        const keyJson=rendererSubmittedKeyJson(payload);
        const displayName=rendererSubmittedDisplayName(payload);
        if(!isBlank(keyJson)){
          newKeyInput.value=keyJson;
          setData("last-key-json", keyJson, addKeyRendererHost);
        }
        if(!isBlank(displayName))newKeyAliasInput.value=displayName;
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
    const rendererShape=isActorArguPage(definition)?"actor-argu":definition.shape;
    clear(form);
    const effectiveKeyJson=effectiveSelectedKeyJson();
    const effectiveKeyId=effectiveSelectedKeyId();
    const selectedKeys=effectiveSelectedKeys();
    const x=setData("selected-key-json", effectiveKeyJson, setData("selected-key-id", effectiveKeyId, setData("shape", rendererShape, setData("renderer-state", "fallback", form))));
    const n=setData("selected-key-source", isBlank(effectiveKeyJson)?"none":"selected", x);
    setData("composer-mode", composerMode, n);
    const setComposerMode=(nextMode) => {
      const normalized=sameText(asText(nextMode), "form")?"form":"plain";
      if(!sameText(composerMode, normalized)){
        composerMode=normalized;
        rerenderAppendForm();
      }
    };
    const renderPlainComposer=() => {
      form.className="append-form actor-argu-form plain-composer";
      appendButton.textContent="Send";
      const actions=setTestId("append-composer-actions", element("div", "append-composer-actions", null));
      let _3=actions;
      const n_1=setTestId("append-composer-mode", element("div", "append-composer-mode", null));
      const group=setData("mode", composerMode, n_1);
      const plain=setTestId("append-composer-mode-plain", button("append-composer-mode-button", "Plain"));
      const formButton=setTestId("append-composer-mode-form", button("append-composer-mode-button", "Form"));
      let _4=(plain.setAttribute("aria-pressed", sameText(composerMode, "plain")?"true":"false"),formButton.setAttribute("aria-pressed", sameText(composerMode, "form")?"true":"false"),plain.addEventListener("click", () => setComposerMode("plain")),formButton.addEventListener("click", () => setComposerMode("form")),append(group, [plain, formButton]),group);
      let _5=[_4, appendButton];
      append(_3, _5);
      append(form, [valueInput, actions]);
    };
    const customNode=!sameText(composerMode, "form")||isBlank(effectiveKeyJson)?null:tryRenderAppendInputWithRegisteredRenderers(definition.pageId, rendererShape, definition.title, definition.setName, effectiveKeyId, effectiveKeyJson, selectedKeys, valueInput.placeholder, valueInput.value, (payload) => {
      let _3;
      const submitted=rendererSubmittedText(payload);
      const submittedKeyJson=rendererSubmittedKeyJson(payload);
      if(isBlank(submitted))setStatus(workState, "Renderer value text is required");
      else {
        if(!isBlank(submittedKeyJson)){
          const submittedKeys=keysFromJson(submittedKeyJson);
          _3=length(submittedKeys)>0?(selectedKeyJson=submittedKeyJson,selected=appendPageKeyId(submittedKeys),newKeyInput.value=submittedKeyJson):void 0;
        }
        else _3=void 0;
        applyEffectiveKeySelection();
        valueInput.value=submitted;
        setData("last-raw-argu", submitted, form);
        appendValue();
      }
    }, (payload) => {
      const submitted=rendererSubmittedText(payload);
      if(!isBlank(submitted)){
        valueInput.value=submitted;
        setData("last-raw-argu", submitted, form);
      }
    }, composerMode, (value) => {
      setComposerMode(String(value));
    });
    if(composerMode=="form"){
      if(customNode==null){
        setData("renderer-state", "form-unavailable", form);
        renderPlainComposer();
      }
      else {
        const node=customNode.$0;
        form.className="append-form custom-append-input-form";
        setData("renderer-state", "custom", form);
        form.appendChild(node);
      }
    }
    else isActorArguPage(definition)||isActorDynamicPage(definition)?renderPlainComposer():asText(definition.shape).toLowerCase()=="fcell-chat"?(form.className="append-form chat-form",append(form, [directionInput, valueInput, appendButton])):(form.className="append-form",append(form, [valueInput, appendButton]));
  };
  rerenderAddKeyBuilder();
  rerenderAppendForm();
  replayingPending=false;
  replayPendingCommands=() => {
    if(!replayingPending){
      replayingPending=true;
      readAllPending((commands) => {
        let remaining, accepted;
        const mine=filter_1((command) => sameText(command.method, "POST")&&!isBlank(command.url)&&!isBlank(command.payloadJson), filter_1(isPendingForThisPage, commands));
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
                    const reply=json(isBlank(body)?"{}":body);
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
    const normalizedMode=asText(mode).toLowerCase();
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
  }),cancelKeyButton.addEventListener("click", cancelAddKeyEditor),okKeyButton.addEventListener("click", () => addKeyWithKeyJson(isBlank(newKeyInput.value)?asText(definition.defaultKey):Trim(newKeyInput.value), newKeyAliasInput.value)),removeKeyButton.addEventListener("click", () => {
    applyEffectiveKeySelection();
    const removedKeyId=effectiveSelectedKeyId();
    if(isBlank(removedKeyId))setStatus(status, "Select a key first");
    else {
      const request=New_16(definition.pageId, removedKeyId);
      const pendingId=rememberPending("append-page-remove-key", definition.pageId, "/pages/api/remove-key", request);
      refreshPendingState();
      setStatus(status, "Removing key; pending command saved in browser DB");
      postRemoveAppendPageKey("/pages/api/remove-key", request, () => {
        deletePendingThen(pendingId, () => {
          rememberLocallyHiddenKeyId(removedKeyId);
          buckets=filter_1((bucket) =>!sameText(bucket.keyId, removedKeyId), buckets);
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
    return postJson("/pages/api/remove-page", request, (reply) => {
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
    const label=_1[1];
    const x=setHref(href, element("a", isCurrentPage(activePath, href)?"nav-link active":"nav-link", label));
    let _2=setTestId("nav-"+label.toLowerCase(), x);
    nav.appendChild(_2);
  }, staticNavigationDestinations());
  iter((page) => {
    const href=pagePath(page);
    const x=setHref(href, element("a", isCurrentPage(activePath, href)?"nav-link active":"nav-link", null));
    let _1=setTestId("nav-append-page-"+asText(page.pageId), x);
    let _2=setData("page-id", page.pageId, _1);
    const link=setData("shape", page.shape, _2);
    const x_1=element("span", "nav-type-badge "+pageTypeClass(page), pageTypeBadge(page));
    const badge=setTestId("nav-type-badge-append-page-"+asText(page.pageId), x_1);
    badge.setAttribute("title", pageTypeLabel(page));
    badge.setAttribute("aria-label", pageTypeLabel(page));
    const x_2=button("nav-close", "x");
    const closeButton=setTestId("nav-close-append-page-"+asText(page.pageId), x_2);
    closeButton.setAttribute("aria-label", "Remove page "+pageTitle(page));
    closeButton.setAttribute("title", "Remove page");
    closeButton.addEventListener("click", (event) => {
      event.preventDefault();
      event.stopPropagation();
      closeButton.setAttribute("disabled", "disabled");
      return postJson("/pages/api/remove-page", New_15(page.pageId), (reply) => {
        writeAppendPagesDefinitions(reply);
        isCurrentPage(activePath, href)?globalThis.location.assign("/chat"):renderNav(nav, activePath, reply.pages);
      }, (error) => {
        closeButton.removeAttribute("disabled");
        closeButton.textContent="!";
        closeButton.setAttribute("title", "Remove page failed: "+error);
      });
    });
    append(link, [badge, element("span", "nav-title", pageTitle(page)), closeButton]);
    nav.appendChild(link);
  }, arrayOrEmpty(pages));
  const jump=doc().getElementById("ptc-tab-jump");
  if(!(jump==null))renderTabJumpOptions(jump, activePath, staticNavigationDestinations().concat(map((page) =>[pagePath(page), pageTitle(page)], arrayOrEmpty(pages))));
}
function shell(activePath, pages){
  const app=element("div", "app", null);
  const top=element("header", "topbar", null);
  const topRow=element("div", "topbar-main", null);
  const brandCluster=element("div", "brand-cluster", null);
  const navShell=element("div", "nav-shell", null);
  const navJump=setTestId("nav-jump-control", element("div", "nav-jump", null));
  const navJumpSelect=setTestId("nav-jump-select", setId("ptc-tab-jump", select([])));
  const navJumpGo=setTestId("nav-jump-go", button("nav-jump-go", "Go"));
  const navViewport=setTestId("nav-viewport", element("div", "nav-viewport", null));
  const nav=setId("ptc-nav", element("nav", "nav", null));
  const navBack=setTestId("nav-scroll-left", button("nav-scroll", "<"));
  const navForward=setTestId("nav-scroll-right", button("nav-scroll", ">"));
  const create_1=renderPageCreator(nav, activePath, pages);
  const registryHealth=setTestId("append-registry-health", element("div", "state registry-health", "append registry ws pending"));
  const scrollTabs=(delta) => {
    navViewport.scrollLeft=navViewport.scrollLeft+delta;
  };
  navBack.setAttribute("aria-label", "Scroll tabs left");
  navForward.setAttribute("aria-label", "Scroll tabs right");
  navJumpSelect.setAttribute("aria-label", "Jump to tab");
  navJumpGo.setAttribute("aria-label", "Go to selected tab");
  navBack.addEventListener("click", () => scrollTabs(-260));
  navForward.addEventListener("click", () => scrollTabs(260));
  const activateSelectedTab=() => {
    const href=asText(navJumpSelect.value);
    if(!isBlank(href))globalThis.location.assign(href);
  };
  navJumpGo.addEventListener("click", activateSelectedTab);
  navJumpSelect.addEventListener("keydown", (event) => event.key=="Enter"?(event.preventDefault(),activateSelectedTab()):null);
  append(brandCluster, [element("div", "brand", "PTC.Comm SPA"), registryHealth]);
  renderNav(nav, activePath, pages);
  renderTabJumpOptions(navJumpSelect, activePath, staticNavigationDestinations().concat(map((page_1) =>[pagePath(page_1), pageTitle(page_1)], arrayOrEmpty(pages))));
  const x=element("a", "logout", "Logout");
  const logout=setHref(currentLogoutPath(), x);
  const page=element("main", "page", null);
  append(navViewport, [nav]);
  append(navJump, [navJumpSelect, navJumpGo]);
  append(navShell, [navJump, navViewport, navBack, navForward]);
  append(topRow, [brandCluster, logout]);
  append(top, [topRow, create_1, navShell]);
  append(app, [top, page]);
  return[app, page];
}
function setMain(node){
  const main=doc().getElementById("main");
  if(!(main==null)){
    clear(main);
    main.appendChild(node);
  }
}
function mountSets(page){
  let selected, buckets, syncSocket, queuedSyncFrames, subscribedStreams, tailRequestedStreams, registryTailRequested, ensureSetsSubscriptions, loadGeneration, hiddenSetStreams;
  page.className="page sets-grid";
  selected="";
  buckets=[];
  const side=element("aside", "sidebar", null);
  const sideHead=element("div", "panel-head", null);
  const actionPool=setTestId("sets-action-pool", element("details", "append-page-actions", null));
  const actionSummary=setTestId("sets-action-summary", element("summary", "append-page-actions-summary", "Actions"));
  const actionMenu=setTestId("sets-action-menu", element("div", "append-page-actions-menu", null));
  const reloadAction=setTestId("sets-action-reload", button("", "Reload"));
  const cleanNoShowAction=setTestId("sets-action-clean-noshow", button("", "CleanAllNoShow Actors"));
  const cleanParticipantsAction=setTestId("sets-action-clean-participants", button("", "Clean Inactive Inboxes"));
  cleanParticipantsAction.setAttribute("title", "Tombstone fully acknowledged inbox/ack collections for inactive participants.");
  const filters=element("div", "filters", null);
  const keyFilter=input("key contains");
  const setFilter=input("set name");
  const status=element("div", "state", "Loading sets");
  const list=element("div", "list", null);
  const work=element("section", "work", null);
  append(actionMenu, [reloadAction, cleanNoShowAction, cleanParticipantsAction]);
  append(actionPool, [actionSummary, actionMenu]);
  append(sideHead, [element("h1", "", "Sets"), actionPool]);
  append(filters, [keyFilter, setFilter, status]);
  append(side, [sideHead, filters, list]);
  append(page, [side, work]);
  syncSocket=null;
  queuedSyncFrames=[];
  subscribedStreams=[];
  tailRequestedStreams=[];
  registryTailRequested=false;
  ensureSetsSubscriptions=() => { };
  loadGeneration=0;
  hiddenSetStreams=[];
  const sameText=(left, right) => asText(left).toLowerCase()==asText(right).toLowerCase();
  const streamIdentity=(streamKey) => concat_1("\n", [asText(streamKey.pageId), asText(streamKey.mode), asText(streamKey.setName), concat_1("\u001f", arrayOrEmpty(streamKey.keys))]);
  const setValueStreamKey=(pageId, mode, setName, keys) => New_6(asText(pageId), textOr("set", mode), asText(setName), arrayOrEmpty(keys));
  const setKeyId=(setName, keys) => asText(setName)+"::"+concat_1(" + ", arrayOrEmpty(keys));
  const forgetHidden=(keyId) => {
    hiddenSetStreams=filter_1((_1) =>!sameText(_1[0], keyId), hiddenSetStreams);
  };
  const eventIsVisibleAfterTombstone=(keyId, createdAtUtc) => {
    const m=tryPick((_1) => sameText(_1[0], keyId)?Some(_1[1]):null, hiddenSetStreams);
    if(m!=null&&m.$==1){
      const hiddenAtUtc=m.$0;
      return Compare(asText(createdAtUtc), hiddenAtUtc)>0;
    }
    else return true;
  };
  const currentFilterTexts=() =>[isBlank(keyFilter.value)?"":Trim(keyFilter.value), isBlank(setFilter.value)?"":Trim(setFilter.value)];
  const currentCacheKey=() => {
    const p=currentFilterTexts();
    return cacheKey("sets-state", ofArray([p[0], p[1]]));
  };
  const filtersAccept=(setName, keys) => {
    const p=currentFilterTexts();
    const setText=p[1];
    const keyText=p[0];
    return(isBlank(setText)||sameText(setName, setText))&&(isBlank(keyText)||exists((key) => asText(key).toLowerCase().indexOf(keyText.toLowerCase())!=-1, arrayOrEmpty(keys)));
  };
  const sortSetBuckets=(rows) => sortBy((bucket) =>[asText(bucket.setName), asText(bucket.keyId)], arrayOrEmpty(rows));
  const writeSetsCache=() => {
    const snapshot=New_21(fold((_1, _2) => Compare(_1, _2)===1?_1:_2, 0n, map((bucket) => bucket==null?0n:bucket.maxSequence, buckets)), buckets);
    writeSnapshotWithWatermark(currentCacheKey(), snapshot, snapshot.maxSequence, setValueCount(snapshot.buckets), "sets-state");
  };
  function renderList(){
    clear(list);
    iter((bucket) => {
      const item=button(bucket.keyId==selected?"list-card active":"list-card", null);
      setData("key-id", bucket.keyId, setTestId("sets-bucket", item));
      append(item, [element("div", "strong wrap", asText(bucket.setName)), element("div", "muted wrap", joinValues(bucket.keys)), element("div", "meta", "values="+String(bucket.valueCount)+" seq="+String(bucket.maxSequence)+" updated="+String(asText(bucket.updatedAtUtc)))]);
      item.addEventListener("click", () => {
        selected=bucket.keyId;
        renderList();
        renderDetail();
        return ensureSetsSubscriptions();
      });
      list.appendChild(item);
    }, buckets);
  }
  function renderDetail(){
    clear(work);
    const bucket=tryFind((bucket_2) => bucket_2.keyId==selected, buckets);
    if(bucket!=null&&bucket.$==1){
      const bucket_1=bucket.$0;
      const detail=element("div", "detail", null);
      const head_2=element("div", "work-head", null);
      const title=element("div", "", null);
      append(title, [element("label", "", "Key set"), element("h2", "", bucket_1.keyId)]);
      append(head_2, [title, element("div", "state", String(bucket_1.valueCount)+" value(s)")]);
      detail.appendChild(head_2);
      const table=element("table", "data-table", null);
      const thead=element("thead", "", null);
      const headerRow=element("tr", "", null);
      iter((label) => {
        headerRow.appendChild(element("th", "", label));
      }, ["Value", "Keys", "Created", "Body", "Tags"]);
      thead.appendChild(headerRow);
      const tbody=element("tbody", "", null);
      iter((value) => {
        const row=element("tr", "", null);
        iter((_1) => {
          row.appendChild(element("td", _1[1], _1[0]));
        }, [[value.valueId, "wrap"], [joinValues(value.keys), "wrap"], [asText(value.createdAtUtc), "wrap"], [asText(value.value), "preview"], [joinValues(value.tags), "wrap"]]);
        tbody.appendChild(row);
      }, arrayOrEmpty(bucket_1.values));
      append(table, [thead, tbody]);
      append(detail, [table]);
      work.appendChild(detail);
    }
    else work.appendChild(element("div", "empty", "No set selected."));
  }
  const applySnapshot=(source, data) => {
    buckets=arrayOrEmpty(data.buckets);
    (isBlank(selected)||!exists((bucket) => bucket.keyId==selected, buckets))&&length(buckets)>0?selected=get(buckets, 0).keyId:length(buckets)===0?selected="":void 0;
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
    if(!isBlank(keyText))parts.push("participantId="+encodeURIComponent(keyText));
    if(!isBlank(setText))parts.push("setName="+encodeURIComponent(setText));
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
    getJson("/sets/api/state?"+concat_1("&", ofSeq(parts)), (data) => {
      if(generation===loadGeneration){
        writeSnapshotWithWatermark(cacheKey_1, data, data.maxSequence, setValueCount(data.buckets), "sets-state");
        applySnapshot("backend", data);
      }
    }, (error) => {
      if(generation===loadGeneration)setStatus(status, error);
    });
  };
  const closeActionPool=() => {
    actionPool.removeAttribute("open");
  };
  const tryReadSetRegistry=(event) => {
    if(event==null||isBlank(event.payload))return null;
    else try {
      const wire=json(event.payload);
      return wire==null||asText(wire.schema)!="ptc.comm.spa.set.stream.v1"?null:Some(setValueStreamKey(wire.pageId, wire.mode, wire.setName, wire.keys));
    }
    catch(m){
      return null;
    }
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
          const request=New_4("read-tail", newRequestId("sets-read-tail"), _1, defaultRenderLimit());
          _1=JSON.stringify(request);
          recI=0;
          break;
        case 2:
          const identity=streamIdentity(_1);
          if(!isBlank(identity)&&!(((p) =>(a) => exists(p, a))(((identity_1) =>(existing) => existing==identity_1)(identity)))(tailRequestedStreams)){
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
    if(!isBlank(identity)&&!exists((existing) => existing==identity, subscribedStreams)){
      subscribedStreams=subscribedStreams.concat([identity]);
      setWsStreamCount();
      setWsState("subscribing");
      sendSyncFrame(JSON.stringify(New_3("subscribe", newRequestId("sets-subscribe"), streamKey)));
    }
  }
  function requestReadTail(streamKey){
    return recF(1, streamKey);
  }
  function requestReadTailOnce(streamKey){
    return recF(2, streamKey);
  }
  function ensureSelectedBucketSubscription(){
    const m=tryFind((bucket_1) => sameText(bucket_1.keyId, selected), buckets);
    if(m==null)void 0;
    else {
      const bucket=m.$0;
      const streamKey=setValueStreamKey("", "set", bucket.setName, bucket.keys);
      subscribeStream(streamKey);
      requestReadTailOnce(streamKey);
    }
  }
  function handleSyncEvent(event){
    if(!(event==null)&&!(event.streamKey==null)){
      let _1, updated, _2;
      const m=asText(event.sourceKind).toLowerCase();
      if(m=="set.stream"){
        const m_1=tryReadSetRegistry(event);
        if(m_1==null)void 0;
        else {
          const streamKey=m_1.$0;
          const setName=asText(streamKey.setName);
          const keys=arrayOrEmpty(streamKey.keys);
          const keyId=setKeyId(setName, keys);
          if(filtersAccept(setName, keys)&&eventIsVisibleAfterTombstone(keyId, event.createdAtUtc)){
            forgetHidden(keyId);
            if(!exists((bucket) => sameText(bucket.keyId, keyId), buckets)){
              buckets=sortSetBuckets(buckets.concat([New_20(keyId, setName, keys, 0, event.sequence, asText(event.createdAtUtc), [])]));
              isBlank(selected)?selected=keyId:void 0;
              renderList();
              renderDetail();
              writeSetsCache();
            }
            if(sameText(keyId, selected)){
              const selectedStreamKey=setValueStreamKey("", "set", setName, keys);
              subscribeStream(selectedStreamKey);
              requestReadTailOnce(selectedStreamKey);
            }
            else void 0;
          }
          else void 0;
        }
      }
      else if(m=="set.stream.hidden"){
        const m_2=tryReadSetRegistry(event);
        if(m_2==null)void 0;
        else {
          const streamKey_1=m_2.$0;
          const keyId_1=setKeyId(asText(streamKey_1.setName), arrayOrEmpty(streamKey_1.keys));
          hiddenSetStreams=filter_1((_5) =>!sameText(_5[0], keyId_1), hiddenSetStreams).concat([[keyId_1, asText(event.createdAtUtc)]]);
          buckets=filter_1((bucket) =>!sameText(bucket.keyId, keyId_1), buckets);
          if(sameText(selected, keyId_1)){
            const o=tryHead(buckets);
            const o_1=o==null?null:Some(o.$0.keyId);
            _1=selected=o_1==null?"":o_1.$0;
          }
          else _1=void 0;
          renderList();
          renderDetail();
          writeSetsCache();
          setStatus(status, "Hidden set stream "+keyId_1);
        }
      }
      else if(m=="set"){
        const keyId_2=setKeyId(asText(event.streamKey.setName), arrayOrEmpty(event.streamKey.keys));
        if(eventIsVisibleAfterTombstone(keyId_2, event.createdAtUtc)){
          forgetHidden(keyId_2);
          if(!(event==null)&&event.sequence>0n&&!(event.streamKey==null)){
            const setName_1=asText(event.streamKey.setName);
            const keys_1=arrayOrEmpty(event.streamKey.keys);
            if(filtersAccept(setName_1, keys_1)){
              const value=New_22(textOr(event.eventId, event.sourceId), arrayOrEmpty(event.streamKey.keys), asText(event.createdAtUtc), asText(event.payload), arrayOrEmpty(event.tags));
              const keyId_3=setKeyId(setName_1, keys_1);
              const m_3=tryFind((bucket) => sameText(bucket.keyId, keyId_3), buckets);
              if(m_3==null)updated=New_20(keyId_3, setName_1, keys_1, 1, event.sequence, asText(event.createdAtUtc), [value]);
              else {
                const existing=m_3.$0;
                const existingValues=arrayOrEmpty(existing.values);
                const alreadyVisible=exists((row) => sameText(row.valueId, value.valueId), existingValues);
                const v=filter_1((row) =>!sameText(row.valueId, value.valueId), existingValues).concat([value]);
                const mergedValues=latestArray(defaultRenderLimit(), v);
                if(alreadyVisible)_2=existing.valueCount;
                else {
                  const a=existing.valueCount;
                  const b=length(existingValues);
                  let _3=Compare(a, b)===1?a:b;
                  _2=_3+1;
                }
                const a_1=existing.maxSequence;
                const b_1=event.sequence;
                let _4=Compare(a_1, b_1)===1?a_1:b_1;
                updated=New_20(existing.keyId, existing.setName, existing.keys, _2, _4, textOr(existing.updatedAtUtc, event.createdAtUtc), mergedValues);
              }
              buckets=sortSetBuckets(filter_1((bucket) =>!sameText(bucket.keyId, keyId_3), buckets).concat([updated]));
              selected=keyId_3;
              renderList();
              renderDetail();
              writeSetsCache();
              ensureSetsSubscriptions();
              setStatus(status, "Synced set event "+value.valueId);
            }
            else void 0;
          }
          else void 0;
        }
        else void 0;
      }
      else void 0;
    }
  }
  function handleSyncMessage(text_1){
    try {
      const response=json(text_1);
      const responseType=asText(response.type).toLowerCase();
      const responseStatus=asText(response.status).toLowerCase();
      switch(responseStatus=="ok"?responseType=="subscribe"?0:responseType=="stream-event"?1:responseType=="read-tail"?2:responseType=="read"?2:responseType=="tail"?2:4:responseStatus=="error"?3:4){
        case 0:
          setData("ws-last-stream", response.streamKey, page);
          setWsState("subscribed");
          break;
        case 1:
          handleSyncEvent(response.event);
          break;
        case 2:
          iter(handleSyncEvent, arrayOrEmpty(response.events));
          break;
        case 3:
          setStatus(status, "WebSocket sets sync error: "+asText(response.error));
          break;
        case 4:
          null;
          break;
      }
    }
    catch(error){
      setStatus(status, "WebSocket sets sync parse failed: "+errorMessage(error));
    }
  }
  ensureSetsSubscriptions=() => {
    const registryKey=New_6("__set-registry", "set-registry", "__sets", ["__sets"]);
    subscribeStream(registryKey);
    if(!registryTailRequested){
      registryTailRequested=true;
      requestReadTail(registryKey);
    }
    ensureSelectedBucketSubscription();
  };
  reloadAction.addEventListener("click", () => {
    closeActionPool();
    return load();
  });
  cleanNoShowAction.addEventListener("click", () => {
    closeActionPool();
    setStatus(status, "Cleaning no-show actor set streams");
    return postJson("/sets/api/clean-no-show-actors", New_23("browser-action"), (reply) => {
      deleteSnapshotsByPrefix(cacheKey("sets-state", FSharpList.Empty), () => {
        subscribedStreams=[];
        tailRequestedStreams=[];
        registryTailRequested=false;
        setData("ws-stream-count", String(length(subscribedStreams)), page);
        setStatus(status, "Cleaned "+String(reply.hiddenCount)+" no-show actor stream(s)");
        load();
      });
    }, (error) => {
      setStatus(status, "CleanAllNoShow Actors failed: "+error);
    });
  });
  cleanParticipantsAction.addEventListener("click", () => {
    closeActionPool();
    setStatus(status, "Cleaning inactive participant collections");
    postJson("/sets/api/clean-inactive-participant-collections", New_23("browser-action"), (reply) => {
      deleteSnapshotsByPrefix(cacheKey("sets-state", FSharpList.Empty), () => {
        subscribedStreams=[];
        tailRequestedStreams=[];
        registryTailRequested=false;
        setData("ws-stream-count", String(length(subscribedStreams)), page);
        setStatus(status, "Cleaned "+String(reply.cleanedParticipantCount)+" inactive participant(s); hidden "+String(reply.hiddenCount)+" stream(s)");
        load();
      });
    }, (error) => {
      setStatus(status, "Clean Inactive Participant Collections failed: "+error);
    });
  });
  keyFilter.addEventListener("input", load);
  setFilter.addEventListener("input", load);
  load();
}
function mountActors(page){
  let actorSnapshot, syncSocket, queuedSyncFrames, subscribedRegistry, registryTailRequested, dynamicActorsPageAccepted;
  page.className="page actors-page";
  const head_2=element("div", "work-head actors-head", null);
  const title=element("div", "", null);
  const actions=element("div", "head-actions", null);
  const status=element("div", "state", "Loading actors");
  const reload=button("", "Reload");
  const nodes=element("div", "nodes", null);
  const treePanel=setTestId("actor-tree-panel", element("section", "actor-tree-panel", null));
  append(title, [element("label", "", "Actor / Participant Management"), element("h1", "", "Actors")]);
  append(actions, [status, reload]);
  append(head_2, [title, actions]);
  append(page, [head_2, treePanel, nodes]);
  const emptySnapshot=New_24(0, 0, 0n, []);
  actorSnapshot=emptySnapshot;
  syncSocket=null;
  queuedSyncFrames=[];
  subscribedRegistry=false;
  registryTailRequested=false;
  dynamicActorsPageAccepted=false;
  const collapsedTreeNodes=new HashSet("New_3");
  const cacheKey_1=cacheKey("actors-snapshot", FSharpList.Empty);
  const sameText=(left, right) => asText(left).toLowerCase()==asText(right).toLowerCase();
  const actorRegistryStreamKey=() => New_6("__actor-registry", "actor-registry", "__actors", ["__actors"]);
  const isAkkaAddress=(value) => {
    const text_1=asText(value).toLowerCase();
    return StartsWith(text_1, "akka://")||StartsWith(text_1, "akka.tcp://")||StartsWith(text_1, "akka.ssl.tcp://");
  };
  function renderActorTree(source, tree){
    clear(treePanel);
    const safeNodes=arrayOrEmpty(tree.nodes);
    const title_1=element("div", "actor-tree-title", null);
    const content=setTestId("actor-tree-content", element("div", "actor-tree-content", null));
    const treeViewport=setTestId("actor-tree-viewport", element("div", "actor-tree-viewport", null));
    const treeBody=setTestId("actor-tree-body", element("div", "actor-tree-body", null));
    const tableViewport=setTestId("actor-tree-table-viewport", element("div", "actor-tree-table-viewport", null));
    const table=setTestId("actor-tree-table", element("table", "actor-tree-table", null));
    const thead=element("thead", "", null);
    const tbody=element("tbody", "", null);
    append(title_1, [element("label", "", "ActorTree"), element("h2", "", String(asText(tree.projectionId))+" / v"+String(tree.projectionVersion)), element("div", "state", String(source)+"; "+String(length(safeNodes))+" node(s); "+String(arrayOrEmpty(tree.edges).length)+" edge(s)")]);
    const safeNodes_1=arrayOrEmpty(tree.nodes);
    const jsonString=(value) =>"\""+Replace(Replace(Replace(Replace(Replace(asText(value), "\\", "\\\\"), "\"", "\\\""), "\r", "\\r"), "\n", "\\n"), "\u0009", "\\t")+"\"";
    const jsonArray=(values) =>"["+concat_1(",", map(jsonString, arrayOrEmpty(values)))+"]";
    const nodesJson=concat_1(",", map((node) => {
      const tags=jsonArray(arrayOrEmpty(node.tags));
      return"{\"id\":"+jsonString(node.id)+","+"\"parentId\":"+jsonString(node.parentId)+","+"\"label\":"+jsonString(node.label)+","+"\"fullPath\":"+jsonString(node.fullPath)+","+"\"kind\":"+jsonString(node.kind)+","+"\"status\":"+jsonString(node.status)+","+"\"address\":"+jsonString(node.address)+","+"\"tags\":"+tags+"}";
    }, safeNodes_1));
    const rootIdsJson=jsonArray(map(asText, arrayOrEmpty(tree.rootNodeIds)));
    let _1="{\"schema\":\"fskynet-sdui\",\"version\":\"1\",\"documentId\":"+jsonString("ptcs.actors."+textOr("actor-tree", tree.projectionId))+","+"\"surface\":\"ActorsPage\","+"\"documentType\":\"ActorTopologyPage\","+"\"projectionId\":"+jsonString(tree.projectionId)+","+"\"projectionVersion\":"+String(tree.projectionVersion)+","+"\"ui\":[{\"type\":\"ActorsPage\",\"id\":\"ptcs-actors-page\",\"dataRef\":\"actorTreeNodes\",\"rootNodeIds\":"+rootIdsJson+",\"nodeIdField\":\"id\",\"parentIdField\":\"parentId\",\"labelField\":\"label\",\"statusField\":\"status\",\"columns\":[\"kind\",\"status\",\"address\",\"fullPath\"],\"groupBy\":\"actorSystemHostPort\",\"roleOrder\":[\"ptcs-host\",\"gw-host\",\"rn-host\",\"unknown\"]}],"+"\"actions\":[{\"kind\":\"reload\"},{\"kind\":\"generate-report\"},{\"kind\":\"schedule-report\"}],"+"\"data\":{\"actorTreeNodes\":["+nodesJson+"]}"+"}";
    const m=tryRenderWithRegisteredPageRenderers(_1);
    if(m==null){
      dynamicActorsPageAccepted=false;
      nodes.removeAttribute("style");
      setData("renderer", "fallback", treePanel);
      const childMap=OfArray(groupBy((node) => asText(node.parentId), safeNodes));
      const nodeMap=OfArray(map((node) =>[asText(node.id), node], safeNodes));
      function renderNode(depth, node){
        let toggle;
        const id=asText(node.id);
        const o=childMap.TryFind(id);
        let _6=o==null?[]:o.$0;
        const children=sortBy((node_1) => asText(node_1.label), _6);
        const hasChildren=length(children)>0;
        const row=setData("node-id", id, setTestId("actor-tree-row", element("div", "actor-tree-row", null)));
        setData("parent-id", asText(node.parentId), row);
        const a=12;
        const a_1=0;
        const b=Compare(a_1, depth)===1?a_1:depth;
        let _7=Compare(a, b)===-1?a:b;
        let _8=String(_7);
        setData("depth", _8, row);
        const toggleText=!hasChildren?"":collapsedTreeNodes.Contains(id)?"+":"-";
        if(hasChildren){
          const value=setTestId("actor-tree-toggle", button("actor-tree-toggle", toggleText));
          toggle=(value.setAttribute("aria-expanded", collapsedTreeNodes.Contains(id)?"false":"true"),value.setAttribute("title", collapsedTreeNodes.Contains(id)?"Expand":"Collapse"),value);
        }
        else toggle=element("span", "actor-tree-toggle actor-tree-toggle-placeholder", "");
        if(hasChildren)toggle.addEventListener("click", () => {
          collapsedTreeNodes.Contains(id)?collapsedTreeNodes.Remove(id):collapsedTreeNodes.SAdd(id);
          return renderActorTree("toggle", tree);
        });
        else null;
        const labelText=asText(node.label);
        const kindText=asText(node.kind);
        const statusText=asText(node.status);
        const fullPathText=asText(node.fullPath);
        const addressText=asText(node.address);
        const displayText=!isBlank(addressText)?addressText:!isBlank(fullPathText)?fullPathText:!isBlank(labelText)?labelText:id;
        const label=element("span", "actor-tree-label", displayText);
        const statusDot_1=setData("status", statusText, element("span", "actor-tree-status-dot", ""));
        const kindPill=element("span", "actor-tree-kind-pill", kindText);
        const statusPill=setData("status", statusText, element("span", "actor-tree-status-pill", statusText));
        label.setAttribute("title", displayText);
        kindPill.setAttribute("title", "kind: "+kindText);
        statusPill.setAttribute("title", "status: "+statusText);
        append(row, [toggle, statusDot_1, label, kindPill, statusPill]);
        treeBody.appendChild(row);
        if(!collapsedTreeNodes.Contains(id)){
          const _9=depth+1;
          return iter((_10) => renderNode(_9, _10), children);
        }
        else return null;
      }
      const roots=arrayOrEmpty(tree.rootNodeIds);
      let _2=length(roots)===0?map((a) => a.id, filter_1((node) => isBlank(node.parentId), safeNodes)):roots;
      let _3=choose((id) => nodeMap.TryFind(asText(id)), _2);
      let _4=sortBy((node) => asText(node.label), _3);
      iter((_6) => renderNode(0, _6), _4);
      const headerRow=element("tr", "", null);
      let _5=(iter((text_1) => {
        headerRow.appendChild(element("th", "", text_1));
      }, ["parentId", "id", "kind", "status", "address", "fullPath"]),thead.appendChild(headerRow),iter((node) => {
        const x=setTestId("actor-tree-table-row", element("tr", "", null));
        const row=setData("node-id", asText(node.id), x);
        iter((text_1) => {
          row.appendChild(element("td", "", text_1));
        }, [asText(node.parentId), asText(node.id), asText(node.kind), asText(node.status), asText(node.address), asText(node.fullPath)]);
        tbody.appendChild(row);
      }, sortBy((node) => asText(node.fullPath), safeNodes)),table.appendChild(thead),table.appendChild(tbody),treeViewport.appendChild(treeBody),tableViewport.appendChild(table),append(content, [treeViewport, tableViewport]),void append(treePanel, [title_1, content]));
      return _5;
    }
    else {
      const dynamicNode=m.$0;
      dynamicActorsPageAccepted=true;
      clear(nodes);
      nodes.setAttribute("hidden", "");
      const host=setTestId("actor-tree-dynamic-page", element("div", "actor-tree-dynamic-page", null));
      setData("renderer", "dynamic-actors-page", treePanel);
      host.appendChild(dynamicNode);
      treePanel.appendChild(host);
      return;
    }
  }
  const applySnapshot=(source, data) => {
    actorSnapshot=data==null?emptySnapshot:data;
    clear(nodes);
    dynamicActorsPageAccepted?nodes.setAttribute("hidden", ""):(nodes.removeAttribute("hidden"),iter((node) => {
      const block=setData("node-id", node.nodeId, setTestId("actor-node", element("section", "node-block", null)));
      const blockHead=element("div", "work-head", null);
      const title_1=element("div", "", null);
      const grid=element("div", "actor-grid", null);
      let _1=(append(title_1, [element("label", "", "Node"), element("h2", "", asText(node.nodeId))]),append(blockHead, [title_1, element("div", "state", asText(node.status)+" / "+joinValues(node.roles))]),iter((actor) => {
        const card=setData("actor-id", actor.actorId, setTestId("actor-card", element("div", "actor-card", null)));
        const line=asText(actor.kind)+" / "+joinValues(actor.keys);
        const routees=element("div", "routees", null);
        const address=TrimEnd(Trim(asText(node.nodeAddress)), ["/"]);
        const logicalNode=TrimEnd(Trim(asText(node.nodeId)), ["/"]);
        const node_1=isBlank(address)?logicalNode:address;
        const actor_1=Trim(asText(actor.actorId));
        const fullAddress=isBlank(actor_1)?node_1:isAkkaAddress(actor_1)?actor_1:isBlank(node_1)?actor_1:StartsWith(actor_1, "/")?node_1+actor_1:isAkkaAddress(node_1)?node_1+"/user/"+TrimStart(actor_1, ["/"]):node_1+"/"+TrimStart(actor_1, ["/"]);
        const addressRow=setData("actor-address", fullAddress, setTestId("actor-address", element("div", "meta wrap actor-address", "address "+fullAddress)));
        let _2=(card.appendChild(cardTitle(textOr(actor.actorId, actor.displayName), actor.actorId, actor.status, line)),card.appendChild(addressRow),iter((routee) => {
          const row=element("div", "routee", null);
          let _3=(append(row, [statusDot(routee.status), element("span", "strong", asText(routee.routeeId)), element("span", "muted wrap", joinValues(routee.tags))]),row);
          routees.appendChild(_3);
        }, arrayOrEmpty(actor.routees)),card.appendChild(routees),card);
        grid.appendChild(_2);
      }, arrayOrEmpty(node.actors)),append(block, [blockHead, grid]),block);
      nodes.appendChild(_1);
    }, arrayOrEmpty(actorSnapshot.nodes)));
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
      treePanel.appendChild(element("div", "empty", "ActorTree unavailable: "+error));
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
      sendSyncFrame(JSON.stringify(New_3("subscribe", newRequestId("actors-subscribe"), streamKey)));
    }
  }
  function requestRegistryTail(){
    if(!registryTailRequested){
      registryTailRequested=true;
      sendSyncFrame(JSON.stringify(New_4("read-tail", newRequestId("actors-read-tail"), actorRegistryStreamKey(), defaultRenderLimit())));
    }
  }
  function handleSyncEvent(event){
    if(!(event==null)&&asText(event.sourceKind).toLowerCase()=="actor.registered"){
      let x, updatedNode;
      if(event==null||isBlank(event.payload))x=null;
      else try {
        const wire=json(event.payload);
        x=wire==null||asText(wire.schema)!="ptc.comm.spa.actor.registration.v1"?null:Some(wire);
      }
      catch(m_1){
        x=null;
      }
      if(x==null)void 0;
      else {
        const _1=x.$0;
        const nodeId=asText(_1.nodeId);
        const nodeAddress=asText(_1.nodeAddress);
        const actorId=asText(_1.actorId);
        if(!isBlank(nodeId)&&!isBlank(actorId)){
          const tags=arrayOrEmpty(_1.tags);
          const roles=arrayOrEmpty(_1.roles);
          const actor=New_25(actorId, textOr(actorId, _1.displayName), textOr("actor", _1.kind), [nodeId, actorId].concat(tags), textOr("running", _1.status), arrayOrEmpty(_1.routees));
          const m=tryFind((node) => sameText(node.nodeId, nodeId), arrayOrEmpty(actorSnapshot.nodes));
          if(m==null)updatedNode=New_26(nodeId, nodeAddress, "up", roles, [actor]);
          else {
            const existing=m.$0;
            const actors=sortBy((row) => asText(row.actorId), filter_1((row) =>!sameText(row.actorId, actorId), arrayOrEmpty(existing.actors)).concat([actor]));
            updatedNode=New_26(existing.nodeId, isBlank(nodeAddress)?asText(existing.nodeAddress):nodeAddress, textOr("up", existing.status), length(roles)===0?arrayOrEmpty(existing.roles):roles, actors);
          }
          const nodes_1=sortBy((node) => asText(node.nodeId), filter_1((node) =>!sameText(node.nodeId, nodeId), arrayOrEmpty(actorSnapshot.nodes)).concat([updatedNode]));
          let _2=length(nodes_1);
          let _3=fold((_5, _6) => _5+_6, 0, map((node) => arrayOrEmpty(node.actors).length, nodes_1));
          const a=actorSnapshot.maxSequence;
          const b=event.sequence;
          let _4=Compare(a, b)===1?a:b;
          actorSnapshot=New_24(_2, _3, _4, nodes_1);
          writeSnapshotWithWatermark(cacheKey_1, actorSnapshot, actorSnapshot.maxSequence, actorValueCount(actorSnapshot), "actors-snapshot");
          applySnapshot("synced", actorSnapshot);
          setStatus(status, "Synced actor "+actorId);
        }
        else void 0;
      }
    }
  }
  function handleSyncMessage(text_1){
    try {
      const response=json(text_1);
      const responseType=asText(response.type).toLowerCase();
      const responseStatus=asText(response.status).toLowerCase();
      switch(responseStatus=="ok"?responseType=="subscribe"?0:responseType=="stream-event"?1:responseType=="read-tail"?2:responseType=="read"?2:responseType=="tail"?2:4:responseStatus=="error"?3:4){
        case 0:
          setWsState("subscribed");
          break;
        case 1:
          handleSyncEvent(response.event);
          break;
        case 2:
          iter(handleSyncEvent, arrayOrEmpty(response.events));
          break;
        case 3:
          setStatus(status, "WebSocket actors sync error: "+asText(response.error));
          break;
        case 4:
          null;
          break;
      }
    }
    catch(error){
      setStatus(status, "WebSocket actors sync parse failed: "+errorMessage(error));
    }
  }
  reload.addEventListener("click", load);
  load();
  subscribeRegistry();
}
function mountChat(page){
  let selected, cursor, polling, participants, selectedThreadMessages, replayingPending, chatSocket, queuedChatSyncFrames, subscribedChatStream, pendingWsChatIds;
  selected="";
  cursor="";
  polling=false;
  participants=[];
  selectedThreadMessages=[];
  const participantId=currentUserId();
  page.className="page chat-grid";
  const side=element("aside", "sidebar", null);
  const sideHead=element("div", "panel-head", null);
  const sideActions=element("div", "head-actions", null);
  const export_1=setTestId("chat-export", button("", "Export"));
  setData("message-count", "0", export_1);
  const reload=setTestId("chat-reload", button("", "Reload"));
  const list=element("div", "list", null);
  append(sideActions, [export_1, reload]);
  append(sideHead, [element("h1", "", "Chat"), sideActions]);
  append(side, [sideHead, element("div", "", null), list]);
  const work=setTestId("chat-work", element("section", "work", null));
  const workHead=element("div", "work-head", null);
  const titleBox=element("div", "", null);
  const toTitle=element("h2", "", "No participant selected");
  const state=element("div", "state", "Loading participants");
  const pendingState=setTestId("chat-pending-state", element("div", "state pending-state", ""));
  const thread=setTestId("thread-list", setId("thread-list", element("div", "thread-list", null)));
  thread.setAttribute("tabindex", "0");
  setData("follow-bottom", "true", thread);
  const composer=setTestId("chat-composer", element("div", "chat-composer", null));
  const draft=setTestId("chat-draft", textarea("draft", "Type a message"));
  const actions=element("div", "actions", null);
  const send=setTestId("chat-send", button("primary", "Send"));
  const participantsCacheKey=cacheKey("chat-agents", ofArray([participantId]));
  const threadCacheKey=(peerId) => cacheKey("chat-thread", ofArray([participantId, peerId]));
  append(titleBox, [element("label", "", "To"), toTitle]);
  append(workHead, [titleBox, state]);
  append(actions, [send]);
  append(composer, [draft, actions]);
  append(work, [workHead, pendingState, thread, composer]);
  append(page, [side, work]);
  const sameText=(left, right) => asText(left).toLowerCase()==asText(right).toLowerCase();
  const isPendingForThisChat=(command) =>!(command==null)&&sameText(command.kind, "chat-send")&&StartsWith(asText(command.target), participantId+"->");
  replayingPending=false;
  chatSocket=null;
  queuedChatSyncFrames=[];
  subscribedChatStream="";
  pendingWsChatIds=[];
  const setChatWsState=(value) => {
    setData("ws-state", value, work);
  };
  const chatStreamKey=(peerId) => New_6("", "set", "chat", sameText(peerId, "channel.public")?["channel:public"]:[participantId, peerId]);
  const streamIdentity=(streamKey) => concat_1("\n", [asText(streamKey.pageId), asText(streamKey.mode), asText(streamKey.setName), concat_1("\u001f", arrayOrEmpty(streamKey.keys))]);
  function renderParticipants(){
    let _1;
    clear(list);
    iter((p_1) => {
      const className=p_1.participantId==selected?"list-card active":"list-card";
      const name=textOr(p_1.participantId, p_1.displayName);
      const line=asText(p_1.kind)+" / "+joinValues(p_1.labels);
      const item=button(className, null);
      setData("participant-id", p_1.participantId, setTestId("chat-participant", item));
      item.appendChild(cardTitle(name, p_1.participantId, p_1.status, line));
      item.addEventListener("click", () => {
        selected=p_1.participantId;
        cursor="";
        selectedThreadMessages=[];
        setData("message-count", "0", export_1);
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
    let appendedCount;
    const shouldFollow=isNearBottom(thread);
    appendedCount=0;
    iter((message) => {
      if(!(message==null)&&!isBlank(message.messageId)&&doc().getElementById("thread-"+message.messageId)==null){
        appendedCount=appendedCount+1;
        const outbound=message.fromId==participantId;
        const wrap=setId("thread-"+message.messageId, element("div", outbound?"message outbound":"message inbound", null));
        setData("message-id", message.messageId, setTestId("chat-message", wrap));
        const meta=element("div", "message-meta", null);
        const route=message.scope=="public"?outbound?"You -> Public":asText(message.fromId)+" -> Public":outbound?"You -> "+asText(message.toId):asText(message.fromId)+" -> You";
        const idNode=setData("full-message-id", message.messageId, element("span", "message-id", compactMessageId(message.messageId)+"  "+asText(message.createdAtUtc)));
        idNode.setAttribute("title", message.messageId+"  "+asText(message.createdAtUtc));
        append(meta, [element("span", "", route), idNode]);
        append(wrap, [meta, element("pre", "message-body", asText(message.body))]);
        thread.appendChild(wrap);
      }
    }, arrayOrEmpty(messages));
    selectedThreadMessages=distinctMessages(selectedThreadMessages.concat(arrayOrEmpty(messages)));
    setData("message-count", String(length(selectedThreadMessages)), export_1);
    if(appendedCount>0&&shouldFollow)scrollToBottomNow(thread);
    setData("follow-bottom", isNearBottom(thread)?"true":"false", thread);
  }
  function loadParticipants(refreshSelectedThread){
    setStatus(state, "Loading participants");
    readJson(participantsCacheKey, (a) => {
      if(a!=null&&a.$==1)if(a.$0,length(participants)===0){
        participants=arrayOrEmpty(a.$0.participants);
        isBlank(selected)&&length(participants)>0?selected=get(participants, 0).participantId:void 0;
        renderParticipants();
        setStatus(state, "Loaded "+String(length(participants))+" cached participant(s)");
        refreshSelectedThread?(pollThread(true),ensureSelectedChatSubscription(),replayPendingChatCommands()):void 0;
      }
    });
    getJson("/chat/api/agents", (data) => {
      participants=arrayOrEmpty(data.participants);
      writeSnapshotWithWatermark(participantsCacheKey, data, 0n, length(participants), "chat-agents");
      const selectedWasBlank=isBlank(selected);
      if(selectedWasBlank&&length(participants)>0)selected=get(participants, 0).participantId;
      renderParticipants();
      setStatus(state, "Loaded "+String(length(participants))+" participant(s)");
      if(refreshSelectedThread||selectedWasBlank){
        pollThread(true);
        ensureSelectedChatSubscription();
        replayPendingChatCommands();
      }
    }, (t) => {
      setStatus(state, t);
    });
  }
  function pollThread(force){
    if(!isBlank(selected)&&!polling){
      polling=true;
      const cacheKey_1=threadCacheKey(selected);
      const fetchThread=(useCursor) => {
        let url;
        url="/chat/api/thread?participantId="+encodeURIComponent(participantId)+"&peerId="+encodeURIComponent(selected);
        if(useCursor&&!isBlank(cursor))url=url+"&afterMessageId="+encodeURIComponent(cursor);
        getJson(url, (data) => {
          const messages=force&&!useCursor?latestArray(defaultRenderLimit(), data.messages):arrayOrEmpty(data.messages);
          appendMessages(messages);
          if(!isBlank(data.nextAfterMessageId))cursor=data.nextAfterMessageId;
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
              writeSnapshotWithWatermark(cacheKey_1, New_28(merged, nextAfterMessageId), _3, length(merged), "chat-thread");
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
          if(!isBlank(cached.nextAfterMessageId))cursor=cached.nextAfterMessageId;
          setStatus(state, "Loaded "+String(length(messages))+" cached message(s); syncing missing tail");
          fetchThread(false);
        }
      });
      else fetchThread(!isBlank(cursor));
    }
  }
  function refreshChatPendingState(){
    readPendingRealitySplit((_1, _2) => renderPendingInspection(pendingState, filter_1(isPendingForThisChat, _1), filter_1(isPendingForThisChat, _2)));
  }
  function replayPendingChatCommands(){
    if(!replayingPending){
      replayingPending=true;
      readAllPending((commands) => {
        let remaining, accepted;
        const mine=filter_1((command) => sameText(command.method, "POST")&&!isBlank(command.url)&&!isBlank(command.payloadJson), filter_1(isPendingForThisChat, commands));
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
                const reply=json(isBlank(responseBody)?"{}":responseBody);
                if(!(reply.message==null)&&!isBlank(reply.message.messageId))deletePendingThen(command.commandId, () => {
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
    if(!(message==null)&&!isBlank(message.messageId)&&!isBlank(selected)){
      const cacheKey_1=threadCacheKey(selected);
      return readJson(cacheKey_1, (cached) => {
        const merged=mergeThreadMessages(cached==null?[]:cached.$0.messages, [message]);
        writeSnapshotWithWatermark(cacheKey_1, New_28(merged, message.messageId), sequence>0n?sequence:maxMessageSequence(merged), length(merged), "chat-thread");
      });
    }
    else return null;
  }
  function handleChatSyncMessage(text_1){
    try {
      let o;
      const response=json(text_1);
      const responseType=asText(response.type).toLowerCase();
      const responseStatus=asText(response.status).toLowerCase();
      const requestId=asText(response.requestId);
      if(responseStatus=="ok"){
        if(responseType=="subscribe")setChatWsState("subscribed");
        else if(responseType=="chat-send"){
          exists((id) => id==requestId, pendingWsChatIds)?(pendingWsChatIds=filter_1((id) => id!=requestId, pendingWsChatIds),deletePendingThen(requestId, () => {
            refreshChatPendingState();
            draft.value="";
          })):void 0;
          !(response.message==null)&&!isBlank(response.message.messageId)?(appendMessages([response.message]),cacheAcceptedChatMessage(response.event==null?0n:response.event.sequence, response.message),cursor=response.message.messageId):void 0;
          setStatus(state, "Sent "+textOr("message", response.message==null?"":response.message.messageId)+" "+asText(response.deliveryHint));
        }
        else if(responseType=="stream-event"){
          const event=response.event;
          if(!isBlank(selected)&&!(event==null)&&!(event.streamKey==null)&&streamIdentity(event.streamKey)==streamIdentity(chatStreamKey(selected))){
            const event_1=response.event;
            if(event_1==null||isBlank(event_1.payload))o=null;
            else try {
              const message=json(event_1.payload);
              o=message==null||isBlank(message.messageId)?null:Some(message);
            }
            catch(m){
              o=Some(New_27(textOr(event_1.eventId, event_1.sourceId), "", participantId, "direct", asText(event_1.payload), asText(event_1.createdAtUtc)));
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
      else responseStatus=="error"?exists((id) => id==requestId, pendingWsChatIds)?(setStatus(state, pendingFailure("WebSocket chat send", asText(response.error))),refreshChatPendingState()):setStatus(state, "WebSocket chat error: "+asText(response.error)):null;
    }
    catch(error){
      setStatus(state, "WebSocket chat parse failed: "+errorMessage(error));
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
    if(!isBlank(selected)){
      const streamKey=chatStreamKey(selected);
      const identity=streamIdentity(streamKey);
      if(!isBlank(identity)&&identity!=subscribedChatStream){
        subscribedChatStream=identity;
        setChatWsState("subscribing");
        sendChatSyncFrame(JSON.stringify(New_3("subscribe", newRequestId("chat-subscribe"), streamKey)));
      }
    }
  }
  function sendMessage(){
    const body=Trim(draft.value);
    if(isBlank(selected))setStatus(state, "Select a participant first");
    else if(isBlank(body))setStatus(state, "Message is empty");
    else {
      const request=New_31(participantId, selected, body, ["web-chat"]);
      const pendingId=rememberPending("chat-send", participantId+"->"+selected, "/chat/api/send", request);
      const wsRequest=New_30("chat-send", pendingId, participantId, selected, body, ["web-chat"], participantId, "chat");
      pendingWsChatIds=pendingWsChatIds.concat([pendingId]);
      refreshChatPendingState();
      setStatus(state, "Sending through WebSocket; pending command saved in browser DB");
      sendChatSyncFrame(JSON.stringify(wsRequest));
    }
  }
  function exportSelectedThread(){
    if(isBlank(selected))setStatus(state, "Select a participant first");
    else if(length(selectedThreadMessages)===0)setStatus(state, "No loaded messages to export");
    else if(globalThis.document.body==null)setStatus(state, "Document body is unavailable");
    else {
      try {
        const rows=map((message) => New_32(asText(message.messageId), asText(message.fromId), asText(message.createdAtUtc), asText(message.body)), selectedThreadMessages);
        const url=URL.createObjectURL(new Blob([concat_1("\n", map((v) => JSON.stringify(v), rows))], {type:"application/x-ndjson;charset=utf-8"}));
        const now=new Date();
        const twoDigits_1=(value) => value<10?"0"+String(value):String(value);
        const timestamp=String(now.getFullYear())+twoDigits_1(now.getMonth()+1)+twoDigits_1(now.getDate())+twoDigits_1(now.getHours())+twoDigits_1(now.getMinutes())+twoDigits_1(now.getSeconds());
        const anchor=globalThis.document.createElement("a");
        anchor.setAttribute("href", url);
        anchor.setAttribute("download", "ptcs-chat-"+timestamp+".jsonl");
        anchor.setAttribute("aria-hidden", "true");
        anchor.className="download-anchor";
        globalThis.document.body.appendChild(anchor);
        anchor.click();
        globalThis.document.body.removeChild(anchor);
        setTimeout(() => {
          URL.revokeObjectURL(url);
        }, 250);
        setStatus(state, "Exported "+String(length(rows))+" message(s)");
      }
      catch(error){
        setStatus(state, "Chat export failed: "+errorMessage(error));
      }
    }
  }
  reload.addEventListener("click", () => loadParticipants(true));
  export_1.addEventListener("click", exportSelectedThread);
  send.addEventListener("click", sendMessage);
  thread.addEventListener("scroll", () => {
    setData("follow-bottom", isNearBottom(thread)?"true":"false", thread);
  });
  draft.addEventListener("keydown", (event) => event.key=="Enter"&&!event.shiftKey?(event.preventDefault(),sendMessage()):null);
  globalThis.setInterval(() => pollThread(false), 2500);
  globalThis.setInterval(() => loadParticipants(false), 2500);
  refreshChatPendingState();
  loadParticipants(true);
}
function refreshAppendNav(activePath){
  const applyDefinitions=(data) => {
    const nav=doc().getElementById("ptc-nav");
    if(!(nav==null))renderNav(nav, activePath, arrayOrEmpty(data.pages));
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
function tryMountLoginWithRegisteredRenderers(root, configJson){
  let r;
  const _1=root;
  const _2=configJson;
  if(!(globalThis.PulseTrade&&globalThis.PulseTrade.LoginRenderers))return false;
  let renderers=globalThis.PulseTrade.LoginRenderers;
  for(let i=0;i<renderers.length;i++){
    let r_1=renderers[i];
    try {
      let value=(r_1.render||r_1[1])(_1, _2);
      if(value===true)return true;
    }
    catch(e){
      console.error("Login renderer exception:", e);
    }
  }
  return false;
}
function mountLoginFallback(root){
  const config=loginConfig();
  const frame=element("section", "login-frame", null);
  frame.setAttribute("aria-label", "PTCS Login");
  const systemPanel=element("aside", "system-panel", null);
  const brand=element("div", "", null);
  append(brand, [element("div", "brand-mark", "PT"), element("p", "brand-title", "PulseTrade Comm Spa"), element("p", "brand-subtitle", "\u672c\u9801\u793a\u610f PTCS.Login provider \u7684\u81ea\u6709\u767b\u5165\u5165\u53e3\u3002\u767b\u5165\u6210\u529f\u5f8c\u7531 server \u8a2d\u5b9a HttpOnly session cookie\uff0c\u518d\u56de\u5230\u53d7\u4fdd\u8b77\u7684 PTCS \u9801\u9762\u3002")]);
  const routes=element("ul", "route-list", null);
  routes.setAttribute("aria-label", "Login context");
  append(routes, [routeItem("P", "Protected route", textOr("/actors", config.protectedRoute)), routeItem("S", "Session cookie", textOr("ptc_login_session", config.sessionCookieName)), routeItem("A", "ACL mode", textOr("enabled or authenticated-only", config.aclLabel))]);
  append(systemPanel, [brand, routes]);
  const formPanel=element("section", "form-panel", null);
  const card=element("div", "form-card", null);
  const statusRow=element("div", "status-row", null);
  const providerPill=element("span", "pill", null);
  const bypassPill=element("span", "pill", null);
  append(providerPill, [element("span", "dot", ""), doc().createTextNode(textOr("PTCS.Login", config.providerLabel))]);
  append(bypassPill, [element("span", "dot warn", ""), doc().createTextNode("OAuth bypass")]);
  append(statusRow, [providerPill, bypassPill]);
  const errorBox=setTestId("ptcs-login-error", element("p", "error-box", "\u767b\u5165\u5931\u6557\u3002\u8acb\u78ba\u8a8d\u5e33\u865f\u6216\u5bc6\u78bc\u3002"));
  errorBox.setAttribute("role", "alert");
  const userName=setTestId("ptcs-login-username", setId("username", input("admin")));
  userName.setAttribute("name", "username");
  userName.setAttribute("type", "text");
  userName.setAttribute("autocomplete", "username");
  const password=setTestId("ptcs-login-password", setId("password", input("\u8f38\u5165\u5bc6\u78bc")));
  password.setAttribute("name", "password");
  password.setAttribute("type", "password");
  password.setAttribute("autocomplete", "current-password");
  const keepSession=doc().createElement("input");
  keepSession.setAttribute("name", "keepSession");
  keepSession.setAttribute("type", "checkbox");
  keepSession.setAttribute("value", "true");
  const inlineRow=element("div", "inline-row", null);
  const checkboxLabel=element("label", "checkbox-row", null);
  append(checkboxLabel, [keepSession, doc().createTextNode("\u4fdd\u6301\u6b64\u700f\u89bd\u5668\u767b\u5165")]);
  append(inlineRow, [checkboxLabel, setHref("/login/help", element("a", "link", "\u9700\u8981\u5354\u52a9?"))]);
  const form=element("form", "", null);
  form.setAttribute("method", "post");
  form.setAttribute("action", config.submitPath);
  const submit_1=setTestId("ptcs-login-submit", button("", "\u767b\u5165\u4e26\u8fd4\u56de PTCS"));
  const setError=(text_1) => {
    errorBox.textContent=textOr("\u767b\u5165\u5931\u6557\u3002\u8acb\u78ba\u8a8d\u5e33\u865f\u6216\u5bc6\u78bc\u3002", text_1);
    errorBox.className="error-box visible";
  };
  const submitLogin=() => {
    const request=New_35(Trim(userName.value), password.value, config.returnUrl, keepSession.checked);
    if(isBlank(request.userName)||isBlank(request.password))setError("\u8acb\u8f38\u5165\u5e33\u865f\u8207\u5bc6\u78bc\u3002");
    else {
      errorBox.className="error-box";
      submit_1.setAttribute("disabled", "disabled");
      submit_1.textContent="\u767b\u5165\u4e2d";
      postJson(config.submitPath, request, (reply) => {
        const target=textOr(config.returnUrl, reply.returnUrl);
        globalThis.location.assign(target);
      }, (error) => {
        submit_1.removeAttribute("disabled");
        submit_1.textContent="\u767b\u5165\u4e26\u8fd4\u56de PTCS";
        setError(isBlank(error)?"\u767b\u5165\u5931\u6557\u3002\u8acb\u78ba\u8a8d\u5e33\u865f\u6216\u5bc6\u78bc\u3002":error);
      });
    }
  };
  form.addEventListener("submit", (event) => {
    event.preventDefault();
    return submitLogin();
  });
  submit_1.addEventListener("click", submitLogin);
  append(form, [field("\u5e33\u865f", "username", userName), field("\u5bc6\u78bc", "password", password), inlineRow, submit_1]);
  append(card, [statusRow, element("h1", "", textOr("\u767b\u5165 PTCS", config.title)), element("p", "lead", textOr("\u4f7f\u7528 host \u63d0\u4f9b\u7684\u5e33\u865f\u767b\u5165\u3002\u6b0a\u9650\u7531\u767b\u5165\u5f8c\u53d6\u5f97\u7684 principal \u8207 ACL policy \u6c7a\u5b9a\u3002", config.lead)), errorBox, form, element("p", "footer-note", "Browser flow \u61c9\u53ea\u56de HttpOnly cookie\uff1bheadless/API/WS \u624d\u4f7f\u7528 bearer token\u3002\u63d0\u4ea4\u7aef\u9ede\u793a\u610f\u70ba /login/api/submit\u3002")]);
  append(formPanel, [card]);
  append(frame, [systemPanel, formPanel]);
  clear(root);
  root.appendChild(frame);
}
function loginConfig(){
  const node=doc().getElementById("ptcs-login-config");
  return node==null||isBlank(node.textContent)?New_34("/login/api/submit", "/login/api/session", "/login/logout", "/actors", "/actors", "ptc_login_session", "\u767b\u5165 PTCS", "\u4f7f\u7528 host \u63d0\u4f9b\u7684\u5e33\u865f\u767b\u5165\u3002\u6b0a\u9650\u7531\u767b\u5165\u5f8c\u53d6\u5f97\u7684 principal \u8207 ACL policy \u6c7a\u5b9a\u3002", "PTCS.Login", "ACL mode"):json(node.textContent);
}
function textOr(fallback, value){
  return isBlank(value)?fallback:value;
}
function pageDefinitionFromWire(wire){
  if(wire==null||asText(wire.schema)!="ptc.comm.spa.append-page.definition.v1"||isBlank(wire.pageId))return null;
  else {
    const pageId=asText(wire.pageId);
    return Some(New_5(pageId, textOr(pageId, wire.tabId), textOr("/page/"+pageId, wire.path), textOr(pageId, wire.title), textOr(pageId, wire.setName), textOr("raw", wire.shape), asText(wire.description), textOr("\"Aster\"", wire.keyPlaceholder), textOr("JSON value", wire.valuePlaceholder), asText(wire.defaultKey), arrayOrEmpty(wire.tags)));
  }
}
function hiddenPageFromWire(wire){
  if(wire==null||asText(wire.schema)!="ptc.comm.spa.append-page.hidden.v1"||isBlank(wire.pageId))return null;
  else {
    const pageId=asText(wire.pageId);
    return Some([pageId, textOr(pageId, wire.tabId)]);
  }
}
function sameTextInvariant(left, right){
  return asText(left).toLowerCase()==asText(right).toLowerCase();
}
function sortAppendPages(pages){
  return sortBy((page) =>[asText(page.title).toLowerCase(), asText(page.pageId).toLowerCase()], arrayOrEmpty(pages));
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
function requestOptions(){
  return{credentials:"same-origin"};
}
function errorMessage(error){
  return error==null?"request failed":String(error);
}
function isCurrentPage(activePath, href){
  return TrimEnd(activePath, ["/"])==TrimEnd(href, ["/"]);
}
function pagePath(page){
  const pageId=asText(page.pageId);
  const path=asText(page.path);
  return exists((alias) => sameTextInvariant(path, alias), ["/fcell-chat", "/fcell-list", "/fcell-grid"])?path:"/page/"+pageId;
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
function defaultRenderLimit(){
  return _c_1.defaultRenderLimit;
}
function button(className, text_1){
  const node=element("button", className, text_1);
  node.setAttribute("type", "button");
  return node;
}
function input(placeholder){
  const node=doc().createElement("input");
  node.placeholder=placeholder;
  return node;
}
function textarea(className, placeholder){
  const node=doc().createElement("textarea");
  node.className=className;
  node.placeholder=placeholder;
  return node;
}
function pageAclAllows(pageId, action){
  return aclAllows(action, "ptcs.page", pageId);
}
function setHidden(hidden, node){
  hidden?node.setAttribute("hidden", "hidden"):node.removeAttribute("hidden");
  return node;
}
function append(parent, children){
  for(let i=0, _1=children.length-1;i<=_1;i++)parent.appendChild(get(children, i));
  return parent;
}
function actorArguButtonLabel(page){
  return isActorArguPage(page)?"Tell":"Append";
}
function pageTitle(page){
  return textOr(asText(page.pageId), asText(page.title));
}
function pageTypeLabel(page){
  const shapeText=asText(page.shape).toLowerCase();
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
  return currentBrowserUser().participantId;
}
function renderPendingInspection(node, commands, foreignCommands){
  let _1, shown, shown_1;
  const commands_1=arrayOrEmpty(commands);
  const foreignCommands_1=arrayOrEmpty(foreignCommands);
  node.setAttribute("data-pending-count", String(length(commands_1)));
  node.setAttribute("data-foreign-pending-count", String(length(foreignCommands_1)));
  node.setAttribute("data-foreign-pending-realities", concat_1(",", distinct(map((command) => asText(command.serverRealityId), foreignCommands_1))));
  node.setAttribute("data-pending-kinds", concat_1(",", map((a) => a.kind, commands_1)));
  node.setAttribute("data-pending-targets", concat_1("\n", map((a) => a.target, commands_1)));
  node.setAttribute("data-pending-urls", concat_1("\n", map((a) => a.url, commands_1)));
  node.setAttribute("data-pending-statuses", concat_1(",", map((a) => a.status, commands_1)));
  clear(node);
  if(length(commands_1)>0){
    node.appendChild(element("div", "strong", "Pending commands: "+String(length(commands_1))));
    const list=setTestId("pending-command-list", element("div", "pending-inspection-list", null));
    _1=(shown=0,iter((command) => {
      if(shown<4){
        shown=shown+1;
        const row=setData("pending-status", command.status, setData("pending-url", command.url, setData("pending-target", command.target, setData("pending-kind", command.kind, setTestId("pending-command-row", element("div", "pending-command-row wrap", null))))));
        append(row, [element("span", "strong pending-command-kind", asText(command.kind)), element("span", "muted wrap pending-command-target", asText(command.target)), element("span", "meta wrap pending-command-status", String(asText(command.method))+" "+String(asText(command.url))+" / "+String(asText(command.status)))]);
        list.appendChild(row);
      }
    }, commands_1),length(commands_1)>shown?list.appendChild(element("div", "meta", "+"+String(length(commands_1)-shown)+" more pending command(s)")):void 0,node.appendChild(list));
  }
  else _1=void 0;
  if(length(foreignCommands_1)>0){
    node.appendChild(setData("foreign-pending-count", String(length(foreignCommands_1)), setTestId("foreign-pending-summary", element("div", "pending-foreign-summary meta", "Foreign pending blocked/stale: "+String(length(foreignCommands_1))))));
    const list_1=setTestId("foreign-pending-list", element("div", "pending-foreign-list", null));
    shown_1=0;
    iter((command) => {
      if(shown_1<3){
        shown_1=shown_1+1;
        const x=setTestId("foreign-pending-row", element("div", "pending-command-row pending-command-foreign wrap", null));
        let _2=setData("pending-reality", asText(command.serverRealityId), x);
        let _3=setData("pending-kind", command.kind, _2);
        const row=setData("pending-target", command.target, _3);
        append(row, [element("span", "strong pending-command-kind", asText(command.kind)), element("span", "muted wrap pending-command-target", asText(command.target)), element("span", "meta wrap pending-command-status", "blocked/stale / "+asText(command.serverRealityId))]);
        list_1.appendChild(row);
      }
    }, foreignCommands_1);
    if(length(foreignCommands_1)>shown_1)list_1.appendChild(element("div", "meta", "+"+String(length(foreignCommands_1)-shown_1)+" more foreign pending command(s)"));
    node.appendChild(list_1);
  }
  else void 0;
}
function appendPageValueCount(snapshot){
  return snapshot==null?0:fold((_1, _2) => _1+_2, 0, map((bucket) => {
    if(bucket==null)return 0;
    else {
      const a=bucket.valueCount;
      const b=length(arrayOrEmpty(bucket.values));
      return Compare(a, b)===1?a:b;
    }
  }, arrayOrEmpty(snapshot.buckets)));
}
function keysAsJson(keys){
  const keys_1=arrayOrEmpty(keys);
  return length(keys_1)===1?JSON.stringify(get(keys_1, 0)):JSON.stringify(keys_1);
}
function disposeReplyPresentation(identity){
  let _1;
  const o=tryPick((_2) => _2[0]==identity?Some(_2[1]):null, replyPresentationDisposers());
  if(o==null)_1=void 0;
  else try {
    _1=o.$0();
  }
  catch(m){
    _1=null;
  }
  set_replyPresentationDisposers(filter_1((_2) => _2[0]!=identity, replyPresentationDisposers()));
}
function joinValues(values){
  const values_1=arrayOrEmpty(values);
  return length(values_1)===0?"":concat_1(" / ", values_1);
}
function latestArray(limit, values){
  const values_1=arrayOrEmpty(values);
  return length(values_1)<=limit?values_1:skip(length(values_1)-limit, values_1);
}
function setStatus(node, text_1){
  node.textContent=text_1;
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
  const options=requestOptions();
  options.method="POST";
  options.headers=headers;
  options.body=JSON.stringify(body);
  (globalThis.fetch(url, options).then((response) => response.text().then((responseBody) => response.ok?onOk(json(isBlank(responseBody)?"{}":responseBody)):onError(isBlank(responseBody)?"POST "+String(url)+" "+String(response.status):responseBody))))["catch"]((error) => onError(errorMessage(error)));
}
function pendingFailure(action, error){
  return String(action)+" failed; pending command kept in browser DB: "+String(asText(error));
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
function tryRenderAppendInputWithRegisteredRenderers(pageId, shape, title, setName, selectedKeyId, selectedKeyJson, selectedKeys, valuePlaceholder, valueText, submit_1, setValue, composerMode, setComposerMode){
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
  const _10=submit_1;
  const _11=setValue;
  const _12=composerMode;
  const _13=setComposerMode;
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
    },
    composerMode:String(_12||"plain"),
    setComposerMode:(mode) => {
      _13(mode);
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
  const options=requestOptions();
  options.method="POST";
  options.headers=headers;
  options.body=textOr("{}", payloadJson);
  (globalThis.fetch(url, options).then((response) => response.text().then((responseBody) => response.ok?onOk(responseBody):onError(isBlank(responseBody)?"POST "+String(url)+" "+String(response.status):responseBody))))["catch"]((error) => onError(errorMessage(error)));
}
function postJson(url, body, onOk, onError){
  const headers=new Headers();
  headers.set("Content-Type", "application/json");
  const options=requestOptions();
  options.method="POST";
  options.headers=headers;
  options.body=JSON.stringify(body);
  (globalThis.fetch(url, options).then((response) => response.text().then((responseBody) => response.ok?onOk(json(isBlank(responseBody)?"{}":responseBody)):onError(isBlank(responseBody)?"POST "+String(url)+" "+String(response.status):responseBody))))["catch"]((error) => onError(errorMessage(error)));
}
function postRemoveAppendPageKey(url, body, onOk, onError){
  const headers=new Headers();
  headers.set("Content-Type", "application/json");
  const options=requestOptions();
  options.method="POST";
  options.headers=headers;
  options.body=JSON.stringify(body);
  (globalThis.fetch(url, options).then((response) => response.text().then((responseBody) => response.ok?onOk(json(isBlank(responseBody)?"{}":responseBody)):onError(isBlank(responseBody)?"POST "+String(url)+" "+String(response.status):responseBody))))["catch"]((error) => onError(errorMessage(error)));
}
function renderAppendValue(definition, value){
  let mounted, savedScrollTop, modeBeforeFullscreen, focusBeforeFullscreen, presentationRendered, _1, _2;
  const mode=asText(value.mode);
  const m=mode.toLowerCase();
  const className=m=="inbound-message"?"fcell-card fcell-chat inbound":m=="outbound-message"?"fcell-card fcell-chat outbound":m=="list"?"fcell-card fcell-list":m=="grid"?"fcell-card fcell-grid":"fcell-card";
  const card=setData("mode", mode, setTestId("append-value-card", element("div", className, null)));
  const head_2=element("div", "fcell-head", null);
  append(head_2, [element("span", "fcell-pill", fcellValueModeLabel(mode, value.tags)), element("span", "muted wrap", asText(value.valueId)+" / "+asText(value.createdAtUtc))]);
  card.appendChild(head_2);
  const presentationContext=New_37(asText(definition.pageId), asText(definition.tabId), asText(value.valueId), asText(value.createdAtUtc), mode, arrayOrEmpty(value.tags), asText(value.rawValue));
  const m_1=tryResolveReplyPresentation(presentationContext);
  if(m_1!=null&&m_1.$==1){
    const presentation=m_1.$0;
    const identity=replyPresentationIdentity(presentationContext);
    const x=setData("reply-id", identity, setTestId("reply-presentation", element("section", "reply-presentation", null)));
    const shell_1=setData("presentation-kind", asText(presentation.Kind), x);
    const summary=setTestId("reply-presentation-summary", element("div", "reply-presentation-summary", null));
    summary.setAttribute("role", "button");
    summary.setAttribute("tabindex", "0");
    summary.appendChild(presentation.RenderSummary());
    const controls=element("div", "reply-presentation-actions", null);
    const actionFeedback=setTestId("reply-presentation-action-feedback", element("span", "reply-presentation-action-feedback", null));
    actionFeedback.setAttribute("aria-live", "polite");
    const presentationActions=presentation.Actions==null?[]:presentation.Actions;
    for(let i=0, _4=presentationActions.length-1;i<=_4;i++)((() => {
      const action=get(presentationActions, i);
      const actionButton=setData("action-id", asText(action.ActionId), setTestId("reply-presentation-extension-action", button("reply-presentation-extension-action", textOr("Action", action.Label))));
      actionButton.setAttribute("title", textOr(action.Label, action.Title));
      actionButton.setAttribute("aria-label", textOr(action.Label, action.Title));
      actionButton.addEventListener("click", (event) => {
        event.stopPropagation();
        try {
          const m_3=action.Invoke();
          if(m_3.$==1){
            const message=m_3.$0;
            actionFeedback.className="reply-presentation-action-feedback error";
            actionFeedback.textContent=textOr("Action failed", message);
            return;
          }
          else {
            const message_1=m_3.$0;
            actionFeedback.className="reply-presentation-action-feedback success";
            actionFeedback.textContent=textOr("Done", message_1);
            return;
          }
        }
        catch(error){
          actionFeedback.className="reply-presentation-action-feedback error";
          actionFeedback.textContent=textOr("Action failed", errorMessage(error));
          return;
        }
      });
      controls.appendChild(actionButton);
    })());
    const toggle=setTestId("reply-presentation-toggle", button("reply-presentation-toggle", "+"));
    toggle.setAttribute("title", "Expand in chat session");
    toggle.setAttribute("aria-label", "Expand reply in chat session");
    const fullscreen=setTestId("reply-presentation-fullscreen-toggle", button("reply-presentation-fullscreen-toggle", "\u5c55\u958b"));
    fullscreen.setAttribute("title", "Open near-fullscreen canvas");
    fullscreen.setAttribute("aria-label", "Open reply as near-fullscreen canvas");
    const inlineHost=setTestId("reply-presentation-inline", element("div", "reply-presentation-inline", null));
    controls.appendChild(toggle);
    controls.appendChild(fullscreen);
    controls.appendChild(actionFeedback);
    shell_1.appendChild(summary);
    shell_1.appendChild(controls);
    shell_1.appendChild(inlineHost);
    card.appendChild(shell_1);
    mounted=false;
    savedScrollTop=0;
    modeBeforeFullscreen="collapsed";
    focusBeforeFullscreen=null;
    const mountInline=() => {
      if(!mounted){
        clear(inlineHost);
        try {
          registerReplyPresentationDisposer(identity, (presentation.Mount("inline"))(inlineHost));
          mounted=true;
          setData("mount-state", "mounted", shell_1);
        }
        catch(error){
          inlineHost.appendChild(element("div", "reply-presentation-error", textOr("Reply presentation failed.", errorMessage(error))));
          setData("mount-state", "error", shell_1);
        }
      }
    };
    function applyPresentationMode(nextMode){
      const m_3=asText(nextMode).toLowerCase();
      const normalized=m_3=="inline"?"inline":m_3=="fullscreen"?"fullscreen":"collapsed";
      setReplyPresentationMode(identity, normalized);
      setData("presentation-mode", normalized, shell_1);
      if(normalized=="collapsed"){
        disposeReplyPresentation(identity);
        mounted=false;
        clear(inlineHost);
        setData("mount-state", "unmounted", shell_1);
        inlineHost.setAttribute("hidden", "hidden");
        fullscreen.removeAttribute("hidden");
        shell_1.className="reply-presentation";
        summary.setAttribute("aria-expanded", "false");
        toggle.textContent="+";
        toggle.setAttribute("title", "Expand in chat session");
        toggle.setAttribute("aria-label", "Expand reply in chat session");
        fullscreen.textContent="\u5c55\u958b";
        fullscreen.setAttribute("title", "Open near-fullscreen canvas");
        fullscreen.setAttribute("aria-label", "Open reply as near-fullscreen canvas");
      }
      else normalized=="fullscreen"?(mountInline(),inlineHost.removeAttribute("hidden"),fullscreen.removeAttribute("hidden"),shell_1.className="reply-presentation fullscreen",summary.setAttribute("aria-expanded", "true"),toggle.textContent="\u2212",toggle.setAttribute("title", "Collapse reply"),toggle.setAttribute("aria-label", "Collapse reply"),fullscreen.textContent="\u8fd4\u56de",fullscreen.setAttribute("title", "Return to inline canvas"),fullscreen.setAttribute("aria-label", "Return to inline canvas")):(mountInline(),inlineHost.removeAttribute("hidden"),fullscreen.removeAttribute("hidden"),shell_1.className="reply-presentation",summary.setAttribute("aria-expanded", "true"),toggle.textContent="\u2212",toggle.setAttribute("title", "Collapse reply"),toggle.setAttribute("aria-label", "Collapse reply"),fullscreen.textContent="\u5c55\u958b",fullscreen.setAttribute("title", "Open near-fullscreen canvas"),fullscreen.setAttribute("aria-label", "Open reply as near-fullscreen canvas"));
    }
    const toggleInline=() => {
      if(replyPresentationMode(identity)=="collapsed")applyPresentationMode("inline");
      else applyPresentationMode("collapsed");
    };
    presentationRendered=(summary.addEventListener("click", toggleInline),toggle.addEventListener("click", toggleInline),fullscreen.addEventListener("click", () => {
      if(replyPresentationMode(identity)=="fullscreen"){
        applyPresentationMode(modeBeforeFullscreen);
        const m_3=card.parentElement;
        if(Equals(m_3, null))null;
        else m_3.scrollTop=savedScrollTop;
        return!(focusBeforeFullscreen==null)?focusBeforeFullscreen.focus():null;
      }
      else {
        modeBeforeFullscreen=replyPresentationMode(identity);
        focusBeforeFullscreen=globalThis.document.activeElement;
        const m_4=card.parentElement;
        if(Equals(m_4, null))savedScrollTop=0;
        else savedScrollTop=m_4.scrollTop;
        return applyPresentationMode("fullscreen");
      }
    }),applyPresentationMode(replyPresentationMode(identity)),true);
  }
  else presentationRendered=false;
  if(!presentationRendered){
    const m_2=mode.toLowerCase();
    switch(m_2){
      case"outbound-message":
      case"inbound-message":
        _1=iter((row) => {
          card.appendChild(renderTextBlock("fcell-message-body", row));
        }, arrayOrEmpty(value.rows));
        break;
      case"list":
        const list=element("ul", "fcell-list-items", null);
        _1=(iter((row) => {
          list.appendChild(element("li", "", asText(row)));
        }, arrayOrEmpty(value.rows)),void card.appendChild(list));
        break;
      case"grid":
        let _3;
        const table=element("table", "fcell-grid-table", null);
        const columns=arrayOrEmpty(value.columns);
        if(length(columns)>0){
          const thead=element("thead", "", null);
          const header=element("tr", "", null);
          _3=(iter((column) => {
            header.appendChild(element("th", "wrap", asText(column)));
          }, columns),thead.appendChild(header),void table.appendChild(thead));
        }
        else _3=null;
        const tbody=element("tbody", "", null);
        _1=(iter((cells) => {
          const tr=element("tr", "", null);
          iter((cell) => {
            tr.appendChild(element("td", "wrap", asText(cell)));
          }, arrayOrEmpty(cells));
          tbody.appendChild(tr);
        }, arrayOrEmpty(value.tableRows)),table.appendChild(tbody),void card.appendChild(table));
        break;
      default:
        _1=void card.appendChild(renderTextBlock("fcell-source", value.rawValue));
        break;
    }
    _2=!isBlank(value.source)&&mode.toLowerCase()!="inbound-message"&&mode.toLowerCase()!="outbound-message"?void card.appendChild(renderTextBlock("fcell-source", value.source)):null;
  }
  else _2=null;
  return card;
}
function staticNavigationDestinations(){
  return _c_1.staticNavigationDestinations;
}
function setHref(href, node){
  node.setAttribute("href", href);
  return node;
}
function pageTypeClass(page){
  const shapeText=asText(page.shape).toLowerCase();
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
  const shapeText=asText(page.shape).toLowerCase();
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
function renderTabJumpOptions(jump, activePath, destinations){
  let selectedPath;
  const draftPath=asText(jump.value);
  if(exists((_1) => sameTextInvariant(_1[0], draftPath), destinations))selectedPath=draftPath;
  else {
    const o=tryFind((_1) => isCurrentPage(activePath, _1[0]), destinations);
    const o_1=o==null?null:Some(o.$0[0]);
    selectedPath=o_1==null?"/chat":o_1.$0;
  }
  clear(jump);
  iter((_1) => {
    const href=_1[0];
    const option=doc().createElement("option");
    option.setAttribute("value", href);
    option.textContent=_1[1];
    if(sameTextInvariant(selectedPath, href))option.setAttribute("selected", "selected");
    jump.appendChild(option);
  }, destinations);
  if(jump.childElementCount>0)jump.value=selectedPath;
}
function select(options){
  const node=doc().createElement("select");
  iter((_1) => {
    const option=doc().createElement("option");
    option.setAttribute("value", _1[0]);
    option.textContent=_1[1];
    node.appendChild(option);
  }, options);
  return node;
}
function setId(id, node){
  node.setAttribute("id", id);
  return node;
}
function currentLogoutPath(){
  const path=currentBrowserUser().logoutPath;
  return isBlank(path)?"/chat/logout":path;
}
function renderPageCreator(nav, activePath, pages){
  let candidatePageId, candidatesLoaded, replayingPendingPageRegistration;
  const wrap=setTestId("page-create", element("div", "page-create", null));
  const shape=setTestId("page-create-shape", select(appendPageShapeOptions()));
  const pageId=setTestId("page-create-id", input("page id"));
  const title=setTestId("page-create-title", input("title"));
  const binding=setTestId("page-create-binding", select([]));
  const add=setTestId("page-create-submit", button("", "+ Page"));
  const status=setTestId("page-create-status", element("span", "state page-create-status", ""));
  candidatePageId="";
  candidatesLoaded=false;
  const sameText=(left, right) => asText(left).toLowerCase()==asText(right).toLowerCase();
  const appendOption=(value, label, target) => {
    const option=doc().createElement("option");
    option.setAttribute("value", value);
    option.textContent=label;
    target.appendChild(option);
  };
  const resetBinding=() => {
    clear(binding);
    appendOption("", "Use page id history", binding);
    binding.value="";
    binding.setAttribute("data-candidate-count", "0");
    candidatesLoaded=false;
    candidatePageId="";
  };
  const refresh=(pages_1) => {
    renderNav(nav, activePath, arrayOrEmpty(pages_1));
  };
  const loadCandidates=(pageIdText, onDone) => {
    if(isBlank(pageIdText)){
      resetBinding();
      return onDone();
    }
    else {
      const normalizedInput=Trim(pageIdText);
      return candidatesLoaded&&candidatePageId.toLowerCase()==normalizedInput.toLowerCase()?onDone():(setStatus(status, "Checking history"),getJson("/pages/api/tab-candidates?pageId="+encodeURIComponent(normalizedInput), (reply) => {
        const candidates=arrayOrEmpty(reply.candidates);
        clear(binding);
        if(length(candidates)===0){
          appendOption("", "Use page id history", binding);
          binding.value="";
          setStatus(status, "Ready");
        }
        else {
          iter((candidate) => {
            appendOption("reuse:"+asText(candidate.tabId), "Reuse "+textOr(asText(candidate.pageId), asText(candidate.tabId))+" ("+(candidate.visible?"visible":"hidden")+")", binding);
          }, candidates);
          appendOption("new", "New history", binding);
          binding.value="reuse:"+asText(get(candidates, 0).tabId);
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
    if(isBlank(pageIdText)&&isBlank(titleText))setStatus(status, "Page id or title is required");
    else {
      const bindingValue=asText(binding.value);
      const p=StartsWith(bindingValue, "reuse:")?[bindingValue.substring("reuse:".length), "reuse"]:bindingValue=="new"?["", "new"]:["", ""];
      const request=New_38(pageIdText, titleText, "", shape.value, p[0], p[1], "", "");
      const pendingId=rememberPending("append-page-register", textOr(titleText, pageIdText), "/pages/api/register-page", request);
      setStatus(status, "Saving");
      postJson("/pages/api/register-page", request, (reply) => {
        deletePendingThen(pendingId, () => {
          writeAppendPagesDefinitions(New_2(reply.status, length(arrayOrEmpty(reply.pages)), reply.maxSequence, reply.pages));
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
    return isBlank(pageIdText)?resetBinding():loadCandidates(pageIdText, () => { });
  });
  title.addEventListener("keydown", (event) => event.key=="Enter"?addPage():null);
  add.addEventListener("click", addPage);
  resetBinding();
  refresh(pages);
  append(wrap, [shape, pageId, title, binding, add, status]);
  setHidden(!systemAclAllows("*", "ptcs.page.create"), wrap);
  if(!replayingPendingPageRegistration){
    replayingPendingPageRegistration=true;
    readAllPending((commands) => {
      let remaining, accepted;
      const mine=filter_1((command) =>!(command==null)&&sameText(command.kind, "append-page-register")&&sameText(command.method, "POST")&&!isBlank(command.url)&&!isBlank(command.payloadJson), commands);
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
              const reply=json(isBlank(body)?"{}":body);
              deletePendingThen(command.commandId, () => {
                accepted=accepted+1;
                !(reply==null)?(writeAppendPagesDefinitions(New_2(reply.status, length(arrayOrEmpty(reply.pages)), reply.maxSequence, reply.pages)),refresh(reply.pages)):void 0;
                finishOne();
              });
            }
            catch(error){
              setStatus(status, "Replay create page parse failed: "+errorMessage(error));
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
  return fold((_1, _2) => _1+_2, 0, map((bucket) => bucket==null?0:bucket.valueCount, arrayOrEmpty(buckets)));
}
function tryRenderWithRegisteredPageRenderers(text_1){
  let r;
  const content=asText(text_1);
  if(isBlank(content))return null;
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
  const wrap=doc().createDocumentFragment();
  const row=element("div", "name-row", null);
  append(row, [statusDot(status), element("span", "strong wrap", title)]);
  wrap.appendChild(row);
  if(!isBlank(id))wrap.appendChild(element("div", "muted wrap", id));
  if(!isBlank(line))wrap.appendChild(element("div", "meta wrap", line));
  return wrap;
}
function statusDot(status){
  const node=element("span", isLive(status)?"status-dot online":"status-dot offline", null);
  node.setAttribute("title", asText(status));
  return node;
}
function compactMessageId(value){
  const text_1=asText(value);
  return text_1.length<=32?text_1:StartsWith(text_1.toLowerCase(), "pending-command")?"pending-command:"+String(text_1.length):Substring(text_1, 0, 24)+"..."+text_1.substring(text_1.length-6);
}
function distinctMessages(messages){
  let kept;
  kept=[];
  iter((message) => {
    if(!(message==null)&&!isBlank(message.messageId)&&!exists((row) => row.messageId==message.messageId, kept))kept=kept.concat([message]);
  }, arrayOrEmpty(messages));
  return kept;
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
function isNearBottom(node){
  if(node==null)return false;
  else try {
    return node.scrollHeight-node.scrollTop-node.clientHeight<=8;
  }
  catch(m){
    return false;
  }
}
function mergeThreadMessages(existing, incoming){
  const v=distinctMessages(arrayOrEmpty(existing).concat(arrayOrEmpty(incoming)));
  return latestArray(defaultRenderLimit(), v);
}
function maxMessageSequence(messages){
  return fold((_1, _2) => Compare(_1, _2)===1?_1:_2, 0n, map((message) => message==null?0n:tryParseSequence("msg-", message.messageId), arrayOrEmpty(messages)));
}
function int64OrZero(value){
  const parsed=parseInt(asText(value), globalThis.$radix);
  return isNaN(parsed)||parsed<0?0n:BigInt(parsed);
}
function initializeClientExtensionGlobals(){
  if(!globalThis.PulseTrade)globalThis.PulseTrade={};
  if(!globalThis.PulseTrade.MessageRenderers)globalThis.PulseTrade.MessageRenderers=[];
  if(!globalThis.PulseTrade.PageRenderers)globalThis.PulseTrade.PageRenderers=[];
  if(!globalThis.PulseTrade.AppendInputRenderers)globalThis.PulseTrade.AppendInputRenderers=[];
  if(!globalThis.PulseTrade.AddKeyRenderers)globalThis.PulseTrade.AddKeyRenderers=[];
  if(!globalThis.PulseTrade.LoginRenderers)globalThis.PulseTrade.LoginRenderers=[];
  if(!globalThis.PulseTrade.AclSnapshotObservers)globalThis.PulseTrade.AclSnapshotObservers=[];
  if(!globalThis.PulseTrade.AclCapabilityProviders)globalThis.PulseTrade.AclCapabilityProviders=[];
  if(!globalThis.PulseTrade.ReplyPresentationResolvers)globalThis.PulseTrade.ReplyPresentationResolvers=[];
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
  globalThis.PulseTradeRegisterLoginRenderer=(name, priority, func) => {
    register(globalThis.PulseTrade.LoginRenderers, name, priority, func);
  };
  globalThis.PulseTradeRegisterAclSnapshotObserver=(name, priority, func) => {
    register(globalThis.PulseTrade.AclSnapshotObservers, name, priority, func);
  };
  globalThis.PulseTradeRegisterAclCapabilityProvider=(name, priority, func) => {
    register(globalThis.PulseTrade.AclCapabilityProviders, name, priority, func);
  };
  globalThis.PulseTradeRegisterReplyPresentation=(name, priority, func) => {
    register(globalThis.PulseTrade.ReplyPresentationResolvers, name, priority, func);
  };
}
function routeItem(icon, name, value){
  const item=element("li", "route-item", null);
  const content=element("div", "", null);
  append(content, [element("p", "route-name", name), element("p", "route-value", value)]);
  append(item, [element("span", "route-icon", icon), content]);
  return item;
}
function field(labelText, inputId, control){
  const wrap=element("div", "field", null);
  const label=element("label", "", labelText);
  label.setAttribute("for", inputId);
  append(wrap, [label, control]);
  return wrap;
}
function aclAllows(action, resourceKind, resourceId){
  const m=tryAclCapabilityProvider(action, resourceKind, resourceId);
  return m==null?aclAllowsFallback(action, resourceKind, resourceId):m.$0;
}
function findAppendPageShape(shape){
  const normalized=normalizeShapeText(shape);
  return tryFind((candidate) => normalizeShapeText(candidate.shape)==normalized, appendPageShapeRegistry());
}
function normalizeShapeText(value){
  const text_1=Trim(asText(value)).toLowerCase();
  return text_1.length>0&&text_1.length<=64&&forall_2((ch) => ch>="a"&&ch<="z"||ch>="0"&&ch<="9"||ch==="-"||ch==="_"||ch===".", text_1)?text_1:"raw";
}
function hasTag(tag, tags){
  return exists((value) => asText(value).toLowerCase()==tag, arrayOrEmpty(tags));
}
function currentBrowserUser(){
  const userNode=doc().getElementById("ptc-comm-user");
  if(userNode==null||isBlank(userNode.textContent))return New_36("user.web", "Web User", "", false, "anonymous", "/chat/logout");
  else {
    const user=json(userNode.textContent);
    return user==null||isBlank(user.participantId)?New_36("user.web", "Web User", "", false, "anonymous", "/chat/logout"):user;
  }
}
function replyPresentationDisposers(){
  return _c_1.replyPresentationDisposers;
}
function set_replyPresentationDisposers(_1){
  _c_1.replyPresentationDisposers=_1;
}
function newPendingCommandId(kind, target, url, payloadJson){
  set_pendingCommandSeq(pendingCommandSeq()+1);
  return cacheKey("pending-command", ofArray([kind, target, url, payloadJson, "attempt-"+String(pendingCommandSeq()), "rand-"+String(Math.floor(Math.random()*1000000000))]));
}
function fcellValueModeLabel(mode, tags){
  return hasTag("actor-argu-command", tags)?"Actor Argu Outbound":hasTag("actor-argu-reply", tags)?"Actor Argu Reply":hasTag("actor-argu-error", tags)?"Actor Argu Error":fcellModeLabel(mode);
}
function renderTextBlock(className, text_1){
  const m=tryRenderWithRegisteredRenderers(text_1);
  return m==null?element("pre", className, asText(text_1)):m.$0;
}
function tryResolveReplyPresentation(context){
  const o=((context_1) => {
    let found=null;
    if(globalThis.PulseTrade&&globalThis.PulseTrade.ReplyPresentationResolvers){
      const resolvers=globalThis.PulseTrade.ReplyPresentationResolvers;
      for(let i=0;i<resolvers.length&&found==null;i++){
        const resolver=resolvers[i];
        try {
          const value=(resolver.render||resolver[1])(context_1);
          if(value!=null)found=value;
        }
        catch(e){
          console.error("Reply presentation resolver exception:", e);
        }
      }
    }
    return found;
  })(context);
  return o==null?tryPick((_1) => {
    try {
      return _1[1](context);
    }
    catch(m){
      return null;
    }
  }, registeredReplyPresentationResolvers()):(o.$0,o);
}
function replyPresentationIdentity(context){
  return concat_1("\u001f", [context.PageId, context.TabId, context.ValueId]);
}
function registerReplyPresentationDisposer(identity, dispose){
  disposeReplyPresentation(identity);
  set_replyPresentationDisposers(replyPresentationDisposers().concat([[identity, dispose]]));
}
function setReplyPresentationMode(identity, mode){
  set_replyPresentationModes(filter_1((_1) => _1[0]!=identity, replyPresentationModes()).concat([[identity, mode]]));
}
function replyPresentationMode(identity){
  const o=tryPick((_1) => _1[0]==identity?Some(_1[1]):null, replyPresentationModes());
  return o==null?"collapsed":o.$0;
}
function appendPageShapeOptions(){
  return map((shape) =>[normalizeShapeText(shape.shape), textOr(normalizeShapeText(shape.shape), shape.label)], appendPageShapeRegistry());
}
function systemAclAllows(resourceId, action){
  return aclAllows(action, "ptcs.system", resourceId);
}
function navigationPathForCreatedPage(page){
  const pageId=asText(page.pageId);
  const path=asText(page.path);
  return exists((alias) => sameTextInvariant(path, alias), ["/fcell-chat", "/fcell-list", "/fcell-grid"])?path:"/page/"+pageId;
}
function isLive(status){
  const m=asText(status).toLowerCase();
  return m=="online"||(m=="running"||(m=="up"||m=="available"));
}
function tryParseSequence(prefix, value){
  const text_1=asText(value);
  if(isBlank(text_1)||!StartsWith(text_1, prefix))return 0n;
  else try {
    return BigInt(text_1.substring(prefix.length));
  }
  catch(m){
    return 0n;
  }
}
function tryAclCapabilityProvider(action, resourceKind, resourceId){
  const normalized=Trim(asText(((action_1, resourceKind_1, resourceId_1, snapshotJson) => {
    if(!(globalThis.PulseTrade&&globalThis.PulseTrade.AclCapabilityProviders))return"unknown";
    const providers=globalThis.PulseTrade.AclCapabilityProviders;
    for(let i=0;i<providers.length;i++){
      const provider=providers[i];
      try {
        const value=(provider.render||provider[1])(action_1, resourceKind_1, resourceId_1, snapshotJson||"");
        if(value===true)return"allow";
        if(value===false)return"deny";
        const text_1=String(value||"").toLowerCase();
        if(text_1==="allow"||text_1==="allowed"||text_1==="true")return"allow";
        if(text_1==="deny"||text_1==="denied"||text_1==="false")return"deny";
      }
      catch(e){
        console.error("ACL capability provider exception:", e);
      }
    }
    return"unknown";
  })(action, resourceKind, resourceId, currentAclSnapshotJson()))).toLowerCase();
  return normalized=="allow"?Some(true):normalized=="deny"?Some(false):null;
}
function aclAllowsFallback(action, resourceKind, resourceId){
  const _1=currentAclSnapshot();
  if(_1!=null&&_1.$==1){
    if(!currentAclSnapshot().$0.enabled){
      currentAclSnapshot().$0;
      return true;
    }
    else {
      const snapshot=currentAclSnapshot().$0;
      const o=tryFind((resource) => aclSameText(resource.resourceKind, resourceKind)&&aclSameText(resource.resourceId, resourceId), arrayOrEmpty(snapshot.resources));
      const o_1=o==null?null:aclCapabilityAllowed(action, o.$0.capabilities);
      const o_2=o_1==null?aclCapabilityAllowed(action, snapshot.globalCapabilities):(o_1.$0,o_1);
      return o_2==null?false:o_2.$0;
    }
  }
  else return true;
}
function appendPageShapeRegistry(){
  return distinctBy((shape) => normalizeShapeText(shape.shape), concat([builtInAppendPageShapes(), manifestAppendPageShapes(), runtimeAppendPageShapes()]));
}
function set_pendingCommandSeq(_1){
  _c_1.pendingCommandSeq=_1;
}
function pendingCommandSeq(){
  return _c_1.pendingCommandSeq;
}
function fcellModeLabel(mode){
  const m=asText(mode).toLowerCase();
  return m=="inbound-message"?"FCell Chat":m=="outbound-message"?"FCell Chat":m=="list"?"FCell List":m=="table"?"FCell Grid":m=="grid"?"FCell Grid":"FCell Value";
}
function tryRenderWithRegisteredRenderers(text_1){
  let r;
  const content=asText(text_1);
  if(isBlank(content))return null;
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
function registeredReplyPresentationResolvers(){
  return _c_1.registeredReplyPresentationResolvers;
}
function set_replyPresentationModes(_1){
  _c_1.replyPresentationModes=_1;
}
function replyPresentationModes(){
  return _c_1.replyPresentationModes;
}
function currentAclSnapshot(){
  return _c_1.currentAclSnapshot;
}
function aclCapabilityAllowed(action, capabilities){
  const o=tryFind((item) => aclSameText(item.action, action), arrayOrEmpty(capabilities));
  return o==null?null:Some(o.$0.allowed);
}
function aclSameText(left, right){
  return asText(left).toLowerCase()==asText(right).toLowerCase();
}
function builtInAppendPageShapes(){
  return[shapeRegistration("fcell-chat", "FCell Chat", "C", "fcell-chat"), shapeRegistration("fcell-list", "FCell List", "L", "fcell-list"), shapeRegistration("fcell-grid", "FCell Grid", "G", "fcell-grid"), shapeRegistration("actor-argu", "Actor Argu", "aa", "actor-argu"), shapeRegistration("raw", "Raw", "R", "raw")];
}
function manifestAppendPageShapes(){
  return filter_1((shape) => shape.shape!="raw", map((shape) => shape==null?shapeRegistration("raw", "Raw", "R", "raw"):shapeRegistration(shape.shape, shape.label, shape.badge, shape.className), collect((extension) => extension==null?[]:arrayOrEmpty(extension.appendPageShapes), serverClientExtensions())));
}
function runtimeAppendPageShapes(){
  return _c_1.runtimeAppendPageShapes;
}
function registeredRenderers(){
  return _c_1.registeredRenderers;
}
function shapeRegistration(shape, label, badge, className){
  return New_33(normalizeShapeText(shape), textOr(normalizeShapeText(shape), label), textOr("?", badge), textOr(normalizeShapeText(shape), className));
}
function serverClientExtensions(){
  const node=doc().getElementById("ptc-comm-client-extensions");
  if(node==null||isBlank(node.textContent))return[];
  else {
    const o=tryJson(node.textContent);
    return o==null?[]:o.$0;
  }
}
function FailWith(msg){
  throw new Error(msg);
}
function KeyValue(kvp){
  return[kvp.K, kvp.V];
}
function range(min_1, max_2){
  const count=1+max_2-min_1;
  return count<=0?[]:init_1(count, (x) => x+min_1);
}
function toInt(x){
  const u=toUInt(x);
  return u>2147483647?u-4294967296:u;
}
function toUInt(x){
  return(x<0?Math.ceil(x):Math.floor(x))>>>0;
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
function GetFieldValues(o){
  let r=[];
  let k;
  for(var k_1 in o)r.push(o[k_1]);
  return r;
}
function mountByIdWithOptions(rootId, extensionId, channelId, canvasId, lifecycleOptions){
  return mountWithOptions((_1) => {
    LoadLocalTemplates("");
    Doc.RunById(rootId, _1);
  }, extensionId, channelId, canvasId, lifecycleOptions);
}
function mountWithOptions(mountDocument, extensionId, channelId, canvasId, lifecycleOptions){
  return mountCore(mountDocument, extensionId, channelId, canvasId, lifecycleOptions, false);
}
function mountCore(mountDocument, extensionId, channelId, canvasId, lifecycleOptions, disposeAfterJsonExport){
  let socket, requestSequence, lifecycle, pollTimer, timeoutTimer, reconnectTimer, jsonExportRequested, jsonExportBootstrapAttempts, jsonExportBootstrapInFlight, jsonExportInFlight, actionRequestOverride, pendingActionCompletion;
  const identity={DocumentId:{$:0, $0:"pending-"+channelId}, CanvasInstanceId:{$:0, $0:canvasId}};
  const runtimeState=_c_3.Create_1({
    Identity:identity,
    Document:null,
    Data:new FSharpMap("New", []),
    DocumentRevision:0n,
    DataRevision:0n,
    LastTransportSequence:0n,
    View:{Values:new FSharpMap("New", [])},
    Poll:{$:0},
    LastError:null
  });
  socket=null;
  requestSequence=0;
  lifecycle=initial(identity.CanvasInstanceId);
  pollTimer=null;
  timeoutTimer=null;
  reconnectTimer=null;
  jsonExportRequested=false;
  jsonExportBootstrapAttempts=0;
  jsonExportBootstrapInFlight=false;
  jsonExportInFlight=false;
  actionRequestOverride=null;
  pendingActionCompletion=null;
  const nextRequestId=() => {
    requestSequence=requestSequence+1;
    return channelId+":"+String(requestSequence);
  };
  const sendPayloadWithRequestId=(requestId, operation, payload) => {
    const text_1=JSON.stringify(New_39("extension-transient", requestId, extensionId, channelId, operation, JSON.stringify(payload)));
    return socket!=null&&socket.$==1&&(Equals(socket.$0.readyState, 1)&&(socket.$0.send(text_1),true));
  };
  const sendPayload=(operation, payload) => sendPayloadWithRequestId(nextRequestId(), operation, payload);
  const completePendingAction=(requestId, result) => {
    if(pendingActionCompletion!=null&&pendingActionCompletion.$==1){
      const pendingRequestId=pendingActionCompletion.$0[0];
      if(pendingActionCompletion.$0,pendingRequestId==requestId){
        const continuation=pendingActionCompletion.$0[1];
        pendingActionCompletion.$0;
        pendingActionCompletion=null;
        return continuation(Ok(result));
      }
      else return null;
    }
    else return null;
  };
  const failPendingAction=(code, message) => {
    if(pendingActionCompletion==null)return null;
    else {
      const continuation=pendingActionCompletion.$0[1];
      pendingActionCompletion=null;
      return continuation(Error_1({Code:code, Message:message}));
    }
  };
  const cancelPollTimer=() => {
    pollTimer==null?void 0:clearTimeout(pollTimer.$0);
    pollTimer=null;
  };
  const cancelTimeoutTimer=() => {
    timeoutTimer==null?void 0:clearTimeout(timeoutTimer.$0);
    timeoutTimer=null;
  };
  const cancelReconnectTimer=() => {
    reconnectTimer==null?void 0:clearTimeout(reconnectTimer.$0);
    reconnectTimer=null;
  };
  const closeSocket=() => {
    let _1;
    cancelTimeoutTimer();
    if(socket==null)_1=void 0;
    else {
      const value=socket.$0;
      _1=Equals(value.readyState, 1)||Equals(value.readyState, 0)?value.close():void 0;
    }
    socket=null;
  };
  function apply(event){
    if(event.$==2||(event.$==5||(event.$==6||event.$==9))){
      jsonExportRequested=false;
      jsonExportBootstrapAttempts=0;
      jsonExportBootstrapInFlight=false;
      jsonExportInFlight=false;
      event.$==5?failPendingAction("transient-command-timeout", "The TA action response timed out."):event.$==6?failPendingAction("transient-channel-disconnected", "The TA transient channel disconnected before the action completed."):event.$==9?failPendingAction("transient-channel-disposed", "The TA transient channel was disposed before the action completed."):null;
    }
    else null;
    const p=transition(lifecycleOptions, event, lifecycle);
    const next=p[0];
    const effects=p[1];
    lifecycle=next;
    const _1=runtimeState.Get();
    let _2={
      Identity:_1.Identity,
      Document:_1.Document,
      Data:_1.Data,
      DocumentRevision:_1.DocumentRevision,
      DataRevision:_1.DataRevision,
      LastTransportSequence:_1.LastTransportSequence,
      View:_1.View,
      Poll:next.Poll,
      LastError:_1.LastError
    };
    runtimeState.Set(_2);
    interpret(effects);
    return effects;
  }
  function interpret(effects){
    for(let i=0, _1=effects.length-1;i<=_1;i++){
      let p;
      const effect=get(effects, i);
      if(effect.$==1)sendPayload("close", emptyFrame("unmounted", "", canvasId));
      else if(effect.$==2){
        const action=effect.$0;
        if(actionRequestOverride==null)p=[nextRequestId(), actionToWire(action)];
        else {
          const request=actionRequestOverride.$0;
          p=(actionRequestOverride=null,[request.RequestId, actionRequestToWire(request)]);
        }
        if(!sendPayloadWithRequestId(p[0], "action", p[1]))apply(Disconnected);
      }
      else if(effect.$==3){
        const delayMs=effect.$0;
        cancelPollTimer();
        pollTimer=Some(setTimeout(() => {
          pollTimer=null;
          apply(PollDue({d:Date.now(), o:0}));
        }, delayMs));
      }
      else if(effect.$==4){
        const delayMs_1=effect.$0;
        cancelTimeoutTimer();
        timeoutTimer=Some(setTimeout(() => {
          timeoutTimer=null;
          apply(RequestTimedOut({d:Date.now(), o:0}));
        }, delayMs_1));
      }
      else if(effect.$==5){
        const delayMs_2=effect.$0;
        cancelReconnectTimer();
        reconnectTimer=Some(setTimeout(() => {
          reconnectTimer=null;
          connect();
        }, delayMs_2));
      }
      else if(effect.$==6)cancelPollTimer();
      else if(effect.$==7)cancelTimeoutTimer();
      else if(effect.$==8)cancelReconnectTimer();
      else if(effect.$==9)closeSocket();
      else if(!sendPayload("open", emptyFrame("mounted", "", canvasId)))apply(Disconnected);
    }
  }
  function connect(){
    if(!lifecycle.Disposed){
      const value=new WebSocket(syncWebSocketUrl_1());
      socket=Some(value);
      value.onopen=() => {
        apply(Connected);
      };
      value.onmessage=(event) => {
        try {
          let jsonExportCompleted, _1, o, m, _2, _3;
          const response=JSON.parse(String(event.data));
          if(response.type=="extension-transient"&&response.operation=="close")return closeSocket();
          else if(response.type=="extension-transient"&&response.status=="ok"){
            const wire=JSON.parse(response.payload);
            const m_1=applyWire(runtimeState.Get(), wire);
            if(m_1.$==1){
              apply(ResyncRequired("invalid-browser-state"));
              return;
            }
            else {
              const state=m_1.$0;
              runtimeState.Set(state);
              const completesJsonExport=jsonExportInFlight;
              jsonExportCompleted=false;
              if(jsonExportBootstrapInFlight)jsonExportBootstrapInFlight=false;
              else null;
              if(completesJsonExport){
                jsonExportRequested=false;
                jsonExportInFlight=false;
                if(wire.updateKind=="full"){
                  const m_2=downloadJsonExport(wire);
                  if(m_2.$==1){
                    const message=m_2.$0;
                    const _4=runtimeState.Get();
                    let _5={
                      Identity:_4.Identity,
                      Document:_4.Document,
                      Data:_4.Data,
                      DocumentRevision:_4.DocumentRevision,
                      DataRevision:_4.DataRevision,
                      LastTransportSequence:_4.LastTransportSequence,
                      View:_4.View,
                      Poll:_4.Poll,
                      LastError:Some({
                        ReasonCode:"ta-export-download-failed",
                        Message:message,
                        Recoverable:true
                      })
                    };
                    _1=runtimeState.Set(_5);
                  }
                  else _1=void(jsonExportCompleted=true);
                }
                else {
                  const _6=runtimeState.Get();
                  let _7={
                    Identity:_6.Identity,
                    Document:_6.Document,
                    Data:_6.Data,
                    DocumentRevision:_6.DocumentRevision,
                    DataRevision:_6.DataRevision,
                    LastTransportSequence:_6.LastTransportSequence,
                    View:_6.View,
                    Poll:_6.Poll,
                    LastError:Some({
                      ReasonCode:"ta-export-full-state-required",
                      Message:"The TA export response was not a full runtime state.",
                      Recoverable:true
                    })
                  };
                  _1=runtimeState.Set(_7);
                }
              }
              else _1=null;
              const o_1=state.Document;
              let _8=o_1==null?false:exists((action) => action=="poll-delta", o_1.$0.AllowedActions);
              let _9=StateAccepted(state.DataRevision, _8);
              apply(_9);
              completePendingAction(response.requestId, {
                $:0,
                $0:response.requestId,
                $1:state.DocumentRevision
              });
              return jsonExportCompleted&&disposeAfterJsonExport?void apply(Dispose):tryStartJsonExport();
            }
          }
          else if(response.type=="extension-transient"){
            const responseError=text(response.error);
            const conflictPrefix="ta-revision-conflict:";
            if(StartsWith(responseError, conflictPrefix)){
              o=0;
              const _10=Number(responseError.substring(conflictPrefix.length));
              let _11=isNaN(_10)?false:(o=_10,true);
              m=[_11, o];
              if(m[0]){
                const revision=m[1];
                _2=revision>=0&&(revision<0?Math.ceil(revision):Math.floor(revision))===revision;
              }
              else _2=false;
              _3=_2?{
                $:2,
                $0:response.requestId,
                $1:BigInt(Math.trunc(m[1]))
              }:{
                $:1,
                $0:response.requestId,
                $1:"transient-command-failed",
                $2:responseError
              };
            }
            else _3={
              $:1,
              $0:response.requestId,
              $1:"transient-command-failed",
              $2:responseError
            };
            completePendingAction(response.requestId, _3);
            const _12=runtimeState.Get();
            let _13={
              Identity:_12.Identity,
              Document:_12.Document,
              Data:_12.Data,
              DocumentRevision:_12.DocumentRevision,
              DataRevision:_12.DataRevision,
              LastTransportSequence:_12.LastTransportSequence,
              View:_12.View,
              Poll:_12.Poll,
              LastError:Some({
                ReasonCode:"transient-command-failed",
                Message:text(response.error),
                Recoverable:true
              })
            };
            runtimeState.Set(_13);
            apply(CommandRejected);
            return;
          }
          else return null;
        }
        catch(m_3){
          failPendingAction("invalid-transient-response", "The TA transient response could not be decoded.");
          apply(ResyncRequired("invalid-transient-response"));
          return;
        }
      };
      value.onclose=() => {
        socket=null;
        apply(Disconnected);
      };
      value.onerror=() => null;
    }
  }
  function tryStartJsonExport(){
    if(jsonExportRequested&&!jsonExportBootstrapInFlight&&!jsonExportInFlight&&lifecycle.Connected&&lifecycle.Active&&!lifecycle.InFlight){
      const hasRuntimeData=runtimeState.Get().DataRevision>0n;
      if(!hasRuntimeData&&jsonExportBootstrapAttempts>=3){
        jsonExportRequested=false;
        jsonExportBootstrapInFlight=false;
        const _1=runtimeState.Get();
        let _2={
          Identity:_1.Identity,
          Document:_1.Document,
          Data:_1.Data,
          DocumentRevision:_1.DocumentRevision,
          DataRevision:_1.DataRevision,
          LastTransportSequence:_1.LastTransportSequence,
          View:_1.View,
          Poll:_1.Poll,
          LastError:Some({
            ReasonCode:"ta-export-bootstrap-empty",
            Message:"TA export bootstrap returned no runtime data after three attempts.",
            Recoverable:true
          })
        };
        runtimeState.Set(_2);
        if(disposeAfterJsonExport)apply(Dispose);
      }
      else if(exists((a) => a.$==2, apply(StartAction(hasRuntimeData?{
        $:7,
        $0:identity.CanvasInstanceId,
        $1:"json-export"
      }:{
        $:6,
        $0:identity.CanvasInstanceId,
        $1:runtimeState.Get().DataRevision
      }))))if(hasRuntimeData){
        jsonExportRequested=false;
        jsonExportInFlight=true;
      }
      else {
        jsonExportBootstrapAttempts=jsonExportBootstrapAttempts+1;
        jsonExportBootstrapInFlight=true;
      }
    }
  }
  mountDocument(render(defaultOptions(), {SubmitAction:(request) => FromContinuations((continuation) => pendingActionCompletion!=null?continuation(Error_1({Code:"transient-command-busy", Message:"A TA transient command is already in flight."})):(pendingActionCompletion=Some([request.RequestId, continuation]),actionRequestOverride=Some(request),!exists((a) => a.$==2, apply(StartAction(request.Action)))?(actionRequestOverride=null,pendingActionCompletion=null,continuation(Error_1({Code:lifecycle.Connected?"transient-command-busy":"transient-channel-not-open", Message:lifecycle.Connected?"A TA transient command is already in flight.":"TA transient channel is not open."}))):null))}, runtimeState));
  connect();
  return New_1(runtimeState, (active) => {
    apply(ActiveChanged(active));
  }, () => jsonExportRequested||jsonExportBootstrapInFlight||jsonExportInFlight?Error_1("A TA Research JSON export is already pending."):lifecycle.Disposed||lifecycle.DisposePending?Error_1("The TA transient channel has already been disposed."):(jsonExportBootstrapAttempts=0,jsonExportRequested=true,tryStartJsonExport(),Ok("TA Research JSON export requested.")), () => {
    apply(Dispose);
  });
}
function syncWebSocketUrl_1(){
  return(globalThis.location.protocol=="https:"?"wss://":"ws://")+globalThis.location.host+"/sync/ws";
}
function downloadJsonExport(wire){
  if(globalThis.document.body==null)return Error_1("Document body is unavailable.");
  else try {
    const url=URL.createObjectURL(new Blob([JSON.stringify(New_43("ptcs-ta-research-export.v1", (new Date()).toISOString(), wire.documentRevision, wire.dataRevision, wire))], {type:"application/json;charset=utf-8"}));
    const anchor=globalThis.document.createElement("a");
    anchor.setAttribute("href", url);
    anchor.setAttribute("download", exportFileName());
    anchor.setAttribute("aria-hidden", "true");
    anchor.setAttribute("style", "display:none;");
    globalThis.document.body.appendChild(anchor);
    anchor.click();
    globalThis.document.body.removeChild(anchor);
    setTimeout(() => {
      URL.revokeObjectURL(url);
    }, 250);
    return Ok("TA Research JSON download started.");
  }
  catch(error){
    return Error_1(error.message);
  }
}
function exportFileName(){
  const now=new Date();
  const random=new Random();
  const hex="0123456789abcdef";
  const compactGuid=concat_1("", map((v) => v, init(32, () => hex[random.Next_1(hex.length)])));
  const guid=Substring(compactGuid, 0, 8)+"-"+Substring(compactGuid, 8, 4)+"-4"+Substring(compactGuid, 13, 3)+"-"+"8"+Substring(compactGuid, 17, 3)+"-"+Substring(compactGuid, 20, 12);
  return String(now.getFullYear())+twoDigits(now.getMonth()+1)+twoDigits(now.getDate())+twoDigits(now.getHours())+twoDigits(now.getMinutes())+twoDigits(now.getSeconds())+"-"+guid+".json";
}
function twoDigits(value){
  return value<10?"0"+String(value):String(value);
}
function Some(Value){
  return{$:1, $0:Value};
}
function defaults(){
  return _c.defaults;
}
function initial(canvasInstanceId){
  return New_40(canvasInstanceId, {$:0}, false, false, true, false, 0n, 0, false, false);
}
function transition(options, event, state){
  let _1;
  if(state.Disposed&&event.$!==9)return[state, []];
  else if(state.DisposePending)switch(event.$==1?0:event.$==5?1:event.$==6?1:event.$==9?2:3){
    case 0:
      return[New_40(state.CanvasInstanceId, {$:7}, event.$1, state.Connected, state.Active, true, event.$0, state.ReconnectAttempt, state.DisposePending, state.Disposed), [CancelPoll, CancelTimeout, CancelReconnect, SendUnmounted, ScheduleTimeout(options.RequestTimeoutMs)]];
    case 1:
      return[New_40(state.CanvasInstanceId, {$:7}, state.PollEnabled, false, state.Active, false, state.DataRevision, state.ReconnectAttempt, false, true), [CancelPoll, CancelTimeout, CancelReconnect, CloseTransport]];
    case 2:
      return[state, []];
    case 3:
      return[state, []];
  }
  else switch(event.$==1?(_1=[event.$1, event.$0],1):event.$==2?state.Connected&&state.InFlight?2:11:event.$==3?(event.$0,state.Connected&&state.Active&&!state.InFlight?(_1=event.$0,3):11):event.$==4?state.Connected&&state.Active&&state.PollEnabled&&!state.InFlight?4:11:event.$==5?state.InFlight?5:11:event.$==6?!state.Connected?6:7:event.$==7?(_1=event.$0,8):event.$==8?(event.$0,state.Connected&&state.Active?(_1=event.$0,9):11):event.$==9?10:0){
    case 0:
      return[New_40(state.CanvasInstanceId, {$:1}, state.PollEnabled, true, state.Active, true, state.DataRevision, 0, state.DisposePending, state.Disposed), [CancelReconnect, SendMounted, ScheduleTimeout(options.RequestTimeoutMs)]];
    case 1:
      const pollEnabled=_1[0];
      return[New_40(state.CanvasInstanceId, state.Active&&pollEnabled?{$:2}:{$:5}, pollEnabled, state.Connected, state.Active, false, _1[1], state.ReconnectAttempt, state.DisposePending, state.Disposed), [CancelTimeout].concat(state.Active&&pollEnabled?[SchedulePoll(options.PollIntervalMs)]:[])];
    case 2:
      return[New_40(state.CanvasInstanceId, state.Active&&state.PollEnabled?{$:2}:{$:5}, state.PollEnabled, state.Connected, state.Active, false, state.DataRevision, state.ReconnectAttempt, state.DisposePending, state.Disposed), [CancelTimeout].concat(state.Active&&state.PollEnabled?[SchedulePoll(options.PollIntervalMs)]:[])];
    case 3:
      return[New_40(state.CanvasInstanceId, {$:3}, state.PollEnabled, state.Connected, state.Active, true, state.DataRevision, state.ReconnectAttempt, state.DisposePending, state.Disposed), [CancelPoll, SendAction(_1), ScheduleTimeout(options.RequestTimeoutMs)]];
    case 4:
      return[New_40(state.CanvasInstanceId, {$:3}, state.PollEnabled, state.Connected, state.Active, true, state.DataRevision, state.ReconnectAttempt, state.DisposePending, state.Disposed), [SendAction({
        $:6,
        $0:state.CanvasInstanceId,
        $1:state.DataRevision
      }), ScheduleTimeout(options.RequestTimeoutMs)]];
    case 5:
      const attempt=state.ReconnectAttempt+1;
      return[New_40(state.CanvasInstanceId, {$:5}, state.PollEnabled, false, state.Active, false, state.DataRevision, attempt, state.DisposePending, state.Disposed), [CancelPoll, CancelTimeout, CancelReconnect, CloseTransport, ScheduleReconnect(reconnectDelay(options, attempt))]];
    case 6:
      return[state, []];
    case 7:
      const attempt_1=state.ReconnectAttempt+1;
      return[New_40(state.CanvasInstanceId, {$:5}, state.PollEnabled, false, state.Active, false, state.DataRevision, attempt_1, state.DisposePending, state.Disposed), [CancelPoll, CancelTimeout, ScheduleReconnect(reconnectDelay(options, attempt_1))]];
    case 8:
      return _1&&state.Connected&&state.PollEnabled&&!state.InFlight?[New_40(state.CanvasInstanceId, {$:2}, state.PollEnabled, state.Connected, true, state.InFlight, state.DataRevision, state.ReconnectAttempt, state.DisposePending, state.Disposed), [SchedulePoll(options.PollIntervalMs)]]:_1?[New_40(state.CanvasInstanceId, state.Poll, state.PollEnabled, state.Connected, true, state.InFlight, state.DataRevision, state.ReconnectAttempt, state.DisposePending, state.Disposed), []]:[New_40(state.CanvasInstanceId, {$:5}, state.PollEnabled, state.Connected, false, state.InFlight, state.DataRevision, state.ReconnectAttempt, state.DisposePending, state.Disposed), [CancelPoll]];
    case 9:
      return[New_40(state.CanvasInstanceId, {$:6}, state.PollEnabled, state.Connected, state.Active, true, state.DataRevision, state.ReconnectAttempt, state.DisposePending, state.Disposed), [CancelPoll, CancelTimeout, SendAction({
        $:7,
        $0:state.CanvasInstanceId,
        $1:_1
      }), ScheduleTimeout(options.RequestTimeoutMs)]];
    case 10:
      return state.Connected?[New_40(state.CanvasInstanceId, {$:7}, state.PollEnabled, state.Connected, false, true, state.DataRevision, state.ReconnectAttempt, true, state.Disposed), ofSeq(delay(() => append_1([CancelPoll], delay(() => append_1([CancelReconnect], delay(() =>!state.InFlight?append_1([SendUnmounted], delay(() =>[ScheduleTimeout(options.RequestTimeoutMs)])):[]))))))]:[New_40(state.CanvasInstanceId, {$:7}, state.PollEnabled, false, false, false, state.DataRevision, state.ReconnectAttempt, state.DisposePending, true), [CancelPoll, CancelTimeout, CancelReconnect, CloseTransport]];
    case 11:
      return[state, []];
  }
}
function reconnectDelay(options, attempt){
  function expand(current, remaining){
    while(true)
      {
        if(remaining<=1)return current;
        else {
          const a_1=options.ReconnectMaximumMs;
          const b=current*2;
          current=Compare(a_1, b)===-1?a_1:b;
          remaining=remaining-1;
        }
      }
  }
  const a=1;
  let _1=Compare(a, attempt)===1?a:attempt;
  return expand(options.ReconnectBaseMs, _1);
}
function New(PollIntervalMs, RequestTimeoutMs, PollRetryMs, ReconnectBaseMs, ReconnectMaximumMs){
  return{
    PollIntervalMs:PollIntervalMs,
    RequestTimeoutMs:RequestTimeoutMs,
    PollRetryMs:PollRetryMs,
    ReconnectBaseMs:ReconnectBaseMs,
    ReconnectMaximumMs:ReconnectMaximumMs
  };
}
class Object_1 {
  Equals(obj){
    return this===obj;
  }
  GetHashCode(){
    return -1;
  }
}
class attr extends Object_1 { }
class Attr {
  static Create(name, value){
    return Attr.A3((el) => {
      el.setAttribute(name, value);
    });
  }
  static A3(init_2){
    return Create_1(Attr, {$:3, $0:init_2});
  }
  static Concat(xs){
    const x=ofSeqNonCopying(xs);
    return TreeReduce(EmptyAttr(), (_1, _2) => AppendTree(_1, _2), x);
  }
  static A1(Item){
    return Create_1(Attr, {$:1, $0:Item});
  }
  static A4(onAfterRender){
    return Create_1(Attr, {$:4, $0:onAfterRender});
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
function filter(f, o){
  let _1;
  return o!=null&&o.$==1&&(f(o.$0)&&(_1=o.$0,true))?o:null;
}
function New_1(RuntimeState, SetActive, RequestJsonExport, Dispose_1){
  return{
    RuntimeState:RuntimeState,
    SetActive:SetActive,
    RequestJsonExport:RequestJsonExport,
    Dispose:Dispose_1
  };
}
function New_2(status, count, maxSequence, pages){
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
function filter_1(f, arr){
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
function map(f, arr){
  const r=new Array(arr.length);
  for(let i=0, _1=arr.length-1;i<=_1;i++)r[i]=f(arr[i]);
  return r;
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
function mapi(f, arr){
  const y=new Array(arr.length);
  for(let i=0, _1=arr.length-1;i<=_1;i++)y[i]=f(i, arr[i]);
  return y;
}
function iteri(f, arr){
  for(let i=0, _1=arr.length-1;i<=_1;i++)f(i, arr[i]);
}
function skip(i, ar){
  return i<0?nonNegative():i>ar.length?insufficient():ar.slice(i);
}
function collect(f, x){
  return Array.prototype.concat.apply([], map(f, x));
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
function tryHead(arr){
  return arr.length===0?null:Some(arr[0]);
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
function distinctBy(f, a){
  return ofSeq(distinctBy_1(f, a));
}
function fold(f, zero, arr){
  let acc;
  acc=zero;
  for(let i=0, _1=arr.length-1;i<=_1;i++)acc=f(acc, arr[i]);
  return acc;
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
function distinct(l){
  return ofSeq(distinct_1(l));
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
function checkLength(arr1, arr2){
  if(arr1.length!==arr2.length)FailWith("The arrays have different lengths.");
}
function ofList(xs){
  let l;
  const q=[];
  l=xs;
  while(!(l.$==0))
    {
      q.push(head(l));
      l=tail(l);
    }
  return q;
}
function sortInPlace(arr){
  mapInPlace((t) => t[0], mapiInPlace((_1, _2) =>[_2, _1], arr).sort(Compare));
}
function foldBack(f, arr, zero){
  let acc;
  acc=zero;
  const len=arr.length;
  for(let i=1, _1=len;i<=_1;i++)acc=f(arr[len-i], acc);
  return acc;
}
function indexed(ar){
  return mapi((_1, _2) =>[_1, _2], ar);
}
function concat(xs){
  return Array.prototype.concat.apply([], ofSeq(xs));
}
function init(size, f){
  if(size<0)FailWith("Negative size given.");
  else null;
  const r=new Array(size);
  for(let i=0, _1=size-1;i<=_1;i++)r[i]=f(i);
  return r;
}
function sortDescending(arr){
  return map((t) => t[0], mapi((_1, _2) =>[_2, _1], arr).sort((_1, _2) =>-Compare(_1, _2)));
}
function tryLast(arr){
  const len=arr.length;
  return len===0?null:Some(arr[len-1]);
}
function sort(arr){
  return map((t) => t[0], mapi((_1, _2) =>[_2, _1], arr).sort(Compare));
}
function sortByDescending(f, arr){
  return map((t) => t[0], mapi((_1, _2) =>[_2, [f(_2), _1]], arr).sort((_1, _2) =>-Compare(_1[1], _2[1])));
}
function pick(f, arr){
  const m=tryPick(f, arr);
  return m==null?FailWith("KeyNotFoundException"):m.$0;
}
function min(arr){
  let m;
  nonEmpty(arr);
  m=arr[0];
  for(let i=1, _1=arr.length-1;i<=_1;i++){
    const x=arr[i];
    if(Compare(x, m)===-1)m=x;
  }
  return m;
}
function max(arr){
  let m;
  nonEmpty(arr);
  m=arr[0];
  for(let i=1, _1=arr.length-1;i<=_1;i++){
    const x=arr[i];
    if(Compare(x, m)===1)m=x;
  }
  return m;
}
function create(size, value){
  const r=new Array(size);
  for(let i=0, _1=size-1;i<=_1;i++)r[i]=value;
  return r;
}
function nonEmpty(arr){
  if(arr.length===0)FailWith("The input array was empty.");
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
  if(isBlank(key))onRead(null);
  else withStore(snapshotStore(), "readonly", (store) => {
    try {
      const request=store.get(key);
      request.onsuccess=(event) => {
        const value=eventResult(event);
        if(isMissing(value))return onRead(null);
        else try {
          const text_1=String(value);
          return isBlank(text_1)?onRead(null):onRead(tryJson(text_1));
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
  return currentServerRealityId()+":"+scope+":"+concat_1(":", map_1((part) => encodeURIComponent(asText(part)), parts));
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
    onRead(filter_1((command) =>!(command==null)&&textOr("legacy", command.serverRealityId)==reality, commands), filter_1((command) =>!(command==null)&&textOr("legacy", command.serverRealityId)!=reality, commands));
  });
}
function writeWatermark(streamId, newestSequence, cachedCount, source){
  if(!isBlank(streamId)){
    let _1=watermarkStore();
    const a=0n;
    let _2=Compare(a, newestSequence)===1?a:newestSequence;
    let _3=String(_2);
    const a_1=0;
    let _4=Compare(a_1, cachedCount)===1?a_1:cachedCount;
    let _5=New_29(streamId, _3, _4, asText(source), nowTicks());
    writeJsonTo(_1, streamId, _5);
    compactSnapshots();
  }
}
function readAllPending(onRead){
  readAllPendingRaw((commands) => {
    const reality=currentServerRealityId();
    onRead(filter_1((command) =>!(command==null)&&textOr("legacy", command.serverRealityId)==reality, commands));
  });
}
function deletePendingThen(commandId, onDeleted){
  deleteFromThen(pendingStore(), commandId, onDeleted);
}
function deleteSnapshotsByPrefix(prefix, onDeleted){
  if(isBlank(prefix))onDeleted();
  else readAllSnapshotKeys((keys) => {
    const matching=filter_1((key) =>!isBlank(key)&&StartsWith(key, prefix), keys);
    if(length(matching)===0)onDeleted();
    else withSnapshotWatermarkStores("readwrite", (_1, _2, _3) =>((((tx) =>(snapshots) =>(watermarks) => {
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
        return iter((key) => {
          snapshots["delete"](key);
          watermarks["delete"](key);
        }, matching);
      }
      catch(m){
        return finish();
      }
    })(_1))(_2))(_3), onDeleted);
  });
}
function readWatermark(key, onRead){
  if(isBlank(key))onRead(null);
  else withStore(watermarkStore(), "readonly", (store) => {
    try {
      const request=store.get(key);
      request.onsuccess=(event) => {
        const value=eventResult(event);
        if(isMissing(value))return onRead(null);
        else try {
          const text_1=String(value);
          return isBlank(text_1)?onRead(null):onRead(tryJson(text_1));
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
          return onRead(choose((text_1) => {
            try {
              return isBlank(text_1)?null:tryJson(text_1);
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
  if(!isBlank(key))withStore(storeName, "readwrite", (store) => {
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
    const watermarks_1=arrayOrEmpty(watermarks);
    const overflow=length(watermarks_1)-maxSnapshotRecords();
    if(overflow>0)iter((watermark) => {
      deleteSnapshotAndWatermark(watermark.streamId);
    }, sortBy(watermarkTouchedAt, filter_1((watermark) =>!(watermark==null)&&!isBlank(watermark.streamId)&&!protectedSnapshotKey(watermark.streamId), watermarks_1)).slice(0, overflow));
    readAllSnapshotKeys((snapshotKeys) => {
      iter((key) => {
        deleteFrom(snapshotStore(), key);
      }, filter_1((key) =>!isBlank(key)&&!protectedSnapshotKey(key)&&!exists((watermark) =>!(watermark==null)&&watermark.streamId==key, watermarks_1), snapshotKeys));
    });
  });
}
function deleteFromThen(storeName, key, onDeleted){
  if(isBlank(key))onDeleted();
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
          return onRead(choose((text_1) => {
            try {
              return isBlank(text_1)?null:tryJson(text_1);
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
  const key_1=asText(key);
  return key_1=="append-pages-definitions:"||key_1.indexOf(":append-pages-definitions:")!=-1||StartsWith(key_1, "chat-agents:")||key_1.indexOf(":chat-agents:")!=-1||StartsWith(key_1, "actors-snapshot:")||key_1.indexOf(":actors-snapshot:")!=-1;
}
function watermarkTouchedAt(watermark){
  let o;
  if(watermark==null)return 0n;
  else {
    const m=(o=0n,[TryParse_1(asText(watermark.touchedAt), {get:() => o, set:(v) => {
      o=v;
    }}), o]);
    return m[0]?m[1]:0n;
  }
}
function deleteSnapshotAndWatermark(key){
  if(!isBlank(key))withSnapshotWatermarkStores("readwrite", (_1, _2, _3) => {
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
function deleteFrom(storeName, key){
  if(!isBlank(key))withStore(storeName, "readwrite", (store) => {
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
function json(text_1){
  return JSON.parse(asText(text_1));
}
function tryJson(text_1){
  try {
    return isBlank(text_1)?null:Some(json(text_1));
  }
  catch(m){
    return null;
  }
}
function New_3(type, requestId, streamKey){
  return{
    type:type,
    requestId:requestId,
    streamKey:streamKey
  };
}
function New_4(type, requestId, streamKey, count){
  return{
    type:type,
    requestId:requestId,
    streamKey:streamKey,
    count:count
  };
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
    prepareTemplate(head_1(rawTpls.Keys));
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
class Doc extends Object_1 {
  docNode;
  updates;
  static Run(parent, doc_1){
    LinkElement(parent, doc_1.docNode);
    Doc.RunInPlace(false, parent, doc_1);
  }
  static TextNode(v){
    return Doc.Mk(TextNodeDoc(globalThis.document.createTextNode(v)), Const());
  }
  static RunInPlace(childrenOnly, parent, doc_1){
    const st=CreateRunState(parent, doc_1.docNode);
    Sink(get_UseAnimations()||BatchUpdatesEnabled()?StartProcessor(PerformAnimatedUpdate(childrenOnly, st, doc_1.docNode)):() => {
      PerformSyncUpdate(childrenOnly, st, doc_1.docNode);
    }, doc_1.updates);
  }
  static RunById(id, tr){
    const m=globalThis.document.getElementById(id);
    if(Equals(m, null))FailWith("invalid id: "+id);
    else Doc.Run(m, tr);
  }
  static Element(name, attr_1, children){
    const a=Attr.Concat(attr_1);
    const c=Doc.Concat(children);
    return Elt.New(globalThis.document.createElement(name), a, c);
  }
  static Mk(node, updates){
    return new Doc(node, updates);
  }
  static Concat(xs){
    return TreeReduce(Doc.Empty, Doc.Append, ofSeqNonCopying(xs));
  }
  static get Empty(){
    return Doc.Mk(null, Const());
  }
  static Append(a, b){
    return Doc.Mk(AppendDoc(a.docNode, b.docNode), Map2Unit(a.updates, b.updates));
  }
  static EmbedView(view){
    const node=CreateEmbedNode();
    return Doc.Mk(EmbedDoc(node), Map(() => { }, Bind((doc_1) => {
      UpdateEmbedNode(node, doc_1.docNode);
      return doc_1.updates;
    }, view)));
  }
  static TextView(txt){
    const node=CreateTextNode();
    return Doc.Mk(TextDoc(node), Map((t) => {
      UpdateTextNode(node, t);
    }, txt));
  }
  static SvgElement(name, attr_1, children){
    const a=Attr.Concat(attr_1);
    const c=Doc.Concat(children);
    return Elt.New(globalThis.document.createElementNS("http://www.w3.org/2000/svg", name), a, c);
  }
  constructor(docNode, updates){
    super();
    this.docNode=docNode;
    this.updates=updates;
  }
}
let _c=Lazy((_i) => class $StartupCode_Client {
  static {
    _c=_i(this);
  }
  static defaults;
  static {
    this.defaults=New(5000, 150000, 2000, 1000, 30000);
  }
});
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
  let _1=New_45(elem, Flags(tree), arr, oar.length===0?null:Some((el) => {
    iter_1((f) => {
      f(el);
    }, oar);
  }));
  return _1;
}
function EmptyAttr(){
  return _c_8.EmptyAttr;
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
function Dynamic(view, set_1){
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
function Flags(a){
  return a!==null&&a.hasOwnProperty("flags")?a.flags:0;
}
function GetAnim(dyn, f){
  return Concat(map((n) => f(n, dyn.DynElem), dyn.DynNodes));
}
function Sync(elem, dyn){
  iter((d) => {
    d.NSync(elem);
  }, dyn.DynNodes);
}
function SetFlags(a, f){
  a.flags=f;
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
function Handler(name, callback){
  return Attr.A3((el) => {
    el.addEventListener(name, (d) =>(callback(el))(d), false);
  });
}
function Dynamic_1(name, view){
  return Dynamic(view, (el) =>(v) => el.setAttribute(name, v));
}
function DynamicBool(name, boolview){
  return Dynamic(boolview, (_1) =>(_2) => _2?_1.setAttribute(name, ""):_1.removeAttribute(name));
}
function OnAfterRender(callback){
  return Attr.A4(callback);
}
class Var extends Object_1 { }
let _c_1=Lazy((_i) => class $StartupCode_Client {
  static {
    _c_1=_i(this);
  }
  static staticNavigationDestinations;
  static requestSeq;
  static pendingCommandSeq;
  static maxSnapshotRecords;
  static watermarkStore;
  static pendingStore;
  static snapshotStore;
  static databaseVersion;
  static databaseName;
  static initializeClientExtensionGlobalsOnce;
  static currentAclSnapshotJson;
  static currentAclSnapshot;
  static runtimeAppendPageShapes;
  static replyPresentationDisposers;
  static replyPresentationModes;
  static registeredReplyPresentationResolvers;
  static registeredRenderers;
  static defaultCacheLimit;
  static defaultRenderLimit;
  static doc;
  static {
    this.doc=globalThis.document;
    this.defaultRenderLimit=200;
    this.defaultCacheLimit=1000;
    this.registeredRenderers=[];
    this.registeredReplyPresentationResolvers=[];
    this.replyPresentationModes=[];
    this.replyPresentationDisposers=[];
    this.runtimeAppendPageShapes=[];
    this.currentAclSnapshot=null;
    this.currentAclSnapshotJson="";
    this.initializeClientExtensionGlobalsOnce=(initializeClientExtensionGlobals(),0);
    this.databaseName="PulseTrade.Comm.Spa.BrowserDb";
    this.databaseVersion=3;
    this.snapshotStore="uiSnapshots";
    this.pendingStore="pendingCommands";
    this.watermarkStore="streamWatermarks";
    this.maxSnapshotRecords=256;
    this.pendingCommandSeq=0;
    this.requestSeq=0;
    this.staticNavigationDestinations=[["/chat", "Chat"], ["/sets", "Sets"], ["/actors", "Actors"]];
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
function concat_1(separator, strings){
  return ofSeq(strings).join(separator);
}
function TrimEndWS(s){
  return s.replace(new RegExp("\\s+$"), "");
}
function Trim(s){
  return s.replace(new RegExp("^\\s+"), "").replace(new RegExp("\\s+$"), "");
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
function Substring(s, ix, ct){
  return s.substr(ix, ct);
}
function ReplaceOnce(string_1, search, replace){
  return string_1.replace(search, replace);
}
function TrimStartWS(s){
  return s.replace(new RegExp("^\\s+"), "");
}
function SplitChars(s, sep, opts){
  return Split(s, new RegExp("["+RegexEscape(sep.join(""))+"]"), opts);
}
function IsNullOrWhiteSpace(x){
  return x==null||(new RegExp("^\\s*$")).test(x);
}
function Split(s, pat, opts){
  return opts===1?filter_1((x) => x!=="", SplitWith(s, pat)):SplitWith(s, pat);
}
function RegexEscape(s){
  return s.replace(new RegExp("[-\\/\\\\^$*+?.()|[\\]{}]", "g"), "\\$&");
}
function SplitWith(str, pat){
  return str.split(pat);
}
function IndexOf(s, c, i){
  return s.indexOf(c, i);
}
function forall_1(f, s){
  return forall_2(f, protect(s));
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
function length(arr){
  return arr.dims===2?arr.length*arr.length:arr.length;
}
function get(arr, n){
  checkBounds(arr, n);
  return arr[n];
}
function checkBounds(arr, n){
  if(n<0||n>=arr.length)FailWith("Index was outside the bounds of the array.");
}
function set(arr, n, x){
  checkBounds(arr, n);
  arr[n]=x;
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
function map_1(f, x){
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
function head(l){
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
function New_13(pageId, keyJson, keyMode, displayName){
  return{
    pageId:pageId,
    keyJson:keyJson,
    keyMode:keyMode,
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
function map_2(f, s){
  return{GetEnumerator:() => {
    const en=Get(s);
    return new T(null, null, (e) => en.MoveNext()&&(e.c=f(en.Current),true), () => {
      en.Dispose();
    });
  }};
}
function head_1(s){
  const e=Get(s);
  try {
    return e.MoveNext()?e.Current:insufficient();
  }
  finally {
    const _1=e;
    if(typeof _1=="object"&&isIDisposable(_1))e.Dispose();
  }
}
function forall_2(p, s){
  return!exists_1((x) =>!p(x), s);
}
function distinct_1(s){
  return distinctBy_1((x) => x, s);
}
function collect_1(f, s){
  return concat_2(map_2(f, s));
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
function forall2_1(p, s1, s2){
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
function concat_2(ss){
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
function init_1(n, f){
  return take(n, initInfinite(f));
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
function rev(s){
  return delay(() => ofSeq(s).slice().reverse());
}
function take(n, s){
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
function max_1(s){
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
function seqEmpty(){
  return FailWith("The input sequence was empty.");
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
function New_21(maxSequence, buckets){
  return{maxSequence:maxSequence, buckets:buckets};
}
function New_22(valueId, keys, createdAtUtc, value, tags){
  return{
    valueId:valueId,
    keys:keys,
    createdAtUtc:createdAtUtc,
    value:value,
    tags:tags
  };
}
function New_23(reason){
  return{reason:reason};
}
function New_24(nodeCount, actorCount, maxSequence, nodes){
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
  return new FSharpMap("New_1", OfSeq(map_2((_1) => Pair.New(_1[0], _1[1]), a)));
}
function ToSeq(m){
  return map_2((kv) =>[kv.Key, kv.Value], Enumerate(false, m.Tree));
}
function New_25(actorId, displayName, kind, keys, status, routees){
  return{
    actorId:actorId,
    displayName:displayName,
    kind:kind,
    keys:keys,
    status:status,
    routees:routees
  };
}
function New_26(nodeId, nodeAddress, status, roles, actors){
  return{
    nodeId:nodeId,
    nodeAddress:nodeAddress,
    status:status,
    roles:roles,
    actors:actors
  };
}
function New_27(messageId, fromId, toId, scope, body, createdAtUtc){
  return{
    messageId:messageId,
    fromId:fromId,
    toId:toId,
    scope:scope,
    body:body,
    createdAtUtc:createdAtUtc
  };
}
function New_28(messages, nextAfterMessageId){
  return{messages:messages, nextAfterMessageId:nextAfterMessageId};
}
function New_29(streamId, newestSequence, cachedCount, source, touchedAt){
  return{
    streamId:streamId,
    newestSequence:newestSequence,
    cachedCount:cachedCount,
    source:source,
    touchedAt:touchedAt
  };
}
function New_30(type, requestId, fromId, toId, body, tags, browserId, tabId){
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
function New_31(fromId, toId, body, tags){
  return{
    fromId:fromId,
    toId:toId,
    body:body,
    tags:tags
  };
}
function New_32(messageId, speaker, createdAtUtc, body){
  return{
    messageId:messageId,
    speaker:speaker,
    createdAtUtc:createdAtUtc,
    body:body
  };
}
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
      const r=filter_1((a) =>!this.equals.apply(null, [(KeyValue(a))[0], k]), d);
      return length(r)<d.length&&(this.count=this.count-1,this.data[h]=r,true);
    }
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
  GetEnumerator(){
    return Get0(concat(GetFieldValues(this.data)));
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
  return New_41(get_Empty_1(), CreateElemNode(parent, EmptyAttr(), doc_1));
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
  Sync_1(el.Children);
  AfterRender(el);
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
  Sync(el.El, el.Attr);
  if(hasDirtyChildren(el))DoSyncElement(el);
}
function Sync_1(doc_1){
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
          Sync(t_1[0], t_1[1]);
        }, t.Attrs);
        return AfterRender(t);
      }
      else {
        const b=doc_1.$1;
        const a=doc_1.$0;
        Sync_1(a);
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
function CreateEmbedNode(){
  return{Current:null, Dirty:false};
}
function UpdateEmbedNode(node, upd){
  node.Current=upd;
  node.Dirty=true;
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
function Const(x){
  const o={s:Forever(x)};
  return() => o;
}
function Sink(act, a){
  function loop(){
    WhenRun(a(), act, () => {
      scheduler().Fork(loop);
    });
  }
  scheduler().Fork(loop);
}
function Map2(fn, a, a_1){
  return CreateLazy(() => Map2_1(fn, a(), a_1()));
}
function Map(fn, a){
  return CreateLazy(() => Map_1(fn, a()));
}
function MapCachedBy(eq, fn, a){
  const vref=[null];
  return CreateLazy(() => MapCachedBy_1(eq, vref, fn, a()));
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
function Map2Unit(a, a_1){
  return CreateLazy(() => Map2Unit_1(a(), a_1()));
}
function Bind(fn, view){
  return Join(Map(fn, view));
}
function Join(a){
  return CreateLazy(() => Join_1(a()));
}
function TextNodeDoc(Item){
  return{$:5, $0:Item};
}
function ElemDoc(Item){
  return{$:1, $0:Item};
}
function AppendDoc(Item1, Item2){
  return{
    $:0,
    $0:Item1,
    $1:Item2
  };
}
function EmbedDoc(Item){
  return{$:2, $0:Item};
}
function TextDoc(Item){
  return{$:4, $0:Item};
}
function New_33(shape, label, badge, className){
  return{
    shape:shape,
    label:label,
    badge:badge,
    className:className
  };
}
function New_34(submitPath, sessionPath, logoutPath, returnUrl, protectedRoute, sessionCookieName, title, lead, providerLabel, aclLabel){
  return{
    submitPath:submitPath,
    sessionPath:sessionPath,
    logoutPath:logoutPath,
    returnUrl:returnUrl,
    protectedRoute:protectedRoute,
    sessionCookieName:sessionCookieName,
    title:title,
    lead:lead,
    providerLabel:providerLabel,
    aclLabel:aclLabel
  };
}
function New_35(userName, password, returnUrl, keepSession){
  return{
    userName:userName,
    password:password,
    returnUrl:returnUrl,
    keepSession:keepSession
  };
}
function New_36(participantId, displayName, login, authenticated, provider, logoutPath){
  return{
    participantId:participantId,
    displayName:displayName,
    login:login,
    authenticated:authenticated,
    provider:provider,
    logoutPath:logoutPath
  };
}
function nonNegative(){
  return FailWith("The input must be non-negative.");
}
function insufficient(){
  return FailWith("The input sequence has an insufficient number of elements.");
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
function New_37(PageId, TabId, ValueId, CreatedAtUtc, Direction, Tags, Payload){
  return{
    PageId:PageId,
    TabId:TabId,
    ValueId:ValueId,
    CreatedAtUtc:CreatedAtUtc,
    Direction:Direction,
    Tags:Tags,
    Payload:Payload
  };
}
function New_38(pageId, title, setName, shape, tabId, tabMode, path, description){
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
    return Get(map_2((kv) =>({K:kv.Key, V:kv.Value}), Enumerate(false, this.tree)));
  }
  Add_1(k, v){
    return new FSharpMap("New_1", Add(Pair.New(k, v), this.tree));
  }
  GetHashCode(){
    return Hash(ofSeq(this));
  }
  get Tree(){
    return this.tree;
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
function Build(data, min_1, max_2){
  if(max_2-min_1+1<=0)return null;
  else {
    const center=(min_1+max_2)/2>>0;
    return Branch(get(data, center), Build(data, min_1, center-1), Build(data, center+1, max_2));
  }
}
function Branch(node, left, right){
  const a=left==null?0:left.Height;
  const b=right==null?0:right.Height;
  let _1=Compare(a, b)===1?a:b;
  let _2=1+_1;
  return New_44(node, left, right, _2, 1+(left==null?0:left.Count)+(right==null?0:right.Count));
}
function Add(x, t){
  return Put((_1, _2) => _2, x, t);
}
function Contains(v, t){
  return!((Lookup(v, t))[0]==null);
}
function Remove(k, src){
  const p=Lookup(k, src);
  const t=p[0];
  const spine=p[1];
  if(t==null)return src;
  else if(t.Right==null)return Rebuild(spine, t.Left);
  else if(t.Left==null)return Rebuild(spine, t.Right);
  else {
    const d=ofSeq(append_1(Enumerate(false, t.Left), Enumerate(false, t.Right)));
    let _1=Build(d, 0, d.length-1);
    return Rebuild(spine, _1);
  }
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
function Put(combine, k, t){
  const p=Lookup(k, t);
  const t_1=p[0];
  return t_1==null?Rebuild(p[1], Branch(k, null, null)):Rebuild(p[1], Branch(combine(t_1.Node, k), t_1.Left, t_1.Right));
}
function Rebuild(spine, t){
  let t_1;
  const h=(x_2) => x_2==null?0:x_2.Height;
  t_1=t;
  for(let i=0, _1=length(spine)-1;i<=_1;i++){
    const m=get(spine, i);
    if(m[0]){
      const x=m[1];
      const l=m[2];
      if(h(t_1)>h(l)+1){
        if(h(t_1.Left)===h(t_1.Right)+1){
          const m_1=t_1.Left;
          t_1=Branch(m_1.Node, Branch(x, l, m_1.Left), Branch(t_1.Node, m_1.Right, t_1.Right));
        }
        else t_1=Branch(t_1.Node, Branch(x, l, t_1.Left), t_1.Right);
      }
      else t_1=Branch(x, l, t_1);
    }
    else {
      const x_1=m[1];
      const r=m[2];
      if(h(t_1)>h(r)+1){
        if(h(t_1.Right)===h(t_1.Left)+1){
          const m_2=t_1.Right;
          t_1=Branch(m_2.Node, Branch(t_1.Node, t_1.Left, m_2.Left), Branch(x_1, m_2.Right, r));
        }
        else t_1=Branch(t_1.Node, t_1.Left, Branch(x_1, t_1.Right, r));
      }
      else t_1=Branch(x_1, t_1, r);
    }
  }
  return t_1;
}
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
class View { }
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
  return Anim(Concat_1(map_2(List, xs)));
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
let _c_3=Lazy((_i) => class Var_1 extends Object_1 {
  static {
    _c_3=_i(this);
  }
  static Create_1(v){
    return new ConcreteVar(false, {s:Ready(v, [])}, v);
  }
  static { }
});
function New_39(type, requestId, extensionId, channelId, operation, payload){
  return{
    type:type,
    requestId:requestId,
    extensionId:extensionId,
    channelId:channelId,
    operation:operation,
    payload:payload
  };
}
function Ok(ResultValue){
  return{$:0, $0:ResultValue};
}
function Error_1(ErrorValue){
  return{$:1, $0:ErrorValue};
}
function New_40(CanvasInstanceId, Poll, PollEnabled, Connected_1, Active, InFlight, DataRevision, ReconnectAttempt, DisposePending, Disposed){
  return{
    CanvasInstanceId:CanvasInstanceId,
    Poll:Poll,
    PollEnabled:PollEnabled,
    Connected:Connected_1,
    Active:Active,
    InFlight:InFlight,
    DataRevision:DataRevision,
    ReconnectAttempt:ReconnectAttempt,
    DisposePending:DisposePending,
    Disposed:Disposed
  };
}
function emptyFrame(kind, actionKind, canvasId){
  return New_42("ta-browser.v1", kind, actionKind, canvasId, "", "", "", 0, false, "", "", 0, "", "", false, 0, 0, "", "", false, [], 0, false);
}
function actionToWire(action){
  if(action.$==1)return emptyFrame("action", "reset-canvas", canvasText(action.$0));
  else if(action.$==2){
    const row=action.$1;
    const _1=emptyFrame("action", "add-row", canvasText(action.$0));
    return New_42(_1.wireVersion, _1.kind, _1.actionKind, _1.canvasInstanceId, row.RowId, rowKindText(row.Kind), row.DataRef, row.HeightWeight, row.Visible, _1.sourceId, _1.instrument, _1.intervalMinutes, _1.fromUtc, _1.toUtcExclusive, _1.includePartial, _1.afterDataRevision, _1.dataRevision, _1.reasonCode, _1.templateKey, _1.hasTemplateRowId, _1.editorValues, _1.expectedDocumentRevision, _1.hasExpectedDocumentRevision);
  }
  else if(action.$==3){
    const values=action.$3;
    const templateKey=action.$2;
    const rowId=action.$1;
    const _2=emptyFrame("action", "apply-template", canvasText(action.$0));
    return New_42(_2.wireVersion, _2.kind, _2.actionKind, _2.canvasInstanceId, rowId==null?"":rowId.$0, _2.rowKind, _2.dataRef, _2.heightWeight, _2.visible, _2.sourceId, _2.instrument, _2.intervalMinutes, _2.fromUtc, _2.toUtcExclusive, _2.includePartial, _2.afterDataRevision, _2.dataRevision, _2.reasonCode, templateKey, rowId!=null, map(editorInputToWire, values==null?[]:values), _2.expectedDocumentRevision, _2.hasExpectedDocumentRevision);
  }
  else if(action.$==4){
    const rowId_1=action.$1;
    const _3=emptyFrame("action", "remove-row", canvasText(action.$0));
    return New_42(_3.wireVersion, _3.kind, _3.actionKind, _3.canvasInstanceId, rowId_1, _3.rowKind, _3.dataRef, _3.heightWeight, _3.visible, _3.sourceId, _3.instrument, _3.intervalMinutes, _3.fromUtc, _3.toUtcExclusive, _3.includePartial, _3.afterDataRevision, _3.dataRevision, _3.reasonCode, _3.templateKey, _3.hasTemplateRowId, _3.editorValues, _3.expectedDocumentRevision, _3.hasExpectedDocumentRevision);
  }
  else if(action.$==5){
    const query=action.$1;
    const _4=emptyFrame("action", "change-query", canvasText(action.$0));
    return New_42(_4.wireVersion, _4.kind, _4.actionKind, _4.canvasInstanceId, _4.rowId, _4.rowKind, _4.dataRef, _4.heightWeight, _4.visible, optionText(query.SourceId), optionText(query.Instrument), optionInt(query.IntervalMinutes), optionText(query.FromUtc), optionText(query.ToUtcExclusive), optionBool(query.IncludePartial), _4.afterDataRevision, _4.dataRevision, _4.reasonCode, _4.templateKey, _4.hasTemplateRowId, _4.editorValues, _4.expectedDocumentRevision, _4.hasExpectedDocumentRevision);
  }
  else if(action.$==6){
    const revision=action.$1;
    const _5=emptyFrame("action", "poll-delta", canvasText(action.$0));
    return New_42(_5.wireVersion, _5.kind, _5.actionKind, _5.canvasInstanceId, _5.rowId, _5.rowKind, _5.dataRef, _5.heightWeight, _5.visible, _5.sourceId, _5.instrument, _5.intervalMinutes, _5.fromUtc, _5.toUtcExclusive, _5.includePartial, Number(revision), _5.dataRevision, _5.reasonCode, _5.templateKey, _5.hasTemplateRowId, _5.editorValues, _5.expectedDocumentRevision, _5.hasExpectedDocumentRevision);
  }
  else if(action.$==7){
    const reason=action.$1;
    const _6=emptyFrame("action", "full-snapshot", canvasText(action.$0));
    return New_42(_6.wireVersion, _6.kind, _6.actionKind, _6.canvasInstanceId, _6.rowId, _6.rowKind, _6.dataRef, _6.heightWeight, _6.visible, _6.sourceId, _6.instrument, _6.intervalMinutes, _6.fromUtc, _6.toUtcExclusive, _6.includePartial, _6.afterDataRevision, _6.dataRevision, reason, _6.templateKey, _6.hasTemplateRowId, _6.editorValues, _6.expectedDocumentRevision, _6.hasExpectedDocumentRevision);
  }
  else return emptyFrame("action", "reset-view", canvasText(action.$0));
}
function actionRequestToWire(request){
  const _1=actionToWire(request.Action);
  const o=request.ExpectedDocumentRevision;
  const o_1=o==null?null:Some(Number(o.$0));
  let _2=o_1==null?0:o_1.$0;
  return New_42(_1.wireVersion, _1.kind, _1.actionKind, _1.canvasInstanceId, _1.rowId, _1.rowKind, _1.dataRef, _1.heightWeight, _1.visible, _1.sourceId, _1.instrument, _1.intervalMinutes, _1.fromUtc, _1.toUtcExclusive, _1.includePartial, _1.afterDataRevision, _1.dataRevision, _1.reasonCode, _1.templateKey, _1.hasTemplateRowId, _1.editorValues, _2, request.ExpectedDocumentRevision!=null);
}
function applyWire(current, wire){
  return Bind_2((decoded) => {
    if(wire.wireVersion=="ta-browser.v1"||text(wire.updateKind)=="full")return Ok(decoded);
    else if(text(wire.updateKind)!="delta")return Error_1("Unsupported TA browser update kind.");
    else if(wire.baseDataRevision!==current.DataRevision)return Error_1("TA browser delta base revision does not match current state.");
    else if(!Equals(decoded.Identity, current.Identity)||decoded.DocumentRevision!==current.DocumentRevision)return Error_1("TA browser delta document identity or revision changed.");
    else {
      const mergedSeries=wire.series==null?current.Data:fold((_3, _4) => {
        const p=mergeSeries({
          Identity:current.Identity,
          Document:current.Document,
          Data:_3,
          DocumentRevision:current.DocumentRevision,
          DataRevision:current.DataRevision,
          LastTransportSequence:current.LastTransportSequence,
          View:current.View,
          Poll:current.Poll,
          LastError:current.LastError
        }, wire.timeline, _4);
        return _3.Add_1(p[0], p[1]);
      }, current.Data, wire.series);
      const o=decoded.Document;
      const o_1=o==null?null:Some(o.$0.StatusRef);
      const statusRef=o_1==null?"status":o_1.$0;
      const m=decoded.Data.TryFind(statusRef);
      let _1=m==null?mergedSeries:mergedSeries.Add_1(statusRef, m.$0);
      let _2={
        Identity:decoded.Identity,
        Document:decoded.Document,
        Data:_1,
        DocumentRevision:decoded.DocumentRevision,
        DataRevision:decoded.DataRevision,
        LastTransportSequence:decoded.LastTransportSequence,
        View:current.View,
        Poll:decoded.Poll,
        LastError:decoded.LastError
      };
      return Ok(_2);
    }
  }, stateFromWire(wire));
}
function text(value){
  return value==null?"":value;
}
function canvasText(a){
  return a.$0;
}
function rowKindText(a){
  return a.$==1?"volume":a.$==2?"sma":a.$==3?"dmi":a.$==4?"adx":a.$==5?"macd":a.$==6?"heikin-ashi":"candlestick";
}
function editorInputToWire(input_1){
  const m=input_1.Value;
  return m.$==1?New_46(input_1.Path, "number", "", m.$0, false):m.$==2?New_46(input_1.Path, "bool", "", 0, m.$0):New_46(input_1.Path, "text", m.$0, 0, false);
}
function optionBool(value){
  return value==null?false:value.$0;
}
function optionText(value){
  return value==null?"":value.$0;
}
function optionInt(value){
  return value==null?0:value.$0;
}
function stateFromWire(wire){
  if(wire==null||wire.wireVersion!="ta-browser.v1"&&wire.wireVersion!="ta-browser.v2"&&wire.wireVersion!="ta-browser.v3"&&wire.wireVersion!="ta-browser.v4")return Error_1("Unsupported TA browser state wire.");
  else if(wire.wireVersion=="ta-browser.v4"&&!(wire.series==null)&&exists((series) => {
    const a=0;
    const b=series.pointCount;
    let _1=Compare(a, b)===1?a:b;
    return!temporalSeriesMetadataIsValid(_1, series);
  }, wire.series))return Error_1("TA browser temporal metadata arrays do not match pointCount.");
  else {
    const rows=wire.rows==null?[]:map((row) => {
      const R=text(row.rowId);
      const K=rowKind(row.kind);
      const D=text(row.dataRef);
      const T_1=row.traces==null?[]:map((trace) =>({
        TraceId:text(trace.traceId),
        Kind:traceKind(trace.kind),
        DataRef:text(trace.dataRef),
        Label:text(trace.label),
        Color:text(trace.color),
        Width:trace.width,
        Visible:trace.visible,
        Options:new FSharpMap("New", [])
      }), row.traces);
      return{
        RowId:R,
        Kind:K,
        DataRef:D,
        HeightWeight:row.heightWeight,
        Visible:row.visible,
        Options:new FSharpMap("New", []),
        Traces:T_1
      };
    }, wire.rows);
    const seriesData=wire.series==null?new FSharpMap("New", []):OfArray(map((series) => {
      const points=seriesPointValues(wire, series);
      return[text(series.dataRef), {$:4, $0:points}];
    }, wire.series));
    const status={$:5, $0:new FSharpMap("New", ofArray([["label", {$:3, $0:text(wire.statusLabel)}], ["freshness", {$:3, $0:text(wire.freshness)}], ["watermarkUtc", {$:3, $0:text(wire.watermarkUtc)}], ["quality", {$:3, $0:text(wire.quality)}], ["lagSeconds", {$:2, $0:wire.lagSeconds}], ["reasonCode", {$:3, $0:text(wire.reasonCode)}]]))};
    const data=seriesData.Add_1(text(wire.statusRef), status);
    const defaultView=OfArray(ofSeq(ofSeq_1(delay(() => append_1(!IsNullOrWhiteSpace(wire.querySourceId)?[["query.sourceId", {$:3, $0:text(wire.querySourceId)}]]:[], delay(() => append_1(!IsNullOrWhiteSpace(wire.queryInstrument)?[["query.instrument", {$:3, $0:text(wire.queryInstrument)}]]:[], delay(() => append_1(wire.queryIntervalMinutes>0?[["query.intervalMinutes", {$:2, $0:wire.queryIntervalMinutes}]]:[], delay(() => append_1(!IsNullOrWhiteSpace(wire.queryFromUtc)?[["query.fromUtc", {$:3, $0:text(wire.queryFromUtc)}]]:[], delay(() => append_1(!IsNullOrWhiteSpace(wire.queryToUtcExclusive)?[["query.toUtcExclusive", {$:3, $0:text(wire.queryToUtcExclusive)}]]:[], delay(() =>[["query.includePartial", {$:1, $0:wire.queryIncludePartial}]]))))))))))))));
    const lastError=IsNullOrWhiteSpace(wire.errorCode)&&IsNullOrWhiteSpace(wire.errorMessage)?null:Some({
      ReasonCode:text(wire.errorCode),
      Message:text(wire.errorMessage),
      Recoverable:wire.errorRecoverable
    });
    return Ok({
      Identity:{DocumentId:{$:0, $0:text(wire.documentId)}, CanvasInstanceId:{$:0, $0:text(wire.canvasInstanceId)}},
      Document:Some({
        WorkspaceId:text(wire.workspaceId),
        Title:text(wire.title),
        RowsRef:text(wire.rowsRef),
        StatusRef:text(wire.statusRef),
        SharedTimeAxis:wire.sharedTimeAxis,
        Rows:rows,
        AllowedActions:wire.allowedActions==null?[]:wire.allowedActions,
        DefaultView:defaultView
      }),
      Data:data,
      DocumentRevision:wire.documentRevision,
      DataRevision:wire.dataRevision,
      LastTransportSequence:wire.transportSequence,
      View:{Values:new FSharpMap("New", [])},
      Poll:pollState(wire.pollKind),
      LastError:lastError
    });
  }
}
function mergeSeries(current, timeline, wire){
  let _1;
  const dataRef=text(wire.dataRef);
  const m=current.Data.TryFind(dataRef);
  const currentPoints=m!=null&&m.$==1&&(m.$0.$==4&&(_1=m.$0.$0,true))?_1:[];
  const f=(x) => IsNullOrWhiteSpace(pointTime(x));
  let _2=filter_1((x) =>!f(x), (wire.hasRemoveBeforeTime&&!IsNullOrWhiteSpace(wire.removeBeforeTime)?filter_1((point) => Compare(pointTime(point), wire.removeBeforeTime)>=0, currentPoints):currentPoints).concat(wire.pointCount>0?columnarPointValues(timeline, wire):wire.points==null?[]:map(pointValue, wire.points)));
  let _3=map((point) =>[pointTime(point), point], _2);
  let _4=OfArray(_3);
  let _5=ToSeq(_4);
  let _6=ofSeq(_5);
  let _7=map((t) => t[1], _6);
  let _8={$:4, $0:_7};
  return[dataRef, _8];
}
function temporalSeriesMetadataIsValid(count, series){
  return!series.hasTemporal||!(series.sourceIntervalIds==null)&&length(series.sourceIntervalIds)===count&&!(series.scaleKeys==null)&&length(series.scaleKeys)===count&&!(series.intervalStartUtc==null)&&length(series.intervalStartUtc)===count&&!(series.intervalEndUtc==null)&&length(series.intervalEndUtc)===count&&!(series.observedThroughUtc==null)&&length(series.observedThroughUtc)===count&&!(series.availableAtUtc==null)&&length(series.availableAtUtc)===count&&!(series.hasAvailableAtUtc==null)&&length(series.hasAvailableAtUtc)===count&&!(series.finality==null)&&length(series.finality)===count&&!(series.projections==null)&&length(series.projections)===count&&!(series.qualities==null)&&length(series.qualities)===count;
}
function pollState(value){
  const m=text(value);
  return m=="mounted-idle"?{$:1}:m=="ready"?{$:2}:m=="poll-in-flight"?{$:3}:m=="suspended"?{$:5}:m=="paused-for-resync"?{$:6}:m=="disposed"?{$:7}:{$:0};
}
function seriesPointValues(wire, series){
  return wire.wireVersion=="ta-browser.v3"||wire.wireVersion=="ta-browser.v4"?columnarPointValues(wire.timeline, series):series.points==null?[]:map(pointValue, series.points);
}
function traceKind(value){
  const m=text(value);
  return m=="volume"?{$:1}:m=="line"?{$:2}:m=="histogram"?{$:3}:{$:0};
}
function rowKind(value){
  const m=text(value).toLowerCase();
  return m=="volume"?{$:1}:m=="sma"?{$:2}:m=="dmi"?{$:3}:m=="adx"?{$:4}:m=="macd"?{$:5}:m=="heikin-ashi"?{$:6}:{$:0};
}
function pointTime(value){
  let _1;
  if(value.$==5){
    const values=value.$0;
    const _2=values.TryFind("_type");
    const _3=values.TryFind("intervalStartUtc");
    const _4=values.TryFind("t");
    switch(_2!=null&&_2.$==1?_2.$0.$==3?_2.$0.$0=="temporal-point.v1"?_3!=null&&_3.$==1?_3.$0.$==3?(_1=_3.$0.$0,0):_4!=null&&_4.$==1?_4.$0.$==3?(_1=_4.$0.$0,1):2:2:_4!=null&&_4.$==1?_4.$0.$==3?(_1=_4.$0.$0,1):2:2:_4!=null&&_4.$==1?_4.$0.$==3?(_1=_4.$0.$0,1):2:2:_4!=null&&_4.$==1?_4.$0.$==3?(_1=_4.$0.$0,1):2:2:_4!=null&&_4.$==1?_4.$0.$==3?(_1=_4.$0.$0,1):2:2){
      case 0:
        return text(_1);
      case 1:
        return text(_1);
      case 2:
        return"";
    }
  }
  else return"";
}
function columnarPointValues(timeline, series){
  const timeline_1=timeline==null?[]:timeline;
  const a=0;
  const b=series.pointCount;
  const count=Compare(a, b)===1?a:b;
  const indices=series.timeIndices==null?[]:series.timeIndices;
  return init(count, (offset) => {
    const timelineIndex=length(indices)===count?get(indices, offset):series.startIndex+offset;
    return series.hasTemporal?temporalPointValue(get(series.sourceIntervalIds, offset), get(series.scaleKeys, offset), get(series.intervalStartUtc, offset), get(series.intervalEndUtc, offset), get(series.observedThroughUtc, offset), get(series.availableAtUtc, offset), get(series.hasAvailableAtUtc, offset), get(series.finality, offset), get(series.projections, offset), get(series.qualities, offset), pointPayload(timelineIndex>=0&&timelineIndex<length(timeline_1)?text(get(timeline_1, timelineIndex)):"", !(series.openValues==null)&&length(series.openValues)===count, series.openValues==null||length(series.openValues)!==count?0:get(series.openValues, offset), !(series.highValues==null)&&length(series.highValues)===count, series.highValues==null||length(series.highValues)!==count?0:get(series.highValues, offset), !(series.lowValues==null)&&length(series.lowValues)===count, series.lowValues==null||length(series.lowValues)!==count?0:get(series.lowValues, offset), !(series.closeValues==null)&&length(series.closeValues)===count, series.closeValues==null||length(series.closeValues)!==count?0:get(series.closeValues, offset), !(series.volumeValues==null)&&length(series.volumeValues)===count, series.volumeValues==null||length(series.volumeValues)!==count?0:get(series.volumeValues, offset), !(series.lineValues==null)&&length(series.lineValues)===count, series.lineValues==null||length(series.lineValues)!==count?0:get(series.lineValues, offset))):pointPayload(timelineIndex>=0&&timelineIndex<length(timeline_1)?text(get(timeline_1, timelineIndex)):"", !(series.openValues==null)&&length(series.openValues)===count, series.openValues==null||length(series.openValues)!==count?0:get(series.openValues, offset), !(series.highValues==null)&&length(series.highValues)===count, series.highValues==null||length(series.highValues)!==count?0:get(series.highValues, offset), !(series.lowValues==null)&&length(series.lowValues)===count, series.lowValues==null||length(series.lowValues)!==count?0:get(series.lowValues, offset), !(series.closeValues==null)&&length(series.closeValues)===count, series.closeValues==null||length(series.closeValues)!==count?0:get(series.closeValues, offset), !(series.volumeValues==null)&&length(series.volumeValues)===count, series.volumeValues==null||length(series.volumeValues)!==count?0:get(series.volumeValues, offset), !(series.lineValues==null)&&length(series.lineValues)===count, series.lineValues==null||length(series.lineValues)!==count?0:get(series.lineValues, offset));
  });
}
function pointValue(point){
  return point.hasTemporal?temporalPointValue(point.sourceIntervalId, point.scaleKey, point.intervalStartUtc, point.intervalEndUtc, point.observedThroughUtc, point.availableAtUtc, point.hasAvailableAtUtc, point.finality, point.projection, point.quality, pointPayload(point.time, point.hasOpen, point.openValue, point.hasHigh, point.highValue, point.hasLow, point.lowValue, point.hasClose, point.closeValue, point.hasVolume, point.volumeValue, point.hasLineValue, point.lineValue)):pointPayload(point.time, point.hasOpen, point.openValue, point.hasHigh, point.highValue, point.hasLow, point.lowValue, point.hasClose, point.closeValue, point.hasVolume, point.volumeValue, point.hasLineValue, point.lineValue);
}
function pointPayload(time, hasOpen, openValue, hasHigh, highValue, hasLow, lowValue, hasClose, closeValue, hasVolume, volumeValue, hasLineValue, lineValue){
  return{$:5, $0:OfArray(ofSeq(ofSeq_1(delay(() => append_1(!IsNullOrWhiteSpace(time)?[["t", {$:3, $0:time}]]:[], delay(() => append_1(hasOpen?[["o", {$:2, $0:openValue}]]:[], delay(() => append_1(hasHigh?[["h", {$:2, $0:highValue}]]:[], delay(() => append_1(hasLow?[["l", {$:2, $0:lowValue}]]:[], delay(() => append_1(hasClose?[["c", {$:2, $0:closeValue}]]:[], delay(() => append_1(hasVolume?[["v", {$:2, $0:volumeValue}]]:[], delay(() => hasLineValue?[["v", {$:2, $0:lineValue}]]:[]))))))))))))))))};
}
function temporalPointValue(sourceIntervalId, scaleKey, intervalStartUtc, intervalEndUtc, observedThroughUtc, availableAtUtc, hasAvailableAtUtc, finality, projection, quality, payload){
  return{$:5, $0:OfArray(ofSeq(ofSeq_1(delay(() => append_1([["_type", {$:3, $0:"temporal-point.v1"}]], delay(() => append_1([["sourceIntervalId", {$:3, $0:sourceIntervalId}]], delay(() => append_1([["scaleKey", {$:3, $0:scaleKey}]], delay(() => append_1([["intervalStartUtc", {$:3, $0:intervalStartUtc}]], delay(() => append_1([["intervalEndUtc", {$:3, $0:intervalEndUtc}]], delay(() => append_1([["observedThroughUtc", {$:3, $0:observedThroughUtc}]], delay(() => append_1([["finality", {$:3, $0:finality}]], delay(() => append_1([["projection", {$:3, $0:projection}]], delay(() => append_1([["value", payload]], delay(() => append_1(hasAvailableAtUtc?[["availableAtUtc", {$:3, $0:availableAtUtc}]]:[], delay(() =>!IsNullOrWhiteSpace(quality)?[["quality", {$:3, $0:quality}]]:[]))))))))))))))))))))))))};
}
let Disconnected={$:6};
function PollDue(nowUtc){
  return{$:4, $0:nowUtc};
}
function RequestTimedOut(nowUtc){
  return{$:5, $0:nowUtc};
}
let Connected={$:0};
function ResyncRequired(reasonCode){
  return{$:8, $0:reasonCode};
}
function StateAccepted(dataRevision, pollEnabled){
  return{
    $:1,
    $0:dataRevision,
    $1:pollEnabled
  };
}
let Dispose={$:9};
let CommandRejected={$:2};
function StartAction(Item){
  return{$:3, $0:Item};
}
function ActiveChanged(Item){
  return{$:7, $0:Item};
}
function render(options, callbacks, runtimeState){
  let instrumentDraft, intervalDraft, fromDateDraft, toDateDraft, synchronizedDocumentRevision, addRowSequence, pendingAddRowId, navigatorElement, chartRenderSequence, actionSequence;
  const canvasId=runtimeState.Get().Identity.CanvasInstanceId;
  const editorSchemas=options.EditorSchemas==null?[]:options.EditorSchemas;
  const initialEditorSchema=tryHead(editorSchemas);
  const o=initialEditorSchema==null?null:Some(initialEditorSchema.$0.TemplateKey);
  let _1=o==null?"":o.$0;
  const selectedTemplate=_c_3.Create_1(_1);
  const o_1=initialEditorSchema==null?null:Some(initialEditorInputs(initialEditorSchema.$0));
  let _2=o_1==null?[]:o_1.$0;
  const editorValues=_c_3.Create_1(_2);
  instrumentDraft="";
  intervalDraft="";
  fromDateDraft="";
  toDateDraft="";
  synchronizedDocumentRevision=-1n;
  const addKind=_c_3.Create_1("Sma");
  const addDataRef=_c_3.Create_1("series.sma");
  const addPeriod=_c_3.Create_1("20");
  const addDiPeriod=_c_3.Create_1("14");
  const addAdxPeriod=_c_3.Create_1("14");
  const addFastPeriod=_c_3.Create_1("12");
  const addSlowPeriod=_c_3.Create_1("26");
  const addSignalPeriod=_c_3.Create_1("9");
  const draftWindow=_c_3.Create_1(null);
  addRowSequence=0;
  pendingAddRowId=null;
  navigatorElement=null;
  chartRenderSequence=0;
  const uiState=_c_3.Create_1({
    Window:{StartIndex:0, Count:options.DefaultVisibleBars},
    FollowLatest:true,
    HiddenRows:new FSharpSet("New_2", null),
    AddRowOpen:false,
    CursorIndex:null,
    PendingActionId:null,
    Feedback:""
  });
  actionSequence=0;
  const commandsDisabledView=Map2((_3, _4) => remoteDisabled(_3.Poll)||_4.PendingActionId!=null, runtimeState.View, uiState.View);
  const commandsDisabledNow=() => remoteDisabled(runtimeState.Get().Poll)||uiState.Get().PendingActionId!=null;
  const startAction=(action, successText, onAccepted) => {
    actionSequence=actionSequence+1;
    const request={
      RequestId:canvasIdText(canvasId)+":ui:"+String(actionSequence),
      ExpectedDocumentRevision:Some(runtimeState.Get().DocumentRevision),
      Action:action
    };
    return submit(callbacks, uiState, runtimeState.Get().DocumentRevision, request, successText, onAccepted);
  };
  const chartRuntimeView=MapCachedBy((_3, _4) => _3.DocumentRevision===_4.DocumentRevision&&_3.DataRevision===_4.DataRevision&&_3.LastTransportSequence===_4.LastTransportSequence, (x) => x, runtimeState.View);
  const referenceLength=() => {
    const m=runtimeState.Get().Document;
    if(m!=null&&m.$==1){
      const o_2=tryFind((a) => a.Visible, m.$0.Rows);
      const o_3=o_2==null?null:Some(rowReferenceLength(o_2.$0, runtimeState.Get().Data));
      return o_3==null?0:o_3.$0;
    }
    else return 0;
  };
  const resolvedWindow=(ui) => resolveWindow(options.MinimumVisibleBars, options.MaximumVisibleBars, referenceLength(), ui.FollowLatest, ui.Window);
  const setWindow=(followLatest, window_1) => {
    const current=uiState.Get();
    uiState.Set({
      Window:resolveWindow(options.MinimumVisibleBars, options.MaximumVisibleBars, referenceLength(), followLatest, window_1),
      FollowLatest:followLatest,
      HiddenRows:current.HiddenRows,
      AddRowOpen:current.AddRowOpen,
      CursorIndex:null,
      PendingActionId:current.PendingActionId,
      Feedback:current.Feedback
    });
    return draftWindow.Set(null);
  };
  const panWindow=(delta) => {
    const current=uiState.Get();
    const total=referenceLength();
    const visible=resolvedWindow(current);
    const candidate=clampWindow(options.MinimumVisibleBars, options.MaximumVisibleBars, total, {StartIndex:visible.StartIndex+delta, Count:visible.Count});
    setWindow(candidate.StartIndex===viewportMaximumStart(total, candidate), candidate);
  };
  const zoomWindow=(delta) => {
    const current=uiState.Get();
    const visible=resolvedWindow(current);
    setWindow(current.FollowLatest, {StartIndex:visible.StartIndex, Count:visible.Count+delta});
  };
  const resetWindow=() => {
    setWindow(true, {StartIndex:0, Count:options.DefaultVisibleBars});
    const _3=uiState.Get();
    let _4={
      Window:_3.Window,
      FollowLatest:_3.FollowLatest,
      HiddenRows:_3.HiddenRows,
      AddRowOpen:_3.AddRowOpen,
      CursorIndex:_3.CursorIndex,
      PendingActionId:_3.PendingActionId,
      Feedback:"Local view reset."
    };
    uiState.Set(_4);
  };
  const setWindowCount=(count) => {
    const total=referenceLength();
    const a=options.MinimumVisibleBars;
    const b=Compare(total, count)===-1?total:count;
    const boundedCount=Compare(a, b)===1?a:b;
    const a_1=0;
    const b_1=total-boundedCount;
    let _3=Compare(a_1, b_1)===1?a_1:b_1;
    let _4={StartIndex:_3, Count:boundedCount};
    setWindow(true, _4);
  };
  const setCursorIndex=(value) => {
    if(!Equals(uiState.Get().CursorIndex, value)){
      const _3=uiState.Get();
      let _4={
        Window:_3.Window,
        FollowLatest:_3.FollowLatest,
        HiddenRows:_3.HiddenRows,
        AddRowOpen:_3.AddRowOpen,
        CursorIndex:value,
        PendingActionId:_3.PendingActionId,
        Feedback:_3.Feedback
      };
      uiState.Set(_4);
    }
  };
  const resetEditorFor=(templateKey) => {
    selectedTemplate.Set(templateKey);
    const o_2=tryFind((schema) => schema.TemplateKey==templateKey, editorSchemas);
    const o_3=o_2==null?null:Some(initialEditorInputs(o_2.$0));
    let _3=o_3==null?[]:o_3.$0;
    editorValues.Set(_3);
  };
  const editorTestId=(path) =>"ta-editor-"+Replace(Replace(Replace(path, ".", "-"), "[", "-"), "]", "");
  const setEditorScalar=(path, value) => editorValues.Set(setEditorInput({Path:path, Value:value}, editorValues.Get()));
  const removeEditorScalar=(path) => {
    editorValues.Set(filter_1((current) => current.Path!=path, editorValues.Get()));
  };
  const scalarText=(path) => {
    const o_2=tryEditorInput(path, editorValues.Get());
    const o_3=o_2==null?null:Some(editorScalarText(o_2.$0));
    return o_3==null?"":o_3.$0;
  };
  function editorKind(path){
    return(labelText) =>(required) =>(kind) => {
      if(kind.$==7){
        const fields=kind.$0;
        return Doc.Element("fieldset", [Attr.Create("style", "min-width:0; margin:0; padding:7px; border:1px solid #cbd6e5; border-radius:5px;")], [Doc.Element("legend", [Attr.Create("style", "padding:0 4px; font-size:11px; color:#40536d;")], [Doc.TextNode(labelText)]), Doc.Element("div", [Attr.Create("style", "display:grid; grid-template-columns:repeat(auto-fit,minmax(130px,1fr)); gap:7px; min-width:0;")], ofSeq_1(delay(() => map_2((field_1) =>(((editorKind(path+"."+field_1.Key))(field_1.Label))(field_1.Required))(field_1.Kind), fields))))]);
      }
      else if(kind.$==6){
        const maximum=kind.$2;
        const itemKind=kind.$0;
        const indexesView=MapCachedBy(Equals, (x) => x, Map((v) => listIndexes(path, v), editorValues.View));
        return Doc.Element("div", [Attr.Create("style", "display:flex; flex-direction:column; gap:5px; min-width:0;")], [Doc.Element("span", [Attr.Create("style", "font-size:10px; color:#60738b;")], [Doc.TextNode(required?labelText+" *":labelText)]), Doc.EmbedView(Map((indexes) => Doc.Element("div", [Attr.Create("data-testid", editorTestId(path)+"-items"), Attr.Create("style", "display:flex; flex-direction:column; gap:5px;")], ofSeq_1(delay(() => collect_1((m_1) => {
          const position=m_1[0];
          const index=m_1[1];
          const itemPath=String(path)+"["+String(index)+"]";
          return[Doc.Element("div", [Attr.Create("style", "display:grid; grid-template-columns:auto minmax(0,1fr); gap:5px; align-items:end;")], [Doc.Element("div", [Attr.Create("style", "display:flex; align-items:center; gap:3px; height:30px;")], [compactButton(editorTestId(itemPath)+"-up", "\u2191", "Move item up", () => {
            if(position>0)editorValues.Set(moveListItem(path, index, get(indexes, position-1), editorValues.Get()));
          }), compactButton(editorTestId(itemPath)+"-down", "\u2193", "Move item down", () => {
            if(position<length(indexes)-1)editorValues.Set(moveListItem(path, index, get(indexes, position+1), editorValues.Get()));
          }), compactButton(editorTestId(itemPath)+"-remove", "×", "Remove item", () => {
            editorValues.Set(removeListItem(path, index, editorValues.Get()));
          })]), (((editorKind(itemPath))("Item "+String(position+1)))(true))(itemKind)])];
        }, indexed(indexes))))), indexesView)), compactButton(editorTestId(path)+"-add", "+ Add", "Add "+labelText, () => {
          let _3;
          const count=listIndexes(path, editorValues.Get()).length;
          if(maximum!=null&&maximum.$==1&&(count>=maximum.$0&&(_3=maximum.$0,true))){
            const _4=uiState.Get();
            let _5={
              Window:_4.Window,
              FollowLatest:_4.FollowLatest,
              HiddenRows:_4.HiddenRows,
              AddRowOpen:_4.AddRowOpen,
              CursorIndex:_4.CursorIndex,
              PendingActionId:_4.PendingActionId,
              Feedback:String(labelText)+" allows at most "+String(_3)+" item(s)."
            };
            uiState.Set(_5);
          }
          else editorValues.Set(addListItem(path, itemKind, editorValues.Get()));
        })]);
      }
      else {
        const caption=required?labelText+" *":labelText;
        const shell_1=(control) => Doc.Element("label", [Attr.Create("style", "display:flex; flex-direction:column; gap:3px; min-width:0; font-size:10px; color:#60738b;")], [Doc.TextNode(caption), control]);
        if(kind.$==0)return shell_1(inputText(editorTestId(path), labelText, scalarText(path), (value) => {
          setEditorScalar(path, {$:0, $0:value});
        }));
        else if(kind.$==1){
          const minimum=kind.$0;
          const maximum_1=kind.$1;
          return shell_1(element_1("input", ofSeq_1(delay(() => append_1([Attr.Create("data-testid", editorTestId(path))], delay(() => append_1([Attr.Create("type", "number")], delay(() => append_1([Attr.Create("value", scalarText(path))], delay(() => append_1([Attr.Create("step", "1")], delay(() => append_1([Attr.Create("style", "height:30px; min-width:0; width:100%; border:1px solid #b9c6d8; border-radius:4px; background:#fff; color:#142033; padding:4px 7px; box-sizing:border-box; font-size:12px;")], delay(() => append_1([OnAfterRender((node) => {
            const input_1=node;
            input_1.addEventListener("input", () => {
              let o_4;
              const m_1=(o_4=0n,[TryParse_1(input_1.value, {get:() => o_4, set:(v) => {
                o_4=v;
              }}), o_4]);
              return m_1[0]?setEditorScalar(path, {$:1, $0:Number(m_1[1])}):removeEditorScalar(path);
            });
          })], delay(() => append_1(minimum==null?[EmptyAttr()]:[Attr.Create("min", String(minimum.$0))], delay(() => maximum_1==null?[EmptyAttr()]:[Attr.Create("max", String(maximum_1.$0))])))))))))))))))), []));
        }
        else if(kind.$==2){
          const minimum_1=kind.$0;
          const maximum_2=kind.$1;
          return shell_1(element_1("input", ofSeq_1(delay(() => append_1([Attr.Create("data-testid", editorTestId(path))], delay(() => append_1([Attr.Create("type", "number")], delay(() => append_1([Attr.Create("value", scalarText(path))], delay(() => append_1([Attr.Create("step", "any")], delay(() => append_1([Attr.Create("style", "height:30px; min-width:0; width:100%; border:1px solid #b9c6d8; border-radius:4px; background:#fff; color:#142033; padding:4px 7px; box-sizing:border-box; font-size:12px;")], delay(() => append_1([OnAfterRender((node) => {
            const input_1=node;
            input_1.addEventListener("input", () => {
              let o_4;
              o_4=0;
              const _3=Number(input_1.value);
              let _4=isNaN(_3)?false:(o_4=_3,true);
              const m_1=[_4, o_4];
              return m_1[0]?setEditorScalar(path, {$:1, $0:m_1[1]}):removeEditorScalar(path);
            });
          })], delay(() => append_1(minimum_1==null?[EmptyAttr()]:[Attr.Create("min", fixedText(minimum_1.$0))], delay(() => maximum_2==null?[EmptyAttr()]:[Attr.Create("max", fixedText(maximum_2.$0))])))))))))))))))), []));
        }
        else if(kind.$==3){
          const m=tryEditorInput(path, editorValues.Get());
          const isChecked=m!=null&&m.$==1&&(m.$0.$==2&&m.$0.$0);
          return Doc.Element("label", [Attr.Create("style", "display:flex; align-items:center; gap:6px; min-height:30px; font-size:11px; color:#40536d;")], [element_1("input", ofSeq_1(delay(() => append_1([Attr.Create("data-testid", editorTestId(path))], delay(() => append_1([Attr.Create("type", "checkbox")], delay(() => append_1(isChecked?[Attr.Create("checked", "checked")]:[], delay(() =>[OnAfterRender((node) => {
            const input_1=node;
            input_1.addEventListener("change", () => setEditorScalar(path, {$:2, $0:input_1.checked}));
          })])))))))), []), Doc.TextNode(caption)]);
        }
        else if(kind.$==4){
          const choices=kind.$0;
          const o_2=tryFind((choice) => {
            const o_4=tryEditorInput(path, editorValues.Get());
            return o_4==null?false:editorScalarEqualsSdui(o_4.$0, choice.Value);
          }, choices);
          const o_3=o_2==null?null:Some(o_2.$0.Key);
          const selectedKey=o_3==null?"":o_3.$0;
          return shell_1(selectInput(editorTestId(path), selectedKey, ofArray(map((choice) =>[choice.Key, choice.Label], choices)), (key) => {
            let x;
            const o_4=tryFind((choice) => choice.Key==key, choices);
            if(o_4==null)x=null;
            else {
              const m_1=o_4.$0.Value;
              x=m_1.$==3?Some({$:0, $0:m_1.$0}):m_1.$==2?Some({$:1, $0:m_1.$0}):m_1.$==1?Some({$:2, $0:m_1.$0}):null;
            }
            if(x!=null)setEditorScalar(path, x.$0);
          }));
        }
        else if(kind.$==5){
          const scaleKeys=kind.$0;
          return shell_1(selectInput(editorTestId(path), scalarText(path), ofArray(map((value) =>[value, value], scaleKeys)), (value) => {
            setEditorScalar(path, {$:0, $0:value});
          }));
        }
        else return Doc.Empty;
      }
    };
  }
  const applyQuery=() => {
    let o_2;
    const m=(o_2=0,[TryParse(intervalDraft, {get:() => o_2, set:(v) => {
      o_2=v;
    }}), o_2]);
    const parsedInterval=m[0]&&m[1]>0?Some(m[1]):null;
    startAction({
      $:5,
      $0:canvasId,
      $1:{
        SourceId:null,
        Instrument:IsNullOrWhiteSpace(instrumentDraft)?null:Some(instrumentDraft),
        IntervalMinutes:parsedInterval,
        FromUtc:IsNullOrWhiteSpace(fromDateDraft)?null:Some(fromDateDraft),
        ToUtcExclusive:IsNullOrWhiteSpace(toDateDraft)?null:Some(toDateDraft),
        IncludePartial:Some(true)
      }
    }, "Query accepted.", () => { });
  };
  const addRow=() => {
    let optionsResult;
    const m=tryFind((schema_1) => schema_1.TemplateKey==selectedTemplate.Get(), editorSchemas);
    if(m!=null&&m.$==1){
      const schema=m.$0;
      const errors=validateEditorSubmission(schema, editorValues.Get());
      if(length(errors)>0){
        const _3=uiState.Get();
        let _4={
          Window:_3.Window,
          FollowLatest:_3.FollowLatest,
          HiddenRows:_3.HiddenRows,
          AddRowOpen:_3.AddRowOpen,
          CursorIndex:_3.CursorIndex,
          PendingActionId:_3.PendingActionId,
          Feedback:concat_1(" ", errors)
        };
        uiState.Set(_4);
      }
      else startAction({
        $:3,
        $0:canvasId,
        $1:null,
        $2:schema.TemplateKey,
        $3:editorValues.Get()
      }, schema.DisplayName+" accepted.", () => {
        const _14=uiState.Get();
        let _15={
          Window:_14.Window,
          FollowLatest:_14.FollowLatest,
          HiddenRows:_14.HiddenRows,
          AddRowOpen:false,
          CursorIndex:_14.CursorIndex,
          PendingActionId:_14.PendingActionId,
          Feedback:_14.Feedback
        };
        uiState.Set(_15);
      });
    }
    else {
      const m_1=addKind.Get();
      const kind=m_1=="Volume"?{$:1}:m_1=="Dmi"?{$:3}:m_1=="Adx"?{$:4}:m_1=="Macd"?{$:5}:m_1=="HeikinAshi"?{$:6}:{$:2};
      const positive=(fieldName, textValue) => {
        let o_2;
        const m_2=(o_2=0,[TryParse(textValue, {get:() => o_2, set:(v) => {
          o_2=v;
        }}), o_2]);
        return m_2[0]&&m_2[1]>0?Ok(m_2[1]):Error_1(fieldName+" must be a positive integer.");
      };
      switch(kind.$==2?0:kind.$==3?0:kind.$==4?1:kind.$==5?2:3){
        case 0:
          optionsResult=Map_2((value) => new FSharpMap("New", ofArray([["period", {$:2, $0:value}]])), positive("Period", addPeriod.Get()));
          break;
        case 1:
          let _5;
          const _6=positive("DI period", addDiPeriod.Get());
          const _7=positive("ADX period", addAdxPeriod.Get());
          optionsResult=(_6.$==1?(_5=_6.$0,false):_7.$==1?(_5=_7.$0,false):(_5=[_7.$0, _6.$0],true))?Ok(new FSharpMap("New", ofArray([["diPeriod", {$:2, $0:_5[1]}], ["adxPeriod", {$:2, $0:_5[0]}]]))):Error_1(_5);
          break;
        case 2:
          let _8;
          const _9=positive("Fast period", addFastPeriod.Get());
          const _10=positive("Slow period", addSlowPeriod.Get());
          const _11=positive("Signal period", addSignalPeriod.Get());
          switch(_9.$==1?(_8=_9.$0,2):_10.$==1?(_8=_10.$0,2):_11.$==1?(_8=_11.$0,2):(_11.$0,_9.$0<_10.$0?(_8=[_9.$0, _11.$0, _10.$0],0):1)){
            case 0:
              optionsResult=Ok(new FSharpMap("New", ofArray([["fastPeriod", {$:2, $0:_8[0]}], ["slowPeriod", {$:2, $0:_8[2]}], ["signalPeriod", {$:2, $0:_8[1]}]])));
              break;
            case 1:
              optionsResult=Error_1("MACD fast period must be smaller than slow period.");
              break;
            case 2:
              optionsResult=Error_1(_8);
              break;
          }
          break;
        case 3:
          optionsResult=Ok(new FSharpMap("New", []));
          break;
      }
      if(optionsResult.$==0){
        const rowOptions=optionsResult.$0;
        addRowSequence=addRowSequence+1;
        const rowId="row-"+addKind.Get().toLowerCase()+"-"+String(addRowSequence);
        const spec={
          RowId:rowId,
          Kind:kind,
          DataRef:IsNullOrWhiteSpace(addDataRef.Get())?"series."+rowId:Trim(addDataRef.Get()),
          HeightWeight:1,
          Visible:true,
          Options:rowOptions,
          Traces:[]
        };
        pendingAddRowId=Some(rowId);
        startAction({
          $:2,
          $0:canvasId,
          $1:spec
        }, "Row accepted.", () => { });
      }
      else {
        const message=optionsResult.$0;
        const _12=uiState.Get();
        let _13={
          Window:_12.Window,
          FollowLatest:_12.FollowLatest,
          HiddenRows:_12.HiddenRows,
          AddRowOpen:_12.AddRowOpen,
          CursorIndex:_12.CursorIndex,
          PendingActionId:_12.PendingActionId,
          Feedback:message
        };
        uiState.Set(_13);
      }
    }
  };
  return Doc.Element("div", [Attr.Create("class", "ptcs-ta-workspace"), Attr.Create("data-testid", "ta-workspace"), Attr.Create("style", "display:flex; flex-direction:column; min-width:0; width:100%; min-height:640px; color:#142033; background:#f4f7fb; font-family:Segoe UI, Arial, sans-serif; letter-spacing:0;")], [Doc.EmbedView(MapCachedBy((_3, _4) => {
    const _5=_3.Document;
    const _6=_4.Document;
    return(_5!=null&&_5.$==1?_6!=null&&_6.$==1&&_5.$0.WorkspaceId==_6.$0.WorkspaceId:_6==null)&&_3.DocumentRevision===_4.DocumentRevision;
  }, (state) => {
    let _3, _4;
    const m=state.Document;
    if(m!=null&&m.$==1){
      const document=m.$0;
      if(state.DocumentRevision!==synchronizedDocumentRevision){
        const query=queryDraft(document.DefaultView);
        instrumentDraft=query.Instrument;
        intervalDraft=query.IntervalMinutes;
        fromDateDraft=query.FromUtc;
        toDateDraft=query.ToUtcExclusive;
        if(pendingAddRowId!=null&&pendingAddRowId.$==1){
          const rowId=pendingAddRowId.$0;
          if(exists((row) => row.RowId==rowId, document.Rows)){
            pendingAddRowId.$0;
            pendingAddRowId=null;
            const _5=uiState.Get();
            let _6={
              Window:_5.Window,
              FollowLatest:_5.FollowLatest,
              HiddenRows:_5.HiddenRows,
              AddRowOpen:false,
              CursorIndex:_5.CursorIndex,
              PendingActionId:_5.PendingActionId,
              Feedback:"Row added."
            };
            _3=uiState.Set(_6);
          }
          else _3=null;
        }
        else _3=null;
        _4=void(synchronizedDocumentRevision=state.DocumentRevision);
      }
      else _4=null;
      return Doc.Element("div", [Attr.Create("style", "display:flex; flex-direction:column; min-width:0;")], [Doc.Element("header", [Attr.Create("style", "display:flex; flex-direction:column; gap:7px; padding:10px 12px 8px; background:#fff; border-bottom:1px solid #dbe3ee;")], [Doc.Element("div", [Attr.Create("style", "display:flex; align-items:center; justify-content:space-between; gap:10px; flex-wrap:wrap;")], [Doc.Element("div", [Attr.Create("style", "min-width:0;")], [Doc.Element("h2", [Attr.Create("data-testid", "ta-workspace-title"), Attr.Create("style", "margin:0; font-size:17px; line-height:22px; font-weight:700; color:#152944;")], [Doc.TextNode(document.Title)]), Doc.Element("div", [Attr.Create("style", "font-size:11px; color:#667891; white-space:nowrap; overflow:hidden; text-overflow:ellipsis;")], [Doc.TextView(Map((current) =>"canvas "+canvasIdText(canvasId)+" / revision "+String(current.DataRevision), runtimeState.View))])]), Doc.EmbedView(Map((current) => {
        const status=statusPresentation(document.StatusRef, current);
        return Doc.Element("div", [Attr.Create("style", "display:flex; align-items:center; gap:5px; flex-wrap:wrap; justify-content:flex-end;")], [Doc.Element("div", [Attr.Create("data-testid", "ta-freshness"), Attr.Create("data-freshness", freshnessClass(status.Freshness)), Attr.Create("style", "border:1px solid #9fb0c6; border-radius:4px; padding:3px 7px; font-size:11px; font-weight:650; color:#27415f; background:#f8fafc;")], [Doc.TextNode(status.Label)]), Doc.Element("div", [Attr.Create("data-testid", "ta-poll-state"), Attr.Create("data-poll-state", pollText(current.Poll)), Attr.Create("style", "border:1px solid #c3cfdd; border-radius:4px; padding:3px 7px; font-size:10px; color:#53667d; background:#fff;")], [Doc.TextNode(pollText(current.Poll))])]);
      }, runtimeState.View))]), Doc.EmbedView(Map((current) => {
        const status=statusPresentation(document.StatusRef, current);
        return Doc.Element("div", [Attr.Create("data-testid", "ta-status-detail"), Attr.Create("style", "display:flex; gap:10px; flex-wrap:wrap; min-height:16px; font-size:10px; color:#60738b;")], ofSeq_1(delay(() => {
          const m_1=status.Watermark;
          let _7=m_1==null?[]:[Doc.Element("span", [], [Doc.TextNode("watermark "+m_1.$0)])];
          return append_1(_7, delay(() => {
            const m_2=status.Quality;
            let _8=m_2==null?[]:[Doc.Element("span", [], [Doc.TextNode("quality "+m_2.$0)])];
            return append_1(_8, delay(() => {
              const m_3=status.Error;
              if(m_3==null)return[];
              else {
                const value=m_3.$0;
                return[Doc.Element("span", [Attr.Create("data-testid", "ta-last-good-error"), Attr.Create("style", "color:#a33b43; font-weight:600;")], [Doc.TextNode(value)])];
              }
            }));
          }));
        })));
      }, runtimeState.View)), Doc.Element("div", [Attr.Create("data-testid", "ta-query-toolbar"), Attr.Create("style", "display:grid; grid-template-columns:repeat(auto-fit,minmax(120px,1fr)); gap:6px; align-items:end;")], [Doc.Element("label", [Attr.Create("style", "display:flex; flex-direction:column; gap:2px; min-width:0; font-size:10px; color:#60738b;")], [Doc.TextNode("Instrument"), inputText("ta-instrument", "Instrument", instrumentDraft, (value) => {
        instrumentDraft=value;
      })]), Doc.Element("label", [Attr.Create("style", "display:flex; flex-direction:column; gap:2px; min-width:0; font-size:10px; color:#60738b;")], [Doc.TextNode("Interval"), selectInput("ta-interval", intervalDraft, ofArray([["1", "1m"], ["5", "5m"], ["30", "30m"], ["60", "60m"], ["930", "Session"]]), (value) => {
        intervalDraft=value;
      })]), Doc.Element("label", [Attr.Create("style", "display:flex; flex-direction:column; gap:2px; min-width:0; font-size:10px; color:#60738b;")], [Doc.TextNode("From"), inputText("ta-from", "YYYY-MM-DD", fromDateDraft, (value) => {
        fromDateDraft=value;
      })]), Doc.Element("label", [Attr.Create("style", "display:flex; flex-direction:column; gap:2px; min-width:0; font-size:10px; color:#60738b;")], [Doc.TextNode("To"), inputText("ta-to", "YYYY-MM-DD", toDateDraft, (value) => {
        toDateDraft=value;
      })]), primaryButtonView("ta-apply-query", "Load / Apply", commandsDisabledView, commandsDisabledNow, applyQuery)]), Doc.Element("div", [Attr.Create("data-testid", "ta-local-toolbar"), Attr.Create("style", "display:flex; align-items:center; gap:5px; flex-wrap:wrap;")], [compactButton("ta-pan-left", "\u2190", "Pan earlier", () => {
        const a=1;
        const b=resolvedWindow(uiState.Get()).Count/4>>0;
        let _7=-(Compare(a, b)===1?a:b);
        panWindow(_7);
      }), compactButton("ta-pan-right", "\u2192", "Pan later", () => {
        const a=1;
        const b=resolvedWindow(uiState.Get()).Count/4>>0;
        let _7=Compare(a, b)===1?a:b;
        panWindow(_7);
      }), compactButton("ta-zoom-in", "+", "Show fewer bars", () => {
        zoomWindow(-8);
      }), compactButton("ta-zoom-out", "\u2212", "Show more bars", () => {
        zoomWindow(8);
      }), compactButton("ta-reset-view", "Reset View", "Reset local viewport to the latest bars", resetWindow), compactRemoteButton("ta-reset-canvas", "Reset Canvas", "Request server canvas reset", commandsDisabledView, commandsDisabledNow, () => {
        startAction({$:1, $0:canvasId}, "Canvas reset accepted.", () => { });
      }), compactButton("ta-add-row-toggle", "Add Row", "Open row request editor", () => {
        const _7=uiState.Get();
        let _8={
          Window:_7.Window,
          FollowLatest:_7.FollowLatest,
          HiddenRows:_7.HiddenRows,
          AddRowOpen:!uiState.Get().AddRowOpen,
          CursorIndex:_7.CursorIndex,
          PendingActionId:_7.PendingActionId,
          Feedback:_7.Feedback
        };
        uiState.Set(_8);
      }), Doc.Element("span", [Attr.Create("style", "margin-left:auto; color:#60738b; font-size:11px;")], [Doc.TextNode("local view controls do not query the backend")])]), Doc.EmbedView(Map((ui) => Doc.Element("div", [Attr.Create("data-testid", "ta-row-toggles"), Attr.Create("style", "display:flex; align-items:center; gap:5px; flex-wrap:wrap;")], ofSeq_1(delay(() => collect_1((row) => {
        const hidden=ui.HiddenRows.Contains(row.RowId);
        return[Doc.Element("div", [Attr.Create("style", "display:inline-flex; align-items:stretch; height:26px;")], [Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("data-testid", "ta-toggle-row-"+row.RowId), Attr.Create("aria-pressed", hidden?"false":"true"), Attr.Create("style", hidden?"height:26px; border:1px solid #c8d2df; border-right:0; border-radius:4px 0 0 4px; background:#fff; color:#7a8798; padding:2px 7px; font-size:11px; cursor:pointer;":"height:26px; border:1px solid #7da39d; border-right:0; border-radius:4px 0 0 4px; background:#edf8f6; color:#155d55; padding:2px 7px; font-size:11px; cursor:pointer;"), Handler("click", () =>() => {
          const nextHidden=hidden?uiState.Get().HiddenRows.Remove_1(row.RowId):uiState.Get().HiddenRows.Add_1(row.RowId);
          const _7=uiState.Get();
          let _8={
            Window:_7.Window,
            FollowLatest:_7.FollowLatest,
            HiddenRows:nextHidden,
            AddRowOpen:_7.AddRowOpen,
            CursorIndex:_7.CursorIndex,
            PendingActionId:_7.PendingActionId,
            Feedback:_7.Feedback
          };
          return uiState.Set(_8);
        })], [Doc.TextNode(rowKindText_1(row.Kind))]), Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("data-testid", "ta-remove-row-"+row.RowId), Attr.Create("title", "Remove "+rowKindText_1(row.Kind)+" row"), DynamicBool("disabled", commandsDisabledView), Dynamic_1("style", Map((disabled) => disabled?"width:26px; height:26px; border:1px solid #c8d2df; border-radius:0 4px 4px 0; background:#edf1f5; color:#8b98a8; padding:0; font-size:14px; cursor:not-allowed;":"width:26px; height:26px; border:1px solid #c8a7ab; border-radius:0 4px 4px 0; background:#fff; color:#8d3039; padding:0; font-size:14px; cursor:pointer;", commandsDisabledView)), Handler("click", () =>() =>!commandsDisabledNow()?startAction({
          $:4,
          $0:canvasId,
          $1:row.RowId
        }, rowKindText_1(row.Kind)+" row removal accepted.", () => { }):null)], [Doc.TextNode("×")])])];
      }, document.Rows)))), uiState.View)), Doc.EmbedView(Map((ui) =>!ui.AddRowOpen?Doc.Empty:Doc.Element("div", [Attr.Create("data-testid", "ta-add-row-editor"), Attr.Create("style", "display:flex; flex-direction:column; gap:7px; padding:7px; border:1px solid #cbd6e5; border-radius:5px; background:#f8fafc;")], ofSeq_1(delay(() => append_1(length(editorSchemas)>0?[Doc.Element("div", [Attr.Create("data-testid", "ta-generic-row-editor"), Attr.Create("style", "display:flex; flex-direction:column; gap:7px; min-width:0;")], [Doc.Element("label", [Attr.Create("style", "display:flex; flex-direction:column; gap:2px; min-width:0; font-size:10px; color:#60738b;")], [Doc.TextNode("Template"), selectInput("ta-editor-template", selectedTemplate.Get(), ofArray(map((schema) =>[schema.TemplateKey, schema.DisplayName], editorSchemas)), resetEditorFor)]), Doc.EmbedView(Map((templateKey) => {
        const m_1=tryFind((schema_1) => schema_1.TemplateKey==templateKey, editorSchemas);
        if(m_1!=null&&m_1.$==1){
          const schema=m_1.$0;
          return Doc.Element("div", [Attr.Create("style", "display:grid; grid-template-columns:repeat(auto-fit,minmax(150px,1fr)); gap:7px; min-width:0;")], ofSeq_1(delay(() => map_2((field_1) =>(((editorKind(field_1.Key))(field_1.Label))(field_1.Required))(field_1.Kind), schema.Fields))));
        }
        else return Doc.Element("div", [Attr.Create("style", "font-size:11px; color:#9a2f2f;")], [Doc.TextNode("Template schema is unavailable.")]);
      }, selectedTemplate.View))])]:[Doc.Element("div", [Attr.Create("style", "display:grid; grid-template-columns:repeat(auto-fit,minmax(120px,1fr)); gap:6px; align-items:end;")], [Doc.Element("label", [Attr.Create("style", "display:flex; flex-direction:column; gap:2px; font-size:10px; color:#60738b;")], [Doc.TextNode("Row kind"), selectInput("ta-add-row-kind", addKind.Get(), ofArray([["Sma", "SMA"], ["Volume", "Volume"], ["Dmi", "DMI"], ["Adx", "ADX"], ["Macd", "MACD"], ["HeikinAshi", "Heikin-Ashi"]]), (value) => {
        addKind.Set(value);
        addDataRef.Set("series."+value.toLowerCase());
      })]), Doc.Element("label", [Attr.Create("style", "display:flex; flex-direction:column; gap:2px; font-size:10px; color:#60738b;")], [Doc.TextNode("Data ref"), inputText("ta-add-row-data-ref", "series.sma", addDataRef.Get(), (value) => {
        addDataRef.Set(value);
      })]), Doc.EmbedView(Map((kind) => {
        const field_1=(labelText, testId, value, onChanged) => Doc.Element("label", [Attr.Create("style", "display:flex; flex-direction:column; gap:2px; font-size:10px; color:#60738b;")], [Doc.TextNode(labelText), inputText(testId, labelText, value, onChanged)]);
        switch(kind){
          case"Dmi":
          case"Sma":
            return field_1("Period", "ta-add-row-period", addPeriod.Get(), (value) => {
              addPeriod.Set(value);
            });
          case"Adx":
            return Doc.Element("div", [Attr.Create("style", "display:grid; grid-template-columns:1fr 1fr; gap:6px; min-width:0;")], [field_1("DI period", "ta-add-row-di-period", addDiPeriod.Get(), (value) => {
              addDiPeriod.Set(value);
            }), field_1("ADX period", "ta-add-row-adx-period", addAdxPeriod.Get(), (value) => {
              addAdxPeriod.Set(value);
            })]);
          case"Macd":
            return Doc.Element("div", [Attr.Create("style", "display:grid; grid-template-columns:repeat(3,1fr); gap:6px; min-width:0;")], [field_1("Fast", "ta-add-row-fast-period", addFastPeriod.Get(), (value) => {
              addFastPeriod.Set(value);
            }), field_1("Slow", "ta-add-row-slow-period", addSlowPeriod.Get(), (value) => {
              addSlowPeriod.Set(value);
            }), field_1("Signal", "ta-add-row-signal-period", addSignalPeriod.Get(), (value) => {
              addSignalPeriod.Set(value);
            })]);
          default:
            return Doc.Element("span", [Attr.Create("style", "font-size:11px; color:#728196;")], [Doc.TextNode("No indicator parameters.")]);
        }
      }, addKind.View))])], delay(() =>[Doc.Element("div", [Attr.Create("style", "display:flex; justify-content:flex-end; gap:6px;")], [compactButton("ta-add-row-cancel", "Cancel", "Close without submitting", () => {
        const _7=uiState.Get();
        let _8={
          Window:_7.Window,
          FollowLatest:_7.FollowLatest,
          HiddenRows:_7.HiddenRows,
          AddRowOpen:false,
          CursorIndex:_7.CursorIndex,
          PendingActionId:_7.PendingActionId,
          Feedback:_7.Feedback
        };
        uiState.Set(_8);
      }), primaryButtonView("ta-add-row-submit", "Add", commandsDisabledView, commandsDisabledNow, addRow)])]))))), uiState.View)), Doc.EmbedView(Map((ui) => IsNullOrWhiteSpace(ui.Feedback)?Doc.Empty:Doc.Element("div", [Attr.Create("data-testid", "ta-feedback"), Attr.Create("style", "font-size:11px; color:#40536d; min-height:15px;")], [Doc.TextNode(ui.Feedback)]), uiState.View))]), Doc.EmbedView(Map2((_7, _8) => {
        let cursorIndex, cursorValues;
        chartRenderSequence=chartRenderSequence+1;
        const visibleRows=filter_1((row) => row.Visible&&!_8.HiddenRows.Contains(row.RowId), document.Rows);
        const referenceTimeline_1=referenceTimeline(visibleRows, _7.Data);
        const referenceLength_1=length(referenceTimeline_1);
        const o_2=tryFind((trace) => trace.Visible&&Equals(trace.Kind, {$:0}), collect(effectiveTraces, visibleRows));
        const o_3=o_2==null?null:Some(candleSeries(o_2.$0.DataRef, _7.Data));
        const overviewPoints=o_3==null?[]:o_3.$0;
        const visibleWindow=resolveWindow(options.MinimumVisibleBars, options.MaximumVisibleBars, referenceLength_1, _8.FollowLatest, _8.Window);
        const visibleTimestamps=selectWindow(visibleWindow, referenceTimeline_1);
        const o_4=_8.CursorIndex;
        if(o_4==null)cursorIndex=null;
        else {
          const value=o_4.$0;
          const a=0;
          const x=Compare(a, value)===1?a:value;
          const a_1=0;
          const b=length(visibleTimestamps)-1;
          const e=Compare(a_1, b)===1?a_1:b;
          let _9=Compare(e, x)===-1?e:x;
          cursorIndex=Some(_9);
        }
        const cursorDocument={
          WorkspaceId:document.WorkspaceId,
          Title:document.Title,
          RowsRef:document.RowsRef,
          StatusRef:document.StatusRef,
          SharedTimeAxis:document.SharedTimeAxis,
          Rows:visibleRows,
          AllowedActions:document.AllowedActions,
          DefaultView:document.DefaultView
        };
        const cursor=cursorIndex==null?null:cursorSnapshot(cursorDocument, _7.Data, visibleWindow, cursorIndex.$0);
        if(cursor!=null&&cursor.$==1){
          const value_1=cursor.$0;
          cursorValues=Doc.Element("div", [Attr.Create("data-testid", "ta-cursor-values"), Attr.Create("style", "display:flex; align-items:center; gap:4px 12px; min-width:0; flex-wrap:wrap; white-space:normal; overflow-wrap:anywhere; font-family:Consolas, monospace; font-size:11px; line-height:16px; color:#263b55;")], ofSeq_1(delay(() => append_1([Doc.Element("strong", [Attr.Create("style", "white-space:nowrap;")], [Doc.TextNode(compactTimestamp(value_1.Timestamp))])], delay(() => map_2((item) => Doc.Element("span", [Attr.Create("data-cursor-row", item.Label), Attr.Create("style", "min-width:0;")], [Doc.TextNode(item.Label+" "+item.Value)]), value_1.Values))))));
        }
        else cursorValues=Doc.Element("div", [Attr.Create("style", "font-size:11px; color:#718197;")], [Doc.TextNode("Move the pointer over any chart row to inspect one shared bar.")]);
        const visibleStart=visibleWindow.Count===0?0:visibleWindow.StartIndex+1;
        const visibleEnd=visibleWindow.StartIndex+visibleWindow.Count;
        const viewportRangeText=Map((draft) => {
          if(draft!=null&&draft.$==1){
            const preview=draft.$0;
            return"Loaded "+String(referenceLength_1)+" bars · Preview "+String(preview.Count===0?0:preview.StartIndex+1)+"-"+String(preview.StartIndex+preview.Count)+" · release to render";
          }
          else return"Loaded "+String(referenceLength_1)+" bars · Viewing "+String(visibleStart)+"-"+String(visibleEnd);
        }, draftWindow.View);
        let _10=Attr.Create("data-testid", "ta-chart-stack");
        let _11=Attr.Create("data-chart-render-sequence", String(chartRenderSequence));
        let _12=Attr.Create("data-loaded-bars", String(referenceLength_1));
        let _13=Attr.Create("data-visible-start", String(visibleStart));
        let _14=Attr.Create("data-visible-end", String(visibleEnd));
        let _15=Attr.Create("data-follow-latest", _8.FollowLatest?"true":"false");
        const o_5=cursorIndex==null?null:Some(String(cursorIndex.$0));
        let _16=o_5==null?"":o_5.$0;
        let _17=Attr.Create("data-cursor-index", _16);
        let _18=[_10, _11, _12, _13, _14, _15, _17, Attr.Create("style", "display:flex; flex-direction:column; min-width:0; padding:0 12px 14px;")];
        return Doc.Element("div", _18, ofSeq_1(delay(() => append_1([Doc.Element("div", [Attr.Create("data-testid", "ta-cursor-panel"), Attr.Create("style", "order:-2; display:flex; flex-direction:column; gap:5px; align-items:stretch; min-height:34px; padding:6px 8px; border-bottom:1px solid #dce4ef; background:#f8fafc;")], ofSeq_1(delay(() =>[cursorValues])))], delay(() => append_1(length(visibleRows)===0?[Doc.Element("div", [Attr.Create("style", "padding:18px; color:#667891;")], [Doc.TextNode("No visible TA rows.")])]:map_2((index) => renderRow(_7, {
          Window:_8.Window,
          FollowLatest:_8.FollowLatest,
          HiddenRows:_8.HiddenRows,
          AddRowOpen:_8.AddRowOpen,
          CursorIndex:cursorIndex,
          PendingActionId:_8.PendingActionId,
          Feedback:_8.Feedback
        }, visibleTimestamps, setCursorIndex, index===length(visibleRows)-1, get(visibleRows, index)), range(0, length(visibleRows)-1)), delay(() =>[Doc.Element("div", [Attr.Create("data-testid", "ta-viewport-panel"), Attr.Create("style", "order:-1; display:grid; grid-template-columns:minmax(220px,1fr) auto; gap:6px 10px; align-items:center; padding:8px; border-bottom:1px solid #d4deea; background:#f8fafc;")], [Doc.Element("span", [Attr.Create("data-testid", "ta-viewport-range"), Attr.Create("style", "font-family:Consolas,monospace; font-size:11px; color:#344a65; white-space:nowrap;")], [Doc.TextView(viewportRangeText)]), Doc.Element("div", [Attr.Create("data-testid", "ta-viewport-presets"), Attr.Create("style", "display:flex; gap:4px; align-items:center;")], [compactButton("ta-view-48", "48", "Show latest 48 bars", () => {
          setWindowCount(48);
        }), compactButton("ta-view-200", "200", "Show latest 200 bars", () => {
          setWindowCount(200);
        }), compactButton("ta-view-all", "All", "Show the complete loaded range", () => {
          setWindowCount(referenceLength_1);
        })]), Doc.Element("div", [Attr.Create("style", "grid-column:1 / -1; min-width:0;")], [Doc.EmbedView(Map((draft) => {
          const ratios=selectionRatios(referenceLength_1, draft==null?visibleWindow:draft.$0);
          return overviewSvg(overviewPoints, ratios[0], ratios[1], (node) => {
            navigatorElement=node;
          }, (drag, event) => {
            let moveHandler, upHandler;
            if(!(navigatorElement==null)){
              event.preventDefault();
              event.stopPropagation();
              const bounds=navigatorElement.getBoundingClientRect();
              const total=referenceLength();
              const committed=resolvedWindow(uiState.Get());
              const startClientX=event.clientX;
              moveHandler=null;
              upHandler=null;
              moveHandler=(rawEvent) => draftWindow.Set(Some(previewWindowBounds(options.MinimumVisibleBars, options.MaximumVisibleBars, total, committed, drag, bounds.width<=0||total<=0?0:toInt(Math.round((rawEvent.clientX-startClientX)/bounds.width*total)))));
              upHandler=() => {
                const x_1=draftWindow.Get();
                let _19=x_1==null?committed:x_1.$0;
                const p=commitWindowBounds(options.MinimumVisibleBars, options.MaximumVisibleBars, total, _19);
                const next=p[1];
                const followLatest=p[0];
                if(!(moveHandler==null))globalThis.document.removeEventListener("mousemove", moveHandler);
                if(!(upHandler==null))globalThis.document.removeEventListener("mouseup", upHandler);
                return!Equals(next, committed)||followLatest!=uiState.Get().FollowLatest?setWindow(followLatest, next):draftWindow.Set(null);
              };
              globalThis.document.addEventListener("mousemove", moveHandler);
              return globalThis.document.addEventListener("mouseup", upHandler);
            }
            else return null;
          });
        }, draftWindow.View))])])])))))));
      }, chartRuntimeView, uiState.View))]);
    }
    else {
      const pending=workspaceBootstrapPresentation(state);
      return Doc.Element("div", [Attr.Create("data-testid", "ta-workspace-bootstrap"), Attr.Create("data-state", pending.State), Attr.Create("style", "display:flex; flex-direction:column; gap:4px; padding:18px; color:"+(pending.IsError?"#9a2f2f":"#5d6d83")+";")], [Doc.Element("strong", [], [Doc.TextNode(pending.Title)]), Doc.Element("span", [Attr.Create("style", "font-size:12px;")], [Doc.TextNode(pending.Detail)])]);
    }
  }, runtimeState.View))]);
}
function defaultOptions(){
  return _c_5.defaultOptions;
}
function remoteDisabled(a){
  return a.$==3||(a.$==6||(a.$==0||a.$==7));
}
function submit(callbacks, uiState, actualDocumentRevision, request, successText, onAccepted){
  const m=request.ExpectedDocumentRevision;
  const expectedRevisionMatches=m==null||m.$0===actualDocumentRevision;
  if(uiState.Get().PendingActionId!=null){
    const _1=uiState.Get();
    let _2={
      Window:_1.Window,
      FollowLatest:_1.FollowLatest,
      HiddenRows:_1.HiddenRows,
      AddRowOpen:_1.AddRowOpen,
      CursorIndex:_1.CursorIndex,
      PendingActionId:_1.PendingActionId,
      Feedback:"action-in-flight: wait for the pending action result."
    };
    uiState.Set(_2);
  }
  else if(!expectedRevisionMatches){
    const _3=uiState.Get();
    let _4={
      Window:_3.Window,
      FollowLatest:_3.FollowLatest,
      HiddenRows:_3.HiddenRows,
      AddRowOpen:_3.AddRowOpen,
      CursorIndex:_3.CursorIndex,
      PendingActionId:null,
      Feedback:"revision-conflict: workspace is at revision "+String(actualDocumentRevision)+"."
    };
    uiState.Set(_4);
  }
  else {
    const _5=uiState.Get();
    let _6={
      Window:_5.Window,
      FollowLatest:_5.FollowLatest,
      HiddenRows:_5.HiddenRows,
      AddRowOpen:_5.AddRowOpen,
      CursorIndex:_5.CursorIndex,
      PendingActionId:Some(request.RequestId),
      Feedback:"Submitting "+request.RequestId+"..."
    };
    uiState.Set(_6);
    StartImmediate(Delay(() => Bind_1(callbacks.SubmitAction(request), (a) => {
      let result;
      if(a.$==1){
        const error=a.$0;
        result={
          $:1,
          $0:request.RequestId,
          $1:error.Code,
          $2:error.Message
        };
      }
      else result=a.$0;
      if((result.$==1?result.$0:result.$==2?result.$0:result.$0)!=request.RequestId){
        const _7=uiState.Get();
        let _8={
          Window:_7.Window,
          FollowLatest:_7.FollowLatest,
          HiddenRows:_7.HiddenRows,
          AddRowOpen:_7.AddRowOpen,
          CursorIndex:_7.CursorIndex,
          PendingActionId:null,
          Feedback:"action-correlation-mismatch: result does not match the pending request."
        };
        uiState.Set(_8);
        return Zero();
      }
      else if(result.$==1){
        const message=result.$2;
        const code=result.$1;
        const _9=uiState.Get();
        let _10={
          Window:_9.Window,
          FollowLatest:_9.FollowLatest,
          HiddenRows:_9.HiddenRows,
          AddRowOpen:_9.AddRowOpen,
          CursorIndex:_9.CursorIndex,
          PendingActionId:null,
          Feedback:code+": "+message
        };
        uiState.Set(_10);
        return Zero();
      }
      else if(result.$==2){
        const actualRevision=result.$1;
        const _11=uiState.Get();
        let _12={
          Window:_11.Window,
          FollowLatest:_11.FollowLatest,
          HiddenRows:_11.HiddenRows,
          AddRowOpen:_11.AddRowOpen,
          CursorIndex:_11.CursorIndex,
          PendingActionId:null,
          Feedback:"revision-conflict: workspace is at revision "+String(actualRevision)+"."
        };
        uiState.Set(_12);
        return Zero();
      }
      else {
        const revision=result.$1;
        onAccepted();
        const _13=uiState.Get();
        let _14={
          Window:_13.Window,
          FollowLatest:_13.FollowLatest,
          HiddenRows:_13.HiddenRows,
          AddRowOpen:_13.AddRowOpen,
          CursorIndex:_13.CursorIndex,
          PendingActionId:null,
          Feedback:successText+" Revision "+String(revision)+"."
        };
        uiState.Set(_14);
        return Zero();
      }
    })), null);
  }
}
function canvasIdText(a){
  return a.$0;
}
function compactButton(testId, label, titleText, onClick){
  return Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("data-testid", testId), Attr.Create("title", titleText), Attr.Create("style", "height:30px; border:1px solid #9fb0c6; border-radius:4px; background:#f8fafc; color:#20344f; padding:3px 9px; font-size:12px; cursor:pointer; white-space:nowrap;"), Handler("click", () =>() => onClick())], [Doc.TextNode(label)]);
}
function freshnessClass(freshness){
  return freshness.$==1?"delayed":freshness.$==3?"delayed":freshness.$==2?"stale":freshness.$==4?"stale":"live";
}
function pollText(a){
  return a.$==1?"MOUNTED":a.$==2?"READY":a.$==3?"UPDATING":a.$==5?"SUSPENDED":a.$==6?"RESYNC":a.$==4?"BACKOFF":a.$==7?"DISPOSED":"UNMOUNTED";
}
function inputText(testId, placeholder, initial_1, onChanged){
  return element_1("input", [Attr.Create("data-testid", testId), Attr.Create("type", "text"), Attr.Create("placeholder", placeholder), Attr.Create("value", initial_1), Attr.Create("style", "height:30px; min-width:0; width:100%; border:1px solid #b9c6d8; border-radius:4px; background:#fff; color:#142033; padding:4px 7px; box-sizing:border-box; font-size:12px;"), OnAfterRender((node) => {
    const input_1=node;
    input_1.addEventListener("input", () => onChanged(input_1.value));
  })], []);
}
function selectInput(testId, initial_1, values, onChanged){
  return element_1("select", [Attr.Create("data-testid", testId), Attr.Create("style", "height:30px; min-width:0; width:100%; border:1px solid #b9c6d8; border-radius:4px; background:#fff; color:#142033; padding:3px 6px; box-sizing:border-box; font-size:12px;"), OnAfterRender((node) => {
    const input_1=node;
    input_1.value=initial_1;
    input_1.addEventListener("change", () => onChanged(input_1.value));
  })], ofSeq_1(delay(() => collect_1((m) =>[element_1("option", [Attr.Create("value", m[0])], [Doc.TextNode(m[1])])], values))));
}
function primaryButtonView(testId, label, disabled, isDisabled, onClick){
  return Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("data-testid", testId), DynamicBool("disabled", disabled), Dynamic_1("style", Map((value) => value?"height:30px; border:1px solid #9aa8b8; border-radius:4px; background:#d8e0e8; color:#667587; padding:3px 11px; font-size:12px; cursor:not-allowed; white-space:nowrap;":"height:30px; border:1px solid #0f766e; border-radius:4px; background:#0f766e; color:#fff; padding:3px 11px; font-size:12px; cursor:pointer; white-space:nowrap;", disabled)), Handler("click", () =>() =>!isDisabled()?onClick():null)], [Doc.TextNode(label)]);
}
function compactRemoteButton(testId, label, titleText, disabled, isDisabled, onClick){
  return Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("data-testid", testId), Attr.Create("title", titleText), DynamicBool("disabled", disabled), Dynamic_1("style", Map((value) => value?"height:30px; border:1px solid #c8d2df; border-radius:4px; background:#edf1f5; color:#8b98a8; padding:3px 9px; font-size:12px; cursor:not-allowed; white-space:nowrap;":"height:30px; border:1px solid #9fb0c6; border-radius:4px; background:#f8fafc; color:#20344f; padding:3px 9px; font-size:12px; cursor:pointer; white-space:nowrap;", disabled)), Handler("click", () =>() =>!isDisabled()?onClick():null)], [Doc.TextNode(label)]);
}
function rowKindText_1(a){
  return a.$==1?"Volume":a.$==2?"SMA":a.$==3?"DMI":a.$==4?"ADX":a.$==5?"MACD":a.$==6?"Heikin-Ashi":"Candlestick";
}
function renderRow(state, ui, visibleTimestamps, setCursorIndex, showSharedTimeAxis, row){
  let title;
  const traces=filter_1((a) => a.Visible, effectiveTraces(row));
  const p=compositeSvg(row.RowId, traces, state.Data, visibleTimestamps, ui.CursorIndex, setCursorIndex);
  const chart=p[0];
  if(row.Traces==null||length(row.Traces)===0)title=rowKindText_1(row.Kind);
  else {
    const value=concat_1(" / ", map((trace) => IsNullOrWhiteSpace(trace.Label)?trace.TraceId:trace.Label, traces));
    title=IsNullOrWhiteSpace(value)?rowKindText_1(row.Kind):value;
  }
  const chartHeight=exists((trace) => Equals(trace.Kind, {$:0}), traces)?262:124;
  const children=showSharedTimeAxis?ofArray([chart, timeAxis("ta-time-axis-shared", p[1])]):ofArray([chart]);
  return chartFrame(title, ofArray(map((value_1) => {
    const o=value_1.AvailableAtUtc;
    const o_1=o==null?null:Some(compactTimestamp(o.$0));
    const availability=o_1==null?"unknown":o_1.$0;
    const o_2=value_1.Quality;
    const quality=o_2==null?"unknown":o_2.$0;
    return Doc.Element("span", [Attr.Create("data-testid", "ta-row-meta-"+row.RowId+"-"+value_1.ScaleKey), Attr.Create("data-scale-key", value_1.ScaleKey), Attr.Create("data-finality", value_1.Finality), Attr.Create("data-quality", quality), Attr.Create("title", temporalDetail(value_1)), Attr.Create("style", "display:inline-flex; align-items:center; min-height:20px; padding:1px 6px; border:1px solid #bcc9d8; border-radius:4px; background:#f7fafc; color:#465b74; font-family:Consolas,monospace; font-size:10px; white-space:nowrap;")], [Doc.TextNode(value_1.ScaleKey+" | "+value_1.Finality+" | "+quality+" | frontier "+compactTimestamp(value_1.ObservedThroughUtc)+" | available "+availability)]);
  }, rowTemporalMetadata(row, state.Data))), "ta-row-"+row.RowId, chartHeight+(showSharedTimeAxis?16:0), children);
}
function overviewSvg(points, s, s_1, onReady, onDragStart){
  const width=1000;
  const sampled=sampleEvenly(280, points);
  const p=paddedRange(0, 1, collect((point) =>[point.Low, point.High], sampled));
  const low=p[0];
  const high=p[1];
  const closePath=concat_1(" ", mapi((_1, _2) =>(_1===0?"M ":"L ")+fixedText(length(sampled)<=1?width/2:width*_1/(length(sampled)-1))+" "+fixedText(normalize(low, high, 8, 62, _2.Close)), sampled));
  const selectionX=s*width;
  const a=4;
  const b=(s_1-s)*width;
  const selectionWidth=Compare(a, b)===1?a:b;
  const handleWidth=8;
  const handleX=(edge) => {
    const a_1=0;
    const a_2=width-handleWidth;
    const b_1=edge-handleWidth/2;
    const b_2=Compare(a_2, b_1)===-1?a_2:b_1;
    return Compare(a_1, b_2)===1?a_1:b_2;
  };
  return svgElement("svg", [Attr.Create("data-testid", "ta-overview-navigator"), Attr.Create("data-loaded-sample-count", String(length(sampled))), svgAttr("viewBox", "0 0 1000 82"), svgAttr("preserveAspectRatio", "none"), Attr.Create("style", "display:block; width:100%; height:82px; min-width:0; background:#eef3f8; border:1px solid #c7d3e2; border-radius:4px; box-sizing:border-box; touch-action:none;"), OnAfterRender(onReady)], [svgElement("path", [svgAttr("d", closePath), svgAttr("fill", "none"), svgAttr("stroke", "#3d718e"), svgAttr("stroke-width", "1.5")], []), svgElement("rect", [Attr.Create("data-testid", "ta-overview-selection"), svgAttr("x", fixedText(selectionX)), svgAttr("y", "1"), svgAttr("width", fixedText(selectionWidth)), svgAttr("height", "80"), svgAttr("fill", "rgba(15,118,110,.10)"), svgAttr("stroke", "#0f766e"), svgAttr("stroke-width", "2"), svgAttr("style", "cursor:grab;"), Handler("mousedown", () =>(event) => onDragStart("move", event))], []), svgElement("rect", [Attr.Create("data-testid", "ta-overview-left-handle"), svgAttr("x", fixedText(handleX(selectionX))), svgAttr("y", "0"), svgAttr("width", fixedText(handleWidth)), svgAttr("height", "82"), svgAttr("fill", "#155f73"), svgAttr("fill-opacity", "0.82"), svgAttr("style", "cursor:ew-resize;"), Handler("mousedown", () =>(event) => onDragStart("resize-left", event))], []), svgElement("rect", [Attr.Create("data-testid", "ta-overview-right-handle"), svgAttr("x", fixedText(handleX(selectionX+selectionWidth))), svgAttr("y", "0"), svgAttr("width", fixedText(handleWidth)), svgAttr("height", "82"), svgAttr("fill", "#155f73"), svgAttr("fill-opacity", "0.82"), svgAttr("style", "cursor:ew-resize;"), Handler("mousedown", () =>(event) => onDragStart("resize-right", event))], [])]);
}
function compactTimestamp(value){
  return IsNullOrWhiteSpace(value)?"":value.length>=16&&value[4]==="-"&&value[7]==="-"&&(value[10]==="T"||value[10]===" ")?Substring(value, 5, 5)+" "+Substring(value, 11, 5):value;
}
function element_1(name, attrs, children){
  return Doc.Element(name, attrs, children);
}
function fixedText(value){
  return String(value);
}
function compositeSvg(rowId, traces, data, referenceTimestamps, cursorIndex, setCursorIndex){
  const width=1000;
  const hasCandles=exists((trace) => Equals(trace.Kind, {$:0}), traces);
  const top=10;
  const plotHeight=hasCandles?214:82;
  const palette=["#2764b0", "#9b5b24", "#6a4ca3", "#0f766e", "#b45309", "#be185d", "#475569", "#0891b2"];
  const color=(index, trace) => IsNullOrWhiteSpace(trace.Color)?get(palette, index%length(palette)):trace.Color;
  const candleSeries_1=collect((_11) => {
    const traceIndex=_11[0];
    const trace=_11[1];
    return map((point) =>[traceIndex, trace, point], filter_1((point) => candleSlotRange(referenceTimestamps, point)!=null, candleSeries(trace.DataRef, data)));
  }, filter_1((_11) => Equals(_11[1].Kind, {$:0}), mapi((_11, _12) =>[_11, _12], traces)));
  const linePoints=mapi((_11, _12) => {
    let _13;
    const m=_12.Kind;
    switch(m.$==1?0:m.$==2?1:m.$==3?1:2){
      case 0:
        _13=map((point) =>({
          Timestamp:point.Timestamp,
          Value:point.Volume,
          Temporal:point.Temporal
        }), candleSeries(_12.DataRef, data));
        break;
      case 1:
        _13=lineSeries(_12.DataRef, data);
        break;
      case 2:
        _13=[];
        break;
    }
    let _14=projectedLinePoints(referenceTimestamps, _13);
    return[_11, _12, _14];
  }, traces);
  const p=paddedRange(0, 1, ofSeq(delay(() => append_1(collect((_11) => {
    const point=_11[2];
    return[point.Low, point.High];
  }, candleSeries_1), delay(() => collect((_11) => {
    const values=map((_12) => _12[1].Value, _11[2]);
    return Equals(_11[1].Kind, {$:3})?[0].concat(values):values;
  }, linePoints))))));
  const low=p[0];
  const high=p[1];
  const slot=length(referenceTimestamps)===0?width:width/length(referenceTimestamps);
  const svgTestId=hasCandles?"ta-candle-"+rowId:"ta-composite-"+rowId;
  let _1=svgAttr("viewBox", hasCandles?"0 0 1000 250":"0 0 1000 112");
  let _2=svgAttr("preserveAspectRatio", "none");
  let _3=svgAttr("role", "img");
  let _4=svgAttr("aria-label", "Composite TA row "+rowId);
  let _5=Attr.Create("data-testid", svgTestId);
  let _6=Attr.Create("data-point-count", String(length(referenceTimestamps)));
  const o=cursorIndex==null?null:Some(String(cursorIndex.$0));
  let _7=o==null?"":o.$0;
  let _8=Attr.Create("data-cursor-index", _7);
  let _9=[_1, _2, _3, _4, _5, _6, _8, Attr.Create("style", "display:block; width:100%; height:"+fixedText(hasCandles?250:112)+"px; background:#fbfcfe;"), Handler("mousemove", (element_2) =>(event) => {
    const bounds=element_2.getBoundingClientRect();
    const m=cursorIndexFromClientX(length(referenceTimestamps), bounds.left, bounds.width, event.clientX);
    return m==null?null:setCursorIndex(Some(m.$0));
  })];
  let _10=svgElement("svg", _9, ofSeq_1(delay(() => append_1(collect_1((gridIndex) => {
    const y=top+plotHeight*gridIndex/4;
    return[svgElement("line", [svgAttr("x1", "0"), svgAttr("x2", "1000"), svgAttr("y1", fixedText(y)), svgAttr("y2", fixedText(y)), svgAttr("stroke", "#e7ecf3"), svgAttr("stroke-width", "1")], [])];
  }, range(0, 4)), delay(() => append_1(collect_1((m) => {
    const trace=m[1];
    const point=m[2];
    const m_1=candleSlotRange(referenceTimestamps, point);
    if(m_1!=null&&m_1.$==1){
      const firstSlot=m_1.$0[0];
      const endExclusive=m_1.$0[1];
      const left=slot*firstSlot;
      const right=slot*endExclusive;
      const center=(left+right)/2;
      const a=2;
      const b=(right-left)*0.72;
      const bodyWidth=Compare(a, b)===1?a:b;
      const candleColor=point.Close>=point.Open?"#0f8a78":"#c2414b";
      const highY=normalize(low, high, top, plotHeight, point.High);
      const lowY=normalize(low, high, top, plotHeight, point.Low);
      const openY=normalize(low, high, top, plotHeight, point.Open);
      const closeY=normalize(low, high, top, plotHeight, point.Close);
      const o_1=point.Temporal;
      const o_2=o_1==null?null:Some(o_1.$0.SourceIntervalId);
      const sourceIntervalId=o_2==null?point.Timestamp:o_2.$0;
      const spanCount=endExclusive-firstSlot;
      const traceColor=spanCount>1?color(m[0], trace):candleColor;
      const traceTestId="ta-candle-"+rowId+"-"+trace.TraceId;
      return append_1([svgElement("line", ofSeq_1(delay(() => append_1([Attr.Create("data-testid", traceTestId)], delay(() => append_1([Attr.Create("data-candle-part", "wick")], delay(() => append_1([Attr.Create("data-source-interval-id", sourceIntervalId)], delay(() => append_1([Attr.Create("data-span-slots", String(spanCount))], delay(() => append_1([svgAttr("x1", fixedText(center))], delay(() => append_1([svgAttr("x2", fixedText(center))], delay(() => append_1([svgAttr("y1", fixedText(highY))], delay(() => append_1([svgAttr("y2", fixedText(lowY))], delay(() => append_1([svgAttr("stroke", traceColor)], delay(() => append_1([svgAttr("stroke-width", spanCount>1?"1.8":"1.2")], delay(() => spanCount>1?[svgAttr("stroke-dasharray", "4 2")]:[])))))))))))))))))))))), [])], delay(() => {
        let _11=Attr.Create("data-testid", traceTestId);
        let _12=Attr.Create("data-candle-part", "body");
        let _13=Attr.Create("data-source-interval-id", sourceIntervalId);
        let _14=Attr.Create("data-span-slots", String(spanCount));
        let _15=svgAttr("x", fixedText(center-bodyWidth/2));
        let _16=svgAttr("y", fixedText(Compare(openY, closeY)===-1?openY:closeY));
        let _17=svgAttr("width", fixedText(bodyWidth));
        const a_1=1.2;
        const b_1=Math.abs(closeY-openY);
        let _18=Compare(a_1, b_1)===1?a_1:b_1;
        let _19=fixedText(_18);
        let _20=svgAttr("height", _19);
        let _21=[_11, _12, _13, _14, _15, _16, _17, _20, svgAttr("fill", spanCount>1?"none":candleColor), svgAttr("stroke", traceColor), svgAttr("stroke-width", spanCount>1?"1.8":"0"), svgAttr("rx", "0.6")];
        return[svgElement("rect", _21, [])];
      }));
    }
    else return[];
  }, candleSeries_1), delay(() => append_1(collect_1((m) => {
    const trace=m[1];
    const points=m[2];
    const traceColor=color(m[0], trace);
    const m_1=trace.Kind;
    switch(m_1.$==3?0:m_1.$==1?0:m_1.$==2?1:2){
      case 0:
        const zeroY=normalize(low, high, top, plotHeight, 0);
        const a=1;
        const b=slot*0.64;
        const barWidth=Compare(a, b)===1?a:b;
        const path=concat_1(" ", map((_11) => {
          const valueY=normalize(low, high, top, plotHeight, _11[1].Value);
          let _12=Compare(zeroY, valueY)===-1?zeroY:valueY;
          const a_1=1;
          const b_1=Math.abs(zeroY-valueY);
          let _13=Compare(a_1, b_1)===1?a_1:b_1;
          return rectanglePath(slot*(_11[0]+0.18), _12, barWidth, _13);
        }, points));
        return[svgElement("path", [Attr.Create("data-testid", "ta-trace-"+rowId+"-"+trace.TraceId), svgAttr("d", path), svgAttr("fill", traceColor), svgAttr("fill-opacity", "0.62")], [])];
      case 1:
        const path_1=concat_1(" ", mapi((_11, _12) =>(_11===0?"M":"L")+" "+fixedText(_12[0])+" "+fixedText(_12[1]), map((_11) => {
          const o_1=slotCenter(width, length(referenceTimestamps), _11[0]);
          let _12=o_1==null?width/2:o_1.$0;
          return[_12, normalize(low, high, top, plotHeight, _11[1].Value)];
        }, points)));
        return[svgElement("path", [Attr.Create("data-testid", "ta-trace-"+rowId+"-"+trace.TraceId), svgAttr("d", path_1), svgAttr("fill", "none"), svgAttr("stroke", traceColor), svgAttr("stroke-width", fixedText(trace.Width)), svgAttr("stroke-linejoin", "round"), svgAttr("stroke-linecap", "round")], [])];
      case 2:
        return[];
    }
  }, linePoints), delay(() => {
    const m=cursorPosition(width, length(referenceTimestamps), cursorIndex);
    if(m==null)return[];
    else {
      const x=m.$0;
      return[svgElement("line", [Attr.Create("data-testid", svgTestId+"-crosshair"), svgAttr("x1", fixedText(x)), svgAttr("x2", fixedText(x)), svgAttr("y1", "0"), svgAttr("y2", fixedText(plotHeight)), svgAttr("stroke", "#1f4f73"), svgAttr("stroke-width", "1"), svgAttr("stroke-dasharray", "3 3")], [])];
    }
  })))))))));
  return[_10, referenceTimestamps];
}
function chartFrame(titleText, metadata, testId, height, children){
  return Doc.Element("section", [Attr.Create("data-testid", testId), Attr.Create("style", "display:flex; flex-direction:column; min-width:0; min-height:"+String(height)+"px; border-top:1px solid #e1e7ef; background:#fff;")], [Doc.Element("div", [Attr.Create("style", "display:flex; align-items:center; gap:6px 10px; min-height:28px; padding:4px 8px; color:#40536d; font-size:11px; flex-wrap:wrap;")], ofSeq_1(delay(() => append_1([Doc.Element("strong", [Attr.Create("style", "margin-right:auto;")], [Doc.TextNode(titleText)])], delay(() => metadata))))), element_1("div", [Attr.Create("style", "min-width:0; overflow:hidden;")], children)]);
}
function timeAxis(testId, timestamps){
  const labels=mapi((_1, _2) => Doc.Element("span", [Attr.Create("style", "min-width:0; text-align:"+(_1===0?"left":_1===2?"right":"center")+"; color:#708198; font-size:10px; line-height:16px; white-space:nowrap; overflow:hidden; text-overflow:ellipsis;")], [Doc.TextNode(compactTimestamp(_2[1]))]), timeLabels(timestamps));
  return Doc.Element("div", [Attr.Create("data-testid", testId), Attr.Create("style", "display:grid; grid-template-columns:1fr 1fr 1fr; min-width:0; height:16px; padding:0 1px;")], labels);
}
function svgElement(name, attrs, children){
  return Doc.SvgElement(name, attrs, children);
}
function svgAttr(name, value){
  return Attr.Create(name, value);
}
function rectanglePath(x, y, width, height){
  return"M "+fixedText(x)+" "+fixedText(y)+" h "+fixedText(width)+" v "+fixedText(height)+" h "+fixedText(-width)+" Z";
}
function cursorPosition(width, pointCount, cursorIndex){
  return cursorIndex==null?null:slotCenter(width, pointCount, cursorIndex.$0);
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
function Map2_1(fn, sn1, sn2){
  const _1=sn1.s;
  const _2=sn2.s;
  if(_1!=null&&_1.$==0)return _2!=null&&_2.$==0?{s:Forever(fn(_1.$0, _2.$0))}:Map2Opt1(fn, _1.$0, sn2);
  else if(_2!=null&&_2.$==0)return Map2Opt2(fn, _2.$0, sn1);
  else {
    const res={s:Waiting([], [])};
    const cont=() => {
      const m=res.s;
      if(!(m!=null&&m.$==0||m!=null&&m.$==2)){
        const _3=ValueAndForever(sn1);
        const _4=ValueAndForever(sn2);
        if(_3!=null&&_3.$==1)if(_4!=null&&_4.$==1)if(_3.$0[1]&&_4.$0[1])MarkForever(res, fn(_3.$0[0], _4.$0[0]));
        else MarkReady(res, fn(_3.$0[0], _4.$0[0]));
      }
    };
    When(sn1, cont, res);
    When(sn2, cont, res);
    return res;
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
function MapCachedBy_1(eq, prev, fn, sn){
  return Map_1((x) => {
    let _1;
    const m=prev[0];
    if(m!=null&&m.$==1&&(m.$0,eq(x, m.$0[0])&&(_1=[m.$0[0], m.$0[1]],true)))return _1[1];
    else {
      const y=fn(x);
      prev[0]=Some([x, y]);
      return y;
    }
  }, sn);
}
function WhenObsoleteRun(snap, obs){
  const m=snap.s;
  if(m==null)obs();
  else m!=null&&m.$==2?(m.$0,m.$1.push(obs)):m!=null&&m.$==3?(m.$0,m.$1.push(obs)):m.$0;
}
function Map2Opt1(fn, x, sn2){
  return Map_1((y) => fn(x, y), sn2);
}
function Map2Opt2(fn, y, sn1){
  return Map_1((x) => fn(x, y), sn1);
}
function ValueAndForever(snap){
  const m=snap.s;
  return m!=null&&m.$==0?Some([m.$0, true]):m!=null&&m.$==2?Some([m.$0, false]):null;
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
function WhenObsolete(snap, obs){
  const m=snap.s;
  if(m==null)Obsolete(obs);
  else m!=null&&m.$==2?(m.$0,EnqueueSafe(m.$1, obs)):m!=null&&m.$==3?(m.$0,EnqueueSafe(m.$1, obs)):m.$0;
}
function Join_1(snap){
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
class Exception extends Object_1 { }
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
    e.setAttribute("ws-on", concat_1(" ", filter_1((x) => dontRemove.Contains(get(SplitChars(x, [":"], 1), 1)), SplitChars(e.getAttribute("ws-on"), [" "], 1))));
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
    e.setAttribute("ws-on", concat_1(" ", map((x) => {
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
  if(!(events.length==0))el.setAttribute("ws-on", concat_1(" ", events));
  if(!(holedAttrs.length==0))el.setAttribute("ws-attr-holes", concat_1(" ", holedAttrs));
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
    return Get(map_2((kvp) => kvp.K, this.d));
  }
  constructor(d){
    super();
    this.d=d;
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
function New_41(PreviousNodes, Top){
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
        c.k(Ok_1(a));
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
    r(New_47((a) => {
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
    if(!ct.c)c(New_47((a) => {
      if(a.$==1)UncaughtAsyncError(a.$0);
    }, ct));
  });
}
function Return(x){
  return(c) => {
    c.k(Ok_1(x));
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
function StartImmediate(c, ctOpt){
  const d=(defCTS())[0];
  const ct=ctOpt==null?d:ctOpt.$0;
  if(!ct.c)c(New_47((a) => {
    if(a.$==1)UncaughtAsyncError(a.$0);
  }, ct));
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
    const dur=max_1(map_2((anim) => anim.Duration, xs_1));
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
class ConcreteVar extends Var {
  isConst;
  current;
  snap;
  view;
  id;
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
  get View(){
    return this.view;
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
let CancelPoll={$:6};
let CancelTimeout={$:7};
let CancelReconnect={$:8};
let SendUnmounted={$:1};
function ScheduleTimeout(delayMs){
  return{$:4, $0:delayMs};
}
let CloseTransport={$:9};
let SendMounted={$:0};
function SchedulePoll(delayMs){
  return{$:3, $0:delayMs};
}
function SendAction(Item){
  return{$:2, $0:Item};
}
function ScheduleReconnect(delayMs){
  return{$:5, $0:delayMs};
}
function New_42(wireVersion, kind, actionKind, canvasInstanceId, rowId, rowKind_1, dataRef, heightWeight, visible, sourceId, instrument, intervalMinutes, fromUtc, toUtcExclusive, includePartial, afterDataRevision, dataRevision, reasonCode, templateKey, hasTemplateRowId, editorValues, expectedDocumentRevision, hasExpectedDocumentRevision){
  return{
    wireVersion:wireVersion,
    kind:kind,
    actionKind:actionKind,
    canvasInstanceId:canvasInstanceId,
    rowId:rowId,
    rowKind:rowKind_1,
    dataRef:dataRef,
    heightWeight:heightWeight,
    visible:visible,
    sourceId:sourceId,
    instrument:instrument,
    intervalMinutes:intervalMinutes,
    fromUtc:fromUtc,
    toUtcExclusive:toUtcExclusive,
    includePartial:includePartial,
    afterDataRevision:afterDataRevision,
    dataRevision:dataRevision,
    reasonCode:reasonCode,
    templateKey:templateKey,
    hasTemplateRowId:hasTemplateRowId,
    editorValues:editorValues,
    expectedDocumentRevision:expectedDocumentRevision,
    hasExpectedDocumentRevision:hasExpectedDocumentRevision
  };
}
function Bind_2(f, r){
  return r.$==1?Error_1(r.$0):f(r.$0);
}
function Map_2(f, r){
  return r.$==1?Error_1(r.$0):Ok(f(r.$0));
}
function New_43(schema, exportedAtUtc, documentRevision, dataRevision, state){
  return{
    schema:schema,
    exportedAtUtc:exportedAtUtc,
    documentRevision:documentRevision,
    dataRevision:dataRevision,
    state:state
  };
}
function initialEditorInputs(schema){
  return collect((field_1) => {
    const m=field_1.DefaultValue;
    return m==null?fallbackInputs(field_1.Key, field_1.Kind):flattenEditorValue(field_1.Key, field_1.Kind, m.$0);
  }, schema.Fields);
}
function rowReferenceLength(row, data){
  const o=tryHead(sortDescending(map((trace) => seriesValues(trace.DataRef, data).length, filter_1((a) => a.Visible, effectiveTraces(row)))));
  return o==null?0:o.$0;
}
function resolveWindow(minimumCount, maximumCount, total, followLatest, requested){
  const bounded=clampWindow(minimumCount, maximumCount, total, requested);
  if(followLatest&&bounded.Count>0){
    const a=0;
    const b=total-bounded.Count;
    let _1=Compare(a, b)===1?a:b;
    return{StartIndex:_1, Count:bounded.Count};
  }
  else return bounded;
}
function clampWindow(minimumCount, maximumCount, total, requested){
  if(total<=0)return{StartIndex:0, Count:0};
  else {
    const upper=Compare(maximumCount, total)===-1?maximumCount:total;
    const a=Compare(minimumCount, upper)===-1?minimumCount:upper;
    const a_1=requested.Count;
    const b=Compare(a_1, upper)===-1?a_1:upper;
    const count=Compare(a, b)===1?a:b;
    const a_2=0;
    const a_3=requested.StartIndex;
    const b_1=total-count;
    const b_2=Compare(a_3, b_1)===-1?a_3:b_1;
    let _1=Compare(a_2, b_2)===1?a_2:b_2;
    return{StartIndex:_1, Count:count};
  }
}
function viewportMaximumStart(total, window_1){
  const a=0;
  const a_1=0;
  const b=window_1.Count;
  let _1=Compare(a_1, b)===1?a_1:b;
  const b_1=total-_1;
  return Compare(a, b_1)===1?a:b_1;
}
function setEditorInput(input_1, values){
  return sortBy((a) => a.Path, [input_1].concat(filter_1((current) => current.Path!=input_1.Path, values)));
}
function tryEditorInput(path, values){
  const o=tryFind((value) => value.Path==path, values);
  return o==null?null:Some(o.$0.Value);
}
function editorScalarText(a){
  return a.$==1?fixedNumber(a.$0):a.$==2?a.$0?"true":"false":a.$0;
}
function moveListItem(listPath, fromIndex, toIndex, values){
  return sortBy((a) => a.Path, map((value) => {
    const m=tryListIndex(listPath, value.Path);
    return m!=null&&m.$==1?m.$0===fromIndex?(m.$0,{Path:replaceListIndex(listPath, fromIndex, toIndex, value.Path), Value:value.Value}):m.$0===toIndex?(m.$0,{Path:replaceListIndex(listPath, toIndex, fromIndex, value.Path), Value:value.Value}):value:value;
  }, values));
}
function removeListItem(listPath, index, values){
  return sortBy((a) => a.Path, choose((value) => {
    let _1;
    const m=tryListIndex(listPath, value.Path);
    switch(m!=null&&m.$==1?m.$0===index?(_1=m.$0,0):m.$0>index?(_1=m.$0,1):2:2){
      case 0:
        return null;
      case 1:
        return Some({Path:replaceListIndex(listPath, _1, _1-1, value.Path), Value:value.Value});
      case 2:
        return Some(value);
    }
  }, values));
}
function addListItem(listPath, itemKind, values){
  const m=tryLast(listIndexes(listPath, values));
  let _1=m==null?0:m.$0+1;
  let _2=String(_1);
  let _3=String(listPath)+"["+_2;
  let _4=_3+"]";
  let _5=fallbackInputs(_4, itemKind);
  let _6=values.concat(_5);
  return sortBy((a) => a.Path, _6);
}
function listIndexes(listPath, values){
  return sort(distinct(choose((value) => tryListIndex(listPath, value.Path), values)));
}
function queryDraft(values){
  const textValue=(name) => {
    const o_5=values.TryFind(name);
    const o_6=o_5==null?null:tryText(o_5.$0);
    return o_6==null?"":o_6.$0;
  };
  const o=values.TryFind("query.intervalMinutes");
  const o_1=o==null?null:tryNumber(o.$0);
  const o_2=o_1==null?null:Some(String(toInt(o_1.$0)));
  const interval=o_2==null?"":o_2.$0;
  const o_3=values.TryFind("query.includePartial");
  const o_4=o_3==null?null:tryBool(o_3.$0);
  const includePartial=o_4==null||o_4.$0;
  return{
    SourceId:textValue("query.sourceId"),
    Instrument:textValue("query.instrument"),
    IntervalMinutes:interval,
    FromUtc:textValue("query.fromUtc"),
    ToUtcExclusive:textValue("query.toUtcExclusive"),
    IncludePartial:includePartial
  };
}
function statusPresentation(statusRef, state){
  let _1;
  const o=state.Data.TryFind(statusRef);
  const x=o==null?null:tryObject(o.$0);
  const v=new FSharpMap("New", []);
  const status=x==null?v:x.$0;
  const freshness=freshnessFromStatus(status);
  const o_1=objectText("label", status);
  let _2=o_1==null?String(freshness):o_1.$0;
  let _3=objectText("watermarkUtc", status);
  let _4=objectText("quality", status);
  const o_2=state.LastError;
  if(o_2==null)_1=null;
  else {
    const error=o_2.$0;
    _1=Some(error.ReasonCode+": "+error.Message);
  }
  return{
    Freshness:freshness,
    Label:_2,
    Watermark:_3,
    Quality:_4,
    Error:_1
  };
}
function referenceTimeline(rows, data){
  const o=tryHead(sortByDescending((_1) =>[length(_1[1]), Equals(_1[0].Kind, {$:0})], filter_1((_1) => length(_1[1])>0, map((trace) =>[trace, distinct(traceTimestamps(trace, data))], filter_1((a) => a.Visible, collect(effectiveTraces, filter_1((a) => a.Visible, rows)))))));
  const o_1=o==null?null:Some(o.$0[1]);
  return o_1==null?[]:o_1.$0;
}
function selectWindow(window_1, values){
  if(window_1.Count<=0||length(values)===0)return[];
  else {
    const a=0;
    const a_1=window_1.StartIndex;
    const b=length(values);
    const b_1=Compare(a_1, b)===-1?a_1:b;
    let _1=Compare(a, b_1)===1?a:b_1;
    let _2=skip(_1, values);
    return _2.slice(0, window_1.Count);
  }
}
function cursorSnapshot(document, data, window_1, cursorIndex){
  const visibleRows=filter_1((a_1) => a_1.Visible, document.Rows);
  const timeline=referenceTimeline(visibleRows, data);
  const visibleTimestamps=selectWindow(clampWindow(1, 2147483647, length(timeline), window_1), timeline);
  if(length(visibleTimestamps)===0)return null;
  else {
    const a=0;
    const b=length(visibleTimestamps)-1;
    const b_1=Compare(cursorIndex, b)===-1?cursorIndex:b;
    const index=Compare(a, b_1)===1?a:b_1;
    const timestamp=get(visibleTimestamps, index);
    return Some({
      VisibleIndex:index,
      Timestamp:timestamp,
      Values:map((t) => t[1], collect((row) => choose((trace) => {
        let _1, _2;
        const label=IsNullOrWhiteSpace(trace.Label)?trace.TraceId:trace.Label;
        const m=trace.Kind;
        if(m.$==1||(m.$==2?false:m.$!=3)){
          const o=tryCandleAt(timestamp, candleSeries(trace.DataRef, data));
          if(o==null)return null;
          else {
            const point=o.$0;
            const baseValue=Equals(trace.Kind, {$:1})?fixedNumber(point.Volume):"O "+fixedNumber(point.Open)+" H "+fixedNumber(point.High)+" L "+fixedNumber(point.Low)+" C "+fixedNumber(point.Close);
            const m_1=point.Temporal;
            if(m_1==null)_1=baseValue;
            else {
              const metadata=m_1.$0;
              _1=baseValue+" | "+metadata.ScaleKey+" "+metadata.Finality+" | "+metadata.SourceIntervalId;
            }
            let _3={Label:label, Value:_1};
            let _4=[point.Timestamp, _3];
            return Some(_4);
          }
        }
        else {
          const o_1=tryLineAt(timestamp, lineSeries(trace.DataRef, data));
          if(o_1==null)return null;
          else {
            const point_1=o_1.$0;
            const m_2=point_1.Temporal;
            if(m_2==null)_2=fixedNumber(point_1.Value);
            else {
              const metadata_1=m_2.$0;
              _2=fixedNumber(point_1.Value)+" | "+metadata_1.ScaleKey+" "+metadata_1.Finality+" | "+metadata_1.SourceIntervalId;
            }
            let _5={Label:label, Value:_2};
            let _6=[point_1.Timestamp, _5];
            return Some(_6);
          }
        }
      }, filter_1((a_1) => a_1.Visible, effectiveTraces(row))), visibleRows))
    });
  }
}
function selectionRatios(total, window_1){
  if(total<=0||window_1.Count<=0)return[0, 0];
  else {
    const bounded=clampWindow(1, 2147483647, total, window_1);
    return[bounded.StartIndex/total, (bounded.StartIndex+bounded.Count)/total];
  }
}
function effectiveTraces(row){
  let _1;
  if(!(row.Traces==null)&&length(row.Traces)>0)return row.Traces;
  else {
    const m=row.Kind;
    switch(m.$==0?0:m.$==6?0:m.$==1?1:2){
      case 0:
        _1={$:0};
        break;
      case 1:
        _1={$:1};
        break;
      case 2:
        _1={$:2};
        break;
    }
    return[{
      TraceId:row.RowId,
      Kind:_1,
      DataRef:row.DataRef,
      Label:row.RowId,
      Color:"",
      Width:1.25,
      Visible:true,
      Options:new FSharpMap("New", [])
    }];
  }
}
function candleSeries(dataRef, data){
  return choose(parseCandle, seriesValues(dataRef, data));
}
function workspaceBootstrapPresentation(state){
  const m=state.LastError;
  if(m==null){
    const m_1=state.Poll;
    switch(m_1.$==1?1:m_1.$==4?2:m_1.$==6?3:m_1.$==7?4:m_1.$==2?5:m_1.$==3?5:m_1.$==5?5:0){
      case 0:
        return{
          State:"preparing",
          Title:"Preparing TA workspace",
          Detail:"Waiting for the workspace channel to mount.",
          IsError:false
        };
      case 1:
        return{
          State:"connecting",
          Title:"Connecting TA workspace",
          Detail:"Waiting for the initial workspace document.",
          IsError:false
        };
      case 2:
        return{
          State:"retrying",
          Title:"Restoring TA workspace",
          Detail:"A reconnect attempt is scheduled.",
          IsError:false
        };
      case 3:
        return{
          State:"resyncing",
          Title:"Resynchronizing TA workspace",
          Detail:"Requesting a full workspace document.",
          IsError:false
        };
      case 4:
        return{
          State:"closed",
          Title:"TA workspace closed",
          Detail:"Open the page again to reconnect.",
          IsError:false
        };
      case 5:
        return{
          State:"loading",
          Title:"Loading TA workspace",
          Detail:"Waiting for the workspace document.",
          IsError:false
        };
    }
  }
  else if(!m.$0.Recoverable){
    const error=m.$0;
    return{
      State:"unavailable",
      Title:"TA workspace unavailable",
      Detail:error.ReasonCode+": "+error.Message,
      IsError:true
    };
  }
  else {
    const error_1=m.$0;
    return{
      State:"recovering",
      Title:"Restoring TA workspace",
      Detail:error_1.ReasonCode+": "+error_1.Message,
      IsError:false
    };
  }
}
function validateEditorSubmission(schema, values){
  return collect((field_1) => editorSubmissionErrors(field_1.Key, field_1.Required, field_1.Kind, values), schema.Fields);
}
function editorScalarEqualsSdui(scalar, value){
  return scalar.$==1?value.$==2&&scalar.$0===value.$0:scalar.$==2?value.$==1&&scalar.$0==value.$0:value.$==3&&scalar.$0==value.$0;
}
function previewWindowBounds(minimumCount, maximumCount, total, committed, drag, delta){
  const committed_1=clampWindow(minimumCount, maximumCount, total, committed);
  if(committed_1.Count<=0)return committed_1;
  else {
    const startIndex=committed_1.StartIndex;
    const endExclusive=startIndex+committed_1.Count;
    if(drag=="move"){
      const a=0;
      const a_1=startIndex+delta;
      const b=total-committed_1.Count;
      const b_1=Compare(a_1, b)===-1?a_1:b;
      let _1=Compare(a, b_1)===1?a:b_1;
      return{StartIndex:_1, Count:committed_1.Count};
    }
    else if(drag=="resize-left"){
      const a_2=0;
      const a_3=startIndex+delta;
      const b_2=committed_1.Count;
      let _2=Compare(minimumCount, b_2)===-1?minimumCount:b_2;
      const b_3=endExclusive-_2;
      const b_4=Compare(a_3, b_3)===-1?a_3:b_3;
      const nextStart=Compare(a_2, b_4)===1?a_2:b_4;
      return clampWindow(minimumCount, maximumCount, total, {StartIndex:nextStart, Count:endExclusive-nextStart});
    }
    else {
      const a_4=1;
      const b_5=Compare(a_4, total)===1?a_4:total;
      let _3=Compare(minimumCount, b_5)===-1?minimumCount:b_5;
      const a_5=startIndex+_3;
      const b_6=endExclusive+delta;
      const b_7=Compare(total, b_6)===-1?total:b_6;
      let _4=Compare(a_5, b_7)===1?a_5:b_7;
      let _5=_4-startIndex;
      let _6={StartIndex:startIndex, Count:_5};
      return clampWindow(minimumCount, maximumCount, total, _6);
    }
  }
}
function commitWindowBounds(minimumCount, maximumCount, total, draft){
  const next=clampWindow(minimumCount, maximumCount, total, draft);
  return[next.StartIndex===viewportMaximumStart(total, next), next];
}
function fallbackInputs(path, kind){
  let o;
  if(kind.$==1){
    const x=kind.$0;
    let _1=x==null?0n:x.$0;
    let _2=Number(_1);
    let _3={$:1, $0:_2};
    return[{Path:path, Value:_3}];
  }
  else if(kind.$==2){
    const x_1=kind.$0;
    let _4=x_1==null?0:x_1.$0;
    let _5={$:1, $0:_4};
    return[{Path:path, Value:_5}];
  }
  else if(kind.$==3)return[{Path:path, Value:{$:2, $0:false}}];
  else if(kind.$==4){
    const o_1=tryHead(kind.$0);
    if(o_1==null)o=null;
    else {
      const m=o_1.$0.Value;
      o=m.$==3?Some({$:0, $0:m.$0}):m.$==2?Some({$:1, $0:m.$0}):m.$==1?Some({$:2, $0:m.$0}):null;
    }
    const o_2=o==null?null:Some([{Path:path, Value:o.$0}]);
    return o_2==null?[]:o_2.$0;
  }
  else if(kind.$==5){
    const o_3=tryHead(kind.$0);
    const o_4=o_3==null?null:Some([{Path:path, Value:{$:0, $0:o_3.$0}}]);
    return o_4==null?[]:o_4.$0;
  }
  else if(kind.$==6){
    const minimum=kind.$1;
    const itemKind=kind.$0;
    return concat(init(minimum==null?0:minimum.$0, (index) => fallbackInputs(String(path)+"["+String(index)+"]", itemKind)));
  }
  else return kind.$==7?collect((field_1) => {
    const childPath=path+"."+field_1.Key;
    const m_1=field_1.DefaultValue;
    return m_1==null?fallbackInputs(childPath, field_1.Kind):flattenEditorValue(childPath, field_1.Kind, m_1.$0);
  }, kind.$0):[{Path:path, Value:{$:0, $0:""}}];
}
function flattenEditorValue(path, kind, value){
  let _1;
  switch(kind.$==7?value.$==5?(_1=[kind.$0, value.$0],0):value.$==3?(_1=value.$0,2):value.$==2?(_1=value.$0,3):value.$==1?(_1=value.$0,4):5:kind.$==6?value.$==4?(_1=[kind.$0, value.$0],1):value.$==3?(_1=value.$0,2):value.$==2?(_1=value.$0,3):value.$==1?(_1=value.$0,4):5:value.$==3?(_1=value.$0,2):value.$==2?(_1=value.$0,3):value.$==1?(_1=value.$0,4):5){
    case 0:
      const values=_1[1];
      return collect((field_1) => {
        const m=values.TryFind(field_1.Key);
        return m==null?[]:flattenEditorValue(path+"."+field_1.Key, field_1.Kind, m.$0);
      }, _1[0]);
    case 1:
      const itemKind=_1[0];
      return collect((_2) => flattenEditorValue(String(path)+"["+String(_2[0])+"]", itemKind, _2[1]), indexed(_1[1]));
    case 2:
      return[{Path:path, Value:{$:0, $0:_1}}];
    case 3:
      return[{Path:path, Value:{$:1, $0:_1}}];
    case 4:
      return[{Path:path, Value:{$:2, $0:_1}}];
    case 5:
      return[];
  }
}
function seriesValues(dataRef, data){
  let _1;
  const m=data.TryFind(dataRef);
  return m!=null&&m.$==1&&(m.$0.$==4&&(_1=m.$0.$0,true))?_1:[];
}
function fixedNumber(value){
  return String(value);
}
function tryListIndex(listPath, path){
  let o;
  const prefix=listPath+"[";
  if(path==null||!StartsWith(path, prefix))return null;
  else {
    const closeIndex=IndexOf(path, "]", prefix.length);
    if(closeIndex<prefix.length)return null;
    else {
      const m=(o=0,[TryParse(Substring(path, prefix.length, closeIndex-prefix.length), {get:() => o, set:(v) => {
        o=v;
      }}), o]);
      return m[0]&&m[1]>=0?Some(m[1]):null;
    }
  }
}
function replaceListIndex(listPath, oldIndex, newIndex, path){
  const oldPrefix=String(listPath)+"["+String(oldIndex)+"]";
  return StartsWith(path, oldPrefix)?String(listPath)+"["+String(newIndex)+"]"+path.substring(oldPrefix.length):path;
}
function tryText(a){
  return a.$==3?Some(a.$0):null;
}
function tryBool(a){
  return a.$==1?Some(a.$0):null;
}
function tryNumber(a){
  return a.$==2?Some(a.$0):null;
}
function tryObject(a){
  return a.$==5?Some(a.$0):null;
}
function freshnessFromStatus(status){
  const o=objectText("freshness", status);
  let _1=o==null?"unavailable":o.$0;
  const kind=_1.toLowerCase();
  const o_1=objectNumber("lagSeconds", status);
  let _2=o_1==null?0:o_1.$0;
  const lag=_2*1E3;
  const o_2=objectText("reasonCode", status);
  const reason=o_2==null?kind:o_2.$0;
  return kind=="live"?{$:0}:kind=="delayed"?{$:1, $0:lag}:kind=="stale"?{
    $:2,
    $0:lag,
    $1:reason
  }:kind=="backfill"?{$:3, $0:reason}:{$:4, $0:reason};
}
function objectText(name, value){
  const o=objectField(name, value);
  return o==null?null:tryText(o.$0);
}
function traceTimestamps(trace, data){
  const m=trace.Kind;
  return m.$==1||(m.$==2?false:m.$!=3)?map((a) => a.Timestamp, candleSeries(trace.DataRef, data)):map((a) => a.Timestamp, lineSeries(trace.DataRef, data));
}
function tryCandleAt(timestamp, values){
  return tryLast(filter_1((value) => pointMatchesTimestamp(timestamp, value.Timestamp, value.Temporal), values));
}
function lineSeries(dataRef, data){
  return choose(parseLine, seriesValues(dataRef, data));
}
function tryLineAt(timestamp, values){
  return tryLast(filter_1((value) => pointMatchesTimestamp(timestamp, value.Timestamp, value.Temporal), values));
}
function rowTemporalMetadata(row, data){
  return distinctBy((value) =>[value.ScaleKey, value.Finality, value.ObservedThroughUtc, value.Quality], choose((trace) => latestTemporalMetadata(trace, data), filter_1((a) => a.Visible, effectiveTraces(row))));
}
function temporalDetail(metadata){
  const o=metadata.Quality;
  let _1=o==null?"unknown":o.$0;
  let _2=String(_1);
  let _3=String(metadata.ScaleKey)+" | "+String(metadata.Finality)+" | quality "+_2;
  let _4=_3+" | frontier ";
  let _5=_4+String(metadata.ObservedThroughUtc);
  let _6=_5+" | available ";
  const o_1=metadata.AvailableAtUtc;
  let _7=o_1==null?"unknown":o_1.$0;
  let _8=String(_7);
  return _6+_8;
}
function sampleEvenly(maximumCount, values){
  return maximumCount<=0||length(values)===0?[]:length(values)<=maximumCount?values.slice():maximumCount===1?[get(values, length(values)-1)]:ofSeq(delay(() => collect_1((sampleIndex) =>[get(values, toInt(Math.round(sampleIndex*(length(values)-1)/(maximumCount-1))))], range(0, maximumCount-1))));
}
function paddedRange(fallbackLow, fallbackHigh, values){
  if(values.length==0)return[fallbackLow, fallbackHigh];
  else {
    const low=min(values);
    const high=max(values);
    if(low===high)return[low-1, high+1];
    else {
      const a=(high-low)*0.08;
      const b=0.0001;
      const padding=Compare(a, b)===1?a:b;
      return[low-padding, high+padding];
    }
  }
}
function normalize(low, high, top, height, value){
  return low===high?top+height/2:top+height-(value-low)/(high-low)*height;
}
function parseCandle(value){
  let _1;
  const p=pointPayload_1(value);
  const temporal=p[0];
  const o=p[1];
  const o_1=o==null?null:tryObject(o.$0);
  if(o_1==null)return null;
  else {
    const item=o_1.$0;
    const o_2=temporal==null?null:Some(temporal.$0.IntervalStartUtc);
    const _2=o_2==null?objectText("t", item):(o_2.$0,o_2);
    const _3=objectNumber("o", item);
    const _4=objectNumber("h", item);
    const _5=objectNumber("l", item);
    const _6=objectNumber("c", item);
    const _7=objectNumber("v", item);
    return _2!=null&&_2.$==1&&(_3!=null&&_3.$==1&&(_4!=null&&_4.$==1&&(_5!=null&&_5.$==1&&(_6!=null&&_6.$==1&&(_7!=null&&_7.$==1&&(_1=[_6.$0, _4.$0, _5.$0, _3.$0, _2.$0, _7.$0],true))))))?Some({
      Timestamp:_1[4],
      Open:_1[3],
      High:_1[1],
      Low:_1[2],
      Close:_1[0],
      Volume:_1[5],
      Temporal:temporal
    }):null;
  }
}
function editorSubmissionErrors(path, required, kind, values){
  let _1;
  const missing=() => required?[path+" is required."]:[];
  if(kind.$==7)return collect((field_1) => editorSubmissionErrors(path+"."+field_1.Key, field_1.Required, field_1.Kind, values), kind.$0);
  else if(kind.$==6){
    const minimum=kind.$1;
    const maximum=kind.$2;
    const itemKind=kind.$0;
    const indexes=listIndexes(path, values);
    return ofSeq(delay(() => {
      let _2;
      return append_1(minimum!=null&&minimum.$==1&&(length(indexes)<minimum.$0&&(_2=minimum.$0,true))?[String(path)+" requires at least "+String(_2)+" item(s)."]:[], delay(() => {
        let _3;
        return append_1(maximum!=null&&maximum.$==1&&(length(indexes)>maximum.$0&&(_3=maximum.$0,true))?[String(path)+" allows at most "+String(_3)+" item(s)."]:[], delay(() => collect_1((index) => editorSubmissionErrors(String(path)+"["+String(index)+"]", true, itemKind, values), indexes)));
      }));
    }));
  }
  else {
    const m=tryEditorInput(path, values);
    if(m!=null&&m.$==1){
      const scalar=m.$0;
      switch(kind.$==0?scalar.$==0?required&&IsNullOrWhiteSpace(scalar.$0)?(_1=scalar.$0,0):1:6:kind.$==3?scalar.$==2?1:6:kind.$==1?scalar.$==1?(_1=[kind.$1, kind.$0, scalar.$0],2):6:kind.$==2?scalar.$==1?(_1=[kind.$1, kind.$0, scalar.$0],3):6:kind.$==4?exists((choice) => editorScalarEqualsSdui(scalar, choice.Value), kind.$0)?(_1=kind.$0,4):6:kind.$==5?scalar.$==0?arrContains(scalar.$0, kind.$0)?(_1=[kind.$0, scalar.$0],5):6:6:6){
        case 0:
          return missing();
        case 1:
          return[];
        case 2:
          const maximum_1=_1[0];
          const minimum_1=_1[1];
          const value=_1[2];
          return ofSeq(delay(() => append_1(isNaN(value)||Math.abs(value)===Infinity||(value<0?Math.ceil(value):Math.floor(value))!==value?[path+" must be an integer."]:[], delay(() => {
            let _2;
            return append_1(minimum_1!=null&&minimum_1.$==1&&(value<Number(minimum_1.$0)&&(_2=minimum_1.$0,true))?[path+" is below its minimum."]:[], delay(() => {
              let _3;
              return maximum_1!=null&&maximum_1.$==1&&(value>Number(maximum_1.$0)&&(_3=maximum_1.$0,true))?[path+" exceeds its maximum."]:[];
            }));
          }))));
        case 3:
          const maximum_2=_1[0];
          const minimum_2=_1[1];
          const value_1=_1[2];
          return ofSeq(delay(() => append_1(isNaN(value_1)||Math.abs(value_1)===Infinity?[path+" must be finite."]:[], delay(() => {
            let _2;
            return append_1(minimum_2!=null&&minimum_2.$==1&&(value_1<minimum_2.$0&&(_2=minimum_2.$0,true))?[path+" is below its minimum."]:[], delay(() => {
              let _3;
              return maximum_2!=null&&maximum_2.$==1&&(value_1>maximum_2.$0&&(_3=maximum_2.$0,true))?[path+" exceeds its maximum."]:[];
            }));
          }))));
        case 4:
          return[];
        case 5:
          return[];
        case 6:
          return[path+" does not match its editor kind."];
      }
    }
    else return missing();
  }
}
function objectNumber(name, value){
  const o=objectField(name, value);
  return o==null?null:tryNumber(o.$0);
}
function objectField(name, value){
  return value.TryFind(name);
}
function pointMatchesTimestamp(timestamp, pointTimestamp, temporal){
  let _1, _2;
  if(temporal!=null&&temporal.$==1){
    const metadata=temporal.$0;
    _2=metadata.Projection=="repeat-across-base-buckets"||metadata.Projection=="candle-span"?(_1=temporal.$0,0):temporal.$0.Projection=="step-after-close"?(_1=temporal.$0,1):2;
  }
  else _2=2;
  switch(_2){
    case 0:
      return timestampInInterval(timestamp, _1);
    case 1:
      return availableAtOrAfter(timestamp, _1);
    case 2:
      return pointTimestamp==timestamp;
  }
}
function parseLine(value){
  let _1;
  const p=pointPayload_1(value);
  const temporal=p[0];
  const o=p[1];
  const o_1=o==null?null:tryObject(o.$0);
  if(o_1==null)return null;
  else {
    const item=o_1.$0;
    const o_2=temporal==null?null:Some(temporal.$0.IntervalStartUtc);
    const _2=o_2==null?objectText("t", item):(o_2.$0,o_2);
    const _3=objectNumber("v", item);
    return _2!=null&&_2.$==1&&(_3!=null&&_3.$==1&&(_1=[_3.$0, _2.$0],true))?Some({
      Timestamp:_1[1],
      Value:_1[0],
      Temporal:temporal
    }):null;
  }
}
function candleSlotRange(referenceTimestamps, point){
  let _1;
  const matching=choose((_4) => pointMatchesTimestamp(_4[1], point.Timestamp, point.Temporal)?Some(_4[0]):null, indexed(referenceTimestamps));
  const _2=tryHead(matching);
  const _3=tryLast(matching);
  return _2!=null&&_2.$==1&&(_3!=null&&_3.$==1&&(_1=[_2.$0, _3.$0],true))?Some([_1[0], _1[1]+1]):null;
}
function projectedLinePoints(referenceTimestamps, points){
  return choose((_1) => {
    const o=tryLineAt(_1[1], points);
    return o==null?null:Some([_1[0], o.$0]);
  }, indexed(referenceTimestamps));
}
function cursorIndexFromClientX(visibleCount, left, width, clientX){
  return width<=0?null:cursorIndexFromRatio(visibleCount, (clientX-left)/width);
}
function slotCenter(width, visibleCount, index){
  if(visibleCount<=0)return null;
  else {
    const a=0;
    const b=visibleCount-1;
    const b_1=Compare(index, b)===-1?index:b;
    let _1=Compare(a, b_1)===1?a:b_1;
    let _2=_1+0.5;
    let _3=width/visibleCount*_2;
    return Some(_3);
  }
}
function latestTemporalMetadata(trace, data){
  return tryLast(choose((x) => {
    const o=tryTemporalPoint(x);
    return o==null?null:Some(o.$0[0]);
  }, seriesValues(trace.DataRef, data)));
}
function timeLabels(timestamps){
  return length(timestamps)===0?[]:length(timestamps)===1?[[0, get(timestamps, 0)]]:map((index) =>[index, get(timestamps, index)], distinct([0, length(timestamps)/2>>0, length(timestamps)-1]));
}
function pointPayload_1(value){
  const m=tryTemporalPoint(value);
  return m==null?[null, Some(value)]:[Some(m.$0[0]), m.$0[1]];
}
function timestampInInterval(timestamp, metadata){
  return Compare(timestamp, metadata.IntervalStartUtc)>=0&&Compare(timestamp, metadata.IntervalEndUtc)<0;
}
function availableAtOrAfter(timestamp, metadata){
  const o=metadata.AvailableAtUtc;
  return o==null?false:Compare(o.$0, timestamp)<=0;
}
function cursorIndexFromRatio(visibleCount, ratio){
  if(visibleCount<=0)return null;
  else {
    const a=visibleCount-1;
    const a_1=0;
    const a_2=1;
    const b=Compare(a_2, ratio)===-1?a_2:ratio;
    let _1=Compare(a_1, b)===1?a_1:b;
    let _2=_1*visibleCount;
    let _3=Math.floor(_2);
    const b_1=toInt(_3);
    let _4=Compare(a, b_1)===-1?a:b_1;
    return Some(_4);
  }
}
function tryTemporalPoint(value){
  let _1, _2;
  const o=tryObject(value);
  if(o==null)return null;
  else {
    const fields=o.$0;
    if(!Equals(objectText("_type", fields), Some("temporal-point.v1")))return null;
    else {
      const _3=requiredObjectText("sourceIntervalId", fields);
      const _4=requiredObjectText("scaleKey", fields);
      const _5=requiredObjectText("intervalStartUtc", fields);
      const _6=requiredObjectText("intervalEndUtc", fields);
      const _7=requiredObjectText("observedThroughUtc", fields);
      const _8=requiredObjectText("finality", fields);
      const _9=requiredObjectText("projection", fields);
      if(_3!=null&&_3.$==1&&(_4!=null&&_4.$==1&&(_5!=null&&_5.$==1&&(_6!=null&&_6.$==1&&(_7!=null&&_7.$==1&&(_8!=null&&_8.$==1&&(_9!=null&&_9.$==1&&(_1=[_8.$0, _6.$0, _5.$0, _7.$0, _9.$0, _4.$0, _3.$0],true)))))))){
        let _10={
          SourceIntervalId:_1[6],
          ScaleKey:_1[5],
          IntervalStartUtc:_1[2],
          IntervalEndUtc:_1[1],
          ObservedThroughUtc:_1[3],
          AvailableAtUtc:requiredObjectText("availableAtUtc", fields),
          Finality:_1[0],
          Projection:_1[4],
          Quality:requiredObjectText("quality", fields)
        };
        const m=fields.TryFind("value");
        let _11=m==null||(m.$0.$==0||(_2=m.$0,false))?null:Some(_2);
        let _12=[_10, _11];
        return Some(_12);
      }
      else return null;
    }
  }
}
function requiredObjectText(name, value){
  return filter((x) =>!IsNullOrWhiteSpace(x), objectText(name, value));
}
let _c_5=Lazy((_i) => class $StartupCode_Renderer {
  static {
    _c_5=_i(this);
  }
  static defaultOptions;
  static {
    this.defaultOptions={
      MinimumVisibleBars:12,
      DefaultVisibleBars:48,
      MaximumVisibleBars:2000,
      EditorSchemas:[]
    };
  }
});
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
function Forever(Item){
  return{$:0, $0:Item};
}
function Ready(Item1, Item2){
  return{
    $:2,
    $0:Item1,
    $1:Item2
  };
}
function Waiting(Item1, Item2){
  return{
    $:3,
    $0:Item1,
    $1:Item2
  };
}
function New_44(Node_1, Left, Right, Height, Count){
  return{
    Node:Node_1,
    Left:Left,
    Right:Right,
    Height:Height,
    Count:Count
  };
}
function New_45(DynElem, DynFlags, DynNodes, OnAfterRender_1){
  const _1={
    DynElem:DynElem,
    DynFlags:DynFlags,
    DynNodes:DynNodes
  };
  SetOptional(_1, "OnAfterRender", OnAfterRender_1);
  return _1;
}
function Int(){
  set_counter(counter()+1);
  return counter();
}
function set_counter(_1){
  _c_10.counter=_1;
}
function counter(){
  return _c_10.counter;
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
  return _c_11.Empty;
}
function fromSeq(s){
  const a=ofSeq(map_2((_1) => Pair.New(_1[0], _1[1]), distinctBy_1((t) => t[0], rev(s))));
  sortInPlace(a);
  return Build(a, 0, a.length-1);
}
function New_46(path, kind, textValue, numberValue, boolValue){
  return{
    path:path,
    kind:kind,
    textValue:textValue,
    numberValue:numberValue,
    boolValue:boolValue
  };
}
class Random extends Object_1 {
  Next_1(maxValue){
    return maxValue<0?FailWith("'maxValue' must be greater than zero."):Math.floor(Math.random()*maxValue);
  }
}
class FSharpSet extends Object_1 {
  tree;
  Contains(v){
    return Contains(v, this.tree);
  }
  Remove_1(v){
    return new FSharpSet("New_2", Remove(v, this.tree));
  }
  Add_1(x){
    return new FSharpSet("New_2", Add(x, this.tree));
  }
  Equals(other){
    return this.Count===other.Count&&forall2_1(Equals, this, other);
  }
  GetHashCode(){
    return -1741749453+Hash(ofSeq(this));
  }
  get Count(){
    const tree=this.tree;
    return tree==null?0:tree.Count;
  }
  GetEnumerator(){
    return Get(Enumerate(false, this.tree));
  }
  CompareTo0(other){
    return compareWith(Compare, this, other);
  }
  constructor(i, _1){
    if(i=="New_2"){
      const tree=_1;
      super();
      this.tree=tree;
    }
  }
}
function TryParse_2(s, min_1, max_2, r){
  const x=+s;
  const ok=x===x-x%1&&x>=min_1&&x<=max_2;
  if(ok)r.set(x);
  return ok;
}
function TryParseBigInt(s, min_1, max_2, r){
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
    const ok=x===x-x%1n&&x>=min_1&&x<=max_2;
    if(ok)r.set(x);
    return ok;
  }
  else return false;
}
function New_47(k, ct){
  return{k:k, ct:ct};
}
function Ok_1(Item){
  return{$:0, $0:Item};
}
function No(Item){
  return{$:1, $0:Item};
}
function Cc(Item){
  return{$:2, $0:Item};
}
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
function concat_3(o){
  let r=[];
  let k;
  for(var k_1 in o)r.push.apply(r, o[k_1]);
  return r;
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
let _c_8=Lazy((_i) => class Client {
  static {
    _c_8=_i(this);
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
    }, (_1) =>(_2) => _2!=null&&_2.$==1?void(_1.checked=_2.$0):null, Map((V) => Some(V), var_1.View)];
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
      if(isBlank_1(s_8))return Some(-8640000000000000);
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
      return Some(ofSeq(delay(() => map_2((i) => files.item(i), range(0, files.length-1)))));
    };
    const g_3=FileGetUnchecked();
    const s_3=FileSetUnchecked();
    this.FileApplyUnchecked=(v) => FileApplyValue(g_3, s_3, v);
    this.IntSetUnchecked=(el) =>(i) => {
      el.value=String(i);
    };
    this.IntGetUnchecked=(el) => {
      const s_8=el.value;
      if(isBlank_1(s_8))return Some(0);
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
      if(isBlank_1(s_8))_1=(el.checkValidity?el.checkValidity():true)?CheckedInput.Blank(s_8):CheckedInput.Invalid(s_8);
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
      if(isBlank_1(s_8))return Some(0);
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
      if(isBlank_1(s_8))_1=(el.checkValidity?el.checkValidity():true)?CheckedInput.Blank(s_8):CheckedInput.Invalid(s_8);
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
    this.noneCT=New_48(false, []);
    this.scheduler=new Scheduler();
    this.defCTS=[new CancellationTokenSource()];
    this.Zero=Return();
    this.GetCT=(c) => {
      c.k(Ok_1(c.ct));
    };
  }
});
function New_48(IsCancellationRequested, Registrations){
  return{c:IsCancellationRequested, r:Registrations};
}
function Filter_1(ok, set_1){
  return new HashSet("New_2", filter_1(ok, ToArray_2(set_1)));
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
class DynamicAttrNode extends Object_1 {
  push;
  value;
  dirty;
  updates;
  NGetExitAnim(parent){
    return get_Empty();
  }
  NGetEnterAnim(parent){
    return get_Empty();
  }
  NGetChangeAnim(parent){
    return get_Empty();
  }
  get NChanged(){
    return this.updates;
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
let _c_10=Lazy((_i) => class $StartupCode_Abbrev {
  static {
    _c_10=_i(this);
  }
  static counter;
  static {
    this.counter=0;
  }
});
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
  return _c_8.StringSet;
}
function StringGet(){
  return _c_8.StringGet;
}
function StringListSet(){
  return _c_8.StringListSet;
}
function StringListGet(){
  return _c_8.StringListGet;
}
function DateTimeSetUnchecked(){
  return _c_8.DateTimeSetUnchecked;
}
function DateTimeGetUnchecked(){
  return _c_8.DateTimeGetUnchecked;
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
  return _c_8.FileSetUnchecked;
}
function FileGetUnchecked(){
  return _c_8.FileGetUnchecked;
}
function IntSetUnchecked(){
  return _c_8.IntSetUnchecked;
}
function IntGetUnchecked(){
  return _c_8.IntGetUnchecked;
}
function IntSetChecked(){
  return _c_8.IntSetChecked;
}
function IntGetChecked(){
  return _c_8.IntGetChecked;
}
function FloatSetUnchecked(){
  return _c_8.FloatSetUnchecked;
}
function FloatGetUnchecked(){
  return _c_8.FloatGetUnchecked;
}
function FloatSetChecked(){
  return _c_8.FloatSetChecked;
}
function FloatGetChecked(){
  return _c_8.FloatGetChecked;
}
function isBlank_1(s){
  return forall_1(IsWhiteSpace, s);
}
class CheckedInput {
  get Input(){
    return this.$==1?this.$0:this.$==2?this.$0:this.$1;
  }
  static Blank(inputText_1){
    return Create_1(CheckedInput, {$:2, $0:inputText_1});
  }
  static Invalid(inputText_1){
    return Create_1(CheckedInput, {$:1, $0:inputText_1});
  }
  static Valid(value, inputText_1){
    return Create_1(CheckedInput, {
      $:0,
      $0:value,
      $1:inputText_1
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
  return DomNodes(filter_1((n) => forall((k) =>!(n===k), excluded), a_1.$0));
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
function IsWhiteSpace(c){
  return c.match(new RegExp("\\s"))!==null;
}
function TryParse_3(s){
  const d=Date.parse(s);
  return isNaN(d)?null:Some(d);
}
function Create(f){
  return New_49(false, f, forceLazy);
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
let _c_11=Lazy((_i) => class $StartupCode_AppendList {
  static {
    _c_11=_i(this);
  }
  static Empty;
  static {
    this.Empty={$:0};
  }
});
function New_49(created, evalOrVal, force){
  return{
    c:created,
    v:evalOrVal,
    f:force
  };
}
function Clear(a){
  a.splice(0, length(a));
}
Main();

