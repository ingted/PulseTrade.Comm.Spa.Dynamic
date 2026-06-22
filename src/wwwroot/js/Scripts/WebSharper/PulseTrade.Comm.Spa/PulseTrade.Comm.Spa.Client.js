import { TrimEnd, StartsWith, Trim, TrimStart, concat, Substring } from "./Microsoft.FSharp.Core.StringModule.js"
import { Some } from "./Microsoft.FSharp.Core.FSharpOption`1.js"
import { Compare, Equals } from "./Microsoft.FSharp.Core.Operators.Unchecked.js"
import { length, get } from "./Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicFunctions.js"
import { readJson, cacheKey, readPendingRealitySplit, writeWatermark, readAllPending, deletePendingThen, readWatermark, writePending, writeJson } from "./PulseTrade.Comm.Spa.Client.BrowserDb.js"
import { iter, tryFind, sortBy, filter, fold, map, exists, forall2, tryHead, ofSeq, distinct, distinctBy, concat as concat_1, collect, skip } from "./Microsoft.FSharp.Collections.ArrayModule.js"
import { json, tryJson } from "./PulseTrade.Comm.Spa.Client.Decode.js"
import { New } from "./PulseTrade.Comm.Spa.SyncSubscribeRequestDto.js"
import { New as New_1 } from "./PulseTrade.Comm.Spa.SyncReadTailRequestDto.js"
import { New as New_2 } from "./PulseTrade.Comm.Spa.ActorsReplyDto.js"
import FSharpList from "./Microsoft.FSharp.Collections.FSharpList`1.js"
import { New as New_3 } from "./PulseTrade.Comm.Spa.SyncStreamKeyDto.js"
import { New as New_4 } from "./PulseTrade.Comm.Spa.ActorDto.js"
import { New as New_5 } from "./PulseTrade.Comm.Spa.ActorNodeDto.js"
import { New as New_6 } from "./PulseTrade.Comm.Spa.AppendPageLineageDto.js"
import { New as New_7 } from "./PulseTrade.Comm.Spa.AppendPageReadRepairHealthDto.js"
import { ofArray } from "./Microsoft.FSharp.Collections.ListModule.js"
import { New as New_8 } from "./PulseTrade.Comm.Spa.AppendPageSnapshotDto.js"
import { New as New_9 } from "./PulseTrade.Comm.Spa.AppendPageBucketViewDto.js"
import { New as New_10 } from "./PulseTrade.Comm.Spa.AppendPageKeyRequestDto.js"
import { New as New_11 } from "./PulseTrade.Comm.Spa.RemoveAppendPageKeyRequestDto.js"
import { New as New_12 } from "./PulseTrade.Comm.Spa.RemoveAppendPageRequestDto.js"
import { New as New_13 } from "./PulseTrade.Comm.Spa.AppendPageAppendRequestDto.js"
import { New as New_14 } from "./PulseTrade.Comm.Spa.ActorArguSendRequestDto.js"
import { New as New_15 } from "./PulseTrade.Comm.Spa.SyncActorArguRequestDto.js"
import { delay, append as append_1, forall } from "./Microsoft.FSharp.Collections.SeqModule.js"
import { New as New_16 } from "./PulseTrade.Comm.Spa.SyncAppendRequestDto.js"
import { New as New_17 } from "./PulseTrade.Comm.Spa.SyncAppendPageRequestDto.js"
import { MarkResizable } from "./Runtime.js"
import { New as New_18 } from "./PulseTrade.Comm.Spa.SetValueDto.js"
import { New as New_19 } from "./PulseTrade.Comm.Spa.SetBucketDto.js"
import { New as New_20 } from "./PulseTrade.Comm.Spa.SetsReplyDto.js"
import { New as New_21 } from "./PulseTrade.Comm.Spa.ThreadReplyDto.js"
import { New as New_22 } from "./PulseTrade.Comm.Spa.MessageDto.js"
import { New as New_23 } from "./PulseTrade.Comm.Spa.SendRequestDto.js"
import { New as New_24 } from "./PulseTrade.Comm.Spa.SyncChatSendRequestDto.js"
import { New as New_25 } from "./PulseTrade.Comm.Spa.RegisterAppendPageRequestDto.js"
import { New as New_26 } from "./PulseTrade.Comm.Spa.AppendPagesReplyDto.js"
import $StartupCode_Client from "./$StartupCode_Client.js"
import { New as New_27 } from "./PulseTrade.Comm.Spa.PendingCommandDto.js"
import { New as New_28 } from "./PulseTrade.Comm.Spa.AppendPageDefinitionDto.js"
import { New as New_29 } from "./PulseTrade.Comm.Spa.ClientAppendPageShapeRegistrationDto.js"
export function Main(){
  let mountedPageElement, mountedAppendPageResolved, mounted, appendRegistryWsState, appendRegistryPageCount, appendRegistryMaxSequence, appendRegistrySocket, queuedAppendRegistryFrames, appendRegistrySubscribed, appendRegistryTailRequested;
  if(!(doc().body==null))doc().body.setAttribute("data-server-reality-id", currentServerRealityId());
  const trimmed=TrimEnd(asText(globalThis.location.pathname), ["/"]);
  const path=isBlank(trimmed)?"/chat":trimmed;
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
      const pages_1=arrayOrEmpty(pages);
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
      if(path!="/sets"&&path!="/actors"&&path!="/chat"&&!mountedAppendPageResolved){
        const _2=findAppendPage(path, arrayOrEmpty(data_1.pages));
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
      sendAppendRegistryFrame(JSON.stringify(New("subscribe", newRequestId("append-pages-subscribe"), streamKey)));
    }
    if(!appendRegistryTailRequested){
      appendRegistryTailRequested=true;
      sendAppendRegistryFrame(JSON.stringify(New_1("read-tail", newRequestId("append-pages-read-tail"), streamKey, defaultCacheLimit())));
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
export function refreshAppendNav(activePath){
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
export function findAppendPage(path, pages){
  return tryFind((page) => isCurrentPage(path, pagePath(page))||isCurrentPage(path, "/page/"+asText(page.pageId))||isCurrentPage(path, "/"+asText(page.pageId)), arrayOrEmpty(pages));
}
export function mountUnknownPage(page, path){
  page.className="page actors-page";
  page.appendChild(element("div", "empty", "No append page is registered for "+String(path)+"."));
}
export function mountActors(page){
  let actorSnapshot, syncSocket, queuedSyncFrames, subscribedRegistry, registryTailRequested;
  page.className="page actors-page";
  const head=element("div", "work-head actors-head", null);
  const title=element("div", "", null);
  const actions=element("div", "head-actions", null);
  const status=element("div", "state", "Loading actors");
  const reload=button("", "Reload");
  const nodes=element("div", "nodes", null);
  append(title, [element("label", "", "Actor / Participant Management"), element("h1", "", "Actors")]);
  append(actions, [status, reload]);
  append(head, [title, actions]);
  append(page, [head, nodes]);
  const emptySnapshot=New_2(0, 0, 0n, []);
  actorSnapshot=emptySnapshot;
  syncSocket=null;
  queuedSyncFrames=[];
  subscribedRegistry=false;
  registryTailRequested=false;
  const cacheKey_1=cacheKey("actors-snapshot", FSharpList.Empty);
  const sameText=(left, right) => asText(left).toLowerCase()==asText(right).toLowerCase();
  const actorRegistryStreamKey=() => New_3("__actor-registry", "actor-registry", "__actors", ["__actors"]);
  const isAkkaAddress=(value) => {
    const text=asText(value).toLowerCase();
    return StartsWith(text, "akka://")||StartsWith(text, "akka.tcp://")||StartsWith(text, "akka.ssl.tcp://");
  };
  const applySnapshot=(source, data) => {
    actorSnapshot=data==null?emptySnapshot:data;
    clear(nodes);
    iter((node) => {
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
    }, arrayOrEmpty(actorSnapshot.nodes));
    return setStatus(status, "Loaded "+String(actorSnapshot.nodeCount)+" "+String(source)+" node(s), "+String(actorSnapshot.actorCount)+" actor(s)");
  };
  const load=() => {
    readJson(cacheKey_1, (a) => {
      if(a==null){ }
      else applySnapshot("cached", a.$0);
    });
    getJson("/actors/api/snapshot", (data) => {
      writeSnapshotWithWatermark(cacheKey_1, data, data.maxSequence, actorValueCount(data), "actors-snapshot");
      applySnapshot("backend", data);
    }, (t) => {
      setStatus(status, t);
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
      sendSyncFrame(JSON.stringify(New("subscribe", newRequestId("actors-subscribe"), streamKey)));
    }
  }
  function requestRegistryTail(){
    if(!registryTailRequested){
      registryTailRequested=true;
      sendSyncFrame(JSON.stringify(New_1("read-tail", newRequestId("actors-read-tail"), actorRegistryStreamKey(), defaultRenderLimit())));
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
          const actor=New_4(actorId, textOr(actorId, _1.displayName), textOr("actor", _1.kind), [nodeId, actorId].concat(tags), textOr("running", _1.status), arrayOrEmpty(_1.routees));
          const m=tryFind((node) => sameText(node.nodeId, nodeId), arrayOrEmpty(actorSnapshot.nodes));
          if(m==null)updatedNode=New_5(nodeId, nodeAddress, "up", roles, [actor]);
          else {
            const existing=m.$0;
            const actors=sortBy((row) => asText(row.actorId), filter((row) =>!sameText(row.actorId, actorId), arrayOrEmpty(existing.actors)).concat([actor]));
            updatedNode=New_5(existing.nodeId, isBlank(nodeAddress)?asText(existing.nodeAddress):nodeAddress, textOr("up", existing.status), length(roles)===0?arrayOrEmpty(existing.roles):roles, actors);
          }
          const nodes_1=sortBy((node) => asText(node.nodeId), filter((node) =>!sameText(node.nodeId, nodeId), arrayOrEmpty(actorSnapshot.nodes)).concat([updatedNode]));
          let _2=length(nodes_1);
          let _3=fold((_5, _6) => _5+_6, 0, map((node) => arrayOrEmpty(node.actors).length, nodes_1));
          const a=actorSnapshot.maxSequence;
          const b=event.sequence;
          let _4=Compare(a, b)===1?a:b;
          actorSnapshot=New_2(_2, _3, _4, nodes_1);
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
  requestRegistryTail();
}
export function mountAppendPage(page, definition){
  let currentLineageHealth, selected, selectedKeyJson, buckets, loadGeneration, visibleValueLimit, scrollValuesToBottomAfterNextRender, ensureSelectedSubscription, replayPendingCommands, deleteAcceptedPendingAppends, currentKeyMaxSequence, keyRegistryWsState, syncSocket, queuedSyncFrames, subscribedValueStream, keyRegistrySubscribed, keyRegistryTailRequested, pendingWsAppendIds, syncRepairScheduled, repairSyncAfterClose, replayingPending;
  page.className="page append-page";
  setData("tab-id", definition.tabId, setData("page-id", definition.pageId, setTestId("append-page-"+asText(definition.pageId), page)));
  const sameText=(left, right) => asText(left).toLowerCase()==asText(right).toLowerCase();
  const readsLegacy=sameText(definition.tabId, definition.pageId);
  let currentLineage=New_6(definition.tabId, readsLegacy?"default":"fresh", readsLegacy?definition.pageId:"", readsLegacy, readsLegacy?"read-current-tab-and-legacy-page-streams":"read-current-tab-stream-only");
  const applyLineage=(lineage) => {
    const lineage_1=lineage==null?currentLineage:lineage;
    currentLineage=lineage_1;
    setData("lineage-read-repair-policy", lineage_1.readRepairPolicy, setData("lineage-reads-legacy", lineage_1.readsLegacyPageStreams?"true":"false", setData("lineage-legacy-page-id-alias", lineage_1.legacyPageIdAlias, setData("lineage-kind", lineage_1.lineageKind, setData("lineage-stream-page-id", lineage_1.streamPageId, page)))));
  };
  applyLineage(currentLineage);
  const defaultLineageHealth=() => New_7(currentLineage.streamPageId, currentLineage.lineageKind, currentLineage.legacyPageIdAlias, currentLineage.readsLegacyPageStreams, currentLineage.readRepairPolicy, [], 0, [], 0);
  currentLineageHealth=defaultLineageHealth();
  selected="";
  selectedKeyJson="";
  buckets=[];
  loadGeneration=0;
  visibleValueLimit=defaultRenderLimit();
  scrollValuesToBottomAfterNextRender=false;
  const side=element("aside", "sidebar append-sidebar", null);
  const sideHead=element("div", "panel-head", null);
  const sideActions=element("div", "head-actions", null);
  const addKeyButton=setTestId("append-add-key", button("", "Add"));
  const removeKeyButton=setTestId("append-remove-key", button("", "Remove"));
  const removePageButton=setTestId("append-remove-page", button("", "Remove page"));
  const reload=setTestId("append-reload", button("", "Reload"));
  const filters=element("div", "filters", null);
  const keyFilter=setTestId("append-key-filter", input("key contains"));
  const newKeyInput=setTestId("append-key-input", input(textOr("\"Aster\"", definition.keyPlaceholder)));
  const status=setTestId("append-key-status", element("div", "state", "Loading"));
  const list=setTestId("append-key-list", element("div", "list", null));
  const work=setTestId("append-work", element("section", "append-work", null));
  const values=setTestId("append-values", element("div", "append-values", null));
  const form=setTestId("append-form", element("div", "append-form", null));
  const valueInput=setTestId("append-value-input", textarea("append-value-input", textOr("JSON value", definition.valuePlaceholder)));
  const directionInput=setTestId("append-direction", input("outbound-message"));
  const appendButton=setTestId("append-submit", button("primary", "Append"));
  const head=element("div", "work-head", null);
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
  append(sideActions, [addKeyButton, removeKeyButton, removePageButton, reload]);
  append(sideHead, [sideTitle]);
  append(filters, [newKeyInput, keyFilter, status]);
  append(side, [sideHead, sideActions, filters, list]);
  append(titleBox, [element("label", "", asText(definition.shape)+" / "+asText(definition.setName)), element("h2", "", pageTitle(definition)), element("div", "meta wrap", asText(definition.description)), lineageInfo]);
  append(head, [titleBox, workState]);
  const applyLineageHealth=(health) => {
    const health_1=health==null?defaultLineageHealth():health;
    currentLineageHealth=health_1;
    const valueStreamKeys=health_1.candidateValueStreamKeys==null?"":concat("\n", map(asText, health_1.candidateValueStreamKeys));
    const keyRegistryStreamKeys=health_1.candidateKeyRegistryStreamKeys==null?"":concat("\n", map(asText, health_1.candidateKeyRegistryStreamKeys));
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
  append(work, [head, pendingState, values, form]);
  append(page, [side, work]);
  const browserId=currentUserId();
  ensureSelectedSubscription=() => { };
  replayPendingCommands=() => { };
  deleteAcceptedPendingAppends=() =>() => null;
  const refreshPendingState=() => {
    readPendingRealitySplit((_2, _3) => renderPendingInspection(pendingState, filter((command) =>!(command==null)&&(sameText(command.target, definition.pageId)||!isBlank(command.payloadJson)&&command.payloadJson.indexOf("\"pageId\":\""+asText(definition.pageId)+"\"")!=-1), _2), filter((command) =>!(command==null)&&(sameText(command.target, definition.pageId)||!isBlank(command.payloadJson)&&command.payloadJson.indexOf("\"pageId\":\""+asText(definition.pageId)+"\"")!=-1), _3)));
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
    let _2=setData("selected-key-id", selected, n);
    let _3=setData("rendered-count", String(renderedCount), _2);
    let _4=setData("cached-count", String(cachedCount), _3);
    let _5=setData("min-sequence", String(minSequence), _4);
    let _6=setData("max-sequence", String(maxSequence), _5);
    let _7=setData("snapshot-seqid", String(snapshotSeqId), _6);
    setData("backend-gap", gapText, _7);
    browserCacheHealthBox.setAttribute("title", "cacheKey="+String(cacheKey_1)+"\nselectedKey="+String(selectedText)+"\nrendered="+String(renderedCount)+"\ncached="+String(cachedCount)+"\nrange="+String(minSequence)+".."+String(maxSequence)+"\nsnapshotSeqId="+String(snapshotSeqId)+"\nbackendGap="+String(gapText));
    browserCacheHealthBox.textContent="browser cache "+String(cacheKey_1)+" | rendered "+String(renderedCount)+" | cached "+String(cachedCount)+" | seq "+String(minSequence)+".."+String(maxSequence)+" | snapshot "+String(snapshotSeqId)+" | gap "+String(gapText);
  };
  const updateKeyRegistryHealth=() => {
    const cacheKey_1=keyRegistryCacheKey();
    const x=setData("ws-state", keyRegistryWsState, keyRegistryHealthBox);
    const x_1=setData("key-count", String(length(buckets)), x);
    let _2=setData("max-sequence", String(currentKeyMaxSequence), x_1);
    setData("cache-key", cacheKey_1, _2);
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
    const snapshot=New_8("ok", definition, length(buckets), fold((_2, _3) => Compare(_2, _3)===1?_2:_3, 0n, map((bucket) => bucket.maxSequence, buckets)), currentKeyMaxSequence, currentLineage, currentLineageHealth, buckets);
    writeSnapshotWithWatermark(stateCacheKey(), snapshot, snapshot.maxSequence, appendPageValueCount(snapshot), "append-page-state");
    writeAppendPageKeyWatermark(snapshot);
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
  function renderList(){
    clear(list);
    iter((bucket) => {
      const item=button(bucket.keyId==selected?"list-card active":"list-card", null);
      setData("max-sequence", String(bucket.maxSequence), setData("min-sequence", String(bucket.minSequence), setData("key-id", bucket.keyId, setTestId("append-key-card", item))));
      append(item, [element("div", "strong wrap", joinValues(bucket.keys)), element("div", "muted wrap", asText(bucket.setName)), element("div", "meta", "values="+String(bucket.valueCount)+" seq="+String(bucket.maxSequence)+" updated="+String(asText(bucket.updatedAtUtc)))]);
      item.addEventListener("click", () => {
        selected=bucket.keyId;
        selectedKeyJson=keysAsJson(bucket.keys);
        newKeyInput.value=selectedKeyJson;
        visibleValueLimit=defaultRenderLimit();
        renderList();
        requestValuesScrollToBottom();
        renderValues();
        return ensureSelectedSubscription();
      });
      list.appendChild(item);
    }, buckets);
  }
  function renderValues(){
    while(true)
      {
        let _2, _3, _4;
        clear(values);
        const x=(((n) =>(n_1) => setData(n, selected, n_1))("selected-key-id"))(work);
        ((((n) =>(n_1) => setData(n, selectedKeyJson, n_1))("selected-key-json"))(x));
        const bucket=(((p) =>(a_3) => tryFind(p, a_3))((bucket_2) => bucket_2.keyId==selected))(buckets);
        if(bucket!=null&&bucket.$==1){
          const bucket_1=bucket.$0;
          const allValues=arrayOrEmpty(bucket_1.values);
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
          const x_1=[asText(definition.tabId), asText(definition.shape), asText(definition.setName), concat("\u001f", arrayOrEmpty(bucket_1.keys))];
          const selectedValueStreamKey=(((s) =>(s_1) => concat(s, s_1))("\n"))(x_1);
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
          if(length(visible)===0)_2=void values.appendChild(element("div", "empty", "No values appended yet."));
          else {
            if(hiddenCached>0){
              const x_11=button("", "Load older ("+String(hiddenCached)+")");
              const loadOlder=(((i) =>(n) => setTestId(i, n))("append-load-older"))(x_11);
              _3=(loadOlder.addEventListener("click", ((allValues_1) =>() => {
                const a_3=length(allValues_1);
                const b_3=visibleValueLimit+defaultRenderLimit();
                visibleValueLimit=Compare(a_3, b_3)===-1?a_3:b_3;
                return renderValues();
              })(allValues)),void values.appendChild(loadOlder));
            }
            else if(backendGapAvailable){
              const x_12=button("", "Load older (backend)");
              const loadOlder_1=(((i) =>(n) => setTestId(i, n))("append-load-older"))(x_12);
              _3=(loadOlder_1.addEventListener("click", ((bucket_2, oldestSequence_1) =>() => readOlderFromBackend(bucket_2, oldestSequence_1))(bucket_1, oldestSequence)),void values.appendChild(loadOlder_1));
            }
            else _3=null;
            _2=(((a_3) =>(a_4) => {
              iter(a_3, a_4);
            })((value) => {
              values.appendChild(renderAppendValue(value));
            }))(visible);
          }
          _4=length(visible)<reportedCount?setStatus(workState, "Showing "+String(length(visible))+"/"+String(reportedCount)+" value(s)"):setStatus(workState, String(reportedCount)+" value(s)");
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
          values.appendChild(element("div", "empty", "No key selected."));
          _4=setStatus(workState, "No key selected");
        }
        return scrollValuesToBottomAfterNextRender?(scrollValuesToBottomAfterNextRender=false,scrollToBottomAfterRender(values)):null;
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
        let _2=Compare(a, b_2)===1?a:b_2;
        const a_1=bucket.maxSequence;
        const b_3=p[1];
        let _3=Compare(a_1, b_3)===1?a_1:b_3;
        return New_9(bucket.keyId, bucket.keys, bucket.setName, _2, p[0], _3, bucket.updatedAtUtc, merged);
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
              let _2=Compare(a, b)===1?a:b;
              const a_1=bucket_1.maxSequence;
              const b_1=p[1];
              let _3=Compare(a_1, b_1)===1?a_1:b_1;
              return New_9(bucket_1.keyId, bucket_1.keys, bucket_1.setName, _2, minSequence>0n?minSequence:bucket_1.minSequence, _3, bucket_1.updatedAtUtc, merged);
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
    applyLineage(data.lineage);
    applyLineageHealth(data.lineageHealth);
    const b=data.keyMaxSequence;
    currentKeyMaxSequence=Compare(currentKeyMaxSequence, b)===1?currentKeyMaxSequence:b;
    buckets=arrayOrEmpty(data.buckets);
    visibleValueLimit=defaultRenderLimit();
    if((isBlank(selected)||!exists((bucket) => bucket.keyId==selected, buckets))&&length(buckets)>0){
      selected=get(buckets, 0).keyId;
      selectedKeyJson=keysAsJson(get(buckets, 0).keys);
      newKeyInput.value=selectedKeyJson;
    }
    else length(buckets)===0?(selected="",selectedKeyJson=""):void 0;
    setStatus(status, "Loaded "+String(length(buckets))+" "+String(source)+" bucket(s)");
    renderList();
    requestValuesScrollToBottom();
    renderValues();
    ensureSelectedSubscription();
    iter((bucket) => {
      (deleteAcceptedPendingAppends(bucket))(bucket.values);
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
        const sequenceBuckets=filter((bucket) => bucket.maxSequence>0n, arrayOrEmpty(cached.buckets));
        if(length(sequenceBuckets)===0)fetchFullState();
        else {
          iter((_2) => readNewerFromBackend(generation, _2), sequenceBuckets);
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
  const selectedBucket=() => tryFind((bucket) => bucket.keyId==selected, buckets);
  deleteAcceptedPendingAppends=(bucket) =>(acceptedValues) => {
    const acceptedValues_1=arrayOrEmpty(acceptedValues);
    if(length(acceptedValues_1)>0){
      const keyJson=keysAsJson(bucket.keys);
      const commandMatches=(command) => {
        if(sameText(command.kind, "append-page-append-value")&&sameText(command.url, "/pages/api/append")&&isPendingForThisPage(command)&&!isBlank(command.payloadJson))try {
          const x=json(command.payloadJson);
          const _2=command.commandId;
          return sameText(x.pageId, definition.pageId)&&sameText(x.keyJson, keyJson)&&exists((value) => sameText(value.valueId, _2), acceptedValues_1);
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
  const streamKeyFor=(bucket) => New_3(definition.tabId, definition.shape, definition.setName, arrayOrEmpty(bucket.keys));
  const handleSyncEvent=(source, event) => {
    let o, updated, _2, o_1;
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
                const keys=filter((key) =>!isBlank(key), map(asText, arrayOrEmpty(wire.keys)));
                o=length(keys)===0?null:Some(keys);
              }
            }
            catch(m_3){
              o=null;
            }
            if(o==null)return null;
            else {
              const keys_1=o.$0;
              const b=event.sequence;
              currentKeyMaxSequence=Compare(currentKeyMaxSequence, b)===1?currentKeyMaxSequence:b;
              const filterText=currentFilterText();
              if(isBlank(filterText)||exists((key) => asText(key).toLowerCase().indexOf(filterText.toLowerCase())!=-1, arrayOrEmpty(keys_1))){
                const keyId=asText(definition.setName)+"::"+concat(" + ", arrayOrEmpty(keys_1));
                const m_2=tryFind((bucket_1) => sameText(bucket_1.keyId, keyId), buckets);
                if(m_2==null)updated=New_9(keyId, keys_1, definition.setName, 0, 0n, 0n, asText(event.createdAtUtc), []);
                else {
                  const existing=m_2.$0;
                  updated=New_9(existing.keyId, keys_1, definition.setName, existing.valueCount, existing.minSequence, existing.maxSequence, textOr(existing.updatedAtUtc, event.createdAtUtc), existing.values);
                }
                _2=(buckets=sortAppendPageBuckets(filter((bucket_1) =>!sameText(bucket_1.keyId, keyId), buckets).concat([updated])),isBlank(selected)||!exists((bucket_1) => sameText(bucket_1.keyId, selected), buckets)?(selected=keyId,selectedKeyJson=keysAsJson(keys_1),void(newKeyInput.value=selectedKeyJson)):null);
              }
              else _2=null;
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
    let _2, _3;
    if(syncSocket!=null&&syncSocket.$==1){
      const socket=syncSocket.$0;
      _2=(Equals(socket.readyState, 1)||Equals(socket.readyState, 0))&&(_3=syncSocket.$0,true);
    }
    else _2=false;
    if(_2)return _3;
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
          const responseType=asText(response.type).toLowerCase();
          const responseStatus=asText(response.status).toLowerCase();
          const requestId=asText(response.requestId);
          switch(responseStatus=="ok"?responseType=="subscribe"?0:responseType=="append"?1:responseType=="append-page"?1:responseType=="actor-argu"?1:responseType=="stream-event"?2:responseType=="read-tail"?3:responseType=="read"?3:responseType=="tail"?3:5:responseStatus=="error"?4:5){
            case 0:
              return asText(response.streamKey).indexOf("append-page-key-registry")!=-1?setKeyRegistryWsState("subscribed"):setWsState("subscribed");
            case 1:
              if(exists((id) => id==requestId, pendingWsAppendIds)){
                pendingWsAppendIds=filter((id) => id!=requestId, pendingWsAppendIds);
                deletePendingThen(requestId, () => {
                  valueInput.value="";
                  refreshPendingState();
                  setStatus(workState, "Appended through WebSocket");
                });
              }
              return handleSyncEvent("live", response.event);
            case 2:
              return handleSyncEvent("live", response.event);
            case 3:
              return iter((_4) => handleSyncEvent("tail", _4), arrayOrEmpty(response.events));
            case 4:
              return exists((id) => id==requestId, pendingWsAppendIds)?setStatus(workState, pendingFailure("WebSocket append", asText(response.error))):setStatus(status, "WebSocket sync error: "+asText(response.error));
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
    const streamKey=New_3(streamPageId, "append-page-key-registry", definition.setName, ["__append-page-keys", streamPageId]);
    if(!keyRegistrySubscribed){
      keyRegistrySubscribed=true;
      setKeyRegistryWsState("subscribing");
      sendSyncFrame(JSON.stringify(New("subscribe", newRequestId("append-page-keys-subscribe"), streamKey)));
    }
    if(!keyRegistryTailRequested){
      keyRegistryTailRequested=true;
      sendSyncFrame(JSON.stringify(New_1("read-tail", newRequestId("append-page-keys-read-tail"), streamKey, defaultCacheLimit())));
    }
  };
  let _1=(ensureSelectedSubscription=() => {
    const o=selectedBucket();
    if(o==null)void 0;
    else {
      const streamKey=streamKeyFor(o.$0);
      const identity=concat("\n", [asText(streamKey.pageId), asText(streamKey.mode), asText(streamKey.setName), concat("\u001f", arrayOrEmpty(streamKey.keys))]);
      if(!isBlank(identity)&&identity!=subscribedValueStream){
        subscribedValueStream=identity;
        setWsState("subscribing");
        sendSyncFrame(JSON.stringify(New("subscribe", newRequestId("subscribe"), streamKey)));
      }
    }
  },repairSyncAfterClose=() => {
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
  },replayingPending=false,replayPendingCommands=() => {
    if(!replayingPending){
      replayingPending=true;
      readAllPending((commands) => {
        let remaining, accepted;
        const mine=filter((command) => sameText(command.method, "POST")&&!isBlank(command.url)&&!isBlank(command.payloadJson), filter(isPendingForThisPage, commands));
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
                let _2, _3;
                accepted=accepted+1;
                if(sameText(command.kind, "append-page-remove-page")){
                  try {
                    const reply=json(isBlank(body)?"{}":body);
                    _2=!(reply==null)?writeAppendPagesDefinitions(reply):null;
                  }
                  catch(m){
                    _2=null;
                  }
                  _3=globalThis.location.assign("/chat");
                }
                else _3=void 0;
                finishOne();
              });
            }, () => {
              finishOne();
            });
          }, mine);
        }
      });
    }
  },addKeyButton.addEventListener("click", () => {
    const keyJson=isBlank(newKeyInput.value)?asText(definition.defaultKey):Trim(newKeyInput.value);
    if(isBlank(keyJson))setStatus(status, "Key JSON is required");
    else {
      const request=New_10(definition.pageId, keyJson);
      const pendingId=rememberPending("append-page-add-key", definition.pageId, "/pages/api/add-key", request);
      refreshPendingState();
      setStatus(status, "Adding key; pending command saved in browser DB");
      postAppendPageKey("/pages/api/add-key", request, (reply) => {
        deletePendingThen(pendingId, () => {
          !(reply.key==null)?(selected=reply.key.keyId,selectedKeyJson=keysAsJson(reply.key.keys),newKeyInput.value=selectedKeyJson):void 0;
          setStatus(status, "Key added");
          refreshPendingState();
          load();
        });
      }, (error) => {
        setStatus(status, pendingFailure("Add key", error));
        refreshPendingState();
      });
    }
  }),removeKeyButton.addEventListener("click", () => {
    if(isBlank(selected))return setStatus(status, "Select a key first");
    else {
      const removedKeyId=selected;
      const request=New_11(definition.pageId, removedKeyId);
      const pendingId=rememberPending("append-page-remove-key", definition.pageId, "/pages/api/remove-key", request);
      refreshPendingState();
      setStatus(status, "Removing key; pending command saved in browser DB");
      return postRemoveAppendPageKey("/pages/api/remove-key", request, () => {
        deletePendingThen(pendingId, () => {
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
    const request=New_12(definition.pageId);
    const pendingId=rememberPending("append-page-remove-page", definition.pageId, "/pages/api/remove-page", request);
    refreshPendingState();
    setStatus(status, "Removing page; pending command saved in browser DB");
    postJson("/pages/api/remove-page", request, (reply) => {
      deletePendingThen(pendingId, () => {
        writeAppendPagesDefinitions(reply);
        setStatus(status, "Page removed");
        globalThis.location.assign("/chat");
      });
    }, (error) => {
      setStatus(status, pendingFailure("Remove page", error));
      refreshPendingState();
    });
  }),reload.addEventListener("click", load),keyFilter.addEventListener("input", load),appendButton.addEventListener("click", () => {
    const request=New_13(definition.pageId, selectedKeyJson, Trim(valueInput.value), Trim(directionInput.value), ["web-append"]);
    if(isBlank(request.keyJson))return setStatus(workState, "Select or add a key first");
    else if(isBlank(request.valueText))return setStatus(workState, "Value text is required");
    else if(isActorArguPage(definition)){
      const request_1=New_14(definition.pageId, request.keyJson, request.valueText, ["web-append", "actor-argu"]);
      const m=selectedBucket();
      if(m!=null&&m.$==1){
        const bucket=m.$0;
        const o=tryHead(arrayOrEmpty(bucket.keys));
        const actorAddress=o==null?"":o.$0;
        if(isBlank(actorAddress))return setStatus(workState, "Actor address key is required");
        else {
          const pendingId=rememberPending("actor-argu-send", definition.pageId, "/pages/api/actor-argu/send", request_1);
          const wsRequest=New_15("actor-argu", pendingId, definition.pageId, definition.title, definition.setName, streamKeyFor(bucket), actorAddress, request_1.rawArgu, definition.shape, ofSeq(delay(() => append_1(arrayOrEmpty(definition.tags), delay(() => append_1(arrayOrEmpty(request_1.tags), delay(() => append_1(["page:"+asText(definition.pageId)], delay(() => append_1(["tab:"+asText(definition.tabId)], delay(() =>["shape:"+asText(definition.shape)])))))))))), browserId, definition.tabId);
          pendingWsAppendIds=pendingWsAppendIds.concat([pendingId]);
          refreshPendingState();
          setStatus(workState, "Sending through WebSocket; pending command saved in browser DB");
          ensureSelectedSubscription();
          sendSyncFrame(JSON.stringify(wsRequest));
          return scrollToBottomAfterRender(values);
        }
      }
      else return setStatus(workState, "Select or add a key first");
    }
    else if(sameText(definition.shape, "raw")){
      const m_1=selectedBucket();
      if(m_1!=null&&m_1.$==1){
        const bucket_1=m_1.$0;
        const pendingId_1=rememberPending("append-page-append-value", definition.pageId, "/pages/api/append", request);
        const wsRequest_1=New_16("append", pendingId_1, streamKeyFor(bucket_1), request.valueText, "append-page.value", definition.shape, pendingId_1, ofSeq(delay(() => append_1(arrayOrEmpty(definition.tags), delay(() => append_1(arrayOrEmpty(request.tags), delay(() => append_1(["page:"+asText(definition.pageId)], delay(() => append_1(["tab:"+asText(definition.tabId)], delay(() =>["shape:"+asText(definition.shape)])))))))))), browserId, definition.tabId);
        pendingWsAppendIds=pendingWsAppendIds.concat([pendingId_1]);
        refreshPendingState();
        setStatus(workState, "Appending through WebSocket; pending command saved in browser DB");
        ensureSelectedSubscription();
        sendSyncFrame(JSON.stringify(wsRequest_1));
        return scrollToBottomAfterRender(values);
      }
      else return setStatus(workState, "Select or add a key first");
    }
    else {
      const m_2=selectedBucket();
      if(m_2!=null&&m_2.$==1){
        const bucket_2=m_2.$0;
        const pendingId_2=rememberPending("append-page-append-value", definition.pageId, "/pages/api/append", request);
        const wsRequest_2=New_17("append-page", pendingId_2, definition.pageId, definition.title, definition.setName, streamKeyFor(bucket_2), request.keyJson, request.valueText, request.direction, definition.shape, pendingId_2, ofSeq(delay(() => append_1(arrayOrEmpty(definition.tags), delay(() => append_1(arrayOrEmpty(request.tags), delay(() => append_1(["page:"+asText(definition.pageId)], delay(() => append_1(["tab:"+asText(definition.tabId)], delay(() =>["shape:"+asText(definition.shape)])))))))))), browserId, definition.tabId);
        pendingWsAppendIds=pendingWsAppendIds.concat([pendingId_2]);
        refreshPendingState();
        setStatus(workState, "Appending through WebSocket; pending command saved in browser DB");
        ensureSelectedSubscription();
        sendSyncFrame(JSON.stringify(wsRequest_2));
        return scrollToBottomAfterRender(values);
      }
      else return setStatus(workState, "Select or add a key first");
    }
  }),load(),subscribeKeyRegistry(),refreshPendingState());
  _1;
}
export function renderAppendValue(value){
  let _1;
  const mode=asText(value.mode);
  const m=mode.toLowerCase();
  const className=m=="inbound-message"?"fcell-card fcell-chat inbound":m=="outbound-message"?"fcell-card fcell-chat outbound":m=="list"?"fcell-card fcell-list":m=="grid"?"fcell-card fcell-grid":"fcell-card";
  const card=setData("mode", mode, setTestId("append-value-card", element("div", className, null)));
  const head=element("div", "fcell-head", null);
  append(head, [element("span", "fcell-pill", fcellValueModeLabel(mode, value.tags)), element("span", "muted wrap", asText(value.valueId)+" / "+asText(value.createdAtUtc))]);
  card.appendChild(head);
  const m_1=mode.toLowerCase();
  switch(m_1){
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
      let _2;
      const table=element("table", "fcell-grid-table", null);
      const columns=arrayOrEmpty(value.columns);
      if(length(columns)>0){
        const thead=element("thead", "", null);
        const header=element("tr", "", null);
        _2=(iter((column) => {
          header.appendChild(element("th", "wrap", asText(column)));
        }, columns),thead.appendChild(header),void table.appendChild(thead));
      }
      else _2=null;
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
  if(!isBlank(value.source)&&mode.toLowerCase()!="inbound-message"&&mode.toLowerCase()!="outbound-message")card.appendChild(renderTextBlock("fcell-source", value.source));
  return card;
}
export function mountSets(page){
  let selected, buckets, syncSocket, queuedSyncFrames, subscribedStreams, tailRequestedStreams, registryTailRequested, ensureSetsSubscriptions, loadGeneration;
  page.className="page sets-grid";
  selected="";
  buckets=[];
  const side=element("aside", "sidebar", null);
  const sideHead=element("div", "panel-head", null);
  const reload=button("", "Reload");
  const filters=element("div", "filters", null);
  const keyFilter=input("key contains");
  const setFilter=input("set name");
  const status=element("div", "state", "Loading sets");
  const list=element("div", "list", null);
  const work=element("section", "work", null);
  append(sideHead, [element("h1", "", "Sets"), reload]);
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
  const sameText=(left, right) => asText(left).toLowerCase()==asText(right).toLowerCase();
  const streamIdentity=(streamKey) => concat("\n", [asText(streamKey.pageId), asText(streamKey.mode), asText(streamKey.setName), concat("\u001f", arrayOrEmpty(streamKey.keys))]);
  const setValueStreamKey=(pageId, mode, setName, keys) => New_3(asText(pageId), textOr("set", mode), asText(setName), arrayOrEmpty(keys));
  const currentFilterTexts=() =>[isBlank(keyFilter.value)?"":Trim(keyFilter.value), isBlank(setFilter.value)?"":Trim(setFilter.value)];
  const currentCacheKey=() => {
    const p=currentFilterTexts();
    return cacheKey("sets-state", ofArray([p[0], p[1]]));
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
      const detail=element("div", "detail", null);
      const head=element("div", "work-head", null);
      const title=element("div", "", null);
      append(title, [element("label", "", "Key set"), element("h2", "", bucket_1.keyId)]);
      append(head, [title, element("div", "state", String(bucket_1.valueCount)+" value(s)")]);
      detail.appendChild(head);
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
    getJson("/sets/api/state?"+concat("&", ofSeq(parts)), (data) => {
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
          const request=New_1("read-tail", newRequestId("sets-read-tail"), _1, defaultRenderLimit());
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
      sendSyncFrame(JSON.stringify(New("subscribe", newRequestId("sets-subscribe"), streamKey)));
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
      const m_1=asText(event.sourceKind).toLowerCase();
      if(m_1=="set.stream"){
        if(event==null||isBlank(event.payload))m=null;
        else try {
          const wire=json(event.payload);
          m=wire==null||asText(wire.schema)!="ptc.comm.spa.set.stream.v1"?null:Some(setValueStreamKey(wire.pageId, wire.mode, wire.setName, wire.keys));
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
          const setName=asText(event.streamKey.setName);
          const keys=arrayOrEmpty(event.streamKey.keys);
          const p=currentFilterTexts();
          const setText=p[1];
          const keyText=p[0];
          if((isBlank(setText)||sameText(setName, setText))&&(isBlank(keyText)||exists((key) => asText(key).toLowerCase().indexOf(keyText.toLowerCase())!=-1, arrayOrEmpty(keys)))){
            const value=New_18(textOr(event.eventId, event.sourceId), arrayOrEmpty(event.streamKey.keys), asText(event.createdAtUtc), asText(event.payload), arrayOrEmpty(event.tags));
            const keyId=asText(setName)+"::"+concat(" + ", arrayOrEmpty(keys));
            const m_2=tryFind((bucket) => sameText(bucket.keyId, keyId), buckets);
            if(m_2==null)updated=New_19(keyId, setName, keys, 1, event.sequence, asText(event.createdAtUtc), [value]);
            else {
              const existing=m_2.$0;
              const existingValues=arrayOrEmpty(existing.values);
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
              updated=New_19(existing.keyId, existing.setName, existing.keys, _1, _3, textOr(existing.updatedAtUtc, event.createdAtUtc), mergedValues);
            }
            buckets=sortBy((bucket) =>[asText(bucket.setName), asText(bucket.keyId)], arrayOrEmpty(filter((bucket) =>!sameText(bucket.keyId, keyId), buckets).concat([updated])));
            selected=keyId;
            renderList();
            renderDetail();
            const snapshot=New_20(fold((_4, _5) => Compare(_4, _5)===1?_4:_5, 0n, map((bucket) => bucket==null?0n:bucket.maxSequence, buckets)), buckets);
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
    const registryKey=New_3("__set-registry", "set-registry", "__sets", ["__sets"]);
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
export function mountChat(page){
  let selected, cursor, polling, participants, replayingPending, chatSocket, queuedChatSyncFrames, subscribedChatStream, pendingWsChatIds;
  selected="";
  cursor="";
  polling=false;
  participants=[];
  const participantId=currentUserId();
  page.className="page chat-grid";
  const side=element("aside", "sidebar", null);
  const sideHead=element("div", "panel-head", null);
  const reload=button("", "Reload");
  const list=element("div", "list", null);
  append(sideHead, [element("h1", "", "Chat"), reload]);
  append(side, [sideHead, element("div", "", null), list]);
  const work=setTestId("chat-work", element("section", "work", null));
  const workHead=element("div", "work-head", null);
  const titleBox=element("div", "", null);
  const toTitle=element("h2", "", "No participant selected");
  const state=element("div", "state", "Loading participants");
  const pendingState=setTestId("chat-pending-state", element("div", "state pending-state", ""));
  const thread=setTestId("thread-list", setId("thread-list", element("div", "thread-list", null)));
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
  const chatStreamKey=(peerId) => New_3("", "set", "chat", sameText(peerId, "channel.public")?["channel:public"]:[participantId, peerId]);
  const streamIdentity=(streamKey) => concat("\n", [asText(streamKey.pageId), asText(streamKey.mode), asText(streamKey.setName), concat("\u001f", arrayOrEmpty(streamKey.keys))]);
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
      if(!(message==null)&&!isBlank(message.messageId)&&doc().getElementById("thread-"+message.messageId)==null){
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
    scrollToBottomAfterRender(thread);
  }
  function loadParticipants(){
    setStatus(state, "Loading participants");
    readJson(participantsCacheKey, (a) => {
      if(a!=null&&a.$==1)if(a.$0,length(participants)===0){
        participants=arrayOrEmpty(a.$0.participants);
        isBlank(selected)&&length(participants)>0?selected=get(participants, 0).participantId:void 0;
        renderParticipants();
        setStatus(state, "Loaded "+String(length(participants))+" cached participant(s)");
        pollThread(true);
        ensureSelectedChatSubscription();
        replayPendingChatCommands();
      }
    });
    getJson("/chat/api/agents", (data) => {
      participants=arrayOrEmpty(data.participants);
      writeSnapshotWithWatermark(participantsCacheKey, data, 0n, length(participants), "chat-agents");
      isBlank(selected)&&length(participants)>0?selected=get(participants, 0).participantId:void 0;
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
              writeSnapshotWithWatermark(cacheKey_1, New_21(merged, nextAfterMessageId), _3, length(merged), "chat-thread");
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
          fetchThread(!isBlank(cursor));
        }
      });
      else fetchThread(!isBlank(cursor));
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
        const mine=filter((command) => sameText(command.method, "POST")&&!isBlank(command.url)&&!isBlank(command.payloadJson), filter(isPendingForThisChat, commands));
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
        writeSnapshotWithWatermark(cacheKey_1, New_21(merged, message.messageId), sequence>0n?sequence:maxMessageSequence(merged), length(merged), "chat-thread");
      });
    }
    else return null;
  }
  function handleChatSyncMessage(text){
    try {
      let o;
      const response=json(text);
      const responseType=asText(response.type).toLowerCase();
      const responseStatus=asText(response.status).toLowerCase();
      const requestId=asText(response.requestId);
      if(responseStatus=="ok"){
        if(responseType=="subscribe")setChatWsState("subscribed");
        else if(responseType=="chat-send"){
          exists((id) => id==requestId, pendingWsChatIds)?(pendingWsChatIds=filter((id) => id!=requestId, pendingWsChatIds),deletePendingThen(requestId, () => {
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
              o=Some(New_22(textOr(event_1.eventId, event_1.sourceId), "", participantId, "direct", asText(event_1.payload), asText(event_1.createdAtUtc)));
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
        sendChatSyncFrame(JSON.stringify(New("subscribe", newRequestId("chat-subscribe"), streamKey)));
      }
    }
  }
  function sendMessage(){
    const body=Trim(draft.value);
    if(isBlank(selected))setStatus(state, "Select a participant first");
    else if(isBlank(body))setStatus(state, "Message is empty");
    else {
      const request=New_23(participantId, selected, body, ["web-chat"]);
      const pendingId=rememberPending("chat-send", participantId+"->"+selected, "/chat/api/send", request);
      const wsRequest=New_24("chat-send", pendingId, participantId, selected, body, ["web-chat"], participantId, "chat");
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
export function mergeThreadMessages(existing, incoming){
  const v=distinctMessages(arrayOrEmpty(existing).concat(arrayOrEmpty(incoming)));
  return latestArray(defaultRenderLimit(), v);
}
export function distinctMessages(messages){
  let kept;
  kept=[];
  iter((message) => {
    if(!(message==null)&&!isBlank(message.messageId)&&!exists((row) => row.messageId==message.messageId, kept))kept=kept.concat([message]);
  }, arrayOrEmpty(messages));
  return kept;
}
export function keysAsJson(keys){
  const keys_1=arrayOrEmpty(keys);
  return length(keys_1)===1?JSON.stringify(get(keys_1, 0)):JSON.stringify(keys_1);
}
export function scrollToBottomAfterRender(node){
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
export function scrollToBottomNow(node){
  if(!(node==null)){
    try {
      node.scrollTop=node.scrollHeight;
    }
    catch(m){
      null;
    }
  }
}
export function setMain(node){
  const main=doc().getElementById("main");
  if(!(main==null)){
    clear(main);
    main.appendChild(node);
  }
}
export function shell(activePath, pages){
  const app=element("div", "app", null);
  const top=element("header", "topbar", null);
  const topRow=element("div", "topbar-main", null);
  const brandCluster=element("div", "brand-cluster", null);
  const navShell=element("div", "nav-shell", null);
  const navViewport=setTestId("nav-viewport", element("div", "nav-viewport", null));
  const nav=setId("ptc-nav", element("nav", "nav", null));
  const navBack=setTestId("nav-scroll-left", button("nav-scroll", "<"));
  const navForward=setTestId("nav-scroll-right", button("nav-scroll", ">"));
  const create=renderPageCreator(nav, activePath, pages);
  const registryHealth=setTestId("append-registry-health", element("div", "state registry-health", "append registry ws pending"));
  const scrollTabs=(delta) => {
    navViewport.scrollLeft=navViewport.scrollLeft+delta;
  };
  navBack.setAttribute("aria-label", "Scroll tabs left");
  navForward.setAttribute("aria-label", "Scroll tabs right");
  navBack.addEventListener("click", () => scrollTabs(-260));
  navForward.addEventListener("click", () => scrollTabs(260));
  append(brandCluster, [element("div", "brand", "PTC.Comm SPA"), registryHealth]);
  renderNav(nav, activePath, pages);
  const logout=setHref("/chat/logout", element("a", "logout", "Logout"));
  const page=element("main", "page", null);
  append(navViewport, [nav]);
  append(navShell, [navBack, navViewport, navForward]);
  append(topRow, [brandCluster, navShell, logout]);
  append(top, [topRow, create]);
  append(app, [top, page]);
  return[app, page];
}
export function actorArguButtonLabel(page){
  return isActorArguPage(page)?"Tell":"Append";
}
export function renderPageCreator(nav, activePath, pages){
  let candidatePageId, candidatesLoaded, replayingPendingPageRegistration;
  const wrap=setTestId("page-create", element("div", "page-create", null));
  const shape=setTestId("page-create-shape", select(appendPageShapeOptions()));
  const pageId=setTestId("page-create-id", input("page id"));
  const title=setTestId("page-create-title", input("title"));
  const binding=setTestId("page-create-binding", select([]));
  const add=setTestId("page-create-submit", button("", "Add"));
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
    appendOption("", "Default", binding);
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
          appendOption("", "Default", binding);
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
      const request=New_25(pageIdText, titleText, "", shape.value, p[0], p[1], "", "");
      const pendingId=rememberPending("append-page-register", textOr(titleText, pageIdText), "/pages/api/register-page", request);
      setStatus(status, "Saving");
      postJson("/pages/api/register-page", request, (reply) => {
        deletePendingThen(pendingId, () => {
          writeAppendPagesDefinitions(New_26(reply.status, length(arrayOrEmpty(reply.pages)), reply.maxSequence, reply.pages));
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
  if(!replayingPendingPageRegistration){
    replayingPendingPageRegistration=true;
    readAllPending((commands) => {
      let remaining, accepted;
      const mine=filter((command) =>!(command==null)&&sameText(command.kind, "append-page-register")&&sameText(command.method, "POST")&&!isBlank(command.url)&&!isBlank(command.payloadJson), commands);
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
                !(reply==null)?(writeAppendPagesDefinitions(New_26(reply.status, length(arrayOrEmpty(reply.pages)), reply.maxSequence, reply.pages)),refresh(reply.pages)):void 0;
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
export function renderNav(nav, activePath, pages){
  clear(nav);
  iter((_1) => {
    const href=_1[0];
    const label=_1[1];
    const x=setHref(href, element("a", isCurrentPage(activePath, href)?"nav-link active":"nav-link", label));
    let _2=setTestId("nav-"+label.toLowerCase(), x);
    nav.appendChild(_2);
  }, [["/chat", "Chat"], ["/sets", "Sets"], ["/actors", "Actors"]]);
  iter((page) => {
    const href=pagePath(page);
    const x=setHref(href, element("a", isCurrentPage(activePath, href)?"nav-link active":"nav-link", null));
    let _1=setTestId("nav-append-page-"+asText(page.pageId), x);
    let _2=setData("page-id", page.pageId, _1);
    const link=setData("shape", page.shape, _2);
    const badge=element("span", "nav-type-badge "+pageTypeClass(page), pageTypeBadge(page));
    badge.setAttribute("title", pageTypeLabel(page));
    badge.setAttribute("aria-label", pageTypeLabel(page));
    append(link, [badge, element("span", "nav-title", pageTitle(page))]);
    nav.appendChild(link);
  }, arrayOrEmpty(pages));
}
export function isCurrentPage(activePath, href){
  return TrimEnd(activePath, ["/"])==TrimEnd(href, ["/"]);
}
export function pageTypeClass(page){
  if(isActorArguPage(page))return asText(page.shape).toLowerCase()=="raw"?"raw":"actor-argu";
  else {
    const m=findAppendPageShape(page.shape);
    if(m==null)return"raw";
    else {
      const shape=m.$0;
      return textOr(normalizeShapeText(page.shape), shape.className);
    }
  }
}
export function pageTypeBadge(page){
  if(isActorArguPage(page))return asText(page.shape).toLowerCase()=="raw"?"R":"A";
  else {
    const m=findAppendPageShape(page.shape);
    return m==null?"R":textOr("?", m.$0.badge);
  }
}
export function pageTypeLabel(page){
  if(isActorArguPage(page))return asText(page.shape).toLowerCase()=="raw"?"Raw":"Actor Argu";
  else {
    const m=findAppendPageShape(page.shape);
    if(m==null)return"Raw";
    else {
      const shape=m.$0;
      return textOr(normalizeShapeText(page.shape), shape.label);
    }
  }
}
export function pageTitle(page){
  return textOr(asText(page.pageId), asText(page.title));
}
export function navigationPathForCreatedPage(page){
  const pageId=asText(page.pageId);
  const path=asText(page.path);
  return exists((alias) => sameTextInvariant(path, alias), ["/fcell-chat", "/fcell-list", "/fcell-grid"])?path:"/page/"+pageId;
}
export function pagePath(page){
  const pageId=asText(page.pageId);
  const path=asText(page.path);
  return exists((alias) => sameTextInvariant(path, alias), ["/fcell-chat", "/fcell-list", "/fcell-grid"])?path:"/page/"+pageId;
}
export function cardTitle(title, id, status, line){
  const wrap=doc().createDocumentFragment();
  const row=element("div", "name-row", null);
  append(row, [statusDot(status), element("span", "strong wrap", title)]);
  wrap.appendChild(row);
  if(!isBlank(id))wrap.appendChild(element("div", "muted wrap", id));
  if(!isBlank(line))wrap.appendChild(element("div", "meta wrap", line));
  return wrap;
}
export function postRemoveAppendPageKey(url, body, onOk, onError){
  const headers=new Headers();
  headers.set("Content-Type", "application/json");
  (globalThis.fetch(url, {
    method:"POST", 
    headers:headers, 
    body:JSON.stringify(body)
  }).then((response) => response.text().then((responseBody) => response.ok?onOk(json(isBlank(responseBody)?"{}":responseBody)):onError(isBlank(responseBody)?"POST "+String(url)+" "+String(response.status):responseBody))))["catch"]((error) => onError(errorMessage(error)));
}
export function postAppendPageKey(url, body, onOk, onError){
  const headers=new Headers();
  headers.set("Content-Type", "application/json");
  (globalThis.fetch(url, {
    method:"POST", 
    headers:headers, 
    body:JSON.stringify(body)
  }).then((response) => response.text().then((responseBody) => response.ok?onOk(json(isBlank(responseBody)?"{}":responseBody)):onError(isBlank(responseBody)?"POST "+String(url)+" "+String(response.status):responseBody))))["catch"]((error) => onError(errorMessage(error)));
}
export function postAppendPage(url, body, onOk, onError){
  const headers=new Headers();
  headers.set("Content-Type", "application/json");
  (globalThis.fetch(url, {
    method:"POST", 
    headers:headers, 
    body:JSON.stringify(body)
  }).then((response) => response.text().then((responseBody) => response.ok?onOk(json(isBlank(responseBody)?"{}":responseBody)):onError(isBlank(responseBody)?"POST "+String(url)+" "+String(response.status):responseBody))))["catch"]((error) => onError(errorMessage(error)));
}
export function postJsonText(url, payloadJson, onOk, onError){
  const headers=new Headers();
  headers.set("Content-Type", "application/json");
  (globalThis.fetch(url, {
    method:"POST", 
    headers:headers, 
    body:textOr("{}", payloadJson)
  }).then((response) => response.text().then((responseBody) => response.ok?onOk(responseBody):onError(isBlank(responseBody)?"POST "+String(url)+" "+String(response.status):responseBody))))["catch"]((error) => onError(errorMessage(error)));
}
export function postJson(url, body, onOk, onError){
  const headers=new Headers();
  headers.set("Content-Type", "application/json");
  (globalThis.fetch(url, {
    method:"POST", 
    headers:headers, 
    body:JSON.stringify(body)
  }).then((response) => response.text().then((responseBody) => response.ok?onOk(json(isBlank(responseBody)?"{}":responseBody)):onError(isBlank(responseBody)?"POST "+String(url)+" "+String(response.status):responseBody))))["catch"]((error) => onError(errorMessage(error)));
}
export function getJson(url, onOk, onError){
  (globalThis.fetch(url, {cache:"no-store"}).then((response) => response.text().then((body) => response.ok?onOk(json(isBlank(body)?"{}":body)):onError(isBlank(body)?"GET "+String(url)+" "+String(response.status):body))))["catch"]((error) => onError(errorMessage(error)));
}
export function newRequestId(prefix){
  set_requestSeq(requestSeq()+1);
  return prefix+"-"+String(requestSeq())+"-"+String(Math.floor(Math.random()*1000000000));
}
export function requestSeq(){
  return $StartupCode_Client.requestSeq;
}
export function set_requestSeq(_1){
  $StartupCode_Client.requestSeq=_1;
}
export function syncWebSocketUrl(){
  const location=globalThis.location;
  return(location.protocol=="https:"?"wss:":"ws:")+"//"+location.host+"/sync/ws";
}
export function currentUserId(){
  const userNode=doc().getElementById("ptc-comm-user");
  if(userNode==null||isBlank(userNode.textContent))return"user.web";
  else {
    const user=json(userNode.textContent);
    return user==null||isBlank(user.participantId)?"user.web":user.participantId;
  }
}
export function renderFCell2Rendered(rendered){
  let _1;
  const mode=asText(rendered.mode);
  const m=mode.toLowerCase();
  const className=m=="inbound-message"?"fcell-card fcell-chat inbound":m=="outbound-message"?"fcell-card fcell-chat outbound":m=="list"?"fcell-card fcell-list":m=="table"?"fcell-card fcell-grid":"fcell-card";
  const card=element("div", className, null);
  const head=element("div", "fcell-head", null);
  append(head, [element("span", "fcell-pill", fcellModeLabel(mode)), element("span", "muted wrap", mode)]);
  card.appendChild(head);
  const m_1=mode.toLowerCase();
  switch(m_1){
    case"outbound-message":
    case"inbound-message":
      const rows=arrayOrEmpty(rendered.rows);
      _1=iter((text) => {
        card.appendChild(renderTextBlock("fcell-message-body", text));
      }, length(rows)===0?[asText(rendered.source)]:rows);
      break;
    case"list":
      const list=element("ul", "fcell-list-items", null);
      _1=(iter((row) => {
        list.appendChild(element("li", "", asText(row)));
      }, arrayOrEmpty(rendered.rows)),void card.appendChild(list));
      break;
    case"table":
      const table=element("table", "fcell-grid-table", null);
      const tbody=element("tbody", "", null);
      _1=(iter((cells) => {
        const tr=element("tr", "", null);
        iter((cell) => {
          tr.appendChild(element("td", "wrap", asText(cell)));
        }, arrayOrEmpty(cells));
        tbody.appendChild(tr);
      }, arrayOrEmpty(rendered.tableRows)),append(table, [tbody]),void card.appendChild(table));
      break;
    default:
      _1=void card.appendChild(renderTextBlock("fcell-source", rendered.source));
      break;
  }
  if(!isBlank(rendered.source)&&mode.toLowerCase()!="inbound-message"&&mode.toLowerCase()!="outbound-message")card.appendChild(renderTextBlock("fcell-source", rendered.source));
  return card;
}
export function tryFCell2Rendered(text){
  if(isBlank(text))return null;
  else try {
    const rendered=json(text);
    return rendered==null||asText(rendered.schema)!="ptc.comm.fcell2.value.v1"?null:Some(rendered);
  }
  catch(m){
    return null;
  }
}
export function isActorArguPage(page){
  return hasTag("actor-argu", page.tags);
}
export function fcellValueModeLabel(mode, tags){
  return hasTag("actor-argu-command", tags)?"Actor Argu Outbound":hasTag("actor-argu-reply", tags)?"Actor Argu Reply":hasTag("actor-argu-error", tags)?"Actor Argu Error":fcellModeLabel(mode);
}
export function hasTag(tag, tags){
  return exists((value) => asText(value).toLowerCase()==tag, arrayOrEmpty(tags));
}
export function fcellModeLabel(mode){
  const m=asText(mode).toLowerCase();
  return m=="inbound-message"?"FCell Chat":m=="outbound-message"?"FCell Chat":m=="list"?"FCell List":m=="table"?"FCell Grid":m=="grid"?"FCell Grid":"FCell Value";
}
export function statusDot(status){
  const node=element("span", isLive(status)?"status-dot online":"status-dot offline", null);
  node.setAttribute("title", asText(status));
  return node;
}
export function isLive(status){
  const m=asText(status).toLowerCase();
  return m=="online"||(m=="running"||(m=="up"||m=="available"));
}
export function renderPendingInspection(node, commands, foreignCommands){
  let _1, shown, shown_1;
  const commands_1=arrayOrEmpty(commands);
  const foreignCommands_1=arrayOrEmpty(foreignCommands);
  node.setAttribute("data-pending-count", String(length(commands_1)));
  node.setAttribute("data-foreign-pending-count", String(length(foreignCommands_1)));
  node.setAttribute("data-foreign-pending-realities", concat(",", distinct(map((command) => asText(command.serverRealityId), foreignCommands_1))));
  node.setAttribute("data-pending-kinds", concat(",", map((a) => a.kind, commands_1)));
  node.setAttribute("data-pending-targets", concat("\n", map((a) => a.target, commands_1)));
  node.setAttribute("data-pending-urls", concat("\n", map((a) => a.url, commands_1)));
  node.setAttribute("data-pending-statuses", concat(",", map((a) => a.status, commands_1)));
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
export function pendingFailure(action, error){
  return String(action)+" failed; pending command kept in browser DB: "+String(asText(error));
}
export function rememberPending(kind, target, url, body){
  const payloadJson=JSON.stringify(body);
  const commandId=newPendingCommandId(kind, target, url, payloadJson);
  writePending(New_27(commandId, currentServerRealityId(), kind, target, url, "POST", payloadJson, "pending"));
  return commandId;
}
export function newPendingCommandId(kind, target, url, payloadJson){
  set_pendingCommandSeq(pendingCommandSeq()+1);
  return cacheKey("pending-command", ofArray([kind, target, url, payloadJson, "attempt-"+String(pendingCommandSeq()), "rand-"+String(Math.floor(Math.random()*1000000000))]));
}
export function pendingCommandSeq(){
  return $StartupCode_Client.pendingCommandSeq;
}
export function set_pendingCommandSeq(_1){
  $StartupCode_Client.pendingCommandSeq=_1;
}
export function mergeAppendPageRegistryEvents(baseline, events){
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
  }, arrayOrEmpty(events));
  return New_26("ok", length(pages), maxSequence, pages);
}
export function hiddenPageFromWire(wire){
  if(wire==null||asText(wire.schema)!="ptc.comm.spa.append-page.hidden.v1"||isBlank(wire.pageId))return null;
  else {
    const pageId=asText(wire.pageId);
    return Some([pageId, textOr(pageId, wire.tabId)]);
  }
}
export function pageDefinitionFromWire(wire){
  if(wire==null||asText(wire.schema)!="ptc.comm.spa.append-page.definition.v1"||isBlank(wire.pageId))return null;
  else {
    const pageId=asText(wire.pageId);
    return Some(New_28(pageId, textOr(pageId, wire.tabId), textOr("/page/"+pageId, wire.path), textOr(pageId, wire.title), textOr(pageId, wire.setName), textOr("raw", wire.shape), asText(wire.description), textOr("\"Aster\"", wire.keyPlaceholder), textOr("JSON value", wire.valuePlaceholder), asText(wire.defaultKey), arrayOrEmpty(wire.tags)));
  }
}
export function sortAppendPages(pages){
  return sortBy((page) =>[asText(page.title).toLowerCase(), asText(page.pageId).toLowerCase()], arrayOrEmpty(pages));
}
export function sameTextInvariant(left, right){
  return asText(left).toLowerCase()==asText(right).toLowerCase();
}
export function appendPageRegistryStreamKey(){
  return New_3("__append-page-registry", "append-page-registry", "__append-pages", ["__append-pages"]);
}
export function emptyAppendPagesReply(){
  return New_26("ok", 0, 0n, []);
}
export function writeAppendPagesDefinitions(data){
  writeSnapshotWithWatermark(appendPagesDefinitionsCacheKey(), data, data.maxSequence, length(arrayOrEmpty(data.pages)), "append-pages-definitions");
}
export function appendPagesDefinitionsCacheKey(){
  return cacheKey("append-pages-definitions", FSharpList.Empty);
}
export function writeSnapshotWithWatermark(cacheKey_1, value, newestSequence, cachedCount, source){
  writeJson(cacheKey_1, value);
  writeWatermark(cacheKey_1, newestSequence, cachedCount, source);
}
export function appendPageValueCount(snapshot){
  return snapshot==null?0:fold((_1, _2) => _1+_2, 0, map((bucket) => {
    if(bucket==null)return 0;
    else {
      const a=bucket.valueCount;
      const b=length(arrayOrEmpty(bucket.values));
      return Compare(a, b)===1?a:b;
    }
  }, arrayOrEmpty(snapshot.buckets)));
}
export function actorValueCount(data){
  if(data==null)return 0;
  else {
    const a=data.actorCount;
    const b=data.nodeCount;
    return Compare(a, b)===1?a:b;
  }
}
export function setValueCount(buckets){
  return fold((_1, _2) => _1+_2, 0, map((bucket) => bucket==null?0:bucket.valueCount, arrayOrEmpty(buckets)));
}
export function maxMessageSequence(messages){
  return fold((_1, _2) => Compare(_1, _2)===1?_1:_2, 0n, map((message) => message==null?0n:tryParseSequence("msg-", message.messageId), arrayOrEmpty(messages)));
}
export function tryParseSequence(prefix, value){
  const text=asText(value);
  if(isBlank(text)||!StartsWith(text, prefix))return 0n;
  else try {
    return BigInt(text.substring(prefix.length));
  }
  catch(m){
    return 0n;
  }
}
export function currentServerRealityId(){
  const node=doc().getElementById("ptc-comm-reality");
  if(node==null||isBlank(node.textContent))return"legacy";
  else try {
    return textOr("legacy", json(node.textContent).serverRealityId);
  }
  catch(m){
    return"legacy";
  }
}
export function renderTextBlock(className, text){
  const m=tryRenderWithRegisteredRenderers(text);
  return m==null?element("pre", className, asText(text)):m.$0;
}
export function tryRenderWithRegisteredRenderers(text){
  let r;
  const content=asText(text);
  if(isBlank(content))return null;
  else {
    const _1=content;
    if(globalThis.PulseTrade&&globalThis.PulseTrade.Renderers){
      let renderers=globalThis.PulseTrade.Renderers;
      for(let i=0;i<renderers.length;i++){
        let r_1=renderers[i];
        try {
          let nodeOpt=r_1[1](_1);
          if(nodeOpt!=null&&nodeOpt.$==1)return nodeOpt;
        }
        catch(e){
          console.error("Renderer exception:", e);
        }
      }
    }
    return null;
  }
}
export function _initGlobally(){
  return $StartupCode_Client._initGlobally;
}
export function _registerRendererGlobally(){
  if(!globalThis.PulseTrade)globalThis.PulseTrade={};
  if(!globalThis.PulseTrade.Renderers)globalThis.PulseTrade.Renderers=[];
  globalThis.PulseTradeRegisterRenderer=(name, func) => {
    console.log("PulseTradeRegisterRenderer called!", name);
    globalThis.PulseTrade.Renderers.push([name, func]);
  };
}
export function RegisterAppendPageShape(shape, label, badge, className){
  const registration=shapeRegistration(shape, label, badge, className);
  if(registration.shape!="raw")set_runtimeAppendPageShapes(distinctBy((item) => normalizeShapeText(item.shape), runtimeAppendPageShapes().concat([registration])));
}
export function findAppendPageShape(shape){
  const normalized=normalizeShapeText(shape);
  return tryFind((candidate) => normalizeShapeText(candidate.shape)==normalized, appendPageShapeRegistry());
}
export function appendPageShapeOptions(){
  return map((shape) =>[normalizeShapeText(shape.shape), textOr(normalizeShapeText(shape.shape), shape.label)], appendPageShapeRegistry());
}
export function appendPageShapeRegistry(){
  return distinctBy((shape) => normalizeShapeText(shape.shape), concat_1([builtInAppendPageShapes(), manifestAppendPageShapes(), runtimeAppendPageShapes()]));
}
export function manifestAppendPageShapes(){
  return filter((shape) => shape.shape!="raw", map((shape) => shape==null?shapeRegistration("raw", "Raw", "R", "raw"):shapeRegistration(shape.shape, shape.label, shape.badge, shape.className), collect((extension) => extension==null?[]:arrayOrEmpty(extension.appendPageShapes), serverClientExtensions())));
}
export function serverClientExtensions(){
  const node=doc().getElementById("ptc-comm-client-extensions");
  if(node==null||isBlank(node.textContent))return[];
  else {
    const o=tryJson(node.textContent);
    return o==null?[]:o.$0;
  }
}
export function builtInAppendPageShapes(){
  return[shapeRegistration("fcell-chat", "FCell Chat", "C", "fcell-chat"), shapeRegistration("fcell-list", "FCell List", "L", "fcell-list"), shapeRegistration("fcell-grid", "FCell Grid", "G", "fcell-grid"), shapeRegistration("actor-argu", "Actor Argu", "A", "actor-argu"), shapeRegistration("raw", "Raw", "R", "raw")];
}
export function shapeRegistration(shape, label, badge, className){
  return New_29(normalizeShapeText(shape), textOr(normalizeShapeText(shape), label), textOr("?", badge), textOr(normalizeShapeText(shape), className));
}
export function normalizeShapeText(value){
  const text=Trim(asText(value)).toLowerCase();
  return text.length>0&&text.length<=64&&forall((ch) => ch>="a"&&ch<="z"||ch>="0"&&ch<="9"||ch==="-"||ch==="_"||ch===".", text)?text:"raw";
}
export function runtimeAppendPageShapes(){
  return $StartupCode_Client.runtimeAppendPageShapes;
}
export function set_runtimeAppendPageShapes(_1){
  $StartupCode_Client.runtimeAppendPageShapes=_1;
}
export function registeredRenderers(){
  return $StartupCode_Client.registeredRenderers;
}
export function set_registeredRenderers(_1){
  $StartupCode_Client.registeredRenderers=_1;
}
export function errorMessage(error){
  return error==null?"request failed":String(error);
}
export function setStatus(node, text){
  node.textContent=text;
}
export function setData(name, value, node){
  !isBlank(name)?node.setAttribute("data-"+name, asText(value)):void 0;
  return node;
}
export function setTestId(id, node){
  !isBlank(id)?node.setAttribute("data-testid", id):void 0;
  return node;
}
export function setId(id, node){
  node.setAttribute("id", id);
  return node;
}
export function setHref(href, node){
  node.setAttribute("href", href);
  return node;
}
export function textarea(className, placeholder){
  const node=doc().createElement("textarea");
  node.className=className;
  node.placeholder=placeholder;
  return node;
}
export function select(options){
  const node=doc().createElement("select");
  iter((_1) => {
    const option=doc().createElement("option");
    option.setAttribute("value", _1[0]);
    option.textContent=_1[1];
    node.appendChild(option);
  }, options);
  return node;
}
export function input(placeholder){
  const node=doc().createElement("input");
  node.placeholder=placeholder;
  return node;
}
export function button(className, text){
  const node=element("button", className, text);
  node.setAttribute("type", "button");
  return node;
}
export function clear(node){
  node.textContent="";
}
export function append(parent, children){
  for(let i=0, _1=children.length-1;i<=_1;i++)parent.appendChild(get(children, i));
  return parent;
}
export function element(tag, className, textValue){
  const node=doc().createElement(tag);
  if(!isBlank(className))node.className=className;
  if(!(textValue==null))node.textContent=textValue;
  return node;
}
export function int64OrZero(value){
  const parsed=parseInt(asText(value), globalThis.$radix);
  return isNaN(parsed)||parsed<0?0n:BigInt(parsed);
}
export function compactMessageId(value){
  const text=asText(value);
  return text.length<=32?text:StartsWith(text.toLowerCase(), "pending-command")?"pending-command:"+String(text.length):Substring(text, 0, 24)+"..."+text.substring(text.length-6);
}
export function textOr(fallback, value){
  return isBlank(value)?fallback:value;
}
export function joinValues(values){
  const values_1=arrayOrEmpty(values);
  return length(values_1)===0?"":concat(" / ", values_1);
}
export function latestArray(limit, values){
  const values_1=arrayOrEmpty(values);
  return length(values_1)<=limit?values_1:skip(length(values_1)-limit, values_1);
}
export function arrayOrEmpty(values){
  return values==null?[]:values;
}
export function isBlank(value){
  return value==null||Trim(value)=="";
}
export function asText(value){
  return value==null||Equals(typeof value, "undefined")?"":value;
}
export function defaultCacheLimit(){
  return $StartupCode_Client.defaultCacheLimit;
}
export function defaultRenderLimit(){
  return $StartupCode_Client.defaultRenderLimit;
}
export function doc(){
  return $StartupCode_Client.doc;
}
