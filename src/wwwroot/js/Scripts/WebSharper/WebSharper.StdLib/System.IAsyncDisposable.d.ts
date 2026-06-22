import ValueTask from "../../../Content/WebSharper/WebSharper.StdLib/System.Threading.Tasks.ValueTask"
export function isIAsyncDisposable(x):x is IAsyncDisposable
export default interface IAsyncDisposable {
  DisposeAsync():ValueTask
}
