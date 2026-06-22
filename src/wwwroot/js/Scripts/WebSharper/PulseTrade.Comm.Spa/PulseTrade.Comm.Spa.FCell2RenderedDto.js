export function New(schema, mode, source, rows, tableRows){
  return{
    schema:schema, 
    mode:mode, 
    source:source, 
    rows:rows, 
    tableRows:tableRows
  };
}
