import Object from "../../../Content/WebSharper/WebSharper.StdLib/System.Object"
import IEqualityComparer from "../../../Content/WebSharper/WebSharper.StdLib/System.Collections.IEqualityComparer"
import IEqualityComparer_1 from "../../../Content/WebSharper/WebSharper.StdLib/System.Collections.Generic.IEqualityComparer`1"
export default class EqualityComparer<T0>extends Object implements IEqualityComparer, IEqualityComparer_1<T0> {
  CGetHashCode0(x):number
  CEquals0(x, y):boolean
  CGetHashCode():number
  CGetHashCode(x?:T0):number
  CEquals(x:T0, y:T0):boolean
}
