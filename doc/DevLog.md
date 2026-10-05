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

## 2026-09-04 - DYN-TA-017 generic editor and action lifecycle

- Added transport-neutral Text/Integer/Decimal/Boolean/Choice/Scale/List/Group editor schema with recursive hard-limit, key, range, safe-payload and default-value validation.
- Added single-in-flight correlated action request/result lifecycle. Revision conflicts do not submit; mismatched results fail closed; accepted/rejected feedback never mutates the authoritative document.
- Stable row identity remains `TaRowSpec.RowId`; same-kind parameterized rows coexist while duplicate row ids reject the frame. Contracts advanced to 12/12 and Contracts/Interactive.Client full WebSharper rebuilds passed.

## 2026-09-04 - DYN-TA-017 temporal projection and action channel

- Added validated `TemporalPoint` metadata and bounded `ta-browser.v4` PTCS wire so multi-scale rows retain source interval, scale, observation/availability frontier, preview/final state, quality and explicit projection semantics.
- Renderer now draws multiple candlestick traces in one row. Coarse candles span base slots, repeated lines align to each base bucket, and causal step values appear only after source interval close; cursor metadata identifies the source interval.
- Added `ptcs-dynamic-action.v1` request/result frames. Interactive.Client now sends correlated requests and fails closed on busy, send failure, 30-second timeout, disconnect or request-id mismatch. Accepted results do not mutate authoritative state; the host must publish the new `RuntimeFrame` revision.
- Exact local package graph: Contracts `0.1.0-alpha11`, Renderer `0.1.0-alpha32`, Dynamic.Ptcs `0.1.0-alpha7-win50`, Ptcs.Client `0.1.0-alpha8-win68`, Interactive.Client `0.1.0-alpha2`. Packages were copied to SDK 10.0.301/10.0.400 library-packs; no public push is claimed.
- Gates passed Contracts `15/15`, Renderer `22/22`, PTCS `10/10`, Ptcs.Client `12/12`, Interactive.Client/BrowserDemo/LiveDemo Release WebSharper builds. Playwright MCP verified desktop/mobile multi-scale presentation, generic editor pending/accept lifecycle and zero console errors. Daedalus SessionHost and real owner-backed DIB remain external production gates.

## 2026-09-04 - Interactive.Client browser artifact packaging correction

- Daedalus integration review found that Interactive.Client alpha2 carried only DLL metadata, so a SessionHost package reference could compile without receiving the browser application artifact. Backend compile was therefore insufficient evidence of browser completion.
- Interactive.Client alpha3 now packages `client.js`, `client.min.js`, WebSharper `Runtime.js` and a versioned bundle manifest under `contentFiles/any/any/ptcs-dynamic-interactive/`, with `copyToOutput=true`. The application module contains Contracts/Renderer/UI/FSharp logic and only imports the packaged Runtime.
- `scripts/verify-interactive-client-package.fsx` validates package entries, manifest, nuspec copy metadata, action protocol marker and external-import boundary. Revision 1 passed; package SHA-256 is `33b14a9f03c93af1d26851f64c4ac0b50da69610b786af1ed5f3436cffbb1cbd`.

## 2026-09-04 - Daedalus FSharp.Core consumer compatibility correction

- Daedalus integration restore exposed that Contracts alpha11 inherited `FSharp.Core >= 10.1.400` from the active SDK while the owner repository and first-party graph intentionally pin `10.1.302`. The leaf packages now disable the implicit FSharp.Core reference and publish exact `[10.1.302]` dependencies: Contracts `0.1.0-alpha12`, Renderer `0.1.0-alpha33`, Interactive.Client `0.1.0-alpha4`.
- Contracts 15/15 and Renderer 22/22 passed with 10.1.302 consumers. Interactive.Client and BrowserDemo full WebSharper builds passed; package verifier revision 2 passed with SHA-256 `93a644db23022ed9f3755a39b0a474f191c4a8db396c607daa3821f958247f85`. The three immutable packages were copied to SDK 10.0.301/10.0.400 library-packs; no public push is claimed.
- `ApplyTemplate` remains the authoritative prepare/swap/release operation in the Daedalus Interactive.Extension controller. PTCS.Dynamic transports typed actions/results and renders state; it does not introduce a competing workspace mutation implementation.

## 2026-09-04 - PTCS beta117 exact dependency alignment

- Advanced the active Dynamic package graph to `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta106`, `PulseTrade.Comm.Spa.Dynamic.Ptcs 0.1.0-alpha7-win52`, and `PulseTrade.Comm.Spa.Dynamic.Ptcs.Client 0.1.0-alpha8-win70`. Unpublished win51/win69 local artifacts were superseded after NuGet cached their pre-alignment nuspecs; they are not release versions.
- All three packages now consume exact `PulseTrade.Comm.Spa 0.2.5-beta117`; the adapters also align to current Contracts alpha12 / Renderer alpha33 and exact FSharp.Core 10.1.302. The LiveDemo graph is aligned to the same versions. No Dynamic renderer, SDUI wire, ActorArgu target or transient-channel behavior changed.
- Build/package/publication evidence is recorded by the 2026-09-04 exact-dependency operation log.

## 2026-09-05 - DYN-TA-017D authoritative row reconfigure closeout

- Completed the transport-neutral editor catalog and stable-row binding path across Contracts, Renderer, PTCS server/client wires and the browser demo. Add creates a new authoritative row；Edit preloads persisted values and applies the same RowId；rejection preserves the row and draft.
- Fixed the navigator release race by retaining one SVG during drag and updating selection/handles through dynamic attributes. The F# Playwright gate now uses bounded state waits and the actual shared temporal-axis label；two consecutive desktop/mobile runs passed Add/Edit/reject/remove/reset, navigator release, seven-row shared cursor and zero console/page errors.
- Exact local package graph：Contracts `0.1.0-alpha15`、Renderer `0.1.0-alpha37`、Dynamic.Ptcs `0.1.0-alpha7-win54`、Ptcs.Client `0.1.0-alpha8-win73`、Interactive.Client `0.1.0-alpha8`。All exact-pin FSharp.Core `[10.1.302]`；packages were copied to SDK 10.0.301/10.0.400 library-packs. No public NuGet push is claimed.
- Gates passed Contracts `15/15`、Renderer `22/22`、PTCS `11/11`、Ptcs.Client `13/13`、Interactive bundle package manifest/nuspec verifier、BrowserDemo and PTCS LiveDemo full WebSharper builds. Intermediate local-only candidates alpha13/14、Renderer alpha34..36、Ptcs win53、Ptcs.Client win71/72 and Interactive alpha5..7 are superseded and are not release evidence.
- DYN-TA-017D is 100% complete. Daedalus-owned chart-root/resource transition and the real MDCQ DIB/Playwright gate remain DYN-TA-017E/F；this closeout does not claim those external production gates.

## 2026-09-06 - FSharp.Core 10.1.400 production consumer compatibility

- Re-published the transport-neutral package set without changing the domain contract: Contracts `0.1.0-alpha16`, Renderer `0.1.0-alpha38`, Interactive.Client `0.1.0-alpha9`. All three exact-pin `FSharp.Core [10.1.400]`; Renderer exact-pins Contracts alpha16 and Interactive.Client exact-pins the same Contracts/Renderer pair.
- Contracts tests pass `15/15`, Renderer tests pass `22/22`, Interactive bundle package verifier revision 4 passes with SHA-256 `cfe09b2bdc71b90c42079039bfab6423c846ac17442bb15a5feaa6bbbd3d1bbb`. All three packages were copied to SDK 10.0.400 library-packs and NuGet returned `Created`.
- `ApplyTemplate` remains `(canvasId, rowId option, templateKey, values)` and does not acquire source/instrument/IndicatorSlot fields. Daedalus owns authoritative edit lookup and new-row source inference/rejection in the Interactive Extension controller.

## 2026-09-08 - DYN-TA-017G retention/resync atomic candidate

- `RuntimeReducer`改以完整ordered patch計算不可見candidate data，再驗受影響series的最終retained count；移除逐operation以原state估算造成的same-key與trim/append誤判。
- 新增`DYN-TA-T-066`：same-key at limit與trim後append均接受；多個upsert合計超限則atomic reject、保留last-good data/revision並要求full resync。
- Contracts完整WebSharper build通過，Expecto `16/16`通過；`DYN-VFY-018`三項deterministic diagnostics均PASS、findings=0。已發布Contracts `0.1.0-alpha17`、Renderer `0.1.0-alpha39`、Interactive.Client `0.1.0-alpha10` exact package graph；Renderer `22/22`、BrowserDemo full WebSharper build與bundle package verifier均通過。下一個Aster-owned slice為`BaseRowId` shared event-time cursor與`VisibleRangeChanged`。

## 2026-09-08 - DYN-TA-017G BaseRow event-time cursor/range

- Contracts新增validated `TaWorkspaceDocument.BaseRowId`、`SharedCursorChanged`與`VisibleRangeChanged`。range使用`[startEventTimeUtc, endEventTimeExclusiveUtc)`並限制`MaximumBasePoints <= 4000`；兩者沿用correlated action與single-pending lifecycle。
- Renderer改以BaseRow真實timestamp作shared axis；其他row只呈現finalized containing、finalized as-of或missing，不讀未完成coarse point。viewport release才送range action，pending期間controls不可重入；pending/feedback不再觸發2000-point chart重建。
- PTCS server/client `ta-browser.v4` wire同步BaseRowId及新增actions。Current exact graph為Contracts `0.1.0-alpha19`、Renderer `0.1.0-alpha45`、Interactive.Client `0.1.0-alpha16`、Dynamic.Ptcs `0.1.0-alpha7-win55`、Ptcs.Client `0.1.0-alpha8-win78`，全部exact-pin FSharp.Core `[10.1.400]`。
- Gates：Contracts `17/17`、Renderer `24/24`、PTCS `12/12`、Ptcs.Client `14/14`、兩個full WebSharper demo build與Interactive bundle verifier通過；F# Playwright及Playwright MCP desktop/mobile驗event-time actions、pending lock、無overflow/overlap與console 0。real MDCQ/DIB仍由DYN-TA-T-063/064驗收。
- 五個current package push均回`Created`；首次NuGet flat-container讀取仍為404 propagation pending，待索引後再完成public dependency readback。

## 2026-09-08 - Correction: DYN-TA-017G package indexing complete

- NuGet flat-container已可讀取Contracts alpha19、Renderer alpha45、Interactive.Client alpha16、Dynamic.Ptcs win55與Ptcs.Client win78 nuspec；五版的FSharp.Core與Contracts/Renderer/PTCS dependencies均與current exact graph一致。

## 2026-09-08 - DYN-TA-017G Interactive.Client lifecycle

- Interactive.Client新增pure `InteractiveClientLifecycle`及single `Client.Application` Start/Dispose API。socket generation隔離stale callback，斷線以bounded backoff重連；replacement transport只送一次Mounted/full snapshot，snapshot timeout會淘汰該generation並重試。重連期間保留同一last-good RuntimeState/renderer。
- 新增exact-package unit與獨立WebSocket LiveDemo/F# Playwright verifier。Unit `4/4`；force-drop gate量得maximum active connection `1`、full snapshot request `1`、resync前`LAST GOOD V1`保留、完成後`RESYNCED V2`，Dispose後active `0`/unmounted `1`；Playwright MCP desktop/mobile geometry及本次console通過。
- Current package `PulseTrade.Comm.Spa.Dynamic.Interactive.Client 0.1.0-alpha18` exact依賴Contracts `[0.1.0-alpha19]`、Renderer `[0.1.0-alpha45]`、FSharp.Core `[10.1.400]`。DYN-TA-017G已100%；real DIB/MDCQ production acceptance仍由Daedalus/MdcQuoteAgent owned DYN-TA-017E/F追蹤。

## 2026-09-08 - Correction: Interactive.Client recovery completion semantics

- TCP/WebSocket open不足以證明workspace恢復；Interactive.Client alpha19新增`SnapshotAccepted` lifecycle event，僅在reducer接受authoritative Snapshot後重設reconnect backoff。可接受socket但無法完成resync的端點因此保留遞增退避。
- Exact-package lifecycle unit `4/4`、package verifier revision 8與F# Playwright/Playwright MCP force-drop gate通過；maximum active connection `1`、full snapshot request `1`、Dispose後active `0`/unmounted `1`、console 0。Package SHA-256為`72238a5117b2ab5b3bf755713ab44bde79d76a7f60ddee980f1d8d47c6b6fa4f`，NuGet push回`Created`；flat-container propagation尚待索引。

## 2026-09-08 - Correction: Interactive.Client alpha19 indexing complete

- NuGet flat-container已可讀取Interactive.Client alpha19 nuspec；FSharp.Core `[10.1.400]`、Contracts `[0.1.0-alpha19]`與Renderer `[0.1.0-alpha45]` exact dependencies符合current graph。

## 2026-09-08 - DYN-TA-017H shared temporal axis

- Contracts新增versioned `TemporalAxis`/`TemporalSeries`、axis/series patch operations、`TaWorkspaceDocument.TemporalAxisRefs`與five-component `TaCandleDataRefs`。Position只作join key；sparse gap不補值，同一current-K preview以相同Position與新revision原位替換。malformed/missing axis、revision mismatch或unknown Position會保留last-good並要求resync；legacy `temporal-point.v1`維持相容。
- Renderer可由同一axis上的O/H/L/C/V五條scalar series合成candlestick。PTCS `ta-browser.v5`直接傳shared temporal values，不展開成legacy timeline/columns；client保留axis revision與candle refs。working set hard cap升為4000，3820 positions x 28 series deterministic frame低於16MiB。
- Current exact graph為Contracts `0.1.0-alpha20`、Renderer `0.1.0-alpha46`、Interactive.Client `0.1.0-alpha20`、Dynamic.Ptcs `0.1.0-alpha7-win56`、Ptcs.Client `0.1.0-alpha8-win79`。五顆NuGet push均回`Created`；local nuspec readback確認FSharp.Core及自有package edges皆exact，public flat-container仍在indexing propagation。
- Gates：Contracts `19/19`、Renderer `25/25`、Interactive lifecycle `4/4`、PTCS `13/13`、Ptcs.Client `15/15`，五專案與兩個LiveDemo full build通過。Interactive package verifier revision 10 hash為`8e4555a06e710853b90eeb6dbbdcd3354c91acb8396ec591ecdbe77f6700d6cc`。混合legacy/shared BrowserDemo的F# Playwright desktop/mobile通過，shared SMA SVG path非空；內建Browser runtime無可用instance，未宣稱Playwright MCP gate通過。
- 已透過Comm `msg-fsi-fb6b5f44e4c747d69b899d89a7736828`交付Daedalus exact producer contract與版本，等待其真MDCQ/DIB 3820x28 frame、browser OOM與Playwright結果完成production T-070。

## 2026-09-08 - Correction: DYN-TA-017H public NuGet indexing complete

- Contracts `0.1.0-alpha20`、Renderer `0.1.0-alpha46`、Interactive.Client `0.1.0-alpha20`、Dynamic.Ptcs `0.1.0-alpha7-win56`與Ptcs.Client `0.1.0-alpha8-win79`的public flat-container均已回`200`。
- Public nuspec readback確認Contracts/Renderer/Interactive.Client與PTCS adapter edges皆符合exact package graph；不重複push。T-070只剩Daedalus real MDCQ/DIB 3820x28與Playwright MCP gate。

## 2026-09-08 - DYN-TA-017H generic 3,820 x 28 browser capacity

- BrowserDemo改為實際建立3,820-position shared temporal axis及28條shared scalar series；既有七列TA畫面仍只投影所需refs，capacity markers供F# verifier確認state確實進入browser。
- 3,820密度下48-bar overview selection暴露固定handle互相覆蓋的操作缺陷；Renderer alpha48改用互斥left-resize/move/right-resize geometry，視覺selection不再攔pointer。F# Playwright驗平移保留48 bars、左右resize與All；Playwright MCP desktop/mobile驗viewport、geometry與console 0。
- Exact packages Renderer `0.1.0-alpha48`、Interactive.Client `0.1.0-alpha22`、Ptcs.Client `0.1.0-alpha8-win81`均已push且回`Created`。Renderer 25/25、Interactive lifecycle 4/4、Ptcs.Client 15/15及三個full demo build通過；public indexing另由Verification revision 11追蹤。Real MDCQ/DIB 3,820×28仍由Daedalus gate，未宣稱production完成。

## 2026-09-08 - Correction: generic capacity release indexing complete

- Renderer alpha48、Interactive.Client alpha22及Ptcs.Client win81的public flat-container nuspec均已回`200`；FSharp.Core、Contracts、Renderer與PTCS dependencies均符合local exact graph，不重複push。

## 2026-09-08 - Shared temporal patch producer conformance

- Canonical SD補充producer/controller責任：保存committed axis revision；同一ordered patch內更新axis及全部相依series；未變series使用empty-items pin新revision；超過operation上限或無法保證完整相依集合時改送authoritative snapshot。
- 擴充既有DYN-TA-T-069，以兩條series證明漏掉相依revision update會atomic reject、保留last-good並要求resync；補empty-items update後接受且未變series值保持。Contracts suite維持`19/19`通過，未修改public API或重發NuGet。
- Read-only consumer review另發現Daedalus shared-axis projector目前只支援snapshot，以及不同priority的同Position interval衝突未完整拒絕；已透過Comm交付owner修正，不在Aster repo越界修改。

## 2026-09-09 - Contracts Visual Studio Release auto-push hook

- `PulseTrade.Comm.Spa.Dynamic.Contracts`工作樹版本由既有`0.1.0-alpha20`調整為`0.1.1`；本輪未push。其fsproj啟用`GeneratePackageOnBuild`。Windows上的Visual Studio Release build在Pack完成後，使用`G:\PulseTrade.fs\Libs\Akka.Proc.Supervisor\PostBuildEvent.ps1`將精確PackageId的最新Release nupkg push至NuGet並同步library-packs。
- 一般CLI與Debug build預設不push；可用`PulseTradeCommSpaDynamicContractsPushNuGet=false`明確停用VS push。驗證只產生本機package，不觸發外部NuGet發布。

Correction：0.1.1 consumer exact-reference alignment尚未完成；既有`DYN-TA-008 Package/release closure`已提升為priority 0追蹤，不將本次pack成功誤述為整個package graph已結案。

## 2026-09-09 - PTCS 0.2.16 Host dependency cascade

- Dynamic direct PTCS consumers were aligned from exact `PulseTrade.Comm.Spa 0.2.15` to `0.2.16`; the Host-facing `PulseTrade.Comm.Spa.Dynamic.Ptcs` package was published as `0.1.3`.
- Main `PulseTrade.Comm.Spa.Dynamic` package was published as `0.1.5` for the formal SPA Host graph. This release is a dependency alignment for PTCS startup-memory/product-identity work and does not claim additional Dynamic UI behavior.

## 2026-09-09 - PTCS 0.2.18 dependency and package-integrity cascade

- Direct PTCS consumers now pin exact `PulseTrade.Comm.Spa 0.2.18`. Fresh build-first releases are Dynamic.Ptcs `0.1.5`, Dynamic.Ptcs.Client `0.1.3`, and main Dynamic `0.1.7`.
- `0.1.6` was superseded because pack-first can retain a previous Release assembly. Dynamic.Ptcs and Ptcs.Client were rebuilt before `pack --no-build`; main Dynamic was built from a source-identical staging copy because canonical generated `src/websharper.log` remained OS access-denied.
- The staging WebSharper build completed with only existing WS9002, and generated tracked JS hashes were identical to canonical source. NuGet pushes returned `Created`; no new Dynamic UI behavior is claimed by this dependency release.

## 2026-09-09 - M15 FloatingPoint collection and notebook acceptance gate

- Aster在Daedalus repo新增`Milestone15.FsStlMultiScaleFloatingPointCollections.dib`，以Interactive.Extension exact `0.1.0-alpha14`驗證三條公開projection API；canonical `FsStlSeries<FloatingPoint>`保留SnapshotId/EventTime/AvailableAt，顯式時間鍵sequence、Deedle Frame與NestedMap皆可進shared temporal axis。
- `yyyyMMdd`只允許1440K UTC calendar day，930K／1380K date-only須fail closed；無時間鍵collection不推測時間。package hash與Daedalus公告一致，`dotnet dib` package/collection/shared-axis markers全GREEN。
- real SPAA preflight使用文字FSSTL描述1K／5K／30K／60K bars及SMA／DMI／MACD／Heikin-Ashi。MDCQ Next恢復期間曾取得真實frames但仍可能`actor-resolve-failed`；Daedalus正在實作compile-time typed `TA_CHART`取代VALUE-name presentation inference。Notebook iframe與Playwright尚未完成，故DYN-TA-017E/F不宣稱production完成。

## 2026-09-09 - M15 typed chart real-provider and browser gate

- current SPAA source已以compile-time typed `TA_CHART`作display selection authority；M15 fixed-range真MDCQ run回2 frames、7個宣告順序row、1條SMA overlay及4條shared temporal axes，axis points為`763/3820/127/63`。
- Playwright MCP確認1K K棒與SMA同列、5K DMI、30K MACD histogram、60K Heikin-Ashi、shared hover、DMI hide/show與console 0；畫面截圖為Daedalus repo Playwright artifact `.playwright-mcp/page-2026-09-09T01-28-03-377Z.png`。
- repeat request另重現MDCQ ES access role先timeout再退出；MdcQuoteAgent已接手root cause。此批只把E/F提升為partial，不宣稱OPEN_END/ACK、repeat generation、exact win139/alpha15或notebook iframe已完成。

## 2026-09-09 - Authored TA row label 與 Interactive bundle 0.1.3

- root cause是SPAA已將`TA_ROW` label寫入`TaRowSpec.Options["label"]`，但Renderer的row card與toolbar分別使用trace labels與row kind。Renderer現統一precedence為authored row label、trace labels、typed row kind，並讓toggle、card、edit/remove tooltip及feedback共用同一顯示名稱。
- Renderer exact-package focused suite `26/26`、source-identical WebSharper BrowserDemo與Playwright MCP通過；toolbar與row card均呈現`ES 1K + SMA(20)`，console warning/error為0。
- 正式發布`PulseTrade.Comm.Spa.Dynamic.Renderer 0.1.3`與`PulseTrade.Comm.Spa.Dynamic.Interactive.Client 0.1.3`。Interactive bundle manifest與nuspec已同版，package gate及lifecycle `4/4`通過；Interactive package SHA-256為`8aaf9033a34981323884e319e25fb88cf04b005a978e893637b1ce197b3af045`。

## 2026-09-09 - M15 alpha15 typed FSSTL automation gate

- Aster acceptance notebook已以commit `70afe353`獨立交付，exact載入Interactive.Extension `0.1.0-alpha15`與TradeCore.FsStl `10.1.400-win139`。`dotnet dib`驗證canonical FloatingPoint series、time-keyed sequence、Deedle Frame、NestedMap、shared temporal axis與typed `TA_CHART -> FsStlTaView.compile -> ForQuoteSlot -> Decode`皆GREEN。
- typed view為7 rows、12 requests、12 dataRefs、minimum scale 1；dev.69後兩輪alpha14及一輪alpha15 fixed-range真SPAA均回2 frames／7 rows／4 axes，latest points=`3820,763,127,63`，未重現先前0-row／timeout／role exit。
- Daedalus確認M13才是production DIB：沿用真QuoteSlot/session並以`taView.ForQuoteSlot slotId`作最後expression。M15不複製`createBinding`或手動`Display`。本session Browser MCP仍無可控browser，因此alpha15 iframe/Playwright與OPEN_END history→live ACK維持未完成。

## 2026-09-09 - M15/M13 alpha16 integration evidence

- M15 exact pin已更新為Interactive.Extension `0.1.0-alpha16`；`dotnet dib`實際載入alpha16 assembly，FloatingPoint canonical series、time-keyed sequence、NestedMap、Deedle Frame、date-scale validation、shared temporal axis及typed multi-scale TA chart全數GREEN。真SPAA回2 frames／7 rows／4 axes，points=`3820,763,127,63`。
- Daedalus M13 fixed-history production notebook gate以真MDCQ通過3,820 committed bars、1/5/30/60K與76 TA series，stderr為空。Playwright MCP另驗桌機shared hover／hide-show／console 0；行動版確認canvas正常而SPAA fixed-grid top form overflow，已精確回報owner。
- OPEN_END adapter仍需保證bootstrap callback在initial frames被transport接受後才回`Ok`／ACK，並覆蓋sink reject、取消、bootstrap前失敗與single-use continuation；本批不宣稱History-to-Live完成。
- 後續執行M13真browser gate時，OPEN_END在iframe建立前失敗。MdcQuote EsK直測證明2026-07-07到current cut超過1K每尺度4,000筆上限；M13 realtime start沿用fixed fixture日期是立即根因。跨尺度長週期warm-up仍需provider owner決定per-scale range/tail policy，不能只縮短demo日期後宣稱production完成。

## 2026-09-09 - M15 alpha17 local integration gate

- Aster-owned M15 acceptance notebook已切至Interactive.Extension exact `0.1.0-alpha17` local integration artifact。`dotnet-dib`驗證canonical `FsStlSeries<FloatingPoint>`、time-keyed sequence、NestedMap、Deedle Frame、shared temporal axis與typed multi-scale TA view全GREEN；真SPAA回2 frames／7 rows／4 axes，points=`3820/763/127/63`。
- alpha17 provider-composed bridge focused gates已涵蓋初始ACK ordering、sink rejection不commit、timeout、cancellation與single-use continuation。package由Daedalus尚未提交的owner source產生，本筆不宣稱正式release。
- M13 OPEN_END仍受單一`StartUtc`同時承擔1K 4,000上限與30K/60K長週期warm-up的contract限制。running 18883也尚未載入responsive source；兩者都保留為owner E2E gate，不以fixed-history成功取代。

## 2026-09-09 - M15/M13 alpha18 lifecycle evidence

- M15已exact載入Interactive.Extension `0.1.0-alpha18`與owner build commit `03cc7860`；FloatingPoint collection、typed TA view及真SPAA fixed-range均GREEN，SPAA為2 frames／7 rows／4 axes，points=`3820/763/127/63`。
- alpha18 two-phase provider bridge focused tests新增live cancellation no-ACK/no-commit，連同initial ordering、sink reject、timeout、prepare cancellation與single-use皆通過。
- M13 OPEN_END改為`currentCut - 3999m`後可建立iframe，但MDCQ initial只有`committed=0/previews=4`，Playwright在180秒內無1K SMA trace。這只證明transport/lifecycle接通；per-scale/tail warm-history contract仍由MdcQuote owner處理。

## 2026-09-09 - DYN-TA-018 browser range cache foundation

- Accepted RFC-PTCS-DYNAMIC-0014並同步REQ/SA/SD/WBS/Test。Dynamic cache identity維持generic `{ OwnerFingerprint; SchemaRevision }`；SPAA以bounded SHA-256 `QueryFingerprint`映射exact identity，Program/DataSource/Query診斷與跨range secondary index仍由owner負責。
- `RuntimeCache`新增accepted-state entry、actual temporal-axis coverage、16MiB codec、write gate與non-authoritative rehydrate；Contracts `21/21`。`BrowserRuntimeCache`以純WebSharper F#提供IndexedDB write/readLatest/readCovering/clear與8-entry LRU，F# Playwright驗reload persistence、latest hit、oldest eviction、exact-owner coverage hit/miss、clear及console 0。
- 正式發布Contracts `0.1.4`、Renderer `0.1.6`、Interactive.Client `0.1.5`；exact dependency graph為Client -> Renderer `[0.1.6]` + Contracts `[0.1.4]`。Renderer `26/26`、Interactive lifecycle `4/4`與nupkg manifest verifier通過，三個NuGet push均回`Created`。
- Daedalus SPAA Client integration與真DIB reload/range E2E仍由DYN-TA-018D/E追蹤。MdcQuoteAgent確認dev.70尚無per-scale tail/lookback，預計dev.71提供fixed-cut typed selection；完成前OPEN_END不宣稱production ready。
- DYN-VFY-020 revision 2另以browser fixture直接注入corrupt IndexedDB record，驗證cache read回Miss、record被刪除且不產生console/page error；corrupt storage不再只靠source inspection宣稱。

## 2026-09-09 - Browser cache semantic validation investigation

- `log/20260909/20260909143300_issue_browser_cache_semantic_validation.hypothesis.md`記錄DYN-TA-018C的fail-closed缺口：browser read目前只做淺層decode，JSON合法但Document/Snapshot語意無效的entry仍可能被回報為Hit。後續實驗將以完整`RuntimeCache.validateEntry`作read gate，並補semantic-invalid IndexedDB record的F# Playwright回歸。
- 第一個實驗因整個`RuntimeCache`進入WebSharper graph而觸發`TemporalAxisCodec`非JavaScript type的`WS9001`，已完整撤回。`log/20260909/20260909144000_issue_browser_cache_validation_boundary.hypothesis.md`改採窄邊界：抽出browser-safe entry validator，coverage derivation維持server-only。

## 2026-09-09 - Browser cache semantic fail-closed gate

- Contracts新增browser/server共用的`RuntimeCacheEntryValidation`，保留coverage derivation於server-only `RuntimeCache`。Interactive browser read現在會拒絕並刪除JSON shape有效、但Document/Snapshot/reducer語意無效的IndexedDB record，不再將其回報為cache hit。
- Contracts `21/21`、Renderer `26/26`、Interactive lifecycle `4/4`通過。DYN-VFY-020 revision 3以F# Playwright驗證8-entry LRU、reload、coverage、malformed與semantic-invalid刪除、clear及console/page error 0。
- package graph升為Contracts `0.1.5`、Renderer `0.1.7`、Interactive.Client `0.1.6`；NuGet push與package hash待final package gate後補記。
- 2026-09-09：DYN-TA-018 browser application surface完成可獨立交付切片。Contracts `0.1.6`新增WebSharper-safe `RuntimeCacheProjection`，Interactive.Client `0.1.7`提供`BrowserRuntimeCache.writeAcceptedState`/`tryRehydrate`，Renderer `0.1.8`在`PausedForResync`保留local pan/zoom/hover/cursor並抑制remote range/cursor。修正SPAA identity contract：OwnerFingerprint由stable Program/DataSource/cache schema組成且不含range，QueryFingerprint只作診斷，range只進Coverage。
- 2026-09-09：Contracts 21/21、Renderer 26/26、PTCS adapters 13/13及15/15通過；Renderer與Interactive BrowserCache兩組F# Playwright通過。BrowserCacheDemo改用exact package驗新API可由NuGet consumer編譯/執行；PTCS LiveDemo full WebSharper build通過。NuGet push均回Created：Contracts `0.1.6`、Renderer `0.1.8`、Interactive.Client `0.1.7`、Dynamic.Ptcs `0.1.6`、Ptcs.Client `0.1.4`；public flat-container readback待propagation。

## 2026-09-09 - DYN-TA-018 SPAA fixed-history cache E2E

- Public flat-container已可讀Contracts `0.1.6`、Renderer `0.1.8`、Interactive.Client `0.1.7`、Dynamic.Ptcs `0.1.6`與Ptcs.Client `0.1.4`；Interactive.Client nuspec exact依賴Contracts `[0.1.6]`與Renderer `[0.1.8]`。
- M15 collection/typed view與M13 fixed-history真MDCQ gate通過；M13得到3,820 committed bars、四尺度、76 TA series與28 visible refs。Playwright MCP驗desktop/mobile、viewport、row toggle與1K/5K shared hover，TA runtime console為0。
- SPAA `18883` double-run race最後READY且兩次inspect只建立一次provider run；reload後為`CACHE READY`，只有inspect、沒有第二次runs，console 0。Owner source已加入inspect/cache/provider callback generation guard及stale run best-effort stop。
- OPEN_END cache仍需owner定案source revision與resume/full語意。Aster只提出generic cache projection選項，不在未對齊前實作或宣稱完成。

## 2026-09-09 - DYN-TA-018 OPEN_END finalized cache and renderer replacement

- Daedalus與Aster定案OPEN_END authority：cache只保存每個temporal axis的Final位置，所有temporal series依自身axis同步裁切；rehydrate只供display-first、保留current `DataRevision`並進`PausedForResync`，owner仍立即重連authoritative full/provider，cache revision不作continuation authority。
- Contracts 0.1.7新增`RuntimeCacheProjection.finalAxisProjection`、`projectTemporalSeries`與`tryFinalizedProjection`。T-072/073及BrowserCache F# Playwright驗finalized prefix、preview-only fail closed、axis/series一致與revision-continuation=false。
- Renderer修正same-revision Document/Canvas替換仍重用舊DOM/route的根因，並將高尺度candle投影成actual base-axis constituent slots；source/storage仍只有canonical candle，不補不存在的slot。Renderer 0.1.12 unit 27/27與3,820-position F# Playwright通過。
- final focused gates：Contracts 21/21、Renderer 27/27、Dynamic.Ptcs 13/13、Interactive.Client 4/4、Ptcs.Client 15/15；Interactive lifecycle、BrowserCache與Renderer三組F# Playwright及WebSharper builds通過。final Playwright MCP runtime當下回`No browser is available`；同source interim gate已通過，但未將其冒充0.1.12 final MCP結果。
- final exact graph均已push且public NuGet index可讀：Contracts 0.1.7、Renderer 0.1.12、Interactive.Client 0.1.9、Dynamic.Ptcs 0.1.7、Ptcs.Client 0.1.6。Daedalus已收到exact SHA/API，DYN-TA-018E待其以final packages重跑Milestone16 true-provider shared-axis與reload/range authoritative replacement gate。

## 2026-09-09 - Renderer shared cursor no-rerender correction

- Daedalus的真provider M16量得visible 48/source 3,820時單次cursor move約1,300ms，且`data-chart-render-sequence`由3變4；根因是`CursorIndex`位於main chart reconciliation key，mousemove會重算全部timeline/projection並重建7-row SVG。
- Renderer將cursor移至獨立`Var`；crosshair改為persistent SVG line搭配`Attr.Dynamic`只更新x/visibility，cursor detail由獨立`Doc.EmbedView`更新。document/series/viewport變更與remote `SharedCursorChanged`語意不變。
- Renderer 0.1.13 unit 27/27及3,820-position/28-series F# Playwright通過；7-row cursor為180ms、chart render sequence不變、desktop/mobile無overlap、console/page error 0。Interactive.Client 0.1.10 unit 4/4與lifecycle/cache Playwright通過；Ptcs.Client 0.1.7 unit 15/15及LiveDemo full WebSharper build通過。
- Renderer 0.1.13、Interactive.Client 0.1.10、Ptcs.Client 0.1.7皆經既有encrypted-key PostBuild流程push並回`Created`，且public flat-container可讀。Daedalus final exact-package M16仍待回讀，不在此紀錄冒充完成。

## 2026-09-09 - DYN-TA-017/018 final true-provider closure

- Correction：前一節的0.1.13/0.1.10/0.1.7不是最終圖。Daedalus M16雖確認no-rerender，但兩次真provider cursor latency為543ms/508ms，仍未達既定500ms gate；因此未以該結果關閉WBS。
- Renderer 0.1.14在chart建立時保存已解析且bounded的cursor readers，mousemove不再對28條完整series重做decode/resolution，並以`Array.tryFindBack`移除cursor lookup的暫存配置。generic F# Playwright直接量SVG crosshair `x1`變更為62ms、7列`x1`一致、chart sequence不變，desktop/mobile無重疊且console 0。
- Daedalus M16真provider原failing gate以final exact graph通過：loaded=3,820、visible=48、7 rows、projected candles、30K horizontal-step、SVG cursor 125ms、7列`x1`一致且chart sequence不變。M17亦通過loaded=4,000、current 1K preview、IndexedDB finalized-prefix cache hit、authoritative provider reattach及live revision 2->3，未發現generic package缺口。
- Final exact-package gates通過：Contracts 21/21、Renderer 27/27、Interactive.Client 4/4、Dynamic.Ptcs 13/13、Ptcs.Client 15/15；Interactive lifecycle與browser-cache F# Playwright分別驗single reconnect/resync/dispose及finalized-prefix/coverage/corrupt eviction/authority邊界。
- Final graph為Contracts 0.1.7、Renderer 0.1.14、Interactive.Client 0.1.11、Dynamic.Ptcs 0.1.7、Ptcs.Client 0.1.8。三顆本輪新package push均回`Created`且public flat-container/nuspec可讀；Renderer、Interactive.Client、Ptcs.Client SHA-256依序為`8D0E185BB7A8800EA23DFDD6EFFD8A44FFD7E77005731C71B2C420672A7AAC26`、`053883DDB8586FFE51D43C0DC53994FE3E26FE43F3758DB5983AF818901B9ACE`、`85F547AA9A99F60A42F40AC68EB17399DDFCA138CEB5B97F2DCE02704085779C`。
- `DYN-TA-017`與`DYN-TA-018`依真FSSTL/MDCQ `.dib`、browser、cache authority及immutable package gates改為100% Done；不將舊E2EQ/static/legacy package closure納入本次九月主線。

## 2026-09-09 - Overlay and separate-row shared series contract

- Daedalus確認同一FSSTL TA view需同時支援大小K overlay row與各尺度separate rows。Renderer model原本可逐row重用immutable series，但Contracts仍以Canvas-wide `duplicate-trace-data-ref`拒絕完整document；本輪移除此錯誤限制，保留document-wide `RowId`與row-local `TraceId`唯一規則。
- 新增Contracts與Renderer明確回歸：同一`series.1k`／`series.5k`可同時被overlay與separate rows引用，shared cursor仍獨立產生四個trace值。Contracts 22/22、Renderer 28/28、Interactive.Client 4/4、Dynamic.Ptcs 13/13、Ptcs.Client 15/15通過。
- Exact graph升為Contracts `0.1.8`、Renderer `0.1.15`、Interactive.Client `0.1.12`、Dynamic.Ptcs `0.1.8`、Ptcs.Client `0.1.9`。Interactive bundle manifest已與0.1.12對齊；package push/public readback於本log後續紀錄。
- 五顆package push均回`Created`，NuGet public flat-container已可讀全部exact版本。Daedalus已收到版本、contract與M18 owner compose gate交接訊息`msg-fsi-0a72fc890c1c497faccd356ba4f3054d`。

## 2026-09-09 - Live preview visible-close / chart hot-path correction

- 使用者在真`02-history-live-1k-5k-sma.fsstl`觀察到最新bar close不隨成交價變動，且cursor約每秒卡頓。SPAA actual API verifier已證明同一current 1K Position的close由`7656`變成`7655.75`，剩餘根因為Renderer把每個`DataRevision`都送入chart composition，替換整個SVG stack。
- Renderer改為分離chart topology state與live data state；同Position preview以WebSharper dynamic attributes原位更新candlestick/line/histogram及cursor reader，只有identity、DocumentRevision或visible timestamp topology改變才重建。新增unit明確區分same-position與appended-position。
- Renderer `0.1.17` unit 29/29及exact-package BrowserDemo F# Playwright通過：visible close `28278.2038 -> 28279.4538`、chart render sequence不變、latest rerun cursor 168ms、console/page error 0。原project full WebSharper build被不可存取的ignored `websharper.log`阻擋；source-identical isolated staging full compiler build通過，未將工具檔ACL問題誤稱source failure。
- 已發布Renderer `0.1.17`、Interactive.Client `0.1.13`、Ptcs.Client `0.1.10`並同步active demos/tests exact refs。Daedalus已收到版本與owner boundary，正以真SPAA重跑visible close/no-rerender/cursor gate；完成前DYN-TA-019維持95%。

## 2026-09-10 - Live preview sustained cursor hot-path correction

- Correction：Renderer `0.1.20`雖通過generic單次cursor/no-rerender gate，Daedalus以真SPAA 4,000 bars、12 SMA重測時第二次cursor為2650ms；12秒僅174個rAF sample／28次crosshair change。SPAA host CPU約6.97%，因此0.1.20不視為final。
- Renderer `0.1.21`將mousemove從chart-wide `cursorIndex Var/View`移出，以單一requestAnimationFrame latest-wins slot直接更新固定crosshair/time/value DOM；live geometry改為per-candle與per-trace結構比較，只通知實際變動的SVG element/path。
- 完整WebSharper編譯與exact-package gates通過：Renderer 29/29；Interactive lifecycle 4/4；Ptcs.Client 15/15。3,820 bars／28 series F# Playwright量得單次cursor 87ms、連續120 transitions共4919ms、最大246ms，same-position close原位更新、chart sequence不變、desktop/mobile console error 0。
- 發布candidate graph：Contracts `0.1.8`、Renderer `0.1.21`、Interactive.Client `0.1.17`、Dynamic.Ptcs `0.1.8`、Ptcs.Client `0.1.14`。三顆push均回`Created`；local package SHA-256依序為`9C7BFD8E7EA6BC6FDBE412212F20A10AB0B3B2E798068A045A28BBBC6D0B131A`、`A7BAFBCC68F7447BCD7E975E4DB8CCD4E50E195B1E3EE1797462F1C2565D54D3`、`AA62AF871F82F0144707201D9716F058847C32D79A1A5436DB4D39673CE831E0`。DYN-TA-019仍待Daedalus真`02-history-live-1k-5k-sma.fsstl` M17 sustained gate後才關閉。
- 三顆版本均已出現在public NuGet index；排除NuGet repository signing metadata後，public/local package entries逐項SHA-256 mismatch皆為0。

## 2026-09-10 - Incremental preparation and stable live value readers

- 真SPAA M17顯示Renderer `0.1.21`雖已移除cursor reactive fan-out，DataRevision仍會對全部retained data執行`prepareData`。Contracts `0.1.10`改為保留temporal prefix references並以operation-local revision驗證一次套用；T-078以精確4,000-point append/trim證明final axis/series revision一致且不要求resync。
- Renderer `0.1.25`改為按`dataRef`、shared prefix與changed axis position增量prepare。WebSharper會把單純轉呼叫mutable tuple函式的reader wrapper eta-reduce成初始snapshot；改以每條trace的mutable reader cell保存最新cursor/legend reader後，同Position preview可同步更新SVG與既有row-value node。
- UI補齊每列固定30px value band、label/value相鄰、`Undef`與長值不改row geometry，以及預設收合的chart-bottom跨尺度summary。F# Playwright以3,820 positions/28 series驗300次crosshair transitions共5602ms、max132ms、5次並行live preview、render sequence不變、historical close isolation與console/page error 0。
- Local exact candidate graph為Contracts `0.1.10`、Renderer `0.1.25`、Interactive.Client `0.1.21`、Dynamic.Ptcs `0.1.10`、Ptcs.Client `0.1.18`；focused suites依序23/23、29/29、4/4、13/13、15/15通過。已以comm訊息`msg-fsi-b2b0281c782b41798814143bc22c1912`交接Daedalus重跑真`02-history-live-1k-5k-sma.fsstl` M17；完成前不public push、不標final。

## 2026-09-14 - Dynamic.Ptcs stable PTCS 0.2.19 alignment

- `PulseTrade.Comm.Spa.Dynamic.Ptcs`由0.1.10升至0.1.11，exact dependency由PTCS 0.2.18升至0.2.19；Contracts維持0.1.10，無wire/reducer/renderer程式變更。
- Release package push回`Created`；transient adapter suite 13/13通過。PTC SPA Host改用0.1.11後Release restore/build無PTCS dependency warning。

### Correction - active client adapter dependency cascade

- LiveDemo package-consumer build另定位`PulseTrade.Comm.Spa.Dynamic.Ptcs.Client 0.1.18`仍exact PTCS 0.2.18；因此追加發布Ptcs.Client 0.1.19並同步兩個active test consumers。本段補充前述server adapter記錄，不取代或改寫它。
- Ptcs.Client 0.1.19 push回`Created`，client suite 15/15通過。LiveDemo移除PTCS dependency skew後Release build通過；僅保留既有WebSharper WS9002，首次build的generated-log access failure由停止明確持有該檔的project-scoped compiler helper修復。

## 2026-09-14 - PTCS 0.2.20 Management alignment

- `PulseTrade.Comm.Spa.Dynamic.Ptcs`由0.1.11升至0.1.12、`PulseTrade.Comm.Spa.Dynamic.Ptcs.Client`由0.1.19升至0.1.20；兩者將exact PTCS dependency由0.2.19升至0.2.20。
- 本次只對齊PTCS Management current-page select-all/deselect-all與bulk page visibility release，未改Dynamic contracts、renderer或wire behavior；active adapter tests與LiveDemo同步使用新exact graph。
- GW aggregate suite仍直接消費legacy `PulseTrade.Comm.Spa.Dynamic` package，因此同批將aggregate由0.1.7升至0.1.8並把exact PTCS由0.2.18升至0.2.20；不改aggregate功能碼。

## 2026-09-14 - PTCS 0.2.21 adapter alignment

- `PulseTrade.Comm.Spa.Dynamic.Ptcs`進版0.1.13，`PulseTrade.Comm.Spa.Dynamic.Ptcs.Client`進版0.1.21；兩者exact-lock `PulseTrade.Comm.Spa 0.2.21`，runtime code、renderer、contracts與Interactive行為不變。
- 兩個Release build皆0 warning/error，package已推送NuGet並供formal PTCS Host graph使用；Dynamic transient adapter 13/13與PTCS TA client 15/15測試通過。

## 2026-09-15 - PTCS 0.2.22 group contract alignment

- `PulseTrade.Comm.Spa.Dynamic.Ptcs`進版0.1.14、`PulseTrade.Comm.Spa.Dynamic.Ptcs.Client`進版0.1.22、aggregate `PulseTrade.Comm.Spa.Dynamic`進版0.1.9；三者exact-lock PTCS 0.2.22。
- 本次只承接PTCS durable group lifecycle binary contract，不改Dynamic renderer、SDUI DSL或Interactive行為。三個package均完成Release build/pack/push；aggregate僅保留既有WebSharper Bundle deprecation warning。
- Active test consumers同步至Dynamic.Ptcs 0.1.14、Ptcs.Client 0.1.22與PTCS 0.2.22；transient adapter 13/13、TA client 15/15通過，LiveDemo Release build僅保留既有WebSharper Bundle deprecation warning。

## 2026-09-15 - PTCS 0.2.23 Management contract alignment

- Aggregate `PulseTrade.Comm.Spa.Dynamic`進版0.1.10、`Dynamic.Ptcs`進版0.1.15、`Dynamic.Ptcs.Client`進版0.1.23；三者exact-lock PTCS 0.2.23。本次只承接bounded Management/participant batch binary contract，不改SDUI/TA runtime行為。
- 三個package均完成Release build/pack/push。Transient adapter 13/13與TA client 15/15通過；LiveDemo active references已同步。canonical目錄既有不可刪`websharper.log` ACL造成wsfsc啟動前失敗；相同fsproj/Client.fs/Program.fs在run-scoped乾淨目錄完整啟用WebSharper compiler建置通過，僅WS9002。

## 2026-09-16 - PTCS 0.2.26 physical-delete contract alignment

- Aggregate `PulseTrade.Comm.Spa.Dynamic`進版0.1.13、`Dynamic.Ptcs`進版0.1.17、`Dynamic.Ptcs.Client`進版0.1.25；三者exact-lock PTCS 0.2.26。本次只承接Management physical-delete binary contract，不改Dynamic renderer、SDUI DSL或TA runtime行為。
- 三個final nupkg均已推送並核對exact dependency。canonical目錄既有generated `websharper.log` ACL仍阻止WebSharper刪檔；aggregate與Ptcs.Client由排除該generated log的run-scoped source staging完整編譯/pack，未停用WebSharper compiler；Dynamic.Ptcs在canonical目錄直接完成。
- LiveDemo active refs同步至PTCS 0.2.26、Dynamic.Ptcs 0.1.17與Dynamic.Ptcs.Client 0.1.25。

## 2026-09-16 - PTCS 0.2.27 bulk-delete contract alignment

- Aggregate `PulseTrade.Comm.Spa.Dynamic`進版0.1.14、`Dynamic.Ptcs`進版0.1.18、`Dynamic.Ptcs.Client`進版0.1.26；三者exact-lock PTCS 0.2.27。本次只承接Management batch registry wire與bulk-delete binary contract，不改Dynamic renderer、SDUI DSL或TA runtime行為。
- 三個package已完成Release pack/push，LiveDemo refs同步。canonical generated `websharper.log` ACL仍阻止WebSharper compiler刪檔；因本輪沒有JS source變更，aggregate與Ptcs.Client以`WebSharperRunCompiler=false`重建server/client assembly與exact dependencies並沿用上一版已驗證bundle，未宣稱bundle重新產生。
- Active package tests同步至Dynamic.Ptcs 0.1.18與Dynamic.Ptcs.Client 0.1.26，避免測試仍消費0.1.15/0.1.23舊contract。

## 2026-09-16 - PTCS 0.2.32 adapter alignment

- `PulseTrade.Comm.Spa.Dynamic.Ptcs`進版0.1.23、`PulseTrade.Comm.Spa.Dynamic.Ptcs.Client`進版0.1.31；兩者exact-lock PTCS 0.2.32。本次只承接group Actions/Management/bounded activation binary contract，不改Dynamic renderer、SDUI DSL或TA runtime行為。
- 兩個package均已完成build/pack/push並進入formal Host graph；deployed artifact載入Dynamic.Ptcs 0.1.23，81/82/8798 health與PTCS focused browser gates通過。
- Active consumers同步：Dynamic.Ptcs.Tests與LiveDemo鎖0.1.23，PtcsTaClient.Tests與LiveDemo鎖Ptcs.Client 0.1.31，避免測試繼續驗舊contract。
- LiveDemo direct PTCS reference與contentFiles path同步0.2.32，消除adapter 0.1.23所需0.2.32被direct 0.2.27降版的NU1605。
## 2026-09-17 - PTCS 0.2.34 exact adapter alignment

- `PulseTrade.Comm.Spa.Dynamic.Ptcs` 0.1.24→0.1.25與`PulseTrade.Comm.Spa.Dynamic.Ptcs.Client` 0.1.32→0.1.33，兩者exact-lock PTCS 0.2.34。Release build各0 error，兩個NuGet package已push並由正式Spa.Host依賴鏈消費。本輪未變更Dynamic renderer或DSL行為。
- Active consumers同步：`Dynamic.Ptcs.Tests`與`LiveDemo`鎖Dynamic.Ptcs 0.1.25，`PtcsTaClient.Tests`與`LiveDemo`鎖Ptcs.Client 0.1.33；`LiveDemo`的direct PTCS引用鎖0.2.34，消除NU1605降版。Expecto實跑13/13與15/15通過，LiveDemo Release build 0 error；僅剩既有WS9002 Bundle警告。

## 2026-09-19 - PTCS 0.2.35 exact adapter alignment

- `PulseTrade.Comm.Spa.Dynamic.Ptcs` 0.1.25→0.1.26與`PulseTrade.Comm.Spa.Dynamic.Ptcs.Client` 0.1.33→0.1.34，兩者exact-lock PTCS 0.2.35；只調依賴，不改Dynamic renderer/DSL/TA邏輯。Release build/pack/push完成，NuGet flat-container HEAD 200，正式SPA Host graph已使用新版。
- Active `Dynamic.Ptcs.Tests`、`PtcsTaClient.Tests`及`LiveDemo`引用同步；LiveDemo的direct PTCS與build asset path更新為0.2.35（原path殘留0.2.32）。Expecto 13/13、15/15通過。LiveDemo一般Release WebSharper步驟因既有`websharper.log` ACL拒絕而失敗；未變動的前端源碼以`WebSharperRunCompiler=false`完成F# Release，正式SPA Host的實際WebSharper bundle/頁面另已通過Playwright gate，不宣稱LiveDemo本輪重生bundle。

## 2026-09-20 - PTCS 0.2.36 exact adapter alignment

- `PulseTrade.Comm.Spa.Dynamic.Ptcs` 0.1.26→0.1.27與`PulseTrade.Comm.Spa.Dynamic.Ptcs.Client` 0.1.34→0.1.35，兩者exact-lock PTCS 0.2.36；無renderer、DSL、schema或TA行為變更。
- 兩個package皆以clean tracked-source staging完成Release build/pack/push；原workspace的protected `websharper.log`未修改。PTCS Host已改用Dynamic.Ptcs 0.1.27、Ptcs.Client 0.1.35並build通過。
- Active `Dynamic.Ptcs.Tests`、`PtcsTaClient.Tests`與`LiveDemo`引用已同步；LiveDemo direct PTCS reference/content path鎖0.2.36。兩個test consumer exit 0，LiveDemo以不含受保護`websharper.log`的clean staging執行完整WebSharper build成功，僅既有WS9002警告。

## 2026-09-20 - PTCS 0.2.37 exact adapter alignment

- `PulseTrade.Comm.Spa.Dynamic.Ptcs` 0.1.27→0.1.28、`PulseTrade.Comm.Spa.Dynamic.Ptcs.Client` 0.1.35→0.1.36及aggregate `PulseTrade.Comm.Spa.Dynamic` 0.1.14→0.1.15，exact-lock PTCS 0.2.37；本輪只同步agent delivery authority相依圖，不改renderer、DSL、schema或TA行為。
- 三個package已完成Release build/pack/push；正式Spa.Host與TAResearch.Client 0.1.18使用同一exact dependency graph編譯部署。既有受保護`websharper.log`未修改。
- Active consumers同步：Dynamic.Ptcs.Tests與LiveDemo鎖0.1.28，PtcsTaClient.Tests與LiveDemo鎖Ptcs.Client 0.1.36；LiveDemo direct PTCS/content path鎖0.2.37。Expecto 13/13、15/15及LiveDemo Release build皆通過，後者0 warning/0 error。

## 2026-09-20 - PTCS 0.2.38 exact adapter alignment

- `PulseTrade.Comm.Spa.Dynamic.Ptcs` 0.1.28→0.1.29、`PulseTrade.Comm.Spa.Dynamic.Ptcs.Client` 0.1.36→0.1.37及aggregate `PulseTrade.Comm.Spa.Dynamic` 0.1.15→0.1.16，exact-lock PTCS 0.2.38；只同步isolated announcement authority相依圖，不改renderer、DSL、schema或TA行為。
- 三個package已完成Release build/pack/push。Active Dynamic.Ptcs.Tests、PtcsTaClient.Tests及LiveDemo consumers同步到新版；Expecto 13/13、15/15通過，LiveDemo 0 error且不再有PTCS 0.2.37 constraint mismatch，僅既有WS9002 warning。

## 2026-09-20 - PTCS 0.2.39 exact adapter alignment

- `PulseTrade.Comm.Spa.Dynamic.Ptcs` 0.1.29→0.1.30、`PulseTrade.Comm.Spa.Dynamic.Ptcs.Client` 0.1.37→0.1.38及aggregate `PulseTrade.Comm.Spa.Dynamic` 0.1.16→0.1.17，exact-lock PTCS 0.2.39；只同步structured MessageFabric rejection相依圖，不改renderer、DSL、schema或TA行為。
- 三個package已完成Release build/pack/push。Active Dynamic.Ptcs.Tests、PtcsTaClient.Tests及LiveDemo consumers同步；Expecto 13/13與15/15通過。
- LiveDemo完整WebSharper build的`wsfsc.exe`以CLR fatal code結束且沒有F# diagnostic；本輪未修改其前端源碼，正式SPA Host Release build已通過。此toolchain regression保留為未完成gate，不誤宣稱LiveDemo全綠。

## 2026-09-20 - PTCS 0.2.40 exact adapter alignment

- `PulseTrade.Comm.Spa.Dynamic.Ptcs` 0.1.30→0.1.31、`PulseTrade.Comm.Spa.Dynamic.Ptcs.Client` 0.1.38→0.1.39及aggregate `PulseTrade.Comm.Spa.Dynamic` 0.1.17→0.1.18，exact-lock PTCS 0.2.40；本輪只承接View As unread競態修正的binary graph，不改renderer、DSL、schema或TA行為。
- 三個package均以canonical tracked source的clean isolated staging完成Release WebSharper build/pack/push。canonical輸出目錄內既有`websharper.log` ACL拒絕讀取、改名及刪除，未繞過ACL或修改該generated log；fresh package bundle由staging重新產生，不沿用舊JS。
- 正式SPA Host已消費Dynamic.Ptcs 0.1.31與Ptcs.Client 0.1.39，Release build通過且無PTCS exact-version NU1608。Dynamic package source未變更功能，未另跑renderer E2E；PTCS Management與Group Chromium gates負責本輪行為回歸。

## 2026-09-21 - PTCS 0.2.46 read projection exact alignment

- `PulseTrade.Comm.Spa.Dynamic` 0.1.22→0.1.23、`PulseTrade.Comm.Spa.Dynamic.Ptcs` 0.1.35→0.1.36及`PulseTrade.Comm.Spa.Dynamic.Ptcs.Client` 0.1.43→0.1.44，exact-lock PTCS 0.2.46；本輪只同步WBS-086F persistent read projection binary graph，不改renderer、DSL、schema或TA行為。
- 三個package均完成Release build/pack/push。正式SPA Host消費Dynamic.Ptcs 0.1.36與Ptcs.Client 0.1.44並build/deploy通過。

## 2026-09-21 - Generic Marker runtime v2

- 依`RFC-PTCS-DYNAMIC-0015`完成transport-neutral generic marker：`TaTraceKind.Marker`、strict/bounded `ta-marker.v1` codec、same-row composite/split candle target、runtime v2 gate、candidate commit前validation、structured `RejectFrame`與last-good preservation。Position是唯一spatial authority，EventTime只作tooltip evidence。
- Renderer新增bounded SVG marker overlay、fixed lane/deterministic stacking、edge/row clipping與visible-only nodes；marker不參與reference timeline、Y autoscale、cursor value或numeric legend。專用F# Playwright在3,820 bars fixture通過desktop/mobile gate，200次cursor transition總3597ms、max54ms。
- 發布aggregate Dynamic `0.1.24`、Contracts `0.1.12`、Renderer `0.1.29`、Dynamic.Ptcs `0.1.37`、Ptcs.Client `0.1.47`、Interactive.Client `0.1.24`，NuGet均回`Created`。Interactive bundle manifest與package version一致，cache schema升至2。
- Focused suites通過：Contracts 27/27、Renderer 30/30、PTCS adapter 14/14、PTCS client 16/16、Interactive lifecycle 4/4；aggregate測試的Actor.Registry exact reference由legacy alpha5對齊主package `[0.1.3]`後24/24通過；package verifier與PTCS Host active consumer Release build通過。
- Daedalus consumer handoff為Contracts `[0.1.12]`／Interactive.Client `[0.1.24]`與Notebook runtime v2 parity；domain projection、Backtest transaction與SPAA state仍由consumer owner負責。
- `doc/Traceability.md`已加入RFC-PTCS-DYNAMIC-0015與DYN-T-538..544索引；canonical RFC只保留在本repo `doc/RFC`。

## 2026-09-21 - Generic Marker direction / hollow correction RFC

- 新增 proposed `RFC-PTCS-DYNAMIC-0016`，處理 consumer review 發現的三項 conformance 缺口：marker direction 與 anchor 解耦、`Outline` 真正透明，以及 bucket limit 文件語意一致。
- RFC 採 `ta-marker.v2` current encoder、v1 legacy arrow compatibility decoder與browser cache schema 3；另把跨 marker traces 的 lane collision 納入 candidate aggregate validation及 deterministic stacking gate。
- 本輪只完成 RFC 與測試矩陣，未修改 source、package version、NuGet 或 consumer project；待 review 接受後才同步 SA／SD／WBS／Test 並實作。

## 2026-09-21 - Generic Marker direction / hollow implementation

- `RFC-PTCS-DYNAMIC-0016`依consumer feedback定案並完成：direction與anchor正交、v1 arrow visual decode、v2 strict rejection、true hollow/full hit target、cache schema 3、wire bucket與跨trace aggregate lane雙層limit。
- Owner focused suites118/118與desktop/mobile F# Playwright通過；3,820-bar fixture的200次cursor transitions總5022ms、max91ms，marker不改candle refs、Y-domain、numeric legend、time slot或shared cursor hot path。
- 發布Contracts `0.1.13`、Renderer `0.1.30`、Dynamic.Ptcs `0.1.38`、Ptcs.Client `0.1.48`、Interactive.Client `0.1.25`與aggregate Dynamic `0.1.25`，NuGet均回`Created`。
- Aster active consumers同步：Host TA client `0.1.27`、E2EQuotation Adapter `0.1.0-alpha10`與Browser `0.1.0-alpha4`已發布；Host Release build、E2EQuotation focused 10/10與GW 494/494通過。Daedalus SPAA／Notebook與producer enablement仍為external handoff，不宣稱完成。

## 2026-09-22 - RFC-TRADECORE-0026 Renderer owner gate

- 新增`RFC-PTCS-DYNAMIC-0017`並完成cache repaint、row-qualified legend與full-range performance修正。cache rehydrate在identity/revision不變但validated Data object替換時會重畫，仍不偽造authoritative revision；legend reader改以RowId與local trace index分區。
- 3,820 bars All-mode改為bounded candle/line paths；non-base candle cursor由O(n²)逐timestamp反向掃描改為reverse range projection與path compression。Renderer BrowserDemo量得48→All 1,842ms、24條batched candle paths、200次pointer transition p95 29.35ms/max 102ms。
- Focused gates通過：Renderer 33/33、Interactive lifecycle 4/4、Ptcs.Client 16/16、Interactive package verifier與browser cache Playwright。Interactive package內確認包含新batched/projection bundle，manifest為0.1.26。
- 發布Renderer `0.1.33`、Interactive.Client `0.1.26`與Ptcs.Client `0.1.49`，NuGet push均回`Created`；Contracts維持`0.1.13`。local SHA-256依序為`F314C78601E12B51E5039746F8E9678AAC28A095578545BF34D43A8EE63D0B8E`、`1701C19BFD81BFBFF8799E811515027C113D2F7114B7FF815D2119D221BADEA9`、`31529AAF69C4A49B5D2C63FF08E761FA8EC92FC0B40ED89E4865581A62CD7AF4`；public flat-container仍在index propagation。
- Daedalus真MDCQ/SPAA gate須採新版exact packages後執行`Rfc0026.Spaa.Backtest.playwright.fsx`；本輪不以仍載入舊bundle的18883 host冒充新版consumer驗收。

## 2026-09-22 - RFC-TRADECORE-0026 ChangeQuery viewport owner completion

- 新增`RFC-PTCS-DYNAMIC-0018`：accepted `ChangeTaQuery`在host frames已合併後，依base/reference temporal axis及半開`[FromUtc, ToUtcExclusive)`選local window；invalid、gap/no-intersection與stale generation保留current viewport並回明確feedback，不改document/data identity、authoritative revision或cache。
- Renderer以單一in-flight query與replaceable latest queue配合正式client one-in-flight contract。連續Apply不再平行呼叫callback；舊response可完成canonical frame merge，但generation stale時不套viewport，settled後只送最新query。
- Focused gates通過：Renderer 34/34、Interactive.Client 4/4、Ptcs.Client 16/16；3,820-bar BrowserDemo callback主動拒絕concurrent submit，Q1/Q2 race最後window為2001..3000、loaded維持3,820、canvas identity不變。All=1,829ms、24 batched candle paths、pointer p95=47.70ms/max=206ms、console/page error 0。
- final exact packages已存在於NuGet：Renderer `0.1.35`（SHA-256 `4E0C09C61483515C98BC0C90CD998580F217B852E2AEC1E62856FF46AC6A6023`）、Interactive.Client `0.1.28`（`755FA0A14FF8611206A22C38D0FCFC83396FDF03BA30D9056B097A7B23A2A1F9`）、Ptcs.Client `0.1.51`（`ADA699CAE8050D7A446B11E121D2F18975F3BB6E56698C660B8349E1A9A54586`）；Contracts維持`0.1.13`。中間graph `0.1.34/0.1.27/0.1.50`不可採用。
- Daedalus須以final graph重跑真SPAA `Rfc0026.Spaa.Backtest.playwright.fsx`；owner fixture不宣稱consumer production gate完成。

## 2026-09-22 - Correction: ChangeQuery final exact graph

- 收尾code review補上UTC calendar date validation與`2026-02-31`負向測試；因此前一段的`0.1.35/0.1.28/0.1.51`已被取代，不是consumer final graph。
- final exact packages為Renderer `0.1.36`（SHA-256 `9DCF3DA6B5D9C576B0B0CB701FB7209CC3408ECA9ED76A559AE6B96CD113B03B`）、Interactive.Client `0.1.29`（`26461AD96C5F371DC943CD165CE079AF964E3F896D5CCC5882CB979EA2D1E98F`）、Ptcs.Client `0.1.52`（`442A05388DBD1E4956703C47E65A286CA108DC4F9E26D558C94460571229FD9C`）；三顆NuGet push均回`Created`。
- final2 clean staging gates：Renderer 34/34、Interactive.Client 4/4、Ptcs.Client 16/16與F# Playwright PASS；All=1,899ms、pointer p95=44.03ms/max=97ms、24 batched paths、query latest-window與identity invariants通過。

## 2026-09-22 - Renderer scheduling、row axis與progressive coverage owner completion

- 完成`RFC-PTCS-DYNAMIC-0019/0020`：Renderer preparation、row mount/refresh改為generation-aware frame scheduling；每列使用adaptive event-time axis與plot-bounded crosshair，loaded coverage與visible cap分離。
- boundary pan沿用`VisibleRangeChanged`要求document authority內相鄰範圍；3820→4220 merge後以event-time anchor重定位，visible維持不超過4000並顯示`Max 4000`。provider query、source merge/cache與真FSSTL E2E仍由Daedalus consumer持有。
- Focused gates通過：Renderer 37/37、Interactive.Client 4/4、Ptcs.Client 16/16、Interactive bundle verifier與F# Playwright。owner All/marker/document/progressive phase均無 >100ms task；300次cursor transition p95=72ms/max=165ms；Playwright MCP確認七列axis、coverage擴張與console 0。
- 發布Renderer `0.1.37`、Interactive.Client `0.1.30`、Ptcs.Client `0.1.53`，NuGet push均回`Created`。SHA-256依序為`9440140AE57DA41A0CE058DE4DF97E56C577A434871810447C02C12A2D6F5931`、`14109B5C85B6A675F37EFD27A102BAE9BCC4E6598991B2AF80FB2D5C98B6890E`、`45B3A6F3E589EA7820FD360255C324EBEA3B674E6BC8A091FB68E8F7179E5625`。
- Package gate在push前抓到Interactive manifest仍為0.1.29；修正為0.1.30、重打包並再次驗證後才發布。canonical checkout的WebSharper compiler仍無diagnostic崩潰，release採乾淨staging build；source與exact packages均已由focused tests驗證。

## 2026-09-23 - Consumer frame ingestion / row geometry owner release

- Daedalus真SPAA 3,819 x 7 workload在上一exact graph重現initial 879ms、48→All 117ms、backtest/accounting 176ms；這是consumer long-task evidence，不是owner fixture誤判。
- Contracts對大型合法snapshot採allocation-light unsafe fast scan，只在命中unsafe subtree時建立精確diagnostic path/list。Renderer以indexed line readers、direct candle slot buffer與single-pass eight-bucket candle paths降低row geometry中間配置；shared crosshair改為覆蓋整列SVG高度。
- Focused gates通過：Contracts 30/30、Renderer 37/37、Interactive.Client 4/4、Dynamic.Ptcs 14/14、Ptcs.Client 16/16。Owner browser All/marker/document/progressive max=53.07/48.75/29.60/61.02ms，無 >100ms task；cursor 300 transitions p95=89ms/max=305ms，無chart rerender，full-row crosshair geometry通過。
- 發布exact graph：Contracts `0.1.14`、Renderer `0.1.38`、Interactive.Client `0.1.31`、Dynamic.Ptcs `0.1.39`、Ptcs.Client `0.1.54`，NuGet push均回`Created`。Interactive nupkg gate確認manifest、`client.js`、`client.min.js`與WebSharper Runtime共4 entries，SHA-256 `218A560C66E1C709D7AEFE74C81EA1FC1364D907FF5A9A535BB4C118B0B8AA2E`。
- 新增`scripts/diagnose-spaa-consumer-long-tasks.fsx`作為真consumer CDP bounded attribution gate。DYN-TA-T-093仍等Daedalus以上述exact graph啟動persistent host重跑；本節不冒稱consumer production gate已關閉。

## 2026-09-24 - Inline trade marker label／row cursor data window owner release

- 依Daedalus `RFC-TRADECORE-0027`／`0026` feedback完成`RFC-PTCS-DYNAMIC-0022`：Renderer只呈現producer提供的generic `TaMarker.Label`，不由Order/Fill推論進出場；可見label使用相鄰X區間collision lanes、viewport-aware字級、左右clamp與row-edge反向展開，且不改Y-domain或row height。
- 每列shared cursor新增兩行日期／時間；data window與row reader同源。Candlestick顯示timestamp及`O/H/L/C/V`，line/histogram顯示timestamp及value，缺值整組顯示`Unavailable`。
- Focused gates通過：Renderer 39/39、Interactive.Client 4/4、Ptcs.Client 16/16；Interactive BrowserCacheDemo／LiveDemo與Ptcs.LiveDemo Release build通過（後者保留既有PTCS `0.2.39`對transitive `0.2.46`的NU1605 warning）。F# Playwright在3,820 bars量得All=179ms、pointer p95=27.48ms/max=64ms；Playwright MCP desktop/mobile確認四個marker label無重疊、cursor/data window正確、console 0。Interactive nupkg實際包含`data-marker-label-lane`、`data-ta-row-cursor-date`、`data-ta-row-data-time`。
- 發布exact graph：Renderer `0.1.39`（SHA-256 `8B09279D34461D5A3C77140984879E15BD6D647F1E9A03B6C3AF4BAF2A8A1CF9`）、Interactive.Client `0.1.32`（`0517155121633E441CEEFEF604469DDACA722917B2BF3852C328FAEB8C5DF657`）、Ptcs.Client `0.1.55`（`89289398AD875664EF0F094C063B03087ED202783F1E143E7E8CF30740D09BA9`）；三包NuGet push均回`Created`。Daedalus須以此graph執行真SPAA Run／Run Backtests consumer gate。
- Correction／補充：exact graph發布後，Interactive BrowserCacheDemo／LiveDemo與Ptcs.LiveDemo Release build亦通過；Ptcs.LiveDemo保留既有PTCS `0.2.39`對transitive `0.2.46`的NU1605 warning，本owner slice未改寫該legacy demo dependency。
- Process correction：第一次closeout baseline建立於同一個尚未提交的append batch中，後續補寫該batch文字而觸發append-only checker。實際Git邊界`d20b542..b07cc56`對DevLog只有檔尾新增；後續更正均改以檔尾追加，不再原地補寫。

## 2026-09-24 - Correction: inline marker immutable package graph

- Daedalus consumer gate確認NuGet公開Renderer `0.1.39`缺少final `data-marker-label-lane`，但owner local library-packs內同版DLL含有該contract；這是不可接受的同version不同bytes。舊`Renderer 0.1.39 / Interactive.Client 0.1.32 / Ptcs.Client 0.1.55`雖已發布，不再作final consumer graph。
- Runtime source行為不變；只升版並exact-lock immutable graph：Renderer `0.1.40`、Interactive.Client `0.1.33`、Ptcs.Client `0.1.56`。三包NuGet push均回`Created`，client nuspec均exact依賴Renderer `[0.1.40]`，Interactive manifest為`0.1.33`。
- 從official flat-container下載的nupkg SHA-256依序為`DA49DD1FDBBD96EC5C8F17D9EA32C15A8157A32101C6BAF374D5055E0167919B`、`A8E7924E65D20A61D0F59520A45B4A14F1F6889BE68EBE19ACEF60AAB0018ABC`、`858CAF0AFE979927FB7464124814A88CD5A14D2CA7B4DF125378AA829CF2F19D`。公開Renderer DLL SHA-256 `0FAAD35C4B9A96A340353A2CF501EA689E117660CDFF1EE73FB8FCAA41B1E97A`與owner final local DLL完全一致；公開Interactive bundle含`data-marker-label-lane`、`data-ta-row-cursor-date`、`data-ta-row-data-time`。
- 以三個全新package roots、`--source https://api.nuget.org/v3/index.json --no-cache`完成official-source-only restore/run：Renderer 39/39、Interactive.Client 4/4、Ptcs.Client 16/16。Daedalus真SPAA Run／Run Backtests仍為consumer owner gate。

## 2026-09-24 - Shared-axis canonical event time

- 依Daedalus真SPAA證據完成RFC-0023：`TemporalAxisPoint`新增optional UTC `EventTimeUtc`作shared scale/interval的presentation authority；`TemporalSeriesPoint`維持Position/Value。legacy `temporal-axis.v1`缺欄位仍解為`None`，新encoder保留canonical time，non-UTC值fail closed。
- Renderer的candle、line、reference timeline、marker與row-local cursor/data window使用authored event time；interval projection/query/coverage仍用IntervalStart/End。`chartTopologySignature`明確維持Position/IntervalStart identity，forming preview在同position更新event time不觸發全圖topology rebuild。
- Focused suites通過：Contracts 31/31、Renderer 40/40、Interactive.Client 4/4、Dynamic.Ptcs 14/14、Ptcs.Client 16/16。F# Playwright在3,820 bars量得All=223ms、cursor p95=28.69ms/max=41ms；desktop/mobile與Playwright MCP console 0，截圖為`artifacts/ta-shared-axis-event-time-playwright.png`。
- Final exact graph已發布：Contracts `0.1.15`、Renderer `0.1.41`、Interactive.Client `0.1.35`、Dynamic.Ptcs `0.1.40`、Ptcs.Client `0.1.57`。official nupkg SHA-256依序為`D55D11646B7B4AB0FBF430C036979AAFCA42DB6CA7BFAAFB40736CAFDBBA3CD5`、`F6A327837077F9CD3C9EB5B1B3ED68D48395C7D8E2B41CFFF3F35125965EF2F8`、`CC88CA01B71A5B924970F0780454B819EE718DA2F8739E0604CA1082E768B42A`、`14B5C187B11FC9AC8CF039EC87BFE645FA2FA154F1DCA6FC0856D899EAED8EC4`、`6229ABF3CD739BC09F8143BCD0D32C3C6A1AE08A7E047B3AA92463BCF06408B7`。Interactive official DLL hash `8490E48C7FCDCE5990A92DC48D172EF8050A70CBC4C114ECB195DD230F58C7D7`與owner local verified package一致。
- Correction：Interactive.Client `0.1.34`雖包含新版DLL/bundle，manifest仍標`0.1.33`，因此不可採用且未覆寫；以新immutable `0.1.35`修正，package verifier與全新cache nuget.org-only 4/4 test通過。Daedalus真SPAA consumer mapping/restart gate仍由consumer owner執行。

## 2026-09-24 - RFC-TRADECORE-0028 Backtest presentation UX owner review

- Review接受navigator事件摘要、Signal／Order／Fill分離、row resize、plot overlay、scenario dropdown與atomic overlay replacement的產品方向；正式review為`doc/RFC/RFC-TRADECORE-0028.PTCS-BacktestPresentationUX.OwnerReview.md`，目前狀態`Changes required before DEV`。
- Owner決策：同X不同navigator traces以垂直lane同時呈現；Order events保留stable identity並由generic bounded cluster處理碰撞；`HeightWeight`維持authored/reset default、resolved px只屬active canvas；新增generic OverviewStripe contract但不允許`Signal|Fill` domain DU；Order lifecycle使用非PnL紅綠的outline/solid palette。
- DEV前必修：consumer RFC須將client monotonic selection generation列為新功能；timeline／summary／downloads須與multi-dataRef overlay先完整prepare再以同revision publish；marker wire hard limit與visual glyph budget須分離。Review已透過COMM direct message `msg-fsi-3887efbee42a453eaced683ba2922a3a`交付Daedalus。

## 2026-09-24 - RFC-PTCS-DYNAMIC-0024 Backtest Presentation UX owner contract

- Daedalus以commit `818d2e70`接受owner review並修訂`RFC-TRADECORE-0028`。本repo建立正式`doc/RFC/RFC-PTCS-DYNAMIC-0024.backtest-presentation-ux.md`，狀態Accepted／DEV尚未開始，並同步REQ／SA／SD／WBS／TEST／Verification。
- Public contract定案為domain-neutral `TaOverviewStripe`、`TaOverviewStripeTraceOptions`、strict codec/options/limits/candidate validation及`TaTraceKind.OverviewStripe`。Stripe以canonical event time定位；same-X traces垂直分lane，同trace保留全部stable ids/count，PTCS不理解Signal／Order／Fill。
- Marker wire bucket/lane hard limit由4規劃提升為64，direct glyph budget維持4，其餘使用可keyboard/focus逐筆存取的generic `+N` cluster；wire overflow structured reject完整candidate並保留last-good，不允許consumer截斷identity。
- Row height採`HeightWeight` authored default＋`CanvasInstanceId + RowId` local resolved px；scenario切換保留override，Reset/reload回default。Marker presence不再切250/310px固定geometry，scalar plot移除無資料空band。
- Atomic boundary沿用單一`RuntimePatch.Operations` multi-`ReplaceDataRef` candidate-before-commit。PTCS不新增scenario service；`selectionGeneration`與summary/trades/timeline/download manifest complete candidate仍由Daedalus consumer擁有。
- Planned exact graph：Contracts `0.1.16`、Renderer `0.1.42`、Interactive.Client `0.1.36`、Dynamic.Ptcs `0.1.41`、Ptcs.Client `0.1.58`；root aggregate無runtime dependency，本輪不為湊包升版。實作與package push尚未開始。

## 2026-09-24 - RFC-PTCS-DYNAMIC-0024 owner implementation and release

- 完成generic `TaTraceKind.OverviewStripe`、typed stripe/options codecs、limits、canonical axis/target/identity validation與multi-operation candidate-before-commit；marker wire bucket上限提升為64，Renderer維持前4個direct glyph並提供可focus／Enter／Arrow／Escape操作的`+N` cluster。
- Renderer新增navigator batched overview stripe paths、same-X deterministic lanes與bounded hover lookup；row height改為`CanvasInstanceId + RowId` local override，支援pointer、8/32px keyboard、Home／double-click reset，同canvas replacement保留，canvas identity替換／reload清除。Marker presence不再改row baseline。
- Focused runner通過Contracts 32/32、Renderer 43/43、Interactive.Client 4/4、Dynamic.Ptcs 14/14、Ptcs.Client 16/16。4,000-slot F# Playwright驗66 wire markers／`+60`、兩條same-X stripes、row resize lifecycle、desktop/mobile與console 0；300 cursor transitions最大104ms，All/marker/document/progressive renderer phases皆無>100ms task。
- 初始Contracts `0.1.16/0.1.17`及Renderer `0.1.42/0.1.43`候選在full build/package gate前淘汰且未push。Final exact graph為Contracts `0.1.19`、Renderer `0.1.44`、Interactive.Client `0.1.36`、Dynamic.Ptcs `0.1.41`、Ptcs.Client `0.1.58`。
- Package verifier發現Interactive.Client source manifest仍標`0.1.35`；修正為pack前由MSBuild以`$(Version)`生成manifest，避免再發布nuspec／bundle版本漂移。五包push均回`Created`；fresh cache只走nuget.org完成43/4/14/16 tests，public nupkg hashes與owner local完全一致。Daedalus真SPAA／fresh `.dib` consumer adoption仍由DYN-WBS-555／DYN-T-577追蹤。
- Traceability correction：補入RFC-0024 reading/map、DYN-WBS-550..555與DYN-T-571..577 owner／consumer索引；owner release證據集中於`DYN-VFY-025`，未把Daedalus external gate誤列為本repo PASS。

## 2026-09-24 - RFC-PTCS-DYNAMIC-0024 same-topology OverviewStripe refresh correction

- Daedalus真SPAA證明Order／Fill row glyph正常，但同topology scenario data replacement不會更新navigator Signal／Fill stripe。根因是Renderer `0.1.44`只在full preparation／topology change更新shell prepared data；row-only refresh無法觸發navigator閉包重算。
- Renderer改用獨立reactive shell prepared-data authority；same-topology patch同步更新shell與row Vars，不更新chart runtime mount state。F# Playwright驗OverviewStripe path `2→0→2`，chart render sequence與ready-row count不變，並重跑4,000-slot、300 cursor transitions及replacement performance gates；全部通過，renderer各phase無>100ms task。
- Hotfix exact graph為Contracts `0.1.19`、Renderer `0.1.45`、Interactive.Client `0.1.37`、Dynamic.Ptcs `0.1.41`、Ptcs.Client `0.1.59`。三顆變更package push均回`Created`；nuget.org fresh-cache Renderer 43/43、Interactive 4/4、Ptcs.Client 16/16及repository signature驗證通過。Daedalus真SPAA重跑仍屬DYN-WBS-555／DYN-T-577 consumer gate。

## 2026-09-25 - RFC-PTCS-DYNAMIC-0025 chunked Snapshot transport release

- Contracts新增deterministic `ptcs-dynamic-snapshot-chunk.v1` start／ordered item／commit encoder；Interactive.Client新增generation-safe bounded queue、packet staging、phased value decode與commit-only canonical publish。Partial／duplicate／out-of-order／invalid／stale batch保留last-good並要求resync，不觸發accepted lifecycle或cache eligibility。
- Production lifecycle與cache fixture改走同一encoder／pump；initial/reconnect皆在完整commit後切換，invalid batch與disconnect保留last-good，max active transport為1。Browser cache驗8-entry LRU、coverage、invalid removal、accepted write、paused reject/rehydrate及clear。
- 五列4,000-slot candle fixture重現舊projection的allocation/GC尖峰；Renderer改為固定bucket array single-pass OHLCV聚合，不建立per-slot tuple／`groupBy`。Row cursor timestamp移至SVG外固定32px gutter的92×28固定CSS-pixel兩行tag，row resize前後geometry/computed style invariant已納入F# Playwright。
- Final exact graph已發布：Contracts `0.1.22`、Renderer `0.1.49`、Interactive.Client `0.1.41`、Dynamic.Ptcs `0.1.44`、Ptcs.Client `0.1.63`。Fresh NuGet.org caches通過34/45/12/14/16 focused tests與Renderer/lifecycle/cache三組browser gates；official package signatures、exact nuspec dependencies、DLL及Interactive bundle entry parity均通過。
- Official Renderer browser結果：five-candle replacement max92.04ms、cursor main-thread max74.73ms、All/marker/document/progressive max49.30/79.15/40.12/57.14ms，所有受驗phase皆無大於100ms task。外部SPAA `127.0.0.1:18883`／PID 17004全程保留，未重啟或覆蓋。

## 2026-09-25 - Renderer prepared-geometry performance hotfix

- Daedalus真SPAA A/B證明shared-cursor click仍有292ms EventDispatch，consumer coalescing無效並已撤回。Owner根因為click從raw runtime data重建timeline；改為accepted prepared timeline。另將line `groupBy` compaction改為固定bucket extrema arrays，Y-domain改為first-value initialized單次accumulator，並以`box-sizing:border-box`讓含border gutter實體高度固定32px。
- WebSharper package metadata會固化建置時Renderer graph；只更新global Renderer cache仍產生舊`scaleLow=Infinity` bundle。重建Interactive.Client並強制還原後，bundle顯示`scaleLow=0`，無NaN SVG。Repo內wsfscservice PID 32384曾鎖BrowserDemo generated log；只停止該helper，Daedalus SPAA wsfscservice PID 2880未動。
- Owner gate通過：Renderer 46/46、Interactive.Client 12/12、Ptcs.Client 16/16；4,000-bar five-candle/click/All/selection/document/progressive max=`78.56/8.49/49.05/58.30/27.23/40.63ms`，全部over100=0，32px gutter resize前後固定。
- Exact graph已push：Contracts `0.1.22`、Renderer `0.1.50`、Interactive.Client `0.1.42`、Dynamic.Ptcs `0.1.44`、Ptcs.Client `0.1.64`。Official flat-container三包皆含repository signature，nupkg SHA-256依序為`D10881675090C4355B79B3C73F0B510C89FCF79F4DF11901C97096AD68771361`、`519C66C822E613BC121464D1CEA4A315C3B2458A22A04350AC9130AD1F3245F6`、`B6BD3BAC7A2A6F878A33C7744E0E0499C47CABD886D9B3AD3D78806DE30F6877`，內部DLL與owner local final bytes相同。Daedalus已收到COMM `msg-fsi-2c99c905dfe848faa1b3b34312d370c7`並持有真3,820-bar SPAA驗收。

## 2026-09-25 - Sparse latest-presentation performance hotfix

- 真SPAA CDP把All 132.66ms定位至Renderer `scheduleVisibleValueRefresh -> applyVisibleCursorValues`；7個heterogeneous rows含higher-scale sparse readers，而owner dense fixture未覆蓋missing series。`cursor=None`原本會對每條trace自3820 tail執行at-or-before反向搜尋。
- Renderer在每次prepared geometry建立／更新時同步保存latest trace presentation；無固定cursor時直接讀cache，bounded cursor行為、source interval與last-good語意不變。BrowserDemo明列20條missing/sparse traces並由verifier assertion鎖定，避免fixture簡化後失去此回歸形狀。
- 舊graph在新fixture重現five-candle replacement 105.42ms／over100=1；新graph為74.01ms／over100=0。完整owner gate的cursor movement／commit／All／marker／document／progressive最大值為26.56／8.18／38.59／53.25／28.39／44.94ms；Renderer／Interactive／Ptcs suites 46/12/16通過。
- Exact graph已push且NuGet回`Created`：Contracts `0.1.22`、Renderer `0.1.51`、Interactive.Client `0.1.43`、Dynamic.Ptcs `0.1.44`、Ptcs.Client `0.1.65`。Official nupkg hashes為`318B5FF9...79D4 / 72B1FCC1...7394 / FA221DB0...1027`；三包repository signature存在且official DLL與owner local final DLL相同。Interactive package verifier確認manifest `0.1.43`、exact Renderer `0.1.51`與四個bundle entries。Daedalus已收到COMM `msg-fsi-52a44a89cb1b464d88e00566b4c968e3`，將以原3,820-bar SPAA重編DIExt後驗收。Selection FramePump line773保持分案，不與本根因混修。

## 2026-09-25 - Five-candle prepared geometry CPU-profile correction

- 真SPAA 0.1.56的bounded V8 CPU profile將約148ms long-task鎖定在row geometry preparation：五組OHLCV component series反覆建立immutable Map並執行TryFind／generic comparison／hash；marker trace另重建candle map且逐marker線性掃reference timeline。先前visible-value、DOM write、renderer remount與scheduler-body假設均已由consumer telemetry反證。
- Renderer以typed `Dictionary<string,_>`組裝OHLCV，單次geometry preparation共用candle target及first-occurrence reference slot indexes；保留incomplete candle省略、duplicate timestamp first-slot、marker lane與last-good語意。舊`markerPlacementsPrepared` API仍委派新indexed seam，避免不必要的public破壞。
- Local immutable candidate graph為Renderer `0.1.57`、Interactive.Client `0.1.49`、Ptcs.Client `0.1.71`，尚未public push。三套focused runner `46/46、12/12、16/16`，isolated full WebSharper BrowserDemo與Interactive package gate通過；nupkg SHA-256依序為`4963D1C4D2A769B363DF83F7079BBD01221B0C6D6718DD8A2E0D99E6BE504357`、`719CFE9CB92B76FE06F2A002DB2E5054D36B54E4988231222A0C8EA245B98A40`、`8624B2D47C041BF1085107DAB93CEC183215FE2CEDF9F3CFFF0BB54FD37E1D56`。
- Owner 4,000-bar F# Playwright gate：five-candle／All／marker／document／progressive max=`67.33/38.53/61.41/30.08/50.28ms`，各phase `over100=0`；300 cursor transitions main-thread max22ms，console/page error 0。候選已由COMM `msg-fsi-937a953644dd476db88ca89c56477d4a`交Daedalus以fresh cache重跑真3,820/4,000 SPAA；未將owner gate冒稱產品E2E完成。

## 2026-09-25 - Scenario selection temporal reducer CPU-profile correction

- Daedalus以selection-only V8 CPU profile證明backtest/scenario 121–134ms FunctionCall的hot stack位於Contracts `RuntimeReducer.patchCandidate`：任何`ReplaceDataRef`都觸發完整`temporalDataError`，對未變更的3,820-slot axis建立persistent `Set`並逐series做tree membership；Renderer 0.1.57 geometry修正本身已把48→All降至57.97ms。
- Reducer改為只有替換declared temporal axis才重驗完整graph；一般replacement只驗被替換的temporal series。Authority保存validated raw axis arrays，strict series使用linear subset merge；malformed series使用order-independent membership，維持revision→unknown-position→invalid-series既有reason-code priority。
- Daedalus review補出的兩個atomic gate已納入：同一patch `UpsertTemporalAxisPoints + ReplaceDataRef`須以完整candidate authority驗證；descending／duplicate known positions仍回`invalid-temporal-series`。Axis-only replacement仍檢查全部dependent revisions，invalid replacement保留last-good並request resync。
- Local immutable graph為Contracts `0.1.24`、Renderer `0.1.59`、Interactive.Client `0.1.51`、Dynamic.Ptcs `0.1.46`、Ptcs.Client `0.1.73`，未public push。Focused suites `34/46/12/14/16`與Interactive package verifier通過；4,000-bar F# Playwright scenario overlay=94.81ms、over100=0，five-candle/All/marker/document/progressive=`60.69/36.66/57.57/25.14/55.09ms`，cursor max20.18ms。真3,820-bar selection gate仍由Daedalus完成後才決定發布。

## Correction 2026-09-25 - Final immutable selection candidate

- 前一段`0.1.24 / 0.1.59 / 0.1.51 / 0.1.46 / 0.1.73`在最後ABI review前建立，已作廢且不得交付。`RuntimeSnapshotAuthority` public shape維持不變，selective raw-array authority只留在Contracts reducer內部。
- Final local graph為Contracts `0.1.25`、Renderer `0.1.60`、Interactive.Client `0.1.52`、Dynamic.Ptcs `0.1.47`、Ptcs.Client `0.1.74`；isolated exact-package focused suites`34/46/12/14/16`與Interactive package verifier通過。4,000-bar F# Playwright scenario overlay=82.19ms、over100=0，five-candle/All/marker/document/progressive=`67.70/39.12/58.38/25.75/47.06ms`，cursor main-thread max23.47ms。
- Final local nupkg SHA-256依序為`023931FF0A2E465E3ECA238EECEF9D585F06744DF55B499474357584E8C4EDD2`、`45562DD8E1C9964E3341CAB78C1716AFBF8DEBC791C86E2674E6C44BA599EA66`、`0F2D64D3F889EE459EE961670D91AD87C3D0D3DB95866761E11F28BED347C9DE`、`D67A6774E6A9148646EC2FA083376EC0207586C0983FB6C37DC7BDF5AB4EF6D7`、`7FD504EA2EC5AC41F4E2F61561F8253DE78074ED06E3A88A75CFDE45FFCD47AF`。尚未public push，真SPAA 3,820 selection gate仍由Daedalus驗收。

## Correction 2026-09-25 - Package identity and consumer acceptance

- Daedalus真SPAA 3,820 gate以0.1.60 graph通過：48→All/backtest/scenario=`52.11/56.01/45.70ms`、cursor `9.34ms`、cache-hit max `49.50ms`，正式phase over100=0。Consumer同時發現Renderer 0.1.60與Dynamic.Ptcs 0.1.47的nupkg內AssemblyVersion仍為前版，故兩包及其exact consumers不得發布。
- 依immutable package規則另建identity-correct graph：Contracts `0.1.25`、Renderer `0.1.61`、Interactive.Client `0.1.53`、Dynamic.Ptcs `0.1.48`、Ptcs.Client `0.1.75`。五包package version與AssemblyVersion逐包一致，exact dependencies已讀回；focused suites`34/46/12/14/16`與Interactive package verifier通過。
- Final owner browser scenario overlay=77.99ms，five-candle/All/marker/document/progressive=`68.51/36.86/59.74/27.64/51.16ms`，所有acceptance phase over100=0。Final nupkg SHA-256依序為`023931FF0A2E465E3ECA238EECEF9D585F06744DF55B499474357584E8C4EDD2`、`63C8156A22FCCC64934710FD5599BE0AACAB1AA5716FE6CE9DC7E1F6CF5A8C05`、`74CE7DC1FDE14A2988C3ECDD5D7EB6CF394B5644CDBCA3BDED2C67761B669CD0`、`3828AE47DC7FE7C4DFE5B97091FE551A221AFEF0638282AD17A43D9EDD53519B`、`57293A2CC09447EB1B34E38631281DCBB8C9407EBC46B1A60B72827C3C2EF029`。未public push，待Daedalus最小consumer identity確認。

## 2026-09-25 - Snapshot ordered-stream owner assembler

- Daedalus正式machine E2E取得`ptcs-dynamic-snapshot-chunk.v1` packets後，確認Contracts僅提供encoder，machine consumer只能複製Interactive browser assembler。Contracts新增純.NET/WebSharper共用`RuntimeSnapshotTransportAssembler`，支援incremental accept／finish與legacy＋多batch ordered stream decode，structured failure攜帶global packet index。
- Browser `Client.fs`與`FramePump.fs`改用同一framing transition，保留requestAnimationFrame分批`SduiValue` decode、canonical reducer與atomic publish。Active batch只有在legacy frame確實decode＋validate成功時才回interleaved；schema／kind／malformed chunk若legacy也失敗，保留原chunk error。
- 首輪在commit同步完整canonical validation，BrowserDemo量得208ms commit task；改為accept/commit只產assembled candidate，machine `finish/decodeFrames`仍完整fail-closed validation，browser則交既有phased reducer。新增DYN-T-599鎖定invalid candidate在`finish`被拒且commit packet index正確。
- Local exact graph為Contracts `0.1.26`、Renderer `0.1.62`、Interactive.Client `0.1.54`、Dynamic.Ptcs `0.1.49`、Ptcs.Client `0.1.76`。Focused suites`41/46/12/14/16`、Interactive package verifier及full WebSharper BrowserDemo通過；five-candle/scenario/All=`67.28/79.81/42.76ms`，所有acceptance phase over100=0。
- 五包SHA-256依序為`41136DD904793E7010B13C16C635A15240209F1810AD68972C5D0CF4C8525615`、`39808F9A31A6B2B031B38AFD61A1B73DCB439F9506461ABE49319F336853D8DF`、`213E4B7DBFFB951C64B76287E40EE4A7ABBC0C1E74EB2D322CF3732FCA1E88B6`、`E8C3EFCA937B542479D802D1E314232DC09AB6383EE1732B09659FF1387AE105`、`E21430D90EAE6D3AE58E7E2E79F376CCC70AFA12908F2D46E2E6F276388309A4`。PackageVersion／AssemblyVersion、exact dependencies與Interactive bundle manifest已讀回一致；未public push，等待Daedalus fresh-cache machine consumer與真SPAA gate。

## 2026-09-25 - Snapshot assembler consumer acceptance and release

- Daedalus以fresh isolated graph完成consumer驗收：exact graph/build PASS；owner `RuntimeSnapshotTransportAssembler.decodeFrames`正確解出mixed 60 frames／10.43MB與4,000 workspace（SMA13=2718、SMA34=2697、crossings 45/45）。1,140 functional E2E GREEN：loaded786／decision60／K48／orders3／entry1／exits2／red1／green1。
- 3,820 standard-trace 48→All為59.41ms、over100=0；visible scheduler global max1ms。Initial cold phase兩個>100ms事件是V8 module parse/eval，非renderer interaction。4,000×12 backtest在600秒未完成列TradeCore/SPAA backend throughput blocker，不阻擋owner transport/renderer release。
- Public push Contracts `0.1.26`、Renderer `0.1.62`、Interactive.Client `0.1.54`、Dynamic.Ptcs `0.1.49`、Ptcs.Client `0.1.76`均回`Created`，並同步exact nupkg至`lib-packs.txt`指定路徑。NuGet索引完成後，五包repository signature均有效；official nuspec／DLL／bundle functional entries與local immutable artifact逐項相同。Official nupkg SHA-256依序為`826396B910E49231B422D1D53595666D2F2B01C7EB6549D87C0825104A52518C`、`FE8164579FA6EA08AC0DF0C28DD500B79F6D4A8918691E891FF71E2513AC1D3C`、`7FD459973839AE12F7ADAA66B25DF656E7761BB0068EF3338D49BA20F6AAC175`、`4B9E59CD3ACD890280250231BBB6EB9083A18C9E2AA51A75F757BAF0A9265045`、`B2294405407B55D33A818DB1B55FED2C21C9768AB3AC39BCE144FF81ED86F8BD`。
- 下游`PulseTrade.Comm.Spa.Dynamic.Interactive.Extension 0.1.0-alpha33`已完成正式發布與official readback：flat-container HTTP 200、772,643 bytes，12個產品entries排除`.signature.p7s`後official／source／local `Different=0`。至此owner exact graph到DIExt consumer的發布鏈完整關閉；4,000×12 backtest throughput另列TradeCore/SPAA owner blocker。

## 2026-09-25 Runtime projection commit owner slice

- RFC：`doc/RFC/RFC-PTCS-DYNAMIC-0026.runtime-projection-commit.md`。新增generic `RuntimeProjectionCommitReceiptV1`，明確區分action ACK、reducer acceptance與browser projection/paint completion。
- 實作：Contracts提供create/validate/satisfies與stable DOM contract names；Renderer新增`renderWithProjectionCommit`及generation-safe completion gate；Interactive.Client在application root發布edge event＋level watermark，並提供typed current/subscription API。
- 驗證：local exact candidates `Contracts 0.1.28 / Renderer 0.1.65 / Interactive.Client 0.1.56` full build通過；focused suites `42/47/12`全綠。未public push；browser lifecycle/performance及Daedalus fresh-kernel consumer acceptance仍待完成。
- Evidence：`log/20260925/20260925225250.runtime-projection-commit.log`、`DYN-VFY-028`。

## 2026-09-25 Visible K-bar row Y-domain hotfix

- Daedalus真SPAA發現所有K-bar rows以完整source candles計算Y-domain，current viewport的candles因此被遠端extrema壓縮；既有8% data padding又會隨row resize放大為過多CSS空白。
- Renderer改為每row只掃current projected candle Low/High與same-row projected TA lines，並依實際row pixel height反算固定15 CSS px padding；其他row、viewport外資料與marker不參與domain。
- Local candidate為Renderer `0.1.66`、Interactive.Client `0.1.57`（Contracts維持`0.1.28`）。Renderer 48/48、Interactive 12/12、package verifier及4,000-slot F# Playwright完整PASS；steady cursor p95=67ms/max147ms，互動phase over100=0。真SPAA逐row 48/200/All/resize consumer gate尚待Daedalus回報，未public push。
- Evidence：`log/20260925/20260925233300.visible-row-y-domain.log`、`log/20260925/20260925233300_issue_visible_row_y_domain.hypothesis.md`、`DYN-VFY-030`。

### Correction 2026-09-26 - Release-built edge-padding candidate

- `0.1.66/0.1.57`的第一版雖通過owner gate，但真SPAA指出它漏算既有SVG edge到plot的固定10 viewBox inset，實際edge gap仍約43.8px，因此作廢且不發布。後續Debug `0.1.67/0.1.58`只用於驗證修法，也不作為public candidate。
- 最終Release-built immutable candidate為Contracts `0.1.28`、Renderer `0.1.68`、Interactive.Client `0.1.59`。K-bar row改用完整SVG高度，15 CSS px定義為SVG edge到visible extrema centerline的總padding；真DOM path外緣因1.8 viewBox stroke向外擴張，在default、pointer resize與reset皆量得12.60px。
- Focused gates為Renderer 48/48、Interactive 12/12、package verifier PASS；4,000-slot browser gate完整PASS，cursor p95=65ms/max129ms且interaction over100=0。Renderer／Interactive Release nupkg SHA-256為`56C2004058A6BE21F5782E152EF25564D5EFB8B4EA6890B5B15A5FD1C7EA0229`／`FD09F0D88C017F0594989B0BB340AEDFED1CD3AB6D004B2CE04E8E9B18BD8451`；public push仍等待Daedalus真SPAA consumer gate。
- Consumer closure：Daedalus以真SPAA完成narrowed／48／200／All／resize逐row驗收，visible-domain gates全數通過。該輪另量得pointer p95=82.69ms，高於consumer 50ms門檻；此項不推翻Y-domain修正，但在第二輪與focused root-cause完成前仍阻擋`0.1.68/0.1.59` public push。
- Consumer release correction：停止同機owner BrowserDemo後，診斷run pointer p95=28.78ms；正式50ms hard gate為42.20ms，cold/cache無>100ms task，證實82.69/78.28ms為資源競爭而非產品regression。Daedalus正式放行`0.1.68/0.1.59`公開發布。
- Public release：Renderer `0.1.68`與Interactive.Client `0.1.59` push均回`Created`。首次在Renderer current directory呼叫Interactive通用PostBuildEvent找不到nupkg，已改於Interactive project directory重跑成功，未重推Renderer。Flat-container HTTP 200後，official nupkg SHA-256分別為`920AA2C04E86982C01685127EE2A7B9B206FBBCB384903D02A23CDB867D1FF10`／`95100DDE9528D70C1ED743B49B5AE3871659E1747E1A2B4F2C40656EC255AE7F`；repository signatures通過，排除`.signature.p7s`後與owner Release candidate逐entry `Different=0`。

## 2026-09-26 Fixed CSS-pixel line and overview boundary candidate

- Daedalus要求SMA／一般line不隨SVG viewBox或row resize變粗，並要求overview左右可見邊界固定2 CSS px且與較寬transparent drag hit target分離。Renderer對line width夾1–2並加`non-scaling-stroke`；overview新增`ta-overview-left/right-handle-visual`，既有handle testid維持transparent rect。
- Local exact graph為Contracts `0.1.28`、Renderer `0.1.69`、Interactive.Client `0.1.60`。Canonical generated `websharper.log` ACL仍拒絕存取，故完整WebSharper builds使用source-identical disposable staging；沒有停用compiler或沿用舊bundle。
- Owner gates通過：Renderer 48/48、Interactive.Client 12/12、package verifier及4,000-slot F# Playwright。Browser gate驗computed stroke、row resize、左右navigator drag及既有interaction/performance；acceptance phases無>100ms task。Local SHA-256為`5A5D72E2494185385CA35E08311FF404625BC799E9694F7480691004C1C02006`／`30F534D3E9E5B95C10399B3E12FDC235354EB8C7E3733E656C18201C3BF3941B`。
- Candidate與stable DOM testids已透過COMM交Daedalus執行真SPAA geometry／截圖／效能 gate。Consumer放行前不public push。

### Correction 2026-09-26 - Capped authored/default chart height

- Daedalus以舊真SPAA重現1140 bars／48 viewport時price chart預設720 CSS px。此為Renderer將`HeightWeight=3`直接映射到candle maximum所致；同一未發布candidate改為所有chart row初始化／reset／reload default封頂250px，但保留candlestick 720與scalar 480的manual上限。
- Owner package gates重新執行：Renderer 48/48、Interactive.Client 12/12、package verifier及4,000-slot F# Playwright全部通過。Browser在任何resize前逐一量測chart SVG `<=250px`，price row為default 250、manual 282、reset/reload 250；five-candle／scenario／All／marker／document／progressive max=`81.49/88.24/53.26/62.34/35.00/54.25ms`且無>100ms task，cursor p95=61ms/max143ms。
- 前述candidate SHA作廢且未發布。新的Renderer／Interactive.Client nupkg SHA-256為`E36BDD64F0D3CD391B115787D5D244BBF48AB5F6DFAADFF982DF42A19C9F6A78`／`6522E035921017ADF905C005CA6E75DF80A0D49A9822960ACFEA8ACCA85A8AAF`；仍等待真SPAA consumer gate，不public push。

### Correction 2026-09-26 - Immutable final candidate identity

- 因`0.1.69/0.1.60`的舊SHA已交給consumer，後續default-height補丁不得以相同版本替換bytes。該組版本完整作廢且未public push；immutable final local graph改為Contracts `0.1.28`、Renderer `0.1.70`、Interactive.Client `0.1.61`。
- Final package SHA-256為`F2293616A5C619150BF08DD67FC0CB64C983A5ADC0DF6962D5C7278BC6833107`／`2E142CA3300C90F453C40F141CFCA9809143740989322EB0E010A9ED4A33931B`；focused 48/48、12/12與package verifier通過。Final identity browser功能斷言通過，但正式performance run受同機Chrome／系統負載影響，scenario 106.75ms、cursor 13.86s而正確fail；安靜環境重跑前不宣稱final owner browser PASS、不public push。

## 2026-09-26 - Fixed CSS-pixel stroke and default row-height release closure

- Daedalus以真SPAA驗收immutable graph Contracts `0.1.28`／Renderer `0.1.70`／Interactive.Client `0.1.61`：1,140 bars、5 markers；line/SMA 1–2 CSS px、overview visible boundaries exact 2px且drag rect透明、全部初始candle/composite SVG <=250px均GREEN。All=285ms、pointer p95=28.18ms、cold/cache long tasks=0，Release builds 0 errors。
- Renderer與Interactive.Client public push均回`Created`。Official nupkg SHA-256為`7B6A570D1333EE7E15F596737A08E55EA4C47035A3DA500405F2E889293EA675`／`1F668DB14F75D7B3C00BEB9684A0453B494E07558783CB525CDF9E789E45CB1D`；repository signatures有效。排除`.signature.p7s`後local／official entries均`Different=0`，Interactive nuspec exact依賴Contracts `[0.1.28]`與Renderer `[0.1.70]`。

## 2026-09-26 - Dark plot surface and typed histogram polarity candidate

- RFC-0027新增generic `TaPlotSurfacePresentation` light/dark contract與`TaHistogramTraceOptions`正負色彩contract；Renderer集中解析palette，將candle／TA／overview surface切為黑底且維持grid、cursor、axis與legend可讀。Histogram依typed positive／negative colors建立兩個bounded batched paths，不由trace名稱推論domain。
- Overview左右visible line仍為2 CSS px與既有transparent 8-unit drag hit rect分離；初始貼SVG edge時只將可見線向內平移1px，drag後移除transform，未改hit target或selection語意。
- Release exact graph為Contracts `0.1.29`、Renderer `0.1.71`、Interactive.Client `0.1.62`。一般incremental Release build曾產出缺少`WebSharper.meta`的Contracts DLL；改以Release `Rebuild`後確認151個manifest resources，再用獨立restore cache重建下游與驗證package，沒有停用WebSharper。
- Owner gates通過：獨立cache focused `44/44、48/48、12/12`、Interactive nupkg verifier、Release exact graph 4,000-bar F# Playwright與Playwright MCP。Release正式update phases max為`72.92/83.35/40.34/59.85/24.19/42.93ms`且over100=0，console/page error為0。
- Release nupkg SHA-256依序為`7A59C0A671E5E995B88D1BCD23F074D2ADC0280D6DFD9EC09C354208D51FFC9E`、`2D68D4CAF1234634090B5417B2ED7FEEED0B26ABF18DAE613130B8699AB5A98F`、`3B5997F91F32150949402A8E57D9DDDA1012A4F4D98C5206D1F19319CD3A417B`。Daedalus已完成DIExt typed adoption、signature smoke與SPAA Release build；真SPAA visual/performance gate前不public push。

## 2026-09-26 - Default viewport and marker OFI owner candidate

- RFC-0028修正fresh canvas presentation：合法`DefaultView.visibleBars`成為initial viewport authority並依loaded/max clamp，缺值／不合法維持48；同canvas後續user viewport不被default重設。Overview selection改淺灰，左右visual boundaries改亮綠2 CSS px，transparent hit targets與drag math不變。
- 依Daedalus `msg-fsi-55dcc87ce84f4ba88405201207acbf92`，marker plot移除inline label，只留glyph＋tooltip；每列在cursor gutter與plot之間加入固定24px OFI band，shared cursor投影該slot的Label/Tooltip，最多4筆＋`+N`，無事件維持空白高度。未新增Signal／Order／Fill domain contract。
- `0.1.72/0.1.63`與`0.1.73/0.1.64`均為未交付、未public push的退休bytes。Immutable local graph為Contracts `0.1.29`、Renderer `0.1.74`、Interactive.Client `0.1.65`；fresh full WebSharper Release builds、focused `48/48、12/12`及Interactive package verifier通過。
- F# Playwright fresh 4,000 viewport、single/dense/empty OFI、inline-label absent、overview geometry及既有regressions全通過；five-candle/scenario/All/marker/document/progressive max=`69.27/81.02/39.91/31.39/56.03/43.75ms`，acceptance phases over100=0。Playwright MCP live inspection無overlap/layout shift。
- Final local nupkg SHA-256：Contracts `A93FCEBA298DAC3A17CFE291119BA742B199D4258A355CA31B3E06A3B5DF9087`、Renderer `863A56E5404EF9E4586191C0FD6B23EC30A759FC5896E6090C19E98A3AAF9BEC`、Interactive.Client `4DA2B1908DBE9B7CFBF850294B63E3774E1A6E64CB81C00E1BCB205F7E372CDC`。已回覆Daedalus；真SPAA clean-cache GREEN前不public push。

### Correction 2026-09-26 - Frozen Contracts package identity

- 上一筆將canonical `bin/Release`後重打包的Contracts `0.1.29 / A93F...`誤列為final nupkg。Contracts本輪source/API未變；已凍結且交付consumer的正確package identity為`0.1.29 / 7A59C0A671E5E995B88D1BCD23F074D2ADC0280D6DFD9EC09C354208D51FFC9E`。
- `7A59...`與`A93F...`內部contract DLL SHA-256同為`D3BF2F29F53FC98B4547827EC244F0BD40CD75B6D666A60C9179E6A1AD4CEFB4`，不需bump Contracts；canonical bin已恢復凍結nupkg，`A93F...`禁止交付。Renderer `0.1.74 / 863A...`與Interactive.Client `0.1.65 / 4DA2...`不變。

### Correction 2026-09-26 - Marker hover selects accepted placement

- Daedalus真SPAA反證Renderer `0.1.74`／Interactive.Client `0.1.65`：marker glyph存在，但`HoverAsync`事件冒泡至plot後依滑鼠X／row axis二次推算slot，coarse candle內22:01 marker無法命中60K row OFI，180秒後仍為0。兩版退休且禁止public push。
- Renderer glyph與overflow cluster改為直接選已驗證`TaMarkerPlacement.SlotIndex`並停止冒泡；一般plot hover仍依row axis。`TaMarkerCursorItem`、glyph與OFI DOM保留原始`EventTimeUtc`，不改marker wire或交易domain contract。
- 新immutable local graph為Contracts `0.1.29 / 7A59...`、Renderer `0.1.75`、Interactive.Client `0.1.66`。Focused suites `48/48、12/12`，Interactive package verifier與`:30` intra-bar direct-hover F# Playwright通過；five-candle/scenario/All/marker/document/progressive max=`69.04/81.48/38.36/57.35/45.51/48.07ms`，acceptance phases over100=0。
- Renderer／Interactive nupkg SHA-256為`A8F771EF212166AB85883CEADB9D5CFD896932083C6DC449A7DAC6705D42CE44`／`778F3261C178CC06BFD73E37537EE0501BF2DD4B547B336276FE72EC33599D1E`，AssemblyVersion分別為`0.1.75.0`／`0.1.66.0`。待Daedalus真SPAA clean-cache重驗後才public push。

### Correction 2026-09-26 - Consumer marker gate used the wrong interaction contract

- Daedalus隨即確認前一個真SPAA失敗是gate誤用`entries.First.HoverAsync()`：正式契約一直是讀glyph的`data-marker-slot`，再把real pointer移到chart slot，由plot shared-cursor path更新OFI。直接hover glyph不是產品互動契約，故不存在已證實的Renderer root cause。
- Commit `746f6e8`在durable inbox讀到更正前已建立，但未public push；其中新增的glyph direct-hover seam與`0.1.75/0.1.66`已以forward correction完整撤除，不供consumer採版。Source與exact references恢復`0.1.74/0.1.65`。
- Canonical local candidate仍為Contracts `0.1.29 / 7A59...`、Renderer `0.1.74 / 863A...`、Interactive.Client `0.1.65 / 4DA2...`。等待Daedalus以`data-marker-slot` real-pointer流程重跑真SPAA，再決定public push。

### Correction 2026-09-26 - High-density marker hit requires exact placement slot

- Daedalus後續真SPAA 3,563-point證據反證上一個Correction：glyph `data-marker-slot=1119`，但實際pointer event經plot X snap後為`data-cursor-slot=1118`且OFI count=0；多個reference slots落在同一CSS pixel時，X座標無法還原已命中的marker identity。
- 上一個forward correction未commit。Renderer保留單一shared cursor state，但直接glyph／overflow hit以accepted placement exact slot優先並停止冒泡；一般plot hover仍依row axis。`TaMarkerCursorItem`、glyph與OFI DOM保留原始`EventTimeUtc`。
- 因高密度fixture加入後不得替換已凍結`0.1.75/0.1.66` bytes，最終immutable local graph為Contracts `0.1.29 / 7A59...`、Renderer `0.1.76`、Interactive.Client `0.1.67`。Focused suites `48/48、12/12`、package verifier與4,000-slot F# Playwright通過；回歸先證明marker與相鄰slot同CSS pixel，再以真實SVG hit-testing驗exact slot／OFI。
- Renderer／Interactive nupkg SHA-256為`F7A27C19BD48732B82B291BFB285C2F6AA573873418448AB8A8B203348F23AB0`／`45052F853CEBFAE0711595D62463F2CD51D998A1DD656AECAD421D95E4D7D9FC`。five-candle/scenario/All/marker/document/progressive max=`72.54/79.73/36.53/41.93/38.27/40.59ms`，acceptance phases over100=0；真SPAA consumer GREEN前不public push。
- Daedalus以同語意的`0.1.75/0.1.66`真SPAA 3,563-point先驗證修法：fixed strategy為marker/band slot `1119/1119`、SMA scenario為`2123/2123`，兩者OFI count=2；consumer端non-finite StrategyValue JSON 500亦已修復。最終`0.1.76/0.1.67`仍須consumer同步後再跑final gate。

## 2026-09-26 - RFC-0028 final consumer acceptance and public release

- Daedalus使用final exact graph `Contracts 0.1.29 / Renderer 0.1.76 / Interactive.Client 0.1.67`完成真SPAA 3,563-point gate：fixed strategy marker/band=`1119/1119` count=2，SMA scenario=`2123/2123` count=2；兩策略selector／marker／OFI、fresh 4,000、overview style/stripe、dark/TA colors全部通過。
- Renderer與Interactive.Client public push均回`Created`。Official NuGet nupkg SHA-256為`96B9D779411E115FC1A0DFCCD2EACA2F22676133165C190A671DC1EC28164376`／`90C070B7D0142EE2D9F0EA1F1744CB410ABFE36B24A3ABAC7C5A41E77D89657E`；兩包repository signature有效。
- 排除`.signature.p7s`後local／official entries皆`Different=0`。Official Renderer依賴Contracts `[0.1.29]`；Interactive.Client依賴Contracts `[0.1.29]`與Renderer `[0.1.76]`，RFC-0028 closure完成。

## 2026-09-26 - Dynamic PTCS adapter exact graph candidate

- `DYN-WBS-564`同步已發布owner graph：Dynamic.Ptcs升`0.1.49 -> 0.1.50`並exact依賴PTCS `[0.2.46]`／Contracts `[0.1.29]`；Dynamic.Ptcs.Client升`0.1.76 -> 0.1.77`並另exact依賴Renderer `[0.1.76]`。LiveDemo direct PTCS由`[0.2.39]`同步`[0.2.46]`。
- Dynamic.Ptcs focused suite `14/14`、Ptcs.Client `16/16`，LiveDemo完整WebSharper Release build 0 errors（僅既有WS9002 warning）；兩包Release pack與package/AssemblyVersion/dependency readback通過。
- Local nupkg SHA-256為Dynamic.Ptcs `BFF5920ABFCE072EAE6242A46E36F9D1DB293887E643830BED3C8479C777F378`、Ptcs.Client `BB4C2D9EC28E0ABE63555E56FFDD15ACCA040A85C6CA103EEB5AC3590739F745`；public push/readback待candidate closeout後進行。

## 2026-09-26 - Dynamic PTCS adapter exact graph release

- Dynamic.Ptcs `0.1.50`與Ptcs.Client `0.1.77` public push均回`Created`。Official signed nupkg SHA-256為`B228418E317C8532290E2EC79C515DD86A8CBD8A6BDFA52590CEE29F833D673D`／`85B9BBF86429466C8B4431FD388DD89BDDD43E3154096635E320ECC49A4C60B6`。
- `dotnet nuget verify --all`確認兩包NuGet.org repository signatures有效；排除`.signature.p7s`後local／official entries均`Different=0`。Official exact dependencies分別為PTCS `[0.2.46]`＋Contracts `[0.1.29]`，以及再加Renderer `[0.1.76]`；`DYN-WBS-564`結案。

## 2026-09-27 - RFC-0029 owner candidate

- 新增`RFC-PTCS-DYNAMIC-0029`並完成owner slice：dark hollow marker保留semantic stroke，另畫pointer-inert contrast halo；plot visible marker label維持0，Label/Tooltip由SVG title與row-local OFI承接。
- OFI改為合併Marker與OverviewStripe的generic cursor events；stable event id重複時Marker rich chip優先，跨authored traces round-robin分配4個visible slots，其餘`+N`。capability存在但slot無event顯示`None`，缺capability顯示`Unavailable`。
- 新增typed`RemoveTaTrace`／wire `remove-trace`與PTCS adapters；non-system trace支援local hide/show及remote remove。same-canvas patch保留state，Reset Canvas／identity replacement清除；最後一條non-system trace被remove後收合row。Data window改wrap/auto height、無內部scrollbar。
- 修正WebSharper list computation漏`yield`造成trace/row controls未進DOM，以及inline authored display覆蓋hidden attribute的visibility問題；已補BrowserDemo regression。
- Focused suites實際執行/通過：Contracts `44/44`、Renderer `49/49`、Dynamic.Ptcs `15/15`、Ptcs.Client `17/17`、Interactive.Client `12/12`。兩套F# Playwright PASS；generic gate all=75ms、pointer p95=19.92ms/max35ms，renderer gate cursor p95=35ms/max59ms且正式phases全低於100ms；package verifier PASS。
- Immutable local candidate graph：Contracts `0.1.30 / E1F046A2...E747`、Renderer `0.1.81 / 5C726F3B...D623`、Interactive.Client `0.1.72 / 6B368E92...6187`、Dynamic.Ptcs `0.1.51 / 71ECA1CB...4CF2`、Ptcs.Client `0.1.82 / 51537CA6...D8D4`。Retired versions不得採用；Daedalus真SPAA consumer gate與public push/readback尚待完成。

## 2026-09-27 - RFC-0029 consumer acceptance and public release

- Daedalus回報composite candlestick hide後show為0；owner最小回歸證明正式Renderer可恢復8條batched candle paths。根因是consumer gate誤用line selector`ta-trace-*`，candlestick應使用`ta-candle-*`；修正後的batched locator再改為等待`.First`後驗count。此項未修改Renderer source或升版，並把正確行為固化於`verify-ta-generic-marker-playwright.fsx`。
- 修正consumer gate後，真SPAA 3,563-point完整GREEN：fixed strategy order/fill、OFI、halo、forced expiry、scenario切換、K/SMA獨立hide/show/remove與Reset Canvas均通過。Owner generic gate為4,000 bars／48 paths／all=53ms／pointer p95=30.93ms/max71ms；renderer正式phases over100=0。
- Exact graph Contracts `0.1.30`、Renderer `0.1.81`、Interactive.Client `0.1.72`、Dynamic.Ptcs `0.1.51`、Ptcs.Client `0.1.82`已public push。Official SHA-256依序為`726F4E1E...1B57 / 8FAC1159...163 / 79767D28...9CE / 7445F84C...1C5 / 7B68ECB0...873`；五包repository signature有效，official exact dependencies符合RFC，排除`.signature.p7s`後local/official entries皆`Different=0`。
## 2026-09-28 - RFC-0030 row-local control line owner candidate

- Daedalus真SPAA 8-row UI指出舊`ta-row-toggles`把所有row/trace controls放入同一flex-wrap flow。RFC-0030把責任固定在generic Renderer：每 authored RowId一條`ta-row-control-line-*`，左側row controls固定nowrap，右側trace region nowrap並只在自身水平overflow；Marker／OverviewStripe及RFC-0029 lifecycle/action semantics不變。
- 新增DYN-T-626..628與F# Playwright geometry gate。七個BrowserDemo rows在1440／390px皆唯一、40px高且Y band分離；同row trace buttons同Y band，390px MACD long trace region證明row-local overflow。因控制區增高會觸發Playwright auto-scroll，兩套既有verifier改為scroll後重取chart geometry，避免使用stale bounding box。
- Source-built focused suites`49/12/17`、完整renderer Playwright、generic marker Playwright及Interactive package verifier通過；正式performance phases皆無>100ms task。Local exact graph為Contracts `0.1.30`（不變）、Renderer `0.1.82`、Interactive.Client `0.1.73`、Dynamic.Ptcs `0.1.51`（不變）、Ptcs.Client `0.1.83`。
- Local nupkg SHA-256依序為Renderer `4AC9E21A53854C3A01A7FBCFA3F1AFC470C52FD691BE9118104AD2B89C2B02A3`、Interactive.Client `A5B5B0BF942FCD86D6E00D7523995813C306D43B9BA92D71C7D08AB7A8137CAD`、Ptcs.Client `D294C63E35D595407F5197D3C2FB35CEDF6B7797E55D3C5E2A23028DED161D95`。等待Daedalus真SPAA desktop/640px consumer GREEN後才public push。

## 2026-09-28 - RFC-0030 official immutable candidate release

- Correction：consumer要求先取得official immutable artifact與readback，以免相同版本在local feed被覆寫；因此發布順序由「consumer GREEN後push」改為「唯一official版本先發布、consumer只驗該artifact」。產品驗收仍pending，沒有以push成功取代真SPAA gate。
- Source commit `15b1f8b`已push。Renderer `0.1.82`、Interactive.Client `0.1.73`、Ptcs.Client `0.1.83` public push均回`Created`；官方SHA-256依序為`20D89407F1588618E88E297EB86E5DC4C658ED7510D581519EE29AC76515A073`、`21CC8DDA7DAE5C1D3C8D6C16668AAC02C0E67CC7161F248A017627DB37407C8B`、`5EE7C03DC3308E26BD58063BC51D0831F95F4349F89CDB5BF71C6A47F95F36CB`。
- 三包`dotnet nuget verify --all`均確認NuGet.org repository signature有效；official nuspec exact dependencies符合RFC：Renderer→Contracts `[0.1.30]`，Interactive→Contracts `[0.1.30]`＋Renderer `[0.1.82]`，Ptcs.Client→PTCS `[0.2.46]`＋Contracts `[0.1.30]`＋Renderer `[0.1.82]`。Daedalus已收到source commit、official hashes與consumer gate請求。

## 2026-09-28 - RFC-0031 page-scoped display time zone owner candidate

- 依TradeCore RFC-0029新增generic `SduiDisplayTimeZone`，stable ids為`UTC`、`America/Chicago`、`America/New_York`、`UTC+08:00`。Renderer集中提供純F# formatter，依event instant處理CST／CDT、EST／EDT與UTC+8跨日；invalid timestamp fail closed，不使用browser local timezone。
- `TaWorkspaceRenderer`新增reactive display-time overload，axis、row cursor、data window、temporal metadata、marker／stripe／OFI tooltip及status watermark共用同一selection；canonical UTC DOM attribute、query/cache/wire、viewport與revision不變。Interactive與PTCS clients新增typed `View<SduiDisplayTimeZone>`入口，既有API維持UTC。
- Exact local graph為Contracts `0.1.31`、Renderer `0.1.83`、Interactive.Client `0.1.74`、Dynamic.Ptcs `0.1.52`、Ptcs.Client `0.1.84`。Focused suites`45/51/12/15/17`、五包build、Interactive package verifier與4,000-bar F# Playwright snapshot→patch gate通過；切換未送action且canonical/viewport/loaded bars不漂移。
- Canonical checkout的既有`websharper.log` ACL會使compiler process crash；依既有流程使用source-identical disposable staging完整編譯，未停用WebSharper作release evidence。Local nupkg SHA依序為`75CDE97F...D427 / B56A6277...1110 / E9521B20...B1DA / 2C3BEDA2...E8C9 / 2A938D58...F82`；public release/readback與Daedalus真SPAA gate待後續完成。

## 2026-09-28 - RFC-0031 official immutable release

- Source commit `5bdc13c6214ccf3c4a0b5e31c0f777b8c7eb00ca`已push。Contracts `0.1.31`、Renderer `0.1.83`、Interactive.Client `0.1.74`、Dynamic.Ptcs `0.1.52`、Ptcs.Client `0.1.84` public push全回`Created`。
- NuGet.org official SHA-256依序為`CEB70E74EC1143646261DD43079F26177B01D428A726CBDD129356ED91CEA086 / 211031F700E7F6DA24A766D05DF4FC11D015B074ED32C251776BBC2E6D5C7B9D / 1F4EA667DA2997DE90A1E556284EC2DC7ADF88C964A1C2D06668C8B7B4F65517 / D3C7A0D8DBEA6E93D5867B4A1A9A1CBBDAA9D3017D3453A75E620BD3FFC13478 / 6E2D1271F0BA0BC101209F8A57AC731C73A6B591CFF55181765E4969F5F1A325`。
- 五包`dotnet nuget verify --all`均確認NuGet.org Repository signature有效；排除`.signature.p7s`後，local/official package entry名稱與內容hash差異全為0。Owner release gate完成；Daedalus真SPAA no-fetch/no-run/no-revision-drift仍是consumer acceptance gate。

## 2026-09-28 - RFC-0031 consumer acceptance

- Daedalus consumer commit `G:\coldfar_py\coldfar-symbolics@02d3432e`整合Contracts `0.1.31`、Renderer `0.1.83`、Interactive.Client `0.1.74`；canonical evidence為`doc_new2/DevLog.md`及`doc_new2/RFC/RFC-TRADECORE-0029.page-display-time-zone.md`。
- 真SPAA Playwright使用3,563根ES 1K、兩個Backtest scenarios及固定成交`2026-06-17T22:01:00Z`，UTC→CT/CDT→ET/EDT→UTC+8全頁投影GREEN。切換期間API request count、document/data revision、loaded bars、viewport、marker slot、selected scenario及End K query zone全部不變，最終`SMA13X34_DMI_SPAA=GREEN`。DYN-WBS-571完成。

## 2026-09-28 - RFC-0032 loaded coverage／navigator coherence owner candidate

- Renderer selection移除24 viewBox-unit visual floor；2 CSS px visual boundaries與24 CSS px transparent interaction resolver分離，tiny selection的left／move／right gesture由single root route deterministic解析。
- Contracts新增validated `ta-loaded-coverage.v1`與`ta-coverage-window.v1`，分離完整ordinal/gap/overview authority及<=4000 active detail。Interactive cache以temporal adjacency選頁；revision advance只在唯一segment仍精確對應舊page時rebase，ambiguous/coalesced/future revision fail closed。
- active-detail page替換改走full scheduled preparation，同projection live patch才incremental。BrowserDemo修正延遲舊action缺少optimistic revision檢查的fixture bug，過期`ExpectedDocumentRevision`現在回`RevisionConflict`，不再污染新page。
- Fresh exact candidate suites為`47/55/14/15/17`。F# Playwright在4,000 bars驗tiny selection 3.813/1400、move/雙側resize、500 loaded／250 active detail與Earlier/Later；正式phase全無>100ms task，cursor p95=37ms/max85ms。
- Candidate graph為Contracts `0.1.32`、Renderer `0.1.85`、Interactive.Client `0.1.76`、Dynamic.Ptcs `0.1.53`、Ptcs.Client `0.1.86`；Interactive bundle verifier PASS。official immutable push/readback與Daedalus真SPAA／fresh-kernel DIB仍pending，不以owner gate代替consumer acceptance。

## 2026-09-28 - Correction：RFC-0032 adjacent cache atomicity

- Daedalus在candidate handoff前指出adjacent cache必須同時證明cached A的實際temporal/data identity與rebased document。調查確認舊phased rehydrate只從entry套用snapshot、完成時卻保留current B document，可能形成A data＋B `activeDetail/queryGeneration`；前一節candidate graph因此retired，不得供consumer採用。
- Contracts新增entry-level prepare/completion：完整驗證cached entry，以cached Document/View/data建立candidate，保留current authoritative revisions、transport envelope與cache identity，再由單一presentation commit發布並進入`PausedForResync`。既有API維持相容。
- DYN-T-648C模擬B prepend後cached A由ordinal 0 rebase到250，使用250筆實際A temporal data驗identity、latest segments/query generation，並驗同一Renderer publish顯示`Viewing 251-500`。Fresh exact-package suites為`47/55/15/15/17`；BrowserCache與Renderer F# Playwright通過，正式renderer phases皆無>100ms main-thread task。
- Final staging graph為Contracts `0.1.33`、Renderer `0.1.86`、Interactive.Client `0.1.77`、Dynamic.Ptcs `0.1.54`、Ptcs.Client `0.1.87`；local SHA-256依序為`553C62AF6D03D4DFE2360E82D3D17C43C349BD4605233AF5CDB1D6DC803131A0 / 2A8A3D41559FC53D9C011ABD551A7BAF63D9E2013ABA506187ACBD95B14351BB / 637D0D93FE682689F894D9A6A7A4B53B524638D6081476306A3D92144367DFC8 / 5C04F89D75FC1C934E204AA2239E6460A45F9A1411F483BC02992DF7564908D6 / 3EC126F7466C914AF65DD6BE51AB22079713B40648D08E0181B5730EE9D96C1A`。official push/readback與consumer真SPAA／fresh-kernel DIB仍pending。

## 2026-09-28 - RFC-0032 official immutable release

- Source commit `9e0e1a3`已push；Contracts `0.1.33`、Renderer `0.1.86`、Interactive.Client `0.1.77`、Dynamic.Ptcs `0.1.54`、Ptcs.Client `0.1.87` public push全回`Created`。
- NuGet.org official SHA-256依序為`F4CA05493F016B36B91614433363AACC4E235D8E995B8D89C3E17BB234C9DD4B / 5D11632F5A8E6EE67B0379104B6FBADF676831E5BBEDC9F9254B2B249143E7E2 / 2940618380EA00ED746FBD3D0083F0624F24C82A0355937B5427B8510FDB0D38 / ECB53E7812A42E81CFCD1E0C960F797B63D3DEB7E2EAFC8F5965BDA25D0BAD3C / C5310581C08CB07397A1472FC3538D0F06410D5F62E279691EE3252277DFA6D7`。
- 五包`dotnet nuget verify --all`均確認NuGet.org repository signature有效。Owner release gate完成；Daedalus真SPAA／fresh-kernel DIB仍是consumer acceptance gate，不以package發布取代產品驗收。

## 2026-09-28 - Row barrier／visible cursor resize owner hotfix

- Daedalus以official `0.1.33/0.1.86/0.1.77`在真SPAA重現1,124 bars／8 rows的200→All 2,042–2,067ms，以及shared cursor visible後keyboard row resize使label回hidden。
- Renderer把reader maps與global cursor publication從每列mount後重掃改為rows-complete barrier一次；row-height geometry refresh改重套`displayedCursorIndex`。既有API、viewport、chart order與cursor identity不變。
- Local exact candidate為Contracts `0.1.33`、Renderer `0.1.87`、Interactive.Client `0.1.78`、Dynamic.Ptcs `0.1.54`、Ptcs.Client `0.1.88`。Focused `55/15/17`、完整WebSharper BrowserDemo、F# Playwright及Interactive package verifier通過；official release與真SPAA consumer gate待續。

## 2026-09-28 - Row barrier／visible cursor resize official release

- Source commit `75c7d98`已push；Renderer `0.1.87`、Interactive.Client `0.1.78`、Ptcs.Client `0.1.88` public push全回`Created`。
- NuGet.org official SHA-256依序為`933629F07247B840A6EB069CE2F567FE530101FE1D7F469610A7CC79B0681FAC / B8ECD9E242441EE42478AF7062E6A0C3017DDA2DA61766D7F7EC08D2AB114816 / BE24CFCECD47B14F60D3E33DC049EB0F5F7D1B86F58511BAEF23BAB40B89F3A6`。
- 三包`dotnet nuget verify --all`均確認NuGet.org repository signature有效；exact dependency正確，排除`.signature.p7s`後local/official entry differences均為0。剩Daedalus真SPAA consumer gate。

## 2026-09-28 - Row mount／reactive cursor consumer correction

- Daedalus以official `0.1.87/0.1.78/0.1.88`在真SPAA 960 bars／8 rows重跑，200→All仍為2,065–2,075ms；visible hover cursor在keyboard resize後由left 64.90%變為hidden。前一版因此只保留release integrity，不標consumer PASS。
- Renderer `0.1.88`移除每列一個animation-frame的mount等待，改zero-delay cooperative scheduling；reader同步以`displayedCursorIndex`優先，row-height更新再於下一個animation frame重套geometry，避免reactive DOM rebuild清掉hover-only cursor。
- 新local graph為Renderer `0.1.88`、Interactive.Client `0.1.79`、Ptcs.Client `0.1.89`。Focused `55/15/17`、full WebSharper BrowserDemo與package verifierPASS；4,000-bar F# Playwright量得200→All 435.30ms、正式phases無>100ms，resize後等待220ms仍保持7個cursor labels/crosshairs。等official release/readback及Daedalus真SPAA重跑。

## 2026-09-28 - Row mount／reactive cursor second official release

- Source commit `1a508e4`已push；Renderer `0.1.88`、Interactive.Client `0.1.79`、Ptcs.Client `0.1.89` public push均回`Created`。Interactive首次並行pack因project-scoped `wsfscservice`持有受保護log失敗；停止該明確helper後單獨full WebSharper pack成功，未停用compiler或沿用舊bundle。
- NuGet.org official SHA-256依序為`3426477273FFF51A466441DDE860E25236767025E7326BB7278E81247F37E194 / B88AA9511EC928549E21A1B7C02C7AFBC115979992CCF540704688FA56E617B1 / D372E85227A2EDA6500167BECA6073D4AF1D6C69FBA8E114F0F711DD6C8F9C9E`；三包repository signature有效、exact dependencies正確，排除`.signature.p7s`後local/official entry differences均為0。剩Daedalus真SPAA consumer gate。

## 2026-09-28 - True-consumer viewport verifier observer effect

- Daedalus以official graph在真SPAA 960 bars／8 rows回報48 preset偶發10秒timeout，以及200→All為2153–2240ms。相同服務與graph下，Chrome 2,444 bars interaction INP為17ms；把formal gate從whole-page `GetByText(Regex)`改讀既有`data-testid=ta-viewport-range`後，960 bars的48/200/All click為119/318/144ms，200→All總計1315ms。
- 原formal locator每50ms重掃大型DOM全文，observer cost被算入2秒產品門檻；同時段原locator仍量得2115ms，形成可區分的A/B。Renderer contract與package graph不變，不為測試觀測器差異發布空版本。
- Consumer gate仍維持2秒且需由Daedalus正式重跑：使用direct test-id locator，initial barrier明確等待非零Loaded。Diagnostic副本後段在unrelated marker wait失敗，故本輪不宣稱完整真SPAA E2E PASS。

## 2026-09-28 - Committed viewport state barrier candidate

- Correction：Daedalus以direct `ta-viewport-range`與non-zero Loaded barrier仍連續量得200→All `2072/2137ms`、click約40ms；先前「主要是whole-page locator observer effect」不足以解釋真consumer RED。
- Renderer `0.1.89`讓range文字直接訂閱committed `uiState`與draft，不再等待整棵chart stack replacement；scheduled rows仍由既有ready barrier表示完成，public API、generation與single-render contract不變。
- 新增`verify-ta-viewport-state-commit-playwright.fsx`。4,000-bar owner gate量得committed state `170.88ms`、rows-ready total `369.95ms`、render sequence `3->4`、visible `1..4000`、console/page error=0；focused suites維持`55/15/17`。
- Candidate exact graph為Contracts `0.1.33`、Renderer `0.1.89`、Interactive.Client `0.1.80`、Dynamic.Ptcs `0.1.54`、Ptcs.Client `0.1.90`。Official publication/readback與Daedalus真SPAA consumer gate尚未完成，不宣稱產品驗收。

## 2026-09-28 - Committed viewport state official release

- Source commit `d7d04e8`已push；Renderer `0.1.89`、Interactive.Client `0.1.80`、Ptcs.Client `0.1.90` public push均回`Created`。
- NuGet.org official SHA-256依序為`94FAC37832CC1AD94FE0B355F7DB2FA4E452F2EDC5016DA75D88037F8121B1F6 / 4E97D7FE0FFE0E001ADB7C1B7A5A7113A5FC99B5838CB703FFFB413E68BFCCDE / 39E6C62BE773D7D85D84291C0D9F24445A9086E9087AF6BB1FB104E5792C14FF`。
- 三包NuGet.org repository signature有效、exact dependencies正確，排除`.signature.p7s`後local/official entry differences皆為0；Interactive bundle manifest明列packageVersion `0.1.80`。Exact graph已交Daedalus跑真SPAA，consumer結果pending。

## 2026-09-28 - Stable viewport controls／coverage refresh correction candidate

- Daedalus以official `0.1.89/0.1.80/0.1.90`四次量得direct committed state `1788..2012ms`，並重現48 intent被accepted same-identity coverage refresh覆蓋回All；前版owner pass不足以關閉consumer RED。
- Renderer將range/presets移出`chartRuntimeView × chartUiState` replacement subtree；LoadedCoverage projection change仍可選full preparation，但不再冒充`CoverageIdentity` replacement套用default viewport。API／wire／action contract不變。
- 新local graph為Renderer `0.1.91`、Interactive.Client `0.1.82`、Ptcs.Client `0.1.92`。Focused `55/15/17`；4,000-bar focused state/rows=`160.21/373.24ms`，完整gate=`586.83/690.59ms`且正式phases over100=0；same-identity revision accepted callback後保留48 bars。Official release/readback與真SPAA pending。

## 2026-09-28 - Stable viewport controls／coverage refresh official release

- Source commit `83df45b`已push；Renderer `0.1.91`、Interactive.Client `0.1.82`、Ptcs.Client `0.1.92`已public push。Interactive首次pack遇到殘留project-scoped `wsfscservice` CLR exception；停止該明確compiler helper後，同一source完整WebSharper build成功，保留既有`WSB9002` standalone fallback，未停用compiler或沿用舊bundle。
- NuGet.org official SHA-256依序為`FC4A84D8635F33EC8537AC20E24C55EDB6A79612D722C5F2B8A9BDF6B159ED83 / 564F1544CDABEE10A1E53B213703E4E0108B09823DC22B3C1812B07892953F2E / 63040D116859EA08DA4E6196A136F6620D83837389F3B550CA218C8FB7350A88`。
- 三包repository signature有效；official nuspec exact依賴Contracts `[0.1.33]`、Renderer `[0.1.91]`及PTCS `[0.2.46]`，Interactive manifest為`0.1.82`；排除`.signature.p7s`後local／official entries皆`Different=0`。Daedalus真SPAA consumer gate仍pending。

## 2026-09-29 - Latest viewport／row Edit／overview OHLC owner candidate

- 真SPAA回饋揭露pending remote action可能吞掉後續viewport intent、長label會遮蔽Edit；另需以OHLC micro-candles取代overview close-only線，且generic renderer不得推論Signal／Order／Fill。
- Renderer完成local immediate＋single latest queued viewport、52px Edit、bounded wick/up/down overview paths與browser event-to-render telemetry。BrowserDemo增加versioned bundle URL及no-store，避免人工驗收黏到舊bundle。
- Focused Renderer／Interactive.Client／Ptcs.Client為`55/15/17`；完整browser gate的200→All state／rows為`655.91/734.35ms`，cursor event-to-render p95/max=`2/26ms`，正式phase無>100ms task。Candidate graph為`0.1.92/0.1.83/0.1.93`；official release/readback及Daedalus consumer gate待完成。

## 2026-09-29 - Latest viewport／row Edit／overview OHLC official release

- Source commit `ee7e77e`已push；Renderer `0.1.92`、Interactive.Client `0.1.83`、Ptcs.Client `0.1.93` push均回`Created`。完整exact graph為Contracts `0.1.33`／Renderer `0.1.92`／Interactive.Client `0.1.83`／Dynamic.Ptcs `0.1.54`／Ptcs.Client `0.1.93`。
- NuGet.org official SHA-256依序為`524DF633CD3D185D44AD74ECD401300958AE0DC8D9817AC4564571301E56C58F / BEC14B98569220941967F1A24B95154FDBDE15460DCCB26A4BB517F5B3A9CA80 / 7F539F22755FAC321BD39C05C62816400146833488A21C7BAB625AC7DDFEE643`；三包repository signature、exact dependencies、Interactive manifest `0.1.83`與排除`.signature.p7s`後local/official entry parity全綠。
- Exact graph與owner gates已direct交付Daedalus。Scenario atomic switch仍屬consumer complete-candidate prepare/commit；generic cursor-event-only trace本輪不存在，已列為後續contract gap而未擴 scope。

## 2026-09-29 - Accepted semantic-equivalence owner candidate

- Daedalus真SPAA重現200→All有時render `+2`：local viewport已提交後，accepted callback只提高Document/Coverage/Query/transport counters，Renderer仍將counter當presentation replacement再畫一次。`EditorSchemas=[]`則是consumer document未author capability，不由Renderer合成。
- Renderer新增semantic document comparison，只正規化LoadedCoverage的CoverageRevision/QueryGeneration；CoverageIdentity、segments、overview anchors、active detail、rows、schemas、actions與其餘DefaultView仍是authority。stable shell的revision diagnostics改為reactive attributes，另移除preparation前置chart-state寫入。
- Focused suites為Renderer／Interactive.Client／Ptcs.Client `55/15/17`。Callback-settled 200→All state／rows=`175.88/378.52ms`、render=`3->4`；完整browser gate五列資料替換、cursor與正式phase效能全綠。Candidate graph為Renderer `0.1.94`、Interactive.Client `0.1.85`、Ptcs.Client `0.1.95`；official publication/readback及Daedalus真SPAA pending。

## 2026-09-29 - Accepted semantic-equivalence official release

- Source commit `2c7bf76`已push；Renderer `0.1.94`、Interactive.Client `0.1.85`、Ptcs.Client `0.1.95` push均回`Created`。
- NuGet.org official SHA-256依序為`DB3565D8CC8A39C8DD35961B5D812AC550071EA9E4DCBC8A92CDC01ABAEB68AD / 63E1AEECE6DFE2431B62007A85A771B92D20980D17626FE4509CDAD9113086C4 / 3C01541AFCC1FA72E576F794D60B6314104BE9928B70D0537A36C69C85275009`。三包repository signature、exact dependencies與排除signature後entry parity全綠；Interactive bundle manifest明列packageVersion `0.1.85`。
- Official graph已direct交付Daedalus；其consumer bump與真SPAA T-123/T-124 gate待執行，尚未宣稱產品驗收。

## 2026-09-29 - Pending navigator boundary latest-intent owner candidate

- Daedalus更正progressive gate證據：前一個toolbar Earlier已成功，後續whole-selection越左界時action count `3->3`，並非response payload未套用。根因是Renderer以`PendingActionId`拒絕navigator mousedown/release，把remote serialization誤當local pointer hard-disable。
- Renderer讓boundary drag與preset共用單一latest-wins viewport queue；pending期間保留local preview，settled後以最新runtime重算adjacent intent，identity replacement清除queue。Public action/wire/API不變。
- Candidate exact graph為Renderer `0.1.95`、Interactive.Client `0.1.86`、Ptcs.Client `0.1.96`。Focused suites `55/15/17`；focused F# Playwright兩次PASS `callbacks 0->2 / queryGeneration 2 / visible 405-452`；完整BrowserDemo gate PASS，正式phase無>100ms task。Official publication/readback與真SPAA progressive gate pending。

## 2026-09-29 - Pending navigator boundary official release

- Source commit `67d8ea2`已push；Renderer `0.1.95`、Interactive.Client `0.1.86`、Ptcs.Client `0.1.96` push均回`Created`。
- NuGet.org official SHA-256依序為`B57E548F2B3E0BABB01F3B687DAE587078AB09236F924CB7EBF739187D553978 / 751A0A9A1D4A45EAFE7761B80B5848F1DB5B25AC46F4D802BE6E5859889A8DFF / 3E7BFB0C122336742C03EF0D88F168EBFFF96102C552D4C520B9E51E94914DB6`。三包repository signatures、exact dependencies與排除signature後entry parity全綠；Interactive bundle manifest=`0.1.86`。
- Official graph已可供Daedalus升版重跑ProgressiveCoverage；consumer gate未回覆前仍標示pending。

## 2026-09-29 - Poll-state drain／browser cache phased rehydrate owner candidate

- 真SPAA揭露前一版仍有兩個owner缺口：實際artifact把`PollInFlight`列入local hard-disable；`PausedForResync -> Ready`若沒有local action settlement，queued boundary intent不會排出。另大型BrowserRuntimeCache cache hit在phased pump前重複同步掃完整snapshot，造成155ms long task。
- Renderer將local hard-disable限縮為`Unmounted/Disposed`，並由runtime state transition與action settlement共同drain single latest queue。Interactive.Client先驗header/document/workspace/current authority，再以既有frame pump分段驗nested data並atomic publish；fail-closed、generation supersede與public API不變。
- Candidate graph為Renderer `0.1.98`、Interactive.Client `0.1.90`、Ptcs.Client `0.1.99`。Focused `55/15/17`；PollInFlight與PausedForResync兩條boundary gate、3820x28 cache gate、Edit geometry及完整Renderer Playwright皆PASS。Official publication/readback與Daedalus真SPAA progressive/cache-hit gates pending。

## 2026-09-29 - Poll-state drain／browser cache phased rehydrate official release

- Source commit `16dd825`已push；Renderer `0.1.98`、Interactive.Client `0.1.90`、Ptcs.Client `0.1.99` push均回`Created`。
- NuGet.org official SHA-256依序為`4792432C548E2B62E9C1F73FAA77D49EF7ED00326507BF604900DA6CD002EE41 / BF9B06EA76660CB1F27AC774E00A5CD9999A283624284B898528A14BD8087CC3 / 00863B675F01162070076F71FB5C9B25B5D43AF8A26F81781AADBA3525414D3D`。三包repository signatures、exact dependencies與排除`.signature.p7s`後entry parity全綠；Interactive bundle manifest=`0.1.90`。
- Official graph已可供Daedalus升版重跑真SPAA progressive／cache-hit／Edit gates；consumer回覆前DYN-TA-027D/E仍不結案。

## 2026-09-29 - Live navigator-root geometry candidate

- Daedalus以official Renderer `0.1.100`／Interactive.Client `0.1.92`真SPAA證明direct interaction-surface仍未送出boundary action；package與bundle皆為新版本，非stale asset。
- 完整owner gate定位inner rect與outer bordered SVG geometry不同：48-bar drag出現delta `-3954`、requestedStart `-2`，誤走adjacent Earlier。Renderer改由transparent surface bubble到live outer SVG，直接以currentTarget作event與bounds authority，移除mutable DOM reference；action/wire與queue contract不變。
- Candidate graph為Renderer `0.1.102`、Interactive.Client `0.1.94`、Ptcs.Client `0.1.103`。Focused suites `55/15/17`；focused四條boundary及完整Renderer F# Playwright均PASS，Interactive package verifierPASS。Official publication/readback與真SPAA consumer gate pending。

## 2026-09-29 - SPAA RESYNC/action settlement joint drain candidate

- 真SPAA以official `0.1.98/0.1.90`重現：navigator boundary drag發生於`RESYNC`且前一個HTTP action仍pending；該action完成revision 4並回到`READY`後，action count仍`3 -> 3`。正式SPAA直接將Renderer綁至HTTP/cache callback，非Interactive WebSocket remount問題。
- Renderer新增`(RuntimePollState, PendingActionId)`聯合reactive gate，任一gate轉換均重新嘗試single-latest queued viewport intent；identity replacement清queue、single remote in-flight與wire shape不變。F# Playwright新增pending-action期間轉RESYNC、拖boundary、READY後唯一重送的交錯情境。
- Candidate graph為Renderer `0.1.99`、Interactive.Client `0.1.91`、Ptcs.Client `0.1.100`。Focused suites `55/15/17`；三條boundary gate為`0->2 / 0->1 / 0->2`，完整Renderer browser正式phase無>100ms task，Interactive package manifest/exact dependency gate通過。Official publication/readback與真SPAA rerun pending。

## 2026-09-29 - Navigator interaction-surface direct binding candidate

- Daedalus A/B證明真SPAA即使先等READY，selection-center drag仍action `3->3`且feedback未進queue；`.99/.91/.100`的joint drain保留為state-race hardening，但不是gesture miss closure。
- Owner focused gate改用相同的一次性框外12px座標後仍通過，排除負座標／document mouseup。Renderer將drag start由SVG root bubble移至canonical透明interaction surface direct handler；document move/up、24 CSS px hit resolver及coverage contract不變。
- Candidate exact graph為Renderer `0.1.100`、Interactive.Client `0.1.92`、Ptcs.Client `0.1.101`。Focused suites `55/15/17`，focused browser最終三情境`0->2 / 0->1 / 0->2`；完整Renderer正式phases無>100ms，package verifier PASS。第一次Paused focused run受前一action settlement競態失敗，立即重跑通過；consumer formal progressive仍是最終gate。

## 2026-09-29 - Navigator interaction-surface official package handoff

- Source commit `7458f3a`已push；Renderer `0.1.100`、Interactive.Client `0.1.92`、Ptcs.Client `0.1.101`三包push均回`Created`。
- NuGet.org official SHA-256依序為`759432FCE6029F0011D6BEB5D6A6141C8A1B2AD9B0468A40273345A828D7A3F / 66F93800F42094FEC82889C389C19B9CA6F760AD286EC1648BEFA2014F0B0899 / 535FE654ACCB9E412AEF1686238CAA4EAB81ABC0B1EF76F15B69A67B651698B3`。Repository signatures、exact dependencies、Interactive manifest `0.1.92`及排除`.signature.p7s`後entry parity均PASS；真SPAA formal progressive仍由consumer驗收。

## 2026-09-29 - Navigator live-root package provenance correction

- Renderer `0.1.102`、Interactive.Client `0.1.94`、Ptcs.Client `0.1.103` 的功能驗證與NuGet.org驗簽通過，但其nuspec repository commit仍指向打包前HEAD `d50f180`，不交付consumer。
- 只升版重建graph為PulseTrade.Comm.Spa.Dynamic.Renderer `0.1.103`、PulseTrade.Comm.Spa.Dynamic.Interactive.Client `0.1.95`、PulseTrade.Comm.Spa.Dynamic.Ptcs.Client `0.1.104`；兩個client exact依賴Renderer `[0.1.103]`。此段提交後才由該HEAD重建，以使package provenance對應實際source。

## 2026-09-29 - Navigator live-root official immutable handoff

- Source commit `651ecc5`已push；Renderer `0.1.103`、Interactive.Client `0.1.95`、Ptcs.Client `0.1.104`依序push並由NuGet.org flat-container重新下載。
- Official SHA-256依序為`EACCEAB5485514FC2052190BBF0CE5060069DCA060948F17A9B9334D46E68FC8 / 317AADCE892318CFE02E02D2F9C98F51B8B4426C109936B7C51492E0CFC8E32E / 3FA87B12ABE2958F1B3BA5AAD6F92F10BA4C1243977F629C71C2C11BCFDADE09`。Repository signatures、source commit、exact dependencies、Interactive manifest `0.1.95`與排除signature後entry parity皆PASS；真SPAA formal gate交由Daedalus執行。

## 2026-09-29 - Navigator cross-frame pointer capture candidate

- Daedalus以official `.103/.95/.104`真SPAA驗出root x=82、release x=70時action仍`3->3`。bundle已含child document mouse handlers，故根因是pointer離開child browsing context後release未回到該document。
- Renderer `.104`改以pointerdown取得live outer SVG capture，move/up/cancel由同一root處理；capture不可用才退回document pointer listeners。新增iframe fixture與F# Playwright，frame x=80、navigator x=101、release x=68仍callback `1->2`、outcome=`request-earlier`。
- Local exact graph為Renderer `.104`、Interactive.Client `.96`、Ptcs.Client `.105`。Focused suites `55/15/17`、同document四條boundary、跨iframe及完整非效能功能／幾何gatePASS，Interactive package verifierPASS。第一次strict完整gate僅既有200→All state `808.45ms`超750ms，acceptance phases無>100ms task；official publish/readback與真SPAA closure pending。

## 2026-09-29 - Navigator cross-frame official immutable handoff

- Source commit `237236b`已push；Renderer `.104`、Interactive.Client `.96`、Ptcs.Client `.105`依序push並由NuGet.org flat-container重新下載。
- Official SHA-256依序為`BDA12686932E40C5C89C0D9BB6E28E36BDCE5A217449DC51A493ED6DBB9DB64C / C994700042F0E556B367200AF7312A56B23735C4062052A4BCD5F43927A6204F / 4E85B0FC3FF2BE962401CDB015BE36B9E9A8BF5D73FFFAF3E9DB4F3421A643A3`。Repository signatures、source commit、exact dependencies、Interactive manifest `.96`、official bundle pointer-capture code與排除signature後entry parity皆PASS；真SPAA formal gate交由Daedalus執行。

### Correction：完整package版本追溯

- `PulseTrade.Comm.Spa.Dynamic.Renderer`由`0.1.103`升至`0.1.104`；`PulseTrade.Comm.Spa.Dynamic.Interactive.Client`由`0.1.95`升至`0.1.96`；`PulseTrade.Comm.Spa.Dynamic.Ptcs.Client`由`0.1.104`升至`0.1.105`。原因皆為navigator跨browsing-context Pointer Capture修正及exact downstream closure。

## 2026-09-29 - Navigator cross-frame consumer closure

- Daedalus對official Renderer `0.1.104`／Interactive.Client `0.1.96`的首次RED diagnostic顯示drag start `elementFromPoint`為空，且handler／capture／mode／delta attrs皆未出現。projection後navigator已離開viewport，consumer verifier沿用stale raw page mouse座標；這不是owner bundle failure。
- Consumer gate加入`ScrollIntoViewIfNeeded`、重新取得navigator／selection geometry及drag-start overview hit-test後，18883正式SPAA action由`3`增至`4`，最終initial=250、merged=500、authorityLoaded=1000、visible=1-250、selectionWidth=250、axes=8、runPosts=2、actions=5。DYN-WBS-575與DYN-T-656完成；不發布額外package。

## 2026-09-29 - DECIDE_ON overview／provider-open-earlier owner candidate

- RFC-PTCS-DYNAMIC-0033實作8,000 bounded overview、explicit overview/detail axis、跨軸event-time selection/stripe alignment與stable `viewport.loadedCoverageDataRef`。LoadedCoverage observation domain不再由overview ordinal延長。
- `TaCoverageWindowIntent`新增additive `RangeAuthority`；range-less Earlier以`ProviderOpenEarlier`及loaded-head anchor表達，不產生UnixEpoch假起點。PTCS flat wire使用`provider-open-earlier`，舊wire缺欄位維持`explicit-bounds`。
- Renderer在current generation prepared rows追上authored rows前停用toolbar／navigator adjacent outcome，避免document replacement race；完成後保留既有pending latest-wins。
- Owner focused suites為`49/58/15/15/17`。完整Renderer Playwright PASS：500 coverage／250 detail、tiny selection、8,000 bounded overview與既有interaction均通過；progressive max45.37ms，正式phase over100=0。Interactive.Client使用fresh package cache重建bundle並通過package verifier；global-cache stale Renderer同版號未作release evidence。
- Candidate exact graph為Contracts `0.1.34`、Renderer `0.1.106`、Interactive.Client `0.1.97`、Dynamic.Ptcs `0.1.55`、Ptcs.Client `0.1.106`。本段先提交source以固定package provenance；official push/readback及Daedalus consumer gate待後續。

## 2026-09-29 - DECIDE_ON overview official immutable handoff

- Source commit `f0cd0d3`已push；五包Release build及NuGet push均成功。NuGet.org official repository signatures、repository commit、exact dependencies與排除`.signature.p7s`後local/official entry parity全綠。
- Official SHA-256依序為Contracts `383164BBDFEC6A75581320F85029D6DC75439BB5F829D164EC7DE284B85A7387`、Renderer `615209F243D619943B30AD98048AAE3494559BC16FB0ABF966DA83F7E17B18AD`、Interactive.Client `E9EDEA509C7117F93ACCC13DBC63E5B2411C22D2ADC15CA53FE2B8F29CA2EDB9`、Dynamic.Ptcs `63B20745E8ACE165102CA27950C5E1E702A478068685EA36B134455A1209187C`、Ptcs.Client `69EC25D9E8726FC8C503DCAB9F9407DFE20ED5A85FE50A2F7FE5749F53B5F4EA`。
- nuget.org-only fresh cache focused suites為Renderer／Interactive／Dynamic.Ptcs／Ptcs.Client=`58/15/15/17`；Contracts source suite=`49/49`。Final graph已direct交付Daedalus；真SPAA與fresh DIB仍是consumer pending。

## 2026-09-30 - Navigator local-first responsiveness owner candidate

- 真SPAA回報drag preview、View All／pan可見更新與overview時間軸缺口。Renderer改為rAF latest-draft、pointerup同步flush、stable shell先更新而chart延後一個paint；另將`viewportDataReady`與`preparedRowsReady`分離，remote pending／row remount不再停用loaded-range local pan。
- Overview新增canonical event-time adaptive axis；View All在loaded<=maximum時selection立即滿寬。完整F# Playwright與focused `49/58/15/15/17`通過；200→All=`373.16/752.64ms`，正式phases over100=0。
- Candidate graph為Contracts `0.1.35`、Renderer `0.1.109`、Interactive.Client `0.1.100`、Dynamic.Ptcs `0.1.56`、Ptcs.Client `0.1.109`。先前本機中間graph不可發布；official provenance build/readback與Daedalus真SPAA pending。

## 2026-09-30 - Loaded-domain View All owner correction

- 真SPAA的`Loaded 3022 · Viewing 789-1512`暴露View All仍以724根active detail計算。Renderer新增pure loaded-domain intent，target為`min(loaded total, MaximumVisibleBars)`；超出active detail時沿用`VisibleRangeChanged + ta-coverage-window.v1`，並以generation-aware latest queue等待authoritative replacement。
- BrowserDemo新增3022/724 regression：按All後唯一callback使generation前進至2、active detail變為`0+3022`、range=`1-3022`、runtime axis/price=`3022`且navigator selection滿寬。完整Playwright PASS，focused suites=`49/58/15/15/17`。
- Candidate graph為Contracts `0.1.35`、Renderer `0.1.121`、Interactive.Client `0.1.112`、Dynamic.Ptcs `0.1.56`、Ptcs.Client `0.1.121`；official provenance build/readback與Daedalus真SPAA pending。

## 2026-09-30 - Adjacent cache KnownEmpty authority correction

- Daedalus真persistent cache gate證明`.112`在active=`0+724`、loaded domain=`4724`時，會因Later遠端零長segment誤回KnownEmpty，導致provider follow page被Accepted no-op短路。
- `BrowserRuntimeCache.selectAdjacent`改為先判direction target ordinal；domain內仍有target時回Miss，只有沒有target且零長segment緊貼active boundary時才KnownEmpty。Public API／wire不變。
- 舊`.112` unit穩定RED；`.113` unit `15/15`、IndexedDB seed→reload→Later read=`MISS`、large cache既有gate及package verifierPASS。`.112`雖已push但標為中間版本，final consumer graph改用Interactive.Client `.113`。
- Package version：`PulseTrade.Comm.Spa.Dynamic.Interactive.Client 0.1.113`（前版`0.1.112`）；原因為adjacent cache KnownEmpty authority correction，exact依賴維持Contracts `[0.1.35]`與Renderer `[0.1.121]`。

## 2026-09-30 - Navigator responsiveness official immutable handoff

- Final graph為Contracts `0.1.35`、Renderer `0.1.121`、Interactive.Client `0.1.113`、Dynamic.Ptcs `0.1.56`、Ptcs.Client `0.1.121`。Renderer／Ptcs.Client source commit=`c2510e71e8b517bb203efa7ea3e34d42c78ce901`；Interactive.Client source commit=`1cee2206db26c4cb85f9b38a7036f23b61f2d540`。
- NuGet.org official SHA-256為Renderer `06C1D94A3906AFB725E0AD029A2381E78355296C90715AF8E2407A03B52A5CE9`、Interactive.Client `7594E172BF9E8F50B601419FA85CDB21EB7FE7C3A3D8A4AE40954E0B3ACF2DEF`、Ptcs.Client `D7D7767B8EB811AA9F7404CB137349D9485F938B2A8274D2D93DA2F2E470B674`。三包repository signatures、exact dependencies、provenance與排除`.signature.p7s`後entry parity通過；Interactive manifest=`0.1.113`。真SPAA gate交由Daedalus完成。

## 2026-09-30 - Navigator responsiveness consumer closure

- Daedalus以final graph完成兩次max4000 persistent SPAA完整run，area／left／right／retained為`477/362/418/409ms`及`612/478/470/440ms`；4000→8000、View All、persistent rerun、provider left/right與overview axis同輪PASS，consumer alpha47已push。
- 先前一次cold left `1499ms`未在連續run重現，保留為observation，不宣稱已修產品bug也不列owner blocker。DYN-WBS-577／DYN-TA-028關閉為100%。

## 2026-10-01 - RFC-0034 navigator overview integrity owner gate

- Renderer新增contiguous OHLC aggregation，保留完整source coverage與bucket extrema；OC／scalar及mixed-authority bucket維持body-only。Navigator補齊`ew-resize`／`grab`／`grabbing` affordance及typed loaded Start／Latest controls，不新增wire contract。
- Exact candidate graph為Renderer `0.1.123`、Interactive.Client `0.1.115`、Ptcs.Client `0.1.123`；focused tests=`63/15/17`，Interactive package manifest gate PASS。
- 完整Renderer F# Playwright gate通過：300 cursor transitions、chart rerender=`false`，正式five-candle／scenario／All／marker／document／progressive phases均無>100ms task。Verifier改以相對revision／generation驗每個accepted intent恰前進一次，避免edge jumps後沿用過時絕對值。
- NuGet.org尚無上述三個版本。先提交source固定package provenance，再重建、push及official readback；Daedalus真SPAA consumer gate仍待執行。

## 2026-10-01 - RFC-0034 official immutable handoff

- Source commit `97cb2244e239192b55bb3ef37c0347974823f9ef`已push；exact graph為Renderer `0.1.123`、Interactive.Client `0.1.115`、Ptcs.Client `0.1.123`，兩client exact依Renderer `[0.1.123]`。
- 三包已由該commit重建並push。NuGet.org official SHA-256依序為`9CD49C793D5A5BA5B596A187859D695BC2484B16886B38404AD98D24CE8947D8`、`628CCC9BB8E0005AA60DE5F18F3B4461ED872519DADEF0F75131EB42EDDAD761`、`B9ECA910CFD36EE7EC918EC46D1C59A706293F54AC2B8DD3526194AAFDF31D03`。
- Official repository signatures、repository commit、exact dependencies與Interactive manifest `0.1.115`均正確；排除`.signature.p7s`後local／official entries=`7/10/7`且differences皆0。nuget.org-only fresh-cache focused suites=`63/15/17`。Owner／release gates完成，Daedalus真SPAA consumer gate待完成。

## 2026-10-01 - PTCS bounded inbox chunk dependency alignment

- PTCS新增agent inbox snapshot chunk authority後，三個direct consumers改為exact `PulseTrade.Comm.Spa [0.2.47]`：`PulseTrade.Comm.Spa.Dynamic.Ptcs 0.1.57`、`PulseTrade.Comm.Spa.Dynamic.Ptcs.Client 0.1.124`、`PulseTrade.Comm.Spa.Dynamic 0.1.26`。
- 這是package dependency/provenance alignment；未改Dynamic renderer、WebSharper bundle source或runtime行為。
- `Dynamic.Ptcs`與`Ptcs.Client` Debug build/pack皆為0 warnings / 0 errors；主Dynamic Debug compile為0 errors，僅既有WebSharper project-type warning。正式發布需在source commit後執行。

## 2026-10-01 - Navigator global loaded-domain drag correction candidate

- Daedalus真SPAA驗出loaded=900／active=200時selection與pointer delta使用不同domain。Renderer改以完整loaded observation domain產生rAF local draft；active detail只作release local mapping，跨頁沿用`ta-coverage-window.v1`。
- latest-wins coverage queue不再保存已序列化action；只保存global target，settle／READY後以最新projection重建coverage revision與query generation，避免stale intent。
- 未發布中間graph `.124/.116/.125`與`.125/.117/.126`均不交付；int64 loaded ordinal regression後final candidate為Renderer `0.1.126`、Interactive.Client `0.1.118`、Ptcs.Client `0.1.127`。前一candidate focused=`64/15/17`且完整Renderer、pending/resync/iframe與Playwright MCP gates全綠；final graph須重建重驗。Source provenance commit、official push/readback與Daedalus真SPAA待完成。

## 2026-10-01 - Correction: navigator final owner graph and performance gate

- 前一candidate在final browser gate重現global draft latency `283.08ms`，超過`<=250ms`契約。根因是pointermove每一步仍同步改寫navigator與chart-stack diagnostics；draft本身雖已rAF合併，diagnostics未合併。
- Renderer把draft與diagnostics收斂到同一local rAF，pointermove不dispatch callback、不重建chart rows。為避免本機NuGet cache誤吃已建過的candidate，final graph順延為Renderer `0.1.127`、Interactive.Client `0.1.119`、Ptcs.Client `0.1.128`。
- Final owner gates：focused=`64/15/17`；完整Renderer Playwright、pending/resync/iframe五情境、Interactive package verifier與Playwright MCP loaded fixture均PASS；正式phase over100=`0`、console error=`0`。Source provenance commit、official push/readback與Daedalus真SPAA仍待完成。

## 2026-10-01 - Global loaded-domain navigator official immutable handoff

- Source commit `f27b43bc634e7458bb2818b76ae0475b78268db0`已push；official graph為Renderer `0.1.127`、Interactive.Client `0.1.119`、Ptcs.Client `0.1.128`，兩client exact依Renderer `[0.1.127]`。
- NuGet.org official SHA-256依序為`CFA4B9C843AC459A312380B8981FA5E8F73CD118F850B90BBE33D232D4B88DC8`、`D705CA283E4C022CF3D6A0BB0AE661D7E795089272E2309E68921C268E7A7522`、`C41669E240827E660E6C05DC90A526E8196176F8D925D4774F885E30EA586E72`。
- 三包repository signatures、repository commit、exact dependencies與Interactive manifest `0.1.119`正確；排除`.signature.p7s`後local／official entries=`7/10/7`且differences皆0。NuGet.org-only fresh-cache suites=`64/15/17`。Owner／release gates完成，Daedalus真SPAA 900/200 consumer gate待完成。

## 2026-10-01 - Navigator rendered-preview release receipt candidate

- Daedalus真SPAA exact equality gate在official `.127/.119/.128`穩定重現Preview start=`678`、wire/final start=`676`；BrowserDemo 900/200 deterministic fixture亦以Preview=`677`、wire ordinal=`674`重現。根因是WebSharper reactive `Var`可先於可見DOM前進，pointerup讀published state仍可能提交人類尚未看到的下一sample。
- Renderer以viewport range `afterRender`保存exact visible receipt；普通release優先該receipt，只有pointer明確跨出loaded boundary時才優先final clamped pending intent。Wire、consumer contract、pointermove zero-action與pointerup single-action不變。
- Local candidate graph為Renderer `0.1.130`、Interactive.Client `0.1.122`、Ptcs.Client `0.1.131`。Focused suites=`65/15/17`；exact parity、pending/resync/iframe及完整F# Playwright全綠，正式phase無>100ms task。Playwright MCP因`G:`零可用空間回`ENOSPC`，不列PASS；official push/readback與真SPAA consumer gate待source commit後執行。

Correction：Renderer `0.1.130`首次push雖被NuGet.org接受，但增量Pack沿用舊nuspec，repository commit仍為`667e963`，故該版retired且不得納入graph。正式correction graph升為Renderer `0.1.131`、Interactive.Client `0.1.123`、Ptcs.Client `0.1.132`，須由version commit後clean rebuild並在push前驗repository commit。

## 2026-10-01 - Navigator exact parity official correction release

- Source `4812e6efa99631183f889fefb7d2036f7849c62e` clean rebuild並發布Renderer `0.1.131`、Interactive.Client `0.1.123`、Ptcs.Client `0.1.132`。三包push前本機nuspec已驗repository commit；NuGet.org readback signatures、exact dependencies、Interactive manifest與排除signature後entry parity均PASS。
- Official SHA-256依序為`E63806A5F898B297601D6F9FC1BC6DDAA7C988A5C9E8CDACF9D81FD9B842E332`、`2D0C1D09D49C699AC41142A3D4F64ECF075F288870A63FC0919AE0616AE231BA`、`597235E485659CB3EA38BF839E4D897E9EA404832770125BA5E392C4438F5CE1`；NuGet.org-only fresh-cache suites=`65/15/17`。
- Final official BrowserDemo在40px／12-step move後等待250ms，Preview=`675`、wire ordinal=`674`、count=`200`且authoritative final exact；pending/resync/iframe及完整browser gate PASS，正式phases無>100ms task。Daedalus真SPAA exact equality仍為consumer closure。

## 2026-10-03 Aster Spa48 adapter source checkpoint
Package compatibility: Ptcs0.1.57→0.1.58, Ptcs.Client0.1.132→0.1.133, umbrella0.1.26→0.1.27 exactSpa0.2.48. NuGet push pending; mainHost/GWTests/TAClient downstream highest priority; Contracts35/Renderer131/Interactive123 unchanged。Baseline0errors，新version finalbuild/package/official仍pending；不是TA semanticfix。REQ/SA/SD/UPSTREAM_RFC/WBS581/TEST676/Verification050 updated；preworklog20261003/20261003005800.aster_spa48_adapter_cascade.log。Renderer existing11diagnosticattrs另WIP保留，不stage。

Active local package test/demo closure: Ptcs.Tests old[0.1.56]→[0.1.58], PtcsTaClient.Tests old[0.1.132]→[0.1.133], Ptcs.LiveDemo oldSpa46/Ptcs56/Client132→Spa48/Ptcs58/Client133. These known active clients must restore/build against final candidate before PASS; no source test expectations change. No Run of LiveDemo/automatic hostspawn asbuild gate. Legacy sourceACL/Login18 remain excluded.

## 2026-10-03 Aster Spa48 local package compatibility checkpoint
- Dynamic Ptcs58/Client133/umbrella27 finalproduct identity2927953，local3nupkg zip/DLL/deps/generatedbundle PASS；NuGetpush未執行。兩existingexactpackage consumers15＋17=32/32PASS，0ignoredfailedErrored；loadeddependencySpa48source83/hash相符，不混source/RendererWIP。
- WebSharper failedfinalbuild為unconditionaldeleteignoredlog ACLdenial，與memory無關；兩file rename被OS拒絕且原檔保留。FrozenGitblob/rawSHA immutablebuildstage862inputs成功，無Gitcheckout/sourceedit/daemonkill/productionmutation。TimingLog=false無效，失敗證據保留。
- 第三activeLiveDemo baseline0errors；發現ContentUpdate staleSpa37，同步48（PackageRef已48），修後build待驗。WBS58140/Test676/Verification050 updated；official/mainHostcascade仍pending，不宣稱TAfirst200/SMA bugfix。

## 2026-10-03 Aster Spa48 LiveDemo consumer asset closure
- LiveDemo asset path37→48修正後 fullWebSharper build6.93s/0errors/1existingWS9002 PASS；loadedSpa48+83/hashAF43及7buildassets rawSHA/copy PASS，report C:/Users/Administrator/AppData/Local/Temp/aster-ptcs-package-0.2.48/dynamic-livedemo48.asset-proof.json。未啟demoHost；local unsigned candidate的build/assetproof，不是正式部署/browser/TA semanticfix。
- Compiler regenerated existing tracked LiveDemo JS/min bytes；按既有 generated delivery例外入本批，非手寫JS。Producer package input/hash仍 frozen292，Renderer11attrs WIP不stage。NuGetpush、main13/正式TestHost closure pending。

- 2026-10-03T03:57:33.1906521+08:00 Spa48 exactdependency localcandidate83→055必要consumer refresh完成：Ptcs58/Client133 actual32/32，LiveDemo compiler＋7assets/canonical055DLL PASS；source292三producerinputs unchanged，沒有盲目repack。Main13 localcompile完成，newgraphGW/threeTestHosts/official仍pending。Test DYN-T-676r2/VFY050r2回鏈；selector projection failures guard拒絕並修正，不修改product或expected。

### 2026-10-03T06:21:53.0300588+08:00 DYN-VFY-050 revision3 official／consumer closure
Dynamic0.1.27／Ptcs0.1.58／Ptcs.Client0.1.133 producer29279539806118e04f1b563d9cb881aadddf53db unchanged；3包HTTP201、officialsignature/source/payload/canonicalhash/網站Dependencies PASS。Existing packageconsumer15/15＋17/17、LiveDemo fullWebSharper/7assets/latestSpa055已驗；official normalrestore assets/contentHash/runtimeDLL再驗PASS，無需重編譯/重跑semantic tests。
MaincanonicalG:/PulseTrade.fs source2f7057／四exact builds／GW496／真三TestHostsSDK/resultrestart／active18signedcache/七consumerPASS。Raw C:\Users\Administrator\AppData\Local\Temp\aster-ptcs-package-0.2.48/official12/readback-20261002205627356/result.json、dynamic-consumers055/proof.json、official-active18-promotion/result.json；detail G:/PulseTrade.fs/Libs/PulseTrade.Comm/doc/GW/WBS.PTCS-PACK-REF-001.md。SDK401feed官方bytes，不稱NuGet-only。正式未部署；Renderer diagnostic WIP不compile/stage，無TAsemantic變更。

## 2026-10-03 Aster Spa49 source checkpoint intent
Dynamic `0.1.27` → `0.1.28`、Ptcs `0.1.58` → `0.1.59`、Ptcs.Client `0.1.133` → `0.1.134`，三 producer exact Spa `0.2.49`；同步三 active consumers 與 LiveDemo asset path。原因為 upstream wallet UI/recovery/readiness 相容性，非 TA 行為修改。SA/SD/REQ/WBS582/TEST677/Verification050r4 已先記，baseline三 full builds PASS；新版 local archive/consumer gates pending、NuGet push 未執行。Renderer WIP 保留。prework log `20261003/20261003135116.aster_spa49_adapter_cascade.log`。

DYN-VFY-050r4 pre-checkpoint 新版三 producer full WebSharper/compiler PASS：Dynamic28 11.04s、Ptcs59 9.70s、Client134 8.71s。TEMP/aster-ptcs-package-0.2.48/dynamic-wallet49-baseline-source-r1 保存 865 raw inputs/舊 HEAD + 三 metadata delta proof；其餘 canonical/captured bytes 與 Renderer WIP 不變。兩份既有 umbrella JS/min 為真 compiler Spa49 輸出，依 generated delivery 例外原樣同步，未手改。此批尚未 localpack/consumer/publish。

## 2026-10-03 Aster Spa49 local cascade completed
Dynamic `0.1.28` / Ptcs `0.1.59` / Ptcs.Client `0.1.134` 的 source `cfb6f3b1aef86de5f8adbd676284d0d4c06f6532` 已 push並固定重編；三包 exact Spa `0.2.49`、RepositoryCommit/DLL/deps/assets與SDK feed raw bytes PASS，NuGet push未執行。Main TAClient owner已收到Client134可用通知。
三activeconsumers完成：15+17 actualtests全PASS/0ignoredfailedErrored；LiveDemo fullWeb `7.912s`、新Spa49三consumerDLLidentity與七asset集合/bytes/hash PASS。兩份LiveDemo generated JS/min由compiler原樣同步，這次文件/consumer資產commit不改producer packageprovenance。DYN-WBS-582 local100%，official/main服務/production仍獨立；Renderer既有11diagnosticattrs WIP與.pcsl全保留。詳 DYN-VFY-050r4、原prework與14:25 continuation log。


## 2026-10-03 Spa50 adapter cascade prework（DYN-WBS-583）
Upstream listener startup修正使Spa49鏈維持未public failed-candidate；本片已授權重做exact依賴：PulseTrade.Comm.Spa.Dynamic `0.1.28 -> 0.1.29`、PulseTrade.Comm.Spa.Dynamic.Ptcs `0.1.59 -> 0.1.60`、PulseTrade.Comm.Spa.Dynamic.Ptcs.Client `0.1.134 -> 0.1.135`，共用Spa `[0.2.50]`。Contracts35/Renderer131與既有WIP不動。新fullcompiler/package/consumer尚pending；NuGet push由upstream統籌，本片不public。設計/驗收見SA/SD、DYN-VFY-050r5；prework `log/20261003/20261003155425.aster_spa50_adapter_cascade.log`，新TEMP根 `C:/Users/Administrator/AppData/Local/Temp/aster-ptcs-package-0.2.48/dynamic-wallet50-preparation-20261003`。

Spa50 precheckpoint-r1的10個missing script均>260字元；已建立 `log/20261003/20261003160134_issue_dynamic50_longpath.hypothesis.md`，只用short fresh TEMP roots重驗fullcompiler，舊失敗保留，不弱化gate。

DYN-VFY-050r5 precheckpoint PASS：短路徑 dyn50-pre-r2 三 full WebSharper 為 Dynamic29 9.453s、Ptcs60 8.270s、Client135 9.368s，0 errors；865 canonical/captured raw SHA與clean blob、Renderer hash不變。先前巢狀root r1 MSB3030（10路徑>260）保留，不修ACL/造假assets。兩份umbrella JS/min只從真compiler原bytes同步，固定source checkpoint/final archives/consumer仍pending。

## 2026-10-03 Spa50 adapter local completion
DYN-WBS-583 / DYN-VFY-050r5 本機範圍完成：producer source `636c19b1bb62f9425591c2d2fdb543de1445a9e5`，三fixed fullWeb 10.865/8.550/8.839s；三archive/exactdeps/DLL/assets與SDK401feed逐hash相符。真15+17=32/32（0ignored/failed/errored），LiveDemo fullWeb8.309s、三consumer runtime Spa50 DLL87569E35…與core f217c898匹配、七asset exact集合/bytes/hash與七ownbundle齊全。公開發布、main Host/browser與正式部署仍由upstream另驗。
PulseTrade.Comm.Spa.Dynamic `0.1.28 -> 0.1.29`、PulseTrade.Comm.Spa.Dynamic.Ptcs `0.1.59 -> 0.1.60`、PulseTrade.Comm.Spa.Dynamic.Ptcs.Client `0.1.134 -> 0.1.135` fixed source636c19b，NuGet push未執行；main TAClient owner已收到新3包feed/proof。Final docs commit不重包同version。

## 2026-10-03 Spa50 adapter 公開發布文件收尾

DYN-WBS-583 / DYN-T-678 / DYN-VFY-050r5：Dynamic 0.1.29、Ptcs 0.1.60、Client 0.1.135 已 NuGet 公開，官方12包 closure finalized12/12、pending0；三包 signature、payload/content hash及網站 dependency group/ID/exact range gate皆PASS。原 unsigned archive hashes保留，signed official hashes另列 `doc/Verification.md`，producer636c19b不重包。upstream GW525/525、canonical wallet22/22及三Host隔離MCP/PCSL restart與native cleanup已通過；本repo32/32、LiveDemo七asset proof保持原證據。公開package與隔離測試不表示production已更新，本輪production未動。

本輪只改四文件與既有 `log/20261003/20261003155425.aster_spa50_adapter_cascade.log`；Renderer tracked WIP與五個PCSL目錄保留。fresh掃描/check/scoped commit/push evidence放 `C:/Users/Administrator/AppData/Local/Temp/aster-ptcs-package-0.2.48/dynamic-wallet50-public-closeout`，原branch/origin不變。


## 2026-10-03T20:42:14.2625586+08:00 — DYN-WBS-584 / publisher hooks
八個fsproj九個AfterPack gate修正：專屬opt-in實際生效＋PublishNuGetAfterPack=false全域veto；保留VS/defaults/Config/OS/Exec/version/reference與Renderer/五PCSL WIP。RFC0035、DYN-T-679/VFY051r1；hypothesis `log/20261003/20261003203004_issue_publish_hook_guards.hypothesis.md`。真MSBuild isolated marker RED160/34failed後，PS7/PS5各160/160/sourcehashstable；154行共用typed verifier可供main用ProjectPath[]驗targets。r1 harness RemoveProperties失敗與原尾空白修正均保留。無key/原Exec/真Pack/build/restore/push，已發布版本未重包；這是unit gate而非發布E2E。詳細evidence見Verification051r1；本輪log `log/20261003/20261003203004.aster_publish_hook_guards.log`。


## 2026-10-04 DEP52 Dynamic metadata checkpoint

Dynamic 0.1.29→0.1.30、Ptcs 0.1.60→0.1.61、Ptcs.Client 0.1.135→0.1.136；7檔14literal同步 active Spa [0.2.50]→[0.2.52]、Actor.Registry [0.1.3]→[0.1.4]，接 core ACT-A2 修正，Contracts/Renderer/Interactive及legacy ACL/Login不動。原a299 source單一umbrella fullWeb baseline12.712s／865 inputs穩定；metadata inverse byte/XML/BOM驗證通過。DYN-WBS-585 / DYN-T-680 / DYN-VFY-050r6 與 ownlog log/20261004/20261004034358.aster_dep52_dynamic_metadata.log 追溯；本 checkpoint 不是新版build/package或NuGet push，root尚未交52/4固定proof，production未動。

同批整合 peer DYN-VFY-051r2：共用 hook verifier嚴格支援main Actor.Registry legacy profile，main10每shell200PASS、Dynamic8每shell160PASS、拒絕7/7各PS5/7。只執行 import-free marker，沒有真Pack/key/上傳；prework/hypothesis log/20261004/20261004034629* 與Verification保留RED與PS5 harness失敗。Renderer.fs/PCSL/Playwright並行WIP不stage。

## 2026-10-04 DEP52 generated bundles source checkpoint

三 producer full WebSharper 與865 input freeze通過後，pack前發現兩份 tracked umbrella JS需跟新Spa52相依同步，按 compiler exact bytes更新而非手改JS。先固定新source再重建/pack；consumer與public/host未宣稱完成。證據/限制見 DYN-VFY-050r6、log/20261004/20261004034358.aster_dep52_dynamic_metadata.log。Renderer/PCSL WIP保持。

## 2026-10-04 DEP52 local candidates and required consumers

Dynamic0.1.29→0.1.30、Ptcs0.1.60→0.1.61、Client0.1.135→0.1.136 已以source5267完成三producer、exact本機package/feed、56actualunit、LiveDemo full compiler/7coreassets。依賴Spa52與Registry4；generated2JS先checkpoint再重建，fixedartifact不受後續docs-only commit影響。完整raw/hash與harness反例見DYN-VFY-050r6 actual、DYN-T-680、DYN-WBS-585及log/20261004/20261004034358.aster_dep52_dynamic_metadata.log。NuGet公開/下游Host由root整合，不宣稱prod或real-provider通過；Renderer/PCSL WIP保留。

## 2026-10-05 06:50 +08:00 REL-01 source metadata checkpoint
- PulseTrade.Comm.Spa.Dynamic0.1.30→0.1.31，PulseTrade.Comm.Spa.Dynamic.Ptcs0.1.61→0.1.62，PulseTrade.Comm.Spa.Dynamic.Ptcs.Client0.1.136→0.1.137；7project/10exactrefs。Core53/Registry6要求，Renderer131 unchangedpublished；LegacySpa18/Renderer.fs foreignWIP untouched。
- Main03b1b80a／Coref60767a producer metadata已pushed；Native401/Registry6/WSwin5/IFS401固定localSDK包已verified，Core1374full正在跑，Dynamicnewcandidatefull/unit/Browser/三Hostpending。本metadata只sourcecheckpoint，沒有formalpush/deploy/prod。

### 2026-10-05T08:04:56.3963436+08:00 REL three fixed local producers
Dynamic0.1.31/Ptcs0.1.62/Client0.1.137 source29b3bcd固定build/pack/SDKcopy與Root20closure匹配。InitialDynamic packNU5019漏canvasContent保留FAIL，新增trackedContentcapture（869inputs/845wwwroot）於shortfreshC:/ptc-rel20261005/dynamic-r2完成，既有None/README/Content保留、不改default或foreignRenderer.fs。Package SHA2C18D781/5752E4F5/689EE6C5，完整deterministicProof各root/package-proof.json；localHostbaseline实际MCP通过，正式NuGet/新TA功能/prod另gate。Continuation log/20261005/20261005070705.aster_host_release_continuation.log。

## 2026-10-06 RF-09 Core54 相依整合 checkpoint

Core57a24a5已完成完整WebSharper及1378/1378真native/unit回歸；本repo僅metadata更新Dynamic0.1.31→0.1.32、Ptcs0.1.62→0.1.63、Ptcs.Client0.1.137→0.1.138與三個tests/demo references，Renderer source WIP保留。全域SA/SD/版本影響/pseudocode/驗收由G:/PulseTrade.fs/doc/20261006.REFACTOR/Release.Closure.md與RF-09持有；本repo尚未build/pack/publish/deploy，不宣稱完成。來源checkpoint後從各專案bin/net10.0/agent.aster內固定Git來源建置；bundle的wwwroot仍在該bin內，保持原封裝路徑。

## 2026-10-06 RF-09 package／unit evidence

Source70f5e22：Dynamic0.1.32／Ptcs0.1.63／Ptcs.Client0.1.138完整compiler、pack、nuspec RepositoryCommit、exact Core0.2.54依賴與主DLL hash均PASS；Dynamic bundle7個asset與fresh output逐檔相同。既有PTCS adapter15/15、TA client17/17，共32 executed/passed、0ignored/failed/errored；消費本次本機immutable packages。Core DLL DAF4EB482A15ACB44C237D430512B4229F544A3AE308FEBC4DCE50E365CA1BFE，Mainclosure共12套件仍未publicpublish。Host建置在G:0bytes時中止；RNlocalpublish完成，SPA copy失敗，GW尚未build，正式三Host沒有切換。Renderer WIP保持。使用者自行在https://my-ai.co.in:81/chat登入；工具無可連線visiblebrowser，UI未驗。Evidence：各producer bin/net10.0/agent.aster/core54-20261006/receipt.json、Main temp/agent.aster/20261006-refactor/package-closure-proof.json（28freshassets），主要追溯RF-09 Release.Closure.md。

## 20261006 RF-12 Actors 長路徑 UI slice

Status=InProgress，Progress=20%，開始202610060631，已耗4分鐘，尚需Dev20–35/Test20–30，ETA202610060800(+08)。已在Core54真Host重現1682px內容超出1600viewport；Dynamic五行style與0.1.32→0.1.33 metadata修改完成，尚未build/pack/public/Host驗收。沿Main doc/20261006.REFACTOR/SA.md、SD.md及hypothesis RF12，實測長/短actor地址、窄/寬viewport、controls/focus/scroll；不以source/style文字測試冒充UI。Renderer WIP保持；MainHost單獨Runtime404修正unit87/87，不算本DynamicbrowserPASS。