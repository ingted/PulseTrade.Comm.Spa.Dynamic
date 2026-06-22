import AppendPageValueViewDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageValueViewDto"
export function New(keyId, keys, setName, valueCount, minSequence, maxSequence, updatedAtUtc, values)
export default interface AppendPageBucketViewDto {
  keyId:string;
  keys:string[];
  setName:string;
  valueCount:number;
  minSequence:bigint;
  maxSequence:bigint;
  updatedAtUtc:string;
  values:(AppendPageValueViewDto)[];
}
