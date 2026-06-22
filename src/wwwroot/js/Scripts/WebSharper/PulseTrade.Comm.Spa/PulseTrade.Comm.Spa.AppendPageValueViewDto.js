export function New(valueId, sequence, mode, source, rows, columns, tableRows, rawValue, tags, createdAtUtc){
  return{
    valueId:valueId, 
    sequence:sequence, 
    mode:mode, 
    source:source, 
    rows:rows, 
    columns:columns, 
    tableRows:tableRows, 
    rawValue:rawValue, 
    tags:tags, 
    createdAtUtc:createdAtUtc
  };
}
