export function New(serverRealityId, journalId, persistenceNamespace, projectionId, projectionEpochId, schemaName, deliveryProfileId, isCrashDurable){
  return{
    serverRealityId:serverRealityId, 
    journalId:journalId, 
    persistenceNamespace:persistenceNamespace, 
    projectionId:projectionId, 
    projectionEpochId:projectionEpochId, 
    schemaName:schemaName, 
    deliveryProfileId:deliveryProfileId, 
    isCrashDurable:isCrashDurable
  };
}
