import Doc from "../../../Content/WebSharper/WebSharper.UI/WebSharper.UI.Doc"
export function Create(doc:Doc):FlowPage
export default interface FlowPage {
  Doc:Doc;
  GoNext:((a:(() => void)) => void);
}
