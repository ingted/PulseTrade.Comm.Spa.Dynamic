# Browser cache semantic validation hypothesis

## Repository state

| Repo | Branch | Baseline | State before experiment |
| --- | --- | --- | --- |
| PulseTrade.Comm.Spa.Dynamic | `20260715_030.win.TACanvas_cross.bar.cursor.first.done` | `b8bf4058d9ffd258c84f63c93e4f10d8161ec86f` | clean |

## Hypothesis

- Symptom: `BrowserRuntimeCache.readMatching` treats every entry accepted by `BrowserRuntimeCodec.decodeCacheEntry` as a cache hit.
- Suspected root cause: the browser decoder validates only required identity/coverage/revision fields; it does not execute the full document/snapshot/reducer validation implemented by `RuntimeCache.validateEntry`.
- Affected surface: `BrowserRuntimeCache.readMatching`; corrupt-entry browser fixture and Playwright verifier.
- Evidence: malformed JSON is deleted by the current gate, while a JSON-valid entry with a semantically invalid document can pass the shallow decoder and be selected.
- Experiment: call `RuntimeCache.validateEntry DynamicRuntimeDefaults.limits` before selecting a record; collect failed keys for deletion and return `Miss` when no valid candidate remains.

## Estimated change

- Product logic: about 10 non-type-declaration lines.
- Browser fixture/verifier: about 25 lines for a semantic-invalid seed/read/removal assertion.

## Validation

1. Build Contracts and Interactive.Client.
2. Run Contracts, Renderer, and lifecycle regression suites.
3. Run the F# Playwright browser-cache verifier and assert valid cache behavior plus malformed/semantic-invalid deletion.
