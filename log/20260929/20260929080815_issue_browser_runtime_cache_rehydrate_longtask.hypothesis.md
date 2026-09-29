# BrowserRuntimeCache rehydrate long-task hypothesis

## 現象

IndexedDB cache hit可正確還原SPAA accepted projection，但`rehydrateLatestPhased`仍產生155ms browser long task；總read/core耗時約2944ms。

## 核心假設

1. phased data-item decode的固定batch未限制單次wall-clock成本，大JSON item或後續轉換可在一個callback內超過100ms。
2. header／record validation或rehydrated state assembly仍有未切片的O(payload)同步路徑。

## 區分實驗

- 建立接近3.8MB、80 frames的BrowserCache fixture，分別量測IndexedDB read、data-item decode、rehydrate/validate與publish callback。
- 以`PerformanceObserver` longtask加phase timing，確認最長slice；不得只用總耗時推論。
- 修正後驗證supersede在任一phase都不發布candidate，且成功只發布一次完整state。
