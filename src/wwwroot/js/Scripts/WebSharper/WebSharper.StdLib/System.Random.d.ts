import Object from "../../../Content/WebSharper/WebSharper.StdLib/System.Object"
export default class Random extends Object {
  NextBytes(buffer:number[]):void
  Next(minValue:number, maxValue:number):number
  Next_1(maxValue:number):number
  Next_2():number
}
