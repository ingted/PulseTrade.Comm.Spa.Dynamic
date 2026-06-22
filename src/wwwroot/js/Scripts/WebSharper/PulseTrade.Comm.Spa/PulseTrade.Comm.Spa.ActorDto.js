export function New(actorId, displayName, kind, keys, status, routees){
  return{
    actorId:actorId, 
    displayName:displayName, 
    kind:kind, 
    keys:keys, 
    status:status, 
    routees:routees
  };
}
