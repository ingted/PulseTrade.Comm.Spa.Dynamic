import IEnumerable from "../../../Content/WebSharper/WebSharper.StdLib/System.Collections.Generic.IEnumerable`1"
import Pair from "../../../Content/WebSharper/WebSharper.StdLib/WebSharper.Collections.Pair`2"
import Tree from "../../../Content/WebSharper/WebSharper.StdLib/WebSharper.Collections.BalancedTree.Tree`1"
export function fromSeq<T0, T1>(s:IEnumerable<[T0, T1]>):Tree<Pair<T0, T1>>
