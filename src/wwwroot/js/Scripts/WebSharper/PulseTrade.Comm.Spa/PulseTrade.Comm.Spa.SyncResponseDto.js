export function New(type, requestId, status, streamKey, event, events, error){
  return{
    type:type, 
    requestId:requestId, 
    status:status, 
    streamKey:streamKey, 
    event:event, 
    events:events, 
    error:error
  };
}
