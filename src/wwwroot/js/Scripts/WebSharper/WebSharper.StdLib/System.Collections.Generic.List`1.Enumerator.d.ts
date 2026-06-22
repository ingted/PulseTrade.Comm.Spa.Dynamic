import Object from "../../../Content/WebSharper/WebSharper.StdLib/System.Object"
import IEnumerator from "../../../Content/WebSharper/WebSharper.StdLib/System.Collections.Generic.IEnumerator`1"
import IDisposable from "../../../Content/WebSharper/WebSharper.StdLib/System.IDisposable"
import IEnumerator_1 from "../../../Content/WebSharper/WebSharper.StdLib/System.Collections.IEnumerator"
export default class Enumerator<T0>extends Object implements IEnumerator<T0>, IDisposable, IEnumerator_1 {
  arr:(T0)[];
  i:number;
  get Current_1():T0
  MoveNext_1():boolean
  Dispose():void
  get Current():T0
  get Current0()
  MoveNext():boolean
  constructor(arr:(T0)[])
}
