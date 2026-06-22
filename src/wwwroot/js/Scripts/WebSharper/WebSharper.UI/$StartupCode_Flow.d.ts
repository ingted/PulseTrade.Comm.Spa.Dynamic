import FlowBuilder from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.FlowBuilder"
import WeakRef from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.WeakRef`1"
export default class $StartupCode_Flow {
  static flow:FlowBuilder;
  static flowPrevStateName:string;
  static flowStateName:string;
  static flowVars:(WeakRef<{Get:(() => number),Set:((a:number) => void)}>)[];
}
