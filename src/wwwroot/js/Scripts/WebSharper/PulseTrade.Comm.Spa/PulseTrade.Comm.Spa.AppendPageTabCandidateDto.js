export function New(pageId, tabId, title, setName, shape, path, visible, registeredAtUtc, hiddenAtUtc){
  return{
    pageId:pageId, 
    tabId:tabId, 
    title:title, 
    setName:setName, 
    shape:shape, 
    path:path, 
    visible:visible, 
    registeredAtUtc:registeredAtUtc, 
    hiddenAtUtc:hiddenAtUtc
  };
}
