export function New(type, requestId, fromId, toId, body, tags, browserId, tabId){
  return{
    type:type, 
    requestId:requestId, 
    fromId:fromId, 
    toId:toId, 
    body:body, 
    tags:tags, 
    browserId:browserId, 
    tabId:tabId
  };
}
