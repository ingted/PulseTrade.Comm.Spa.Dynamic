export function New(valueId, keys, createdAtUtc, value, tags){
  return{
    valueId:valueId, 
    keys:keys, 
    createdAtUtc:createdAtUtc, 
    value:value, 
    tags:tags
  };
}
