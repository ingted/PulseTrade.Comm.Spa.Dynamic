# @DYN-TA-012 Loaded-range Viewport / Pointer Cross-row Cursor

Status: Active / 82%

## 背景

2026-07-14正式82截圖顯示：Collapsed摘要中的`last 2000 bars`是requested range，但展開Canvas沒有actual loaded/visible readout；頂端range input其實是cursor index，不是viewport。crosshair固定由state index產生，沒有pointer hit-test。

Evidence：`G:\PulseTrade2.fs\misc\2026-07-14_摘要說有 2000 根，但是看起來就沒有(缺 x軸 scroll bar？)，另外缺了 cross-row verical cursor (直的虛線，滑鼠移動到不同的 x 軸位置會跟著移動).png`。

## Slices

| Slice | Deliverable | Test | Progress | Status |
| --- | --- | --- | ---: | --- |
| DYN-TA-012A | RFC/REQ/SA/SD/WBS/Test correction與假陽性說明 | T-031 | 100% | Done |
| DYN-TA-012B | pure viewport/follow-latest/cursor hit-test model | T-032 | 100% | Done |
| DYN-TA-012C | pure WebSharper navigator、loaded/viewing readout、single shared X axis | T-033 | 100% | Done |
| DYN-TA-012D | pointer move驅動四列shared cursor/readout，零network | T-033 | 100% | Done |
| DYN-TA-012E | delta/reconnect/inline/fullscreen/collapse state regression | T-034 | 70% | Active |
| DYN-TA-012F | exact package、formal 82 F# Playwright、deployment/readback | T-034 | 50% | Blocked：formal terminal completion/memory |

## Done gate

- requested、loaded、visible三者可辨識；不存在「requested 2000等同loaded 2000」文案。
- loaded range可經navigator移動，SVG/DOM只render bounded visible bars。
- pointer在任一row移動時四列crosshair同X且readout同timestamp。
- historical viewport不被delta拉回tail；Reset View回latest/follow-latest。
- inline展開後navigator在rows之前立即可見；不得要求使用者先捲到第四列之後才發現loaded-range控制。
- PTCS inline/fullscreen與collapsed zero-resource gate均通過。

## 2026-07-14 closeout evidence

- Renderer/model 15/15 Pass；loaded/visible viewport、horizontal navigator、follow-latest、pointer hit-test、四列同X crosshair與OHLC/DMI/MACD floating values已實作。
- Dynamic.Ptcs.Client 7/7、Dynamic.Ptcs 7/7 Pass；exact package graph已產生。
- formal 82未關閉：native TA actor有完成計算，但舊RN deployment未將terminal completion投影回新PTCS chat route，formal process memory亦曾約8 GiB。這不回退DYN-TA-012B/C/D，但DYN-TA-012E/F維持未完成。
