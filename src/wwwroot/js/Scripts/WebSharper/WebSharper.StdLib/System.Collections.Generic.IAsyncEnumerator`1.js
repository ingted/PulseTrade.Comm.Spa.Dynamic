export function isIAsyncEnumerator(x){
  return"MoveNextAsync"in x&&"Current"in x;
}
