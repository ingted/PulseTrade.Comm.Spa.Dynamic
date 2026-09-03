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

## 2026-06-26 - RFC-PTCS-DYNAMIC-0003 Unified SDUI / Form DSL roadmap

- Added `doc/RFC-PTCS-DYNAMIC-0003.unified-sdui-form-dsl-roadmap.md` to correct the product direction after the Dynamic Argu Form UI/design churn.
- Updated `doc/SDUI_DSL_zh-Hant.md`, `doc/REQ.md`, `doc/SA.md`, `doc/SD.md`, `doc/WBS.md`, `doc/TEST.md`, and `doc/Traceability.md`.
- Decision: PTCS.Dynamic owns a common SDUI DSL and renderers; Argu / DU support is an adapter into Form DSL, not renderer input.
- Decision: PTCS.Host owns the demo DU and live deployment wiring; `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\doc\example DU.txt` is Big5/cp950 source material for host-local demo subset.
- New target keys: direct DSL target `[ actorAddress; formDslId ]`; DU target `[ actorAddress; duTypeName; unionCase1; unionCase2; ... ]`.

## 2026-06-26 - PTCS.Dynamic package first slice / beta2

- Added package-level SDUI FormInput DTOs and metadata: `SduiFormDocument`, `SduiFormNode`, `SduiFormAction`, `SduiFormBinding`, and `DynamicArguMetadata`.
- Added `DynamicTargetKey.tryResolve` for direct DSL target `[actorAddress; formDslId]` and DU target `[actorAddress; duTypeName; unionCaseNames...]`; unknown discriminator, unknown union case, and invalid direct-target tail fail with controlled errors.
- Added `ClientRawArguCodec` and aligned it with server `SubmitArguFormCodec`, so frontend-produced raw Argu strings are testable from F# without handwritten JavaScript.
- Tests now include a PFCF_AKKA_CMD fixture based on `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\doc\example DU.txt` and verify raw arg output for `SimpleAction`, `Entrust`, `PFCFGTC`, `BBA`, `Cooperative`, `ParentChilds`, `FractionalQuote`, `GenByColMeta`, and `TableName`.
- Verification: `dotnet build .\src\PulseTrade.Comm.Spa.Dynamic.fsproj --no-restore -v minimal` passed after clearing the recurring WebSharper compiler-helper log lock; warnings were WS9002, NU5123 long paths, and missing readme. `dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj --no-restore` passed 9/9.
- Package version bumped from `0.1.3-beta1` to `0.1.3-beta2` for NuGet push.

## 2026-06-26 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta2 NuGet pushed

- Release build generated `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\src\bin\Release\PulseTrade.Comm.Spa.Dynamic.0.1.3-beta2.nupkg`.
- Nuspec check confirmed dependency `PulseTrade.Comm.Spa` version `0.2.5-beta15`.
- Copied the package to `C:\Program Files\dotnet\sdk\10.0.301\FSharp\library-packs`.
- NuGet push returned `Created` / `Your package was pushed`; API key value was not logged.

## 2026-06-26 - RFC-PTCS-DYNAMIC-0003 arg-string target realignment

- Revised `doc/RFC-PTCS-DYNAMIC-0003.unified-sdui-form-dsl-roadmap.md` after architecture review: DU/template target key is now `[ actorAddress; duTypeOrTemplateKey; canonicalArgString ]`, not `[ actorAddress; duTypeName; unionCase1; ... ]`.
- Decision: PTCS.Dynamic backend uses the registered Argu parser to parse the canonical arg string, then generates FormInput DSL from parse result plus original token order. PTCS frontend only renders backend-resolved DSL.
- Decision: alias binding belongs to Dynamic/Host metadata; case/field/option aliases are display labels only and do not enter raw Argu command semantics.
- Decision: without `hub.useDynamicSdui(...)`, PTCS core ignores Dynamic segments and uses only `keys[0]` actor address through built-in actor-argu/raw textarea path.
- Added WBS/Test traceability for parser-backed target resolution, alias binding, `ParseResults<PFCF_AKKA_CMD_DATA_RANGE>` subcommand ordering, and Playwright E2E for the PFCF `datarange` command.
- Updated current-state docs: `doc/REQ.md`, `doc/SA.md`, `doc/SD.md`, `doc/SDUI_DSL_zh-Hant.md`, `doc/WBS.md`, `doc/TEST.md`, and `doc/Traceability.md`.

## 2026-06-26 - PTCS.Dynamic arg-string backend package slice / beta3

- Implemented parser-backed Dynamic target support in `src/Server/ArguForm.fs`: `DynamicArguTemplateRegistration`, `DynamicArgStringTarget`, parsed target DTOs, target-key validation for `[ actorAddress; duTypeOrTemplateKey; canonicalArgString ]`, and controlled parser failure.
- Added `DynamicArguAliasBinding` and `DynamicFormDsl` helpers so case / field / option aliases enter FormInput DSL display labels without changing canonical raw Argu values.
- Added `SduiFormNode.DefaultValues` and backend projection from canonical arg string into FormInput DSL defaults for root cases, list values, and named tuple values.
- Added `ParseResults<PFCF_AKKA_CMD_DATA_RANGE>` scanning and raw rebuild support; verified the exact expected command keeps `datarange` after root args and before subcommand args.
- Updated PFCF package fixture tests based on `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\doc\example DU.txt`, including `PFCFEDX of mode`, additional `PFCF_GTC_CONF` values, and `Calibrate2CurDayIfLargerThanCurDay`.
- Verification: `dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-restore -- --summary --no-spinner` passed 13/13. Warnings were existing WebSharper `WS9002` and NuGet `NU5123` long path warnings.
- Package version bumped from `0.1.3-beta2` to `0.1.3-beta3` for NuGet push.

## 2026-06-26 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta3 NuGet pushed

- Release build generated `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\src\bin\Release\PulseTrade.Comm.Spa.Dynamic.0.1.3-beta3.nupkg`.
- Nuspec check confirmed dependencies: `PulseTrade.Comm.Spa 0.2.5-beta15`, `FAkka.Argu 10.1.301`, `FAkka.FCell2 10.1.301`, `FSharp.Core 10.1.301`, and `WebSharper.FSharp 10.1.5.674`.
- Copied the package to `C:\Program Files\dotnet\sdk\10.0.301\FSharp\library-packs`.
- NuGet push returned `Created` / `Your package was pushed`; API key value was not logged.

## 2026-06-26 - PTCS.Dynamic backend-resolved FormInput DSL / beta4

- Implemented backend resolver endpoint support in `src/Server/ArguForm.fs`: `DynamicArguResolveTargetRequest`, `DynamicArguResolveTargetReply`, and `DynamicArguResolveEndpoint.handle`.
- `CommHub.useDynamicSdui(...)` can now register `/client-extensions/dynamic/argu/resolve-target` through the PTCS client-extension JSON handler seam while preserving the metadata-only overload.
- The browser append renderer now treats `[ actorAddress; duTypeOrTemplateKey; canonicalArgString ]` as a backend-resolved target: it POSTs the full key list to the Dynamic resolver, renders the returned FormInput DSL, and uses server-projected defaults.
- Added package test `DYN-T-511`, verifying the PFCF data-range canonical arg string resolves to FormInput DSL through registered parser metadata. Verification: `dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-restore -- --summary --no-spinner` passed 14/14 after clearing a stale WebSharper `wsfsc.exe` process.
- Package version bumped from `0.1.3-beta3` to `0.1.3-beta4`; nuspec dependency points to `PulseTrade.Comm.Spa 0.2.5-beta16`. Live `PTCS.Host` Playwright E2E remains tracked as `DYN-WBS-512` / `DYN-T-512`.

## 2026-06-26 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta4 NuGet pushed

- Release pack generated `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\src\bin\Release\PulseTrade.Comm.Spa.Dynamic.0.1.3-beta4.nupkg`.
- Nuspec metadata points to branch `20260623_001_嘗試GPT-OSS` commit `1dfcbfebc598714b781eefb1d217610103e757e8` and dependency `PulseTrade.Comm.Spa 0.2.5-beta16`.
- Copied the package to `C:\Program Files\dotnet\sdk\10.0.301\FSharp\library-packs`.
- NuGet push returned `Created` / `Your package was pushed`; API key value was not logged.

## 2026-06-26 - PTCS.Dynamic full-form arg-string package gate / beta5

- Completed the package-side gap found during PTCS.Host live probing: backend-resolved arg-string target now projects the parsed `ParseResults<PFCF_AKKA_CMD_DATA_RANGE>` tail subcommand into the returned FormInput DSL as a `DataRange` section.
- The client renderer now treats document-backed targets as one full form: all parsed sections render simultaneously, per-case Send buttons are suppressed, and one full-form Send submits the reconstructed raw Argu string.
- Adjusted list raw output to inline values for Argu list cases and added regression coverage for root tuple defaults (`BBA`, `DecimalQuote`, `Round`) plus tail tuple defaults (`DataRange.Between`).
- Verification: `dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-restore -- --summary --no-spinner` passed 14/14.
- Package version bumped from `0.1.3-beta4` to `0.1.3-beta5`; live PTCS.Host browser/RN E2E remains tracked as `DYN-WBS-512` / `DYN-T-512`.

## 2026-06-26 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta5 NuGet pushed

- Release pack generated `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\src\bin\Release\PulseTrade.Comm.Spa.Dynamic.0.1.3-beta5.nupkg`.
- Nuspec metadata confirmed dependency `PulseTrade.Comm.Spa 0.2.5-beta16` plus existing FAkka/WebSharper dependencies.
- Copied the package to `C:\Program Files\dotnet\sdk\10.0.301\FSharp\library-packs`.
- NuGet push returned `Created` / `Your package was pushed`; API key value was not logged.

## 2026-06-26 - PTCS.Dynamic live FormInput fixes / beta6

- Fixed WebSharper client renderer registration in `src/Client/ArguFormRenderer.fs`: Dynamic append/add-key/general renderers now call PTCS global registries with three arguments instead of one array argument, so Host-loaded extension JS actually registers the append input renderer.
- Fixed backend FormInput DSL defaults in `src/Server/ArguForm.fs`: exact canonical enum default values parsed from the arg string are appended to select option values when the schema only exposes lower-case generated options.
- Extended `DYN-T-511` package tests to verify canonical enum defaults such as `OIInf` and `ModeAccountingDate` are available as select option values.
- Verification: `dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-restore -p:WebSharperRunCompiler=false -- --summary --no-spinner` passed 14/14.
- Verification: `dotnet fsi --exec G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\verify-ptcs-host-dynamic-argu-live.fsx` passed and reported `ptcsHostDynamicArguLive.ok ... submit=echo-verified`, proving PTCS.Host loopback can render the backend-resolved PFCF FormInput and echo the exact raw command.
- Package version bumped from `0.1.3-beta5` to `0.1.3-beta6`.

## 2026-06-26 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta6 NuGet pushed

- Release pack generated `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\src\bin\Release\PulseTrade.Comm.Spa.Dynamic.0.1.3-beta6.nupkg`.
- Nuspec metadata confirmed dependencies: `PulseTrade.Comm.Spa 0.2.5-beta16`, `FAkka.Argu 10.1.301`, `FAkka.FCell2 10.1.301`, `FSharp.Core 10.1.301`, and `WebSharper.FSharp 10.1.5.674`.
- Copied the package to `C:\Program Files\dotnet\sdk\10.0.301\FSharp\library-packs`.
- NuGet push returned `Created` / `Your package was pushed`; API key value was not logged.
- Local build caveat: normal WebSharper compiler execution is currently blocked by inaccessible untracked `src\websharper.log` in this checkout. The beta6 release build used `/p:WebSharperRunCompiler=false`; the client JavaScript file was already regenerated before the lock appeared, and the beta6 semantic change is backend-side canonical enum option projection.

## 2026-06-26 - PTCS.Dynamic SDUI add-target UX fix / beta7

- Added `doc\SDUI_Developer_Manual.md` as the ongoing SDK/manual surface for SDUI, Canvas renderer, FormInput renderer, target key lifecycle, extension loading, and verifier rules.
- Clarified the design boundary: PTCS.Dynamic renders extension-owned add-target/FormInput UI, but PTCS core owns selected target lifecycle, key-registry replay, and whether append-input renderer should be invoked. Therefore no-target FormInput residue cannot be fixed reliably in Dynamic alone.
- Updated `src\Client\ArguFormRenderer.fs` so the Dynamic add-target renderer supports both `actor-dynamic` and generic `actor-argu` pages.
- The add-target UI now exposes actor address as an explicit input instead of relying on Host demo `DefaultKey`; submitted target keys remain `[ actorAddress; duTypeOrTemplateKey; canonicalArgString ]`.
- Verification: cross-repo `G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\verify-ptcs-host-dynamic-argu-live.fsx` passed `--skip-submit` and full `submit=echo-verified` runs against PTCS beta18 + Dynamic beta7. The verifier uses F# + Playwright native locator APIs and no inline DOM JavaScript.
- Package version bumped to `0.1.3-beta7`. Because the original checkout still has an inaccessible untracked `src\websharper.log`, the release build was produced from the clean temp copy `G:\PulseTrade.fs.Comm.Log\build\ptcs-dynamic-beta7-06a9f9ee\src`; the generated package was copied to `C:\Program Files\dotnet\sdk\10.0.301\FSharp\library-packs`.
- NuGet push returned `Your package was pushed`; immediate NuGet public index/registration check was still propagation-pending.

## 2026-06-26 - Correction: PulseTrade.Comm.Spa.Dynamic 0.1.3-beta7 NuGet propagation complete

- NuGet flat-container and registration metadata are now available for `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta7`.
- Local nupkg nuspec dependency inspection confirmed `PulseTrade.Comm.Spa 0.2.5-beta18`, `FAkka.Argu 10.1.301`, `FAkka.FCell2 10.1.301`, `FSharp.Core 10.1.301`, and `WebSharper.FSharp 10.1.5.674`.

## 2026-06-26 - PTCS.Dynamic parsed add-target regression / beta8

- Fixed the Dynamic add-target client UI so DU/template key is always an editable text input with a datalist, even when the registry contains only one template. The old single-template path rendered an immutable `<code>` node and made type-string testing impossible.
- Removed the touched raw JS value setter in `src\Client\ArguFormRenderer.fs`; the new code uses typed DOM property assignment. Existing WebSharper global registration shims remain isolated interop boundaries.
- Added package regression `DYN-T-512`: partial canonical arg string `--pfcfedx trivial --pfcfgtcconf OIInf TAIFEX` resolves only to `PFCFEDX` and `PFCFGTCCONF`, with defaults `trivial` and `OIInf/TAIFEX`.
- Extended cross-repo verifier `G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\verify-ptcs-host-dynamic-argu-live.fsx` to cover full and partial commands, editable type key, no-target cleanup, generic `actor-argu` add-target, and canonical raw input preservation after remove/re-add so it cannot regress to `"s"`.
- Verification: `dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-restore -p:WebSharperRunCompiler=false -- --summary --no-spinner` passed 15/15.
- WebSharper build initially reproduced the stale `wsfscservice.exe` / `src\websharper.log` lock; after stopping the stale helper, Release build regenerated `wwwroot\js\PulseTrade.Comm.Spa.Dynamic.js` with `dynamic-argu-key-du-type-list` and no old `length(keys)===1 -> code` path.
- Verification: `dotnet fsi --exec G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\verify-ptcs-host-dynamic-argu-live.fsx -- --skip-submit --port 0 --extension-dir C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\src\bin\Release\net10.0` passed.
- Verification: full submit run without `--skip-submit` passed with `submit=echo-verified`.
- Package version bumped from `0.1.3-beta7` to `0.1.3-beta8`; release pack generated `src\bin\Release\PulseTrade.Comm.Spa.Dynamic.0.1.3-beta8.nupkg`, copied to `C:\Program Files\dotnet\sdk\10.0.301\FSharp\library-packs`, and NuGet push returned `Created` / `Your package was pushed`. Immediate flat-container check was still propagation-pending.

## 2026-06-27 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta9 list/free-type UX fix

- Fixed Dynamic add-target UI: the DU/template key is now a plain editable text input for a full type name or template key. The renderer no longer creates `dynamic-argu-key-du-type-list` datalist/select lock-in, even when Host registration contains a single demo template.
- Fixed FormInput list rendering: Argu `'T list` fields, including `PFCFGTCCONF`, render as editable textbox rows. Parser-projected values remain defaults only; Add creates an empty textbox and each row has a Remove button. List item enum/options metadata is not used as a dropdown for repeatable list values.
- Verification: `dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-restore -p:WebSharperRunCompiler=false -- --summary --no-spinner` passed 15/15.
- Verification: `dotnet build .\src\PulseTrade.Comm.Spa.Dynamic.fsproj -c Release --no-restore` regenerated the release bundle after stopping a stale WebSharper helper that held `src\websharper.log`.
- Cross-repo verification: `G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\verify-ptcs-host-dynamic-argu-live.fsx` passed against `src\bin\Release\net10.0`, including full command echo, partial command rendering, free DU/template key input, no-target cleanup, generic `actor-argu` add-target, and `PFCFGTCCONF` editable list Add/Remove behavior.
- Package version bumped from `0.1.3-beta8` to `0.1.3-beta9`; release pack generated `src\bin\Release\PulseTrade.Comm.Spa.Dynamic.0.1.3-beta9.nupkg`, copied to `C:\Program Files\dotnet\sdk\10.0.301\FSharp\library-packs`, and NuGet push returned `Created` / `Your package was pushed`. Immediate flat-container check was still propagation-pending.

## 2026-06-27 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta10 backend list-item DSL correction

- Correction after beta9: live resolver inspection showed the outer `PFCFGTCCONF` node was `List`, but the inner `valueItem` metadata still said `Select`. The beta9 browser renderer ignored that and displayed textboxes, but the DSL itself still invited future dropdown regressions.
- Fixed `Server/ArguForm.fs`: Argu `ArgumentType.List` item schema is now always `text` with no enum options. Enum/options metadata for the element type no longer leaks into repeatable list item UI semantics.
- Updated package tests: `DYN-T-511` now asserts `PFCFGTCCONF.valueItem.Kind = "text"` and options are empty, while `DataRange.ReferenceDateMode` enum select still keeps canonical default casing.
- Verification: `dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-restore -p:WebSharperRunCompiler=false -- --summary --no-spinner` passed 15/15.
- Verification: `dotnet build .\src\PulseTrade.Comm.Spa.Dynamic.fsproj -c Release --no-restore` passed and generated `src\bin\Release\PulseTrade.Comm.Spa.Dynamic.0.1.3-beta10.nupkg`.
- Cross-repo verification: `G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\verify-ptcs-host-dynamic-argu-live.fsx -- --extension-dir C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\src\bin\Release\net10.0` passed with `submit=echo-verified`.
- Package version bumped from `0.1.3-beta9` to `0.1.3-beta10`; release pack copied to `C:\Program Files\dotnet\sdk\10.0.301\FSharp\library-packs`, and NuGet push returned `Created` / `Your package was pushed`. Immediate flat-container check was still propagation-pending.

## 2026-06-27 - Correction: PulseTrade.Comm.Spa.Dynamic 0.1.3-beta10 NuGet propagation complete

- NuGet flat-container now lists `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta10`.

## 2026-06-27 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta11 compact target binding UX

- Updated `Client/ArguFormRenderer.fs`: Dynamic target submit label is now `Bind target`, repeatable list Add button is `Add value`, and list rows render Remove on the left of the textbox.
- Responsibility boundary: PTCS.Dynamic owns Dynamic target binding/FormInput renderer; PTCS core owns page lifecycle chrome such as tab close, `+ Page`, and sidebar `Actions` pool. Dynamic package still does not contain Host-specific sample DU.
- Package version bumped from `0.1.3-beta10` to `0.1.3-beta11`.
- Verification: `dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-restore -- --summary --no-spinner` passed 15/15.
- Verification: `dotnet build .\src\PulseTrade.Comm.Spa.Dynamic.fsproj -c Release -v:minimal` passed with existing WS9002 / NU5123 / missing readme warnings and generated `src\bin\Release\PulseTrade.Comm.Spa.Dynamic.0.1.3-beta11.nupkg`.
- Cross-repo verification: `G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\verify-ptcs-host-dynamic-argu-live.fsx` passed against PTCS beta19 + Dynamic beta11, including `Bind target`, Remove-left list rows, PTCS action pool/tab close/`+ Page`, and exact PFCF echo.
- NuGet bundle/live-host verification: `verify-ptcs-dynamic-nuget-bundle.fsx` passed for PTCS beta19 + Dynamic beta11; `run-ptcs-dynamic-nuget-live-host.fsx -- --no-wait` started an in-process `#r` host, printed URLs/actor/template/PCSL root, verified health/probe, and stopped.
- Package copied to `C:\Program Files\dotnet\sdk\10.0.301\FSharp\library-packs`; NuGet push returned `Created` / `Your package was pushed`. Immediate flat-container check was still propagation-pending.

## 2026-06-27 - Correction: PulseTrade.Comm.Spa.Dynamic 0.1.3-beta11 NuGet propagation complete

- NuGet flat-container now lists `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta11`.
- PTC cross-repo `run-ptcs-dynamic-nuget-live-host.fsx -- --no-wait` remains the current direct `#r` consumer gate for PTCS beta19 + Dynamic beta11.

## 2026-06-27 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta12 page-type badge alignment

- Updated Dynamic server extension manifest badge for `actor-dynamic` from `D` to `ad`; PTCS core owns the corresponding logical page label/badge rendering and distinguishes generic `actor-argu` as `aa`.
- Package version bumped from `0.1.3-beta11` to `0.1.3-beta12`.
- Verification: `dotnet build .\src\PulseTrade.Comm.Spa.Dynamic.fsproj -c Release -v:minimal` passed with existing WS9002 / NU5123 / missing-readme warnings and generated `src\bin\Release\PulseTrade.Comm.Spa.Dynamic.0.1.3-beta12.nupkg`.
- Verification: `dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-restore -- --summary --no-spinner` passed 15/15.
- Cross-repo verification: `G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\verify-ptcs-host-dynamic-argu-live.fsx -- --extension-dir C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\src\bin\Release\net10.0` passed with `submit=echo-verified`, including default `actor-dynamic`, re-created generic `actor-argu`, re-created `actor-dynamic`, `ad`/`aa` nav badges, Dynamic add-target renderer recovery, and Canonical Argu string visibility.
- NuGet bundle/live-host verification: `verify-ptcs-dynamic-nuget-bundle.fsx` passed for PTCS beta20 + Dynamic beta12; `run-ptcs-dynamic-nuget-live-host.fsx -- --no-wait` started an in-process `#r` host with auto web/cluster ports, printed URLs/actor/template/PCSL root, verified health/probe, and stopped.
- NuGet push returned `Created` / `Your package was pushed`; follow-up flat-container lookup lists `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta12`.

## 2026-06-27 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta13 PTCS beta21 dependency rollout

- Package version bumped from `0.1.3-beta12` to `0.1.3-beta13` so Dynamic package metadata and consumer gates align with `PulseTrade.Comm.Spa 0.2.5-beta21` and `FAkka.WebSocket 1.569.101.301-win1`.
- No Host-specific sample DU was added to Dynamic; PTCS.Host remains responsible for demo DU/live deployment wiring.
- Build initially failed because WebSharper `wsfscservice.exe` held an inaccessible generated `src\websharper.log`; stopping the stale compiler service removed the artifact and Release build passed with existing WS9002 / NU5123 / missing-readme warnings.
- Verification: `dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-restore -- --summary --no-spinner` passed 15/15.
- Cross-repo verification: `G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\verify-ptcs-dynamic-nuget-bundle.fsx` passed for PTCS beta21 + Dynamic beta13; `run-ptcs-dynamic-nuget-live-host.fsx -- --no-wait` passed health, HTTP actor-argu send, WebSocket actor-argu send, and state readback.
- Package copied to `C:\Program Files\dotnet\sdk\10.0.301\FSharp\library-packs`; NuGet push returned `Created` / `Your package was pushed`. Immediate flat-container lookup was still propagation-pending.

## 2026-06-27 - Correction: PulseTrade.Comm.Spa.Dynamic 0.1.3-beta13 NuGet propagation complete

- NuGet flat-container now lists `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta13`.

## 2026-06-27 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta14 Ordered Target-Key Submit Compatibility

- Background: PTCS Actor Dynamic target keys are positional `[actor; template; raw]`, but legacy live PCSL could expose sorted triples from older PTCS stream canonicalization. When Dynamic read those triples, it could treat the actor address as the template key and report `Unknown Dynamic Argu template`.
- Change: `Client/ArguFormRenderer.fs` now includes `keyJson` in append submit payloads and normalizes legacy sorted triples where the first segment is not an actor, the second segment is an actor, and the third segment is a registered Argu schema/template. The repaired order is `[actor; template; raw]`.
- Boundary: PTCS beta23 owns append-page stream key ordering, snapshot overlay, browser keyId canonical identity, and Host stale demo target cleanup. Dynamic beta14 only repairs renderer payloads and does not add Host-specific sample DU code.
- Package version bumped from `0.1.3-beta13` to `0.1.3-beta14`.
- NuGet push returned `Created` / `Your package was pushed` for `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta14`; immediate v3 flat-container/registration lookup at 2026-06-27 18:37 +08:00 was still propagation-pending and did not list beta14 yet.
- Verification: Release build passed with existing WS9002 / NU5123 / missing-readme warnings and generated `src\bin\Release\PulseTrade.Comm.Spa.Dynamic.0.1.3-beta14.nupkg`; cross-repo PTC `verify-ptcs-host-dynamic-argu-live.fsx` passed with legacy sorted key injection, echo, and canvas; `verify-ptcs-dynamic-nuget-bundle.fsx` and `run-ptcs-dynamic-nuget-live-host.fsx -- --no-wait` passed for PTCS beta23 + Dynamic beta14.

## 2026-06-27 18:47 +08:00 - Correction: PulseTrade.Comm.Spa.Dynamic 0.1.3-beta14 NuGet indexing complete

- Follow-up NuGet flat-container lookup lists `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta14`.

## 2026-06-27 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta15 Add-Target Alias UX

- Background: PTCS beta24 adds display-only append target alias metadata. Dynamic add-key renderer needed to collect that alias without changing the canonical target tuple `[actorAddress; duTypeOrTemplateKey; canonicalArgString]`.
- Change: `Client/ArguFormRenderer.fs` add-key payload is now `{ keys; displayName }`.
- Change: add-key UI renders actor address, DU/template key, target alias, canonical Argu string, and Clean/OK actions. `Bind target` text is retired; PTCS core owns panel open/collapse and target-list alias display.
- Package version bumped from `0.1.3-beta14` to `0.1.3-beta15`.
- Verification: Release build passed with existing WS9002 / NU5123 / missing-readme warnings and generated `src\bin\Release\PulseTrade.Comm.Spa.Dynamic.0.1.3-beta15.nupkg`. Cross-repo PTC browser/bundle/live-host gates were updated for beta15 and await NuGet flat-container indexing.
- NuGet push returned `Created` / `Your package was pushed` for `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta15`; flat-container lookup at 2026-06-27 19:55 +08:00 was still propagation-pending.

## 2026-06-27 20:05 +08:00 - Correction: PulseTrade.Comm.Spa.Dynamic 0.1.3-beta15 NuGet indexing and deployment complete

- Follow-up NuGet flat-container lookup lists `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta15`.
- Package tests passed `15/15`; cross-repo PTC gates passed: `verify-ptcs-dynamic-nuget-bundle.fsx`, `run-ptcs-dynamic-nuget-live-host.fsx -- --no-wait`, and `verify-ptcs-host-dynamic-argu-live.fsx`.
- PTC redeployed public 81 to `live81-ptcs-beta24-dynamic-beta15-alias-202606272001`; release-local Dynamic JS contains alias/OK markers and no `Bind target`.

## 2026-06-27 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta16 Add-Target Cancel Action

- Background: Add target key UX needed a non-destructive Cancel action distinct from Clean and OK.
- Change: `AddKeyContextDto` now consumes PTCS core `cancelKey`; Dynamic add-key renderer renders `Clean / Cancel / OK`.
- Behavior: Cancel calls `context.cancelKey()` and only collapses the PTCS Add target panel. It does not submit a target, clear existing targets, or alter `[actorAddress; duTypeOrTemplateKey; canonicalArgString]`.
- Package version bumped from `0.1.3-beta15` to `0.1.3-beta16`.
- Verification: Release build passed after stopping stale `wsfscservice.exe`; existing warnings remain WS9002 / NU5123 / missing-readme. Package tests passed `15/15`; cross-repo PTC browser/bundle/live-host gates passed for beta25/beta16 and now assert Cancel label plus Cancel-time panel collapse.
- NuGet push returned `Created` / `Your package was pushed` for `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta16`; flat-container lookup at 2026-06-27 21:15 +08:00 was still propagation-pending.
- PTC redeployed public 81 to `live81-ptcs-beta25-dynamic-beta16-cancel-202606272112`; release-local Dynamic JS contains Cancel/OK markers and no `Bind target`.

## 2026-06-28 - DYN-WBS-517 Canvas Tree renderer planning

- Updated RFC-PTCS-DYNAMIC-0003, REQ, SA, SD, SDUI DSL manual, WBS, TEST, and Traceability for the ActorTree follow-up.
- Dynamic now reserves a Canvas `Tree` node for PTCS Actors tab: `id/parentId/label/status` field mapping, orthogonal connectors, boxed plus/minus toggles, and optional columns.
- Boundary clarified: Dynamic renders `ActorTreeDocument` but does not own Actor Registry truth source, PTCS PCSL projection, browser IndexedDB cache, fallback table, or state report writing.
- Added planned `DYN-WBS-517` / `DYN-T-517`; no package source implementation was changed in this slice.

## 2026-06-28 - RFC-PTCS-DYNAMIC-0004 Actor Dynamic action modes

- Background: PTCS action shell now needs explicit Actor Dynamic / Actor Argu mode separation. Actor Argu is FormInput-only; Actor Dynamic must support direct actor key, DU target key, and proxy key.
- Change: added `doc/RFC-PTCS-DYNAMIC-0004.actor-dynamic-action-modes.md` and synchronized REQ/SA/SD/WBS/README/SDUI developer manual.
- Planned implementation: mode-aware add-key renderer shapes `actor-dynamic-target`, `actor-dynamic-proxy`, and `actor-argu-target`; Dynamic message renderer remains payload-based canvas-only.
## 2026-06-28 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta22 Actor Dynamic action modes

- RFC-PTCS-DYNAMIC-0004 accepted and implemented the clarified mode split: Actor Argu remains FormInput-only and never exposes proxy/canvas behavior; Actor Dynamic supports direct actor key, DU/FormInput target key, and Dynamic proxy key.
- Add-key renderer now claims `actor-dynamic-target`, `actor-dynamic-proxy`, and `actor-argu-target`. Direct Actor Dynamic actor key intentionally falls back to PTCS arbitrary textarea input so JSON DSL can round-trip to the canvas renderer.
- Proxy key builder stores `[proxyActorAddress; "proxy-v1"; rnActorAddress; targetKind]`; payload is not part of the key and is carried by append input value.
- Dynamic message renderer remains payload-based: it renders canvas only for `schema=fskynet-sdui` JSON DSL and returns `None` for ordinary replies.
- Package version advanced to `0.1.3-beta22`; Release build passed with existing WS9002 / NU5123 / missing-readme warnings; package tests passed 15/15; PTC bundle and live Playwright verifiers passed against PTCS `0.2.5-beta37`. NuGet push returned `Created` / `Your package was pushed`; immediate flat-container lookup was propagation-pending.

## 2026-06-28 - Correction: PulseTrade.Comm.Spa.Dynamic 0.1.3-beta22 NuGet indexing complete

- Follow-up NuGet flat-container lookup now lists `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta22`.
- Dynamic WBS current row `DYN-WBS-518` was updated so beta22 indexing is no longer an open gap.

## 2026-06-28 - RFC-PTCS-DYNAMIC-0005 ActorsPage renderer

- Added `doc/RFC-PTCS-DYNAMIC-0005.actors-page-renderer.md` to mirror PTCS `RFC-PTC-SPA-0010` from the Dynamic implementation side.
- Decision under review: `/actors` must not use the generic Canvas summary/preview renderer as final UI. Dynamic support means a dedicated `ActorsPage` renderer that owns the whole page: node blocks, actor hierarchy tree, grid, cards, reload/report controls, and status UI.
- Updated README with the ActorsPage renderer boundary and updated WBS: `DYN-WBS-517` is superseded by page-level `DYN-WBS-519`.

## 2026-06-28 - RFC-PTCS-DYNAMIC-0005 ActorsPage first implementation slice

- Implemented first-slice ActorsPage renderer registration in `src/Client/ActorDynamicTab.fs`; it registers a page-level renderer and returns a whole-page Dynamic host for `ActorTopologyPage` payloads.
- Kept generic Canvas message renderer unchanged. ActorsPage is not rendered through the `FSkynet 動態畫布 (Canvas)` summary card path.
- Added package tests `DYN-T-526` and `DYN-T-527`; Dynamic tests passed 17/17 with `WebSharperRunCompiler=false` after a separate full WebSharper short-path build passed.
- Documented WebSharper limitations found during implementation: a new `[<JavaScript>]` client compile unit and `String.Contains` both crash `wsfsc.exe`; current first slice stays in `ActorDynamicTab.fs` and uses one `IndexOf("ActorTopologyPage")` classifier.
- Updated `REQ.md`, `SA.md`, `SD.md`, `SDUI_DSL_zh-Hant.md`, `SDUI_Developer_Manual.md`, `WBS.md`, `TEST.md`, `Verification.md`, `Traceability.md`, and `README.md`.
- Remaining `DYN-WBS-519`: strict parser, node grouping by host:port, role ordering, full tree/grid/cards/actions, PTCS `/actors` Playwright gate, and NuGet rollout.
- No package source or version was changed in this planning slice.

## 2026-06-28 - RFC-PTCS-DYNAMIC-0005 ActorsPage source-host verification gate

- Extended `src/Client/ActorDynamicTab.fs` so `createActorsPageDocument` renders a page-level Actors UI with action shell, count cards, node blocks, hierarchy rows, and grid rows. This remains a first gate, not the final PTCS/GW/RN grouped IA.
- Dynamic now registers the ActorsPage renderer through the page renderer bridge and also routes `ActorTopologyPage` through the existing `fskynet-sdui` message renderer as a compatibility fallback for PTCS source-host dispatch.
- PTCS source-host Playwright MCP gate passed against `http://127.0.0.1:3716/actors`: page renderer registered, Dynamic page host present, fallback rows `0`, Dynamic rows `17`, node blocks `14`, and full actor addresses visible. Evidence: `G:\PulseTrade.fs\log\20260628\20260628195755.actors-page-dynamic-3716.png`.
- Documented operational lessons: initialize page renderer registry in both PTCS bootstrap locations, avoid WebSharper dynamic call array-argument emission for `PulseTradeRegisterPageRenderer`, prefer Dynamic source Release `#I` before stale `C:\ptcsdyn-build\bin`, and clean WebSharper output when bundle markers are stale.
- Remaining `DYN-WBS-519`: strict parser, clean host:port grouping, PTCS/GW/RN role ordering, report actions, restart/failover visual states, reusable F# Playwright verifier, NuGet rollout, and public 81 deployment proof.

## 2026-06-28 - RFC-PTCS-DYNAMIC-0005 ActorsPage grouping and toggle slice

- Updated `Client/ActorDynamicTab.fs` so ActorsPage groups transported actor addresses by actor-system host/port and sorts blocks as PTCS Host -> GW Host -> RN Host -> Unknown. Local virtual parent paths now stay in the Unknown block instead of being misclassified by child path tokens.
- Added boxed stateful tree toggles in the Dynamic ActorsPage renderer. Buttons expose `data-testid="dynamic-actor-tree-toggle"`, update `aria-expanded`, switch `-`/`+`, and remove/restore child rows from the rendered tree.
- Verification: Dynamic package tests passed 18/18; short-path WebSharper bundle build passed after stopping stale `wsfscservice.exe`. PTCS source-host Playwright MCP gate passed at `http://127.0.0.1:3721/actors`: fallback rows `0`, four blocks in PTCS/GW/RN/Unknown order, full actor addresses visible, and toggle row count changed `17 -> 16 -> 17`. Evidence: `G:\PulseTrade.fs\log\20260628\20260628220000.actors-page-toggle-check.json`; screenshot: `G:\PulseTrade.fs\log\20260628\20260628220000.actors-page-toggle-fixed.png`.

## 2026-06-28 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta23 ActorsPage rollout

- Package version advanced to `0.1.3-beta23` for the ActorsPage grouping/toggle renderer.
- Short-path Release build/pack produced `PulseTrade.Comm.Spa.Dynamic.0.1.3-beta23.nupkg`; warnings remain the known WS9002 / NU5123 / missing-readme package warnings.
- Cross-repo PTC package bundle verifier passed with PTCS `0.2.5-beta39` and Dynamic `0.1.3-beta23`, including new ActorsPage/toggle markers.
- Public 81 deployment `live81-ptcs-beta39-dynamic-beta23-actorspage-toggle-202606282235` now renders `Actors / Dynamic`; Playwright MCP screenshot `G:\PulseTrade.fs\log\20260628\20260628223500.actors-public81-beta39-toggle-collapse.png` shows the boxed `+` collapsed state.
- NuGet push returned `Created` / `Your package was pushed`; immediate flat-container lookup was propagation-pending.

## 2026-06-28 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta23 indexing confirmation

- Follow-up NuGet flat-container lookup now lists `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta23`.
- `DYN-WBS-519` moves to 90; remaining work is strict schema parser, production report actions, restart/failover visual states, and reusable F# Playwright verifier.

## 2026-06-28 - ActorsPage reusable F# verifier handoff

- PTCS added `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Scripts\verify.actorsPageDynamic.playwright.fsx` as the reusable F# Playwright accepted-path gate for the Dynamic ActorsPage renderer.
- The gate starts PTCS with the Dynamic source Release bundle and verifies Dynamic owns `/actors`, fallback rows are absent, PTCS/GW/RN/Unknown blocks are ordered, full `akka.tcp://...` addresses are visible, report/reload controls exist, and boxed toggle collapse/expand changes visible rows `17 -> 16 -> 17`.
- Evidence screenshot is `G:\PulseTrade.fs\log\20260628\20260628230455.actors-page-dynamic-fsharp-verifier.png`.
- `DYN-WBS-519` moves to 92. Remaining work is strict schema parser, production report actions, restart/failover visual states, and Dynamic absent/unsupported renderer failure-injection.

## 2026-06-28 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta24 ActorsPage hierarchy restore

- Restored the ActorsPage hierarchy visual model inside Dynamic node blocks. Concrete actor addresses are grouped by their real actor-system address, then virtual ancestors such as `/user` and `/system` are reattached inside the owning PTCS/GW/RN block instead of forming a synthetic Unknown block.
- Tree rows now expose status-dot and connector markers for reusable verification, keep full `akka.tcp://...` labels visible, and preserve boxed `+` / `-` toggles with collapse/expand behavior.
- Package version advanced from `0.1.3-beta23` to `0.1.3-beta24`. Release build passed after stopping stale `wsfscservice.exe`; package tests passed; NuGet push returned `Your package was pushed`. Immediate flat-container lookup was still propagation-pending for beta24.
- Cross-repo verification passed: `verify.actorsPageDynamic.playwright.fsx`, `verify-ptcs-dynamic-nuget-bundle.fsx`, and `run-ptcs-dynamic-nuget-live-host.fsx -- --no-wait`. Evidence: `G:\PulseTrade.fs\log\20260628\20260628233906.actors-page-dynamic-beta24-hierarchy.png`.
- Public 81 redeployed to `live81-ptcs-beta39-dynamic-beta24-hierarchy-restore-202606282340`; Playwright MCP evidence is `G:\PulseTrade.fs\log\20260628\20260628234106.actors-public81-beta24-hierarchy.png` and `G:\PulseTrade.fs\log\20260628\20260628234106.actors-public81-beta24-hierarchy-snapshot.md`.

## 2026-06-29 - Correction: PulseTrade.Comm.Spa.Dynamic 0.1.3-beta24 NuGet indexing complete

- Follow-up NuGet flat-container lookup now lists `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta24`.
- `DYN-WBS-519` moves to 95. Remaining work is strict schema parser, production report actions, restart/failover visual states, and Dynamic absent/unsupported renderer failure-injection.

## 2026-06-29 - DYN-WBS-519 unsupported ActorsPage fallback gate

- No Dynamic package source changed in this slice.
- PTCS verifier `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Scripts\verify.actorsActorTree.playwright.fsx -- --with-unsupported-client-extension` now injects a client extension manifest with a missing script URL, proving PTCS falls back to its built-in ActorTree/table when no usable `ActorsPage` renderer exists.
- Playwright MCP visual evidence: `G:\PulseTrade.fs\log\20260629\20260629001159.actors-unsupported-fallback-playwright-mcp.png`.
- `DYN-WBS-519` moves to 96. Remaining: strict schema parser, production report actions, and restart/failover visual states.

## 2026-06-29 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta27 ActorsPage accepted ownership gate

- Dynamic advanced to `0.1.3-beta27` for the ActorsPage hierarchy/metadata slice paired with PTCS `0.2.5-beta40`.
- `src\Client\ActorDynamicTab.fs` now keeps virtual path rows inside their concrete actor-system group, exposes stable row metadata (`data-node-kind`, `data-display-address`, `data-parent-id`), and keeps report controls in the page-level renderer.
- PTCS beta40 fixes the core-side accepted ownership issue: after Dynamic accepts `/actors`, PTCS no longer appends fallback `actor-node` / `actor-card` DOM below the Dynamic page. This is verified from the PTCS F# Playwright gate rather than Dynamic package tests alone.
- Verification passed: Dynamic package tests 18/18, Dynamic Release/WebSharper build, PTC `verify-ptcs-dynamic-nuget-bundle.fsx`, PTC `run-ptcs-dynamic-nuget-live-host.fsx -- --no-wait`, PTCS `verify.actorsPageDynamic.playwright.fsx`, and public 81 Playwright MCP proof.
- Evidence: `G:\PulseTrade.fs\log\20260629\20260629011000.actorspage-beta40-dyn27-mcp.png`, `G:\PulseTrade.fs\log\20260629\20260629011000.actorspage-beta40-dyn27-mcp-after-collapse.png`, `G:\PulseTrade.fs\log\20260629\20260629011000.actors-public81-beta40-dyn27.png`, and `G:\PulseTrade.fs\log\20260629\20260629011000.actors-public81-beta40-dyn27-snapshot.md`.
- `DYN-WBS-519` moves to 98. Remaining: strict schema parser, production report schedule, and restart/failover visual states.

## 2026-06-29 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta29 ActorsPage report schedule

- Dynamic advanced to `0.1.3-beta29` for the browser-local Actors report schedule start/stop control. The pushed `0.1.3-beta28` package is superseded because its source Release nupkg contained stale JS where the schedule button remained disabled.
- `src\Client\ActorDynamicTab.fs` now toggles the report schedule button between `Schedule` and `Stop schedule`, calls the existing report endpoint immediately, and repeats every 60 seconds while the browser page remains open. This is not a server daemon or production persisted schedule.
- Verification passed: Dynamic package tests 18/18, short-path Release WebSharper bundle/pack, nupkg JS marker check, PTCS `verify.actorsPageDynamic.playwright.fsx -- --dynamic-bin-dir C:\ptcsdyn-release-beta29b\bin`, PTC `verify-ptcs-dynamic-nuget-bundle.fsx`, PTC `run-ptcs-dynamic-nuget-live-host.fsx -- --no-wait`, public 81 deployment alignment, and Playwright MCP public `/actors` start/stop proof.
- Public 81 release is `live81-ptcs-beta40-dynamic-beta29-report-schedule-202606290205`. Evidence: `G:\PulseTrade.fs\log\20260629\actors-public81-beta40-dyn29.png`, `G:\PulseTrade.fs\log\20260629\actors-public81-beta40-dyn29-deep-snapshot.md`, `G:\PulseTrade.fs\log\20260629\actors-public81-beta40-dyn29-schedule-started.md`, and generated report `G:\PulseTrade.fs\log\20260629\actors-report-public81-beta29\20260628180415.md`.
- Remaining `DYN-WBS-519`: strict ActorsPage schema parser, server-side persisted report scheduling, IndexedDB restart/cache sync, cross-service GW/RN registry feed, and failover/passivation visual states.

## 2026-06-29 - Correction: PulseTrade.Comm.Spa.Dynamic 0.1.3-beta29 NuGet indexing complete

- Follow-up NuGet flat-container lookup now lists `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta29`. Remaining `DYN-WBS-519` work is implementation/runtime scope, not public package propagation.

## 2026-06-29 - poc.full.nuget.fsx beta40/beta29 execution repair

- Updated `src\poc.full.nuget.fsx` so the full NuGet POC runs against PTCS `0.2.5-beta40` and Dynamic `0.1.3-beta29`.
- Fixes: add the new `ActorArguSendArgs.HistoryKeys` field, parse `defaultArgumentsText` first and apply CLI args as overrides, use `--cluster-port 0` with a random free Akka port by default, suppress Dynamic extension asset-list noise unless `--verbose-startup` is supplied, replace `Console.ReadLine()` with `stopPocFullNuget()` for manual FSI mode, and ignore generated `src/.pcsl/` demo runtime data.
- Verification passed: `dotnet fsi --exec .\src\poc.full.nuget.fsx -- --no-wait`. The run printed Chat/Sets/Actors/ActorArgu/DynEcho URLs, PCSL root, full `akka.tcp://...` addresses, message tickets, ingress health `pending=0 deadLetters=0`, Dynamic Canvas JSON parse success, Showcase JSON reply, and stopped the server automatically.

## 2026-06-29 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta30 offline cleanup and POC2

- Dynamic advanced to `0.1.3-beta30` for ActorsPage offline-like status rendering and offline-last ordering support paired with PTCS `0.2.5-beta41`.
- `src\Client\ActorDynamicTab.fs` now maps offline/unreachable/stale/terminated/stopped/passivated/failed statuses into red status treatment and `OFFLINE` display, counts offline rows, and sorts all-offline node blocks after online blocks when PTCS supplies diagnostic offline nodes.
- Added `src\poc.full.nuget.2.fsx` without changing `src\poc.full.nuget.fsx`. POC2 directly `#r`s PTCS beta41/Dynamic beta30, registers a host-local Argu DU/template plus Actor Argu target key, includes Actors page support, and disables `+ Page` Actor Dynamic tab-page creation through the extension manifest override.
- Verification passed: short-path Release build/pack at `C:\ptcsdyn-release-beta30d\bin`, package tests 18/18 with `WebSharperRunCompiler=false`, PTC bundle verifier for beta41/beta30, PTCS `verify.actorsPageDynamic.playwright.fsx -- --dynamic-bin-dir C:\ptcsdyn-release-beta30d\bin`, and `dotnet fsi --exec .\src\poc.full.nuget.2.fsx -- --no-wait`.
- Public 81 release `live81-ptcs-beta41-dynamic-beta30-offline-poc2-202606290748` shows one active backend node after reload instead of stale multi-node service-run blocks. Evidence: `G:\PulseTrade.fs\log\20260629\public81-actors-beta41-dyn30.png`, `G:\PulseTrade.fs\log\20260629\public81-actors-beta41-dyn30-snapshot.md`, and `G:\PulseTrade.fs\log\20260629\public81-actors-beta41-dyn30-dom.json`.
- NuGet push returned `Created` / `Your package was pushed` for `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta30`; immediate flat-container lookup was still propagation-pending.

## 2026-06-29 - poc.full.nuget.2 ActorsPage registry projection fix

- Fixed `src\poc.full.nuget.2.fsx` so its local echo actor is projected into PTCS actor registry with `hub.RegisterActor` after creation. The previous script created a valid actor and ActorArgu send path, but `/actors` stayed empty because no actor lifecycle data was present in the hub snapshot.
- `--no-wait` now fetches `/actors/api/snapshot` and requires non-zero node/actor counts plus the `nuget2-echo` actor path and node address.
- Verification passed: `dotnet fsi --exec .\src\poc.full.nuget.2.fsx -- --no-wait` printed `Actors data   nodes=1 actors=1`; Playwright MCP on local POC2 `/actors` captured `G:\PulseTrade.fs\log\20260629\poc2-actors-page-fixed-deep-snapshot.md` and `G:\PulseTrade.fs\log\20260629\poc2-actors-page-fixed-deep.png` showing `akka.tcp://PtcsDynamicPocFullNuget2@127.0.0.1:9582/user/nuget2-echo`.

## 2026-06-29 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta31 alias and WebSocket cleanup

- Advanced Dynamic to `0.1.3-beta31` so its package dependency closure uses PTCS `0.2.5-beta43` and `FAkka.WebSocket 1.569.101.301-win6`.
- Updated `src\poc.full.nuget.2.fsx` to reference beta43/beta31 and assert the `POC2 FormInput target` alias remains after the server-side ActorArgu send/probe path.
- Verification passed: short-path Release build/pack at `C:\ptcsdyn-release-beta31\bin`, nupkg dependency inspection for FAkka.WebSocket win6, PTC bundle verifier, PTC NuGet live-host verifier, and `dotnet fsi --exec .\src\poc.full.nuget.2.fsx -- --no-wait`.
- The POC2 run completed without `WebSocket disconnected` / `ConnectionAborted` console noise, relying on FAkka.WebSocket win6 rather than a PTCS Host workaround.

## 2026-06-29 - ProjectReference removed from package project

- User direction: PTCS.Dynamic package project should consume PTCS as a NuGet package because referenced packages are local-deployed for development; future work should not reintroduce ProjectReference.
- Changed `src\PulseTrade.Comm.Spa.Dynamic.fsproj` from ProjectReference to `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\PulseTrade.Comm.Spa.fsproj` into exact `PackageReference Include="PulseTrade.Comm.Spa" Version="[0.2.5-beta43]"`.
- Verification passed: `rg ProjectReference src\PulseTrade.Comm.Spa.Dynamic.fsproj` has no hit; Release build passed with existing WebSharper/NU5123/missing-readme warnings after stopping a stale `wsfscservice.exe`; generated `PulseTrade.Comm.Spa.Dynamic.0.1.3-beta31.nupkg` nuspec contains `PulseTrade.Comm.Spa [0.2.5-beta43]`.
- Rebuilt beta31 nupkg was copied to `C:\Program Files\dotnet\sdk\10.0.301\FSharp\library-packs`; no `nuget.config` was created.

## 2026-06-29 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta32 package-only release

- Advanced `src\PulseTrade.Comm.Spa.Dynamic.fsproj` to `0.1.3-beta32` after removing the PTCS source `ProjectReference`; the package now consumes `PulseTrade.Comm.Spa [0.2.5-beta43]` through an exact NuGet `PackageReference`.
- Updated `src\poc.full.nuget.2.fsx` to reference Dynamic beta32 and kept PTCS at beta43.
- Verification passed: package tests 18/18, short-path Release build/pack at `C:\ptcsdyn-release-beta32\bin`, and cross-repo `G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\verify-ptcs-dynamic-nuget-bundle.fsx`.
- NuGet push for `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta32` returned `Created`; flat-container indexing was still pending immediately after push.

## 2026-06-29 - Correction: Dynamic beta32 NuGet indexing complete

- Follow-up NuGet flat-container lookup now lists `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta32`.

## 2026-06-29 - Correction: Dynamic beta32 live-host gate complete

- Cross-repo `G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\run-ptcs-dynamic-nuget-live-host.fsx -- --no-wait` passed against PTCS `0.2.5-beta43` and Dynamic `0.1.3-beta32`; HTTP/WebSocket/state probes completed and the ActorArgu ticket returned `Completed`.

## 2026-06-29 - Correction: Dynamic beta32 POC2 gate complete

- `src\poc.full.nuget.2.fsx -- --no-wait` passed against Dynamic `0.1.3-beta32`; it started the in-process PTCS/Dynamic NuGet host, reported `Actors data nodes=1 actors=1`, and stopped cleanly.

## 2026-06-29 - poc.full.nuget.journal SQL journal projection rebuild POC

- Added `src\poc.full.nuget.journal.fsx` as a package-only PTCS/Dynamic POC for durable Akka journal replay. It uses `PulseTrade.Comm.Spa [0.2.5-beta43]` and `PulseTrade.Comm.Spa.Dynamic [0.1.3-beta32]` from NuGet/library-packs.
- The script treats PCSL as projection/cache and SQL Server Akka.Persistence journal as canonical truth. It derives the default SQL DB name from the selected `pcslRoot`, configures `CommSpaActorFabricOptions.withJournal`, and wraps the UI hub in `PcslActorProxyCommSpaPersistenceBackend(remoteWire=true)` so HTTP/UI append-page writes go through journaled sharding instead of direct PCSL append.
- Startup warm-up forces replay of append-page registry/key/value streams plus actor/participant/generic-set registries. A stable default cluster port `9787` and stable template key `poc-full-nuget-journal-argu` keep durable ActorArgu target keys usable across process restarts.
- Verification passed:
  - `dotnet fsi --exec .\src\poc.full.nuget.journal.fsx -- --no-wait`
  - `dotnet fsi --exec .\src\poc.full.nuget.journal.fsx -- --clear-pcsl-before-start --no-wait`
- The second run cleared `G:\PulseTrade.fs.Comm.Log\manual\ptcsDynamicNugetJournal\pcsl_journal_001`, reused SQL DB `PTCSDynJ_7168b47cef9f5493`, reported `journal warm-up streams=7 pages=1 actors=1`, and kept the same `akka.tcp://PtcsDynamicPocJournal83446001@127.0.0.1:9787/user/nuget-journal-echo` target address.

## 2026-06-29 - poc.full.nuget.journal ActorRegistry spawn correction

- Replaced the journal POC's direct `CommHub.RegisterActor` shortcut with `PulseTrade.Comm.Actor.Registry.ActorOfRegistered`.
- The script now explicitly references `PulseTrade.Comm.Actor.Registry [0.1.0-alpha4]`, builds `ActorRegistrySettings.create (hub.ActorRegistrySink())`, and spawns the echo actor through `fabric.System.ActorOfRegistered(...)`. Actor display in `/actors` therefore comes from the same lifecycle registry path used by PTCS Host, including tags/metadata and termination watcher support.
- Verification passed with a separate root/port because default `9787` was already owned by a Visual Studio FSI session:
  - `dotnet fsi --exec .\src\poc.full.nuget.journal.fsx -- --pcsl-root "G:/PulseTrade.fs.Comm.Log/manual/ptcsDynamicNugetJournal/pcsl_actor_registry_001" --cluster-port 9797 --no-wait`
  - `dotnet fsi --exec .\src\poc.full.nuget.journal.fsx -- --pcsl-root "G:/PulseTrade.fs.Comm.Log/manual/ptcsDynamicNugetJournal/pcsl_actor_registry_001" --cluster-port 9797 --clear-pcsl-before-start --no-wait`
- The clear/restart run reused SQL DB `PTCSDynJ_132444f8634d536e`, reported `journal warm-up streams=7 pages=1 actors=1`, and `/actors` diagnostics reported `visibleNodes=1 visibleActors=1 includeOfflineNodes=1 includeOfflineActors=1 hubActors=1`.

## 2026-06-29 - poc.full.nuget.journal PingPong ActorRegistry reload probe

- Added `PocFullNugetJournalPingPongActor` and `PocFullNugetJournalPingPongMessage` to `src\poc.full.nuget.journal.fsx`.
- The PingPong actor is spawned with `fabric.System.ActorOfRegistered(...)` using its own `ActorRegistrySettings` and tags `ptcs-dynamic`, `poc-full-nuget-journal`, `pingpong`, `actor-registry-reload`; the script still does not call `CommHub.RegisterActor`.
- FSI/manual mode now prints the PingPong `akka.tcp://.../user/...-pingpong` address plus `stopPingPongActor()`. This lets a browser session reload `/actors`, observe the PingPong actor, call `stopPingPongActor()` in FSI, then reload again to verify ActorRegistry termination projection behavior.
- Verification passed:
  - `dotnet build .\src\PulseTrade.Comm.Spa.Dynamic.fsproj -c Release -p:BaseIntermediateOutputPath=C:\ptcsdyn-journal-pingpong\obj\ -p:OutputPath=C:\ptcsdyn-journal-pingpong\bin\`
  - `dotnet fsi --exec .\src\poc.full.nuget.journal.fsx -- --pcsl-root "G:/PulseTrade.fs.Comm.Log/manual/ptcsDynamicNugetJournal/pcsl_pingpong_001" --cluster-port 9797 --no-wait`
  - `dotnet fsi --exec .\src\poc.full.nuget.journal.fsx -- --pcsl-root "G:/PulseTrade.fs.Comm.Log/manual/ptcsDynamicNugetJournal/pcsl_pingpong_001" --cluster-port 9797 --clear-pcsl-before-start --no-wait`
- The first FSI gate reported `visibleNodes=1 visibleActors=2 includeOfflineNodes=1 includeOfflineActors=2 hubActors=2` and printed both the echo actor address and the PingPong actor address. The clear-PCSL gate reported `journal warm-up streams=7 pages=1 actors=2`.

## 2026-06-29 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta33 ActorsPage DSL console logging

- Added ActorsPage browser console diagnostics in `Client\ActorDynamicTab.fs`. Every Dynamic ActorsPage render emits a collapsed console group titled `[PTCS.Dynamic ActorTree DSL] RENDER ...`; clicking the Dynamic Reload button emits `[PTCS.Dynamic ActorTree DSL] RELOAD ...` before the browser reloads.
- Each console group logs `phase`, full raw ActorTopologyPage DSL string, parsed `dsl` object, and `nodes` array so the browser can distinguish stale backend payload from stale frontend rendering.
- Advanced package version to `0.1.3-beta33`; updated `src\poc.full.nuget.2.fsx`, `src\poc.full.nuget.journal.fsx`, and cross-repo PTC NuGet verifier/live-host scripts to consume beta33.
- Verification passed: package tests 18/18, short-path Release build/pack at `C:\ptcsdyn-release-beta33\bin`, nupkg marker check for `[PTCS.Dynamic ActorTree DSL]`, PTC bundle verifier, PTC NuGet live-host verifier, journal POC no-wait/clear-PCSL gates, and Playwright MCP console proof on `http://127.0.0.1:14933/actors` showing both `RENDER` and `RELOAD` console groups.
- NuGet push for `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta33` returned `Created`; immediate flat-container lookup was propagation-pending.

## 2026-06-29 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta38 PingPong ActorTree cleanup

- Advanced Dynamic to `0.1.3-beta38`, paired with PTCS `0.2.5-beta48`.
- Updated `src\poc.full.nuget.2.fsx` and `src\poc.full.nuget.journal.fsx` to reference beta48/beta38.
- `src\poc.full.nuget.journal.fsx -- --no-wait` now verifies the PingPong stop/reload path through ActorRegistry and PTCS ActorTree DSL: projected status becomes `terminated`, registry events are `Registered/Active@1, Unregistered/Terminated@2`, active DSL filters PingPong, and `includeOffline` retains stopped diagnostics.
- Verification passed: short-path Release build at `C:\ptcsdyn-release-beta38\bin`, package tests `18/18`, PTC bundle verifier beta48/beta38, and NuGet push for `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta38`.

## 2026-06-30 - poc.full.nuget.journal Echo actor fixed-name lifecycle helper

- Clarified `src\poc.full.nuget.journal.fsx` manual FSI lifecycle: the bootstrap section already creates the fixed-name Echo actor `nuget-journal-echo` through `fabric.System.ActorOfRegistered(...)`, so rerunning the same call in the same ActorSystem correctly fails with Akka `InvalidActorNameException`.
- Added `ensureEchoActorRegistered()`, `stopEchoActor()`, and `recreateEchoActor()` helpers. `ensureEchoActorRegistered()` resolves and reuses the live `/user/nuget-journal-echo` actor instead of attempting a duplicate spawn; `recreateEchoActor()` stops the live actor, waits for the path to release, then registers a fresh actor with the same stable path.
- Verification passed: `dotnet fsi --exec .\src\poc.full.nuget.journal.fsx -- --pcsl-root "G:/PulseTrade.fs.Comm.Log/manual/ptcsDynamicNugetJournal/pcsl_echo_respawn_20260630" --cluster-port 9807 --no-wait` printed `Echo actor is already live`, projected PingPong as `terminated`, and reported `After stop visibleNodes=1 visibleActors=1 includeOfflineActors=2 pingPongFiltered=true`.

## 2026-06-30 - Correction: Echo fixed-name reuse after stop is verified

- Corrected the previous wording: a live fixed-name Echo actor cannot be duplicate-spawned, but after stop and actor path release the same actor name must be reusable.
- Tightened `src\poc.full.nuget.journal.fsx` lifecycle helpers: Echo status/event matching now uses exact actor-name suffix matching so `nuget-journal-echo-pingpong` does not pollute `nuget-journal-echo` diagnostics.
- `--no-wait` now calls `recreateEchoActor()` after the PingPong stop gate. The recreate path uses strict `ActorOfRegistered` after stop/wait, so duplicate actor name would fail the verifier instead of being silently reused.
## 2026-06-30 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta41 ACL/Login demo package slice

- Advanced Dynamic package version to `0.1.3-beta41` and pinned `PulseTrade.Comm.Spa [0.2.5-beta51]` by exact PackageReference.
- Added `src\poc.full.nuget.journal.ACL.fsx`, a NuGet-only dual-auth PTCS demo: 81-style GitHub OAuth host, 82-style PTCS.Login local username/password host, shared SQL journal + PCSL projection, DamnWZ/AssTerry actor-argu pages, Echo/PingPong target keys, and ActorRegistry `ActorOfRegistered` actors.
- Verification passed: Dynamic tests `18/18`, ACL demo no-wait gate on 18081/18082, and Playwright MCP checks for login visual, admin `+ Page`, Terry黑粉 no `+ Page` / no Add target, FormInput visible, and no `ptcs.extension.post` alert.
- Full Dynamic WebSharper compile currently crashes `wsfsc.exe` against PTCS beta51 with `MSB6006 ... -532462766`; beta41 package was produced with `WebSharperRunCompiler=false` and existing verified `src/wwwroot/js` contentFiles. This is tracked as follow-up compiler/metadata work, not as an ACL/Login runtime failure.

## 2026-06-30 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta41 NuGet push

- `PulseTrade.Comm.Spa.Dynamic.0.1.3-beta41.nupkg` was pushed to nuget.org with the existing local API key path. The key value was not logged.
- NuGet push returned `Created` / `Your package was pushed`; immediate v3 flat-container lookup was still propagation-pending.

## 2026-06-30 - Correction: PulseTrade.Comm.Spa.Dynamic 0.1.3-beta41 NuGet indexing complete

- Follow-up NuGet flat-container lookup now lists `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta41`.

## 2026-07-01 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta42 PTCS beta52 alignment

- Advanced Dynamic package version to `0.1.3-beta42` and pinned `PulseTrade.Comm.Spa [0.2.5-beta52]` by exact PackageReference.
- Restored normal Release WebSharper build/pack path. Initial `MSB6006 wsfsc.exe -532462766` was diagnosed as `UnauthorizedAccessException` deleting generated `src\websharper.log`; stopping stale `wsfscservice.exe` and removing the generated log fixed the build.
- Verification passed: Dynamic Release build/pack, Dynamic tests `18/18`, and `src\poc.full.nuget.journal.ACL.fsx -- --no-wait --local-port 18082 --github-port 18081 --cluster-port 18787 --pcsl-root .\.pcsl\verify.acl.beta42`.
- `PulseTrade.Comm.Spa.Dynamic.0.1.3-beta42.nupkg` was copied to SDK `10.0.301` `FSharp\library-packs`; NuGet.org push returned `401` because the current shell has no NuGet API key configured.

## 2026-07-01 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta43 PTCS beta53 alignment

- Advanced Dynamic package version to `0.1.3-beta43` and pinned `PulseTrade.Comm.Spa [0.2.5-beta53]` by exact PackageReference.
- Updated `src\poc.full.nuget.journal.ACL.fsx` to load PTCS beta53 and Dynamic beta43 from NuGet/local library-packs.
- Verification passed: Release build/pack produced `PulseTrade.Comm.Spa.Dynamic.0.1.3-beta43.nupkg`, Dynamic tests passed `18/18`, and ACL no-wait script passed on local ports `18081/18082` with cluster port `18787`.
- The no-wait script reported `After stop visibleActors=1 pingPongFiltered=true` and `Echo reuse reuseAfterStop=true`, preserving the actors page stop/recreate regression proof on the beta53/beta43 package pair.

## 2026-07-01 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta44 PTCS beta54 alignment

- Advanced Dynamic package version to `0.1.3-beta44` and pinned `PulseTrade.Comm.Spa [0.2.5-beta54]` by exact PackageReference.
- Updated `src\poc.full.nuget.journal.ACL.fsx` to load PTCS beta54 and Dynamic beta44 from NuGet/local library-packs.
- Purpose: keep Dynamic on the current PTCS package after PTCS beta54 added server-only JSONL ACL audit sink/readback.
- Verification passed: after stopping stale `wsfscservice.exe`, Release build/pack produced `PulseTrade.Comm.Spa.Dynamic.0.1.3-beta44.nupkg`, Dynamic tests passed `18/18`, and ACL no-wait script passed on local ports `18081/18082` with cluster port `18787`.
- The no-wait script reported `After stop visibleActors=1 pingPongFiltered=true` and `Echo reuse reuseAfterStop=true`, preserving the actors page stop/recreate regression proof on the beta54/beta44 package pair.

## 2026-07-01 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta45 PTCS beta55 alignment

- Advanced Dynamic package version to `0.1.3-beta45` and pinned `PulseTrade.Comm.Spa [0.2.5-beta55]` by exact PackageReference.
- Updated `src\poc.full.nuget.journal.ACL.fsx` to load PTCS beta55 and Dynamic beta45 from NuGet/local library-packs.
- Purpose: keep Dynamic on the current PTCS package after PTCS beta55 added WebSocket ACL principal revalidation for long-lived sessions.
- Verification passed: after stopping stale `wsfscservice.exe`, Release build/pack produced `PulseTrade.Comm.Spa.Dynamic.0.1.3-beta45.nupkg`, Dynamic tests passed `18/18`, and ACL no-wait script passed on local ports `18081/18082` with cluster port `18787`.

## 2026-07-01 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta46 PTCS beta56 alignment

- Advanced Dynamic package version to `0.1.3-beta46` and pinned `PulseTrade.Comm.Spa [0.2.5-beta56]` by exact PackageReference.
- Updated `src\poc.full.nuget.journal.ACL.fsx` to load PTCS beta56 and Dynamic beta46 from NuGet/local library-packs.
- Purpose: keep Dynamic on the current PTCS package after PTCS beta56 added WebSocket ACL proxy cleanup.
- Verification passed: Release build/pack produced `PulseTrade.Comm.Spa.Dynamic.0.1.3-beta46.nupkg`, Dynamic tests passed `18/18`, and ACL no-wait script passed on local ports `18081/18082` with cluster port `18787`.

## 2026-07-01 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta47 PTCS beta57 alignment

- Advanced Dynamic package version to `0.1.3-beta47` and pinned `PulseTrade.Comm.Spa [0.2.5-beta57]` by exact PackageReference.
- Updated `src\poc.full.nuget.journal.ACL.fsx` to load PTCS beta57 and Dynamic beta47 from NuGet/local library-packs.
- Purpose: keep Dynamic on the current PTCS package after PTCS beta57 added HTTP ACL matrix coverage and canonical ACL resource mapping for normalized page ids such as `assterry` -> `AssTerry`.
- Verification passed: Release build/pack produced `PulseTrade.Comm.Spa.Dynamic.0.1.3-beta47.nupkg`, Dynamic tests passed `18/18`, and ACL no-wait script passed on local ports `18081/18082` with cluster port `18787`.
- The no-wait script reported `After stop visibleActors=1 pingPongFiltered=true` and `Echo reuse reuseAfterStop=true`, preserving the actors page stop/recreate regression proof on the beta57/beta47 package pair.

## 2026-07-01 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta48 PTCS beta58 alignment

- Advanced Dynamic package version to `0.1.3-beta48` and pinned `PulseTrade.Comm.Spa [0.2.5-beta58]` by exact PackageReference.
- Updated `src\poc.full.nuget.journal.ACL.fsx` to load PTCS beta58 and Dynamic beta48 from NuGet/local library-packs.
- Purpose: keep Dynamic on the current PTCS package after PTCS beta58 added TLS-offload same-origin ACL gate coverage for public 81 deployments.
- Verification passed: Release build/pack produced `PulseTrade.Comm.Spa.Dynamic.0.1.3-beta48.nupkg`, Dynamic tests passed `18/18`, PTC bundle verifier loaded beta58/beta48, and ACL no-wait script passed on local ports `18081/18082` with cluster port `18787`.
- Public 81 PTC deployment `live81-ptcs-beta58-dynamic-beta48-acl-demo-stale-cleanup-202607010337` loaded this Dynamic bundle; Playwright MCP verified `/page/assterry` renders FormInput and Send appends an echo reply. Evidence is retained under `G:\PulseTrade.fs\log\20260630\public81-assterry-beta58-stale-cleanup-after-send-*`.

## 2026-07-01 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta53 PTCS beta63 alignment

- Advanced Dynamic package version to `0.1.3-beta53` and pinned `PulseTrade.Comm.Spa [0.2.5-beta63]` by exact PackageReference.
- Updated `src\poc.full.nuget.journal.ACL.fsx` to load PTCS beta63 and Dynamic beta53 from NuGet/local library-packs.
- Purpose: keep Dynamic on the current PTCS package after PTCS beta63 fixed browser protected API fetch credentials for public OAuth deployments.
- Verification passed: Release build/pack produced `PulseTrade.Comm.Spa.Dynamic.0.1.3-beta53.nupkg`, Dynamic tests passed `18/18`, PTC bundle verifier loaded beta63/beta53, PTC NuGet live-host no-wait gate completed, and PTCS ACL/Login browser verifier passed with exact beta63/beta53 packages.
- Public 81 PTC deployment `live81-ptcs-beta63-dynamic-beta53-fetch-credentials-202607010522` loaded this Dynamic bundle; Playwright MCP verified `/actors` no longer logs `/pages/api/definitions` 401 and `/page/assterry` renders FormInput/send/reply with `replied msg: echo:...`. Evidence is retained at `G:\PulseTrade.fs\log\20260630\ptcs-81-actors-beta63-console.txt` and `G:\PulseTrade.fs\log\20260630\ptcs-81-assterry-beta63-after.md`.

## 2026-07-01 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta54 PTCS beta64 alignment

- Advanced Dynamic package version to `0.1.3-beta54` and pinned `PulseTrade.Comm.Spa [0.2.5-beta64]` by exact PackageReference.
- Updated `src\poc.full.nuget.journal.ACL.fsx` to load PTCS beta64 and Dynamic beta54 from NuGet/local library-packs.
- Purpose: keep Dynamic on the current PTCS package after PTCS beta64 added SQL Server ACL audit sink API/tests.
- Verification passed: Release build/pack produced `PulseTrade.Comm.Spa.Dynamic.0.1.3-beta54.nupkg`, Dynamic tests passed `18/18`, PTC bundle verifier loaded beta64/beta54, PTC NuGet live-host no-wait gate completed, and PTCS ACL/Login browser verifier passed with exact beta64/beta54 packages.
- NuGet.org push returned `Created` and the nupkg was copied to SDK `10.0.301` `FSharp\library-packs`.
- Public 81 PTC deployment `live81-ptcs-beta64-dynamic-beta54-sql-audit-202607010610` loaded this Dynamic bundle; Playwright MCP verified `/actors` and `/page/assterry` FormInput/send/reply with `replied msg: echo:...`. Evidence is retained at `G:\PulseTrade.fs\log\20260630\ptcs-81-actors-beta64-console.txt` and `G:\PulseTrade.fs\log\20260630\ptcs-81-assterry-beta64-after.md`.

## 2026-07-01 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta55 PTCS beta65 alignment

- Advanced Dynamic package version to `0.1.3-beta55` and pinned `PulseTrade.Comm.Spa [0.2.5-beta65]` by exact PackageReference.
- Updated `src\poc.full.nuget.journal.ACL.fsx` to load PTCS beta65 and Dynamic beta55 from NuGet/local library-packs.
- Purpose: keep Dynamic aligned after PTCS beta65 added ACL policy runtime hot-reload APIs (`currentSnapshot/currentRevision/reloadSnapshot`).
- Fixed `src\PostBuildEvent.ps1` to select the exact fsproj package version instead of sorting only by numeric core; this prevents stale `0.1.3-beta48` nupkg selection when newer beta packages exist.
- Verification passed: Release build/pack produced `PulseTrade.Comm.Spa.Dynamic.0.1.3-beta55.nupkg`, NuGet.org push returned `Created`, the nupkg was copied to SDK `10.0.301` `FSharp\library-packs`, Dynamic tests passed `18/18`, and PTC bundle verifier loaded exact beta65/beta55 assembly/package paths.
- Public 81 deployment `live81-ptcs-beta65-dynamic-beta55-acl-hot-reload-202607010725` loaded this Dynamic bundle; Playwright MCP verified `/actors` Dynamic renderer registration and `/page/assterry` FormInput send/reply with Echo target `values=1 seq=2`. Evidence is retained at `G:\PulseTrade.fs\log\20260701\ptcs81-beta65-actors-console.txt` and `G:\PulseTrade.fs\log\20260701\ptcs81-beta65-assterry-after.md`.

## 2026-07-01 - PulseTrade.Comm.Spa.Dynamic 0.1.3-beta56 PTCS beta66 alignment

- Advanced Dynamic package version to `0.1.3-beta56` and pinned `PulseTrade.Comm.Spa [0.2.5-beta66]` by exact PackageReference.
- Updated `src\poc.full.nuget.journal.ACL.fsx` to load PTCS beta66 and Dynamic beta56 from NuGet/local library-packs.
- Purpose: keep Dynamic aligned after PTCS beta66 added protected `POST /acl/api/reload` and `PtcsAclPolicyConfigDto` for JSON-friendly ACL policy reload.
- Verification target: Release build/pack, Dynamic tests `18/18`, NuGet push, SDK library-packs copy, and PTC bundle verifier using exact beta66/beta56 assembly/package paths.

## 2026-07-01 - poc.full.nuget.journal.ACL.fsx quiet dual-host startup fix

- Fixed quiet startup suppression in `src\poc.full.nuget.journal.ACL.fsx`: replaced disposable `StringWriter` output capture with `TextWriter.Null`. Suave can retain `Console.Out` after `startWithSharing` returns; if that writer is disposed, the background listener can fail with `ObjectDisposedException` and the GitHub-OAuth side becomes unreachable.
- Verification passed: `dotnet fsi --exec .\src\poc.full.nuget.journal.ACL.fsx -- --no-wait --local-port 18102 --github-port 18101 --cluster-port 18801 --pcsl-root .\.pcsl\verify.acl.beta56.dual-host.quiet`.
- The same script still prints the GitHub OAuth URL, local PTCS.Login URL, PingPong stop filtering result, and fixed-name Echo actor reuse result before cleanly stopping both listeners.

## 2026-07-01 - poc.full.nuget.journal.ACL.fsx dynamic-port and production SQL proof

- Extended `src\poc.full.nuget.journal.ACL.fsx` with `--if-dyna-port` so GitHub, local-login, and cluster ports can be selected from free loopback ports when fixed 81/82 are occupied.
- Added production-sql mode using `PulseTrade.Comm.Security`, `PulseTrade.Comm.Login.SqlServer`, and `PulseTrade.Comm.ACL.SqlServer` packages. The script reads an encrypted SQL connection-string file plus private key path, seeds SQL credential/session/ACL policy tables, and authenticates the local PTCS.Login side through `SqlServerLoginCredentialVerifier` instead of demo credentials.
- Verification passed:
  - Demo dynamic-port no-wait: `dotnet fsi --exec C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\src\poc.full.nuget.journal.ACL.fsx -- --if-dyna-port --no-wait --pcsl-root G:\PulseTrade.fs.Comm.Log\manual\ptcsDynamicNugetJournalAcl\pcsl_verify_20260701_01`.
  - Production-sql dynamic-port no-wait: `dotnet fsi --exec C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\src\poc.full.nuget.journal.ACL.fsx -- --if-dyna-port --production-sql --sql-connection-string-encrypted-file G:\PulseTrade.fs.Comm.Log\manual\ptcsDynamicNugetJournalAcl\ptcs-local-integrated-20260701.enc.txt --sql-private-key-path D:\ingted.com\myKey.private.txt --sql-security-schema ptcs_poc_acl --sql-acl-table AclPolicySnapshot --no-wait --pcsl-root G:\PulseTrade.fs.Comm.Log\manual\ptcsDynamicNugetJournalAcl\pcsl_verify_20260701_sql_01`.
- The retained encrypted SQL file contains only encrypted text; plaintext SQL connection values were not written to repo files or verifier output.

## 2026-07-01 - Formal PTCS service production SQL proof with Dynamic beta56

- PTC formal Windows service release `live81-82-ptcs-beta66-dynamic-beta56-production-sql-private-lan-202607011125` is deployed with `PulseTrade.Comm.Spa 0.2.5-beta66` and `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta56`.
- The service loads Dynamic from the release extension directory and uses 81 GitHub OAuth plus loopback 82 SQL-backed PTCS.Login in the same process.
- SQL credential/session/ACL policy/audit providers use encrypted SQL connection file `D:\ingted.com\ptcs-sql-connection.enc.txt` and private key path `D:\ingted.com\myKey.private.txt`; no plaintext SQL password is written to Dynamic repo docs/logs.
- Verification passed through PTC deployment alignment and loopback 81/82/8798 health; 82 SQL `admin` login returns an HttpOnly `ptc_login_session` cookie and `/acl/api/snapshot` returns HTTP 200.
- Dynamic WBS/Verification now distinguish POC production-sql proof from the formal service proof. Remaining Dynamic-adjacent gaps are strict ActorsPage schema parser, server-side report schedule, IndexedDB restart/cache sync, cross-service GW/RN registry feed, and failover visual states.

## 2026-07-01 - poc.full.nuget.journal.ACL.fsx WZ/Terry SQL ACL proof

- Updated `src\poc.full.nuget.journal.ACL.fsx` to consume `PulseTrade.Comm.ACL.SqlServer [0.1.0-alpha2]`.
- Production-sql seeding is now bounded to the script-owned users `wz`, `terry`, `disabled-terry`, and legacy `admin`; it no longer clears unrelated credential rows.
- Verification passed: `dotnet fsi --exec C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\src\poc.full.nuget.journal.ACL.fsx -- --if-dyna-port --production-sql --sql-connection-string-encrypted-file D:\ingted.com\ptcs-sql-connection.enc.txt --sql-private-key-path D:\ingted.com\myKey.private.txt --sql-security-schema ptcs_security --sql-acl-table AclPolicySnapshotPoc --no-wait --pcsl-root G:\PulseTrade.fs.Comm.Log\manual\ptcsDynamicNugetJournalAcl\pcsl_wz_terry_20260701_01`.
- Stdout proof: WZ/sys-admin full rights, Terry黑粉 add-target denied but remove/send allowed on AssTerry, send denied on DamnWZ, disabled-terry login rejected, HTTP difference proof passed, PingPong stopped actor filtered from active actors, and fixed-name Echo actor stop/recreate works.

## 2026-07-02 - ACL2 final boundary planning

- Synced with PTCS `RFC-PTC-SPA-0013.acl-login-open-extension-boundary.md`.
- Added planned `DYN-WBS-521` / `DYN-VFY-009` for `src\poc.full.nuget.journal.ACL2.fsx`.
- `ACL.fsx` remains the beta66 transitional runtime behavior proof. `ACL2.fsx` will prove final open-extension boundary with `PulseTrade.Comm.Spa.ACL` and `PulseTrade.Comm.Spa.Login`, while closed ACL/Login Core packages remain exact binary NuGet dependencies.

## 2026-07-02 - ACL2 open-extension first slice

- Added open package projects `src\PulseTrade.Comm.Spa.ACL` and `src\PulseTrade.Comm.Spa.Login`, both versioned `0.1.0-alpha1`.
- Added `src\poc.full.nuget.journal.ACL2.fsx`; it references PTCS beta66, Dynamic beta56, Spa.ACL alpha1, and Spa.Login alpha1.
- Verification passed: Release build/pack for both alpha1 packages, nupkg copy to SDK `10.0.301` `FSharp\library-packs`, and `dotnet fsi --exec .\src\poc.full.nuget.journal.ACL2.fsx -- --if-dyna-port --no-wait --demo`.
- Remaining `DYN-WBS-521` gates: production SQL mode, explicit disabled-user ACL2 assertion, browser Playwright, public 81/82 redeploy, and full browser/client bundle extraction from PTCS core.

## 2026-07-02 - ACL2 production SQL no-wait gate

- `src\poc.full.nuget.journal.ACL2.fsx` production-SQL no-wait gate passed with encrypted SQL file/key args, schema `ptcs_security`, table `AclPolicySnapshotPoc`, and PCSL root `G:\PulseTrade.fs.Comm.Log\manual\ptcsDynamicNugetJournalAcl2\pcsl_wz_terry_20260702_01`.
- Evidence: `Mode production-sql`, WZ/sys-admin full rights, Terry黑粉 add-target denied but AssTerry remove/send allowed, DamnWZ send denied, wrong-password login 401, disabled-terry login 401, PingPong stop filtering, and Echo fixed-name actor reuse.
- Demo no-wait regression also passed with PCSL root `G:\PulseTrade.fs.Comm.Log\manual\ptcsDynamicNugetJournalAcl2\pcsl_demo_20260702_01`.
- Remaining `DYN-WBS-521` gates: browser Playwright, public 81/82 redeploy, and full browser/client bundle extraction from PTCS core.

## 2026-07-02 - ACL2 browser Playwright gate

- Cross-repo PTCS browser verifier passed against exact `PulseTrade.Comm.Spa 0.2.5-beta66`, `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta56`, `PulseTrade.Comm.Spa.ACL 0.1.0-alpha1`, and `PulseTrade.Comm.Spa.Login 0.1.0-alpha1`.
- Command: `dotnet fsi --exec G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Scripts\verify.aclLoginBrowser.playwright.fsx -- --port 0`.
- Coverage: admin/Terry local-login cookie flow, ACL capability UI differences, Dynamic FormInput visible/sendable state, and live ActorFabric echo replies.
- Remaining `DYN-WBS-521` gates: public 81/82 redeploy on extracted packages and full browser/client bundle extraction from PTCS core.

## 2026-07-02 - ACL/Login extension asset slice

- Advanced `PulseTrade.Comm.Spa.ACL` and `PulseTrade.Comm.Spa.Login` to `0.1.0-alpha3`.
- `PtcsAclExtension.useAcl` and `PtcsLoginExtension.usePtcsLogin` now register PTCS client-extension manifests and package `contentFiles` script assets in addition to calling the closed PTCS runtime SPI.
- `src\poc.full.nuget.journal.ACL2.fsx` now references Spa.ACL/Login alpha3 and asserts the authenticated page contains both extension ids/script URLs, while direct fetches of `/client-extensions/acl/PulseTrade.Comm.Spa.ACL.js` and `/client-extensions/login/PulseTrade.Comm.Spa.Login.js` return the expected package markers.
- Verification passed: Release build/pack for both alpha3 packages, NuGet push without missing-license warning, nupkg copy to SDK `10.0.301` `FSharp\library-packs`, ACL2 demo no-wait, ACL2 production-SQL no-wait with encrypted SQL file/key, and PTCS browser Playwright with client-extension manifest/script assertions.
- Remaining `DYN-WBS-521` gates: public 81/82 redeploy on extracted packages and moving the actual ACL/Login client/page behavior out of PTCS core into the open packages.

## 2026-07-02 - ACL/Login alpha8 client hook slice

- Advanced open packages to `PulseTrade.Comm.Spa.ACL 0.1.0-alpha8` and `PulseTrade.Comm.Spa.Login 0.1.0-alpha8`, consuming PTCS beta68.
- Both packages now register their WebSharper runtime dependency assets under their `/client-extensions/.../WebSharper.Core.JavaScript/Runtime.js` URL prefixes. This fixes ES module import resolution for package bundles loaded from PTCS pages.
- `PulseTrade.Comm.Spa.Login` now owns the browser login renderer hook through `PulseTradeRegisterLoginRenderer`; `PulseTrade.Comm.Spa.ACL` owns the ACL snapshot observer hook through `PulseTradeRegisterAclSnapshotObserver`.
- Verification passed: Release rebuild/pack for both alpha8 packages, nupkg copy to SDK `10.0.301` `FSharp\library-packs`, `src\poc.full.nuget.journal.ACL2.fsx -- --if-dyna-port --no-wait --demo --pcsl-root G:\PulseTrade.fs.Comm.Log\manual\ptcsDynamicNugetJournalAcl2\pcsl_client_hook_alpha8_20260702_01`, and PTCS browser Playwright with active renderer/observer assertions.
- NuGet push returned `Created` for Dynamic beta58, Spa.ACL alpha8, and Spa.Login alpha8. Immediate public index lookup was delayed; local nupkg nuspec inspection confirmed exact dependencies.

## 2026-07-02 - ACL/Login alpha10 provider-dispatch gate

- Advanced `PulseTrade.Comm.Spa.Dynamic` to `0.1.3-beta60`, `PulseTrade.Comm.Spa.ACL` to `0.1.0-alpha10`, and `PulseTrade.Comm.Spa.Login` to `0.1.0-alpha10`, all consuming `PulseTrade.Comm.Spa [0.2.5-beta70]` as exact NuGet packages.
- `PulseTrade.Comm.Spa.ACL` now documents and packages both ACL snapshot observer and ACL capability provider hooks; alpha10 is paired with PTCS beta70 because beta70 fixes the generated provider dispatch bridge.
- Verification passed: Release build/pack for Dynamic beta60 and Spa.ACL/Login alpha10, ACL2 dynamic-port no-wait gate using `pcsl_provider_iife_alpha10_20260702_01`, and the PTCS browser Playwright gate with exact beta70/beta60/alpha10 packages.

## 2026-07-02 - PTCS.Login alpha12 open provider slice

- Advanced `PulseTrade.Comm.Spa.Login` to `0.1.0-alpha12`. Alpha12 supersedes alpha11 so the package README and source/package content stay aligned.
- The open Login package now owns `PtcsLoginOptions`, `PtcsLogin.provider`, `/login` route composition, `/login/api/submit`, `/login/api/session`, `/login/logout`, `/chat/login`, `/chat/logout`, HttpOnly SameSite cookie handling, and session-to-principal resolution. `PtcsLoginExtension.usePtcsLogin` installs that provider through PTCS `Server.withBrowserAuth` instead of calling PTCS core `Server.withPtcsLogin`.
- Verification passed: Spa.Login Release build/pack, ACL2 dynamic-port no-wait gate using `pcsl_login_open_provider_alpha12_20260702_01`, and the PTCS browser Playwright gate with PTCS beta70 / Dynamic beta60 / Spa.ACL alpha10 / Spa.Login alpha12 exact packages.
- NuGet push for `PulseTrade.Comm.Spa.Login 0.1.0-alpha12` returned `Created`; immediate flat-container lookup returned 404, so public indexing is propagation-pending.

## 2026-07-02 - ACL2 formal service deploy gate

- PTC `PulseTrade.Comm.Spa.Host` now consumes Dynamic beta60, Spa.ACL alpha10, and Spa.Login alpha12 exact NuGet packages in the formal Windows service release.
- Formal service release `live81-82-ptcs-beta70-dynamic-beta60-open-acl-login-assetfix-202607021416` passed public 81 OAuth redirect, loopback 82 SQL local login, HttpOnly session cookie, `/acl/api/snapshot`, and direct Spa.ACL/Spa.Login script marker fetches.
- This closes the public 81/82 redeploy gate for `DYN-WBS-521` / `DYN-VFY-009`; remaining work is transitional PTCS core fallback cleanup after downstream consumers are stable.

## 2026-07-02 - ACL2 startup preflight fix

- User report: direct `src\poc.full.nuget.journal.ACL2.fsx` execution appeared to hang after `journal warm-up streams=15 pages=2 actors=2`.
- Root cause: the line means journal projection warm-up already completed; the next startup stages were mostly quiet, and fixed 81/82/9787 ports can be occupied by the formal PTCS service or a live FSI session.
- Change: ACL2 now preflights fixed GitHub/local-login/Akka ports before fabric startup, fails fast with a clear `--if-dyna-port` hint when occupied, and prints ACL/Login provider plus listener startup stage logs after journal warm-up.
- Verification passed: `dotnet fsi --exec .\src\poc.full.nuget.journal.ACL2.fsx -- --if-dyna-port --no-wait --demo --pcsl-root G:\PulseTrade.fs.Comm.Log\manual\ptcsDynamicNugetJournalAcl2\debug_hang_dyn_afterfix_20260702_02`.
- Verification safety check: with formal service occupying 81/82, `dotnet fsi --exec .\src\poc.full.nuget.journal.ACL2.fsx -- --no-wait --demo --pcsl-root G:\PulseTrade.fs.Comm.Log\manual\ptcsDynamicNugetJournalAcl2\debug_hang_fixed_afterfix_20260702_02` exits before startup with `ACL2 startup preflight failed: GitHub OAuth HTTP listener port 0.0.0.0:81 is unavailable...`, which is the expected fixed-port protection.

## 2026-07-02 - ACL2 NoLogin GitHub-only variant

- Added/fixed `src\full.nuget.journal.ACL2.NoLogin.fsx` as a GitHub OAuth only variant of ACL2.
- `PulseTrade.Comm.Spa.Login`, Login Core, and Login SQL package references remain commented out. The script no longer builds `PtcsLogin` options, no longer starts a local PTCS.Login listener, and no longer calls `/login/api/submit`.
- The NoLogin ACL policy uses `BrowserAuthProvider=github-oauth` and binds `github:ingted` to `sys-admin`, so public ACL evaluation has a real browser auth provider without reintroducing username/password login.
- No-wait verification intentionally avoids protected `/acl/api` / `/pages/api` HTTP matrix checks because there is no local session cookie provider. It still verifies health, journal/persistence health, ACL/Dynamic static assets, internal ActorFabric durable probe, PingPong stop request, and fixed-name Echo reuse.
- Verification passed: `dotnet fsi --exec .\src\full.nuget.journal.ACL2.NoLogin.fsx -- --if-dyna-port --no-wait --demo --pcsl-root G:\PulseTrade.fs.Comm.Log\manual\ptcsDynamicNugetJournalAcl2\nologin_20260702_02`.

## 2026-07-03 - ACL2 NoLogin PFCF prototype Argu template

- Extended `src\full.nuget.journal.ACL2.NoLogin.fsx` with `PFCF_AKKA_CMD_FOR_ProtoTyping` and related nested DU types from the PFCF prototyping shape.
- Registered the Dynamic Argu template key `pfcf-akka-cmd-prototyping` and seeded a default target key for the GitHub-only NoLogin script.
- The prototype intentionally models `PFCFEDX` as `mode:string` so current prototyping input `--pfcfedx trivial` is parsed and rendered as a FormInput text field; `ParseResults<PFCF_AKKA_CMD_DATA_RANGE_FOR_ProtoTyping>` is preserved for `datarange` tail ordering.
- Verification passed: `dotnet fsi --exec .\src\full.nuget.journal.ACL2.NoLogin.fsx -- --if-dyna-port --no-wait --demo --pcsl-root C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\.pcsl\verify.pfcf.nologin.20260703_0913 --delivery-profile nologin-pfcf-20260703 --actor-name nologin-pfcf-echo`.

## 2026-07-03 - ACL2 NoGithubOAuth local-login variant

- Added `src\full.nuget.journal.ACL2.NoGithubOAuth.fsx` as the local-login-only ACL2 script variant.
- The script starts only the PTCS.Login host, defaults fixed mode to port 82, keeps PTCS.ACL/PTCS.Login/Dynamic/PFCF prototype active, and removes the GitHub OAuth listener/client-id/secret path.
- Verification passed: `dotnet fsi --exec .\src\full.nuget.journal.ACL2.NoGithubOAuth.fsx -- --if-dyna-port --no-wait --demo --pcsl-root C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\.pcsl\verify.nogithub.local-login.20260703_1027 --delivery-profile nogithub-local-20260703 --actor-name nogithub-local-echo`.

## 2026-07-03 - ACL2 NoGithubOAuth live-host startup probe guard

- User report from another machine: production-SQL NoGithubOAuth startup reached PTCS.Login listener 82, then the inherited startup `ActorArgu.sendDurableAsync` server probe attempted to persist a page/value event and timed out in `Akka.Persistence.Sql`.
- Change: NoGithubOAuth live-host mode now skips that startup server probe unless `--no-wait` explicitly requests the full verifier proof.
- Verification passed: `dotnet fsi --exec .\src\full.nuget.journal.ACL2.NoGithubOAuth.fsx -- --if-dyna-port --no-wait --demo --pcsl-root C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\.pcsl\verify.nogithub.local-login.skipprobe.20260703_1158 --delivery-profile nogithub-local-skipprobe-20260703 --actor-name nogithub-local-skipprobe-echo`.

## 2026-07-03 - FAkka.WebSocket stack-safe package alignment

- Advanced Dynamic to `0.1.3-beta61`, Spa.ACL to `0.1.0-alpha11`, and Spa.Login to `0.1.0-alpha13`, all consuming exact `PulseTrade.Comm.Spa [0.2.5-beta71]`.
- PTCS beta71 consumes `FAkka.WebSocket [1.569.101.301-win12]`, which replaces the net10 recursive Suave WebSocket read loop with an iterative loop to prevent stack overflow on long-lived/busy sessions.
- Updated ACL2 scripts (`poc.full.nuget.journal.ACL2.fsx`, `full.nuget.journal.ACL2.NoGithubOAuth.fsx`, `full.nuget.journal.ACL2.NoLogin.fsx`) to the beta71/beta61/alpha11/alpha13 package set.
- NuGet push returned `Created` for Dynamic beta61, Spa.ACL alpha11, and Spa.Login alpha13. Verification passed: PTC `verify-ptcs-dynamic-nuget-bundle.fsx` and `dotnet fsi --exec .\src\full.nuget.journal.ACL2.NoGithubOAuth.fsx -- --if-dyna-port --no-wait --demo --pcsl-root G:\PulseTrade.fs.Comm.Log\verification\ptcsDynamicNoGithubOAuth\pcsl_win12_20260703_01 --delivery-profile nogithub-win12 --actor-name nogithub-win12-echo`.

## 2026-07-04 - Dynamic proxy-key route binding note

- Updated README and `doc\SDUI_Developer_Manual.md` to clarify that Actor Dynamic proxy keys currently route through the first key segment only: the proxy actor address.
- `rnActorAddress` in `[proxyActorAddress; "proxy-v1"; rnActorAddress; targetKind]` is retained as binding/diagnostic metadata. The live native/RN target must be captured by the proxy actor/spec when that proxy is created.
- A single shared proxy that chooses different native/RN targets per send is a future PTCS route-envelope/resolver change, not current Dynamic behavior.

## 2026-07-06 - ActorArgu proxy target proof

- Advanced `PulseTrade.Comm.Spa.Dynamic` to `0.1.3-beta63`, `PulseTrade.Comm.Spa.ACL` to `0.1.0-alpha13`, and `PulseTrade.Comm.Spa.Login` to `0.1.0-alpha15`, all consuming `PulseTrade.Comm.Spa [0.2.5-beta73]`.
- Dynamic FormInput now supports the `actor-argu-proxy` add-key renderer shape. The UI asks for native actor address + DU/template + canonical Argu string; PTCS command hooks own proxy creation and persisted target-key rewrite.
- `src\GenFileActorInvocationTest4.fsx` now proves the flow with two Akka.Remote nodes in one script: PTCS/proxy fabric node plus a separate native PingPong node. The proxy asks the native actor with raw Argu text and normalizes `fCell2<string>` / string / Newtonsoft `JObject` replies back to `ActorArguTargetReply`.
- Verification passed with `--if-dyna-port --no-wait --native-node-host 127.0.0.1 --startup-probe`; evidence is recorded in `G:\PulseTrade.fs\log\20260706\20260706134000.ptcs-proxy-orchestration-rfc-dev.op_log`.
- NuGet publish note: beta63/alpha13/alpha15 nupkgs were built, copied to SDK `10.0.301` `FSharp\library-packs`, and pushed to nuget.org. The first direct manual push without `--api-key` returned `401 Unauthorized`; rerun used the existing PostBuildEvent key-path workflow and NuGet returned `Created` / `Your package was pushed`.

## 2026-07-06 - ActorArgu proxy target browser proof

- Advanced `PulseTrade.Comm.Spa.Dynamic` to `0.1.3-beta64` to fix the Add proxy key renderer for registered template keys.
- The Add proxy key panel no longer pre-fills the native actor address from the currently selected proxy key, and template keys now show the Canonical Argu string input instead of a schema-not-found error. Backend resolver validation remains authoritative.
- Playwright MCP proof passed against `GenFileActorInvocationTest4.fsx` live host: local login on `127.0.0.1:18729`, PTCS/proxy node on `127.0.0.1:18730`, native PingPong node on `127.0.0.1:18731`.
- Browser evidence: Add proxy key UI showed Native actor address, DU type/template key, Target alias, Canonical Argu string, Clean/Cancel/OK; PingPong target send rendered `Actor Argu Reply` with `poc.full.nuget.journal.acl pingpong fcell2 raw=...`.
- NuGet push for Dynamic beta64 returned `Created`, and the nupkg was copied to SDK `10.0.301` `FSharp\library-packs` for immediate FSI restore.

## 2026-07-07 Explicit ActorArgu target key recovery

- Added `doc/RFC-PTCS-DYNAMIC-0006.explicit-actor-argu-target-key.md` as the Dynamic companion to PTCS `RFC-PTC-SPA-0015`.
- Superseded the beta64 `actor-argu-proxy` / hidden `BeforeAddKey` persisted-key rewrite path. New ActorArgu Add Target Key schema is `[proxyActorAddress; "target-v1"; targetActorAddress; duTypeOrTemplateKey; canonicalArgString]`.
- Advanced Dynamic to `0.1.3-beta65`, paired with PTCS `0.2.5-beta74`, Spa.ACL `0.1.0-alpha14`, and Spa.Login `0.1.0-alpha16`.
- `GenFileActorInvocationTest4.fsx -- --if-dyna-port --no-wait` passed with separate PTCS/proxy and native PingPong Akka.Remote nodes; proxy received `ActorArguTargetCommand.TargetActorAddress=Some(...)` and returned the native `fCell2<string>` reply through ActorArgu history.

## 2026-07-07 - Dynamic 0.1.3-beta65 explicit ActorArgu target key proof

- Completed `DYN-WBS-523` / `RFC-PTCS-DYNAMIC-0006`: Actor Argu Add Target Key now exposes Proxy actor address and Target actor address instead of the invalidated user-facing Add proxy key + hidden rewrite path.
- Playwright MCP local proof passed on `http://127.0.0.1:18182/page/damnwz`; evidence is `G:\PulseTrade.fs\log\20260707\ptcs-explicit-target-damnwz-before-actions.md` and `G:\PulseTrade.fs\log\20260707\ptcs-explicit-target-damnwz-after-send.md`.
- Formal `PulseTradeCommSpaHumanUi` service was redeployed to `live81-82-ptcs-beta74-dynamic-beta65-explicit-target-202607071430`; deployment alignment, 82 local-login, and GW/PTCS/RN three-host E2E passed.
- NuGet push for `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta65` returned `Created`; immediate flat-container lookup was propagation-pending.

## 2026-07-07 - GenFileActorInvocationTest4 dual-IP fCell2.T reply proof

- Corrected `src\GenFileActorInvocationTest4.fsx` so the default proof uses PTCS/proxy on `10.28.112.109` and native PingPong on `10.28.112.93`.
- Native PingPong now returns `fCell2.T` with schema `ptcs.dynamic.poc.pingpong.reply.v1`; the script-level proxy handler converts `fCell2.T -> fCell2.S` before returning `ActorArguTargetReply`.
- Added no-wait fail-fast checks for distinct local IPv4 hosts, recursive `JObject -> fCell2<string>` fallback for remote Akka serialization, and assertions for `fCell2.T->S` / `native-pingpong-fcell2-t`.
- Verification passed: `dotnet fsi .\src\GenFileActorInvocationTest4.fsx -- --if-dyna-port --no-wait --host 10.28.112.109 --cluster-host 10.28.112.109 --native-node-host 10.28.112.93 --native-node-port 0 --pcsl-root "G:\PulseTrade.fs.Comm.Log\verification\genfile4DualIpFCellT\run-202607071504" --delivery-profile genfile4-dual-ip-fcellt --actor-name genfile4-dual-ip-fcellt`.
- Evidence log: `G:\PulseTrade.fs\log\20260707\20260707144649.dynamic-genfile4-dual-ip-fcellt.op_log`.

## 2026-07-08 - Dynamic 0.1.3-beta66 live showcase address repair

- Fixed Actor Dynamic showcase template registration so `/user/showcase-dynamic-actor` is derived from the current host `ActorSystem` address instead of the stale `PulseTradeCommSpaDynamicPoc` POC address.
- Added narrow best-effort read-repair for stale showcase keys on actor-dynamic pages; the repair is scoped to keys ending in `/user/showcase-dynamic-actor` and must not block extension loading.
- Advanced package version to `0.1.3-beta66`.
- Verification passed: Release build completed with existing WebSharper/NU5123 warnings, Expecto passed 18/18, NuGet push returned `Created`, and formal 82 local-login browser proof on `/page/actor-dynamic-dd` rendered `Live Showcase`, `FSkynet 動態畫布 (Canvas)`, and `PulseTrade Actor Dynamic Dashboard` without `Timeout after`.
- Evidence: `G:\PulseTrade.fs\log\20260708\20260708133604.ptcs82-dynamic-timeout.00001.00001.log` and `G:\PulseTrade.fs\log\20260708\20260708133604.ptcs82-dynamic-timeout.op_log`.

## 2026-07-08 - Dynamic 0.1.3-beta67 direct SDUI echo and showcase2 actors

- Added `SduiEchoActor` at `/user/sdui-echo-actor`; Actor Dynamic Add actor key can send a raw `schema=fskynet-sdui` JSON DSL and receive the same payload back as `ActorArguTargetReply`.
- Added `ShowcaseDemoActor2` at `/user/showcase-dynamic-actor2`; it returns a richer `fskynet-sdui` payload with `data`, `sdui`, Rolling, Row, DataGrid, controls, AppLoader, Tree, and ContextMenu nodes.
- `CommHub.useDynamicSdui(...)` now spawns `showcase-dynamic-actor`, `sdui-echo-actor`, and `showcase-dynamic-actor2`, and prints their full Akka addresses for host/operator diagnostics.
- Advanced package version to `0.1.3-beta67`.
- Evidence: `G:\PulseTrade.fs\log\20260708\20260708141805.ptcs-dynamic-sdui-echo-showcase2.00001.00001.log`.

## 2026-07-08 - Dynamic 0.1.3-beta68 actors page projection for showcase actors

- Root cause: beta67 started real Akka actors for `/user/sdui-echo-actor` and `/user/showcase-dynamic-actor2`, but actors page reads PTCS actor registry projection, not raw `/user` children from Akka. The actors were callable but invisible on `/actors`.
- `CommHub.useDynamicSdui(...)` now projects the three long-lived Dynamic showcase actors through command-first `CommHub.RegisterActor` after spawning the real actors: `/user/showcase-dynamic-actor`, `/user/sdui-echo-actor`, and `/user/showcase-dynamic-actor2`.
- The projection includes current PTCS ActorSystem node address, `ptcs-dynamic-extension` role, and tags for `showcase`, `echo`, `showcase2`, `canvas`, and `complex-sdui`.
- Advanced package version to `0.1.3-beta68`.
- Evidence: `G:\PulseTrade.fs\log\20260708\20260708153544.ptcs-dynamic-showcase2-actor-registry.00001.00001.log`.

## 2026-07-10 - Dynamic 0.1.3-beta70 beta78 alignment

- Advanced `PulseTrade.Comm.Spa.Dynamic` to `0.1.3-beta70` with exact `PulseTrade.Comm.Spa [0.2.5-beta78]` consumption.
- Clarified the public demo actor contracts: `ShowcaseDemoActor2` always returns the built-in complex showcase and intentionally ignores caller marquee data; `SduiEchoActor` echoes a caller-provided SDUI DSL; `ShowcaseDemoActor` echoes valid SDUI and otherwise returns the simple showcase.
- Release build passed with existing WebSharper WS9002 and NuGet long-path/readme warnings. NuGet push returned Created.

## 2026-07-10 - Dynamic beta71 PTCS beta79 provider alignment

- Advanced `PulseTrade.Comm.Spa.Dynamic` from `0.1.3-beta70` to `0.1.3-beta71` and exact-pinned `PulseTrade.Comm.Spa [0.2.5-beta79]`.
- No renderer behavior changed. The release aligns Dynamic with PTCS's shared Actor Argu dispatch-provider seam so the PTC notes/00508 demo can use Add Actor Key against a native RN.Host `fCell2<string>` actor.
- Added `DYN-WBS-524` / `DYN-T-533`; Release build and package tests `18/18` passed. Cross-repo browser result is recorded by PTC after execution.

## 2026-07-10 - Dynamic beta72 NuGet bundle discovery and Canvas reply gate

- Advanced `PulseTrade.Comm.Spa.Dynamic` to `0.1.3-beta72`, still exact-pinned to PTCS `0.2.5-beta79`.
- Fixed extension script discovery for packages whose WebSharper assets are under `content/wwwroot/js`; local `wwwroot/js` and `contentFiles/any/net10.0/wwwroot/js` remain supported fallbacks.
- The Dynamic renderer now ignores outbound-only `argu msg:` history when deciding whether to open Canvas. Direct SDUI JSON and inbound `replied msg:` payloads remain supported.
- An isolated source copy was used because the generated `src\websharper.log` in this checkout was inaccessible to WebSharper cleanup. Release build/pack passed with existing warnings, Dynamic tests passed `19/19`, and the generated bundle was copied back to the tracked `src\wwwroot\js` output.
- PTC split-node Playwright MCP E2E rendered the RN-echoed notes/00508 payload in Canvas with zero browser console errors. Added `DYN-WBS-525` / `DYN-T-534`.

## 2026-07-10 - Dynamic beta72 release closeout

- `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta72` NuGet push returned `Created`。Downstream PTC verifier loaded the exact beta72 cache assembly and validated the generated bundle/classifier markers together with PTCS beta79。
- Product/source commit `57e21f2 Fix Dynamic NuGet bundle discovery` was pushed to `origin/20260710_027.ptcs_dynamic_beta79_alignment`。The isolated `.pcsl/dynamic-beta72-build-202607102220` build copy remains untracked generated evidence and is not part of the package source commit。

## 2026-07-11 - Dynamic beta73 PTCS beta80 dependency alignment

- Advanced `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta72 -> 0.1.3-beta73` and exact dependency `PulseTrade.Comm.Spa [0.2.5-beta79] -> [0.2.5-beta80]` for the durable agent-task submission policy seam.
- This is a dependency-only release. Target schemas, Dynamic renderers, bundle discovery and Canvas classification remain unchanged from beta72; PTC Host production adapter remains separately tracked by PTC3-068F.

## 2026-07-11 - Dynamic beta73 NuGet push accepted

- Repacked `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta73` from source commit `98864663c588682e42186bc00f294102ff5fab28`; nuspec dependency is exact `PulseTrade.Comm.Spa [0.2.5-beta80]`.
- Existing `PostBuildEvent.ps1` push returned `Created` / `Your package was pushed`. The flat-container did not list beta73 within the first three minutes, so propagation remains pending and the package was not pushed again.

## 2026-07-11 - Proposed realtime TA Canvas runtime

- Added `doc/TAResearch/REQ.md`, `doc/TAResearch/SA.md` and `RFC-PTCS-DYNAMIC-0007` for immutable SDUI documents plus bounded typed snapshot/patch frames, TA row vocabulary, local viewport state, resync and five-second client-pull lifecycle.
- Extended `SDUI_DSL_zh-Hant.md` with `sdui-runtime.v1` envelope/patch/node/reset/poll semantics. Existing static `fskynet-sdui` remains compatible; the prior `RealtimeChart` document term was not an implemented renderer.
- New TA runtime code must be pure WebSharper F# and use typed codecs; it cannot extend the current `JS.Inline` dynamic-object pattern or depend on Plotly JavaScript.
- PTCS core still needs a companion authenticated WebSocket mount/unmount/target-submit/transient-frame seam. Until accepted, HTTP polling or history-appending updates are not valid E2E acceptance.

## 2026-07-11 - Realtime TA Canvas RFC split into reusable packages

- Revised `RFC-PTCS-DYNAMIC-0007` and completed `doc/TAResearch/REQ.md`, `SA.md`, `SD.md`, `Test.md` and `WBS.md`.
- The accepted review proposal now separates transport-neutral `PulseTrade.Comm.Spa.Dynamic.Contracts`, pure WebSharper `PulseTrade.Comm.Spa.Dynamic.Renderer`, and the current PTCS-specific Dynamic facade. Existing `CommHub.useDynamicSdui` remains a compatibility entry.
- E2EQ is planned to consume Contracts + Renderer through an E2EQ adapter and parity migration; it does not import PTCS.Host, fCell2, ACL or MessageFabric dependencies.
- This batch changed documents only. Package split, renderer implementation, PTCS transient seam, E2EQ migration, NuGet release and browser tests remain planned.

## 2026-07-11 - Realtime TA Canvas RFC accepted for DEV

- Accepted `RFC-PTCS-DYNAMIC-0007` and rebuilt TA Test/WBS tracking with legacy readiness `DYN-TA-00A`, explicit priorities and `@DYN-TA-*` detail files.
- `DYN-TA-00A` closes common DSL/direct static Canvas/strict schema/invalid-node prerequisites before new Contracts/Renderer code. Public OAuth and production RN service evidence remain in their owning WBS.
- Playwright acceptance now specifies first-viewport chart dominance, compact query/Add Row workflows, local interaction with zero network effects, stale/error/resync recovery, desktop/mobile geometry and PTCS/E2EQ parity.

## 2026-07-11 - Dynamic TAResearch legacy readiness closure

- ActorsPage classifier由token `IndexOf`改為strict JSON discriminator，package negative tests與真host ActorsPage Playwright通過。
- 修復invalid canonical Argu string使FormInput消失；錯誤可見且保留template-default controls。
- Showcase/SduiEcho actors補native `fCell2<string>` request/reply，direct actor key Canvas與explicit proxy target兩條路徑均由既有F# Playwright verifier通過。
- Dynamic Expecto 21/21、WebSharper Release build通過；`DYN-TA-00A`完成。

## 2026-07-11 - Dynamic TA runtime contracts and reducer

- 新增packable `PulseTrade.Comm.Spa.Dynamic.Contracts 0.1.0-alpha3`，包含transport-neutral frame/action/TA vocabulary、strict codec/limits、pure last-good reducer、runtime registry與poll/dispose lifecycle；無WebSharper/PTCS/fCell2/PTMD/SQL dependency。
- Exact-package Contracts tests 7/7通過，涵蓋五種frame roundtrip、unknown/unsafe/oversize fail-closed、duplicate/gap/base/target resync、ResetView/ResetCanvas、one-in-flight poll、registry dispose與hard limits。
- nupkg已複製至SDK 10.0.301 library-packs；未使用ProjectReference consumer。Renderer/UI尚未開始，本切片不宣稱Playwright或visual gate完成。
- 精確停止stale `wsfscservice`後，既有Dynamic facade正常Release WebSharper build成功，legacy Expecto 21/21通過；build generated bundle whitespace未納入本切片。

## 2026-07-11 - Dynamic TA Renderer alpha1 milestone

- Contracts升至`0.1.0-alpha4`：browser-facing numeric由`decimal`改為JSON number/`float`，query range改canonical ISO-8601 string；host/server仍負責domain time validation。此變更移除WebSharper不支援decimal construction的根因。
- 新增packable `PulseTrade.Comm.Spa.Dynamic.Renderer 0.1.0-alpha1`，pure WebSharper F#實作七種TA row、bounded chart stack、compact responsive query/Add Row UI、local pan/zoom/toggle/reset-view與typed remote action callback；無JavaScript escape hatch或PTCS/fCell2/PTMD/SQL dependency。
- exact-package Renderer tests 7/7、Contracts alpha4 tests 7/7；browser demo改用exact Renderer NuGet。F# Playwright在1440x900與390x844完成操作/geometry/console gate，desktop primary chart位於y=228且寬1416；截圖為ignored `artifacts/ta-renderer-playwright/*.png`。
- `DYN-TA-003`目前72%；crosshair/cursor values、研究級indicator detail、完整freshness/error/in-flight visual states仍未完成，不宣稱Renderer final。

## 2026-07-11 - Dynamic.Ptcs transient server adapter alpha2

- Added packable `PulseTrade.Comm.Spa.Dynamic.Ptcs 0.1.0-alpha2` with exact PTCS beta82 and Contracts alpha4 dependencies. The adapter decodes typed client frames, passes server-derived session context to a host backend, validates returned runtime frames and applies the canonical reducer per session/extension/channel.

## 2026-07-11 - bounded PTCS TA browser adapters

- Advanced `PulseTrade.Comm.Spa.Dynamic.Ptcs` to `0.1.0-alpha3` with explicit `ta-browser.v1` bounded wire while retaining legacy recursive-wire compatibility; exact-package tests pass 4/4.
- Added `PulseTrade.Comm.Spa.Dynamic.Ptcs.Client 0.1.0-alpha2`, a pure WebSharper same-origin `/sync/ws` adapter that projects bounded TA state into the shared Renderer and emits typed actions. Exact-package model tests pass 2/2.
- WebSharper 10.1.5.674 initially crashed without diagnostics because a stale `wsfscservice` had remained alive since 22:33. After stopping the helper, the canonical descriptive project `src/PulseTrade.Comm.Spa.Dynamic.Ptcs.Client` builds normally; no short-path workaround or disabled compiler is retained. Build then `pack --no-build` is the verified sequence.
- Real PTCS host mount, ack/in-flight UI, polling/reconnect/resync and desktop/mobile Playwright remain open under `DYN-TA-004/006`; PTCS.Host was not changed in this slice.
- Exact-package tests passed 3/3: recursive SDUI wire roundtrip, document/snapshot reducer state, same-channel cross-session isolation, invalid payload fail-closed and disconnect cleanup.
- Browser adapter is not claimed complete. WebSharper 10.1.5 terminates without diagnostics when the recursive generic browser wire or PTCS beta81/82 metadata enters the legacy Bundle merge. Legacy Dynamic remains on PTCS beta80; the next adapter slice uses a bounded non-recursive TA browser wire and requires Playwright acceptance.

### Correction - alpha2 / bounded adapter paragraph boundary

- The two bullets immediately above this correction that mention `3/3` and “Browser adapter is not claimed complete” are the remaining alpha2 status notes. The later authoritative state is alpha3 server tests `4/4` plus Ptcs.Client alpha2 tests `2/2`; real-host Playwright is still open.

## 2026-07-12 - Dynamic TA Renderer shared cursor and status milestone

- Advanced Renderer to `0.1.0-alpha3` and Ptcs.Client to `0.1.0-alpha4` with exact Renderer `[0.1.0-alpha3]`; local nupkgs were copied to SDK `10.0.301` library-packs, without ProjectReference or public NuGet push。
- Renderer now provides seven-row shared crosshair/cursor values, readable HTML time axes, freshness/watermark/quality/recoverable error presentation, remote in-flight disablement and stale last-good preservation。Exact-package tests pass Renderer `10/10` and Ptcs.Client `2/2`。
- F# Playwright passed desktop 1440x900 and mobile 390x844 operation/geometry/console gates. Human screenshot review found SVG text compression on mobile; the final alpha3 moves time labels outside scaled SVG so B1/B25/B48 remain readable。
- Canonical Renderer project WebSharper compilation was blocked by an access-denied generated `websharper.log` handle. A disposable non-git shadow copy of the same canonical dirty tree was used only for full WebSharper compile/pack; canonical F# compile and all package-consumer tests ran in place. This is environment evidence, not a retained worktree or source of truth。
- `DYN-TA-003` advances to 88%。Research-grade DMI/MACD multi-line details, patch focus/viewport evidence and the full Delayed/Backfill/Unavailable browser matrix remain open；real PTCS polling/reconnect/resync remains under DYN-TA-004/006。

## 2026-07-12 - PTCS TA transient lifecycle alpha4/alpha5

- Advanced Dynamic.Ptcs to `0.1.0-alpha4` and Ptcs.Client to `0.1.0-alpha5`；exact local packages were copied to SDK 10.0.301 library-packs without public NuGet push。
- Bounded `ta-browser.v1` now preserves watermark、quality、lag seconds and reason code。Server/client exact-package tests both pass `4/4`。
- Added a pure typed `TaClientLifecycle` and WebSharper interpreter for mounted handshake、one-in-flight action/poll、timeout retry、active suspension、bounded reconnect、full snapshot resync and terminal dispose。`mountByIdWithOptions` returns a handle with runtime state、SetActive and Dispose；existing `mountById` remains compatible。
- Canonical F# build and disposable-shadow full WebSharper compile/pack passed with no raw JavaScript or HTTP polling。True PTCS shell mount、host restart reconnect、500 bars/20 polls and browser resource/history evidence remain open, so DYN-TA-004 advances only to 74%。

## 2026-07-12 - true PTCS transient TA browser gate

- Added `PulseTrade.Comm.Spa.Dynamic.Ptcs.LiveDemo` using real `CommHub + CommSpaActorFabric + Server.start`, same-origin `/sync/ws`, registered extension assets and no HTTP polling/fake host path。
- Advanced packages to Dynamic.Ptcs `0.1.0-alpha6-win1`, Renderer `0.1.0-alpha5` and Ptcs.Client `0.1.0-alpha7-win4`。Browser revision JSON uses JS-safe numbers and server-side finite integer validation；dispose waits for the extension close response before closing its dedicated socket。
- Exact-package tests pass: server adapter 5/5, Renderer 11/11, client 5/5。F# Playwright passes desktop/mobile three-row 500-bar rendering, 20 polls, suspend/resume/dispose, compact cursor geometry, mobile SMA scroll, PCSL event count 0 and zero console/page errors。
- The close gate found upgraded-stream corruption in FAkka.WebSocket/Suave: a valid Close frame was followed by `HTTP/1.1 404` after the continuation returned。FAkka.WebSocket win16 now waits for client TCP shutdown after replying Close；PTCS beta85 consumes win16 and the gate passes。
- DYN-TA-003/004 advance to 94%、DYN-TA-006 to 45%、aggregate to 59%。Host-restart last-good resync、present-invalid visual behavior、E2EQ adapter/parity and release push remain open。

## 2026-07-12 - E2EQuotation Dynamic TA adapter isolation

- Added packable root packages `PulseTrade.MarketData.E2EQuotation.Dynamic.Adapter 0.1.0-alpha1` and `PulseTrade.MarketData.E2EQuotation.Dynamic.Browser 0.1.0-alpha2` with exact Dynamic Contracts/Renderer dependencies；browser alpha2 aligns the action allowlist and rejects null/non-finite wire values。
- E2EQ exact-package tests pass 187/187 for bounded snapshots, canonical document/action mapping, local-view preservation, server/browser `dataRef`/action parity, fractional revision and non-finite point fail-closed。
- DYN-TA-T-013 contract parity is complete；DYN-TA-005 advances to 55%。Browser parity and AgentE2E remain blocked because a clean legacy E2EQ main WebSharper merge exits `-532462766`；an incremental stale bundle is explicitly rejected as evidence。

## 2026-07-12 - Dynamic beta74 canonical static payload classifier

- Added typed `SduiPayloadKind` classification for absent/unrelated、static Canvas、FormInput、ActorsPage、runtime v1 and present-invalid SDUI reason codes；ActorsPage strict gate now delegates this canonical classifier。
- Package tests pass 23/23。DYN-TA-007 advances from 20% to 40%；browser absent/present-invalid visual proof remains open and is not inferred from the non-UI tests。
- Advanced `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta73 -> 0.1.3-beta74` with exact PTCS dependency `[0.2.5-beta80]` unchanged；local pack/library-packs alignment is required before root script consumers run。
- beta74 full WebSharper build/pack then passed，the repo README is now packaged as NuGet readme，nupkg was copied to SDK 10.0.301 library-packs，and root PTC revision-10 bundle verifier loaded exact beta74 from NuGet cache and passed README/classifier/assets。No public push or formal service deployment is claimed；DYN-TA-008 advances to 35%。
## 2026-07-12 - PTCS beta86 package alignment

- Advanced `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta74 -> 0.1.3-beta75` and `PulseTrade.Comm.Spa.Dynamic.Ptcs 0.1.0-alpha6-win1 -> 0.1.0-alpha6-win2`; both exact-pin `PulseTrade.Comm.Spa [0.2.5-beta86]`.
- This is dependency-only alignment for the ActorArgu terminal-observer package chain. SDUI classification, renderer behavior and transient wire contracts are unchanged.
- Release builds passed and Dynamic tests passed 23/23 with the established checked-in-bundle recipe. Preexisting generated JS/test-project content hashes were unchanged and those files were not staged by this slice.
- NuGet push returned Created for both packages.

## 2026-07-12 - PTCS TA query metadata and action wire closure

- Removed renderer demo query literals。`TaWorkspaceDocument.DefaultView` now drives BTCUSDT/interval/range query draft through the bounded `ta-browser.v1` wire；poll frames do not overwrite an in-progress draft。
- Canonicalized Add Row kind to lowercase browser wire text and made the server parser case-insensitive，preventing `Sma` from silently becoming Candlestick。
- Released and pushed Renderer `0.1.0-alpha7`、Dynamic.Ptcs `0.1.0-alpha6-win4`、Ptcs.Client `0.1.0-alpha7-win8`；downstream Host client `0.1.0-alpha6` exact-pins win8。All four pushes returned `Created`；public NuGet indexing remained pending at immediate readback。
- Tests passed：Renderer 12/12、Dynamic.Ptcs 5/5、Ptcs.Client 6/6、PTCS.Host focused 24/24。PTCS.Host real-SQL F# Playwright passed FormInput、query readback、Add SMA Row、Apply、20 polls、desktop/mobile geometry and stable PCSL metric at `G:\PulseTrade.fs.Comm.Log\verification\ptcsHostTaResearchLive\run-d095bba2885846d0aa88a755f3a2d92c`。
- DYN-TA-003/004 advance to 98%、DYN-TA-006 to 65%、DYN-TA-008 to 70%。Remaining gates are restart/resync、E2EQ parity、static invalid visual proof and formal service alignment。
- Final docs-aligned package repack/push advanced the current versions to Renderer `0.1.0-alpha8`、Dynamic.Ptcs `0.1.0-alpha6-win5`、Ptcs.Client `0.1.0-alpha7-win9` and Host client `0.1.0-alpha7`；all four pushes returned `Created`。

## 2026-07-12 - TA controlled recovery, row removal and reconnect gate

- Renderer `0.1.0-alpha9` adds compact typed TA row removal controls without JavaScript. The control is disabled with remote actions and dispatches `SduiAction.RemoveTaRow`; Renderer tests pass 12/12.
- Ptcs.Client `0.1.0-alpha7-win10` preserves FormInput and the last-good canvas on controlled query failure, then clears the error after a valid action. A second fresh browser context now proves reconnect/open produces a complete FormInput/Canvas state rather than a sequence-gap resync loop; client tests pass 6/6.
- Dynamic.Ptcs `0.1.0-alpha6-win6`, Renderer alpha9, Ptcs.Client win10 and Host client alpha8 were packed/pushed with exact dependency metadata. The deployed F# Playwright gate passed From-only error, recovery, Add/Remove SMA, Apply, Reset, desktop/mobile geometry and fresh-context reconnect at `G:\PulseTrade.fs.Comm.Log\verification\ptcsHostTaResearchLive\deployed-channel-rebase-20260712204956`.
- Canonical WebSharper project directories still contain generated `websharper.log` files whose ACL/owner blocks replacement. Source-only clean build mirrors prove the package graph and deployed behavior, but canonical generated-log ownership remains an explicit tooling blocker rather than a resolved condition.

## 2026-07-12 - Deployed restart and browser history bound

- The formal beta87 Host process was replaced and the same Dynamic TA FormInput/Canvas flow recovered without a sequence-gap resync。A second browser context still bootstrapped from channel sequence 1。
- Cross-repo `PTC-VFY-027` revision 5 reads IndexedDB through Playwright CDP only；20 transient polls left all PTCS browser object-store counts unchanged。Evidence：`G:\PulseTrade.fs.Comm.Log\verification\ptcsHostTaResearchLive\deployed-restart-indexeddb-202607122114`。
- A side-by-side beta87 rolling candidate passed Dynamic error/recovery、Add/Remove、Reset and poll rendering while canonical service health remained continuous。External reverse-proxy cutover and E2EQ browser parity remain outside this evidence。

## 2026-07-13 - Composite rows and browser delta wire v2

- Added additive `TaTraceSpec` contracts and composite Candlestick/Line/Histogram rendering while retaining legacy `Traces=[||]` behavior. Renderer `0.1.0-alpha11` restores legacy row-kind titles and renders the Host four-row 17-trace layout.
- Dynamic.Ptcs `0.1.0-alpha6-win10` emits `ta-browser.v2` full/delta state, exact base revision, changed-point upserts and rolling prefix tombstones. Each browser bootstrap series is bounded to the latest 200 points while canonical RuntimeState remains complete.
- Ptcs.Client `0.1.0-alpha7-win12` validates the delta base, merges by timestamp and requests resync on mismatch. Dynamic.Ptcs and Ptcs.Client focused tests pass 7/7 each.
- Formal PTCS Host Playwright gate passed four rows, 17 series, desktop/mobile, error recovery, typed actions and fresh reconnect at `G:\PulseTrade.fs.Comm.Log\verification\ptcsHostTaResearchLive\deployed-composite-win10-final-202607130030`.

## 2026-07-13 - Proposed reply summary / inline / fullscreen adapter

- Added proposed `RFC-PTCS-DYNAMIC-0008` and draft `REQ-PTCS-DYNAMIC-TA-0002` to move TA runtime presentation from a page-level mount back into per-reply PTCS message cards.
- The proposal defines strict plain/static/runtime classification, domain-aware TA summaries, lazy inline mount, existing fullscreen preservation, per-reply state isolation and one logical transient channel across inline/fullscreen modes.
- Collapsed replies do not mount chart DOM or poll. Inline Canvas delegates vertical scrolling to the PTCS chat timeline; snapshot/delta remains transient and does not create new reply/history rows.
- This entry records design only. RFC status remains Proposed pending review; no implementation or package version change is claimed.

## 2026-07-13 - TA axes and cross-row cursor contract correction

- Extended RFC-PTCS-DYNAMIC-0008 and REQ-PTCS-DYNAMIC-TA-0002 with per-row Y-axis, single shared X-axis and same-bar cross-row cursor/readout requirements.
- The PTCS review mock demonstrates the corrected interaction in inline, fullscreen and mobile layouts. Playwright verified four Y axes, one shared X axis, cursor movement across three bars, synchronized price/DMI/MACD values and zero current-page console warnings/errors.
- The mock uses CSS hover zones only to review presentation. Production Dynamic must resolve pointer coordinates through the real time scale and must not implement discrete mock zones or handwritten JavaScript.
- This remains design/review evidence; no Dynamic package version or production renderer status changed in this slice.

## 2026-07-13 - Bootstrap state and PTCS beta89 package alignment

- Renderer `0.1.0-alpha12` no longer treats every `Document=None` as terminal unavailable. Normal channel bootstrap now reports preparing/connecting/loading/retrying/resyncing；only nonrecoverable error is terminal.
- Renderer tests pass 13/13；Dynamic.Ptcs and Ptcs.Client pass 7/7 each，and root Dynamic tests pass 23/23. Existing generated-log ACL remains a tooling constraint；staging build evidence does not claim that ACL issue is fixed.
- Published Dynamic.Ptcs `0.1.0-alpha6-win12` and Dynamic `0.1.3-beta78`, both exact-pinning `PulseTrade.Comm.Spa 0.2.5-beta89` where applicable. Formal 81/82 service now consumes those versions.
- The currently observed page-level TA mount with zero chat replies is not accepted behavior；it remains the next RFC-0020 / RFC-PTCS-DYNAMIC-0008 implementation slice.

## 2026-07-14 - DYN-TA-011 reply-owned TA presentation closure

- Implemented the accepted RFC/REQ/SA/SD boundary: Dynamic registers presentation/input renderers but does not select Plain/Form or own the chat timeline.
- Production RuntimeFrame classification now handles direct, JSON-string, canonical fCell2 envelope, fCell2.A and Case/Fields shapes with bounded controlled fallback. Collapsed TA shows only instrument/range/scale/indicator/freshness summary and does not mount chart/open channel/poll.
- Added direct detached-element mount for reply cards, per-reply Collapsed/Inline/Fullscreen handles, and close/resync state that does not discard an in-flight full snapshot. Package suites passed: Contracts, root, Ptcs, Renderer and PtcsTaClient.
- Formal 82 F# Playwright evidence `G:\PulseTrade.fs.Comm.Log\verification\ptcsHostTaResearchLive\beta96-acl-readback-20260714025738` passed strict decode, summary, four-row chart, lazy lifecycle, reconnect and Plain/Form boundary. Public NuGet push credential is the only remaining DYN-TA-011 release blocker.

## 2026-07-14 - DYN-TA-012 temporary closeout

- Renderer/model 15/15 passes with loaded/visible range, horizontal navigator, follow-latest, pointer hit-test, four-row shared-X cursor and current-bar OHLC/DMI/MACD values.
- Dynamic.Ptcs.Client and Dynamic.Ptcs each pass 7/7 against the new exact package graph.
- Formal PTCS 82 remains blocked outside renderer ownership: the old RN deployment does not return terminal completion to the new PTCS chat projection, and the formal process memory gate failed. No E2EQ code was changed.
- Dynamic `0.1.3-beta97`, Dynamic.Ptcs `0.1.0-alpha7-win32` and Dynamic.Ptcs.Client `0.1.0-alpha8-win40` were pushed successfully to NuGet.org.

## 2026-07-14 - Correction: DYN-TA-012 formal gate completed

- The prior RN/PTCS blocker is resolved with PTCS beta111, Dynamic beta100 and RN DurableProxy alpha60. Formal `ta.research.query` terminal completion reaches the PTCS chat projection without the accepted-inline bridge.
- Formal 82 F# Playwright passed 200 loaded/48 visible, four rows/17 traces, navigator, inline/fullscreen, shared-X cross-row cursor/floating values, five transient polls and second-context reconnect.
- Same-PID memory stayed within the 1024/512 MiB limits (total +542 MiB, reconnect +420 MiB). DYN-TA-012 is complete; long-running/E2EQ work remains separately tracked and was not advanced in this closeout.
- Evidence: `G:\PulseTrade.fs.Comm.Log\verification\ptcsHostTaResearchLive\run-formal-doc-only-202607142115`.

## 2026-07-15 - DYN-TA-013 2000-point viewport closure

- Released Renderer `0.1.0-alpha19`, Dynamic.Ptcs `0.1.0-alpha7-win39` and Dynamic.Ptcs.Client `0.1.0-alpha8-win47` with exact package references.
- `ta-browser.v3` separates a 2000-point authoritative full snapshot from the 200-point stable delta cap. The browser keeps the loaded range locally while mounting only the bounded visible window.
- The horizontal navigator now previews on `input` and commits one chart render on release/`change`; shorter SMA/ADX/MACD warm-up traces align by timestamp rather than array index.
- A rejected transient command returns `CommandRejected` without terminating a healthy WebSocket, so the next request can succeed on the same connection.
- Package gates passed Renderer `17/17`, Ptcs.Client `8/8` and Dynamic.Ptcs `7/7`. Formal 82 evidence `G:\PulseTrade.fs.Comm.Log\verification\ptcsHostTaResearchLive\run-ta2000-final-bounded-win39-alpha45-20260715022108` loaded 2000 points, rendered 48 candles, passed drag/release/head-tail/reconnect and remained within the memory gates.

## 2026-07-15 - DYN-TA-014 overview/editor/reset/copy closure

- Renderer alpha24 adds a bounded 2000-point overview, left/right resize handles, move region, 48/200/All selection and draft-during-drag/commit-on-release rendering.
- Add Row now exposes stable typed parameters: SMA/DMI period, ADX DI+ADX periods and MACD fast+slow+signal. Poll no longer closes or rewrites the editor; remove/re-add and initial-command Reset Canvas pass.
- Dynamic Ptcs.Client win52 consumes PTCS beta112 typed reply action support and copies canonical SDUI JSON without changing expand/mount/poll state.
- Renderer model passes 19/19, isolated F# Playwright passes, and formal 82 artifact `G:\PulseTrade.fs\Libs\PulseTrade.Comm\.pcsl\verify.ptcsHostTaResearchFormal82.20260715111500` passes 2000 loaded points, clipboard, typed rows, reset, inline/fullscreen, mobile, reconnect and memory gates.
- NuGet.org accepted Dynamic beta101, Renderer alpha24, Dynamic.Ptcs win40 and Ptcs.Client win52 (`Created`); immediate flat-container indexing remained propagation-pending.
- Follow-up flat-container HEAD returned 200 for all four Dynamic versions; public indexing is complete.

## 2026-07-15 - DYN-TA-015 full runtime export / draft query / slot cursor closure

- Durable `SduiDocument`維持compact provider/query/layout metadata，不包含OHLCV或indicator points；下載改為由authenticated transient state組成`ptcs-ta-research-export.v1`，並以`yyyyMMddHHmmss-GUID.json`交付。
- Collapsed reply平時仍不mount/poll；明確下載使用bounded one-shot open/bootstrap/full/close。Interval/日期欄位維持local draft，只有Apply送一次typed action並重render。
- Renderer `0.1.0-alpha25`統一line、K棒與cross-row cursor的slot-center geometry，fallback line width為1.25。Dynamic.Ptcs `0.1.0-alpha7-win41`與Ptcs.Client `0.1.0-alpha8-win55`已發布並由Host exact-pin。
- Ptcs.Client win54曾暴露WebSharper GUID字串與zero-data collapsed bootstrap缺陷，已由win55取代。Package gates Renderer `20/20`、Ptcs `7/7`、Ptcs.Client `8/8`與LiveDemo WebSharper build均通過。
- 正式F# Playwright artifact `G:\PulseTrade.fs\Libs\PulseTrade.Comm\.pcsl\verify.ptcsHostTaResearchExport.alpha53.final.20260715150500`實際下載729355-byte JSON並回讀2000筆完整資料；Apply、cursor、row order、ACL、reconnect與memory gates通過。

## 2026-07-15 - DYN-TA-016 editor poll / Reset regression closure

- Renderer `0.1.0-alpha27`把document/editor shell與runtime status/chart更新拆開；純poll不再替換Add Row原生select，chart cache同時使用document/data revision與transport sequence，避免reopen時漏掉same-revision full frame。
- Ptcs.Client `0.1.0-alpha8-win57`只在Document宣告`poll-delta` capability時排程週期poll；static document在open、stray due與reactivate均保持zero-poll。
- Renderer `20/20`、Ptcs.Client `9/9`及Host focused `31/31`通過。正式F# Playwright在82 port跨兩次poll保留`ta-add-row-kind` focus，連續刪除DMI與兩列MACD後一次Reset完整恢復四列17 traces。
- 正式release為`live81-82-ptcs-beta112-ta-editor-reset-alpha55-win57-20260715153000`；artifact為`G:\PulseTrade.fs\Libs\PulseTrade.Comm\.pcsl\verify.ptcsHostTaEditorPollReset.alpha55.final.20260715153500\artifacts`。NuGet.org public push因本機未提供API key回401；local immutable packages與正式service不受影響，public publication仍待具備核准secret的release流程。

## 2026-09-04 - PTCS beta116 dependency alignment

- Published dependency-only compatibility slices `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta105`, `PulseTrade.Comm.Spa.Dynamic.Ptcs 0.1.0-alpha7-win45`, and `PulseTrade.Comm.Spa.Dynamic.Ptcs.Client 0.1.0-alpha8-win61` against exact `PulseTrade.Comm.Spa 0.2.5-beta116`.
- Added explicit `FSharp.Core 10.1.301` references so package metadata does not drift with the active .NET SDK. Renderer/SDUI wire behavior is unchanged.
- Updated LiveDemo and focused package consumers to the same exact graph. Release builds/packs passed; NuGet pushes returned successful creation. A stale WebSharper compiler service lock caused one transient `websharper.log` access failure; stopping that project-scoped compiler process and rebuilding closed it.

## 2026-09-04 - DYN-TA-017 Notebook production source envelope

- Added `RFC-PTCS-DYNAMIC-0013` and synchronized TA REQ/SA/SD/WBS/Test/Verification for the four-agent owner boundary. Dynamic consumes owner-normalized projection data; it does not duplicate Daedalus `StructuredSeries` types or reference MDCQ/TradeCore/FsStl/FCell2/SQL.
- Added `SourceSnapshotEnvelope`, `SourceEventEnvelope`, strict codec/validation and `SourceProjection`. Valid events delegate payload changes to an injected domain reducer; duplicate is a no-op, while gap/identity/revision/order/reducer conflicts retain last-good state and return a typed snapshot request.
- Contracts tests advanced from 7/7 to 11/11. Canonical Contracts and Interactive.Client full WebSharper rebuilds passed. A no-WebSharper test build can overwrite the assembly without JS metadata, so the full downstream gate now requires rebuilding Contracts before Interactive.Client.
