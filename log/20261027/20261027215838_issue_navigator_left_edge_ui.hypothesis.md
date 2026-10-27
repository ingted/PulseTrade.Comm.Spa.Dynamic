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

## Experiment：consumer路由與settlement接點

- 0.3.12真formal ui:2帶Earlier/ProviderOpenEarlier、count=max1500與loaded-head雙anchor；Research durable cross-axis local resolver卻一律要求start<end且只能resident內，故拒絕。已交Research owner daedilus補既有directional contract，不在Dynamic猜日期繞過validator。
- Producer另一個已定位的不變量破壞：no-ordinal pending在generation前進後無條件選StartIndex=0，會遺失剛拖到的時間接點。應由old timeline中的released target timestamp對齊new timeline；4500固定resident可evict原pre-drag viewport，所以不能只查舊viewport的起點。
- 加DYN-T-741：old1500..5999→new0..4499、old window3000/80、delta=-3000；預期new1500/80（接點T01500），舊helper找已evicted的T04500而None。增長resident也需同一接點。
- 原H1約5行擴至約30行production，因需同驗frame先到/ACK先到的同一settlement，不新增framework或consumer代碼。
