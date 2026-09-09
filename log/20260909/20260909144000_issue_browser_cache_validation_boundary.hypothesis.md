# Browser cache validation boundary hypothesis

## Repository state

| Repo | Branch | Baseline | State before experiment |
| --- | --- | --- | --- |
| PulseTrade.Comm.Spa.Dynamic | `20260715_030.win.TACanvas_cross.bar.cursor.first.done` | `bd5382f95eec936e02ef4ddbfe863262c6c4eb37` | clean |

## Hypothesis

- Symptom: browser cache read needs the same semantic entry validation as server-side `RuntimeCache.validateEntry`, but the complete `RuntimeCache` module is not browser-safe because it also derives coverage through `TemporalAxisCodec`.
- Root-cause refinement: validation and server-side cache construction currently share one module even though only validation is a browser contract.
- Affected surface: `RuntimeCache.fs`, `BrowserRuntimeCache.readMatching`, browser cache fixture/verifier.
- Experiment: extract the existing entry validation body into a narrow `[<WebSharper.JavaScript>] RuntimeCacheEntryValidation.validate` module. Keep `RuntimeCache.validateEntry` as an alias for server compatibility, and call the narrow validator from browser reads.
- Expected behavior: valid entries remain hits; malformed JSON and semantic-invalid entries are deleted and return `Miss`; coverage derivation stays server-only.

## Estimated change

- Product logic: about 80 moved/shared lines plus 8 browser-read lines; no wire/type change.
- Browser fixture/verifier: about 30 lines.

## Validation

1. Contracts and Interactive.Client Release builds, proving the narrow validation graph is WebSharper-safe.
2. Contracts 21/21, Renderer 26/26, Interactive lifecycle 4/4.
3. F# Playwright browser cache gate covering valid persistence/LRU/coverage plus malformed and semantic-invalid deletion.
