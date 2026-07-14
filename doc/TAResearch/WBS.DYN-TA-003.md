# @DYN-TA-003 WebSharper TA renderer

Status: Active / 94%

## Deliverables

- TA workspace/toolbar/chart-stack/status using pure WebSharper。
- Candlestick/Volume/SMA/DMI/ADX/MACD/Heikin-Ashi rows and shared viewport。
- Local interaction, bounded working set, controlled errors and last-good-state behavior。
- F# Playwright operation/geometry tests for desktop/mobile。

## UX acceptance

Primary chart dominates first viewport; controls are compact and predictable; Add Row editor collapses; no overlaps; status is visible but does not displace the chart; interaction never unexpectedly resets focus/viewport。

## 2026-07-11 milestone

已完成：

- packable `PulseTrade.Comm.Spa.Dynamic.Renderer 0.1.0-alpha1`，exact依賴Contracts alpha4，無PTCS/fCell2/PTMD/SQL dependency。
- pure WebSharper F# workspace、compact query toolbar、local pan/zoom/reset-view、row visibility、remote Reset Canvas/Change Query/Add Row typed callbacks。
- Candlestick、Volume、SMA、DMI、ADX、MACD、Heikin-Ashi七種row與bounded visible window；row-scoped SVG identity。
- Add Row editor open/cancel/submit後收合；desktop 1440x900與mobile 390x844無水平溢出或controls overlap。
- exact-package model tests 7/7；Contracts alpha4 tests 7/7；F# Playwright真browser操作與geometry通過。截圖為ignored test artifact：`artifacts/ta-renderer-playwright/desktop.png`與`mobile.png`。

尚未完成：

- shared crosshair、cursor values、時間軸label與研究級indicator legends/多線DMI/MACD細節。
- in-flight/disabled/error/stale/backfill/last-good visual states與frame patch後viewport/focus不變browser證據。
- host transient channel/poll scheduler不屬Renderer alpha1，追蹤於DYN-TA-004/006。

## 2026-07-12 alpha3 milestone

已完成：

- `PulseTrade.Comm.Spa.Dynamic.Renderer 0.1.0-alpha3`新增shared cursor slider、七列同步crosshair、可讀的HTML first/middle/last time axis與同一visible index的OHLC/indicator cursor values。
- freshness/status projection保留Live/Delayed/Stale/Backfill/Unavailable typed state、watermark、quality與recoverable last-good error；PollInFlight/PausedForResync期間remote query與Add Row submit disabled，local view操作不受影響。
- F# Playwright在desktop/mobile操作cursor、in-flight、stale與Ready recovery；stale時price body count保持不變，證明last-good Canvas未被錯誤狀態清掉。mobile截圖人工檢視後把受SVG非等比縮放影響的文字軸移到HTML grid，B1/B25/B48均清楚可讀。
- exact-package renderer tests 10/10、Ptcs.Client tests 2/2、BrowserDemo build及Playwright皆通過；ignored evidence為`artifacts/ta-renderer-playwright/desktop.png`與`mobile.png`。

尚未完成：

- DMI/MACD研究級多線、legends與indicator-specific cursor detail。
- authoritative patch後focus/viewport保持、Delayed/Backfill/Unavailable各狀態的browser visual matrix。
- 真PTCS transient poll/reconnect/resync仍由DYN-TA-004/006追蹤。

## 2026-07-14 correction

`shared cursor slider`只證明同一visible index的model projection，不是RFC要求的pointer cross-row cursor；renderer也沒有代表完整loaded range的viewport navigator。這兩項回歸改由`DYN-TA-012`完成，DYN-TA-003維持Active，不再以原T-006宣稱完整browser interaction已完成。
