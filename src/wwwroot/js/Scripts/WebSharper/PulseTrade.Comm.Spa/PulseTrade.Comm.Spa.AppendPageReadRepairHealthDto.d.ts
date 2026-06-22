export function New(streamPageId, lineageKind, legacyPageIdAlias, readsLegacyPageStreams, readRepairPolicy, candidateValueStreamKeys, candidateValueStreamCount, candidateKeyRegistryStreamKeys, candidateKeyRegistryStreamCount)
export default interface AppendPageReadRepairHealthDto {
  streamPageId:string;
  lineageKind:string;
  legacyPageIdAlias:string;
  readsLegacyPageStreams:boolean;
  readRepairPolicy:string;
  candidateValueStreamKeys:string[];
  candidateValueStreamCount:number;
  candidateKeyRegistryStreamKeys:string[];
  candidateKeyRegistryStreamCount:number;
}
