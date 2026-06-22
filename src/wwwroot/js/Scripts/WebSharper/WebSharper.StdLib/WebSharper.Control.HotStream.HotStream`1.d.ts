import { FSharpOption } from "../../../Content/WebSharper/WebSharper.StdLib/Microsoft.FSharp.Core.FSharpOption`1"
import FSharpEvent from "../../../Content/WebSharper/WebSharper.StdLib/Microsoft.FSharp.Control.FSharpEvent`1"
import IObserver from "../../../Content/WebSharper/WebSharper.StdLib/System.IObserver`1"
import IDisposable from "../../../Content/WebSharper/WebSharper.StdLib/System.IDisposable"
import IObservable from "../../../Content/WebSharper/WebSharper.StdLib/System.IObservable`1"
export default class HotStream<T0>implements IObservable<T0> {
  Latest:[FSharpOption<T0>];
  Event:FSharpEvent<T0>;
  static New_1<T0, T1>():HotStream<T1>
  Trigger():void
  Trigger(v?:T0):void
  Subscribe(o:IObserver<T0>):IDisposable
  static New<T0>(Latest:[FSharpOption<T0>], Event:FSharpEvent<T0>):HotStream<T0>
}
