import AppendPageDefinitionDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageDefinitionDto"
export function New(status, count, maxSequence, pages)
export default interface AppendPagesReplyDto {
  status:string;
  count:number;
  maxSequence:bigint;
  pages:(AppendPageDefinitionDto)[];
}
