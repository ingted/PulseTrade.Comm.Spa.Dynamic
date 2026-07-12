# DYN-TA-010 Browser delta wire v2

Status: Completed
Progress: 100%

## Scope

- Full initial/document/gap frame與stable-document delta frame。
- keyed changed-point upsert、rolling remove-before、status delta。
- client base/revision validation、deterministic merge、2000-point retention、resync。
- 量測poll payload不隨完整2000-bar history線性重送。

## Acceptance

`DYN-TA-T-022`通過；fresh reconnect先full，後續poll為delta。

## Evidence

- `ta-browser.v2`以`baseDataRevision`驗證delta；client測試涵蓋append、rolling prefix trim與revision mismatch fail-closed。
- Bootstrap/initial delta每series保留最新200 points，server RuntimeState與Host query仍保留完整bounded history；後續stable poll為0-point delta。
- Dynamic.Ptcs 7/7、Ptcs.Client 7/7；正式Playwright觀測revision `978 -> 979`後連續零點delta，WebSocket channel未中斷。
