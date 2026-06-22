export function New(nodeId, nodeAddress, status, roles, actors){
  return{
    nodeId:nodeId, 
    nodeAddress:nodeAddress, 
    status:status, 
    roles:roles, 
    actors:actors
  };
}
