# RFC-PTCS-DYNAMIC-0004 Actor Dynamic Action Modes

狀態：Accepted / In development

日期：2026-06-28

關聯文件：

- `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\doc\RFC-PTCS-DYNAMIC-0003.unified-sdui-form-dsl-roadmap.md`
- `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\doc\RFC-PTC-SPA-0009.actor-dynamic-action-modes-and-full-address-tree.md`
- `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\src\canvas_demo.json`

## 背景

PTCS.Dynamic 原始能力包含 direct canvas route：browser 送 JSON DSL 到 actor，actor reply 同一份 `schema=fskynet-sdui` JSON 後由 Dynamic renderer 畫 canvas。Dynamic Argu Form first slices 將 `actor-dynamic` 的 add-key UX 收斂到 DU/FormInput target，導致 direct actor-key canvas route 不明顯。

使用者確認最新產品切分：

- `Actor Argu`：固定 FormInput / Argu command；不支援 canvas；不支援 Add proxy key。
- `Actor Dynamic`：可走 direct actor key、DU/FormInput target key、live proxy key；reply 若是 canvas JSON DSL 則 render canvas，否則一般呈現。

## 目標

1. Dynamic add-key renderer 依 PTCS core shape discriminator 支援：
   - `actor-dynamic-target`
   - `actor-dynamic-proxy`
   - `actor-argu-target`
2. `actor-dynamic-target` 無 DU/template 時可退回任意字串 direct actor key；有 DU/template + canonical arg string 時走 FormInput。
3. `actor-dynamic-proxy` 建立 live proxy key，輸入 PTCS Host durable proxy actor address 與 RN actor address。
4. `actor-argu-target` 固定要求 DU/template + canonical arg string，產生 FormInput target key。
5. Canvas renderer 只在 reply 是 `fskynet-sdui` JSON DSL 時接管；非 canvas reply 留給 PTCS 一般 text/fCell rendering。

## 非目標

1. Dynamic package 不實作 PTC RN Host 或 delivery runtime。
2. Dynamic package 不把 PTCS.Host demo DU 或 demo actor 寫成 package API。
3. Dynamic renderer 不直接寫 PCSL/Journal/MessageFabric。
4. Dynamic 不讓 browser 指定 arbitrary URL、script 或 secret-bearing provider。

## 決策

### D1. Shape discriminator owns add-key mode

PTCS core 傳入 add-key renderer 的 `shape` 會是 mode-aware string。Dynamic renderer 只 claim 這些 mode：

```text
actor-dynamic-target
actor-dynamic-proxy
actor-argu-target
```

收到 plain `actor-dynamic` / `actor-argu` 時，Dynamic first slice 可視為 legacy compatibility，但新 UI 不應依賴 plain shape 判斷 mode。

### D2. Proxy key uses proxy actor as first key segment

為了沿用 PTCS existing actor-argu route，proxy key 第一段必須是 proxy actor address：

```text
[ proxyActorAddress; "proxy-v1"; rnActorAddress; targetKind ]
```

其中：

- `proxyActorAddress`：PTCS Host 中正式 durable proxy actor 的完整 `akka.tcp://.../user/...` address。
- `rnActorAddress`：RN Host actor / sharding entity / logical durable target address。
- `targetKind`：`raw`、`canvas-json`、`argu` 等 Dynamic-owned discriminator；第一波只做 route/key contract，不把 payload 放入 key。

### D3. Actor Dynamic input behavior

Selected key:

| Key shape | Input renderer |
| --- | --- |
| `[ actorAddress ]` | PTCS fallback textarea / arbitrary string input |
| `[ actorAddress; duTypeOrTemplateKey; canonicalArgString ]` | Dynamic FormInput |
| `[ proxyActorAddress; "proxy-v1"; rnActorAddress; targetKind ]` | PTCS fallback textarea / arbitrary string input; proxy actor is responsible for forwarding to RN target |

### D4. Actor Argu input behavior

`Actor Argu` only accepts:

```text
[ actorAddress; duTypeOrTemplateKey; canonicalArgString ]
```

It always renders FormInput when Dynamic is present; without Dynamic it falls back to PTCS core raw actor-argu text path. It must not canvas render.

### D5. Canvas renderer remains payload-based

Dynamic message renderer only checks payload:

```text
schema = "fskynet-sdui"
```

It must not infer canvas mode from page title or actor key. Non-canvas reply returns `None`, allowing PTCS normal renderer to show the message.

## 影響範圍

| Area | Change |
| --- | --- |
| `Client/ArguFormRenderer.fs` | Mode-aware add-key renderer; proxy key builder; Actor Argu FormInput-only guard。 |
| `Client/DynamicRenderer.fs` | Ensure non-canvas reply returns None and canvas JSON DSL renders as before。 |
| `Server/ArguForm.fs` | No schema change required for first slice; later proxy target resolver can be added as registered provider。 |
| `README.md` | Document Actor Dynamic vs Actor Argu modes。 |
| `doc/WBS.md` / tests | Add focused gates for target/proxy/direct actor modes。 |

## 驗收

1. `DYN-T-520` Add-key renderer claims `actor-dynamic-target`, `actor-dynamic-proxy`, `actor-argu-target` only.
2. `DYN-T-521` Actor Dynamic direct actor key can be added without DU/template.
3. `DYN-T-522` Actor Dynamic DU target key renders FormInput.
4. `DYN-T-523` Actor Dynamic proxy key stores `[proxyActorAddress; "proxy-v1"; rnActorAddress; targetKind]`.
5. `DYN-T-524` Actor Argu does not expose proxy key and never canvas-renders non-canvas replies.
6. `DYN-T-525` `canvas_demo.json` round-trips through direct actor key and renders canvas when actor reply is the same JSON DSL.
