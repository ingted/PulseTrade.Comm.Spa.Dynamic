export function New(serverRealityId, journalId, persistenceNamespace, projectionId, projectionEpochId, schemaName, deliveryProfileId, isCrashDurable)
export default interface BrowserServerRealityDto {
  serverRealityId:string;
  journalId:string;
  persistenceNamespace:string;
  projectionId:string;
  projectionEpochId:string;
  schemaName:string;
  deliveryProfileId:string;
  isCrashDurable:boolean;
}
