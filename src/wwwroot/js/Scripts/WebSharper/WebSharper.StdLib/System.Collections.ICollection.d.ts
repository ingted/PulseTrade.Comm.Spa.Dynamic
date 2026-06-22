import IEnumerable from "../../../Content/WebSharper/WebSharper.StdLib/System.Collections.IEnumerable"
export function isICollection(x):x is ICollection
export default interface ICollection extends IEnumerable { }
