import ClientAppendPageShapeRegistrationDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.ClientAppendPageShapeRegistrationDto"
import { FSharpOption } from "../../Content/WebSharper/WebSharper.StdLib/Microsoft.FSharp.Core.FSharpOption`1"
export default class $StartupCode_Client {
  static requestSeq:number;
  static pendingCommandSeq:number;
  static maxSnapshotRecords:number;
  static watermarkStore:string;
  static pendingStore:string;
  static snapshotStore:string;
  static databaseVersion:number;
  static databaseName:string;
  static _initGlobally:number;
  static runtimeAppendPageShapes:(ClientAppendPageShapeRegistrationDto)[];
  static registeredRenderers:([string, ((a:string) => FSharpOption<Node>)])[];
  static defaultCacheLimit:number;
  static defaultRenderLimit:number;
  static doc:Document;
}
