import ActorDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.ActorDto"
export function New(nodeId, nodeAddress, status, roles, actors)
export default interface ActorNodeDto {
  nodeId:string;
  nodeAddress:string;
  status:string;
  roles:string[];
  actors:(ActorDto)[];
}
