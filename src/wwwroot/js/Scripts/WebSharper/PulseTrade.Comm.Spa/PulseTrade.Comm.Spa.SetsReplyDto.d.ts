import SetBucketDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.SetBucketDto"
export function New(maxSequence, buckets)
export default interface SetsReplyDto {
  maxSequence:bigint;
  buckets:(SetBucketDto)[];
}
