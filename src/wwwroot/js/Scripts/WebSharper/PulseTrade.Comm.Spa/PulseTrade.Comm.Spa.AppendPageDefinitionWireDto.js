export function New(schema, pageId, tabId, path, title, setName, shape, description, keyPlaceholder, valuePlaceholder, defaultKey, tags){
  return{
    schema:schema, 
    pageId:pageId, 
    tabId:tabId, 
    path:path, 
    title:title, 
    setName:setName, 
    shape:shape, 
    description:description, 
    keyPlaceholder:keyPlaceholder, 
    valuePlaceholder:valuePlaceholder, 
    defaultKey:defaultKey, 
    tags:tags
  };
}
