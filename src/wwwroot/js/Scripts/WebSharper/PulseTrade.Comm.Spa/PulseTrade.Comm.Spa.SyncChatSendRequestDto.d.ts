export function New(type, requestId, fromId, toId, body, tags, browserId, tabId)
export default interface SyncChatSendRequestDto {
  type:string;
  requestId:string;
  fromId:string;
  toId:string;
  body:string;
  tags:string[];
  browserId:string;
  tabId:string;
}
