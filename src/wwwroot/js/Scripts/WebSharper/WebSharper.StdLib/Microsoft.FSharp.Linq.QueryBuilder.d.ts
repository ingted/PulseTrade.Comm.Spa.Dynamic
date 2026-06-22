import IEnumerable from "../../../Content/WebSharper/WebSharper.StdLib/System.Collections.Generic.IEnumerable`1"
import IOrderedEnumerable from "../../../Content/WebSharper/WebSharper.StdLib/System.Linq.IOrderedEnumerable`1"
import Object from "../../../Content/WebSharper/WebSharper.StdLib/System.Object"
export default class QueryBuilder extends Object {
  static averageByNullable<T0, T1, T2>(source:IEnumerable<T0>, projection:((a?:T0) => T2)):T2
  static sumByNullable<T0, T1, T2>(source:IEnumerable<T0>, projection:((a?:T0) => T2)):T2
  static CheckThenBySource<T0>(source:IEnumerable<T0>):IOrderedEnumerable<T0>
}
