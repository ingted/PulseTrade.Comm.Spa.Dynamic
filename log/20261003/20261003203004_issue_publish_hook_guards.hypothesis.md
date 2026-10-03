# Issue: AfterPack 忽略發布控制旗標

觀測：umbrella PostBuildR/PostBuildD 只判 Configuration；專屬條件位於 XML comment。其餘七個 actual opt-in targets 沒有 PublishNuGetAfterPack=false veto。
根因假設：MSBuild 不會把註解當 Condition，global false 也不會自動傳成各 target 的否決條件；CLI 安全旗標與 VS 默認行為因此可能仍進 publisher。
最小區分實驗：從八個真 fsproj 抽取發布 properties/targets，在 import-free MSBuild fixture 中把全部 target body 替換 WriteLinesToFile marker。實際 MSBuild 評估原 Condition，禁止執行/保留任何 Exec/import/using task。先用明傳 per-flag false/global false 跑 RED，再只修 gate 跑同一 oracle。
預估：九個 Condition 修改共約 9–12 非型別行；可重用 verifier 約 140–200 非型別行。若超過兩倍先記 Experiment。
不變：七個既有 VS auto-true defaults、所有版本/PackageReference、Release/Debug/OS 限制、Renderer/PCSL WIP。umbrella 未有 VS default，本片不新增。
測試界線：unit 證明 MSBuild target gating；不讀 key、不發布、不建立真 nupkg、不證明遠端發布/runtime。
