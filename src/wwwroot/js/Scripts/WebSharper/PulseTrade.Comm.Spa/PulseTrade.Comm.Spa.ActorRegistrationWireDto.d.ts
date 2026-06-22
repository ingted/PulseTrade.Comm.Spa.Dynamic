import RouteeDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.RouteeDto"
export function New(schema, nodeId, nodeAddress, actorId, displayName, kind, status, roles, routees, tags)
export default interface ActorRegistrationWireDto {
  schema:string;
  nodeId:string;
  nodeAddress:string;
  actorId:string;
  displayName:string;
  kind:string;
  status:string;
  roles:string[];
  routees:(RouteeDto)[];
  tags:string[];
}
