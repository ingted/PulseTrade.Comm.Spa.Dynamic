export function New(status, page, keyId, afterSequence, count, lineage, lineageHealth, values){
  return{
    status:status, 
    page:page, 
    keyId:keyId, 
    afterSequence:afterSequence, 
    count:count, 
    lineage:lineage, 
    lineageHealth:lineageHealth, 
    values:values
  };
}
