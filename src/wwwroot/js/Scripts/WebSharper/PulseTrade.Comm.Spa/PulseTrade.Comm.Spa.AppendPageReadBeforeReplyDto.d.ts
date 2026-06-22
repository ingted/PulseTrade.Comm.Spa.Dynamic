import AppendPageDefinitionDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageDefinitionDto"
import AppendPageLineageDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageLineageDto"
import AppendPageReadRepairHealthDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageReadRepairHealthDto"
import AppendPageValueViewDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageValueViewDto"
export function New(status, page, keyId, beforeSequence, count, lineage, lineageHealth, values)
export default interface AppendPageReadBeforeReplyDto {
  status:string;
  page:AppendPageDefinitionDto;
  keyId:string;
  beforeSequence:bigint;
  count:number;
  lineage:AppendPageLineageDto;
  lineageHealth:AppendPageReadRepairHealthDto;
  values:(AppendPageValueViewDto)[];
}
