export function New(extensionId, displayName, scriptUrls, appendPageShapes){
  return{
    extensionId:extensionId, 
    displayName:displayName, 
    scriptUrls:scriptUrls, 
    appendPageShapes:appendPageShapes
  };
}
