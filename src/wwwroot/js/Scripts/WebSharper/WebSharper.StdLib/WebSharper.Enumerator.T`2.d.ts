import Object from "../../../Content/WebSharper/WebSharper.StdLib/System.Object"
import IEnumerator from "../../../Content/WebSharper/WebSharper.StdLib/System.Collections.Generic.IEnumerator`1"
import IDisposable from "../../../Content/WebSharper/WebSharper.StdLib/System.IDisposable"
import IEnumerator_1 from "../../../Content/WebSharper/WebSharper.StdLib/System.Collections.IEnumerator"
export default class T<T0, T1>extends Object implements IEnumerator<T1>, IDisposable, IEnumerator_1 {
  s:T0;
  c:T1;
  n:((a:T<T0, T1>) => boolean);
  d:((a:T<T0, T1>) => void);
  e:number;
  Dispose():void
  get Current():T1
  get Current0()
  MoveNext():boolean
  constructor(s:T0, c:T1, n:((a:T<T0, T1>) => boolean), d:((a:T<T0, T1>) => void))
}
