# @DYN-TA-016 Editor shell / capability poll / reset regression

- RFC：`doc/RFC/RFC-PTCS-DYNAMIC-0012.ta-editor-poll-reset-stability.md`
- Host companion：`PTCSH-TA-016`
- Status：Done
- Progress：100%

## Slices

| Slice | Deliverable | Test | Progress | Status |
| --- | --- | --- | ---: | --- |
| DYN-TA-016A | RFC/REQ/SA/SD/WBS/Test文件鏈 | T-051 | 100% | Done |
| DYN-TA-016B | capability-gated lifecycle poll | T-052 | 100% | Done |
| DYN-TA-016C | stable editor shell與dynamic remote-action state | T-053 | 100% | Done |
| DYN-TA-016D | multi-row Reset formal browser regression | T-054 | 100% | Done |
| DYN-TA-016E | exact packages、正式82部署與closeout | T-055 | 100% | Done |

## Closure

不得只驗editor仍visible；須證明focus不因poll遺失、static Document不發poll、live chart仍更新，且DMI/MACD多列刪除後Reset完整恢復原始ordered document。

完成證據：Renderer `0.1.0-alpha27` package test `20/20`、Ptcs.Client `0.1.0-alpha8-win57` lifecycle test `9/9`；正式F# Playwright gate跨兩次live poll保留Add Row select focus，連續刪除DMI與兩列MACD後一次Reset恢復`price,dmi,macd-short,macd-long`及17 traces。正式artifact：`G:\PulseTrade.fs\Libs\PulseTrade.Comm\.pcsl\verify.ptcsHostTaEditorPollReset.alpha55.final.20260715153500\artifacts`。
