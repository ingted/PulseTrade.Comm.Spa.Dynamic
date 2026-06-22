export function New(schema, nodeId, nodeAddress, actorId, displayName, kind, status, roles, routees, tags){
  return{
    schema:schema, 
    nodeId:nodeId, 
    nodeAddress:nodeAddress, 
    actorId:actorId, 
    displayName:displayName, 
    kind:kind, 
    status:status, 
    roles:roles, 
    routees:routees, 
    tags:tags
  };
}
