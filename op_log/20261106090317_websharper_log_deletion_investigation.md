# Operation Log: 20261106090317_websharper_log_deletion_investigation

## 1. 使用者需求 (User Prompt)
> 請只刪除這個已驗證的自動產生檔：E:\deletable\asteer-load05\src\PulseTrade.Comm.Spa.Dynamic.Renderer\websharper.log。這是我獲使用者授權的候選工作區，沒有候選 wsfsc.exe writer，非 reparse。coreutils rm 已失敗。請不要更改 ACL、程式碼、其他路徑、服務或程序；若仍不能刪，請回報精確原因。完成只回報該檔是否已不存在與採用方法。

## 2. 執行操作與調查記錄 (Actions Taken)
1. **屬性與基本權限檢查**：
   - 檢查 `E:\deletable\asteer-load05\src\PulseTrade.Comm.Spa.Dynamic.Renderer\websharper.log` 屬性為 `Archive`（非唯讀、非 Reparse Point）。
   - 嘗試以 PowerShell `Remove-Item`、cmd `del` 進行刪除，均回報 `Access is denied` (Win32 Error 5)。
2. **底層 NT 狀態碼診斷**：
   - 透過 Native API `NtOpenFile` 探測該路徑，回傳 NTSTATUS `0xC0000056` (`STATUS_DELETE_PENDING`)。
   - 此狀態代表先前的刪除操作（如 coreutils `rm`）已成功將該檔案設定了刪除標記（Delete Pending），但在 NTFS 機制下，只要仍有行程持有該檔案的開啟 Handle，檔案目錄項目即不會真正消失，且後續任何開啟/刪除請求均會被拒絕並映射為 `ERROR_ACCESS_DENIED`。
3. **持有 Handle 行程定位**：
   - 透過 `NtQuerySystemInformation` (SystemExtendedHandleInformation) 針對候選編譯行程掃描 File Handle。
   - 定位到常駐編譯器服務 `wsfscservice.exe`（PID `48740`）的 Handle `0x240C` 正持有此檔案：
     - 行程命令列：`"C:\Users\Administrator\.nuget\packages\websharper.fsharp\10.1.7.680\tools\net10.0\win-x64\wsfscservice.exe" E:\deletable\faedilus-load02-close\PulseTrade.Comm.Spa.Dynamic.FsStl.BacktestWorkspace.Client\websharper.log`
     - 持有 Handle：`0x240C` -> `\\?\E:\deletable\asteer-load05\src\PulseTrade.Comm.Spa.Dynamic.Renderer\websharper.log`
4. **遵守約束限制**：
   - 依使用者明確指示：「請不要更改 ACL、程式碼、其他路徑、服務或程序；若仍不能刪，請回報精確原因」。
   - 未終止 PID 48740 (`wsfscservice.exe`)，亦未強制關閉其 Handle 或變更任何服務/程序。

## 3. 結果 (Result)
- 檔案當前狀態：**仍存在**（因 PID 48740 之 Handle 尚未釋放）。
- 精確原因：受到背景常駐 WebSharper F# 編譯服務 `wsfscservice.exe` (PID 48740) 開啟之 Handle `0x240C` 鎖定，檔案處於 NTFS `STATUS_DELETE_PENDING` (0xC0000056) 狀態。在受限不可變更服務或程序的前提下，必須待該行程釋放 Handle 或該行程結束後，NTFS 方會由檔案系統自動卸載該實體項目。
