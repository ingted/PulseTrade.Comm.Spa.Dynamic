export function New(valueId, sequence, mode, source, rows, columns, tableRows, rawValue, tags, createdAtUtc)
export default interface AppendPageValueViewDto {
  valueId:string;
  sequence:bigint;
  mode:string;
  source:string;
  rows:string[];
  columns:string[];
  tableRows:(string[])[];
  rawValue:string;
  tags:string[];
  createdAtUtc:string;
}
