# Editor schema authority and accepted redraw hypotheses

## 現象

- 真SPAA document `EditorSchemas=[]`，因此不呈現row Edit。
- 200→All低於2秒且無>100ms long task，但chart render sequence由10增加至12。

## 核心假設

1. Schema authority：Edit只在consumer author `EditorSchemas`時成立；Renderer僅依capability render，不能補造schema。
2. Redraw：Interactive client將server accepted的等價viewport再次送入Renderer，Renderer未辨識與local immediate committed viewport相同，造成第二次重畫。

## 驗證方式

- 搜尋schema contract與fixture authoring位置。
- 搜尋viewport command local apply、accepted callback與render-sequence instrumentation。
- 以等價ack與不同authority correction做對照測試。

## Experiment

- `EditorSchemas=[]`符合consumer/document capability ownership；Renderer不得合成，故不列為owner bug。
- `DocumentRevision`與loaded coverage的`CoverageRevision/QueryGeneration`單獨前進時，document presentation、active detail、rows與data未變；舊判斷仍replace shell，驗證假設2。
- 修正後純revision與counter-only case重用shell；active-detail ordinal變更仍replacement。Browser gate等待accepted callback settled後render恰好`+1`，資料替換仍更新五列。
- 結論：假設1、2成立。Release仍須official NuGet readback與Daedalus真SPAA consumer gate。
