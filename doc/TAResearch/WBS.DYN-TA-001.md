# @DYN-TA-001 Contracts and reducer

Status: Done

## Deliverables

- Packable transport-neutral Contracts project。
- Strict typed codec/validation/limits and source dependency gate。
- Pure reducer, runtime effects, registry, poll/dispose state machine。
- SDK/API documentation and deterministic fixtures。

## Acceptance

`DYN-TA-T-001..004/008/009/011/015/018` pass; no PTCS/fCell2/PTMD/SQL dependency leak。

## Implemented

- `PulseTrade.Comm.Spa.Dynamic.Contracts 0.1.0-alpha4` packable package；browser-facing numeric使用`float`，query range使用canonical ISO-8601 string，由host/server重新驗證並轉domain time。
- strict runtime/frame/action/TA row vocabulary、FSharp.SystemTextJson codec、protocol/payload/limit/script/URL/selector validation。
- pure last-good reducer、typed resync/action effects、multi-canvas registry、one-in-flight poll lifecycle與terminal dispose。
- SDK README、exact-package tests 7/7、assembly/source forbidden dependency gate。
