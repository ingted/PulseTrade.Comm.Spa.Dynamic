# PulseTrade.Comm.Spa.Dynamic KM

## WebSharper library Release artifact

- `WebSharperProject=Library` 的 package 必須由完整 WebSharper compile 產生；一般 incremental `Release build` 可能因 `CoreCompile` 判定 up-to-date，而留下不含 `WebSharper.meta` 的 DLL。
- 建立不可變 Release candidate 時使用 `Release Rebuild`，且在 pack／交付下游前以 `Assembly.GetManifestResourceNames()` 確認 `WebSharper.meta` 存在。Contracts、Renderer、Interactive.Client 的 exact package graph應使用乾淨restore cache重建一次。
- 下游發生大量 `WS9001 Type not found in JavaScript compilation` 時，先檢查被參考package DLL的WebSharper metadata與同版號cache bytes；不可先改domain type、關閉WebSharper compiler或沿用舊bundle掩蓋artifact問題。
- Interactive.Client package另執行`scripts/verify-interactive-client-package.fsx`，驗bundle manifest、entry、minified entry與Runtime.js。該gate不取代Contracts／Renderer assembly metadata檢查。

關聯：`doc/RFC/RFC-PTCS-DYNAMIC-0027.dark-plot-histogram-style.md`、`doc/Verification.md`的`DYN-VFY-032`。

## Marker plot 與 cursor detail 分工

- 密集事件的長文字不可直接畫在K bar plot glyph旁；plot只保留shape/color與`<title>`，shared cursor detail放固定高度的row-local band，避免遮蔽、縮放碰撞與row跳動。
- Detail band從accepted prepared placements建立slot index，pointer hot path只更新預建DOM；不可重新decode wire、掃描全部markers、重建chart或送provider action。
- Wire/candidate hard limit與presentation budget分離：現行marker bucket最多64，OFI直接顯示4筆並以`+N`表達其餘；不得因DOM budget丟失stable identities。

關聯：`doc/RFC/RFC-PTCS-DYNAMIC-0028.default-viewport-marker-ofi-band.md`、`DYN-VFY-033`。
