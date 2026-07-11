# @DYN-TA-003 WebSharper TA renderer

Status: Active / 72%

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
