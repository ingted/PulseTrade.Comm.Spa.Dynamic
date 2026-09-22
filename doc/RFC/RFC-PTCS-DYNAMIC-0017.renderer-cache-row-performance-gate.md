# RFC-PTCS-DYNAMIC-0017 Renderer cache／row／performance gate

- ID：RFC-PTCS-DYNAMIC-0017
- 狀態：Accepted / Implemented
- 日期：2026-09-22
- 上游關聯：COMM `rfc-tradecore-0026-ptcs-renderer-gate-20260922`
- 工項：DYN-TA-020
- 測試：DYN-TA-T-079..081、DYN-VFY-021 revision 3

## 背景

Daedalus 的 RFC-TRADECORE-0026 真 MDCQ gate 使用3,820個base bars與七列TA時，暴露三個Dynamic owner缺口：browser cache rehydrate保留current session identity與authoritative `DataRevision`，但Renderer只比較identity/revision，因而不重畫新Data；row legend以全域trace index查reader，DMI/MACD列讀到price/SMA值；48切All會建立大量逐bar SVG primitive，且non-base candle cursor對每個timestamp反向掃描完整序列，形成O(n²)阻塞。

## 目標

1. cache candidate替換Data object時立即重畫，但不偽造server revision。
2. legend reader由`rowId + local trace index`唯一定位。
3. source、cursor與Y-domain保留完整資料語意；presentation SVG有界，non-base cursor projection近線性。
4. exact-package gate證明3,820 bars、七列、All/48、pointer與cache路徑可操作。

## 非目標

- 不修改FSSTL、MDCQ provider、calendar或TA計算。
- 不讓cache成為authoritative data source，也不以提高`DataRevision`強迫重畫。
- 不刪source points、降低working-set contract或改marker/cursor語意換取速度。

## 決策

### Cache repaint identity

Renderer的data change判斷為：runtime identity不同、authoritative revision不同，或Data object reference不同。`RuntimeCacheProjection.tryRehydrate`可保留current identity/revision並替換validated cached Data；reference change是presentation invalidation，不是domain revision。

```fsharp
let runtimeDataChanged left right =
    left.Identity <> right.Identity
    || left.DataRevision <> right.DataRevision
    || not (obj.ReferenceEquals(left.Data, right.Data))
```

### Row-qualified legend

每個legend node帶`data-ta-row-value-row-id`與row-local trace index；reader registry為`Map<rowId, reader array>`。查詢缺row或index時顯示`Undef`，不可回退到其他row。

### Bounded presentation, complete semantics

完整projected arrays仍供Y-domain、cursor reader與legend使用。SVG presentation在超過1,000個base slots時，以time bucket壓成最多1,000個candles；line/histogram每bucket保留min/max，最多1,000點。candlestick以固定八條batched path表示normal/projected、up/down、wick/body，不建立per-bar DOM/Var。

non-base candle cursor reader不再對每個timestamp呼叫`Array.tryFindBack`。source由尾至頭透過range assignment與next-unassigned path compression一次投影：先保留「最後一筆finalized containing/matching point勝出」，缺值再以`AvailableAtUtc`建立latest finalized as-of suffix。語意以舊逐點函式的等價測試鎖定。

## 影響

- 變更：Renderer、Interactive.Client bundle、Ptcs.Client exact dependency與focused tests。
- 不變：Contracts `0.1.13`、wire/cache schema、RuntimeReducer、provider authority。
- Consumer須採Renderer `0.1.33`與Interactive.Client `0.1.26`；Ptcs.Client owner graph為`0.1.49`。

## 驗收

1. Renderer focused suite涵蓋cache Data reference invalidation、row-qualified legend及linear projection語意等價。
2. 3,820 bars BrowserDemo：48→All <2,000ms、batched candle paths <=32、pointer transition p95 <50ms、無console/page error或crash。
3. Interactive lifecycle、browser IndexedDB cache、package manifest與Ptcs.Client suites通過。
4. Daedalus採新版exact packages後，以`Rfc0026.Spaa.Backtest.playwright.fsx`驗cache hit bars、row legends與真provider gate；此consumer部署不由Dynamic owner冒充完成。

## 回退

Consumer可退回Renderer `0.1.30`／Interactive.Client `0.1.25`，但會重新出現本RFC三項缺口。不得只退Interactive bundle而保留新版server graph；package依賴須維持exact一致。
