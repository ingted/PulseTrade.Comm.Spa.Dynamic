import Snap from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.Snap`1"
export default interface LazyView<T0>{
  c:Snap<T0>;
  o:(() => Snap<T0>);
}
