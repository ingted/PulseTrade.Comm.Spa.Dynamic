# @DYN-TA-003 WebSharper TA renderer

Status: Planned

## Deliverables

- TA workspace/toolbar/chart-stack/status using pure WebSharper。
- Candlestick/Volume/SMA/DMI/ADX/MACD/Heikin-Ashi rows and shared viewport。
- Local interaction, bounded working set, controlled errors and last-good-state behavior。
- F# Playwright operation/geometry tests for desktop/mobile。

## UX acceptance

Primary chart dominates first viewport; controls are compact and predictable; Add Row editor collapses; no overlaps; status is visible but does not displace the chart; interaction never unexpectedly resets focus/viewport。
