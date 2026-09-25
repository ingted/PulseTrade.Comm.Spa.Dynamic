# RFC-PTCS-DYNAMIC-0027：Dark Plot Surface 與 Histogram Polarity Style

- ID：`RFC-PTCS-DYNAMIC-0027`
- 狀態：Owner Implemented / Consumer Gate Pending
- Owner：PTCS Dynamic Contracts／Renderer（Aster）
- Consumer：PulseTrade.Comm.Spa.Dynamic.Interactive.Extension／SPAA（Daedalus）
- 關聯：`RFC-PTCS-DYNAMIC-0024`、`DYN-WBS-567`、`DYN-T-610..614`、Daedalus message `msg-fsi-591297270dc141f68bf42f96afd40064`

## 背景

真 SPAA 已驗證 fixed CSS-pixel stroke，但 overview 初始 selection 貼 SVG 邊界時仍會裁掉外側半個 2px stroke。產品同時需要 generic dark plot surface，以及不依 series name 推論的 Histogram 正負值配色。

## 目標

1. Overview 初始／拖曳後可見左右邊界皆精確 2 CSS px；selection、range 與 transparent hit target 語意不變。
2. 由 PTCS document 啟用 generic dark plot presentation，涵蓋 candle、TA、overview、grid、cursor、axis 與 legend。
3. Histogram 以 typed options 指定 positive／negative color；未指定時維持既有單色 `TaTraceSpec.Color`。
4. 以 exact NuGet graph 交付 Contracts／Renderer／Interactive.Client，真 SPAA GREEN 前不公開發布。

## 非目標

- 不在 SPAA 注入私有 CSS。
- 不由 trace id、label、MACD 等 domain 名稱推論配色。
- 不改 overview selection/range、hit target 寬度或 drag 行為。
- 不改 FSSTL parser、TA 計算或資料 provider。

## 情境

1. 文件未設定 theme：Renderer 保持既有 light presentation。
2. `DefaultView["ta.plotSurface.theme"] = "dark"`：所有 plot surfaces 使用單一 dark palette；workspace 控制區不被強迫改為 dark。
3. Histogram 未提供 polarity options：所有 bars 沿用 `trace.Color`。
4. Histogram 同時提供 `histogram.positiveColor` 與 `histogram.negativeColor`：`value >= 0` 使用 positive、`value < 0` 使用 negative。
5. Histogram polarity 只提供一個欄位、格式錯誤，或非 Histogram trace 使用該 option：candidate validation 拒絕，不靜默猜測。

## 決策

### Document presentation

```fsharp
[<RequireQualifiedAccess>]
type TaPlotSurfaceTheme =
    | Light
    | Dark

type TaPlotSurfacePresentation =
    { Theme: TaPlotSurfaceTheme }

TaPlotSurfacePresentationCodec.ThemeKey = "ta.plotSurface.theme"
```

Codec 讀寫 `TaWorkspaceDocument.DefaultView`，wire record 不增欄位。Renderer 將 presentation 一次解析為集中 palette，再傳入 chart／overview rendering path。

### Histogram style

```fsharp
type TaHistogramTraceOptions =
    { PositiveColor: string
      NegativeColor: string }

TaHistogramTraceOptionsCodec.PositiveColorKey = "histogram.positiveColor"
TaHistogramTraceOptionsCodec.NegativeColorKey = "histogram.negativeColor"
```

Renderer 使用一個 stable trace group，內含 positive／negative 兩個 batched path；不為每一 bar 建 DOM node。legacy 單色仍走相同 bounded geometry path。

### Overview edge stroke

可見 line 位於左／右 SVG boundary 時，僅以 CSS pixel inward transform 將 2px centerline 移入 1px；其 authored selection X、透明 hit rect、move region、drag math 全部保持原值。非邊界位置不位移。

## Package Graph

- `PulseTrade.Comm.Spa.Dynamic.Contracts 0.1.29`
- `PulseTrade.Comm.Spa.Dynamic.Renderer 0.1.71` exact `Contracts [0.1.29]`
- `PulseTrade.Comm.Spa.Dynamic.Interactive.Client 0.1.62` exact `Contracts [0.1.29]`、`Renderer [0.1.71]`

Daedalus DIExt 目前只有 direct exact references `Contracts` 與 `Interactive.Client`，不新增 Renderer direct reference。

## 失敗與相容性

- theme 缺值等同 light；未知／錯誤值 structured reject。
- histogram polarity 兩鍵皆缺等同 legacy；partial／invalid structured reject。
- same-topology replacement 只更新 path/palette，不重建 provider state。
- dark theme 不降低 durability、ACK、projection commit 或 cache identity 保證。

## 驗收

1. Contracts round-trip、legacy fallback、partial/invalid/kind-mismatch tests。
2. Renderer unit 驗 palette 與正負 geometry 分離。
3. F# Playwright 驗 initial-edge／drag visible stroke 皆 2px，transparent hit target width/行為不變。
4. F# Playwright 驗 candle/composite/overview surfaces 為 black，grid/cursor/axis/legend 有非黑且可讀的 computed colors。
5. 4,000-slot owner performance 無新增 >100ms acceptance phase，console/page error 為零。
6. Daedalus 真 SPAA 升 exact graph、驗 explicit FSSTL colors 與 dark product screenshot；GREEN 後才 public push/readback。

## Owner Evidence（2026-09-26）

- Contracts／Renderer／Interactive.Client focused suites：`44/44`、`48/48`、`12/12`。
- 三包以Release `Rebuild`產生完整WebSharper metadata，並以獨立restore cache驗local exact package graph；Interactive bundle nupkg verifier通過。
- F# Playwright 4,000 bars：initial edge與moved handle皆2 CSS px，hit rect維持transparent／width 8；dark candle／TA／overview、axis、legend、cursor與正紅／負綠Histogram通過。
- Release exact graph正式update phases max：five-candle `72.92ms`、scenario `83.35ms`、All `40.34ms`、marker `59.85ms`、document `24.19ms`、progressive `42.93ms`；全部`over100=0`。
- Playwright MCP桌面檢視及console gate通過，warnings/errors為0。
- Release nupkg SHA-256：Contracts `7A59C0A671E5E995B88D1BCD23F074D2ADC0280D6DFD9EC09C354208D51FFC9E`、Renderer `2D68D4CAF1234634090B5417B2ED7FEEED0B26ABF18DAE613130B8699AB5A98F`、Interactive.Client `3B5997F91F32150949402A8E57D9DDDA1012A4F4D98C5206D1F19319CD3A417B`。
- Release candidate已由COMM `msg-fsi-24ad39d9518e4d689548939dbe299014`更正交付Daedalus；前一則Debug provisional hashes作廢，尚未public push。
