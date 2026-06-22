import IComparer from "../../../Content/WebSharper/WebSharper.StdLib/System.Collections.IComparer"
export function isIStructuralComparable(x):x is IStructuralComparable
export default interface IStructuralComparable {
  SCompareTo(a, b:IComparer):number
}
