# DYN-TA-007 Static compatibility and DSL sync

Status: Active
Progress: 40%
Updated: 2026-07-12

## Completed

- Added canonical server-side `SduiPayloadKind` classification for unrelated/absent payload、legacy/explicit static Canvas、FormInput、ActorsPage、runtime v1 and present-invalid SDUI reason codes。
- Existing ActorsPage strict discriminator delegates the same classifier；malformed JSON and unrelated schema remain unclaimed。
- Dynamic package regression passes 23/23，including static Canvas/FormInput compatibility and absent-versus-present-invalid semantics。
- Package advances to `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta74` with exact PTCS `[0.2.5-beta80]` unchanged。

## Remaining

- Locator-only F# Playwright real-host gate for extension absent fallback、valid static Canvas/FormInput and present-invalid controlled UI。
- Confirm invalid static payload preserves the append/FormInput surface and does not silently switch to a raw textarea。
- Sync browser dispatch to the canonical classifier without introducing additional raw JavaScript or inline script。

## Evidence

- Source：`src/Server/SduiPayload.fs`。
- Tests：`tests/Program.fs`，23/23 pass。
- Operation log：`log/20260712/20260712045600.dyn-ta-static-compat.op_log`。
