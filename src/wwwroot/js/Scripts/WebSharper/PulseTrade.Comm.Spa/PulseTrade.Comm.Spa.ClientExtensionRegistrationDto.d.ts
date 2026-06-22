import ClientAppendPageShapeRegistrationDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.ClientAppendPageShapeRegistrationDto"
export function New(extensionId, displayName, scriptUrls, appendPageShapes)
export default interface ClientExtensionRegistrationDto {
  extensionId:string;
  displayName:string;
  scriptUrls:string[];
  appendPageShapes:(ClientAppendPageShapeRegistrationDto)[];
}
