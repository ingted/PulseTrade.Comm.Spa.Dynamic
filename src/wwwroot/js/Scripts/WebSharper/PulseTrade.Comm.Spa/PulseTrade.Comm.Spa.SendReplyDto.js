export function New(status, message, deliveryHint, streamSequence){
  return{
    status:status, 
    message:message, 
    deliveryHint:deliveryHint, 
    streamSequence:streamSequence
  };
}
