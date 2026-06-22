import AppendPageTabCandidateDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageTabCandidateDto"
export function New(status, pageId, count, candidates)
export default interface AppendPageTabCandidatesReplyDto {
  status:string;
  pageId:string;
  count:number;
  candidates:(AppendPageTabCandidateDto)[];
}
