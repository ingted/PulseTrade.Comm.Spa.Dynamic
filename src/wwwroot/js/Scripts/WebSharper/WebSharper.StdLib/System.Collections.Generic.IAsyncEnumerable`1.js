export function isIAsyncEnumerable(x){
  return"GetAsyncEnumerator"in x;
}
