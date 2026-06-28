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
