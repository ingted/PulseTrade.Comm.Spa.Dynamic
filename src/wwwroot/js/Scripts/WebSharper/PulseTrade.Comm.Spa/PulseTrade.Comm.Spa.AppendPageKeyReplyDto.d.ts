import AppendPageDefinitionDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageDefinitionDto"
import AppendPageKeyDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageKeyDto"
export function New(status, page, key)
export default interface AppendPageKeyReplyDto {
  status:string;
  page:AppendPageDefinitionDto;
  key:AppendPageKeyDto;
}
