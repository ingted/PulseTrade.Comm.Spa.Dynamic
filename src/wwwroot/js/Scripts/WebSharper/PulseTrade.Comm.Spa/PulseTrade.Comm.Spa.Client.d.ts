import AppendPageDefinitionDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageDefinitionDto"
import { FSharpOption } from "../../Content/WebSharper/WebSharper.StdLib/Microsoft.FSharp.Core.FSharpOption`1"
import AppendPageValueViewDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageValueViewDto"
import MessageDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.MessageDto"
import RemoveAppendPageKeyRequestDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.RemoveAppendPageKeyRequestDto"
import AppendPageKeyListReplyDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageKeyListReplyDto"
import AppendPageKeyRequestDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageKeyRequestDto"
import AppendPageKeyReplyDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageKeyReplyDto"
import AppendPageAppendRequestDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageAppendRequestDto"
import AppendPageAppendReplyDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageAppendReplyDto"
import FCell2RenderedDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.FCell2RenderedDto"
import PendingCommandDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.PendingCommandDto"
import AppendPagesReplyDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPagesReplyDto"
import SyncStreamEventDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.SyncStreamEventDto"
import AppendPageHiddenWireDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageHiddenWireDto"
import AppendPageDefinitionWireDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageDefinitionWireDto"
import SyncStreamKeyDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.SyncStreamKeyDto"
import AppendPageSnapshotDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.AppendPageSnapshotDto"
import ActorsReplyDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.ActorsReplyDto"
import SetBucketDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.SetBucketDto"
import ClientAppendPageShapeRegistrationDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.ClientAppendPageShapeRegistrationDto"
import ClientExtensionRegistrationDto from "../../Content/WebSharper/PulseTrade.Comm.Spa/PulseTrade.Comm.Spa.ClientExtensionRegistrationDto"
export function Main():void
export function refreshAppendNav(activePath:string):void
export function findAppendPage(path:string, pages:(AppendPageDefinitionDto)[]):FSharpOption<AppendPageDefinitionDto>
export function mountUnknownPage<T0>(page:Element, path:T0):void
export function mountActors(page:Element):void
export function mountAppendPage(page:Element, definition:AppendPageDefinitionDto):void
export function renderAppendValue(value:AppendPageValueViewDto):Element
export function mountSets(page:Element):void
export function mountChat(page:Element):void
export function mergeThreadMessages(existing:(MessageDto)[], incoming:(MessageDto)[]):(MessageDto)[]
export function distinctMessages(messages:(MessageDto)[]):(MessageDto)[]
export function keysAsJson(keys:string[]):string
export function scrollToBottomAfterRender(node:Element):void
export function scrollToBottomNow(node:Element):void
export function setMain(node:Node):void
export function shell(activePath:string, pages:(AppendPageDefinitionDto)[]):[Element, Element]
export function actorArguButtonLabel(page:AppendPageDefinitionDto):string
export function renderPageCreator(nav:Element, activePath:string, pages:(AppendPageDefinitionDto)[]):Element
export function renderNav(nav:Element, activePath:string, pages:(AppendPageDefinitionDto)[]):void
export function isCurrentPage(activePath:string, href:string):boolean
export function pageTypeClass(page:AppendPageDefinitionDto):string
export function pageTypeBadge(page:AppendPageDefinitionDto):string
export function pageTypeLabel(page:AppendPageDefinitionDto):string
export function pageTitle(page:AppendPageDefinitionDto):string
export function navigationPathForCreatedPage(page:AppendPageDefinitionDto):string
export function pagePath(page:AppendPageDefinitionDto):string
export function cardTitle(title:string, id:string, status:string, line:string):DocumentFragment
export function postRemoveAppendPageKey(url:string, body:RemoveAppendPageKeyRequestDto, onOk:((a:AppendPageKeyListReplyDto) => void), onError:((a:string) => void)):void
export function postAppendPageKey(url:string, body:AppendPageKeyRequestDto, onOk:((a:AppendPageKeyReplyDto) => void), onError:((a:string) => void)):void
export function postAppendPage(url:string, body:AppendPageAppendRequestDto, onOk:((a:AppendPageAppendReplyDto) => void), onError:((a:string) => void)):void
export function postJsonText(url:string, payloadJson:string, onOk:((a:string) => void), onError:((a:string) => void)):void
export function postJson<T0, T1>(url:string, body:T0, onOk:((a?:T1) => void), onError:((a:string) => void)):void
export function getJson<T0>(url:string, onOk:((a?:T0) => void), onError:((a:string) => void)):void
export function newRequestId(prefix:string):string
export function requestSeq():number
export function set_requestSeq(_1:number):number
export function syncWebSocketUrl():string
export function currentUserId():string
export function renderFCell2Rendered(rendered:FCell2RenderedDto):Element
export function tryFCell2Rendered(text:string):FSharpOption<FCell2RenderedDto>
export function isActorArguPage(page:AppendPageDefinitionDto):boolean
export function fcellValueModeLabel(mode:string, tags:string[]):string
export function hasTag(tag:string, tags:string[]):boolean
export function fcellModeLabel(mode:string):string
export function statusDot(status:string):Element
export function isLive(status:string):boolean
export function renderPendingInspection(node:Element, commands:(PendingCommandDto)[], foreignCommands:(PendingCommandDto)[]):void
export function pendingFailure<T0>(action:T0, error:string):string
export function rememberPending<T0>(kind:string, target:string, url:string, body:T0):string
export function newPendingCommandId(kind:string, target:string, url:string, payloadJson:string):string
export function pendingCommandSeq():number
export function set_pendingCommandSeq(_1:number):number
export function mergeAppendPageRegistryEvents(baseline:AppendPagesReplyDto, events:(SyncStreamEventDto)[]):AppendPagesReplyDto
export function hiddenPageFromWire(wire:AppendPageHiddenWireDto):FSharpOption<[string, string]>
export function pageDefinitionFromWire(wire:AppendPageDefinitionWireDto):FSharpOption<AppendPageDefinitionDto>
export function sortAppendPages(pages:(AppendPageDefinitionDto)[]):(AppendPageDefinitionDto)[]
export function sameTextInvariant(left:string, right:string):boolean
export function appendPageRegistryStreamKey():SyncStreamKeyDto
export function emptyAppendPagesReply():AppendPagesReplyDto
export function writeAppendPagesDefinitions(data:AppendPagesReplyDto):void
export function appendPagesDefinitionsCacheKey():string
export function writeSnapshotWithWatermark<T0>(cacheKey:string, value:T0, newestSequence:bigint, cachedCount:number, source:string):void
export function appendPageValueCount(snapshot:AppendPageSnapshotDto):number
export function actorValueCount(data:ActorsReplyDto):number
export function setValueCount(buckets:(SetBucketDto)[]):number
export function maxMessageSequence(messages:(MessageDto)[]):bigint
export function tryParseSequence(prefix:string, value:string):bigint
export function currentServerRealityId():string
export function renderTextBlock(className:string, text:string):Node
export function tryRenderWithRegisteredRenderers<T0>(text:string):FSharpOption<T0>
export function _initGlobally():number
export function _registerRendererGlobally<T0>():T0
export function RegisterAppendPageShape(shape:string, label:string, badge:string, className:string):void
export function findAppendPageShape(shape:string):FSharpOption<ClientAppendPageShapeRegistrationDto>
export function appendPageShapeOptions():([string, string])[]
export function appendPageShapeRegistry():(ClientAppendPageShapeRegistrationDto)[]
export function manifestAppendPageShapes():(ClientAppendPageShapeRegistrationDto)[]
export function serverClientExtensions():(ClientExtensionRegistrationDto)[]
export function builtInAppendPageShapes():(ClientAppendPageShapeRegistrationDto)[]
export function shapeRegistration(shape:string, label:string, badge:string, className:string):ClientAppendPageShapeRegistrationDto
export function normalizeShapeText(value:string):string
export function runtimeAppendPageShapes():(ClientAppendPageShapeRegistrationDto)[]
export function set_runtimeAppendPageShapes(_1:(ClientAppendPageShapeRegistrationDto)[]):(ClientAppendPageShapeRegistrationDto)[]
export function registeredRenderers():([string, ((a:string) => FSharpOption<Node>)])[]
export function set_registeredRenderers(_1:([string, ((a:string) => FSharpOption<Node>)])[]):([string, ((a:string) => FSharpOption<Node>)])[]
export function errorMessage(error):string
export function setStatus(node:Element, text:string):void
export function setData<T0>(name:string, value:string, node:T0):T0
export function setTestId<T0>(id:string, node:T0):T0
export function setId<T0>(id:string, node:T0):T0
export function setHref<T0>(href:string, node:T0):T0
export function textarea(className:string, placeholder:string):HTMLTextAreaElement
export function select(options:([string, string])[]):HTMLSelectElement
export function input(placeholder:string):HTMLInputElement
export function button(className:string, text:string):Element
export function clear(node:Node):void
export function append(parent:Node, children:Node[]):Node
export function element(tag:string, className:string, textValue:string):Element
export function int64OrZero(value:string):bigint
export function compactMessageId(value:string):string
export function textOr(fallback:string, value:string):string
export function joinValues(values:string[]):string
export function latestArray<T0>(limit:number, values:(T0)[]):(T0)[]
export function arrayOrEmpty<T0>(values:(T0)[]):(T0)[]
export function isBlank(value:string):boolean
export function asText(value:string):string
export function defaultCacheLimit():number
export function defaultRenderLimit():number
export function doc():Document
