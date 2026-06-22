export function isICommSpaPersistenceBackend(x){
  return"PulseTrade_Comm_Spa_ICommSpaPersistenceBackend$Append"in x&&"PulseTrade_Comm_Spa_ICommSpaPersistenceBackend$ReadAfter"in x&&"PulseTrade_Comm_Spa_ICommSpaPersistenceBackend$ReadBefore"in x&&"PulseTrade_Comm_Spa_ICommSpaPersistenceBackend$ReadTail"in x&&"PulseTrade_Comm_Spa_ICommSpaPersistenceBackend$Snapshot"in x;
}
