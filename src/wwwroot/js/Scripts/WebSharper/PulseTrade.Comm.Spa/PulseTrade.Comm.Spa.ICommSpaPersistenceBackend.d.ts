import { FSharpList_T } from "../../Content/WebSharper/WebSharper.StdLib/Microsoft.FSharp.Collections.FSharpList`1"
export function isICommSpaPersistenceBackend(x):x is ICommSpaPersistenceBackend
export default interface ICommSpaPersistenceBackend {
  PulseTrade_Comm_Spa_ICommSpaPersistenceBackend$Append(a)
  PulseTrade_Comm_Spa_ICommSpaPersistenceBackend$ReadAfter(a, b:bigint, c:number):FSharpList_T<any>
  PulseTrade_Comm_Spa_ICommSpaPersistenceBackend$ReadBefore(a, b:bigint, c:number):FSharpList_T<any>
  PulseTrade_Comm_Spa_ICommSpaPersistenceBackend$ReadTail(a, b:number):FSharpList_T<any>
  PulseTrade_Comm_Spa_ICommSpaPersistenceBackend$Snapshot(a)
}
