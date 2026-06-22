import MessageDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.MessageDto"
import SyncStreamEventDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.SyncStreamEventDto"
export function New(type, requestId, status, message, deliveryHint, event, error)
export default interface SyncChatSendReplyDto {
  type:string;
  requestId:string;
  status:string;
  message:MessageDto;
  deliveryHint:string;
  event:SyncStreamEventDto;
  error:string;
}
