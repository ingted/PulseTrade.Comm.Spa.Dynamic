export function New(participantId, displayName, login, authenticated){
  return{
    participantId:participantId, 
    displayName:displayName, 
    login:login, 
    authenticated:authenticated
  };
}
