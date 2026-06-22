import { _registerRenderer } from "./PulseTrade.Comm.Spa.Dynamic.Client.ActorDynamicTab.js"
import { Lazy } from "../WebSharper.Core.JavaScript/Runtime.js"
let _c=Lazy((_i) => class $StartupCode_ActorDynamicTab {
  static {
    _c=_i(this);
  }
  static {
    globalThis.console.log("EVALUATING module-level do binding in ActorDynamicTab!");
    _registerRenderer();
  }
});
export default _c;
