# RFC-PTCS-DYNAMIC-0016 Review Feedback

- Reviewer：Daedalus（FSSTL／TradeCore／SPAA／Interactive Extension integration owner）
- Review target：`RFC-PTCS-DYNAMIC-0016.marker-direction-hollow-contract.md`
- 狀態：`ACCEPT DIRECTION / CHANGES REQUIRED BEFORE DEV`
- 日期：2026-09-21

## 1. 結論

接受本 RFC 的主要修正，不需要重開 generic marker 架構：

- `TriangleUp／TriangleDown`與`AboveBar／BelowBar`正交。
- encoder輸出`ta-marker.v2`，decoder保留v1 `arrow`視覺相容。
- `Outline`以`fill="none"`實作真正透明內部。
- wire bucket總量與跨trace visual lane分層限制。
- lane依document trace order → bucket order聚合，避免跨trace重疊。
- runtime維持`sdui-runtime.v2`，以marker item version＋cache schema 3隔離，不新增通用capability framework。
- Position authority、last-good、Y-domain isolation與owner邊界維持不變。

正式DEV前只需修正下列consumer mapping、cross-owner gate與interaction驗收。這些修正不要求新增transport、runtime或domain type。

## 2. Required change：§4.2 DMI mapping必須使用已凍結語意

目前§4.2範例寫成：

- entry：Below／TriangleUp／Solid／green。
- exit：Above／TriangleDown／Outline／black。

雖然文字說明這只是consumer mapping範例，但它與上游已凍結的DMI Backtest視覺語意相反，會讓後續MDC-005／DYN-T-555以錯誤範例產生screenshot與fixture。

請改為完整四列：

| Consumer event | Anchor | Shape | Fill | Color |
| --- | --- | --- | --- | --- |
| long entry | `BelowBar` | `TriangleUp` | `Outline` | `#000000` |
| short entry | `AboveBar` | `TriangleDown` | `Solid` | `#000000` |
| long exit | `AboveBar` | `TriangleDown` | `Solid` | take-profit red／stop-loss green |
| short exit | `BelowBar` | `TriangleUp` | `Solid` | take-profit red／stop-loss green |

補充：

- entry marker使用strategy signal time。
- exit marker使用actual simulated fill time。
- 這仍只是Daedalus consumer acceptance mapping；PTCS不解析long／short、entry／exit或PnL。
- 其他exit reason由consumer傳neutral color，不得被renderer猜成停利／停損。

## 3. Required change：MDC-005／DYN-T-555必須切開owner責任

RFC目前把「consumer handoff與Notebook E2E」列為Aster實作切片，並把desktop/mobile Notebook DMI path列為`DYN-T-555`。正式責任應拆開：

### Aster owner gate

- Contracts／Renderer／PTCS／Interactive Client exact package graph。
- 以PTCS test／browser-demo資料驗`TriangleUp／TriangleDown`、Solid／Outline、tooltip、shared cursor、desktop/mobile renderer parity。
- 提供typed authoring及runtime v2 frame最小範例。
- 發布exact package versions與consumer migration matrix。

### Daedalus integration gate

- 升級SPAA與Interactive Extension package closure。
- 將真`BacktestPresentationEvent`映射成上述四種marker。
- 驗真SPAA active run切換、summary／trades與marker presentation revision。
- 驗ColdFar Notebook `.dib` real path與SPAA identical。

因此建議：

- 將`DYN-T-555`改成PTCS browser-demo／Interactive Client renderer gate，不要求真FSSTL／TradeCore run。
- 另列`Consumer handoff acceptance（Daedalus-owned）`，不作為Aster packages發布前的owner test blocker。
- Aster不得因Daedalus尚未完成cross-repo整合而把owner RFC標為失敗；同樣地，Daedalus不得用Aster browser demo宣稱Notebook真路徑完成。

## 4. Required change：Hollow hit target必須與shared cursor共存

§5.4正確要求visible glyph為`fill="none"`，並允許不可見interaction hit target。但正式contract還需凍結：

- invisible hit target不得產生任何可見填色。
- hit target不得吞掉或阻止row/shared vertical cursor的pointer move。
- marker tooltip與shared cursor可在同一次pointer interaction共存。
- hit target不得加入Y-domain、numeric legend或額外time slot。

請在DYN-T-549或新增browser case驗：pointer穿越Outline triangle內部時，marker tooltip可觸發，shared cursor仍移至相同slot，且cursor transition不重建candle series。

## 5. Release語意修正

「所有active first-party consumers同一release wave exact-pin」應理解為**啟用v2 marker producer前的deployment closure**，不是要求Aster跨repo同步修改SPAA／Interactive Extension。

建議正式寫成：

1. Aster完成并發布新Contracts／Renderer／clients exact versions。
2. Daedalus依handoff升級SPAA／Interactive Extension並完成focused build／runtime v2 smoke。
3. Daedalus確認兩個consumer皆已使用相容bundle後，才讓producer輸出`ta-marker.v2`。

不可用binding redirect、NU1608容忍或local ProjectReference混版；但不要求兩個repo在同一commit或同一owner task內完成。

## 6. 已接受、無須再討論的決策

以下內容可以直接進SD／Tests／DEV：

1. 移除public `Arrow`，新增`TriangleUp／TriangleDown`。
2. v1 `arrow + anchor`相容decode，current encoder只輸出v2。
3. `Outline`case名稱可保留；其normative語意已是transparent hollow。
4. 單一DataRef／position bucket跨anchor合計最多4。
5. 同row／target／position／anchor跨marker traces合計最多4。
6. aggregate lane order為document trace order → bucket array order。
7. cache schema 2視為miss，schema 3承載current marker cache。
8. malformed／over-limit／invalid target回non-recoverable`RejectFrame`；recoverable缺資料仍走`RequestResync`。
9. runtime protocol維持v2，不另造capability negotiation framework。

## 7. 進入DEV條件

完成§2～§5的文件修訂後即可進Aster owner DEV，不需再等待新的架構選擇。實作完成handoff至少提供：

- exact package versions；
- `TaMarker`／trace options正式authoring API；
- v1/v2 codec與cache migration結果；
- triangle direction、true hollow、cross-trace lane與shared cursor browser evidence；
- consumer最小runtime v2 frame範例。

收到handoff後，Daedalus再開始SPAA／Interactive Extension升版與真Backtest marker integration。

