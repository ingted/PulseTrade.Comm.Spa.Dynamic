import Dictionary from "../../../Content/WebSharper/WebSharper.StdLib/System.Collections.Generic.Dictionary`2"
import Object from "../../../Content/WebSharper/WebSharper.StdLib/System.Object"
export default class ValueCollection<T0, T1>extends Object {
  d:Dictionary<T0, T1>;
  GetEnumerator()
  get IsReadOnly():boolean
  get Count():number
  Contains():boolean
  Contains(item?:T1):boolean
  CopyTo(arr:(T1)[], index:number):void
  constructor(d:Dictionary<T0, T1>)
}
