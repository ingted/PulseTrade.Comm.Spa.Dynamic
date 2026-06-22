import AppendPageDefinitionDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageDefinitionDto"
export function New(status, page, maxSequence, pages)
export default interface RegisterAppendPageReplyDto {
  status:string;
  page:AppendPageDefinitionDto;
  maxSequence:bigint;
  pages:(AppendPageDefinitionDto)[];
}
