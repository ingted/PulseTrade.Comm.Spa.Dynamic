# @DYN-TA-015 Full runtime export / draft query / slot cursor

- RFC：`doc/RFC/RFC-PTCS-DYNAMIC-0011.ta-export-draft-cursor-defaults.md`
- Status：Done
- Progress：100%

## Slices

| Slice | Deliverable | Test | Progress | Status |
| --- | --- | --- | ---: | --- |
| DYN-TA-015A | RFC/current-state document chain | T-045 | 100% | Done |
| DYN-TA-015B | full-snapshot export lifecycle and typed browser download | T-046/T-047 | 100% | Done |
| DYN-TA-015C | non-reactive query draft and Apply boundary | T-048 | 100% | Done |
| DYN-TA-015D | shared slot-center cursor/line/K-bar geometry | T-049 | 100% | Done |
| DYN-TA-015E | exact packages and formal 82 closure | T-050 | 100% | Done |

## Closure

下載檔需以F# Playwright實際接收、parse並驗2000-bar完整資料；不能只驗source marker、Blob呼叫或compact Document。Apply與cursor亦需真DOM/geometry evidence。

完成證據：exact graph為Renderer `0.1.0-alpha25`、Dynamic.Ptcs `0.1.0-alpha7-win41`、Ptcs.Client `0.1.0-alpha8-win55`。正式F# Playwright artifact `G:\PulseTrade.fs\Libs\PulseTrade.Comm\.pcsl\verify.ptcsHostTaResearchExport.alpha53.final.20260715150500`已實際下載並parse 2000筆timeline、OHLCV與全部indicator series，且通過collapsed one-shot lifecycle、Apply唯一action與slot-center geometry。
