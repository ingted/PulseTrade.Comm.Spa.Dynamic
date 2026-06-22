export function isIAsyncDisposable(x){
  return"DisposeAsync"in x;
}
