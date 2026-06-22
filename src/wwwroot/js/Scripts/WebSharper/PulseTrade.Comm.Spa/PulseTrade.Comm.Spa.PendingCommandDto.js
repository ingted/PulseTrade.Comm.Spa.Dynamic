export function New(commandId, serverRealityId, kind, target, url, method, payloadJson, status){
  return{
    commandId:commandId, 
    serverRealityId:serverRealityId, 
    kind:kind, 
    target:target, 
    url:url, 
    method:method, 
    payloadJson:payloadJson, 
    status:status
  };
}
