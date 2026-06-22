import ActorNodeDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.ActorNodeDto"
export function New(nodeCount, actorCount, maxSequence, nodes)
export default interface ActorsReplyDto {
  nodeCount:number;
  actorCount:number;
  maxSequence:bigint;
  nodes:(ActorNodeDto)[];
}
