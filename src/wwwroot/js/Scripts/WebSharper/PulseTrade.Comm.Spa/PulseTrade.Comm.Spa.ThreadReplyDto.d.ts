import MessageDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.MessageDto"
export function New(messages, nextAfterMessageId)
export default interface ThreadReplyDto {
  messages:(MessageDto)[];
  nextAfterMessageId:string;
}
