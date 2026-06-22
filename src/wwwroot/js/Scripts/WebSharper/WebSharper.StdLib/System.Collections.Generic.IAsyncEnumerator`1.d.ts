import ValueTask from "../../../Content/WebSharper/WebSharper.StdLib/System.Threading.Tasks.ValueTask`1"
import IAsyncDisposable from "../../../Content/WebSharper/WebSharper.StdLib/System.IAsyncDisposable"
export function isIAsyncEnumerator<T0, T1>(x):x is IAsyncEnumerator<T0>
export default interface IAsyncEnumerator<T0>extends IAsyncDisposable {
  MoveNextAsync():ValueTask<boolean>
  get Current():T0
}
