import IAsyncEnumerator from "../../../Content/WebSharper/WebSharper.StdLib/System.Collections.Generic.IAsyncEnumerator`1"
export function isIAsyncEnumerable<T0>(x):x is IAsyncEnumerable<T0>
export default interface IAsyncEnumerable<T0>{
  GetAsyncEnumerator(a:{c:boolean,r:(() => void)[]}):IAsyncEnumerator<T0>
}
