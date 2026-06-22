export function New(type, requestId, streamKey, payload, sourceKind, renderMode, idempotencyKey, tags, browserId, tabId){
  return{
    type:type, 
    requestId:requestId, 
    streamKey:streamKey, 
    payload:payload, 
    sourceKind:sourceKind, 
    renderMode:renderMode, 
    idempotencyKey:idempotencyKey, 
    tags:tags, 
    browserId:browserId, 
    tabId:tabId
  };
}
