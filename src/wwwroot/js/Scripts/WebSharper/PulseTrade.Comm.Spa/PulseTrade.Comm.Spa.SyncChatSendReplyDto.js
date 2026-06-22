export function New(type, requestId, status, message, deliveryHint, event, error){
  return{
    type:type, 
    requestId:requestId, 
    status:status, 
    message:message, 
    deliveryHint:deliveryHint, 
    event:event, 
    error:error
  };
}
