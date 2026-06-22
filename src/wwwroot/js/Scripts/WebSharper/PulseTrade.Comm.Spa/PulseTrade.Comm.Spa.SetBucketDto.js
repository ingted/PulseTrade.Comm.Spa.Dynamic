export function New(keyId, setName, keys, valueCount, maxSequence, updatedAtUtc, values){
  return{
    keyId:keyId, 
    setName:setName, 
    keys:keys, 
    valueCount:valueCount, 
    maxSequence:maxSequence, 
    updatedAtUtc:updatedAtUtc, 
    values:values
  };
}
