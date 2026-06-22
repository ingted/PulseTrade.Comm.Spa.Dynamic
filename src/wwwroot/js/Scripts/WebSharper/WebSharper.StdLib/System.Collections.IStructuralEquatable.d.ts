import IEqualityComparer from "../../../Content/WebSharper/WebSharper.StdLib/System.Collections.IEqualityComparer"
export function isIStructuralEquatable(x):x is IStructuralEquatable
export default interface IStructuralEquatable {
  SEquals(a, b:IEqualityComparer):boolean
  SGetHashCode(a:IEqualityComparer):number
}
