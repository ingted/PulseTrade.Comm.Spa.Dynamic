import RouteeDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.RouteeDto"
export function New(actorId, displayName, kind, keys, status, routees)
export default interface ActorDto {
  actorId:string;
  displayName:string;
  kind:string;
  keys:string[];
  status:string;
  routees:(RouteeDto)[];
}
