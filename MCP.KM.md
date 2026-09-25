# PulseTrade.Comm.Spa.Dynamic KM

## WebSharper library Release artifact

- `WebSharperProject=Library` 的 package 必須由完整 WebSharper compile 產生；一般 incremental `Release build` 可能因 `CoreCompile` 判定 up-to-date，而留下不含 `WebSharper.meta` 的 DLL。
- 建立不可變 Release candidate 時使用 `Release Rebuild`，且在 pack／交付下游前以 `Assembly.GetManifestResourceNames()` 確認 `WebSharper.meta` 存在。Contracts、Renderer、Interactive.Client 的 exact package graph應使用乾淨restore cache重建一次。
- 下游發生大量 `WS9001 Type not found in JavaScript compilation` 時，先檢查被參考package DLL的WebSharper metadata與同版號cache bytes；不可先改domain type、關閉WebSharper compiler或沿用舊bundle掩蓋artifact問題。
- Interactive.Client package另執行`scripts/verify-interactive-client-package.fsx`，驗bundle manifest、entry、minified entry與Runtime.js。該gate不取代Contracts／Renderer assembly metadata檢查。

關聯：`doc/RFC/RFC-PTCS-DYNAMIC-0027.dark-plot-histogram-style.md`、`doc/Verification.md`的`DYN-VFY-032`。
