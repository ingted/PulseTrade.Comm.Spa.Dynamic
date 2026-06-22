import IEnumerable from "../../../Content/WebSharper/WebSharper.StdLib/System.Collections.Generic.IEnumerable`1"
import Dictionary from "../../../Content/WebSharper/WebSharper.StdLib/System.Collections.Generic.Dictionary`2"
export function dict<T0, T1>(s:IEnumerable<[T0, T1]>):Dictionary<T0, T1>
export function create2D<T0, T1>(rows:IEnumerable<T0>):((T1)[])[]
