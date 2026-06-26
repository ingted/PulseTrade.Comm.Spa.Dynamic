# FSkynet SDUI DSL (JSON 語意) 說明文件

本文檔詳細說明了 `PulseTrade.Comm.Spa` 中 Server-Driven UI (SDUI) 所使用的 JSON DSL 結構、元件型別，以及前後端的通訊與互動機制。

## 0. 2026-06-26 DSL vNext：Canvas 與 Form Input 共用模型

`RFC-PTCS-DYNAMIC-0003.unified-sdui-form-dsl-roadmap.md` 起，`fskynet-sdui` 的 canonical artifact 是通用 `SduiDocument`，不是 Canvas-only JSON，也不是 Argu-only form schema。

同一份 DSL 應可由不同 renderer 解讀：

- `Canvas` surface：展開在畫布上，以展示、查閱、局部 UI 操作為主，例如 toggle、mode switch、local sort、open panel。
- `FormInput` surface：展開在 append input area，以輸入、validation、backend-linked options、submit 為主。

Argu / DU 不是 renderer 的直接輸入。它是 adapter input：

```text
IArgParserTemplate / DU metadata + canonical Argu arg string
  -> PTCS.Dynamic backend parser / adapter
  -> ArguToFormDsl adapter
  -> SduiDocument(surface = FormInput)
  -> Dynamic FormInput renderer
```

Canvas DSL 與 Form Input DSL 共用：

- document id / schema / version；
- `data` / `dataRef`；
- node tree；
- input binding；
- local / remote action；
- declared option provider；
- render hints。

Form Input 額外要求 stateful submit contract；Canvas 額外要求 bounded interactive view lifecycle。兩者不應各自發明不相容 JSON shape。

### Target key convention

Dynamic target key 是 PTCS `AppendPageKey.Keys : string list` 的使用規範。第一個 item 一律是 actor address。

Direct DSL target：

```json
["/user/durable-proxy", "ptcs.host.demo.pfcf.form"]
```

DU / Argu target：

```json
[
  "/user/durable-proxy",
  "PulseTrade.Comm.Spa.Host.DynamicArguDemo.PFCF_AKKA_CMD",
  "--pfcfedx trivial --pfcfgtcconf OIInf TAIFEX FillSquareCombine OrderByTXDT CathayBKTaifexFill --to 90000 --parentchilds 2 5 --bba F008 000 9910357 --decimalquote 6 0 --round 6 4 2 datarange --referencedatemode ModeAccountingDate --between 20251104 20251104 --calibrate2curdayiflargerthancurday"
]
```

第三段是該 DU/template 的 canonical Argu arg string。它是資料，不是 shell command。PTCS.Dynamic 後端以 registered parser 驗證與解析，再根據 parse result 與原 token order 產生 FormInput DSL。舊版 `unionCaseNames` key list tail、`1:duType:`、`2:unionCases:` 都只是歷史相容資訊，不是新 canonical。

若 host 沒有呼叫 `hub.useDynamicSdui(...)`，PTCS core 不處理第二/第三段，只取第一段 actor address 走 built-in actor-argu/raw textarea path。若 Dynamic extension 存在，Dynamic 先查 direct DSL registry，再查 DU/template registry；兩者都不存在或 arg string parse 失敗時顯示 controlled error，不假裝 fallback 成可用 textarea。

## 1. 核心概念與結構

`fskynet-sdui` 是一種宣告式的 JSON DSL，其結構借鏡了 Python Dash 的理念。後端 Actor 在回傳結果時，會帶有一組 Payload，其基本結構如下：

```json
{
  "schema": "fskynet-sdui",
  "data": {
    "orderData": [ ... ],
    "historyData": [ ... ]
  },
  "sdui": "[ ... JSON String of Components ... ]"
}
```

- **`schema`**：必須固定為 `"fskynet-sdui"`，前端依據此標記決定將 Payload 交由 SDUI 渲染引擎處理。
- **`data`**：存放由後端 fCell 轉換而來的原始資料，以 key-value 形式儲存，供 SDUI 元件綁定 (透過 `dataRef`)。
- **`sdui`**：legacy compatibility 欄位。一個包含元件陣列的 JSON 字串 (或直接為 JSON 陣列)，描述了動態畫布 (Canvas) 的佈局與互動。

### 1.1 vNext document format

新文件應優先使用 `document` shape。`sdui` raw array/string 保留為舊 payload 相容層。

```json
{
  "schema": "fskynet-sdui",
  "version": "0.3",
  "documentId": "ptcs.host.demo.pfcf.form",
  "surface": "FormInput",
  "data": {
    "marketOptions": [
      { "value": "Domestic", "label": "Domestic" },
      { "value": "Foreign", "label": "Foreign" }
    ]
  },
  "nodes": [
    {
      "type": "Section",
      "id": "case-simple-action",
      "title": "SimpleAction",
      "children": [
        {
          "type": "Input",
          "id": "simple-action-name",
          "label": "action_name",
          "kind": "Text",
          "binding": "SimpleAction.action_name"
        },
        {
          "type": "Button",
          "id": "send-simple-action",
          "label": "Send",
          "actionId": "submit-simple-action"
        }
      ]
    }
  ],
  "actions": {
    "submit-simple-action": {
      "type": "SubmitForm",
      "targetBindingId": "ptcs.actor-argu.raw",
      "includeStateOf": ["SimpleAction.action_name"],
      "adapter": {
        "type": "ArguRaw",
        "duTypeName": "PulseTrade.Comm.Spa.Host.DynamicArguDemo.PFCF_AKKA_CMD",
        "unionCaseName": "SimpleAction"
      }
    }
  },
  "bindings": [
    {
      "id": "SimpleAction.action_name",
      "path": "$.SimpleAction.action_name",
      "required": true
    }
  ]
}
```

欄位語意：

| Field | Required | Meaning |
| --- | --- | --- |
| `schema` | yes | 固定 `"fskynet-sdui"`。 |
| `version` | yes | DSL schema version；breaking change 需升版。 |
| `documentId` | yes | 可被 target key `[ actorAddress; formDslId ]` 指涉的 registry id。 |
| `surface` | yes | `"Canvas"` 或 `"FormInput"`。 |
| `data` | no | 靜態資料、初始 options 或 readonly render data。 |
| `nodes` | yes | UI node tree。 |
| `actions` | no | 以 `actionId` 索引的 declarative actions。 |
| `bindings` | no | input / selection / local state binding metadata。 |

### 1.2 Surface semantics

`surface = "Canvas"`：

- 預設 readonly；
- 可使用 local actions，例如 toggle、mode switch、local sort、open panel；
- 若需要 remote action，必須透過 registered action provider 或 PTCS command path，不可由 DSL 指定 arbitrary URL。

`surface = "FormInput"`：

- 預設 stateful；
- input / select / list / tuple controls 必須有 binding；
- submit action 必須回到 PTCS append / actor-argu callback；
- backend-linked options 必須透過 registered provider；
- 同一個 DU target 的 requested union cases 必須以多個 `Section` 同屏呈現，不以 primary dropdown 隱藏其他 case。

### 1.3 Direct DSL target and DU target

Direct DSL target 指向已註冊 document：

```json
["/user/durable-proxy", "ptcs.host.demo.pfcf.form"]
```

DU target 指向 registered adapter，並提供 canonical default/template command：

```json
[
  "/user/durable-proxy",
  "PulseTrade.Comm.Spa.Host.DynamicArguDemo.PFCF_AKKA_CMD",
  "--pfcfedx trivial --pfcfgtcconf OIInf TAIFEX FillSquareCombine OrderByTXDT CathayBKTaifexFill --to 90000 --parentchilds 2 5 --bba F008 000 9910357 --decimalquote 6 0 --round 6 4 2 datarange --referencedatemode ModeAccountingDate --between 20251104 20251104 --calibrate2curdayiflargerthancurday"
]
```

DU target 解析成功後，PTCS.Dynamic backend 使用 registered `IArgParserTemplate` parser parse 第三段 arg string，adapter 產生一份 `surface = "FormInput"` 的 `SduiDocument`。Renderer 不知道也不需要知道原始 DU type 如何反射。

### 1.4 Alias binding

Argu DU union case / field / option 通常是英文 canonical name，但 FormInput 需要中文或 domain display label。Alias 是 display metadata，不可改變 submit semantics。

```json
{
  "templateKey": "PulseTrade.Comm.Spa.Host.DynamicArguDemo.PFCF_AKKA_CMD",
  "caseAliases": {
    "PFCFEDX": "電子檔",
    "PFCFGTCCONF": "交易設定",
    "DataRange": "日期區間"
  },
  "fieldAliases": {
    "BBA.期貨商": "期貨商",
    "BBA.分公司": "分公司",
    "BBA.母帳帳號": "母帳帳號"
  },
  "optionAliases": {
    "ReferenceDateMode.ModeAccountingDate": "過帳日",
    "ReferenceDateMode.ModeTradingDate": "交易日"
  }
}
```

Alias precedence：

1. Host / template registration alias map。
2. Attribute / resource metadata if supported later。
3. Canonical case / field / option name。

Target key 不攜帶 alias pair；target key 只描述 actor、template 與 canonical default command。

### 1.5 Subcommand ordering

`ParseResults<'T>` / Argu `ArgumentType.SubCommand` 必須以 tail subcommand group 進入 FormInput DSL。Raw command rebuild 時：

- root-level args 先輸出；
- subcommand token 例如 `datarange` 放在 root args 後；
- subcommand args 例如 `--referencedatemode ... --between ...` 放在 subcommand token 後。

Example expected output：

```text
--pfcfedx trivial --pfcfgtcconf OIInf TAIFEX FillSquareCombine OrderByTXDT CathayBKTaifexFill --to 90000 --parentchilds 2 5 --bba F008 000 9910357 --decimalquote 6 0 --round 6 4 2 datarange --referencedatemode ModeAccountingDate --between 20251104 20251104 --calibrate2curdayiflargerthancurday
```

---

## 2. 動作與互動定義 (Actions)

SDUI 將業務邏輯保留在後端，前端的互動行為透過宣告式的 `Action` 定義。目前支援的 Action 分為兩種：

### A. Remote Action (遠端動作)
將使用者在前端互動的狀態，送回給後端的 Actor 處理。
- **`SendCommand`**：
  - `action`: `"SendCommand"`
  - `command`: 要執行的指令名稱字串。
  - `includeStateOf`: 字串陣列，指定要一起打包送回後端的元件狀態 (如 `"grid1.selectedRow"`, `"input1.value"`)。

### B. Local Action (本地動作)
前端純粹的 UI 狀態改變，不消耗 WebSocket 頻寬與後端運算資源。
- **`LocalSort`**：針對表格資料的本地排序。
  - `action`: `"LocalSort"`
  - `targetGridId`: 目標 DataGrid 的 ID。
  - `column`: 要排序的欄位名稱。
  - `direction`: 排序方向 (e.g. `"Asc"`, `"Desc"`)。
- **`LocalAggregate`**：針對表格資料的本地聚合運算。
  - `action`: `"LocalAggregate"`
  - `targetGridId`: 目標 DataGrid 的 ID。
  - `column`: 作為聚合依據的欄位名稱。
  - `functionType`: 聚合函數 (e.g. `"Sum"`, `"Average"`)。
- **`OpenCanvas`**：開啟另一個動態畫布。
  - `action`: `"OpenCanvas"`
  - `moduleId`: 目標模組 ID。
  - `targetId`: 目標頁面或資源 ID。

### C. Form Action (表單動作)

Form Input surface 使用的 action 必須可被 PTCS append / actor-argu callback 收斂。

- **`SubmitForm`**：
  - `type`: `"SubmitForm"`
  - `targetBindingId`: submit 的 logical target，例如 `"ptcs.actor-argu.raw"`。
  - `includeStateOf`: 要收集的 binding id 清單。
  - `adapter`: optional adapter metadata；例如 `{ "type": "ArguRaw", "duTypeName": "...", "unionCaseName": "..." }`。
  - 結果：產生 `AppendInputSubmission.ValueText`，由 PTCS core 走既有 append / actor-argu path。

- **`QueryOptions`**：
  - `type`: `"QueryOptions"`
  - `providerId`: host / extension 已註冊 provider id。
  - `dependsOn`: 依賴的 binding id 清單。
  - `outputBinding`: 寫回的 option binding id。
  - 限制：不可帶 arbitrary URL、headers、tokens 或 script。

- **`ValidateForm`**：
  - `type`: `"ValidateForm"`
  - `includeStateOf`: 需驗證的 binding id 清單。
  - `rules`: declarative local validation rules。
  - 若需 remote validation，必須透過 registered provider。

---

## 3. 核心與進階元件清單 (Components)

每一個 SDUI 元件在 JSON 中都是一個包含 `"type"` 屬性的物件。

### 核心元件 (Core Components)

1. **Heading (標題)**
   - `type`: `"Heading"`
   - `text`: 標題顯示文字。
   - 範例：`{"type": "Heading", "text": "即時交易深度"}`

2. **Button (按鈕)**
   - `type`: `"Button"`
   - `id`: 元件唯一識別碼。
   - `text`: 按鈕文字。
   - `onClick`: 按下按鈕時執行的 `Action`。
   - 範例：`{"type": "Button", "id": "btn1", "text": "市價買入", "onClick": {"action": "SendCommand", "command": "buy", "includeStateOf": []}}`

3. **Dropdown (下拉選單)**
   - `type`: `"Dropdown"`
   - `id`: 元件唯一識別碼。
   - `options`: 選項字串陣列。
   - `onSelect`: 選擇時觸發的 `Action`。

4. **DataGrid (資料網格)**
   - `type`: `"DataGrid"`
   - `id`: 元件唯一識別碼。
   - `dataRef`: 對應到 `data` 區塊中的 key 名稱 (綁定資料源)。
   - `features`: `{ "AllowSorting": true, "AllowAggregation": true, "Pagination": false }` 等網格特性。

5. **RealtimeChart (即時圖表)**
   - `type`: `"RealtimeChart"`
   - `id`: 元件唯一識別碼。
   - `dataRef`: 綁定的資料源 key。
   - `indicators`: 技術指標清單 (e.g. `["MA", "MACD"]`)。

### 進階元件 (Advanced Components - 基於 tui-chart 概念擴展)

6. **AppLoader (載入器)**
   - `type`: `"AppLoader"`
   - `id`: 元件唯一識別碼。
   - `text`: 載入中提示文字。

7. **AutoComplete (自動完成輸入框)**
   - `type`: `"AutoComplete"`
   - `id`: 元件唯一識別碼。
   - `dataRef`: 提供自動完成建議的資料源 key。
   - `onInput`: 輸入時觸發的 `Action` (可選)。

8. **ColorPicker (顏色選擇器)**
   - `type`: `"ColorPicker"`
   - `id`: 元件唯一識別碼。
   - `defaultColor`: 預設顏色 hex 代碼。
   - `onChange`: 選擇變更時觸發的 `Action`。

9. **ContextMenu (右鍵選單)**
   - `type`: `"ContextMenu"`
   - `id`: 元件唯一識別碼。
   - `targetId`: 綁定右鍵選單的目標元件 ID。
   - `menuItems`: 選單項目陣列，每個項目包含 `text` 與對應的 `Action`。

10. **DatePicker (日期選擇器)**
    - `type`: `"DatePicker"`
    - `id`: 元件唯一識別碼。
    - `format`: 日期格式 (e.g. `"yyyy-MM-dd"`)。
    - `onChange`: 選擇變更時觸發的 `Action`。

11. **Pagination (分頁控制)**
    - `type`: `"Pagination"`
    - `id`: 元件唯一識別碼。
    - `targetGridId`: 受控的 DataGrid ID。
    - `pageSize`: 每頁顯示筆數。

12. **Rolling (滾動播報元件)**
    - `type`: `"Rolling"`
    - `id`: 元件唯一識別碼。
    - `dataRef`: 綁定的資料源 key (通常是一維字串陣列)。
    - `direction`: 滾動方向 (`"up"`, `"down"`, `"left"`, `"right"`)。

13. **SelectBox (進階選擇框)**
    - `type`: `"SelectBox"`
    - `id`: 元件唯一識別碼。
    - `options`: 選項陣列 (支援 key-value pair)。
    - `multiple`: 是否支援多選 (boolean)。
    - `onChange`: 變更時觸發的 `Action`。

14. **TimePicker (時間選擇器)**
    - `type`: `"TimePicker"`
    - `id`: 元件唯一識別碼。
    - `format`: 時間格式 (e.g. `"HH:mm"`)。
    - `onChange`: 變更時觸發的 `Action`。

15. **Tree (樹狀視圖)**
    - `type`: `"Tree"`
    - `id`: 元件唯一識別碼。
    - `dataRef`: 樹狀資料源 key。
    - `onNodeClick`: 點擊節點時觸發的 `Action`。

### Form Input Components

Form Input components 使用相同 node tree，只是 renderer 將其放在 append input area。

1. **Section**
   - `type`: `"Section"`
   - `id`: section id。
   - `title`: 顯示標題，Argu adapter 會用 union case name。
   - `children`: inputs/buttons。

2. **Input**
   - `type`: `"Input"`
   - `id`: input id。
   - `label`: 顯示 label。
   - `kind`: `"Text"`、`"Number"`、`"Bool"`、`"Date"`、`"Time"`、`"Color"`。
   - `binding`: state binding id。

3. **Select**
   - `type`: `"Select"`
   - `id`: select id。
   - `label`: 顯示 label。
   - `options`: `StaticOptions`、`QueryOptions` 或 `StreamOptions`。
   - `binding`: state binding id。

4. **Tuple**
   - `type`: `"Tuple"`
   - `id`: tuple group id。
   - `items`: ordered child inputs。
   - renderer 必須以順序編號呈現，例如 `1.`、`2.`。

5. **ListInput**
   - `type`: `"ListInput"`
   - `id`: list group id。
   - `item`: item input node。
   - renderer 必須提供 add item control，並維持 bounded layout。

6. **Submit Button**
   - `type`: `"Button"`
   - `actionId`: 指向 `SubmitForm`。
   - 每個 union case section 應有自己的 submit button，避免使用者切換 case 才能送出。

### Option Source

```json
{
  "type": "Select",
  "id": "bank-edx-market",
  "label": "Bank market",
  "binding": "BankEdx.market",
  "options": {
    "type": "QueryOptions",
    "providerId": "ptcs.host.demo.bank-edx-market",
    "dependsOn": ["BankEdx.output"]
  }
}
```

`QueryOptions` 只描述 provider id 與 dependency；provider 執行由 PTCS/host registered callback 負責。

---

## 4. WebSharper 實作守則

- **純 Native WebSharper**：上述所有元件的渲染與生命週期管理，均使用 F# 搭配 WebSharper 實作，不依賴任何第三方 JavaScript 函式庫或 raw JS snippet。
- **型別安全解析**：SDUI JSON 在 WebSharper 端將使用嚴格定義的 Record/Union 型別進行反序列化 (Deserialization)。
- **技術分析考量**：在實作技術分析圖表 (`RealtimeChart`) 等與資料運算相關的功能時，必須參考既有的 `PulseTrade.MarketData.Analytics.fs` 架構以維持體系一致性。
- **Renderer / Adapter 分離**：Renderer 只吃 `SduiDocument`；Argu / DU reflection、`IArgParserTemplate` 與 union case 選擇都在 adapter / registry 層完成。
- **Controlled error**：schema、document id、provider id、DU type、union case 或 binding 無法解析時，renderer 顯示 controlled error；不得 silent fallback 成看似可用的 textarea。
- **No arbitrary execution**：DSL 不可含 JavaScript、shell command、absolute URL、secret-bearing headers 或任意 code block。
