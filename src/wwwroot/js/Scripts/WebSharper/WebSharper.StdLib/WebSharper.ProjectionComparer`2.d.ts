import IComparer from "../../../Content/WebSharper/WebSharper.StdLib/System.Collections.Generic.IComparer`1"
import Object from "../../../Content/WebSharper/WebSharper.StdLib/System.Object"
export default class ProjectionComparer<T0, T1>extends Object implements IComparer<T0> {
  primary:IComparer<T1>;
  projection:((a:T0) => T1);
  Compare(x:T0, y:T0):number
  constructor(primary:IComparer<T1>, projection:((a:T0) => T1))
}
