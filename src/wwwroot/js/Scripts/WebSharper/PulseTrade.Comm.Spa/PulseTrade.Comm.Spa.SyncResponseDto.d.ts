import SyncStreamEventDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.SyncStreamEventDto"
export function New(type, requestId, status, streamKey, event, events, error)
export default interface SyncResponseDto {
  type:string;
  requestId:string;
  status:string;
  streamKey:string;
  event:SyncStreamEventDto;
  events:(SyncStreamEventDto)[];
  error:string;
}
