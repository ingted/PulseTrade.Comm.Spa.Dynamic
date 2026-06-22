import { _registerRendererGlobally } from "./PulseTrade.Comm.Spa.Client.js"
import { Lazy } from "./Runtime.js"
let _c=Lazy((_i) => class $StartupCode_Client {
  static {
    _c=_i(this);
  }
  static requestSeq;
  static pendingCommandSeq;
  static maxSnapshotRecords;
  static watermarkStore;
  static pendingStore;
  static snapshotStore;
  static databaseVersion;
  static databaseName;
  static _initGlobally;
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
    this._initGlobally=(_registerRendererGlobally(),0);
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
export default _c;
