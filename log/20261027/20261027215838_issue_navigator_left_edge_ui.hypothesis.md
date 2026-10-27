# Left-edge release hypothesis

- Requirement：user指定將可視框由位置1拉至既有左界，應以既有loaded head接續向左擴1500，不能閃回位置2。Chrome已關閉並授權重開。
- Baseline：4b46434 / Renderer0.3.12。final DLL focused10/10、provider-open1/1；true consumer原為0.3.11，daedilus正裝0.3.12驗證。
- H1：`navigatorGestureBoundaryDirection`對有GLOBAL的navigator要求pointerX<0；selection觸左界但pointer在內仍回None，與最新操作需求不符。最小失敗例：4500 loaded、start1198/count80、move delta=-1198、pointerX=10/width1200，預期Earlier。
- H2：non-resident-authority且有GLOBAL時，release把任何boundary改走commitLocalDraft；即使H1修正仍不保證向左擴充。必須以true-host action/authority確認，禁止把1K ordinal當30K ordinal。
- Settlement不變量：request capacity1500與viewport width分開；loaded head作接點；資料不重複/遺失；保持query/revision fence；中間拖曳與resize不誤觸發fetch。
- 預估：H1 production約5行；H2依true-host結果決定，未據此假設修改consumer。

```text
move release reaches loaded left edge and delta < 0 -> Earlier
dispatch with the authority of the visible navigator axis
wait for accepted prepared data, preserve viewport width and temporal anchor
```
