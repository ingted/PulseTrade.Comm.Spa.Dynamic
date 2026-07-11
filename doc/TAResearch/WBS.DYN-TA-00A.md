# @DYN-TA-00A Legacy SDUI readiness closure

Status: Done

## Scope

- Close direct static DSL target browser gate from `DYN-WBS-506/512`。
- Replace `ActorsPage` token `IndexOf` classification with strict typed/schema decode or explicit discriminator。
- Add invalid-node/schema regression that preserves FormInput/last-good Canvas。
- Confirm common Form DSL and Canvas DSL share typed primitives without merging host-specific transport。

## Completed

- [x] `SduiPayloadClassifier`以JSON discriminator精確要求`schema=fskynet-sdui`、`surface=ActorsPage`、`documentType=ActorTopologyPage`；token-only/wrong schema/wrong surface/malformed payload均fail closed。
- [x] WebSharper client使用typed discriminator DTO，不再以`IndexOf("ActorTopologyPage")` claim頁面。
- [x] backend Argu parse failure顯示錯誤並保留template-default FormInput；explicit `target-v1` key不再被誤當legacy union-case tail而清空表單。
- [x] Dynamic showcase/sdui-echo actors支援native `fCell2<string> -> fCell2<string>`，direct Add actor key可回Canvas DSL；explicit Add target key仍走proxy DTO路徑。
- [x] Root live verifier同步目前五段key wire shape：`[proxy; target-v1; nativeTarget; template; raw]`。
- [x] Dynamic Expecto 21/21與正常WebSharper Release build通過。
- [x] `verify-ptcs-host-dynamic-argu-live.fsx`真host Playwright通過：FormInput、invalid fallback、spec-bound/resolved proxy、direct actor Canvas與send/reply。
- [x] `verify.actorsPageDynamic.playwright.fsx`真host Playwright通過：4 blocks、26 rows、Dynamic owns page、fallback rows 0、report/toggle/full-address gate。

## Acceptance

`DYN-TA-T-000A`通過。新TA runtime的unknown op/node/limit與extension adapter完整矩陣仍由`T-002/016/017`及後續WBS關閉。

## Evidence

- Dynamic package tests：21 passed，2026-07-11。
- Live Dynamic/Argu gate：`G:\PulseTrade.fs\log\20260711\verify-ptcs-host-dynamic-argu-ta-readiness5.service.log`。
- ActorsPage screenshot：`G:\PulseTrade.fs\Libs\PulseTrade.Comm\.pcsl\verify.actorsPageDynamicTaReadiness\run-438de670e39e4e848d5b0cd3e814c2e5\actors-page-dynamic.png`。
- Public OAuth、production RN service與cross-service registry仍留原WBS，不在本readiness slice假性宣稱完成。
