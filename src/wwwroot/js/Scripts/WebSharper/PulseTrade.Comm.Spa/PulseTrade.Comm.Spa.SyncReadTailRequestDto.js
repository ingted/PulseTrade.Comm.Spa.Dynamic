export function New(type, requestId, streamKey, count){
  return{
    type:type, 
    requestId:requestId, 
    streamKey:streamKey, 
    count:count
  };
}
