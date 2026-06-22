import SyncStreamKeyDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.SyncStreamKeyDto"
export function New(type, requestId, pageId, title, setName, streamKey, actorAddress, rawArgu, renderMode, tags, browserId, tabId)
export default interface SyncActorArguRequestDto {
  type:string;
  requestId:string;
  pageId:string;
  title:string;
  setName:string;
  streamKey:SyncStreamKeyDto;
  actorAddress:string;
  rawArgu:string;
  renderMode:string;
  tags:string[];
  browserId:string;
  tabId:string;
}
