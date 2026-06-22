export function New(participantId, displayName, kind, labels, status){
  return{
    participantId:participantId, 
    displayName:displayName, 
    kind:kind, 
    labels:labels, 
    status:status
  };
}
