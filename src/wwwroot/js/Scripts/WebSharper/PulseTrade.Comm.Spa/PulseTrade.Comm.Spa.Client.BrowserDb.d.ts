import { FSharpList_T } from "../../Content/WebSharper/WebSharper.StdLib/Microsoft.FSharp.Collections.FSharpList`1"
import BrowserWatermarkDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.BrowserWatermarkDto"
import { FSharpOption } from "../../Content/WebSharper/WebSharper.StdLib/Microsoft.FSharp.Core.FSharpOption`1"
import PendingCommandDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.PendingCommandDto"
export function cacheKey(scope:string, parts:FSharpList_T<string>):string
export function readWatermark(key:string, onRead:((a:FSharpOption<BrowserWatermarkDto>) => void)):void
export function readJson<T0>(key:string, onRead:((a:FSharpOption<T0>) => void)):void
export function readPendingRealitySplit(onRead:((a:(PendingCommandDto)[], b:(PendingCommandDto)[]) => void)):void
export function readAllPending(onRead:((a:(PendingCommandDto)[]) => void)):void
export function readAllPendingRaw(onRead:((a:(PendingCommandDto)[]) => void)):void
export function deletePendingThen(commandId:string, onDeleted:(() => void)):void
export function deletePending(commandId:string):void
export function writePending(command:PendingCommandDto):void
export function writeWatermark(streamId:string, newestSequence:bigint, cachedCount:number, source:string):void
export function writeJson<T0>(key:string, value:T0):void
export function writeJsonTo<T0, T1>(storeName:T0, key:string, value:T1):void
export function compactSnapshots():void
export function readAllSnapshotKeys(onRead:((a:string[]) => void)):void
export function readAllWatermarks(onRead:((a:(BrowserWatermarkDto)[]) => void)):void
export function deleteSnapshotAndWatermark(key:string):void
export function deleteFromThen<T0>(storeName:T0, key:string, onDeleted:(() => void)):void
export function deleteFrom<T0>(storeName:T0, key:string):void
export function protectedSnapshotKey(key:string):boolean
export function watermarkTouchedAt(watermark:BrowserWatermarkDto):bigint
export function nowTicks():string
export function withSnapshotWatermarkStores<T0>(mode:T0, onStores:((a:any, b:any, c:any) => void), onUnavailable:(() => void)):void
export function withTransactionStore<T0, T1>(storeName:T0, mode:T1, onStore:((a:any, b:any) => void), onUnavailable:(() => void)):void
export function withStore<T0, T1>(storeName:T0, mode:T1, onStore:((a:any) => void), onUnavailable:(() => void)):void
export function openDb(onReady:((a:any) => void), onUnavailable:(() => void)):void
export function ensureStores(db):void
export function ensureStore<T0>(storeName:T0, db):void
export function eventResult(event)
export function isMissing(value):boolean
export function maxSnapshotRecords():number
export function watermarkStore():string
export function pendingStore():string
export function snapshotStore():string
export function databaseVersion():number
export function databaseName():string
