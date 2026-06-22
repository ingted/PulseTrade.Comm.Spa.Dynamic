export function New(streamPageId, lineageKind, legacyPageIdAlias, readsLegacyPageStreams, readRepairPolicy, candidateValueStreamKeys, candidateValueStreamCount, candidateKeyRegistryStreamKeys, candidateKeyRegistryStreamCount){
  return{
    streamPageId:streamPageId, 
    lineageKind:lineageKind, 
    legacyPageIdAlias:legacyPageIdAlias, 
    readsLegacyPageStreams:readsLegacyPageStreams, 
    readRepairPolicy:readRepairPolicy, 
    candidateValueStreamKeys:candidateValueStreamKeys, 
    candidateValueStreamCount:candidateValueStreamCount, 
    candidateKeyRegistryStreamKeys:candidateKeyRegistryStreamKeys, 
    candidateKeyRegistryStreamCount:candidateKeyRegistryStreamCount
  };
}
