export function New(messageId, fromId, toId, scope, body, createdAtUtc){
  return{
    messageId:messageId, 
    fromId:fromId, 
    toId:toId, 
    scope:scope, 
    body:body, 
    createdAtUtc:createdAtUtc
  };
}
