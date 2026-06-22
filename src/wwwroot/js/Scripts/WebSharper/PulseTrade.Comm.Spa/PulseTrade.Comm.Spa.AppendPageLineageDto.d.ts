export function New(streamPageId, lineageKind, legacyPageIdAlias, readsLegacyPageStreams, readRepairPolicy)
export default interface AppendPageLineageDto {
  streamPageId:string;
  lineageKind:string;
  legacyPageIdAlias:string;
  readsLegacyPageStreams:boolean;
  readRepairPolicy:string;
}
