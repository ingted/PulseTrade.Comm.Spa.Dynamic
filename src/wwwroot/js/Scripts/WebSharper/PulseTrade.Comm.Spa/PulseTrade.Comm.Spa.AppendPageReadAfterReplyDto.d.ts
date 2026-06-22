import AppendPageDefinitionDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageDefinitionDto"
import AppendPageLineageDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageLineageDto"
import AppendPageReadRepairHealthDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageReadRepairHealthDto"
import AppendPageValueViewDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageValueViewDto"
export function New(status, page, keyId, afterSequence, count, lineage, lineageHealth, values)
export default interface AppendPageReadAfterReplyDto {
  status:string;
  page:AppendPageDefinitionDto;
  keyId:string;
  afterSequence:bigint;
  count:number;
  lineage:AppendPageLineageDto;
  lineageHealth:AppendPageReadRepairHealthDto;
  values:(AppendPageValueViewDto)[];
}
