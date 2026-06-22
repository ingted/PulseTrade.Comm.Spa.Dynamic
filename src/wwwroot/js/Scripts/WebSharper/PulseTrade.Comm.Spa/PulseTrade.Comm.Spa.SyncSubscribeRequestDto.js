export function New(type, requestId, streamKey){
  return{
    type:type, 
    requestId:requestId, 
    streamKey:streamKey
  };
}
