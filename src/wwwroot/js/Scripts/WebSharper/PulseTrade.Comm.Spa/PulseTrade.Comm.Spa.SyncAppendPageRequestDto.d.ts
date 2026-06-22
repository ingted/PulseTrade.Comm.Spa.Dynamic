import SyncStreamKeyDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.SyncStreamKeyDto"
export function New(type, requestId, pageId, title, setName, streamKey, keyJson, valueText, direction, renderMode, idempotencyKey, tags, browserId, tabId)
export default interface SyncAppendPageRequestDto {
  type:string;
  requestId:string;
  pageId:string;
  title:string;
  setName:string;
  streamKey:SyncStreamKeyDto;
  keyJson:string;
  valueText:string;
  direction:string;
  renderMode:string;
  idempotencyKey:string;
  tags:string[];
  browserId:string;
  tabId:string;
}
