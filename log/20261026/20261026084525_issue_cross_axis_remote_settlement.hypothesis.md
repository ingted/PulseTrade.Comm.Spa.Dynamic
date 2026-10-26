# Hypothesis — cross-axis remote settlement mixes navigator domains

- Issue：0.2.41在30K/60K local preview正確，remote acceptance/settlement後卻送出1K GLOBAL-era ordinal並跳回2018。
- Repo/branch/HEAD：`C:/Users/Administrator/test_gemini/PulseTrade.Comm.Spa.Dynamic` / `20260915_033.ptcs_group_support` / `7e8f8ae19a8b831ed69dc990814bf3ec665474de`。
- Dirty baseline：`lib-packs.txt`、`tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.Tests/Program.fs`、`.pcsl/*`、`temp/`為既有其他owner/WIP，本輪不碰。
- 已證實事實：GLOBAL axis=1K resident `2,952,345/44,649`；active/base axis=30K、1491 bars；initial retained authority=None；錯誤remote action為start `2,995,494`/count1500。
- 不變量：cross-axis navigator不得以active local index加GLOBAL resident start；只有main-plot CURSOR deliberate relocation可送GLOBAL-domain `ResidentCoverageChanged`。Navigator action若無exact active/base-axis mapping，必須fail closed而不能合成ordinal。
- 候選根因：`tryCrossAxisResidentNavigatorAuthority`用`targetStart - GlobalOverview.ResidentStart`計offset，卻用active `OverviewAnchors.Length`驗界；這把1K normalized target與30K domain混在同一authority。settlement後該authority再被render/dispatch共用，將錯誤持久化。
- 最小區分證據：以exact projection先證authority builder是否接受tag8/global target；再以同一accepted settlement重跑gesture，驗coverage intent是否由None變成錯誤Some。若builder拒絕後仍錯，沿queued rebase/coverage settlement查下一個最早mapping。
- Canonical mechanism：same-axis可用absolute ordinal；cross-axis需Research exact time/base-axis mapping或保留active-local `VisibleRangeChanged`而不帶GLOBAL coverage intent。不得用比例或同值start猜測。
- 預估修正：model/tests約20–40非型別行，renderer wiring應為0–20行；超過80行或需wire改動時先補Experiment說明。

## Experiment

- RED：DYN-T-710反轉舊DYN-WBS-597 oracle，要求accepted GLOBAL target與手工legacy retained value都不能啟用cross-axis resident domain；修前98 cases為96 PASS/2 FAIL，分別證builder產生Some與legacy retained使`UsesResidentAuthority=true`。
- 定位：`tryCrossAxisResidentNavigatorAuthority`是唯一runtime builder，Renderer在projection settlement兩處寫回；`resolveNavigatorGestureDomain`、render ratios、queued rebase及dispatch再共同消費。錯誤不是pointer delta，而是把GLOBAL normalized ordinal相減後當active slot。
- 修正：保留公開相容surface，但cross-axis builder固定None、legacy matcher固定false、with-authority resolver只走same-axis canonical resolver；render若same-axis不可解析就直接使用active timeline ratios。Cross-axis release因此只會走既有`setWindow` event-time action。
- GREEN：同一source harness build 0 errors，Expecto98/98；修正約32非型別行，未超過預估2倍，不需wire/schema/Research改動。
