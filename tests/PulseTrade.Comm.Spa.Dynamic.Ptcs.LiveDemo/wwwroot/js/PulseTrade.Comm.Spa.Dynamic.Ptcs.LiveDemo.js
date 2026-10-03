import Runtime from "./WebSharper.Core.JavaScript/Runtime.js"
Runtime.ScriptBasePath="/Scripts/";
import { MarkResizable, Create as Create_2, Lazy, GetOptional, SetOptional } from "./WebSharper.Core.JavaScript/Runtime.js"
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
        else if(path=="/management")_1=mountManagement(page);
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
        if(path!="/sets"&&path!="/actors"&&path!="/management"&&path!="/chat"){
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
  (globalThis.fetch(url, options).then((response) => response.text().then((body) => response.ok?onOk(json(isBlank(body)?"{}":body)):onError(isBlank(body)?"GET "+String(url)+" "+String(response.status):body))))["catch"]((error_5) => onError(errorMessage(error_5)));
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
  const appendPageKeyId=(keys) => asText(definition.setName)+"::"+concat_1(" + ", sortBy((key_1) => key_1.toLowerCase(), distinctBy((key_1) => key_1.toLowerCase(), choose((key_1) => {
    const text_1=Trim(asText(key_1));
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
      const key_1=_3[1];
      return exists((y) => key_1==y, retainedKeys);
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
    const key_1=replyCardKey(value);
    const m=tryFindIndex((y) => key_1==y, renderedValueCardKeys);
    if(m==null){
      const card=renderAppendValue(definition, value);
      renderedValueCardKeys=renderedValueCardKeys.concat([key_1]);
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
    }, (error_5) => {
      if(generation===loadGeneration)setStatus(status, "Cached data loaded; tail sync failed: "+error_5);
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
    const fetchFullState=(onFailure) => {
      getJson(url, (data) => {
        if(generation===loadGeneration){
          writeSnapshotWithWatermark(cacheKey_1, data, data.maxSequence, appendPageValueCount(data), "append-page-state");
          writeAppendPageKeyWatermark(data);
          applySnapshot("backend", data);
        }
      }, (error_5) => {
        if(generation===loadGeneration){
          setStatus(status, error_5);
          setStatus(workState, error_5);
          onFailure();
        }
      });
    };
    readJson(cacheKey_1, (a) => {
      if(a==null){
        if(generation===loadGeneration)fetchFullState(() => { });
      }
      else if(a.$0,generation===loadGeneration){
        const cached=a.$0;
        applySnapshot("cached", cached);
        fetchFullState(() => {
          const o=tryFind((bucket) => sameText(bucket.keyId, selected), filter_1((bucket) => bucket.maxSequence>0n, arrayOrEmpty(cached.buckets)));
          const x=o==null?tryFind((bucket) => bucket.maxSequence>0n, arrayOrEmpty(cached.buckets)):(o.$0,o);
          if(x==null){ }
          else readNewerFromBackend(generation, x.$0);
        });
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
                const keys=filter_1((key_1) =>!isBlank(key_1), map(asText, arrayOrEmpty(wire.keys)));
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
              if((isBlank(filterText)||exists((key_1) => asText(key_1).toLowerCase().indexOf(filterText.toLowerCase())!=-1, arrayOrEmpty(_4)))&&!isLocallyHiddenKeyId(keyId)){
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
              if(exists((id_1) => id_1==requestId, pendingWsAppendIds)){
                pendingWsAppendIds=filter_1((id_1) => id_1!=requestId, pendingWsAppendIds);
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
              return exists((id_1) => id_1==requestId, pendingWsAppendIds)?(pendingWsAppendIds=filter_1((id_1) => id_1!=requestId, pendingWsAppendIds),deletePendingThen(requestId, () => {
                refreshPendingState();
                setStatus(workState, pendingFailure("WebSocket command", asText(response.error)));
              })):setStatus(status, "WebSocket sync error: "+asText(response.error));
            case 5:
              return null;
          }
        }
        catch(error_5){
          return setStatus(status, "WebSocket sync parse failed: "+errorMessage(error_5));
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
      const request_1=New_13(definition.pageId, keyJson, addKeyMode, displayName_1);
      const pendingId=rememberPending("append-page-add-key", definition.pageId, "/pages/api/add-key", request_1);
      refreshPendingState();
      setStatus(status, "Adding key; pending command saved in browser DB");
      return postAppendPageKey("/pages/api/add-key", request_1, (reply) => {
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
      }, (error_5) => {
        setStatus(status, pendingFailure("Add key", error_5));
        refreshPendingState();
      });
    }
  };
  const appendValue=() => {
    const request_1=New_12(definition.pageId, selectedKeyJson, Trim(valueInput.value), Trim(directionInput.value), ["web-append"]);
    if(isBlank(request_1.keyJson))setStatus(workState, "Select or add a key first");
    else if(isBlank(request_1.valueText))setStatus(workState, "Value text is required");
    else if(isActorArguPage(definition)){
      const request_2=New_14(definition.pageId, request_1.keyJson, request_1.valueText, ["web-append", "actor-argu"]);
      const m=selectedBucket();
      if(m!=null&&m.$==1){
        const bucket=m.$0;
        const o=tryHead(arrayOrEmpty(bucket.keys));
        const actorAddress=o==null?"":o.$0;
        if(isBlank(actorAddress))setStatus(workState, "Actor address key is required");
        else {
          const pendingId=rememberPending("actor-argu-send", definition.pageId, "/pages/api/actor-argu/send", request_2);
          const wsRequest=New_17("actor-argu", pendingId, definition.pageId, definition.title, definition.setName, streamKeyFor(bucket), actorAddress, request_2.rawArgu, definition.shape, ofSeq(delay(() => append_2(arrayOrEmpty(definition.tags), delay(() => append_2(arrayOrEmpty(request_2.tags), delay(() => append_2(["page:"+asText(definition.pageId)], delay(() => append_2(["tab:"+asText(definition.tabId)], delay(() =>["shape:"+asText(definition.shape)])))))))))), browserId, definition.tabId);
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
        const pendingId_1=rememberPending("append-page-append-value", definition.pageId, "/pages/api/append", request_1);
        const wsRequest_1=New_19("append", pendingId_1, streamKeyFor(bucket_1), request_1.valueText, "append-page.value", definition.shape, pendingId_1, ofSeq(delay(() => append_2(arrayOrEmpty(definition.tags), delay(() => append_2(arrayOrEmpty(request_1.tags), delay(() => append_2(["page:"+asText(definition.pageId)], delay(() => append_2(["tab:"+asText(definition.tabId)], delay(() =>["shape:"+asText(definition.shape)])))))))))), browserId, definition.tabId);
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
        const pendingId_2=rememberPending("append-page-append-value", definition.pageId, "/pages/api/append", request_1);
        const wsRequest_2=New_18("append-page", pendingId_2, definition.pageId, definition.title, definition.setName, streamKeyFor(bucket_2), request_1.keyJson, request_1.valueText, request_1.direction, definition.shape, pendingId_2, ofSeq(delay(() => append_2(arrayOrEmpty(definition.tags), delay(() => append_2(arrayOrEmpty(request_1.tags), delay(() => append_2(["page:"+asText(definition.pageId)], delay(() => append_2(["tab:"+asText(definition.tabId)], delay(() =>["shape:"+asText(definition.shape)])))))))))), browserId, definition.tabId);
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
      const request_1=New_16(definition.pageId, removedKeyId);
      const pendingId=rememberPending("append-page-remove-key", definition.pageId, "/pages/api/remove-key", request_1);
      refreshPendingState();
      setStatus(status, "Removing key; pending command saved in browser DB");
      postRemoveAppendPageKey("/pages/api/remove-key", request_1, () => {
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
      }, (error_5) => {
        setStatus(status, pendingFailure("Remove key", error_5));
        refreshPendingState();
      });
    }
  }),removePageButton.addEventListener("click", () => {
    const request_1=New_15(definition.pageId);
    const pendingId=rememberPending("append-page-remove-page", definition.pageId, "/pages/api/remove-page", request_1);
    refreshPendingState();
    setStatus(status, "Removing page; pending command saved in browser DB");
    return postJson("/pages/api/remove-page", request_1, (reply) => {
      deletePendingThen(pendingId, () => {
        writeAppendPagesDefinitions(reply);
        setStatus(status, "Page removed");
        globalThis.location.assign("/chat");
      });
    }, (error_5) => {
      setStatus(status, pendingFailure("Remove page", error_5));
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
      }, (error_5) => {
        closeButton.removeAttribute("disabled");
        closeButton.textContent="!";
        closeButton.setAttribute("title", "Remove page failed: "+error_5);
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
  append(brandCluster, [element("div", "brand", currentProductLabel()), registryHealth]);
  renderNav(nav, activePath, pages);
  renderTabJumpOptions(navJumpSelect, activePath, staticNavigationDestinations().concat(map((page_1) =>[pagePath(page_1), pageTitle(page_1)], arrayOrEmpty(pages))));
  const userActions=element("div", "topbar-user-actions", null);
  const viewAs=renderViewAsControl();
  const x=element("a", "logout", "Logout");
  const logout=setHref(currentLogoutPath(), x);
  const page=element("main", "page", null);
  append(navViewport, [nav]);
  append(navJump, [navJumpSelect, navJumpGo]);
  append(navShell, [navJump, navViewport, navBack, navForward]);
  append(userActions, [viewAs, logout]);
  append(topRow, [brandCluster, userActions]);
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
  let selected, buckets, syncSocket, queuedSyncFrames, subscribedStreams, tailRequestedStreams, registryTailRequested, ensureSetsSubscriptions, loadGeneration, hiddenSetStreams, batchingSetSyncEvents;
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
  batchingSetSyncEvents=false;
  const maxSidebarBuckets=200;
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
    return cacheKey("sets-state-v2", ofArray([p[0], p[1]]));
  };
  const filtersAccept=(setName, keys) => {
    const p=currentFilterTexts();
    const setText=p[1];
    const keyText=p[0];
    return(isBlank(setText)||sameText(setName, setText))&&(isBlank(keyText)||exists((key_1) => asText(key_1).toLowerCase().indexOf(keyText.toLowerCase())!=-1, arrayOrEmpty(keys)));
  };
  const sortSetBuckets=(rows) => sortBy((bucket) =>[asText(bucket.setName), asText(bucket.keyId)], arrayOrEmpty(rows));
  const writeSetsCache=() => {
    const snapshot=New_21(fold((_1, _2) => Compare(_1, _2)===1?_1:_2, 0n, map((bucket) => bucket==null?0n:bucket.maxSequence, buckets)), buckets);
    writeSnapshotWithWatermark(currentCacheKey(), snapshot, snapshot.maxSequence, setValueCount(snapshot.buckets), "sets-state-v2");
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
    }, buckets.slice(0, maxSidebarBuckets));
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
    length(buckets)>maxSidebarBuckets?setStatus(status, "Loaded "+String(length(buckets))+" "+String(source)+" bucket(s); showing "+String(maxSidebarBuckets)+". Use filters to narrow the list."):setStatus(status, "Loaded "+String(length(buckets))+" "+String(source)+" bucket(s)");
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
    parts.push("metadataOnly=true");
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
        writeSnapshotWithWatermark(cacheKey_1, data, data.maxSequence, setValueCount(data.buckets), "sets-state-v2");
        applySnapshot("backend", data);
      }
    }, (error_5) => {
      if(generation===loadGeneration)setStatus(status, error_5);
    });
  };
  const closeActionPool=() => {
    actionPool.removeAttribute("open");
  };
  const tryReadSetRegistries=(event) => {
    if(event==null||isBlank(event.payload))return[];
    else try {
      if(asText(event.sourceKind).toLowerCase()=="set.stream.hidden.batch"){
        const batch=json(event.payload);
        return batch==null||asText(batch.schema)!="ptc.comm.spa.set.stream.batch.v1"?[]:map((wire_1) => setValueStreamKey(wire_1.pageId, wire_1.mode, wire_1.setName, wire_1.keys), arrayOrEmpty(batch.streams));
      }
      else {
        const wire=json(event.payload);
        return wire==null||asText(wire.schema)!="ptc.comm.spa.set.stream.v1"?[]:[setValueStreamKey(wire.pageId, wire.mode, wire.setName, wire.keys)];
      }
    }
    catch(m){
      return[];
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
          const request_1=New_4("read-tail", newRequestId("sets-read-tail"), _1, defaultRenderLimit());
          _1=JSON.stringify(request_1);
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
      const m=asText(event.sourceKind).toLowerCase();
      switch(m){
        case"set.stream":
          iter((streamKey) => {
            const setName_1=asText(streamKey.setName);
            const keys_1=arrayOrEmpty(streamKey.keys);
            const keyId_2=setKeyId(setName_1, keys_1);
            if(filtersAccept(setName_1, keys_1)&&eventIsVisibleAfterTombstone(keyId_2, event.createdAtUtc)){
              forgetHidden(keyId_2);
              if(!exists((bucket) => sameText(bucket.keyId, keyId_2), buckets)){
                buckets=sortSetBuckets(buckets.concat([New_20(keyId_2, setName_1, keys_1, 0, event.sequence, asText(event.createdAtUtc), [])]));
                isBlank(selected)?selected=keyId_2:void 0;
                !batchingSetSyncEvents?(renderList(),renderDetail(),writeSetsCache()):void 0;
              }
              if(sameText(keyId_2, selected)){
                const selectedStreamKey=setValueStreamKey("", "set", setName_1, keys_1);
                subscribeStream(selectedStreamKey);
                requestReadTailOnce(selectedStreamKey);
              }
              else void 0;
            }
            else void 0;
          }, tryReadSetRegistries(event));
          break;
        case"set.stream.hidden.batch":
        case"set.stream.hidden":
          iter((streamKey) => {
            let _4;
            const keyId_2=setKeyId(asText(streamKey.setName), arrayOrEmpty(streamKey.keys));
            hiddenSetStreams=filter_1((_5) =>!sameText(_5[0], keyId_2), hiddenSetStreams).concat([[keyId_2, asText(event.createdAtUtc)]]);
            buckets=filter_1((bucket) =>!sameText(bucket.keyId, keyId_2), buckets);
            if(sameText(selected, keyId_2)){
              const o=tryHead(buckets);
              const o_1=o==null?null:Some(o.$0.keyId);
              _4=selected=o_1==null?"":o_1.$0;
            }
            else _4=void 0;
            if(!batchingSetSyncEvents){
              renderList();
              renderDetail();
              writeSetsCache();
            }
            setStatus(status, "Hidden set stream "+keyId_2);
          }, tryReadSetRegistries(event));
          break;
        case"set":
          let updated, _1;
          const keyId=setKeyId(asText(event.streamKey.setName), arrayOrEmpty(event.streamKey.keys));
          if(eventIsVisibleAfterTombstone(keyId, event.createdAtUtc)){
            forgetHidden(keyId);
            if(!(event==null)&&event.sequence>0n&&!(event.streamKey==null)){
              const setName=asText(event.streamKey.setName);
              const keys=arrayOrEmpty(event.streamKey.keys);
              if(filtersAccept(setName, keys)){
                const value=New_22(textOr(event.eventId, event.sourceId), arrayOrEmpty(event.streamKey.keys), asText(event.createdAtUtc), asText(event.payload), arrayOrEmpty(event.tags));
                const keyId_1=setKeyId(setName, keys);
                const m_1=tryFind((bucket) => sameText(bucket.keyId, keyId_1), buckets);
                if(m_1==null)updated=New_20(keyId_1, setName, keys, 1, event.sequence, asText(event.createdAtUtc), [value]);
                else {
                  const existing=m_1.$0;
                  const existingValues=arrayOrEmpty(existing.values);
                  const alreadyVisible=exists((row) => sameText(row.valueId, value.valueId), existingValues);
                  const v=filter_1((row) =>!sameText(row.valueId, value.valueId), existingValues).concat([value]);
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
                buckets=sortSetBuckets(filter_1((bucket) =>!sameText(bucket.keyId, keyId_1), buckets).concat([updated]));
                selected=keyId_1;
                if(!batchingSetSyncEvents){
                  renderList();
                  renderDetail();
                  writeSetsCache();
                  ensureSetsSubscriptions();
                }
                setStatus(status, "Synced set event "+value.valueId);
              }
              else null;
            }
            else null;
          }
          else null;
          break;
        default:
          null;
          break;
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
          setData("ws-last-stream", response.streamKey, page);
          setWsState("subscribed");
          break;
        case 1:
          handleSyncEvent(response.event);
          break;
        case 2:
          let _1;
          batchingSetSyncEvents=true;
          try {
            _1=iter(handleSyncEvent, arrayOrEmpty(response.events));
          }
          finally {
            batchingSetSyncEvents=false;
          }
          renderList();
          renderDetail();
          writeSetsCache();
          ensureSetsSubscriptions();
          break;
        case 3:
          setStatus(status, "WebSocket sets sync error: "+asText(response.error));
          break;
        case 4:
          null;
          break;
      }
    }
    catch(error_5){
      setStatus(status, "WebSocket sets sync parse failed: "+errorMessage(error_5));
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
      deleteSnapshotsByPrefix(cacheKey("sets-state-v2", FSharpList.Empty), () => {
        subscribedStreams=[];
        tailRequestedStreams=[];
        registryTailRequested=false;
        setData("ws-stream-count", String(length(subscribedStreams)), page);
        setStatus(status, "Cleaned "+String(reply.hiddenCount)+" no-show actor stream(s)");
        load();
      });
    }, (error_5) => {
      setStatus(status, "CleanAllNoShow Actors failed: "+error_5);
    });
  });
  cleanParticipantsAction.addEventListener("click", () => {
    closeActionPool();
    setStatus(status, "Cleaning inactive participant collections");
    postJson("/sets/api/clean-inactive-participant-collections", New_23("browser-action"), (reply) => {
      deleteSnapshotsByPrefix(cacheKey("sets-state-v2", FSharpList.Empty), () => {
        subscribedStreams=[];
        tailRequestedStreams=[];
        registryTailRequested=false;
        setData("ws-stream-count", String(length(subscribedStreams)), page);
        setStatus(status, "Cleaned "+String(reply.cleanedParticipantCount)+" inactive participant(s); hidden "+String(reply.hiddenCount)+" stream(s)");
        load();
      });
    }, (error_5) => {
      setStatus(status, "Clean Inactive Participant Collections failed: "+error_5);
    });
  });
  keyFilter.addEventListener("input", load);
  setFilter.addEventListener("input", load);
  load();
}
function mountActors(page){
  let reportRequestCount, reportEditVersion, selectedReportStatus, actorSnapshot, syncSocket, queuedSyncFrames, subscribedRegistry, registryTailRequested, dynamicActorsPageAccepted;
  page.className="page actors-page";
  const head_2=element("div", "work-head actors-head", null);
  const title=element("div", "", null);
  const actions=element("div", "head-actions", null);
  const status=element("div", "state", "Loading actors");
  const reload=button("", "Reload");
  const nodes=element("div", "nodes", null);
  const treePanel=setTestId("actor-tree-panel", element("section", "actor-tree-panel", null));
  const reportToolbar=setTestId("actors-report-controls", element("section", "actor-report-controls", null));
  reportToolbar.setAttribute("style", "display:flex; flex-direction:column; gap:8px; padding:12px; border:1px solid #d8e1ee; border-radius:8px;");
  const reportForm=element("div", "", null);
  reportForm.setAttribute("style", "display:flex; flex-wrap:wrap; gap:8px; align-items:end;");
  const reportDirectory=setTestId("actors-report-directory", input("G:\\Reports"));
  reportDirectory.setAttribute("style", "width:100%; min-height:39px; padding:8px; font:inherit; box-sizing:border-box;");
  const directoryLabel=element("label", "", "\u5831\u544a\u76ee\u9304\uff08\u4f3a\u670d\u5668\u672c\u6a5f\uff09");
  directoryLabel.setAttribute("style", "display:flex; flex-direction:column; gap:4px; margin:0; flex:1 1 360px; min-width:220px;");
  append(directoryLabel, [reportDirectory]);
  const reportSeconds=setTestId("actors-report-interval", input("300"));
  reportSeconds.setAttribute("type", "number");
  reportSeconds.setAttribute("min", "30");
  reportSeconds.setAttribute("max", "86400");
  reportSeconds.value="300";
  reportSeconds.setAttribute("style", "width:120px; min-height:39px; padding:8px; font:inherit; box-sizing:border-box;");
  const intervalLabel=element("label", "", "\u6392\u7a0b\u9593\u9694\uff08\u79d2\uff09");
  intervalLabel.setAttribute("style", "display:flex; flex-direction:column; gap:4px; margin:0;");
  append(intervalLabel, [reportSeconds]);
  const generateReport=setTestId("actors-report-generate", button("", "\u7522\u751f\u5831\u544a"));
  const scheduleReport=setTestId("actors-report-schedule", button("", "\u555f\u7528\uff0f\u66f4\u65b0\u6392\u7a0b"));
  const cancelReport=setTestId("actors-report-cancel", button("", "\u53d6\u6d88\u6392\u7a0b"));
  const readReportStatus=setTestId("actors-report-read-status", button("", "\u8b80\u53d6\u72c0\u614b"));
  const reportStatus=setTestId("actors-report-status", element("div", "state", "\u4f3a\u670d\u5668\u4fdd\u5b58\u6392\u7a0b\uff1b\u95dc\u9589\u6b64\u9801\u9762\u5f8c\u4ecd\u6703\u57f7\u884c\u3002"));
  reportStatus.setAttribute("style", "overflow-wrap:anywhere; max-width:100%;");
  append(reportForm, [directoryLabel, intervalLabel, generateReport, scheduleReport, cancelReport, readReportStatus]);
  append(reportToolbar, [reportForm, reportStatus]);
  reportRequestCount=0;
  reportEditVersion=0;
  selectedReportStatus=null;
  const reportEndpoint="/actors/api/report/schedules";
  const reportReadOnly=currentBrowserUser().viewAsActive;
  const normalizeReportPath=(value) => TrimEnd(Replace(Trim(asText(value)), "\\", "/"), ["/"]).toLowerCase();
  const setReportDisabled=(disabled, control_1) => disabled?control_1.setAttribute("disabled", "disabled"):control_1.removeAttribute("disabled");
  const updateReportControls=() => {
    const busy=reportRequestCount>0;
    setReportDisabled(busy||reportReadOnly, generateReport);
    setReportDisabled(busy||reportReadOnly, scheduleReport);
    setReportDisabled(busy||reportReadOnly||!(selectedReportStatus==null?false:selectedReportStatus.$0.enabled), cancelReport);
    setReportDisabled(busy, readReportStatus);
  };
  const finishReportRequest=() => {
    const a=0;
    const b=reportRequestCount-1;
    reportRequestCount=Compare(a, b)===1?a:b;
    updateReportControls();
  };
  const beginReportRequest=() => {
    reportRequestCount=reportRequestCount+1;
    reportEditVersion=reportEditVersion+1;
    updateReportControls();
    return reportEditVersion;
  };
  const showReportStatus=(version, reply) => version===reportEditVersion&&normalizeReportPath(reportDirectory.value)==normalizeReportPath(reply.configuration.outputDirectory)?(selectedReportStatus=Some(reply),setStatus(reportStatus, (reply.enabled?"\u4f3a\u670d\u5668\u6392\u7a0b\u5df2\u555f\u7528\uff0c\u6bcf "+String(reply.configuration.intervalSeconds)+" \u79d2\uff1b\u8b80\u53d6\u6642\u4e0b\u6b21\u57f7\u884c\u6642\u9593 "+String(asText(reply.dueAtUtc)):"\u4f3a\u670d\u5668\u6392\u7a0b\u5df2\u53d6\u6d88")+(!isBlank(reply.lastError)?"\uff1b\u932f\u8aa4\uff1a"+reply.lastError:reply.hasPendingReport?"\uff1b\u5831\u544a\u8f38\u51fa\u5f85\u5b8c\u6210\u3002":!isBlank(reply.lastFilePath)?"\uff1b\u6700\u8fd1\u5831\u544a\uff1a"+reply.lastFilePath:""))):null;
  const reportRequestFailed=(version, error_5) => {
    version===reportEditVersion?setStatus(reportStatus, error_5):void 0;
    return finishReportRequest();
  };
  const loadReportSchedules=() => {
    const version=beginReportRequest();
    getJson(reportEndpoint, (rows) => {
      let _1;
      if(version===reportEditVersion){
        const selected=tryFind((row) => normalizeReportPath(row.configuration.outputDirectory)==normalizeReportPath(reportDirectory.value), arrayOrEmpty(rows));
        _1=(selectedReportStatus=selected,selected==null?setStatus(reportStatus, "\u6b64\u76ee\u9304\u5c1a\u672a\u8a2d\u5b9a\u4f3a\u670d\u5668\u6392\u7a0b\u3002"):showReportStatus(version, selected.$0));
      }
      else _1=void 0;
      finishReportRequest();
    }, (_1) => reportRequestFailed(version, _1));
  };
  reportDirectory.addEventListener("input", () => {
    reportEditVersion=reportEditVersion+1;
    selectedReportStatus=null;
    setStatus(reportStatus, "\u76ee\u9304\u5df2\u8b8a\u66f4\uff1b\u8acb\u8b80\u53d6\u72c0\u614b\u6216\u555f\u7528\u6392\u7a0b\u3002");
    return updateReportControls();
  });
  generateReport.addEventListener("click", () => {
    if(isBlank(reportDirectory.value))return setStatus(reportStatus, "\u8acb\u8f38\u5165\u4f3a\u670d\u5668\u672c\u6a5f\u5b8c\u6574\u5831\u544a\u76ee\u9304\u3002");
    else {
      const version=beginReportRequest();
      return postJson("/actors/api/report", New_26(reportDirectory.value), (reply) => {
        version===reportEditVersion?setStatus(reportStatus, "\u5831\u544a\u5df2\u5beb\u5165\uff1a"+asText(reply.filePath)):void 0;
        finishReportRequest();
      }, (_1) => reportRequestFailed(version, _1));
    }
  });
  scheduleReport.addEventListener("click", () => {
    let o, _1;
    const m=(o=0,[TryParse(reportSeconds.value, {get:() => o, set:(v) => {
      o=v;
    }}), o]);
    if(m[0]){
      const seconds=m[1];
      _1=!isBlank(reportDirectory.value)&&seconds>=30&&seconds<=86400;
    }
    else _1=false;
    if(_1){
      const version=beginReportRequest();
      return postJson(reportEndpoint, New_25(reportDirectory.value, m[1], false), (reply) => {
        showReportStatus(version, reply);
        finishReportRequest();
      }, (_2) => reportRequestFailed(version, _2));
    }
    else return setStatus(reportStatus, "\u8acb\u8f38\u5165\u5b8c\u6574\u5831\u544a\u76ee\u9304\uff0c\u9593\u9694\u9700\u70ba 30 \u81f3 86400 \u79d2\u3002");
  });
  cancelReport.addEventListener("click", () => {
    if(selectedReportStatus!=null&&selectedReportStatus.$==1){
      if(selectedReportStatus.$0.enabled){
        const selected=selectedReportStatus.$0;
        const version=beginReportRequest();
        return postJson(reportEndpoint+"/cancel", New_27(selected.scheduleId), (reply) => {
          showReportStatus(version, reply);
          finishReportRequest();
        }, (_1) => reportRequestFailed(version, _1));
      }
      else return null;
    }
    else return null;
  });
  readReportStatus.addEventListener("click", loadReportSchedules);
  updateReportControls();
  loadReportSchedules();
  append(title, [element("label", "", "Actor / Participant Management"), element("h1", "", "Actors")]);
  append(actions, [status, reload]);
  append(head_2, [title, actions]);
  append(page, [head_2, reportToolbar, treePanel, nodes]);
  const emptySnapshot=New_28(0, 0, 0n, []);
  actorSnapshot=emptySnapshot;
  syncSocket=null;
  queuedSyncFrames=[];
  subscribedRegistry=false;
  registryTailRequested=false;
  dynamicActorsPageAccepted=false;
  const collapsedTreeNodes=new HashSet("New_3");
  const cacheKey_1=cacheKey("actors-snapshot", FSharpList.Empty);
  const sameText=(left, right) => asText(left).toLowerCase()==asText(right).toLowerCase();
  const actorStatusLooksOffline=(value) => {
    const text_1=Trim(asText(value)).toLowerCase();
    return text_1.indexOf("offline")!=-1||text_1.indexOf("unreachable")!=-1||text_1.indexOf("stale")!=-1||text_1.indexOf("terminated")!=-1||text_1.indexOf("stopped")!=-1||text_1.indexOf("dead")!=-1||text_1.indexOf("failed")!=-1;
  };
  const actorTagValue=(prefix, tags) => tryPick((tag) => {
    const value=asText(tag);
    return StartsWith(value, prefix)?Some(value.substring(prefix.length)):null;
  }, arrayOrEmpty(tags));
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
        const id_1=asText(node.id);
        const o=childMap.TryFind(id_1);
        let _6=o==null?[]:o.$0;
        const children=sortBy((node_1) => asText(node_1.label), _6);
        const hasChildren=length(children)>0;
        const row=setData("node-id", id_1, setTestId("actor-tree-row", element("div", "actor-tree-row", null)));
        setData("parent-id", asText(node.parentId), row);
        const a=12;
        const a_1=0;
        const b=Compare(a_1, depth)===1?a_1:depth;
        let _7=Compare(a, b)===-1?a:b;
        let _8=String(_7);
        setData("depth", _8, row);
        const toggleText=!hasChildren?"":collapsedTreeNodes.Contains(id_1)?"+":"-";
        if(hasChildren){
          const value=setTestId("actor-tree-toggle", button("actor-tree-toggle", toggleText));
          toggle=(value.setAttribute("aria-expanded", collapsedTreeNodes.Contains(id_1)?"false":"true"),value.setAttribute("title", collapsedTreeNodes.Contains(id_1)?"Expand":"Collapse"),value);
        }
        else toggle=element("span", "actor-tree-toggle actor-tree-toggle-placeholder", "");
        if(hasChildren)toggle.addEventListener("click", () => {
          collapsedTreeNodes.Contains(id_1)?collapsedTreeNodes.Remove(id_1):collapsedTreeNodes.SAdd(id_1);
          return renderActorTree("toggle", tree);
        });
        else null;
        const labelText=asText(node.label);
        const kindText=asText(node.kind);
        const statusText=asText(node.status);
        const fullPathText=asText(node.fullPath);
        const addressText=asText(node.address);
        const displayText=!isBlank(addressText)?addressText:!isBlank(fullPathText)?fullPathText:!isBlank(labelText)?labelText:id_1;
        const label=element("span", "actor-tree-label", displayText);
        const statusDot_1=setData("status", statusText, element("span", "actor-tree-status-dot", ""));
        const kindPill=element("span", "actor-tree-kind-pill", kindText);
        const statusPill=setData("status", statusText, element("span", "actor-tree-status-pill", statusText));
        label.setAttribute("title", displayText);
        kindPill.setAttribute("title", "kind: "+kindText);
        statusPill.setAttribute("title", "status: "+statusText);
        append(row, [toggle, statusDot_1, label, kindPill, statusPill]);
        treeBody.appendChild(row);
        if(!collapsedTreeNodes.Contains(id_1)){
          const _9=depth+1;
          return iter((_10) => renderNode(_9, _10), children);
        }
        else return null;
      }
      const roots=arrayOrEmpty(tree.rootNodeIds);
      let _2=length(roots)===0?map((a) => a.id, filter_1((node) => isBlank(node.parentId), safeNodes)):roots;
      let _3=choose((id_1) => nodeMap.TryFind(asText(id_1)), _2);
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
    }, (error_5) => {
      clear(treePanel);
      treePanel.appendChild(element("div", "empty", "ActorTree unavailable: "+error_5));
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
      catch(m){
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
          const incomingGeneration=actorTagValue("generation:", tags);
          const incomingEventKind=actorTagValue("event:", tags);
          const existingNode=tryFind((node) => sameText(node.nodeId, nodeId), arrayOrEmpty(actorSnapshot.nodes));
          const o=existingNode==null?null:tryFind((actor_1) => sameText(actor_1.actorId, actorId), arrayOrEmpty(existingNode.$0.actors));
          const _2=o==null?null:actorTagValue("generation:", o.$0.keys);
          if(_2!=null&&_2.$==1?incomingGeneration!=null&&incomingGeneration.$==1?!sameText(_2.$0, incomingGeneration.$0)?(_2.$0,incomingGeneration.$0,incomingEventKind==null?false:sameText(incomingEventKind.$0, "Registered")):true:true:true){
            const actor=New_30(actorId, textOr(actorId, _1.displayName), textOr("actor", _1.kind), [nodeId, actorId].concat(tags), textOr("running", _1.status), arrayOrEmpty(_1.routees));
            if(existingNode==null)updatedNode=New_29(nodeId, nodeAddress, actorStatusLooksOffline(actor.status)?"offline":"up", roles, actorStatusLooksOffline(actor.status)?[]:[actor]);
            else {
              const existing=existingNode.$0;
              const retainedActors=filter_1((row) =>!sameText(row.actorId, actorId), arrayOrEmpty(existing.actors));
              const actors=sortBy((row) => asText(row.actorId), actorStatusLooksOffline(actor.status)?retainedActors:retainedActors.concat([actor]));
              updatedNode=New_29(existing.nodeId, isBlank(nodeAddress)?asText(existing.nodeAddress):nodeAddress, length(actors)===0?"offline":"up", length(roles)===0?arrayOrEmpty(existing.roles):roles, actors);
            }
            const nodes_1=sortBy((node) => asText(node.nodeId), length(arrayOrEmpty(updatedNode.actors))===0?filter_1((node) =>!sameText(node.nodeId, nodeId), arrayOrEmpty(actorSnapshot.nodes)):filter_1((node) =>!sameText(node.nodeId, nodeId), arrayOrEmpty(actorSnapshot.nodes)).concat([updatedNode]));
            let _3=length(nodes_1);
            let _4=fold((_6, _7) => _6+_7, 0, map((node) => arrayOrEmpty(node.actors).length, nodes_1));
            const a=actorSnapshot.maxSequence;
            const b=event.sequence;
            let _5=Compare(a, b)===1?a:b;
            actorSnapshot=New_28(_3, _4, _5, nodes_1);
            writeSnapshotWithWatermark(cacheKey_1, actorSnapshot, actorSnapshot.maxSequence, actorValueCount(actorSnapshot), "actors-snapshot");
            applySnapshot("synced", actorSnapshot);
            setStatus(status, "Synced actor "+actorId);
          }
          else void 0;
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
          requestRegistryTail();
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
    catch(error_5){
      setStatus(status, "WebSocket actors sync parse failed: "+errorMessage(error_5));
    }
  }
  reload.addEventListener("click", load);
  readJson(cacheKey_1, (a) => {
    if(a==null){ }
    else applySnapshot("cached", a.$0);
  });
  load();
  subscribeRegistry();
}
function mountManagement(page){
  let allPages, allSets, allGroups, allParticipants, selectedPageKeys, selectedSetKeys, selectedParticipantKeys, pageIndex, setPageIndex, groupPageIndex, participantPageIndex, pageSize, setPageSize, groupPageSize, participantPageSize, clearingCache;
  page.className="page management-page";
  const pageRows=Create((row) => asText(row.pageId)+"\u001f"+asText(row.tabId), FSharpList.Empty);
  const setRows=Create((row) => asText(row.keyId), FSharpList.Empty);
  const groupRows=Create((row) => asText(row.groupId), FSharpList.Empty);
  const participantRows=Create((row) => asText(row.participantId), FSharpList.Empty);
  allPages=[];
  allSets=[];
  allGroups=[];
  allParticipants=[];
  selectedPageKeys=[];
  selectedSetKeys=[];
  selectedParticipantKeys=[];
  pageIndex=0;
  setPageIndex=0;
  groupPageIndex=0;
  participantPageIndex=0;
  pageSize=10;
  setPageSize=10;
  groupPageSize=10;
  participantPageSize=10;
  const heading=element("div", "management-head", null);
  const title=element("div", "", null);
  append(title, [element("h1", "", "Management")]);
  const reload=setTestId("management-reload", button("", "Reload"));
  const clearCache=setTestId("management-clear-browser-cache", button("", "\u6e05\u7406\u672c\u6a5f\u5feb\u53d6"));
  const headingActions=element("div", "management-pager", null);
  append(headingActions, [clearCache, reload]);
  append(heading, [title, headingActions]);
  const cacheStatus=setData("state", "idle", setTestId("management-browser-cache-status", element("p", "state", "\u53ea\u6e05\u9664\u6b64\u700f\u89bd\u5668\u76ee\u524d\u670d\u52d9\u7684\u53ef\u91cd\u5efa\u5feb\u7167\uff1b\u4fdd\u7559\u5f85\u767c\u547d\u4ee4\u3001\u672a\u8b80\u72c0\u614b\u8207\u5176\u4ed6\u670d\u52d9\u8cc7\u6599\u3002")));
  cacheStatus.setAttribute("role", "status");
  cacheStatus.setAttribute("aria-live", "polite");
  clearingCache=false;
  clearCache.addEventListener("click", () => {
    if(!clearingCache&&globalThis.confirm("\u6e05\u7406\u6b64\u700f\u89bd\u5668\u76ee\u524d\u670d\u52d9\u7684\u53ef\u91cd\u5efa\u5feb\u7167\uff1f\u5f85\u767c\u547d\u4ee4\u3001\u672a\u8b80\u72c0\u614b\u8207\u5176\u4ed6\u670d\u52d9\u8cc7\u6599\u6703\u4fdd\u7559\u3002\u5176\u4ed6\u5206\u9801\u53ef\u80fd\u91cd\u65b0\u5efa\u7acb\u5feb\u53d6\uff1b\u5b8c\u6210\u5f8c\u53ef\u6309 Reload \u66f4\u65b0\u756b\u9762\u3002")){
      clearingCache=true;
      clearCache.setAttribute("disabled", "disabled");
      setData("state", "clearing", cacheStatus);
      const a=["removed-snapshots", "removed-watermarks", "retained-snapshots", "retained-watermarks"];
      for(let i=0, _3=a.length-1;i<=_3;i++)cacheStatus.removeAttribute("data-"+get(a, i));
      setStatus(cacheStatus, "\u6b63\u5728\u6e05\u7406\u672c\u6a5f\u53ef\u91cd\u5efa\u5feb\u53d6\u2026");
      return clearRebuildableSnapshots((result) => {
        clearingCache=false;
        clearCache.removeAttribute("disabled");
        if(result.$==1){
          const message=result.$0;
          setData("state", "failed", cacheStatus);
          setStatus(cacheStatus, "\u6e05\u7406\u672a\u5b8c\u6210\uff1a"+message);
        }
        else {
          const summary=result.$0;
          setData("server-reality-id", summary.ServerRealityId, setData("state", "committed", cacheStatus));
          const a_1=[["removed-snapshots", summary.RemovedSnapshots], ["removed-watermarks", summary.RemovedWatermarks], ["retained-snapshots", summary.RetainedSnapshots], ["retained-watermarks", summary.RetainedWatermarks]];
          for(let i_1=0, _4=a_1.length-1;i_1<=_4;i_1++)((() => {
            const f=get(a_1, i_1);
            setData(f[0], String(f[1]), cacheStatus);
          })());
          setStatus(cacheStatus, "\u672c\u6b21\u5df2\u6e05\u9664 "+String(summary.RemovedSnapshots)+" \u7b46\u5feb\u7167\u8207 "+String(summary.RemovedWatermarks)+" \u7b46\u5feb\u53d6\u9032\u5ea6\uff1b\u4fdd\u7559 "+String(summary.RetainedSnapshots+summary.RetainedWatermarks)+" \u7b46\u5176\u4ed6\u8a18\u9304\u53ca\u5168\u90e8\u5f85\u767c\u547d\u4ee4\u3001\u672a\u8b80\u72c0\u614b\u3002\u540c\u6b65\u4e2d\u7684\u5206\u9801\u53ef\u80fd\u91cd\u65b0\u5efa\u7acb\u5feb\u53d6\uff1b\u53ef\u6309 Reload \u66f4\u65b0\u756b\u9762\u3002");
        }
      });
    }
    else return null;
  });
  const pageSection=setTestId("management-pages", element("section", "management-section", null));
  const pageSectionHead=element("div", "management-section-head", null);
  const pageCount=setTestId("management-pages-count", element("span", "state", ""));
  const pageFilter=setTestId("management-pages-filter", input("Filter tab pages"));
  append(pageSectionHead, [element("h2", "", "Tab pages"), pageFilter, pageCount]);
  const pageSelectionBar=element("div", "management-selection-bar", null);
  const pageSelectionCount=setTestId("management-pages-selected-count", element("span", "state", "0 selected"));
  const pageShowSelected=setTestId("management-pages-show-selected", button("", "Show selected"));
  const pageHideSelected=setTestId("management-pages-hide-selected", button("", "Hide selected"));
  const pageDeleteSelected=setTestId("management-pages-delete-selected", button("management-delete", "Delete selected"));
  pageShowSelected.setAttribute("disabled", "disabled");
  pageHideSelected.setAttribute("disabled", "disabled");
  pageDeleteSelected.setAttribute("disabled", "disabled");
  append(pageSelectionBar, [pageSelectionCount, pageShowSelected, pageHideSelected, pageDeleteSelected]);
  const pageTableHostId="management-pages-grid";
  const pageTableHost=setId(pageTableHostId, element("div", "management-table-viewport", null));
  const pagePager=element("div", "management-pager", null);
  const pagePrevious=setTestId("management-pages-previous", button("", "Previous"));
  const pageNext=setTestId("management-pages-next", button("", "Next"));
  const pageSizeSelect=setTestId("management-pages-size", select([["10", "10"], ["20", "20"], ["40", "40"], ["0", "All"]]));
  const pagePagerStatus=setTestId("management-pages-page", element("span", "state", ""));
  append(pagePager, [pagePrevious, pageNext, element("span", "management-page-size-label", "Rows"), pageSizeSelect, pagePagerStatus]);
  append(pageSection, [pageSectionHead, pageSelectionBar, pageTableHost, pagePager]);
  const setSection=setTestId("management-sets", element("section", "management-section", null));
  const setSectionHead=element("div", "management-section-head", null);
  const setCount=setTestId("management-sets-count", element("span", "state", ""));
  const setFilter=setTestId("management-sets-filter", input("Filter set contents"));
  append(setSectionHead, [element("h2", "", "Set contents"), setFilter, setCount]);
  const setSelectionBar=element("div", "management-selection-bar", null);
  const setSelectionCount=setTestId("management-sets-selected-count", element("span", "state", "0 selected"));
  const setDeleteSelected=setTestId("management-sets-delete-selected", button("management-delete", "Delete selected"));
  setDeleteSelected.setAttribute("disabled", "disabled");
  append(setSelectionBar, [setSelectionCount, setDeleteSelected]);
  const setTableHostId="management-sets-grid";
  const setTableHost=setId(setTableHostId, element("div", "management-table-viewport", null));
  const setPager=element("div", "management-pager", null);
  const setPrevious=setTestId("management-sets-previous", button("", "Previous"));
  const setNext=setTestId("management-sets-next", button("", "Next"));
  const setSizeSelect=setTestId("management-sets-size", select([["10", "10"], ["20", "20"], ["40", "40"], ["0", "All"]]));
  const setPagerStatus=setTestId("management-sets-page", element("span", "state", ""));
  append(setPager, [setPrevious, setNext, element("span", "management-page-size-label", "Rows"), setSizeSelect, setPagerStatus]);
  append(setSection, [setSectionHead, setSelectionBar, setTableHost, setPager]);
  const groupSection=setTestId("management-groups", element("section", "management-section", null));
  const groupSectionHead=element("div", "management-section-head", null);
  const groupCount=setTestId("management-groups-count", element("span", "state", ""));
  const groupFilter=setTestId("management-groups-filter", input("Filter groups"));
  append(groupSectionHead, [element("h2", "", "Groups"), groupFilter, groupCount]);
  const groupTableHostId="management-groups-grid";
  const groupTableHost=setId(groupTableHostId, element("div", "management-table-viewport", null));
  const groupPager=element("div", "management-pager", null);
  const groupPrevious=setTestId("management-groups-previous", button("", "Previous"));
  const groupNext=setTestId("management-groups-next", button("", "Next"));
  const groupSizeSelect=setTestId("management-groups-size", select([["10", "10"], ["20", "20"], ["40", "40"], ["0", "All"]]));
  const groupPagerStatus=setTestId("management-groups-page", element("span", "state", ""));
  append(groupPager, [groupPrevious, groupNext, element("span", "management-page-size-label", "Rows"), groupSizeSelect, groupPagerStatus]);
  append(groupSection, [groupSectionHead, groupTableHost, groupPager]);
  const participantSection=setTestId("management-participants", element("section", "management-section", null));
  const participantSectionHead=element("div", "management-section-head", null);
  const participantCount=setTestId("management-participants-count", element("span", "state", ""));
  const participantFilter=setTestId("management-participants-filter", input("Filter participants"));
  append(participantSectionHead, [element("h2", "", "Participants"), participantFilter, participantCount]);
  const participantSelectionBar=element("div", "management-selection-bar", null);
  const participantSelectionCount=setTestId("management-participants-selected-count", element("span", "state", "0 selected"));
  const participantShowSelected=setTestId("management-participants-show-selected", button("", "Show selected"));
  const participantHideSelected=setTestId("management-participants-hide-selected", button("", "Hide selected"));
  const participantDeleteSelected=setTestId("management-participants-delete-selected", button("management-delete", "Delete selected"));
  participantShowSelected.setAttribute("disabled", "disabled");
  participantHideSelected.setAttribute("disabled", "disabled");
  participantDeleteSelected.setAttribute("disabled", "disabled");
  append(participantSelectionBar, [participantSelectionCount, participantShowSelected, participantHideSelected, participantDeleteSelected]);
  const participantTableHostId="management-participants-grid";
  const participantTableHost=setId(participantTableHostId, element("div", "management-table-viewport", null));
  const participantPager=element("div", "management-pager", null);
  const participantPrevious=setTestId("management-participants-previous", button("", "Previous"));
  const participantNext=setTestId("management-participants-next", button("", "Next"));
  const participantSizeSelect=setTestId("management-participants-size", select([["10", "10"], ["20", "20"], ["40", "40"], ["0", "All"]]));
  const participantPagerStatus=setTestId("management-participants-page", element("span", "state", ""));
  append(participantPager, [participantPrevious, participantNext, element("span", "management-page-size-label", "Rows"), participantSizeSelect, participantPagerStatus]);
  const walletHost=element("div", "", null);
  append(participantSection, [participantSectionHead, participantSelectionBar, walletHost, participantTableHost, participantPager]);
  append(page, [heading, cacheStatus, pageSection, setSection, groupSection, participantSection]);
  const walletAllows=(action, participantId) => {
    const _3=currentAclSnapshot();
    if(_3!=null&&_3.$==1){
      const snapshot=currentAclSnapshot().$0;
      return snapshot.enabled&&snapshot.authenticated&&!isBlank(snapshot.userId)&&snapshot.userId==currentBrowserUser().authenticatedAclUserId&&!(snapshot.resources==null)&&exists((resource) => resource.resourceKind=="ptcs.participant"&&resource.resourceId==participantId&&!(resource.capabilities==null)&&exists((capability) => capability.action==action&&capability.allowed, resource.capabilities), currentAclSnapshot().$0.resources);
    }
    else return false;
  };
  const _1=readAction();
  const _2=topUpAction();
  const walletController=mount(walletHost, New_32(() => {
    const user=currentBrowserUser();
    return user.authenticated&&!isBlank(user.authenticatedParticipantId)&&StartsWith(user.authenticatedParticipantId, "user.")&&(user.provider=="github-oauth"||user.provider=="ptcs-login");
  }, () => currentBrowserUser().authenticatedParticipantId, () => currentBrowserUser().viewAsActive, (_3) => walletAllows(_1, _3), (_3) => walletAllows(_2, _3)));
  const pageCountFor=(total, size) => total===0?1:size===0?1:toInt(Math.ceil(total/size));
  const pageRowKey=(row) => asText(row.pageId)+"\u001f"+asText(row.tabId);
  const pageSelectionElementId=(row) =>"management-page-selection-"+asText(row.pageId)+"-"+asText(row.tabId);
  const setSelectionElementId=(row) =>"management-set-selection-"+asText(row.keyId);
  const participantSelectionElementId=(row) =>"management-participant-selection-"+asText(row.participantId);
  const toggleSelection=(key_1, isChecked, selected) => isChecked?exists((current) => current==key_1, selected)?selected:selected.concat([key_1]):filter_1((current) => current!=key_1, selected);
  const sliceRows=(index, size, rows) => {
    if(size===0)return rows;
    else {
      const a=length(rows);
      const b=index*size;
      let _3=Compare(a, b)===-1?a:b;
      let _4=skip(_3, rows);
      return _4.slice(0, size);
    }
  };
  const matchesFilter=(filterInput, values) => {
    const query=Trim(asText(filterInput.value)).toLowerCase();
    return isBlank(query)||exists((value) => asText(value).toLowerCase().indexOf(query)!=-1, values);
  };
  const filteredPages=() => filter_1((row) => matchesFilter(pageFilter, [row.pageId, row.tabId, row.title, row.path]), allPages);
  const filteredSets=() => filter_1((row) => matchesFilter(setFilter, [row.keyId, row.setName, concat_1(" ", arrayOrEmpty(row.keys))]), allSets);
  const filteredParticipants=() => filter_1((row) => matchesFilter(participantFilter, [row.participantId, row.displayName, row.kind, row.status]), allParticipants);
  const pageResourceAllows=(row, action) => pageAclAllows(row.pageId, action)||systemAclAllows("*", action);
  const pageCanSelect=(row) => pageResourceAllows(row, "ptcs.management.page.delete");
  const setCanSelect=() => systemAclAllows("sets", "ptcs.set.clean")||systemAclAllows("*", "ptcs.set.clean");
  const participantResourceAllows=(row, action) => aclAllows(action, "ptcs.participant", row.participantId)||systemAclAllows("*", action);
  const participantCanSelect=(row) => participantResourceAllows(row, "ptcs.management.participant.hide")||participantResourceAllows(row, "ptcs.management.participant.show")||participantResourceAllows(row, "ptcs.management.participant.delete")||walletAllows(readAction(), row.participantId)||walletAllows(topUpAction(), row.participantId);
  const currentPageRows=() => {
    const x=filteredPages();
    return sliceRows(pageIndex, pageSize, x);
  };
  const currentSetRows=() => {
    const x=filteredSets();
    return sliceRows(setPageIndex, setPageSize, x);
  };
  const currentParticipantRows=() => {
    const x=filteredParticipants();
    return sliceRows(participantPageIndex, participantPageSize, x);
  };
  const setButtonEnabled=(target, enabled) => enabled?target.removeAttribute("disabled"):target.setAttribute("disabled", "disabled");
  const updateHeaderSelection=(checkboxId, currentKeys, selected) => {
    const node=doc().getElementById(checkboxId);
    if(!(node==null)){
      const selectedCount=filter_1((key_1) => exists((y) => key_1==y, selected), currentKeys).length;
      node.checked=length(currentKeys)>0&&selectedCount===length(currentKeys);
      node.indeterminate=selectedCount>0&&selectedCount<length(currentKeys);
      return length(currentKeys)===0?node.setAttribute("disabled", "disabled"):node.removeAttribute("disabled");
    }
    else return null;
  };
  const updatePageSelectionControl=() => {
    const selectedRows=filter_1((row) => {
      const x=pageRowKey(row);
      return exists((y) => x==y, selectedPageKeys);
    }, allPages);
    pageSelectionCount.textContent=String(length(selectedRows))+" page"+(length(selectedRows)===1?"":"s")+" selected";
    setButtonEnabled(pageDeleteSelected, length(selectedRows)>0);
    setButtonEnabled(pageShowSelected, exists((row) =>!row.visible&&pageResourceAllows(row, "ptcs.management.page.show"), selectedRows));
    setButtonEnabled(pageHideSelected, exists((row) => row.visible&&pageResourceAllows(row, "ptcs.management.page.hide"), selectedRows));
    updateHeaderSelection("management-pages-select-current", map(pageRowKey, filter_1(pageCanSelect, currentPageRows())), selectedPageKeys);
  };
  const updateSetSelectionControl=() => {
    const selectedRows=filter_1((row) => {
      const x=row.keyId;
      return exists((y) => x==y, selectedSetKeys);
    }, allSets);
    setSelectionCount.textContent=String(length(selectedRows))+" set"+(length(selectedRows)===1?"":"s")+" selected";
    setButtonEnabled(setDeleteSelected, length(selectedRows)>0);
    updateHeaderSelection("management-sets-select-current", map((a) => a.keyId, filter_1(setCanSelect, currentSetRows())), selectedSetKeys);
  };
  const updateParticipantSelectionControl=() => {
    const selectedRows=filter_1((row) => {
      const x=row.participantId;
      return exists((y) => x==y, selectedParticipantKeys);
    }, allParticipants);
    walletController.SetSelection(map((a) => a.participantId, selectedRows));
    participantSelectionCount.textContent=String(length(selectedRows))+" participant"+(length(selectedRows)===1?"":"s")+" selected";
    setButtonEnabled(participantDeleteSelected, exists((row) => participantResourceAllows(row, "ptcs.management.participant.delete"), selectedRows));
    setButtonEnabled(participantShowSelected, exists((row) =>!row.visible&&participantResourceAllows(row, "ptcs.management.participant.show"), selectedRows));
    setButtonEnabled(participantHideSelected, exists((row) => row.visible&&participantResourceAllows(row, "ptcs.management.participant.hide"), selectedRows));
    updateHeaderSelection("management-participants-select-current", map((a) => a.participantId, filter_1(participantCanSelect, currentParticipantRows())), selectedParticipantKeys);
  };
  const updateCurrentSelection=(isChecked, currentKeys, selected) => isChecked?distinct(selected.concat(currentKeys)):filter_1((key_1) =>!exists((y) => Equals(key_1, y), currentKeys), selected);
  const applyPageProjection=() => {
    const filtered=filteredPages();
    const pages=pageCountFor(length(filtered), pageSize);
    const a=0;
    const a_1=pages-1;
    const b=Compare(a_1, pageIndex)===-1?a_1:pageIndex;
    pageIndex=Compare(a, b)===1?a:b;
    pageRows.Set(sliceRows(pageIndex, pageSize, filtered));
    pageCount.textContent=String(length(filtered))+" / "+String(length(allPages))+" page lineage(s)";
    pagePagerStatus.textContent="Page "+String(pageIndex+1)+" / "+String(pages);
    setHidden(pageIndex===0, pagePrevious);
    setHidden(pageIndex>=pages-1, pageNext);
    updatePageSelectionControl();
  };
  const applySetProjection=() => {
    const filtered=filteredSets();
    const pages=pageCountFor(length(filtered), setPageSize);
    const a=0;
    const a_1=pages-1;
    const b=Compare(a_1, setPageIndex)===-1?a_1:setPageIndex;
    setPageIndex=Compare(a, b)===1?a:b;
    setRows.Set(sliceRows(setPageIndex, setPageSize, filtered));
    setCount.textContent=String(length(filtered))+" / "+String(length(allSets))+" set bucket(s)";
    setPagerStatus.textContent="Page "+String(setPageIndex+1)+" / "+String(pages);
    setHidden(setPageIndex===0, setPrevious);
    setHidden(setPageIndex>=pages-1, setNext);
    updateSetSelectionControl();
  };
  const applyGroupProjection=() => {
    const filtered=filter_1((row) => matchesFilter(groupFilter, [row.groupId, row.displayName, row.ownerParticipantId]), allGroups);
    const pages=pageCountFor(length(filtered), groupPageSize);
    const a=0;
    const a_1=pages-1;
    const b=Compare(a_1, groupPageIndex)===-1?a_1:groupPageIndex;
    groupPageIndex=Compare(a, b)===1?a:b;
    groupRows.Set(sliceRows(groupPageIndex, groupPageSize, filtered));
    groupCount.textContent=String(length(filtered))+" / "+String(length(allGroups))+" group(s)";
    groupPagerStatus.textContent="Page "+String(groupPageIndex+1)+" / "+String(pages);
    setHidden(groupPageIndex===0, groupPrevious);
    setHidden(groupPageIndex>=pages-1, groupNext);
  };
  const applyParticipantProjection=() => {
    const filtered=filteredParticipants();
    const pages=pageCountFor(length(filtered), participantPageSize);
    const a=0;
    const a_1=pages-1;
    const b=Compare(a_1, participantPageIndex)===-1?a_1:participantPageIndex;
    participantPageIndex=Compare(a, b)===1?a:b;
    participantRows.Set(sliceRows(participantPageIndex, participantPageSize, filtered));
    participantCount.textContent=String(length(filtered))+" / "+String(length(allParticipants))+" participant(s)";
    participantPagerStatus.textContent="Page "+String(participantPageIndex+1)+" / "+String(pages);
    setHidden(participantPageIndex===0, participantPrevious);
    setHidden(participantPageIndex>=pages-1, participantNext);
    updateParticipantSelectionControl();
  };
  const refreshManagementNav=() => {
    getJson("/pages/api/definitions", (definitions) => {
      writeAppendPagesDefinitions(definitions);
      const nav=doc().getElementById("ptc-nav");
      if(!(nav==null))renderNav(nav, "/management", arrayOrEmpty(definitions.pages));
    }, (error_5) => {
      setStatus(pageCount, "Navigation refresh failed: "+error_5);
    });
  };
  function loadPages(){
    setStatus(pageCount, "Loading...");
    getJson("/management/api/pages", (reply) => {
      allPages=arrayOrEmpty(reply.pages);
      const availableKeys=map(pageRowKey, allPages);
      selectedPageKeys=filter_1((selected) => exists((available) => available==selected, availableKeys), selectedPageKeys);
      updatePageSelectionControl();
      applyPageProjection();
    }, (error_5) => {
      setStatus(pageCount, "Load failed: "+error_5);
    });
  }
  function mutatePage(endpoint){
    return(action) =>(row) => {
      const destructive=EndsWith(endpoint, "/delete");
      return!destructive||globalThis.confirm("Delete tab page '"+textOr(row.pageId, row.title)+"'? This cannot be undone for this page lineage.")?(setStatus(pageCount, action+" "+row.pageId+"..."),postJson(endpoint, New_34(row.pageId, row.tabId), () => {
        destructive?selectedPageKeys=filter_1((selected) => selected!=pageRowKey(row), selectedPageKeys):void 0;
        loadPages();
        refreshManagementNav();
      }, (error_5) => {
        setStatus(pageCount, action+" failed: "+error_5);
      })):null;
    };
  }
  function mutateSelectedPages(endpoint){
    return(action) =>(predicate) => {
      const rows=filter_1(predicate, filter_1((row) => {
        const x=pageRowKey(row);
        return exists((y) => x==y, selectedPageKeys);
      }, allPages));
      return length(rows)>0?(setStatus(pageCount, action+" "+String(length(rows))+" selected pages..."),postJson(endpoint, New_35(map((row) => New_34(row.pageId, row.tabId), rows)), () => {
        loadPages();
        refreshManagementNav();
      }, (error_5) => {
        setStatus(pageCount, action+" selected failed: "+error_5);
      })):null;
    };
  }
  function deleteSelectedPages(){
    const rows=filter_1((row) => {
      const x=pageRowKey(row);
      return exists((y) => x==y, selectedPageKeys);
    }, allPages);
    if(length(rows)>0&&globalThis.confirm("Delete "+String(length(rows))+" selected tab page lineages? This cannot be undone.")){
      setStatus(pageCount, "Deleting "+String(length(rows))+" selected pages...");
      postJson("/management/api/pages/delete-many", New_35(map((row) => New_34(row.pageId, row.tabId), rows)), () => {
        selectedPageKeys=[];
        updatePageSelectionControl();
        loadPages();
        refreshManagementNav();
      }, (error_5) => {
        setStatus(pageCount, "Delete selected failed: "+error_5);
      });
    }
  }
  function loadSets(){
    setStatus(setCount, "Loading...");
    getJson("/management/api/sets", (reply) => {
      allSets=arrayOrEmpty(reply.sets);
      const availableKeys=map((row) => row.keyId, allSets);
      selectedSetKeys=filter_1((selected) => exists((available) => available==selected, availableKeys), selectedSetKeys);
      updateSetSelectionControl();
      applySetProjection();
    }, (error_5) => {
      setStatus(setCount, "Load failed: "+error_5);
    });
  }
  function deleteSelectedSets(){
    const rows=filter_1((row) => {
      const x=row.keyId;
      return exists((y) => x==y, selectedSetKeys);
    }, allSets);
    if(length(rows)>0&&globalThis.confirm("Delete "+String(length(rows))+" selected set buckets from the current projection?")){
      setStatus(setCount, "Deleting "+String(length(rows))+" selected sets...");
      postJson("/management/api/sets/delete-many", New_36(map((row) => New_37(row.setName, arrayOrEmpty(row.keys)), rows)), () => {
        selectedSetKeys=[];
        updateSetSelectionControl();
        loadSets();
      }, (error_5) => {
        setStatus(setCount, "Delete selected failed: "+error_5);
      });
    }
  }
  function loadGroups(){
    setStatus(groupCount, "Loading...");
    getJson("/management/api/groups", (reply) => {
      allGroups=arrayOrEmpty(reply.groups);
      applyGroupProjection();
    }, (error_5) => {
      setStatus(groupCount, "Load failed: "+error_5);
    });
  }
  function deleteGroup(row){
    if(row.canDelete&&groupAclAllows(row.groupId, "ptcs.group.delete")&&globalThis.confirm("Delete group '"+textOr(row.groupId, row.displayName)+"'?")){
      setStatus(groupCount, "Deleting "+row.groupId+"...");
      postJson("/chat/api/groups/delete", New_38(newRequestId("management-group-delete"), row.groupId, row.revision, "", "", "", "", null), () => {
        loadGroups();
      }, (error_5) => {
        setStatus(groupCount, "Delete failed: "+error_5);
      });
    }
  }
  function loadParticipants(){
    setStatus(participantCount, "Loading...");
    getJson("/management/api/participants", (reply) => {
      allParticipants=arrayOrEmpty(reply.participants);
      const availableKeys=map((a) => a.participantId, allParticipants);
      selectedParticipantKeys=filter_1((selected) => exists((y) => selected==y, availableKeys), selectedParticipantKeys);
      applyParticipantProjection();
    }, (error_5) => {
      setStatus(participantCount, "Load failed: "+error_5);
    });
  }
  function mutateParticipant(endpoint){
    return(action) =>(row) => {
      const destructive=EndsWith(endpoint, "/delete");
      return!destructive||globalThis.confirm("Delete participant '"+row.participantId+"' and existing inbound direct messages?")?(setStatus(participantCount, action+" "+row.participantId+"..."),postJson(endpoint, New_39(row.participantId), () => {
        let _3;
        if(destructive){
          const x=row.participantId;
          selectedParticipantKeys=filter_1((y) => x!=y, selectedParticipantKeys);
          _3=loadSets();
        }
        else _3=void 0;
        loadParticipants();
      }, (error_5) => {
        setStatus(participantCount, action+" failed: "+error_5);
      })):null;
    };
  }
  function mutateSelectedParticipants(endpoint){
    return(action) =>(predicate) => {
      const rows=filter_1(predicate, filter_1((row) => {
        const x=row.participantId;
        return exists((y) => x==y, selectedParticipantKeys);
      }, allParticipants));
      return length(rows)>0?(setStatus(participantCount, action+" "+String(length(rows))+" selected participants..."),postJson(endpoint, New_40(map((row) => New_39(row.participantId), rows)), () => {
        loadParticipants();
      }, (error_5) => {
        setStatus(participantCount, action+" selected failed: "+error_5);
      })):null;
    };
  }
  function deleteSelectedParticipants(){
    const rows=filter_1((row) => participantResourceAllows(row, "ptcs.management.participant.delete"), filter_1((row) => {
      const x=row.participantId;
      return exists((y) => x==y, selectedParticipantKeys);
    }, allParticipants));
    if(length(rows)>0&&globalThis.confirm("Delete "+String(length(rows))+" selected participants, their inbound direct messages, and participant-scoped set projections?")){
      setStatus(participantCount, "Deleting "+String(length(rows))+" selected participants...");
      postJson("/management/api/participants/delete-many", New_40(map((row) => New_39(row.participantId), rows)), () => {
        selectedParticipantKeys=[];
        updateParticipantSelectionControl();
        loadParticipants();
        loadSets();
      }, (error_5) => {
        setStatus(participantCount, "Delete selected failed: "+error_5);
      });
    }
  }
  const pageHeaderSelector=Doc.Element("input", [Attr.Create("type", "checkbox"), Attr.Create("id", "management-pages-select-current"), Attr.Create("class", "management-row-selector"), Attr.Create("aria-label", "Select all tab pages on the current page"), Attr.Create("title", "Select or deselect the current page"), Attr.Create("data-testid", "management-pages-select-current"), Handler("change", (element_2) =>() => {
    selectedPageKeys=updateCurrentSelection(element_2.checked, map(pageRowKey, filter_1(pageCanSelect, currentPageRows())), selectedPageKeys);
    iter((row) => {
      const node=doc().getElementById(pageSelectionElementId(row));
      if(!(node==null)){
        const x=pageRowKey(row);
        let _3=exists((y) => x==y, selectedPageKeys);
        node.checked=_3;
      }
      else void 0;
    }, currentPageRows());
    return updatePageSelectionControl();
  })], []);
  const setHeaderSelector=Doc.Element("input", [Attr.Create("type", "checkbox"), Attr.Create("id", "management-sets-select-current"), Attr.Create("class", "management-row-selector"), Attr.Create("aria-label", "Select all set contents on the current page"), Attr.Create("title", "Select or deselect the current page"), Attr.Create("data-testid", "management-sets-select-current"), Handler("change", (element_2) =>() => {
    selectedSetKeys=updateCurrentSelection(element_2.checked, map((a) => a.keyId, filter_1(setCanSelect, currentSetRows())), selectedSetKeys);
    iter((row) => {
      const node=doc().getElementById(setSelectionElementId(row));
      if(!(node==null)){
        const x=row.keyId;
        let _3=exists((y) => x==y, selectedSetKeys);
        node.checked=_3;
      }
      else void 0;
    }, currentSetRows());
    return updateSetSelectionControl();
  })], []);
  const participantHeaderSelector=Doc.Element("input", [Attr.Create("type", "checkbox"), Attr.Create("id", "management-participants-select-current"), Attr.Create("class", "management-row-selector"), Attr.Create("aria-label", "Select all participants on the current page"), Attr.Create("title", "Select or deselect the current page"), Attr.Create("data-testid", "management-participants-select-current"), Handler("change", (element_2) =>() => {
    selectedParticipantKeys=updateCurrentSelection(element_2.checked, map((a) => a.participantId, filter_1(participantCanSelect, currentParticipantRows())), selectedParticipantKeys);
    iter((row) => {
      const node=doc().getElementById(participantSelectionElementId(row));
      if(!(node==null)){
        const x=row.participantId;
        let _3=exists((y) => x==y, selectedParticipantKeys);
        node.checked=_3;
      }
      else void 0;
    }, currentParticipantRows());
    return updateParticipantSelectionControl();
  })], []);
  const pageTable=Doc.Element("table", [Attr.Create("class", "data-table management-table"), Attr.Create("data-testid", "management-pages-table")], [Doc.Element("thead", [], [Doc.Element("tr", [], [Doc.Element("th", [Attr.Create("class", "management-select-cell")], [pageHeaderSelector]), Doc.Element("th", [], [Doc.TextNode("Tab page")]), Doc.Element("th", [], [Doc.TextNode("Created")]), Doc.Element("th", [], [Doc.TextNode("Actions")])])]), Doc.Element("tbody", [], [Doc.Convert((row) => {
    const visibilityLabel=row.visible?"Hide":"Show";
    const visibilityEndpoint=row.visible?"/management/api/pages/hide":"/management/api/pages/show";
    const resourceAllows=(action) => pageAclAllows(row.pageId, action)||systemAclAllows("*", action);
    const key_1=pageRowKey(row);
    const canDelete=resourceAllows("ptcs.management.page.delete");
    const selectionControl=canDelete?Doc.Element("input", ofSeq_1(delay(() => append_2([Attr.Create("type", "checkbox")], delay(() => append_2([Attr.Create("class", "management-row-selector")], delay(() => append_2([Attr.Create("id", pageSelectionElementId(row))], delay(() => append_2([Attr.Create("aria-label", "Select tab page "+textOr(row.pageId, row.title))], delay(() => append_2([Attr.Create("data-testid", "management-page-select-"+row.pageId)], delay(() => append_2(exists((y) => key_1==y, selectedPageKeys)?[Attr.Create("checked", "checked")]:[], delay(() =>[Handler("change", (element_2) =>() => {
      selectedPageKeys=toggleSelection(key_1, element_2.checked, selectedPageKeys);
      return updatePageSelectionControl();
    })])))))))))))))), []):Doc.Empty;
    const visibilityButton=resourceAllows(row.visible?"ptcs.management.page.hide":"ptcs.management.page.show")?Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("data-testid", "management-page-visibility-"+row.pageId), Handler("click", () =>() =>((mutatePage(visibilityEndpoint))(visibilityLabel))(row))], [Doc.TextNode(visibilityLabel)]):Doc.Empty;
    const deleteButton=canDelete?Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("class", "management-delete"), Attr.Create("data-testid", "management-page-delete-"+row.pageId), Handler("click", () =>() =>((mutatePage("/management/api/pages/delete"))("Delete"))(row))], [Doc.TextNode("Delete")]):Doc.Empty;
    return Doc.Element("tr", [Attr.Create("data-page-id", row.pageId), Attr.Create("data-visible", String(row.visible).toLowerCase())], [Doc.Element("td", [Attr.Create("class", "management-select-cell"), Attr.Create("data-label", "Select")], [selectionControl]), Doc.Element("td", [Attr.Create("data-label", "Tab page")], [Doc.Element("strong", [], [Doc.TextNode(textOr(row.pageId, row.title))]), Doc.Element("div", [Attr.Create("class", "management-secondary")], [Doc.TextNode(row.pageId)]), Doc.Element("div", [Attr.Create("class", "management-secondary")], [Doc.TextNode("tab: "+row.tabId)])]), Doc.Element("td", [Attr.Create("data-label", "Created")], [Doc.TextNode(row.createdAt)]), Doc.Element("td", [Attr.Create("class", "management-actions"), Attr.Create("data-label", "Actions")], [visibilityButton, deleteButton])]);
  }, pageRows.v)])]);
  const setTable=Doc.Element("table", [Attr.Create("class", "data-table management-table"), Attr.Create("data-testid", "management-sets-table")], [Doc.Element("thead", [], [Doc.Element("tr", [], [Doc.Element("th", [Attr.Create("class", "management-select-cell")], [setHeaderSelector]), Doc.Element("th", [], [Doc.TextNode("Set")]), Doc.Element("th", [], [Doc.TextNode("Values")]), Doc.Element("th", [], [Doc.TextNode("Updated")])])]), Doc.Element("tbody", [], [Doc.Convert((row) => {
    const selectionControl=systemAclAllows("sets", "ptcs.set.clean")||systemAclAllows("*", "ptcs.set.clean")?Doc.Element("input", ofSeq_1(delay(() => append_2([Attr.Create("type", "checkbox")], delay(() => append_2([Attr.Create("class", "management-row-selector")], delay(() => append_2([Attr.Create("id", setSelectionElementId(row))], delay(() => append_2([Attr.Create("aria-label", "Select set "+row.setName)], delay(() => append_2([Attr.Create("data-testid", "management-set-select-"+row.keyId)], delay(() => {
      const x=row.keyId;
      let _3=exists((y) => x==y, selectedSetKeys)?[Attr.Create("checked", "checked")]:[];
      return append_2(_3, delay(() =>[Handler("change", (element_2) =>() => {
        selectedSetKeys=toggleSelection(row.keyId, element_2.checked, selectedSetKeys);
        return updateSetSelectionControl();
      })]));
    })))))))))))), []):Doc.Empty;
    return Doc.Element("tr", [Attr.Create("data-set-key-id", row.keyId)], [Doc.Element("td", [Attr.Create("class", "management-select-cell"), Attr.Create("data-label", "Select")], [selectionControl]), Doc.Element("td", [Attr.Create("data-label", "Set")], [Doc.Element("strong", [], [Doc.TextNode(row.setName)]), Doc.Element("div", [Attr.Create("class", "management-secondary")], [Doc.TextNode(concat_1(" + ", arrayOrEmpty(row.keys)))])]), Doc.Element("td", [Attr.Create("data-label", "Values")], [Doc.TextNode(String(row.valueCount))]), Doc.Element("td", [Attr.Create("data-label", "Updated")], [Doc.TextNode(row.updatedAt)])]);
  }, setRows.v)])]);
  const groupTable=Doc.Element("table", [Attr.Create("class", "data-table management-table"), Attr.Create("data-testid", "management-groups-table")], [Doc.Element("thead", [], [Doc.Element("tr", [], [Doc.Element("th", [], [Doc.TextNode("Group")]), Doc.Element("th", [], [Doc.TextNode("Owner")]), Doc.Element("th", [], [Doc.TextNode("Members")]), Doc.Element("th", [], [Doc.TextNode("Updated")]), Doc.Element("th", [], [Doc.TextNode("Actions")])])]), Doc.Element("tbody", [], [Doc.Convert((row) => {
    const deleteButton=row.canDelete&&groupAclAllows(row.groupId, "ptcs.group.delete")?Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("class", "management-delete"), Attr.Create("data-testid", "management-group-delete-"+row.groupId), Handler("click", () =>() => deleteGroup(row))], [Doc.TextNode("Delete")]):Doc.Empty;
    return Doc.Element("tr", [Attr.Create("data-group-id", row.groupId)], [Doc.Element("td", [Attr.Create("data-label", "Group")], [Doc.Element("strong", [], [Doc.TextNode(textOr(row.groupId, row.displayName))]), Doc.Element("div", [Attr.Create("class", "management-secondary")], [Doc.TextNode(row.groupId)])]), Doc.Element("td", [Attr.Create("data-label", "Owner")], [Doc.TextNode(row.ownerParticipantId)]), Doc.Element("td", [Attr.Create("data-label", "Members")], [Doc.TextNode(String(row.activeMemberCount))]), Doc.Element("td", [Attr.Create("data-label", "Updated")], [Doc.TextNode(row.updatedAt)]), Doc.Element("td", [Attr.Create("class", "management-actions"), Attr.Create("data-label", "Actions")], [deleteButton])]);
  }, groupRows.v)])]);
  const participantTable=Doc.Element("table", [Attr.Create("class", "data-table management-table"), Attr.Create("data-testid", "management-participants-table")], [Doc.Element("thead", [], [Doc.Element("tr", [], [Doc.Element("th", [Attr.Create("class", "management-select-cell")], [participantHeaderSelector]), Doc.Element("th", [], [Doc.TextNode("Participant")]), Doc.Element("th", [], [Doc.TextNode("Registered")]), Doc.Element("th", [], [Doc.TextNode("Last seen")]), Doc.Element("th", [], [Doc.TextNode("Actions")])])]), Doc.Element("tbody", [], [Doc.Convert((row) => {
    const visibilityLabel=row.visible?"Hide":"Show";
    const visibilityEndpoint=row.visible?"/management/api/participants/hide":"/management/api/participants/show";
    const resourceAllows=(action) => aclAllows(action, "ptcs.participant", row.participantId)||systemAclAllows("*", action);
    const selectionControl=participantCanSelect(row)?Doc.Element("input", ofSeq_1(delay(() => append_2([Attr.Create("type", "checkbox")], delay(() => append_2([Attr.Create("class", "management-row-selector")], delay(() => append_2([Attr.Create("id", participantSelectionElementId(row))], delay(() => append_2([Attr.Create("aria-label", "Select participant "+row.participantId)], delay(() => append_2([Attr.Create("data-testid", "management-participant-select-"+row.participantId)], delay(() => {
      const x=row.participantId;
      let _3=exists((y) => x==y, selectedParticipantKeys)?[Attr.Create("checked", "checked")]:[];
      return append_2(_3, delay(() =>[Handler("change", (element_2) =>() => {
        selectedParticipantKeys=toggleSelection(row.participantId, element_2.checked, selectedParticipantKeys);
        return updateParticipantSelectionControl();
      })]));
    })))))))))))), []):Doc.Empty;
    const visibilityButton=resourceAllows(row.visible?"ptcs.management.participant.hide":"ptcs.management.participant.show")?Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("data-testid", "management-participant-visibility-"+row.participantId), Handler("click", () =>() =>((mutateParticipant(visibilityEndpoint))(visibilityLabel))(row))], [Doc.TextNode(visibilityLabel)]):Doc.Empty;
    const deleteButton=resourceAllows("ptcs.management.participant.delete")?Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("class", "management-delete"), Attr.Create("data-testid", "management-participant-delete-"+row.participantId), Handler("click", () =>() =>((mutateParticipant("/management/api/participants/delete"))("Delete"))(row))], [Doc.TextNode("Delete")]):Doc.Empty;
    return Doc.Element("tr", [Attr.Create("data-participant-id", row.participantId), Attr.Create("data-visible", String(row.visible).toLowerCase())], [Doc.Element("td", [Attr.Create("class", "management-select-cell"), Attr.Create("data-label", "Select")], [selectionControl]), Doc.Element("td", [Attr.Create("data-label", "Participant")], [Doc.Element("strong", [], [Doc.TextNode(textOr(row.participantId, row.displayName))]), Doc.Element("div", [Attr.Create("class", "management-secondary")], [Doc.TextNode(row.participantId)]), Doc.Element("div", [Attr.Create("class", "management-secondary")], [Doc.TextNode(row.kind+" / "+row.status)])]), Doc.Element("td", [Attr.Create("data-label", "Registered")], [Doc.TextNode(row.registeredAt)]), Doc.Element("td", [Attr.Create("data-label", "Last seen")], [Doc.TextNode(row.lastSeenAt)]), Doc.Element("td", [Attr.Create("class", "management-actions"), Attr.Create("data-label", "Actions")], [visibilityButton, deleteButton])]);
  }, participantRows.v)])]);
  LoadLocalTemplates("");
  Doc.RunById(pageTableHostId, pageTable);
  LoadLocalTemplates("");
  Doc.RunById(setTableHostId, setTable);
  LoadLocalTemplates("");
  Doc.RunById(groupTableHostId, groupTable);
  LoadLocalTemplates("");
  Doc.RunById(participantTableHostId, participantTable);
  pageShowSelected.addEventListener("click", () =>((mutateSelectedPages("/management/api/pages/show-many"))("Showing"))((row) =>!row.visible&&pageResourceAllows(row, "ptcs.management.page.show")));
  pageHideSelected.addEventListener("click", () =>((mutateSelectedPages("/management/api/pages/hide-many"))("Hiding"))((row) => row.visible&&pageResourceAllows(row, "ptcs.management.page.hide")));
  pageDeleteSelected.addEventListener("click", deleteSelectedPages);
  setDeleteSelected.addEventListener("click", deleteSelectedSets);
  participantShowSelected.addEventListener("click", () =>((mutateSelectedParticipants("/management/api/participants/show-many"))("Showing"))((row) =>!row.visible&&participantResourceAllows(row, "ptcs.management.participant.show")));
  participantHideSelected.addEventListener("click", () =>((mutateSelectedParticipants("/management/api/participants/hide-many"))("Hiding"))((row) => row.visible&&participantResourceAllows(row, "ptcs.management.participant.hide")));
  participantDeleteSelected.addEventListener("click", deleteSelectedParticipants);
  pagePrevious.addEventListener("click", () => {
    const a=0;
    const b=pageIndex-1;
    pageIndex=Compare(a, b)===1?a:b;
    return applyPageProjection();
  });
  pageNext.addEventListener("click", () => {
    pageIndex=pageIndex+1;
    return applyPageProjection();
  });
  pageSizeSelect.addEventListener("change", () => {
    pageSize=toInt(Number(pageSizeSelect.value));
    pageIndex=0;
    return applyPageProjection();
  });
  setPrevious.addEventListener("click", () => {
    const a=0;
    const b=setPageIndex-1;
    setPageIndex=Compare(a, b)===1?a:b;
    return applySetProjection();
  });
  setNext.addEventListener("click", () => {
    setPageIndex=setPageIndex+1;
    return applySetProjection();
  });
  setSizeSelect.addEventListener("change", () => {
    setPageSize=toInt(Number(setSizeSelect.value));
    setPageIndex=0;
    return applySetProjection();
  });
  groupPrevious.addEventListener("click", () => {
    const a=0;
    const b=groupPageIndex-1;
    groupPageIndex=Compare(a, b)===1?a:b;
    return applyGroupProjection();
  });
  groupNext.addEventListener("click", () => {
    groupPageIndex=groupPageIndex+1;
    return applyGroupProjection();
  });
  groupSizeSelect.addEventListener("change", () => {
    groupPageSize=toInt(Number(groupSizeSelect.value));
    groupPageIndex=0;
    return applyGroupProjection();
  });
  participantPrevious.addEventListener("click", () => {
    const a=0;
    const b=participantPageIndex-1;
    participantPageIndex=Compare(a, b)===1?a:b;
    return applyParticipantProjection();
  });
  participantNext.addEventListener("click", () => {
    participantPageIndex=participantPageIndex+1;
    return applyParticipantProjection();
  });
  participantSizeSelect.addEventListener("change", () => {
    participantPageSize=toInt(Number(participantSizeSelect.value));
    participantPageIndex=0;
    return applyParticipantProjection();
  });
  pageFilter.addEventListener("input", () => {
    pageIndex=0;
    return applyPageProjection();
  });
  setFilter.addEventListener("input", () => {
    setPageIndex=0;
    return applySetProjection();
  });
  groupFilter.addEventListener("input", () => {
    groupPageIndex=0;
    return applyGroupProjection();
  });
  participantFilter.addEventListener("input", () => {
    participantPageIndex=0;
    return applyParticipantProjection();
  });
  reload.addEventListener("click", () => {
    loadPages();
    loadSets();
    loadGroups();
    loadParticipants();
    walletController.Refresh();
    return refreshManagementNav();
  });
  loadPages();
  loadSets();
  loadGroups();
  loadParticipants();
}
function mountChat(page){
  let selected, cursor, polling, forceThreadReloadPending, participants, groups, selectedGroup, selectedThreadMessages, oldestSequence, hasOlderMessages, loadingOlderMessages, activityCursors, unreadMessages, activityReady, activityPolling, heldUnreadTarget, replayingPending, chatSocket, queuedChatSyncFrames, subscribedChatStream, pendingWsChatIds;
  selected="";
  cursor="";
  polling=false;
  forceThreadReloadPending=false;
  participants=[];
  groups=[];
  selectedGroup=null;
  selectedThreadMessages=[];
  oldestSequence=0n;
  hasOlderMessages=false;
  loadingOlderMessages=false;
  activityCursors=[];
  unreadMessages=[];
  activityReady=false;
  activityPolling=false;
  heldUnreadTarget="";
  const participantId=currentUserId();
  page.className="page chat-grid";
  const side=element("aside", "sidebar", null);
  const sideHead=element("div", "panel-head", null);
  const sideActions=setTestId("chat-actions", element("div", "head-actions chat-actions", null));
  const actionSelect=setTestId("chat-actions-select", select([["add-group", "Add group"], ["delete-group", "Delete group"], ["export-thread", "Export"], ["reload-thread", "Reload"]]));
  const actionExecute=setTestId("chat-actions-execute", button("primary", "Execute"));
  setData("message-count", "0", actionExecute);
  const list=element("div", "list", null);
  const groupCreatePanel=setHidden(true, setTestId("group-create-panel", element("div", "group-create-panel", null)));
  const groupIdInput=setTestId("group-create-id", input("group id"));
  const groupNameInput=setTestId("group-create-name", input("display name"));
  const groupHistoryInput=setTestId("group-create-history", select([["from-first-join", "History from join"], ["include-history-before-first-join", "Include existing history"]]));
  const groupCreateActions=element("div", "compact-actions", null);
  const groupCreateCancel=setTestId("group-create-cancel", button("", "Cancel"));
  const groupCreateConfirm=setTestId("group-create-confirm", button("primary", "Create"));
  append(groupCreateActions, [groupCreateCancel, groupCreateConfirm]);
  append(groupCreatePanel, [groupIdInput, groupNameInput, groupHistoryInput, groupCreateActions]);
  append(sideActions, [actionSelect, actionExecute]);
  append(sideHead, [element("h1", "", "Chat"), sideActions]);
  append(side, [sideHead, groupCreatePanel, list]);
  const work=setTestId("chat-work", element("section", "work", null));
  const workHead=element("div", "work-head", null);
  const titleBox=element("div", "", null);
  const toTitle=element("h2", "", "No participant selected");
  const state=element("div", "state", "Loading participants");
  const groupManagement=setHidden(true, setTestId("group-management", element("details", "group-management", null)));
  const groupManagementSummary=element("summary", "group-management-summary", "Group details");
  const groupManagementBody=element("div", "group-management-body", null);
  append(groupManagement, [groupManagementSummary, groupManagementBody]);
  const pendingState=setTestId("chat-pending-state", element("div", "state pending-state", ""));
  const thread=setTestId("thread-list", setId("thread-list", element("div", "thread-list", null)));
  thread.setAttribute("tabindex", "0");
  setData("follow-bottom", "true", thread);
  setData("has-older", "false", thread);
  const composer=setTestId("chat-composer", element("div", "chat-composer", null));
  const draft=setTestId("chat-draft", textarea("draft", "Type a message"));
  const actions=element("div", "actions", null);
  const send=setTestId("chat-send", button("primary", "Send"));
  const readOnlyView=currentBrowserUser().viewAsActive;
  setHidden(readOnlyView, composer);
  if(readOnlyView)work.className="work view-as-read-only";
  setData("activity-state", "loading", work);
  const participantsCacheKey=cacheKey("chat-participants-v2", ofArray([participantId]));
  const threadCacheKey=(peerId) => cacheKey("chat-thread", ofArray([participantId, peerId]));
  const activityCacheKey=cacheKey("chat-activity-v1", ofArray([participantId]));
  append(titleBox, [element("label", "", "To"), toTitle]);
  append(workHead, [titleBox, groupManagement, state]);
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
  const isGroupTarget=(value) => StartsWith(asText(value), "group:");
  const groupTarget=(groupId) =>"group:"+asText(groupId);
  const selectedGroupId=() => isGroupTarget(selected)?selected.substring("group:".length):"";
  const isAnnouncementGroup=(groupId) => sameText(groupId, "organization-announcements");
  const unreadFor=(target) => tryFind((row) => sameText(row.target, target), unreadMessages);
  const persistActivity=() => {
    writeJson(activityCacheKey, New_42(activityCursors, unreadMessages));
  };
  const setChatWsState=(value) => {
    setData("ws-state", value, work);
  };
  const chatStreamKey=(peerId) => New_6("", "set", "chat", sameText(peerId, "channel.public")?["channel:public"]:[participantId, peerId]);
  const streamIdentity=(streamKey) => concat_1("\n", [asText(streamKey.pageId), asText(streamKey.mode), asText(streamKey.setName), concat_1("\u001f", arrayOrEmpty(streamKey.keys))]);
  const renderMessageElement=(message) => {
    let route, x;
    const outbound=message.fromId==participantId;
    const wrap=setId("thread-"+message.messageId, element("div", outbound?"message outbound":"message inbound", null));
    setData("message-id", message.messageId, setTestId("chat-message", wrap));
    if(!isBlank(message.channelMessageId))setData("channel-message-id", message.channelMessageId, wrap);
    else null;
    const meta=element("div", "message-meta", null);
    if(message.scope=="public")route=outbound?"You -> Public":asText(message.fromId)+" -> Public";
    else if(message.scope=="group"){
      const o=tryFind((group_1) => sameText(groupTarget(group_1.groupId), selected), groups);
      if(o==null)x=null;
      else {
        const group=o.$0;
        let _1=textOr(group.groupId, group.displayName);
        x=Some(_1);
      }
      const v=selectedGroupId();
      const groupName=x==null?v:x.$0;
      route=outbound?"You -> "+groupName:asText(message.fromId)+" -> "+groupName;
    }
    else route=outbound?"You -> "+asText(message.toId):asText(message.fromId)+" -> You";
    const idNode=setData("full-message-id", message.messageId, element("span", "message-id", compactMessageId(message.messageId)+"  "+asText(message.createdAtUtc)));
    idNode.setAttribute("title", message.messageId+"  "+asText(message.createdAtUtc));
    append(meta, [element("span", "", route), idNode]);
    append(wrap, [meta, element("pre", "message-body", asText(message.body))]);
    return wrap;
  };
  function recF(recI, _1){
    while(true)
      switch(recI){
        case 0:
          let _2;
          let groupSendDenied;
          clear(list);
          if(length(groups)>0)list.appendChild(element("div", "list-section-title", "Groups"));
          else null;
          ((((a) =>(a_1) => {
            iter(a, a_1);
          })((group) => {
            const target=groupTarget(group.groupId);
            const item=button("list-card"+(target==selected?" active":"")+(unreadFor(target)!=null?" chat-unread":""), null);
            setData("group-id", group.groupId, setTestId("chat-group", item));
            setData("unread", unreadFor(target)!=null?"true":"false", item);
            item.appendChild(cardTitle(textOr(group.groupId, group.displayName), group.groupId, group.isActiveMember?group.role:"former", String(group.memberCount)+" member(s)"));
            item.addEventListener("click", () => {
              selected=target;
              heldUnreadTarget="";
              selectedGroup=null;
              cursor="";
              selectedThreadMessages=[];
              oldestSequence=0n;
              hasOlderMessages=false;
              loadingOlderMessages=false;
              setData("has-older", "false", thread);
              setData("message-count", "0", actionExecute);
              clear(thread);
              recF(0);
              loadGroupDetails();
              refreshChatPendingState();
              return pollThread(true);
            });
            list.appendChild(item);
          }))(groups));
          if(length(participants)>0)list.appendChild(element("div", "list-section-title", "People and channels"));
          else null;
          ((((a) =>(a_1) => {
            iter(a, a_1);
          })((p) => {
            const className="list-card"+(p.participantId==selected?" active":"")+(unreadFor(p.participantId)!=null?" chat-unread":"");
            const name=textOr(p.participantId, p.displayName);
            const line=asText(p.kind)+" / "+joinValues(p.labels);
            const item=button(className, null);
            setData("participant-id", p.participantId, setTestId("chat-participant", item));
            setData("unread", unreadFor(p.participantId)!=null?"true":"false", item);
            item.appendChild(cardTitle(name, p.participantId, p.status, line));
            item.addEventListener("click", () => {
              selected=p.participantId;
              heldUnreadTarget="";
              cursor="";
              selectedThreadMessages=[];
              oldestSequence=0n;
              hasOlderMessages=false;
              loadingOlderMessages=false;
              setData("has-older", "false", thread);
              setData("message-count", "0", actionExecute);
              clear(thread);
              recF(0);
              selectedGroup=null;
              setHidden(true, groupManagement);
              refreshChatPendingState();
              pollThread(true);
              return ensureSelectedChatSubscription();
            });
            list.appendChild(item);
          }))(participants));
          if(isGroupTarget(selected)){
            const x=(((p) =>(a) => tryFind(p, a))((group) => groupTarget(group.groupId)==selected))(groups);
            const o=(((m) =>(o_1) => o_1==null?null:Some(m(o_1.$0)))((group) => textOr(group.groupId, group.displayName)+" ("+selected+")"))(x);
            _2=o==null?selected:o.$0;
          }
          else {
            const x_1=(((p) =>(a) => tryFind(p, a))((p) => p.participantId==selected))(participants);
            const x_2=(((m) =>(o_1) => o_1==null?null:Some(m(o_1.$0)))((p) => textOr(p.participantId, p.displayName)+" ("+p.participantId+")"))(x_1);
            _2=(((v) =>(o_1) => o_1==null?v:o_1.$0)("No participant selected"))(x_2);
          }
          toTitle.textContent=_2;
          if(isGroupTarget(selected)){
            if(!groupAclAllows(selectedGroupId(), "ptcs.group.send"))groupSendDenied=true;
            else if(isAnnouncementGroup(selectedGroupId())){
              const x_3=(((m) =>(o_1) => o_1==null?null:Some(m(o_1.$0)))((group) => exists((member_) => member_.isActive&&sameText(member_.participantId, participantId)&&(sameText(member_.role, "owner")||sameText(member_.role, "admin")), arrayOrEmpty(group.members))))(selectedGroup);
              groupSendDenied=!(((v) =>(o_1) => o_1==null?v:o_1.$0)(false))(x_3);
            }
            else groupSendDenied=false;
          }
          else groupSendDenied=false;
          {
            ((((h) =>(n) => setHidden(h, n))(readOnlyView||groupSendDenied))(composer));
            return;
          }
          break;
        case 1:
          const socket=ensureChatSyncSocket();
          return Equals(socket.readyState, 1)?socket.send(_1):void(queuedChatSyncFrames=queuedChatSyncFrames.concat([_1]));
      }
  }
  function renderParticipants(){
    return recF(0);
  }
  function clearReadTarget(target){
    let _1;
    if(!sameText(heldUnreadTarget, target)){
      if(selected==target&&!doc().hidden&&isNearBottom(thread)){
        const o=unreadFor(target);
        if(o==null)_1=false;
        else {
          const row=o.$0;
          _1=!(doc().getElementById("thread-"+row.messageId)==null);
        }
      }
      else _1=false;
    }
    else _1=false;
    if(_1){
      unreadMessages=filter_1((row_1) =>!sameText(row_1.target, target), unreadMessages);
      persistActivity();
      renderParticipants();
    }
  }
  function pollActivity(){
    if(activityReady&&!activityPolling){
      activityPolling=true;
      setData("activity-busy", "true", work);
      postJson("/chat/api/activity", New_44(activityCursors), (reply) => {
        let changed, hasMore, selectedHasNewMessage, _1;
        activityPolling=false;
        setData("activity-busy", "false", work);
        setData("activity-state", "ready", work);
        changed=false;
        hasMore=false;
        selectedHasNewMessage=false;
        iter((stream) => {
          if(!(stream==null)&&!isBlank(stream.target)){
            hasMore=hasMore||stream.hasMore;
            const o=tryFind((row) => sameText(row.target, stream.target), activityCursors);
            let _2=o==null?null:Some(o.$0.sequence);
            if(!Equals(_2, Some(stream.nextSequence))){
              activityCursors=filter_1((row) =>!sameText(row.target, stream.target), activityCursors).concat([New_45(stream.target, stream.nextSequence)]);
              changed=true;
            }
            iter((event) => {
              if(!(event==null)&&!isBlank(event.target)&&!isBlank(event.messageId)){
                const o_1=unreadFor(event.target);
                let _3=o_1==null?null:Some(o_1.$0.messageId);
                if(!Equals(_3, Some(event.messageId))){
                  unreadMessages=filter_1((row) =>!sameText(row.target, event.target), unreadMessages).concat([New_41(event.target, event.messageId)]);
                  changed=true;
                  selectedHasNewMessage=selectedHasNewMessage||sameText(selected, event.target);
                }
              }
            }, arrayOrEmpty(stream.events));
          }
        }, arrayOrEmpty(reply.streams));
        if(changed){
          persistActivity();
          renderParticipants();
        }
        if(selectedHasNewMessage){
          const target=selected;
          _1=(heldUnreadTarget=target,pollThread(false),setTimeout(() => {
            sameText(heldUnreadTarget, target)?heldUnreadTarget="":void 0;
            clearReadTarget(target);
          }, 2200));
        }
        else _1=void 0;
        if(hasMore)setTimeout(() => {
          pollActivity();
        }, 100);
      }, () => {
        activityPolling=false;
        setData("activity-busy", "false", work);
        setData("activity-state", "retrying", work);
      });
    }
  }
  function mutateSelectedGroup(url, participantId_1, displayName, role, historyPolicy, includeHistory, onOk){
    if(selectedGroup!=null&&selectedGroup.$==1){
      const group=selectedGroup.$0;
      return postJson(url, New_38(newRequestId("group-mutation"), group.groupId, group.revision, asText(participantId_1), asText(displayName), asText(role), asText(historyPolicy), includeHistory), (reply) => {
        selectedGroup=Some(reply.group);
        renderGroupManagement();
        loadParticipants(false);
        onOk(reply.group);
      }, (error_5) => {
        setStatus(state, error_5);
      });
    }
    else return setStatus(state, "Group details are not loaded");
  }
  function renderGroupManagement(){
    let _1, _2, _3;
    clear(groupManagementBody);
    if(selectedGroup!=null&&selectedGroup.$==1){
      const group=selectedGroup.$0;
      setHidden(false, groupManagement);
      groupManagementSummary.textContent="Group details / "+group.displayName;
      const currentMember=tryFind((member_) => member_.isActive&&sameText(member_.participantId, participantId), arrayOrEmpty(group.members));
      const o=currentMember==null?null:Some(currentMember.$0.role);
      const currentRole=o==null?"former":o.$0;
      const isOwner=sameText(currentRole, "owner");
      const isAdmin=sameText(currentRole, "admin");
      const isAnnouncement=isAnnouncementGroup(group.groupId);
      const canManageMembers=(isOwner||isAdmin&&!isAnnouncement)&&groupAclAllows(group.groupId, "ptcs.group.member.manage");
      groupManagementBody.appendChild(element("div", "group-meta", "Role: "+String(currentRole)+" / revision "+String(group.revision)));
      if(isAnnouncement&&currentMember!=null&&!readOnlyView){
        const markRead=setTestId("announcement-mark-read", button("", "Mark read"));
        _1=(markRead.addEventListener("click", () => {
          const latest=tryLast(sortBy((a) => a.streamSequence, filter_1((message) =>!isBlank(message.channelMessageId), selectedThreadMessages)));
          return latest!=null&&latest.$==1?postJson("/chat/api/groups/read/ack", New_46(group.groupId, latest.$0.channelMessageId), (reply) => {
            setStatus(state, "Read through #"+String(reply.streamSequence));
          }, (t) => {
            setStatus(state, t);
          }):setStatus(state, "No announcement message to mark read");
        }),groupManagementBody.appendChild(markRead));
      }
      else _1=void 0;
      if(canManageMembers){
        const existing=map((member_) => member_.participantId, filter_1((a) => a.isActive, arrayOrEmpty(group.members)));
        const candidates=map((p) =>[p.participantId, textOr(p.participantId, p.displayName)], filter_1((p) => {
          if(!sameText(p.participantId, "channel.public")){
            const _4=p.participantId;
            return!exists((_5) => sameText(_4, _5), existing);
          }
          else return false;
        }, participants));
        if(length(candidates)>0){
          const chooser=setTestId("group-member-add-select", select(candidates));
          const addMember=setTestId("group-member-add", button("", "Add member"));
          addMember.addEventListener("click", () => mutateSelectedGroup("/chat/api/groups/member/add", chooser.value, "", "", "", null, () => { }));
          const row=element("div", "group-control-row", null);
          _2=(append(row, [chooser, addMember]),groupManagementBody.appendChild(row));
        }
        else _2=void 0;
      }
      else _2=void 0;
      const memberList=element("div", "group-member-list", null);
      iter((member_) => {
        let _4, _5, _6;
        const row_1=setData("participant-id", member_.participantId, element("div", "group-member-row", null));
        const identity=element("span", "group-member-name", member_.participantId+" / "+member_.role);
        const actions_1=element("span", "compact-actions", null);
        if(isOwner&&!sameText(member_.role, "owner")){
          if(groupAclAllows(group.groupId, "ptcs.group.role.manage")){
            if(!isAnnouncement||StartsWith(member_.participantId, "agent.")){
              const nextRole=sameText(member_.role, "admin")?"member":"admin";
              const roleButton=button("", nextRole=="admin"?"Promote":"Demote");
              _4=(setData("participant-id", member_.participantId, setTestId("group-member-role", roleButton)),roleButton.addEventListener("click", () => mutateSelectedGroup("/chat/api/groups/member/role", member_.participantId, "", nextRole, "", null, () => { })),actions_1.appendChild(roleButton));
            }
            else _4=void 0;
          }
          else _4=void 0;
          if(!isAnnouncement&&groupAclAllows(group.groupId, "ptcs.group.owner.transfer")){
            const transfer=setTestId("group-owner-transfer", button("", "Make owner"));
            _5=(transfer.addEventListener("click", () => mutateSelectedGroup("/chat/api/groups/owner/transfer", member_.participantId, "", "", "", null, () => { })),actions_1.appendChild(transfer));
          }
          else _5=void 0;
        }
        else _5=void 0;
        if((isAnnouncement?isOwner&&!sameText(member_.role, "owner"):sameText(member_.participantId, participantId)&&!sameText(member_.role, "owner")||isOwner&&!sameText(member_.role, "owner")||isAdmin&&sameText(member_.role, "member"))&&groupAclAllows(group.groupId, "ptcs.group.member.manage")){
          const remove=setTestId("group-member-remove", button("icon-button", "x"));
          _6=(remove.setAttribute("title", "Remove member"),remove.addEventListener("click", () => mutateSelectedGroup("/chat/api/groups/member/remove", member_.participantId, "", "", "", null, () => {
            if(sameText(member_.participantId, participantId)){
              selected="channel.public";
              selectedGroup=null;
              setHidden(true, groupManagement);
            }
          })),actions_1.appendChild(remove));
        }
        else _6=void 0;
        append(row_1, [identity, actions_1]);
        memberList.appendChild(row_1);
      }, sortBy((member_) =>[member_.role, member_.participantId], filter_1((a) => a.isActive, arrayOrEmpty(group.members))));
      groupManagementBody.appendChild(memberList);
      if(isOwner&&!isAnnouncement){
        const ownerControls=element("div", "group-owner-controls", null);
        if(groupAclAllows(group.groupId, "ptcs.group.settings.manage")){
          const renameInput=setTestId("group-rename-input", input("group display name"));
          renameInput.value=group.displayName;
          const rename=setTestId("group-rename", button("", "Rename"));
          rename.addEventListener("click", () => mutateSelectedGroup("/chat/api/groups/rename", "", renameInput.value, "", "", null, () => { }));
          const history=setTestId("group-history-policy", select([["from-first-join", "History from join"], ["include-history-before-first-join", "Include existing history"]]));
          history.value=group.historyPolicy;
          const saveHistory=setTestId("group-history-save", button("", "Save history policy"));
          _3=(saveHistory.addEventListener("click", () => mutateSelectedGroup("/chat/api/groups/history-policy", "", "", "", history.value, null, () => { })),append(ownerControls, [renameInput, rename, history, saveHistory]));
        }
        else _3=void 0;
        groupManagementBody.appendChild(ownerControls);
      }
      else void 0;
    }
    else setHidden(true, groupManagement);
  }
  function loadGroupDetails(){
    if(isGroupTarget(selected)){
      const requestedTarget=selected;
      getJson("/chat/api/groups/get?groupId="+encodeURIComponent(selectedGroupId()), (reply) => {
        if(selected==requestedTarget){
          selectedGroup=Some(reply.group);
          renderGroupManagement();
          renderParticipants();
        }
      }, (error_5) => {
        selectedGroup=null;
        setHidden(true, groupManagement);
        setStatus(state, error_5);
      });
    }
  }
  function refreshGroupAclSnapshot(onReady){
    getJson("/acl/api/snapshot", (snapshot) => {
      set_currentAclSnapshotJson(JSON.stringify(snapshot));
      set_currentAclSnapshot(Some(snapshot));
      onReady();
    }, () => {
      onReady();
    });
  }
  function appendMessages(messages){
    let appendedCount;
    const shouldFollow=isNearBottom(thread);
    appendedCount=0;
    iter((message) => {
      if(!(message==null)&&!isBlank(message.messageId)&&doc().getElementById("thread-"+message.messageId)==null){
        appendedCount=appendedCount+1;
        thread.appendChild(renderMessageElement(message));
      }
    }, arrayOrEmpty(messages));
    selectedThreadMessages=distinctMessages(selectedThreadMessages.concat(arrayOrEmpty(messages)));
    setData("message-count", String(length(selectedThreadMessages)), actionExecute);
    if(appendedCount>0&&shouldFollow)scrollToBottomNow(thread);
    setData("follow-bottom", isNearBottom(thread)?"true":"false", thread);
  }
  function prependMessages(messages){
    let prependedCount;
    const previousHeight=thread.scrollHeight;
    const previousTop=thread.scrollTop;
    const anchor=thread.querySelector("[data-testid='chat-message']");
    prependedCount=0;
    iter((message) => {
      if(!(message==null)&&!isBlank(message.messageId)&&doc().getElementById("thread-"+message.messageId)==null){
        prependedCount=prependedCount+1;
        anchor==null?thread.appendChild(renderMessageElement(message)):thread.insertBefore(renderMessageElement(message), anchor);
      }
    }, arrayOrEmpty(messages));
    selectedThreadMessages=distinctMessages(arrayOrEmpty(messages).concat(selectedThreadMessages));
    setData("message-count", String(length(selectedThreadMessages)), actionExecute);
    if(prependedCount>0)thread.scrollTop=previousTop+thread.scrollHeight-previousHeight;
    setData("follow-bottom", isNearBottom(thread)?"true":"false", thread);
  }
  function loadParticipants(refreshSelectedThread){
    setStatus(state, "Loading participants");
    readJson(participantsCacheKey, (a) => {
      if(a!=null&&a.$==1)if(a.$0,length(participants)===0){
        const cached=a.$0;
        participants=arrayOrEmpty(cached.participants);
        groups=arrayOrEmpty(cached.groups);
        if(isBlank(selected))length(participants)>0?selected=get(participants, 0).participantId:length(groups)>0?selected=groupTarget(get(groups, 0).groupId):void 0;
        renderParticipants();
        setStatus(state, "Loaded "+String(length(participants))+" cached participant(s)");
        if(refreshSelectedThread){
          pollThread(true);
          ensureSelectedChatSubscription();
          replayPendingChatCommands();
        }
      }
    });
    getJson("/chat/api/participants", (data) => {
      participants=arrayOrEmpty(data.participants);
      groups=arrayOrEmpty(data.groups);
      writeSnapshotWithWatermark(participantsCacheKey, data, 0n, length(participants), "chat-participants");
      const selectedWasBlank=isBlank(selected);
      if(selectedWasBlank)length(participants)>0?selected=get(participants, 0).participantId:length(groups)>0?selected=groupTarget(get(groups, 0).groupId):void 0;
      if(isGroupTarget(selected))loadGroupDetails();
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
    if(isBlank(selected)){ }
    else if(polling){
      if(force)forceThreadReloadPending=true;
    }
    else {
      polling=true;
      const requestedPeer=selected;
      const cacheKey_1=threadCacheKey(requestedPeer);
      const finishPoll=(retryCurrent) => {
        polling=false;
        const retry=retryCurrent||forceThreadReloadPending;
        forceThreadReloadPending=false;
        if(retry&&!isBlank(selected))pollThread(true);
      };
      const fetchThread=(useCursor) => {
        let url;
        url="/chat/api/thread?participantId="+encodeURIComponent(participantId)+"&peerId="+encodeURIComponent(requestedPeer);
        if(useCursor&&!isBlank(cursor))url=url+"&afterMessageId="+encodeURIComponent(cursor);
        getJson(url, (data) => {
          if(!sameText(selected, requestedPeer))finishPoll(true);
          else {
            const messages=force&&!useCursor?latestArray(defaultRenderLimit(), data.messages):arrayOrEmpty(data.messages);
            if(force&&!useCursor){
              clear(thread);
              selectedThreadMessages=[];
              setData("follow-bottom", "true", thread);
            }
            appendMessages(messages);
            if(!isBlank(data.nextAfterMessageId))cursor=data.nextAfterMessageId;
            if(!useCursor){
              oldestSequence=data.oldestSequence;
              hasOlderMessages=data.hasOlderMessages;
              setData("has-older", hasOlderMessages?"true":"false", thread);
            }
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
              const o=cached==null?null:Some(cached.$0.oldestSequence);
              const cachedOldestSequence=o==null?0n:o.$0;
              const o_1=cached==null?null:Some(cached.$0.hasOlderMessages);
              const cachedHasOlderMessages=o_1==null?false:o_1.$0;
              const storedOldestSequence=useCursor?cachedOldestSequence:data.oldestSequence;
              const storedHasOlderMessages=useCursor?cachedHasOlderMessages:data.hasOlderMessages;
              readWatermark(cacheKey_1, (watermark) => {
                const a=watermark==null?0n:int64OrZero(watermark.$0.newestSequence);
                const b=maxMessageSequence(merged);
                let _3=Compare(a, b)===1?a:b;
                writeSnapshotWithWatermark(cacheKey_1, New_47(merged, nextAfterMessageId, storedOldestSequence, storedHasOlderMessages), _3, length(merged), "chat-thread");
              });
            });
            setStatus(state, String(useCursor?"Synced":"Loaded")+" "+String(length(messages))+" backend message(s)");
            clearReadTarget(requestedPeer);
            finishPoll(false);
          }
        }, (error_5) => {
          const stale=!sameText(selected, requestedPeer);
          if(!stale)setStatus(state, error_5);
          finishPoll(stale);
        });
      };
      if(force)readJson(cacheKey_1, (a) => {
        if(!sameText(selected, requestedPeer))finishPoll(true);
        else if(a==null)fetchThread(false);
        else {
          const cached=a.$0;
          const messages=latestArray(defaultRenderLimit(), cached.messages);
          appendMessages(messages);
          if(!isBlank(cached.nextAfterMessageId))cursor=cached.nextAfterMessageId;
          oldestSequence=cached.oldestSequence;
          hasOlderMessages=cached.hasOlderMessages;
          setData("has-older", hasOlderMessages?"true":"false", thread);
          setStatus(state, "Loaded "+String(length(messages))+" cached message(s); syncing missing tail");
          fetchThread(false);
        }
      });
      else fetchThread(!isBlank(cursor));
    }
  }
  function loadOlderThread(){
    if(!loadingOlderMessages&&hasOlderMessages&&oldestSequence>0n&&!isBlank(selected)){
      loadingOlderMessages=true;
      const requestedPeer=selected;
      const requestedBefore=oldestSequence;
      const url="/chat/api/thread?participantId="+encodeURIComponent(participantId)+"&peerId="+encodeURIComponent(requestedPeer)+"&beforeSequence="+String(requestedBefore);
      setStatus(state, "Loading older messages before "+String(requestedBefore));
      getJson(url, (data) => {
        selected==requestedPeer?(prependMessages(data.messages),oldestSequence=data.oldestSequence,hasOlderMessages=data.hasOlderMessages,setData("has-older", hasOlderMessages?"true":"false", thread),setStatus(state, "Loaded "+String(length(data.messages))+" older message(s)")):void 0;
        loadingOlderMessages=false;
      }, (error_5) => {
        loadingOlderMessages=false;
        setStatus(state, "Load older messages failed: "+error_5);
      });
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
  function cacheAcceptedChatMessage(sequence_1, message){
    if(!(message==null)&&!isBlank(message.messageId)&&!isBlank(selected)){
      const cacheKey_1=threadCacheKey(selected);
      return readJson(cacheKey_1, (cached) => {
        const merged=mergeThreadMessages(cached==null?[]:cached.$0.messages, [message]);
        const newestSequence=sequence_1>0n?sequence_1:maxMessageSequence(merged);
        const o=cached==null?null:Some(cached.$0.oldestSequence);
        let _1=o==null?oldestSequence:o.$0;
        const o_1=cached==null?null:Some(cached.$0.hasOlderMessages);
        let _2=o_1==null?hasOlderMessages:o_1.$0;
        let _3=New_47(merged, message.messageId, _1, _2);
        writeSnapshotWithWatermark(cacheKey_1, _3, newestSequence, length(merged), "chat-thread");
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
          exists((id_1) => id_1==requestId, pendingWsChatIds)?(pendingWsChatIds=filter_1((id_1) => id_1!=requestId, pendingWsChatIds),deletePendingThen(requestId, () => {
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
              o=Some(New_43(textOr(event_1.eventId, event_1.sourceId), "", 0n, "", participantId, "direct", asText(event_1.payload), asText(event_1.createdAtUtc)));
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
      else responseStatus=="error"?exists((id_1) => id_1==requestId, pendingWsChatIds)?(setStatus(state, pendingFailure("WebSocket chat send", asText(response.error))),refreshChatPendingState()):setStatus(state, "WebSocket chat error: "+asText(response.error)):null;
    }
    catch(error_5){
      setStatus(state, "WebSocket chat parse failed: "+errorMessage(error_5));
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
    return recF(1, frame);
  }
  function ensureSelectedChatSubscription(){
    if(!isBlank(selected)&&!isGroupTarget(selected)){
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
    else if(isGroupTarget(selected)){
      const request_1=New_49(newRequestId("group-send"), selectedGroupId(), body, ["web-chat"]);
      const pendingId=rememberPending("chat-send", participantId+"->"+selected, "/chat/api/groups/send", request_1);
      refreshChatPendingState();
      setStatus(state, "Sending group message; pending command saved in browser DB");
      postJson("/chat/api/groups/send", request_1, (reply) => {
        deletePendingThen(pendingId, () => {
          draft.value="";
          appendMessages([reply.message]);
          cacheAcceptedChatMessage(int64OrZero(reply.streamSequence), reply.message);
          cursor=reply.message.messageId;
          refreshChatPendingState();
          setStatus(state, "Sent "+compactMessageId(reply.message.messageId)+" "+asText(reply.deliveryHint));
        });
      }, (error_5) => {
        deletePendingThen(pendingId, () => {
          refreshChatPendingState();
          setStatus(state, error_5);
        });
      });
    }
    else {
      const request_2=New_51(participantId, selected, body, ["web-chat"]);
      const pendingId_1=rememberPending("chat-send", participantId+"->"+selected, "/chat/api/send", request_2);
      const wsRequest=New_50("chat-send", pendingId_1, participantId, selected, body, ["web-chat"], participantId, "chat");
      pendingWsChatIds=pendingWsChatIds.concat([pendingId_1]);
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
        const rows=map((message) => New_52(asText(message.messageId), asText(message.fromId), asText(message.createdAtUtc), asText(message.body)), selectedThreadMessages);
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
        setTimeout(() => {
          !(anchor.parentNode==null)?anchor.parentNode.removeChild(anchor):void 0;
          URL.revokeObjectURL(url);
        }, 5000);
        setStatus(state, "Exported "+String(length(rows))+" message(s)");
      }
      catch(error_5){
        setStatus(state, "Chat export failed: "+errorMessage(error_5));
      }
    }
  }
  actionExecute.addEventListener("click", () => {
    const m=actionSelect.value;
    if(m=="add-group")return groupAclAllows("*", "ptcs.group.create")?(setHidden(false, groupCreatePanel),groupIdInput.focus()):setStatus(state, "Add group is not authorized");
    else if(m=="delete-group"){
      if(selectedGroup!=null&&selectedGroup.$==1){
        const group=selectedGroup.$0;
        return!sameText(group.ownerParticipantId, participantId)||!groupAclAllows(group.groupId, "ptcs.group.delete")?(selectedGroup.$0,setStatus(state, "Only the group owner can delete this group")):globalThis.confirm("Delete group '"+selectedGroup.$0.displayName+"'?")?(selectedGroup.$0,mutateSelectedGroup("/chat/api/groups/delete", "", "", "", "", null, () => {
          selected="channel.public";
          selectedGroup=null;
          setHidden(true, groupManagement);
          loadParticipants(true);
        })):null;
      }
      else return setStatus(state, "Select an active group first");
    }
    else return m=="export-thread"?exportSelectedThread():m=="reload-thread"?(loadParticipants(true),pollThread(true)):setStatus(state, "Choose an action");
  });
  groupCreateCancel.addEventListener("click", () => {
    setHidden(true, groupCreatePanel);
  });
  groupCreateConfirm.addEventListener("click", () => {
    const groupId=Trim(groupIdInput.value);
    const displayName=Trim(groupNameInput.value);
    return isBlank(groupId)?setStatus(state, "Group id is required"):postJson("/chat/api/groups/create", New_53(newRequestId("group-create"), groupId, displayName, [], groupHistoryInput.value, ["web-chat"]), (reply) => {
      const createdTarget=groupTarget(reply.group.groupId);
      if(!exists((row) => sameText(row.target, createdTarget), activityCursors)){
        activityCursors=activityCursors.concat([New_45(createdTarget, "0")]);
        persistActivity();
      }
      selected=groupTarget(reply.group.groupId);
      selectedGroup=Some(reply.group);
      groupIdInput.value="";
      groupNameInput.value="";
      setHidden(true, groupCreatePanel);
      refreshGroupAclSnapshot(() => {
        loadParticipants(true);
        pollActivity();
        renderGroupManagement();
      });
    }, (t) => {
      setStatus(state, t);
    });
  });
  send.addEventListener("click", sendMessage);
  thread.addEventListener("scroll", () => {
    setData("follow-bottom", isNearBottom(thread)?"true":"false", thread);
    clearReadTarget(selected);
    try {
      return thread.scrollTop<=8?loadOlderThread():null;
    }
    catch(m){
      return null;
    }
  });
  draft.addEventListener("keydown", (event) => event.key=="Enter"&&!event.shiftKey?(event.preventDefault(),sendMessage()):null);
  readJson(activityCacheKey, (cached) => {
    let _1;
    setData("activity-cache", cached!=null?"hit":"miss", work);
    if(cached==null)_1=void 0;
    else {
      const value=cached.$0;
      _1=(activityCursors=arrayOrEmpty(value.cursors),unreadMessages=arrayOrEmpty(value.unread));
    }
    activityReady=true;
    renderParticipants();
    pollActivity();
  });
  globalThis.setInterval(() => pollThread(false), 2500);
  globalThis.setInterval(pollActivity, 5000);
  globalThis.setInterval(() => loadParticipants(false), 30000);
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
    const request_1=New_56(Trim(userName.value), password.value, config.returnUrl, keepSession.checked);
    if(isBlank(request_1.userName)||isBlank(request_1.password))setError("\u8acb\u8f38\u5165\u5e33\u865f\u8207\u5bc6\u78bc\u3002");
    else {
      errorBox.className="error-box";
      submit_1.setAttribute("disabled", "disabled");
      submit_1.textContent="\u767b\u5165\u4e2d";
      postJson(config.submitPath, request_1, (reply) => {
        const target=textOr(config.returnUrl, reply.returnUrl);
        globalThis.location.assign(target);
      }, (error_5) => {
        submit_1.removeAttribute("disabled");
        submit_1.textContent="\u767b\u5165\u4e26\u8fd4\u56de PTCS";
        setError(isBlank(error_5)?"\u767b\u5165\u5931\u6557\u3002\u8acb\u78ba\u8a8d\u5e33\u865f\u6216\u5bc6\u78bc\u3002":error_5);
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
  return node==null||isBlank(node.textContent)?New_55("/login/api/submit", "/login/api/session", "/login/logout", "/actors", "/actors", "ptc_login_session", "\u767b\u5165 PTCS", "\u4f7f\u7528 host \u63d0\u4f9b\u7684\u5e33\u865f\u767b\u5165\u3002\u6b0a\u9650\u7531\u767b\u5165\u5f8c\u53d6\u5f97\u7684 principal \u8207 ACL policy \u6c7a\u5b9a\u3002", "PTCS.Login", "ACL mode"):json(node.textContent);
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
function errorMessage(error_5){
  return error_5==null?"request failed":String(error_5);
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
function setTestId(id_1, node){
  !isBlank(id_1)?node.setAttribute("data-testid", id_1):void 0;
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
  const shapeText_1=asText(page.shape).toLowerCase();
  if(isActorArguPage(page)){
    if(shapeText_1=="fcell-chat")return"Actor Argu";
    else if(shapeText_1=="actor-argu")return"Actor Argu";
    else if(shapeText_1=="raw")return"Raw Actor Argu";
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
  (globalThis.fetch(url, options).then((response) => response.text().then((responseBody) => response.ok?onOk(json(isBlank(responseBody)?"{}":responseBody)):onError(isBlank(responseBody)?"POST "+String(url)+" "+String(response.status):responseBody))))["catch"]((error_5) => onError(errorMessage(error_5)));
}
function pendingFailure(action, error_5){
  return String(action)+" failed; pending command kept in browser DB: "+String(asText(error_5));
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
  (globalThis.fetch(url, options).then((response) => response.text().then((responseBody) => response.ok?onOk(responseBody):onError(isBlank(responseBody)?"POST "+String(url)+" "+String(response.status):responseBody))))["catch"]((error_5) => onError(errorMessage(error_5)));
}
function postJson(url, body, onOk, onError){
  const headers=new Headers();
  headers.set("Content-Type", "application/json");
  const options=requestOptions();
  options.method="POST";
  options.headers=headers;
  options.body=JSON.stringify(body);
  (globalThis.fetch(url, options).then((response) => response.text().then((responseBody) => response.ok?onOk(json(isBlank(responseBody)?"{}":responseBody)):onError(isBlank(responseBody)?"POST "+String(url)+" "+String(response.status):responseBody))))["catch"]((error_5) => onError(errorMessage(error_5)));
}
function postRemoveAppendPageKey(url, body, onOk, onError){
  const headers=new Headers();
  headers.set("Content-Type", "application/json");
  const options=requestOptions();
  options.method="POST";
  options.headers=headers;
  options.body=JSON.stringify(body);
  (globalThis.fetch(url, options).then((response) => response.text().then((responseBody) => response.ok?onOk(json(isBlank(responseBody)?"{}":responseBody)):onError(isBlank(responseBody)?"POST "+String(url)+" "+String(response.status):responseBody))))["catch"]((error_5) => onError(errorMessage(error_5)));
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
  const presentationContext=New_57(asText(definition.pageId), asText(definition.tabId), asText(value.valueId), asText(value.createdAtUtc), mode, arrayOrEmpty(value.tags), asText(value.rawValue));
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
        catch(error_5){
          actionFeedback.className="reply-presentation-action-feedback error";
          actionFeedback.textContent=textOr("Action failed", errorMessage(error_5));
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
        catch(error_5){
          inlineHost.appendChild(element("div", "reply-presentation-error", textOr("Reply presentation failed.", errorMessage(error_5))));
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
  return filter_1((_1) => _1[0]!="/management"||systemAclAllows("*", "ptcs.management.read"), [["/chat", "Chat"], ["/sets", "Sets"], ["/actors", "Actors"], ["/management", "Management"]]);
}
function setHref(href, node){
  node.setAttribute("href", href);
  return node;
}
function pageTypeClass(page){
  const shapeText_1=asText(page.shape).toLowerCase();
  if(isActorArguPage(page)){
    if(shapeText_1=="fcell-chat")return"actor-argu";
    else if(shapeText_1=="actor-argu")return"actor-argu";
    else if(shapeText_1=="raw")return"raw actor-argu";
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
  const shapeText_1=asText(page.shape).toLowerCase();
  if(isActorArguPage(page)){
    if(shapeText_1=="fcell-chat")return"aa";
    else if(shapeText_1=="actor-argu")return"aa";
    else if(shapeText_1=="raw")return"ra";
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
function setId(id_1, node){
  node.setAttribute("id", id_1);
  return node;
}
function currentProductLabel(){
  const node=doc().getElementById("ptc-comm-product-label");
  return node==null||isBlank(node.textContent)?"PTC.SPA":Trim(node.textContent);
}
function currentLogoutPath(){
  const path=currentBrowserUser().logoutPath;
  return isBlank(path)?"/chat/logout":path;
}
function renderViewAsControl(){
  const user=currentBrowserUser();
  const wrap=setTestId("view-as-control", element("div", "view-as-control", null));
  const toggle=setTestId("view-as-toggle", button("view-as-toggle", user.viewAsActive?"View as: "+textOr(user.viewAsParticipantId, user.displayName):"View as"));
  const panel=setHidden(true, setTestId("view-as-panel", element("div", "view-as-panel", null)));
  const chooser=setTestId("view-as-select", select([]));
  const apply_1=setTestId("view-as-apply", button("primary", "Apply"));
  const cancel_1=setTestId("view-as-cancel", button("", "Cancel"));
  const status=setTestId("view-as-status", element("span", "state view-as-status", ""));
  toggle.addEventListener("click", () => {
    const isHidden=panel.hasAttribute("hidden");
    setHidden(!isHidden, panel);
    return isHidden?(setStatus(status, "Loading participants..."),getJson("/management/api/view-as", (reply) => {
      clear(chooser);
      const own=doc().createElement("option");
      own.setAttribute("value", "");
      own.textContent="Own view ("+reply.actualParticipantId+")";
      chooser.appendChild(own);
      iter((participant) => {
        const option=doc().createElement("option");
        option.setAttribute("value", participant.participantId);
        option.textContent=textOr(participant.participantId, participant.displayName)+" ("+participant.participantId+")";
        chooser.appendChild(option);
      }, arrayOrEmpty(reply.participants));
      chooser.value=reply.viewAsParticipantId;
      setStatus(status, "Read-only conversation view");
    }, (error_5) => {
      setStatus(status, "Unable to load participants: "+error_5);
    })):null;
  });
  cancel_1.addEventListener("click", () => {
    setHidden(true, panel);
  });
  apply_1.addEventListener("click", () => {
    apply_1.setAttribute("disabled", "disabled");
    return postJson("/management/api/view-as", New_58(asText(chooser.value)), () => {
      globalThis.location.reload();
    }, (error_5) => {
      apply_1.removeAttribute("disabled");
      setStatus(status, "View as failed: "+error_5);
    });
  });
  const actions=element("div", "view-as-panel-actions", null);
  append(actions, [cancel_1, apply_1]);
  append(panel, [chooser, actions, status]);
  append(wrap, [toggle, panel]);
  setHidden(!user.authenticated||!systemAclAllows("*", "ptcs.management.view-as"), wrap);
  return wrap;
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
      }, (error_5) => {
        resetBinding();
        setStatus(status, error_5);
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
      const request_1=New_59(pageIdText, titleText, "", shape.value, p[0], p[1], "", "");
      const pendingId=rememberPending("append-page-register", textOr(titleText, pageIdText), "/pages/api/register-page", request_1);
      setStatus(status, "Saving");
      postJson("/pages/api/register-page", request_1, (reply) => {
        deletePendingThen(pendingId, () => {
          writeAppendPagesDefinitions(New_2(reply.status, length(arrayOrEmpty(reply.pages)), reply.maxSequence, reply.pages));
          refresh(reply.pages);
          reply.page==null?setStatus(status, "Saved"):(setStatus(status, "Saved "+pageTitle(reply.page)),globalThis.location.assign(navigationPathForCreatedPage(reply.page)));
        });
      }, (error_5) => {
        setStatus(status, pendingFailure("Create page", error_5));
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
            catch(error_5){
              setStatus(status, "Replay create page parse failed: "+errorMessage(error_5));
              finishOne();
            }
          }, (error_5) => {
            setStatus(status, pendingFailure("Replay create page", error_5));
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
function currentBrowserUser(){
  const userNode=doc().getElementById("ptc-comm-user");
  if(userNode==null||isBlank(userNode.textContent))return New_24("user.web", "Web User", "", false, "anonymous", "/chat/logout", "user.web", "", "", false);
  else {
    const user=json(userNode.textContent);
    return user==null||isBlank(user.participantId)?New_24("user.web", "Web User", "", false, "anonymous", "/chat/logout", "user.web", "", "", false):user;
  }
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
function cardTitle(title, id_1, status, line){
  const wrap=doc().createDocumentFragment();
  const row=element("div", "name-row", null);
  append(row, [statusDot(status), element("span", "strong wrap", title)]);
  wrap.appendChild(row);
  if(!isBlank(id_1))wrap.appendChild(element("div", "muted wrap", id_1));
  if(!isBlank(line))wrap.appendChild(element("div", "meta wrap", line));
  return wrap;
}
function statusDot(status){
  const node=element("span", isLive(status)?"status-dot online":"status-dot offline", null);
  node.setAttribute("title", asText(status));
  return node;
}
function currentAclSnapshot(){
  return _c_1.currentAclSnapshot;
}
function systemAclAllows(resourceId, action){
  return aclAllows(action, "ptcs.system", resourceId);
}
function aclAllows(action, resourceKind, resourceId){
  const m=tryAclCapabilityProvider(action, resourceKind, resourceId);
  return m==null?aclAllowsFallback(action, resourceKind, resourceId):m.$0;
}
function groupAclAllows(groupId, action){
  return aclAllows(action, "ptcs.group", groupId)||systemAclAllows("*", action);
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
function field(labelText, inputId, control_1){
  const wrap=element("div", "field", null);
  const label=element("label", "", labelText);
  label.setAttribute("for", inputId);
  append(wrap, [label, control_1]);
  return wrap;
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
function navigationPathForCreatedPage(page){
  const pageId=asText(page.pageId);
  const path=asText(page.path);
  return exists((alias) => sameTextInvariant(path, alias), ["/fcell-chat", "/fcell-list", "/fcell-grid"])?path:"/page/"+pageId;
}
function isLive(status){
  const m=asText(status).toLowerCase();
  return m=="online"||(m=="running"||(m=="up"||m=="available"));
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
  return New_54(normalizeShapeText(shape), textOr(normalizeShapeText(shape), label), textOr("?", badge), textOr(normalizeShapeText(shape), className));
}
function serverClientExtensions(){
  const node=doc().getElementById("ptc-comm-client-extensions");
  if(node==null||isBlank(node.textContent))return[];
  else {
    const o=tryJson(node.textContent);
    return o==null?[]:o.$0;
  }
}
function toInt(x){
  const u=toUInt(x);
  return u>2147483647?u-4294967296:u;
}
function FailWith(msg){
  throw new Error(msg);
}
function toUInt(x){
  return(x<0?Math.ceil(x):Math.floor(x))>>>0;
}
function KeyValue(kvp){
  return[kvp.K, kvp.V];
}
function InvalidOp(msg){
  throw new InvalidOperationException("New", msg);
}
function range(min, max_2){
  const count=1+max_2-min;
  return count<=0?[]:init_1(count, (x) => x+min);
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
    for(var k_1 in o)if(((key_1) => {
      h[0]=____(____(h[0], hashString(key_1)), Hash(o[key_1]));
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
  return mountWithOptionsAndDisplayTimeZone(mountDocument, extensionId, channelId, canvasId, lifecycleOptions, _c_2.Create_1({$:0}).View);
}
function mountWithOptionsAndDisplayTimeZone(mountDocument, extensionId, channelId, canvasId, lifecycleOptions, displayTimeZone){
  return mountCoreWithDisplayTimeZone(mountDocument, extensionId, channelId, canvasId, lifecycleOptions, displayTimeZone, false);
}
function mountCoreWithDisplayTimeZone(mountDocument, extensionId, channelId, canvasId, lifecycleOptions, displayTimeZone, disposeAfterJsonExport){
  let socket, requestSequence, lifecycle, pollTimer, timeoutTimer, reconnectTimer, jsonExportRequested, jsonExportBootstrapAttempts, jsonExportBootstrapInFlight, jsonExportInFlight, actionRequestOverride, pendingActionCompletion;
  const identity={DocumentId:{$:0, $0:"pending-"+channelId}, CanvasInstanceId:{$:0, $0:canvasId}};
  const runtimeState=_c_2.Create_1({
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
    const text_1=JSON.stringify(New_67("extension-transient", requestId, extensionId, channelId, operation, JSON.stringify(payload)));
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
  function apply_1(event){
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
          const request_1=actionRequestOverride.$0;
          p=(actionRequestOverride=null,[request_1.RequestId, actionRequestToWire(request_1)]);
        }
        if(!sendPayloadWithRequestId(p[0], "action", p[1]))apply_1(Disconnected);
      }
      else if(effect.$==3){
        const delayMs=effect.$0;
        cancelPollTimer();
        pollTimer=Some(setTimeout(() => {
          pollTimer=null;
          apply_1(PollDue({d:Date.now(), o:0}));
        }, delayMs));
      }
      else if(effect.$==4){
        const delayMs_1=effect.$0;
        cancelTimeoutTimer();
        timeoutTimer=Some(setTimeout(() => {
          timeoutTimer=null;
          apply_1(RequestTimedOut({d:Date.now(), o:0}));
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
      else if(!sendPayload("open", emptyFrame("mounted", "", canvasId)))apply_1(Disconnected);
    }
  }
  function connect(){
    if(!lifecycle.Disposed){
      const value=new WebSocket(syncWebSocketUrl_1());
      socket=Some(value);
      value.onopen=() => {
        apply_1(Connected);
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
              apply_1(ResyncRequired("invalid-browser-state"));
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
              apply_1(_9);
              completePendingAction(response.requestId, {
                $:0, 
                $0:response.requestId, 
                $1:state.DocumentRevision
              });
              return jsonExportCompleted&&disposeAfterJsonExport?void apply_1(Dispose):tryStartJsonExport();
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
            apply_1(CommandRejected);
            return;
          }
          else return null;
        }
        catch(m_3){
          failPendingAction("invalid-transient-response", "The TA transient response could not be decoded.");
          apply_1(ResyncRequired("invalid-transient-response"));
          return;
        }
      };
      value.onclose=() => {
        socket=null;
        apply_1(Disconnected);
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
        if(disposeAfterJsonExport)apply_1(Dispose);
      }
      else if(exists((a) => a.$==2, apply_1(StartAction(hasRuntimeData?{
        $:10, 
        $0:identity.CanvasInstanceId, 
        $1:"json-export"
      }:{
        $:9, 
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
  mountDocument(renderWithDisplayTimeZone(defaultOptions(), {SubmitAction:(request_1) => FromContinuations((continuation) => pendingActionCompletion!=null?continuation(Error_1({Code:"transient-command-busy", Message:"A TA transient command is already in flight."})):(pendingActionCompletion=Some([request_1.RequestId, continuation]),actionRequestOverride=Some(request_1),!exists((a) => a.$==2, apply_1(StartAction(request_1.Action)))?(actionRequestOverride=null,pendingActionCompletion=null,continuation(Error_1({Code:lifecycle.Connected?"transient-command-busy":"transient-channel-not-open", Message:lifecycle.Connected?"A TA transient command is already in flight.":"TA transient channel is not open."}))):null))}, displayTimeZone, runtimeState));
  connect();
  return New_1(runtimeState, (active) => {
    apply_1(ActiveChanged(active));
  }, () => jsonExportRequested||jsonExportBootstrapInFlight||jsonExportInFlight?Error_1("A TA Research JSON export is already pending."):lifecycle.Disposed||lifecycle.DisposePending?Error_1("The TA transient channel has already been disposed."):(jsonExportBootstrapAttempts=0,jsonExportRequested=true,tryStartJsonExport(),Ok("TA Research JSON export requested.")), () => {
    apply_1(Dispose);
  });
}
function syncWebSocketUrl_1(){
  return(globalThis.location.protocol=="https:"?"wss://":"ws://")+globalThis.location.host+"/sync/ws";
}
function downloadJsonExport(wire){
  if(globalThis.document.body==null)return Error_1("Document body is unavailable.");
  else try {
    const url=URL.createObjectURL(new Blob([JSON.stringify(New_72("ptcs-ta-research-export.v1", (new Date()).toISOString(), wire.documentRevision, wire.dataRevision, wire))], {type:"application/json;charset=utf-8"}));
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
  catch(error_5){
    return Error_1(error_5.message);
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
  return New_68(canvasInstanceId, {$:0}, false, false, true, false, 0n, 0, false, false);
}
function transition(options, event, state){
  let _1;
  if(state.Disposed&&event.$!==9)return[state, []];
  else if(state.DisposePending)switch(event.$==1?0:event.$==5?1:event.$==6?1:event.$==9?2:3){
    case 0:
      return[New_68(state.CanvasInstanceId, {$:7}, event.$1, state.Connected, state.Active, true, event.$0, state.ReconnectAttempt, state.DisposePending, state.Disposed), [CancelPoll, CancelTimeout, CancelReconnect, SendUnmounted, ScheduleTimeout(options.RequestTimeoutMs)]];
    case 1:
      return[New_68(state.CanvasInstanceId, {$:7}, state.PollEnabled, false, state.Active, false, state.DataRevision, state.ReconnectAttempt, false, true), [CancelPoll, CancelTimeout, CancelReconnect, CloseTransport]];
    case 2:
      return[state, []];
    case 3:
      return[state, []];
  }
  else switch(event.$==1?(_1=[event.$1, event.$0],1):event.$==2?state.Connected&&state.InFlight?2:11:event.$==3?(event.$0,state.Connected&&state.Active&&!state.InFlight?(_1=event.$0,3):11):event.$==4?state.Connected&&state.Active&&state.PollEnabled&&!state.InFlight?4:11:event.$==5?state.InFlight?5:11:event.$==6?!state.Connected?6:7:event.$==7?(_1=event.$0,8):event.$==8?(event.$0,state.Connected&&state.Active?(_1=event.$0,9):11):event.$==9?10:0){
    case 0:
      return[New_68(state.CanvasInstanceId, {$:1}, state.PollEnabled, true, state.Active, true, state.DataRevision, 0, state.DisposePending, state.Disposed), [CancelReconnect, SendMounted, ScheduleTimeout(options.RequestTimeoutMs)]];
    case 1:
      const pollEnabled=_1[0];
      return[New_68(state.CanvasInstanceId, state.Active&&pollEnabled?{$:2}:{$:5}, pollEnabled, state.Connected, state.Active, false, _1[1], state.ReconnectAttempt, state.DisposePending, state.Disposed), [CancelTimeout].concat(state.Active&&pollEnabled?[SchedulePoll(options.PollIntervalMs)]:[])];
    case 2:
      return[New_68(state.CanvasInstanceId, state.Active&&state.PollEnabled?{$:2}:{$:5}, state.PollEnabled, state.Connected, state.Active, false, state.DataRevision, state.ReconnectAttempt, state.DisposePending, state.Disposed), [CancelTimeout].concat(state.Active&&state.PollEnabled?[SchedulePoll(options.PollIntervalMs)]:[])];
    case 3:
      return[New_68(state.CanvasInstanceId, {$:3}, state.PollEnabled, state.Connected, state.Active, true, state.DataRevision, state.ReconnectAttempt, state.DisposePending, state.Disposed), [CancelPoll, SendAction(_1), ScheduleTimeout(options.RequestTimeoutMs)]];
    case 4:
      return[New_68(state.CanvasInstanceId, {$:3}, state.PollEnabled, state.Connected, state.Active, true, state.DataRevision, state.ReconnectAttempt, state.DisposePending, state.Disposed), [SendAction({
        $:9, 
        $0:state.CanvasInstanceId, 
        $1:state.DataRevision
      }), ScheduleTimeout(options.RequestTimeoutMs)]];
    case 5:
      const attempt=state.ReconnectAttempt+1;
      return[New_68(state.CanvasInstanceId, {$:5}, state.PollEnabled, false, state.Active, false, state.DataRevision, attempt, state.DisposePending, state.Disposed), [CancelPoll, CancelTimeout, CancelReconnect, CloseTransport, ScheduleReconnect(reconnectDelay(options, attempt))]];
    case 6:
      return[state, []];
    case 7:
      const attempt_1=state.ReconnectAttempt+1;
      return[New_68(state.CanvasInstanceId, {$:5}, state.PollEnabled, false, state.Active, false, state.DataRevision, attempt_1, state.DisposePending, state.Disposed), [CancelPoll, CancelTimeout, ScheduleReconnect(reconnectDelay(options, attempt_1))]];
    case 8:
      return _1&&state.Connected&&state.PollEnabled&&!state.InFlight?[New_68(state.CanvasInstanceId, {$:2}, state.PollEnabled, state.Connected, true, state.InFlight, state.DataRevision, state.ReconnectAttempt, state.DisposePending, state.Disposed), [SchedulePoll(options.PollIntervalMs)]]:_1?[New_68(state.CanvasInstanceId, state.Poll, state.PollEnabled, state.Connected, true, state.InFlight, state.DataRevision, state.ReconnectAttempt, state.DisposePending, state.Disposed), []]:[New_68(state.CanvasInstanceId, {$:5}, state.PollEnabled, state.Connected, false, state.InFlight, state.DataRevision, state.ReconnectAttempt, state.DisposePending, state.Disposed), [CancelPoll]];
    case 9:
      return[New_68(state.CanvasInstanceId, {$:6}, state.PollEnabled, state.Connected, state.Active, true, state.DataRevision, state.ReconnectAttempt, state.DisposePending, state.Disposed), [CancelPoll, CancelTimeout, SendAction({
        $:10, 
        $0:state.CanvasInstanceId, 
        $1:_1
      }), ScheduleTimeout(options.RequestTimeoutMs)]];
    case 10:
      return state.Connected?[New_68(state.CanvasInstanceId, {$:7}, state.PollEnabled, state.Connected, false, true, state.DataRevision, state.ReconnectAttempt, true, state.Disposed), ofSeq(delay(() => append_2([CancelPoll], delay(() => append_2([CancelReconnect], delay(() =>!state.InFlight?append_2([SendUnmounted], delay(() =>[ScheduleTimeout(options.RequestTimeoutMs)])):[]))))))]:[New_68(state.CanvasInstanceId, {$:7}, state.PollEnabled, false, false, false, state.DataRevision, state.ReconnectAttempt, state.DisposePending, true), [CancelPoll, CancelTimeout, CancelReconnect, CloseTransport]];
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
    return Create_2(Attr, {$:3, $0:init_2});
  }
  static Concat(xs){
    const x=ofSeqNonCopying(xs);
    return TreeReduce(EmptyAttr(), (_1, _2) => AppendTree(_1, _2), x);
  }
  static A2(Item1, Item2){
    return Create_2(Attr, {
      $:2, 
      $0:Item1, 
      $1:Item2
    });
  }
  static A1(Item){
    return Create_2(Attr, {$:1, $0:Item});
  }
  static A4(onAfterRender){
    return Create_2(Attr, {$:4, $0:onAfterRender});
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
function tryLast(arr){
  const len=arr.length;
  return len===0?null:Some(arr[len-1]);
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
function iter2(f, arr1, arr2){
  checkLength(arr1, arr2);
  for(let i=0, _1=arr1.length-1;i<=_1;i++)f(arr1[i], arr2[i]);
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
function concat(xs){
  return Array.prototype.concat.apply([], ofSeq(xs));
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
function pick(f, arr){
  const m=tryPick(f, arr);
  return m==null?FailWith("KeyNotFoundException"):m.$0;
}
function init(size, f){
  if(size<0)FailWith("Negative size given.");
  else null;
  const r=new Array(size);
  for(let i=0, _1=size-1;i<=_1;i++)r[i]=f(i);
  return r;
}
function tryItem(i, arr){
  return arr.length<=i||i<0?null:Some(arr[i]);
}
function indexed(ar){
  return mapi((_1, _2) =>[_1, _2], ar);
}
function create(size, value){
  const r=new Array(size);
  for(let i=0, _1=size-1;i<=_1;i++)r[i]=value;
  return r;
}
function sumBy(f, arr){
  let sum=0;
  for(let i=0;i<arr.length;i++)sum+=f(arr[i]);
  return sum;
}
function sort(arr){
  return map((t) => t[0], mapi((_1, _2) =>[_2, _1], arr).sort(Compare));
}
function pairwise(a){
  return ofSeq(pairwise_1(a));
}
function sortByDescending(f, arr){
  return map((t) => t[0], mapi((_1, _2) =>[_2, [f(_2), _1]], arr).sort((_1, _2) =>-Compare(_1[1], _2[1])));
}
function map3(f, arr1, arr2, arr3){
  checkLength(arr1, arr2);
  checkLength(arr1, arr3);
  const r=new Array(arr3.length);
  for(let i=0, _1=arr3.length-1;i<=_1;i++)r[i]=f(arr1[i], arr2[i], arr3[i]);
  return r;
}
function find(f, arr){
  const m=tryFind(f, arr);
  return m==null?FailWith("KeyNotFoundException"):m.$0;
}
function take(n, ar){
  return n<0?nonNegative():n>ar.length?insufficient():ar.slice(0, n);
}
function map2(f, arr1, arr2){
  checkLength(arr1, arr2);
  const r=new Array(arr2.length);
  for(let i=0, _1=arr2.length-1;i<=_1;i++)r[i]=f(arr1[i], arr2[i]);
  return r;
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
function nonEmpty(arr){
  if(arr.length===0)FailWith("The input array was empty.");
}
function readJson(key_1, onRead){
  if(isBlank(key_1))onRead(null);
  else withStore(snapshotStore(), "readonly", (store) => {
    try {
      const request_1=store.get(key_1);
      request_1.onsuccess=(event) => {
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
      request_1.onerror=() => onRead(null);
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
    let _5=New_48(streamId, _3, _4, asText(source), nowTicks());
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
    const matching=filter_1((key_1) =>!isBlank(key_1)&&StartsWith(key_1, prefix), keys);
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
        return iter((key_1) => {
          snapshots["delete"](key_1);
          watermarks["delete"](key_1);
        }, matching);
      }
      catch(m){
        return finish();
      }
    })(_1))(_2))(_3), onDeleted);
  });
}
function clearRebuildableSnapshots(onFinished){
  let finished, database, transaction, cancelTimeout, removedSnapshots, removedWatermarks, retainedSnapshots, retainedWatermarks;
  const reality=currentServerRealityId();
  finished=false;
  database=null;
  transaction=null;
  cancelTimeout=() => { };
  removedSnapshots=0;
  removedWatermarks=0;
  retainedSnapshots=0;
  retainedWatermarks=0;
  const closeDatabase=(db) => {
    if(!isMissing(db)){
      try {
        db.close();
      }
      catch(m){
        null;
      }
    }
  };
  const finish=(result) => {
    if(!finished){
      finished=true;
      cancelTimeout();
      closeDatabase(database);
      onFinished(result);
    }
  };
  const fail=(message) => {
    if(!finished){
      let _1;
      if(!isMissing(transaction))try {
        _1=void transaction.abort();
      }
      catch(m){
        _1=null;
      }
      else _1=void 0;
      finish(Error_1(message));
    }
  };
  const timeout=setTimeout(() => {
    fail("\u700f\u89bd\u5668\u8cc7\u6599\u5eab\u5fd9\u788c\u6216\u6e05\u7406\u903e\u6642\uff0c\u8acb\u7a0d\u5f8c\u91cd\u8a66\u3002");
  }, 30000);
  cancelTimeout=() => {
    clearTimeout(timeout);
  };
  openDb((db) => {
    if(finished)closeDatabase(db);
    else {
      database=db;
      try {
        const a=[[snapshotStore(), watermarkStore()], "readwrite"];
        transaction=db.transaction.apply(db, a);
        transaction.onabort=() => fail("\u700f\u89bd\u5668\u5feb\u53d6\u6e05\u7406\u4ea4\u6613\u5df2\u4e2d\u6b62\u3002");
        transaction.onerror=() => fail("\u700f\u89bd\u5668\u5feb\u53d6\u6e05\u7406\u4ea4\u6613\u5931\u6557\u3002");
        transaction.oncomplete=() => finish(Ok(New_31(reality, removedSnapshots, removedWatermarks, retainedSnapshots, retainedWatermarks)));
        const visit=(storeName, removed, retained) => {
          const store=transaction.objectStore(storeName);
          const request_1=store.openKeyCursor();
          request_1.onerror=() => fail("\u7121\u6cd5\u8b80\u53d6\u700f\u89bd\u5668\u5feb\u53d6\u6e05\u55ae\u3002");
          request_1.onsuccess=(event) => {
            if(!finished)try {
              const cursor=eventResult(event);
              if(!isMissing(cursor)){
                const key_1=cursor.key;
                if(Equals(typeof key_1, "string")&&isRebuildableCacheKey(reality, key_1)){
                  store["delete"](key_1);
                  removed();
                }
                else retained();
                cursor["continue"]();
                return;
              }
              else return null;
            }
            catch(m){
              return fail("\u7121\u6cd5\u5b8c\u6210\u700f\u89bd\u5668\u5feb\u53d6\u6e05\u7406\uff0c\u4ea4\u6613\u5df2\u53d6\u6d88\u3002");
            }
            else return null;
          };
        };
        visit(snapshotStore(), () => {
          removedSnapshots=removedSnapshots+1;
        }, () => {
          retainedSnapshots=retainedSnapshots+1;
        });
        visit(watermarkStore(), () => {
          removedWatermarks=removedWatermarks+1;
        }, () => {
          retainedWatermarks=retainedWatermarks+1;
        });
      }
      catch(m){
        fail("\u7121\u6cd5\u958b\u59cb\u700f\u89bd\u5668\u5feb\u53d6\u6e05\u7406\u4ea4\u6613\u3002");
      }
    }
  }, () => {
    fail("\u700f\u89bd\u5668\u8cc7\u6599\u5eab\u7121\u6cd5\u4f7f\u7528\uff0c\u5feb\u53d6\u6e05\u7406\u672a\u5b8c\u6210\u3002");
  });
}
function writeJson(key_1, value){
  writeJsonTo(snapshotStore(), key_1, value);
}
function readWatermark(key_1, onRead){
  if(isBlank(key_1))onRead(null);
  else withStore(watermarkStore(), "readonly", (store) => {
    try {
      const request_1=store.get(key_1);
      request_1.onsuccess=(event) => {
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
      request_1.onerror=() => onRead(null);
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
      const request_1=indexedDb.open.apply(indexedDb, a);
      request_1.onupgradeneeded=(event) => {
        const db=eventResult(event);
        return!isMissing(db)?ensureStores(db):null;
      };
      request_1.onsuccess=(event) => {
        const db=eventResult(event);
        return!isMissing(db)?onReady(db):onUnavailable();
      };
      request_1.onerror=() => onUnavailable();
    }
  }
  catch(m){
    onUnavailable();
  }
}
function readAllPendingRaw(onRead){
  withStore(pendingStore(), "readonly", (store) => {
    try {
      const request_1=store.getAll();
      request_1.onsuccess=(event) => {
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
      request_1.onerror=() => onRead([]);
    }
    catch(m){
      onRead([]);
    }
  }, () => {
    onRead([]);
  });
}
function writeJsonTo(storeName, key_1, value){
  if(!isBlank(key_1))withStore(storeName, "readwrite", (store) => {
    try {
      const a=[JSON.stringify(value), key_1];
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
      iter((key_1) => {
        deleteFrom(snapshotStore(), key_1);
      }, filter_1((key_1) =>!isBlank(key_1)&&!protectedSnapshotKey(key_1)&&!exists((watermark) =>!(watermark==null)&&watermark.streamId==key_1, watermarks_1), snapshotKeys));
    });
  });
}
function deleteFromThen(storeName, key_1, onDeleted){
  if(isBlank(key_1))onDeleted();
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
      store["delete"](key_1);
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
      const request_1=store.getAllKeys();
      request_1.onsuccess=(event) => {
        const value=eventResult(event);
        if(isMissing(value))return onRead([]);
        else try {
          return onRead(value);
        }
        catch(m){
          return onRead([]);
        }
      };
      request_1.onerror=() => onRead([]);
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
function isRebuildableCacheKey(reality, key_1){
  return!isBlank(reality)&&!isBlank(key_1)&&exists((scope) => StartsWith(key_1, reality+":"+scope+":"), rebuildableCacheScopes());
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
      const request_1=store.getAll();
      request_1.onsuccess=(event) => {
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
      request_1.onerror=() => onRead([]);
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
function protectedSnapshotKey(key_1){
  const key_2=asText(key_1);
  return key_2=="append-pages-definitions:"||key_2.indexOf(":append-pages-definitions:")!=-1||StartsWith(key_2, "chat-agents:")||key_2.indexOf(":chat-agents:")!=-1||StartsWith(key_2, "actors-snapshot:")||key_2.indexOf(":actors-snapshot:")!=-1;
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
function deleteSnapshotAndWatermark(key_1){
  if(!isBlank(key_1))withSnapshotWatermarkStores("readwrite", (_1, _2, _3) => {
    try {
      _2["delete"](key_1);
      _3["delete"](key_1);
      return;
    }
    catch(m){
      return null;
    }
  }, () => { });
}
function deleteFrom(storeName, key_1){
  if(!isBlank(key_1))withStore(storeName, "readwrite", (store) => {
    try {
      store["delete"](key_1);
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
function rebuildableCacheScopes(){
  return _c_1.rebuildableCacheScopes;
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
  return _c_4.LocalTemplatesLoaded;
}
function set_LocalTemplatesLoaded(_1){
  _c_4.LocalTemplatesLoaded=_1;
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
  return _c_4.LoadedTemplates;
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
  return _c_4.TextHoleRE;
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
  static get Empty(){
    return Doc.Mk(null, Const());
  }
  static RunInPlace(childrenOnly, parent, doc_1){
    const st=CreateRunState(parent, doc_1.docNode);
    Sink(get_UseAnimations()||BatchUpdatesEnabled()?StartProcessor(PerformAnimatedUpdate(childrenOnly, st, doc_1.docNode)):() => {
      PerformSyncUpdate(childrenOnly, st, doc_1.docNode);
    }, doc_1.updates);
  }
  static RunById(id_1, tr){
    const m=globalThis.document.getElementById(id_1);
    if(Equals(m, null))FailWith("invalid id: "+id_1);
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
  static Convert(render, view){
    return Doc.Flatten(MapSeqCached(render, view));
  }
  static Append(a, b){
    return Doc.Mk(AppendDoc(a.docNode, b.docNode), Map2Unit(a.updates, b.updates));
  }
  static Flatten(view){
    return Doc.EmbedView(Map((x) => Doc.Concat(x), view));
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
  let _1=New_70(elem, Flags(tree), arr, oar.length===0?null:Some((el) => {
    iter_1((f) => {
      f(el);
    }, oar);
  }));
  return _1;
}
function EmptyAttr(){
  return _c_10.EmptyAttr;
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
function Dynamic(view, set_1){
  return Attr.A1(new DynamicAttrNode(view, set_1));
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
  return _c_9.rhtml;
}
function wrapMap(){
  return _c_9.wrapMap;
}
function defaultWrap(){
  return _c_9.defaultWrap;
}
function rxhtmlTag(){
  return _c_9.rxhtmlTag;
}
function rtagName(){
  return _c_9.rtagName;
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
  static requestSeq;
  static pendingCommandSeq;
  static rebuildableCacheScopes;
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
    this.rebuildableCacheScopes=["append-pages-definitions", "chat-participants-v2", "chat-thread", "sets-state-v2", "append-page-state", "append-page-keys", "actors-snapshot"];
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
function concat_1(separator, strings){
  return ofSeq(strings).join(separator);
}
function TrimEndWS(s){
  return s.replace(new RegExp("\\s+$"), "");
}
function Trim(s){
  return s.replace(new RegExp("^\\s+"), "").replace(new RegExp("\\s+$"), "");
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
function StartsWith(t, s){
  return t.substring(0, s.length)==s;
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
function EndsWith(x, s){
  return x.substring(x.length-s.length)==s;
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
function Split(s, pat, opts){
  return opts===1?filter_1((x) => x!=="", SplitWith(s, pat)):SplitWith(s, pat);
}
function RegexEscape(s){
  return s.replace(new RegExp("[-\\/\\\\^$*+?.()|[\\]{}]", "g"), "\\$&");
}
function SplitWith(str, pat){
  return str.split(pat);
}
function IsNullOrWhiteSpace(x){
  return x==null||(new RegExp("^\\s*$")).test(x);
}
function forall_1(f, s){
  return forall_2(f, protect(s));
}
function protect(s){
  return s==null?"":s;
}
function replicate(count, s){
  return create(count, s).join("");
}
function IndexOf(s, c, i){
  return s.indexOf(c, i);
}
function ToCharArray(s){
  return init(s.length, (x) => s[x]);
}
function IsNullOrEmpty(x){
  return x==null||x=="";
}
class FSharpList {
  static Empty=Create_2(FSharpList, {$:0});
  static Cons(Head, Tail){
    return Create_2(FSharpList, {
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
function Parse(s){
  return Parse_1(s, -2147483648, 2147483647, "Value was either too large or too small for an Int32.");
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
    const res=Create_2(FSharpList, {$:1});
    r=res;
    l=x;
    go=true;
    while(go)
      {
        r.$0=f(l.$0);
        l=l.$1;
        if(l.$==0)go=false;
        else {
          const t=Create_2(FSharpList, {$:1});
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
        const res=Create_2(FSharpList, {$:1});
        r=res;
        while(go)
          {
            r.$0=e.Current;
            if(e.MoveNext()){
              const t=Create_2(FSharpList, {$:1});
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
function forAll(p, x){
  let a, l;
  a=true;
  l=x;
  while(a&&l.$==1)
    {
      a=p(l.$0);
      l=l.$1;
    }
  return a;
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
function append_1(x, y){
  let r, l, go;
  if(x.$==0)return y;
  else if(y.$==0)return x;
  else {
    const res=Create_2(FSharpList, {$:1});
    r=res;
    l=x;
    go=true;
    while(go)
      {
        r.$0=l.$0;
        l=l.$1;
        if(l.$==0)go=false;
        else {
          const t=Create_2(FSharpList, {$:1});
          r=(r.$1=t,t);
        }
      }
    r.$1=y;
    return res;
  }
}
function concat_2(s){
  return ofSeq_1(concat_3(s));
}
function filter_2(p, x){
  let res, r, l, go;
  if(x.$==0)return x;
  else {
    res=FSharpList.Empty;
    r=res;
    l=x;
    go=true;
    while(go)
      {
        let _1, _2;
        if(p(l.$0)){
          if(res.$==0)_1=(res=Create_2(FSharpList, {$:1}),r=res);
          else {
            const t=Create_2(FSharpList, {$:1});
            _1=r=(r.$1=t,t);
          }
          const v=l.$0;
          _2=(r.$=1,r.$0=v);
        }
        else _2=void 0;
        l=l.$1;
        if(l.$==0)go=false;
      }
    if(!(res.$==0))r.$1=FSharpList.Empty;
    return res;
  }
}
function sort_1(l){
  const a=ofList(l);
  sortInPlace(a);
  return ofArray(a);
}
function tryHead_1(list){
  return list.$==0?null:Some(list.$0);
}
function rev(l){
  let res, r;
  res=FSharpList.Empty;
  r=l;
  while(r.$==1)
    {
      res=FSharpList.Cons(r.$0, res);
      r=r.$1;
    }
  return res;
}
function choose_1(f, l){
  return ofSeq_1(choose_2(f, l));
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
function distinct_1(s){
  return distinctBy_1((x) => x, s);
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
function collect_1(f, s){
  return concat_3(map_2(f, s));
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
function rev_1(s){
  return delay(() => ofSeq(s).slice().reverse());
}
function init_1(n, f){
  return take_1(n, initInfinite(f));
}
function concat_3(ss){
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
function seqEmpty(){
  return FailWith("The input sequence was empty.");
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
function pairwise_1(s){
  return map_2((x) =>[get(x, 0), get(x, 1)], windowed(2, s));
}
function windowed(windowSize, s){
  windowSize<=0?FailWith("The input must be positive."):void 0;
  return delay(() => enumUsing(Get(s), (e) => {
    const q=[];
    return append_2(enumWhile(() => q.length<windowSize&&e.MoveNext(), delay(() => {
      q.push(e.Current);
      return[];
    })), delay(() => q.length===windowSize?append_2([q.slice(0)], delay(() => enumWhile(() => e.MoveNext(), delay(() => {
      q.shift();
      q.push(e.Current);
      return[q.slice(0)];
    })))):[]));
  }));
}
function choose_2(f, s){
  return collect_1((x) => {
    const m=f(x);
    return m==null?FSharpList.Empty:ofArray([m.$0]);
  }, s);
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
function New_24(participantId, displayName, login, authenticated, provider, logoutPath, authenticatedParticipantId, authenticatedAclUserId, viewAsParticipantId, viewAsActive){
  return{
    participantId:participantId, 
    displayName:displayName, 
    login:login, 
    authenticated:authenticated, 
    provider:provider, 
    logoutPath:logoutPath, 
    authenticatedParticipantId:authenticatedParticipantId, 
    authenticatedAclUserId:authenticatedAclUserId, 
    viewAsParticipantId:viewAsParticipantId, 
    viewAsActive:viewAsActive
  };
}
function New_25(outputDirectory, intervalSeconds, includeOffline){
  return{
    outputDirectory:outputDirectory, 
    intervalSeconds:intervalSeconds, 
    includeOffline:includeOffline
  };
}
function New_26(outputDirectory){
  return{outputDirectory:outputDirectory};
}
function New_27(scheduleId){
  return{scheduleId:scheduleId};
}
function New_28(nodeCount, actorCount, maxSequence, nodes){
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
    return Get(concat_4(this.data));
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
    const all=concat_4(this.data);
    for(let i=0, _1=all.length-1;i<=_1;i++){
      const item=all[i];
      if(!other.Contains(item))this.Remove(item);
    }
  }
  CopyTo(arr, index){
    const all=concat_4(this.data);
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
function Fold(f, s, m){
  return fold_1((_1, _2) => f(_1, _2.Key, _2.Value), s, Enumerate(false, m.Tree));
}
function ToSeq(m){
  return map_2((kv) =>[kv.Key, kv.Value], Enumerate(false, m.Tree));
}
function New_29(nodeId, nodeAddress, status, roles, actors){
  return{
    nodeId:nodeId, 
    nodeAddress:nodeAddress, 
    status:status, 
    roles:roles, 
    actors:actors
  };
}
function New_30(actorId, displayName, kind, keys, status, routees){
  return{
    actorId:actorId, 
    displayName:displayName, 
    kind:kind, 
    keys:keys, 
    status:status, 
    routees:routees
  };
}
function Create(key_1, init_2){
  return CreateWithStorage(key_1, InMemory(ofSeq(init_2)));
}
function CreateWithStorage(key_1, storage){
  return new ListModel("New", key_1, storage);
}
function New_31(ServerRealityId, RemovedSnapshots, RemovedWatermarks, RetainedSnapshots, RetainedWatermarks){
  return{
    ServerRealityId:ServerRealityId, 
    RemovedSnapshots:RemovedSnapshots, 
    RemovedWatermarks:RemovedWatermarks, 
    RetainedSnapshots:RetainedSnapshots, 
    RetainedWatermarks:RetainedWatermarks
  };
}
function mount(root, access){
  let disposed, selected, generation, current, knownAuthority, observation, acceptedObservation, stored, rows, storageProblem, busy, resultRequest, refreshRead, refreshPending, recover;
  const node=(tag, testId, text_1) => {
    const value=globalThis.document.createElement(tag);
    value.setAttribute("data-testid", testId);
    value.textContent=text_1;
    return value;
  };
  const add=(parent, children) => iter((child) => {
    parent.appendChild(child);
  }, children);
  const enabled=(target_1, value) => value?target_1.removeAttribute("disabled"):target_1.setAttribute("disabled", "disabled");
  const makeButton=(testId, text_1) => {
    const value=node("button", testId, text_1);
    value.setAttribute("type", "button");
    return value;
  };
  root.setAttribute("data-testid", "wallet-panel");
  root.setAttribute("style", "display:grid;gap:10px;min-width:0;border:1px solid #d8dee8;border-radius:6px;padding:12px");
  const title=node("h3", "wallet-title", "Wallet");
  title.setAttribute("style", "margin:0");
  const target=node("div", "wallet-target", "\u8acb\u9078\u53d6\u4e00\u4f4d participant\u3002");
  const snapshot=node("div", "wallet-snapshot", "");
  snapshot.setAttribute("style", "display:flex;flex-wrap:wrap;gap:10px 24px");
  const fields=map((_1) => {
    const key_1=_1[0];
    const host=node("div", "wallet-"+key_1+"-field", _1[1]+"\uff1a");
    const value=node("span", "wallet-"+key_1, "\u2014");
    ((((a) => {
      const _2=a;
      return(_3) => add(_2, _3);
    })(host))([value]));
    ((((a) => {
      const _2=a;
      return(_3) => add(_2, _3);
    })(snapshot))([host]));
    return value;
  }, [["balance", "\u9918\u984d"], ["available", "\u53ef\u7528"], ["reserved", "\u4fdd\u7559"], ["revision", "Revision"]]);
  const readButton=makeButton("wallet-refresh", "\u8b80\u53d6\uff0f\u91cd\u65b0\u6574\u7406");
  const readStatus=node("div", "wallet-read-status", "");
  readStatus.setAttribute("aria-live", "polite");
  const form=node("div", "wallet-form", "");
  form.setAttribute("style", "display:flex;flex-wrap:wrap;align-items:end;gap:10px");
  const makeInput=(testId, labelText, placeholder) => {
    const label=node("label", testId+"-label", labelText);
    label.setAttribute("style", "display:grid;gap:4px;min-width:0;flex:1 1 180px");
    const value=node("input", testId, "");
    value.type="text";
    value.placeholder=placeholder;
    value.setAttribute("style", "min-width:0;width:100%;box-sizing:border-box;padding:7px;border:1px solid #cfd7e4;border-radius:6px");
    ((((a) => {
      const _1=a;
      return(_2) => add(_1, _2);
    })(label))([value]));
    ((((a) => {
      const _1=a;
      return(_2) => add(_1, _2);
    })(form))([label]));
    return value;
  };
  const amount=makeInput("wallet-amount", "\u52a0\u9ede\u6578\u91cf", "\u6b63\u6574\u6578");
  amount.setAttribute("inputmode", "numeric");
  amount.setAttribute("maxlength", "19");
  const reason=makeInput("wallet-reason", "\u539f\u56e0", "\u52a0\u9ede\u539f\u56e0");
  reason.setAttribute("maxlength", "512");
  const submit_1=makeButton("wallet-top-up", "\u52a0\u9ede");
  ((((a) => {
    const _1=a;
    return(_2) => add(_1, _2);
  })(form))([submit_1]));
  const result=node("div", "wallet-result", "");
  result.setAttribute("aria-live", "polite");
  const showResult=(text_1) => {
    result.textContent=text_1;
    iter((x) => {
      result.removeAttribute(x);
    }, ["data-receipt-id", "data-request-id", "data-receipt-status"]);
  };
  const pendingTitle=node("div", "wallet-pending-count", "");
  const pendingHost=node("div", "wallet-pending", "");
  pendingHost.setAttribute("style", "display:grid;gap:8px;min-width:0");
  ((((a) => {
    const _1=a;
    return(_2) => add(_1, _2);
  })(root))([title, target, snapshot, readButton, readStatus, form, result, pendingTitle, pendingHost]));
  disposed=false;
  selected=[];
  generation=0;
  current=null;
  knownAuthority=null;
  observation=0;
  acceptedObservation=0;
  stored=[];
  rows=[];
  storageProblem="";
  busy=new FSharpSet("New_2", null);
  resultRequest="";
  refreshRead=() => { };
  refreshPending=() => { };
  recover=() =>() => null;
  const nextObservation=() => {
    observation=observation+1;
    return observation;
  };
  const acceptAuthority=(ticket, read) => {
    const candidate=Some([read.authorityRealm, read.actorParticipantId]);
    return ticket<acceptedObservation&&!Equals(knownAuthority, candidate)?false:(ticket>=acceptedObservation?(acceptedObservation=ticket,knownAuthority=candidate):void 0,true);
  };
  const context=(read) => New_60(access.IsAuthenticatedHuman(), access.ViewAsActive()||read.viewAsActive, read.authorityRealm, access.ActorHint(), read.wallet.participantId, access.CanRead(read.wallet.participantId), access.CanTopUp(read.wallet.participantId));
  const hasPending=(read) => exists((item) => item.intent.authorityRealm==read.authorityRealm&&item.intent.actorParticipantId==read.actorParticipantId&&item.intent.payload.participantId==read.wallet.participantId, stored);
  const updateControls=() => {
    let canSubmit;
    enabled(readButton, length(selected)===1&&access.IsAuthenticatedHuman()&&access.CanRead(get(selected, 0)));
    if(current!=null&&current.$==1){
      const value=current.$0;
      if(length(selected)===1&&get(selected, 0)==value.wallet.participantId&&storageProblem==""){
        const value_1=current.$0;
        canSubmit=Equals(contextError(context(value_1), value_1.authorityRealm, value_1.actorParticipantId, value_1.wallet.participantId), null)&&!hasPending(value_1);
      }
      else canSubmit=false;
    }
    else canSubmit=false;
    enabled(submit_1, canSubmit);
    for(let i=0, _1=rows.length-1;i<=_1;i++)((() => {
      const row=get(rows, i);
      const intent=row.stored.intent;
      const sameAuthority=knownAuthority==null||Equals([intent.authorityRealm, intent.actorParticipantId], knownAuthority.$0);
      const canRecover=sameAuthority&&access.IsAuthenticatedHuman()&&access.ActorHint()==intent.actorParticipantId&&access.CanRead(intent.payload.participantId)&&access.CanTopUp(intent.payload.participantId)&&!busy.Contains(row.stored.key);
      enabled(row.query, canRecover);
      enabled(row.retry, canRecover&&!access.ViewAsActive());
      return!sameAuthority||access.ActorHint()!=intent.actorParticipantId?void(row.status.textContent="\u5148\u524d authority\uff0f\u767b\u5165\u8005\u7684\u7d00\u9304\u5df2\u4fdd\u7559\uff1b\u76ee\u524d\u7121\u6cd5\u67e5\u8a62\u6216\u91cd\u8a66\u3002"):null;
    })());
  };
  const rowStatus=(key_1, text_1) => {
    const o=tryFind((row) => row.stored.key==key_1, rows);
    return o==null?null:void(o.$0.status.textContent=text_1);
  };
  refreshPending=() => {
    if(!disposed){
      let _1;
      storageProblem="";
      try {
        const storage=globalThis.localStorage;
        const found=MarkResizable([]);
        for(let index=0, _2=storage.length-1;index<=_2;index++){
          const key_1=storage.key(index);
          if(!(key_1==null)&&StartsWith(key_1, storagePrefix())){
            const m=readStored(key_1);
            if(m==null){ }
            else {
              m.$0;
              if(length(found)<128){
                const m_1=decodeStored(key_1, m.$0);
                if(m_1==null)storageProblem="\u672c\u6a5f\u6709\u7121\u6cd5\u8fa8\u8b58\u7684\u5f85\u78ba\u8a8d\u7d00\u9304\uff1b\u4fdd\u7559\u539f\u8cc7\u6599\u4e26\u66ab\u505c\u65b0\u589e\u3002";
                else found.push(m_1.$0);
              }
              else storageProblem="\u5f85\u78ba\u8a8d\u7d00\u9304\u8d85\u904e 128 \u7b46\uff1b\u4fdd\u7559\u5168\u90e8\u7d00\u9304\u4e26\u66ab\u505c\u65b0\u589e\u3002";
            }
          }
        }
        _1=void(stored=sortBy((a) => a.key, found.slice()));
      }
      catch(m_2){
        _1=void(storageProblem="\u7121\u6cd5\u8b80\u53d6\u672c\u6a5f\u5f85\u78ba\u8a8d\u7d00\u9304\uff1b\u66ab\u505c\u65b0\u589e\u8207\u50b3\u9001\u3002");
      }
      const own=filter_1((item) => item.intent.actorParticipantId==access.ActorHint(), stored);
      const visible=own.slice(0, 50);
      iter((row) => {
        if(!exists((item) => item.key==row.stored.key&&item.serialized==row.stored.serialized, visible))pendingHost.removeChild(row.node);
      }, rows);
      rows=filter_1((row) => exists((item) => item.key==row.stored.key&&item.serialized==row.stored.serialized, visible), rows);
      for(let i=0, _3=visible.length-1;i<=_3;i++)((() => {
        const item=get(visible, i);
        if(!exists((row) => row.stored.key==item.key, rows)){
          const host=node("div", "wallet-pending-item", "");
          host.setAttribute("data-request-id", item.intent.payload.requestId);
          host.setAttribute("style", "display:grid;gap:6px;min-width:0;overflow-wrap:anywhere;border-top:1px solid #d8dee8;padding-top:8px");
          const info=node("div", "wallet-pending-info", item.intent.payload.participantId+" · "+item.intent.payload.amountUnits+" · "+item.intent.payload.requestId+" · "+item.intent.actorParticipantId+" · "+item.intent.authorityRealm);
          const status=node("div", "wallet-pending-status", "\u7d50\u679c\u5c1a\u5f85\u78ba\u8a8d\uff1b\u67e5\u8a62\u56de\u57f7\u6216\u4f7f\u7528\u76f8\u540c request \u91cd\u8a66\u3002");
          const actions=node("div", "wallet-pending-actions", "");
          actions.setAttribute("style", "display:flex;flex-wrap:wrap;gap:8px");
          const query=makeButton("wallet-pending-query", "\u67e5\u8a62\u56de\u57f7");
          const retry=makeButton("wallet-pending-retry", "\u539f\u6a23\u91cd\u8a66");
          query.addEventListener("click", () =>(recover(item))(false));
          retry.addEventListener("click", () =>(recover(item))(true));
          ((((a) => {
            const _4=a;
            return(_5) => add(_4, _5);
          })(actions))([query, retry]));
          ((((a) => {
            const _4=a;
            return(_5) => add(_4, _5);
          })(host))([info, status, actions]));
          ((((a) => {
            const _4=a;
            return(_5) => add(_4, _5);
          })(pendingHost))([host]));
          rows=rows.concat([New_64(item, host, status, query, retry)]);
          return;
        }
        else return null;
      })());
      pendingTitle.textContent="\u5f85\u78ba\u8a8d\uff1a"+String(length(own))+(length(own)>50?"\uff08\u986f\u793a\u524d 50 \u7b46\uff09":"")+(storageProblem==""?"":" · "+storageProblem);
      updateControls();
    }
  };
  const settle=(item, reply) => {
    let _1;
    if(!disposed){
      busy=busy.Remove_1(item.key);
      const p=reply.$==1?[Some(reply.$0), ""]:reply.$==2?[null, reply.$0]:[null, "invalid-wallet-response"];
      const m=classifyReply(item.intent, p[0], p[1]);
      if(m.$==0){
        if(access.ActorHint()!=item.intent.actorParticipantId||!Equals(knownAuthority, Some([item.intent.authorityRealm, item.intent.actorParticipantId])))_1=rowStatus(item.key, "\u56de\u57f7\u5df2\u62b5\u9054\uff0c\u4f46 authority\uff0f\u767b\u5165\u8005\u5df2\u6539\u8b8a\uff1b\u4fdd\u7559\u539f\u7d00\u9304\u3002");
        else {
          const value=m.$0;
          try {
            const m_1=confirmStored(item, item.key, readStored(item.key));
            _1=m_1.$==1?rowStatus(item.key, "\u56de\u57f7\u5df2\u78ba\u8a8d\uff0c\u672c\u6a5f\u7d00\u9304\u672a\u6e05\u9664\uff1a"+m_1.$0):(globalThis.localStorage.removeItem(item.key),resultRequest==item.key&&access.ActorHint()==item.intent.actorParticipantId?(result.textContent=(value.status=="applied"?"\u5df2\u78ba\u8a8d\u52a0\u9ede":"\u5df2\u78ba\u8a8d\u62d2\u7d55")+"\uff1a"+value.participantId+" · "+value.code+" · "+value.requestId+" · receipt "+value.receiptId,result.setAttribute("data-receipt-id", value.receiptId),result.setAttribute("data-request-id", value.requestId),result.setAttribute("data-receipt-status", value.status)):void 0,refreshPending(),refreshRead());
          }
          catch(m_2){
            _1=rowStatus(item.key, "\u56de\u57f7\u5df2\u78ba\u8a8d\uff0c\u4f46\u672c\u6a5f\u7d00\u9304\u7121\u6cd5\u6e05\u9664\uff1b\u8acb\u518d\u67e5\u8a62\u56de\u57f7\u3002");
          }
        }
      }
      else _1=rowStatus(item.key, "\u5f85\u78ba\u8a8d\uff1a"+m.$0+"\uff1b\u4fdd\u7559\u76f8\u540c request\uff0c\u52ff\u91cd\u5efa\u64cd\u4f5c\u3002");
      return updateControls();
    }
    else return null;
  };
  const send=(item, read) => {
    try {
      const m=validateSend(context(read), item, item.key, readStored(item.key));
      if(m.$==0){
        const payload=m.$0;
        rowStatus(item.key, "\u50b3\u9001\u4e2d\uff1b\u95dc\u9589\u9801\u9762\u5f8c\u4ecd\u53ef\u4f7f\u7528\u76f8\u540c request \u67e5\u8a62\u3002");
        return request("/management/api/wallet/top-ups", Some([item.intent.authorityRealm, item.intent.actorParticipantId]), Some(payload), (_1) => settle(item, _1));
      }
      else return settle(item, Failure(m.$0));
    }
    catch(m_1){
      return settle(item, Failure("pending-storage-unavailable"));
    }
  };
  recover=(item) =>(retry) => {
    if(!disposed&&!busy.Contains(item.key)){
      resultRequest=item.key;
      showResult(retry?"\u4f7f\u7528\u539f request \u91cd\u8a66\uff1b\u7d50\u679c\u4ecd\u5f85\u78ba\u8a8d\u3002":"\u67e5\u8a62\u539f request \u56de\u57f7\u2026");
      try {
        const m=confirmStored(item, item.key, readStored(item.key));
        if(m.$==0){
          if(!access.IsAuthenticatedHuman()||access.ActorHint()!=item.intent.actorParticipantId)return rowStatus(item.key, "\u767b\u5165\u8005\u5df2\u6539\u8b8a\uff1b\u539f\u7d00\u9304\u4fdd\u7559\uff0c\u7121\u6cd5\u5728\u76ee\u524d\u8eab\u5206\u91cd\u8a66\u3002");
          else if(!(access.CanRead(item.intent.payload.participantId)&&access.CanTopUp(item.intent.payload.participantId)))return rowStatus(item.key, "\u76ee\u524d\u6c92\u6709\u6b64 participant \u7684 wallet \u6b0a\u9650\uff1b\u539f\u7d00\u9304\u4fdd\u7559\u3002");
          else {
            busy=busy.Add_1(item.key);
            updateControls();
            rowStatus(item.key, "\u78ba\u8a8d\u76ee\u524d authority \u8207\u767b\u5165\u8005\u2026");
            const expectation=Some([item.intent.authorityRealm, item.intent.actorParticipantId]);
            const observationTicket=nextObservation();
            return request("/management/api/wallet?participantId="+encodeURIComponent(item.intent.payload.participantId), expectation, null, (reply) => {
              if(!disposed)if(reply.$==0){
                const read=reply.$0;
                if(read.authorityRealm==item.intent.authorityRealm&&read.actorParticipantId==item.intent.actorParticipantId&&read.wallet.participantId==item.intent.payload.participantId&&access.ActorHint()==read.actorParticipantId&&acceptAuthority(observationTicket, read)){
                  const read_1=reply.$0;
                  if(retry)send(item, read_1);
                  else request("/management/api/wallet/receipts?requestId="+encodeURIComponent(item.intent.payload.requestId), expectation, null, (_1) => settle(item, _1));
                }
                else settle(item, Failure("authority-or-caller-mismatch"));
              }
              else settle(item, reply);
            });
          }
        }
        else {
          rowStatus(item.key, m.$0);
          return refreshPending();
        }
      }
      catch(m_1){
        return settle(item, Failure("pending-storage-unavailable"));
      }
    }
    else return null;
  };
  refreshRead=() => {
    generation=generation+1;
    current=null;
    iter((field_1) => {
      field_1.textContent="\u2014";
    }, fields);
    updateControls();
    if(!disposed&&length(selected)===1){
      const ticket=New_65(generation, get(selected, 0));
      target.textContent=ticket.participantId;
      if(access.IsAuthenticatedHuman()&&access.CanRead(ticket.participantId)){
        readStatus.textContent="\u8b80\u53d6\u4e2d\u2026";
        const observationTicket=nextObservation();
        request("/management/api/wallet?participantId="+encodeURIComponent(ticket.participantId), null, null, (reply) => {
          if(!disposed&&ticket.generation===generation){
            let _1;
            if(reply.$==0){
              const value=reply.$0;
              const m=acceptRead(ticket, New_65(generation, length(selected)===1?get(selected, 0):""), access.ActorHint(), value);
              if(m.$==1)_1=readStatus.textContent=m.$0;
              else if(acceptAuthority(observationTicket, m.$0)){
                const value_1=m.$0;
                _1=(current=Some(value_1),iter2((_2, _3) => {
                  _2.textContent=_3;
                }, fields, [value_1.wallet.balanceUnits, value_1.wallet.availableUnits, value_1.wallet.reservedUnits, value_1.wallet.revision]),readStatus.textContent=value_1.viewAsActive||access.ViewAsActive()?"ViewAs\uff1a\u552f\u8b80":!access.CanTopUp(ticket.participantId)?"\u552f\u8b80\uff1a\u6c92\u6709\u52a0\u9ede\u6b0a\u9650":hasPending(value_1)?"\u6b64 participant \u6709\u5f85\u78ba\u8a8d\u64cd\u4f5c\uff1b\u8acb\u5148\u67e5\u8a62\u56de\u57f7\u3002":"\u5df2\u8b80\u53d6 wallet\u3002");
              }
              else _1=readStatus.textContent="authority\uff0f\u767b\u5165\u8005\u5df2\u6539\u8b8a\uff1b\u8acb\u91cd\u65b0\u8b80\u53d6\u3002";
            }
            else _1=reply.$==2?readStatus.textContent="\u7121\u6cd5\u8b80\u53d6\uff1a"+reply.$0:readStatus.textContent="\u7121\u6cd5\u8b80\u53d6\uff1ainvalid-wallet-response";
            updateControls();
          }
        });
      }
      else readStatus.textContent="\u9700\u4ee5\u6388\u6b0a\u7684\u4eba\u985e\u5e33\u865f\u767b\u5165\uff0c\u4e26\u5177\u5099\u6b64 participant \u7684 wallet.read \u6b0a\u9650\u3002";
    }
    else {
      target.textContent="\u8acb\u9078\u53d6\u4e00\u4f4d participant\uff08\u76ee\u524d "+String(length(selected))+" \u4f4d\uff09\u3002";
      readStatus.textContent="";
    }
  };
  submit_1.addEventListener("click", () => {
    let _1, _2;
    if(!disposed){
      resultRequest="";
      showResult("\u9a57\u8b49\u52a0\u9ede\u8cc7\u6599\u2026");
      refreshPending();
      if(current!=null&&current.$==1){
        const read=current.$0;
        _1=storageProblem==""&&length(selected)===1&&get(selected, 0)==read.wallet.participantId&&(_2=current.$0,true);
      }
      else _1=false;
      if(_1){
        const m=freeze(context(_2), _2, ToString(NewGuid(), "N"), amount.value, reason.value, hasPending(_2));
        if(m.$==0){
          const intent=m.$0;
          try {
            const item=New_62(storageKey(intent), JSON.stringify(intent), intent);
            if(readStored(item.key)==null){
              globalThis.localStorage.setItem(item.key, item.serialized);
              const m_1=confirmStored(item, item.key, readStored(item.key));
              return m_1.$==0?(resultRequest=item.key,showResult("\u5df2\u4fdd\u5b58\u539f request\uff1b\u7d50\u679c\u5f85 durable receipt \u78ba\u8a8d\u3002"),busy=busy.Add_1(item.key),refreshPending(),send(item, _2)):(showResult("\u672a\u50b3\u9001\uff1a"+m_1.$0),refreshPending());
            }
            else return showResult("\u672a\u50b3\u9001\uff1apending-key-collision");
          }
          catch(m_2){
            showResult("\u7121\u6cd5\u4fdd\u5b58\u4e26\u8b80\u56de\u5f85\u78ba\u8a8d\u7d00\u9304\uff1b\u672a\u50b3\u9001\u3002");
            return refreshPending();
          }
        }
        else return showResult("\u672a\u50b3\u9001\uff1a"+m.$0);
      }
      else return showResult("\u672a\u50b3\u9001\uff1a\u8acb\u5148\u8b80\u53d6\u55ae\u4e00 participant \u7684 wallet\uff0c\u4e26\u78ba\u8a8d\u672c\u6a5f\u5132\u5b58\u53ef\u7528\u3002");
    }
    else return null;
  });
  readButton.addEventListener("click", () => {
    refreshPending();
    return refreshRead();
  });
  const storageChanged=() => {
    refreshPending();
  };
  const focused=() => {
    refreshPending();
    refreshRead();
  };
  globalThis.addEventListener("storage", storageChanged);
  globalThis.addEventListener("focus", focused);
  refreshPending();
  updateControls();
  return New_33((values) => {
    const next=distinct(values);
    if(!Equals(next, selected)){
      selected=next;
      resultRequest="";
      refreshRead();
    }
    else updateControls();
  }, () => {
    refreshPending();
    refreshRead();
  }, () => {
    disposed=true;
    generation=generation+1;
    globalThis.removeEventListener("storage", storageChanged);
    globalThis.removeEventListener("focus", focused);
  });
}
function readStored(key_1){
  const value=globalThis.localStorage.getItem(key_1);
  return value==null?null:Some(value);
}
function request(url, expectation, payload, onReply){
  let finished;
  finished=false;
  const complete=(value_1) => {
    if(!finished){
      finished=true;
      onReply(value_1);
    }
  };
  const timer=globalThis.setTimeout(() => complete(Failure("outcome-unknown")), 15000);
  const finish=(value_1) => {
    globalThis.clearTimeout(timer);
    complete(value_1);
  };
  try {
    let _1, _2;
    const headers=new Headers();
    if(expectation==null)_1=null;
    else {
      const _3=expectation.$0[0];
      const _4=expectation.$0[1];
      _1=(headers.set("X-PTC-Wallet-Realm", _3),headers.set("X-PTC-Wallet-Actor", _4));
    }
    const options={
      cache:"no-store", 
      headers:headers, 
      credentials:"same-origin"
    };
    if(payload==null)_2=null;
    else {
      const value=payload.$0;
      _2=(options.method="POST",headers.set("Content-Type", "application/json"),void(options.body=JSON.stringify(value)));
    }
    (globalThis.fetch(url, options).then((response) => response.text().then((body) => finish(decodeReply(body)))))["catch"](() => finish(Failure("outcome-unknown")));
  }
  catch(m){
    finish(Failure("outcome-unknown"));
  }
}
function storageKey(intent){
  return storagePrefix()+concat_1("/", map((u) => encodeURIComponent(u), [intent.authorityRealm, intent.actorParticipantId, intent.payload.requestId]));
}
function storagePrefix(){
  return _c_5.storagePrefix;
}
function decodeStored(key_1, serialized){
  try {
    const intent=parse(serialized);
    return stringFields(intent, ["authorityRealm", "actorParticipantId"])&&stringFields(intent.payload, ["contractVersion", "requestId", "participantId", "amountUnits", "reason", "expectedWalletRevision"])&&nonblank(intent.authorityRealm)&&validId(intent.actorParticipantId)&&validPayload(intent.payload)&&storageKey(intent)==key_1?Some(New_62(key_1, serialized, intent)):null;
  }
  catch(m){
    return null;
  }
}
function decodeReply(body){
  try {
    const head_2=parse(body);
    if(!stringFields(head_2, ["contractVersion", "status", "code"])||head_2.contractVersion!="ptc.wallet/1")return Failure("invalid-wallet-response");
    else if(head_2.status=="ok"){
      const value=parse(body);
      return stringFields(value, ["authorityRealm", "actorParticipantId"])&&Equals(typeof value.viewAsActive, "boolean")&&snapshotShape(value.wallet)&&validRead(value)?Read(value):Failure("invalid-wallet-read");
    }
    else if(head_2.status=="applied"||head_2.status=="rejected"){
      const value_1=parse(body);
      return stringFields(value_1, ["operation", "requestId", "receiptId", "committedAtUtc", "actorParticipantId", "participantId"])&&Equals(typeof value_1.isReplay, "boolean")&&!(value_1.wallets==null)&&forall(snapshotShape, value_1.wallets)&&(value_1.status=="rejected"||exists((wallet) => wallet.participantId==value_1.participantId, value_1.wallets))?Receipt(value_1):Failure("invalid-wallet-receipt");
    }
    else return head_2.status=="error"&&nonblank(head_2.code)?Failure(head_2.code):Failure("invalid-wallet-response");
  }
  catch(m){
    return Failure("invalid-wallet-response");
  }
}
function parse(text_1){
  return JSON.parse(text_1);
}
function stringFields(value, fields){
  return!(value==null)&&forall((field_1) => isString(value[field_1]), fields);
}
function snapshotShape(value){
  return stringFields(value, ["participantId", "balanceUnits", "reservedUnits", "availableUnits", "revision"])&&validId(value.participantId)&&forall((v) => validateUnits(true, v), [value.balanceUnits, value.reservedUnits, value.availableUnits, value.revision]);
}
function isString(value){
  return Equals(typeof value, "string");
}
function New_32(IsAuthenticatedHuman, ActorHint, ViewAsActive, CanRead, CanTopUp){
  return{
    IsAuthenticatedHuman:IsAuthenticatedHuman, 
    ActorHint:ActorHint, 
    ViewAsActive:ViewAsActive, 
    CanRead:CanRead, 
    CanTopUp:CanTopUp
  };
}
function readAction(){
  return _c_3.readAction;
}
function topUpAction(){
  return _c_3.topUpAction;
}
function contextError(context, realm, actor, target){
  return!context.authenticatedHuman||!validId(context.actorParticipantId)||!StartsWith(context.actorParticipantId, "user.")?Some("unauthenticated-human"):!nonblank(context.authorityRealm)||context.authorityRealm!=realm?Some("authority-mismatch"):context.actorParticipantId!=actor?Some("caller-mismatch"):context.participantId!=target?Some("target-mismatch"):context.viewAsActive?Some("view-as-read-only"):!context.canRead||!context.canTopUp?Some("unauthorized"):null;
}
function classifyReply(intent, receipt, fallbackCode){
  if(intent==null||!nonblank(intent.authorityRealm)||!validId(intent.actorParticipantId)||!StartsWith(intent.actorParticipantId, "user.")||!validPayload(intent.payload))return KeepPending("pending-invalid");
  else if(receipt==null)return KeepPending(nonblank(fallbackCode)?fallbackCode:"outcome-unknown");
  else {
    const value=receipt.$0;
    return!(value==null)&&value.contractVersion=="ptc.wallet/1"&&value.operation=="wallet.top-up"&&value.requestId==intent.payload.requestId&&value.actorParticipantId==intent.actorParticipantId&&value.participantId==intent.payload.participantId&&validId(value.receiptId)&&nonblank(value.committedAtUtc)&&(value.status=="applied"&&value.code=="applied"||value.status=="rejected"&&nonblank(value.code)&&value.code!="applied")?Terminal(receipt.$0):KeepPending("receipt-mismatch");
  }
}
function confirmStored(original, readbackKey, readback){
  return!nonblank(original.key)||original.key!=readbackKey?Error_1("pending-key-mismatch"):!nonblank(original.serialized)?Error_1("pending-invalid"):readback!=null&&readback.$==1?readback.$0!=original.serialized?(readback.$0,Error_1("pending-changed")):Ok(original):Error_1("pending-missing");
}
function validateSend(context, original, readbackKey, readback){
  const m=confirmStored(original, readbackKey, readback);
  if(m.$==0){
    const intent=m.$0.intent;
    if(intent==null||!validPayload(intent.payload))return Error_1("pending-invalid");
    else {
      const m_1=contextError(context, intent.authorityRealm, intent.actorParticipantId, intent.payload.participantId);
      return m_1==null?Ok(intent.payload):Error_1(m_1.$0);
    }
  }
  else return Error_1(m.$0);
}
function acceptRead(requested, current, actualActorId, reply){
  return!Equals(requested, current)?Error_1("stale-read"):!validRead(reply)?Error_1("invalid-wallet-read"):reply.wallet.participantId!=current.participantId?Error_1("target-mismatch"):reply.actorParticipantId!=actualActorId?Error_1("caller-mismatch"):Ok(reply);
}
function freeze(context, read, requestId, amount, reason, hasUnresolvedPending){
  if(hasUnresolvedPending)return Error_1("pending-unresolved");
  else if(!validRead(read))return Error_1("invalid-wallet-read");
  else if(read.viewAsActive)return Error_1("view-as-read-only");
  else {
    const m=contextError(context, read.authorityRealm, read.actorParticipantId, read.wallet.participantId);
    if(m==null){
      const payload=New_63("ptc.wallet/1", requestId, read.wallet.participantId, amount, reason, read.wallet.revision);
      return!validPayload(payload)?Error_1("invalid-input"):Ok(New_61(read.authorityRealm, read.actorParticipantId, payload));
    }
    else return Error_1(m.$0);
  }
}
function validId(value){
  return nonblank(value)&&value.length<=128&&value==Trim(value)&&!exists_1(control, value);
}
function nonblank(value){
  return!(value==null)&&Trim(value).length>0;
}
function validPayload(payload){
  return!(payload==null)&&payload.contractVersion=="ptc.wallet/1"&&validId(payload.requestId)&&validId(payload.participantId)&&validateUnits(false, payload.amountUnits)&&validateUnits(true, payload.expectedWalletRevision)&&nonblank(payload.reason)&&Trim(payload.reason).length<=512&&!exists_1(control, payload.reason);
}
function validRead(reply){
  return!(reply==null)&&reply.contractVersion=="ptc.wallet/1"&&reply.status=="ok"&&reply.code=="ok"&&nonblank(reply.authorityRealm)&&validId(reply.actorParticipantId)&&!(reply.wallet==null)&&validId(reply.wallet.participantId)&&forAll((v) => validateUnits(true, v), ofArray([reply.wallet.balanceUnits, reply.wallet.reservedUnits, reply.wallet.availableUnits, reply.wallet.revision]));
}
function control(c){
  return c<" "||c>="\u007f"&&c<="\u009f";
}
function validateUnits(allowZero, value){
  return!(value==null)&&value.length>0&&value.length<=19&&(value.length===1||value[0]!=="0")&&forall_2((c) => c>="0"&&c<="9", value)&&(allowZero||value!="0")&&(value.length<19||Compare(value, "9223372036854775807")<=0);
}
function New_33(SetSelection, Refresh, Dispose_1){
  return{
    SetSelection:SetSelection, 
    Refresh:Refresh, 
    Dispose:Dispose_1
  };
}
class ListModel extends Object_1 {
  key;
  u0076ar;
  storage;
  v;
  it;
  Set(lst){
    this.u0076ar.Set(this.storage.SSet(lst));
    this.ObsoleteAll();
  }
  ObsoleteAll(){
    iter_1((ksn) => {
      Obsolete(ksn.V);
    }, this.it);
    this.it.Clear();
  }
  GetEnumerator(){
    return Get(this.u0076ar.Get());
  }
  GetEnumerator0(){
    return Get0(this.u0076ar.Get());
  }
  constructor(i, _1, _2, _3){
    let key_1, storage;
    if(i=="New"){
      key_1=_1;
      storage=_2;
      i="New_3";
      _1=key_1;
      _2=_c_2.Create_1(ofSeq(distinctBy_1(key_1, storage.SInit())));
      _3=storage;
    }
    if(i=="New_3"){
      const key_2=_1;
      const var_1=_2;
      const storage_1=_3;
      super();
      this.key=key_2;
      this.u0076ar=var_1;
      this.storage=storage_1;
      this.v=Map((x) => x.slice(), this.u0076ar.View);
      this.it=new Dictionary("New_5");
    }
  }
}
function New_34(pageId, tabId){
  return{pageId:pageId, tabId:tabId};
}
function New_35(pages){
  return{pages:pages};
}
function New_36(sets){
  return{sets:sets};
}
function New_37(setName, keys){
  return{setName:setName, keys:keys};
}
function New_38(commandId, groupId, expectedRevision, participantId, displayName, role, historyPolicy, includeHistoryBeforeFirstJoin){
  return{
    commandId:commandId, 
    groupId:groupId, 
    expectedRevision:expectedRevision, 
    participantId:participantId, 
    displayName:displayName, 
    role:role, 
    historyPolicy:historyPolicy, 
    includeHistoryBeforeFirstJoin:includeHistoryBeforeFirstJoin
  };
}
function New_39(participantId){
  return{participantId:participantId};
}
function New_40(participants){
  return{participants:participants};
}
function New_41(target, messageId){
  return{target:target, messageId:messageId};
}
function New_42(cursors, unread){
  return{cursors:cursors, unread:unread};
}
function New_43(messageId, channelMessageId, streamSequence, fromId, toId, scope, body, createdAtUtc){
  return{
    messageId:messageId, 
    channelMessageId:channelMessageId, 
    streamSequence:streamSequence, 
    fromId:fromId, 
    toId:toId, 
    scope:scope, 
    body:body, 
    createdAtUtc:createdAtUtc
  };
}
function New_44(cursors){
  return{cursors:cursors};
}
function New_45(target, sequence_1){
  return{target:target, sequence:sequence_1};
}
function New_46(groupId, channelMessageId){
  return{groupId:groupId, channelMessageId:channelMessageId};
}
function New_47(messages, nextAfterMessageId, oldestSequence, hasOlderMessages){
  return{
    messages:messages, 
    nextAfterMessageId:nextAfterMessageId, 
    oldestSequence:oldestSequence, 
    hasOlderMessages:hasOlderMessages
  };
}
function New_48(streamId, newestSequence, cachedCount, source, touchedAt){
  return{
    streamId:streamId, 
    newestSequence:newestSequence, 
    cachedCount:cachedCount, 
    source:source, 
    touchedAt:touchedAt
  };
}
function New_49(commandId, groupId, body, tags){
  return{
    commandId:commandId, 
    groupId:groupId, 
    body:body, 
    tags:tags
  };
}
function New_50(type, requestId, fromId, toId, body, tags, browserId, tabId){
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
function New_51(fromId, toId, body, tags){
  return{
    fromId:fromId, 
    toId:toId, 
    body:body, 
    tags:tags
  };
}
function New_52(messageId, speaker, createdAtUtc, body){
  return{
    messageId:messageId, 
    speaker:speaker, 
    createdAtUtc:createdAtUtc, 
    body:body
  };
}
function New_53(commandId, groupId, displayName, initialParticipantIds, historyPolicy, tags){
  return{
    commandId:commandId, 
    groupId:groupId, 
    displayName:displayName, 
    initialParticipantIds:initialParticipantIds, 
    historyPolicy:historyPolicy, 
    tags:tags
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
  Clear(){
    this.data=[];
    this.count=0;
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
  return New_66(get_Empty_1(), CreateElemNode(parent, EmptyAttr(), doc_1));
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
let _c_2=Lazy((_i) => class Var_1 extends Object_1 {
  static {
    _c_2=_i(this);
  }
  static Create_1(v){
    return new ConcreteVar(false, {s:Ready(v, [])}, v);
  }
  static { }
});
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
function MapSeqCached(conv, view){
  return MapSeqCachedBy((x) => x, conv, view);
}
function Map2Unit(a, a_1){
  return CreateLazy(() => Map2Unit_1(a(), a_1()));
}
function MapSeqCachedBy(key_1, conv, view){
  const state=[new Dictionary("New_5")];
  return Map((xs) => {
    const prevState=state[0];
    const newState=new Dictionary("New_5");
    const result=mapInPlace_1((x) => {
      const k=key_1(x);
      const res=prevState.ContainsKey(k)?prevState.Item(k):conv(x);
      newState.set_Item(k, res);
      return res;
    }, ofSeq(xs));
    state[0]=newState;
    return result;
  }, view);
}
function Map(fn, a){
  return CreateLazy(() => Map_1(fn, a()));
}
function Map2(fn, a, a_1){
  return CreateLazy(() => Map2_1(fn, a(), a_1()));
}
function MapCachedBy(eq, fn, a){
  const vref=[null];
  return CreateLazy(() => MapCachedBy_1(eq, vref, fn, a()));
}
function Map3(fn, a, a_1, a_2){
  return CreateLazy(() => Map3_1(fn, a(), a_1(), a_2()));
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
function New_54(shape, label, badge, className){
  return{
    shape:shape, 
    label:label, 
    badge:badge, 
    className:className
  };
}
function New_55(submitPath, sessionPath, logoutPath, returnUrl, protectedRoute, sessionCookieName, title, lead, providerLabel, aclLabel){
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
function New_56(userName, password, returnUrl, keepSession){
  return{
    userName:userName, 
    password:password, 
    returnUrl:returnUrl, 
    keepSession:keepSession
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
function countBy(f, a){
  const d=new Dictionary("New_5");
  const keys=[];
  for(let i=0, _1=length(a)-1;i<=_1;i++){
    const k=f(a[i]);
    if(d.ContainsKey(k))d.set_Item(k, d.Item(k)+1);
    else {
      keys.push(k);
      d.DAdd(k, 1);
    }
  }
  mapInPlace((k_1) =>[k_1, d.Item(k_1)], keys);
  return keys;
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
function New_57(PageId, TabId, ValueId, CreatedAtUtc, Direction, Tags, Payload){
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
function New_58(participantId){
  return{participantId:participantId};
}
function New_59(pageId, title, setName, shape, tabId, tabMode, path, description){
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
function TryParse_2(s, min, max_2, r){
  const x=+s;
  const ok=x===x-x%1&&x>=min&&x<=max_2;
  if(ok)r.set(x);
  return ok;
}
function TryParseBigInt(s, min, max_2, r){
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
    const ok=x===x-x%1n&&x>=min&&x<=max_2;
    if(ok)r.set(x);
    return ok;
  }
  else return false;
}
function Parse_1(s, min, max_2, overflowMsg){
  const x=+s;
  if(x!==x-x%1)throw new FormatException("New_1", "Input string was not in a correct format.");
  else if(x<min||x>max_2)throw new OverflowException("New_1", overflowMsg);
  else return x;
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
  GetHashCode(){
    return Hash(ofSeq(this));
  }
  Add_1(k, v){
    return new FSharpMap("New_1", Add(Pair.New(k, v), this.tree));
  }
  get Tree(){
    return this.tree;
  }
  Remove_1(k){
    return new FSharpMap("New_1", Remove(Pair.New(k, void 0), this.tree));
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
    return Create_2(Pair, {Key:Key, Value:Value});
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
    const d=ofSeq(append_2(Enumerate(false, t.Left), Enumerate(false, t.Right)));
    let _1=Build(d, 0, d.length-1);
    return Rebuild(spine, _1);
  }
}
function Add(x, t){
  return Put((_1, _2) => _2, x, t);
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
function Build(data, min, max_2){
  if(max_2-min+1<=0)return null;
  else {
    const center=(min+max_2)/2>>0;
    return Branch(get(data, center), Build(data, min, center-1), Build(data, center+1, max_2));
  }
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
function Put(combine, k, t){
  const p=Lookup(k, t);
  const t_1=p[0];
  return t_1==null?Rebuild(p[1], Branch(k, null, null)):Rebuild(p[1], Branch(combine(t_1.Node, k), t_1.Left, t_1.Right));
}
function Branch(node, left, right){
  const a=left==null?0:left.Height;
  const b=right==null?0:right.Height;
  let _1=Compare(a, b)===1?a:b;
  let _2=1+_1;
  return New_69(node, left, right, _2, 1+(left==null?0:left.Count)+(right==null?0:right.Count));
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
function InMemory(init_2){
  return new ArrayStorage(init_2);
}
function Error_1(ErrorValue){
  return{$:1, $0:ErrorValue};
}
function Ok(ResultValue){
  return{$:0, $0:ResultValue};
}
function New_60(authenticatedHuman, viewAsActive, authorityRealm, actorParticipantId, participantId, canRead, canTopUp){
  return{
    authenticatedHuman:authenticatedHuman, 
    viewAsActive:viewAsActive, 
    authorityRealm:authorityRealm, 
    actorParticipantId:actorParticipantId, 
    participantId:participantId, 
    canRead:canRead, 
    canTopUp:canTopUp
  };
}
function New_61(authorityRealm, actorParticipantId, payload){
  return{
    authorityRealm:authorityRealm, 
    actorParticipantId:actorParticipantId, 
    payload:payload
  };
}
function New_62(key_1, serialized, intent){
  return{
    key:key_1, 
    serialized:serialized, 
    intent:intent
  };
}
function New_63(contractVersion, requestId, participantId, amountUnits, reason, expectedWalletRevision){
  return{
    contractVersion:contractVersion, 
    requestId:requestId, 
    participantId:participantId, 
    amountUnits:amountUnits, 
    reason:reason, 
    expectedWalletRevision:expectedWalletRevision
  };
}
function New_64(stored, node, status, query, retry){
  return{
    stored:stored, 
    node:node, 
    status:status, 
    query:query, 
    retry:retry
  };
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
  get Count(){
    const tree=this.tree;
    return tree==null?0:tree.Count;
  }
  GetEnumerator(){
    return Get(Enumerate(false, this.tree));
  }
  GetHashCode(){
    return -1741749453+Hash(ofSeq(this));
  }
  get IsEmpty(){
    return this.tree==null;
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
function Failure(Item){
  return{$:2, $0:Item};
}
function Read(Item){
  return{$:0, $0:Item};
}
function Receipt(Item){
  return{$:1, $0:Item};
}
function New_65(generation, participantId){
  return{generation:generation, participantId:participantId};
}
function ToString(this_1, format){
  const m=format.toUpperCase();
  if(m=="N")return Replace(this_1, "-", "");
  else if(m=="D")return this_1;
  else if(m=="B")return"{"+this_1+"}";
  else if(m=="P")return"("+this_1+")";
  else if(m=="X"){
    const s=this_1;
    return"{0x"+Substring(s, 0, 8)+",0x"+Substring(s, 9, 4)+",0x"+Substring(s, 14, 4)+",{0x"+Substring(s, 19, 2)+",0x"+Substring(s, 21, 2)+",0x"+Substring(s, 24, 2)+",0x"+Substring(s, 26, 2)+",0x"+Substring(s, 28, 2)+",0x"+Substring(s, 30, 2)+",0x"+Substring(s, 32, 2)+",0x"+Substring(s, 34, 2)+"}}";
  }
  else return FormatError();
}
function NewGuid(){
  return"xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(new RegExp("[xy]", "g"), (c) => {
    const r=Math.random()*16|0;
    const v=c=="x"?r:r&3|8;
    return v.toString(16);
  });
}
function FormatError(){
  throw new FormatException("New_1", "Format String can be only \"D\", \"d\", \"N\", \"n\", \"P\", \"p\", \"B\", \"b\", \"X\" or \"x\".");
}
let _c_3=Lazy((_i) => class $StartupCode_WalletClientState {
  static {
    _c_3=_i(this);
  }
  static topUpAction;
  static readAction;
  static {
    this.readAction="ptcs.wallet.read";
    this.topUpAction="ptcs.wallet.top-up";
  }
});
let _c_4=Lazy((_i) => class $StartupCode_Templates {
  static {
    _c_4=_i(this);
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
  return _c_6.BatchUpdatesEnabled;
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
function WhenObsoleteRun(snap, obs){
  const m=snap.s;
  if(m==null)obs();
  else m!=null&&m.$==2?(m.$0,m.$1.push(obs)):m!=null&&m.$==3?(m.$0,m.$1.push(obs)):m.$0;
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
function Map3_1(fn, sn1, sn2, sn3){
  const _1=sn1.s;
  const _2=sn2.s;
  const _3=sn3.s;
  if(_1!=null&&_1.$==0)return _2!=null&&_2.$==0?_3!=null&&_3.$==0?{s:Forever(fn(_1.$0, _2.$0, _3.$0))}:Map3Opt1(fn, _1.$0, _2.$0, sn3):_3!=null&&_3.$==0?Map3Opt2(fn, _1.$0, _3.$0, sn2):Map3Opt3(fn, _1.$0, sn2, sn3);
  else if(_2!=null&&_2.$==0)return _3!=null&&_3.$==0?Map3Opt4(fn, _2.$0, _3.$0, sn1):Map3Opt5(fn, _2.$0, sn1, sn3);
  else if(_3!=null&&_3.$==0)return Map3Opt6(fn, _3.$0, sn1, sn2);
  else {
    const res={s:Waiting([], [])};
    const cont=() => {
      const m=res.s;
      if(!(m!=null&&m.$==0||m!=null&&m.$==2)){
        const _4=ValueAndForever(sn1);
        const _5=ValueAndForever(sn2);
        const _6=ValueAndForever(sn3);
        if(_4!=null&&_4.$==1)if(_5!=null&&_5.$==1)if(_6!=null&&_6.$==1)if(_4.$0[1]&&_5.$0[1]&&_6.$0[1])MarkForever(res, fn(_4.$0[0], _5.$0[0], _6.$0[0]));
        else MarkReady(res, fn(_4.$0[0], _5.$0[0], _6.$0[0]));
      }
    };
    When(sn1, cont, res);
    When(sn2, cont, res);
    When(sn3, cont, res);
    return res;
  }
}
function WhenObsolete(snap, obs){
  const m=snap.s;
  if(m==null)Obsolete(obs);
  else m!=null&&m.$==2?(m.$0,EnqueueSafe(m.$1, obs)):m!=null&&m.$==3?(m.$0,EnqueueSafe(m.$1, obs)):m.$0;
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
function Map2Opt1(fn, x, sn2){
  return Map_1((y) => fn(x, y), sn2);
}
function Map2Opt2(fn, y, sn1){
  return Map_1((x) => fn(x, y), sn1);
}
function Map3Opt1(fn, x, y, sn3){
  return Map_1((z) => fn(x, y, z), sn3);
}
function Map3Opt2(fn, x, z, sn2){
  return Map_1((y) => fn(x, y, z), sn2);
}
function Map3Opt3(fn, x, sn2, sn3){
  return Map2_1((_1, _2) => fn(x, _1, _2), sn2, sn3);
}
function Map3Opt4(fn, y, z, sn1){
  return Map_1((x) => fn(x, y, z), sn1);
}
function Map3Opt5(fn, y, sn1, sn3){
  return Map2_1((_1, _2) => fn(_1, y, _2), sn1, sn3);
}
function Map3Opt6(fn, z, sn1, sn2){
  return Map2_1((_1, _2) => fn(_1, _2, z), sn1, sn2);
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
class ArrayStorage extends Object_1 {
  init;
  SSet(coll){
    return ofSeq(coll);
  }
  SInit(){
    return this.init;
  }
  constructor(init_2){
    super();
    this.init=init_2;
  }
}
function KeepPending(Item){
  return{$:1, $0:Item};
}
function Terminal(Item){
  return{$:0, $0:Item};
}
let _c_5=Lazy((_i) => class $StartupCode_WalletClient {
  static {
    _c_5=_i(this);
  }
  static storagePrefix;
  static {
    this.storagePrefix="ptc.wallet.pending.v1/";
  }
});
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
    return Create_2(DocElemNode, _2);
  }
}
function New_66(PreviousNodes, Top){
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
    r(New_73((a) => {
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
  return _c_11.Zero;
}
function Start(c, ctOpt){
  const d=(defCTS())[0];
  const ct=ctOpt==null?d:ctOpt.$0;
  scheduler().Fork(() => {
    if(!ct.c)c(New_73((a) => {
      if(a.$==1)UncaughtAsyncError(a.$0);
    }, ct));
  });
}
function Return(x){
  return(c) => {
    c.k(Ok_1(x));
  };
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
function scheduler(){
  return _c_11.scheduler;
}
function checkCancel(r){
  return(c) => {
    if(c.ct.c)cancel(c);
    else r(c);
  };
}
function defCTS(){
  return _c_11.defCTS;
}
function UncaughtAsyncError(e){
  console.log("WebSharper: Uncaught asynchronous exception", e);
}
function cancel(c){
  c.k(Cc(new OperationCanceledException("New", c.ct)));
}
function Combine(a, b){
  return Bind_1(a, () => b);
}
function StartImmediate(c, ctOpt){
  const d=(defCTS())[0];
  const ct=ctOpt==null?d:ctOpt.$0;
  if(!ct.c)c(New_73((a) => {
    if(a.$==1)UncaughtAsyncError(a.$0);
  }, ct));
}
function UseAnimations(){
  return _c_7.UseAnimations;
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
  const last=Create_1(() => anim.Compute(anim.Duration));
  return{Compute:(t) => t>=dur?last.f():comp(t), Duration:nextDuration};
}
let _c_6=Lazy((_i) => class Proxy {
  static {
    _c_6=_i(this);
  }
  static BatchUpdatesEnabled;
  static {
    this.BatchUpdatesEnabled=true;
  }
});
function New_67(type, requestId, extensionId, channelId, operation, payload){
  return{
    type:type, 
    requestId:requestId, 
    extensionId:extensionId, 
    channelId:channelId, 
    operation:operation, 
    payload:payload
  };
}
function New_68(CanvasInstanceId, Poll, PollEnabled, Connected_1, Active, InFlight, DataRevision, ReconnectAttempt, DisposePending, Disposed){
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
  return New_71("ta-browser.v1", kind, actionKind, canvasId, "", "", "", "", 0, false, "", "", 0, "", "", false, 0, 0, "", "", false, [], 0, false, "", "", "", "", 0, "", false, "", false, "", "", false, 0, "", "");
}
function actionToWire(action){
  if(action.$==1)return emptyFrame("action", "reset-canvas", canvasText(action.$0));
  else if(action.$==2){
    const row=action.$1;
    const _1=emptyFrame("action", "add-row", canvasText(action.$0));
    return New_71(_1.wireVersion, _1.kind, _1.actionKind, _1.canvasInstanceId, row.RowId, _1.traceId, rowKindText(row.Kind), row.DataRef, row.HeightWeight, row.Visible, _1.sourceId, _1.instrument, _1.intervalMinutes, _1.fromUtc, _1.toUtcExclusive, _1.includePartial, _1.afterDataRevision, _1.dataRevision, _1.reasonCode, _1.templateKey, _1.hasTemplateRowId, _1.editorValues, _1.expectedDocumentRevision, _1.hasExpectedDocumentRevision, _1.baseRowId, _1.eventTimeUtc, _1.startEventTimeUtc, _1.endEventTimeExclusiveUtc, _1.maximumBasePoints, _1.coverageIntentVersion, _1.hasCoverageIntent, _1.expectedCoverageRevision, _1.hasExpectedCoverageRevision, _1.queryGeneration, _1.startObservationOrdinal, _1.hasStartObservationOrdinal, _1.coverageObservationCount, _1.coverageDirection, _1.coverageRangeAuthority);
  }
  else if(action.$==3){
    const values=action.$3;
    const templateKey=action.$2;
    const rowId=action.$1;
    const _2=emptyFrame("action", "apply-template", canvasText(action.$0));
    return New_71(_2.wireVersion, _2.kind, _2.actionKind, _2.canvasInstanceId, rowId==null?"":rowId.$0, _2.traceId, _2.rowKind, _2.dataRef, _2.heightWeight, _2.visible, _2.sourceId, _2.instrument, _2.intervalMinutes, _2.fromUtc, _2.toUtcExclusive, _2.includePartial, _2.afterDataRevision, _2.dataRevision, _2.reasonCode, templateKey, rowId!=null, map(editorInputToWire, values==null?[]:values), _2.expectedDocumentRevision, _2.hasExpectedDocumentRevision, _2.baseRowId, _2.eventTimeUtc, _2.startEventTimeUtc, _2.endEventTimeExclusiveUtc, _2.maximumBasePoints, _2.coverageIntentVersion, _2.hasCoverageIntent, _2.expectedCoverageRevision, _2.hasExpectedCoverageRevision, _2.queryGeneration, _2.startObservationOrdinal, _2.hasStartObservationOrdinal, _2.coverageObservationCount, _2.coverageDirection, _2.coverageRangeAuthority);
  }
  else if(action.$==4){
    const rowId_1=action.$1;
    const _3=emptyFrame("action", "remove-row", canvasText(action.$0));
    return New_71(_3.wireVersion, _3.kind, _3.actionKind, _3.canvasInstanceId, rowId_1, _3.traceId, _3.rowKind, _3.dataRef, _3.heightWeight, _3.visible, _3.sourceId, _3.instrument, _3.intervalMinutes, _3.fromUtc, _3.toUtcExclusive, _3.includePartial, _3.afterDataRevision, _3.dataRevision, _3.reasonCode, _3.templateKey, _3.hasTemplateRowId, _3.editorValues, _3.expectedDocumentRevision, _3.hasExpectedDocumentRevision, _3.baseRowId, _3.eventTimeUtc, _3.startEventTimeUtc, _3.endEventTimeExclusiveUtc, _3.maximumBasePoints, _3.coverageIntentVersion, _3.hasCoverageIntent, _3.expectedCoverageRevision, _3.hasExpectedCoverageRevision, _3.queryGeneration, _3.startObservationOrdinal, _3.hasStartObservationOrdinal, _3.coverageObservationCount, _3.coverageDirection, _3.coverageRangeAuthority);
  }
  else if(action.$==5){
    const traceId=action.$2;
    const rowId_2=action.$1;
    const _4=emptyFrame("action", "remove-trace", canvasText(action.$0));
    return New_71(_4.wireVersion, _4.kind, _4.actionKind, _4.canvasInstanceId, rowId_2, traceId, _4.rowKind, _4.dataRef, _4.heightWeight, _4.visible, _4.sourceId, _4.instrument, _4.intervalMinutes, _4.fromUtc, _4.toUtcExclusive, _4.includePartial, _4.afterDataRevision, _4.dataRevision, _4.reasonCode, _4.templateKey, _4.hasTemplateRowId, _4.editorValues, _4.expectedDocumentRevision, _4.hasExpectedDocumentRevision, _4.baseRowId, _4.eventTimeUtc, _4.startEventTimeUtc, _4.endEventTimeExclusiveUtc, _4.maximumBasePoints, _4.coverageIntentVersion, _4.hasCoverageIntent, _4.expectedCoverageRevision, _4.hasExpectedCoverageRevision, _4.queryGeneration, _4.startObservationOrdinal, _4.hasStartObservationOrdinal, _4.coverageObservationCount, _4.coverageDirection, _4.coverageRangeAuthority);
  }
  else if(action.$==6){
    const query=action.$1;
    const _5=emptyFrame("action", "change-query", canvasText(action.$0));
    return New_71(_5.wireVersion, _5.kind, _5.actionKind, _5.canvasInstanceId, _5.rowId, _5.traceId, _5.rowKind, _5.dataRef, _5.heightWeight, _5.visible, optionText(query.SourceId), optionText(query.Instrument), optionInt(query.IntervalMinutes), optionText(query.FromUtc), optionText(query.ToUtcExclusive), optionBool(query.IncludePartial), _5.afterDataRevision, _5.dataRevision, _5.reasonCode, _5.templateKey, _5.hasTemplateRowId, _5.editorValues, _5.expectedDocumentRevision, _5.hasExpectedDocumentRevision, _5.baseRowId, _5.eventTimeUtc, _5.startEventTimeUtc, _5.endEventTimeExclusiveUtc, _5.maximumBasePoints, _5.coverageIntentVersion, _5.hasCoverageIntent, _5.expectedCoverageRevision, _5.hasExpectedCoverageRevision, _5.queryGeneration, _5.startObservationOrdinal, _5.hasStartObservationOrdinal, _5.coverageObservationCount, _5.coverageDirection, _5.coverageRangeAuthority);
  }
  else if(action.$==7){
    const change=action.$1;
    const _6=emptyFrame("action", "shared-cursor-changed", canvasText(action.$0));
    return New_71(_6.wireVersion, _6.kind, _6.actionKind, _6.canvasInstanceId, _6.rowId, _6.traceId, _6.rowKind, _6.dataRef, _6.heightWeight, _6.visible, _6.sourceId, _6.instrument, _6.intervalMinutes, _6.fromUtc, _6.toUtcExclusive, _6.includePartial, _6.afterDataRevision, _6.dataRevision, _6.reasonCode, _6.templateKey, _6.hasTemplateRowId, _6.editorValues, _6.expectedDocumentRevision, _6.hasExpectedDocumentRevision, change.BaseRowId, change.EventTimeUtc, _6.startEventTimeUtc, _6.endEventTimeExclusiveUtc, _6.maximumBasePoints, _6.coverageIntentVersion, _6.hasCoverageIntent, _6.expectedCoverageRevision, _6.hasExpectedCoverageRevision, _6.queryGeneration, _6.startObservationOrdinal, _6.hasStartObservationOrdinal, _6.coverageObservationCount, _6.coverageDirection, _6.coverageRangeAuthority);
  }
  else if(action.$==8){
    const change_1=action.$1;
    const _7=emptyFrame("action", "visible-range-changed", canvasText(action.$0));
    const frame=New_71(_7.wireVersion, _7.kind, _7.actionKind, _7.canvasInstanceId, _7.rowId, _7.traceId, _7.rowKind, _7.dataRef, _7.heightWeight, _7.visible, _7.sourceId, _7.instrument, _7.intervalMinutes, _7.fromUtc, _7.toUtcExclusive, _7.includePartial, _7.afterDataRevision, _7.dataRevision, _7.reasonCode, _7.templateKey, _7.hasTemplateRowId, _7.editorValues, _7.expectedDocumentRevision, _7.hasExpectedDocumentRevision, change_1.BaseRowId, _7.eventTimeUtc, change_1.StartEventTimeUtc, change_1.EndEventTimeExclusiveUtc, change_1.MaximumBasePoints, _7.coverageIntentVersion, _7.hasCoverageIntent, _7.expectedCoverageRevision, _7.hasExpectedCoverageRevision, _7.queryGeneration, _7.startObservationOrdinal, _7.hasStartObservationOrdinal, _7.coverageObservationCount, _7.coverageDirection, _7.coverageRangeAuthority);
    const m=change_1.CoverageIntent;
    if(m!=null&&m.$==1){
      const intent=m.$0;
      const o=intent.ExpectedCoverageRevision;
      const o_1=o==null?null:Some(String(o.$0));
      let _8=o_1==null?"":o_1.$0;
      const o_2=intent.StartObservationOrdinal;
      const o_3=o_2==null?null:Some(String(o_2.$0));
      let _9=o_3==null?"":o_3.$0;
      const m_1=intent.Direction;
      let _10=m_1==null?"":m_1.$0.$==1?"later":"earlier";
      return New_71(frame.wireVersion, frame.kind, frame.actionKind, frame.canvasInstanceId, frame.rowId, frame.traceId, frame.rowKind, frame.dataRef, frame.heightWeight, frame.visible, frame.sourceId, frame.instrument, frame.intervalMinutes, frame.fromUtc, frame.toUtcExclusive, frame.includePartial, frame.afterDataRevision, frame.dataRevision, frame.reasonCode, frame.templateKey, frame.hasTemplateRowId, frame.editorValues, frame.expectedDocumentRevision, frame.hasExpectedDocumentRevision, frame.baseRowId, frame.eventTimeUtc, frame.startEventTimeUtc, frame.endEventTimeExclusiveUtc, frame.maximumBasePoints, "ta-coverage-window.v1", true, _8, intent.ExpectedCoverageRevision!=null, String(intent.QueryGeneration), _9, intent.StartObservationOrdinal!=null, intent.ObservationCount, _10, intent.RangeAuthority.$==1?"provider-open-earlier":"explicit-bounds");
    }
    else return frame;
  }
  else if(action.$==9){
    const revision=action.$1;
    const _11=emptyFrame("action", "poll-delta", canvasText(action.$0));
    return New_71(_11.wireVersion, _11.kind, _11.actionKind, _11.canvasInstanceId, _11.rowId, _11.traceId, _11.rowKind, _11.dataRef, _11.heightWeight, _11.visible, _11.sourceId, _11.instrument, _11.intervalMinutes, _11.fromUtc, _11.toUtcExclusive, _11.includePartial, Number(revision), _11.dataRevision, _11.reasonCode, _11.templateKey, _11.hasTemplateRowId, _11.editorValues, _11.expectedDocumentRevision, _11.hasExpectedDocumentRevision, _11.baseRowId, _11.eventTimeUtc, _11.startEventTimeUtc, _11.endEventTimeExclusiveUtc, _11.maximumBasePoints, _11.coverageIntentVersion, _11.hasCoverageIntent, _11.expectedCoverageRevision, _11.hasExpectedCoverageRevision, _11.queryGeneration, _11.startObservationOrdinal, _11.hasStartObservationOrdinal, _11.coverageObservationCount, _11.coverageDirection, _11.coverageRangeAuthority);
  }
  else if(action.$==10){
    const reason=action.$1;
    const _12=emptyFrame("action", "full-snapshot", canvasText(action.$0));
    return New_71(_12.wireVersion, _12.kind, _12.actionKind, _12.canvasInstanceId, _12.rowId, _12.traceId, _12.rowKind, _12.dataRef, _12.heightWeight, _12.visible, _12.sourceId, _12.instrument, _12.intervalMinutes, _12.fromUtc, _12.toUtcExclusive, _12.includePartial, _12.afterDataRevision, _12.dataRevision, reason, _12.templateKey, _12.hasTemplateRowId, _12.editorValues, _12.expectedDocumentRevision, _12.hasExpectedDocumentRevision, _12.baseRowId, _12.eventTimeUtc, _12.startEventTimeUtc, _12.endEventTimeExclusiveUtc, _12.maximumBasePoints, _12.coverageIntentVersion, _12.hasCoverageIntent, _12.expectedCoverageRevision, _12.hasExpectedCoverageRevision, _12.queryGeneration, _12.startObservationOrdinal, _12.hasStartObservationOrdinal, _12.coverageObservationCount, _12.coverageDirection, _12.coverageRangeAuthority);
  }
  else return emptyFrame("action", "reset-view", canvasText(action.$0));
}
function actionRequestToWire(request_1){
  const _1=actionToWire(request_1.Action);
  const o=request_1.ExpectedDocumentRevision;
  const o_1=o==null?null:Some(Number(o.$0));
  let _2=o_1==null?0:o_1.$0;
  return New_71(_1.wireVersion, _1.kind, _1.actionKind, _1.canvasInstanceId, _1.rowId, _1.traceId, _1.rowKind, _1.dataRef, _1.heightWeight, _1.visible, _1.sourceId, _1.instrument, _1.intervalMinutes, _1.fromUtc, _1.toUtcExclusive, _1.includePartial, _1.afterDataRevision, _1.dataRevision, _1.reasonCode, _1.templateKey, _1.hasTemplateRowId, _1.editorValues, _2, request_1.ExpectedDocumentRevision!=null, _1.baseRowId, _1.eventTimeUtc, _1.startEventTimeUtc, _1.endEventTimeExclusiveUtc, _1.maximumBasePoints, _1.coverageIntentVersion, _1.hasCoverageIntent, _1.expectedCoverageRevision, _1.hasExpectedCoverageRevision, _1.queryGeneration, _1.startObservationOrdinal, _1.hasStartObservationOrdinal, _1.coverageObservationCount, _1.coverageDirection, _1.coverageRangeAuthority);
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
      const mergedSharedTemporal=Fold((_3, _4, _5) => _3.Add_1(_4, _5), mergedSeries, mapFromWire(wire.sharedTemporalData));
      const m=decoded.Data.TryFind(statusRef);
      let _1=m==null?mergedSharedTemporal:mergedSharedTemporal.Add_1(statusRef, m.$0);
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
  return m.$==1?New_75(input_1.Path, "number", "", m.$0, false):m.$==2?New_75(input_1.Path, "bool", "", 0, m.$0):New_75(input_1.Path, "text", m.$0, 0, false);
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
  let editorSchemas;
  if(wire==null||wire.wireVersion!="ta-browser.v1"&&wire.wireVersion!="ta-browser.v2"&&wire.wireVersion!="ta-browser.v3"&&wire.wireVersion!="ta-browser.v4"&&wire.wireVersion!="ta-browser.v5")return Error_1("Unsupported TA browser state wire.");
  else if(!(wire.rows==null)&&exists((row) =>!(row.traces==null)&&exists((trace) => IsError(traceKind(trace.kind)), row.traces), wire.rows))return Error_1("TA browser state contains an unsupported trace kind.");
  else if((wire.wireVersion=="ta-browser.v4"||wire.wireVersion=="ta-browser.v5")&&!(wire.series==null)&&exists((series) => {
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
        Kind:DefaultWith(InvalidOp, traceKind(trace.kind)), 
        DataRef:text(trace.dataRef), 
        Label:text(trace.label), 
        Color:text(trace.color), 
        Width:trace.width, 
        Visible:trace.visible, 
        CandleDataRefs:trace.hasCandleDataRefs?Some({
          OpenRef:text(trace.candleOpenRef), 
          HighRef:text(trace.candleHighRef), 
          LowRef:text(trace.candleLowRef), 
          CloseRef:text(trace.candleCloseRef), 
          VolumeRef:text(trace.candleVolumeRef)
        }):null, 
        Options:new FSharpMap("New", [])
      }), row.traces);
      return{
        RowId:R, 
        Kind:K, 
        DataRef:D, 
        HeightWeight:row.heightWeight, 
        Visible:row.visible, 
        Options:mapFromWire(row.options), 
        Traces:T_1
      };
    }, wire.rows);
    const seriesData=wire.series==null?new FSharpMap("New", []):OfArray(map((series) => {
      const points=seriesPointValues(wire, series);
      return[text(series.dataRef), {$:4, $0:points}];
    }, wire.series));
    const sharedTemporalData=mapFromWire(wire.sharedTemporalData);
    const status={$:5, $0:new FSharpMap("New", ofArray([["label", {$:3, $0:text(wire.statusLabel)}], ["freshness", {$:3, $0:text(wire.freshness)}], ["watermarkUtc", {$:3, $0:text(wire.watermarkUtc)}], ["quality", {$:3, $0:text(wire.quality)}], ["lagSeconds", {$:2, $0:wire.lagSeconds}], ["reasonCode", {$:3, $0:text(wire.reasonCode)}]]))};
    const data=Fold((_1, _2, _3) => _1.Add_1(_2, _3), seriesData, sharedTemporalData).Add_1(text(wire.statusRef), status);
    const defaultView=OfArray(ofSeq(ofSeq_1(delay(() => append_2(!IsNullOrWhiteSpace(wire.querySourceId)?[["query.sourceId", {$:3, $0:text(wire.querySourceId)}]]:[], delay(() => append_2(!IsNullOrWhiteSpace(wire.queryInstrument)?[["query.instrument", {$:3, $0:text(wire.queryInstrument)}]]:[], delay(() => append_2(wire.queryIntervalMinutes>0?[["query.intervalMinutes", {$:2, $0:wire.queryIntervalMinutes}]]:[], delay(() => append_2(!IsNullOrWhiteSpace(wire.queryFromUtc)?[["query.fromUtc", {$:3, $0:text(wire.queryFromUtc)}]]:[], delay(() => append_2(!IsNullOrWhiteSpace(wire.queryToUtcExclusive)?[["query.toUtcExclusive", {$:3, $0:text(wire.queryToUtcExclusive)}]]:[], delay(() =>[["query.includePartial", {$:1, $0:wire.queryIncludePartial}]]))))))))))))));
    const lastError=IsNullOrWhiteSpace(wire.errorCode)&&IsNullOrWhiteSpace(wire.errorMessage)?null:Some({
      ReasonCode:text(wire.errorCode), 
      Message:text(wire.errorMessage), 
      Recoverable:wire.errorRecoverable
    });
    if(wire.editorSchemas==null)editorSchemas=[];
    else {
      const f=(x) => fromValue(valueFromWire(x));
      editorSchemas=choose((x) => ToOption(f(x)), wire.editorSchemas);
    }
    return!(wire.editorSchemas==null)&&length(editorSchemas)!==length(wire.editorSchemas)?Error_1("TA browser editor schema catalog is invalid."):Ok({
      Identity:{DocumentId:{$:0, $0:text(wire.documentId)}, CanvasInstanceId:{$:0, $0:text(wire.canvasInstanceId)}}, 
      Document:Some({
        WorkspaceId:text(wire.workspaceId), 
        Title:text(wire.title), 
        RowsRef:text(wire.rowsRef), 
        StatusRef:text(wire.statusRef), 
        SharedTimeAxis:wire.sharedTimeAxis, 
        TemporalAxisRefs:wire.temporalAxisRefs==null?[]:wire.temporalAxisRefs, 
        BaseRowId:IsNullOrWhiteSpace(wire.baseRowId)?null:Some(text(wire.baseRowId)), 
        Rows:rows, 
        EditorSchemas:editorSchemas, 
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
function mapFromWire(fields){
  return fields==null?new FSharpMap("New", []):OfArray(map((field_1) =>[text(field_1.key), valueFromWire(field_1.value)], fields));
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
function traceKind(value){
  const m=Trim(text(value)).toLowerCase();
  return m=="candlestick"?Ok({$:0}):m=="volume"?Ok({$:1}):m=="line"?Ok({$:2}):m=="histogram"?Ok({$:3}):m=="marker"?Ok({$:4}):m=="overview-stripe"?Ok({$:5}):Error_1("Unsupported TA browser trace kind `"+m+"`.");
}
function temporalSeriesMetadataIsValid(count, series){
  return!series.hasTemporal||!(series.sourceIntervalIds==null)&&length(series.sourceIntervalIds)===count&&!(series.scaleKeys==null)&&length(series.scaleKeys)===count&&!(series.intervalStartUtc==null)&&length(series.intervalStartUtc)===count&&!(series.intervalEndUtc==null)&&length(series.intervalEndUtc)===count&&!(series.observedThroughUtc==null)&&length(series.observedThroughUtc)===count&&!(series.availableAtUtc==null)&&length(series.availableAtUtc)===count&&!(series.hasAvailableAtUtc==null)&&length(series.hasAvailableAtUtc)===count&&!(series.finality==null)&&length(series.finality)===count&&!(series.projections==null)&&length(series.projections)===count&&!(series.qualities==null)&&length(series.qualities)===count;
}
function valueFromWire(wire){
  if(wire==null)return{$:0};
  else {
    const m=text(wire.kind);
    return m=="bool"?{$:1, $0:wire.boolValue}:m=="number"?{$:2, $0:wire.numberValue}:m=="text"?{$:3, $0:text(wire.textValue)}:m=="array"?wire.items==null?{$:4, $0:[]}:{$:4, $0:map(valueFromWire, wire.items)}:m=="object"?{$:5, $0:mapFromWire(wire.fields)}:{$:0};
  }
}
function pollState(value){
  const m=text(value);
  return m=="mounted-idle"?{$:1}:m=="ready"?{$:2}:m=="poll-in-flight"?{$:3}:m=="suspended"?{$:5}:m=="paused-for-resync"?{$:6}:m=="disposed"?{$:7}:{$:0};
}
function seriesPointValues(wire, series){
  return wire.wireVersion=="ta-browser.v3"||wire.wireVersion=="ta-browser.v4"||wire.wireVersion=="ta-browser.v5"?columnarPointValues(wire.timeline, series):series.points==null?[]:map(pointValue, series.points);
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
  return{$:5, $0:OfArray(ofSeq(ofSeq_1(delay(() => append_2(!IsNullOrWhiteSpace(time)?[["t", {$:3, $0:time}]]:[], delay(() => append_2(hasOpen?[["o", {$:2, $0:openValue}]]:[], delay(() => append_2(hasHigh?[["h", {$:2, $0:highValue}]]:[], delay(() => append_2(hasLow?[["l", {$:2, $0:lowValue}]]:[], delay(() => append_2(hasClose?[["c", {$:2, $0:closeValue}]]:[], delay(() => append_2(hasVolume?[["v", {$:2, $0:volumeValue}]]:[], delay(() => hasLineValue?[["v", {$:2, $0:lineValue}]]:[]))))))))))))))))};
}
function temporalPointValue(sourceIntervalId, scaleKey, intervalStartUtc, intervalEndUtc, observedThroughUtc, availableAtUtc, hasAvailableAtUtc, finality, projection, quality, payload){
  return{$:5, $0:OfArray(ofSeq(ofSeq_1(delay(() => append_2([["_type", {$:3, $0:"temporal-point.v1"}]], delay(() => append_2([["sourceIntervalId", {$:3, $0:sourceIntervalId}]], delay(() => append_2([["scaleKey", {$:3, $0:scaleKey}]], delay(() => append_2([["intervalStartUtc", {$:3, $0:intervalStartUtc}]], delay(() => append_2([["intervalEndUtc", {$:3, $0:intervalEndUtc}]], delay(() => append_2([["observedThroughUtc", {$:3, $0:observedThroughUtc}]], delay(() => append_2([["finality", {$:3, $0:finality}]], delay(() => append_2([["projection", {$:3, $0:projection}]], delay(() => append_2([["value", payload]], delay(() => append_2(hasAvailableAtUtc?[["availableAtUtc", {$:3, $0:availableAtUtc}]]:[], delay(() =>!IsNullOrWhiteSpace(quality)?[["quality", {$:3, $0:quality}]]:[]))))))))))))))))))))))))};
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
function renderWithDisplayTimeZone(options, callbacks, displayTimeZone, runtimeState){
  return renderWithProjectionCommitAndDisplayTimeZone(options, callbacks, () => { }, displayTimeZone, runtimeState);
}
function defaultOptions(){
  return _c_8.defaultOptions;
}
function renderWithProjectionCommitAndDisplayTimeZone(options, callbacks, onProjectionCommitted, displayTimeZone, runtimeState){
  let currentDisplayTimeZone, refreshDisplayTime, instrumentDraft, intervalDraft, fromDateDraft, toDateDraft, synchronizedDocumentKey, renderedNavigatorDraft, addRowSequence, pendingAddRowId, editingRowId, pendingEditorMutation, finishNavigatorDrag, activeNavigatorCursor, navigatorCursorAfterRender, chartRenderSequence, chartRenderReason, chartStackElement, latestCursorTimestamps, latestCursorReaders, latestLegendReaders, latestLegendValueReaders, latestMarkerCursorReaders, cursorPanelElement, latestRowLegendElements, displayedCursorIndex, refreshVisibleValues, visibleValueRefreshScheduled, visibleValueSchedulerSequence, visibleValueTelemetrySequence, visibleValueTelemetryMaxTotalMs, chartWorkGeneration, dataWorkGeneration, activeRowDataStates, projectionCommitGate, pendingCursorIndex, cursorFrameScheduled, pendingCursorRequestedAtMs, cursorRenderLatencySequence, pendingViewportChartState, viewportChartFrameScheduled, defaultViewportAppliedToCanvas, actionSequence, querySelectionGeneration, queryInFlight, queuedQuery, pendingBoundaryPan, queuedViewportIntent, flushQueuedViewportIntent, dispatchAdjacentCoverage, dispatchCoverageWindow, latestPreparedData, preparedDataReady, preparationGeneration, observedChartTopology, observedDataState;
  ensureAxisResizeTracking();
  currentDisplayTimeZone={$:0};
  const displayTime={Zone:displayTimeZone, Current:() => currentDisplayTimeZone};
  refreshDisplayTime=() => { };
  const rendererTelemetryInstanceId=nextRendererTelemetryInstanceId();
  const currentCanvasId=() => runtimeState.Get().Identity.CanvasInstanceId;
  const rowHeightStates=new Dictionary("New_5");
  const configuredEditorSchemas=options.EditorSchemas==null?[]:options.EditorSchemas;
  const editorSchemasNow=() => {
    const o_2=runtimeState.Get().Document;
    const o_3=o_2==null?null:Some(o_2.$0.EditorSchemas);
    const values=o_3==null?[]:o_3.$0;
    const documentSchemas=values==null?[]:values;
    return filter_1((schema) => validateSchema(limits(), schema).$!=1, length(documentSchemas)>0?documentSchemas:configuredEditorSchemas);
  };
  const initialEditorSchema=tryHead(editorSchemasNow());
  const o=initialEditorSchema==null?null:Some(initialEditorSchema.$0.TemplateKey);
  let _1=o==null?"":o.$0;
  const selectedTemplate=_c_2.Create_1(_1);
  const o_1=initialEditorSchema==null?null:Some(initialEditorInputs(initialEditorSchema.$0));
  let _2=o_1==null?[]:o_1.$0;
  const editorValues=_c_2.Create_1(_2);
  instrumentDraft="";
  intervalDraft="";
  fromDateDraft="";
  toDateDraft="";
  synchronizedDocumentKey=null;
  const addKind=_c_2.Create_1("Sma");
  const addDataRef=_c_2.Create_1("series.sma");
  const addPeriod=_c_2.Create_1("20");
  const addDiPeriod=_c_2.Create_1("14");
  const addAdxPeriod=_c_2.Create_1("14");
  const addFastPeriod=_c_2.Create_1("12");
  const addSlowPeriod=_c_2.Create_1("26");
  const addSignalPeriod=_c_2.Create_1("9");
  const draftWindow=_c_2.Create_1(null);
  renderedNavigatorDraft=null;
  addRowSequence=0;
  pendingAddRowId=null;
  editingRowId=null;
  pendingEditorMutation=null;
  finishNavigatorDrag=null;
  activeNavigatorCursor=null;
  navigatorCursorAfterRender=null;
  chartRenderSequence=0;
  chartRenderReason="initial";
  const cursorIndex=_c_2.Create_1(null);
  chartStackElement=null;
  latestCursorTimestamps=[];
  latestCursorReaders=[];
  latestLegendReaders=new FSharpMap("New", []);
  latestLegendValueReaders=new FSharpMap("New", []);
  latestMarkerCursorReaders=new FSharpMap("New", []);
  cursorPanelElement=null;
  latestRowLegendElements=[];
  displayedCursorIndex=null;
  refreshVisibleValues=() => { };
  visibleValueRefreshScheduled=false;
  visibleValueSchedulerSequence=0;
  visibleValueTelemetrySequence=0;
  visibleValueTelemetryMaxTotalMs=0;
  const scheduleVisibleValueRefresh=() => {
    if(!visibleValueRefreshScheduled){
      visibleValueRefreshScheduled=true;
      requestAnimationFrame(() => {
        let o_2, m, _3;
        const schedulerStarted=Date.now();
        visibleValueRefreshScheduled=false;
        refreshVisibleValues();
        const schedulerElapsed=Date.now()-schedulerStarted;
        const root=globalThis.document.documentElement;
        if(!(root==null)){
          visibleValueSchedulerSequence=visibleValueSchedulerSequence+1;
          o_2=0;
          const _4=Number(root.getAttribute("data-visible-value-global-max-scheduler-ms"));
          let _5=isNaN(_4)?false:(o_2=_4,true);
          m=[_5, o_2];
          const previousMax=m[0]?m[1]:0;
          _3=(root.setAttribute("data-visible-value-global-last-scheduler-ms", fixedText(schedulerElapsed)),root.setAttribute("data-visible-value-global-last-instance", rendererTelemetryInstanceId),root.setAttribute("data-visible-value-global-last-render-sequence", String(chartRenderSequence)),root.setAttribute("data-visible-value-global-last-scheduler-sequence", String(visibleValueSchedulerSequence)),schedulerElapsed>previousMax?(root.setAttribute("data-visible-value-global-max-scheduler-ms", fixedText(schedulerElapsed)),root.setAttribute("data-visible-value-global-max-instance", rendererTelemetryInstanceId),root.setAttribute("data-visible-value-global-max-render-sequence", String(chartRenderSequence)),root.setAttribute("data-visible-value-global-max-scheduler-sequence", String(visibleValueSchedulerSequence))):void 0);
        }
        else _3=void 0;
        if(!(chartStackElement==null)){
          chartStackElement.setAttribute("data-visible-value-scheduler-ms", fixedText(schedulerElapsed));
          chartStackElement.setAttribute("data-visible-value-renderer-instance", rendererTelemetryInstanceId);
        }
      });
    }
  };
  Sink((zone) => {
    currentDisplayTimeZone=zone;
    refreshDisplayTime();
  }, displayTimeZone);
  chartWorkGeneration=0;
  dataWorkGeneration=0;
  activeRowDataStates=[];
  const scheduleNextFrame=(work) => {
    requestAnimationFrame(() => {
      work();
    });
  };
  projectionCommitGate=initial_1();
  const beginProjection=(state) => {
    const p=beginCandidate(state, projectionCommitGate);
    projectionCommitGate=p[0];
    return p[1];
  };
  const completeProjection=(generation, state) => scheduleNextFrame(() => {
    const p=tryCommit(runtimeState.Get(), generation, state, projectionCommitGate);
    projectionCommitGate=p[0];
    const o_2=p[1];
    if(o_2==null){ }
    else onProjectionCommitted(o_2.$0);
  });
  pendingCursorIndex=null;
  cursorFrameScheduled=false;
  pendingCursorRequestedAtMs=0;
  cursorRenderLatencySequence=0;
  const crossScaleSummaryOpen=_c_2.Create_1(false);
  const uiState=_c_2.Create_1({
    Window:{StartIndex:0, Count:options.DefaultVisibleBars}, 
    FollowLatest:true, 
    HiddenRows:new FSharpSet("New_2", null), 
    HiddenTraces:new FSharpSet("New_2", null), 
    RemovedTraces:new FSharpSet("New_2", null), 
    AddRowOpen:false, 
    CursorIndex:null, 
    PendingActionId:null, 
    Feedback:""
  });
  const sameChartUiState=(left, right) => Equals(left.Window, right.Window)&&left.FollowLatest==right.FollowLatest&&Equals(left.HiddenRows, right.HiddenRows)&&Equals(left.HiddenTraces, right.HiddenTraces)&&Equals(left.RemovedTraces, right.RemovedTraces);
  const chartUiState=_c_2.Create_1(uiState.Get());
  pendingViewportChartState=null;
  viewportChartFrameScheduled=false;
  const setUiState=(next) => {
    let _3;
    const previousChartState=chartUiState.Get();
    uiState.Set(next);
    if(pendingViewportChartState!=null&&pendingViewportChartState.$==1&&(sameChartUiState(pendingViewportChartState.$0, next)&&(_3=pendingViewportChartState.$0,true)))pendingViewportChartState=Some(next);
    else {
      pendingViewportChartState=null;
      !sameChartUiState(previousChartState, next)?(chartRenderReason="ui-state",chartUiState.Set(next)):void 0;
    }
  };
  defaultViewportAppliedToCanvas=null;
  const maximumVisibleBarsFor=(document) => documentMaximumVisibleBars(options.MaximumVisibleBars, document.DefaultView);
  const viewportScopeKey=(state, document) => {
    let _3=canvasIdText(state.Identity.CanvasInstanceId)+"|";
    const o_2=tryLoadedCoverageResolved(document.DefaultView, state.Data);
    const o_3=o_2==null?null:Some(o_2.$0.CoverageIdentity);
    let _4=o_3==null?"legacy":o_3.$0;
    return _3+_4;
  };
  actionSequence=0;
  querySelectionGeneration=0;
  queryInFlight=false;
  queuedQuery=null;
  pendingBoundaryPan=null;
  queuedViewportIntent=null;
  flushQueuedViewportIntent=() => { };
  dispatchAdjacentCoverage=() =>() => null;
  dispatchCoverageWindow=() =>() =>() => null;
  const preparedRowsReady=_c_2.Create_1(false);
  const viewportDataReady=_c_2.Create_1(false);
  const commandsDisabledView=Map2((_3, _4) => remoteDisabled(_3.Poll)||_4.PendingActionId!=null, runtimeState.View, uiState.View);
  const commandsDisabledNow=() => remoteDisabled(runtimeState.Get().Poll)||uiState.Get().PendingActionId!=null;
  const viewportCommandsDisabledView=Map2((_3, _4) => _3||!_4, Map((state) => localViewportDisabled(state.Poll), runtimeState.View), viewportDataReady.View);
  const viewportCommandsDisabledNow=() =>!viewportDataReady.Get()||localViewportDisabled(runtimeState.Get().Poll);
  const startActionWithFeedback=(action, successText, onAccepted, onRejected, afterSettled) => {
    actionSequence=actionSequence+1;
    const request_1={
      RequestId:canvasIdText(currentCanvasId())+":ui:"+String(actionSequence), 
      ExpectedDocumentRevision:Some(runtimeState.Get().DocumentRevision), 
      Action:action
    };
    return submit(callbacks, uiState, runtimeState.Get().DocumentRevision, request_1, successText, onAccepted, onRejected, () => {
      afterSettled();
      scheduleNextFrame(flushQueuedViewportIntent);
    });
  };
  const startActionWith=(action, successText, onAccepted, onRejected) => startActionWithFeedback(action, successText, () => {
    onAccepted();
    return null;
  }, onRejected, () => { });
  const startAction=(action, successText, onAccepted) => startActionWith(action, successText, onAccepted, () => { });
  const sendOrQueueCoverageWindowAction=(targetStart, targetCount, successText) => {
    if(uiState.Get().PendingActionId!=null||remoteDisabled(runtimeState.Get().Poll)){
      queuedViewportIntent=Some({
        $:2, 
        $0:targetStart, 
        $1:targetCount, 
        $2:successText
      });
      const _3=uiState.Get();
      let _4={
        Window:_3.Window, 
        FollowLatest:_3.FollowLatest, 
        HiddenRows:_3.HiddenRows, 
        HiddenTraces:_3.HiddenTraces, 
        RemovedTraces:_3.RemovedTraces, 
        AddRowOpen:_3.AddRowOpen, 
        CursorIndex:_3.CursorIndex, 
        PendingActionId:_3.PendingActionId, 
        Feedback:"Loaded coverage request queued."
      };
      return setUiState(_4);
    }
    else return((dispatchCoverageWindow(targetStart))(targetCount))(successText);
  };
  flushQueuedViewportIntent=() => {
    if(queuedViewportIntent!=null&&queuedViewportIntent.$==1)if(queuedViewportIntent.$0,uiState.Get().PendingActionId==null&&!remoteDisabled(runtimeState.Get().Poll)){
      const intent=queuedViewportIntent.$0;
      queuedViewportIntent=null;
      if(intent.$==1){
        const direction=intent.$0;
        const delta=intent.$1;
        (dispatchAdjacentCoverage(direction))(delta);
      }
      else if(intent.$==2){
        const targetStart=intent.$0;
        const targetCount=intent.$1;
        const successText=intent.$2;
        ((dispatchCoverageWindow(targetStart))(targetCount))(successText);
      }
      else startAction(intent.$0, "Visible range synchronized.", () => { });
    }
  };
  Sink(() => {
    scheduleNextFrame(flushQueuedViewportIntent);
  }, Map2((_3, _4) =>[_3.Poll, _4.PendingActionId], runtimeState.View, uiState.View));
  const chartRuntimeState=_c_2.Create_1(runtimeState.Get());
  const initialPreparedData={
    RawData:new FSharpMap("New", []), 
    ResolvedAxes:new FSharpMap("New", []), 
    ResolvedSeries:new FSharpMap("New", [])
  };
  latestPreparedData=initialPreparedData;
  const shellPreparedData=_c_2.Create_1(initialPreparedData);
  const chartShellPreparedData=_c_2.Create_1(initialPreparedData);
  preparedDataReady=false;
  preparationGeneration=0;
  observedChartTopology=chartTopologySignaturePrepared(runtimeState.Get(), initialPreparedData);
  observedDataState=runtimeState.Get();
  const acceptPreparedData=(refreshRows, next, nextPreparedData) => {
    let o_2, _3, reanchored, _4, _5;
    const _6=next.Document;
    if(!(pendingBoundaryPan!=null&&pendingBoundaryPan.$==1&&(_6!=null&&_6.$==1&&isStaleCoverageCandidate(pendingBoundaryPan.$0.QueryGeneration, _6.$0.DefaultView)))){
      const nextChartTopology=chartTopologySignaturePrepared(next, nextPreparedData);
      const o_3=next.Document;
      if(o_3==null)o_2=null;
      else {
        const document=o_3.$0;
        let _7=!Equals(defaultViewportAppliedToCanvas, Some(viewportScopeKey(next, document)));
        o_2=Some(_7);
      }
      const viewportScopeChanged=o_2==null?false:o_2.$0;
      const topologyChanged=!Equals(next.Identity, chartRuntimeState.Get().Identity)||!sameDocumentPresentation(chartRuntimeState.Get(), next)||!Equals(nextChartTopology, observedChartTopology);
      if(topologyChanged){
        beginProjection(next);
        chartRenderReason=!Equals(next.Identity, chartRuntimeState.Get().Identity)?"identity":next.DocumentRevision!==chartRuntimeState.Get().DocumentRevision?"document-revision":"topology-signature";
        observedChartTopology=nextChartTopology;
        shellPreparedData.Set(nextPreparedData);
        chartShellPreparedData.Set(nextPreparedData);
        const m=next.Document;
        if(m==null)_3=void(pendingBoundaryPan=null);
        else {
          const document_1=m.$0;
          const maximumVisibleBars=maximumVisibleBarsFor(document_1);
          const nextTimeline=referenceTimelineForDocumentPrepared(document_1, nextPreparedData);
          const currentUi=uiState.Get();
          const generalOldTimeline=referenceTimelineForDocumentPrepared(document_1, latestPreparedData);
          const generalOldWindow=resolveWindow(options.MinimumVisibleBars, maximumVisibleBars, length(generalOldTimeline), currentUi.FollowLatest, currentUi.Window);
          if(viewportScopeChanged)reanchored=(pendingBoundaryPan=null,Some(initialViewportWindow(options.MinimumVisibleBars, options.DefaultVisibleBars, maximumVisibleBars, length(nextTimeline), document_1.DefaultView)));
          else if(pendingBoundaryPan!=null&&pendingBoundaryPan.$==1){
            let _8, _9;
            const pending=pendingBoundaryPan.$0;
            const m_1=tryLoadedCoverageResolved(document_1.DefaultView, next.Data);
            if(m_1!=null&&m_1.$==1){
              const projection=m_1.$0;
              if(projection.QueryGeneration>=pending.QueryGeneration){
                const o_4=pending.TargetStartObservationOrdinal;
                _8=o_4==null||projection.ActiveDetail.StartObservationOrdinal===o_4.$0;
              }
              else _8=false;
              _9=_8?(_4=m_1.$0,0):coverageExtended(pending.Direction, pending.IntentTimeline, nextTimeline)?1:2;
            }
            else _9=coverageExtended(pending.Direction, pending.IntentTimeline, nextTimeline)?1:2;
            switch(_9){
              case 0:
                pendingBoundaryPan=null;
                let _10=length(nextTimeline);
                const a=pending.ObservationCount;
                const b=length(nextTimeline);
                let _11=Compare(a, b)===-1?a:b;
                let _12={StartIndex:0, Count:_11};
                let _13=clampWindow(options.MinimumVisibleBars, maximumVisibleBars, _10, _12);
                reanchored=Some(_13);
                break;
              case 1:
                reanchored=(pendingBoundaryPan=null,tryReanchorWindow(options.MinimumVisibleBars, maximumVisibleBars, pending.LegacyDelta, pending.IntentTimeline, nextTimeline, pending.IntentWindow));
                break;
              case 2:
                reanchored=null;
                break;
            }
          }
          else reanchored=!currentUi.FollowLatest&&length(generalOldTimeline)>0&&length(nextTimeline)>length(generalOldTimeline)?tryReanchorWindow(options.MinimumVisibleBars, maximumVisibleBars, 0, generalOldTimeline, nextTimeline, generalOldWindow):null;
          if(reanchored==null)_3=null;
          else {
            const window_1=reanchored.$0;
            _3=(setUiState({
              Window:window_1, 
              FollowLatest:window_1.StartIndex===viewportMaximumStart(length(nextTimeline), window_1), 
              HiddenRows:currentUi.HiddenRows, 
              HiddenTraces:currentUi.HiddenTraces, 
              RemovedTraces:currentUi.RemovedTraces, 
              AddRowOpen:currentUi.AddRowOpen, 
              CursorIndex:null, 
              PendingActionId:currentUi.PendingActionId, 
              Feedback:currentUi.Feedback
            }),cursorIndex.Set(null),viewportScopeChanged?void(defaultViewportAppliedToCanvas=Some(viewportScopeKey(next, document_1))):null);
          }
        }
        _5=chartRuntimeState.Set(next);
      }
      else if(refreshRows){
        const projectionCandidateGeneration=beginProjection(next);
        shellPreparedData.Set(nextPreparedData);
        dataWorkGeneration=dataWorkGeneration+1;
        const generation=dataWorkGeneration;
        const targets=activeRowDataStates;
        function update(index){
          if(generation===dataWorkGeneration)if(index<length(targets))scheduleNextFrame(() => {
            if(generation===dataWorkGeneration){
              get(targets, index).Set(nextPreparedData);
              update(index+1);
            }
          });
          else completeProjection(projectionCandidateGeneration, next);
        }
        _5=update(0);
      }
      else _5=null;
      latestPreparedData=nextPreparedData;
      observedDataState=next;
      return;
    }
    else return null;
  };
  const scheduleFullPreparation=() => {
    preparationGeneration=preparationGeneration+1;
    const generation=preparationGeneration;
    preparedDataReady=false;
    viewportDataReady.Set(false);
    const candidate=runtimeState.Get();
    chartRuntimeState.Set(candidate);
    prepareDataScheduled(scheduleNextFrame, candidate.Data, (prepared) => {
      if(generation===preparationGeneration){
        let _3;
        const current=runtimeState.Get();
        if(!runtimeDataChanged(candidate, current)){
          latestPreparedData=prepared;
          shellPreparedData.Set(prepared);
          chartShellPreparedData.Set(prepared);
          observedChartTopology=chartTopologySignaturePrepared(current, prepared);
          observedDataState=current;
          preparedDataReady=true;
          viewportDataReady.Set(true);
          const m=current.Document;
          if(m!=null&&m.$==1){
            const document=m.$0;
            const scopeKey=viewportScopeKey(current, document);
            if(!Equals(defaultViewportAppliedToCanvas, Some(scopeKey))){
              const total=referenceTimelineForDocumentPrepared(document, prepared).length;
              if(total>0){
                const window_1=initialViewportWindow(options.MinimumVisibleBars, options.DefaultVisibleBars, maximumVisibleBarsFor(document), total, document.DefaultView);
                const _4=uiState.Get();
                let _5={
                  Window:window_1, 
                  FollowLatest:true, 
                  HiddenRows:_4.HiddenRows, 
                  HiddenTraces:_4.HiddenTraces, 
                  RemovedTraces:_4.RemovedTraces, 
                  AddRowOpen:_4.AddRowOpen, 
                  CursorIndex:null, 
                  PendingActionId:_4.PendingActionId, 
                  Feedback:_4.Feedback
                };
                setUiState(_5);
                cursorIndex.Set(null);
                _3=defaultViewportAppliedToCanvas=Some(scopeKey);
              }
              else _3=void 0;
            }
            else _3=void 0;
          }
          else _3=void 0;
          beginProjection(current);
          chartRuntimeState.Set(current);
        }
      }
    });
  };
  Sink((next) => {
    let o_2, _3;
    const dataChanged=runtimeDataChanged(observedDataState, next);
    const o_3=next.Document;
    if(o_3==null)o_2=null;
    else {
      const document=o_3.$0;
      let _4=!Equals(defaultViewportAppliedToCanvas, Some(viewportScopeKey(next, document)));
      o_2=Some(_4);
    }
    const viewportScopeChanged=o_2==null?false:o_2.$0;
    if(!Equals(next.Identity, observedDataState.Identity)){
      pendingBoundaryPan=null;
      queuedViewportIntent=null;
      const _5=uiState.Get();
      let _6={
        Window:_5.Window, 
        FollowLatest:_5.FollowLatest, 
        HiddenRows:new FSharpSet("New_2", null), 
        HiddenTraces:new FSharpSet("New_2", null), 
        RemovedTraces:new FSharpSet("New_2", null), 
        AddRowOpen:_5.AddRowOpen, 
        CursorIndex:null, 
        PendingActionId:_5.PendingActionId, 
        Feedback:_5.Feedback
      };
      setUiState(_6);
      cursorIndex.Set(null);
      observedDataState=next;
      _3=scheduleFullPreparation();
    }
    else if(!preparedDataReady)_3=dataChanged?(observedDataState=next,scheduleFullPreparation()):observedDataState=next;
    else if(dataChanged){
      if(viewportScopeChanged)_3=(queuedViewportIntent=null,pendingBoundaryPan=null,observedDataState=next,scheduleFullPreparation());
      else {
        preparationGeneration=preparationGeneration+1;
        const generation=preparationGeneration;
        const o_4=observedDataState.Document;
        const previousCoverageProjection=o_4==null?null:tryLoadedCoverageResolved(o_4.$0.DefaultView, latestPreparedData.RawData);
        observedDataState=next;
        const o_5=next.Document;
        let _7=o_5==null?null:tryLoadedCoverageResolved(o_5.$0.DefaultView, next.Data);
        const coverageChanged=!Equals(previousCoverageProjection, _7);
        const accept=(prepared) => {
          if(generation===preparationGeneration){
            const current=runtimeState.Get();
            if(!runtimeDataChanged(next, current)){
              acceptPreparedData(true, current, prepared);
              coverageChanged?chartShellPreparedData.Set(prepared):void 0;
            }
          }
        };
        _3=coverageChanged?prepareDataScheduled(scheduleNextFrame, next.Data, accept):prepareDataIncrementalScheduled(scheduleNextFrame, latestPreparedData, next.Data, accept);
      }
    }
    else _3=acceptPreparedData(false, next, latestPreparedData);
    flushQueuedViewportIntent();
  }, runtimeState.View);
  scheduleFullPreparation();
  const chartRuntimeView=chartRuntimeState.View;
  const actionAllowed=(actionName) => {
    const o_2=runtimeState.Get().Document;
    const o_3=o_2==null?null:Some(o_2.$0.AllowedActions);
    let _3=o_3==null?[]:o_3.$0;
    return arrContains(actionName, _3);
  };
  const referenceLength=() => {
    const m=runtimeState.Get().Document;
    return m!=null&&m.$==1?referenceTimelineForDocumentPrepared(m.$0, latestPreparedData).length:0;
  };
  const maximumVisibleBarsNow=() => {
    const o_2=runtimeState.Get().Document;
    const o_3=o_2==null?null:Some(maximumVisibleBarsFor(o_2.$0));
    return o_3==null?options.MaximumVisibleBars:o_3.$0;
  };
  const resolvedWindow=(ui) => resolveWindow(options.MinimumVisibleBars, maximumVisibleBarsNow(), referenceLength(), ui.FollowLatest, ui.Window);
  const commitLocalWindow=(followLatest, window_1) => {
    const current=uiState.Get();
    const total=referenceLength();
    const bounded=resolveWindow(options.MinimumVisibleBars, maximumVisibleBarsNow(), total, followLatest, window_1);
    const changed=!Equals(bounded, resolvedWindow(current))||followLatest!=current.FollowLatest;
    const next={
      Window:bounded, 
      FollowLatest:followLatest, 
      HiddenRows:current.HiddenRows, 
      HiddenTraces:current.HiddenTraces, 
      RemovedTraces:current.RemovedTraces, 
      AddRowOpen:current.AddRowOpen, 
      CursorIndex:null, 
      PendingActionId:current.PendingActionId, 
      Feedback:current.Feedback
    };
    uiState.Set(next);
    if(!sameChartUiState(chartUiState.Get(), next)){
      pendingViewportChartState=Some(next);
      !viewportChartFrameScheduled?(viewportChartFrameScheduled=true,void requestAnimationFrame(() => {
        requestAnimationFrame(() => {
          viewportChartFrameScheduled=false;
          if(pendingViewportChartState==null){ }
          else {
            const pending=pendingViewportChartState.$0;
            pendingViewportChartState=null;
            if(!sameChartUiState(chartUiState.Get(), pending)){
              chartRenderReason="viewport";
              chartUiState.Set(pending);
            }
          }
        });
      })):null;
    }
    else null;
    cursorIndex.Set(null);
    draftWindow.Set(null);
    return[changed, bounded];
  };
  const setWindow=(followLatest, window_1) => {
    if(!localViewportDisabled(runtimeState.Get().Poll)){
      const p=commitLocalWindow(followLatest, window_1);
      if(p[0]&&actionAllowed("visible-range-changed")){
        const m=runtimeState.Get().Document;
        if(m==null)return null;
        else {
          const document=m.$0;
          const m_1=visibleEventRangePrepared(document, latestPreparedData, p[1]);
          if(m_1==null)return null;
          else {
            const range_1=m_1.$0;
            let _3=currentCanvasId();
            const a=4000;
            const a_1=1;
            const b=maximumVisibleBarsFor(document);
            const b_1=Compare(a_1, b)===1?a_1:b;
            let _4=Compare(a, b_1)===-1?a:b_1;
            let _5={
              BaseRowId:range_1.BaseRowId, 
              StartEventTimeUtc:range_1.StartEventTimeUtc, 
              EndEventTimeExclusiveUtc:range_1.EndEventTimeExclusiveUtc, 
              MaximumBasePoints:_4, 
              CoverageIntent:null
            };
            const action={
              $:8, 
              $0:_3, 
              $1:_5
            };
            return uiState.Get().PendingActionId!=null||remoteDisabled(runtimeState.Get().Poll)?void(queuedViewportIntent=Some({$:0, $0:action})):startAction(action, "Visible range synchronized.", () => { });
          }
        }
      }
      else return null;
    }
    else return null;
  };
  dispatchAdjacentCoverage=(_3) =>(_4) => {
    let _5, coverageIntent;
    if(actionAllowed("visible-range-changed")&&preparedRowsReady.Get()&&!localViewportDisabled(runtimeState.Get().Poll)){
      const m=runtimeState.Get().Document;
      if(m==null)return null;
      else {
        const document=m.$0;
        const a=4000;
        const a_1=1;
        const b=maximumVisibleBarsFor(document);
        const b_1=Compare(a_1, b)===1?a_1:b;
        let _6=Compare(a, b_1)===-1?a:b_1;
        const m_1=tryAdjacentCoverageRange(_3, _6, document, latestPreparedData);
        if(m_1==null){
          const _7=uiState.Get();
          let _8={
            Window:_7.Window, 
            FollowLatest:_7.FollowLatest, 
            HiddenRows:_7.HiddenRows, 
            HiddenTraces:_7.HiddenTraces, 
            RemovedTraces:_7.RemovedTraces, 
            AddRowOpen:_7.AddRowOpen, 
            CursorIndex:_7.CursorIndex, 
            PendingActionId:_7.PendingActionId, 
            Feedback:Equals(_3, {$:0})?"Earlier coverage is outside the configured query boundary.":"Later coverage is outside the configured query boundary."
          };
          return setUiState(_8);
        }
        else {
          const change=m_1.$0;
          const timeline=referenceTimelineForDocumentPrepared(document, latestPreparedData);
          const maximumVisibleBars=maximumVisibleBarsFor(document);
          const window_1=resolveWindow(options.MinimumVisibleBars, maximumVisibleBars, length(timeline), uiState.Get().FollowLatest, uiState.Get().Window);
          const projection=tryLoadedCoverageResolved(document.DefaultView, runtimeState.Get().Data);
          const a_2=1;
          const b_2=window_1.Count;
          const b_3=Compare(maximumVisibleBars, b_2)===-1?maximumVisibleBars:b_2;
          const count=Compare(a_2, b_3)===1?a_2:b_3;
          const m_2=change.CoverageIntent;
          if(m_2!=null&&m_2.$==1&&(Equals(m_2.$0.RangeAuthority, {$:1})&&(_5=m_2.$0,true))){
            if(projection==null)coverageIntent=tryProviderOpenEarlierWindowIntent(null, 0n, count);
            else {
              const coverage=projection.$0;
              coverageIntent=tryProviderOpenEarlierWindowIntent(Some(coverage.CoverageRevision), coverage.QueryGeneration+1n, count);
            }
          }
          else coverageIntent=projection==null?null:tryAdjacentCoverageIntent(_3, maximumVisibleBars, length(timeline), window_1, projection.$0);
          const targetStart=coverageIntent==null?null:coverageIntent.$0.StartObservationOrdinal;
          const o_2=coverageIntent==null?null:Some(coverageIntent.$0.QueryGeneration);
          const queryGeneration=o_2==null?0n:o_2.$0;
          const request_1={
            BaseRowId:change.BaseRowId, 
            StartEventTimeUtc:change.StartEventTimeUtc, 
            EndEventTimeExclusiveUtc:change.EndEventTimeExclusiveUtc, 
            MaximumBasePoints:count, 
            CoverageIntent:coverageIntent
          };
          pendingBoundaryPan=Some({
            Direction:_3, 
            LegacyDelta:_4, 
            IntentTimeline:timeline, 
            IntentWindow:window_1, 
            TargetStartObservationOrdinal:targetStart, 
            ObservationCount:count, 
            QueryGeneration:queryGeneration
          });
          return startActionWith({
            $:8, 
            $0:currentCanvasId(), 
            $1:request_1
          }, Equals(_3, {$:0})?"Earlier coverage requested.":"Later coverage requested.", () => { }, () => {
            pendingBoundaryPan=null;
          });
        }
      }
    }
    else return null;
  };
  const requestAdjacentCoverage=(direction, delta) => {
    if(actionAllowed("visible-range-changed")&&preparedRowsReady.Get()&&!localViewportDisabled(runtimeState.Get().Poll)){
      if(uiState.Get().PendingActionId!=null||remoteDisabled(runtimeState.Get().Poll)){
        queuedViewportIntent=Some({
          $:1, 
          $0:direction, 
          $1:delta
        });
        const _3=uiState.Get();
        let _4={
          Window:_3.Window, 
          FollowLatest:_3.FollowLatest, 
          HiddenRows:_3.HiddenRows, 
          HiddenTraces:_3.HiddenTraces, 
          RemovedTraces:_3.RemovedTraces, 
          AddRowOpen:_3.AddRowOpen, 
          CursorIndex:_3.CursorIndex, 
          PendingActionId:_3.PendingActionId, 
          Feedback:Equals(direction, {$:0})?"Earlier coverage queued.":"Later coverage queued."
        };
        return setUiState(_4);
      }
      else return(dispatchAdjacentCoverage(direction))(delta);
    }
    else return null;
  };
  const panWindow=(delta) => {
    const current=uiState.Get();
    const total=referenceLength();
    const visible=resolvedWindow(current);
    const requestedStart=visible.StartIndex+delta;
    const maximumStart=viewportMaximumStart(total, visible);
    if(requestedStart<0){
      if(remoteDisabled(runtimeState.Get().Poll))setWindow(false, {StartIndex:0, Count:visible.Count});
      else requestAdjacentCoverage({$:0}, delta);
    }
    else if(requestedStart>maximumStart){
      if(remoteDisabled(runtimeState.Get().Poll))setWindow(true, {StartIndex:maximumStart, Count:visible.Count});
      else requestAdjacentCoverage({$:1}, delta);
    }
    else {
      const candidate=clampWindow(options.MinimumVisibleBars, maximumVisibleBarsNow(), total, {StartIndex:requestedStart, Count:visible.Count});
      setWindow(candidate.StartIndex===viewportMaximumStart(total, candidate), candidate);
    }
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
      HiddenTraces:_3.HiddenTraces, 
      RemovedTraces:_3.RemovedTraces, 
      AddRowOpen:_3.AddRowOpen, 
      CursorIndex:_3.CursorIndex, 
      PendingActionId:_3.PendingActionId, 
      Feedback:"Local view reset."
    };
    setUiState(_4);
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
  dispatchCoverageWindow=(targetStart) =>(targetCount) =>(successText) => {
    const m=runtimeState.Get().Document;
    if(m==null)return null;
    else {
      const document=m.$0;
      const timeline=referenceTimelineForDocumentPrepared(document, latestPreparedData);
      const currentWindow=resolvedWindow(uiState.Get());
      const m_1=tryLoadedCoverageResolved(document.DefaultView, runtimeState.Get().Data);
      if(m_1==null){
        const _3=uiState.Get();
        let _4={
          Window:_3.Window, 
          FollowLatest:_3.FollowLatest, 
          HiddenRows:_3.HiddenRows, 
          HiddenTraces:_3.HiddenTraces, 
          RemovedTraces:_3.RemovedTraces, 
          AddRowOpen:_3.AddRowOpen, 
          CursorIndex:_3.CursorIndex, 
          PendingActionId:_3.PendingActionId, 
          Feedback:"Loaded coverage is unavailable."
        };
        return setUiState(_4);
      }
      else {
        const projection=m_1.$0;
        const m_2=tryLoadedCoverageWindowIntentAt(projection, targetStart, targetCount);
        if(m_2==null){
          const _5=uiState.Get();
          let _6={
            Window:_5.Window, 
            FollowLatest:_5.FollowLatest, 
            HiddenRows:_5.HiddenRows, 
            HiddenTraces:_5.HiddenTraces, 
            RemovedTraces:_5.RemovedTraces, 
            AddRowOpen:_5.AddRowOpen, 
            CursorIndex:_5.CursorIndex, 
            PendingActionId:_5.PendingActionId, 
            Feedback:"Loaded coverage target is no longer available."
          };
          return setUiState(_6);
        }
        else {
          const intent=m_2.$0;
          const o_2=intent.StartObservationOrdinal;
          const targetStart_1=o_2==null?0n:o_2.$0;
          const activeStart=projection.ActiveDetail.StartObservationOrdinal;
          const activeEnd=activeStart+BigInt(length(timeline));
          const targetEnd=targetStart_1+BigInt(intent.ObservationCount);
          if(targetStart_1>=activeStart&&targetEnd<=activeEnd)return setWindow(targetEnd===observationDomainCount(projection), {StartIndex:Number((targetStart_1-activeStart+2147483648n&4294967295n)-2147483648n), Count:intent.ObservationCount});
          else {
            const direction=targetStart_1<activeStart+BigInt(currentWindow.StartIndex)?{$:0}:{$:1};
            const m_3=tryAdjacentCoverageRange(direction, intent.ObservationCount, document, latestPreparedData);
            if(m_3==null){
              const _7=uiState.Get();
              let _8={
                Window:_7.Window, 
                FollowLatest:_7.FollowLatest, 
                HiddenRows:_7.HiddenRows, 
                HiddenTraces:_7.HiddenTraces, 
                RemovedTraces:_7.RemovedTraces, 
                AddRowOpen:_7.AddRowOpen, 
                CursorIndex:_7.CursorIndex, 
                PendingActionId:_7.PendingActionId, 
                Feedback:"Loaded coverage is outside the configured query boundary."
              };
              return setUiState(_8);
            }
            else {
              const range_1=m_3.$0;
              const pending={
                Direction:direction, 
                LegacyDelta:Equals(direction, {$:0})?-currentWindow.Count:currentWindow.Count, 
                IntentTimeline:timeline, 
                IntentWindow:currentWindow, 
                TargetStartObservationOrdinal:intent.StartObservationOrdinal, 
                ObservationCount:intent.ObservationCount, 
                QueryGeneration:intent.QueryGeneration
              };
              const action={
                $:8, 
                $0:currentCanvasId(), 
                $1:{
                  BaseRowId:range_1.BaseRowId, 
                  StartEventTimeUtc:range_1.StartEventTimeUtc, 
                  EndEventTimeExclusiveUtc:range_1.EndEventTimeExclusiveUtc, 
                  MaximumBasePoints:intent.ObservationCount, 
                  CoverageIntent:Some(intent)
                }
              };
              pendingBoundaryPan=Some(pending);
              return startActionWith(action, successText, () => { }, () => {
                pendingBoundaryPan=null;
              });
            }
          }
        }
      }
    }
  };
  const withLoadedCoverage=(operation) => {
    const m=runtimeState.Get().Document;
    if(m==null){ }
    else {
      const document=m.$0;
      const timeline=referenceTimelineForDocumentPrepared(document, latestPreparedData);
      const currentWindow=resolvedWindow(uiState.Get());
      const m_1=tryLoadedCoverageResolved(document.DefaultView, runtimeState.Get().Data);
      if(m_1==null)setWindowCount(maximumVisibleBarsFor(document));
      else {
        const projection=m_1.$0;
        (((operation(document))(timeline))(currentWindow))(projection);
      }
    }
  };
  const showLoadedCoverage=() => {
    withLoadedCoverage((document) =>() =>() =>(projection) => {
      const m=tryViewAllCoverageIntent(maximumVisibleBarsFor(document), projection);
      if(m==null)return setWindowCount(maximumVisibleBarsFor(document));
      else {
        const intent=m.$0;
        const o_2=intent.StartObservationOrdinal;
        return o_2==null?null:sendOrQueueCoverageWindowAction(o_2.$0, intent.ObservationCount, "Loaded coverage requested.");
      }
    });
  };
  const jumpToLoadedCoverageEdge=(edge) => {
    withLoadedCoverage((document) =>() =>(currentWindow) =>(projection) => {
      const m=tryLoadedCoverageEdgeIntent(edge, currentWindow.Count, maximumVisibleBarsFor(document), projection);
      if(m==null){
        const _3=uiState.Get();
        let _4={
          Window:_3.Window, 
          FollowLatest:_3.FollowLatest, 
          HiddenRows:_3.HiddenRows, 
          HiddenTraces:_3.HiddenTraces, 
          RemovedTraces:_3.RemovedTraces, 
          AddRowOpen:_3.AddRowOpen, 
          CursorIndex:_3.CursorIndex, 
          PendingActionId:_3.PendingActionId, 
          Feedback:"Loaded coverage is unavailable."
        };
        return setUiState(_4);
      }
      else {
        const intent=m.$0;
        const o_2=intent.StartObservationOrdinal;
        return o_2==null?null:sendOrQueueCoverageWindowAction(o_2.$0, intent.ObservationCount, Equals(edge, {$:0})?"Loaded start requested.":"Loaded end requested.");
      }
    });
  };
  const finishNavigatorDragFromElement=(event) => {
    event.preventDefault();
    finishNavigatorDrag==null?void 0:finishNavigatorDrag.$0();
  };
  const cursorElements=(selector) => {
    if(chartStackElement==null)return[];
    else {
      const nodes=chartStackElement.querySelectorAll(selector);
      return ofSeq(delay(() => map_2((index) => nodes[index], range(0, toInt(nodes.length)-1))));
    }
  };
  const scopedElements=(root, selector) => {
    if(root==null)return[];
    else {
      const nodes=root.querySelectorAll(selector);
      return ofSeq(delay(() => map_2((index) => nodes[index], range(0, toInt(nodes.length)-1))));
    }
  };
  const cursorPanelElements=(selector) => scopedElements(cursorPanelElement, selector);
  const rowLegendElements=(selector) => collect((root) => scopedElements(root, selector), latestRowLegendElements);
  const setElementTextIfChanged=(value, element_2) => element_2.textContent!=value&&(element_2.textContent=value,true);
  const setElementAttributeIfChanged=(name, value, element_2) => element_2.getAttribute(name)!=value&&(element_2.setAttribute(name, value),true);
  const removeElementAttributeIfPresent=(name, element_2) => element_2.hasAttribute(name)&&(element_2.removeAttribute(name),true);
  const setElementHiddenIfChanged=(hidden, element_2) => {
    let styleChanged, _3;
    const html=element_2;
    const authoredDisplayAttribute="data-ptcs-authored-display";
    styleChanged=false;
    if(hidden)_3=(!element_2.hasAttribute(authoredDisplayAttribute)?element_2.setAttribute(authoredDisplayAttribute, html.style.display):void 0,html.style.display!="none"?(html.style.display="none",void(styleChanged=true)):null);
    else if(element_2.hasAttribute(authoredDisplayAttribute)){
      const authoredDisplay=element_2.getAttribute(authoredDisplayAttribute);
      _3=(html.style.display!=authoredDisplay?(html.style.display=authoredDisplay,styleChanged=true):void 0,element_2.removeAttribute(authoredDisplayAttribute));
    }
    else _3=null;
    return hidden?!element_2.hasAttribute("hidden")?(element_2.setAttribute("hidden", "hidden"),true):styleChanged:removeElementAttributeIfPresent("hidden", element_2)||styleChanged;
  };
  const browserNowMs=() => Date.now();
  const applyVisibleCursorValues=(bounded) => {
    if(!(chartStackElement==null)){
      let cursorTimeUpdate, cursorValueUpdates, textWrites, attributeWrites, _3, visibilityWrites, _4, c, c_1;
      const totalStarted=browserNowMs();
      const hint=tryHead(cursorPanelElements("[data-ta-cursor-hint]"));
      const time=tryHead(cursorPanelElements("[data-ta-cursor-time]"));
      const valueNodes=cursorPanelElements("[data-ta-cursor-value-index]");
      const legendValueNodes=rowLegendElements("[data-ta-row-value-index]");
      const rowTimeNodes=rowLegendElements("[data-ta-row-data-time='true']");
      const ofiBands=cursorElements("[data-ta-row-ofi-band='true']");
      const queryCompleted=browserNowMs();
      const legendIndex=bounded==null?length(latestCursorTimestamps)>0?Some(length(latestCursorTimestamps)-1):null:Some(bounded.$0);
      const legendUpdates=mapi((valueIndex, node_3) => {
        let o_2, o_3;
        const m=(o_2=0,[TryParse(node_3.getAttribute("data-ta-row-value-index"), {get:() => o_2, set:(v) => {
          o_2=v;
        }}), o_2]);
        const traceIndex=m[0]?m[1]:valueIndex;
        const rowId=node_3.getAttribute("data-ta-row-value-row-id");
        if(legendIndex==null)o_3=null;
        else {
          const index_3=legendIndex.$0;
          o_3=bounded==null?tryLatestLegendValue(latestLegendValueReaders, rowId, traceIndex):tryLegendValue(latestLegendReaders, rowId, traceIndex, index_3);
        }
        const o_4=o_3==null?null:Some(o_3.$0.Value);
        const nextValue=o_4==null?"Unavailable":o_4.$0;
        return[node_3, nextValue, nextValue=="Unavailable"?"undefined":"defined"];
      }, legendValueNodes);
      const rowTimeUpdates=map((node_3) => {
        let o_2, o_3;
        const rowId=node_3.getAttribute("data-ta-row-data-time-row-id");
        if(legendIndex==null)o_2=null;
        else {
          const index_3=legendIndex.$0;
          o_2=bounded==null?tryLatestRowPresentation(latestLegendValueReaders, rowId):tryRowPresentation(latestLegendReaders, rowId, index_3);
        }
        if(o_2==null)o_3=null;
        else {
          const value_1=o_2.$0;
          let _6=fullOrOriginal(displayTime.Current(), value_1.Timestamp);
          o_3=Some(_6);
        }
        let _7=o_3==null?"Unavailable":o_3.$0;
        return[node_3, _7];
      }, rowTimeNodes);
      if(bounded==null)cursorTimeUpdate=null;
      else {
        const index=bounded.$0;
        let _5=compactOrOriginal(displayTime.Current(), get(latestCursorTimestamps, index));
        cursorTimeUpdate=Some(_5);
      }
      if(bounded!=null&&bounded.$==1){
        const index_1=bounded.$0;
        cursorValueUpdates=mapi((_6, _7) => {
          const o_2=tryItem(_6, latestCursorReaders);
          let _8=o_2==null?null:o_2.$0(index_1);
          return[_7, _8];
        }, valueNodes);
      }
      else cursorValueUpdates=[];
      const ofiUpdates=map((band) => {
        let o_2;
        const rowId=band.getAttribute("data-ta-row-ofi-row-id");
        if(bounded==null)o_2=null;
        else {
          const index_3=bounded.$0;
          const o_3=latestMarkerCursorReaders.TryFind(rowId);
          o_2=o_3==null?null:Some(o_3.$0(index_3));
        }
        let _6=o_2==null?[]:o_2.$0;
        return[band, _6];
      }, ofiBands);
      const resolveCompleted=browserNowMs();
      textWrites=0;
      attributeWrites=0;
      for(let i=0, _6=legendUpdates.length-1;i<=_6;i++){
        const f=get(legendUpdates, i);
        const node=f[0];
        if(setElementTextIfChanged(f[1], node))textWrites=textWrites+1;
        if(setElementAttributeIfChanged("data-value-state", f[2], node))attributeWrites=attributeWrites+1;
      }
      for(let i_1=0, _7=rowTimeUpdates.length-1;i_1<=_7;i_1++){
        const f_1=get(rowTimeUpdates, i_1);
        if(setElementTextIfChanged(f_1[1], f_1[0]))textWrites=textWrites+1;
      }
      if(cursorTimeUpdate!=null&&cursorTimeUpdate.$==1){
        if(time!=null&&time.$==1){
          const nextTime=cursorTimeUpdate.$0;
          const node_1=time.$0;
          if(setElementTextIfChanged(nextTime, node_1))textWrites=textWrites+1;
          if(setElementAttributeIfChanged("data-display-time-zone", id(displayTime.Current()), node_1))attributeWrites=attributeWrites+1;
          if(bounded==null)_3=void 0;
          else {
            const index_2=bounded.$0;
            _3=setElementAttributeIfChanged("data-canonical-event-time", get(latestCursorTimestamps, index_2), node_1)?attributeWrites=attributeWrites+1:void 0;
          }
        }
        else _3=void 0;
      }
      else _3=void 0;
      for(let i_2=0, _8=cursorValueUpdates.length-1;i_2<=_8;i_2++){
        const f_2=get(cursorValueUpdates, i_2);
        const node_2=f_2[0];
        const current=f_2[1];
        if(current==null){
          setElementTextIfChanged("", node_2)?textWrites=textWrites+1:void 0;
          removeElementAttributeIfPresent("data-cursor-row", node_2)?attributeWrites=attributeWrites+1:void 0;
        }
        else {
          const value=current.$0;
          if(setElementTextIfChanged(value.Label+" "+value.Value, node_2))textWrites=textWrites+1;
          if(setElementAttributeIfChanged("data-cursor-row", value.Label, node_2))attributeWrites=attributeWrites+1;
        }
      }
      for(let i_3=0, _9=ofiUpdates.length-1;i_3<=_9;i_3++)((() => {
        let _10, _11;
        const f_4=get(ofiUpdates, i_3);
        const items=f_4[1];
        const band=f_4[0];
        if(setElementAttributeIfChanged("data-display-time-zone", id(displayTime.Current()), band))attributeWrites=attributeWrites+1;
        else null;
        if(setElementAttributeIfChanged("data-marker-event-count", String(length(items)), band))attributeWrites=attributeWrites+1;
        else null;
        const m=tryHead(scopedElements(band, "[data-ta-row-ofi-empty='true']"));
        if(m==null)_10=null;
        else {
          const node_3=m.$0;
          const p=band.getAttribute("data-cursor-event-capability")=="available"?["none", ""]:["unavailable", ""];
          _10=(setElementTextIfChanged(p[1], node_3)?textWrites=textWrites+1:void 0,setElementAttributeIfChanged("data-cursor-event-state", p[0], node_3)?void(attributeWrites=attributeWrites+1):null);
        }
        if(bounded==null)_11=(removeElementAttributeIfPresent("data-cursor-slot", band)?attributeWrites=attributeWrites+1:void 0,removeElementAttributeIfPresent("data-cursor-event-time", band)?void(attributeWrites=attributeWrites+1):null);
        else {
          const index_3=bounded.$0;
          _11=(setElementAttributeIfChanged("data-cursor-slot", String(index_3), band)?attributeWrites=attributeWrites+1:void 0,setElementAttributeIfChanged("data-cursor-event-time", get(latestCursorTimestamps, index_3), band)?void(attributeWrites=attributeWrites+1):null);
        }
        const itemNodes=scopedElements(band, "[data-ta-row-ofi-item-index]");
        for(let i_4=0, _12=itemNodes.length-1;i_4<=_12;i_4++)((() => {
          let o_2;
          const node_6=get(itemNodes, i_4);
          const m_1=(o_2=0,[TryParse(node_6.getAttribute("data-ta-row-ofi-item-index"), {get:() => o_2, set:(v) => {
            o_2=v;
          }}), o_2]);
          const m_2=tryItem(m_1[0]?m_1[1]:-1, items);
          if(m_2==null){
            setElementTextIfChanged("", node_6)?textWrites=textWrites+1:void 0;
            removeElementAttributeIfPresent("title", node_6)?attributeWrites=attributeWrites+1:void 0;
            removeElementAttributeIfPresent("data-marker-id", node_6)?attributeWrites=attributeWrites+1:void 0;
            removeElementAttributeIfPresent("data-marker-event-time", node_6)?attributeWrites=attributeWrites+1:void 0;
            removeElementAttributeIfPresent("data-marker-color", node_6)?attributeWrites=attributeWrites+1:void 0;
            removeElementAttributeIfPresent("data-cursor-event-id", node_6)?attributeWrites=attributeWrites+1:void 0;
            removeElementAttributeIfPresent("data-cursor-event-time", node_6)?attributeWrites=attributeWrites+1:void 0;
            removeElementAttributeIfPresent("data-cursor-event-category", node_6)?attributeWrites=attributeWrites+1:void 0;
            removeElementAttributeIfPresent("data-cursor-event-source-kind", node_6)?attributeWrites=attributeWrites+1:void 0;
            return removeElementAttributeIfPresent("data-cursor-event-color", node_6)?void(attributeWrites=attributeWrites+1):null;
          }
          else {
            const item=m_2.$0;
            if(setElementTextIfChanged(item.Label==item.Category?item.Category:item.Category+": "+item.Label, node_6))textWrites=textWrites+1;
            if(setElementAttributeIfChanged("title", item.Tooltip, node_6))attributeWrites=attributeWrites+1;
            if(setElementAttributeIfChanged("data-marker-id", item.MarkerId, node_6))attributeWrites=attributeWrites+1;
            if(setElementAttributeIfChanged("data-marker-event-time", item.EventTimeUtc, node_6))attributeWrites=attributeWrites+1;
            if(setElementAttributeIfChanged("data-marker-color", item.Color, node_6))attributeWrites=attributeWrites+1;
            if(setElementAttributeIfChanged("data-cursor-event-id", item.MarkerId, node_6))attributeWrites=attributeWrites+1;
            if(setElementAttributeIfChanged("data-cursor-event-time", item.EventTimeUtc, node_6))attributeWrites=attributeWrites+1;
            if(setElementAttributeIfChanged("data-cursor-event-category", item.Category, node_6))attributeWrites=attributeWrites+1;
            if(setElementAttributeIfChanged("data-cursor-event-source-kind", item.SourceKind, node_6))attributeWrites=attributeWrites+1;
            return setElementAttributeIfChanged("data-cursor-event-color", item.Color, node_6)?void(attributeWrites=attributeWrites+1):null;
          }
        })());
        const overflowNode=tryHead(scopedElements(band, "[data-ta-row-ofi-overflow='true']"));
        if(overflowNode==null)return null;
        else {
          overflowNode.$0;
          if(length(items)>4){
            const node_4=overflowNode.$0;
            const overflow=skip(4, items);
            if(setElementTextIfChanged("+"+String(length(overflow)), node_4))textWrites=textWrites+1;
            return setElementAttributeIfChanged("title", concat_1("\n---\n", map((a) => a.Tooltip, overflow.slice(0, 8))), node_4)?void(attributeWrites=attributeWrites+1):null;
          }
          else {
            const node_5=overflowNode.$0;
            if(setElementTextIfChanged("", node_5))textWrites=textWrites+1;
            return removeElementAttributeIfPresent("title", node_5)?void(attributeWrites=attributeWrites+1):null;
          }
        }
      })());
      const writeCompleted=browserNowMs();
      visibilityWrites=0;
      if(bounded!=null&&bounded.$==1){
        if(hint==null){ }
        else setElementHiddenIfChanged(true, hint.$0)?visibilityWrites=visibilityWrites+1:void 0;
        if(time==null){ }
        else setElementHiddenIfChanged(false, time.$0)?visibilityWrites=visibilityWrites+1:void 0;
        for(let i_4=0, _10=cursorValueUpdates.length-1;i_4<=_10;i_4++){
          const f_3=get(cursorValueUpdates, i_4);
          if(setElementHiddenIfChanged(f_3[1]==null, f_3[0]))visibilityWrites=visibilityWrites+1;
        }
        _4=void 0;
      }
      else {
        if(hint==null){ }
        else setElementHiddenIfChanged(false, hint.$0)?visibilityWrites=visibilityWrites+1:void 0;
        if(time==null){ }
        else setElementHiddenIfChanged(true, time.$0)?visibilityWrites=visibilityWrites+1:void 0;
        for(let i_5=0, _11=valueNodes.length-1;i_5<=_11;i_5++)if(setElementHiddenIfChanged(true, get(valueNodes, i_5)))visibilityWrites=visibilityWrites+1;
        _4=void 0;
      }
      for(let i_6=0, _12=ofiUpdates.length-1;i_6<=_12;i_6++)((() => {
        const f_4=get(ofiUpdates, i_6);
        const items=f_4[1];
        const band=f_4[0];
        const m=tryHead(scopedElements(band, "[data-ta-row-ofi-empty='true']"));
        if(m==null)null;
        else setElementHiddenIfChanged(length(items)>0, m.$0)?void(visibilityWrites=visibilityWrites+1):null;
        const itemNodes=scopedElements(band, "[data-ta-row-ofi-item-index]");
        for(let i_7=0, _13=itemNodes.length-1;i_7<=_13;i_7++)((() => {
          let o_2;
          const node_3=get(itemNodes, i_7);
          const m_2=(o_2=0,[TryParse(node_3.getAttribute("data-ta-row-ofi-item-index"), {get:() => o_2, set:(v) => {
            o_2=v;
          }}), o_2]);
          const index_3=m_2[0]?m_2[1]:-1;
          return setElementHiddenIfChanged(index_3<0||index_3>=length(items), node_3)?void(visibilityWrites=visibilityWrites+1):null;
        })());
        const m_1=tryHead(scopedElements(band, "[data-ta-row-ofi-overflow='true']"));
        return m_1==null?null:setElementHiddenIfChanged(length(items)<=4, m_1.$0)?void(visibilityWrites=visibilityWrites+1):null;
      })());
      const visibilityCompleted=browserNowMs();
      const totalMs=visibilityCompleted-totalStarted;
      visibleValueTelemetrySequence=visibleValueTelemetrySequence+1;
      visibleValueTelemetryMaxTotalMs=Compare(visibleValueTelemetryMaxTotalMs, totalMs)===1?visibleValueTelemetryMaxTotalMs:totalMs;
      setElementAttributeIfChanged("data-visible-value-telemetry-sequence", String(visibleValueTelemetrySequence), chartStackElement);
      setElementAttributeIfChanged("data-visible-value-query-ms", fixedText(queryCompleted-totalStarted), chartStackElement);
      setElementAttributeIfChanged("data-visible-value-resolve-ms", fixedText(resolveCompleted-queryCompleted), chartStackElement);
      setElementAttributeIfChanged("data-visible-value-write-ms", fixedText(writeCompleted-resolveCompleted), chartStackElement);
      setElementAttributeIfChanged("data-visible-value-visibility-ms", fixedText(visibilityCompleted-writeCompleted), chartStackElement);
      setElementAttributeIfChanged("data-visible-value-total-ms", fixedText(totalMs), chartStackElement);
      setElementAttributeIfChanged("data-visible-value-max-total-ms", fixedText(visibleValueTelemetryMaxTotalMs), chartStackElement);
      setElementAttributeIfChanged("data-visible-value-text-writes", String(textWrites), chartStackElement);
      setElementAttributeIfChanged("data-visible-value-attribute-writes", String(attributeWrites), chartStackElement);
      setElementAttributeIfChanged("data-visible-value-visibility-writes", String(visibilityWrites), chartStackElement);
      setElementAttributeIfChanged("data-visible-value-node-count", String(length(valueNodes)+length(legendValueNodes)+length(rowTimeNodes)+length(ofiBands)*(4+1)+(c=hint!=null,Hash(c))+(c_1=time!=null,Hash(c_1))), chartStackElement);
      setElementAttributeIfChanged("data-visible-value-publication-ms", fixedText(browserNowMs()-visibilityCompleted), chartStackElement);
    }
  };
  refreshVisibleValues=() => {
    applyVisibleCursorValues(displayedCursorIndex);
  };
  const applyCursorIndex=(value) => {
    if(!(chartStackElement==null)){
      let bounded, _3;
      if(value==null)bounded=null;
      else {
        const index=value.$0;
        if(length(latestCursorTimestamps)===0)bounded=null;
        else {
          const a=0;
          const b=length(latestCursorTimestamps)-1;
          const b_1=Compare(index, b)===-1?index:b;
          let _4=Compare(a, b_1)===1?a:b_1;
          bounded=Some(_4);
        }
      }
      displayedCursorIndex=bounded;
      let _5=chartStackElement;
      const o_2=bounded==null?null:Some(String(bounded.$0));
      let _6=o_2==null?"":o_2.$0;
      _5.setAttribute("data-cursor-index", _6);
      const crosshairs=cursorElements("[data-ta-shared-crosshair='true']");
      const rowCursorLabels=cursorElements("[data-ta-row-cursor-label='true']");
      const m=cursorPosition(1000, length(latestCursorTimestamps), bounded);
      if(m==null){
        for(let i=0, _9=crosshairs.length-1;i<=_9;i++)get(crosshairs, i).setAttribute("visibility", "hidden");
        for(let i_1=0, _10=rowCursorLabels.length-1;i_1<=_10;i_1++)get(rowCursorLabels, i_1).setAttribute("style", rowCursorTagStyle("50", false));
        _3=void 0;
      }
      else {
        const x=m.$0;
        const xText=fixedText(x);
        for(let i_2=0, _11=crosshairs.length-1;i_2<=_11;i_2++){
          const line=get(crosshairs, i_2);
          line.setAttribute("x1", xText);
          line.setAttribute("x2", xText);
          line.setAttribute("visibility", "visible");
        }
        const a_1=48;
        const a_2=952;
        const b_2=Compare(a_2, x)===-1?a_2:x;
        let _7=Compare(a_1, b_2)===1?a_1:b_2;
        let _8=_7/10;
        const labelLeftPercent=fixedText(_8);
        for(let i_3=0, _12=rowCursorLabels.length-1;i_3<=_12;i_3++)((() => {
          let presentation, o_3;
          const group=get(rowCursorLabels, i_3);
          const rowId=group.getAttribute("data-ta-row-cursor-row-id");
          if(bounded==null)presentation=null;
          else {
            const c=bounded.$0;
            presentation=tryRowPresentation(latestLegendReaders, rowId, c);
          }
          if(presentation==null)o_3=null;
          else {
            const value_1=presentation.$0;
            let _13=dateAndClockOrUnavailable(displayTime.Current(), value_1.Timestamp);
            o_3=Some(_13);
          }
          const p=o_3==null?["Unavailable", "Unavailable"]:o_3.$0;
          const o_4=presentation==null?null:Some(presentation.$0.Timestamp);
          const canonicalEventTime=o_4==null?"":o_4.$0;
          group.setAttribute("style", rowCursorTagStyle(labelLeftPercent, true));
          group.setAttribute("data-display-time-zone", id(displayTime.Current()));
          group.setAttribute("data-canonical-event-time", canonicalEventTime);
          const dateNode=group.querySelector("[data-ta-row-cursor-date='true']");
          const timeNode=group.querySelector("[data-ta-row-cursor-clock='true']");
          if(!(dateNode==null))dateNode.textContent=p[0];
          return!(timeNode==null)?void(timeNode.textContent=p[1]):null;
        })());
        _3=void 0;
      }
      applyVisibleCursorValues(bounded);
    }
  };
  refreshDisplayTime=() => {
    applyCursorIndex(displayedCursorIndex);
  };
  const setCursorIndex=(value) => {
    pendingCursorIndex=Some(value);
    pendingCursorRequestedAtMs=browserNowMs();
    !cursorFrameScheduled?(cursorFrameScheduled=true,requestAnimationFrame(() => {
      cursorFrameScheduled=false;
      if(pendingCursorIndex==null){ }
      else {
        const value_1=pendingCursorIndex.$0;
        pendingCursorIndex=null;
        applyCursorIndex(value_1);
        cursorRenderLatencySequence=cursorRenderLatencySequence+1;
        if(!(chartStackElement==null)){
          chartStackElement.setAttribute("data-cursor-render-latency-sequence", String(cursorRenderLatencySequence));
          let _3=chartStackElement;
          const a=0;
          const b=browserNowMs()-pendingCursorRequestedAtMs;
          let _4=Compare(a, b)===1?a:b;
          let _5=fixedText(_4);
          _3.setAttribute("data-cursor-render-latency-ms", _5);
        }
      }
    })):void 0;
  };
  const scheduleCursorGeometryRefresh=() => {
    requestAnimationFrame(() => {
      setCursorIndex(displayedCursorIndex);
    });
  };
  const commitCursorIndex=(index) => {
    setCursorIndex(Some(index));
    if(!Equals(cursorIndex.Get(), Some(index)))cursorIndex.Set(Some(index));
    if(actionAllowed("shared-cursor-changed")&&!commandsDisabledNow()){
      const m=runtimeState.Get().Document;
      if(m==null){ }
      else {
        const document=m.$0;
        const timeline=referenceTimelineForDocumentPrepared(document, latestPreparedData);
        const visible=selectWindow(resolvedWindow(uiState.Get()), timeline);
        const m_1=document.BaseRowId;
        if(m_1!=null&&m_1.$==1){
          m_1.$0;
          if(index>=0&&index<length(visible)){
            const baseRowId=m_1.$0;
            startAction({
              $:7, 
              $0:currentCanvasId(), 
              $1:{BaseRowId:baseRowId, EventTimeUtc:get(visible, index)}
            }, "Shared cursor synchronized.", () => { });
          }
        }
      }
    }
  };
  const resetEditorFor=(templateKey) => {
    selectedTemplate.Set(templateKey);
    const o_2=tryFind((schema) => schema.TemplateKey==templateKey, editorSchemasNow());
    const o_3=o_2==null?null:Some(initialEditorInputs(o_2.$0));
    let _3=o_3==null?[]:o_3.$0;
    editorValues.Set(_3);
  };
  const forceCloseRowEditor=() => {
    editingRowId=null;
    pendingEditorMutation=null;
    const _3=uiState.Get();
    let _4={
      Window:_3.Window, 
      FollowLatest:_3.FollowLatest, 
      HiddenRows:_3.HiddenRows, 
      HiddenTraces:_3.HiddenTraces, 
      RemovedTraces:_3.RemovedTraces, 
      AddRowOpen:false, 
      CursorIndex:_3.CursorIndex, 
      PendingActionId:_3.PendingActionId, 
      Feedback:_3.Feedback
    };
    setUiState(_4);
  };
  const closeRowEditor=() => {
    if(uiState.Get().PendingActionId==null)forceCloseRowEditor();
  };
  const editorTestId=(path) =>"ta-editor-"+Replace(Replace(Replace(path, ".", "-"), "[", "-"), "]", "");
  const setEditorScalar=(path, value) => editorValues.Set(setEditorInput({Path:path, Value:value}, editorValues.Get()));
  const removeEditorScalar=(path) => {
    editorValues.Set(filter_1((current) => current.Path!=path, editorValues.Get()));
  };
  const scalarText=(path) => {
    const o_2=tryEditorInput(path, editorValues.Get());
    const o_3=o_2==null?null:Some(editorScalarText_1(o_2.$0));
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
              HiddenTraces:_4.HiddenTraces, 
              RemovedTraces:_4.RemovedTraces, 
              AddRowOpen:_4.AddRowOpen, 
              CursorIndex:_4.CursorIndex, 
              PendingActionId:_4.PendingActionId, 
              Feedback:String(labelText)+" allows at most "+String(_3)+" item(s)."
            };
            setUiState(_5);
          }
          else editorValues.Set(addListItem(path, itemKind, editorValues.Get()));
        })]);
      }
      else {
        const caption=required?labelText+" *":labelText;
        const shell_1=(control_1) => Doc.Element("label", [Attr.Create("style", "display:flex; flex-direction:column; gap:3px; min-width:0; font-size:10px; color:#60738b;")], [Doc.TextNode(caption), control_1]);
        if(kind.$==0)return shell_1(inputText(editorTestId(path), labelText, scalarText(path), (value) => {
          setEditorScalar(path, {$:0, $0:value});
        }));
        else if(kind.$==1){
          const minimum=kind.$0;
          const maximum_1=kind.$1;
          return shell_1(element_1("input", ofSeq_1(delay(() => append_2([Attr.Create("data-testid", editorTestId(path))], delay(() => append_2([Attr.Create("type", "number")], delay(() => append_2([Attr.Create("value", scalarText(path))], delay(() => append_2([Attr.Create("step", "1")], delay(() => append_2([Attr.Create("style", "height:30px; min-width:0; width:100%; border:1px solid #b9c6d8; border-radius:4px; background:#fff; color:#142033; padding:4px 7px; box-sizing:border-box; font-size:12px;")], delay(() => append_2([OnAfterRender((node) => {
            const input_1=node;
            input_1.addEventListener("input", () => {
              let o_4;
              const m_1=(o_4=0n,[TryParse_1(input_1.value, {get:() => o_4, set:(v) => {
                o_4=v;
              }}), o_4]);
              return m_1[0]?setEditorScalar(path, {$:1, $0:Number(m_1[1])}):removeEditorScalar(path);
            });
          })], delay(() => append_2(minimum==null?[EmptyAttr()]:[Attr.Create("min", String(minimum.$0))], delay(() => maximum_1==null?[EmptyAttr()]:[Attr.Create("max", String(maximum_1.$0))])))))))))))))))), []));
        }
        else if(kind.$==2){
          const minimum_1=kind.$0;
          const maximum_2=kind.$1;
          return shell_1(element_1("input", ofSeq_1(delay(() => append_2([Attr.Create("data-testid", editorTestId(path))], delay(() => append_2([Attr.Create("type", "number")], delay(() => append_2([Attr.Create("value", scalarText(path))], delay(() => append_2([Attr.Create("step", "any")], delay(() => append_2([Attr.Create("style", "height:30px; min-width:0; width:100%; border:1px solid #b9c6d8; border-radius:4px; background:#fff; color:#142033; padding:4px 7px; box-sizing:border-box; font-size:12px;")], delay(() => append_2([OnAfterRender((node) => {
            const input_1=node;
            input_1.addEventListener("input", () => {
              let o_4;
              o_4=0;
              const _3=Number(input_1.value);
              let _4=isNaN(_3)?false:(o_4=_3,true);
              const m_1=[_4, o_4];
              return m_1[0]?setEditorScalar(path, {$:1, $0:m_1[1]}):removeEditorScalar(path);
            });
          })], delay(() => append_2(minimum_1==null?[EmptyAttr()]:[Attr.Create("min", fixedText(minimum_1.$0))], delay(() => maximum_2==null?[EmptyAttr()]:[Attr.Create("max", fixedText(maximum_2.$0))])))))))))))))))), []));
        }
        else if(kind.$==3){
          const m=tryEditorInput(path, editorValues.Get());
          const isChecked=m!=null&&m.$==1&&(m.$0.$==2&&m.$0.$0);
          return Doc.Element("label", [Attr.Create("style", "display:flex; align-items:center; gap:6px; min-height:30px; font-size:11px; color:#40536d;")], [element_1("input", ofSeq_1(delay(() => append_2([Attr.Create("data-testid", editorTestId(path))], delay(() => append_2([Attr.Create("type", "checkbox")], delay(() => append_2(isChecked?[Attr.Create("checked", "checked")]:[], delay(() =>[OnAfterRender((node) => {
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
          return shell_1(selectInput(editorTestId(path), selectedKey, ofArray(map((choice) =>[choice.Key, choice.Label], choices)), (key_1) => {
            let x;
            const o_4=tryFind((choice) => choice.Key==key_1, choices);
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
  function submitQuery(query){
    return(intentGeneration) => {
      queryInFlight=true;
      return startActionWithFeedback({
        $:6, 
        $0:currentCanvasId(), 
        $1:query
      }, "Query accepted.", () => {
        const m=runtimeState.Get().Document;
        if(m!=null&&m.$==1){
          const currentDocument=m.$0;
          const m_1=queryViewportSelection(querySelectionGeneration, intentGeneration, query, currentDocument, runtimeState.Get().Data);
          return m_1.$==1?(commitLocalWindow(false, m_1.$0),null):m_1.$==2?Some("Query accepted, but the requested range has no loaded observations; the current viewport was preserved."):m_1.$==3?Some(m_1.$0):m_1.$==4?Some("A newer query superseded this response; the current viewport was preserved."):null;
        }
        else return Some("query-viewport-unavailable: the workspace document is not loaded.");
      }, () => { }, () => {
        queryInFlight=false;
        if(queuedQuery==null){ }
        else {
          const nextQuery=queuedQuery.$0[0];
          const nextGeneration=queuedQuery.$0[1];
          queuedQuery=null;
          (submitQuery(nextQuery))(nextGeneration);
        }
      });
    };
  }
  const applyQuery=() => {
    let o_2;
    const m=(o_2=0,[TryParse(intervalDraft, {get:() => o_2, set:(v) => {
      o_2=v;
    }}), o_2]);
    const parsedInterval=m[0]&&m[1]>0?Some(m[1]):null;
    const query={
      SourceId:null, 
      Instrument:IsNullOrWhiteSpace(instrumentDraft)?null:Some(instrumentDraft), 
      IntervalMinutes:parsedInterval, 
      FromUtc:IsNullOrWhiteSpace(fromDateDraft)?null:Some(fromDateDraft), 
      ToUtcExclusive:IsNullOrWhiteSpace(toDateDraft)?null:Some(toDateDraft), 
      IncludePartial:Some(true)
    };
    if(!remoteDisabled(runtimeState.Get().Poll)){
      querySelectionGeneration=querySelectionGeneration+1;
      if(queryInFlight){
        queuedQuery=Some([query, querySelectionGeneration]);
        const _3=uiState.Get();
        let _4={
          Window:_3.Window, 
          FollowLatest:_3.FollowLatest, 
          HiddenRows:_3.HiddenRows, 
          HiddenTraces:_3.HiddenTraces, 
          RemovedTraces:_3.RemovedTraces, 
          AddRowOpen:_3.AddRowOpen, 
          CursorIndex:_3.CursorIndex, 
          PendingActionId:_3.PendingActionId, 
          Feedback:"A newer query is queued and will supersede the pending viewport selection."
        };
        setUiState(_4);
      }
      else if(uiState.Get().PendingActionId!=null){
        const _5=uiState.Get();
        let _6={
          Window:_5.Window, 
          FollowLatest:_5.FollowLatest, 
          HiddenRows:_5.HiddenRows, 
          HiddenTraces:_5.HiddenTraces, 
          RemovedTraces:_5.RemovedTraces, 
          AddRowOpen:_5.AddRowOpen, 
          CursorIndex:_5.CursorIndex, 
          PendingActionId:_5.PendingActionId, 
          Feedback:"action-in-flight: wait for the pending action result."
        };
        setUiState(_6);
      }
      else(submitQuery(query))(querySelectionGeneration);
    }
  };
  const addRow=() => {
    let optionsResult;
    const m=tryFind((schema_1) => schema_1.TemplateKey==selectedTemplate.Get(), editorSchemasNow());
    if(m!=null&&m.$==1){
      const schema=m.$0;
      const errors_1=validateEditorSubmission(schema, editorValues.Get());
      if(length(errors_1)>0){
        const _3=uiState.Get();
        let _4={
          Window:_3.Window, 
          FollowLatest:_3.FollowLatest, 
          HiddenRows:_3.HiddenRows, 
          HiddenTraces:_3.HiddenTraces, 
          RemovedTraces:_3.RemovedTraces, 
          AddRowOpen:_3.AddRowOpen, 
          CursorIndex:_3.CursorIndex, 
          PendingActionId:_3.PendingActionId, 
          Feedback:concat_1(" ", errors_1)
        };
        setUiState(_4);
      }
      else {
        const o_2=runtimeState.Get().Document;
        const o_3=o_2==null?null:Some(o_2.$0.Rows);
        const currentRows=o_3==null?[]:o_3.$0;
        const binding={TemplateKey:schema.TemplateKey, Values:editorValues.Get().slice()};
        pendingEditorMutation=Some([runtimeState.Get().Identity, runtimeState.Get().DocumentRevision, editingRowId, new FSharpSet("New_2", OfSeq(map((a) => a.RowId, currentRows))), binding]);
        startActionWith({
          $:3, 
          $0:currentCanvasId(), 
          $1:editingRowId, 
          $2:schema.TemplateKey, 
          $3:editorValues.Get()
        }, schema.DisplayName+" accepted; awaiting authoritative document.", () => { }, () => {
          pendingEditorMutation=null;
        });
      }
    }
    else {
      const m_1=addKind.Get();
      const kind=m_1=="Volume"?{$:1}:m_1=="Dmi"?{$:3}:m_1=="Adx"?{$:4}:m_1=="Macd"?{$:5}:m_1=="HeikinAshi"?{$:6}:{$:2};
      const positive=(fieldName, textValue) => {
        let o_4;
        const m_2=(o_4=0,[TryParse(textValue, {get:() => o_4, set:(v) => {
          o_4=v;
        }}), o_4]);
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
          $0:currentCanvasId(), 
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
          HiddenTraces:_12.HiddenTraces, 
          RemovedTraces:_12.RemovedTraces, 
          AddRowOpen:_12.AddRowOpen, 
          CursorIndex:_12.CursorIndex, 
          PendingActionId:_12.PendingActionId, 
          Feedback:message
        };
        setUiState(_13);
      }
    }
  };
  return Doc.Element("div", [Attr.Create("class", "ptcs-ta-workspace"), Attr.Create("data-testid", "ta-workspace"), Dynamic_1("data-display-time-zone", Map(id, displayTime.Zone)), Attr.Create("style", "display:flex; flex-direction:column; min-width:0; width:100%; min-height:640px; color:#142033; background:#f4f7fb; font-family:Segoe UI, Arial, sans-serif; letter-spacing:0;")], [Doc.EmbedView(MapCachedBy(sameDocumentShell, (state) => {
    let _3, _4, _5, _6, _7;
    const m=state.Document;
    if(m!=null&&m.$==1){
      const document=m.$0;
      const currentPlotPalette=plotPalette(document.DefaultView);
      const currentDocumentKey=[state.Identity, state.DocumentRevision];
      if(!Equals(synchronizedDocumentKey, Some(currentDocumentKey))){
        const query=queryDraft(document.DefaultView);
        instrumentDraft=query.Instrument;
        intervalDraft=query.IntervalMinutes;
        fromDateDraft=query.FromUtc;
        toDateDraft=query.ToUtcExclusive;
        const currentSchemas=editorSchemasNow();
        if(!exists((schema) => schema.TemplateKey==selectedTemplate.Get(), currentSchemas)){
          const m_1=tryHead(currentSchemas);
          _3=m_1==null?closeRowEditor():resetEditorFor(m_1.$0.TemplateKey);
        }
        else _3=null;
        if(pendingAddRowId!=null&&pendingAddRowId.$==1){
          const rowId=pendingAddRowId.$0;
          if(exists((row) => row.RowId==rowId, document.Rows)){
            pendingAddRowId.$0;
            pendingAddRowId=null;
            const _8=uiState.Get();
            let _9={
              Window:_8.Window, 
              FollowLatest:_8.FollowLatest, 
              HiddenRows:_8.HiddenRows, 
              HiddenTraces:_8.HiddenTraces, 
              RemovedTraces:_8.RemovedTraces, 
              AddRowOpen:false, 
              CursorIndex:_8.CursorIndex, 
              PendingActionId:_8.PendingActionId, 
              Feedback:"Row added."
            };
            _4=setUiState(_9);
          }
          else _4=null;
        }
        else _4=null;
        const bindingOf=(row) => {
          const o_4=ToOption(tryFind_1(row));
          return o_4==null?null:o_4.$0;
        };
        if(pendingEditorMutation!=null&&pendingEditorMutation.$==1){
          const targetRowId=pendingEditorMutation.$0[2];
          const priorRowIds=pendingEditorMutation.$0[3];
          const expectedBinding=pendingEditorMutation.$0[4];
          const baseRevision=pendingEditorMutation.$0[1];
          let _10=pendingEditorMutation.$0[0];
          if(targetRowId==null)_5=exists((row) =>!priorRowIds.Contains(row.RowId)&&Equals(bindingOf(row), Some(expectedBinding)), document.Rows);
          else {
            const rowId_1=targetRowId.$0;
            const o_2=tryFind((row) => row.RowId==rowId_1, document.Rows);
            const o_3=o_2==null?null:bindingOf(o_2.$0);
            _5=o_3==null?false:Equals(expectedBinding, o_3.$0);
          }
          if(authoritativeDocumentAdvanced(_10, baseRevision, state.Identity, state.DocumentRevision, _5)){
            forceCloseRowEditor();
            const _11=uiState.Get();
            let _12={
              Window:_11.Window, 
              FollowLatest:_11.FollowLatest, 
              HiddenRows:_11.HiddenRows, 
              HiddenTraces:_11.HiddenTraces, 
              RemovedTraces:_11.RemovedTraces, 
              AddRowOpen:_11.AddRowOpen, 
              CursorIndex:_11.CursorIndex, 
              PendingActionId:_11.PendingActionId, 
              Feedback:targetRowId!=null?"Row updated.":"Row added."
            };
            _6=setUiState(_12);
          }
          else _6=null;
        }
        else _6=null;
        _7=void(synchronizedDocumentKey=Some(currentDocumentKey));
      }
      else _7=null;
      let _13=[Attr.Create("style", "display:flex; flex-direction:column; min-width:0;")];
      let _14=Doc.Element("header", [Attr.Create("style", "display:flex; flex-direction:column; gap:7px; padding:10px 12px 8px; background:#fff; border-bottom:1px solid #dbe3ee;")], [Doc.Element("div", [Attr.Create("style", "display:flex; align-items:center; justify-content:space-between; gap:10px; flex-wrap:wrap;")], [Doc.Element("div", [Attr.Create("style", "min-width:0;")], [Doc.Element("h2", [Attr.Create("data-testid", "ta-workspace-title"), Attr.Create("style", "margin:0; font-size:17px; line-height:22px; font-weight:700; color:#152944;")], [Doc.TextNode(document.Title)]), Doc.Element("div", [Attr.Create("data-testid", "ta-canvas-identity"), Attr.Create("style", "font-size:11px; color:#667891; white-space:nowrap; overflow:hidden; text-overflow:ellipsis;")], [Doc.TextView(Map((current) =>"canvas "+canvasIdText(current.Identity.CanvasInstanceId)+" / revision "+String(current.DataRevision), runtimeState.View))])]), Doc.EmbedView(Map((_17) => {
        const current=_17[0];
        const status=statusPresentation(document.StatusRef, current);
        return Doc.Element("div", [Attr.Create("style", "display:flex; align-items:center; gap:5px; flex-wrap:wrap; justify-content:flex-end;")], [Doc.Element("div", [Attr.Create("data-testid", "ta-freshness"), Attr.Create("data-freshness", freshnessClass(status.Freshness)), Attr.Create("style", "border:1px solid #9fb0c6; border-radius:4px; padding:3px 7px; font-size:11px; font-weight:650; color:#27415f; background:#f8fafc;")], [Doc.TextNode(status.Label)]), Doc.Element("div", [Attr.Create("data-testid", "ta-poll-state"), Attr.Create("data-poll-state", pollText(current.Poll)), Attr.Create("style", "border:1px solid #c3cfdd; border-radius:4px; padding:3px 7px; font-size:10px; color:#53667d; background:#fff;")], [Doc.TextNode(pollText(current.Poll))])]);
      }, Map2((_17, _18) =>[_17, _18], runtimeState.View, displayTime.Zone)))]), Doc.EmbedView(Map((_17) => {
        const zone=_17[1];
        const status=statusPresentation(document.StatusRef, _17[0]);
        return Doc.Element("div", [Attr.Create("data-testid", "ta-status-detail"), Attr.Create("data-display-time-zone", id(zone)), Attr.Create("style", "display:flex; gap:10px; flex-wrap:wrap; min-height:16px; font-size:10px; color:#60738b;")], ofSeq_1(delay(() => {
          let _18;
          const m_2=status.Watermark;
          if(m_2==null)_18=[];
          else {
            const value=m_2.$0;
            _18=[Doc.Element("span", [Attr.Create("data-canonical-event-time", value)], [Doc.TextNode("watermark "+fullOrOriginal(zone, value))])];
          }
          return append_2(_18, delay(() => {
            const m_3=status.Quality;
            let _19=m_3==null?[]:[Doc.Element("span", [], [Doc.TextNode("quality "+m_3.$0)])];
            return append_2(_19, delay(() => {
              const m_4=status.Error;
              if(m_4==null)return[];
              else {
                const value_1=m_4.$0;
                return[Doc.Element("span", [Attr.Create("data-testid", "ta-last-good-error"), Attr.Create("style", "color:#a33b43; font-weight:600;")], [Doc.TextNode(value_1)])];
              }
            }));
          }));
        })));
      }, Map2((_17, _18) =>[_17, _18], runtimeState.View, displayTime.Zone))), Doc.Element("div", [Attr.Create("data-testid", "ta-query-toolbar"), Attr.Create("style", "display:grid; grid-template-columns:repeat(auto-fit,minmax(120px,1fr)); gap:6px; align-items:end;")], [Doc.Element("label", [Attr.Create("style", "display:flex; flex-direction:column; gap:2px; min-width:0; font-size:10px; color:#60738b;")], [Doc.TextNode("Instrument"), inputText("ta-instrument", "Instrument", instrumentDraft, (value) => {
        instrumentDraft=value;
      })]), Doc.Element("label", [Attr.Create("style", "display:flex; flex-direction:column; gap:2px; min-width:0; font-size:10px; color:#60738b;")], [Doc.TextNode("Interval"), selectInput("ta-interval", intervalDraft, ofArray([["1", "1m"], ["5", "5m"], ["30", "30m"], ["60", "60m"], ["930", "Session"]]), (value) => {
        intervalDraft=value;
      })]), Doc.Element("label", [Attr.Create("style", "display:flex; flex-direction:column; gap:2px; min-width:0; font-size:10px; color:#60738b;")], [Doc.TextNode("From"), inputText("ta-from", "YYYY-MM-DD", fromDateDraft, (value) => {
        fromDateDraft=value;
      })]), Doc.Element("label", [Attr.Create("style", "display:flex; flex-direction:column; gap:2px; min-width:0; font-size:10px; color:#60738b;")], [Doc.TextNode("To"), inputText("ta-to", "YYYY-MM-DD", toDateDraft, (value) => {
        toDateDraft=value;
      })]), primaryButtonView("ta-apply-query", "Load / Apply", commandsDisabledView, commandsDisabledNow, applyQuery)]), Doc.Element("div", [Attr.Create("data-testid", "ta-local-toolbar"), Attr.Create("style", "display:flex; align-items:center; gap:5px; flex-wrap:wrap;")], append_1(ofArray([compactRemoteButton("ta-pan-left", "\u2190", "Pan earlier", viewportCommandsDisabledView, viewportCommandsDisabledNow, () => {
        const a=1;
        const b=resolvedWindow(uiState.Get()).Count;
        let _17=-(Compare(a, b)===1?a:b);
        panWindow(_17);
      }), compactRemoteButton("ta-pan-right", "\u2192", "Pan later", viewportCommandsDisabledView, viewportCommandsDisabledNow, () => {
        const a=1;
        const b=resolvedWindow(uiState.Get()).Count;
        let _17=Compare(a, b)===1?a:b;
        panWindow(_17);
      }), compactRemoteButton("ta-zoom-in", "+", "Show fewer bars", viewportCommandsDisabledView, viewportCommandsDisabledNow, () => {
        zoomWindow(-8);
      }), compactRemoteButton("ta-zoom-out", "\u2212", "Show more bars", viewportCommandsDisabledView, viewportCommandsDisabledNow, () => {
        zoomWindow(8);
      }), compactRemoteButton("ta-reset-view", "Reset View", "Reset local viewport to the latest bars", viewportCommandsDisabledView, viewportCommandsDisabledNow, resetWindow), compactRemoteButton("ta-reset-canvas", "Reset Canvas", "Request server canvas reset", commandsDisabledView, commandsDisabledNow, () => {
        startAction({$:1, $0:currentCanvasId()}, "Canvas reset accepted.", () => {
          const _17=uiState.Get();
          let _18={
            Window:_17.Window, 
            FollowLatest:_17.FollowLatest, 
            HiddenRows:new FSharpSet("New_2", null), 
            HiddenTraces:new FSharpSet("New_2", null), 
            RemovedTraces:new FSharpSet("New_2", null), 
            AddRowOpen:_17.AddRowOpen, 
            CursorIndex:_17.CursorIndex, 
            PendingActionId:_17.PendingActionId, 
            Feedback:_17.Feedback
          };
          setUiState(_18);
        });
      })]), append_1(length(editorSchemasNow())>0?ofArray([compactButton("ta-add-row-toggle", "Add Row", "Open row request editor", () => {
        if(uiState.Get().AddRowOpen)closeRowEditor();
        else {
          editingRowId=null;
          const m_2=tryHead(editorSchemasNow());
          if(m_2==null){ }
          else resetEditorFor(m_2.$0.TemplateKey);
          const _17=uiState.Get();
          let _18={
            Window:_17.Window, 
            FollowLatest:_17.FollowLatest, 
            HiddenRows:_17.HiddenRows, 
            HiddenTraces:_17.HiddenTraces, 
            RemovedTraces:_17.RemovedTraces, 
            AddRowOpen:true, 
            CursorIndex:_17.CursorIndex, 
            PendingActionId:_17.PendingActionId, 
            Feedback:""
          };
          setUiState(_18);
        }
      })]):FSharpList.Empty, ofArray([Doc.Element("span", [Attr.Create("style", "margin-left:auto; color:#60738b; font-size:11px;")], [Doc.TextNode("viewport changes request the selected event-time range when enabled")])])))), Doc.EmbedView(Map((ui) => Doc.Element("div", [Attr.Create("data-testid", "ta-row-toggles"), Attr.Create("style", "display:flex; flex-direction:column; align-items:stretch; gap:4px; width:100%; min-width:0;")], ofSeq_1(delay(() => collect_1((row) => {
        let editable;
        const hidden=ui.HiddenRows.Contains(row.RowId);
        const displayLabel=rowDisplayLabelWithEditor(editorSchemasNow(), row);
        const m_2=tryResolve(editorSchemasNow(), row);
        if(m_2.$==0){
          const _17=m_2.$0;
          editable=_17!=null&&_17.$==1;
        }
        else editable=false;
        const controllableTraces=filter_1((trace) => trace.Visible&&!Equals(trace.Kind, {$:4})&&!Equals(trace.Kind, {$:5})&&!ui.RemovedTraces.Contains([row.RowId, trace.TraceId]), effectiveTraces(row));
        return[Doc.Element("div", [Attr.Create("data-testid", "ta-row-control-line-"+row.RowId), Attr.Create("data-row-id", row.RowId), Attr.Create("style", "display:flex; align-items:center; gap:5px; width:100%; min-width:0; height:40px; max-height:40px; overflow:hidden; box-sizing:border-box;")], ofSeq_1(delay(() => append_2([Doc.Element("div", [Attr.Create("data-testid", "ta-row-controls-"+row.RowId), Attr.Create("style", "display:inline-flex; align-items:stretch; flex:0 1 auto; min-width:0; max-width:100%; height:26px; white-space:nowrap;")], ofSeq_1(delay(() => append_2([Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("data-testid", "ta-toggle-row-"+row.RowId), Attr.Create("aria-pressed", hidden?"false":"true"), Attr.Create("title", displayLabel), Attr.Create("style", hidden?"height:26px; min-width:0; max-width:360px; flex:1 1 auto; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; border:1px solid #c8d2df; border-right:0; border-radius:4px 0 0 4px; background:#fff; color:#7a8798; padding:2px 7px; font-size:11px; cursor:pointer;":"height:26px; min-width:0; max-width:360px; flex:1 1 auto; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; border:1px solid #7da39d; border-right:0; border-radius:4px 0 0 4px; background:#edf8f6; color:#155d55; padding:2px 7px; font-size:11px; cursor:pointer;"), Handler("click", () =>() => {
          const nextHidden=hidden?uiState.Get().HiddenRows.Remove_1(row.RowId):uiState.Get().HiddenRows.Add_1(row.RowId);
          const _18=uiState.Get();
          let _19={
            Window:_18.Window, 
            FollowLatest:_18.FollowLatest, 
            HiddenRows:nextHidden, 
            HiddenTraces:_18.HiddenTraces, 
            RemovedTraces:_18.RemovedTraces, 
            AddRowOpen:_18.AddRowOpen, 
            CursorIndex:_18.CursorIndex, 
            PendingActionId:_18.PendingActionId, 
            Feedback:_18.Feedback
          };
          return setUiState(_19);
        })], [Doc.TextNode(displayLabel)])], delay(() => append_2(editable?[Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("data-testid", "ta-edit-row-"+row.RowId), Attr.Create("title", "Edit "+displayLabel+" parameters"), DynamicBool("disabled", commandsDisabledView), Dynamic_1("style", Map((disabled) => disabled?"width:52px; min-width:52px; height:26px; flex:0 0 52px; border:1px solid #c8d2df; border-right:0; background:#edf1f5; color:#8b98a8; padding:2px 7px; font-size:11px; font-weight:600; cursor:not-allowed;":"width:52px; min-width:52px; height:26px; flex:0 0 52px; border:1px solid #7f9fbe; border-right:0; background:#e8f2ff; color:#174f82; padding:2px 7px; font-size:11px; font-weight:600; cursor:pointer;", commandsDisabledView)), Handler("click", () =>() => {
          if(!commandsDisabledNow()){
            const m_3=tryResolve(editorSchemasNow(), row);
            if(m_3.$==1){
              editingRowId=null;
              const _18=uiState.Get();
              let _19={
                Window:_18.Window, 
                FollowLatest:_18.FollowLatest, 
                HiddenRows:_18.HiddenRows, 
                HiddenTraces:_18.HiddenTraces, 
                RemovedTraces:_18.RemovedTraces, 
                AddRowOpen:false, 
                CursorIndex:_18.CursorIndex, 
                PendingActionId:_18.PendingActionId, 
                Feedback:"This row's editor metadata is invalid; the row remains read-only."
              };
              return setUiState(_19);
            }
            else if(m_3.$0==null)return null;
            else {
              const values=m_3.$0.$0[1];
              const schema=m_3.$0.$0[0];
              editingRowId=Some(row.RowId);
              selectedTemplate.Set(schema.TemplateKey);
              editorValues.Set(values);
              const _20=uiState.Get();
              let _21={
                Window:_20.Window, 
                FollowLatest:_20.FollowLatest, 
                HiddenRows:_20.HiddenRows, 
                HiddenTraces:_20.HiddenTraces, 
                RemovedTraces:_20.RemovedTraces, 
                AddRowOpen:true, 
                CursorIndex:_20.CursorIndex, 
                PendingActionId:_20.PendingActionId, 
                Feedback:""
              };
              return setUiState(_21);
            }
          }
          else return null;
        })], [Doc.TextNode("Edit")])]:[], delay(() =>!actionAllowed("remove-trace")?[Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("data-testid", "ta-remove-row-"+row.RowId), Attr.Create("title", "Remove "+displayLabel+" row"), DynamicBool("disabled", commandsDisabledView), Dynamic_1("style", Map((disabled) => disabled?"width:26px; height:26px; border:1px solid #c8d2df; border-radius:0 4px 4px 0; background:#edf1f5; color:#8b98a8; padding:0; font-size:14px; cursor:not-allowed;":"width:26px; height:26px; border:1px solid #c8a7ab; border-radius:0 4px 4px 0; background:#fff; color:#8d3039; padding:0; font-size:14px; cursor:pointer;", commandsDisabledView)), Handler("click", () =>() =>!commandsDisabledNow()?startAction({
          $:4, 
          $0:currentCanvasId(), 
          $1:row.RowId
        }, displayLabel+" row removal accepted.", () => { }):null)], [Doc.TextNode("×")])]:[])))))))], delay(() => length(controllableTraces)>0?[Doc.Element("div", [Attr.Create("data-testid", "ta-trace-toggles-"+row.RowId), Attr.Create("style", "display:flex; align-items:center; flex:1 1 auto; min-width:0; height:38px; gap:3px; padding:0 0 2px 3px; border-left:1px solid #d7e0eb; box-sizing:border-box; overflow-x:auto; overflow-y:hidden; flex-wrap:nowrap; white-space:nowrap;")], ofSeq_1(delay(() => collect_1((trace) => {
          const traceHidden=ui.HiddenTraces.Contains([row.RowId, trace.TraceId]);
          const traceLabel=IsNullOrWhiteSpace(trace.Label)?trace.TraceId:trace.Label;
          return[Doc.Element("div", [Attr.Create("style", "display:inline-flex; align-items:stretch; flex:0 0 auto; height:24px;")], ofSeq_1(delay(() => append_2([Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("data-testid", "ta-toggle-trace-"+row.RowId+"-"+trace.TraceId), Attr.Create("data-row-id", row.RowId), Attr.Create("data-trace-id", trace.TraceId), Attr.Create("aria-pressed", traceHidden?"false":"true"), Attr.Create("title", (traceHidden?"Show ":"Hide ")+traceLabel+" trace"), Attr.Create("style", traceHidden?"height:24px; border:1px solid #c8d2df; border-radius:4px 0 0 4px; background:#fff; color:#7a8798; padding:2px 7px; font-size:10px; cursor:pointer;":"height:24px; border:1px solid #9cb3cc; border-radius:4px 0 0 4px; background:#f4f8fc; color:#315d88; padding:2px 7px; font-size:10px; cursor:pointer;"), Handler("click", () =>() => {
            const key_1=[row.RowId, trace.TraceId];
            const nextHidden=traceHidden?uiState.Get().HiddenTraces.Remove_1(key_1):uiState.Get().HiddenTraces.Add_1(key_1);
            const _18=uiState.Get();
            let _19={
              Window:_18.Window, 
              FollowLatest:_18.FollowLatest, 
              HiddenRows:_18.HiddenRows, 
              HiddenTraces:nextHidden, 
              RemovedTraces:_18.RemovedTraces, 
              AddRowOpen:_18.AddRowOpen, 
              CursorIndex:_18.CursorIndex, 
              PendingActionId:_18.PendingActionId, 
              Feedback:_18.Feedback
            };
            return setUiState(_19);
          })], [Doc.TextNode(traceLabel)])], delay(() => actionAllowed("remove-trace")?[Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("data-testid", "ta-remove-trace-"+row.RowId+"-"+trace.TraceId), Attr.Create("title", "Remove "+traceLabel+" trace until Reset Canvas"), DynamicBool("disabled", commandsDisabledView), Dynamic_1("style", Map((disabled) => disabled?"width:24px; height:24px; border:1px solid #c8d2df; border-left:0; border-radius:0 4px 4px 0; background:#edf1f5; color:#8b98a8; padding:0; font-size:13px; cursor:not-allowed;":"width:24px; height:24px; border:1px solid #c8a7ab; border-left:0; border-radius:0 4px 4px 0; background:#fff; color:#8d3039; padding:0; font-size:13px; cursor:pointer;", commandsDisabledView)), Handler("click", () =>() => {
            if(!commandsDisabledNow()){
              const key_1=[row.RowId, trace.TraceId];
              return startAction({
                $:5, 
                $0:currentCanvasId(), 
                $1:row.RowId, 
                $2:trace.TraceId
              }, traceLabel+" trace removal accepted.", () => {
                const _18=uiState.Get();
                let _19={
                  Window:_18.Window, 
                  FollowLatest:_18.FollowLatest, 
                  HiddenRows:_18.HiddenRows, 
                  HiddenTraces:uiState.Get().HiddenTraces.Remove_1(key_1), 
                  RemovedTraces:uiState.Get().RemovedTraces.Add_1(key_1), 
                  AddRowOpen:_18.AddRowOpen, 
                  CursorIndex:_18.CursorIndex, 
                  PendingActionId:_18.PendingActionId, 
                  Feedback:_18.Feedback
                };
                setUiState(_19);
              });
            }
            else return null;
          })], [Doc.TextNode("×")])]:[])))))];
        }, controllableTraces))))]:[])))))];
      }, document.Rows)))), uiState.View)), Doc.EmbedView(Map((ui) =>!ui.AddRowOpen?Doc.Empty:Doc.Element("div", [Attr.Create("data-testid", "ta-add-row-editor"), Attr.Create("style", "display:flex; flex-direction:column; gap:7px; padding:7px; border:1px solid #cbd6e5; border-radius:5px; background:#f8fafc;")], [Doc.Element("div", [Attr.Create("data-testid", "ta-generic-row-editor"), Attr.Create("style", "display:flex; flex-direction:column; gap:7px; min-width:0;")], [Doc.Element("label", [Attr.Create("style", "display:flex; flex-direction:column; gap:2px; min-width:0; font-size:10px; color:#60738b;")], [Doc.TextNode("Template"), selectInput("ta-editor-template", selectedTemplate.Get(), ofArray(map((schema) =>[schema.TemplateKey, schema.DisplayName], editorSchemasNow())), resetEditorFor)]), Doc.EmbedView(Map((templateKey) => {
        const m_2=tryFind((schema_1) => schema_1.TemplateKey==templateKey, editorSchemasNow());
        if(m_2!=null&&m_2.$==1){
          const schema=m_2.$0;
          return Doc.Element("div", [Attr.Create("style", "display:grid; grid-template-columns:repeat(auto-fit,minmax(150px,1fr)); gap:7px; min-width:0;")], ofSeq_1(delay(() => map_2((field_1) =>(((editorKind(field_1.Key))(field_1.Label))(field_1.Required))(field_1.Kind), schema.Fields))));
        }
        else return Doc.Element("div", [Attr.Create("style", "font-size:11px; color:#9a2f2f;")], [Doc.TextNode("Template schema is unavailable.")]);
      }, selectedTemplate.View))]), Doc.Element("div", [Attr.Create("style", "display:flex; align-items:center; justify-content:flex-end; gap:6px;")], ofSeq_1(delay(() => {
        let _17;
        if(editingRowId==null)_17=[];
        else {
          const rowId_2=editingRowId.$0;
          _17=[Doc.Element("span", [Attr.Create("data-testid", "ta-row-editor-mode"), Attr.Create("style", "margin-right:auto; font-size:11px; color:#40536d;")], [Doc.TextNode("Editing "+rowId_2)])];
        }
        return append_2(_17, delay(() => append_2([compactButton("ta-add-row-cancel", "Cancel", "Close without submitting", closeRowEditor)], delay(() =>[primaryButtonView("ta-add-row-submit", editingRowId!=null?"Apply":"Add", commandsDisabledView, commandsDisabledNow, addRow)]))));
      })))]), uiState.View)), Doc.EmbedView(Map((ui) => IsNullOrWhiteSpace(ui.Feedback)?Doc.Empty:Doc.Element("div", [Attr.Create("data-testid", "ta-feedback"), Attr.Create("style", "font-size:11px; color:#40536d; min-height:15px;")], [Doc.TextNode(ui.Feedback)]), uiState.View))]);
      const currentViewport=Map2((_17, _18) => {
        const referenceLength_1=referenceTimelineForDocumentPrepared(document, _18).length;
        const maximumVisibleBars=maximumVisibleBarsFor(document);
        const currentWindow=resolveWindow(options.MinimumVisibleBars, maximumVisibleBars, referenceLength_1, _17.FollowLatest, _17.Window);
        const o_4=tryCoverageNavigatorWindowResolved(referenceLength_1, currentWindow, document.DefaultView, _18.RawData);
        const o_5=o_4==null?null:Some((o_4.$0,[o_4.$0[1], o_4.$0[2]]));
        const p=o_5==null?[referenceLength_1, currentWindow]:o_5.$0;
        return[referenceLength_1, maximumVisibleBars, p[0], p[1]];
      }, uiState.View, shellPreparedData.View);
      let _15=Doc.Element("div", [Attr.Create("data-testid", "ta-viewport-panel"), Attr.Create("style", "display:grid; grid-template-columns:minmax(220px,1fr) auto; gap:6px 10px; align-items:center; margin:0 12px; padding:8px; border-bottom:1px solid #d4deea; background:#f8fafc;")], [Doc.Element("span", [Attr.Create("data-testid", "ta-viewport-range"), Attr.Create("style", "font-family:Consolas,monospace; font-size:11px; color:#344a65; white-space:nowrap;")], [Doc.EmbedView(Map2((_17, _18) =>(((_19) => {
        const loadedObservationCount=_19[2];
        const globalCurrentWindow=_19[3];
        return(draft) => {
          const rangeText=(prefix, window_1) =>"Loaded "+String(loadedObservationCount)+" bars · "+String(prefix)+" "+String(window_1.Count===0?0:window_1.StartIndex+1)+"-"+String(window_1.StartIndex+window_1.Count);
          const value=draft!=null&&draft.$==1?rangeText("Preview", draft.$0)+" · release to render":rangeText("Viewing", globalCurrentWindow);
          const o_4=draft==null?null:Some(draft.$0.StartIndex);
          const o_5=o_4==null?null:Some(String(o_4.$0));
          let _20=o_5==null?"":o_5.$0;
          let _21=Attr.Create("data-rendered-preview-start", _20);
          const o_6=draft==null?null:Some(draft.$0.Count);
          const o_7=o_6==null?null:Some(String(o_6.$0));
          let _22=o_7==null?"":o_7.$0;
          let _23=Attr.Create("data-rendered-preview-count", _22);
          let _24=[_21, _23, OnAfterRender(() => {
            renderedNavigatorDraft=draft;
          })];
          return Doc.Element("span", _24, [Doc.TextNode(value)]);
        };
      })(_17))(_18), currentViewport, draftWindow.View))]), Doc.EmbedView(Map((_17) => {
        const maximumVisibleBars=_17[1];
        const loadedObservationCount=_17[2];
        const capped=Compare(loadedObservationCount, maximumVisibleBars)===-1?loadedObservationCount:maximumVisibleBars;
        return Doc.Element("div", [Attr.Create("data-testid", "ta-viewport-presets"), Attr.Create("style", "display:flex; gap:4px; align-items:center;")], [compactButton("ta-jump-loaded-start", "|\u2190", "Jump to the first loaded bars", () => {
          jumpToLoadedCoverageEdge({$:0});
        }), compactButton("ta-view-48", "48", "Show latest 48 bars", () => {
          setWindowCount(48);
        }), compactButton("ta-view-200", "200", "Show latest 200 bars", () => {
          setWindowCount(200);
        }), compactButton("ta-view-all", loadedObservationCount>maximumVisibleBars?"Max "+String(maximumVisibleBars):"All", "Show up to "+String(capped)+" loaded bars", showLoadedCoverage), compactButton("ta-jump-loaded-end", "\u2192|", "Jump to the latest loaded bars", () => {
          jumpToLoadedCoverageEdge({$:1});
        })]);
      }, currentViewport))]);
      let _16=[_14, _15, Doc.EmbedView(Map3((_17, _18, _19) => {
        chartRenderSequence=chartRenderSequence+1;
        chartWorkGeneration=chartWorkGeneration+1;
        dataWorkGeneration=dataWorkGeneration+1;
        const workGeneration=chartWorkGeneration;
        preparedRowsReady.Set(false);
        const projectionCandidateGeneration=pendingGenerationFor(_17, projectionCommitGate);
        chartStackElement=null;
        cursorPanelElement=null;
        const visibleRows=preparedDataReady?filter_1((row) => row.Visible&&!_18.HiddenRows.Contains(row.RowId)&&exists((trace) => trace.Visible&&!Equals(trace.Kind, {$:4})&&!Equals(trace.Kind, {$:5})&&!_18.RemovedTraces.Contains([row.RowId, trace.TraceId]), effectiveTraces(row)), document.Rows):[];
        const cursorReaderCount=sumBy((row) => filter_1((trace) => {
          const key_1=[row.RowId, trace.TraceId];
          return trace.Visible&&!_18.HiddenTraces.Contains(key_1)&&!_18.RemovedTraces.Contains(key_1);
        }, effectiveTraces(row)).length, visibleRows);
        const referenceTimeline=referenceTimelineForDocumentPrepared(document, _19);
        const referenceLength_1=length(referenceTimeline);
        const maximumVisibleBars=maximumVisibleBarsFor(document);
        const visibleWindow=resolveWindow(options.MinimumVisibleBars, maximumVisibleBars, referenceLength_1, _18.FollowLatest, _18.Window);
        const coverageProjection=tryLoadedCoverageResolved(document.DefaultView, _19.RawData);
        const o_4=tryCoverageNavigatorWindowResolved(referenceLength_1, visibleWindow, document.DefaultView, _19.RawData);
        const o_5=o_4==null?null:Some((o_4.$0,[o_4.$0[1], o_4.$0[2]]));
        const p=o_5==null?[referenceLength_1, visibleWindow]:o_5.$0;
        const globalVisibleWindow=p[1];
        const o_6=coverageProjection==null?null:Some(String(coverageProjection.$0.ActiveDetail.StartObservationOrdinal));
        const activeDetailStart=o_6==null?"":o_6.$0;
        const o_7=coverageProjection==null?null:Some(String(coverageProjection.$0.ActiveDetail.ObservationCount));
        const activeDetailCount=o_7==null?"":o_7.$0;
        const visibleTimestamps=selectWindow(visibleWindow, referenceTimeline);
        const rowDataStates=map(() => _c_2.Create_1(_19), visibleRows);
        const rowHeights=map((row) => {
          const traces=filter_1((trace) => {
            const key_2=[row.RowId, trace.TraceId];
            return trace.Visible&&!_18.HiddenTraces.Contains(key_2)&&!_18.RemovedTraces.Contains(key_2);
          }, effectiveTraces(row));
          let o_9;
          const key_1=rowHeightStorageKey(currentCanvasId(), row.RowId);
          const m_2=(o_9=null,[rowHeightStates.TryGetValue(key_1, {get:() => o_9, set:(v) => {
            o_9=v;
          }}), o_9]);
          if(m_2[0])return m_2[1];
          else {
            const value=_c_2.Create_1(rowHeightBounds(row, traces).DefaultHeight);
            rowHeightStates.set_Item(key_1, value);
            return value;
          }
        }, visibleRows);
        const readyRowCount=_c_2.Create_1(0);
        const currentRowLegendElements=create(length(visibleRows), null);
        latestRowLegendElements=currentRowLegendElements;
        const rowDocs=mapi((_40, _41) => {
          const reservedHeight=get(rowHeights, _40).Get()+82;
          return _c_2.Create_1(Doc.Element("div", [Attr.Create("data-testid", "ta-row-loading-"+_41.RowId), Attr.Create("style", "height:"+String(reservedHeight)+"px; min-height:"+String(reservedHeight)+"px; padding:12px; border-top:1px solid #e1e7ef; box-sizing:border-box; color:#718197; background:#fff;")], [Doc.TextNode("Preparing "+rowDisplayLabel(_41)+"...")]));
        }, visibleRows);
        const stagedCursorReaders=create(length(visibleRows), null);
        const stagedLegendReaders=create(length(visibleRows), null);
        const stagedLegendValueReaders=create(length(visibleRows), null);
        const stagedMarkerCursorReaders=create(length(visibleRows), null);
        activeRowDataStates=rowDataStates;
        latestCursorTimestamps=visibleTimestamps;
        latestCursorReaders=[];
        latestLegendReaders=new FSharpMap("New", []);
        latestLegendValueReaders=new FSharpMap("New", []);
        latestMarkerCursorReaders=new FSharpMap("New", []);
        function mountRow(index){
          if(workGeneration===chartWorkGeneration)if(index<length(visibleRows))setTimeout(() => {
            if(workGeneration===chartWorkGeneration){
              const p_1=renderRowReactivePreparedLiveWithHeightPalette(currentPlotPalette, displayTime, _17, _18, get(rowDataStates, index).Get(), get(rowDataStates, index).View, visibleTimestamps, cursorIndex.View, setCursorIndex, commitCursorIndex, true, Equals(document.BaseRowId, Some(get(visibleRows, index).RowId)), get(rowHeights, index), scheduleCursorGeometryRefresh, (node) => {
                set(currentRowLegendElements, index, node);
                scheduleVisibleValueRefresh();
              }, get(visibleRows, index));
              set(stagedCursorReaders, index, Some(p_1[1]));
              set(stagedLegendReaders, index, Some(p_1[2]));
              set(stagedLegendValueReaders, index, Some(p_1[3]));
              set(stagedMarkerCursorReaders, index, Some(p_1[4]));
              get(rowDocs, index).Set(p_1[0]);
              readyRowCount.Set(index+1);
              mountRow(index+1);
            }
          }, 0);
          else {
            latestCursorReaders=collect((x) => x, choose((x) => x, stagedCursorReaders));
            latestLegendReaders=OfArray(choose((x) => x, mapi((_41, _42) => _42==null?null:Some([get(visibleRows, _41).RowId, _42.$0]), stagedLegendReaders)));
            latestLegendValueReaders=OfArray(choose((x) => x, mapi((_41, _42) => _42==null?null:Some([get(visibleRows, _41).RowId, _42.$0]), stagedLegendValueReaders)));
            latestMarkerCursorReaders=OfArray(choose((x) => x, mapi((_41, _42) => _42==null?null:Some([get(visibleRows, _41).RowId, _42.$0]), stagedMarkerCursorReaders)));
            const i=cursorIndex.Get();
            let _40=displayedCursorIndex==null?i:(displayedCursorIndex.$0,displayedCursorIndex);
            applyCursorIndex(_40);
            preparedRowsReady.Set(arePreparedRowsReady(length(visibleRows), readyRowCount.Get()));
            if(projectionCandidateGeneration!=null)completeProjection(projectionCandidateGeneration.$0, _17);
          }
        }
        mountRow(0);
        let _20=Attr.Create("data-testid", "ta-chart-stack");
        let _21=Attr.Create("data-chart-render-sequence", String(chartRenderSequence));
        let _22=Attr.Create("data-chart-preparation-generation", String(preparationGeneration));
        let _23=Attr.Create("data-chart-render-reason", chartRenderReason);
        let _24=Dynamic_1("data-chart-document-revision", Map((current) => String(current.DocumentRevision), runtimeState.View));
        let _25=Dynamic_1("data-chart-data-revision", Map((current) => String(current.DataRevision), runtimeState.View));
        let _26=Dynamic_1("data-chart-transport-sequence", Map((current) => String(current.LastTransportSequence), runtimeState.View));
        let _27=Attr.Create("data-loaded-bars", String(p[0]));
        let _28=Attr.Create("data-active-reference-bars", String(referenceLength_1));
        let _29=Attr.Create("data-local-visible-start", String(visibleWindow.StartIndex));
        let _30=Attr.Create("data-local-visible-count", String(visibleWindow.Count));
        let _31=Attr.Create("data-active-detail-start", activeDetailStart);
        let _32=Attr.Create("data-active-detail-count", activeDetailCount);
        let _33=Attr.Create("data-visible-start", String(globalVisibleWindow.Count===0?0:globalVisibleWindow.StartIndex+1));
        let _34=Attr.Create("data-visible-end", String(globalVisibleWindow.StartIndex+globalVisibleWindow.Count));
        let _35=Attr.Create("data-maximum-visible-bars", String(maximumVisibleBars));
        const o_8=coverageProjection==null?null:Some(coverageProjection.$0.CoverageIdentity);
        let _36=o_8==null?"":o_8.$0;
        let _37=Attr.Create("data-coverage-identity", _36);
        let _38=[_20, _21, _22, _23, _24, _25, _26, _27, _28, _29, _30, _31, _32, _33, _34, _35, _37, Dynamic_1("data-coverage-revision", Map((current) => {
          const o_9=current.Document;
          const o_10=o_9==null?null:tryLoadedCoverageResolved(o_9.$0.DefaultView, current.Data);
          const o_11=o_10==null?null:Some(String(o_10.$0.CoverageRevision));
          return o_11==null?"":o_11.$0;
        }, runtimeState.View)), Dynamic_1("data-query-generation", Map((current) => {
          const o_9=current.Document;
          const o_10=o_9==null?null:tryLoadedCoverageResolved(o_9.$0.DefaultView, current.Data);
          const o_11=o_10==null?null:Some(String(o_10.$0.QueryGeneration));
          return o_11==null?"":o_11.$0;
        }, runtimeState.View)), Attr.Create("data-follow-latest", _18.FollowLatest?"true":"false"), Attr.Create("data-row-count", String(length(visibleRows))), Attr.Create("data-visible-value-query-scope", "cursor-panel+row-legends"), Dynamic_1("data-ready-row-count", Map(String, readyRowCount.View)), Attr.Create("data-cursor-index", ""), Attr.Create("style", "display:flex; flex-direction:column; min-width:0; padding:0 12px 14px;"), OnAfterRender((node) => {
          chartStackElement=node;
          latestCursorTimestamps=visibleTimestamps;
          applyCursorIndex(cursorIndex.Get());
        })];
        let _39=Doc.Element("div", _38, ofSeq_1(delay(() => append_2([Doc.Element("div", [Attr.Create("data-testid", "ta-cursor-panel"), Attr.Create("style", "order:1; display:flex; flex-direction:column; align-items:stretch; border-top:1px solid #dce4ef; background:#f8fafc;"), OnAfterRender((node) => {
          cursorPanelElement=node;
          scheduleVisibleValueRefresh();
        })], ofSeq_1(delay(() => append_2([Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("data-testid", "ta-cross-scale-values-toggle"), Dynamic_1("aria-expanded", Map((expanded) => expanded?"true":"false", crossScaleSummaryOpen.View)), Attr.Create("style", "height:28px; min-height:28px; padding:0 8px; border:0; background:#f8fafc; color:#40536d; font-size:11px; font-weight:650; text-align:left; cursor:pointer;"), Handler("click", () =>() => crossScaleSummaryOpen.Set(!crossScaleSummaryOpen.Get()))], [Doc.TextView(Map((expanded) => expanded?"Hide cross-scale values":"Show cross-scale values", crossScaleSummaryOpen.View))])], delay(() =>[Doc.Element("div", [Attr.Create("data-testid", "ta-cross-scale-values"), Dynamic_1("data-expanded", Map((expanded) => expanded?"true":"false", crossScaleSummaryOpen.View)), Dynamic_1("style", Map((expanded) => expanded?"display:flex; align-items:center; gap:4px 12px; height:30px; min-height:30px; padding:0 8px; overflow-x:auto; overflow-y:hidden; white-space:nowrap; font-family:Consolas,monospace; font-size:11px; line-height:16px; color:#263b55;":"display:none; height:30px; min-height:30px;", crossScaleSummaryOpen.View))], ofSeq_1(delay(() => append_2([Doc.Element("span", [Attr.Create("data-ta-cursor-hint", "true"), Attr.Create("style", "flex:0 0 auto; font-size:11px; color:#718197;")], [Doc.TextNode("Move the pointer over any chart row to inspect one shared bar.")])], delay(() => append_2([Doc.Element("strong", [Attr.Create("data-ta-cursor-time", "true"), Attr.Create("hidden", "hidden"), Attr.Create("style", "flex:0 0 auto; white-space:nowrap;")], [Doc.TextNode("")])], delay(() => map_2((index) => Doc.Element("span", [Attr.Create("data-ta-cursor-value-index", String(index)), Attr.Create("hidden", "hidden"), Attr.Create("style", "flex:0 0 auto; white-space:nowrap;")], [Doc.TextNode("")]), range(0, cursorReaderCount-1)))))))))])))))], delay(() => append_2(length(visibleRows)===0?[Doc.Element("div", [Attr.Create("style", "padding:18px; color:#667891;")], [Doc.TextNode("No visible TA rows.")])]:map_2((rowDoc) => Doc.EmbedView(rowDoc.View), rowDocs), delay(() =>[Doc.Element("div", [Attr.Create("data-testid", "ta-viewport-navigator"), Attr.Create("style", "order:-1; min-width:0; padding:0 8px 8px; border-bottom:1px solid #d4deea; background:#f8fafc;")], [Doc.Element("div", [Attr.Create("style", "min-width:0;")], [Doc.EmbedView(Map((currentPreparedData) => {
          let overviewPoints;
          const currentReferenceTimeline=referenceTimelineForDocumentPrepared(document, currentPreparedData);
          const currentReferenceLength=length(currentReferenceTimeline);
          const currentCoverageProjection=tryLoadedCoverageResolved(document.DefaultView, currentPreparedData.RawData);
          const o_9=currentCoverageProjection==null?null:tryOverviewTimeline(currentCoverageProjection.$0);
          const overviewReferenceTimeline=o_9==null?currentReferenceTimeline:o_9.$0;
          const o_10=currentCoverageProjection==null?null:Some(overviewPointsForCoverage(currentCoverageProjection.$0));
          const projected=o_10==null?[]:o_10.$0;
          if(length(projected)>0)overviewPoints=projected;
          else {
            const o_11=tryFind((trace) => trace.Visible&&Equals(trace.Kind, {$:0}), collect(effectiveTraces, visibleRows));
            const o_12=o_11==null?null:Some(overviewPointsFromCandles(candleSeriesForTracePrepared(o_11.$0, currentPreparedData)));
            overviewPoints=o_12==null?[]:o_12.$0;
          }
          const overviewStripeVisuals_1=overviewStripeVisuals(collect((trace) => overviewStripePlacementsPrepared(trace, currentPreparedData, overviewReferenceTimeline), filter_1((trace) => trace.Visible&&Equals(trace.Kind, {$:5}), collect(effectiveTraces, visibleRows))));
          const selectionWindow=Map2((_40, _41) => {
            let _42;
            const currentVisibleWindow=resolveWindow(options.MinimumVisibleBars, maximumVisibleBarsFor(document), currentReferenceLength, _41.FollowLatest, _41.Window);
            if(_40!=null&&_40.$==1&&(currentCoverageProjection!=null&&currentCoverageProjection.$==1&&(_42=[_40.$0, currentCoverageProjection.$0],true))){
              const domainCount=observationDomainCount(_42[1]);
              return domainCount>0n&&domainCount<=BigInt(2147483647)?selectionRatios(Number((domainCount+2147483648n&4294967295n)-2147483648n), _42[0]):selectionRatios(currentReferenceLength, currentVisibleWindow);
            }
            else {
              const o_13=currentCoverageProjection==null?null:overviewSelectionRatios(currentCoverageProjection.$0, currentReferenceTimeline, currentVisibleWindow);
              if(o_13==null){
                const o_14=currentCoverageProjection==null?null:coverageNavigatorWindow(currentReferenceLength, currentVisibleWindow, currentCoverageProjection.$0);
                const x=o_14==null?null:Some((o_14.$0,selectionRatios(o_14.$0[1], o_14.$0[2])));
                const v=selectionRatios(currentReferenceLength, currentVisibleWindow);
                return x==null?v:x.$0;
              }
              else return o_13.$0;
            }
          }, draftWindow.View, uiState.View);
          return Doc.Element("div", [Attr.Create("data-testid", "ta-overview-with-axis"), Attr.Create("style", "min-width:0;")], [overviewSvgWithPalette(currentPlotPalette, overviewPoints, overviewStripeVisuals_1, length(overviewReferenceTimeline), selectionWindow, (element_2) => {
            if(activeNavigatorCursor==null){
              if(navigatorCursorAfterRender!=null){
                const cursor=navigatorCursorAfterRender.$0;
                navigatorCursorAfterRender=null;
                element_2.style.cursor=cursor;
              }
            }
            else {
              const cursor_1=activeNavigatorCursor.$0;
              element_2.style.cursor=cursor_1;
            }
          }, (navigatorRoot, rawEvent) => {
            let loadedDragDomain, ratios, latestRawDelta, moveHandler, upHandler, cancelHandler, finished, pendingDraft, draftFrameScheduled, documentFallback;
            if(!(navigatorRoot==null)){
              const setDragDiagnostic=(name, value) => {
                navigatorRoot.setAttribute(name, value);
                return!(chartStackElement==null)?chartStackElement.setAttribute(name, value):null;
              };
              setDragDiagnostic("data-drag-handler-invoked", "true");
              setDragDiagnostic("data-drag-mode", "pending");
              setDragDiagnostic("data-drag-last-delta", "0");
              setDragDiagnostic("data-drag-outcome", "started");
              if(!preparedRowsReady.Get()||localViewportDisabled(runtimeState.Get().Poll))return setDragDiagnostic("data-drag-outcome", "disabled");
              else {
                const bounds=navigatorRoot.getBoundingClientRect();
                const localTotal=referenceLength();
                const localCommitted=resolvedWindow(uiState.Get());
                const m_2=runtimeState.Get().Document;
                if(m_2==null)loadedDragDomain=null;
                else {
                  const document_1=m_2.$0;
                  const o_13=tryLoadedCoverageResolved(document_1.DefaultView, runtimeState.Get().Data);
                  if(o_13==null)loadedDragDomain=null;
                  else {
                    const projection=o_13.$0;
                    const o_14=coverageNavigatorWindow(localTotal, localCommitted, projection);
                    loadedDragDomain=o_14==null?null:Some((o_14.$0,[document_1, projection, o_14.$0[1], o_14.$0[2]]));
                  }
                }
                const o_15=loadedDragDomain==null?null:Some((loadedDragDomain.$0,loadedDragDomain.$0,[loadedDragDomain.$0[2], loadedDragDomain.$0[3]]));
                const p_1=o_15==null?[localTotal, localCommitted]:o_15.$0;
                const total=p_1[0];
                const committed=p_1[1];
                const pointerX=rawEvent.clientX-bounds.left;
                const m_3=runtimeState.Get().Document;
                if(m_3!=null&&m_3.$==1){
                  const document_2=m_3.$0;
                  const timeline=referenceTimelineForDocumentPrepared(document_2, latestPreparedData);
                  const m_4=tryLoadedCoverageResolved(document_2.DefaultView, runtimeState.Get().Data);
                  if(m_4==null)ratios=selectionRatios(length(timeline), localCommitted);
                  else {
                    const projection_1=m_4.$0;
                    const o_16=overviewSelectionRatios(projection_1, timeline, localCommitted);
                    if(o_16==null){
                      const o_17=coverageNavigatorWindow(length(timeline), localCommitted, projection_1);
                      const x=o_17==null?null:Some((o_17.$0,selectionRatios(o_17.$0[1], o_17.$0[2])));
                      const v=selectionRatios(length(timeline), localCommitted);
                      ratios=x==null?v:x.$0;
                    }
                    else ratios=o_16.$0;
                  }
                }
                else ratios=selectionRatios(referenceLength(), localCommitted);
                renderedNavigatorDraft=null;
                draftWindow.Set(null);
                setDragDiagnostic("data-drag-domain", loadedDragDomain!=null?"global-loaded":"active-detail");
                setDragDiagnostic("data-drag-domain-count", String(total));
                const m_5=navigatorDragMode(bounds.width, 24, ratios[0], ratios[1], pointerX);
                if(m_5!=null&&m_5.$==1){
                  const drag=m_5.$0;
                  setDragDiagnostic("data-drag-mode", drag);
                  setDragDiagnostic("data-drag-outcome", "tracking");
                  rawEvent.preventDefault();
                  rawEvent.stopPropagation();
                  activeNavigatorCursor=Some("grabbing");
                  navigatorRoot.style.cursor="grabbing";
                  const startClientX=rawEvent.clientX;
                  latestRawDelta=0;
                  moveHandler=null;
                  upHandler=null;
                  cancelHandler=null;
                  finished=false;
                  pendingDraft=null;
                  draftFrameScheduled=false;
                  const pointerId=rawEvent.pointerId;
                  documentFallback=false;
                  const cleanup=() => {
                    let _40;
                    if(documentFallback)_40=(!(moveHandler==null)?globalThis.document.removeEventListener("pointermove", moveHandler):void 0,!(upHandler==null)?globalThis.document.removeEventListener("pointerup", upHandler):void 0,!(cancelHandler==null)?globalThis.document.removeEventListener("pointercancel", cancelHandler):void 0);
                    else {
                      if(!(moveHandler==null))navigatorRoot.removeEventListener("pointermove", moveHandler);
                      if(!(upHandler==null))navigatorRoot.removeEventListener("pointerup", upHandler);
                      if(!(cancelHandler==null))navigatorRoot.removeEventListener("pointercancel", cancelHandler);
                      try {
                        _40=navigatorRoot.releasePointerCapture(pointerId);
                      }
                      catch(m_6){
                        _40=null;
                      }
                    }
                    const restoredCursor=drag=="move"?"grab":"ew-resize";
                    navigatorRoot.style.cursor=restoredCursor;
                    requestAnimationFrame(() => {
                      if(!(chartStackElement==null)){
                        const currentNavigator=chartStackElement.querySelector("[data-testid='ta-overview-navigator']");
                        if(!(currentNavigator==null))currentNavigator.style.cursor=restoredCursor;
                      }
                    });
                  };
                  const finish=() => {
                    if(!finished){
                      let _40;
                      finished=true;
                      finishNavigatorDrag=null;
                      activeNavigatorCursor=null;
                      const draft=releaseNavigatorDraft(committed, loadedDragDomain!=null&&navigatorBoundaryDirection(total, committed, drag, latestRawDelta)!=null, renderedNavigatorDraft, draftWindow.Get(), pendingDraft);
                      const requestedStart=committed.StartIndex+latestRawDelta;
                      setDragDiagnostic("data-drag-committed-start", String(committed.StartIndex));
                      setDragDiagnostic("data-drag-draft-start", String(draft.StartIndex));
                      setDragDiagnostic("data-drag-requested-start", String(requestedStart));
                      if(loadedDragDomain==null){
                        const m_6=navigatorBoundaryDirection(total, committed, drag, latestRawDelta);
                        if(m_6==null){
                          const p_2=commitWindowBounds(options.MinimumVisibleBars, maximumVisibleBarsNow(), total, draft);
                          const next=p_2[1];
                          const followLatest=p_2[0];
                          _40=!Equals(next, committed)||followLatest!=uiState.Get().FollowLatest?(setDragDiagnostic("data-drag-outcome", "commit-local"),navigatorCursorAfterRender=Some(drag=="move"?"grab":"ew-resize"),setWindow(followLatest, next)):(setDragDiagnostic("data-drag-outcome", "no-change"),draftWindow.Set(null));
                        }
                        else {
                          const direction=m_6.$0;
                          _40=(setDragDiagnostic("data-drag-outcome", Equals(direction, {$:0})?"request-earlier":"request-later"),draftWindow.Set(null),requestAdjacentCoverage(direction, Equals(direction, {$:0})?-committed.Count:committed.Count));
                        }
                      }
                      else {
                        loadedDragDomain.$0[1];
                        loadedDragDomain.$0[0];
                        if(!Equals(draft, committed)){
                          loadedDragDomain.$0;
                          const projection_2=loadedDragDomain.$0[1];
                          const m_7=tryLocalWindowForLoadedCoverage(localTotal, projection_2, draft);
                          if(m_7==null){
                            const m_8=tryLoadedCoverageWindowIntent(projection_2, draft);
                            if(m_8==null)_40=(setDragDiagnostic("data-drag-outcome", "invalid-loaded-draft"),draftWindow.Set(null));
                            else {
                              const intent=m_8.$0;
                              setDragDiagnostic("data-drag-outcome", Equals(draft.StartIndex<committed.StartIndex?{$:0}:{$:1}, {$:0})?"request-earlier":"request-later");
                              draftWindow.Set(null);
                              const o_18=intent.StartObservationOrdinal;
                              let _41=o_18==null?0n:o_18.$0;
                              _40=sendOrQueueCoverageWindowAction(_41, intent.ObservationCount, "Loaded coverage requested.");
                            }
                          }
                          else {
                            const localDraft=m_7.$0;
                            const p_3=commitWindowBounds(options.MinimumVisibleBars, maximumVisibleBarsNow(), localTotal, localDraft);
                            _40=(setDragDiagnostic("data-drag-outcome", "commit-local"),navigatorCursorAfterRender=Some(drag=="move"?"grab":"ew-resize"),setWindow(p_3[0], p_3[1]));
                          }
                        }
                        else _40=(setDragDiagnostic("data-drag-outcome", "no-change"),draftWindow.Set(null));
                      }
                      cleanup();
                    }
                  };
                  moveHandler=(rawEvent_1) => {
                    const delta=bounds.width<=0||total<=0?0:toInt(Math.round((rawEvent_1.clientX-startClientX)/bounds.width*total));
                    latestRawDelta=delta;
                    pendingDraft=Some(previewWindowBounds(options.MinimumVisibleBars, maximumVisibleBarsNow(), total, committed, drag, delta));
                    return!draftFrameScheduled?(draftFrameScheduled=true,void requestAnimationFrame(() => {
                      if(!finished){
                        draftFrameScheduled=false;
                        setDragDiagnostic("data-drag-last-delta", String(latestRawDelta));
                        setDragDiagnostic("data-drag-outcome", "moving");
                        pendingDraft==null?void 0:draftWindow.Set(Some(pendingDraft.$0));
                      }
                    })):null;
                  };
                  upHandler=() => finish();
                  cancelHandler=() =>!finished?(finished=true,finishNavigatorDrag=null,activeNavigatorCursor=null,draftWindow.Set(null),setDragDiagnostic("data-drag-outcome", "cancelled"),cleanup()):null;
                  finishNavigatorDrag=Some(finish);
                  try {
                    navigatorRoot.setPointerCapture(pointerId);
                    setDragDiagnostic("data-drag-capture", "pointer");
                    navigatorRoot.addEventListener("pointermove", moveHandler);
                    navigatorRoot.addEventListener("pointerup", upHandler);
                    return navigatorRoot.addEventListener("pointercancel", cancelHandler);
                  }
                  catch(m_6){
                    documentFallback=true;
                    setDragDiagnostic("data-drag-capture", "document-fallback");
                    globalThis.document.addEventListener("pointermove", moveHandler);
                    globalThis.document.addEventListener("pointerup", upHandler);
                    return globalThis.document.addEventListener("pointercancel", cancelHandler);
                  }
                }
                else {
                  setDragDiagnostic("data-drag-mode", "none");
                  return setDragDiagnostic("data-drag-outcome", "outside-selection");
                }
              }
            }
            else return null;
          }, finishNavigatorDragFromElement), timeAxisWithPalette(currentPlotPalette, displayTime, "ta-overview-time-axis", "overview", overviewReferenceTimeline)]);
        }, shellPreparedData.View))])])])))))));
        return _39;
      }, chartRuntimeView, chartUiState.View, chartShellPreparedData.View))];
      return Doc.Element("div", _13, _16);
    }
    else {
      const pending=workspaceBootstrapPresentation(state);
      return Doc.Element("div", [Attr.Create("data-testid", "ta-workspace-bootstrap"), Attr.Create("data-state", pending.State), Attr.Create("style", "display:flex; flex-direction:column; gap:4px; padding:18px; color:"+(pending.IsError?"#9a2f2f":"#5d6d83")+";")], [Doc.Element("strong", [], [Doc.TextNode(pending.Title)]), Doc.Element("span", [Attr.Create("style", "font-size:12px;")], [Doc.TextNode(pending.Detail)])]);
    }
  }, runtimeState.View))]);
}
function ensureAxisResizeTracking(){
  if(!axisResizeBound()){
    set_axisResizeBound(true);
    const refresh=() => {
      let _1=axisViewportWidth();
      const a=320;
      const b=globalThis.innerWidth-48;
      let _2=Compare(a, b)===1?a:b;
      _1.Set(_2);
    };
    refresh();
    globalThis.addEventListener("resize", () => refresh());
  }
}
function nextRendererTelemetryInstanceId(){
  set_rendererTelemetryInstanceSequence(rendererTelemetryInstanceSequence()+1);
  return String(rendererTelemetryInstanceSequence());
}
function fixedText(value){
  return String(value);
}
function canvasIdText(a){
  return a.$0;
}
function remoteDisabled(a){
  return a.$==3||(a.$==6||(a.$==0||a.$==7));
}
function localViewportDisabled(a){
  return a.$==0||a.$==7;
}
function submit(callbacks, uiState, actualDocumentRevision, request_1, successText, onAccepted, onRejected, afterSettled){
  const m=request_1.ExpectedDocumentRevision;
  const expectedRevisionMatches=m==null||m.$0===actualDocumentRevision;
  if(uiState.Get().PendingActionId!=null){
    const _1=uiState.Get();
    let _2={
      Window:_1.Window, 
      FollowLatest:_1.FollowLatest, 
      HiddenRows:_1.HiddenRows, 
      HiddenTraces:_1.HiddenTraces, 
      RemovedTraces:_1.RemovedTraces, 
      AddRowOpen:_1.AddRowOpen, 
      CursorIndex:_1.CursorIndex, 
      PendingActionId:_1.PendingActionId, 
      Feedback:"action-in-flight: wait for the pending action result."
    };
    uiState.Set(_2);
  }
  else if(!expectedRevisionMatches){
    onRejected();
    const _3=uiState.Get();
    let _4={
      Window:_3.Window, 
      FollowLatest:_3.FollowLatest, 
      HiddenRows:_3.HiddenRows, 
      HiddenTraces:_3.HiddenTraces, 
      RemovedTraces:_3.RemovedTraces, 
      AddRowOpen:_3.AddRowOpen, 
      CursorIndex:_3.CursorIndex, 
      PendingActionId:null, 
      Feedback:"revision-conflict: workspace is at revision "+String(actualDocumentRevision)+"."
    };
    uiState.Set(_4);
    afterSettled();
  }
  else {
    const _5=uiState.Get();
    let _6={
      Window:_5.Window, 
      FollowLatest:_5.FollowLatest, 
      HiddenRows:_5.HiddenRows, 
      HiddenTraces:_5.HiddenTraces, 
      RemovedTraces:_5.RemovedTraces, 
      AddRowOpen:_5.AddRowOpen, 
      CursorIndex:_5.CursorIndex, 
      PendingActionId:Some(request_1.RequestId), 
      Feedback:"Submitting "+request_1.RequestId+"..."
    };
    uiState.Set(_6);
    StartImmediate(Delay(() => Bind_1(callbacks.SubmitAction(request_1), (a) => {
      let result, _7;
      if(a.$==1){
        const error_5=a.$0;
        result={
          $:1, 
          $0:request_1.RequestId, 
          $1:error_5.Code, 
          $2:error_5.Message
        };
      }
      else result=a.$0;
      const resultRequestId=result.$==1?result.$0:result.$==2?result.$0:result.$0;
      if(!Equals(uiState.Get().PendingActionId, Some(request_1.RequestId)))_7=Zero();
      else if(resultRequestId!=request_1.RequestId){
        onRejected();
        const _8=uiState.Get();
        let _9={
          Window:_8.Window, 
          FollowLatest:_8.FollowLatest, 
          HiddenRows:_8.HiddenRows, 
          HiddenTraces:_8.HiddenTraces, 
          RemovedTraces:_8.RemovedTraces, 
          AddRowOpen:_8.AddRowOpen, 
          CursorIndex:_8.CursorIndex, 
          PendingActionId:null, 
          Feedback:"action-correlation-mismatch: result does not match the pending request."
        };
        uiState.Set(_9);
        _7=Zero();
      }
      else if(result.$==1){
        const message=result.$2;
        const code=result.$1;
        onRejected();
        const _10=uiState.Get();
        let _11={
          Window:_10.Window, 
          FollowLatest:_10.FollowLatest, 
          HiddenRows:_10.HiddenRows, 
          HiddenTraces:_10.HiddenTraces, 
          RemovedTraces:_10.RemovedTraces, 
          AddRowOpen:_10.AddRowOpen, 
          CursorIndex:_10.CursorIndex, 
          PendingActionId:null, 
          Feedback:code+": "+message
        };
        uiState.Set(_11);
        _7=Zero();
      }
      else if(result.$==2){
        const actualRevision=result.$1;
        onRejected();
        const _12=uiState.Get();
        let _13={
          Window:_12.Window, 
          FollowLatest:_12.FollowLatest, 
          HiddenRows:_12.HiddenRows, 
          HiddenTraces:_12.HiddenTraces, 
          RemovedTraces:_12.RemovedTraces, 
          AddRowOpen:_12.AddRowOpen, 
          CursorIndex:_12.CursorIndex, 
          PendingActionId:null, 
          Feedback:"revision-conflict: workspace is at revision "+String(actualRevision)+"."
        };
        uiState.Set(_13);
        _7=Zero();
      }
      else {
        const revision=result.$1;
        const feedbackOverride=onAccepted();
        const _14=uiState.Get();
        let _15={
          Window:_14.Window, 
          FollowLatest:_14.FollowLatest, 
          HiddenRows:_14.HiddenRows, 
          HiddenTraces:_14.HiddenTraces, 
          RemovedTraces:_14.RemovedTraces, 
          AddRowOpen:_14.AddRowOpen, 
          CursorIndex:_14.CursorIndex, 
          PendingActionId:null, 
          Feedback:feedbackOverride==null?successText+" Revision "+String(revision)+".":feedbackOverride.$0
        };
        uiState.Set(_15);
        _7=Zero();
      }
      let _16=_7;
      return Combine(_16, Delay(() => {
        afterSettled();
        return Zero();
      }));
    })), null);
  }
}
function chartTopologySignaturePrepared(state, prepared){
  const m=state.Document;
  return m!=null&&m.$==1?collect((row) => map((trace) => {
    const timestamps=traceTopologyTimestampsPrepared(trace, prepared);
    let _1=length(timestamps);
    const o=tryHead(timestamps);
    let _2=o==null?"":o.$0;
    const o_1=tryLast(timestamps);
    let _3=o_1==null?"":o_1.$0;
    return[row.RowId, trace.TraceId, _1, _2, _3];
  }, filter_1((a) => a.Visible, effectiveTraces(row))), m.$0.Rows):[];
}
function runtimeDataChanged(left, right){
  return!Equals(left.Identity, right.Identity)||left.DataRevision!==right.DataRevision||!(left.Data===right.Data);
}
function tryLatestLegendValue(readersByRow, rowId, traceIndex){
  const o=readersByRow.TryFind(rowId);
  const o_1=o==null?null:tryItem(traceIndex, o.$0);
  return o_1==null?null:o_1.$0();
}
function tryLegendValue(readersByRow, rowId, traceIndex, cursorIndex){
  const o=readersByRow.TryFind(rowId);
  const o_1=o==null?null:tryItem(traceIndex, o.$0);
  return o_1==null?null:o_1.$0(cursorIndex);
}
function tryLatestRowPresentation(readersByRow, rowId){
  const o=readersByRow.TryFind(rowId);
  return o==null?null:tryPick((readLatest) => readLatest(), o.$0);
}
function tryRowPresentation(readersByRow, rowId, cursorIndex){
  const o=readersByRow.TryFind(rowId);
  return o==null?null:tryPick((readValue) => readValue(cursorIndex), o.$0);
}
function cursorPosition(width, pointCount, cursorIndex){
  return cursorIndex==null?null:slotCenter(width, pointCount, cursorIndex.$0);
}
function rowCursorTagStyle(leftPercent, visible){
  return"position:absolute; z-index:3; top:2px; left:"+leftPercent+"%; transform:translateX(-50%); box-sizing:border-box; display:grid; grid-template-rows:12px 12px;"+" width:92px; min-width:92px; max-width:92px; height:28px; min-height:28px; max-height:28px;"+" padding:1px 5px; border:1px solid #9eabba; border-radius:2px; background:#f8fafc; color:#263b55;"+" font-family:Consolas,monospace; font-size:10px; font-weight:500; line-height:12px;"+" text-align:center; font-variant-numeric:tabular-nums; white-space:nowrap; pointer-events:none;"+(visible?" visibility:visible;":" visibility:hidden;");
}
function compactButton(testId, label, titleText, onClick){
  return Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("data-testid", testId), Attr.Create("title", titleText), Attr.Create("style", "height:30px; border:1px solid #9fb0c6; border-radius:4px; background:#f8fafc; color:#20344f; padding:3px 9px; font-size:12px; cursor:pointer; white-space:nowrap;"), Handler("click", () =>() => onClick())], [Doc.TextNode(label)]);
}
function plotPalette(defaultView){
  return resolve(defaultView).Theme.$==0?lightPlotPalette():darkPlotPalette();
}
function freshnessClass(freshness){
  return freshness.$==1?"delayed":freshness.$==3?"delayed":freshness.$==2?"stale":freshness.$==4?"stale":"live";
}
function pollText(a){
  return a.$==1?"MOUNTED":a.$==2?"READY":a.$==3?"UPDATING":a.$==5?"SUSPENDED":a.$==6?"RESYNC":a.$==4?"BACKOFF":a.$==7?"DISPOSED":"UNMOUNTED";
}
function inputText(testId, placeholder, initial_2, onChanged){
  return element_1("input", [Attr.Create("data-testid", testId), Attr.Create("type", "text"), Attr.Create("placeholder", placeholder), Attr.Create("value", initial_2), Attr.Create("style", "height:30px; min-width:0; width:100%; border:1px solid #b9c6d8; border-radius:4px; background:#fff; color:#142033; padding:4px 7px; box-sizing:border-box; font-size:12px;"), OnAfterRender((node) => {
    const input_1=node;
    input_1.addEventListener("input", () => onChanged(input_1.value));
  })], []);
}
function selectInput(testId, initial_2, values, onChanged){
  return element_1("select", [Attr.Create("data-testid", testId), Attr.Create("style", "height:30px; min-width:0; width:100%; border:1px solid #b9c6d8; border-radius:4px; background:#fff; color:#142033; padding:3px 6px; box-sizing:border-box; font-size:12px;"), OnAfterRender((node) => {
    const input_1=node;
    input_1.value=initial_2;
    input_1.addEventListener("change", () => onChanged(input_1.value));
  })], ofSeq_1(delay(() => collect_1((m) =>[element_1("option", [Attr.Create("value", m[0])], [Doc.TextNode(m[1])])], values))));
}
function primaryButtonView(testId, label, disabled, isDisabled, onClick){
  return Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("data-testid", testId), DynamicBool("disabled", disabled), Dynamic_1("style", Map((value) => value?"height:30px; border:1px solid #9aa8b8; border-radius:4px; background:#d8e0e8; color:#667587; padding:3px 11px; font-size:12px; cursor:not-allowed; white-space:nowrap;":"height:30px; border:1px solid #0f766e; border-radius:4px; background:#0f766e; color:#fff; padding:3px 11px; font-size:12px; cursor:pointer; white-space:nowrap;", disabled)), Handler("click", () =>() =>!isDisabled()?onClick():null)], [Doc.TextNode(label)]);
}
function compactRemoteButton(testId, label, titleText, disabled, isDisabled, onClick){
  return Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("data-testid", testId), Attr.Create("title", titleText), DynamicBool("disabled", disabled), Dynamic_1("style", Map((value) => value?"height:30px; border:1px solid #c8d2df; border-radius:4px; background:#edf1f5; color:#8b98a8; padding:3px 9px; font-size:12px; cursor:not-allowed; white-space:nowrap;":"height:30px; border:1px solid #9fb0c6; border-radius:4px; background:#f8fafc; color:#20344f; padding:3px 9px; font-size:12px; cursor:pointer; white-space:nowrap;", disabled)), Handler("click", () =>() =>!isDisabled()?onClick():null)], [Doc.TextNode(label)]);
}
function rowDisplayLabelWithEditor(schemas, row){
  const _1=rowExplicitLabel(row);
  const _2=rowEditorSummary(schemas, row);
  return _1==null?_2==null?rowKindText_1(row.Kind):_2.$0:_2==null?_1.$0:_1.$0+" · "+_2.$0;
}
function rowDisplayLabel(row){
  const x=rowExplicitLabel(row);
  const v=rowKindText_1(row.Kind);
  return x==null?v:x.$0;
}
function renderRowReactivePreparedLiveWithHeightPalette(palette, displayTime, _1, ui, preparedData, dataView, visibleTimestamps, cursorIndex, setCursorIndex, commitCursorIndex, showSharedTimeAxis, isBaseRow, rowHeight, scheduleValueRefresh, registerLegendElement, row){
  const traces=filter_1((trace) => {
    const key_1=[row.RowId, trace.TraceId];
    return trace.Visible&&!ui.HiddenTraces.Contains(key_1)&&!ui.RemovedTraces.Contains(key_1);
  }, effectiveTraces(row));
  const hasCursorEventCapability=exists((trace) => Equals(trace.Kind, {$:4})||Equals(trace.Kind, {$:5}), traces);
  const p=compositeSvgReactivePreparedLiveWithHeightPalette(palette, displayTime, row.RowId, isBaseRow, traces, preparedData, dataView, visibleTimestamps, cursorIndex, setCursorIndex, commitCursorIndex, rowHeight, scheduleValueRefresh);
  const timestamps=p[1];
  const latestLegendReaders=p[4];
  const chart=p[0];
  const title=rowTitle(row, traces);
  const heightBounds=rowHeightBounds(row, traces);
  const cursorTag=Doc.Element("div", [Attr.Create("data-testid", "ta-row-cursor-label-"+row.RowId), Attr.Create("data-ta-row-cursor-label", "true"), Attr.Create("data-ta-row-cursor-row-id", row.RowId), Attr.Create("data-fixed-css-overlay", "true"), Attr.Create("style", rowCursorTagStyle("50", false))], [Doc.Element("span", [Attr.Create("data-testid", "ta-row-cursor-date-"+row.RowId), Attr.Create("data-ta-row-cursor-date", "true"), Attr.Create("style", "display:block; width:80px; height:12px; line-height:12px; overflow:hidden;")], [Doc.TextNode("Unavailable")]), Doc.Element("span", [Attr.Create("data-testid", "ta-row-cursor-time-"+row.RowId), Attr.Create("data-ta-row-cursor-clock", "true"), Attr.Create("style", "display:block; width:80px; height:12px; line-height:12px; overflow:hidden;")], [Doc.TextNode("Unavailable")])]);
  const children=ofArray([Doc.Element("div", [Attr.Create("data-testid", "ta-row-plot-shell-"+row.RowId), Attr.Create("style", "position:relative; display:flex; flex-direction:column; min-width:0; overflow:hidden;")], ofSeq_1(delay(() => append_2([Doc.Element("div", [Attr.Create("data-testid", "ta-row-cursor-gutter-"+row.RowId), Attr.Create("data-fixed-height", "32"), Attr.Create("data-plot-surface-theme", palette.ThemeName), Attr.Create("style", "box-sizing:border-box; height:32px; min-height:32px; max-height:32px; flex:0 0 32px; border-bottom:1px solid "+palette.Grid+"; background:"+palette.AxisSurface+";")], [])], delay(() => append_2([cursorTag], delay(() => append_2([Doc.Element("div", [Attr.Create("data-testid", "ta-row-ofi-band-"+row.RowId), Attr.Create("data-ta-row-ofi-band", "true"), Attr.Create("data-ta-row-ofi-row-id", row.RowId), Attr.Create("data-ta-row-cursor-events", "true"), Attr.Create("data-cursor-event-capability", hasCursorEventCapability?"available":"unavailable"), Attr.Create("data-marker-event-count", "0"), Attr.Create("data-fixed-height", "24"), Attr.Create("style", "box-sizing:border-box; display:flex; align-items:center; gap:4px; height:24px; min-height:24px; max-height:24px; flex:0 0 24px; padding:2px 8px; border-bottom:1px solid "+palette.Grid+"; background:"+palette.AxisSurface+"; overflow:hidden; white-space:nowrap; font-family:Consolas,monospace; font-size:10px; color:"+palette.LegendText+";")], ofSeq_1(delay(() => append_2([Doc.Element("span", [Attr.Create("data-ta-row-ofi-empty", "true"), Attr.Create("data-cursor-event-state", hasCursorEventCapability?"none":"unavailable"), Attr.Create("style", "display:inline-flex; align-items:center; height:18px; color:#708198;")], [Doc.TextNode("")])], delay(() => append_2(map_2((index) => Doc.Element("span", [Attr.Create("data-ta-row-ofi-item-index", String(index)), Attr.Create("hidden", "hidden"), Attr.Create("style", "display:inline-flex; align-items:center; max-width:220px; height:18px; padding:0 5px; border:1px solid "+palette.Border+"; border-radius:3px; overflow:hidden; text-overflow:ellipsis; white-space:nowrap;")], [Doc.TextNode("")]), range(0, 4-1)), delay(() =>[Doc.Element("span", [Attr.Create("data-ta-row-ofi-overflow", "true"), Attr.Create("hidden", "hidden"), Attr.Create("style", "display:inline-flex; align-items:center; height:18px; padding:0 5px; border:1px solid "+palette.Border+"; border-radius:3px; white-space:nowrap;")], [Doc.TextNode("")])])))))))], delay(() => append_2([chart], delay(() => showSharedTimeAxis?[timeAxisWithPalette(palette, displayTime, "ta-time-axis-"+row.RowId, row.RowId, timestamps)]:[])))))))))))]);
  const metadata=Doc.EmbedView(Map((_2) => {
    const currentData=_2[0];
    const zone=_2[1];
    return Doc.Element("span", [Attr.Create("style", "display:inline-flex; align-items:center; gap:6px 10px; flex:0 0 auto; flex-wrap:nowrap; white-space:nowrap;")], ofSeq_1(delay(() => collect_1((value) => {
      const o=value.AvailableAtUtc;
      const o_1=o==null?null:Some(compactOrOriginal(zone, o.$0));
      const availability=o_1==null?"unknown":o_1.$0;
      const o_2=value.Quality;
      const quality=o_2==null?"unknown":o_2.$0;
      return[Doc.Element("span", [Attr.Create("data-testid", "ta-row-meta-"+row.RowId+"-"+value.ScaleKey), Attr.Create("data-scale-key", value.ScaleKey), Attr.Create("data-finality", value.Finality), Attr.Create("data-quality", quality), Attr.Create("data-display-time-zone", id(zone)), Attr.Create("title", temporalDetailWith((c) => fullOrOriginal(zone, c), value)), Attr.Create("style", "display:inline-flex; align-items:center; min-height:20px; padding:1px 6px; border:1px solid #bcc9d8; border-radius:4px; background:#f7fafc; color:#465b74; font-family:Consolas,monospace; font-size:10px; white-space:nowrap;")], [Doc.TextNode(value.ScaleKey+" | "+value.Finality+" | "+quality+" | frontier "+compactOrOriginal(zone, value.ObservedThroughUtc)+" | available "+availability)])];
    }, rowTemporalMetadataPrepared(row, currentData)))));
  }, Map2((_2, _3) =>[_2, _3], dataView, displayTime.Zone)));
  const legend=Doc.Element("div", [Attr.Create("data-testid", "ta-row-values-"+row.RowId), Attr.Create("data-ta-row-values", "true"), Attr.Create("data-auto-height", "true"), Attr.Create("data-plot-surface-theme", palette.ThemeName), Attr.Create("style", "box-sizing:border-box; display:flex; align-items:center; align-content:center; gap:4px 14px; min-height:30px; padding:4px 8px; border-top:1px solid "+palette.Grid+"; border-bottom:1px solid "+palette.Grid+"; background:"+palette.AxisSurface+"; overflow:visible; flex-wrap:wrap; white-space:normal; font-family:Consolas,monospace; font-size:11px; line-height:16px; color:"+palette.LegendText+";"), OnAfterRender(registerLegendElement)], ofSeq_1(delay(() => {
    let o;
    const o_1=length(timestamps)===0?null:tryPick((readLatest) => readLatest(), latestLegendReaders);
    if(o_1==null)o=null;
    else {
      const value=o_1.$0;
      let _2=fullOrOriginal(displayTime.Current(), value.Timestamp);
      o=Some(_2);
    }
    const initialTimestamp=o==null?"Unavailable":o.$0;
    return append_2([Doc.Element("span", [Attr.Create("data-testid", "ta-row-data-time-"+row.RowId), Attr.Create("data-ta-row-data-time", "true"), Attr.Create("data-ta-row-data-time-row-id", row.RowId), Dynamic_1("data-display-time-zone", Map(id, displayTime.Zone)), Attr.Create("style", "display:inline-block; width:19ch; min-width:19ch; max-width:19ch; overflow:hidden; white-space:nowrap; font-variant-numeric:tabular-nums; font-weight:650;")], [Doc.TextNode(initialTimestamp)])], delay(() => collect_1((index) => {
      let initialValue;
      const trace=get(traces, index);
      if(!Equals(trace.Kind, {$:4})&&!Equals(trace.Kind, {$:5})){
        const label=IsNullOrWhiteSpace(trace.Label)?trace.TraceId:trace.Label;
        if(length(timestamps)===0)initialValue="Unavailable";
        else {
          const o_2=(get(latestLegendReaders, index))();
          const o_3=o_2==null?null:Some(o_2.$0.Value);
          initialValue=o_3==null?"Unavailable":o_3.$0;
        }
        const valueWidth=trace.Kind.$==0?"46ch":"16ch";
        return[Doc.Element("span", [Attr.Create("data-testid", "ta-row-value-"+row.RowId+"-"+trace.TraceId), Attr.Create("data-ta-row-value-token", "true"), Attr.Create("style", "display:inline-flex; align-items:baseline; gap:4px; flex:0 0 auto; height:20px; line-height:20px; white-space:nowrap;")], [Doc.Element("span", [Attr.Create("data-ta-row-value-label", "true"), Attr.Create("style", "font-weight:650;")], [Doc.TextNode(label)]), Doc.Element("span", [Attr.Create("data-ta-row-value-index", String(index)), Attr.Create("data-ta-row-value-row-id", row.RowId), Attr.Create("data-ta-row-value-text", "true"), Attr.Create("data-value-state", initialValue=="Unavailable"?"undefined":"defined"), Attr.Create("title", label+" value"), Attr.Create("style", "display:inline-block; width:"+valueWidth+"; min-width:"+valueWidth+"; max-width:"+valueWidth+"; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; font-variant-numeric:tabular-nums;")], [Doc.TextNode(initialValue)])])];
      }
      else return[];
    }, range(0, length(traces)-1))));
  })));
  const frameHeight=Map((value) => value+114+(showSharedTimeAxis?16:0), rowHeight.View);
  return[Doc.Element("div", [Attr.Create("data-testid", "ta-row-shell-"+row.RowId), Attr.Create("style", "display:flex; flex-direction:column; min-width:0;")], [chartFrame(title, [metadata], legend, "ta-row-"+row.RowId, frameHeight, children), rowResizeHandle(row.RowId, heightBounds, rowHeight)]), p[2], p[3], latestLegendReaders, p[5]];
}
function overviewSvgWithPalette(palette, points, stripeVisuals, referenceLength, selectionWindow, onReady, onPointerDown, onDragEnd){
  let latestSelectionRatios;
  const width=1000;
  const stripeTooltip=_c_2.Create_1(null);
  const sampled=compactOverviewCandles(280, points);
  const authoredWickCount=sumBy((point) => point.High!=null&&point.Low!=null?1:0, sampled);
  const bodyOnlyCount=length(sampled)-authoredWickCount;
  const p=paddedRange(0, 1, collect((point) => {
    const a_2=point.Open;
    const b_2=point.Close;
    const bodyLow=Compare(a_2, b_2)===-1?a_2:b_2;
    const a_3=point.Open;
    const b_3=point.Close;
    const bodyHigh=Compare(a_3, b_3)===1?a_3:b_3;
    const o=point.Low;
    let _2=o==null?bodyLow:o.$0;
    const o_1=point.High;
    let _3=o_1==null?bodyHigh:o_1.$0;
    return[_2, _3];
  }, sampled));
  const low=p[0];
  const high=p[1];
  const candleSlot=length(sampled)===0?width:width/length(sampled);
  const a=1;
  const a_1=3.2;
  const b=candleSlot*0.58;
  const b_1=Compare(a_1, b)===-1?a_1:b;
  const candleBodyWidth=Compare(a, b_1)===1?a:b_1;
  const xAt=(index) => candleSlot*(index+0.5);
  const yAt=(value) => normalize(low, high, 8, 62, value);
  const wickPath=concat_1(" ", choose((_2) => {
    let _3;
    const point=_2[1];
    const _4=point.High;
    const _5=point.Low;
    if(_4!=null&&_4.$==1&&(_5!=null&&_5.$==1&&(_3=[_4.$0, _5.$0],true))){
      const x=fixedText(xAt(_2[0]));
      return Some("M "+x+" "+fixedText(yAt(_3[0]))+" L "+x+" "+fixedText(yAt(_3[1])));
    }
    else return null;
  }, mapi((_2, _3) =>[_2, _3], sampled)));
  const bodyPath=(keep) => concat_1(" ", map((_2) => {
    const point=_2[1];
    const openY=yAt(point.Open);
    const closeY=yAt(point.Close);
    let _3=xAt(_2[0])-candleBodyWidth/2;
    let _4=Compare(openY, closeY)===-1?openY:closeY;
    const a_2=0.8;
    const b_2=Math.abs(closeY-openY);
    let _5=Compare(a_2, b_2)===1?a_2:b_2;
    return rectanglePath(_3, _4, candleBodyWidth, _5);
  }, filter_1((x) => keep(x[1]), mapi((_2, _3) =>[_2, _3], sampled))));
  const upBodyPath=bodyPath((point) => point.Close>point.Open);
  const downBodyPath=bodyPath((point) => point.Close<point.Open);
  const flatBodyPath=bodyPath((point) => point.Close===point.Open);
  const handleWidth=8;
  const selectionGeometry=(l, r) => navigatorSelectionBounds(width, l, r);
  const geometryText=(projection) => {
    const f_1=(x) => projection(selectionGeometry.apply(null, x));
    return Map((x) => fixedText(f_1(x)), selectionWindow);
  };
  const selectionX=geometryText((t) => t[0]);
  const selectionWidth=geometryText((t) => t[1]);
  const selectionRightX=geometryText((t) => t[0]+t[1]);
  const leftHandleX=geometryText((t) => {
    const a_2=0;
    const a_3=width-handleWidth;
    const b_2=t[0]-handleWidth/2;
    const b_3=Compare(a_3, b_2)===-1?a_3:b_2;
    return Compare(a_2, b_3)===1?a_2:b_3;
  });
  const rightHandleX=geometryText((t) => {
    const a_2=0;
    const a_3=width-handleWidth;
    const b_2=t[0]+t[1]-handleWidth/2;
    const b_3=Compare(a_3, b_2)===-1?a_3:b_2;
    return Compare(a_2, b_3)===1?a_2:b_3;
  });
  const moveHitGeometry=(ratios) => {
    const p_1=selectionGeometry.apply(null, ratios);
    const selectionWidth_1=p_1[1];
    const b_2=selectionWidth_1/2;
    const inset=Compare(handleWidth, b_2)===-1?handleWidth:b_2;
    const a_2=0;
    const b_3=selectionWidth_1-inset*2;
    let _2=Compare(a_2, b_3)===1?a_2:b_3;
    return[p_1[0]+inset, _2];
  };
  const moveHitX=geometryText((x) =>(moveHitGeometry(x))[0]);
  const moveHitWidth=geometryText((x) =>(moveHitGeometry(x))[1]);
  latestSelectionRatios=[0, 0];
  const observedSelectionWindow=Map((ratios) => {
    latestSelectionRatios=ratios;
    return ratios;
  }, selectionWindow);
  const leftRatioText=Map((x) => fixedText(x[0]), observedSelectionWindow);
  const rightRatioText=Map((x) => fixedText(x[1]), observedSelectionWindow);
  const leftVisualStyle=Map((ratios) =>(selectionGeometry.apply(null, ratios))[0]<=0.0001?"transform:translateX(1px);":"", selectionWindow);
  const rightVisualStyle=Map((ratios) => {
    const p_1=selectionGeometry.apply(null, ratios);
    return p_1[0]+p_1[1]>=width-0.0001?"transform:translateX(-1px);":"";
  }, selectionWindow);
  const stripeX=(visual) => {
    const o=slotCenter(width, referenceLength, visual.SlotIndex);
    const value=o==null?width/2:o.$0;
    const a_2=0;
    const b_2=value+visual.Lane*1.5;
    const b_3=Compare(width, b_2)===-1?width:b_2;
    return Compare(a_2, b_3)===1?a_2:b_3;
  };
  const stripePaths=map((_2) => {
    const a_2=_2[0];
    const visuals=_2[1];
    const path=concat_1(" ", map((visual) => {
      const x=fixedText(stripeX(visual));
      return"M "+x+" 0 L "+x+" 82";
    }, visuals));
    return[a_2[0], a_2[1], sumBy((visual) => length(visual.Stripes), visuals), path];
  }, groupBy((visual) => {
    const first=get(visual.Stripes, 0);
    return[first.Color, first.StrokeWidthCssPixels];
  }, stripeVisuals));
  const f=(x) => Math.round(stripeX(x));
  let _1=groupBy((x) => toInt(f(x)), stripeVisuals);
  const stripeBuckets=OfArray(_1);
  return svgElement("svg", [Attr.Create("data-testid", "ta-overview-navigator"), Attr.Create("data-plot-surface-theme", palette.ThemeName), Attr.Create("data-loaded-sample-count", String(length(sampled))), Attr.Create("data-overview-source-count", String(length(points))), Attr.Create("data-drag-hit-target-css-pixels", "24"), Dynamic_1("data-selection-left-ratio", leftRatioText), Dynamic_1("data-selection-right-ratio", rightRatioText), svgAttr("viewBox", "0 0 1000 82"), svgAttr("preserveAspectRatio", "none"), Attr.Create("style", "display:block; width:100%; height:82px; min-width:0; background:"+palette.OverviewSurface+"; border:1px solid "+palette.Border+"; border-radius:4px; box-sizing:border-box; touch-action:none; cursor:default;"), OnAfterRender(onReady), Attr.Create("data-drag-event-binding", "navigator-root"), Attr.Create("data-drag-bounds-source", "navigator-root"), Handler("pointerdown", (element_2) =>(event) => onPointerDown(element_2, event)), Handler("pointerup", () => onDragEnd), Handler("mousemove", (element_2) =>(event) => {
    let _2;
    const bounds=element_2.getBoundingClientRect();
    if(bounds.width>0){
      const html=element_2;
      const dragOutcome=element_2.getAttribute("data-drag-outcome");
      if(dragOutcome=="tracking"||dragOutcome=="moving")_2=html.style.cursor="grabbing";
      else {
        const pointerX=event.clientX-bounds.left;
        const m=navigatorDragMode(bounds.width, 24, latestSelectionRatios[0], latestSelectionRatios[1], pointerX);
        const cursor=m==null?"default":m.$0=="move"?"grab":"ew-resize";
        _2=html.style.cursor=cursor;
      }
      const a_2=0;
      const b_2=(event.clientX-bounds.left)/bounds.width*width;
      const b_3=Compare(width, b_2)===-1?width:b_2;
      const x=Compare(a_2, b_3)===1?a_2:b_3;
      const pixel=toInt(Math.round(x));
      const o=tryPick((key_1) => stripeBuckets.TryFind(key_1), [pixel, pixel-1, pixel+1, pixel-2, pixel+2]);
      let _3=o==null?null:Some([x, concat_1(" | ", collect((visual) => map((stripe) => {
        const o_1=stripe.Label;
        let _4=o_1==null?stripe.StripeId:o_1.$0;
        let _5=_4+" · ";
        return _5+stripe.EventTimeUtc;
      }, visual.Stripes), o.$0).slice(0, 8))]);
      return stripeTooltip.Set(_3);
    }
    else return null;
  }), Handler("mouseleave", (element_2) =>() => {
    stripeTooltip.Set(null);
    const dragOutcome=element_2.getAttribute("data-drag-outcome");
    return dragOutcome!="tracking"&&dragOutcome!="moving"?element_2.style.cursor="default":null;
  })], ofSeq_1(delay(() => append_2([svgElement("rect", [Attr.Create("data-testid", "ta-overview-interaction-surface"), Attr.Create("data-drag-event-binding", "bubbles-to-navigator-root"), Attr.Create("data-drag-bounds-source", "navigator-root"), svgAttr("x", "0"), svgAttr("y", "0"), svgAttr("width", "1000"), svgAttr("height", "82"), svgAttr("fill", "transparent"), svgAttr("pointer-events", "all")], [])], delay(() => append_2([svgElement("path", [Attr.Create("data-testid", "ta-overview-candle-wicks"), Attr.Create("data-candle-sample-count", String(length(sampled))), Attr.Create("data-authored-wick-count", String(authoredWickCount)), Attr.Create("data-body-only-count", String(bodyOnlyCount)), svgAttr("d", wickPath), svgAttr("fill", "none"), svgAttr("stroke", palette.OverviewPrice), svgAttr("stroke-width", "0.8"), svgAttr("vector-effect", "non-scaling-stroke"), svgAttr("pointer-events", "none")], [])], delay(() => append_2([svgElement("path", [Attr.Create("data-testid", "ta-overview-candle-up-bodies"), svgAttr("d", upBodyPath), svgAttr("fill", palette.OverviewCandleUp), svgAttr("stroke", "none"), svgAttr("pointer-events", "none")], [])], delay(() => append_2([svgElement("path", [Attr.Create("data-testid", "ta-overview-candle-down-bodies"), svgAttr("d", downBodyPath), svgAttr("fill", palette.OverviewCandleDown), svgAttr("stroke", "none"), svgAttr("pointer-events", "none")], [])], delay(() => append_2([svgElement("path", [Attr.Create("data-testid", "ta-overview-candle-flat-bodies"), svgAttr("d", flatBodyPath), svgAttr("fill", palette.OverviewCandleFlat), svgAttr("stroke", "none"), svgAttr("pointer-events", "none")], [])], delay(() => append_2(collect_1((m) =>[svgElement("path", [Attr.Create("data-testid", "ta-overview-stripe-path"), Attr.Create("data-stripe-count", String(m[2])), svgAttr("d", m[3]), svgAttr("fill", "none"), svgAttr("stroke", m[0]), svgAttr("stroke-width", fixedText(m[1])), svgAttr("vector-effect", "non-scaling-stroke"), svgAttr("pointer-events", "none")], [])], stripePaths), delay(() => append_2([svgElement("rect", [Attr.Create("data-testid", "ta-overview-selection"), Dynamic_1("x", selectionX), svgAttr("y", "1"), Dynamic_1("width", selectionWidth), svgAttr("height", "80"), svgAttr("fill", palette.OverviewSelection), svgAttr("stroke", "none"), svgAttr("pointer-events", "none")], [])], delay(() => append_2([svgElement("line", [Attr.Create("data-testid", "ta-overview-left-handle-visual"), Attr.Create("data-stroke-width-css-pixels", "2"), Dynamic_1("style", leftVisualStyle), Dynamic_1("x1", selectionX), Dynamic_1("x2", selectionX), svgAttr("y1", "1"), svgAttr("y2", "81"), svgAttr("stroke", palette.OverviewBoundary), svgAttr("stroke-width", "2"), svgAttr("vector-effect", "non-scaling-stroke"), svgAttr("pointer-events", "none")], [])], delay(() => append_2([svgElement("rect", [Attr.Create("data-testid", "ta-overview-left-handle"), Dynamic_1("x", leftHandleX), svgAttr("y", "0"), svgAttr("width", fixedText(handleWidth)), svgAttr("height", "82"), svgAttr("fill", "transparent"), svgAttr("pointer-events", "none"), svgAttr("style", "cursor:ew-resize;")], [])], delay(() => append_2([svgElement("line", [Attr.Create("data-testid", "ta-overview-right-handle-visual"), Attr.Create("data-stroke-width-css-pixels", "2"), Dynamic_1("style", rightVisualStyle), Dynamic_1("x1", selectionRightX), Dynamic_1("x2", selectionRightX), svgAttr("y1", "1"), svgAttr("y2", "81"), svgAttr("stroke", palette.OverviewBoundary), svgAttr("stroke-width", "2"), svgAttr("vector-effect", "non-scaling-stroke"), svgAttr("pointer-events", "none")], [])], delay(() => append_2([svgElement("rect", [Attr.Create("data-testid", "ta-overview-right-handle"), Dynamic_1("x", rightHandleX), svgAttr("y", "0"), svgAttr("width", fixedText(handleWidth)), svgAttr("height", "82"), svgAttr("fill", "transparent"), svgAttr("pointer-events", "none"), svgAttr("style", "cursor:ew-resize;")], [])], delay(() => append_2([svgElement("rect", [Attr.Create("data-testid", "ta-overview-move-hit"), Dynamic_1("x", moveHitX), svgAttr("y", "0"), Dynamic_1("width", moveHitWidth), svgAttr("height", "82"), svgAttr("fill", "transparent"), svgAttr("pointer-events", "none"), svgAttr("style", "cursor:grab;")], [])], delay(() =>[Doc.EmbedView(Map((a_2) => {
    if(a_2!=null&&a_2.$==1){
      const x=a_2.$0[0];
      const value=a_2.$0[1];
      const bounded=value.length<=180?value:Substring(value, 0, 177)+"...";
      const a_3=4;
      const a_4=716;
      const b_2=x+6;
      const b_3=Compare(a_4, b_2)===-1?a_4:b_2;
      const boxX=Compare(a_3, b_3)===1?a_3:b_3;
      return svgElement("g", [Attr.Create("data-testid", "ta-overview-stripe-tooltip"), svgAttr("pointer-events", "none")], [svgElement("rect", [svgAttr("x", fixedText(boxX)), svgAttr("y", "3"), svgAttr("width", "280"), svgAttr("height", "18"), svgAttr("rx", "2"), svgAttr("fill", palette.TooltipSurface), svgAttr("fill-opacity", "0.95"), svgAttr("stroke", palette.TooltipBorder), svgAttr("stroke-width", "0.7")], []), svgElement("text", [svgAttr("x", fixedText(boxX+5)), svgAttr("y", "15"), svgAttr("fill", palette.TooltipText), svgAttr("font-family", "Consolas,monospace"), svgAttr("font-size", "9")], [Doc.TextNode(bounded)])]);
    }
    else return Doc.Empty;
  }, stripeTooltip.View))])))))))))))))))))))))))))));
}
function timeAxisWithPalette(palette, displayTime, testId, rowId, timestamps){
  return Doc.EmbedView(Map((_1) => {
    const zone=_1[1];
    const labels=adaptiveTimeLabels(124, _1[0], timestamps);
    return Doc.Element("div", [Attr.Create("data-testid", testId), Attr.Create("data-time-axis-row-id", rowId), Attr.Create("data-time-axis-tick-count", String(length(labels))), Attr.Create("data-display-time-zone", id(zone)), Attr.Create("data-plot-surface-theme", palette.ThemeName), Attr.Create("style", "position:relative; min-width:0; height:18px; padding:0 1px; overflow:hidden; background:"+palette.AxisSurface+";")], ofSeq_1(delay(() => collect_1((position) => {
      const p=get(labels, position);
      const label=p[1];
      const left=length(timestamps)<=1?50:p[0]/(length(timestamps)-1)*100;
      const transform=position===0?"none":position===length(labels)-1?"translateX(-100%)":"translateX(-50%)";
      return[Doc.Element("span", [Attr.Create("data-time-axis-event-time", label), Attr.Create("data-canonical-event-time", label), Attr.Create("data-display-time-zone", id(zone)), Attr.Create("style", "position:absolute; left:"+fixedText(left)+"%; transform:"+transform+"; max-width:92px; color:"+palette.AxisText+"; font-size:10px; line-height:16px; white-space:nowrap; overflow:hidden; text-overflow:ellipsis;")], [Doc.TextNode(compactOrOriginal(zone, label))])];
    }, range(0, length(labels)-1)))));
  }, Map2((_1, _2) =>[_1, _2], axisViewportWidth().View, displayTime.Zone)));
}
function sameDocumentShell(left, right){
  return Equals(left.Identity, right.Identity)&&sameDocumentPresentation(left, right);
}
function element_1(name, attrs, children){
  return Doc.Element(name, attrs, children);
}
function sameDocumentPresentation(left, right){
  const _1=left.Document;
  const _2=right.Document;
  if(_1!=null&&_1.$==1){
    if(_2!=null&&_2.$==1){
      const leftDocument=_1.$0;
      const rightDocument=_2.$0;
      return Equals(normalizeDocumentPresentation(leftDocument), normalizeDocumentPresentation(rightDocument));
    }
    else return false;
  }
  else return _2==null;
}
function axisResizeBound(){
  return _c_8.axisResizeBound;
}
function set_axisResizeBound(_1){
  _c_8.axisResizeBound=_1;
}
function axisViewportWidth(){
  return _c_8.axisViewportWidth;
}
function set_rendererTelemetryInstanceSequence(_1){
  _c_8.rendererTelemetryInstanceSequence=_1;
}
function rendererTelemetryInstanceSequence(){
  return _c_8.rendererTelemetryInstanceSequence;
}
function lightPlotPalette(){
  return _c_8.lightPlotPalette;
}
function darkPlotPalette(){
  return _c_8.darkPlotPalette;
}
function rowExplicitLabel(row){
  let _1;
  if(row.Options==null)return null;
  else {
    const m=row.Options.TryFind("label");
    return m!=null&&m.$==1&&(m.$0.$==3&&(!IsNullOrWhiteSpace(m.$0.$0)&&(_1=m.$0.$0,true)))?Some(_1):null;
  }
}
function rowEditorSummary(schemas, row){
  let _1;
  const m=tryResolve(schemas, row);
  if(m.$==0){
    const _2=m.$0;
    _1=_2!=null&&_2.$==1;
  }
  else _1=false;
  if(_1){
    const values=m.$0.$0[1];
    const schema=m.$0.$0[0];
    const parameters=concat_1("; ", choose((field_1) => {
      const fieldValues=map((input_1) => editorScalarText(input_1.Value), filter_1((input_1) => editorPathRoot(input_1.Path)==field_1.Key, values));
      return length(fieldValues)===0?null:Some(field_1.Label+" "+concat_1(", ", fieldValues));
    }, schema.Fields));
    return IsNullOrWhiteSpace(parameters)?Some(schema.DisplayName):Some(schema.DisplayName+" · "+parameters);
  }
  else return null;
}
function rowKindText_1(a){
  return a.$==1?"Volume":a.$==2?"SMA":a.$==3?"DMI":a.$==4?"ADX":a.$==5?"MACD":a.$==6?"Heikin-Ashi":"Candlestick";
}
function compositeSvgReactivePreparedLiveWithHeightPalette(palette, displayTime, rowId, isBaseRow, traces, preparedData, dataView, referenceTimestamps, _1, setCursorIndex, commitCursorIndex, chartPixelHeight, scheduleValueRefresh){
  let observedPreparedData, observedChartPixelHeight;
  const width=1000;
  const hasCandles=exists((trace) => Equals(trace.Kind, {$:0}), traces);
  const height=hasCandles?250:112;
  const top=hasCandles?0:10;
  const plotHeight=hasCandles?height:92;
  const tracePalette=["#2764b0", "#9b5b24", "#6a4ca3", "#0f766e", "#b45309", "#be185d", "#475569", "#0891b2"];
  const color=(index, trace) => IsNullOrWhiteSpace(trace.Color)?get(tracePalette, index%length(tracePalette)):trace.Color;
  const xAt=(index) => {
    const o=slotCenter(width, length(referenceTimestamps), index);
    return o==null?width/2:o.$0;
  };
  const maximumVisualPoints=1000;
  const referenceSlots=referenceSlotsByTimestamp(referenceTimestamps);
  const prepareGeometry=(currentPixelHeight, currentData) => {
    let candleSeries, hasScaleValue, scaleLow, scaleHigh, p;
    const preparedTraces=mapi((_7, _8) => {
      const m_2=_8.Kind;
      switch(m_2.$==1?0:m_2.$==2?1:m_2.$==3?1:m_2.$==4?2:m_2.$==5?2:0){
        case 0:
          return[_7, _8, candleSeriesForTracePrepared(_8, currentData), []];
        case 1:
          return[_7, _8, [], lineSeriesPrepared(_8.DataRef, currentData)];
        case 2:
          return[_7, _8, [], []];
      }
    }, traces);
    const candleTargets=new Dictionary("New_5");
    for(let i=0, _7=preparedTraces.length-1;i<=_7;i++){
      const f=get(preparedTraces, i);
      const trace=f[1];
      if(Equals(trace.Kind, {$:0}))candleTargets.set_Item(trace.TraceId, candlePointsByTimestamp(f[2]));
    }
    if(length(referenceTimestamps)<=maximumVisualPoints){
      const projected=MarkResizable([]);
      for(let i_1=0, _8=preparedTraces.length-1;i_1<=_8;i_1++){
        const f_1=get(preparedTraces, i_1);
        const trace_1=f_1[1];
        const candles=f_1[2];
        if(Equals(trace_1.Kind, {$:0})){
          for(let i_2=0, _9=candles.length-1;i_2<=_9;i_2++){
            const point=get(candles, i_2);
            const m=candleSlotRange(referenceTimestamps, point);
            if(m==null){ }
            else {
              const lastExclusive=m.$0[1];
              const first=m.$0[0];
              for(let slotIndex=first, _10=lastExclusive-1;slotIndex<=_10;slotIndex++)projected.push([f_1[0], trace_1, slotIndex, lastExclusive-first, point]);
            }
          }
        }
      }
      candleSeries=projected.slice();
    }
    else {
      const projectionKinds=2;
      const accumulatorCount=length(traces)*projectionKinds*maximumVisualPoints;
      const firstSlots=create(accumulatorCount, 2147483647);
      const lastSlots=create(accumulatorCount, -1);
      const sourceSpans=create(accumulatorCount, 0);
      const opens=create(accumulatorCount, 0);
      const highs=create(accumulatorCount, 0);
      const lows=create(accumulatorCount, 0);
      const closes=create(accumulatorCount, 0);
      const volumes=create(accumulatorCount, 0);
      const lastPoints=create(accumulatorCount, null);
      const accumulatorIndex=(traceIndex, projected_1, bucketIndex) =>(traceIndex*projectionKinds+projected_1)*maximumVisualPoints+bucketIndex;
      for(let i_3=0, _11=preparedTraces.length-1;i_3<=_11;i_3++){
        const f_2=get(preparedTraces, i_3);
        const candles_1=f_2[2];
        if(Equals(f_2[1].Kind, {$:0})){
          for(let i_4=0, _12=candles_1.length-1;i_4<=_12;i_4++){
            const point_1=get(candles_1, i_4);
            const m_1=candleSlotRange(referenceTimestamps, point_1);
            if(m_1==null){ }
            else {
              const lastExclusive_1=m_1.$0[1];
              const first_1=m_1.$0[0];
              const sourceSpanCount=lastExclusive_1-first_1;
              for(let slotIndex_1=first_1, _13=lastExclusive_1-1;slotIndex_1<=_13;slotIndex_1++){
                let _2;
                const a=maximumVisualPoints-1;
                const b=slotIndex_1*maximumVisualPoints/length(referenceTimestamps)>>0;
                let _3=Compare(a, b)===-1?a:b;
                const index=accumulatorIndex(f_2[0], sourceSpanCount>1?1:0, _3);
                if(slotIndex_1<get(firstSlots, index)){
                  set(firstSlots, index, slotIndex_1);
                  set(opens, index, point_1.Open);
                }
                if(slotIndex_1>=get(lastSlots, index)){
                  set(lastSlots, index, slotIndex_1);
                  set(closes, index, point_1.Close);
                  set(lastPoints, index, Some(point_1));
                }
                if(get(sourceSpans, index)===0)_2=(set(highs, index, point_1.High),set(lows, index, point_1.Low));
                else {
                  const a_1=get(highs, index);
                  const b_1=point_1.High;
                  let _4=Compare(a_1, b_1)===1?a_1:b_1;
                  set(highs, index, _4);
                  const a_2=get(lows, index);
                  const b_2=point_1.Low;
                  let _5=Compare(a_2, b_2)===-1?a_2:b_2;
                  _2=set(lows, index, _5);
                }
                const a_3=get(sourceSpans, index);
                let _6=Compare(a_3, sourceSpanCount)===1?a_3:sourceSpanCount;
                set(sourceSpans, index, _6);
                set(volumes, index, get(volumes, index)+point_1.Volume);
              }
            }
          }
        }
      }
      candleSeries=ofSeq(delay(() => collect_1((traceIndex) => collect_1((projected_1) => collect_1((bucketIndex) => {
        const index_1=accumulatorIndex(traceIndex, projected_1, bucketIndex);
        const m_2=get(lastPoints, index_1);
        if(m_2==null)return[];
        else {
          const lastPoint=m_2.$0;
          return[[traceIndex, get(traces, traceIndex), (get(firstSlots, index_1)+get(lastSlots, index_1))/2>>0, get(sourceSpans, index_1), {
            Timestamp:lastPoint.Timestamp, 
            Open:get(opens, index_1), 
            High:get(highs, index_1), 
            Low:get(lows, index_1), 
            Close:get(closes, index_1), 
            Volume:get(volumes, index_1), 
            Temporal:lastPoint.Temporal
          }]];
        }
      }, range(0, maximumVisualPoints-1)), range(0, projectionKinds-1)), range(0, length(traces)-1))));
    }
    const projectedLinePoints_1=map((_14) => {
      const trace_2=_14[1];
      const lines=_14[3];
      const m_2=trace_2.Kind;
      let _15=m_2.$==1?map((point_3) =>({
        Timestamp:point_3.Timestamp, 
        Value:point_3.Volume, 
        Temporal:point_3.Temporal
      }), _14[2]):m_2.$==2?lines:m_2.$==3?lines:[];
      let _16=projectedLinePoints(referenceTimestamps, _15);
      return[_14[0], trace_2, _16];
    }, preparedTraces);
    const linePoints=map((_14) =>[_14[0], _14[1], compactProjectedLinePoints(maximumVisualPoints, length(referenceTimestamps), _14[2])], projectedLinePoints_1);
    const markerPlacements=assignAggregateMarkerLanes(collect((_14) => {
      const trace_2=_14[1];
      if(!Equals(trace_2.Kind, {$:4}))return[];
      else {
        const m_2=tryDecode_3(trace_2.Options);
        if(m_2!=null&&m_2.$==1){
          const options=m_2.$0;
          const o=tryFind((candidate) => candidate.TraceId==options.TargetTraceId&&Equals(candidate.Kind, {$:0}), traces);
          const o_1=o==null?null:((target) => {
            let o_2;
            const m_3=(o_2=null,[candleTargets.TryGetValue(target.TraceId, {get:() => o_2, set:(v) => {
              o_2=v;
            }}), o_2]);
            return m_3[0]?Some(markerPlacementsPreparedWithIndexes(trace_2, target.TraceId, m_3[1], currentData, referenceSlots)):null;
          })(o.$0);
          return o_1==null?[]:o_1.$0;
        }
        else return[];
      }
    }, preparedTraces));
    const overviewStripePlacements=collect((_14) => {
      const trace_2=_14[1];
      return Equals(trace_2.Kind, {$:5})?overviewStripePlacementsPrepared(trace_2, currentData, referenceTimestamps):[];
    }, preparedTraces);
    hasScaleValue=false;
    scaleLow=0;
    scaleHigh=0;
    const includeScaleValue=(value) => {
      scaleHigh=hasScaleValue?(scaleLow=Compare(scaleLow, value)===-1?scaleLow:value,Compare(scaleHigh, value)===1?scaleHigh:value):(hasScaleValue=true,scaleLow=value,value);
    };
    for(let i_5=0, _14=candleSeries.length-1;i_5<=_14;i_5++){
      const point_2=(get(candleSeries, i_5))[4];
      includeScaleValue(point_2.Low);
      includeScaleValue(point_2.High);
    }
    for(let i_6=0, _15=projectedLinePoints_1.length-1;i_6<=_15;i_6++){
      const f_3=get(projectedLinePoints_1, i_6);
      const points=f_3[2];
      if(Equals(f_3[1].Kind, {$:3}))includeScaleValue(0);
      for(let i_7=0, _16=points.length-1;i_7<=_16;i_7++)includeScaleValue((get(points, i_7))[1].Value);
    }
    const bounds=!hasScaleValue?null:Some([scaleLow, scaleHigh]);
    if(hasCandles)p=paddedBoundsForCssPixels(0, 1, height, plotHeight, currentPixelHeight, 15, bounds);
    else if(!hasScaleValue)p=[0, 1];
    else if(scaleLow===scaleHigh)p=[scaleLow-1, scaleHigh+1];
    else {
      const a_4=(scaleHigh-scaleLow)*0.08;
      const b_3=0.0001;
      const padding=Compare(a_4, b_3)===1?a_4:b_3;
      p=[scaleLow-padding, scaleHigh+padding];
    }
    const readers=map((_17) => {
      const traceIndex=_17[0];
      const trace_2=_17[1];
      const label=IsNullOrWhiteSpace(trace_2.Label)?trace_2.TraceId:trace_2.Label;
      const m_2=trace_2.CandleDataRefs;
      let _18=m_2==null?trace_2.DataRef:m_2.$0.OpenRef;
      const sourceTimestamps=projectedLastSourceTimestampWhere((point_3) => {
        const m_4=trace_2.Kind;
        switch(m_4.$==1?0:m_4.$==2?1:m_4.$==3?1:m_4.$==4?2:m_4.$==5?2:0){
          case 0:
            return parseCandleResolved(point_3.Temporal, point_3.Payload)==null;
          case 1:
            return parseLineResolved(point_3.Temporal, point_3.Payload)==null;
          case 2:
            return false;
        }
      }, referenceTimestamps, _18, currentData);
      const unavailablePresentation=(index_2) => {
        const o_1=tryItem(index_2, sourceTimestamps);
        const o_2=o_1==null?null:o_1.$0;
        return o_2==null?null:Some({Timestamp:o_2.$0, Value:"Unavailable"});
      };
      const m_3=trace_2.Kind;
      switch(m_3.$==1?0:m_3.$==2?1:m_3.$==3?1:m_3.$==4?2:m_3.$==5?2:0){
        case 0:
          const values=projectedCandleCursorValues(isBaseRow, referenceTimestamps, _17[2]);
          const legendReader=(index_2) => {
            let o_1;
            const o_2=tryItem(index_2, values);
            const o_3=o_2==null?null:o_2.$0;
            if(o_3==null)o_1=null;
            else {
              const point_3=o_3.$0;
              let _21={Timestamp:point_3.Timestamp, Value:Equals(trace_2.Kind, {$:1})?fixedText(point_3.Volume):"O "+fixedText(point_3.Open)+" H "+fixedText(point_3.High)+" L "+fixedText(point_3.Low)+" C "+fixedText(point_3.Close)+" V "+fixedText(point_3.Volume)};
              o_1=Some(_21);
            }
            return o_1==null?unavailablePresentation(index_2):(o_1.$0,o_1);
          };
          return[(index_2) => {
            const o_1=tryItem(index_2, values);
            const o_2=o_1==null?null:o_1.$0;
            return o_2==null?null:Some(candleCursorPointValue(label, trace_2.Kind, o_2.$0));
          }, legendReader, tryReadAtOrBefore(legendReader, length(referenceTimestamps)-1)];
        case 1:
          let _19;
          const values_1=create(length(referenceTimestamps), null);
          const o=tryFind((_21) => _21[0]===traceIndex, projectedLinePoints_1);
          if(o==null)_19=null;
          else {
            o.$0[0];
            o.$0[1];
            const _20=o.$0[2];
            for(let i_8=0, _21=_20.length-1;i_8<=_21;i_8++){
              const f_4=get(_20, i_8);
              const index_1=f_4[0];
              if(index_1>=0&&index_1<length(values_1))set(values_1, index_1, Some(f_4[1]));
            }
            _19=void 0;
          }
          const legendReader_1=(index_2) => {
            let o_1;
            const o_2=tryItem(index_2, values_1);
            const o_3=o_2==null?null:o_2.$0;
            if(o_3==null)o_1=null;
            else {
              const point_3=o_3.$0;
              let _22={Timestamp:point_3.Timestamp, Value:fixedText(point_3.Value)};
              o_1=Some(_22);
            }
            return o_1==null?unavailablePresentation(index_2):(o_1.$0,o_1);
          };
          return[(index_2) => {
            const o_1=tryItem(index_2, values_1);
            const o_2=o_1==null?null:o_1.$0;
            return o_2==null?null:Some(lineCursorPointValue(label, o_2.$0));
          }, legendReader_1, tryReadAtOrBefore(legendReader_1, length(referenceTimestamps)-1)];
        case 2:
          return[() => null, () => null, null];
      }
    }, preparedTraces);
    return[preparedTraces, candleSeries, linePoints, markerPlacements, overviewStripePlacements, map((_17) => _17[0], readers), map((_17) => _17[1], readers), map((_17) => _17[2], readers), p[0], p[1]];
  };
  const initialGeometry=prepareGeometry(chartPixelHeight.Get(), preparedData);
  const initialLinePoints=initialGeometry[2];
  const markerVisualState=_c_2.Create_1([initialGeometry[3], initialGeometry[4], initialGeometry[8], initialGeometry[9]]);
  const readerStates=map3((_2, _3, _4) =>[[_2, _3, _4]], initialGeometry[5], initialGeometry[6], initialGeometry[7]);
  const slot=length(referenceTimestamps)===0?width:width/length(referenceTimestamps);
  const svgTestId=hasCandles?"ta-candle-"+rowId:"ta-composite-"+rowId;
  const candlePaths=(traceIndex, t) => {
    let _2;
    const low=t[8];
    const high=t[9];
    const buckets=init(8, () => MarkResizable([]));
    const e=Get(t[1]);
    try {
      while(e.MoveNext())
        {
          const f=e.Current;
          const point=f[4];
          if(Equals(f[0], traceIndex)){
            const wickIndex=(f[3]>1?4:0)+(point.Close>=point.Open?0:2);
            const center=xAt(f[2]);
            const a=2;
            const b=slot*0.64;
            const bodyWidth=Compare(a, b)===1?a:b;
            const openY=normalize(low, high, top, plotHeight, point.Open);
            const closeY=normalize(low, high, top, plotHeight, point.Close);
            get(buckets, wickIndex).push("M "+String(fixedText(center))+" "+String(fixedText(normalize(low, high, top, plotHeight, point.High)))+" L "+String(fixedText(center))+" "+String(fixedText(normalize(low, high, top, plotHeight, point.Low))));
            let _3=get(buckets, wickIndex+1);
            let _4=Compare(openY, closeY)===-1?openY:closeY;
            const a_1=1.2;
            const b_1=Math.abs(closeY-openY);
            let _5=Compare(a_1, b_1)===1?a_1:b_1;
            let _6=rectanglePath(center-bodyWidth/2, _4, bodyWidth, _5);
            _3.push(_6);
          }
          else void 0;
        }
      _2=void 0;
    }
    finally {
      const _7=e;
      if(typeof _7=="object"&&isIDisposable(_7))e.Dispose();
    }
    return map((s) => concat_1(" ", s), buckets);
  };
  const lineGeometry=(traceIndex, t) => {
    const o=tryFind((_4) => _4[0]===traceIndex, t[2]);
    let _2=o==null?find((_4) => _4[0]===traceIndex, initialLinePoints):o.$0;
    let _3=_2[2];
    return[_3, t[8], t[9]];
  };
  const linePaths=(traceIndex, trace, geometry) => {
    const p=lineGeometry(traceIndex, geometry);
    const points=p[0];
    const low=p[1];
    const high=p[2];
    const m=trace.Kind;
    if(m.$==3){
      const zeroY=normalize(low, high, top, plotHeight, 0);
      const a=1;
      const b=slot*0.64;
      const barWidth=Compare(a, b)===1?a:b;
      const path=(predicate) => concat_1(" ", choose((_2) => {
        const point=_2[1];
        if(predicate(point.Value)){
          const valueY=normalize(low, high, top, plotHeight, point.Value);
          let _3=Compare(zeroY, valueY)===-1?zeroY:valueY;
          const a_2=1;
          const b_2=Math.abs(zeroY-valueY);
          let _4=Compare(a_2, b_2)===1?a_2:b_2;
          let _5=rectanglePath(slot*(_2[0]+0.18), _3, barWidth, _4);
          return Some(_5);
        }
        else return null;
      }, points));
      return[path((value) => value>=0), path((value) => value<0)];
    }
    else if(m.$==1){
      const zeroY_1=normalize(low, high, top, plotHeight, 0);
      const a_1=1;
      const b_1=slot*0.64;
      const barWidth_1=Compare(a_1, b_1)===1?a_1:b_1;
      return[concat_1(" ", map((_2) => {
        const valueY=normalize(low, high, top, plotHeight, _2[1].Value);
        let _3=Compare(zeroY_1, valueY)===-1?zeroY_1:valueY;
        const a_2=1;
        const b_2=Math.abs(zeroY_1-valueY);
        let _4=Compare(a_2, b_2)===1?a_2:b_2;
        return rectanglePath(slot*(_2[0]+0.18), _3, barWidth_1, _4);
      }, points)), ""];
    }
    else return m.$==2?[concat_1(" ", mapi((_2, _3) =>(_2===0?"M":"L")+" "+fixedText(_3[0])+" "+fixedText(_3[1]), map((_2) =>[xAt(_2[0]), normalize(low, high, top, plotHeight, _2[1].Value)], points))), ""]:["", ""];
  };
  const lineLastValue=(traceIndex, geometry) => {
    const o=tryLast((lineGeometry(traceIndex, geometry))[0]);
    const o_1=o==null?null:Some(fixedText(o.$0[1].Value));
    return o_1==null?"":o_1.$0;
  };
  const markerClusterSelection=_c_2.Create_1(null);
  const markerClusterPosition=(low, high, cluster) => {
    const size=9;
    const half=size/2;
    const laneStep=size+2;
    const target=get(cluster.Markers, 0).Target;
    const anchorY=cluster.Anchor.$==1?normalize(low, high, top, plotHeight, target.Low):normalize(low, high, top, plotHeight, target.High);
    let _2=xAt(cluster.SlotIndex);
    const a=8;
    const a_1=height-8;
    const b=cluster.Anchor.$==1?anchorY+4+half+cluster.Lane*laneStep:anchorY-4-half-cluster.Lane*laneStep;
    const b_1=Compare(a_1, b)===-1?a_1:b;
    let _3=Compare(a, b_1)===1?a:b_1;
    return[_2, _3];
  };
  const markerLayer=Doc.EmbedView(Map((_2) => {
    const a=_2[0];
    const zone=_2[1];
    const placements=a[0];
    const low=a[2];
    const high=a[3];
    const p=markerPresentation(placements);
    const overflowClusters=p[1];
    const directPlacements=p[0];
    return svgElement("g", [Attr.Create("data-testid", "ta-marker-layer-"+rowId), Attr.Create("data-marker-count", String(length(placements))), Attr.Create("data-direct-marker-count", String(length(directPlacements))), Attr.Create("data-marker-overflow-count", String(length(overflowClusters)))], ofSeq_1(delay(() => append_2(map_2((placement) => {
      let p_1;
      const size=9;
      const half=size/2;
      const size_1=9;
      const half_1=size_1/2;
      const laneStep=size_1+2;
      let _3=xAt(placement.SlotIndex);
      const a_1=height-half_1;
      const b=placement.Marker.Anchor.$==1?(placement.Marker.Anchor.$==1?normalize(low, high, top, plotHeight, placement.Target.Low):normalize(low, high, top, plotHeight, placement.Target.High))+4+half_1+placement.Lane*laneStep:(placement.Marker.Anchor.$==1?normalize(low, high, top, plotHeight, placement.Target.Low):normalize(low, high, top, plotHeight, placement.Target.High))-4-half_1-placement.Lane*laneStep;
      const b_1=Compare(a_1, b)===-1?a_1:b;
      let _4=Compare(half_1, b_1)===1?half_1:b_1;
      const p_2=[_3, _4];
      const y=p_2[1];
      const x=p_2[0];
      const p_3=placement.Marker.Fill.$==1?["none", "1"]:[placement.Marker.Color, "1"];
      const common=ofArray([Attr.Create("data-testid", "ta-marker-"+placement.TraceId+"-"+placement.Marker.MarkerId), Attr.Create("data-marker-id", placement.Marker.MarkerId), Attr.Create("data-marker-position", fixedText(placement.Position)), Attr.Create("data-marker-slot", String(placement.SlotIndex)), Attr.Create("data-marker-event-time", placement.Marker.EventTimeUtc), Attr.Create("data-marker-lane", String(placement.Lane)), Attr.Create("data-marker-anchor", Equals(placement.Marker.Anchor, {$:0})?"above-bar":"below-bar"), Attr.Create("data-marker-shape", shapeText(placement.Marker.Shape)), Attr.Create("data-marker-fill", fillText(placement.Marker.Fill)), svgAttr("fill", p_3[0]), svgAttr("fill-opacity", p_3[1]), svgAttr("stroke", placement.Marker.Color), svgAttr("stroke-width", "1.4"), svgAttr("pointer-events", "all"), svgAttr("vector-effect", "non-scaling-stroke"), Handler("mousemove", () =>(event) => {
        event.stopPropagation();
        return setCursorIndex(Some(placement.SlotIndex));
      }), Handler("click", () =>(event) => {
        event.stopPropagation();
        return commitCursorIndex(placement.SlotIndex);
      })]);
      const title=svgElement("title", [], [Doc.TextNode(markerTooltipTextWith((c) => fullOrOriginal(zone, c), placement))]);
      const m=placement.Marker.Shape;
      switch(m.$==3?1:m.$==4?2:m.$==0?3:m.$==1?3:0){
        case 0:
          p_1=["circle", ofArray([svgAttr("cx", fixedText(x)), svgAttr("cy", fixedText(y)), svgAttr("r", fixedText(half))])];
          break;
        case 1:
          p_1=["rect", ofArray([svgAttr("x", fixedText(x-half)), svgAttr("y", fixedText(y-half)), svgAttr("width", fixedText(size)), svgAttr("height", fixedText(size))])];
          break;
        case 2:
          p_1=["polygon", ofArray([svgAttr("points", String(fixedText(x))+","+String(fixedText(y-half))+" "+String(fixedText(x+half))+","+String(fixedText(y))+" "+String(fixedText(x))+","+String(fixedText(y+half))+" "+String(fixedText(x-half))+","+String(fixedText(y)))])];
          break;
        case 3:
          const o=markerTrianglePoints(placement.Marker.Shape, x, y, half);
          let _5=o==null?[]:o.$0;
          let _6=map((_10) => fixedText(_10[0])+","+fixedText(_10[1]), _5);
          let _7=concat_1(" ", _6);
          let _8=[svgAttr("points", _7)];
          let _9=ofArray(_8);
          p_1=["polygon", _9];
          break;
      }
      const geometry=p_1[1];
      const elementName=p_1[0];
      const halo=svgElement(elementName, append_1(ofArray([Attr.Create("data-marker-contrast-halo", "true"), Attr.Create("data-marker-halo-for", placement.Marker.MarkerId), svgAttr("fill", "none"), svgAttr("stroke", palette.ThemeName=="dark"?"#f8fafc":"#0f172a"), svgAttr("stroke-width", "4.4"), svgAttr("stroke-opacity", "0.95"), svgAttr("pointer-events", "none"), svgAttr("vector-effect", "non-scaling-stroke")]), geometry), []);
      const semanticShape=svgElement(elementName, append_1(common, geometry), [title]);
      return svgElement("g", [Attr.Create("data-marker-visual", placement.Marker.MarkerId)], [halo, semanticShape]);
    }, directPlacements), delay(() => append_2(collect_1((cluster) => {
      const p_1=markerClusterPosition(low, high, cluster);
      const y=p_1[1];
      const x=p_1[0];
      const hiddenCount=length(cluster.Markers);
      const selectCluster=(index) => {
        const a_1=0;
        const a_2=hiddenCount-1;
        const b=Compare(a_2, index)===-1?a_2:index;
        let _3=Compare(a_1, b)===1?a_1:b;
        let _4=[cluster.ClusterId, _3];
        let _5=Some(_4);
        markerClusterSelection.Set(_5);
      };
      const toggleCluster=() => {
        let _3;
        const m=markerClusterSelection.Get();
        if(m!=null&&m.$==1&&(m.$0[0]==cluster.ClusterId&&(_3=m.$0[0],true)))markerClusterSelection.Set(null);
        else selectCluster(0);
      };
      return[svgElement("g", [Attr.Create("data-testid", "ta-marker-overflow-"+cluster.ClusterId), Attr.Create("data-marker-overflow-count", String(hiddenCount)), Attr.Create("role", "button"), Attr.Create("tabindex", "0"), Attr.Create("aria-label", String(hiddenCount)+" additional markers. Activate to inspect."), svgAttr("style", "cursor:pointer;"), Handler("mousemove", () =>(event) => {
        event.stopPropagation();
        return setCursorIndex(Some(cluster.SlotIndex));
      }), Handler("click", () =>(event) => {
        event.stopPropagation();
        commitCursorIndex(cluster.SlotIndex);
        return toggleCluster();
      }), Handler("keydown", () =>(event) => {
        const m=event.key;
        switch(m){
          case" ":
          case"Enter":
            event.preventDefault();
            return toggleCluster();
          case"ArrowRight":
          case"ArrowDown":
            let _3;
            event.preventDefault();
            const m_1=markerClusterSelection.Get();
            return m_1!=null&&m_1.$==1&&(m_1.$0,m_1.$0[0]==cluster.ClusterId&&(_3=[m_1.$0[0], m_1.$0[1]],true))?selectCluster(_3[1]+1):selectCluster(0);
          case"ArrowLeft":
          case"ArrowUp":
            let _4;
            event.preventDefault();
            const m_2=markerClusterSelection.Get();
            return m_2!=null&&m_2.$==1&&(m_2.$0,m_2.$0[0]==cluster.ClusterId&&(_4=[m_2.$0[0], m_2.$0[1]],true))?selectCluster(_4[1]-1):selectCluster(hiddenCount-1);
          case"Escape":
            event.preventDefault();
            return markerClusterSelection.Set(null);
          default:
            return null;
        }
      })], [svgElement("rect", [svgAttr("x", fixedText(x-10)), svgAttr("y", fixedText(y-7)), svgAttr("width", "20"), svgAttr("height", "14"), svgAttr("rx", "3"), svgAttr("fill", palette.TooltipSurface), svgAttr("stroke", palette.TooltipBorder), svgAttr("stroke-width", "1.2"), svgAttr("vector-effect", "non-scaling-stroke")], []), svgElement("text", [svgAttr("x", fixedText(x)), svgAttr("y", fixedText(y+0.5)), svgAttr("text-anchor", "middle"), svgAttr("dominant-baseline", "middle"), svgAttr("font-family", "Consolas,monospace"), svgAttr("font-size", "8"), svgAttr("font-weight", "700"), svgAttr("fill", palette.TooltipText), svgAttr("pointer-events", "none")], [Doc.TextNode("+"+String(hiddenCount))]), svgElement("title", [], [Doc.TextNode(String(hiddenCount)+" additional markers")])])];
    }, overflowClusters), delay(() =>[Doc.EmbedView(Map((a_1) => {
      if(a_1!=null&&a_1.$==1){
        const selectedIndex=a_1.$0[1];
        const clusterId=a_1.$0[0];
        const m=tryFind((cluster_1) => cluster_1.ClusterId==clusterId, overflowClusters);
        if(m!=null&&m.$==1){
          const cluster=m.$0;
          const a_2=0;
          const a_3=length(cluster.Markers)-1;
          const b=Compare(a_3, selectedIndex)===-1?a_3:selectedIndex;
          const index=Compare(a_2, b)===1?a_2:b;
          const p_1=markerClusterPosition(low, high, cluster);
          const textValue=markerTooltipTextWith((c) => fullOrOriginal(zone, c), get(cluster.Markers, index));
          const bounded=textValue.length<=160?textValue:Substring(textValue, 0, 157)+"...";
          const a_4=6;
          const a_5=684;
          const b_1=p_1[0]+12;
          const b_2=Compare(a_5, b_1)===-1?a_5:b_1;
          const boxX=Compare(a_4, b_2)===1?a_4:b_2;
          const a_6=4;
          const a_7=height-28;
          const b_3=p_1[1]-12;
          const b_4=Compare(a_7, b_3)===-1?a_7:b_3;
          const boxY=Compare(a_6, b_4)===1?a_6:b_4;
          return svgElement("g", [Attr.Create("data-testid", "ta-marker-overflow-detail"), Attr.Create("data-cluster-id", cluster.ClusterId), Attr.Create("data-cluster-selected-index", String(index)), Attr.Create("aria-live", "polite"), svgAttr("pointer-events", "none")], [svgElement("rect", [svgAttr("x", fixedText(boxX)), svgAttr("y", fixedText(boxY)), svgAttr("width", "304"), svgAttr("height", "24"), svgAttr("rx", "3"), svgAttr("fill", palette.TooltipSurface), svgAttr("fill-opacity", "0.97"), svgAttr("stroke", palette.TooltipBorder), svgAttr("stroke-width", "0.8")], []), svgElement("text", [svgAttr("x", fixedText(boxX+6)), svgAttr("y", fixedText(boxY+15)), svgAttr("fill", palette.TooltipText), svgAttr("font-family", "Consolas,monospace"), svgAttr("font-size", "9")], [Doc.TextNode(String(index+1)+"/"+String(length(cluster.Markers))+" "+String(bounded))])]);
        }
        else return Doc.Empty;
      }
      else return Doc.Empty;
    }, markerClusterSelection.View))])))))));
  }, Map2((_2, _3) =>[_2, _3], markerVisualState.View, displayTime.Zone)));
  const candlePathStates=map((_2) => {
    const traceIndex=_2[0];
    return[traceIndex, _2[1], map(_c_2.Create_1, candlePaths(traceIndex, [initialGeometry[0], initialGeometry[1], initialGeometry[2], initialGeometry[3], initialGeometry[4], initialGeometry[5], initialGeometry[6], initialGeometry[7], initialGeometry[8], initialGeometry[9]]))];
  }, distinctBy((t) => t[0], map((_2) =>[_2[0], _2[1]], initialGeometry[1])));
  const lineVisualStates=map((_2) => {
    const traceIndex=_2[0];
    const trace=_2[1];
    const p=linePaths(traceIndex, trace, initialGeometry);
    return[traceIndex, trace, _c_2.Create_1(p[0]), _c_2.Create_1(p[1]), _c_2.Create_1(lineLastValue(traceIndex, initialGeometry))];
  }, initialLinePoints);
  observedPreparedData=preparedData;
  observedChartPixelHeight=chartPixelHeight.Get();
  Sink((_2) => {
    const currentData=_2[0];
    const currentPixelHeight=_2[1];
    if(!(currentData===observedPreparedData)||currentPixelHeight!==observedChartPixelHeight){
      observedPreparedData=currentData;
      observedChartPixelHeight=currentPixelHeight;
      const geometry=prepareGeometry(currentPixelHeight, currentData);
      const nextMarkerVisual=[geometry[3], geometry[4], geometry[8], geometry[9]];
      if(!Equals(markerVisualState.Get(), nextMarkerVisual))markerVisualState.Set(nextMarkerVisual);
      for(let index=0, _3=length(readerStates)-1;index<=_3;index++)(get(readerStates, index))[0]=[get(geometry[5], index), get(geometry[6], index), get(geometry[7], index)];
      for(let i=0, _4=candlePathStates.length-1;i<=_4;i++)((() => {
        const f_1=get(candlePathStates, i);
        const pathStates=f_1[2];
        const nextPaths=candlePaths(f_1[0], [geometry[0], geometry[1], geometry[2], geometry[3], geometry[4], geometry[5], geometry[6], geometry[7], geometry[8], geometry[9]]);
        for(let index_1=0, _5=length(pathStates)-1;index_1<=_5;index_1++)if(get(pathStates, index_1).Get()!=get(nextPaths, index_1))get(pathStates, index_1).Set(get(nextPaths, index_1));
      })());
      for(let i_1=0, _5=lineVisualStates.length-1;i_1<=_5;i_1++){
        const f=get(lineVisualStates, i_1);
        const traceIndex=f[0];
        const positivePathState=f[2];
        const negativePathState=f[3];
        const lastValueState=f[4];
        const p=linePaths(traceIndex, f[1], geometry);
        const nextPositivePath=p[0];
        const nextNegativePath=p[1];
        const nextLastValue=lineLastValue(traceIndex, geometry);
        if(positivePathState.Get()!=nextPositivePath)positivePathState.Set(nextPositivePath);
        if(negativePathState.Get()!=nextNegativePath)negativePathState.Set(nextNegativePath);
        if(lastValueState.Get()!=nextLastValue)lastValueState.Set(nextLastValue);
      }
      scheduleValueRefresh();
    }
  }, Map2((_2, _3) =>[_2, _3], dataView, chartPixelHeight.View));
  return[svgElement("svg", [svgAttr("viewBox", "0 0 1000 "+fixedText(height)), svgAttr("preserveAspectRatio", "none"), svgAttr("role", "img"), svgAttr("aria-label", "Composite TA row "+rowId), Attr.Create("data-testid", svgTestId), Attr.Create("data-plot-surface-theme", palette.ThemeName), Attr.Create("data-point-count", String(length(referenceTimestamps))), Dynamic_1("style", Map((value) =>"display:block; width:100%; height:"+String(value)+"px; background:"+palette.Surface+";", chartPixelHeight.View)), Handler("mousemove", (element_2) =>(event) => {
    const bounds=element_2.getBoundingClientRect();
    const m=cursorIndexFromClientX(length(referenceTimestamps), bounds.left, bounds.width, event.clientX);
    return m==null?null:setCursorIndex(Some(m.$0));
  }), Handler("click", (element_2) =>(event) => {
    const bounds=element_2.getBoundingClientRect();
    const m=cursorIndexFromClientX(length(referenceTimestamps), bounds.left, bounds.width, event.clientX);
    return m==null?null:commitCursorIndex(m.$0);
  })], ofSeq_1(delay(() => append_2(collect_1((gridIndex) => {
    const y=top+plotHeight*gridIndex/4;
    return[svgElement("line", [svgAttr("x1", "0"), svgAttr("x2", "1000"), svgAttr("y1", fixedText(y)), svgAttr("y2", fixedText(y)), svgAttr("stroke", palette.Grid), svgAttr("stroke-width", "1")], [])];
  }, range(0, 4)), delay(() => append_2(collect_1((m) => {
    const trace=m[1];
    const pathStates=m[2];
    const traceTestId="ta-candle-"+rowId+"-"+trace.TraceId;
    const projectedColor=color(m[0], trace);
    const styles=[["normal-up-wick", "none", "#0f8a78", "1.2", "1"], ["normal-up-body", "#0f8a78", "none", "0", "1"], ["normal-down-wick", "none", "#c2414b", "1.2", "1"], ["normal-down-body", "#c2414b", "none", "0", "1"], ["projected-up-wick", "none", projectedColor, "1.2", "1"], ["projected-up-body", "#0f8a78", projectedColor, "1", "0.48"], ["projected-down-wick", "none", projectedColor, "1.2", "1"], ["projected-down-body", "#c2414b", projectedColor, "1", "0.48"]];
    return collect_1((index) => {
      const p=get(styles, index);
      return[svgElement("path", [Attr.Create("data-testid", traceTestId), Attr.Create("data-candle-part", p[0]), Attr.Create("data-candle-batched", "true"), Dynamic_1("d", get(pathStates, index).View), svgAttr("fill", p[1]), svgAttr("fill-opacity", p[4]), svgAttr("stroke", p[2]), svgAttr("stroke-width", p[3]), svgAttr("vector-effect", "non-scaling-stroke")], [])];
    }, range(0, length(styles)-1));
  }, candlePathStates), delay(() => append_2(collect_1((m) => {
    const traceIndex=m[0];
    const trace=m[1];
    const traceColor=color(traceIndex, trace);
    const p=find((_3) => _3[0]===traceIndex, lineVisualStates);
    const positivePath=p[2].View;
    const negativePath=p[3].View;
    const lastValue=p[4].View;
    const m_1=trace.Kind;
    if(m_1.$==3){
      const v={PositiveColor:traceColor, NegativeColor:traceColor};
      const o=tryDecode_4(trace.Options);
      const histogramStyle=o==null?v:o.$0;
      const traceTestId="ta-trace-"+rowId+"-"+trace.TraceId;
      return append_2([svgElement("path", [Attr.Create("data-testid", traceTestId), Attr.Create("data-histogram-polarity", "positive"), Dynamic_1("d", positivePath), Dynamic_1("data-last-value", lastValue), svgAttr("fill", histogramStyle.PositiveColor), svgAttr("fill-opacity", "0.74")], [])], delay(() =>[svgElement("path", [Attr.Create("data-testid", traceTestId+"-negative"), Attr.Create("data-histogram-polarity", "negative"), Dynamic_1("d", negativePath), svgAttr("fill", histogramStyle.NegativeColor), svgAttr("fill-opacity", "0.74")], [])]));
    }
    else if(m_1.$==1)return[svgElement("path", [Attr.Create("data-testid", "ta-trace-"+rowId+"-"+trace.TraceId), Dynamic_1("d", positivePath), Dynamic_1("data-last-value", lastValue), svgAttr("fill", traceColor), svgAttr("fill-opacity", "0.62")], [])];
    else if(m_1.$==2){
      const a=1;
      const a_1=2;
      const b=trace.Width;
      const b_1=Compare(a_1, b)===-1?a_1:b;
      let _2=Compare(a, b_1)===1?a:b_1;
      const strokeWidthCssPixels=fixedText(_2);
      return[svgElement("path", [Attr.Create("data-testid", "ta-trace-"+rowId+"-"+trace.TraceId), Attr.Create("data-stroke-width-css-pixels", strokeWidthCssPixels), Dynamic_1("d", positivePath), Dynamic_1("data-last-value", lastValue), svgAttr("fill", "none"), svgAttr("stroke", traceColor), svgAttr("stroke-width", strokeWidthCssPixels), svgAttr("stroke-linejoin", "round"), svgAttr("stroke-linecap", "round"), svgAttr("vector-effect", "non-scaling-stroke")], [])];
    }
    else return[];
  }, initialLinePoints), delay(() => append_2([markerLayer], delay(() =>[svgElement("line", [Attr.Create("data-testid", svgTestId+"-crosshair"), Attr.Create("data-ta-shared-crosshair", "true"), svgAttr("x1", "0"), svgAttr("x2", "0"), svgAttr("visibility", "hidden"), svgAttr("y1", "0"), svgAttr("y2", fixedText(height)), svgAttr("stroke", palette.Cursor), svgAttr("stroke-width", "1"), svgAttr("stroke-dasharray", "3 3"), svgAttr("pointer-events", "none")], [])]))))))))))), referenceTimestamps, mapi((_2) =>(_3) =>(get(readerStates, _2))[0][0](_3), traces), mapi((_2) =>(_3) =>(get(readerStates, _2))[0][1](_3), traces), mapi((_2) =>() =>(get(readerStates, _2))[0][2], traces), (cursorIndex) => {
    const p=markerVisualState.Get();
    const z=displayTime.Current();
    return cursorEventItemsWith((c) => fullOrOriginal(z, c), cursorIndex, traces, p[0], p[1]);
  }];
}
function chartFrame(titleText, metadata, legend, testId, height, children){
  return Doc.Element("section", [Attr.Create("data-testid", testId), Dynamic_1("data-row-frame-height", Map(String, height)), Dynamic_1("style", Map((value) =>"display:flex; flex-direction:column; min-width:0; min-height:"+String(value)+"px; border-top:1px solid #e1e7ef; background:#fff;", height))], [Doc.Element("div", [Attr.Create("style", "display:flex; align-items:center; gap:6px 10px; height:28px; min-height:28px; padding:0 8px; color:#40536d; font-size:11px; flex-wrap:nowrap; overflow-x:auto; overflow-y:hidden; white-space:nowrap;")], ofSeq_1(delay(() => append_2([Doc.Element("strong", [Attr.Create("style", "margin-right:auto; flex:0 0 auto;")], [Doc.TextNode(titleText)])], delay(() => metadata))))), legend, element_1("div", [Attr.Create("style", "min-width:0; overflow:hidden;")], children)]);
}
function rowResizeHandle(rowId, bounds, height){
  const setHeight=(value) => {
    height.Set(clamp(bounds.Minimum, bounds.Maximum, value));
  };
  const resetHeight=() => {
    setHeight(bounds.DefaultHeight);
  };
  return Doc.Element("button", [Attr.Create("type", "button"), Attr.Create("data-testid", "ta-row-resize-"+rowId), Attr.Create("role", "separator"), Attr.Create("aria-orientation", "horizontal"), Attr.Create("aria-label", "Resize "+rowId+" chart height"), Attr.Create("aria-valuemin", String(bounds.Minimum)), Attr.Create("aria-valuemax", String(bounds.Maximum)), Dynamic_1("aria-valuenow", Map(String, height.View)), Attr.Create("title", "Drag to resize. Arrow keys resize; Home or double-click resets."), Attr.Create("style", "display:block; width:100%; height:8px; min-height:8px; padding:0; border:0; border-top:1px solid #d6e0eb; border-bottom:1px solid #edf1f6; background:#f5f8fb; cursor:ns-resize;"), Handler("mousedown", () =>(event) => {
    let pendingHeight, framePending, moveHandler, upHandler;
    event.preventDefault();
    const startClientY=event.clientY;
    const startHeight=height.Get();
    pendingHeight=startHeight;
    framePending=false;
    moveHandler=null;
    upHandler=null;
    const flush=() => {
      framePending=false;
      setHeight(pendingHeight);
    };
    moveHandler=(rawEvent) => {
      pendingHeight=startHeight+rawEvent.clientY-startClientY;
      return!framePending?(framePending=true,void requestAnimationFrame(() => {
        flush();
      })):null;
    };
    upHandler=() => {
      !(moveHandler==null)?globalThis.document.removeEventListener("mousemove", moveHandler):void 0;
      !(upHandler==null)?globalThis.document.removeEventListener("mouseup", upHandler):void 0;
      return framePending?flush():null;
    };
    globalThis.document.addEventListener("mousemove", moveHandler);
    globalThis.document.addEventListener("mouseup", upHandler);
  }), Handler("click", () =>(event) => event.detail>=2?resetHeight():null), Handler("keydown", () =>(event) => {
    const step=event.shiftKey?32:8;
    const m=event.key;
    return m=="ArrowUp"?(event.preventDefault(),setHeight(height.Get()-step)):m=="ArrowDown"?(event.preventDefault(),setHeight(height.Get()+step)):m=="Home"?(event.preventDefault(),resetHeight()):null;
  })], []);
}
function rowTitle(row, traces){
  const m=rowExplicitLabel(row);
  if(m==null){
    if(traces==null||length(traces)===0)return rowKindText_1(row.Kind);
    else {
      const value=concat_1(" / ", map((trace) => IsNullOrWhiteSpace(trace.Label)?trace.TraceId:trace.Label, traces));
      return IsNullOrWhiteSpace(value)?rowKindText_1(row.Kind):value;
    }
  }
  else return m.$0;
}
function rectanglePath(x, y, width, height){
  return"M "+fixedText(x)+" "+fixedText(y)+" h "+fixedText(width)+" v "+fixedText(height)+" h "+fixedText(-width)+" Z";
}
function svgElement(name, attrs, children){
  return Doc.SvgElement(name, attrs, children);
}
function svgAttr(name, value){
  return Attr.Create(name, value);
}
function normalizeDocumentPresentation(document){
  let _1, _2;
  const m=tryDecode(document.DefaultView);
  if(m.$==0){
    const _3=m.$0;
    _1=_3!=null&&_3.$==1;
  }
  else _1=false;
  if(_1){
    const projection=m.$0.$0;
    _2=apply({
      CoverageIdentity:projection.CoverageIdentity, 
      CoverageRevision:0n, 
      QueryGeneration:0n, 
      Completeness:projection.Completeness, 
      TotalObservationCount:projection.TotalObservationCount, 
      Segments:projection.Segments, 
      OverviewAxisRef:projection.OverviewAxisRef, 
      OverviewAnchors:projection.OverviewAnchors, 
      ActiveDetail:projection.ActiveDetail
    }, document.DefaultView);
  }
  else _2=document.DefaultView;
  return{
    WorkspaceId:document.WorkspaceId, 
    Title:document.Title, 
    RowsRef:document.RowsRef, 
    StatusRef:document.StatusRef, 
    SharedTimeAxis:document.SharedTimeAxis, 
    TemporalAxisRefs:document.TemporalAxisRefs, 
    BaseRowId:document.BaseRowId, 
    Rows:document.Rows, 
    EditorSchemas:document.EditorSchemas, 
    AllowedActions:document.AllowedActions, 
    DefaultView:_2
  };
}
function editorPathRoot(path){
  const o=tryHead_1(sort_1(filter_2((index) => index>=0, ofArray([path.indexOf("."), path.indexOf("[")]))));
  const o_1=o==null?null:Some(Substring(path, 0, o.$0));
  return o_1==null?path:o_1.$0;
}
function editorScalarText(a){
  return a.$==1?fixedText(a.$0):a.$==2?a.$0?"true":"false":a.$0;
}
function tryReadAtOrBefore(readValue, index){
  while(true)
    {
      if(index<0)return null;
      else {
        const m=readValue(index);
        if(m==null)index=index-1;
        else return Some(m.$0);
      }
    }
}
function Int(){
  set_counter(counter()+1);
  return counter();
}
function set_counter(_1){
  _c_12.counter=_1;
}
function counter(){
  return _c_12.counter;
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
function mapInPlace_1(f, arr){
  for(let i=0, _1=arr.length-1;i<=_1;i++)arr[i]=f(arr[i]);
  return arr;
}
function New_69(Node_1, Left, Right, Height, Count){
  return{
    Node:Node_1, 
    Left:Left, 
    Right:Right, 
    Height:Height, 
    Count:Count
  };
}
class FormatException extends Error {
  constructor(i, _1){
    if(i=="New_1"){
      const message=_1;
      super(message);
    }
  }
}
function New_70(DynElem, DynFlags, DynNodes, OnAfterRender_1){
  const _1={
    DynElem:DynElem, 
    DynFlags:DynFlags, 
    DynNodes:DynNodes
  };
  SetOptional(_1, "OnAfterRender", OnAfterRender_1);
  return _1;
}
let _c_7=Lazy((_i) => class $StartupCode_Animation {
  static {
    _c_7=_i(this);
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
  return _c_15.Empty;
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
function New_71(wireVersion, kind, actionKind, canvasInstanceId, rowId, traceId, rowKind_1, dataRef, heightWeight, visible, sourceId, instrument, intervalMinutes, fromUtc, toUtcExclusive, includePartial, afterDataRevision, dataRevision, reasonCode, templateKey, hasTemplateRowId, editorValues, expectedDocumentRevision, hasExpectedDocumentRevision, baseRowId, eventTimeUtc, startEventTimeUtc, endEventTimeExclusiveUtc, maximumBasePoints, coverageIntentVersion, hasCoverageIntent, expectedCoverageRevision, hasExpectedCoverageRevision, queryGeneration, startObservationOrdinal, hasStartObservationOrdinal, coverageObservationCount, coverageDirection, coverageRangeAuthority){
  return{
    wireVersion:wireVersion, 
    kind:kind, 
    actionKind:actionKind, 
    canvasInstanceId:canvasInstanceId, 
    rowId:rowId, 
    traceId:traceId, 
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
    hasExpectedDocumentRevision:hasExpectedDocumentRevision, 
    baseRowId:baseRowId, 
    eventTimeUtc:eventTimeUtc, 
    startEventTimeUtc:startEventTimeUtc, 
    endEventTimeExclusiveUtc:endEventTimeExclusiveUtc, 
    maximumBasePoints:maximumBasePoints, 
    coverageIntentVersion:coverageIntentVersion, 
    hasCoverageIntent:hasCoverageIntent, 
    expectedCoverageRevision:expectedCoverageRevision, 
    hasExpectedCoverageRevision:hasExpectedCoverageRevision, 
    queryGeneration:queryGeneration, 
    startObservationOrdinal:startObservationOrdinal, 
    hasStartObservationOrdinal:hasStartObservationOrdinal, 
    coverageObservationCount:coverageObservationCount, 
    coverageDirection:coverageDirection, 
    coverageRangeAuthority:coverageRangeAuthority
  };
}
function Bind_2(f, r){
  return r.$==1?Error_1(r.$0):f(r.$0);
}
function IsError(result){
  return result.$==1;
}
function ToOption(result){
  return result.$==0?Some(result.$0):null;
}
function DefaultWith(defThunk, result){
  return result.$==0?result.$0:defThunk(result.$0);
}
function Map_2(f, r){
  return r.$==1?Error_1(r.$0):Ok(f(r.$0));
}
function DefaultValue(value, result){
  return result.$==0?result.$0:value;
}
function New_72(schema, exportedAtUtc, documentRevision, dataRevision, state){
  return{
    schema:schema, 
    exportedAtUtc:exportedAtUtc, 
    documentRevision:documentRevision, 
    dataRevision:dataRevision, 
    state:state
  };
}
let _c_8=Lazy((_i) => class $StartupCode_Renderer {
  static {
    _c_8=_i(this);
  }
  static defaultOptions;
  static darkPlotPalette;
  static lightPlotPalette;
  static rendererTelemetryInstanceSequence;
  static axisResizeBound;
  static axisViewportWidth;
  static {
    this.axisViewportWidth=_c_2.Create_1(1440);
    this.axisResizeBound=false;
    this.rendererTelemetryInstanceSequence=0;
    this.lightPlotPalette=New_76("light", "#fbfcfe", "#eef3f8", "#e7ecf3", "#1f4f73", "#f8fafc", "#708198", "#263b55", "#c7d3e2", "#3d718e", "#138a59", "#c53d3d", "#64748b", "rgba(203,213,225,.20)", "#4ade80", "#ffffff", "#263b55", "#8ca0b8");
    this.darkPlotPalette=New_76("dark", "#000000", "#000000", "#334155", "#7dd3fc", "#0b1017", "#cbd5e1", "#e2e8f0", "#475569", "#60a5fa", "#4ade80", "#f87171", "#94a3b8", "rgba(203,213,225,.20)", "#4ade80", "#111827", "#e2e8f0", "#64748b");
    this.defaultOptions={
      MinimumVisibleBars:12, 
      DefaultVisibleBars:48, 
      MaximumVisibleBars:4000, 
      EditorSchemas:[]
    };
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
    return Create_2(Updates_1, {
      c:Current, 
      s:Snap, 
      v:VarView
    });
  }
}
function concat_4(o){
  let r=[];
  let k;
  for(var k_1 in o)r.push.apply(r, o[k_1]);
  return r;
}
let _c_9=Lazy((_i) => class $StartupCode_DomUtility {
  static {
    _c_9=_i(this);
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
let _c_10=Lazy((_i) => class Client {
  static {
    _c_10=_i(this);
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
function New_73(k, ct){
  return{k:k, ct:ct};
}
function No(Item){
  return{$:1, $0:Item};
}
function Ok_1(Item){
  return{$:0, $0:Item};
}
function Cc(Item){
  return{$:2, $0:Item};
}
let _c_11=Lazy((_i) => class $StartupCode_Concurrency {
  static {
    _c_11=_i(this);
  }
  static GetCT;
  static Zero;
  static defCTS;
  static scheduler;
  static noneCT;
  static {
    this.noneCT=New_74(false, []);
    this.scheduler=new Scheduler();
    this.defCTS=[new CancellationTokenSource()];
    this.Zero=Return();
    this.GetCT=(c) => {
      c.k(Ok_1(c.ct));
    };
  }
});
function New_74(IsCancellationRequested, Registrations){
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
function fromSeq(s){
  const a=ofSeq(map_2((_1) => Pair.New(_1[0], _1[1]), distinctBy_1((t) => t[0], rev_1(s))));
  sortInPlace(a);
  return Build(a, 0, a.length-1);
}
function New_75(path, kind, textValue, numberValue, boolValue){
  return{
    path:path, 
    kind:kind, 
    textValue:textValue, 
    numberValue:numberValue, 
    boolValue:boolValue
  };
}
function fromValue(value){
  let _1, _2;
  if(value.$==5){
    const values=value.$0;
    const _3=values.TryFind("templateKey");
    const _4=values.TryFind("displayName");
    const _5=values.TryFind("schemaRevision");
    const _6=values.TryFind("fields");
    if(_3!=null&&_3.$==1){
      if(_3.$0.$==3){
        if(_4!=null&&_4.$==1){
          if(_4.$0.$==3){
            if(_5!=null&&_5.$==1){
              if(_5.$0.$==2){
                if(_6!=null&&_6.$==1){
                  if(_6.$0.$==4){
                    _3.$0.$0;
                    const revision=_5.$0.$0;
                    _1=(_6.$0.$0,_4.$0.$0,revision>=0&&revision<=Number(9223372036854775807n)&&(revision<0?Math.ceil(revision):Math.floor(revision))===revision)&&(_2=[_4.$0.$0, _6.$0.$0, _5.$0.$0, _3.$0.$0],true);
                  }
                  else _1=false;
                }
                else _1=false;
              }
              else _1=false;
            }
            else _1=false;
          }
          else _1=false;
        }
        else _1=false;
      }
      else _1=false;
    }
    else _1=false;
    if(_1){
      const displayName=_2[0];
      const revision_1=_2[2];
      const templateKey=_2[3];
      return Bind_2((decodedFields) => validateSchema(limits(), {
        TemplateKey:templateKey, 
        DisplayName:displayName, 
        SchemaRevision:BigInt(Math.trunc(revision_1)), 
        Fields:decodedFields
      }), sequence(mapi((_7, _8) => fieldFromValue("schema.fields["+String(_7)+"]", _8), nonNull(_2[1]))));
    }
    else return error("editor-schema-shape", "schema", "Template schema requires templateKey, displayName, schemaRevision and fields.");
  }
  else return error("editor-schema-shape", "schema", "Template schema must be an object.");
}
function fieldFromValue(field_1, value){
  let _1;
  if(value.$==5){
    const values=value.$0;
    const _2=values.TryFind("key");
    const _3=values.TryFind("label");
    const _4=values.TryFind("kind");
    const _5=values.TryFind("required");
    if(_2!=null&&_2.$==1&&(_2.$0.$==3&&(_3!=null&&_3.$==1&&(_3.$0.$==3&&(_4!=null&&_4.$==1&&(_5!=null&&_5.$==1&&(_5.$0.$==1&&(_1=[_2.$0.$0, _4.$0, _3.$0.$0, _5.$0.$0],true)))))))){
      const key_1=_1[0];
      const label=_1[2];
      const required=_1[3];
      return Map_2((decodedKind) =>({
        Key:key_1, 
        Label:label, 
        Kind:decodedKind, 
        Required:required, 
        DefaultValue:values.TryFind("defaultValue")
      }), kindFromValue(field_1+".kind", _1[1]));
    }
    else return error("editor-schema-field", field_1, "Editor field requires key, label, kind and required.");
  }
  else return error("editor-schema-field", field_1, "Editor field must be an object.");
}
function sequence(results){
  const errors_1=collect((a) => a.$==0?[]:ofList(a.$0), results);
  return length(errors_1)>0?Error_1(ofArray(errors_1)):Ok(choose((a) => a.$==1?null:Some(a.$0), results));
}
function error(code, field_1, message){
  return Error_1(ofArray([error_2(code, field_1, message)]));
}
function kindFromValue(field_1, value){
  if(value.$==5){
    const values=value.$0;
    const m=values.TryFind("type");
    switch(m!=null&&m.$==1?m.$0.$==3?m.$0.$0=="text"?0:m.$0.$0=="boolean"?1:m.$0.$0=="integer"?2:m.$0.$0=="decimal"?3:m.$0.$0=="choice"?4:m.$0.$0=="scale"?5:m.$0.$0=="list"?6:m.$0.$0=="group"?7:8:8:8){
      case 0:
        return Ok({$:0});
      case 1:
        return Ok({$:3});
      case 2:
        let _1;
        const _2=optionalInt64(field_1+".minimum", "minimum", values);
        const _3=optionalInt64(field_1+".maximum", "maximum", values);
        switch(_2.$==1?_3.$==1?(_1=[_2.$0, _3.$0],2):(_1=_2.$0,1):_3.$==1?(_1=_3.$0,1):(_1=[_3.$0, _2.$0],0)){
          case 0:
            return Ok({
              $:1, 
              $0:_1[1], 
              $1:_1[0]
            });
          case 1:
            return Error_1(_1);
          case 2:
            return Error_1(append_1(_1[0], _1[1]));
        }
        break;
      case 3:
        let _4;
        const _5=optionalFloat(field_1+".minimum", "minimum", values);
        const _6=optionalFloat(field_1+".maximum", "maximum", values);
        switch(_5.$==1?_6.$==1?(_4=[_5.$0, _6.$0],2):(_4=_5.$0,1):_6.$==1?(_4=_6.$0,1):(_4=[_6.$0, _5.$0],0)){
          case 0:
            return Ok({
              $:2, 
              $0:_4[1], 
              $1:_4[0]
            });
          case 1:
            return Error_1(_4);
          case 2:
            return Error_1(append_1(_4[0], _4[1]));
        }
        break;
      case 4:
        let _7;
        const m_1=values.TryFind("choices");
        return m_1!=null&&m_1.$==1&&(m_1.$0.$==4&&(_7=m_1.$0.$0,true))?Map_2((I) =>({$:4, $0:I}), sequence(mapi((_12, _13) => choiceFromValue(String(field_1)+".choices["+String(_12)+"]", _13), nonNull(_7)))):error("editor-schema-choice", field_1, "Choice schema requires choices.");
      case 5:
        let _8;
        const m_2=values.TryFind("scaleKeys");
        return m_2!=null&&m_2.$==1&&(m_2.$0.$==4&&(_8=m_2.$0.$0,true))?Map_2((a) =>({$:5, $0:a}), sequence(mapi((_12, _13) => _13.$==3?Ok(_13.$0):error("editor-schema-scale", String(field_1)+".scaleKeys["+String(_12)+"]", "Scale key must be text."), nonNull(_8)))):error("editor-schema-scale", field_1, "Scale schema requires scaleKeys.");
      case 6:
        let _9, _10;
        const m_3=values.TryFind("item");
        if(m_3!=null&&m_3.$==1){
          const m_4=[kindFromValue(field_1+".item", m_3.$0), optionalInt(field_1+".minimum", "minimum", values), optionalInt(field_1+".maximum", "maximum", values)];
          const c=m_4[0];
          if(c.$==0){
            const c_1=m_4[1];
            if(c_1.$==0){
              const c_2=m_4[2];
              _9=c_2.$==0?(_10=[c.$0, c_2.$0, c_1.$0],true):(_10=m_4,false);
            }
            else _9=(_10=m_4,false);
          }
          else _9=(_10=m_4,false);
          return _9?Ok({
            $:6, 
            $0:_10[0], 
            $1:_10[2], 
            $2:_10[1]
          }):Error_1(ofSeq_1(delay(() => {
            const c_3=_10[0];
            let _12=c_3.$==1?c_3.$0:[];
            return append_2(_12, delay(() => {
              const c_4=_10[1];
              let _13=c_4.$==1?c_4.$0:[];
              return append_2(_13, delay(() => {
                const c_5=_10[2];
                return c_5.$==1?c_5.$0:[];
              }));
            }));
          })));
        }
        else return error("editor-schema-list", field_1, "List schema requires an item kind.");
        break;
      case 7:
        let _11;
        const m_5=values.TryFind("fields");
        return m_5!=null&&m_5.$==1&&(m_5.$0.$==4&&(_11=m_5.$0.$0,true))?Map_2((f) =>({$:7, $0:f}), sequence(mapi((_12, _13) => fieldFromValue(String(field_1)+".fields["+String(_12)+"]", _13), nonNull(_11)))):error("editor-schema-group", field_1, "Group schema requires fields.");
      case 8:
        return error("editor-schema-kind", field_1, "Unknown editor schema kind.");
    }
  }
  else return error("editor-schema-kind", field_1, "Editor schema kind must be an object.");
}
function optionalInt64(field_1, key_1, values){
  let _1, _2;
  const m=values.TryFind(key_1);
  if(m!=null&&m.$==1){
    if(m.$0.$==2){
      const value=m.$0.$0;
      _2=value>=Number(-9223372036854775808n)&&value<=Number(9223372036854775807n)&&(value<0?Math.ceil(value):Math.floor(value))===value?(_1=m.$0.$0,1):2;
    }
    else _2=2;
  }
  else _2=0;
  switch(_2){
    case 0:
      return Ok(null);
    case 1:
      return Ok(Some(BigInt(Math.trunc(_1))));
    case 2:
      return error("editor-schema-number", field_1, "Editor schema bound must be an integer.");
  }
}
function optionalFloat(field_1, key_1, values){
  let _1, _2;
  const m=values.TryFind(key_1);
  if(m!=null&&m.$==1){
    if(m.$0.$==2){
      const value=m.$0.$0;
      _2=!(isNaN(value)||Math.abs(value)===Infinity)?(_1=m.$0.$0,1):2;
    }
    else _2=2;
  }
  else _2=0;
  switch(_2){
    case 0:
      return Ok(null);
    case 1:
      return Ok(Some(_1));
    case 2:
      return error("editor-schema-number", field_1, "Editor schema bound must be finite.");
  }
}
function choiceFromValue(field_1, value){
  let _1;
  if(value.$==5){
    const values=value.$0;
    const _2=values.TryFind("key");
    const _3=values.TryFind("label");
    const _4=values.TryFind("value");
    return _2!=null&&_2.$==1&&(_2.$0.$==3&&(_3!=null&&_3.$==1&&(_3.$0.$==3&&(_4!=null&&_4.$==1&&(_1=[_4.$0, _2.$0.$0, _3.$0.$0],true)))))?Ok({
      Key:_1[1], 
      Label:_1[2], 
      Value:_1[0]
    }):error("editor-schema-choice", field_1, "Editor choice requires key, label and value.");
  }
  else return error("editor-schema-choice", field_1, "Editor choice must be an object.");
}
function optionalInt(field_1, key_1, values){
  let _1, _2;
  const m=values.TryFind(key_1);
  if(m!=null&&m.$==1){
    if(m.$0.$==2){
      const value=m.$0.$0;
      _2=value>=0&&value<=2147483647&&(value<0?Math.ceil(value):Math.floor(value))===value?(_1=m.$0.$0,1):2;
    }
    else _2=2;
  }
  else _2=0;
  switch(_2){
    case 0:
      return Ok(null);
    case 1:
      return Ok(Some(toInt(_1)));
    case 2:
      return error("editor-schema-number", field_1, "Editor schema bound must be a non-negative integer.");
  }
}
class Random extends Object_1 {
  Next_1(maxValue){
    return maxValue<0?FailWith("'maxValue' must be greater than zero."):Math.floor(Math.random()*maxValue);
  }
}
function validateSchema(limits_1, schema){
  const m=schemaErrors(limits_1, schema);
  return m.$==0?Ok(schema):Error_1(m);
}
function nonNull(values){
  return values==null?[]:values;
}
function schemaErrors(limits_1, schema){
  const fields=nonNull(schema.Fields);
  return ofSeq_1(delay(() => append_2(identifier("schema.templateKey", schema.TemplateKey), delay(() => append_2(identifier("schema.displayName", schema.DisplayName), delay(() => append_2(schema.SchemaRevision<0n?[error_2("invalid-schema-revision", "schema.schemaRevision", "Schema revision must be non-negative.")]:[], delay(() => append_2(length(fields)+sumBy((field_1) => fieldCount(field_1.Kind), fields)>limits_1.MaxFields?[error_2("limit-editor-fields", "schema.fields", "Editor fields exceed hard limit "+String(limits_1.MaxFields)+".")]:[], delay(() => append_2(duplicateKey("schema.fields", (field_1) => field_1.Key, fields), delay(() => collect_1((m) => fieldErrors(limits_1, "schema.fields["+String(m[0])+"]", 1, m[1]), indexed(fields))))))))))))));
}
function duplicateKey(field_1, keyOf, values){
  return exists((_1) => _1[1]>1, countBy((x) => x, map(keyOf, values)))?ofArray([error_2("duplicate-key", field_1, field_1+" keys must be unique.")]):FSharpList.Empty;
}
function fieldErrors(limits_1, field_1, depth, schema){
  return ofSeq_1(delay(() => append_2(identifier(field_1+".key", schema.Key), delay(() => append_2(identifier(field_1+".label", schema.Label), delay(() => append_2(kindErrors(limits_1, field_1+".kind", depth, schema.Kind), delay(() => {
    const m=schema.DefaultValue;
    return m==null?[]:valueErrors(limits_1, field_1+".defaultValue", depth, schema.Kind, m.$0);
  }))))))));
}
function fieldCount(kind){
  while(true)
    {
      if(kind.$==7){
        const x=nonNull(kind.$0);
        return(((p) =>(a) => sumBy(p, a))((field_1) => 1+fieldCount(field_1.Kind)))(x);
      }
      else if(kind.$==6){
        const item=kind.$0;
        kind=item;
      }
      else return 0;
    }
}
function validateInputs(limits_1, schema, values){
  const m=inputErrors(limits_1, schema, values);
  return m.$==0?Ok(values):Error_1(m);
}
function kindErrors(limits_1, field_1, depth, kind){
  return ofSeq_1(delay(() => append_2(depth>limits_1.MaxSchemaDepth?[error_2("limit-schema-depth", field_1, "Schema depth exceeds hard limit "+String(limits_1.MaxSchemaDepth)+".")]:[], delay(() => {
    if(kind.$==1)return rangeErrors(field_1, kind.$0, kind.$1);
    else if(kind.$==2)return rangeErrors(field_1, kind.$0, kind.$1);
    else if(kind.$==4){
      const values=nonNull(kind.$0);
      return append_2(length(values)===0?[error_2("choice-required", field_1, "Choice editor requires at least one choice.")]:[], delay(() => append_2(length(values)>limits_1.MaxChoicesPerField?[error_2("limit-editor-choices", field_1, "Choices exceed hard limit "+String(limits_1.MaxChoicesPerField)+".")]:[], delay(() => append_2(duplicateKey(field_1, (choice) => choice.Key, values), delay(() => collect_1((m) => {
        const index=m[0];
        const choice=m[1];
        return append_2(identifier(String(field_1)+".choices["+String(index)+"].key", choice.Key), delay(() => append_2(identifier(String(field_1)+".choices["+String(index)+"].label", choice.Label), delay(() => unsafeValue(String(field_1)+".choices["+String(index)+"].value", choice.Value)))));
      }, indexed(values))))))));
    }
    else if(kind.$==5){
      const values_1=nonNull(kind.$0);
      return append_2(length(values_1)===0?[error_2("scale-required", field_1, "Scale editor requires at least one allowed key.")]:[], delay(() => append_2(duplicateKey(field_1, (x) => x, values_1), delay(() => collect_1((m) => identifier(String(field_1)+".scaleKeys["+String(m[0])+"]", m[1]), indexed(values_1))))));
    }
    else if(kind.$==6){
      const minimum=kind.$1;
      const maximum=kind.$2;
      const itemKind=kind.$0;
      return append_2(listRangeErrors(limits_1, field_1, minimum, maximum), delay(() => kindErrors(limits_1, field_1+".item", depth+1, itemKind)));
    }
    else if(kind.$==7){
      const values_2=nonNull(kind.$0);
      return append_2(duplicateKey(field_1, (child) => child.Key, values_2), delay(() => collect_1((m) => fieldErrors(limits_1, String(field_1)+".fields["+String(m[0])+"]", depth+1, m[1]), indexed(values_2))));
    }
    else return[];
  }))));
}
function valueErrors(limits_1, field_1, depth, kind, value){
  return ofSeq_1(delay(() => append_2(unsafeValue(field_1, value), delay(() => {
    let _1;
    switch(kind.$==3?value.$==1?0:7:kind.$==1?value.$==2?(_1=[kind.$1, kind.$0, value.$0],1):7:kind.$==2?value.$==2?(_1=[kind.$1, kind.$0, value.$0],2):7:kind.$==4?(_1=kind.$0,3):kind.$==5?value.$==3?(_1=[value.$0, kind.$0],4):7:kind.$==6?value.$==4?(_1=[kind.$0, value.$0, kind.$2, kind.$1],5):7:kind.$==7?value.$==5?(_1=[kind.$0, value.$0],6):7:value.$==3?0:7){
      case 0:
        return[];
      case 1:
        let _2;
        const maximum=_1[0];
        const number_1=_1[2];
        if(isNaN(number_1)||Math.abs(number_1)===Infinity||(number_1<0?Math.ceil(number_1):Math.floor(number_1))!==number_1)return[error_2("invalid-integer", field_1, field_1+" must be an integer.")];
        else if(number_1<Number(-9223372036854775808n)||number_1>Number(9223372036854775807n))return[error_2("integer-out-of-range", field_1, field_1+" is outside Int64 range.")];
        else {
          const integer=BigInt(Math.trunc(number_1));
          const m=_1[1];
          let _3=m!=null&&m.$==1&&(integer<m.$0&&(_2=m.$0,true))?[error_2("below-minimum", field_1, field_1+" is below its minimum.")]:[];
          return append_2(_3, delay(() => {
            let _8;
            return maximum!=null&&maximum.$==1&&(integer>maximum.$0&&(_8=maximum.$0,true))?[error_2("above-maximum", field_1, field_1+" exceeds its maximum.")]:[];
          }));
        }
        break;
      case 2:
        let _4;
        const maximum_1=_1[0];
        const number_2=_1[2];
        if(isNaN(number_2)||Math.abs(number_2)===Infinity)return[error_2("invalid-decimal", field_1, field_1+" must be finite.")];
        else {
          const m_1=_1[1];
          let _5=m_1!=null&&m_1.$==1&&(number_2<m_1.$0&&(_4=m_1.$0,true))?[error_2("below-minimum", field_1, field_1+" is below its minimum.")]:[];
          return append_2(_5, delay(() => {
            let _8;
            return maximum_1!=null&&maximum_1.$==1&&(number_2>maximum_1.$0&&(_8=maximum_1.$0,true))?[error_2("above-maximum", field_1, field_1+" exceeds its maximum.")]:[];
          }));
        }
        break;
      case 3:
        return!exists((choice) => Equals(choice.Value, value), nonNull(_1))?[error_2("choice-not-allowed", field_1, field_1+" is not one of the declared choices.")]:[];
      case 4:
        return!arrContains(_1[0], nonNull(_1[1]))?[error_2("scale-not-allowed", field_1, field_1+" is not one of the declared scales.")]:[];
      case 5:
        let _6;
        const itemKind=_1[0];
        const maximum_2=_1[2];
        const values=nonNull(_1[1]);
        const m_2=_1[3];
        let _7=m_2!=null&&m_2.$==1&&(length(values)<m_2.$0&&(_6=m_2.$0,true))?[error_2("list-too-short", field_1, field_1+" has fewer than the required items.")]:[];
        return append_2(_7, delay(() => {
          let _8;
          return append_2(maximum_2!=null&&maximum_2.$==1&&(length(values)>maximum_2.$0&&(_8=maximum_2.$0,true))?[error_2("list-too-long", field_1, field_1+" exceeds its item limit.")]:[], delay(() => append_2(length(values)>limits_1.MaxListItems?[error_2("limit-list-items", field_1, String(field_1)+" exceeds hard limit "+String(limits_1.MaxListItems)+".")]:[], delay(() => collect_1((m_3) => valueErrors(limits_1, String(field_1)+"["+String(m_3[0])+"]", depth+1, itemKind, m_3[1]), indexed(values))))));
        }));
      case 6:
        const values_1=_1[1];
        return collect_1((child) => {
          const m_3=values_1.TryFind(child.Key);
          return m_3==null?child.Required?[error_2("required", field_1+"."+child.Key, child.Key+" is required.")]:[]:valueErrors(limits_1, field_1+"."+child.Key, depth+1, child.Kind, m_3.$0);
        }, nonNull(_1[0]));
      case 7:
        return[error_2("editor-value-kind-mismatch", field_1, field_1+" does not match its editor kind.")];
    }
  }))));
}
function inputErrors(limits_1, schema, values){
  const inputs=nonNull(values);
  return ofSeq_1(delay(() => append_2(schemaErrors(limits_1, schema), delay(() => append_2(length(inputs)>limits_1.MaxFields?[error_2("limit-editor-inputs", "editor.values", "Editor inputs exceed hard limit "+String(limits_1.MaxFields)+".")]:[], delay(() => append_2(exists((_1) => _1[1]>1, countBy((a) => a.Path, inputs))?[error_2("duplicate-editor-input", "editor.values", "Editor input paths must be unique.")]:[], delay(() => collect_1((m) => {
    const input_1=m[1];
    const index=m[0];
    return append_2(identifier("editor.values["+String(index)+"].path", input_1.Path), delay(() => {
      const m_1=resolvePath(limits_1, schema, input_1.Path);
      return m_1.$==1?m_1.$0:scalarErrors("editor.values["+String(index)+"].value", m_1.$0, input_1.Value);
    }));
  }, indexed(inputs))))))))));
}
function rangeErrors(field_1, minimum, maximum){
  let _1;
  return minimum!=null&&minimum.$==1&&(maximum!=null&&maximum.$==1&&(Compare(minimum.$0, maximum.$0)===1&&(_1=[minimum.$0, maximum.$0],true)))?ofArray([error_2("invalid-range", field_1, field_1+" minimum must not exceed maximum.")]):FSharpList.Empty;
}
function listRangeErrors(limits_1, field_1, minimum, maximum){
  return ofSeq_1(delay(() => {
    let _1;
    return append_2(minimum!=null&&minimum.$==1&&(minimum.$0<0&&(_1=minimum.$0,true))?[error_2("invalid-list-minimum", field_1, field_1+" minimum must be non-negative.")]:[], delay(() => {
      let _2, _3;
      if(maximum!=null&&maximum.$==1){
        const value=maximum.$0;
        _2=(value<0||value>limits_1.MaxListItems)&&(_3=maximum.$0,true);
      }
      else _2=false;
      let _4=_2?[error_2("invalid-list-maximum", field_1, String(field_1)+" maximum must be between zero and "+String(limits_1.MaxListItems)+".")]:[];
      return append_2(_4, delay(() => rangeErrors(field_1, minimum, maximum)));
    }));
  }));
}
function resolvePath(limits_1, schema, path){
  const separator=path==null?-1:firstPathSeparator(path);
  const key_1=IsNullOrWhiteSpace(path)?"":separator<0?path:Substring(path, 0, separator);
  const remainder=separator<0?"":path.substring(separator);
  const m=tryFind((field_1) => field_1.Key==key_1, nonNull(schema.Fields));
  return m==null?Error_1(ofArray([error_2("editor-path-unknown", path, path+" does not exist in the template schema.")])):resolveKind(limits_1, path, m.$0.Kind, remainder);
}
function scalarErrors(field_1, kind, scalar){
  return valueErrors(limits(), field_1, 1, kind, scalarAsValue(scalar));
}
function firstPathSeparator(text_1){
  const dotIndex=text_1.indexOf(".");
  const bracketIndex=text_1.indexOf("[");
  return dotIndex<0?bracketIndex:bracketIndex<0?dotIndex:Compare(dotIndex, bracketIndex)===-1?dotIndex:bracketIndex;
}
function resolveKind(limits_1, field_1, kind, remainder){
  let _1;
  switch(kind.$==7?(kind.$0,StartsWith(remainder, ".")?(_1=kind.$0,0):remainder==""?2:3):kind.$==6?(_1=kind.$0,1):remainder==""?2:3){
    case 0:
      const childPath=remainder.substring(1);
      const separator=firstPathSeparator(childPath);
      const key_1=separator<0?childPath:Substring(childPath, 0, separator);
      const childRemainder=separator<0?"":childPath.substring(separator);
      const m=tryFind((child) => child.Key==key_1, nonNull(_1));
      return m==null?Error_1(ofArray([error_2("editor-path-unknown", field_1, field_1+" does not exist in the template schema.")])):resolveKind(limits_1, field_1, m.$0.Kind, childRemainder);
    case 1:
      return Bind_2((_2) => resolveKind(limits_1, field_1, _1, _2[1]), takeListIndex(limits_1, field_1, remainder));
    case 2:
      return Ok(kind);
    case 3:
      return Error_1(ofArray([error_2("editor-path-invalid", field_1, field_1+" does not match the template schema.")]));
  }
}
function scalarAsValue(a){
  return a.$==1?{$:2, $0:a.$0}:a.$==2?{$:1, $0:a.$0}:{$:3, $0:a.$0};
}
function takeListIndex(limits_1, field_1, text_1){
  let o, _1;
  if(IsNullOrEmpty(text_1)||text_1[0]!=="[")return Error_1(ofArray([error_2("editor-list-index-required", field_1, field_1+" requires a list index.")]));
  else {
    const closeIndex=text_1.indexOf("]");
    if(closeIndex<=1)return Error_1(ofArray([error_2("editor-list-index-invalid", field_1, field_1+" has an invalid list index.")]));
    else {
      const m=(o=0,[TryParse(Substring(text_1, 1, closeIndex-1), {get:() => o, set:(v) => {
        o=v;
      }}), o]);
      if(m[0]){
        const index=m[1];
        _1=index>=0&&index<limits_1.MaxListItems;
      }
      else _1=false;
      return _1?Ok([m[1], text_1.substring(closeIndex+1)]):Error_1(ofArray([error_2("editor-list-index-invalid", field_1, field_1+" list index is outside the allowed range.")]));
    }
  }
}
function limits(){
  return _c_13.limits;
}
function initialEditorInputs(schema){
  return collect((field_1) => {
    const m=field_1.DefaultValue;
    return m==null?fallbackInputs(field_1.Key, field_1.Kind):flattenEditorValue(field_1.Key, field_1.Kind, m.$0);
  }, schema.Fields);
}
function documentMaximumVisibleBars(fallbackMaximum, defaultView){
  const o=tryMaximumVisibleBars(defaultView);
  let _1=o==null?fallbackMaximum:o.$0;
  return clamp(1, 4000, _1);
}
function isStaleCoverageCandidate(pendingQueryGeneration, defaultView){
  const o=tryLoadedCoverage(defaultView);
  return o==null?false:o.$0.QueryGeneration<pendingQueryGeneration;
}
function prepareDataScheduled(schedule, data, onCompleted){
  let axes, resolved;
  const entries=ofSeq(ToSeq(data));
  axes=new FSharpMap("New", []);
  resolved=new FSharpMap("New", []);
  function prepareSeries(index){
    return index>=length(entries)?onCompleted({
      RawData:data, 
      ResolvedAxes:axes, 
      ResolvedSeries:resolved
    }):schedule(() => {
      let next, _1, _2;
      const p=get(entries, index);
      const value=p[1];
      if(value.$==4)next=Some(map((item) => {
        const p_1=pointPayload_1(item);
        return{Payload:p_1[1], Temporal:p_1[0]};
      }, value.$0));
      else {
        const m=tryTemporalSeries(value);
        if(m==null)next=null;
        else {
          const points=m.$0[2];
          const axisRevision=m.$0[1];
          const k=m.$0[0];
          const m_1=axes.TryFind(k);
          next=m_1!=null&&m_1.$==1&&(m_1.$0.Revision===axisRevision&&(_1=m_1.$0,true))?Some(choose((_3) => {
            const o=_1.Points.TryFind(_3[0]);
            return o==null?null:Some({Payload:Some(_3[1]), Temporal:Some(o.$0)});
          }, points)):Some([]);
        }
      }
      if(next==null)_2=null;
      else {
        const v=next.$0;
        _2=void(resolved=resolved.Add_1(p[0], v));
      }
      return prepareSeries(index+1);
    });
  }
  function prepareAxes(index){
    return index>=length(entries)?prepareSeries(0):schedule(() => {
      let _1;
      const m=prepareAxis((get(entries, index))[1]);
      if(m==null)_1=null;
      else {
        const k=m.$0[0];
        const v=m.$0[1];
        _1=void(axes=axes.Add_1(k, v));
      }
      return prepareAxes(index+1);
    });
  }
  return prepareAxes(0);
}
function referenceTimelineForDocumentPrepared(document, prepared){
  const m=document.BaseRowId;
  if(m==null)return referenceTimelinePrepared(document.Rows, prepared);
  else {
    const baseRowId=m.$0;
    const o=tryFind((row) => row.Visible&&row.RowId==baseRowId, document.Rows);
    const o_1=o==null?null:Some(rowTimelinePrepared(o.$0, prepared));
    return o_1==null?[]:o_1.$0;
  }
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
function visibleEventRangePrepared(document, prepared, window_1){
  const m=tryBaseRow(document);
  if(m!=null&&m.$==1){
    const baseRowId=m.$0[0];
    const baseRow=m.$0[1];
    const timeline=rowTimelinePrepared(baseRow, prepared);
    const selected=selectWindow(window_1, timeline);
    if(length(selected)===0)return null;
    else {
      const startTime=get(selected, 0);
      const endIndex=window_1.StartIndex+length(selected);
      const o=filter((value) => Compare(value, startTime)>0, endIndex<length(timeline)?Some(get(timeline, endIndex)):tryBasePointIntervalEndPrepared(baseRow, prepared, get(selected, length(selected)-1)));
      return o==null?null:Some({
        BaseRowId:baseRowId, 
        StartEventTimeUtc:startTime, 
        EndEventTimeExclusiveUtc:o.$0
      });
    }
  }
  else return null;
}
function viewportMaximumStart(total, window_1){
  const a=0;
  const a_1=0;
  const b=window_1.Count;
  let _1=Compare(a_1, b)===1?a_1:b;
  const b_1=total-_1;
  return Compare(a, b_1)===1?a:b_1;
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
function tryLoadedCoverageResolved(defaultView, data){
  let _1;
  const m=tryDecodeResolved(defaultView, data);
  if(m.$==0){
    const _2=m.$0;
    _1=_2!=null&&_2.$==1;
  }
  else _1=false;
  return _1?Some(m.$0.$0):null;
}
function tryLoadedCoverageWindowIntentAt(projection, targetStart, targetCount){
  const domainCount=observationDomainCount(projection);
  return domainCount<=0n||targetCount<=0||targetCount>4000||targetStart<0n||targetStart+BigInt(targetCount)>domainCount?null:tryWindowIntent(null, Some(projection.CoverageRevision), projection.QueryGeneration+1n, Some(targetStart), targetCount);
}
function tryLoadedCoverageEdgeIntent(edge, requestedCount, maximumVisibleBars, projection){
  let p;
  const domainCount=observationDomainCount(projection);
  const a=1;
  const a_1=4000;
  const b=Compare(a_1, maximumVisibleBars)===-1?a_1:maximumVisibleBars;
  const boundedMaximum=Compare(a, b)===1?a:b;
  if(domainCount<=0n)return null;
  else {
    const a_2=1;
    const b_1=Compare(boundedMaximum, requestedCount)===-1?boundedMaximum:requestedCount;
    let _1=Compare(a_2, b_1)===1?a_2:b_1;
    const b_2=BigInt(_1);
    let _2=Compare(domainCount, b_2)===-1?domainCount:b_2;
    let _3=_2+2147483648n;
    let _4=_3&4294967295n;
    let _5=_4-2147483648n;
    const count=Number(_5);
    if(edge.$==1){
      const a_3=0n;
      const b_3=domainCount-BigInt(count);
      let _6=Compare(a_3, b_3)===1?a_3:b_3;
      p=[_6, {$:1}];
    }
    else p=[0n, {$:0}];
    return tryWindowIntent(Some(p[1]), Some(projection.CoverageRevision), projection.QueryGeneration+1n, Some(p[0]), count);
  }
}
function setEditorInput(input_1, values){
  return sortBy((a) => a.Path, [input_1].concat(filter_1((current) => current.Path!=input_1.Path, values)));
}
function tryEditorInput(path, values){
  const o=tryFind((value) => value.Path==path, values);
  return o==null?null:Some(o.$0.Value);
}
function editorScalarText_1(a){
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
function queryViewportSelection(latestGeneration, intentGeneration, query, document, data){
  let _1;
  if(!Equals(intentGeneration, latestGeneration))return{$:4};
  else {
    const _2=query.FromUtc;
    const _3=query.ToUtcExclusive;
    switch(_2!=null&&_2.$==1?_3!=null&&_3.$==1?(_1=[_2.$0, _3.$0],1):2:_3==null?0:2){
      case 0:
        return{$:0};
      case 1:
        let _4;
        const _5=tryUtcTimestamp(_1[0]);
        const _6=tryUtcTimestamp(_1[1]);
        switch(_5!=null&&_5.$==1?_6!=null&&_6.$==1?_5.$0<_6.$0?(_4=[_5.$0, _6.$0],0):1:2:2){
          case 0:
            let low, high;
            const fromUtc=_4[0];
            const parsedPoints=map((_7) => {
              let _8;
              const temporal=_7[1];
              if(temporal==null){
                const o=tryUtcTimestamp(_7[0]);
                if(o==null)return null;
                else {
                  const instant=o.$0;
                  return Some([instant, instant, false]);
                }
              }
              else {
                const metadata=temporal.$0;
                const _9=tryUtcTimestamp(metadata.IntervalStartUtc);
                const _10=tryUtcTimestamp(metadata.IntervalEndUtc);
                return _9!=null&&_9.$==1&&(_10!=null&&_10.$==1&&(_9.$0<_10.$0&&(_8=[_10.$0, _9.$0],true)))?Some([_8[1], _8[0], true]):null;
              }
            }, referencePointsForDocument(document, data));
            if(exists((o) => o==null, parsedPoints))return{$:3, $0:"query-axis-invalid: one or more reference timestamps are not valid UTC values."};
            else {
              const points=choose((x) => x, parsedPoints);
              low=0;
              high=length(points);
              while(low<high)
                {
                  const middle=low+((high-low)/2>>0);
                  const p=get(points, middle);
                  if(p[2]?p[1]>fromUtc:p[0]>=fromUtc)high=middle;
                  else low=middle+1;
                }
              const first=low;
              low=0;
              high=length(points);
              while(low<high)
                {
                  const middle_1=low+((high-low)/2>>0);
                  if((get(points, middle_1))[0]>=_4[1])high=middle_1;
                  else low=middle_1+1;
                }
              return first>=low?{$:2}:{$:1, $0:{StartIndex:first, Count:low-first}};
            }
            break;
          case 1:
            return{$:3, $0:"query-range-invalid: FromUtc must be earlier than ToUtcExclusive."};
          case 2:
            return{$:3, $0:"query-range-invalid: FromUtc and ToUtcExclusive must be valid UTC timestamps."};
        }
        break;
      case 2:
        return{$:3, $0:"query-range-invalid: FromUtc and ToUtcExclusive must be supplied together."};
    }
  }
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
    const error_5=o_2.$0;
    _1=Some(error_5.ReasonCode+": "+error_5.Message);
  }
  return{
    Freshness:freshness, 
    Label:_2, 
    Watermark:_3, 
    Quality:_4, 
    Error:_1
  };
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
      CandleDataRefs:null, 
      Options:new FSharpMap("New", [])
    }];
  }
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
function arePreparedRowsReady(expectedRowCount, readyRowCount){
  return expectedRowCount>=0&&readyRowCount===expectedRowCount;
}
function tryOverviewTimeline(projection){
  const parsed=map((anchor) => tryUtcTimestamp(anchor.EventTimeUtc), projection.OverviewAnchors);
  if(exists((o) => o==null, parsed))return null;
  else {
    const timeline=choose((x) => x, parsed);
    return exists((_1) => _1[0]>=_1[1], pairwise(timeline))?null:Some(timeline);
  }
}
function selectionRatios(total, window_1){
  if(total<=0||window_1.Count<=0)return[0, 0];
  else {
    const bounded=clampWindow(1, 2147483647, total, window_1);
    return[bounded.StartIndex/total, (bounded.StartIndex+bounded.Count)/total];
  }
}
function overviewSelectionRatios(projection, activeReferenceTimeline, activeWindow){
  let _1;
  if(projection.OverviewAxisRef==projection.ActiveDetail.BaseAxisRef)return null;
  else {
    const m=tryOverviewTimeline(projection);
    if(m!=null&&m.$==1){
      if(length(m.$0)===0||length(activeReferenceTimeline)===0){
        m.$0;
        return null;
      }
      else {
        const overviewTimeline=m.$0;
        const detail=clampWindow(1, 4000, length(activeReferenceTimeline), activeWindow);
        if(detail.Count<=0)return null;
        else {
          const first=get(activeReferenceTimeline, detail.StartIndex);
          const last=get(activeReferenceTimeline, detail.StartIndex+detail.Count-1);
          const _2=tryUtcTimestamp(first);
          const _3=tryUtcTimestamp(last);
          if(_2!=null&&_2.$==1&&(_3!=null&&_3.$==1)){
            const _4=eventTimeSlot(overviewTimeline, first);
            const _5=eventTimeSlot(overviewTimeline, last);
            if(_4!=null&&_4.$==1&&(_5!=null&&_5.$==1&&(_1=[_4.$0, _5.$0],true))){
              const firstSlot=_1[0];
              let _6=firstSlot/length(overviewTimeline);
              const a=length(overviewTimeline);
              const a_1=firstSlot+1;
              const b=_1[1]+1;
              const b_1=Compare(a_1, b)===1?a_1:b;
              let _7=Compare(a, b_1)===-1?a:b_1;
              let _8=_7/length(overviewTimeline);
              let _9=[_6, _8];
              return Some(_9);
            }
            else return null;
          }
          else return null;
        }
      }
    }
    else return null;
  }
}
function coverageNavigatorWindow(activeReferenceLength, activeWindow, projection){
  const domainCount=observationDomainCount(projection);
  if(domainCount<=0n||domainCount>BigInt(2147483647))return null;
  else {
    const local=clampWindow(1, 4000, activeReferenceLength, activeWindow);
    const globalStart=projection.ActiveDetail.StartObservationOrdinal+BigInt(local.StartIndex);
    return globalStart<0n||globalStart>BigInt(2147483647)?null:Some([projection, Number((domainCount+2147483648n&4294967295n)-2147483648n), {StartIndex:Number((globalStart+2147483648n&4294967295n)-2147483648n), Count:local.Count}]);
  }
}
function overviewStripePlacementsPrepared(trace, prepared, referenceTimestamps){
  let o;
  const normalizedReferenceTimestamps=map(normalizeUtcTimestamp, referenceTimestamps);
  const m=tryDecode_2(trace.Options);
  if(m!=null&&m.$==1){
    const options=m.$0;
    const o_1=prepared.RawData.TryFind(trace.DataRef);
    const o_2=o_1==null?null:tryTemporalSeries(o_1.$0);
    if(o_2==null)o=null;
    else {
      const _1=o_2.$0[0];
      const _2=o_2.$0[1];
      const _3=o_2.$0[2];
      const o_3=filter((axis_1) => axis_1.Revision===_2, prepared.ResolvedAxes.TryFind(_1));
      if(o_3==null)o=null;
      else {
        const axis=o_3.$0;
        let _4=collect((_5) => {
          let _6;
          const position=_5[0];
          const _7=axis.Points.TryFind(position);
          const _8=decodeBucket(trace.DataRef, _5[1]);
          return _7!=null&&_7.$==1&&(_8.$==0&&(_6=[_8.$0, _7.$0],true))?choose((stripe) => {
            const o_4=overviewEventTimeSlot(normalizedReferenceTimestamps, stripe.EventTimeUtc);
            return o_4==null?null:Some({
              TraceId:trace.TraceId, 
              TargetTraceId:options.TargetTraceId, 
              CollisionGroup:options.CollisionGroup, 
              LayerOrder:options.LayerOrder, 
              Position:position, 
              SlotIndex:o_4.$0, 
              Stripe:stripe
            });
          }, _6[0]):[];
        }, _3);
        o=Some(_4);
      }
    }
    return o==null?[]:o.$0;
  }
  else return[];
}
function overviewStripeVisuals(placements){
  return sortBy((value) =>[value.SlotIndex, value.LayerOrder, value.TraceId], collect((_1) => mapi((_2, _3) =>({
    TraceId:_3[3], 
    TargetTraceId:_3[0], 
    CollisionGroup:_3[1], 
    LayerOrder:_3[4], 
    Lane:_2, 
    SlotIndex:_3[2], 
    Stripes:_3[5]
  }), sortBy((_2) =>[_2[4], _2[3]], _1[1])), groupBy((_1) =>[_1[0], _1[1], _1[2]], map((_1) => {
    const a=_1[0];
    return[a[0], a[1], a[2], a[3], a[4], map((a_1) => a_1.Stripe, _1[1])];
  }, groupBy((placement) =>[placement.TargetTraceId, placement.CollisionGroup, placement.SlotIndex, placement.TraceId, placement.LayerOrder], placements)))));
}
function overviewPointsForCoverage(projection){
  const scalar=(field_1, fields) => {
    const o=fields.TryFind(field_1);
    if(o==null)return null;
    else {
      const a=o.$0;
      return a.$==2?Some(a.$0):null;
    }
  };
  return choose((_1) => {
    let _2;
    const index=_1[0];
    const anchor=_1[1];
    const m=anchor.Value;
    switch(m.$==2?finiteNumber(m.$0)?(_2=m.$0,0):2:m.$==5?(_2=m.$0,1):2){
      case 0:
        return Some({
          Timestamp:anchor.EventTimeUtc, 
          Open:_2, 
          High:null, 
          Low:null, 
          Close:_2, 
          Volume:0, 
          SourceStartIndex:index, 
          SourceEndExclusive:index+1
        });
      case 1:
        let _3, _4, explicitWick;
        const o=scalar("close", _2);
        if(o==null)return null;
        else {
          const closeValue=o.$0;
          if(!finiteNumber(closeValue))return null;
          else {
            const o_1=scalar("open", _2);
            const openValue=o_1==null?closeValue:o_1.$0;
            const _5=scalar("high", _2);
            const _6=scalar("low", _2);
            if(_5!=null&&_5.$==1){
              if(_6!=null&&_6.$==1){
                const lowValue=_6.$0;
                _3=finiteNumber(_5.$0)&&finiteNumber(lowValue)&&(_4=[_5.$0, _6.$0],true);
              }
              else _3=false;
            }
            else _3=false;
            explicitWick=_3?Some([_4[0], _4[1]]):null;
            const o_2=scalar("volume", _2);
            const volumeValue=o_2==null?0:o_2.$0;
            return!finiteNumber(openValue)||!finiteNumber(volumeValue)?null:Some({
              Timestamp:anchor.EventTimeUtc, 
              Open:openValue, 
              High:explicitWick==null?null:Some(explicitWick.$0[0]), 
              Low:explicitWick==null?null:Some(explicitWick.$0[1]), 
              Close:closeValue, 
              Volume:volumeValue, 
              SourceStartIndex:index, 
              SourceEndExclusive:index+1
            });
          }
        }
        break;
      case 2:
        return null;
    }
  }, mapi((_1, _2) =>[_1, _2], projection.OverviewAnchors));
}
function candleSeriesForTracePrepared(trace, prepared){
  const m=trace.CandleDataRefs;
  if(m!=null&&m.$==1){
    const refs=m.$0;
    const valuesByTimestamp=(dataRef) => linePointsByTimestamp(lineSeriesPrepared(dataRef, prepared));
    const opens=lineSeriesPrepared(refs.OpenRef, prepared);
    const highs=valuesByTimestamp(refs.HighRef);
    const lows=valuesByTimestamp(refs.LowRef);
    const closes=valuesByTimestamp(refs.CloseRef);
    const volumes=valuesByTimestamp(refs.VolumeRef);
    return choose((openPoint) => {
      let _1;
      const _2=tryFindLinePoint(openPoint.Timestamp, highs);
      const _3=tryFindLinePoint(openPoint.Timestamp, lows);
      const _4=tryFindLinePoint(openPoint.Timestamp, closes);
      const _5=tryFindLinePoint(openPoint.Timestamp, volumes);
      return _2!=null&&_2.$==1&&(_3!=null&&_3.$==1&&(_4!=null&&_4.$==1&&(_5!=null&&_5.$==1&&(_1=[_4.$0, _2.$0, _3.$0, _5.$0],true))))?Some({
        Timestamp:openPoint.Timestamp, 
        Open:openPoint.Value, 
        High:_1[1].Value, 
        Low:_1[2].Value, 
        Close:_1[0].Value, 
        Volume:_1[3].Value, 
        Temporal:openPoint.Temporal
      }):null;
    }, opens);
  }
  else return candleSeriesPrepared(trace.DataRef, prepared);
}
function overviewPointsFromCandles(points){
  return choose((_1) => {
    const index=_1[0];
    const point=_1[1];
    return finiteNumber(point.Open)&&finiteNumber(point.High)&&finiteNumber(point.Low)&&finiteNumber(point.Close)&&finiteNumber(point.Volume)?Some({
      Timestamp:point.Timestamp, 
      Open:point.Open, 
      High:Some(point.High), 
      Low:Some(point.Low), 
      Close:point.Close, 
      Volume:point.Volume, 
      SourceStartIndex:index, 
      SourceEndExclusive:index+1
    }):null;
  }, mapi((_1, _2) =>[_1, _2], points));
}
function tryCoverageNavigatorWindowResolved(activeReferenceLength, activeWindow, defaultView, data){
  const o=tryLoadedCoverageResolved(defaultView, data);
  return o==null?null:coverageNavigatorWindow(activeReferenceLength, activeWindow, o.$0);
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
    const error_5=m.$0;
    return{
      State:"unavailable", 
      Title:"TA workspace unavailable", 
      Detail:error_5.ReasonCode+": "+error_5.Message, 
      IsError:true
    };
  }
  else {
    const error_6=m.$0;
    return{
      State:"recovering", 
      Title:"Restoring TA workspace", 
      Detail:error_6.ReasonCode+": "+error_6.Message, 
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
function navigatorDragMode(trackWidth, hitTargetWidth, leftRatio, rightRatio, pointerX){
  if(trackWidth<=0||hitTargetWidth<=0)return null;
  else {
    const p=navigatorSelectionBounds(trackWidth, leftRatio, rightRatio);
    const selectionWidth=p[1];
    const left=p[0];
    const right=left+selectionWidth;
    const a=trackWidth/2;
    const b=hitTargetWidth/2;
    const radius=Compare(a, b)===-1?a:b;
    const a_1=0;
    const b_1=left-radius;
    const interactionLeft=Compare(a_1, b_1)===1?a_1:b_1;
    const b_2=right+radius;
    const interactionRight=Compare(trackWidth, b_2)===-1?trackWidth:b_2;
    if(pointerX<interactionLeft||pointerX>interactionRight)return null;
    else if(selectionWidth<radius*2){
      const edgeZone=selectionWidth/3;
      return pointerX<=left+edgeZone?Some("resize-left"):pointerX>=right-edgeZone?Some("resize-right"):Some("move");
    }
    else return Math.abs(pointerX-left)<=radius?Some("resize-left"):Math.abs(pointerX-right)<=radius?Some("resize-right"):pointerX>=left&&pointerX<=right?Some("move"):null;
  }
}
function releaseNavigatorDraft(committed, preferPendingBoundary, renderedDraft, publishedDraft, pendingDraft){
  if(preferPendingBoundary){
    const o=pendingDraft==null?renderedDraft:(pendingDraft.$0,pendingDraft);
    const o_1=o==null?publishedDraft:(o.$0,o);
    return o_1==null?committed:o_1.$0;
  }
  else {
    const o_2=renderedDraft==null?publishedDraft:(renderedDraft.$0,renderedDraft);
    const o_3=o_2==null?pendingDraft:(o_2.$0,o_2);
    return o_3==null?committed:o_3.$0;
  }
}
function navigatorBoundaryDirection(total, committed, drag, rawDelta){
  if(total<=0||committed.Count<=0)return null;
  else {
    const startIndex=committed.StartIndex;
    const endExclusive=startIndex+committed.Count;
    const p=drag=="move"?[startIndex+rawDelta, endExclusive+rawDelta]:drag=="resize-left"?[startIndex+rawDelta, endExclusive]:drag=="resize-right"?[startIndex, endExclusive+rawDelta]:[startIndex, endExclusive];
    return p[0]<0?Some({$:0}):p[1]>total?Some({$:1}):null;
  }
}
function commitWindowBounds(minimumCount, maximumCount, total, draft){
  const next=clampWindow(minimumCount, maximumCount, total, draft);
  return[next.StartIndex===viewportMaximumStart(total, next), next];
}
function tryLocalWindowForLoadedCoverage(activeReferenceLength, projection, globalWindow){
  const activeStart=projection.ActiveDetail.StartObservationOrdinal;
  const a=0;
  const b=projection.ActiveDetail.ObservationCount;
  const b_1=Compare(activeReferenceLength, b)===-1?activeReferenceLength:b;
  let _1=Compare(a, b_1)===1?a:b_1;
  let _2=BigInt(_1);
  const activeEnd=activeStart+_2;
  const targetStart=BigInt(globalWindow.StartIndex);
  if(activeReferenceLength<=0||globalWindow.Count<=0||targetStart<activeStart||targetStart+BigInt(globalWindow.Count)>activeEnd)return null;
  else {
    const localStart=targetStart-activeStart;
    return localStart<0n||localStart>BigInt(2147483647)?null:Some({StartIndex:Number((localStart+2147483648n&4294967295n)-2147483648n), Count:globalWindow.Count});
  }
}
function tryLoadedCoverageWindowIntent(projection, globalWindow){
  return tryLoadedCoverageWindowIntentAt(projection, BigInt(globalWindow.StartIndex), globalWindow.Count);
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
function tryViewAllCoverageIntent(maximumVisibleBars, projection){
  const domainCount=observationDomainCount(projection);
  const a=1;
  const a_1=4000;
  const b=Compare(a_1, maximumVisibleBars)===-1?a_1:maximumVisibleBars;
  const boundedMaximum=Compare(a, b)===1?a:b;
  if(domainCount<=0n)return null;
  else {
    const b_1=BigInt(boundedMaximum);
    let _1=Compare(domainCount, b_1)===-1?domainCount:b_1;
    let _2=_1+2147483648n;
    let _3=_2&4294967295n;
    let _4=_3-2147483648n;
    const count=Number(_4);
    let _5=Some(projection.CoverageRevision);
    const a_2=0n;
    const b_2=domainCount-BigInt(count);
    let _6=Compare(a_2, b_2)===1?a_2:b_2;
    let _7=Some(_6);
    return tryWindowIntent(null, _5, projection.QueryGeneration+1n, _7, count);
  }
}
function tryAdjacentCoverageRange(direction, maximumBasePoints, document, prepared){
  let _1, o, _2, _3, _4;
  const o_1=document.BaseRowId;
  if(o_1==null)return null;
  else {
    const baseRowId=o_1.$0;
    const o_2=tryFind((row) => row.RowId==baseRowId, document.Rows);
    if(o_2==null)return null;
    else {
      const baseRow=o_2.$0;
      const timeline=referenceTimelineForDocumentPrepared(document, prepared);
      const query=queryDraft(document.DefaultView);
      const _5=tryHead(timeline);
      const _6=tryLast(timeline);
      switch(direction.$==1?_6!=null&&_6.$==1?(_1=_6.$0,2):3:_5!=null&&_5.$==1?(_5.$0,IsNullOrWhiteSpace(query.FromUtc)?(_1=_5.$0,0):(_1=_5.$0,1)):3){
        case 0:
          o=Some([_1, _1, true]);
          break;
        case 1:
          o=Some([query.FromUtc, _1, false]);
          break;
        case 2:
          const m=tryBasePointIntervalEndPrepared(baseRow, prepared, _1);
          o=m!=null&&m.$==1?Some([m.$0, query.ToUtcExclusive, false]):null;
          break;
        case 3:
          o=null;
          break;
      }
      if(o==null)return null;
      else {
        const _7=o.$0[0];
        const _8=o.$0[1];
        const _9=o.$0[2];
        const _10=tryUtcTimestamp(_7);
        const _11=tryUtcTimestamp(_8);
        if(_10!=null&&_10.$==1){
          if(_11!=null&&_11.$==1){
            const normalizedStart=_10.$0;
            const normalizedEnd=_11.$0;
            _2=(Compare(normalizedStart, normalizedEnd)<0||_9&&normalizedStart==normalizedEnd)&&(_3=[_11.$0, _10.$0],true);
          }
          else _2=false;
        }
        else _2=false;
        if(_2){
          const a=1;
          let _12=Compare(a, maximumBasePoints)===1?a:maximumBasePoints;
          if(_9){
            const a_1=1;
            let _13=Compare(a_1, maximumBasePoints)===1?a_1:maximumBasePoints;
            _4=tryProviderOpenEarlierWindowIntent(null, 0n, _13);
          }
          else _4=null;
          let _14={
            BaseRowId:baseRowId, 
            StartEventTimeUtc:_7, 
            EndEventTimeExclusiveUtc:_8, 
            MaximumBasePoints:_12, 
            CoverageIntent:_4
          };
          return Some(_14);
        }
        else return null;
      }
    }
  }
}
function tryAdjacentCoverageIntent(direction, maximumVisibleBars, activeReferenceLength, activeWindow, projection){
  let _1;
  const local=clampWindow(1, maximumVisibleBars, activeReferenceLength, activeWindow);
  const a=1;
  const b=local.Count;
  const b_1=Compare(maximumVisibleBars, b)===-1?maximumVisibleBars:b;
  const count=Compare(a, b_1)===1?a:b_1;
  const currentStart=projection.ActiveDetail.StartObservationOrdinal+BigInt(local.StartIndex);
  const domainCount=observationDomainCount(projection);
  let _2=Some(direction.$==1?{$:1}:{$:0});
  let _3=Some(projection.CoverageRevision);
  if(direction.$==1){
    const a_1=0n;
    const b_2=domainCount-BigInt(count);
    const a_2=Compare(a_1, b_2)===1?a_1:b_2;
    const b_3=currentStart+BigInt(count);
    _1=Compare(a_2, b_3)===-1?a_2:b_3;
  }
  else {
    const a_3=0n;
    const b_4=currentStart-BigInt(count);
    _1=Compare(a_3, b_4)===1?a_3:b_4;
  }
  let _4=Some(_1);
  return tryWindowIntent(_2, _3, projection.QueryGeneration+1n, _4, count);
}
function prepareDataIncrementalScheduled(schedule, previous, data, onCompleted){
  let axes, axisChanges, resolved;
  const entries=ofSeq(ToSeq(data));
  axes=new FSharpMap("New", []);
  axisChanges=new FSharpMap("New", []);
  resolved=new FSharpMap("New", []);
  function prepareSeries(index){
    return index>=length(entries)?onCompleted({
      RawData:data, 
      ResolvedAxes:axes, 
      ResolvedSeries:resolved
    }):schedule(() => {
      let next, _1, _2, _3, _4, _5;
      const p=get(entries, index);
      const value=p[1];
      const dataRef=p[0];
      if(value.$==4){
        const rawPoints=value.$0;
        const _6=previous.RawData.TryFind(dataRef);
        const _7=previous.ResolvedSeries.TryFind(dataRef);
        next=_6!=null&&_6.$==1&&(_6.$0.$==4&&(_7!=null&&_7.$==1&&(_1=[_6.$0.$0, _7.$0],true)))?Some(updateInlineSeries(_1[0], _1[1], rawPoints)):Some(map((item) => {
          const p_1=pointPayload_1(item);
          return{Payload:p_1[1], Temporal:p_1[0]};
        }, rawPoints));
      }
      else {
        const m=tryTemporalSeriesRaw(value);
        if(m==null)next=null;
        else {
          const rawPoints_1=m.$0[2];
          const axisRevision=m.$0[1];
          const axisRef=m.$0[0];
          const m_1=axes.TryFind(axisRef);
          if(m_1!=null&&m_1.$==1&&(m_1.$0.Revision===axisRevision&&(_2=m_1.$0,true))){
            const _8=previous.RawData.TryFind(dataRef);
            const _9=previous.ResolvedSeries.TryFind(dataRef);
            if(_8!=null&&_8.$==1&&(_9!=null&&_9.$==1&&(_3=[_9.$0, _8.$0],true))){
              const m_2=tryTemporalSeriesRaw(_3[1]);
              if(m_2!=null&&m_2.$==1&&(m_2.$0,m_2.$0[0]==axisRef&&(_4=[m_2.$0[0], m_2.$0[2]],true))){
                const o=axisChanges.TryFind(axisRef);
                let _10=o==null?null:o.$0;
                let _11=updateTemporalSeries(_4[1], _3[0], rawPoints_1, _2.Points, _10);
                next=Some(_11);
              }
              else next=Some(resolveTemporalPoints(_2.Points, rawPoints_1));
            }
            else next=Some(resolveTemporalPoints(_2.Points, rawPoints_1));
          }
          else next=Some([]);
        }
      }
      if(next==null)_5=null;
      else {
        const v=next.$0;
        _5=void(resolved=resolved.Add_1(dataRef, v));
      }
      return prepareSeries(index+1);
    });
  }
  function prepareAxes(index){
    return index>=length(entries)?prepareSeries(0):schedule(() => {
      let _1;
      const m=updateAxis(previous, (get(entries, index))[1]);
      if(m==null)_1=null;
      else {
        const changedPositions=m.$0[2];
        const axisRef=m.$0[0];
        const v=m.$0[1];
        axes=axes.Add_1(axisRef, v);
        _1=void(axisChanges=axisChanges.Add_1(axisRef, changedPositions));
      }
      return prepareAxes(index+1);
    });
  }
  return prepareAxes(0);
}
function initialViewportWindow(minimumCount, fallbackCount, maximumCount, total, defaultView){
  let _1;
  const m=defaultView.TryFind("visibleBars");
  if(m!=null&&m.$==1){
    if(m.$0.$==2){
      const value=m.$0.$0;
      _1=!(isNaN(value)||Math.abs(value)===Infinity)&&value>=1&&value<=2147483647&&Math.floor(value)===value?toInt(m.$0.$0):fallbackCount;
    }
    else _1=fallbackCount;
  }
  else _1=fallbackCount;
  let _2={StartIndex:0, Count:_1};
  return resolveWindow(minimumCount, maximumCount, total, true, _2);
}
function coverageExtended(direction, oldTimeline, newTimeline){
  if(length(oldTimeline)===0||length(newTimeline)<=length(oldTimeline))return false;
  else if(direction.$==1){
    const x=get(oldTimeline, length(oldTimeline)-1);
    const o=tryFindIndex((y) => x==y, newTimeline);
    return o==null?false:o.$0<length(newTimeline)-1;
  }
  else {
    const x_1=get(oldTimeline, 0);
    const o_1=tryFindIndex((y) => x_1==y, newTimeline);
    return o_1==null?false:o_1.$0>0;
  }
}
function tryReanchorWindow(minimumCount, maximumCount, delta, oldTimeline, newTimeline, oldWindow){
  let o;
  const oldBounded=clampWindow(minimumCount, maximumCount, length(oldTimeline), oldWindow);
  const o_1=tryItem(oldBounded.StartIndex, oldTimeline);
  if(o_1==null)o=null;
  else {
    const anchor=o_1.$0;
    o=tryFindIndex((y) => anchor==y, newTimeline);
  }
  return o==null?null:Some(clampWindow(minimumCount, maximumCount, length(newTimeline), {StartIndex:o.$0+delta, Count:oldBounded.Count}));
}
function rowHeightStorageKey(a, rowId){
  return a.$0+":"+rowId;
}
function rowHeightBounds(row, traces){
  return exists((trace) => Equals(trace.Kind, {$:0}), traces)?{
    Minimum:180, 
    Maximum:720, 
    DefaultHeight:clamp(180, 250, toInt(Math.round(250*row.HeightWeight)))
  }:{
    Minimum:96, 
    Maximum:480, 
    DefaultHeight:clamp(96, 250, toInt(Math.round(112*row.HeightWeight)))
  };
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
function clamp(minimum, maximum, value){
  const b=Compare(maximum, value)===-1?maximum:value;
  return Compare(minimum, b)===1?minimum:b;
}
function traceTopologyTimestampsPrepared(trace, prepared){
  if(isOverlayTraceKind(trace.Kind))return[];
  else {
    const temporalPositions=choose((point) => {
      const o=point.Temporal;
      return o==null?null:Some(o.$0.IntervalStartUtc);
    }, resolvedSeriesPrepared(trace.DataRef, prepared));
    return length(temporalPositions)>0?temporalPositions:traceTimestampsPrepared(trace, prepared);
  }
}
function tryLoadedCoverage(defaultView){
  let _1;
  const m=tryDecode(defaultView);
  if(m.$==0){
    const _2=m.$0;
    _1=_2!=null&&_2.$==1;
  }
  else _1=false;
  return _1?Some(m.$0.$0):null;
}
function pointPayload_1(value){
  const m=tryTemporalPoint(value);
  return m==null?[null, Some(value)]:[Some(m.$0[0]), m.$0[1]];
}
function tryTemporalSeries(value){
  const o=tryTemporalSeriesRaw(value);
  if(o==null)return null;
  else {
    const _1=o.$0[0];
    const _2=o.$0[1];
    const _3=o.$0[2];
    const decoded=choose(tryTemporalSeriesPoint, _3);
    return length(decoded)!==length(_3)?null:Some([_1, _2, decoded]);
  }
}
function prepareAxis(value){
  const o=tryTemporalAxisRaw(value);
  if(o==null)return null;
  else {
    const _1=o.$0[0];
    const _2=o.$0[1];
    const _3=o.$0[2];
    const decoded=choose(tryTemporalAxisPoint, _3);
    return length(decoded)!==length(_3)?null:Some([_1, {
      Revision:_2, 
      RawPoints:_3, 
      Points:OfArray(decoded)
    }]);
  }
}
function referenceTimelinePrepared(rows, prepared){
  const o=tryHead(sortByDescending((_1) =>[length(_1[1]), Equals(_1[0].Kind, {$:0})], filter_1((_1) => length(_1[1])>0, map((trace) =>[trace, distinct(traceTimestampsPrepared(trace, prepared))], filter_1((a) => a.Visible, collect(effectiveTraces, filter_1((a) => a.Visible, rows)))))));
  const o_1=o==null?null:Some(o.$0[1]);
  return o_1==null?[]:o_1.$0;
}
function rowTimelinePrepared(row, prepared){
  const traces=filter_1((a) => a.Visible, effectiveTraces(row));
  const o=tryFind((trace) => trace.DataRef==row.DataRef, traces);
  const o_1=o==null?tryHead(traces):(o.$0,o);
  const o_2=o_1==null?null:Some(distinct(traceTimestampsPrepared(o_1.$0, prepared)));
  return o_2==null?[]:o_2.$0;
}
function tryBaseRow(document){
  const o=document.BaseRowId;
  if(o==null)return null;
  else {
    const baseRowId=o.$0;
    const o_1=tryFind((row) => row.Visible&&row.RowId==baseRowId, document.Rows);
    return o_1==null?null:Some([baseRowId, o_1.$0]);
  }
}
function tryBasePointIntervalEndPrepared(row, prepared, timestamp){
  const o=tryFind((trace_1) => trace_1.DataRef==row.DataRef, filter_1((a) => a.Visible, effectiveTraces(row)));
  if(o==null)return null;
  else {
    const trace=o.$0;
    const m=trace.CandleDataRefs;
    let _1=m==null?trace.DataRef:m.$0.OpenRef;
    let _2=resolvedSeriesPrepared(_1, prepared);
    return tryPick((point) => {
      const o_1=filter((temporal) => presentationTimestamp(temporal)==timestamp, point.Temporal);
      return o_1==null?null:Some(o_1.$0.IntervalEndUtc);
    }, _2);
  }
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
function tryUtcTimestamp(value){
  let fractionValid;
  const trimmed=value==null?"":Trim(value);
  const candidate=trimmed.length===10&&trimmed[4]==="-"&&trimmed[7]==="-"?trimmed+"T00:00:00Z":trimmed;
  const suffixStart=EndsWith(candidate, "Z")?candidate.length-1:EndsWith(candidate, "+00:00")?candidate.length-6:-1;
  const digitsValid=suffixStart>=19&&candidate.length>=20&&candidate[4]==="-"&&candidate[7]==="-"&&(candidate[10]==="T"||candidate[10]==="t")&&candidate[13]===":"&&candidate[16]===":"&&forall((index) => {
    const character=candidate[index];
    return character>="0"&&character<="9";
  }, [0, 1, 2, 3, 5, 6, 8, 9, 11, 12, 14, 15, 17, 18]);
  const fraction=suffixStart===19?Some(""):suffixStart>20&&candidate[19]==="."?Some(Substring(candidate, 20, suffixStart-20)):null;
  if(fraction==null)fractionValid=false;
  else {
    const digits=fraction.$0;
    fractionValid=digits.length<=7&&forall_2((character) => character>="0"&&character<="9", digits);
  }
  if(!digitsValid||!fractionValid)return null;
  else {
    const number_1=(start, count) => Parse(Substring(candidate, start, count));
    const month=number_1(5, 2);
    const day=number_1(8, 2);
    const hour=number_1(11, 2);
    const minute=number_1(14, 2);
    const second=number_1(17, 2);
    const year=number_1(0, 4);
    if(day<1||day>(month===1?31:month===2?year%400===0||year%4===0&&year%100!==0?29:28:month===3?31:month===4?30:month===5?31:month===6?30:month===7?31:month===8?31:month===9?30:month===10?31:month===11?30:month===12?31:0)||hour>23||minute>59||second>59)return null;
    else {
      const normalizedFraction=Substring((fraction==null?"":fraction.$0)+"0000000", 0, 7);
      return Some(Substring(candidate, 0, 10)+"T"+Substring(candidate, 11, 8)+"."+normalizedFraction);
    }
  }
}
function referencePointsForDocument(document, data){
  const m=document.BaseRowId;
  if(m==null){
    const o=tryHead(sortByDescending((_1) =>[length(_1[1]), Equals(_1[0].Kind, {$:0})], filter_1((_1) => length(_1[1])>0, map((trace) =>[trace, traceReferencePoints(trace, data)], filter_1((a) => a.Visible, collect(effectiveTraces, filter_1((a) => a.Visible, document.Rows)))))));
    const o_1=o==null?null:Some(o.$0[1]);
    return o_1==null?[]:o_1.$0;
  }
  else {
    const baseRowId=m.$0;
    const o_2=tryFind((row) => row.Visible&&row.RowId==baseRowId, document.Rows);
    const o_3=o_2==null?null:Some(rowReferencePoints(o_2.$0, data));
    return o_3==null?[]:o_3.$0;
  }
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
function temporalDetailWith(formatEventTime, metadata){
  const o=metadata.AvailableAtUtc;
  const o_1=o==null?null:Some(formatEventTime(o.$0));
  const availability=o_1==null?"unknown":o_1.$0;
  const o_2=metadata.Quality;
  let _1=o_2==null?"unknown":o_2.$0;
  let _2=String(_1);
  let _3=String(metadata.ScaleKey)+" | "+String(metadata.Finality)+" | quality "+_2;
  let _4=_3+" | frontier ";
  let _5=_4+String(formatEventTime(metadata.ObservedThroughUtc));
  let _6=_5+" | available ";
  return _6+String(availability);
}
function rowTemporalMetadataPrepared(row, prepared){
  return distinctBy((value) =>[value.ScaleKey, value.Finality, value.ObservedThroughUtc, value.Quality], choose((trace) => latestTemporalMetadataPrepared(trace, prepared), filter_1((trace) => trace.Visible&&!isOverlayTraceKind(trace.Kind), effectiveTraces(row))));
}
function compactOverviewCandles(maximumCount, values){
  const finiteValues=choose((value) => {
    let _1, _2, authoredWick;
    if(finiteNumber(value.Open)&&finiteNumber(value.Close)&&finiteNumber(value.Volume)){
      const _3=value.High;
      const _4=value.Low;
      if(_3!=null&&_3.$==1){
        if(_4!=null&&_4.$==1){
          const low=_4.$0;
          _1=finiteNumber(_3.$0)&&finiteNumber(low)&&(_2=[_3.$0, _4.$0],true);
        }
        else _1=false;
      }
      else _1=false;
      authoredWick=_1?Some([_2[0], _2[1]]):null;
      return Some({
        Timestamp:value.Timestamp, 
        Open:value.Open, 
        High:authoredWick==null?null:Some(authoredWick.$0[0]), 
        Low:authoredWick==null?null:Some(authoredWick.$0[1]), 
        Close:value.Close, 
        Volume:value.Volume, 
        SourceStartIndex:value.SourceStartIndex, 
        SourceEndExclusive:value.SourceEndExclusive
      });
    }
    else return null;
  }, values);
  return maximumCount<=0||length(finiteValues)===0?[]:length(finiteValues)<=maximumCount?finiteValues.slice():init(maximumCount, (bucketIndex) => {
    let allHaveWicks, high, low, volume;
    const startIndex=bucketIndex*length(finiteValues)/maximumCount>>0;
    const endExclusive=(bucketIndex+1)*length(finiteValues)/maximumCount>>0;
    const first=get(finiteValues, startIndex);
    const last=get(finiteValues, endExclusive-1);
    allHaveWicks=true;
    high=null;
    low=null;
    volume=0;
    for(let index=startIndex, _1=endExclusive-1;index<=_1;index++)((() => {
      let _2, o, o_1;
      const point=get(finiteValues, index);
      volume=volume+point.Volume;
      const _3=point.High;
      const _4=point.Low;
      if(_3!=null&&_3.$==1&&(_4!=null&&_4.$==1&&(_2=[_3.$0, _4.$0],true))){
        const pointHigh=_2[0];
        const pointLow=_2[1];
        if(high==null)o=null;
        else {
          const current=high.$0;
          let _5=Compare(current, pointHigh)===1?current:pointHigh;
          o=Some(_5);
        }
        let _6=o==null?pointHigh:o.$0;
        high=Some(_6);
        if(low==null)o_1=null;
        else {
          const current_1=low.$0;
          let _7=Compare(current_1, pointLow)===-1?current_1:pointLow;
          o_1=Some(_7);
        }
        let _8=o_1==null?pointLow:o_1.$0;
        low=Some(_8);
        return;
      }
      else {
        allHaveWicks=false;
        return;
      }
    })());
    return{
      Timestamp:last.Timestamp, 
      Open:first.Open, 
      High:allHaveWicks?high:null, 
      Low:allHaveWicks?low:null, 
      Close:last.Close, 
      Volume:volume, 
      SourceStartIndex:first.SourceStartIndex, 
      SourceEndExclusive:last.SourceEndExclusive
    };
  });
}
function paddedRange(fallbackLow, fallbackHigh, values){
  let found, low, high, _1;
  found=false;
  low=fallbackLow;
  high=fallbackHigh;
  const e=Get(values);
  try {
    while(e.MoveNext())
      {
        const value=e.Current;
        if(finiteNumber(value))if(found){
          value<low?low=value:void 0;
          value>high?high=value:void 0;
        }
        else {
          found=true;
          low=value;
          high=value;
        }
      }
    _1=void 0;
  }
  finally {
    const _2=e;
    if(typeof _2=="object"&&isIDisposable(_2))e.Dispose();
  }
  if(!found)return[fallbackLow, fallbackHigh];
  else if(low===high)return[low-1, high+1];
  else {
    const a=(high-low)*0.08;
    const b=0.0001;
    const padding=Compare(a, b)===1?a:b;
    return[low-padding, high+padding];
  }
}
function normalize(low, high, top, height, value){
  return low===high?top+height/2:top+height-(value-low)/(high-low)*height;
}
function navigatorSelectionBounds(trackWidth, leftRatio, rightRatio){
  const a=0;
  const width=Compare(a, trackWidth)===1?a:trackWidth;
  const a_1=0;
  const a_2=1;
  const b=Compare(a_2, leftRatio)===-1?a_2:leftRatio;
  let _1=Compare(a_1, b)===1?a_1:b;
  const left=_1*width;
  const a_3=0;
  const a_4=1;
  const b_1=Compare(a_4, rightRatio)===-1?a_4:rightRatio;
  let _2=Compare(a_3, b_1)===1?a_3:b_1;
  const b_2=_2*width;
  let _3=Compare(left, b_2)===1?left:b_2;
  let _4=_3-left;
  return[left, _4];
}
function adaptiveTimeLabels(minimumSpacing, width, timestamps){
  if(length(timestamps)===0)return[];
  else if(length(timestamps)===1)return[[0, get(timestamps, 0)]];
  else {
    const a=48;
    const spacing=Compare(a, minimumSpacing)===1?a:minimumSpacing;
    const a_1=length(timestamps);
    const a_2=2;
    const a_3=16;
    const b=toInt(Math.floor((Compare(spacing, width)===1?spacing:width)/spacing));
    const b_1=Compare(a_3, b)===-1?a_3:b;
    const b_2=Compare(a_2, b_1)===1?a_2:b_1;
    const count=Compare(a_1, b_2)===-1?a_1:b_2;
    return distinctBy((t) => t[0], ofSeq(delay(() => collect_1((tickIndex) => {
      const sourceIndex=toInt(Math.round(tickIndex*(length(timestamps)-1)/(count-1)));
      return[[sourceIndex, get(timestamps, sourceIndex)]];
    }, range(0, count-1)))));
  }
}
function eventTimeSlot(timeline, eventTime){
  const m=tryUtcTimestamp(eventTime);
  if(m!=null&&m.$==1){
    const target=m.$0;
    if(length(timeline)===0)return null;
    else {
      const index=upperTimestampBound(timeline, target)-1;
      if(index<0)return null;
      else {
        const a=length(timeline)-1;
        let _1=Compare(a, index)===-1?a:index;
        return Some(_1);
      }
    }
  }
  else return null;
}
function overviewEventTimeSlot(timeline, eventTime){
  let low, high;
  if(length(timeline)===0||IsNullOrWhiteSpace(eventTime))return null;
  else {
    const target=normalizeUtcTimestamp(eventTime);
    low=0;
    high=length(timeline);
    while(low<high)
      {
        const middle=low+((high-low)/2>>0);
        if(Compare(get(timeline, middle), target)<=0)low=middle+1;
        else high=middle;
      }
    const index=low-1;
    if(index<0)return null;
    else {
      const a=length(timeline)-1;
      let _1=Compare(a, index)===-1?a:index;
      return Some(_1);
    }
  }
}
function finiteNumber(value){
  return!(isNaN(value)||Math.abs(value)===Infinity);
}
function lineSeriesPrepared(dataRef, prepared){
  return lineSeriesFromResolved(resolvedSeriesPrepared(dataRef, prepared));
}
function linePointsByTimestamp(values){
  const lookup=new Dictionary("New_5");
  for(let i=0, _1=values.length-1;i<=_1;i++){
    const point=get(values, i);
    lookup.set_Item(point.Timestamp, point);
  }
  return lookup;
}
function tryFindLinePoint(key_1, lookup){
  let o;
  const m=(o=null,[lookup.TryGetValue(key_1, {get:() => o, set:(v) => {
    o=v;
  }}), o]);
  return m[0]?Some(m[1]):null;
}
function candleSeriesPrepared(dataRef, prepared){
  return candleSeriesFromResolved(resolvedSeriesPrepared(dataRef, prepared));
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
      return append_2(minimum!=null&&minimum.$==1&&(length(indexes)<minimum.$0&&(_2=minimum.$0,true))?[String(path)+" requires at least "+String(_2)+" item(s)."]:[], delay(() => {
        let _3;
        return append_2(maximum!=null&&maximum.$==1&&(length(indexes)>maximum.$0&&(_3=maximum.$0,true))?[String(path)+" allows at most "+String(_3)+" item(s)."]:[], delay(() => collect_1((index) => editorSubmissionErrors(String(path)+"["+String(index)+"]", true, itemKind, values), indexes)));
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
          return ofSeq(delay(() => append_2(isNaN(value)||Math.abs(value)===Infinity||(value<0?Math.ceil(value):Math.floor(value))!==value?[path+" must be an integer."]:[], delay(() => {
            let _2;
            return append_2(minimum_1!=null&&minimum_1.$==1&&(value<Number(minimum_1.$0)&&(_2=minimum_1.$0,true))?[path+" is below its minimum."]:[], delay(() => {
              let _3;
              return maximum_1!=null&&maximum_1.$==1&&(value>Number(maximum_1.$0)&&(_3=maximum_1.$0,true))?[path+" exceeds its maximum."]:[];
            }));
          }))));
        case 3:
          const maximum_2=_1[0];
          const minimum_2=_1[1];
          const value_1=_1[2];
          return ofSeq(delay(() => append_2(isNaN(value_1)||Math.abs(value_1)===Infinity?[path+" must be finite."]:[], delay(() => {
            let _2;
            return append_2(minimum_2!=null&&minimum_2.$==1&&(value_1<minimum_2.$0&&(_2=minimum_2.$0,true))?[path+" is below its minimum."]:[], delay(() => {
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
function updateInlineSeries(previousRaw, previousResolved, rawPoints){
  const mapPoint=(item) => {
    const p=pointPayload_1(item);
    return{Payload:p[1], Temporal:p[0]};
  };
  if(previousRaw===rawPoints)return previousResolved;
  else if(length(previousResolved)!==length(previousRaw))return map(mapPoint, rawPoints);
  else {
    const prefix=sharedPrefixLength(previousRaw, rawPoints);
    const a=length(previousRaw);
    const b=length(rawPoints);
    let _1=Compare(a, b)===-1?a:b;
    return!(prefix===_1||length(previousRaw)===length(rawPoints))?map(mapPoint, rawPoints):take(prefix, previousResolved).concat(map(mapPoint, skip(prefix, rawPoints)));
  }
}
function tryTemporalSeriesRaw(value){
  let _1;
  const o=tryObject(value);
  if(o==null)return null;
  else {
    const fields=o.$0;
    if(!Equals(objectText("_type", fields), Some("temporal-series.v1")))return null;
    else {
      const _2=requiredObjectText("axisRef", fields);
      const _3=nonNegativeInteger("axisRevision", fields);
      const _4=fields.TryFind("points");
      return _2!=null&&_2.$==1&&(_3!=null&&_3.$==1&&(_4!=null&&_4.$==1&&(_4.$0.$==4&&(_1=[_2.$0, _4.$0.$0, _3.$0],true))))?Some([_1[0], _1[2], _1[1]]):null;
    }
  }
}
function updateTemporalSeries(previousRaw, previousResolved, rawPoints, axisPoints, changedAxisPositions){
  let baseResolved, _1;
  const full=() => resolveTemporalPoints(axisPoints, rawPoints);
  if(length(previousResolved)!==length(previousRaw))baseResolved=full();
  else if(previousRaw===rawPoints)baseResolved=previousResolved;
  else {
    const prefix=sharedPrefixLength(previousRaw, rawPoints);
    const a=length(previousRaw);
    const b=length(rawPoints);
    let _2=Compare(a, b)===-1?a:b;
    if(!(prefix===_2||length(previousRaw)===length(rawPoints)))baseResolved=full();
    else {
      const suffix=resolveTemporalPoints(axisPoints, skip(prefix, rawPoints));
      baseResolved=length(suffix)!==length(rawPoints)-prefix?full():take(prefix, previousResolved).concat(suffix);
    }
  }
  if(changedAxisPositions==null)return full();
  else if(!changedAxisPositions.$0.IsEmpty&&length(baseResolved)===length(rawPoints)){
    const positions=changedAxisPositions.$0;
    const next=baseResolved.slice();
    const e=Get(positions);
    try {
      while(e.MoveNext())
        {
          const position=e.Current;
          const m=tryFindTemporalPointIndex(position, rawPoints);
          if(m==null){ }
          else {
            const index=m.$0;
            const _3=tryTemporalSeriesPoint(get(rawPoints, index));
            const _4=axisPoints.TryFind(position);
            if(_3!=null&&_3.$==1)if(_4!=null&&_4.$==1)set(next, index, {Payload:Some(_3.$0[1]), Temporal:Some(_4.$0)});
          }
        }
      _1=void 0;
    }
    finally {
      const _5=e;
      if(typeof _5=="object"&&isIDisposable(_5))e.Dispose();
    }
    return next;
  }
  else return baseResolved;
}
function resolveTemporalPoints(axisPoints, rawPoints){
  return choose((point) => {
    const o=tryTemporalSeriesPoint(point);
    if(o==null)return null;
    else {
      const _1=o.$0[0];
      const _2=o.$0[1];
      const o_1=axisPoints.TryFind(_1);
      return o_1==null?null:Some({Payload:Some(_2), Temporal:Some(o_1.$0)});
    }
  }, rawPoints);
}
function updateAxis(previous, value){
  const m=tryTemporalAxisRaw(value);
  if(m!=null&&m.$==1){
    const revision=m.$0[1];
    const rawPoints=m.$0[2];
    const axisRef=m.$0[0];
    const m_1=previous.ResolvedAxes.TryFind(axisRef);
    if(m_1==null){
      const o=prepareAxis(value);
      return o==null?null:Some([o.$0[0], o.$0[1], null]);
    }
    else if(m_1.$0.RawPoints===rawPoints){
      const oldAxis=m_1.$0;
      return Some([axisRef, {
        Revision:revision, 
        RawPoints:oldAxis.RawPoints, 
        Points:oldAxis.Points
      }, Some(new FSharpSet("New_2", null))]);
    }
    else {
      const oldAxis_1=m_1.$0;
      const prefix=sharedPrefixLength(oldAxis_1.RawPoints, rawPoints);
      const a=length(oldAxis_1.RawPoints);
      const b=length(rawPoints);
      let _1=Compare(a, b)===-1?a:b;
      if(prefix===_1||length(oldAxis_1.RawPoints)===length(rawPoints)){
        const oldSuffix=choose(tryTemporalAxisPoint, skip(prefix, oldAxis_1.RawPoints));
        const newSuffix=choose(tryTemporalAxisPoint, skip(prefix, rawPoints));
        if(length(oldSuffix)===length(oldAxis_1.RawPoints)-prefix&&length(newSuffix)===length(rawPoints)-prefix){
          const changedPositions=new FSharpSet("New_2", OfSeq(map((t) => t[0], oldSuffix).concat(map((t) => t[0], newSuffix))));
          return Some([axisRef, {
            Revision:revision, 
            RawPoints:rawPoints, 
            Points:fold((_2, _3) => _2.Add_1(_3[0], _3[1]), fold((_2, _3) => _2.Remove_1(_3[0]), oldAxis_1.Points, oldSuffix), newSuffix)
          }, Some(changedPositions)]);
        }
        else {
          const o_1=prepareAxis(value);
          return o_1==null?null:Some([o_1.$0[0], o_1.$0[1], null]);
        }
      }
      else {
        const o_2=prepareAxis(value);
        return o_2==null?null:Some([o_2.$0[0], o_2.$0[1], null]);
      }
    }
  }
  else return null;
}
function isOverlayTraceKind(a){
  return a.$==4||a.$==5;
}
function resolvedSeriesPrepared(dataRef, prepared){
  const o=prepared.ResolvedSeries.TryFind(dataRef);
  return o==null?[]:o.$0;
}
function traceTimestampsPrepared(trace, prepared){
  if(isOverlayTraceKind(trace.Kind))return[];
  else {
    const m=trace.CandleDataRefs;
    const dataRef=m==null?trace.DataRef:m.$0.OpenRef;
    const resolved=resolvedSeriesPrepared(dataRef, prepared);
    const temporal=choose((point) => {
      const o=point.Temporal;
      return o==null?null:Some(presentationTimestamp(o.$0));
    }, resolved);
    if(length(temporal)>0)return temporal;
    else {
      const m_1=trace.Kind;
      switch(m_1.$==1?0:m_1.$==2?1:m_1.$==3?1:m_1.$==4?2:m_1.$==5?2:0){
        case 0:
          return map((a) => a.Timestamp, candleSeriesForTracePrepared(trace, prepared));
        case 1:
          return map((a) => a.Timestamp, lineSeriesFromResolved(resolved));
        case 2:
          return[];
      }
    }
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
          EventTimeUtc:null, 
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
function tryTemporalSeriesPoint(value){
  let _1;
  const o=tryObject(value);
  if(o==null)return null;
  else {
    const values=o.$0;
    const _2=nonNegativeInteger("position", values);
    const _3=values.TryFind("value");
    return _2!=null&&_2.$==1&&(_3!=null&&_3.$==1&&(_1=[_3.$0, _2.$0],true))?Some([_1[1], _1[0]]):null;
  }
}
function tryTemporalAxisRaw(value){
  let _1;
  const o=tryObject(value);
  if(o==null)return null;
  else {
    const fields=o.$0;
    if(!Equals(objectText("_type", fields), Some("temporal-axis.v1")))return null;
    else {
      const _2=requiredObjectText("axisRef", fields);
      const _3=nonNegativeInteger("revision", fields);
      const _4=fields.TryFind("points");
      return _2!=null&&_2.$==1&&(_3!=null&&_3.$==1&&(_4!=null&&_4.$==1&&(_4.$0.$==4&&(_1=[_2.$0, _4.$0.$0, _3.$0],true))))?Some([_1[0], _1[2], _1[1]]):null;
    }
  }
}
function tryTemporalAxisPoint(value){
  let _1;
  const o=tryObject(value);
  if(o==null)return null;
  else {
    const fields=o.$0;
    const _2=nonNegativeInteger("position", fields);
    const _3=requiredObjectText("sourceIntervalId", fields);
    const _4=requiredObjectText("scaleKey", fields);
    const _5=requiredObjectText("intervalStartUtc", fields);
    const _6=requiredObjectText("intervalEndUtc", fields);
    const _7=requiredObjectText("observedThroughUtc", fields);
    const _8=requiredObjectText("finality", fields);
    const _9=requiredObjectText("projection", fields);
    return _2!=null&&_2.$==1&&(_3!=null&&_3.$==1&&(_4!=null&&_4.$==1&&(_5!=null&&_5.$==1&&(_6!=null&&_6.$==1&&(_7!=null&&_7.$==1&&(_8!=null&&_8.$==1&&(_9!=null&&_9.$==1&&(_1=[_8.$0, _6.$0, _5.$0, _7.$0, _2.$0, _9.$0, _4.$0, _3.$0],true))))))))?Some([_1[4], {
      SourceIntervalId:_1[7], 
      ScaleKey:_1[6], 
      IntervalStartUtc:_1[2], 
      IntervalEndUtc:_1[1], 
      EventTimeUtc:requiredObjectText("eventTimeUtc", fields), 
      ObservedThroughUtc:_1[3], 
      AvailableAtUtc:requiredObjectText("availableAtUtc", fields), 
      Finality:_1[0], 
      Projection:_1[5], 
      Quality:requiredObjectText("quality", fields)
    }]):null;
  }
}
function presentationTimestamp(metadata){
  const o=metadata.EventTimeUtc;
  return o==null?metadata.IntervalStartUtc:o.$0;
}
function traceReferencePoints(trace, data){
  let _1;
  const m=trace.Kind;
  switch(m.$==1?0:m.$==2?1:m.$==3?1:m.$==4?2:m.$==5?2:0){
    case 0:
      _1=map((point) =>[point.Timestamp, point.Temporal], candleSeriesForTrace(trace, data));
      break;
    case 1:
      _1=map((point) =>[point.Timestamp, point.Temporal], lineSeries(trace.DataRef, data));
      break;
    case 2:
      _1=[];
      break;
  }
  return distinctBy((t) => t[0], _1);
}
function rowReferencePoints(row, data){
  const traces=filter_1((a) => a.Visible, effectiveTraces(row));
  const o=tryFind((trace) => trace.DataRef==row.DataRef, traces);
  const o_1=o==null?tryHead(traces):(o.$0,o);
  const o_2=o_1==null?null:Some(traceReferencePoints(o_1.$0, data));
  return o_2==null?[]:o_2.$0;
}
function objectNumber(name, value){
  const o=objectField(name, value);
  return o==null?null:tryNumber(o.$0);
}
function objectField(name, value){
  return value.TryFind(name);
}
function candlePointsByTimestamp(values){
  const lookup=new Dictionary("New_5");
  for(let i=0, _1=values.length-1;i<=_1;i++){
    const point=get(values, i);
    lookup.set_Item(point.Timestamp, point);
  }
  return lookup;
}
function candleSlotRange(referenceTimestamps, point){
  return matchingReferenceRange(referenceTimestamps, point.Timestamp, point.Temporal);
}
function projectedLinePoints(referenceTimestamps, points){
  let sourceIndex;
  const projected=create(length(referenceTimestamps), null);
  const nextUnassignedSlot=init(length(referenceTimestamps)+1, (x) => x);
  function findNextUnassigned(index){
    const parent=get(nextUnassignedSlot, index);
    if(parent===index)return index;
    else {
      const root=findNextUnassigned(parent);
      set(nextUnassignedSlot, index, root);
      return root;
    }
  }
  sourceIndex=length(points)-1;
  while(sourceIndex>=0)
    {
      let _1, targetIndex;
      const point=get(points, sourceIndex);
      const m=matchingReferenceRange(referenceTimestamps, point.Timestamp, point.Temporal);
      if(m==null)_1=void 0;
      else {
        const lastExclusive=m.$0[1];
        const first=m.$0[0];
        targetIndex=findNextUnassigned(first);
        while(targetIndex<lastExclusive)
          {
            set(projected, targetIndex, Some(point));
            set(nextUnassignedSlot, targetIndex, findNextUnassigned(targetIndex+1));
            targetIndex=get(nextUnassignedSlot, targetIndex);
          }
        _1=void 0;
      }
      sourceIndex=sourceIndex-1;
    }
  return choose((x) => x, mapi((_2, _3) => _3==null?null:Some([_2, _3.$0]), projected));
}
function paddedBoundsForCssPixels(fallbackLow, fallbackHigh, viewBoxHeight, plotHeight, chartPixelHeight, desiredCssPadding, bounds){
  if(bounds!=null&&bounds.$==1){
    if(bounds.$0[0]===bounds.$0[1])return[bounds.$0[0]-1, bounds.$0[1]+1];
    else {
      const high=bounds.$0[1];
      const low=bounds.$0[0];
      const a=1;
      const effectiveViewBoxHeight=Compare(a, viewBoxHeight)===1?a:viewBoxHeight;
      const a_1=1;
      const effectivePlotHeight=Compare(a_1, plotHeight)===1?a_1:plotHeight;
      const a_2=1;
      const effectiveChartPixelHeight=Compare(a_2, chartPixelHeight)===1?a_2:chartPixelHeight;
      const a_3=effectivePlotHeight*0.45;
      const a_4=0;
      let _1=Compare(a_4, desiredCssPadding)===1?a_4:desiredCssPadding;
      let _2=_1*effectiveViewBoxHeight;
      const b=_2/effectiveChartPixelHeight;
      const viewBoxPadding=Compare(a_3, b)===-1?a_3:b;
      const a_5=0.0001;
      const a_6=0.0001;
      const b_1=effectivePlotHeight-2*viewBoxPadding;
      let _3=Compare(a_6, b_1)===1?a_6:b_1;
      const b_2=(high-low)*viewBoxPadding/_3;
      const valuePadding=Compare(a_5, b_2)===1?a_5:b_2;
      return[low-valuePadding, high+valuePadding];
    }
  }
  else return[fallbackLow, fallbackHigh];
}
function projectedCandleCursorValues(isBaseRow, referenceTimestamps, values){
  return isBaseRow?projectRanges(length(referenceTimestamps), values, (value) => matchingReferenceRange(referenceTimestamps, value.Timestamp, value.Temporal)):map2((_1, _2) => _1==null?_2:(_1.$0,_1), projectRanges(length(referenceTimestamps), values, (value) => {
    let _1;
    const m=value.Temporal;
    return m!=null&&m.$==1&&(finalizedTemporal(m.$0)&&(_1=m.$0,true))?matchingReferenceRange(referenceTimestamps, value.Timestamp, value.Temporal):null;
  }), projectRanges(length(referenceTimestamps), values, (value) => {
    let _1;
    const m=value.Temporal;
    if(m!=null&&m.$==1&&(finalizedTemporal(m.$0)&&(_1=m.$0,true))){
      const o=_1.AvailableAtUtc;
      if(o==null)return null;
      else {
        const first=lowerTimestampBound(referenceTimestamps, o.$0);
        return first<length(referenceTimestamps)?Some([first, length(referenceTimestamps)]):null;
      }
    }
    else return null;
  }));
}
function candleCursorPointValue(label, kind, point){
  let _1;
  const baseValue=Equals(kind, {$:1})?fixedNumber(point.Volume):"O "+fixedNumber(point.Open)+" H "+fixedNumber(point.High)+" L "+fixedNumber(point.Low)+" C "+fixedNumber(point.Close);
  const m=point.Temporal;
  if(m==null)_1=baseValue;
  else {
    const metadata=m.$0;
    _1=baseValue+" | "+metadata.ScaleKey+" "+metadata.Finality+" | "+metadata.SourceIntervalId;
  }
  return{Label:label, Value:_1};
}
function lineCursorPointValue(label, point){
  let _1;
  const m=point.Temporal;
  if(m==null)_1=fixedNumber(point.Value);
  else {
    const metadata=m.$0;
    _1=fixedNumber(point.Value)+" | "+metadata.ScaleKey+" "+metadata.Finality+" | "+metadata.SourceIntervalId;
  }
  return{Label:label, Value:_1};
}
function markerPlacementsPreparedWithIndexes(trace, targetTraceId, targetByTimestamp, prepared, referenceSlots){
  let o;
  const o_1=prepared.RawData.TryFind(trace.DataRef);
  const o_2=o_1==null?null:tryTemporalSeries(o_1.$0);
  if(o_2==null)o=null;
  else {
    const _1=o_2.$0[0];
    const _2=o_2.$0[1];
    const _3=o_2.$0[2];
    const o_3=filter((axis_1) => axis_1.Revision===_2, prepared.ResolvedAxes.TryFind(_1));
    if(o_3==null)o=null;
    else {
      const axis=o_3.$0;
      let _4=collect((_5) => {
        let _6, _7;
        const position=_5[0];
        const _8=axis.Points.TryFind(position);
        const _9=decodeBucket_1(trace.DataRef, _5[1]);
        if(_8!=null&&_8.$==1&&(_9.$==0&&(_6=[_9.$0, _8.$0],true))){
          const timestamp=presentationTimestamp(_6[1]);
          const _10=tryFindReferenceSlot(timestamp, referenceSlots);
          const _11=tryFindCandlePoint(timestamp, targetByTimestamp);
          if(_10!=null&&_10.$==1&&(_11!=null&&_11.$==1&&(_7=[_10.$0, _11.$0],true))){
            const slotIndex=_7[0];
            const targetPoint=_7[1];
            return map((marker) =>({
              TraceId:trace.TraceId, 
              TargetTraceId:targetTraceId, 
              Position:position, 
              SlotIndex:slotIndex, 
              Lane:0, 
              Target:targetPoint, 
              Marker:marker
            }), _6[0]);
          }
          else return[];
        }
        else return[];
      }, _3);
      o=Some(_4);
    }
  }
  return o==null?[]:o.$0;
}
function assignAggregateMarkerLanes(placements){
  function assign(index, counts, assigned){
    while(true)
      {
        if(index>=length(placements))return ofList(rev(assigned));
        else {
          const placement=get(placements, index);
          const key_1=[placement.TargetTraceId, placement.Position, placement.Marker.Anchor];
          const x=counts.TryFind(key_1);
          const lane=(((v) =>(o) => o==null?v:o.$0)(0))(x);
          index=index+1;
          counts=counts.Add_1(key_1, lane+1);
          assigned=FSharpList.Cons({
            TraceId:placement.TraceId, 
            TargetTraceId:placement.TargetTraceId, 
            Position:placement.Position, 
            SlotIndex:placement.SlotIndex, 
            Lane:lane, 
            Target:placement.Target, 
            Marker:placement.Marker
          }, assigned);
        }
      }
  }
  return assign(0, new FSharpMap("New", []), FSharpList.Empty);
}
function projectedLastSourceTimestampWhere(includePoint, referenceTimestamps, dataRef, prepared){
  let _1;
  const projected=create(length(referenceTimestamps), null);
  const o=filter(includePoint, tryLast(resolvedSeriesPrepared(dataRef, prepared)));
  if(o==null)_1=null;
  else {
    const point=o.$0;
    const o_1=point.Temporal;
    const sourceTimestamp=o_1==null?null:Some(presentationTimestamp(o_1.$0));
    const range_1=sourceTimestamp==null?length(referenceTimestamps)>0?Some([length(referenceTimestamps)-1, length(referenceTimestamps)]):null:matchingReferenceRange(referenceTimestamps, sourceTimestamp.$0, point.Temporal);
    if(range_1==null)_1=null;
    else {
      const lastExclusive=range_1.$0[1];
      const first=range_1.$0[0];
      const timestamp=sourceTimestamp==null?get(referenceTimestamps, first):sourceTimestamp.$0;
      for(let index=first, _2=lastExclusive-1;index<=_2;index++)set(projected, index, Some(timestamp));
      _1=void 0;
    }
  }
  return projected;
}
function parseCandleResolved(temporal, payload){
  let _1, _2;
  const o=payload==null?null:tryObject(payload.$0);
  if(o==null)return null;
  else {
    const item=o.$0;
    const o_1=temporal==null?null:Some(presentationTimestamp(temporal.$0));
    const _3=o_1==null?objectText("t", item):(o_1.$0,o_1);
    const _4=objectNumber("o", item);
    const _5=objectNumber("h", item);
    const _6=objectNumber("l", item);
    const _7=objectNumber("c", item);
    const _8=objectNumber("v", item);
    if(_3!=null&&_3.$==1){
      if(_4!=null&&_4.$==1){
        if(_5!=null&&_5.$==1){
          if(_6!=null&&_6.$==1){
            if(_7!=null&&_7.$==1){
              if(_8!=null&&_8.$==1){
                const volume=_8.$0;
                _3.$0;
                const openValue=_4.$0;
                const low=_6.$0;
                const high=_5.$0;
                const close=_7.$0;
                let _9=finiteNumber(openValue)&&finiteNumber(high)&&finiteNumber(low)&&finiteNumber(close)&&finiteNumber(volume);
                _1=_9&&(_2=[_7.$0, _5.$0, _6.$0, _4.$0, _3.$0, _8.$0],true);
              }
              else _1=false;
            }
            else _1=false;
          }
          else _1=false;
        }
        else _1=false;
      }
      else _1=false;
    }
    else _1=false;
    return _1?Some({
      Timestamp:_2[4], 
      Open:_2[3], 
      High:_2[1], 
      Low:_2[2], 
      Close:_2[0], 
      Volume:_2[5], 
      Temporal:temporal
    }):null;
  }
}
function parseLineResolved(temporal, payload){
  let _1, _2;
  if(payload!=null&&payload.$==1&&(payload.$0.$==2&&(temporal!=null&&temporal.$==1&&(finiteNumber(payload.$0.$0)&&(_1=[payload.$0.$0, temporal.$0],true)))))return Some({
    Timestamp:presentationTimestamp(_1[1]), 
    Value:_1[0], 
    Temporal:temporal
  });
  else {
    const o=payload==null?null:tryObject(payload.$0);
    if(o==null)return null;
    else {
      const item=o.$0;
      const o_1=temporal==null?null:Some(presentationTimestamp(temporal.$0));
      const _3=o_1==null?objectText("t", item):(o_1.$0,o_1);
      const _4=objectNumber("v", item);
      return _3!=null&&_3.$==1&&(_4!=null&&_4.$==1&&(_3.$0,finiteNumber(_4.$0)&&(_2=[_4.$0, _3.$0],true)))?Some({
        Timestamp:_2[1], 
        Value:_2[0], 
        Temporal:temporal
      }):null;
    }
  }
}
function cursorIndexFromClientX(visibleCount, left, width, clientX){
  return width<=0?null:cursorIndexFromRatio(visibleCount, (clientX-left)/width);
}
function cursorEventItemsWith(formatEventTime, slotIndex, traces, markers, stripes){
  const traceOrder=OfArray(mapi((_1, _2) =>[_2.TraceId, _1], traces));
  const traceCategory=OfArray(map((trace) =>[trace.TraceId, IsNullOrWhiteSpace(trace.Label)?trace.TraceId:Trim(trace.Label)], traces));
  const category=(traceId) => {
    const o=traceCategory.TryFind(traceId);
    return o==null?traceId:o.$0;
  };
  const markerItems=map((placement) => {
    const categoryValue=category(placement.TraceId);
    const o=placement.Marker.Label;
    let _1=o==null?null:Some(Trim(o.$0));
    const o_1=filter((x) =>!IsNullOrWhiteSpace(x), _1);
    let _2=o_1==null?categoryValue:o_1.$0;
    let _3={
      MarkerId:placement.Marker.MarkerId, 
      EventTimeUtc:placement.Marker.EventTimeUtc, 
      Category:categoryValue, 
      SourceKind:"marker", 
      Label:_2, 
      Color:placement.Marker.Color, 
      Tooltip:markerTooltipTextWith(formatEventTime, placement)
    };
    return[placement.TraceId, [placement.Lane, placement.Marker.MarkerId], _3];
  }, filter_1((placement) => placement.SlotIndex===slotIndex, markers));
  const markerEventIds=new FSharpSet("New_2", OfSeq(map((_1) => _1[2].MarkerId, markerItems)));
  const byTrace=map((_1) =>[_1[0], sortBy((_2) =>[_2[1], _2[2].MarkerId], _1[1])], sortBy((x) => {
    const o=traceOrder.TryFind(x[0]);
    return o==null?2147483647:o.$0;
  }, groupBy((_1) => _1[0], markerItems.concat(map((placement) => {
    const categoryValue=category(placement.TraceId);
    const o=placement.Stripe.Label;
    let _1=o==null?null:Some(Trim(o.$0));
    const o_1=filter((x) =>!IsNullOrWhiteSpace(x), _1);
    let _2=o_1==null?categoryValue:o_1.$0;
    let _3={
      MarkerId:placement.Stripe.StripeId, 
      EventTimeUtc:placement.Stripe.EventTimeUtc, 
      Category:categoryValue, 
      SourceKind:"overview-stripe", 
      Label:_2, 
      Color:placement.Stripe.Color, 
      Tooltip:overviewStripeTooltipTextWith(formatEventTime, categoryValue, placement.Stripe)
    };
    return[placement.TraceId, [placement.LayerOrder, placement.Stripe.StripeId], _3];
  }, filter_1((placement) => placement.SlotIndex===slotIndex&&!markerEventIds.Contains(placement.Stripe.StripeId), stripes))))));
  const maximumItemsPerTrace=max([0].concat(map((x) => x[1].length, byTrace)));
  return ofSeq(delay(() => collect_1((itemIndex) => collect_1((m) => {
    const m_1=tryItem(itemIndex, m[1]);
    return m_1==null?[]:[m_1.$0[2]];
  }, byTrace), range(0, maximumItemsPerTrace-1))));
}
function markerPresentation(placements){
  const groups=groupBy((placement) =>[placement.TargetTraceId, placement.Position, placement.Marker.Anchor], placements);
  return[sortBy((placement) =>[placement.SlotIndex, placement.Lane, placement.TraceId, placement.Marker.MarkerId], collect((_1) => sortBy((a) => a.Lane, _1[1]).slice(0, 4), groups)), sortBy((cluster) =>[cluster.SlotIndex, cluster.Lane, cluster.TargetTraceId], choose((_1) => {
    const a=_1[0];
    const targetTraceId=a[0];
    const position=a[1];
    const anchor=a[2];
    const ordered=sortBy((a_1) => a_1.Lane, _1[1]);
    return length(ordered)<=4?null:Some({
      ClusterId:targetTraceId+":"+String(position)+":"+anchorText(anchor), 
      TargetTraceId:targetTraceId, 
      Position:position, 
      SlotIndex:get(ordered, 0).SlotIndex, 
      Anchor:anchor, 
      Lane:4, 
      Markers:skip(4, ordered)
    });
  }, groups))];
}
function markerTooltipTextWith(formatEventTime, placement){
  return concat_1("\n", ofSeq(delay(() => {
    let _1;
    const m=placement.Marker.Label;
    let _2=m!=null&&m.$==1&&(!IsNullOrWhiteSpace(m.$0)&&(_1=m.$0,true))?[_1]:[];
    return append_2(_2, delay(() => append_2(["Event time: "+formatEventTime(placement.Marker.EventTimeUtc)], delay(() => map_2((field_1) => field_1.Label+": "+field_1.Value, placement.Marker.Tooltip)))));
  })));
}
function markerTrianglePoints(shape, x, y, half){
  return shape.$==0?Some([[x-half, y+half], [x+half, y+half], [x, y-half]]):shape.$==1?Some([[x-half, y-half], [x+half, y-half], [x, y+half]]):null;
}
function compactProjectedLinePoints(maximumVisualPoints, referenceLength, values){
  if(referenceLength<=maximumVisualPoints||length(values)<=maximumVisualPoints)return values;
  else {
    const a=1;
    const b=maximumVisualPoints/2>>0;
    const bucketCount=Compare(a, b)===1?a:b;
    const occupied=create(bucketCount, false);
    const minimumSlots=create(bucketCount, 0);
    const minimumPoints=create(bucketCount, null);
    const maximumSlots=create(bucketCount, 0);
    const maximumPoints=create(bucketCount, null);
    for(let i=0, _1=values.length-1;i<=_1;i++){
      const f=get(values, i);
      const slotIndex=f[0];
      const point=f[1];
      const a_1=bucketCount-1;
      const b_1=slotIndex*bucketCount/referenceLength>>0;
      const bucketIndex=Compare(a_1, b_1)===-1?a_1:b_1;
      if(!get(occupied, bucketIndex)){
        set(occupied, bucketIndex, true);
        set(minimumSlots, bucketIndex, slotIndex);
        set(minimumPoints, bucketIndex, point);
        set(maximumSlots, bucketIndex, slotIndex);
        set(maximumPoints, bucketIndex, point);
      }
      else {
        point.Value<get(minimumPoints, bucketIndex).Value?(set(minimumSlots, bucketIndex, slotIndex),set(minimumPoints, bucketIndex, point)):void 0;
        point.Value>get(maximumPoints, bucketIndex).Value?(set(maximumSlots, bucketIndex, slotIndex),set(maximumPoints, bucketIndex, point)):void 0;
      }
    }
    const compacted=MarkResizable([]);
    for(let bucketIndex_1=0, _2=bucketCount-1;bucketIndex_1<=_2;bucketIndex_1++)if(get(occupied, bucketIndex_1)){
      const minimum=[get(minimumSlots, bucketIndex_1), get(minimumPoints, bucketIndex_1)];
      const maximum=[get(maximumSlots, bucketIndex_1), get(maximumPoints, bucketIndex_1)];
      if(minimum[0]===maximum[0])compacted.push(minimum);
      else if(minimum[0]<maximum[0]){
        compacted.push(minimum);
        compacted.push(maximum);
      }
      else {
        compacted.push(maximum);
        compacted.push(minimum);
      }
    }
    return compacted.slice();
  }
}
function referenceSlotsByTimestamp(timestamps){
  const lookup=new Dictionary("New_5");
  for(let index=0, _1=length(timestamps)-1;index<=_1;index++)if(!lookup.ContainsKey(get(timestamps, index)))lookup.set_Item(get(timestamps, index), index);
  return lookup;
}
function latestTemporalMetadataPrepared(trace, prepared){
  return tryLast(choose((a) => a.Temporal, resolvedSeriesPrepared(trace.DataRef, prepared)));
}
function upperTimestampBound(referenceTimestamps, value){
  let low, high;
  low=0;
  high=length(referenceTimestamps);
  while(low<high)
    {
      const middle=low+((high-low)/2>>0);
      if(Compare(get(referenceTimestamps, middle), value)<=0)low=middle+1;
      else high=middle;
    }
  return low;
}
function lineSeriesFromResolved(resolved){
  return choose((point) => parseLineResolved(point.Temporal, point.Payload), resolved);
}
function candleSeriesFromResolved(resolved){
  return choose((point) => parseCandleResolved(point.Temporal, point.Payload), resolved);
}
function sharedPrefixLength(left, right){
  let index;
  const a=length(left);
  const b=length(right);
  const limit=Compare(a, b)===-1?a:b;
  index=0;
  while(index<limit&&get(left, index)===get(right, index))
    index=index+1;
  return index;
}
function requiredObjectText(name, value){
  return filter((x) =>!IsNullOrWhiteSpace(x), objectText(name, value));
}
function nonNegativeInteger(name, fields){
  return filter((value) => value>=0&&value===(value<0?Math.ceil(value):Math.floor(value)), objectNumber(name, fields));
}
function tryFindTemporalPointIndex(position, rawPoints){
  function search(low, high){
    while(true)
      {
        if(low>high)return null;
        else {
          const middle=low+((high-low)/2>>0);
          const m=tryTemporalSeriesPoint(get(rawPoints, middle));
          if(m==null)return null;
          else if(m.$0[0]===position){
            m.$0;
            return Some(middle);
          }
          else if(m.$0[0]<position){
            m.$0[0];
            low=middle+1;
          }
          else high=middle-1;
        }
      }
  }
  return search(0, length(rawPoints)-1);
}
function candleSeriesForTrace(trace, data){
  return candleSeriesForTracePrepared(trace, prepareData(data));
}
function lineSeries(dataRef, data){
  return lineSeriesFromResolved(resolvedSeries(dataRef, data));
}
function matchingReferenceRange(referenceTimestamps, pointTimestamp, temporal){
  let _1, p, _2;
  if(temporal!=null&&temporal.$==1){
    const metadata=temporal.$0;
    _2=metadata.Projection=="repeat-across-base-buckets"||metadata.Projection=="candle-span"?(_1=temporal.$0,0):temporal.$0.Projection=="step-after-close"?(_1=temporal.$0,1):2;
  }
  else _2=2;
  switch(_2){
    case 0:
      p=[lowerTimestampBound(referenceTimestamps, _1.IntervalStartUtc), lowerTimestampBound(referenceTimestamps, _1.IntervalEndUtc)];
      break;
    case 1:
      const m=_1.AvailableAtUtc;
      p=m==null?[0, 0]:[lowerTimestampBound(referenceTimestamps, m.$0), length(referenceTimestamps)];
      break;
    case 2:
      p=[lowerTimestampBound(referenceTimestamps, pointTimestamp), upperTimestampBound(referenceTimestamps, pointTimestamp)];
      break;
  }
  const lastExclusive=p[1];
  const first=p[0];
  return first>=lastExclusive?null:Some([first, lastExclusive]);
}
function projectRanges(referenceCount, values, rangeForValue){
  let sourceIndex;
  const projected=create(referenceCount, null);
  const nextUnassignedSlot=init(referenceCount+1, (x) => x);
  function findNextUnassigned(index){
    const parent=get(nextUnassignedSlot, index);
    if(parent===index)return index;
    else {
      const root=findNextUnassigned(parent);
      set(nextUnassignedSlot, index, root);
      return root;
    }
  }
  sourceIndex=length(values)-1;
  while(sourceIndex>=0)
    {
      let targetIndex, _1;
      const value=get(values, sourceIndex);
      const m=rangeForValue(value);
      if(m!=null&&m.$==1){
        if(m.$0[0]<m.$0[1]){
          const first=m.$0[0];
          const lastExclusive=m.$0[1];
          targetIndex=findNextUnassigned(first);
          while(targetIndex<lastExclusive)
            {
              set(projected, targetIndex, Some(value));
              set(nextUnassignedSlot, targetIndex, findNextUnassigned(targetIndex+1));
              targetIndex=get(nextUnassignedSlot, targetIndex);
            }
          _1=void 0;
        }
        else _1=void 0;
      }
      else _1=void 0;
      sourceIndex=sourceIndex-1;
    }
  return projected;
}
function finalizedTemporal(metadata){
  return Trim(metadata.Finality).toLowerCase()=="final";
}
function lowerTimestampBound(referenceTimestamps, value){
  let low, high;
  low=0;
  high=length(referenceTimestamps);
  while(low<high)
    {
      const middle=low+((high-low)/2>>0);
      if(Compare(get(referenceTimestamps, middle), value)<0)low=middle+1;
      else high=middle;
    }
  return low;
}
function tryFindReferenceSlot(key_1, lookup){
  let o;
  const m=(o=0,[lookup.TryGetValue(key_1, {get:() => o, set:(v) => {
    o=v;
  }}), o]);
  return m[0]?Some(m[1]):null;
}
function tryFindCandlePoint(key_1, lookup){
  let o;
  const m=(o=null,[lookup.TryGetValue(key_1, {get:() => o, set:(v) => {
    o=v;
  }}), o]);
  return m[0]?Some(m[1]):null;
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
function overviewStripeTooltipTextWith(formatEventTime, category, stripe){
  return concat_1("\n", ofSeq(delay(() => {
    let _1;
    const m=stripe.Label;
    let _2=m!=null&&m.$==1&&(!IsNullOrWhiteSpace(m.$0)&&(_1=m.$0,true))?[_1]:[category];
    return append_2(_2, delay(() => append_2(["Event time: "+formatEventTime(stripe.EventTimeUtc)], delay(() => map_2((field_1) => field_1.Label+": "+field_1.Value, stripe.Tooltip)))));
  })));
}
function prepareData(data){
  const axes=OfArray(choose((_1) => prepareAxis(_1[1]), ofSeq(ToSeq(data))));
  return{
    RawData:data, 
    ResolvedAxes:axes, 
    ResolvedSeries:OfArray(choose((_1) => {
      const dataRef=_1[0];
      const value=_1[1];
      let _2;
      if(value.$==4)return Some([dataRef, map((item) => {
        const p=pointPayload_1(item);
        return{Payload:p[1], Temporal:p[0]};
      }, value.$0)]);
      else {
        const m=tryTemporalSeries(value);
        if(m==null)return null;
        else {
          const points=m.$0[2];
          const axisRevision=m.$0[1];
          const m_1=axes.TryFind(m.$0[0]);
          return m_1!=null&&m_1.$==1&&(m_1.$0.Revision===axisRevision&&(_2=m_1.$0,true))?Some([dataRef, choose((_3) => {
            const o=_2.Points.TryFind(_3[0]);
            return o==null?null:Some({Payload:Some(_3[1]), Temporal:Some(o.$0)});
          }, points)]):Some([dataRef, []]);
        }
      }
    }, ofSeq(ToSeq(data))))
  };
}
function resolvedSeries(dataRef, data){
  let _1;
  const m=data.TryFind(dataRef);
  if(m==null)return[];
  else if(m.$0.$==4)return map((value) => {
    const p=pointPayload_1(value);
    return{Payload:p[1], Temporal:p[0]};
  }, m.$0.$0);
  else {
    const m_1=tryTemporalSeries(m.$0);
    if(m_1==null)return[];
    else {
      const points=m_1.$0[2];
      const axisRevision=m_1.$0[1];
      const axisRef=m_1.$0[0];
      const o=data.TryFind(axisRef);
      const m_2=o==null?null:tryTemporalAxis(o.$0);
      if(m_2!=null&&m_2.$==1&&((m_2.$0,m_2.$0[0]==axisRef&&m_2.$0[1]===axisRevision)&&(_1=[m_2.$0[2], m_2.$0[0], m_2.$0[1]],true))){
        const axis=_1[0];
        return choose((_2) => {
          const o_1=axis.TryFind(_2[0]);
          return o_1==null?null:Some({Payload:Some(_2[1]), Temporal:Some(o_1.$0)});
        }, points);
      }
      else return[];
    }
  }
}
function tryTemporalAxis(value){
  const o=tryTemporalAxisRaw(value);
  if(o==null)return null;
  else {
    const _1=o.$0[0];
    const _2=o.$0[1];
    const _3=o.$0[2];
    const decoded=choose(tryTemporalAxisPoint, _3);
    return length(decoded)!==length(_3)?null:Some([_1, _2, OfArray(decoded)]);
  }
}
function initial_1(){
  return _c_14.initial;
}
function beginCandidate(state, gate){
  const generation=gate.Generation+1;
  return[{
    Generation:generation, 
    Pending:Some({Generation:generation, State:state}), 
    LastCommitted:gate.LastCommitted
  }, generation];
}
function tryCommit(currentRuntimeState, generation, candidateState, gate){
  let _1, _2;
  const m=gate.Pending;
  if(m!=null&&m.$==1){
    const candidate=m.$0;
    _1=generation===candidate.Generation&&generation===gate.Generation&&Equals(key(candidate.State), key(candidateState))&&Equals(key(currentRuntimeState), key(candidateState))&&(_2=m.$0,true);
  }
  else _1=false;
  if(_1){
    const candidateKey=key(candidateState);
    return[{
      Generation:gate.Generation, 
      Pending:null, 
      LastCommitted:Some(candidateKey)
    }, Equals(gate.LastCommitted, Some(candidateKey))?null:Some(candidateState)];
  }
  else return[gate, null];
}
function pendingGenerationFor(state, gate){
  let _1;
  const m=gate.Pending;
  return m!=null&&m.$==1&&(Equals(key(m.$0.State), key(state))&&(_1=m.$0,true))?Some(_1.Generation):null;
}
function key(state){
  return{
    Identity:state.Identity, 
    DocumentRevision:state.DocumentRevision, 
    DataRevision:state.DataRevision, 
    LastTransportSequence:state.LastTransportSequence
  };
}
function fullOrOriginal(zone, canonicalUtc){
  const o=tryFormat(zone, canonicalUtc);
  const o_1=o==null?null:Some(o.$0.FullText);
  return o_1==null?canonicalUtc:o_1.$0;
}
function compactOrOriginal(zone, canonicalUtc){
  const o=tryFormat(zone, canonicalUtc);
  const o_1=o==null?null:Some(o.$0.CompactText);
  return o_1==null?canonicalUtc:o_1.$0;
}
function dateAndClockOrUnavailable(zone, canonicalUtc){
  let o;
  const o_1=tryFormat(zone, canonicalUtc);
  if(o_1==null)o=null;
  else {
    const value=o_1.$0;
    o=Some([value.DateText, value.ClockText]);
  }
  return o==null?["Unavailable", "Unavailable"]:o.$0;
}
function tryFormat(zone, canonicalUtc){
  const o=tryParseCanonicalUtc(canonicalUtc);
  if(o==null)return null;
  else {
    const utc=o.$0;
    const p=offsetAndAbbreviation(zone, utc);
    const abbreviation=p[1];
    const local=applyOffset(p[0], utc);
    const dateText=pad4(local.Year)+"-"+pad2(local.Month)+"-"+pad2(local.Day);
    const clockText=pad2(local.Hour)+":"+pad2(local.Minute)+":"+pad2(local.Second);
    let _1={
      CanonicalUtc:canonicalUtc, 
      Zone:zone, 
      ZoneId:id(zone), 
      ZoneAbbreviation:abbreviation, 
      FullText:dateText+" "+clockText+" "+abbreviation, 
      CompactText:pad2(local.Month)+"-"+pad2(local.Day)+" "+pad2(local.Hour)+":"+pad2(local.Minute)+" "+abbreviation, 
      DateText:dateText, 
      ClockText:clockText+" "+abbreviation
    };
    return Some(_1);
  }
}
function tryParseCanonicalUtc(value){
  let _1, _2;
  const candidate=value==null?"":Trim(value);
  const suffixStart=EndsWith(candidate, "Z")||EndsWith(candidate, "z")?candidate.length-1:EndsWith(candidate, "+00:00")?candidate.length-6:-1;
  const _3=suffixStart>=19&&candidate.length>=20&&candidate[4]==="-"&&candidate[7]==="-"&&(candidate[10]==="T"||candidate[10]==="t"||candidate[10]===" ")&&candidate[13]===":"&&candidate[16]===":";
  const _4=suffixStart===19||suffixStart>20&&candidate[19]==="."&&forall_2((character) => character>="0"&&character<="9", Substring(candidate, 20, suffixStart-20));
  const _5=number(candidate, 0, 4);
  const _6=number(candidate, 5, 2);
  const _7=number(candidate, 8, 2);
  const _8=number(candidate, 11, 2);
  const _9=number(candidate, 14, 2);
  const _10=number(candidate, 17, 2);
  if(_3){
    if(_4){
      if(_5!=null&&_5.$==1){
        if(_6!=null&&_6.$==1){
          if(_7!=null&&_7.$==1){
            if(_8!=null&&_8.$==1){
              if(_9!=null&&_9.$==1){
                if(_10!=null&&_10.$==1){
                  const year=_5.$0;
                  const second=_10.$0;
                  const month=_6.$0;
                  const minute=_9.$0;
                  const hour=_8.$0;
                  const day=_7.$0;
                  _1=year>=1&&day>=1&&day<=daysInMonth(year, month)&&hour<=23&&minute<=59&&second<=59&&(_2=[_7.$0, _8.$0, _9.$0, _6.$0, _10.$0, _5.$0],true);
                }
                else _1=false;
              }
              else _1=false;
            }
            else _1=false;
          }
          else _1=false;
        }
        else _1=false;
      }
      else _1=false;
    }
    else _1=false;
  }
  else _1=false;
  return _1?Some(New_77(_2[5], _2[3], _2[0], _2[1], _2[2], _2[4])):null;
}
function offsetAndAbbreviation(zone, parts){
  return zone.$==3?[8*60, "UTC+8"]:zone.$==1?isModernUsDaylightTime(zone, parts)?[-5*60, "CDT"]:[-6*60, "CST"]:zone.$==2?isModernUsDaylightTime(zone, parts)?[-4*60, "EDT"]:[-5*60, "EST"]:[0, "UTC"];
}
function applyOffset(offsetMinutes, parts){
  let p;
  const totalMinutes=parts.Hour*60+parts.Minute+offsetMinutes;
  if(totalMinutes<0){
    const p_1=previousDay(parts.Year, parts.Month, parts.Day);
    p=[p_1[0], p_1[1], p_1[2], totalMinutes+24*60];
  }
  else if(totalMinutes>=24*60){
    const p_2=nextDay(parts.Year, parts.Month, parts.Day);
    p=[p_2[0], p_2[1], p_2[2], totalMinutes-24*60];
  }
  else p=[parts.Year, parts.Month, parts.Day, totalMinutes];
  const normalizedMinutes=p[3];
  return New_77(p[0], p[1], p[2], normalizedMinutes/60>>0, normalizedMinutes%60, parts.Second);
}
function pad4(value){
  const text_1=String(value);
  return text_1.length>=4?text_1:replicate(4-text_1.length, "0")+text_1;
}
function pad2(value){
  return value<10?"0"+String(value):String(value);
}
function number(value, start, count){
  let result, valid, index;
  if(start<0||count<0||start+count>value.length)return null;
  else {
    result=0;
    valid=true;
    index=start;
    while(valid&&index<start+count)
      {
        let _1;
        const m=digit(value, index);
        if(m==null)_1=valid=false;
        else {
          const valueDigit=m.$0;
          _1=result=result*10+valueDigit;
        }
        index=index+1;
      }
    return valid?Some(result):null;
  }
}
function daysInMonth(year, month){
  return month===1?31:month===2?isLeapYear(year)?29:28:month===3?31:month===4?30:month===5?31:month===6?30:month===7?31:month===8?31:month===9?30:month===10?31:month===11?30:month===12?31:0;
}
function isModernUsDaylightTime(zone, parts){
  const p=zone.$==1?[8, 7]:zone.$==2?[7, 6]:[0, 0];
  const startKey=instantKey(3, nthSunday(parts.Year, 3, 2), p[0], 0, 0);
  const endKey=instantKey(11, nthSunday(parts.Year, 11, 1), p[1], 0, 0);
  const valueKey=instantKey(parts.Month, parts.Day, parts.Hour, parts.Minute, parts.Second);
  return valueKey>=startKey&&valueKey<endKey;
}
function previousDay(year, month, day){
  if(day>1)return[year, month, day-1];
  else if(month>1){
    const previousMonth=month-1;
    return[year, previousMonth, daysInMonth(year, previousMonth)];
  }
  else return[year-1, 12, 31];
}
function nextDay(year, month, day){
  return day<daysInMonth(year, month)?[year, month, day+1]:month<12?[year, month+1, 1]:[year+1, 1, 1];
}
function digit(value, index){
  const character=value[index];
  return character>="0"&&character<="9"?Some(character.charCodeAt()-"0".charCodeAt()):null;
}
function isLeapYear(year){
  return year%400===0||year%4===0&&year%100!==0;
}
function instantKey(month, day, hour, minute, second){
  return(((month*32+day)*24+hour)*60+minute)*60+second;
}
function nthSunday(year, month, occurrence){
  return 1+(7-dayOfWeek(year, month, 1))%7+7*(occurrence-1);
}
function dayOfWeek(year, month, day){
  const adjustedYear=month<3?year-1:year;
  return(adjustedYear+(adjustedYear/4>>0)-(adjustedYear/100>>0)+(adjustedYear/400>>0)+get([0, 3, 2, 5, 0, 3, 5, 1, 4, 6, 2, 4], month-1)+day)%7;
}
function id(a){
  return a.$==1?"America/Chicago":a.$==2?"America/New_York":a.$==3?"UTC+08:00":"UTC";
}
function tryResolve(schemas, row){
  return Bind_2((a) => {
    if(a!=null&&a.$==1){
      const binding=a.$0;
      const m=tryFind((schema_1) => schema_1.TemplateKey==binding.TemplateKey, nonNull(schemas));
      if(m!=null&&m.$==1){
        const schema=m.$0;
        return Map_2((values) => Some([schema, values]), validateForSchema(schema, binding));
      }
      else return Error_1(ofArray([error_2("editor-binding-schema-unavailable", "row.editorBinding.templateKey", "Editor binding schema is unavailable.")]));
    }
    else return Ok(null);
  }, tryFind_1(row));
}
function tryFind_1(row){
  const m=row.Options.TryFind("ptcs.dynamic.editor.binding.v1");
  return m!=null&&m.$==1?Map_2((V) => Some(V), tryDecodeValue(m.$0)):Ok(null);
}
function validateForSchema(schema, binding){
  return schema.TemplateKey!=binding.TemplateKey?Error_1(ofArray([error_2("editor-binding-template-mismatch", "row.editorBinding.templateKey", "Editor binding template does not match the selected schema.")])):validateInputs(limits(), schema, binding.Values);
}
function tryDecodeValue(value){
  let _1;
  if(value.$==5){
    const fields=value.$0;
    const _2=fields.TryFind("templateKey");
    const _3=fields.TryFind("values");
    if(_2!=null&&_2.$==1&&(_2.$0.$==3&&(_3!=null&&_3.$==1&&(_3.$0.$==4&&(_1=[_2.$0.$0, _3.$0.$0],true))))){
      const decoded=mapi((_4, _5) => {
        let _6;
        if(_5.$==5){
          const input_1=_5.$0;
          const _7=input_1.TryFind("path");
          const _8=input_1.TryFind("value");
          if(_7!=null&&_7.$==1&&(_7.$0.$==3&&(_8!=null&&_8.$==1&&(_6=[_7.$0.$0, _8.$0],true)))){
            const m=valueScalar(_6[1]);
            return m==null?Error_1(error_2("editor-binding-value-kind", "row.editorBinding.values["+String(_4)+"].value", "Editor binding values must be text, number or boolean.")):Ok({Path:_6[0], Value:m.$0});
          }
          else return Error_1(error_2("editor-binding-input-shape", "row.editorBinding.values["+String(_4)+"]", "Editor binding input requires path and value."));
        }
        else return Error_1(error_2("editor-binding-input-shape", "row.editorBinding.values["+String(_4)+"]", "Editor binding inputs must be objects."));
      }, nonNull(_1[1]));
      const errors_1=choose((a) => a.$==0?null:Some(a.$0), decoded);
      return length(errors_1)>0?Error_1(ofArray(errors_1)):validate({TemplateKey:_1[0], Values:choose((a) => a.$==1?null:Some(a.$0), decoded)});
    }
    else return Error_1(ofArray([error_2("editor-binding-shape", "row.editorBinding", "Editor binding requires templateKey and values.")]));
  }
  else return Error_1(ofArray([error_2("editor-binding-shape", "row.editorBinding", "Editor binding must be an object.")]));
}
function valueScalar(a){
  return a.$==3?Some({$:0, $0:a.$0}):a.$==2?Some({$:1, $0:a.$0}):a.$==1?Some({$:2, $0:a.$0}):null;
}
function validate(binding){
  const m=bindingErrors(binding);
  return m.$==0?Ok(binding):Error_1(m);
}
function bindingErrors(binding){
  const values=nonNull(binding.Values);
  return ofSeq_1(delay(() => append_2(identifier("row.editorBinding.templateKey", binding.TemplateKey), delay(() => append_2(length(values)>limits().MaxFields?[error_2("limit-editor-inputs", "row.editorBinding.values", "Editor binding inputs exceed hard limit "+String(limits().MaxFields)+".")]:[], delay(() => append_2(exists((_1) => _1[1]>1, countBy((a) => a.Path, values))?[error_2("duplicate-editor-input", "row.editorBinding.values", "Editor binding paths must be unique.")]:[], delay(() => collect_1((m) => {
    const input_1=m[1];
    const index=m[0];
    return append_2(identifier("row.editorBinding.values["+String(index)+"].path", input_1.Path), delay(() => {
      let _1, _2;
      const m_1=input_1.Value;
      if(m_1.$==0)_2=(_1=m_1.$0,0);
      else if(m_1.$==1){
        const value=m_1.$0;
        _2=isNaN(value)||Math.abs(value)===Infinity?(_1=m_1.$0,1):2;
      }
      else _2=2;
      switch(_2){
        case 0:
          return unsafeValue("row.editorBinding.values["+String(index)+"].value", {$:3, $0:_1});
        case 1:
          return[error_2("invalid-editor-number", "row.editorBinding.values["+String(index)+"].value", "Editor binding number must be finite.")];
        case 2:
          return[];
      }
    }));
  }, indexed(values))))))))));
}
function observationDomainCount(projection){
  const m=projection.TotalObservationCount;
  if(m==null){
    const a=fold((_1, _2) => {
      const b_1=_2.StartObservationOrdinal+_2.ObservationCount;
      return Compare(_1, b_1)===1?_1:b_1;
    }, 0n, projection.Segments);
    const b=projection.ActiveDetail.StartObservationOrdinal+BigInt(projection.ActiveDetail.ObservationCount);
    return Compare(a, b)===1?a:b;
  }
  else return m.$0;
}
function tryProviderOpenEarlierWindowIntent(expectedRevision, queryGeneration, observationCount){
  const o=tryWindowIntent(Some({$:0}), expectedRevision, queryGeneration, null, observationCount);
  if(o==null)return null;
  else {
    const intent=o.$0;
    let _1={
      ExpectedCoverageRevision:intent.ExpectedCoverageRevision, 
      QueryGeneration:intent.QueryGeneration, 
      StartObservationOrdinal:intent.StartObservationOrdinal, 
      ObservationCount:intent.ObservationCount, 
      Direction:intent.Direction, 
      RangeAuthority:{$:1}
    };
    return Some(_1);
  }
}
function tryMaximumVisibleBars(values){
  let _1, _2;
  const m=values.TryFind("viewport.maximumVisibleBars");
  if(m!=null&&m.$==1){
    if(m.$0.$==2){
      const value=m.$0.$0;
      _1=!isNaN(value)&&!(Math.abs(value)===Infinity)&&value===Math.floor(value)&&value>=1&&value<=4000&&(_2=m.$0.$0,true);
    }
    else _1=false;
  }
  else _1=false;
  return _1?Some(toInt(_2)):null;
}
function tryDecodeResolved(defaultView, data){
  const m=tryDataRef(defaultView);
  if(m.$==0){
    if(m.$0==null)return tryDecode(defaultView);
    else {
      const dataRef=m.$0.$0;
      const m_1=data.TryFind(dataRef);
      return m_1==null?Error_1(ofArray([error_1("missing-loaded-coverage-data-ref", dataRef, "Loaded coverage dataRef `"+dataRef+"` is missing.")])):tryDecode((new FSharpMap("New", [])).Add_1("viewport.loadedCoverage", m_1.$0));
    }
  }
  else return Error_1(m.$0);
}
function tryWindowIntent(direction, expectedRevision, queryGeneration, startOrdinal, observationCount){
  return queryGeneration<0n||(expectedRevision==null?false:expectedRevision.$0<0n)||(startOrdinal==null?false:startOrdinal.$0<0n)||observationCount<=0||observationCount>4000?null:Some({
    ExpectedCoverageRevision:expectedRevision, 
    QueryGeneration:queryGeneration, 
    StartObservationOrdinal:startOrdinal, 
    ObservationCount:observationCount, 
    Direction:direction, 
    RangeAuthority:{$:0}
  });
}
function tryDecode(values){
  let _1;
  const m=values.TryFind("viewport.loadedCoverage");
  if(m!=null&&m.$==1){
    if(m.$0.$==5){
      const fields=m.$0.$0;
      const _2=requiredText("schema", fields);
      const _3=requiredText("coverageIdentity", fields);
      const _4=requiredInt64("coverageRevision", fields);
      const _5=requiredInt64("queryGeneration", fields);
      const _6=requiredText("completeness", fields);
      const _7=decodeArray("segments", decodeSegment, fields);
      const _8=decodeArray("overviewAnchors", decodeAnchor, fields);
      const _9=fields.TryFind("activeDetail");
      switch(_2.$==0?_3.$==0?_4.$==0?_5.$==0?_6.$==0?_7.$==0?_8.$==0?_9!=null&&_9.$==1?_9.$0.$==5?(_7.$0,_4.$0,_3.$0,_5.$0,_6.$0,_8.$0,_9.$0.$0,_2.$0=="ta-loaded-coverage.v1"?(_1=[_9.$0.$0, _8.$0, _6.$0, _5.$0, _3.$0, _4.$0, _2.$0, _7.$0],0):_2.$0!="ta-loaded-coverage.v1"?(_1=_2.$0,1):2):_2.$0!="ta-loaded-coverage.v1"?(_1=_2.$0,1):2:_2.$0!="ta-loaded-coverage.v1"?(_1=_2.$0,1):2:_2.$0!="ta-loaded-coverage.v1"?(_1=_2.$0,1):2:_2.$0!="ta-loaded-coverage.v1"?(_1=_2.$0,1):2:_2.$0!="ta-loaded-coverage.v1"?(_1=_2.$0,1):2:_2.$0!="ta-loaded-coverage.v1"?(_1=_2.$0,1):2:_2.$0!="ta-loaded-coverage.v1"?(_1=_2.$0,1):2:_2.$0!="ta-loaded-coverage.v1"?(_1=_2.$0,1):2:2){
        case 0:
          let _10, _11, _12, _13, _14;
          const active=_1[0];
          const _15=requiredInt64("startObservationOrdinal", active);
          const _16=requiredInt("observationCount", active);
          const _17=requiredText("baseAxisRef", active);
          if(_15.$==0&&(_16.$==0&&(_17.$==0&&(_10=[_17.$0, _16.$0, _15.$0],true)))){
            const baseAxisRef=_10[0];
            const m_1=Trim(_1[2]).toLowerCase();
            const _18=m_1=="complete"?Ok({$:0}):m_1=="partial"?Ok({$:1}):Error_1(error_1("invalid-loaded-coverage", "completeness", "Completeness must be complete or partial."));
            const m_2=fields.TryFind("totalObservationCount");
            switch(m_2!=null&&m_2.$==1?m_2.$0.$==0?0:m_2.$0.$==3?(_11=m_2.$0.$0,1):2:0){
              case 0:
                _12=Ok(null);
                break;
              case 1:
                let o;
                const m_3=(o=0n,[TryParse_1(_11, {get:() => o, set:(v) => {
                  o=v;
                }}), o]);
                _12=m_3[0]?Ok(Some(m_3[1])):Error_1(error_1("invalid-loaded-coverage", "totalObservationCount", "An invariant Int64 text value is required."));
                break;
              case 2:
                _12=Error_1(error_1("invalid-loaded-coverage", "totalObservationCount", "An invariant Int64 text value or null is required."));
                break;
            }
            const m_4=fields.TryFind("overviewAxisRef");
            switch(m_4!=null&&m_4.$==1?m_4.$0.$==3?!IsNullOrWhiteSpace(m_4.$0.$0)?(_13=m_4.$0.$0,1):2:2:0){
              case 0:
                _14=Ok(baseAxisRef);
                break;
              case 1:
                _14=Ok(_13);
                break;
              case 2:
                _14=Error_1(error_1("invalid-loaded-coverage", "overviewAxisRef", "A non-empty text value is required."));
                break;
            }
            if(_18.$==1)return Error_1(ofArray([_18.$0]));
            else if(_12.$==1)return Error_1(ofArray([_12.$0]));
            else if(_14.$==1)return Error_1(ofArray([_14.$0]));
            else {
              const projection={
                CoverageIdentity:_1[4], 
                CoverageRevision:_1[5], 
                QueryGeneration:_1[3], 
                Completeness:_18.$0, 
                TotalObservationCount:_12.$0, 
                Segments:_1[7], 
                OverviewAxisRef:_14.$0, 
                OverviewAnchors:_1[1], 
                ActiveDetail:{
                  StartObservationOrdinal:_10[2], 
                  ObservationCount:_10[1], 
                  BaseAxisRef:baseAxisRef
                }
              };
              const m_5=validationErrors(projection);
              return m_5.$==0?Ok(Some(projection)):Error_1(m_5);
            }
          }
          else return Error_1(ofArray([error_1("invalid-loaded-coverage", "activeDetail", "A valid active-detail object is required.")]));
          break;
        case 1:
          return Error_1(ofArray([error_1("unsupported-loaded-coverage", "schema", "Unsupported loaded coverage schema `"+_1+"`.")]));
        case 2:
          return Error_1(ofArray([error_1("invalid-loaded-coverage", "viewport.loadedCoverage", "The loaded coverage projection is malformed.")]));
      }
    }
    else return Error_1(ofArray([error_1("invalid-loaded-coverage", "viewport.loadedCoverage", "The loaded coverage projection must be an object.")]));
  }
  else return Ok(null);
}
function tryDataRef(values){
  let _1;
  const m=values.TryFind("viewport.loadedCoverageDataRef");
  switch(m!=null&&m.$==1?m.$0.$==3?!IsNullOrWhiteSpace(m.$0.$0)?(_1=m.$0.$0,1):2:2:0){
    case 0:
      return Ok(null);
    case 1:
      return Ok(Some(_1));
    case 2:
      return Error_1(ofArray([error_1("invalid-loaded-coverage-data-ref", "viewport.loadedCoverageDataRef", "A non-empty text dataRef is required.")]));
  }
}
function error_1(code, field_1, message){
  return{
    Code:code, 
    Field:field_1, 
    Message:message
  };
}
function apply(projection, values){
  return values.Add_1("viewport.loadedCoverage", encode(projection));
}
function requiredText(field_1, fields){
  let _1;
  const m=fields.TryFind(field_1);
  return m!=null&&m.$==1&&(m.$0.$==3&&(!IsNullOrWhiteSpace(m.$0.$0)&&(_1=m.$0.$0,true)))?Ok(_1):Error_1(error_1("invalid-loaded-coverage", field_1, "A non-empty text value is required."));
}
function requiredInt64(field_1, fields){
  let _1, o;
  const m=fields.TryFind(field_1);
  if(m!=null&&m.$==1&&(m.$0.$==3&&(_1=m.$0.$0,true))){
    const m_1=(o=0n,[TryParse_1(_1, {get:() => o, set:(v) => {
      o=v;
    }}), o]);
    return m_1[0]?Ok(m_1[1]):Error_1(error_1("invalid-loaded-coverage", field_1, "An invariant Int64 text value is required."));
  }
  else return Error_1(error_1("invalid-loaded-coverage", field_1, "An invariant Int64 text value is required."));
}
function decodeArray(field_1, decoder, fields){
  let _1;
  const m=fields.TryFind(field_1);
  return m!=null&&m.$==1&&(m.$0.$==4&&(_1=m.$0.$0,true))?sequenceResults(mapi(decoder, _1)):Error_1(ofArray([error_1("invalid-loaded-coverage", field_1, "An array is required.")]));
}
function decodeSegment(index, a){
  let _1, _2;
  if(a.$==5){
    const fields=a.$0;
    const m=[requiredText("segmentId", fields), requiredTime("startEventTimeUtc", fields), requiredTime("endEventTimeExclusiveUtc", fields), requiredInt64("startObservationOrdinal", fields), requiredInt64("observationCount", fields)];
    const c=m[0];
    if(c.$==0){
      const c_1=m[1];
      if(c_1.$==0){
        const c_2=m[2];
        if(c_2.$==0){
          const c_3=m[3];
          if(c_3.$==0){
            const c_4=m[4];
            _1=c_4.$==0?(_2=[c_4.$0, c_2.$0, c.$0, c_3.$0, c_1.$0],true):(_2=m,false);
          }
          else _1=(_2=m,false);
        }
        else _1=(_2=m,false);
      }
      else _1=(_2=m,false);
    }
    else _1=(_2=m,false);
    return _1?Ok({
      SegmentId:_2[2], 
      StartEventTimeUtc:_2[4], 
      EndEventTimeExclusiveUtc:_2[1], 
      StartObservationOrdinal:_2[3], 
      ObservationCount:_2[0]
    }):Error_1(ofSeq_1(delay(() => {
      const c_5=_2[0];
      let _3=c_5.$==1?[c_5.$0]:[];
      return append_2(_3, delay(() => {
        const c_6=_2[1];
        let _4=c_6.$==1?[c_6.$0]:[];
        return append_2(_4, delay(() => {
          const c_7=_2[2];
          let _5=c_7.$==1?[c_7.$0]:[];
          return append_2(_5, delay(() => {
            const c_8=_2[3];
            let _6=c_8.$==1?[c_8.$0]:[];
            return append_2(_6, delay(() => {
              const c_9=_2[4];
              return c_9.$==1?[c_9.$0]:[];
            }));
          }));
        }));
      }));
    })));
  }
  else return Error_1(ofArray([error_1("invalid-loaded-coverage", "segments["+String(index)+"]", "A segment object is required.")]));
}
function decodeAnchor(index, a){
  let _1;
  if(a.$==5){
    const fields=a.$0;
    const _2=requiredInt64("observationOrdinal", fields);
    const _3=requiredTime("eventTimeUtc", fields);
    const _4=fields.TryFind("value");
    return _2.$==0&&(_3.$==0&&(_4!=null&&_4.$==1&&(_1=[_3.$0, _2.$0, _4.$0],true)))?Ok({
      ObservationOrdinal:_1[1], 
      EventTimeUtc:_1[0], 
      Value:_1[2]
    }):Error_1(ofArray([error_1("invalid-loaded-coverage", "overviewAnchors["+String(index)+"]", "A valid ordinal, UTC event time and value are required.")]));
  }
  else return Error_1(ofArray([error_1("invalid-loaded-coverage", "overviewAnchors["+String(index)+"]", "An overview anchor object is required.")]));
}
function requiredInt(field_1, fields){
  let _1, _2;
  const m=fields.TryFind(field_1);
  if(m!=null&&m.$==1){
    if(m.$0.$==2){
      const value=m.$0.$0;
      _1=!isNaN(value)&&!(Math.abs(value)===Infinity)&&value===Math.floor(value)&&value>=-2147483648&&value<=2147483647&&(_2=m.$0.$0,true);
    }
    else _1=false;
  }
  else _1=false;
  return _1?Ok(toInt(_2)):Error_1(error_1("invalid-loaded-coverage", field_1, "An integer number is required."));
}
function validationErrors(projection){
  return ofSeq_1(delay(() => append_2(IsNullOrWhiteSpace(projection.CoverageIdentity)?[error_1("invalid-loaded-coverage", "coverageIdentity", "CoverageIdentity is required.")]:[], delay(() => append_2(projection.CoverageRevision<0n?[error_1("invalid-loaded-coverage", "coverageRevision", "CoverageRevision cannot be negative.")]:[], delay(() => append_2(projection.QueryGeneration<0n?[error_1("invalid-loaded-coverage", "queryGeneration", "QueryGeneration cannot be negative.")]:[], delay(() => append_2(IsNullOrWhiteSpace(projection.OverviewAxisRef)?[error_1("invalid-loaded-coverage", "overviewAxisRef", "OverviewAxisRef is required.")]:[], delay(() => append_2(length(projection.OverviewAnchors)>8000?[error_1("limit-loaded-coverage", "overviewAnchors", "At most "+String(8000)+" anchors are allowed.")]:[], delay(() => append_2(projection.ActiveDetail.StartObservationOrdinal<0n?[error_1("invalid-loaded-coverage", "activeDetail.startObservationOrdinal", "The active-detail ordinal cannot be negative.")]:[], delay(() => append_2(projection.ActiveDetail.ObservationCount<=0||projection.ActiveDetail.ObservationCount>4000?[error_1("invalid-loaded-coverage", "activeDetail.observationCount", "Active detail must contain 1.."+String(4000)+" observations.")]:[], delay(() => append_2(IsNullOrWhiteSpace(projection.ActiveDetail.BaseAxisRef)?[error_1("invalid-loaded-coverage", "activeDetail.baseAxisRef", "BaseAxisRef is required.")]:[], delay(() => {
    let _1, _2;
    const _3=projection.TotalObservationCount;
    switch(projection.Completeness.$==1?_3==null?3:2:_3!=null&&_3.$==1?_3.$0>0n?(_1=_3.$0,0):1:1){
      case 0:
        _2=[];
        break;
      case 1:
        _2=[error_1("invalid-loaded-coverage", "totalObservationCount", "Complete coverage requires a positive total observation count.")];
        break;
      case 2:
        _2=[error_1("invalid-loaded-coverage", "totalObservationCount", "Partial coverage must not claim a total observation count.")];
        break;
      case 3:
        _2=[];
        break;
    }
    return append_2(_2, delay(() => append_2(collect_1((m) => {
      const segment=m[1];
      const index=m[0];
      return append_2(IsNullOrWhiteSpace(segment.SegmentId)?[error_1("invalid-loaded-coverage", "segments["+String(index)+"].segmentId", "SegmentId is required.")]:[], delay(() => append_2(IsNullOrWhiteSpace(segment.StartEventTimeUtc)||IsNullOrWhiteSpace(segment.EndEventTimeExclusiveUtc)?[error_1("invalid-loaded-coverage", "segments["+String(index)+"].eventTime", "Segment UTC bounds are required.")]:[], delay(() => append_2(segment.StartObservationOrdinal<0n||segment.ObservationCount<0n?[error_1("invalid-loaded-coverage", "segments["+String(index)+"].ordinal", "Segment ordinals and counts cannot be negative.")]:[], delay(() => Compare(segment.StartEventTimeUtc, segment.EndEventTimeExclusiveUtc)>=0?[error_1("invalid-loaded-coverage", "segments["+String(index)+"].eventTime", "Segment end must be later than its start.")]:[]))))));
    }, indexed(projection.Segments)), delay(() => append_2(collect_1((index) => {
      const previous=get(projection.Segments, index-1);
      return get(projection.Segments, index).StartObservationOrdinal<previous.StartObservationOrdinal+previous.ObservationCount?[error_1("invalid-loaded-coverage", "segments["+String(index)+"]", "Coverage segments cannot overlap or move backward.")]:[];
    }, range(1, length(projection.Segments)-1)), delay(() => append_2(collect_1((m) => {
      const anchor=m[1];
      return anchor.ObservationOrdinal<0n||IsNullOrWhiteSpace(anchor.EventTimeUtc)?[error_1("invalid-loaded-coverage", "overviewAnchors["+String(m[0])+"]", "Overview anchors require a non-negative ordinal and UTC event time.")]:[];
    }, indexed(projection.OverviewAnchors)), delay(() => append_2(collect_1((index) => append_2(get(projection.OverviewAnchors, index).ObservationOrdinal<=get(projection.OverviewAnchors, index-1).ObservationOrdinal?[error_1("invalid-loaded-coverage", "overviewAnchors["+String(index)+"]", "Overview anchor ordinals must increase.")]:[], delay(() => Compare(get(projection.OverviewAnchors, index).EventTimeUtc, get(projection.OverviewAnchors, index-1).EventTimeUtc)<=0?[error_1("invalid-loaded-coverage", "overviewAnchors["+String(index)+"].eventTimeUtc", "Overview anchor event times must increase.")]:[])), range(1, length(projection.OverviewAnchors)-1)), delay(() => {
      const m=projection.TotalObservationCount;
      if(m==null)return[];
      else if(projection.ActiveDetail.StartObservationOrdinal+BigInt(projection.ActiveDetail.ObservationCount)>m.$0){
        m.$0;
        return[error_1("invalid-loaded-coverage", "activeDetail", "Active detail exceeds the complete observation domain.")];
      }
      else {
        const total=m.$0;
        return collect_1((m_1) => {
          const segment=m_1[1];
          return segment.StartObservationOrdinal+segment.ObservationCount>total?[error_1("invalid-loaded-coverage", "segments["+String(m_1[0])+"]", "Segment exceeds the complete observation domain.")]:[];
        }, indexed(projection.Segments));
      }
    }))))))))));
  }))))))))))))))))));
}
function encode(projection){
  let _1=["coverageRevision", invariantInt64(projection.CoverageRevision)];
  let _2=["queryGeneration", invariantInt64(projection.QueryGeneration)];
  let _3=["completeness", {$:3, $0:completenessText(projection.Completeness)}];
  const o=projection.TotalObservationCount;
  const o_1=o==null?null:Some(invariantInt64(o.$0));
  let _4=o_1==null?{$:0}:o_1.$0;
  let _5=["totalObservationCount", _4];
  let _6=[["schema", {$:3, $0:"ta-loaded-coverage.v1"}], ["coverageIdentity", {$:3, $0:projection.CoverageIdentity}], _1, _2, _3, _5, ["segments", {$:4, $0:map(encodeSegment, projection.Segments)}], ["overviewAxisRef", {$:3, $0:projection.OverviewAxisRef}], ["overviewAnchors", {$:4, $0:map(encodeAnchor, projection.OverviewAnchors)}], ["activeDetail", {$:5, $0:new FSharpMap("New", ofArray([["startObservationOrdinal", invariantInt64(projection.ActiveDetail.StartObservationOrdinal)], ["observationCount", {$:2, $0:projection.ActiveDetail.ObservationCount}], ["baseAxisRef", {$:3, $0:projection.ActiveDetail.BaseAxisRef}]]))}]];
  let _7=ofArray(_6);
  let _8=new FSharpMap("New", _7);
  return{$:5, $0:_8};
}
function sequenceResults(values){
  return Map_2((x) => ofList(rev(x)), fold((_1, _2) => _1.$==1?_2.$==1?Error_1(append_1(_1.$0, _2.$0)):Error_1(_1.$0):_2.$==1?Error_1(_2.$0):Ok(FSharpList.Cons(_2.$0, _1.$0)), Ok(FSharpList.Empty), values));
}
function requiredTime(field_1, fields){
  return Bind_2((value) => EndsWith(value, "Z")||EndsWith(value, "+00:00")?Ok(value):Error_1(error_1("invalid-loaded-coverage", field_1, "A UTC timestamp is required.")), requiredText(field_1, fields));
}
function invariantInt64(value){
  return{$:3, $0:String(value)};
}
function completenessText(a){
  return a.$==1?"partial":"complete";
}
function encodeSegment(segment){
  return{$:5, $0:new FSharpMap("New", ofArray([["segmentId", {$:3, $0:segment.SegmentId}], ["startEventTimeUtc", {$:3, $0:segment.StartEventTimeUtc}], ["endEventTimeExclusiveUtc", {$:3, $0:segment.EndEventTimeExclusiveUtc}], ["startObservationOrdinal", invariantInt64(segment.StartObservationOrdinal)], ["observationCount", invariantInt64(segment.ObservationCount)]]))};
}
function encodeAnchor(anchor){
  return{$:5, $0:new FSharpMap("New", ofArray([["observationOrdinal", invariantInt64(anchor.ObservationOrdinal)], ["eventTimeUtc", {$:3, $0:anchor.EventTimeUtc}], ["value", anchor.Value]]))};
}
function authoritativeDocumentAdvanced(baseIdentity, baseDocumentRevision, currentIdentity, currentDocumentRevision, bindingMatches){
  return bindingMatches&&(!Equals(currentIdentity, baseIdentity)||Compare(currentDocumentRevision, baseDocumentRevision)===1);
}
function New_76(ThemeName, Surface, OverviewSurface, Grid, Cursor, AxisSurface, AxisText, LegendText, Border, OverviewPrice, OverviewCandleUp, OverviewCandleDown, OverviewCandleFlat, OverviewSelection, OverviewBoundary, TooltipSurface, TooltipText, TooltipBorder){
  return{
    ThemeName:ThemeName, 
    Surface:Surface, 
    OverviewSurface:OverviewSurface, 
    Grid:Grid, 
    Cursor:Cursor, 
    AxisSurface:AxisSurface, 
    AxisText:AxisText, 
    LegendText:LegendText, 
    Border:Border, 
    OverviewPrice:OverviewPrice, 
    OverviewCandleUp:OverviewCandleUp, 
    OverviewCandleDown:OverviewCandleDown, 
    OverviewCandleFlat:OverviewCandleFlat, 
    OverviewSelection:OverviewSelection, 
    OverviewBoundary:OverviewBoundary, 
    TooltipSurface:TooltipSurface, 
    TooltipText:TooltipText, 
    TooltipBorder:TooltipBorder
  };
}
let _c_12=Lazy((_i) => class $StartupCode_Abbrev {
  static {
    _c_12=_i(this);
  }
  static counter;
  static {
    this.counter=0;
  }
});
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
  return _c_10.StringSet;
}
function StringGet(){
  return _c_10.StringGet;
}
function StringListSet(){
  return _c_10.StringListSet;
}
function StringListGet(){
  return _c_10.StringListGet;
}
function DateTimeSetUnchecked(){
  return _c_10.DateTimeSetUnchecked;
}
function DateTimeGetUnchecked(){
  return _c_10.DateTimeGetUnchecked;
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
  return _c_10.FileSetUnchecked;
}
function FileGetUnchecked(){
  return _c_10.FileGetUnchecked;
}
function IntSetUnchecked(){
  return _c_10.IntSetUnchecked;
}
function IntGetUnchecked(){
  return _c_10.IntGetUnchecked;
}
function IntSetChecked(){
  return _c_10.IntSetChecked;
}
function IntGetChecked(){
  return _c_10.IntGetChecked;
}
function FloatSetUnchecked(){
  return _c_10.FloatSetUnchecked;
}
function FloatGetUnchecked(){
  return _c_10.FloatGetUnchecked;
}
function FloatSetChecked(){
  return _c_10.FloatSetChecked;
}
function FloatGetChecked(){
  return _c_10.FloatGetChecked;
}
function isBlank_1(s){
  return forall_1(IsWhiteSpace, s);
}
class CheckedInput {
  get Input(){
    return this.$==1?this.$0:this.$==2?this.$0:this.$1;
  }
  static Blank(inputText_1){
    return Create_2(CheckedInput, {$:2, $0:inputText_1});
  }
  static Invalid(inputText_1){
    return Create_2(CheckedInput, {$:1, $0:inputText_1});
  }
  static Valid(value, inputText_1){
    return Create_2(CheckedInput, {
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
class InvalidOperationException extends Error {
  constructor(i, _1, _2){
    let message;
    if(i=="New"){
      message=_1;
      i="New_2";
      _1=message;
      _2=null;
    }
    if(i=="New_2"){
      const message_1=_1;
      const innerExn=_2;
      super(message_1);
      this.inner=innerExn;
    }
  }
}
let _c_13=Lazy((_i) => class $StartupCode_EditorAction {
  static {
    _c_13=_i(this);
  }
  static OptionKey;
  static ResultKind;
  static RequestKind;
  static Protocol;
  static limits;
  static {
    this.limits={
      MaxSchemaDepth:8, 
      MaxFields:128, 
      MaxChoicesPerField:128, 
      MaxListItems:128
    };
    this.Protocol="ptcs-dynamic-action.v1";
    this.RequestKind="action-request";
    this.ResultKind="action-result";
    this.OptionKey="ptcs.dynamic.editor.binding.v1";
  }
});
let _c_14=Lazy((_i) => class $StartupCode_RendererModel {
  static {
    _c_14=_i(this);
  }
  static MarkerCursorItemBudget;
  static DirectMarkerGlyphBudget;
  static TemporalSeriesTypeValue;
  static TemporalAxisTypeValue;
  static TemporalPointTypeValue;
  static TemporalPointTypeKey;
  static ResizeRight;
  static ResizeLeft;
  static Move;
  static initial;
  static {
    this.initial={
      Generation:0, 
      Pending:null, 
      LastCommitted:null
    };
    this.Move="move";
    this.ResizeLeft="resize-left";
    this.ResizeRight="resize-right";
    this.TemporalPointTypeKey="_type";
    this.TemporalPointTypeValue="temporal-point.v1";
    this.TemporalAxisTypeValue="temporal-axis.v1";
    this.TemporalSeriesTypeValue="temporal-series.v1";
    this.DirectMarkerGlyphBudget=4;
    this.MarkerCursorItemBudget=4;
  }
});
function resolve(defaultView){
  const v={Theme:{$:0}};
  const o=tryDecode_1(defaultView);
  return o==null?v:o.$0;
}
function tryDecode_1(defaultView){
  const m=defaultView.TryFind("ta.plotSurface.theme");
  switch(m!=null&&m.$==1?m.$0.$==3?m.$0.$0=="light"?0:m.$0.$0=="dark"?1:2:2:2){
    case 0:
      return Some({Theme:{$:0}});
    case 1:
      return Some({Theme:{$:1}});
    case 2:
      return null;
  }
}
function error_2(code, field_1, message){
  return{
    Code:code, 
    Field:field_1, 
    Message:message
  };
}
function identifier(field_1, value){
  return IsNullOrWhiteSpace(value)?ofArray([error_2("required", field_1, field_1+" is required.")]):value.length>128?ofArray([error_2("too-long", field_1, field_1+" exceeds 128 characters.")]):FSharpList.Empty;
}
function unsafeValue(field_1, value){
  if(!containsUnsafeValue(value))return FSharpList.Empty;
  else {
    const errors_1=MarkResizable([]);
    collectUnsafeValue(errors_1, field_1, value);
    return ofSeq_1(errors_1);
  }
}
function containsUnsafeValue(value){
  return value.$==3?unsafeText(value.$0):value.$==4?exists(containsUnsafeValue, value.$0):value.$==5&&exists_1((a) => {
    const a_1=KeyValue(a);
    return unsafeKey(a_1[0])||containsUnsafeValue(a_1[1]);
  }, value.$0);
}
function collectUnsafeValue(errors_1, field_1, value){
  if(value.$==3){
    const normalized=normalizedText(value.$0);
    if(StartsWith(normalized, "javascript:"))errors_1.push(error_2("script-forbidden", field_1, "Script URLs are forbidden."));
    else StartsWith(normalized, "http://")||StartsWith(normalized, "https://")?errors_1.push(error_2("url-forbidden", field_1, "Arbitrary URLs are forbidden in the shared runtime contract.")):void 0;
  }
  else if(value.$==4){
    const values=value.$0;
    for(let i=0, _2=values.length-1;i<=_2;i++)collectUnsafeValue(errors_1, field_1, get(values, i));
  }
  else if(value.$==5){
    const values_1=value.$0;
    const e=Get(values_1);
    try {
      while(e.MoveNext())
        {
          const a=KeyValue(e.Current);
          const key_1=a[0];
          if(unsafeKey(key_1))errors_1.push(error_2("unsafe-key", field_1, "Unsafe option key `"+key_1+"` is forbidden."));
          collectUnsafeValue(errors_1, field_1+"."+key_1, a[1]);
        }
    }
    finally {
      const _1=e;
      if(typeof _1=="object"&&isIDisposable(_1))e.Dispose();
    }
  }
}
function unsafeText(value){
  const normalized=normalizedText(value);
  return StartsWith(normalized, "javascript:")||StartsWith(normalized, "http://")||StartsWith(normalized, "https://");
}
function unsafeKey(value){
  const m=value==null?"":Trim(value).toLowerCase();
  return m=="script"||(m=="selector"||(m=="url"||m=="href"));
}
function normalizedText(value){
  return value==null?"":TrimStart(value, null).toLowerCase();
}
function tryDecode_2(options){
  let _1, _2;
  const _3=options.TryFind("overviewStripe.targetTraceId");
  const _4=options.TryFind("overviewStripe.collisionGroup");
  const _5=options.TryFind("overviewStripe.layerOrder");
  if(_3!=null&&_3.$==1){
    if(_3.$0.$==3){
      if(_4!=null&&_4.$==1){
        if(_4.$0.$==3){
          if(_5!=null&&_5.$==1){
            if(_5.$0.$==2){
              const targetTraceId=_3.$0.$0;
              const layerOrder=_5.$0.$0;
              const collisionGroup=_4.$0.$0;
              _1=!IsNullOrWhiteSpace(targetTraceId)&&!IsNullOrWhiteSpace(collisionGroup)&&collisionGroup.length<=64&&!isNaN(layerOrder)&&!(Math.abs(layerOrder)===Infinity)&&layerOrder===(layerOrder<0?Math.ceil(layerOrder):Math.floor(layerOrder))&&layerOrder>=0&&layerOrder<=63&&(_2=[_4.$0.$0, _5.$0.$0, _3.$0.$0],true);
            }
            else _1=false;
          }
          else _1=false;
        }
        else _1=false;
      }
      else _1=false;
    }
    else _1=false;
  }
  else _1=false;
  return _1?Some({
    TargetTraceId:_2[2], 
    CollisionGroup:_2[0], 
    LayerOrder:toInt(_2[1])
  }):null;
}
function decodeBucket(field_1, a){
  if(a.$==4){
    if(length(a.$0)<=64){
      const decoded=map((_1) => decode(String(field_1)+"["+String(_1[0])+"]", _1[1]), indexed(a.$0));
      const failures=concat_2(ofArray(choose((a_1) => a_1.$==1?Some(a_1.$0):null, decoded)));
      return failures.$==0?Ok(choose((a_1) => a_1.$==0?Some(a_1.$0):null, decoded)):Error_1(failures);
    }
    else return Error_1(ofArray([error_3("limit-overview-stripe-bucket", field_1, "Overview stripe bucket exceeds "+String(64)+" items.")]));
  }
  else return Error_1(ofArray([error_3("overview-stripe-bucket-required", field_1, "Overview stripe bucket must be an array.")]));
}
function decode(field_1, a){
  let _1, _2, _3, _4, _5, width, _6, label, _7, tooltip;
  if(a.$==5){
    const values=a.$0;
    const expected=new FSharpSet("New_2", OfSeq(ofArray(["_type", "stripeId", "eventTimeUtc", "color", "strokeWidthCssPixels", "label", "tooltip"])));
    const unknown=choose_1((_8) => {
      const key_1=_8[0];
      return expected.Contains(key_1)?null:Some(error_3("unknown-overview-stripe-field", field_1+"."+key_1, "Unknown overview stripe field `"+key_1+"`."));
    }, ofSeq_1(ToSeq(values)));
    const m=objectText_1("_type", values);
    const kind=m!=null&&m.$==1&&(m.$0=="ta-overview-stripe.v1"&&(_1=m.$0,true))?Ok(null):Error_1(error_3("overview-stripe-type-required", field_1+"."+"_type", "Expected `ta-overview-stripe.v1`."));
    const stripeId=requiredText_1("invalid-overview-stripe-id", 128, field_1+".stripeId", "stripeId", values);
    const m_1=objectText_1("eventTimeUtc", values);
    const eventTime=m_1!=null&&m_1.$==1&&(validUtcTimestamp(m_1.$0)&&(_2=m_1.$0,true))?Ok(_2):Error_1(error_3("invalid-overview-stripe-timestamp", field_1+".eventTimeUtc", "eventTimeUtc must be a bounded ISO-8601 UTC timestamp ending in Z or +00:00."));
    const m_2=objectText_1("color", values);
    const color=m_2!=null&&m_2.$==1&&(validColor(m_2.$0)&&(_3=m_2.$0,true))?Ok(_3):Error_1(error_3("invalid-overview-stripe-color", field_1+".color", "color must be #RGB, #RRGGBB or #RRGGBBAA."));
    const m_3=values.TryFind("strokeWidthCssPixels");
    if(m_3!=null&&m_3.$==1){
      if(m_3.$0.$==2){
        const value=m_3.$0.$0;
        _4=!isNaN(value)&&!(Math.abs(value)===Infinity)&&value>=MinimumStrokeWidthCssPixels()&&value<=MaximumStrokeWidthCssPixels()&&(_5=m_3.$0.$0,true);
      }
      else _4=false;
    }
    else _4=false;
    width=_4?Ok(_5):Error_1(error_3("invalid-overview-stripe-width", field_1+".strokeWidthCssPixels", "strokeWidthCssPixels must be between 0.5 and 4.0."));
    const m_4=values.TryFind("label");
    switch(m_4!=null&&m_4.$==1?m_4.$0.$==0?0:m_4.$0.$==3?m_4.$0.$0.length<=64?(_6=m_4.$0.$0,1):2:2:0){
      case 0:
        label=Ok(null);
        break;
      case 1:
        label=Ok(Some(_6));
        break;
      case 2:
        label=Error_1(error_3("invalid-overview-stripe-label", field_1+".label", "label must be at most "+String(64)+" characters."));
        break;
    }
    const m_5=values.TryFind("tooltip");
    switch(m_5!=null&&m_5.$==1?m_5.$0.$==4?length(m_5.$0.$0)<=16?(_7=m_5.$0.$0,0):1:2:2){
      case 0:
        const decoded=map((_8) => decodeTooltipField(String(field_1)+".tooltip["+String(_8[0])+"]", _8[1]), indexed(_7));
        const failures=concat_2(ofArray(choose((a_1) => a_1.$==1?Some(a_1.$0):null, decoded)));
        tooltip=failures.$==0?Ok(choose((a_1) => a_1.$==0?Some(a_1.$0):null, decoded)):Error_1(failures);
        break;
      case 1:
        tooltip=Error_1(ofArray([error_3("limit-overview-stripe-tooltip", field_1+".tooltip", "tooltip exceeds "+String(16)+" fields.")]));
        break;
      case 2:
        tooltip=Error_1(ofArray([error_3("overview-stripe-tooltip-required", field_1+".tooltip", "tooltip must be an array.")]));
        break;
    }
    const failures_1=append_1(unknown, append_1(choose_1((a_1) => a_1.$==1?Some(a_1.$0):null, ofArray([Map_2(() => { }, kind), Map_2(() => { }, stripeId), Map_2(() => { }, eventTime), Map_2(() => { }, color), Map_2(() => { }, width), Map_2(() => { }, label)])), tooltip.$==1?tooltip.$0:FSharpList.Empty));
    return failures_1.$==0?Ok({
      StripeId:DefaultValue("", stripeId), 
      EventTimeUtc:DefaultValue("", eventTime), 
      Color:DefaultValue("#000000", color), 
      StrokeWidthCssPixels:DefaultValue(1, width), 
      Label:DefaultValue(null, label), 
      Tooltip:DefaultValue([], tooltip)
    }):Error_1(failures_1);
  }
  else return Error_1(ofArray([error_3("overview-stripe-object-required", field_1, "Overview stripe must be an object.")]));
}
function error_3(code, field_1, message){
  return{
    Code:code, 
    Field:field_1, 
    Message:message
  };
}
function requiredText_1(code, maximum, field_1, key_1, values){
  let _1, _2;
  const m=objectText_1(key_1, values);
  if(m!=null&&m.$==1){
    const value=m.$0;
    _1=!IsNullOrWhiteSpace(value)&&value.length<=maximum&&(_2=m.$0,true);
  }
  else _1=false;
  return _1?Ok(_2):Error_1(error_3(code, field_1, String(key_1)+" must be nonblank and at most "+String(maximum)+" characters."));
}
function objectText_1(key_1, values){
  let _1;
  const m=values.TryFind(key_1);
  return m!=null&&m.$==1&&(m.$0.$==3&&(_1=m.$0.$0,true))?Some(_1):null;
}
function decodeTooltipField(field_1, a){
  if(a.$==5){
    const values=a.$0;
    const expected=new FSharpSet("New_2", OfSeq(ofArray(["key", "label", "value"])));
    const unknown=choose_1((_1) => {
      const key_2=_1[0];
      return expected.Contains(key_2)?null:Some(error_3("unknown-overview-stripe-field", field_1+"."+key_2, "Unknown overview stripe tooltip field `"+key_2+"`."));
    }, ofSeq_1(ToSeq(values)));
    const key_1=requiredText_1("invalid-overview-stripe-tooltip", 64, field_1+".key", "key", values);
    const label=requiredText_1("invalid-overview-stripe-tooltip", 64, field_1+".label", "label", values);
    const value=requiredText_1("invalid-overview-stripe-tooltip", 256, field_1+".value", "value", values);
    const failures=append_1(unknown, choose_1((a_1) => a_1.$==1?Some(a_1.$0):null, ofArray([key_1, label, value])));
    return failures.$==0?Ok({
      Key:DefaultValue("", key_1), 
      Label:DefaultValue("", label), 
      Value:DefaultValue("", value)
    }):Error_1(failures);
  }
  else return Error_1(ofArray([error_3("overview-stripe-tooltip-object-required", field_1, "Overview stripe tooltip item must be an object.")]));
}
function normalizeUtcTimestamp(value){
  let fractionEnd;
  const trimmed=Trim(value);
  const utc=EndsWith(trimmed, "+00:00")?Substring(trimmed, 0, trimmed.length-6)+"Z":trimmed;
  const dotIndex=utc.indexOf(".");
  const zIndex=utc.length-1;
  if(dotIndex<0||zIndex<=dotIndex||utc[zIndex]!=="Z")return utc;
  else {
    fractionEnd=zIndex;
    while(fractionEnd>dotIndex+1&&utc[fractionEnd-1]==="0")
      fractionEnd=fractionEnd-1;
    return fractionEnd===dotIndex+1?Substring(utc, 0, dotIndex)+"Z":Substring(utc, 0, fractionEnd)+"Z";
  }
}
function IsWhiteSpace(c){
  return c.match(new RegExp("\\s"))!==null;
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
function Create_1(f){
  return New_78(false, f, forceLazy);
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
let _c_15=Lazy((_i) => class $StartupCode_AppendList {
  static {
    _c_15=_i(this);
  }
  static Empty;
  static {
    this.Empty={$:0};
  }
});
function New_77(Year, Month, Day, Hour, Minute, Second){
  return{
    Year:Year, 
    Month:Month, 
    Day:Day, 
    Hour:Hour, 
    Minute:Minute, 
    Second:Second
  };
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
function tryDecode_3(options){
  let _1;
  const m=options.TryFind("marker.targetTraceId");
  return m!=null&&m.$==1&&(m.$0.$==3&&(!IsNullOrWhiteSpace(m.$0.$0)&&(_1=m.$0.$0,true)))?Some({TargetTraceId:_1}):null;
}
function tryDecode_4(options){
  let _1, _2;
  const _3=options.TryFind("histogram.positiveColor");
  const _4=options.TryFind("histogram.negativeColor");
  if(_3!=null&&_3.$==1){
    if(_3.$0.$==3){
      if(_4!=null&&_4.$==1){
        if(_4.$0.$==3){
          const positive=_3.$0.$0;
          const negative=_4.$0.$0;
          _1=validColor(positive)&&validColor(negative)&&(_2=[_4.$0.$0, _3.$0.$0],true);
        }
        else _1=false;
      }
      else _1=false;
    }
    else _1=false;
  }
  else _1=false;
  return _1?Some({PositiveColor:_2[1], NegativeColor:_2[0]}):null;
}
function shapeText(a){
  return a.$==1?"triangle-down":a.$==2?"circle":a.$==3?"square":a.$==4?"diamond":"triangle-up";
}
function fillText(a){
  return a.$==1?"outline":"solid";
}
function decodeBucket_1(field_1, a){
  if(a.$==4){
    if(length(a.$0)<=64){
      const decoded=map((_1) => decode_1(String(field_1)+"["+String(_1[0])+"]", _1[1]), indexed(a.$0));
      const failures=concat_2(ofArray(choose((a_1) => a_1.$==1?Some(a_1.$0):null, decoded)));
      return failures.$==0?Ok(choose((a_1) => a_1.$==0?Some(a_1.$0):null, decoded)):Error_1(failures);
    }
    else return Error_1(ofArray([error_4("limit-marker-bucket", field_1, "Marker bucket exceeds "+String(64)+" items.")]));
  }
  else return Error_1(ofArray([error_4("marker-bucket-required", field_1, "Marker bucket must be an array.")]));
}
function validColor(value){
  return!(value==null)&&(value.length===4||value.length===7||value.length===9)&&value[0]==="#"&&forall(isHex, ToCharArray(value.substring(1)));
}
function anchorText(a){
  return a.$==1?"below-bar":"above-bar";
}
function validUtcTimestamp(value){
  return!(value==null)&&(EndsWith(value, "Z")||EndsWith(value, "+00:00"))&&(!(value==null)&&value.length>=20&&value.length<=35&&value[4]==="-"&&value[7]==="-"&&(value[10]==="T"||value[10]==="t")&&value[13]===":"&&value[16]===":")&&(!(value==null)&&forall((index) => index<value.length&&isDigit(value[index]), [0, 1, 2, 3, 5, 6, 8, 9, 11, 12, 14, 15, 17, 18]));
}
function decode_1(field_1, a){
  let _1, _2, kind, anchor, _3, shape, fill_1, _4, _5, label, _6, tooltip;
  if(a.$==5){
    const values=a.$0;
    const expected=new FSharpSet("New_2", OfSeq(ofArray(["_type", "markerId", "eventTimeUtc", "anchor", "shape", "fill", "color", "label", "tooltip"])));
    const unknown=choose_1((_8) => {
      const key_1=_8[0];
      return expected.Contains(key_1)?null:Some(error_4("unknown-marker-field", field_1+"."+key_1, "Unknown marker field `"+key_1+"`."));
    }, ofSeq_1(ToSeq(values)));
    const markerType=objectText_2("_type", values);
    if(markerType!=null&&markerType.$==1){
      const value=markerType.$0;
      _1=(value=="ta-marker.v2"||value=="ta-marker.v1")&&(_2=markerType.$0,true);
    }
    else _1=false;
    kind=_1?Ok(null):Error_1(error_4("marker-type-required", field_1+"."+"_type", "Expected `ta-marker.v2` or legacy `ta-marker.v1`."));
    const markerId=requiredText_2(128, field_1+".markerId", "markerId", values);
    const m=objectText_2("eventTimeUtc", values);
    const eventTime=m==null?Error_1(error_4("required", field_1+".eventTimeUtc", "eventTimeUtc is required.")):validUtcTimestamp(m.$0)?Ok(m.$0):Error_1(error_4("invalid-timestamp", field_1+".eventTimeUtc", "eventTimeUtc must be a bounded ISO-8601 UTC timestamp ending in Z or +00:00."));
    const anchorTextValue=objectText_2("anchor", values);
    switch(anchorTextValue!=null&&anchorTextValue.$==1?anchorTextValue.$0=="above-bar"?0:anchorTextValue.$0=="below-bar"?1:2:2){
      case 0:
        anchor=Ok({$:0});
        break;
      case 1:
        anchor=Ok({$:1});
        break;
      case 2:
        anchor=Error_1(error_4("invalid-marker-anchor", field_1+".anchor", "anchor must be above-bar or below-bar."));
        break;
    }
    const _7=objectText_2("shape", values);
    switch(markerType!=null&&markerType.$==1?_7!=null&&_7.$==1?_7.$0=="triangle-up"?markerType.$0=="ta-marker.v2"?(_3=markerType.$0,0):10:_7.$0=="triangle-down"?markerType.$0=="ta-marker.v2"?(_3=markerType.$0,1):10:_7.$0=="circle"?markerType.$0=="ta-marker.v2"?(_3=markerType.$0,2):markerType.$0=="ta-marker.v1"?(_3=markerType.$0,7):10:_7.$0=="square"?markerType.$0=="ta-marker.v2"?(_3=markerType.$0,3):markerType.$0=="ta-marker.v1"?(_3=markerType.$0,8):10:_7.$0=="diamond"?markerType.$0=="ta-marker.v2"?(_3=markerType.$0,4):markerType.$0=="ta-marker.v1"?(_3=markerType.$0,9):10:_7.$0=="arrow"?anchorTextValue!=null&&anchorTextValue.$==1?anchorTextValue.$0=="above-bar"?markerType.$0=="ta-marker.v1"?(_3=markerType.$0,5):10:anchorTextValue.$0=="below-bar"?markerType.$0=="ta-marker.v1"?(_3=markerType.$0,6):10:10:10:10:10:10){
      case 0:
        shape=Ok({$:0});
        break;
      case 1:
        shape=Ok({$:1});
        break;
      case 2:
        shape=Ok({$:2});
        break;
      case 3:
        shape=Ok({$:3});
        break;
      case 4:
        shape=Ok({$:4});
        break;
      case 5:
        shape=Ok({$:1});
        break;
      case 6:
        shape=Ok({$:0});
        break;
      case 7:
        shape=Ok({$:2});
        break;
      case 8:
        shape=Ok({$:3});
        break;
      case 9:
        shape=Ok({$:4});
        break;
      case 10:
        shape=Error_1(error_4("invalid-marker-shape", field_1+".shape", "shape is not supported by the declared marker version."));
        break;
    }
    const m_1=objectText_2("fill", values);
    switch(m_1!=null&&m_1.$==1?m_1.$0=="solid"?0:m_1.$0=="outline"?1:2:2){
      case 0:
        fill_1=Ok({$:0});
        break;
      case 1:
        fill_1=Ok({$:1});
        break;
      case 2:
        fill_1=Error_1(error_4("invalid-marker-fill", field_1+".fill", "fill must be solid or outline."));
        break;
    }
    const m_2=objectText_2("color", values);
    const color=m_2!=null&&m_2.$==1&&(validColor(m_2.$0)&&(_4=m_2.$0,true))?Ok(_4):Error_1(error_4("invalid-marker-color", field_1+".color", "color must be #RGB, #RRGGBB or #RRGGBBAA."));
    const m_3=values.TryFind("label");
    switch(m_3!=null&&m_3.$==1?m_3.$0.$==0?0:m_3.$0.$==3?m_3.$0.$0.length<=64?(_5=m_3.$0.$0,1):2:2:0){
      case 0:
        label=Ok(null);
        break;
      case 1:
        label=Ok(Some(_5));
        break;
      case 2:
        label=Error_1(error_4("invalid-marker-label", field_1+".label", "label must be at most "+String(64)+" characters."));
        break;
    }
    const m_4=values.TryFind("tooltip");
    switch(m_4!=null&&m_4.$==1?m_4.$0.$==4?length(m_4.$0.$0)<=16?(_6=m_4.$0.$0,0):1:2:2){
      case 0:
        const decoded=map((_8) => decodeTooltipField_1(String(field_1)+".tooltip["+String(_8[0])+"]", _8[1]), indexed(_6));
        const failures=concat_2(ofArray(choose((a_1) => a_1.$==1?Some(a_1.$0):null, decoded)));
        tooltip=failures.$==0?Ok(choose((a_1) => a_1.$==0?Some(a_1.$0):null, decoded)):Error_1(failures);
        break;
      case 1:
        tooltip=Error_1(ofArray([error_4("limit-marker-tooltip", field_1+".tooltip", "tooltip exceeds "+String(16)+" fields.")]));
        break;
      case 2:
        tooltip=Error_1(ofArray([error_4("marker-tooltip-required", field_1+".tooltip", "tooltip must be an array.")]));
        break;
    }
    const failures_1=append_1(unknown, append_1(errors(ofArray([kind, Map_2(() => { }, markerId), Map_2(() => { }, eventTime), Map_2(() => { }, anchor), Map_2(() => { }, shape), Map_2(() => { }, fill_1), Map_2(() => { }, color), Map_2(() => { }, label)])), tooltip.$==1?tooltip.$0:FSharpList.Empty));
    return failures_1.$==0?Ok({
      MarkerId:DefaultValue("", markerId), 
      EventTimeUtc:DefaultValue("", eventTime), 
      Anchor:DefaultValue({$:0}, anchor), 
      Shape:DefaultValue({$:2}, shape), 
      Fill:DefaultValue({$:0}, fill_1), 
      Color:DefaultValue("#000000", color), 
      Label:DefaultValue(null, label), 
      Tooltip:DefaultValue([], tooltip)
    }):Error_1(failures_1);
  }
  else return Error_1(ofArray([error_4("marker-object-required", field_1, "Marker must be an object.")]));
}
function error_4(code, field_1, message){
  return{
    Code:code, 
    Field:field_1, 
    Message:message
  };
}
function isHex(value){
  return value>="0"&&value<="9"||value>="a"&&value<="f"||value>="A"&&value<="F";
}
function isDigit(value){
  return value>="0"&&value<="9";
}
function objectText_2(key_1, values){
  let _1;
  const m=values.TryFind(key_1);
  return m!=null&&m.$==1&&(m.$0.$==3&&(_1=m.$0.$0,true))?Some(_1):null;
}
function requiredText_2(maximum, field_1, key_1, values){
  const m=objectText_2(key_1, values);
  if(m==null)return Error_1(error_4("required", field_1, field_1+" is required."));
  else {
    const value=m.$0;
    return!IsNullOrWhiteSpace(value)&&value.length<=maximum?Ok(m.$0):Error_1(error_4("invalid-text", field_1, String(field_1)+" must be nonblank and at most "+String(maximum)+" characters."));
  }
}
function decodeTooltipField_1(field_1, a){
  if(a.$==5){
    const values=a.$0;
    const expected=new FSharpSet("New_2", OfSeq(ofArray(["key", "label", "value"])));
    const unknown=choose_1((_1) => {
      const key_2=_1[0];
      return expected.Contains(key_2)?null:Some(error_4("unknown-marker-field", field_1+"."+key_2, "Unknown marker tooltip field `"+key_2+"`."));
    }, ofSeq_1(ToSeq(values)));
    const key_1=requiredText_2(64, field_1+".key", "key", values);
    const label=requiredText_2(64, field_1+".label", "label", values);
    const value=requiredText_2(256, field_1+".value", "value", values);
    const m=append_1(unknown, errors(ofArray([key_1, label, value])));
    return m.$==0?Ok({
      Key:DefaultValue("", key_1), 
      Label:DefaultValue("", label), 
      Value:DefaultValue("", value)
    }):Error_1(m);
  }
  else return Error_1(ofArray([error_4("marker-tooltip-object-required", field_1, "Marker tooltip item must be an object.")]));
}
function errors(values){
  return choose_1((a) => a.$==1?Some(a.$0):null, values);
}
function New_78(created, evalOrVal, force){
  return{
    c:created, 
    v:evalOrVal, 
    f:force
  };
}
function MinimumStrokeWidthCssPixels(){
  return _c_16.MinimumStrokeWidthCssPixels;
}
function MaximumStrokeWidthCssPixels(){
  return _c_16.MaximumStrokeWidthCssPixels;
}
class OverflowException extends Error {
  constructor(i, _1){
    if(i=="New_1"){
      const message=_1;
      super(message);
    }
  }
}
function enumUsing(x, f){
  return{GetEnumerator:() => {
    let enum_1;
    try {
      enum_1=Get(f(x));
    }
    catch(e){
      let c;
      c=x;
      c.Dispose();
      throw e;
    }
    return new T(null, null, (e_1) => enum_1.MoveNext()&&(e_1.c=enum_1.Current,true), () => {
      let c_1;
      enum_1.Dispose();
      c_1=x;
      c_1.Dispose();
    });
  }};
}
function enumWhile(f, s){
  return{GetEnumerator:() => {
    function next(en){
      while(true)
        {
          const m=en.s;
          if(Equals(m, null)){
            if(f()){
              en.s=Get(s);
              en=en;
            }
            else return false;
          }
          else if(m.MoveNext()){
            en.c=m.Current;
            return true;
          }
          else {
            m.Dispose();
            en.s=null;
            en=en;
          }
        }
    }
    return new T(null, null, next, (en) => {
      const x=en.s;
      if(!Equals(x, null))x.Dispose();
    });
  }};
}
let _c_16=Lazy((_i) => class $StartupCode_RuntimeTypes {
  static {
    _c_16=_i(this);
  }
  static TypeValue_1;
  static TypeKey_1;
  static LayerOrderKey;
  static CollisionGroupKey;
  static TargetTraceIdKey_1;
  static MaximumStrokeWidthCssPixels;
  static MinimumStrokeWidthCssPixels;
  static MaximumLayerOrder;
  static MinimumLayerOrder;
  static MaxStripesPerFrame;
  static MaxStripesPerDataRef;
  static MaxStripesPerBucket;
  static MaxTooltipFields_1;
  static MaxCollisionGroupLength;
  static MaxLabelLength_1;
  static MaxStripeIdLength;
  static NegativeColorKey;
  static PositiveColorKey;
  static ThemeKey;
  static LegacyTypeValue;
  static TypeValue;
  static TypeKey;
  static TargetTraceIdKey;
  static MaxMarkersPerFrame;
  static MaxMarkersPerDataRef;
  static MaxMarkersPerLane;
  static MaxMarkersPerBucket;
  static MaxTooltipValueLength;
  static MaxTooltipLabelLength;
  static MaxTooltipKeyLength;
  static MaxTooltipFields;
  static MaxLabelLength;
  static MaxMarkerIdLength;
  static limits;
  static MaximumVisibleRangeBasePoints;
  static markerProtocol;
  static protocol;
  static values;
  static {
    this.values=[{$:0}, {$:1}, {$:2}, {$:3}];
    this.protocol="sdui-runtime.v1";
    this.markerProtocol="sdui-runtime.v2";
    this.MaximumVisibleRangeBasePoints=4000;
    this.limits={
      MaxRowsPerCanvas:8, 
      MaxTracesPerRow:32, 
      MaxTotalTraces:64, 
      MaxInitialBarsPerSeries:5000, 
      MaxRetainedBarsPerSeries:4000, 
      MaxPatchOperations:64, 
      MaxPatchItems:500, 
      MaxFrameBytes:16*1024*1024, 
      MinimumPollInterval:5*1E3
    };
    this.MaxMarkerIdLength=128;
    this.MaxLabelLength=64;
    this.MaxTooltipFields=16;
    this.MaxTooltipKeyLength=64;
    this.MaxTooltipLabelLength=64;
    this.MaxTooltipValueLength=256;
    this.MaxMarkersPerBucket=64;
    this.MaxMarkersPerLane=64;
    this.MaxMarkersPerDataRef=10000;
    this.MaxMarkersPerFrame=20000;
    this.TargetTraceIdKey="marker.targetTraceId";
    this.TypeKey="_type";
    this.TypeValue="ta-marker.v2";
    this.LegacyTypeValue="ta-marker.v1";
    this.ThemeKey="ta.plotSurface.theme";
    this.PositiveColorKey="histogram.positiveColor";
    this.NegativeColorKey="histogram.negativeColor";
    this.MaxStripeIdLength=128;
    this.MaxLabelLength_1=64;
    this.MaxCollisionGroupLength=64;
    this.MaxTooltipFields_1=16;
    this.MaxStripesPerBucket=64;
    this.MaxStripesPerDataRef=10000;
    this.MaxStripesPerFrame=20000;
    this.MinimumLayerOrder=0;
    this.MaximumLayerOrder=63;
    this.MinimumStrokeWidthCssPixels=0.5;
    this.MaximumStrokeWidthCssPixels=4;
    this.TargetTraceIdKey_1="overviewStripe.targetTraceId";
    this.CollisionGroupKey="overviewStripe.collisionGroup";
    this.LayerOrderKey="overviewStripe.layerOrder";
    this.TypeKey_1="_type";
    this.TypeValue_1="ta-overview-stripe.v1";
  }
});
function Clear(a){
  a.splice(0, length(a));
}
Main();

