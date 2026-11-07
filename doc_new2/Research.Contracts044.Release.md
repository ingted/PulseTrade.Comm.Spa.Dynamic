# Research 的 Contracts 0.1.44 發布來源

2026-11-07，owner：agent.daedilus。這是既有 Research 0.1.79 相依套件的來源 checkpoint，沒有新增 contract 語意。

- 基底：Dynamic canonical `9a02352b03f714453829e33a3ffc9eb57c57d0bc`。
- 匯入 canonical 的 Contracts project、RuntimeTypes 與對應 TAResearchContractsTests；備份及逐檔 hash 在 `misc/backups/20261107T132114_contracts044_import/manifest.json`。
- 0.1.44 的既有容量調整是 canvas 16 rows / 128 total traces；其餘各項 bounds 保持不變。回歸包含 public11 的 67 traces、16/128 邊界與超限拒絕。
- SDK library-packs 原 0.1.44 nupkg 內 DLL 與 canonical Release DLL 的 SHA-256 均為 `1D0AFC38C364E1EE16FCEBBE26992259A9EAA0089464B6F9BCEF13996440EDCC`。
- canonical portable PDB 的 11 份本專案來源 hash 已逐一比對，11/11 相符。不是以檔案時間推測來源。
- 原 DLL 已由 Research 0.1.79 的 Renderer 333、Client 59 與實際 Chrome 流程使用；此項紀錄不把完整歷史回測標成通過。
- 發布使用同一份既有 DLL／WebSharper artifacts，先驗來源及 package metadata，再由專案 opt-in Release Pack hook 複製 library-packs 並發布；source 與 destination package hash 必須一致。

完整歷史回測依使用者決定待驗。Source checkpoint、package publication 與整個 repository closeout 是不同證據層級。
