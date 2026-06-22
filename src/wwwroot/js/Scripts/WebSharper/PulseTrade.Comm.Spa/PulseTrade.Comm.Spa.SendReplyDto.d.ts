import MessageDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.MessageDto"
export function New(status, message, deliveryHint, streamSequence)
export default interface SendReplyDto {
  status:string;
  message:MessageDto;
  deliveryHint:string;
  streamSequence:string;
}
