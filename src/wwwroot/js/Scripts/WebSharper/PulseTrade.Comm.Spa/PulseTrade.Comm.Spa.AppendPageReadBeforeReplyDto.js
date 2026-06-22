export function New(status, page, keyId, beforeSequence, count, lineage, lineageHealth, values){
  return{
    status:status, 
    page:page, 
    keyId:keyId, 
    beforeSequence:beforeSequence, 
    count:count, 
    lineage:lineage, 
    lineageHealth:lineageHealth, 
    values:values
  };
}
