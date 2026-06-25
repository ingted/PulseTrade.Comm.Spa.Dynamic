# [RFC] PulseTrade.Comm.Spa.Dynamic: Argu-style Form Renderer — SDUI-Driven Implementation

## 1. 提案摘要 (Summary)
採用純 SDUI 架構實作 Argu Form 功能。**後端**負責透過 .NET Reflection 將 DU Type + Union Case 轉換為 `schema: "fskynet-sdui"` 的 JSON；**前端**完全不需要理解 F# DU 結構，只需用現有的 `DynamicRenderer` 渲染表單元件，並新增一個 `SubmitArguForm` action handler 來收集欄位值、組裝 args string 並送出。

### 與前一版 RFC 的差異
| | 前一版 (Client-Driven) | 本版 (Server-Driven/SDUI) |
|---|---|---|
| DU 解析 | 前端 `generateFormFor` | **後端** `ArguFormSchemaGenerator` (Reflection) |
| 表單定義 | 前端硬編碼元件映射 | 後端推送 SDUI JSON |
| 新增 Union Case | 可能需要前端改版 | 只需後端 DU 改了，SDUI JSON 自動變 |
| 複用現有程式碼 | 需全新 Form Generator 模組 | 複用 `DynamicRenderer.renderNode` |

## 2. 後端設計 (Server-Side)

### 2.1. ArguFormSchemaGenerator 模組 (新增)
位置：`PulseTrade.Comm.Spa.Dynamic/src/Server/ArguFormSchemaGenerator.fs`

此模組透過 .NET Reflection 讀取 DU Type 的指定 Union Case，將其欄位結構轉換為 SDUI JSON。

```fsharp
module ArguFormSchemaGenerator =
    open System
    open System.Text.Json
    open Microsoft.FSharp.Reflection

    /// F# DU 欄位型別 → SDUI 元件型別的映射
    let private fieldToSduiComponent (fieldName: string) (fieldType: Type) (arguParam: string) =
        if fieldType = typeof<bool> then
            {| ``type`` = "SelectBox"
               id = "f-" + fieldName.ToLowerInvariant()
               arguParam = arguParam
               options = [| "true"; "false" |]
               multiple = false |}
        elif fieldType.IsEnum then
            let enumNames = Enum.GetNames(fieldType)
            {| ``type`` = "Dropdown"
               id = "f-" + fieldName.ToLowerInvariant()
               arguParam = arguParam
               options = enumNames |}
        elif fieldType = typeof<int> || fieldType = typeof<float> || fieldType = typeof<decimal> then
            {| ``type`` = "TextInput"
               id = "f-" + fieldName.ToLowerInvariant()
               arguParam = arguParam
               placeholder = fieldType.Name |}
        else // string, etc.
            {| ``type`` = "TextInput"
               id = "f-" + fieldName.ToLowerInvariant()
               arguParam = arguParam
               placeholder = fieldType.Name |}

    /// 為指定的 DU Type + Union Case 產生 SDUI JSON
    let generateFormSchema (duType: Type) (unionCaseName: string) : string =
        let cases = FSharpType.GetUnionCases(duType)
        match cases |> Array.tryFind (fun c -> c.Name = unionCaseName) with
        | None -> failwithf "Union case '%s' not found in type '%s'" unionCaseName duType.FullName
        | Some unionCase ->
            let fields = unionCase.GetFields()
            
            // 將每個 field 轉為 SDUI Row: [Label + Input]
            let rows =
                fields |> Array.map (fun field ->
                    let arguParam = "--" + field.Name.ToLowerInvariant().Replace("_", "-")
                    let inputComponent = fieldToSduiComponent field.Name field.PropertyType arguParam
                    {| ``type`` = "Row"
                       children = [|
                           {| ``type`` = "Label"; text = field.Name + ":" |} :> obj
                           inputComponent :> obj
                       |] |})
            
            // 收集所有 input 的 id
            let inputIds = fields |> Array.map (fun f -> "f-" + f.Name.ToLowerInvariant())
            
            let sdui = {|
                schema = "fskynet-sdui"
                formMode = "argu-form"
                sdui = [|
                    yield {| ``type`` = "Heading"; text = unionCaseName |} :> obj
                    yield! rows |> Array.map (fun r -> r :> obj)
                    yield {| ``type`` = "Button"
                             id = "btn-submit"
                             text = "Send"
                             onClick = {| action = "SubmitArguForm"
                                          includeStateOf = inputIds |} |} :> obj
                |]
            |}
            
            JsonSerializer.Serialize(sdui, JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase))
```

### 2.2. Actor 端 GetFormSchema 訊息處理
後端 Actor 需要回應 `GetFormSchema` 請求：
```fsharp
type GetFormSchemaRequest =
    { DuTypeName: string    // e.g. "Trade.OrderCommands"
      UnionCaseName: string // e.g. "LimitOrder"
    }

// 在 Actor 的 Receive 中：
| :? GetFormSchemaRequest as req ->
    let duType = resolveType req.DuTypeName  // 從已載入的 Assembly 中找到 Type
    let sduiJson = ArguFormSchemaGenerator.generateFormSchema duType req.UnionCaseName
    Sender.Tell(ActorArguTargetReply { Value = fCell2.S sduiJson; Direction = None; Tags = Some ["form-schema"] })
```

### 2.3. Schema 快取機制
同一個 `[DU Type, Union Case]` 的 Schema 不會變（除非後端重新部署）。前端應快取已取得的 Schema：
```
Map<(duType * unionCase), SDUI JSON string>
```
只在第一次選取 Key 時請求，後續直接使用快取。

## 3. 前端設計 (Client-Side)

### 3.1. DynamicRenderer 輸入元件 State 管理 (調整)
現有的 `TextInput`、`Dropdown` 等元件已渲染為正確的 HTML，但缺少 **值收集機制**。需要為 `formMode = "argu-form"` 的 SDUI 渲染加入 state registry：

```fsharp
/// 表單狀態 registry：id → 目前值
let private formState = Dictionary<string, string>()

// TextInput 渲染時加入 oninput 監聽：
| "TextInput" ->
    let idStr = JS.Inline<string>("$0.id || ''", obj)
    let arguParam = JS.Inline<string>("$0.arguParam || ''", obj)
    // ... existing attrs ...
    V "input" [
        // ... existing attrs ...
        on.input (fun el _ ->
            let value = (el :?> HTMLInputElement).Value
            if not (System.String.IsNullOrEmpty idStr) then
                formState.[idStr] <- value
        )
    ]

// Dropdown 渲染時加入 onchange 監聽：
| "Dropdown" | "SelectBox" ->
    // ... existing rendering ...
    E "select" [
        // ... existing attrs ...
        on.change (fun el _ ->
            let value = (el :?> HTMLSelectElement).Value
            if not (System.String.IsNullOrEmpty idStr) then
                formState.[idStr] <- value
        )
    ] optionDocs
```

### 3.2. SubmitArguForm Action Handler (新增)
當 Button 的 `onClick.action` 為 `"SubmitArguForm"` 時：

```fsharp
| "Button" ->
    let btnText = JS.Inline<string>("$0.text || 'Button'", obj)
    let onClickObj = JS.Inline<obj>("$0.onClick || null", obj)
    
    let clickHandler _ _ =
        if onClickObj <> null then
            let action = JS.Inline<string>("$0.action || ''", onClickObj)
            if action = "SubmitArguForm" then
                let includeIds = JS.Inline<string[]>("$0.includeStateOf || []", onClickObj)
                
                // 收集所有指定 id 的值，組裝 argu string
                let args =
                    includeIds
                    |> Array.choose (fun id ->
                        match formState.TryGetValue(id) with
                        | true, value when not (System.String.IsNullOrWhiteSpace value) ->
                            // 找到該元件的 arguParam
                            let arguParam = findArguParamForId id  // 從 SDUI JSON 中查找
                            Some (arguParam + " " + value)
                        | _ -> None
                    )
                    |> String.concat " "
                
                // 呼叫 PTCS Core Extension Point 提供的 submitFn
                currentSubmitFn args
    
    E "button" [ ...; on.click clickHandler ] [ text btnText ]
```

### 3.3. CustomAppendFormRenderer 註冊 (新增)
在 Extension 初始化時向 PTCS Core 註冊：

```fsharp
PulseTradeRegisterAppendFormRenderer {
    CanRender = fun key ->
        // Key 長度為 3：[Actor Address, DU Type, Union Case]
        key.Keys.Length = 3 && key.Keys.[1].Contains(".")
        
    Render = fun key submitFn ->
        let actorAddr = key.Keys.[0]
        let duType = key.Keys.[1]
        let unionCase = key.Keys.[2]
        
        // 儲存 submitFn 供 SubmitArguForm handler 使用
        currentSubmitFn <- submitFn
        
        // 檢查快取
        match schemaCache.TryGetValue((duType, unionCase)) with
        | true, cachedJson ->
            // 直接渲染 SDUI 表單
            renderSduiForm cachedJson
        | _ ->
            // 向後端請求 Schema，拿到後渲染並快取
            requestFormSchema actorAddr duType unionCase (fun sduiJson ->
                schemaCache.[(duType, unionCase)] <- sduiJson
                renderSduiForm sduiJson
            )
            // 先顯示 Loading
            renderLoadingPlaceholder ()
}
```

## 4. 發送流程完整路徑 (End-to-End)
```
[1] 選取 Key ["akka.tcp://.../actor", "Trade.OrderCommands", "LimitOrder"]
         ↓
[2] CanRender → true → Render 呼叫
         ↓
[3] 向後端 Actor 請求 GetFormSchema
         ↓
[4] 後端 ArguFormSchemaGenerator (Reflection)
    讀取 LimitOrder(price: float, volume: int, side: Side)
    輸出 SDUI JSON:
      { schema: "fskynet-sdui", formMode: "argu-form",
        sdui: [ Heading, Row[Label+TextInput(--price)], Row[Label+TextInput(--volume)],
                Row[Label+Dropdown(--side, [Buy,Sell])], Button(SubmitArguForm) ] }
         ↓
[5] 前端 DynamicRenderer.renderNode 渲染 (複用現有程式碼)
    textarea 區域被替換為 form-style UI
         ↓
[6] 使用者填寫: price=100, volume=50, side=Buy
         ↓
[7] 點擊 Send → SubmitArguForm handler
    收集 arguParam 值 → "--price 100 --volume 50 --side Buy"
         ↓
[8] submitFn("--price 100 --volume 50 --side Buy")
    → PTCS Core Client.fs → rawArgu
    → ActorArguCore → fCell2.S → Stream
```

## 5. 與 PTCS Core 的關係
| 層級 | 修改需求 | 說明 |
|------|---------|------|
| `Domain.fs` | ❌ | `Keys: string list` 已支援任意組合 |
| `ActorArguCore.fs` | ❌ | `rawArgu` 直接接收字串 |
| `FCell2Interop.fs` | ❌ | `fCell2.S` 封裝已內建 |
| `Server.fs` | ❌ | WebSocket handler 只轉發 `rawArgu` |
| `Client.fs` | ✅ (已規劃) | Extension Point — 已在 PTCS Core RFC 規劃 |
| **SDUI 輸入元件** | ❌ **PTCS Core 不涉及** | 全部由 PTCS.Dynamic 的 DynamicRenderer 處理 |

## 6. 預期效益
1. **純 SDUI 架構**: 前端不需要理解 DU 結構，表單完全由後端驅動。
2. **零前端改版**: 新增 Union Case 時只需改後端 DU 定義，SDUI JSON 自動更新。
3. **複用現有基礎設施**: TextInput、Dropdown、Button 等元件已實作於 `DynamicRenderer`，只需補充 state 管理。
4. **與 Canvas 一致的架構**: 表單渲染和 Canvas 渲染都走 `schema: "fskynet-sdui"` → `DynamicRenderer` 路徑。
