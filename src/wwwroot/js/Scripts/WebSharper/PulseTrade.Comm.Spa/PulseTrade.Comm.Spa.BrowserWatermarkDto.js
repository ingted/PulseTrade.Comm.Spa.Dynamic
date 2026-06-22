export function New(streamId, newestSequence, cachedCount, source, touchedAt){
  return{
    streamId:streamId, 
    newestSequence:newestSequence, 
    cachedCount:cachedCount, 
    source:source, 
    touchedAt:touchedAt
  };
}
