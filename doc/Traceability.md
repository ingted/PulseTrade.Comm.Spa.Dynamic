# PulseTrade.Comm.Spa.Dynamic Traceability

本文件索引 PTCS.Dynamic 的 current-state 文件、RFC 與測試入口。正式 RFC 與 WBS 更新時同步本文件。

## Reading Order

1. `README.md`：package purpose and WebSharper bundle packaging note。
2. `doc/REQ.md`：current requirements。
3. `doc/SA.md`：architecture and package boundary。
4. `doc/SD.md`：implementation design。
5. `doc/WBS.md`：work breakdown and cross-project dependency order。
6. `doc/TEST.md`：verification gates。
7. `doc/RFC-PTCS-DYNAMIC-0001.adopt-ptcs-dynamic-extension-points.md`：adoption of PTCS dynamic extension points。
8. `doc/RFC-PTCS-DYNAMIC-0002.dynamic-argu-form-runtime.md`：Dynamic Argu Form formal RFC。
9. `doc/DevLog.md`：append-only milestone log。

## RFC Map

| RFC | Status | Purpose |
| --- | --- | --- |
| `RFC-PTCS-DYNAMIC-0001` | Proposed / first implementation exists | Adopt PTCS `RFC-PTC-SPA-0006` dynamic extension points: manifest, script asset, custom shape, message renderer。 |
| `RFC-PTCS-DYNAMIC-0002` | Draft / Review | Dynamic-owned Argu metadata, SDUI form renderer, SubmitArguForm, add-key renderer, and cross-project PTCS/PTC RN integration schedule。 |

## Cross-Project References

| Project | File | Relevance |
| --- | --- | --- |
| PTCS | `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\doc\RFC-PTC-SPA-0007.dynamic-argu-form-extensions.md` | PTCS core seam for append input renderer and add-key dialog renderer。 |
| PTC RN | `G:\PulseTrade.fs\Libs\PulseTrade.Comm\doc\RFC-PTC-0016.resource-node-sharded-function-proxy.md` | RN DurableProxy consumes `ActorArguTargetCommand.RawArgu` and adapts to legacy actor/service。 |
| PTC WBS | `G:\PulseTrade.fs\Libs\PulseTrade.Comm\doc\WBS.md` rows `PTC3-063`..`PTC3-067` | RN/RN.Host production gates and final Dynamic -> RN E2E。 |

## Verification Map

| Test ID | WBS | Expected verifier |
| --- | --- | --- |
| `DYN-T-401` | `DYN-WBS-401` | Document chain review / sensitive scan / encoding scan。 |
| `DYN-T-402` | `DYN-WBS-402` | `tests/PulseTrade.Comm.Spa.Dynamic.Tests` metadata/schema tests。 |
| `DYN-T-403` | `DYN-WBS-403` | `tests/PulseTrade.Comm.Spa.Dynamic.Tests` SubmitArguForm codec tests。 |
| `DYN-T-404` | `DYN-WBS-404` | PTCS `Scripts/verify.dynamicArguFormDurableProxy.playwright.fsx` append input renderer browser path。 |
| `DYN-T-405` | `DYN-WBS-405` | PTCS `Scripts/verify.dynamicArguFormDurableProxy.playwright.fsx` add-key/readback path。 |
| `DYN-T-406` | `DYN-WBS-406` | PTCS `Scripts/verify.dynamicArguFormDurableProxy.playwright.fsx` first Dynamic form -> RN DurableProxy E2E；production split-service proof still open。 |
