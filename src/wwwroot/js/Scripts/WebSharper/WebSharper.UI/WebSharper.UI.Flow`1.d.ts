import FlowState from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.FlowState"
import FlowActions from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.FlowActions`1"
import Doc from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.Doc"
import Object from "../../../Content/WebSharper/WebSharper.StdLib/System.Object"
export default class Flow<T0>extends Object {
  render:((a:FlowState) => ((a:FlowActions<T0>) => void));
  constructor(i:"New", define:((a:FlowActions<T0>) => Doc))
  constructor(i:"New_1", render:((a:FlowState) => ((a:FlowActions<T0>) => void)))
}
