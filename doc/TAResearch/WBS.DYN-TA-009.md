# DYN-TA-009 Composite row / multi-trace DSL

Status: Completed
Progress: 100%

## Scope

- Additive `TaTraceSpec` contracts與legacy effective-trace projection。
- 同row Candlestick/Line/Histogram rendering與共享time axis/scale。
- PTCS/E2EQ wire與adapter相容。
- 四列2000-bar deterministic與Playwright geometry gate。

## Acceptance

`DYN-TA-T-021`通過；legacy single-series fixtures不需重寫資料語意即可呈現。

## Evidence

- Contracts `TaTraceSpec`、legacy fallback與Renderer composite row已發布；四列正式頁面呈現17條server-computed traces。
- F# Playwright artifact：`G:\PulseTrade.fs.Comm.Log\verification\ptcsHostTaResearchLive\deployed-composite-win10-final-202607130030`。
- Desktop/mobile/reconnect皆通過geometry、console/page-error與last-good recovery gate。
