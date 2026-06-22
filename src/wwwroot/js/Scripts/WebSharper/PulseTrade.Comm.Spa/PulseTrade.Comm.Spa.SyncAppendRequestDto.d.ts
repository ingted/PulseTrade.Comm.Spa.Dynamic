import SyncStreamKeyDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.SyncStreamKeyDto"
export function New(type, requestId, streamKey, payload, sourceKind, renderMode, idempotencyKey, tags, browserId, tabId)
export default interface SyncAppendRequestDto {
  type:string;
  requestId:string;
  streamKey:SyncStreamKeyDto;
  payload:string;
  sourceKind:string;
  renderMode:string;
  idempotencyKey:string;
  tags:string[];
  browserId:string;
  tabId:string;
}
