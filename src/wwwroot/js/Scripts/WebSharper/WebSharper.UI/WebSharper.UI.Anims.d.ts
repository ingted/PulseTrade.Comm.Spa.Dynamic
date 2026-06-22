import { Anim } from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.Anim"
import IEnumerable from "../../../Content/WebSharper/WebSharper.StdLib/System.Collections.Generic.IEnumerable`1"
import { Animation } from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.Animation"
import { AppendList } from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.AppendList`1"
export function UseAnimations():boolean
export function set_UseAnimations(_1:boolean):boolean
export function Actions(a:Anim):{Compute:((a:number) => void),Duration:number}
export function ConcatActions(xs:IEnumerable<{Compute:((a:number) => void),Duration:number}>):{Compute:((a:number) => void),Duration:number}
export function Prolong<T0>(nextDuration:number, anim:{Compute:((a:number) => T0),Duration:number}):{Compute:((a:number) => T0),Duration:number}
export function Const<T0>():{Compute:((a:number) => T0),Duration:number}
export function Const<T0>(v?:T0):{Compute:((a:number) => T0),Duration:number}
export function Def<T0>(d:number, f:((a:number) => T0)):{Compute:((a:number) => T0),Duration:number}
export function Finalize(a:Anim):void
export function List(a:Anim):AppendList<Animation>
