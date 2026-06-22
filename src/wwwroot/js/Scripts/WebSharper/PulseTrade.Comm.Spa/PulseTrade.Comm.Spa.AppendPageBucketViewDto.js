export function New(keyId, keys, setName, valueCount, minSequence, maxSequence, updatedAtUtc, values){
  return{
    keyId:keyId, 
    keys:keys, 
    setName:setName, 
    valueCount:valueCount, 
    minSequence:minSequence, 
    maxSequence:maxSequence, 
    updatedAtUtc:updatedAtUtc, 
    values:values
  };
}
