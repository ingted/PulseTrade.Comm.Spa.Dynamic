import SyncStreamKeyDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.SyncStreamKeyDto"
export function New(type, requestId, streamKey, count)
export default interface SyncReadTailRequestDto {
  type:string;
  requestId:string;
  streamKey:SyncStreamKeyDto;
  count:number;
}
