# PulseTrade.Comm.Spa.Dynamic DevLog

Append-only development log.

## 2026-06-25 RFC-PTCS-DYNAMIC-0002 Dynamic Argu Form formalization

Reviewed source drafts:

- `doc/REQ_Dynamic_Argu_Form.md`
- `doc/RFC_Dynamic_Argu_Form.md`

Added formal RFC:

- `doc/RFC-PTCS-DYNAMIC-0002.dynamic-argu-form-runtime.md`

Synchronized current-state files:

- `doc/REQ.md`
- `doc/SA.md`
- `doc/SD.md`
- `doc/WBS.md`
- `doc/TEST.md`
- `doc/Traceability.md`

Key decisions:

- PTCS.Dynamic owns DU/Argu metadata, FSkynet SDUI form schema, DynamicRenderer form state and SubmitArguForm。
- PTCS core owns append input renderer and add-key dialog seams through `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\doc\RFC-PTC-SPA-0007.dynamic-argu-form-extensions.md`。
- PTC RN owns DurableProxy delivery/sharding/legacy actor adaptation through `G:\PulseTrade.fs\Libs\PulseTrade.Comm\doc\RFC-PTC-0016.resource-node-sharded-function-proxy.md`。
- Dynamic key convention is `actorAddress :: duTypeName :: unionCaseNames`; `unionCaseNames` is the string-list tail, not a joined segment。

Implementation status:

- This is a document/RFC flow slice only。
- Runtime implementation remains planned in `DYN-WBS-402`..`DYN-WBS-406`。

## 2026-06-26 Dynamic Argu Form first runtime E2E

- Added `src/Server/ArguForm.fs` for allowlisted sample Argu form schema and `SubmitArguFormCodec.buildRawArgu`.
- Added `src/Client/ArguFormRenderer.fs` and registered Dynamic add-key / append input renderers through PTCS `PulseTradeRegisterAddKeyRenderer` and `PulseTradeRegisterAppendInputRenderer`.
- Updated Dynamic package reference to PTCS `[0.2.5-beta14]`; the cross-repo Playwright verifier uses the local PTCS repo for the new registry seam.
- Verification: Dynamic tests build passed; `dotnet run --project tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-build -- --summary` passed 6/6. PTCS `Scripts\verify.dynamicArguFormDurableProxy.playwright.fsx` passed browser/runtime E2E with Dynamic form -> PTC RN DurableProxy -> legacy echo actor -> PTCS full target-key readback.
- Remaining: renderer fallback/built-in regression/geometry tests and production split-service RN.Host / ShardingDelivery proof.

## 2026-06-26 Dynamic Argu Form regression expansion

- PTCS canonical verifier `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Scripts\verify.dynamicArguFormDurableProxy.playwright.fsx` now also covers append renderer throw fallback, built-in add-key fallback, built-in `fcell-chat` textarea stream readback, and desktop/mobile Dynamic form geometry.
- Dynamic `DYN-WBS-404` moves to 92 and `DYN-WBS-405` moves to 90; `DYN-WBS-406` remains 70 because split-service RN.Host / ShardingDelivery production proof is still open.
- Updated `doc/WBS.md`, `doc/TEST.md`, and `doc/Traceability.md` to reflect the regression gate and remaining invalid/duplicate-key focused tests.
