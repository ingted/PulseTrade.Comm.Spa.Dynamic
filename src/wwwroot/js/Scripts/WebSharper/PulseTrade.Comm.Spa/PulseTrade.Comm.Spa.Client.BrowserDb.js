import { currentServerRealityId, asText, isBlank, textOr, arrayOrEmpty } from "./PulseTrade.Comm.Spa.Client.js"
import { concat, StartsWith } from "./Microsoft.FSharp.Core.StringModule.js"
import { map } from "./Microsoft.FSharp.Collections.ListModule.js"
import { tryJson } from "./PulseTrade.Comm.Spa.Client.Decode.js"
import { filter, choose, iter, sortBy, exists } from "./Microsoft.FSharp.Collections.ArrayModule.js"
import { Compare, Equals } from "./Microsoft.FSharp.Core.Operators.Unchecked.js"
import { New } from "./PulseTrade.Comm.Spa.BrowserWatermarkDto.js"
import { length } from "./Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicFunctions.js"
import { TryParse } from "./System.Int64.js"
import $StartupCode_Client from "./$StartupCode_Client.js"
export function cacheKey(scope, parts){
  return currentServerRealityId()+":"+scope+":"+concat(":", map((part) => encodeURIComponent(asText(part)), parts));
}
export function readWatermark(key, onRead){
  if(isBlank(key))onRead(null);
  else withStore(watermarkStore(), "readonly", (store) => {
    try {
      const request=store.get(key);
      request.onsuccess=(event) => {
        const value=eventResult(event);
        if(isMissing(value))return onRead(null);
        else try {
          const text=String(value);
          return isBlank(text)?onRead(null):onRead(tryJson(text));
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
export function readJson(key, onRead){
  if(isBlank(key))onRead(null);
  else withStore(snapshotStore(), "readonly", (store) => {
    try {
      const request=store.get(key);
      request.onsuccess=(event) => {
        const value=eventResult(event);
        if(isMissing(value))return onRead(null);
        else try {
          const text=String(value);
          return isBlank(text)?onRead(null):onRead(tryJson(text));
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
export function readPendingRealitySplit(onRead){
  readAllPendingRaw((commands) => {
    const reality=currentServerRealityId();
    onRead(filter((command) =>!(command==null)&&textOr("legacy", command.serverRealityId)==reality, commands), filter((command) =>!(command==null)&&textOr("legacy", command.serverRealityId)!=reality, commands));
  });
}
export function readAllPending(onRead){
  readAllPendingRaw((commands) => {
    const reality=currentServerRealityId();
    onRead(filter((command) =>!(command==null)&&textOr("legacy", command.serverRealityId)==reality, commands));
  });
}
export function readAllPendingRaw(onRead){
  withStore(pendingStore(), "readonly", (store) => {
    try {
      const request=store.getAll();
      request.onsuccess=(event) => {
        const value=eventResult(event);
        if(isMissing(value))return onRead([]);
        else try {
          return onRead(choose((text) => {
            try {
              return isBlank(text)?null:tryJson(text);
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
export function deletePendingThen(commandId, onDeleted){
  deleteFromThen(pendingStore(), commandId, onDeleted);
}
export function deletePending(commandId){
  deleteFrom(pendingStore(), commandId);
}
export function writePending(command){
  writeJsonTo(pendingStore(), command.commandId, command);
}
export function writeWatermark(streamId, newestSequence, cachedCount, source){
  if(!isBlank(streamId)){
    let _1=watermarkStore();
    const a=0n;
    let _2=Compare(a, newestSequence)===1?a:newestSequence;
    let _3=String(_2);
    const a_1=0;
    let _4=Compare(a_1, cachedCount)===1?a_1:cachedCount;
    let _5=New(streamId, _3, _4, asText(source), nowTicks());
    writeJsonTo(_1, streamId, _5);
    compactSnapshots();
  }
}
export function writeJson(key, value){
  writeJsonTo(snapshotStore(), key, value);
}
export function writeJsonTo(storeName, key, value){
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
export function compactSnapshots(){
  readAllWatermarks((watermarks) => {
    const watermarks_1=arrayOrEmpty(watermarks);
    const overflow=length(watermarks_1)-maxSnapshotRecords();
    if(overflow>0)iter((watermark) => {
      deleteSnapshotAndWatermark(watermark.streamId);
    }, sortBy(watermarkTouchedAt, filter((watermark) =>!(watermark==null)&&!isBlank(watermark.streamId)&&!protectedSnapshotKey(watermark.streamId), watermarks_1)).slice(0, overflow));
    readAllSnapshotKeys((snapshotKeys) => {
      iter((key) => {
        deleteFrom(snapshotStore(), key);
      }, filter((key) =>!isBlank(key)&&!protectedSnapshotKey(key)&&!exists((watermark) =>!(watermark==null)&&watermark.streamId==key, watermarks_1), snapshotKeys));
    });
  });
}
export function readAllSnapshotKeys(onRead){
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
export function readAllWatermarks(onRead){
  withStore(watermarkStore(), "readonly", (store) => {
    try {
      const request=store.getAll();
      request.onsuccess=(event) => {
        const value=eventResult(event);
        if(isMissing(value))return onRead([]);
        else try {
          return onRead(choose((text) => {
            try {
              return isBlank(text)?null:tryJson(text);
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
export function deleteSnapshotAndWatermark(key){
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
export function deleteFromThen(storeName, key, onDeleted){
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
export function deleteFrom(storeName, key){
  if(!isBlank(key))withStore(storeName, "readwrite", (store) => {
    try {
      store["delete"](key);
    }
    catch(m){
      null;
    }
  }, () => { });
}
export function protectedSnapshotKey(key){
  const key_1=asText(key);
  return key_1=="append-pages-definitions:"||key_1.indexOf(":append-pages-definitions:")!=-1||StartsWith(key_1, "chat-agents:")||key_1.indexOf(":chat-agents:")!=-1||StartsWith(key_1, "actors-snapshot:")||key_1.indexOf(":actors-snapshot:")!=-1;
}
export function watermarkTouchedAt(watermark){
  let o;
  if(watermark==null)return 0n;
  else {
    const m=(o=0n,[TryParse(asText(watermark.touchedAt), {get:() => o, set:(v) => {
      o=v;
    }}), o]);
    return m[0]?m[1]:0n;
  }
}
export function nowTicks(){
  try {
    const this_1=Date.now();
    let _1=BigInt(Math.trunc(this_1))*BigInt(1E4)+BigInt((this_1-Math.trunc(this_1))*1E4);
    return String(_1);
  }
  catch(m){
    return"0";
  }
}
export function withSnapshotWatermarkStores(mode, onStores, onUnavailable){
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
export function withTransactionStore(storeName, mode, onStore, onUnavailable){
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
export function withStore(storeName, mode, onStore, onUnavailable){
  openDb((db) => {
    try {
      onStore(db.transaction([storeName], mode).objectStore(storeName));
    }
    catch(m){
      onUnavailable();
    }
  }, onUnavailable);
}
export function openDb(onReady, onUnavailable){
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
export function ensureStores(db){
  ensureStore(snapshotStore(), db);
  ensureStore(pendingStore(), db);
  ensureStore(watermarkStore(), db);
}
export function ensureStore(storeName, db){
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
export function eventResult(event){
  const target=event.target;
  return isMissing(target)?null:target.result;
}
export function isMissing(value){
  return value==null||Equals(typeof value, "undefined");
}
export function maxSnapshotRecords(){
  return $StartupCode_Client.maxSnapshotRecords;
}
export function watermarkStore(){
  return $StartupCode_Client.watermarkStore;
}
export function pendingStore(){
  return $StartupCode_Client.pendingStore;
}
export function snapshotStore(){
  return $StartupCode_Client.snapshotStore;
}
export function databaseVersion(){
  return $StartupCode_Client.databaseVersion;
}
export function databaseName(){
  return $StartupCode_Client.databaseName;
}
