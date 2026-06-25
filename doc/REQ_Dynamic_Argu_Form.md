# [REQ] PulseTrade.Comm.Spa.Dynamic: Argu-style DU Form Generator (SDUI-Driven)

## 1. 需求背景 (Background)
`PTCS.Dynamic` 目前已經支援將後端傳來的 SDUI JSON Schema 渲染成動態圖表 (Canvas)。
`DynamicRenderer` 已實作 TextInput、Dropdown、SelectBox、DatePicker、TimePicker、ColorPicker、Button 等輸入元件。

我們希望延伸這個已驗證的 SDUI 架構，讓**後端**透過 Reflection 讀取 F# Discriminated Union (Argu-style `IArgParserTemplate`) 的定義，產出一份描述表單的 SDUI JSON，前端只需用現有的 `DynamicRenderer` 渲染即可。

**核心原則：表單定義由後端控制（Server-Driven），前端只是 Renderer。**

## 2. 需求目標 (Objectives)
1. **後端 DU Reflection → SDUI JSON**: 後端新增 `ArguFormSchemaGenerator` 模組，透過 .NET Reflection 讀取指定的 DU Type 與 Union Case，將其欄位轉換為 `schema: "fskynet-sdui"` 格式的 JSON（包含 TextInput、Dropdown 等 SDUI 元件定義）。
2. **前端以 SDUI 渲染表單**: 前端收到 SDUI JSON 後，直接用現有的 `DynamicRenderer.renderNode` 渲染成表單 UI。不需要前端理解 DU 結構 — 後端決定欄位佈局、控件型別、預設值。
3. **SubmitArguForm Action**: 在 `DynamicRenderer` 中新增 `SubmitArguForm` action handler，負責遍歷所有帶 `arguParam` 屬性的 SDUI 元件，收集其值，組裝成 Argu-style 的 args string (e.g. `--price 100 --volume 50`)。
4. **透過現有管道發送**: 組裝好的 args string 透過 PTCS Core Extension Point 提供的 `submitFn` callback 送出，走 `rawArgu → ActorArguCore → fCell2.S` 管道，全程不需修改 PTCS Core 後端。

## 3. 使用者情境 (User Stories)
- **情境一 (Add Key)**: 我在 PTCS 畫面上點選「Add Key」，Dynamic Extension 跳出介面讓我選擇 Actor Address、DU Type (`Trade.OrderCommands`) 與 Union Case (`LimitOrder`)，三者組成 Key 加入清單。
- **情境二 (表單渲染)**: 我點擊 `LimitOrder` Key，前端向後端 Actor 請求 SDUI Form Schema。後端讀取 `LimitOrder` 的 DU 定義 (price: float, volume: int, side: Side)，回傳以下 SDUI JSON：
  ```json
  {
    "schema": "fskynet-sdui",
    "formMode": "argu-form",
    "sdui": [
      { "type": "Heading", "text": "LimitOrder" },
      { "type": "Row", "children": [
        { "type": "Label", "text": "Price:" },
        { "type": "TextInput", "id": "f-price", "arguParam": "--price", "placeholder": "float" },
        { "type": "Label", "text": "Volume:" },
        { "type": "TextInput", "id": "f-volume", "arguParam": "--volume", "placeholder": "int" }
      ]},
      { "type": "Row", "children": [
        { "type": "Label", "text": "Side:" },
        { "type": "Dropdown", "id": "f-side", "arguParam": "--side", "options": ["Buy", "Sell"] }
      ]},
      { "type": "Button", "id": "btn-submit", "text": "Send",
        "onClick": { "action": "SubmitArguForm", "includeStateOf": ["f-price", "f-volume", "f-side"] }
      }
    ]
  }
  ```
  前端 `DynamicRenderer` 原封不動地渲染成表單 UI。
- **情境三 (Send)**: 我填入 `Price=100, Volume=50, Side=Buy` 並點擊 Send。`SubmitArguForm` handler 收集值，組裝成 `--price 100 --volume 50 --side Buy`，呼叫 `submitFn`，走現有 actor-argu 管道送出。

## 4. 資料流示意 (Data Flow)
```
[1. 使用者選取 Key]
  Key: ["akka.tcp://.../actor", "Trade.OrderCommands", "LimitOrder"]
         ↓
[2. 前端向後端請求 Form Schema]
  → Actor 收到 GetFormSchema { duType = "Trade.OrderCommands", unionCase = "LimitOrder" }
         ↓
[3. 後端 ArguFormSchemaGenerator (Reflection)]
  → 讀取 DU type 的 Union Case fields
  → 產出 SDUI JSON (schema: "fskynet-sdui", formMode: "argu-form")
         ↓
[4. 前端 DynamicRenderer.renderNode]
  → 渲染 TextInput, Dropdown, Button 等元件 (已有實作)
  → 元件帶有 arguParam 屬性 ("--price", "--side" 等)
         ↓
[5. 使用者填寫表單 → 點擊 Send]
         ↓
[6. SubmitArguForm action handler (新增)]
  → 收集所有 arguParam 元件的值
  → 組裝: "--price 100 --volume 50 --side Buy"
         ↓
[7. submitFn(arguString)]
  → PTCS Core Client.fs appendValue()
  → ActorArguSendRequestDto { rawArgu = arguString }
  → ActorArguCore → fCell2.S 封裝 → Stream 寫入
```

## 5. 與 PTCS Core 的關係 (Dependency)
| 面向 | 是否需修改 PTCS Core | 說明 |
|------|---------------------|------|
| **後端** (Domain, ActorArguCore, Server, FCell2Interop) | ❌ 不需要 | `rawArgu` 管道與 `fCell2.S` 封裝已完善 |
| **前端** (Client.fs) | ✅ 需要 Extension Point | 讓 Dynamic 套件替換 textarea 為自訂 DOM (已在 PTCS Core RFC 規劃) |
| **SDUI 渲染引擎** | ❌ PTCS Core 不涉及 | SDUI 元件渲染完全由 PTCS.Dynamic 的 `DynamicRenderer` 負責 |

## 6. 需要 PTCS.Dynamic 新增或調整的模組
| 模組 | 位置 | 工作內容 |
|------|------|---------|
| `ArguFormSchemaGenerator` | Server/ (新增) | DU Reflection → SDUI JSON 轉換器 |
| `DynamicRenderer.renderNode` | Client/ (調整) | 為輸入元件加入 state 管理 (WebSharper `Var`)，支援值收集 |
| `SubmitArguForm handler` | Client/ (新增) | 遍歷 `arguParam` 元件、組裝 args string、呼叫 submitFn |
| `CustomAppendFormRenderer` 註冊 | Client/ (新增) | 向 PTCS Core 註冊 CanRender + Render callback |
