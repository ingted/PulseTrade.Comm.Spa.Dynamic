import SetValueDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.SetValueDto"
export function New(keyId, setName, keys, valueCount, maxSequence, updatedAtUtc, values)
export default interface SetBucketDto {
  keyId:string;
  setName:string;
  keys:string[];
  valueCount:number;
  maxSequence:bigint;
  updatedAtUtc:string;
  values:(SetValueDto)[];
}
