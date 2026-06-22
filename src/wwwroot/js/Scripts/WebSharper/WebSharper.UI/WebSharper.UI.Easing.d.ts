import Object from "../../../Content/WebSharper/WebSharper.StdLib/System.Object"
export default class Easing extends Object {
  transformTime:((a:number) => number);
  static get CubicInOut():Easing
  static Custom(f:((a:number) => number)):Easing
  TransformTime(t:number):number
  constructor(transformTime:((a:number) => number))
}
