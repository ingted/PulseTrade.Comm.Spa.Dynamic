import SyncStreamKeyDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.SyncStreamKeyDto"
export function New(type, requestId, streamKey)
export default interface SyncSubscribeRequestDto {
  type:string;
  requestId:string;
  streamKey:SyncStreamKeyDto;
}
