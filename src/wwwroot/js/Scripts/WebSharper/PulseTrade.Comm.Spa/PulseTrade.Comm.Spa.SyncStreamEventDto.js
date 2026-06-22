export function New(eventId, streamKeyText, streamKey, sequence, sourceId, sourceKind, payload, renderMode, tags, createdAtUtc, acceptedAtUtc){
  return{
    eventId:eventId, 
    streamKeyText:streamKeyText, 
    streamKey:streamKey, 
    sequence:sequence, 
    sourceId:sourceId, 
    sourceKind:sourceKind, 
    payload:payload, 
    renderMode:renderMode, 
    tags:tags, 
    createdAtUtc:createdAtUtc, 
    acceptedAtUtc:acceptedAtUtc
  };
}
