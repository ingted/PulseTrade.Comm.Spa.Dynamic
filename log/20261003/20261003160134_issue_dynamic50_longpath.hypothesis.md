# Dynamic50 compiler 長路徑 hypothesis

現象：precheckpoint-r1 fullcompiler在MSBuild copy階段MSB3030，10個WebSharper.UI.Templating.Runtime script source missing，exit1/12.227s；原始stdout/stderr/result和865manifest保留於TEMP/aster-ptcs-package-0.2.48/dynamic-wallet50-preparation-20261003/build-precheckpoint-r1。

假設 H1：新增巢狀preparation prefix使compiler asset路徑超過Win32 260字元邊界。獨立檢查10個缺失path皆261–278字元；皆在原865manifest，canonical存在且raw SHA一致，build後capture缺失。不是缺NuGet package或canonical來源，禁止補假檔、改ACL/停用compiler。

最小修正：只把新build artifact搬到同BASE之fresh dyn50-pre-r2 / dyn50-final-r1；不移動/刪除舊artifact、不變canonical source。harness prefixguard同步且加入captured destination length<260 preflight。估10非型別行，consumer root另用dyn50-consumers-r1縮短。

驗證：同865 current cleanblob/rawbytes重新capture、三fullcompiler、所有captured/canonical hashes與Renderer不變；固定commit後重新fullbuild/pack/32unit/LiveDemo7assets。未執行不稱PASS。external HEAD534bf6b，現在僅本片metadata/currentdocs/log加原Renderer/PCSL WIP；source checkpoint待gate。

## Experiment result
H1獲支持：r1與short r2的865input path/hash完全一致；max captured path 278 -> 230，三precheckpoint與固定commit fullcompiler全PASS。修正限新TEMP root＋lengthpreflight，未改canonical/ACL。Client135/Ptcs60/Dynamic29本機archive/feed與32unit/LiveDemo7assets均PASS；public由upstream另驗。
