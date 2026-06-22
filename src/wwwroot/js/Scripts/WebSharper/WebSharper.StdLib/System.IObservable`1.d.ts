import IObserver from "../../../Content/WebSharper/WebSharper.StdLib/System.IObserver`1"
import IDisposable from "../../../Content/WebSharper/WebSharper.StdLib/System.IDisposable"
export function isIObservable<T0>(x):x is IObservable<T0>
export default interface IObservable<T0>{
  Subscribe(a:IObserver<T0>):IDisposable
}
