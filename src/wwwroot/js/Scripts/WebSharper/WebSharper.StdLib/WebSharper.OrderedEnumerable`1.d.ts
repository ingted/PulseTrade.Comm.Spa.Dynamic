import IEnumerable from "../../../Content/WebSharper/WebSharper.StdLib/System.Collections.Generic.IEnumerable`1"
import IComparer from "../../../Content/WebSharper/WebSharper.StdLib/System.Collections.Generic.IComparer`1"
import IEnumerator from "../../../Content/WebSharper/WebSharper.StdLib/System.Collections.Generic.IEnumerator`1"
import IOrderedEnumerable from "../../../Content/WebSharper/WebSharper.StdLib/System.Linq.IOrderedEnumerable`1"
import Object from "../../../Content/WebSharper/WebSharper.StdLib/System.Object"
export default class OrderedEnumerable<T0>extends Object implements IOrderedEnumerable<T0>, IEnumerable<T0> {
  source:IEnumerable<T0>;
  primary:IComparer<T0>;
  GetEnumerator():IEnumerator<T0>
  System_Linq_IOrderedEnumerable_1$CreateOrderedEnumerable<T1>(keySelector:((a:T0) => T1), secondary:IComparer<T1>, descending:boolean):IOrderedEnumerable<T0>
  constructor(source:IEnumerable<T0>, primary:IComparer<T0>)
}
