# Issue: authoritative frame arrives during active navigator drag

## Phenomenon

Daedalus formal 18884 gate on Contracts 0.1.36 / Renderer 0.2.3 / Interactive.Client 0.2.3 uses fixed seed 20261021 after 2026 -> 2018 -> 2026 relocation and resident growth to 4,500. When an action ACK or authoritative frame arrives after pointerdown but before pointerup, the left gesture moves the right edge by 50.349px; 12 left/right/body gestures produce 16 violations including settled flashback, fixed-edge movement, and missing authoritative action.

Evidence authority: `G:\coldfar_py\coldfar-symbolics\temp\agent.daedalus\formal-overview-edge-drag\gesture-ownership.json` (Daedalus-owned; do not modify).

## Expected invariant

Pointerdown freezes semantic left/right/body ownership and its authoritative starting geometry until exactly one pointerup/cancel terminal event. Incoming ACK/frame may update backing authority but must not overwrite the active preview or change the fixed edge/body width. Pointerup derives exactly one intent from the frozen gesture basis; later matching projection settles it.

## Hypotheses

1. Renderer DOM listeners survive replacement, but `selectionX` / `selectionWidth` remain reactive Views of the incoming authoritative frame; a frame during drag rewrites the visible geometry underneath the frozen mode.
2. Interactive.Client reducer clears or rebases navigator draft on ACK/frame without knowing an active pointer gesture exists, so the replacement tree binds old authoritative geometry before pointerup.

## Experiment

Add a deterministic owner test with the exact order: pointerdown left/right/body -> preview move -> authoritative ACK/frame update -> assert preview and fixed geometry unchanged -> pointerup -> assert exactly one action. Trace which state transition changes the selection geometry, then apply the smallest owner fix and rerun Renderer Expecto plus the existing Playwright parity gate.

## Scope and stop condition

Owner Renderer/Interactive.Client/tests only. Do not modify Research host, port 18884, or Daedalus evidence. Stop if the consumer sends a contract/state sequence that violates the documented protocol rather than exercising an owner transition.

## Operations

- 2026-10-21 22:20 +08:00: hypothesis created before source inspection or modification; Daedalus asked to pause reruns.
- 2026-10-21 22:24 +08:00: root cause confirmed. Document-level listeners preserved the closure, but the selection View remapped the same draft against the newest projection. Matching action/projection settlement also had no active-gesture guard.
- 2026-10-21 22:41 +08:00: Renderer now freezes rendered ratios and semantic mode at pointerdown, updates the override from the frozen index basis, keeps draft/override through shell replacement, and settles only after the active gesture ends. Exact-package Expecto passed 68/68. Focused browser test changed authority from `900/500+400` to `1000/450+500` during four gesture paths and passed 1/1 with exact preview/wire parity.
- Release and consumer proof remain open: commit first, rebuild immutable `0.2.5`, publish through canonical fsproj hooks, then Daedalus reruns the formal 18884 gate.
