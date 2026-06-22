export function New(status, page, bucketCount, maxSequence, keyMaxSequence, lineage, lineageHealth, buckets){
  return{
    status:status, 
    page:page, 
    bucketCount:bucketCount, 
    maxSequence:maxSequence, 
    keyMaxSequence:keyMaxSequence, 
    lineage:lineage, 
    lineageHealth:lineageHealth, 
    buckets:buckets
  };
}
