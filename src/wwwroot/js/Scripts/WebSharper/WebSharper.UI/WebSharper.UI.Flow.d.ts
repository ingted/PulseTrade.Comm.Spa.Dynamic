import EndedFlowActions from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.EndedFlowActions"
import Doc from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.Doc"
import Flow from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.Flow`1"
import { View_T } from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.View`1"
import Var from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.Var`1"
export function EndRestartable(f:((a:EndedFlowActions) => Doc)):Flow<void>
export function End(doc:Doc):Flow<void>
export function EmbedWithCancel<T0>(cancel:((a:EndedFlowActions) => Doc), fl:Flow<T0>):Doc
export function Embed<T0>(fl:Flow<T0>):Doc
export function Return<T0>():Flow<T0>
export function Return<T0>(x?:T0):Flow<T0>
export function View<T0>(f:Flow<T0>):Flow<View_T<T0>>
export function Bind<T0, T1>(m:Flow<T0>, k:((a?:T0) => Flow<T1>)):Flow<T1>
export function ValidateVar<T0>(pred:((a?:T0) => boolean), x:Flow<Var<T0>>):Flow<Var<T0>>
export function ValidateView<T0>(pred:((a?:T0) => boolean), x:Flow<View_T<T0>>):Flow<View_T<T0>>
export function Map<T0, T1>(f:((a?:T0) => T1), x:Flow<T0>):Flow<T1>
