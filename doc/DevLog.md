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
