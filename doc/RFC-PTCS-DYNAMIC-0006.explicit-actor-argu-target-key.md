# RFC-PTCS-DYNAMIC-0006 Explicit ActorArgu Target Key

Status: Accepted
Date: 2026-07-07
Companion: `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\doc\RFC-PTC-SPA-0015.explicit-actor-argu-target-key.md`

## Background

The beta64 `actor-argu-proxy` renderer asked only for a native actor address and relied on a PTCS Host/script hook to create a proxy and rewrite the persisted key. Users could not see which proxy was used, and manually added keys did not match seeded proxy keys.

## Decision

Dynamic must render ActorArgu `Add Target Key` with explicit fields:

```text
Proxy actor address
Target actor address
DU type or template key
Target alias
Canonical Argu string
```

Submit payload:

```text
[ proxyActorAddress; "target-v1"; targetActorAddress; duTypeOrTemplateKey; canonicalArgString ]
```

The selected key keeps the route actor in segment 0. PTCS core projects segment 2 to `ActorArguTargetCommand.TargetActorAddress`. Dynamic uses the target segment only for backend FormInput resolution and display; it does not spawn or rewrite proxy actors.

## Compatibility

Dynamic continues to resolve the historical direct target shape:

```text
[ actorAddress; duTypeOrTemplateKey; canonicalArgString ]
```

The deprecated `actor-argu-proxy` shape is not a new UI target. Existing beta64 notes are treated as invalidated workaround evidence.

## Acceptance

1. Actor Argu action pool does not show `Add proxy key`.
2. Actor Argu `Add Target Key` displays both proxy and target address fields.
3. Backend resolver accepts `[proxy; target-v1; target; template; raw]` by resolving against `target/template/raw`.
4. The FSI verifier proves a proxy actor receives `TargetActorAddress=Some(...)` and asks a native actor on a separate Akka.Remote node.
