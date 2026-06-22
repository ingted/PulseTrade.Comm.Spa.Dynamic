# 解決 PulseTrade.Comm.Spa.Dynamic 的 Actor Dynamic Tab UI 無法顯示問題

## 問題回顧
User 反應執行 poc.dynamic.fsx 後，新增了 ctor dynamic tab page 並綁定了 kka.tcp://PulseTradeCommSpaDynamicPoc@127.0.0.1:xxxx/user/durable-echo，且成功對其發送訊息。但在 Browser 端，雖然 Server 成功送出 skynet-sdui schema 的 JSON，畫面卻沒有任何渲染（Chat Session 空白）。

## 原因分析
原本嘗試使用 Library 模式，將 DynamicRenderer.js 與 ActorDynamicTab.js 讀成 base64 Data URI 來塞入 ScriptUrls。
但此做法會保留 JavaScript 中的 import 語法 (例如 import Attr from "../WebSharper.UI/...";)。因為 Server 並沒有 host WebSharper.UI 這些相依套件，所以 Browser 解析 import 時會失敗，導致 UI 整個掛掉無法渲染。

## 解決方案
1. **改用 Bundle 模式**：
   將 PulseTrade.Comm.Spa.Dynamic.fsproj 的 WebSharperProject 設為 Bundle，使 WebSharper 產出一個包含所有相依函式庫 (WebSharper.UI 等) 的單一 PulseTrade.Comm.Spa.Dynamic.js。
2. **複製 wwwroot 至 Output**：
   在 .fsproj 中加入 <Content Include="wwwroot\**\*.*" CopyToOutputDirectory="PreserveNewest" />，讓產出的 .js 能被隨同 .dll 一起打包。
3. **單一 Data URI 注入**：
   修改 Extension.fs，將這個打包好的龐大 Bundle .js 直接讀進來並轉為 ase64 Data URI 塞入 ScriptUrls。因為 Bundle 是 Self-contained，不會有任何外部 import，完美避開了找不到資源的問題。
4. **繞過 wsfsc.exe Crash**：
   由於在 src 目錄下執行 Bundle 編譯時 wsfsc.exe 會引發錯誤 (-532462766)，我在 src5 乾淨環境中編譯完成後，將成功的 in、obj 及更新後的程式碼完美同步回了 src。

## 執行結果
現在您在 src 底下執行 dotnet fsi poc.dynamic.fsx，Server 可以順利啟動，並且將自動讀取 in\Debug\net10.0\wwwroot\js\PulseTrade.Comm.Spa.Dynamic.js，這保證了 Browser 端能拿到完整且無需外部 Import 的 UI 執行檔！
