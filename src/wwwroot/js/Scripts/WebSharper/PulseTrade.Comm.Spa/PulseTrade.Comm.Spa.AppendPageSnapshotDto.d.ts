import AppendPageDefinitionDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageDefinitionDto"
import AppendPageLineageDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageLineageDto"
import AppendPageReadRepairHealthDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageReadRepairHealthDto"
import AppendPageBucketViewDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageBucketViewDto"
export function New(status, page, bucketCount, maxSequence, keyMaxSequence, lineage, lineageHealth, buckets)
export default interface AppendPageSnapshotDto {
  status:string;
  page:AppendPageDefinitionDto;
  bucketCount:number;
  maxSequence:bigint;
  keyMaxSequence:bigint;
  lineage:AppendPageLineageDto;
  lineageHealth:AppendPageReadRepairHealthDto;
  buckets:(AppendPageBucketViewDto)[];
}
