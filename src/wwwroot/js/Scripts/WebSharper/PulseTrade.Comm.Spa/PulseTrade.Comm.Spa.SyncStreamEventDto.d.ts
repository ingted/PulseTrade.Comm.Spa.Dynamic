import SyncStreamKeyDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.SyncStreamKeyDto"
export function New(eventId, streamKeyText, streamKey, sequence, sourceId, sourceKind, payload, renderMode, tags, createdAtUtc, acceptedAtUtc)
export default interface SyncStreamEventDto {
  eventId:string;
  streamKeyText:string;
  streamKey:SyncStreamKeyDto;
  sequence:bigint;
  sourceId:string;
  sourceKind:string;
  payload:string;
  renderMode:string;
  tags:string[];
  createdAtUtc:string;
  acceptedAtUtc:string;
}
