export function New(type, requestId, pageId, title, setName, streamKey, actorAddress, rawArgu, renderMode, tags, browserId, tabId){
  return{
    type:type, 
    requestId:requestId, 
    pageId:pageId, 
    title:title, 
    setName:setName, 
    streamKey:streamKey, 
    actorAddress:actorAddress, 
    rawArgu:rawArgu, 
    renderMode:renderMode, 
    tags:tags, 
    browserId:browserId, 
    tabId:tabId
  };
}
