import ParticipantDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.ParticipantDto"
export function New(status, count, participants)
export default interface AgentsReplyDto {
  status:string;
  count:number;
  participants:(ParticipantDto)[];
}
