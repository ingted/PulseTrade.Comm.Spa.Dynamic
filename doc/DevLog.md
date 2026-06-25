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

## 2026-06-26 Dynamic Argu Form invalid/duplicate focused gate

- PTCS canonical verifier `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Scripts\verify.dynamicArguFormDurableProxy.playwright.fsx` now also covers invalid blank append renderer submit and duplicate Dynamic target key idempotency.
- Blank renderer submit shows controlled `Renderer value text is required` status and does not reach DurableProxy; duplicate target key submission keeps a single projected key/card.
- Dynamic `DYN-WBS-404` and `DYN-WBS-405` move to 95; `DYN-WBS-406` remains 70 because split-service RN.Host / ShardingDelivery production proof is still open.
- Updated `doc/WBS.md`, `doc/TEST.md`, and `doc/Traceability.md` to reflect the focused gate; no Dynamic runtime source changed in this slice.

## 2026-06-26 - Add-key renderer aligns variable-length union case tail

- Updated `src/Client/ArguFormRenderer.fs` so `dynamic-argu-add-key` submits `[ actorAddress; "1:duType:<type>"; caseA; caseB; ... ]` instead of the deprecated single `2:unionCases:<caseA>|<caseB>` segment.
- Updated `doc/WBS.md` DYN-WBS-405 to reflect the canonical key contract and its remaining split-service registry replay gap.
- Corresponding PTCS verifier path: `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Scripts\verify.dynamicArguFormDurableProxy.playwright.fsx`.

## 2026-06-26 - Cross-project Dynamic Argu E2E reverified with canonical key tail

- Rebuilt Dynamic through a clean temp WebSharper bundle copy because `src\websharper.log` is still locked in the working source folder; copied the generated DLL and runtime JS back to ignored `src\bin\Release\net10.0` output for verifier use.
- Verification passed:
  - `dotnet run --project tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-build -- --summary` passed 6/6.
  - `dotnet fsi --exec G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Scripts\verify.dynamicArguFormDurableProxy.playwright.fsx` passed with fresh PCSL root and full Dynamic form -> RN DurableProxy -> legacy echo -> PTCS history readback.
- The verifier now checks that union cases are separate key segments and that PTCS readback uses the canonical sorted key list; the old `2:unionCases:<...|...>` segment is no longer used for Dynamic add-key submission.
- Updated `doc/WBS.md` DYN-WBS-406 and `doc/TEST.md`; production split-service RN.Host / ProcSupervisor / ShardingDelivery restart-redelivery / provider proof remains open.

## 2026-06-26 - Dynamic Argu UI E2E contract rerun

- Re-audited the PTCS canonical verifier against the requested UI E2E steps.
- `DYN-WBS-406` now reflects that the browser/runtime UI E2E script itself is at 95: run-scoped PCSL root, DurableProxy actor, legacy echo actor, `actor-dynamic` Playwright page, Dynamic add-key target, common Argu controls, raw Argu send, fCell2 forwarding, `ActorArguTargetReply`, and full target-key readback are covered.
- Rerun evidence: `dotnet fsi --exec G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Scripts\verify.dynamicArguFormDurableProxy.playwright.fsx -- --pcsl-root <TEMP>\ptcs-dynamic-argu-e2e-*` passed and printed `dynamicArguFormDurableProxy.ok`.
- Remaining split-service RN.Host / ProcSupervisor / ShardingDelivery restart-redelivery / production provider proof is an external PTC RN/OPS dependency, not a Dynamic UI script gap.
