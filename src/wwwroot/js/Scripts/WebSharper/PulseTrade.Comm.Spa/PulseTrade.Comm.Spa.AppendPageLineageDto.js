export function New(streamPageId, lineageKind, legacyPageIdAlias, readsLegacyPageStreams, readRepairPolicy){
  return{
    streamPageId:streamPageId, 
    lineageKind:lineageKind, 
    legacyPageIdAlias:legacyPageIdAlias, 
    readsLegacyPageStreams:readsLegacyPageStreams, 
    readRepairPolicy:readRepairPolicy
  };
}
