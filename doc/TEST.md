# Test Plan (TEST) - PulseTrade.Comm.Spa.Dynamic

## 2026-10-27 DYN-T-724（0.2.58 owner source / package pending）

- 契約：inner navigator origin在accepted frame使axis relation改變後仍是`NavigatorWindow`。Same-axis event-range intent保持`ExplicitBounds`、StartOrdinal/Direction none、current coverage revision、next query generation與frozen target count；main plot及resident ordinal path不變。
- RED：0.2.57 package-backed Renderer test呼叫canonical unconditional navigator helper得到FS0039；真18884先證resize/middle move purpose正確，再證長body drag local1201/count79→target0只送bare ui:4，settle為local1122而非0。
- GREEN gate：Renderer source使用unconditional navigator helper，保留舊cross-axis compatibility test；fresh Renderer/Interactive tests、package metadata/hash、verifier與package-only BrowserDemo需全綠。真E2E必須同一序列settle到local0，且A/C/highlight/D無回歸。

## 2026-10-27 DYN-T-723（0.2.57 owner package / consumer pending）

- 契約：每個`TaCoverageWindowIntent`都帶required purpose。main plot in-range／boundary／provider-open皆為MainPlotPan；cross-axis navigator move／resize及resident action為NavigatorWindow。有StartOrdinal／Direction的overscroll仍以既有materialization authority處理，purpose不取代range authority。
- RED/GREEN：0.2.56 surface對`.Purpose`／`TaCoverageWindowPurpose`得到8 compiler errors（4 unique）；0.2.57新增DU/field、window schema v2、typed JSON round-trip與resident invalid-purpose rejection。Final source `da94f34286e6947c0ebf61b24f0f0a3308939bb6`；Contracts／Renderer／Interactive／Tagging SHA=`C6A17D37...FD32`／`59470945...25B2`／`BE851444...C6B1`／`DB368F98...A6D8`，RepositoryCommit/ProductVersion exact、SDK parity。fresh Contracts51/51、Renderer113/113、Interactive20/20、Tagging9/9；Interactive package verifierPASS，package-only BrowserDemo exact Contracts0.1.41／Renderer0.2.57／Client0.2.57為0 warnings／0 errors。
- 真E2E：Daedilus consumer total match Purpose；NavigatorWindow resize1500→79後再move79不得搬resident，MainPlotPan仍要求count等於visible並relocate together；不得保留count fallback。Gate C accepted但Frames=[]為host owner獨立缺口。

## 2026-10-27 DYN-T-722（0.2.56 owner package / consumer pending）

- 契約：main CURSOR pan超出captured timeline頭／尾時要導向typed adjacent coverage，保留visible count與signed delta。cross-axis resident的target以ActiveDetail start + delta，不可把local resident offset重複加入；same-axis與in-range exact event-time path不變。
- RED/GREEN：0.2.55真Gate A的`local3150/count1215 @ timeline4500 + delta340`無action；pure 0.2.55得target53580，期望50430而DYN-T-722 0/1 RED。model改以cross-axis ActiveDetail50090為基準，live `commitMainPlotPan` 只在pure boundary classifier得Earlier/Later時呼叫既有`requestAdjacentCoverage`。Final source `02fb0f5526adf20e05c4e4664624c266b3ac0995`；Renderer／Interactive SHA=`D9736149...EC4`／`16946F6A...278A`、RepositoryCommit/ProductVersion exact、Client Renderer `[0.2.56]`、SDK parity；focused1/1、fresh Renderer113/113、Interactive20/20、verifier、package-only BrowserDemo 0 warnings／0 errors。
- 真E2E：Gate A ui action應為Later、StartOrdinal50430、count1215、query+1；server accepted後active start50430且main local reanchor1645，無額外重複delta。

## 2026-10-27 DYN-T-721（0.2.55 owner package / consumer pending）

- 契約：cross-axis global/active domain需要server materialize exact UTC range，與range是否由overview anchors substitution無關。one-to-one overview不可用時使用exact active prepared timeline，但仍附`ExplicitBounds`、StartOrdinal/Direction none；same-axis不附。
- RED/GREEN：以0.2.54 helper與真host形狀（Global axis1K、ActiveDetail axis30K、overview substitution不可用）執行112 tests得到111 passed/1 failed，intent為None；0.2.55拆開timeline selector與materialization predicate。final source `8515d42f7f7bbca03a47cb614740c89c65f2038c`、Renderer/Interactive SHA=`B657A064...3C64`／`A8109ADB...5A76`，RepositoryCommit/ProductVersion exact、Client Renderer `[0.2.55]`、SDK parity。fresh Renderer112/112、Interactive20/20、verifier、BrowserDemo 0 warnings/0 errors。
- 真E2E：Gate B wire必須帶no-ordinal `CoverageIntent ExplicitBounds`，left-edge settle local0且後續overscroll可再取更早資料。

## 2026-10-27 DYN-T-720（0.2.55 owner package / consumer pending）

- 契約：action completion settlement只能使用同一`chartCommittedFrame`的runtime state與prepared timeline；禁止新query projection配舊prepared timeline。accepted preparation則用同次`next + nextPreparedData`。
- RED/GREEN：以exact0.2.54 source/package執行111 tests得到110 passed/1 failed，callback仍含`runtimeState.Value + latestPreparedData`；0.2.55改用coherent committed pair。final source/package metadata與hash同DYN-T-721；fresh Renderer112/112、Interactive20/20、verifier、BrowserDemo全綠，Contracts authority SHA=`D55F53CD...9642`。
- 真E2E：Gate A server relocation後main local維持1645；formal accepted event-time commit在10s gate內可見，不能只在timeout後settle。

## 2026-10-27 DYN-T-719（0.2.54 owner package / consumer RED）

- 契約：matching pending event-range在`runtimeState.View`的dataChanged＋coverage-scope full-preparation邊界不得先被清除；pending/draft須存活至prepared frame的Reanchor／Await／Reject。identity、canvas或base-row mismatch仍clear，其他scope reset不變。
- RED/GREEN：以0.2.53 exact source驗runtime scope-policy usage只有1處，預期sink＋accept共2處而RED；0.2.54 factor matching-document predicate並讓兩階段共用既有ownership policy。final source commit `a22390d530de93b0e5d687d7b940c5a7bf602a9e`已push；fresh Renderer110/110、Interactive20/20、Interactive package verifier PASS、package-only BrowserDemo 0 warnings/0 errors。Renderer/Interactive SHA=`7CFA5CBA...657B`／`8AC38D22...FEF5`，RepositoryCommit/ProductVersion exact、Client Renderer `[0.2.54]`、SDK10.0.402 parity，Contracts authority SHA=`D55F53CD...9642`。
- 真E2E：Daedilus exact0.2.54反證A仍`1645->1985`、B wire無CoverageIntent/local1122；C rapid regression/mounted已GREEN，但formal accepted commit visibility超10s，D待續。轉DYN-T-720/721；任一RED都不宣告全修完。

## 2026-10-27 DYN-T-718（0.2.53 owner package / consumer RED）

- 真反例：0.2.51 package/host ProductVersion exact、新browser context，action與server materialization全GREEN，DOM仍`1645 -> 1985`。`viewportScopeKey`包含CoverageIdentity；accepted ui:5 更新identity時live branch先清pending，再以document default覆寫viewport，故DYN-T-716 pure exact UTC helper未成為live authority。
- policy gate：changed coverage scope＋matching base row時，Reanchor/Reject consumed必須壓過default並完成新scope；Await須保留pending/viewport；base-row mismatch仍不得保留舊action；scope未變不需抑制default。source gate要求`acceptPreparedDataCore`實際使用同一pure policy。
- RED/GREEN：0.2.52 exact package缺`eventRangeSettlementOwnsViewportScope`而4個FS0039 RED；final 0.2.53 source `59c535ecfdc05751c181c82445bda096d7125c0a`已push，owner Renderer109/109、Interactive20/20、package verifier與BrowserDemo全綠。但Daedilus exact package/new context實測A main `1645->1985`、B left-edge local1122、C rapid-frame60個regression frames，證明accept-stage修正太晚；轉DYN-T-719／DYN-WBS-605。

## 2026-10-27 DYN-T-717（0.2.52 source complete / 0.2.53 successor consumer pending）

- 反例：cross-axis resident overview local0與action `StartEventTimeUtc`已同為`2022-03-11T10:00:00Z`，但0.2.50 action `CoverageIntent=None`，consumer只能把ActiveDetail `49646 -> 49567`（-79），settled local start仍1122。最早偏離不是pointer mapping，而是缺少exact-time materialization contract。
- contract gate：只有`GlobalOverview.AxisRef <> ActiveDetail.BaseAxisRef`、overview axis等於active/base、validated overview timeline與active timeline exact同長且非空時，建立`ExplicitBounds` intent；Start ordinal／Direction皆None，revision為當前、generation只+1、count保留draft width。same-axis與長度不符都必須回None，不做GLOBAL ordinal或比例推測。
- RED/GREEN：0.2.51 exact package因helper不存在得到3個FS0039 RED；0.2.52 source commit `ec2470fdd5be67dab0079251e1aca4dfa57ce817`已push，local final Renderer/Interactive SHA=`BEA57060...EE1A`／`94DFD9B2...B632`及owner gates全綠，但因main scope真RED而未handoff。功能保留於0.2.53 successor；Daedilus須驗action後ActiveDetail absolute relocation、settled localStart=0、selection ratio與續left overscroll；2018 query DOCUMENT-CHANGED為consumer獨立診斷。

## 2026-10-27 DYN-T-716（0.2.51 owner package / consumer pending）

- 反例：舊active timeline中requested event start位於local1985；consumer將ActiveDetail按main pan `+340` relocation後，同一exact event time在新timeline位於local1645。舊Renderer保留optimistic local1985，等於double-apply；新pure settlement以normalized UTC exact match回`1645/1215`，不使用GLOBAL ordinal或比例推測。
- lifecycle gate：projection generation較pending舊時Await；matching generation但target尚未出現且action pending時Await；terminal仍缺exact start或完整count時Reject。Renderer只為`ExplicitBounds + Start=None + Direction=None`保存context，identity／scope／reject清除；Resize、navigator、CURSOR不變。
- RED/GREEN：0.2.50 package缺`mainPlotEventRangeProjectionSettlement`而4個FS0039 RED；final 0.2.51 source `7758cc0d243cca85b134053f2936ac0800089979`已push，Renderer/Interactive SHA=`20F3C9C5...6D4A`／`6FD75678...5E7E`、RepositoryCommit/ProductVersion exact、Client exact Renderer `[0.2.51]`。fresh DYN-T-716與Renderer107/107、Interactive20/20、package verifierPASS，package-only BrowserDemo exact0.2.51 build 0 warnings/0 errors。Daedilus真18884須證server relocation後local visible start維持1645、pointer canonical event-time不漂移，且coverage/query generation仍各只前進一次。

## 2026-10-27 DYN-T-715（0.2.50 owner package / consumer pending）

- 反例：cross-axis projection同時持有active prepared timeline與resident overview timeline；兩者長度皆4但local0 UTC不同。0.2.49 package缺`navigatorEventTimeline`而3個FS0039 RED；修後cross-axis選validated overview原始UTC timestamps，same-axis及overview長度不等均保持active fallback，禁止比例插值或GLOBAL ordinal換算。
- source gate：navigator local drop必以pointerdown frozen projection走`setNavigatorWindowFromPrepared`／`navigatorVisibleEventRangePrepared`；其他`setWindow` caller仍走active prepared range。DYN-T-689同步改驗default resolver仍存在，未刪browser-local activation或main CURSOR contract。
- owner final：source commit `9f57a7e015ec42e047d7dbf9f8dfee1f85c95c08`已push；Renderer/Interactive.Client 0.2.50 SHA=`9E9D3186...15F2`／`497EE6B7...6ACA`，兩包RepositoryCommit/ProductVersion exact，Client exact Renderer `[0.2.50]`。以SDK10.0.402 library-packs權威Contracts `0.1.40`（SHA=`D55F53CD...642`）fresh restore後Renderer106/106、Interactive20/20、package verifierPASS；package-only BrowserDemo exact0.2.50 build 0 warnings/0 errors。Daedilus真18884須重現Loaded4500/79-bars body left：Action start等於resident overview local0 timestamp、settlement localStart=0、selection不跳回中段且下一次left-edge overscroll可繼續。

## 2026-10-27 DYN-T-714（0.2.49 owner candidate / consumer pending）

- DYN-T-714 main-plot range preview：pure helper以1000px track固定右拖`300->720`、左拖`720->300`皆正規化為`start=300,width=420`；越界`-40->1040`夾為`0/1000`，stationary為`500/0`。source contract要求非CURSOR/Resize pointermove使用同一plot-local座標即時更新`ta-main-plot-range-preview`，發布visible/start/end/width，pointerup/cancel/mode切換清除；既有CURSOR與Resize commit分支不變。
- RED/GREEN：0.2.48 package-backed test因`mainPlotRangePreviewBounds`不存在而4個FS0039 RED；final 0.2.49 source `f753ac9dec8753a633d94566f6e065537f12b003`後Renderer105/105、Interactive.Client20/20、Interactive package verifier PASS。Renderer/Interactive SHA=`DA1C63F3...66BBA`／`D98C2AD3...40252`；package-only BrowserDemo以同步FSharp.Core10.1.402／WebSharper.FSharp10.1.7.680／WebSharper.UI10.1.6.680 build為0 warnings/0 errors。
- UI gate：owner端in-app Browser控制層在建立kernel assets前失敗；Daedilus真18884已完成補驗（message=`msg-fsi-9ff01c16331342829a4b62c8eaf0db33`）：left during `start=329/end=460.59998/width=131.59998`、right during `start=855.39996/end=987/width=131.60004`，兩者pointerup均visible=false/width=0，正式range resize commit亦通過。此gate GREEN；同一scenario後段main settlement RED由DYN-T-716獨立追蹤。

## 2026-10-27 DYN-T-711 / 712 / 713（0.2.48 owner package / consumer pending）

- DYN-T-711 trace-target precedence：以同x candle Y區間涵蓋click、SMA離散vertex略偏的反例，驗browser event target `fsstl.sma-30k-13`必先限縮候選並回LINE/POINT；空白或unknown DOM trace id仍走既有全體geometry fallback。source contract要求candle/line/histogram/volume SVG path都帶`data-ptcs-ta-trace-id`，不得另造JavaScript activation path。
- DYN-T-712 prompt queued dispatch：以可控callbacks驗action settlement順序固定為`settled -> flush-now -> schedule-fallback`；同一immediate+RAF helper亦須接到`Poll/PendingActionId` readiness View。同步drain仍受pending action、pending boundary、committed coverage、poll與generation/revision gates約束；RAF只處理當下projection尚未ready，不得用低頻poll或paint queue作主要dispatch authority。
- DYN-T-713 logical handle anchor：以4500/478px與right-edge selection `[3000,4500)`固定8px handle被clamp後中心偏logical edge 4px的反例；ResizeRight必須以logical right boundary錨定後精確得到80 bars，ResizeLeft鏡像同驗，Move仍保留raw grab point。
- Owner gate：修前0.2.47 package對DYN-T-713為FS0039 RED；0.2.48 source `3649f1ee364fbb88af0567413fc3fd4373575881`後重建RepositoryCommit exact packages，Renderer104/104、Interactive.Client20/20、package verifier PASS；fresh package-only BrowserDemo exact0.2.48/0.2.48為1 warning/0 errors、bundle7檔。Daedilus真18884仍須驗：30K/60K SMA click回exact SMA target；formal 4500 body drop於5秒內送唯一action；4500 right-handle進入約80 bars。main TA grab-pan coverage revision前進仍是獨立open gate；再跑完整22-scenario。

## 2026-10-26 DYN-WBS-599（0.2.43 / 0.1.40 / 0.1.8 source candidate）

- P0-1 Tagging：`displayLabel`只讀`ContextLabel`並fallback `TargetId`，target key／dispatch identity不變；dirty draft排在persisted rows前，未儲存draft save為Create無DB id、delete為local discard，persisted delete才帶`ExInfoId/Revision`。actions在fields前、dirty prompt在card頂。另以0.1.9 canonical source-compat回歸覆蓋TaggingSearch types/binding、SuggestTags/Suggested、Binding.Changed/InitialDraft；`PtcsTaggingEditor.Tests` 9/9 PASS，0.1.9→candidate public type/member name comparisonmissing=0。
- P0-2 retention：`MaxRetainedBarsPerSeries=4500`；DYN-TA-T-066驗4500 candidate accepted、4501 candidate rejected，Contracts full 51/51 PASS。visible cap仍由consumer 1500 authority持有，generic legacy snapshot不誤套retained-series cap。
- P0-3 exact pan：30K formal-base fixture以20-slot visible、pointer `downSlot=10/currentSlot=6`得active delta `+4`，target window `5/20 -> 9/20`，證`oldStart+downSlot = newStart+currentSlot`且target越captured range fail closed。main CURSOR action鎖`CoverageIntent=Some`、captured revision、generation+1、no ordinal、count不變、Direction=None、ExplicitBounds；Resize/local navigator仍為None。Renderer source gate要求main commit經`setWindowFromPrepared`／`visibleEventRangePrepared`送single `VisibleRangeChanged`，不得含`ResidentCoverageChanged`或sampled GLOBAL lookup；DYN-T-703 1/1、704 1/1、707 1/1、689 2/2，Renderer full 99/99 PASS；Interactive full 20/20 PASS。
- Build／package gate：.NET SDK 10.0.402；`-532462766`的最早共同原因為父程序已不存在的`wsfscservice.exe`仍鎖住project `websharper.log`，非ACL本體。只停止已驗parent不存在的exact owner後，Tagging.Client 0.1.11 canonical WebSharper Release Rebuild 0/0 PASS。0.1.10因Research exact-package Release compile證實它對official 0.1.9造成23個API regression errors，已withdraw。Final 0.1.11 source commit=`e371194dd0b2bd0de0f23f4cf025e5dec7dc862d`，nupkg SHA=`85B4A6298E6EA2CC8EDF507F33BCE51D11F67E5706E8BE380D077AAAC2D152AA`，RepositoryCommit exact、dependencies=`FSharp.Core[10.1.402];Contracts[0.1.40];WebSharper.FSharp 10.1.7.680`，task-local feed與SDK10.0.402 library-packs parity PASS。source與fresh exact-package Expecto均9/9；0.1.9→0.1.11 public type 31→34 missing0、public member 456→480 missing0。Contracts0.1.40、Renderer/Interactive0.2.43及其owner/package證據不變；Research Release/22-case consumer pending。

## 2026-10-26 DYN-T-707 / 708 / 709 / 710（0.2.42 candidate；0.2.40/0.2.41 withdrawn）

- DYN-T-707：pure model固定右鍵mode toggle、CURSOR不論pointer half皆為Move、Resize左右half各選對boundary；離散anchor delta保留同一physical pointer anchor，Resize delta隨實際pointer距離。另以resident4500/window1000-2000驗左boundary向右只得1100-2000、右boundary向右只得1000-2100。source wiring要求persistent mode、contextmenu、dynamic mode attribute，且CURSOR release必走`tryCommitMainPlotResidentPan`、Resize release只走`setWindow`。
- DYN-T-708：重現only-30K/60K formal projection：GLOBAL axis1K與active axis30K同start `2,952,345`，resident count44,649、rendered local count1,491。沒有retained authority時gesture domain必為local1491，left shrink200只成200/1291且不得產生GLOBAL intent；matching typed authority存在時才可明確映至GLOBAL `2,952,545`。queued gesture若authority在settle前消失必fail closed。
- DYN-T-709：bounded source regression檢查`publishRuntimeState`只以`authoritativeRendererReady next` mount early formal frame，且不得依賴`WorkspaceMaterializationPhase.Completed`、generic Completed或rows非空；既有DYN-T-698/702仍驗bootstrap loader ordering與non-zero authoritative geometry後才retire。
- Owner gate：source-project Renderer 98-case process與Interactive.Client 20-case process；版本後另跑fresh exact-package兩套、package metadata/hash與BrowserDemo full WebSharper。focused真mouse要求CURSOR exact-one `ResidentCoverageChanged`、resident固定寬 relocation、inner offset/count不變與anchor不漂；Resize左右各exact-one `VisibleRangeChanged`、coverage intent空且resident不動。Daedalus擁有18884真資料gate：formal GLOBAL須在RUN期間原位接手，navigator三個human gesture不改GLOBAL resident，main plot CURSOR與Resize依上述action authority分流。
- Withdrawn 0.2.40 evidence：source `52b4266`、Renderer/Client SHA `B4CF8A79...AF9A`／`FE30CBA3...AA50`雖通過98/98、20/20、package與舊oracle browser gates，但CURSOR誤送local `VisibleRangeChanged`，不符合Daedalus後續確認的resident relocation authority；package bytes保留且不得覆寫。0.2.41結果待本節後續追加。
- Owner 0.2.41 result：source/package commit `61b3aea377e16433ecc99a0f48541bda13dcd7e4`；Renderer/Client SHA `C4A2410F...45FDC`／`95303D89...FC4C2`，RepositoryCommit與Client exact Renderer`[0.2.41]`一致，SDK10.0.402 feed/canonical bin hash parity。Fresh exact-package Renderer98/98、Client20/20、package verifier與BrowserDemo full WebSharper 0 errors。focused真mouse 1/1：CURSOR exact-one resident `3000->2860`、delta-140、fixed count/inner offset/count與same-X anchor不變；Resize left+70/right-63各exact-one local action、coverage intent空、resident維持2860，crosshair hide/restore、console0；人工檢視主圖/navigator/雙Y無空白、重疊或錯位。前三次RED為verifier oracle（relative/absolute start、fixture resident count1500、右界向外正確clamp），產品source/package未重建或覆寫。
- DYN-T-710 remote-settlement RED：以GLOBAL 1K resident `2,952,345/44,649`、active/base 30K 1491 bars及captured tag8 `2,995,494/count1500`固定反例。任何accepted GLOBAL target（即使差值+200剛好落在1491內）與legacy retained value都不得讓cross-axis `UsesResidentAuthority=true`；settled geometry仍是`200/1291`對1491、coverage intent必為None。修前Renderer96/98，修後source98/98；0.2.42 fresh package與Daedalus remote-settlement真consumer gate pending。
- Owner 0.2.42 result：source/package commit `37e14c7c18e9a3d222c20dc7d14462de465905a8`；Renderer/Client SHA `F1C92CCD...B510F`／`99668572...4BA73`，RepositoryCommit與Client exact Renderer`[0.2.42]`一致，SDK10.0.402 feed/canonical bin hash parity、未public push。Fresh package Renderer98/98、Client20/20、package verifier PASS、BrowserDemo full WebSharper 0 errors；main-plot真mouse1/1與visual GREEN。既有navigator delayed-previous-response verifier在0.2.41/0.2.42都同樣停於`GlobalOverview=None` fixture coverage-start空，判為非本patch baseline warning，不列本slice PASS；Daedalus remote-settlement真驗收仍是release gate。
- Closeout governance：本輪五個owned文件敏感掃描High0/Medium0；root checker為PASS42/WARN3/FAIL1，唯一FAIL是已在舊commit `52b4266` 中存在的 `log/20261026/20261026070734.no1k-navigator-cursor-mode.log` 相對07:22 checker state發生非append修改。該檔目前Git-clean且不在0.2.42 closeout diff；依append-only政策不改寫歷史，故0.2.42 package證據保留，但owner closeout仍待明確例外，不宣稱全gate PASS。

## 2026-10-25 DYN-T-705 / 706（0.2.39 final owner gate）

- DYN-T-705：以真consumer同構projection固定GLOBAL total2,996,994／resident start2,989,574／count7,420、OverviewAnchors1484、active axis5K start600,240／count862。terminal grow accepted target2,989,574後，navigator必保持domain1484/window0-862與selection `0..862/1484`；body-right148後必映為GLOBAL target2,989,722/window148-1010，active-axis601,102必被拒絕。另以source wiring guard要求range、selection、pointer與dispatch使用同一retained authority。
- DYN-T-706：候選與final皆須完整WebSharper build；fresh exact-package Renderer96/96、Interactive.Client19/19與package manifest verifier；exact-package BrowserDemo完整WebSharper 0 errors。最後由Daedalus以0.2.38 deterministic human-paced sequence（grow terminal後停1秒、再material body-right）驗final `Loaded1484`、非全寬selection、GLOBAL target/generation、pending清除、moveSettled與browser errors；owner package gate不冒充consumer acceptance。
- DYN-T-706 result：0.2.39 owner final Renderer96/96、Client19/19、package verifier與BrowserDemo full WebSharper皆PASS。Daedalus真consumer human-paced sequence保持Loaded1484／Viewing149-1010並送GLOBAL target2989722/count862；Research mapping修後active detail 599618→599647、rev1→2。Final UI再驗雙Y cursor同值／leave hidden、main grab delta+421與same-X anchor、GLOBAL close line非黑、同時移除1K/5K row後30K continuityGap0／exactly1 relocation，全部bad/browser0。Consumer `ef6bcee069107f87a9afddd50146381721746408`已push，focused7/7、Release 0 errors、formal checker PASS76/WARN4/FAIL0。

## 2026-10-25 DYN-T-703 / 704（final 0.2.36）

- DYN-T-703：pure model 驗 main plot resident relocation保持4500 resident width、query generation只前進一次；以 fractional CSS positions 造成舊continuous round與slot floor差一根的反例，要求`downSlot-currentSlot=421`並證`oldAbsoluteStart+downSlot = newAbsoluteStart+currentSlot`。
- DYN-T-704：`scripts/verify-ta-main-plot-grab-pan-playwright.fsx`在GLOBAL 10,000／resident 1,500／visible 700 fixture執行真mouse down/move/up；驗exact-one `ResidentCoverageChanged`、delta=140、resident `3000->3140`、resident-relative visible=801不變、absolute visible `3801->3941`、same physical pointer anchor不漂。另驗所有row初始horizontal cursor hidden、hover僅單row visible、Y跟pointer、leave hidden、console error=0與實際截圖。
- 0.2.34 development candidate 的水平線testid誤以`-crosshair`結尾，完整renderer gate命中14而非既有7條shared crosshair；0.2.35修正後通過owner gates，但兩版prefinal皆已物化至SDK feed，故一律withdrawn且不得同版覆寫。正式immutable 0.2.36由source `c38f84628ea3fc0d1291f4803fb7aedafc860bdf`一次性repack：Renderer SHA `688C562C...12E25`、Interactive.Client SHA `3B164E38...E8030`，RepositoryCommit／DLL ProductVersion與Client exact Renderer `[0.2.36]`全一致；fresh package-backed Renderer 95/95、Client 19/19、package verifier及focused真mouse 1/1 PASS，final截圖人工檢查無重疊／錯位。Daedilus真DMI 5K以real mouse重驗`+421`、resident `2994894→2995315`、count1500/local0不變、absolute同`+421`、same-X anchor不漂；horizontal Y跟隨/leave/row isolation、no-1K GLOBAL gap0/count1/total2,996,994/mapped relocation、tagging 9-case/雙Y軸、Release 0 errors、health/cache全PASS。Consumer五refs commit/push=`G:\coldfar_py\coldfar-symbolics@9f7df5dd4af4bdd03904e3e4992f935542f93bef`。完整renderer gate另有既有矛盾：source自2026-10-21要求navigator hit rect `pointer-events=all`，verifier自2026-09-28仍期待`none`；不越界改舊契約。

## 2026-10-25 DYN-T-699 / 700 / 701 / 702（0.2.33）

- DYN-T-699：主圖 boundary pan 對 250-visible fixture 驗 signed delta `-10/+10`只移 exact 10 bars且 count 保持250；真例 resident 擴成3000時，requested 2,995,074、returned start 2,993,994 必須重定位 local 1,080／count 1,500；未完整包含requested window則不得settle。
- DYN-T-700：左右 Y 軸共用五點 high→low interpolation；source contract要求每row左右selectors、side／tick index／value metadata與`pointer-events:none`。
- DYN-T-701：有 actual GLOBAL loader 時，document但`GlobalOverview=None`不得卸bootstrap；resolved validated overview才可monotonic replacement。無loader維持第一個document mount；Client source同驗共用gate。
- DYN-T-702：source/ordering gate要求bootstrap與authoritative stable hosts、authoritative host先`Doc.Run`、RAF觀測GLOBAL non-zero geometry、最後才`Doc.Empty`退役bootstrap。修前focused 1/1 RED（缺stable host），修後GREEN；package bundle另核四個markers存在。
- functional source `6d2f885bf94886d82443ab9ca89cbefbf6c80408`；0.2.31因same-version overwrite退役。0.2.32 exact consumer identity與final no-1K authority均PASS，但MutationObserver仍有5個零visible GLOBAL batch而退役。Immutable 0.2.33 package source `6f13fbcd5fa3b63c86fac244ea62282dc42db111`；source-identical staging完整啟用WebSharper build，兩個DLL ProductVersion與兩包nuspec RepositoryCommit皆為該commit。Fresh isolated package cache實選Renderer／Client 0.2.33；Renderer 94/94、Interactive.Client 19/19 passed，0 ignored/failed/errored；Interactive package verifier PASS。Renderer SHA-256 `8BCF7E4A514B16A1A60C6DC1ACB878E08DBCA211DCE0196280E12CE8644EC1ED`；Interactive.Client SHA-256 `7341C532F3C6194C8709C4D8C044EA75EA2027FEDAC9E2B18C74F17E9319FCE7`，exact Renderer `[0.2.33]`；canonical bin／SDK 10.0.402 library-packs／staging hash parity PASS。
- consumer acceptance（Daedilus，2026-10-25）：三consumer與Research output ProductVersion核為`0.2.33+6f13fbcd`。1500-visible主圖drag `delta=-420`為absolute exact -420且非resize；每row左右各5 ticks、文字／值一致；9-case TAGGING matrix全PASS。Research custom Client原直接呼叫Renderer，故在consumer補等價stable雙host後真no-1K gate為continuityGapCount=0、final globalCount=1／visible、bootstrapTotal=finalTotal=2,996,994、5K mapped relocation PASS；Research Release WebSharper build 0 errors。此E2E不冒充直接執行package Client host，後者由DYN-T-702／bundle owner gates覆蓋。Runtime evidence：`G:\coldfar_py\coldfar-symbolics\temp\agent.daedilus\global-without-1k-row-0.2.33-research-host-final\`；長期receipt為COMM `msg-fsi-5da8df09eb424801a88d8b89e3c029f2`。

## 2026-10-24 DYN-T-696 / 697 / 698（0.2.30）

- DYN-T-696：合法3,001,148筆whole-series overview通過；resident超total、anchor ordinal/time逆序拒絕，且不建立runtime frame。
- DYN-T-697：source contract驗actual GLOBAL selector、既有points/selection/calendar axis、read-only／non-authoritative markers、typed loader、validation、Async start及authoritative monotonic原位取代；禁止CustomEvent、listener、fetch、IndexedDB、action與假RuntimeState。
- DYN-T-698：Interactive.Client只接受optional `unit -> Async<TaGlobalCoverageOverview option>`，在`handle.Start()`前掛載；不保留DOM bootstrap transport，也不發projection receipt。
- final source `e3fd1c47f8e3476b685aa478da379c433c52b25d`；isolated exact-package Renderer 92/92、Interactive.Client 18/18 passed，均0 ignored/failed/errored；package verifier PASS。Renderer 0.2.30 SHA-256 `EAAEE506C72BF28BAA958F820E909FB10B84325A13C0ECBCBC5C3BB09D634982`；Interactive.Client 0.2.30 SHA-256 `CADA0C80C0FBA645A5DE22BCEA863446EF6BABBD498CDEC33ED5B03F131497A9`，exact Renderer `[0.2.30]`、兩包RepositoryCommit皆為final source。未public push；Research loader／18884真3M actual-before-chart由Daedalus續驗。
- consumer acceptance（Daedalus，2026-10-24）：真3M default-launcher cold actual GLOBAL 726ms、main 2255ms、authoritative replace 6143ms；warm封鎖catalog/overview API仍536ms，GLOBAL max node=1、console=0。Tagging official matrix 8/8且每次responseDelta=1；main plot drag同步兩個overview且tagging resolve=0。Launcher既有env預設256MiB並尊重override，warm五gesture 3.346–4.998s、0 miss/SQL；Research Release Rebuild 125 warnings/0 errors，18884 PID7480 health OK。

## 2026-10-24 DYN-T-692 / 693 / 694 / 695（0.2.29）

- DYN-T-692：Interactive.Client source contract要求 body receive、queue wait、publish、commit、double-RAF first paint 與 transport-sequence correlation；final package-backed 17/17 passed，0 ignored/failed/errored。
- DYN-T-693：主圖 pan delta驗正負方向、half-away-from-zero、invalid/zero domain與4px click threshold；Renderer browser source另驗 document pointer lifetime、click suppression及 canonical pan delegation。
- DYN-T-694：activation v2對同 row/event-time聚合 PDI/MDI，排除另一 timestamp，保留 exact DataRef/typed value；source contract要求 v2與legacy v1 dual send。
- DYN-T-695：fresh mount source gate要求 explicit `initialGlobalPhase`、GLOBAL phase-1 marker、row/RESIDENT hidden與full preparation後下一frame release；Renderer telemetry另驗 receive→pending marker。
- source commit `5924bc348a9751b780e55c53a2b47f1e5dfb6f37`；isolated final-package Renderer 90/90、Interactive.Client 17/17 passed。Renderer 0.2.29 SHA-256 `406E9F2BA59C8C924531A2D9726C839AF44B3FAD61F53E5AB846184482C5B3B4`；Interactive.Client 0.2.29 SHA-256 `8D9A8F60A497D1526B3BA012628C9A0894658ED7985026F86BF7BEB1046D56A3`，exact Renderer `[0.2.29]`；兩包 nuspec RepositoryCommit皆為上述 source commit，local feed hash parity PASS，未 public push。真3M drag／fresh-paint／latency telemetry與 official tagging consumer仍由 Daedalus 在18884驗收，unit/package不冒充 real UI。

## 2026-10-24 DYN-T-690 / 691 GLOBAL calendar axis

- DYN-T-690：對0.2.27 package把完整年度fixture縮為900px並要求45筆mandatory ticks，先得到actual 27、88/89的修前RED。0.2.28驗2019–2025每年`YYYY/03/06/09/12`、900px全45筆保留、80px compact viewport不drop、同anchor year＋03占不同numeric lane、同月首anchor dedupe、same-year narrow domain、label無`HH:mm`、canonical timestamp未改，並按numeric lane逐一驗至少24px horizontal spacing。
- DYN-T-691：source contract限定GLOBAL使用dedicated calendar axis／`GlobalOverview.Anchors`、dynamic tick lane、lane-count metadata與動態axis高度；RESIDENT仍使用generic `timeAxisWithPalette`，避免把全域格式變成一般軸全域規則。
- committed-source 0.2.28 final package-backed：Renderer 89/89、Interactive.Client 16/16 passed，均0 ignored/failed/errored；兩個isolated NuGet cache nupkg SHA與final source package逐byte相同。Research 18884真3M 900px／1600px、geometry與navigator互動由Daedalus重驗，不以unit冒充UI。
- consumer acceptance（Daedalus，2026-10-24）：package SHA與DLL ProductVersion `0.2.28+51fe2b43928901778ecdda6133d3ad77c95441f1`一致；1600px為43 ticks／2 lanes、900px為43 ticks／3 lanes，2019–2025完整`YYYY/03/06/09/12`，missing／invalid／browserErrors空、chronological／per-lane nonOverlap true且視覺無`HH:mm`或overlap。Navigator真2,996,994筆、resident 1500、relocation actions 1、atomicSamples 1358／gaps 0，oldest `0+1500`、latest `2995494+1500`與timestamps exact；TAGGING KBar 1K/5K/30K/60K及SMA/DMI/MACD official editor皆status 200/editor true。外部真路徑證據：`G:\coldfar_py\coldfar-symbolics\temp\agent.daedilus\global-calendar-axis-0.2.28-1600\`、`global-calendar-axis-0.2.28-900\`、`global-strip-contract-0.2.28\`、`tagging-scale-matrix-0.2.28\`。

## 2026-10-24 DYN-T-687 / 688 / 689 datapoint activation

- DYN-T-687：相同canonical x下，以rendered y區分candle `BAR`與line `POINT`；不同x選最近visible datapoint，non-finite browser geometry拒絕。
- DYN-T-688：只有等於`ProjectionCommitGate.LastCommitted`的runtime／coverage key能建立versioned receipt；較新未commit state及無commit皆拒絕，payload明確不含`VisibleIndex`。
- DYN-T-689：source contract要求surface selector、rendered hit-test、committed gate、typed CustomEvent及clicked-surface dispatch，且publisher bounded body不得引用`SharedCursorChanged`。
- final package-backed：Renderer 0.2.25為87/87 passed、Interactive.Client 0.2.25為16/16 passed，均0 ignored/failed/errored；兩個isolated restore的nupkg SHA與final feed逐byte相同。真Research main `ta-candle`／`ta-composite`點擊開official editor仍由daedilus在18884驗，不以unit冒充UI。

## 2026-10-23 DYN-T-683 / DYN-T-684 regression

- DYN-T-685：against0.2.23已2 executed/2 failed（literal expected start1800/actual1500及reflection）。4500 resident/max1500，left外拉/內拉保持既有literal；right在cap、跨cap、低於cap、內拉到minimum、resident tail有獨立expected。另驗77組domain reflection（empty、短於minimum、full domain、首中尾、未到cap及正負/零/極大delta）。0.2.24修後package-backed Renderer完整suite於2026-10-23 04:42執行84/84 passed、0 ignored/failed/errored；real 4500/1500左右drag、drop與outside paging由daedilus另驗，unit不冒充UI。
- DYN-T-686：0.2.23低干擾真3M frontend固定約9秒floor為修前RED。新增fake-clock FIFO/1000 tasks→16 bounded turns/count上限、4ms time上限且不可拆atomic step、錯誤後清理、非法budget拒絕；FramePump整合驗跨turn取消與只發表完整candidate一次，保留原decode/chunk/reducer suites。0.2.24 package-backed Interactive.Client於2026-10-23 04:47執行16/16 passed、0 ignored/failed/errored；兩suite均實際restore/build/run且assets驗exact24。Client.Tests先因新增open遮蔽Contracts DU而build四處FS0001，移除廣域open、完整限定唯一scheduler呼叫後build 0 warnings/0 errors，不改產品package或oracle。新真3M timing待驗，callback CPU或unit不可冒充UI wall-clock改善。
- DYN-T-686 / 0.2.44：新增fake-clock policy regression，以相同100個3ms atomic work驗initial 16ms budget在最多17 turns完成、incremental 4ms仍為50 turns，且兩者完成數與work time相同。fresh exact-package focused 1/1、Renderer full100/100、Interactive full20/20 PASS。live telemetry只證Renderer owner share可移除約0.8–0.9s RAF queue；RUN 5.96s、action response 11.47s、30.9M body與3.36s action frame仍須consumer E2E，unit/package PASS不得冒充absolute first paint <=3s。
- 0.2.23 owner回報四項functional真3M PASS（含serial12/12、第二黑閃2361RAF/0gap、GLOBAL1309/0gap、右outside），2018 query UI另open。新GLOBAL latency gate仍RED/open；15.096s首輪被probe污染，不能作unperturbed baseline；action路徑不更新data-phase-*，需分段實測與projection receipt。
- 0.2.23修前RED：對0.2.22 package用TotalObservationCount=None＋真實global/resident/active ordinals，Later expected1502131/actual1500631（1 failed）；Client adjacent cache false KnownEmpty expectedMiss（同filter3 executed/2 passed/1 failed）。修後驗None/Some頂層total、同一queued方向、全域end/view-all、global overflow拒絕與GlobalOverview=None的legacy bound，完整suites與真3M仍分開報。
- 0.2.22：0.2.21 package修前1 failed，獨立last-pointer oracle expected1498/actual1423；最後move→up無RAF、左右handle/body、boundary與非boundary、取消/連續手勢皆需驗。舊unit的paint優先oracle依75bars真實反例更正，非為換綠；source wiring同驗兩條pointerup入口及ratio/draft同步。
- consumer local ACK不得以沒有HTTP判FAIL：CoverageIntent=None且time range完全在已載入range可local Accepted，但需驗實際chart window、ready/pending與target containment；remote仍需correlated requestId/response及matching coverage commit。右端outside request-later未送action仍RED，與此local例外無關。
- 修前RED：resident drop在remote dispatch前清draft；702-bar從local1198越界卻以舊visible起點算page，少載1198。兩個focused Expecto案例已各重現1 failed。
- 0.2.21候選：package-backed Renderer / Client suites；same-resident revision前進仍可排送且保寬，跨identity/resident與倒退revision/generation拒絕；pointer在track內2px須local clamp，真正越界才左右page；source wiring驗draft ownership與frame settlement。
- 真3M由daedilus在18884用無額外settled等待的連續resize/move/drop驗無退回、無stale、single action；左右完整page、global與舊兩黑閃分項回歸。完整resident資料local可用性仍是open contract gate，不以focused或preview PASS代替。

## 1. 測試專案規劃
測試專案位於 `tests/PulseTrade.Comm.Spa.Dynamic.Tests`。
使用 **Expecto** 框架進行單元測試與整合測試。
此測試計畫依據 WBS 的切分進行實作前定義。

## 2. 測試案例規劃 (Test Cases)

### 2.1 後端擴充點掛載測試 (Server Extension Mount Test)
- **測試目標**：確保 `CommHub.useDynamicSdui()` 能正確將 Dynamic Extension 的環境或 Actor 載入。
- **測試原理**：
  1. 實例化一個模擬的 `CommHub` (利用唯讀的 PTCS 上游套件建立 Dummy Hub)。
  2. 呼叫 `.useDynamicSdui()` 擴充方法。
  3. **判讀標準**：驗證呼叫後不拋出例外，且回傳的 Hub 實例非空。如果系統內建 Actor 查詢機制，則檢查指定的 Actor (`ShowcaseDemoActor` 等) 是否存在。

### 2.2 FCell AST Parser 轉換測試 (AST to JSON Test)
- **測試目標**：驗證抽離出來的 `FCell2Interop.fs` 能將 F# DSL 正確序列化為 JSON Payload。
- **測試原理**：
  1. 建立一個包含 `GridFeatures`, `CanvasComponent` 等巢狀 `fCell2` AST。
  2. 呼叫 `toJsonString()` 或 `toMessagePayload()` 函式。
  3. **判讀標準**：驗證輸出的字串是合法的 JSON，且 `schema` 欄位為 `"ptc.comm.fcell2.chat.v1"` 或預期的 `fskynet-sdui` 字串，並且內容符合預先定義的 Schema 格式。

### 2.3 前端 Renderer 註冊測試 (Client Renderer Hook Test)
- **測試目標**：確保前端的 SDUI Renderer 在被觸發時，能正確識別對應的 JSON Payload。
- **測試原理**：
  由於 F# WebSharper 的 DOM 邏輯在 Server 端 (Node/JS) 執行單元測試較為困難，我們在此測試 `TryRender` 的預判定邏輯。
  1. 直接實例化 `DynamicSduiRenderer` 中的 `TryRender` (去除 UI 操作部分，或模擬 DOM Node 回傳)。
  2. 傳入包含 `"schema": "fskynet-sdui"` 的字串，驗證回傳 `Some node`。
  3. 傳入一般字串 `"hello world"`，驗證回傳 `None`。
  4. **判讀標準**：Renderer 必須只對特定的 SDUI Schema 起作用，不會誤攔截一般對話。

## 3. 開發與測試流程 (TDD Execution)
根據 WBS，在進入 `WBS-200` 系列的開發前，必須先完成此文件內 `2.1` 到 `2.3` 所有的 Expecto 測試撰寫 (測試案例初期應該會是 Failing 的，等待實作後轉為 Passing)。

## 4. RFC-PTCS-DYNAMIC-0002 Dynamic Argu Form Gates

### DYN-T-401 Formal RFC flow

驗證文件：

- `doc/RFC-PTCS-DYNAMIC-0002.dynamic-argu-form-runtime.md`
- `doc/REQ.md`
- `doc/SA.md`
- `doc/SD.md`
- `doc/WBS.md`
- `doc/TEST.md`
- `doc/Traceability.md`
- `doc/DevLog.md`

判讀標準：

- 來源草稿 `REQ_Dynamic_Argu_Form.md` / `RFC_Dynamic_Argu_Form.md` 保留且被 formal RFC 引用；
- formal RFC 明確區分 PTCS.Dynamic、PTCS core、PTC RN / RN.Host 責任；
- WBS 有跨專案相依順序。

### DYN-T-402 Metadata / schema generator

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests` 內 `DYN-T-402`。

覆蓋：

- allowlisted metadata 轉成 `schema = "fskynet-sdui"`、`formMode = "argu-form"`；
- int/decimal/string/bool/enum/date/time/color field kind mapping；
- invalid DU type / unknown union case controlled failure；
- browser-supplied arbitrary type name 不會 unrestricted reflection；
- generated input ids / `arguParam` stable。

### DYN-T-403 SubmitArguForm codec

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests` 內 `DYN-T-403`。

覆蓋：

- scoped form state collection；
- whitespace / quote escaping；
- empty optional field omission and required field validation；
- output is raw Argu args string only，不執行 shell command；
- invalid schema / missing `arguParam` controlled failure。

### DYN-T-404 / DYN-T-405 PTCS seam browser gates

PTCS `WBS-051B/C` seam 已有 first implementation；browser/runtime gate 目前由 PTCS repo 的 F# Playwright verifier 執行。2026-06-26 regression expansion 已通過 append renderer throw fallback、invalid blank renderer submit isolation、duplicate target key idempotency、built-in add-key fallback、built-in `fcell-chat` textarea regression 與 desktop/mobile geometry。

預計 verifier：

```powershell
dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-build -- --summary
dotnet fsi --exec G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Scripts\verify.dynamicArguFormDurableProxy.playwright.fsx
```

覆蓋：

- Dynamic append input renderer replaces textarea only for matching `actor-dynamic` key；
- renderer missing/throw fallback textarea；
- blank renderer submission shows controlled validation and does not reach RN DurableProxy；
- Add Key dialog first-slice proof returned `actorAddress :: duTypeName :: unionCaseNames` with no delimiter-joined union-case segment；RFC-0003 revised canonical path is now `actorAddress :: duTypeOrTemplateKey :: canonicalArgString`；
- reload/readback returns the PTCS ordered append-page key list while preserving actor address、template key and canonical arg string as separate key segments；
- duplicate Dynamic target key submit keeps one projected key/card；
- desktop/mobile geometry has no overlap or hidden submit button；
- built-in PTCS append pages and existing `fskynet-sdui` message rendering do not regress；
- built-in `fcell-chat` textarea page can still append and read back from its stream when no Dynamic renderer owns the page。

### DYN-T-406 / PTC3-T-067 Cross-project RN proxy E2E

Browser/runtime verifier 已通過 user-facing UI E2E contract：`--pcsl-root` + run-scoped fresh PCSL root、PTCS + Dynamic extension、DurableProxy actor、legacy echo actor、Playwright 建立 `actor-dynamic` page、Dynamic add-key target、variable-length Dynamic key tail、canonical PTCS key readback、text/number/enum/tuple/bool/list form input、raw Argu preview/send、RN DurableProxy fCell2 string forwarding、legacy echo reply 與 `ActorArguTargetReply` full target-key readback。Production-strength gate 仍需等待：

- PTCS `WBS-051D/E`；
- PTC RN `PTC3-063` / `PTC3-066` controller-region restart redelivery and provider completion gaps；
- PTC RN Host `PTC3-065` service-window operational policy for the selected deployment proof。

預計資料流：

```text
Dynamic form submit
  -> PTCS append / actor-argu path
  -> ActorArguTargetCommand.RawArgu
  -> RN DurableProxy
  -> fCell2 string
  -> legacy actor/service reply
  -> ActorArguTargetReply
  -> PTCS fresh history/result readback
```

此 gate 不得以 fake/mock/internal-only proof 當 final acceptance。

## 5. RFC-PTCS-DYNAMIC-0003 Unified SDUI / Form DSL Gates

### DYN-T-501 DSL document model

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests`。

Current package verifier：`dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-restore -- --summary --no-spinner`，Expecto 15/15 pass。

覆蓋：

- `SduiDocument` 可表達 Canvas 與 FormInput surface；
- shared node/action/binding JSON codec round-trip；
- Canvas-only node 與 FormInput node 不需要不同 schema root；
- invalid schema/version/duplicate id controlled failure。

## 6. RFC-PTCS-DYNAMIC-0005 ActorsPage Renderer Gates

### DYN-T-526 ActorsPage renderer registration/classifier

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests` 內 `DYN-T-526`。

Current first-slice 判讀：

- payload 含 `ActorTopologyPage` 時，`ActorDynamicTab.IsActorsPagePayload` 回 true；
- Dynamic entrypoint 會呼叫 page renderer registration；
- full WebSharper bundle build 必須用 `DYN-VFY-001` short path 先過。

限制：目前 classifier 使用單一 `IndexOf("ActorTopologyPage")`，因 `String.Contains` 與多段 predicate 會讓 WebSharper 10.1.5.674 `wsfsc.exe` crash。嚴格 `schema/surface/documentType` parser 是 DYN-T-528 之後的 gate。

### DYN-T-527 Generic Canvas isolation

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests` 內 `DYN-T-527`。

Canvas message payload 不含 `ActorTopologyPage` 時，ActorsPage classifier 回 false。Generic Canvas renderer 仍由 `DynamicRenderer.TryRender` 處理一般 `schema=fskynet-sdui` message reply。

### DYN-T-528 ActorsPage node grouping and tree toggle gate

2026-06-28 current source-host evidence:

- PTCS source host + Dynamic source Release bundle passes a Playwright MCP gate on `/actors`;
- Dynamic page renderer is registered and the same renderer path is also protected through the transitional `MessageRenderers` fallback;
- Dynamic page host is present, fallback tree/table rows are absent, blocks are ordered PTCS Host -> GW Host -> RN Host, and full `akka.tcp://...` addresses are visible;
- `0.1.3-beta24` restores hierarchy presentation inside each host block: virtual `/user` and `/system` ancestors remain visible, status dots and connector lines are present, and virtual ancestors do not create a synthetic Unknown block;
- `0.1.3-beta27` with PTCS `0.2.5-beta40` verifies the same hierarchy and also asserts PTCS core does not append fallback `actor-node` / `actor-card` DOM after Dynamic accepts `/actors`;
- boxed tree toggle is functional: click changes `- / aria-expanded=true` to `+ / aria-expanded=false` and re-expand restores the child rows;
- evidence is `G:\PulseTrade.fs\log\20260628\20260628220000.actors-page-toggle-check.json`;
- screenshot evidence is `G:\PulseTrade.fs\log\20260628\20260628220000.actors-page-toggle-fixed.png`.
- reusable F# Playwright verifier `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Scripts\verify.actorsPageDynamic.playwright.fsx` now repeats the accepted path with locator-only assertions for Dynamic ownership, fallback absence, core-card absence, full addresses, report controls, status dots, connectors, depth rows, no synthetic Unknown block, and visible row collapse/expand; beta40/Dynamic beta27 screenshot evidence is `G:\PulseTrade.fs\log\20260629\20260629011000.actorspage-beta40-dyn27-mcp.png`.
- `0.1.3-beta29` adds browser-local report schedule start/stop verification to the same F# Playwright verifier; package bundle verification now rejects the beta28 stale schedule-disabled marker.

### DYN-T-529..532 Remaining ActorsPage gates

尚未完成：

- strict ActorsPage DSL parser / codec；
- server-side persisted report schedule / restart / failover visual-state gates beyond the current grouped/toggle/hierarchy/schedule-browser slice；
- Dynamic absent / unsupported renderer fallback is covered by PTCS `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Scripts\verify.actorsActorTree.playwright.fsx -- --with-unsupported-client-extension`; Playwright MCP evidence: `G:\PulseTrade.fs\log\20260629\20260629001159.actors-unsupported-fallback-playwright-mcp.png`。

目前已落地的 package coverage：

- `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta29` rollout completed the package/public 81 hierarchy ownership plus browser-local report schedule gate with PTCS `0.2.5-beta40` and public release `live81-ptcs-beta40-dynamic-beta29-report-schedule-202606290205`；
- `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta30` rollout completed the offline cleanup/display slice with PTCS `0.2.5-beta41` and public release `live81-ptcs-beta41-dynamic-beta30-offline-poc2-202606290748`; `poc.full.nuget.2.fsx -- --no-wait` verifies the beta41/beta30 NuGet POC path with Actor Argu Add target key, no `+ Page` Actor Dynamic page creation, and non-empty `/actors/api/snapshot` from the POC2 `nuget2-echo` actor registry projection. Playwright evidence for local POC2 `/actors` is `G:\PulseTrade.fs\log\20260629\poc2-actors-page-fixed-deep-snapshot.md` and `G:\PulseTrade.fs\log\20260629\poc2-actors-page-fixed-deep.png`.
- `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta33` pairs with PTCS `0.2.5-beta43` and `FAkka.WebSocket 1.569.101.301-win6`. `poc.full.nuget.2.fsx -- --no-wait` now additionally asserts the `POC2 FormInput target` alias survives the server-side ActorArgu send/probe path, covering the regression where a later blank key intent replay overwrote alias display with the long target key. The same run completed without `WebSocket disconnected` / `ConnectionAborted` console noise; beta33 also adds ActorsPage browser console groups `[PTCS.Dynamic ActorTree DSL] RENDER/RELOAD` for raw/parsed tree DSL inspection.
- `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta38` pairs with PTCS `0.2.5-beta48`. `poc.full.nuget.journal.fsx -- --no-wait` verifies the ActorRegistry PingPong stop/reload path: the stopped actor is projected as `terminated`, default active ActorTree DSL filters it (`pingPongFiltered=true`), and `includeOffline` keeps it only for diagnostics.
- `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta48` pairs with PTCS `0.2.5-beta60`. `poc.full.nuget.journal.ACL.fsx -- --no-wait --local-port 18082 --github-port 18081 --cluster-port 18787 --pcsl-root .\.pcsl\verify.acl.beta48` verifies the ACL/Login dual-auth path on non-conflict ports: 81-style GitHub OAuth host, 82-style PTCS.Login host, DamnWZ/AssTerry pages, Echo/PingPong target keys, local login, ACL snapshot, Dynamic bundle, durable ActorArgu echo, PingPong stop filtering, fixed actor name reuse, PTCS beta53 Login session-store package compatibility, PTCS beta54 ACL audit compatibility, PTCS beta55 WebSocket principal revalidation compatibility, PTCS beta56 WebSocket proxy cleanup compatibility, PTCS beta57 HTTP ACL canonical resource compatibility, and PTCS beta60 TLS-offload same-origin compatibility. Public 81 health/alignment and `/page/assterry` FormInput send/reply proof is `live81-ptcs-beta60-dynamic-beta48-acl-demo-stale-cleanup-202607010337`; Playwright MCP evidence is `G:\PulseTrade.fs\log\20260630\public81-assterry-beta60-stale-cleanup-after-send-snapshot.md`, `.png`, and `.txt`.
- `poc.full.nuget.journal.fsx` manual FSI mode must use `ensureEchoActorRegistered()`, `stopEchoActor()`, or `recreateEchoActor()` for the stable `nuget-journal-echo` actor. Re-running `ActorOfRegistered(..., actorName)` while the actor is live is expected to fail because Akka actor names are unique under `/user`; after stop and path release, `recreateEchoActor()` verifies the same fixed actor name can be reused.
- cross-repo PTC package verifier checks ActorsPage/toggle/status-dot/connector/report/schedule bundle markers and rejects the stale beta28 schedule-disabled text；
- latest public 81 Playwright MCP proof is `G:\PulseTrade.fs\log\20260629\public81-actors-beta41-dyn30.png`；snapshot is `G:\PulseTrade.fs\log\20260629\public81-actors-beta41-dyn30-snapshot.md`；DOM summary is `G:\PulseTrade.fs\log\20260629\public81-actors-beta41-dyn30-dom.json`；
- `SduiFormDocument.fromArguFormSchema` 產生 `schema=fskynet-sdui`、`surface=FormInput`、stable `documentId`；
- PFCF_AKKA_CMD fixture 反射 `SimpleAction`、`BBA`、`GenByColMeta`；
- `GenByColMeta` tuple item kinds 驗證為 `bool-value`, `bool-value`, `text`, `enum`。

### DYN-T-517 Canvas Tree renderer for ActorTreeDocument

Verifier：package focused test plus PTCS browser E2E after PTCS `WBS-054` exists。

Required package coverage：

- `SduiDocument Surface=Canvas` can contain a `Tree` node with `dataRef`, node/parent/label/status field names, `connector=orthogonal`, and `toggle=boxed-plus-minus`；
- DSL decode rejects arbitrary script/action payloads and unknown connector/toggle values with controlled errors；
- node data can represent `id`, `parentId`, `label`, `kind`, `status`, `fullPath`, and optional columns without requiring Dynamic to know Actor Registry storage；
- renderer keeps straight connector geometry and boxed plus/minus toggles, with bounded layout and no text overlap。

Required PTCS integration coverage：

- PTCS Actors tab can convert `ActorTreeDocument` to Dynamic Canvas `Tree` when extension is loaded；
- the same `ActorTreeDocument` renders as PTCS fallback table with `parentId` when Dynamic is absent or renderer fails；
- Dynamic does not own PCSL projection, IndexedDB cache, registry truth source, or report write path。

### DYN-T-502 Argu-to-FormDsl adapter

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests`。

Current package verifier：`dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-restore -- --summary --no-spinner`，Expecto 15/15 pass。

覆蓋：

- host-registered `IArgParserTemplate` 轉成 Form DSL document；
- 每個 requested union case 轉成 visible section；
- string/number/bool/enum/tuple/list/nested ParseResults supported or controlled unsupported；
- no `SampleArgu` or PTCS.Host-specific DU in package source；
- unknown DU type / union case 不做 unrestricted browser reflection。

目前已落地的 package coverage：

- server `SubmitArguFormCodec.buildRawArgu` 與 frontend `ClientRawArguCodec.buildRawArguFromValues` 產生一致 raw arg string；
- PFCF_AKKA_CMD covered cases：`SimpleAction`、`Entrust`、`PFCFGTC`、`BBA`、`Cooperative`、`ParentChilds`、`FractionalQuote`、`GenByColMeta`、`TableName`；
- expected raw arg strings include `--simpleaction "rebuild all"`、`--pfcfgtc gf --pfcfgtc goi`、`--bba F001 B001 M123`、`--genbycolmeta true false dbo fsrecord`、`--tablename Orders --tablename "Positions Today"`。

### DYN-T-503 Dynamic target resolver first-slice regression

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests`。

Current package verifier：`dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-restore -- --summary --no-spinner`，Expecto 15/15 pass。

覆蓋：

- `[ actorAddress; formDslId ]` resolves as direct DSL target；
- `[ actorAddress; duTypeName; case1; case2 ]` resolves as Argu adapter target；
- first item is always actor address；
- no canonical `1:duType:` / `2:unionCases:` prefix；
- unknown second segment returns renderer validation error。

目前已落地的 package coverage：

- `DynamicTargetKey.tryResolve` resolves `[ actorAddress; formDslId ]` to `DirectDslTarget`；
- `DynamicTargetKey.tryResolve` resolves `[ actorAddress; duTypeName; SimpleAction; BBA; GenByColMeta ]` to `ArguTemplateTarget` and preserves union-case tail order；
- unknown discriminator、unknown union case、direct DSL target with union-case tail all fail with controlled errors。

此 test 是 first-slice regression。RFC-PTCS-DYNAMIC-0003 的新 canonical DU/template target 已改為 `[ actorAddress; duTypeOrTemplateKey; canonicalArgString ]`，由 DYN-T-507..511 接手。

### DYN-T-504 Backend-linked option provider

Verifier：focused F# test plus browser E2E after PTCS `WBS-053E`。

覆蓋：

- `QueryOptions(providerId, dependsOn)` updates dependent select options；
- unregistered provider rejected；
- arbitrary URL/header/script is not accepted；
- provider diagnostics do not include secrets。

### DYN-T-505 Actor key bound to direct DSL target

Verifier：F# Playwright script in PTCS repo after PTCS `WBS-053` seam and Dynamic v2 renderer are ready。

Required path：

```text
actor key [ actorAddress; formDslId ]
  -> Dynamic resolves direct Form DSL
  -> FormInput renderer submits ValueText
  -> PTCS existing append / actor-argu path
  -> target actor / proxy receives command
  -> PTCS history readback
```

### DYN-T-506 Actor key bound to DU/template + canonical arg string

Verifier：F# Playwright script in PTCS repo with PTCS.Host demo DU。

Required path：

```text
actor key [ actorAddress; duTypeOrTemplateKey; canonicalArgString ]
  -> PTCS.Dynamic backend parses canonicalArgString with registered Argu parser
  -> Argu-to-FormDsl adapter
  -> parsed root cases and supported subcommands visible simultaneously
  -> composite raw Argu submit
  -> RN DurableProxy / legacy echo
  -> ActorArguTargetReply
  -> PTCS full target-key history readback
```

PTCS.Host demo source is `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\doc\example DU.txt` decoded as Big5/cp950。Missing external types/enums must be stubbed or excluded with controlled unsupported-case diagnostics in PTCS.Host, not in PTCS.Dynamic package。

### DYN-T-507 Arg-string target resolver

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests` plus backend resolver verifier。

覆蓋：

- `[ actorAddress; formDslId ]` remains direct DSL target；
- `[ actorAddress; duTypeOrTemplateKey; canonicalArgString ]` resolves to parser-backed Dynamic target；
- no `hub.useDynamicSdui(...)` / no Dynamic resolver means PTCS core can still use only `actorAddress` as built-in actor key；
- unknown template key、missing canonical arg string、parse failure all return controlled error。

### DYN-T-508 Alias binding

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests`。

覆蓋：

- case alias appears in `SduiDocument` section title；
- field alias appears in input label；
- option alias appears in select display label；
- raw Argu command still uses canonical Argu names and values；
- alias metadata does not come from browser target key。

### DYN-T-509 Parser-backed Form DSL defaults

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests`。

覆蓋：

- canonical arg string is parsed by the registered `IArgParserTemplate`；
- parsed root cases determine rendered FormInput section order；
- field default values are projected into `SduiFormNode.DefaultValues` from parse result / token scan；
- list and named tuple defaults are projected without changing canonical raw Argu tokens；
- unsupported template key、missing arg string、parser failure produce controlled errors via DYN-T-507。

Latest evidence：2026-06-26 `dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-restore -- --summary --no-spinner` passed 14/14。Warnings were existing WebSharper `WS9002` and NuGet `NU5123` long path warnings。

### DYN-T-510 ParseResults / subcommand raw command builder

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests`。

Required expected command：

```text
--pfcfedx trivial --pfcfgtcconf OIInf TAIFEX FillSquareCombine OrderByTXDT CathayBKTaifexFill --to 90000 --parentchilds 2 5 --bba F008 000 9910357 --decimalquote 6 0 --round 6 4 2 datarange --referencedatemode ModeAccountingDate --between 20251104 20251104 --calibrate2curdayiflargerthancurday
```

覆蓋：

- `ParseResults<PFCF_AKKA_CMD_DATA_RANGE>` is discovered as tail subcommand；
- root args precede `datarange`；
- `datarange` precedes `--referencedatemode` / `--between` / `--calibrate2curdayiflargerthancurday`；
- command rebuild is deterministic and matches expected raw string exactly。

Latest evidence：2026-06-26 `dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-restore -- --summary --no-spinner` passed 14/14。

### DYN-T-511 Backend resolver endpoint package gate

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests`。

覆蓋：

- `DynamicArguResolveEndpoint.handle` accepts JSON `{ keys = [ actorAddress; duTypeOrTemplateKey; canonicalArgString ] }`；
- backend uses registered `DynamicArguTemplateRegistration`, not browser reflection, to parse the canonical arg string；
- response includes actor address、template key、canonical arg string and a FormInput DSL document；
- returned document contains alias/default projection for the PFCF data-range command；
- returned document contains all parsed root sections plus the `DataRange` tail subcommand section；
- full-form raw reconstruction preserves list-inline tokens、root tuple defaults、tail tuple defaults and exact `datarange` tail ordering；
- controlled failure is returned as JSON `Ok=false` instead of silent fallback；
- WebSharper client append renderer compiles with the backend-resolved fetch path and document-backed full-form Send path.

Latest evidence：2026-06-27 `dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-restore -- --summary --no-spinner` passed 15/15 for beta12。The package gate verifies exact canonical enum defaults are present in FormInput values, including `DataRange.ReferenceDateMode.value = ModeAccountingDate`；it also verifies a partial canonical arg string `--pfcfedx trivial --pfcfgtcconf OIInf TAIFEX` renders only `PFCFEDX` and `PFCFGTCCONF` instead of the full DU schema。List-valued Argu fields keep parser-projected defaults on the list node, but list item schema is `text` with no enum options so FormInput presents editable repeatable textboxes, not fixed dropdowns。

### DYN-T-512 Browser E2E for backend-resolved FormInput DSL

Verifier：F# Playwright script in PTCS repo after PTCS.Host references updated package。

Required path：

```text
create actor-dynamic page
  -> add target key [ actorAddress; duTypeOrTemplateKey; canonicalArgString ]
  -> Dynamic backend resolves FormInput DSL
  -> browser renders alias labels and parsed default values
  -> submit form
  -> ActorArguTargetCommand.RawArgu equals DYN-T-510 expected command
  -> RN DurableProxy / echo path returns ActorArguTargetReply
```

UI gate must inspect visible labels/controls and final submitted raw string. It must not pass by only testing server codec.

Latest evidence：2026-06-27 `dotnet fsi --exec G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\verify-ptcs-host-dynamic-argu-live.fsx -- --port 0 --extension-dir C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\src\bin\Release\net10.0` passed with:

```text
ptcsHostDynamicArguLive.ok url=http://127.0.0.1:9711 page=http://127.0.0.1:9711/page/dyn-argu-live81 template=PulseTrade.Comm.Spa.Host.Program+DynamicArguDemo+PFCF_AKKA_CMD submit=echo-verified pcslRoot=G:\PulseTrade.fs.Comm.Log\verification\ptcsHostDynamicArguLive\pcsl runPcslRoot=G:\PulseTrade.fs.Comm.Log\verification\ptcsHostDynamicArguLive\pcsl\run-6e598757065b483fa25b17b4805bfe85 serviceLog=G:\PulseTrade.fs.Comm.Log\verification\ptcsHostDynamicArguLive\verify-ptcs-host-dynamic-argu-live.service.log
```

The gate used PTCS.Host loopback, loaded the Dynamic extension DLL, rendered all parsed PFCF data-range sections, submitted the expected full raw Argu string, and verified DurableProxy echo readback. It also covers the user-facing add-target path: editable DU/template key text input with no datalist/select lock-in, no-target cleanup after removing the last key, generic `actor-argu` add-target UI after removing the demo page, re-created `actor-dynamic` add-target UI after removing the generic page, partial raw command rendering only `PFCFEDX/PFCFGTCCONF`, list-valued `PFCFGTCCONF` as editable textbox rows with Add value/Remove-left, Dynamic `Bind target` label, Canonical Argu string visibility after a valid template key, and canonical input preservation so it cannot regress to `"s"`. PTCS core action pool/tab close/`+ Page` and logical page labels/badges (`Actor Dynamic`/`ad`, `Actor Argu`/`aa`) are covered by the same cross-repo verifier against PTCS beta20. Public 81 deployment is tracked by PTC verification after beta12 packaging.

### DYN-T-513 NuGet bundle/live host gate

PTC-side package verifiers:

```powershell
dotnet fsi --exec G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\verify-ptcs-dynamic-nuget-bundle.fsx
dotnet fsi --exec G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\run-ptcs-dynamic-nuget-live-host.fsx -- --no-wait
```

Coverage:

- direct `#r` load of `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta13` with `PulseTrade.Comm.Spa 0.2.5-beta21`;
- local nupkg contains `lib/net10.0` DLL and bundled WebSharper JS/min/head assets;
- JS marker contract includes `Bind target`, `Add value`, Remove-left list row classes, and no retired `dynamic-argu-key-du-type-list`;
- live host starts an in-process PTCS + Dynamic server, prints Base/Chat/ActorArgu/Dynamic JS URLs, actor address, template key, web/cluster port, default key/argu, and under `--no-wait` verifies health、HTTP actor-argu send、WebSocket `actor-argu` send、state readback echo reply before calling `stopNuGetLiveHost()`。
- NuGet push accepted `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta13`; follow-up flat-container lookup lists beta13, so public restore availability is confirmed at feed index level.

### DYN-T-520 Actor Dynamic / Actor Argu action mode dispatch

Verifier：package tests plus cross-repo PTC Playwright live-host gate.

Coverage:

- Dynamic add-key renderer claims `actor-dynamic-target`, `actor-dynamic-proxy`, and `actor-argu-target`.
- `Actor Argu` target mode requires actor address + DU/template + canonical arg string.
- `Actor Dynamic` target mode can build DU/FormInput target when DU/template is present.
- `Actor Dynamic` direct actor key is not forced into FormInput when DU/template is blank.

### DYN-T-521 Actor Dynamic direct actor-key canvas route

Verifier：cross-repo PTC Playwright gate.

Required path:

```text
Actor Dynamic page
  -> Add actor key [ actorAddress ]
  -> submit JSON DSL from canvas_demo.json
  -> actor echoes/replies same DSL
  -> Dynamic message renderer renders canvas
```

Non-canvas reply must render through PTCS normal message path.

### DYN-T-522 Actor Dynamic DU/FormInput target

Verifier：existing backend-resolved FormInput gate plus mode-aware action entry.

Coverage:

- `Add target key` uses `actor-dynamic-target`.
- key remains `[ actorAddress; duTypeOrTemplateKey; canonicalArgString ]`.
- FormInput renderer resolves through backend parser and submit still produces exact raw Argu.

### DYN-T-523 Actor Dynamic proxy key builder

Verifier：package test and cross-repo PTC browser gate.

Coverage:

- `Add proxy key` visible only for Actor Dynamic.
- renderer accepts proxy actor address and RN actor address.
- submitted key is `[ proxyActorAddress; "proxy-v1"; rnActorAddress; targetKind ]`.
- first segment is proxy actor address so PTCS actor-argu route can still send to proxy.

### DYN-T-524 Actor Argu no canvas / proxy target support

Verifier：cross-repo PTC Playwright gate.

Coverage:

- Superseded beta64 path: Actor Argu action pool must not show user-facing Add proxy key.
- Dynamic renderer submits explicit `[proxyActorAddress; "target-v1"; targetActorAddress; duTypeOrTemplateKey; canonicalArgString]`.
- PTCS core routes to proxy first segment and projects target actor address into `ActorArguTargetCommand.TargetActorAddress`.
- Actor Argu FormInput route works.
- non-canvas Actor Argu reply does not get converted to canvas.

### DYN-T-526 Explicit ActorArgu target key

Verifier：`GenFileActorInvocationTest4.fsx -- --if-dyna-port --no-wait` plus Playwright MCP visual gate.

Coverage:

- Add Target Key exposes Proxy actor address and Target actor address fields.
- Persisted selected key starts with proxy actor address and contains `"target-v1"` marker plus target actor address.
- Backend resolver accepts explicit target keys by resolving against target/template/raw.
- Proxy actor receives `TargetActorAddress=Some targetActorAddress` and asks native actor on another Akka.Remote node.
- Actor Argu reply remains normal fCell chat; Canvas renderer is not invoked.
- Pass 2026-07-07: no-wait proof confirmed PTCS/proxy node `PFCF@10.28.112.109:10450`, native PingPong node `PFCFNativee9c6a148@10.28.112.93:10451`, persisted key `[proxy; "target-v1"; target; template; raw]`, `TargetActorAddress=Some target`, native `string -> fCell2.T`, and script handler `fCell2.T -> fCell2.S` before ActorArgu reply render.
- Pass 2026-07-07: Playwright MCP visual gate against `http://127.0.0.1:18182/page/damnwz` confirmed Actor Argu hides Add proxy key, Add Target Key shows both proxy and target address fields plus template/alias/canonical arg, and a manual `UI explicit PingPong` key sends through proxy to native PingPong. Evidence: `G:\PulseTrade.fs\log\20260707\ptcs-explicit-target-damnwz-before-actions.md`, `G:\PulseTrade.fs\log\20260707\ptcs-explicit-target-damnwz-after-send.md`.

### DYN-T-525 Canvas payload-only render rule

Verifier：package test or browser gate.

Coverage:

- `DynamicRenderer.TryRender` returns `Some` only for payload with `schema = "fskynet-sdui"`.
- page type / key shape alone cannot force canvas rendering.

### DYN-VFY-009 ACL2 final open-extension boundary POC

狀態：Partial pass；production SQL / disabled-user / browser Playwright / alpha12 open-provider / formal public 81 + loopback 82 deploy gates 已通過。2026-07-03 package-startup gate 已推進到 PTCS beta71 / Dynamic beta61 / Spa.ACL alpha11 / Spa.Login alpha13，並通過 NoGithubOAuth dynamic-port no-wait；fallback cleanup 與 beta71 browser/service rerun 仍未完成。

Verifier：`src\poc.full.nuget.journal.ACL2.fsx`。

目的：

- 驗證 `RFC-PTC-SPA-0013` final boundary：PTCS + Dynamic + `PulseTrade.Comm.Spa.ACL` + `PulseTrade.Comm.Spa.Login`。
- closed `PulseTrade.Comm.ACL.Core` / `PulseTrade.Comm.Login.Core` 只能以 exact binary NuGet dependency 進入。
- 不允許 ProjectReference 到 closed Core source。

Latest open-provider demo command passed on 2026-07-02：

```powershell
dotnet fsi --exec .\src\poc.full.nuget.journal.ACL2.fsx -- --if-dyna-port --no-wait --demo --pcsl-root G:\PulseTrade.fs.Comm.Log\manual\ptcsDynamicNugetJournalAcl2\pcsl_login_open_provider_alpha12_20260702_01
```

Production SQL command passed on 2026-07-02 and remains the encrypted SQL proof：

```powershell
dotnet fsi --exec .\src\poc.full.nuget.journal.ACL2.fsx -- --if-dyna-port --production-sql --sql-connection-string-encrypted-file D:\ingted.com\ptcs-sql-connection.enc.txt --sql-private-key-path D:\ingted.com\myKey.private.txt --sql-security-schema ptcs_security --sql-acl-table AclPolicySnapshotPoc --no-wait --pcsl-root G:\PulseTrade.fs.Comm.Log\manual\ptcsDynamicNugetJournalAcl2\pcsl_wz_terry_20260702_01
```

Browser Playwright command passed on 2026-07-02：

```powershell
dotnet fsi --exec G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Scripts\verify.aclLoginBrowser.playwright.fsx -- --port 0
```

Formal service deploy proof passed on 2026-07-02：

```text
release=live81-82-ptcs-beta70-dynamic-beta60-open-acl-login-assetfix-202607021416
service=PulseTradeCommSpaHumanUi
evidence=G:\PulseTrade.fs\log\20260702\20260702133940.ptcs-acl-login-extension-route-spi.op_log
```

Passed assertions：

- `--if-dyna-port --no-wait` 可使用 free ports 啟動 dual listener。
- admin/WZ 擁有 full rights。
- Terry黑粉可使用 AssTerry allowed actions，但 add-target / DamnWZ send 等 restricted actions 被拒。
- actor echo、PingPong stop/re-register、actor reuse after stop 通過。
- production-SQL mode 使用 encrypted SQL file/key args，不輸出 plaintext secret。
- wrong-password login 與 disabled-terry login 都回 401。
- browser Playwright verifies admin/Terry local-login cookies through the open Login provider, ACL capability UI, Dynamic FormInput send/reply, live ActorFabric echo, client-extension manifest/script asset loading, active Login extension renderer, and active ACL snapshot observer/capability provider with exact PTCS beta70 / Dynamic beta60 / Spa.ACL alpha10 / Spa.Login alpha12 packages. The latest startup/package gate uses PTCS beta71 / Dynamic beta61 / Spa.ACL alpha11 / Spa.Login alpha13; browser rerun on that set remains pending.
- formal service verifies public 81 OAuth redirect, loopback 82 SQL local login, HttpOnly session cookie, `/acl/api/snapshot`, and direct Spa.ACL/Spa.Login script marker fetches from the extracted package set.

Remaining assertions：

- transitional fallback cleanup removes dead PTCS core Login/ACL browser behavior after downstream consumers migrate。

### DYN-VFY-009B ACL2 NoLogin PFCF prototype target

狀態：Pass。

Verifier：`src\full.nuget.journal.ACL2.NoLogin.fsx`。

目的：

- 讓 GitHub-only NoLogin script 能在不引用正式 PFCF package 的情況下，提供可被 PTCS.Dynamic FormInput 解析的 `PFCF_AKKA_CMD_FOR_ProtoTyping`。
- 驗證 target key `[ actorAddress; "pfcf-akka-cmd-prototyping"; canonicalArgString ]` 可被 backend Dynamic resolver 解析。
- 保留 `ParseResults<PFCF_AKKA_CMD_DATA_RANGE_FOR_ProtoTyping>` 的 `datarange` tail ordering。

Command passed on 2026-07-03：

```powershell
dotnet fsi --exec .\src\full.nuget.journal.ACL2.NoLogin.fsx -- --if-dyna-port --no-wait --demo --pcsl-root C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\.pcsl\verify.pfcf.nologin.20260703_0913 --delivery-profile nologin-pfcf-20260703 --actor-name nologin-pfcf-echo
```

Passed assertions：

- `pfcf-akka-cmd-prototyping` template registration is active.
- canonical arg string parses and rebuilds exactly.
- backend Dynamic resolve returns parsed cases only: `PFCFEDX`, `PFCFGTCCONF`, `TO`, `ParentChilds`, `BBA`, `DecimalQuote`, `Round`, `DataRange`.
- `PFCFEDX.mode` default is `trivial`.
- `PFCFGTCCONF` list defaults are `OIInf`, `TAIFEX`, `FillSquareCombine`, `OrderByTXDT`, `CathayBKTaifexFill`.
- NoLogin remains GitHub OAuth only; PTCS.Login package and local username/password login stay disabled.

### DYN-VFY-009C ACL2 NoGithubOAuth local-login variant

狀態：Pass。

Verifier：`src\full.nuget.journal.ACL2.NoGithubOAuth.fsx`。

目的：

- 以 `src\full.nuget.journal.ACL2.NoLogin.fsx` 的 Dynamic/PFCF prototype path 為基礎，但移除 GitHub OAuth listener。
- 驗證同一套 ACL2 local-login / ACL policy / PTCS.Login extension path 可獨立在 82 port 使用。
- 確認腳本不讀 GitHub OAuth client id/secret，也不啟動 GitHub OAuth host。

Command passed on 2026-07-03：

```powershell
dotnet fsi --exec .\src\full.nuget.journal.ACL2.NoGithubOAuth.fsx -- --if-dyna-port --no-wait --demo --pcsl-root C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\.pcsl\verify.nogithub.local-login.20260703_1027 --delivery-profile nogithub-local-20260703 --actor-name nogithub-local-echo
```

Passed assertions：

- `--if-dyna-port --no-wait` 可使用 free local-login port 啟動 single listener。
- fixed-port mode 預設只使用 82 local-login listener；無 GitHub OAuth listener。
- local sys-admin / Terry login 皆可建立 session cookie。
- ACL matrix 保持 WZ/sys-admin full rights 與 Terry黑粉 restricted rights。
- protected HTTP differences、PTCS.Login / PTCS.ACL extension script assets、Dynamic bundle markers 皆通過。
- PFCF prototype target key `pfcf-akka-cmd-prototyping` 保持可解析。
- PingPong stop filtering 與 Echo fixed-name reuse 通過。
- live-host mode without `--no-wait` skips the startup `ActorArgu.sendDurableAsync` server probe. The Durable ActorArgu write proof remains part of explicit no-wait verifier mode only.

## DYN-T-533 PTCS beta79 platform provider compatibility

- Dynamic package dependency is exact `PulseTrade.Comm.Spa [0.2.5-beta79]`.
- Existing package tests remain `18/18` green without changing renderer semantics.
- Cross-repo notes/00508 E2E uses Add Actor Key with an RN.Host native `fCell2<string>` actor; the valid SDUI JSON reply is rendered as Canvas.

## DYN-T-534 NuGet bundle discovery and inbound Canvas classification

- Server extension resolves client bundles from local `wwwroot/js`, NuGet `content/wwwroot/js`, and `contentFiles/any/net10.0/wwwroot/js` in that order.
- Renderer returns no Canvas for outbound-only `argu msg:` history, while direct SDUI JSON and `replied msg:` SDUI remain renderable.
- Release package tests pass `19/19`.
- Playwright MCP split-node E2E sends the notes/00508 JSON through PTCS WebSocket to RN.Host `fCell2<string>` echo and observes `RN JSON ECHO E2E PASS 20260710` inside Canvas with zero console errors.

## DYN-T-535 PTCS beta80 exact dependency alignment

- Dynamic package version is `0.1.3-beta73` and its nuspec dependency is exact `PulseTrade.Comm.Spa [0.2.5-beta80]`.
- Release build/package tests and PTC bundle verifier must retain beta72 bundle discovery, outbound-only `argu msg:` Canvas suppression, direct/inbound SDUI rendering, and current target schema.
- This is dependency-only alignment; it does not claim PTC ActorArgu production provider or service deployment completion.

## DYN-TA-T-016/017 canonical static payload classification

- Dynamic beta74 classifies valid legacy/explicit Canvas、FormInput、ActorsPage and `sdui-runtime.v1` without token-search heuristics。

### DYN-T-536 PTCS beta86 package alignment

- Dynamic `0.1.3-beta75` and Dynamic.Ptcs `0.1.0-alpha6-win2` must restore exact `PulseTrade.Comm.Spa [0.2.5-beta86]` without NU1608.
- Existing Dynamic package tests must remain green; this dependency-only alignment must not change SDUI classification, renderer behavior, or transient wire contracts.
- Result: Pass on 2026-07-12. Dynamic package tests passed 23/23 with `WebSharperRunCompiler=false`; preexisting generated bundle/test-project files retained identical SHA-256 content. Both packages restored exact PTCS beta86 and NuGet push returned Created.
- Unrelated/missing schema is `NonSdui` so a host without Dynamic ownership may keep its fallback；present SDUI with unsupported protocol/surface or invalid ActorsPage document type returns an explicit `InvalidSdui reasonCode`。
- Package tests pass `23/23`。This is a non-UI contract gate；browser absent/present-invalid rendering remains open and must not be inferred from these tests。

## 2026-07-11 DYN-TA-T-001..020 Transport-Neutral TA Canvas Test Plan

Canonical matrix：`doc/TAResearch/Test.md`。`DYN-TA-T-000A..020`已accepted並進入執行；Contracts/reducer/source、renderer shared cursor/local interaction與desktop/mobile geometry已有Pass，PTCS transient真host、E2EQ parity與bounded soak仍依matrix追蹤。不得以fake component fixture取代兩條真host E2E。

## 2026-07-13 DYN-TA-T-021 Bootstrap presentation

- `Document=None`且沒有terminal error時不得顯示`TA workspace document is not available`；依channel狀態顯示Preparing、Connecting、Loading、Retrying或Resyncing。
- 只有nonrecoverable `LastError`可呈現terminal unavailable；last-good document與FormInput不得因transient bootstrap/error消失。
- Renderer exact-package tests通過13/13；Dynamic.Ptcs與Ptcs.Client各7/7，root Dynamic package tests 23/23。formal 82 beta89不再於正常bootstrap顯示terminal unavailable，但page-level auto-mount仍由PTCS RFC-0020下一slice修正。

## 2026-09-14 DYN-T-537 Dynamic.Ptcs dependency alignment

- `PulseTrade.Comm.Spa.Dynamic.Ptcs 0.1.11` and `PulseTrade.Comm.Spa.Dynamic.Ptcs.Client 0.1.19` restore exact `PulseTrade.Comm.Spa [0.2.19]` and `PulseTrade.Comm.Spa.Dynamic.Contracts [0.1.10]`.
- Server adapter suite passed 13/13；client suite and LiveDemo build cover the package consumers；package pushes returned `Created`；no source/runtime behavior changed.

## 2026-09-04 DYN-TA-T-056..064 Notebook TA Workspace production

Canonical matrix：`doc/TAResearch/Test.md`。DYN-TA-T-056..061與066已完成RFC/current-state鏈、generic source envelope codec/validation/reducer、owner dependency gate、editor/action lifecycle及atomic retention/resync；Contracts `16/16`、Renderer `22/22`、Contracts/Renderer/Interactive.Client full WebSharper rebuild通過。Daedalus production consumer只採Contracts alpha17 / Renderer alpha39 / Interactive.Client alpha10 exact `FSharp.Core [10.1.400]`；shared cursor/range、application lifecycle、Notebook adapter與real MDCQ DIB/Playwright仍依DYN-TA-017追蹤，不以synthetic M12取代。

## 2026-09-21 Generic Marker Overlay

| Test ID | Scope | Required cases | Status |
| --- | --- | --- | --- |
| DYN-T-538 | RFC/traceability | owner/non-owner boundary、exact public/wire/limits、failure/version/geometry/package matrix | PASS |
| DYN-T-539 | Contracts/codec | marker/bucket round-trip、order、missing/unknown field、UTC、enum/color、label/tooltip/bucket/series/frame limits、v1/v2 | PASS；Contracts 27/27 |
| DYN-T-540 | Reducer/model | valid snapshot/patch、clear、move、duplicate id、target composite/split candle、missing data、last-good、RejectFrame vs RequestResync | PASS；Contracts 27/27 |
| DYN-T-541 | Renderer | above/below stack order、fixed lane bounds、edge clipping、visible-only nodes、tooltip order、Y-domain/數值圖例 unchanged | PASS；Renderer 30/30 + desktop/mobile Playwright |
| DYN-T-542 | Browser/client/cache | v1 non-marker accepted、v1 marker rejected、v2 marker accepted、unknown kind fail closed、cache schema 2 | PASS；PTCS 14/14、Ptcs.Client 16/16、Interactive 4/4 |
| DYN-T-543 | Owner integration/performance | exact package closure、aggregate regression、visible marker geometry、mobile clipping、3,820-bar cursor cadence、Host active consumer build | PASS；六包push Created，aggregate 24/24，200 transitions總3597ms/max54ms，PTCS Host Release build通過 |
| DYN-T-544 | Consumer integration | SPAA/DIExt同一runtime v2 frame與Notebook真路徑呈現相同marker | EXTERNAL；Daedalus owner升版與驗收 |
| DYN-T-545 | RFC/current-state | feedback §2–§5、owner boundary、DMI mapping、wire/cache/lane/interaction/release決策 | PASS；RFC Accepted / DEV Authorized |
| DYN-T-546 | Contracts codec | v2 five shapes strict round-trip；v1 arrow above/below映射Down/Up且re-encode v2 | PASS；Contracts 29/29 |
| DYN-T-547 | Contracts rejection | v2 `arrow`／unknown shape／unknown marker version structured reject，last-good不變 | PASS；structured codec/reducer rejection cases通過 |
| DYN-T-548 | Renderer geometry | Above/Below × TriangleUp/Down四組方向獨立 | PASS；Renderer 31/31及browser DOM geometry gate |
| DYN-T-549 | Hollow interaction | `fill=none`、stroke=color、full hitbox；tooltip與shared cursor同slot共存且不重建candle series | PASS；desktop/mobile F# Playwright，console/page error 0 |
| DYN-T-550 | Wire bucket bound | mixed-anchor bucket 4 accepted、5 rejected，last-good不變 | PASS；explicit mixed-anchor order/limit cases |
| DYN-T-551 | Cross-trace stacking | 同target/position/anchor跨trace lanes唯一且依document→bucket order | PASS；aggregate renderer model lanes deterministic |
| DYN-T-552 | Aggregate lane bound | 跨trace aggregate 4 accepted、5以`limit-marker-lane`拒絕 | PASS；candidate validation與last-good case |
| DYN-T-553 | Cache migration | schema 2 miss/resync；schema 3 rehydrate後paused-for-resync；v1 durable payload可讀 | PASS；PTCS 14/14、Ptcs.Client 16/16、Interactive 4/4 |
| DYN-T-554 | Runtime invariant | marker-only patch不改candle refs、Y-domain、numeric legend、time slot或cursor hot path | PASS；Renderer model/unit與3,820-bar browser cadence |
| DYN-T-555 | Owner browser/client/package | desktop/mobile PTCS browser-demo／Interactive Client exact graph與typed minimal runtime v2 frame | PASS；owner 118/118、六包NuGet Created、Host/E2EQ/GW consumers通過；不含Daedalus真FSSTL/TradeCore run |
| DYN-T-556 | Scheduled renderer | async preparation與sync結果等價；stale generation不commit；row mount/refresh逐frame | PASS；Renderer 37/37 |
| DYN-T-557 | Per-row adaptive axes | 60K/4000日期、5K/300小時、每列axis、窄viewport無重疊 | PASS；model、F# Playwright與Playwright MCP |
| DYN-T-558 | Plot-bounded crosshair | 每列crosshair覆蓋current plot bounds，300 transitions不重建chart | PASS；F# Playwright geometry/cadence |
| DYN-T-559 | Progressive coverage model | query boundary、Earlier/Later、prepend event-time reanchor、stale/反方向intent保護 | PASS；Renderer 37/37 |
| DYN-T-560 | Progressive coverage browser | 3820→4220、visible<=4000、Max 4000、overview/callback/console gate | PASS；F# Playwright與Playwright MCP |
| DYN-T-561 | Main-thread/package release | owner phases無 >100ms task，cursor p95<125ms/max<400ms，exact graph一致 | PASS；phase max 62.25/53.78/34.56/70.34ms，cursor p95=72ms/max=165ms |
| DYN-T-562 | RFC/current-state | Inline marker feedback與cursor timestamp feedback合併為owner RFC；domain／presentation boundary、DOM／格式、失敗與performance gate可追溯 | PASS |
| DYN-T-563 | Marker visible label | BUY/SELL entry/exit文字、相鄰區間collision lane、left/right clamp、row-edge反向展開、blank/long、tooltip/hitbox/Y-domain invariant | PASS：Renderer 39/39。 |
| DYN-T-564 | Row cursor/data window | 1K/coarse row-local timestamp、SVG外固定CSS-pixel top gutter／兩行tag、candle OHLCV與line/hist值同源、missing unavailable；row resize前後除top/left外geometry與computed style不變 | PASS：F# Playwright以CDP computed style及bounding box驗固定92×28 tag、32px gutter、日期／時間與O/H/L/C/V。 |
| DYN-T-565 | Browser/package | desktop/mobile、4,000 bars、300 pointer transitions、無request/rebuild/>100ms owner task、exact package與consumer handoff | PASS（owner）：F# Playwright及immutable `Renderer 0.1.40 / Interactive.Client 0.1.33 / Ptcs.Client 0.1.56`官方套件readback與clean nuget.org-only gates通過；consumer真SPAA rerun由Daedalus持有。 |
| DYN-T-566 | Canonical event-time RFC/current-state | axis-vs-series ownership、legacy compatibility、renderer/interval邊界及package graph可追溯 | PASS：RFC-0023與TA current docs完成。 |
| DYN-T-567 | Contracts event-time codec | optional UTC round-trip、legacy missing、invalid/non-UTC rejection | PASS：Contracts 31/31。 |
| DYN-T-568 | Renderer event-time presentation | candle/line/timeline/marker/cursor與completed/forming例子 | PASS：Renderer 40/40；same-position preview event-time更新不改topology signature。 |
| DYN-T-569 | Regression/package/consumer | interval semantics不變、exact graph、browser與SPAA handoff | PASS（owner）／consumer pending：exact graph、package readback、fresh-cache與browser gates通過；Daedalus真SPAA gate待final graph重跑。 |

## 2026-09-24 Backtest Presentation UX

| Test ID | Scope | Required cases | Status |
| --- | --- | --- | --- |
| DYN-T-570 | RFC/traceability | generic/domain boundary、public names、wire/options、limits、failure、atomic boundary、package graph及consumer handoff | PASS：RFC-0024與current docs完成。 |
| DYN-T-571 | OverviewStripe codec/contract | strict round-trip、unknown/missing fields、UTC、color/width/label/tooltip、options、target kind、axis/position、duplicate id及bucket/dataRef/frame limits | PASS：Contracts 32/32；WebSharper full build通過。 |
| DYN-T-572 | Runtime candidate atomicity | 單patch三個ReplaceDataRef全commit；第二／第三ref invalid時全不變；RejectFrame vs RequestResync、revision/last-good不變 | PASS：Contracts 32/32覆蓋invalid stripe candidate保留revision/last-good；single candidate commit。 |
| DYN-T-573 | Marker wire/glyph split | 4、5、64 accepted；65 structured denial；前4 glyph＋`+N`、stable order、Enter/Space/Arrow/Escape、focus與完整逐筆tooltip | PASS：Renderer 43/43；browser以66 wire markers驗三個bucket、dense bucket前4＋`+60`、Enter/Arrow/Escape。 |
| DYN-T-574 | Navigator renderer | single trace full-height、same-X multi-trace vertical lanes、same-trace多id/count、exact event-time X、close sampling隔離、drag handles不被攔截 | PASS：unit驗collapse/lane，browser驗signal/fill兩條batched path、same-X不同lane、canonical X、tooltip及drag coexistence。 |
| DYN-T-575 | Row resize/layout | candle/scalar default、min/max、pointer capture、rAF preview、keyboard、reset/double-click、scenario retain、reload reset、unmount cleanup、無marker高度特例／空band | PASS：unit驗height policy；browser驗keyboard/pointer/double-click、same-canvas replacement retain、canvas replacement/reload reset及console 0。 |
| DYN-T-576 | Browser/performance/package | desktop/mobile、4,000 slots＋dense bounded events、selection/render無>100ms task、pointer不全掃／不request、exact graph、bundle及official package readback | PASS（owner）：4,000-slot F# Playwright PASS；renderer phases >100ms=0，300 cursor transitions max104ms；five-package exact graph、manifest/nuspec readback及NuGet push完成。 |
| DYN-T-583 | Snapshot chunk encoder | start/N item/commit、header Data empty、stable order、non-Snapshot legacy singleton | PASS：Contracts 34/34；deterministic order、frame flatten與256 data-ref hard limit。 |
| DYN-T-584 | Packet staging success／atomicity | complete batch只publish一次且只在commit後觸發accepted lifecycle／取得cache eligibility | PASS：Interactive.Client 12/12 metadata／single-candidate gates與production lifecycle Playwright initial commit。 |
| DYN-T-585 | Packet staging rejection | missing／duplicate／out-of-order／batch/count mismatch／invalid item保留last-good、不觸發accepted lifecycle並request resync | PASS：unit涵蓋premature commit、duplicate、out-of-order、batch/count mismatch；production Playwright注入out-of-order與malformed `SduiValue`，兩次皆保留last-good且各只要求一次snapshot。 |
| DYN-T-586 | Generation／disconnect | stale socket generation與中斷batch不得覆蓋新state或建立第二個pump | PASS：unit stale-generation gate；lifecycle Playwright斷線期間保留last-good、active/max transport皆1、replacement snapshot後才切V2。 |
| DYN-T-587 | Production Client integration | OnMessage只enqueue；legacy與chunked path共用canonical reducer，reconnect full snapshot正常 | PASS：LiveDemo initial/reconnect皆用`encodeFrames`；Playwright lifecycle gate通過。 |
| DYN-T-588 | Browser/cache/package hard gate | 4,000 slots／28 refs／5 candle rows cold+replacement無>100ms target task；cache cold/cache、focused suites、fresh official exact graph | PASS：17 packets；official exact Renderer browser replacement max 92.04ms、cursor main-thread max 74.73ms、All/marker/document/progressive max 49.30/79.15/40.12/57.14ms，全部over100=0。Contracts/Renderer/Interactive/Dynamic.Ptcs/Ptcs.Client為34/45/12/14/16全通過，三組browser gate與official signature/readback通過。 |
| DYN-T-577 | Consumer integration | Signal/Order/Fill mapping、A→B→A及out-of-order、完整candidate single publish、failure保留A、真SPAA與fresh `.dib` parity | External／Daedalus owner |
| DYN-T-578 | Same-topology OverviewStripe refresh | 同document/topology先清空再補Signal／Fill；navigator path 2→0→2，chart render sequence與ready-row count不變；重跑4,000-slot／cursor／replacement long-task gate | PASS：F# Playwright全通過；300 cursor transitions p95 55ms/max 112ms，renderer各phase over100=0，chartRerender=false。 |
| DYN-T-589 | Renderer hotfix owner gate | 32px gutter resize invariant；line bucket extrema順序；finite Y-domain；shared-cursor click/commit不得重做raw projection；cold/All/selection/document/progressive與console | PASS：Renderer 46/46、Interactive 12/12、Ptcs 16/16；4,000-bar five-candle/click/All/selection/document/progressive max=`78.56/8.49/49.05/58.30/27.23/40.63ms`，全部over100=0，無NaN console error。 |
| DYN-T-590 | 真SPAA hotfix acceptance | exact graph升版後，同一3,820-bar workload重跑cold、48→All、selection replacement、cursor click及正式E2E | EXTERNAL：Daedalus owner；已以COMM `msg-fsi-2c99c905dfe848faa1b3b34312d370c7`交付。 |
| DYN-T-591 | Five-candle prepared geometry indexing | incomplete candle仍省略；重複reference timestamp保留first slot；indexed marker placement與legacy語意相同；4,000×28×5 replacement及All/marker/document/progressive無>100ms | OWNER PASS：Renderer `0.1.57` 46/46、Interactive.Client `0.1.49` 12/12、Ptcs.Client `0.1.71` 16/16；browser phase=`67.33/38.53/61.41/30.08/50.28ms`、over100=0，cursor main-thread max22ms。Local candidate已交Daedalus；真SPAA consumer gate pending。 |
| DYN-T-592 | Selective temporal replacement validation | non-temporal replacement不掃全graph；temporal series replacement仍驗revision／position／shape；axis replacement仍驗全部dependencies；mixed axis upsert＋series replacement採candidate authority；descending/duplicate維持`invalid-temporal-series` | OWNER PASS：Contracts 34/34、Renderer 46/46、Interactive 12/12、Dynamic.Ptcs 14/14、Ptcs.Client 16/16。Final identity graph的4,000-bar scenario overlay 77.99ms、所有acceptance phase over100=0。真SPAA 3,820 selection functional gate PASS；local exact graph `0.1.25 / 0.1.61 / 0.1.53 / 0.1.48 / 0.1.75`待consumer identity確認。 |
| DYN-T-593 | Pure assembler round-trip | `encodeFrame`輸出的normal與zero-item snapshot可由純Contracts API還原相同canonical frame | PASS：Contracts 41/41。 |
| DYN-T-594 | Framing negative | partial、duplicate、out-of-order與trailing packet回stable structured code及global packet index | PASS：Contracts 41/41。 |
| DYN-T-595 | Incremental generation | 另一transport generation不得接續active batch | PASS：Contracts 41/41。 |
| DYN-T-596 | Mixed ordered stream | legacy frame＋多個chunk batch維持原順序，consumer不需自行切batch | PASS：Contracts 41/41。 |
| DYN-T-597 | Global stream negative | partial、interleaved legacy與orphan item回整條wire的global index | PASS：Contracts 41/41。 |
| DYN-T-598 | Active-batch error priority | wrong schema、wrong kind、malformed JSON在legacy fallback也失敗時保留原chunk error | PASS：Contracts 41/41。 |
| DYN-T-599 | Framing／canonical validation boundary | incremental commit產生candidate；machine `finish`仍拒絕invalid canonical frame並定位commit packet；browser不在commit RAF重複full validation | OWNER PASS：Contracts 41/41；4,000-bar browser five-candle/scenario/All=`67.28/79.81/42.76ms`，all acceptance phases over100=0。 |

## 2026-09-25 Runtime Projection Commit

| Test ID | Scope | Required cases | Status |
| --- | --- | --- | --- |
| DYN-T-600 | RFC/current-state | action ACK、reducer acceptance、projection與paint邊界；generic/domain owner；public names、failure與package gate可追溯 | READY：RFC-0026及TA current docs已建立。 |
| DYN-T-601 | Contracts | receipt create/validation；identity match/mismatch；revision threshold；positive monotonic sequences | PASS：Contracts `42/42`，含invalid sequence/revision、identity及revision threshold。 |
| DYN-T-602 | Renderer model | same-topology及full mount全部rows完成後只commit一次；zero-row；tuple dedupe | PARTIAL PASS：pure gate驗current candidate與tuple dedupe；Renderer `47/47`。rows-complete/zero-row browser evidence待DYN-T-605。 |
| DYN-T-603 | Renderer lifecycle negative | rapid A->B、stale generation、reject/resync、cancelled row work、dispose不發布 | PARTIAL PASS：pure gate驗superseded、stale與current-state mismatch；dispose/browser negative待DYN-T-605。 |
| DYN-T-604 | Interactive client | stable root attrs、typed current/subscription、bubbling event detail相同；late subscriber；new application清舊watermark | IMPLEMENTED／browser pending：full WebSharper build通過；typed handle與root edge＋level已編入bundle。 |
| DYN-T-605 | Browser/performance | 4,000 slots replacement、full mount、same-topology、rapid replacement、dispose；event在下一paint boundary；console/page error 0且owner phases無>100ms | Pending |
| DYN-T-606 | Package/consumer | exact local graph、fresh-cache owner gates、Daedalus fresh-kernel identity/revision wait；通過後才public push/readback | PARTIAL：local exact graph `0.1.28 / 0.1.65 / 0.1.56`完成；未public push，consumer gate pending。 |
| DYN-T-607 | Visible per-row Y-domain | 每個K-bar row只取當前viewport projected candlesticks與同row projected TA lines；遠端source extrema、其他row及不可見points不得壓縮可見K棒 | OWNER PASS：Renderer 48/48；pure regression以遠端low control證明visible domain不受完整source污染。 |
| DYN-T-608 | Fixed CSS padding / browser regression | default及resize後SVG edge到visible candle/TA extrema維持15 CSS px centerline；48/200/All、pan/zoom、resize、same-topology replacement不remount且owner phase無>100ms | PASS：Release-built `Renderer 0.1.68 / Interactive.Client 0.1.59`；250/720px pure geometry centerline皆15px，真DOM path外緣因1.8 viewBox stroke為12.60px，default／pointer resize／reset一致。Owner 4,000-slot gate PASS；Daedalus真SPAA narrowed／48／200／All／resize逐row全綠，正式consumer pointer p95=42.20ms且cold/cache無>100ms task。早先82.69/78.28ms由同機owner BrowserDemo競爭造成，停掉後診斷28.78ms、正式42.20ms。 |
| DYN-T-609 | Fixed CSS-pixel strokes／default row height | line/SMA authored width夾1–2 CSS px且default／row resize／viewport commit不變；overview左右visual line固定2 CSS px、`#155f73`、pointer-inert；既有handle為transparent rect且drag仍可用；任何resize前全部chart SVG `<=250px`，manual可超過250，reset/reload回capped default | PASS：immutable Renderer `0.1.70` 48/48、Interactive.Client `0.1.61` 12/12與package verifier通過；owner功能gate驗price default=250、manual=282、reset/reload=250。Daedalus真SPAA 1,140 bars驗三項新contract全綠，pointer p95=28.18ms、cold/cache long tasks=0；official package readback通過。 |

| DYN-T-610 | Contracts theme | `ta.plotSurface.theme` light/dark round-trip；absent=light相容；unknown/non-text reject | PASS：Contracts 44/44。 |
| DYN-T-611 | Contracts histogram polarity | positive/negative typed round-trip；兩鍵皆缺legacy；partial/invalid/kind-mismatch reject；不做name inference | PASS：Contracts 44/44。 |
| DYN-T-612 | Renderer geometry | Histogram value>=0與value<0分成兩個batched paths；legacy單色、same-topology update與DOM bounded | PASS：Renderer 48/48；BrowserDemo正負path non-empty。 |
| DYN-T-613 | Browser visual/interaction | overview initial edge與drag後皆2 CSS px；transparent 8-unit hit rect與drag語意不變；dark candle/composite/overview、grid/cursor/axis/legend可讀 | PASS（owner）：F# Playwright 4,000 bars與Playwright MCP檢視通過，console 0。 |
| DYN-T-614 | Package/consumer | exact `0.1.29 / 0.1.71 / 0.1.62` owner gates、4,000-slot performance、Daedalus真SPAA explicit colors/dark screenshot；GREEN後public push/readback | OWNER PASS／consumer pending：Release nupkg、獨立cache exact graph與bundle verifier通過；正確SHA已交Daedalus，未public push。 |
| DYN-T-615 | Initial viewport authority | fresh canvas合法`visibleBars=4000`顯示1..4000；超量clamp；缺少／fraction／NaN／非正數fallback 48；same-application patch不重設user viewport | PASS：pure model與F# Playwright fresh DOM gate通過。 |
| DYN-T-616 | Marker OFI projection | plot無inline marker label；glyph/title保留；24px OFI位於cursor gutter與plot間；single、dense64顯示4＋`+60`、empty slot維持空白高度；超過3,000 points且相鄰slot同CSS pixel時，直接glyph hover精確選accepted marker slot／EventTimeUtc；一般plot axis snap不變；tooltip/排序穩定 | PASS：Renderer 48/48及4,000-slot F# Playwright通過。 |
| DYN-T-617 | Overview palette correction | selection `rgba(203,213,225,.20)`；initial及drag後visual boundaries `#4ade80`／2 CSS px；transparent hit rect width 8及drag語意不變 | PASS：F# Playwright與Playwright MCP live檢視通過。 |
| DYN-T-618 | Exact package/consumer | `0.1.29 / 0.1.76 / 0.1.67` full WebSharper build、focused tests、bundle manifest／dependency／SHA；Daedalus真SPAA clean-cache驗收後public push | PASS：owner 48/48、12/12、package verifier及高密度browser gate通過；final真SPAA 3,563 points兩策略 exact marker/band與OFI通過；兩包public push/readback、repository signature及entry parity通過。 |
| DYN-T-619 | PTCS adapter exact graph | Dynamic.Ptcs exact PTCS `[0.2.46]`／Contracts `[0.1.29]`；Ptcs.Client再exact Renderer `[0.1.76]`；adapter focused suites與LiveDemo full WebSharper build；package/AssemblyVersion/dependencies一致 | PASS：Dynamic.Ptcs `0.1.50` 14/14、Ptcs.Client `0.1.77` 16/16、LiveDemo Release build 0 errors；local identity與official signed nupkg dependency/entry parity通過。 |
| DYN-T-620 | Unified cursor event model | Marker＋OverviewStripe同slot merge、stable event-id dedupe、Marker rich-chip precedence、authored-trace round-robin、`+N`、None／Unavailable | PASS：Renderer `49/49`；dense browser fixture 66 events顯示2 Marker＋2 OverviewStripe＋`+62`。 |
| DYN-T-621 | remove-trace PTCS client wire | `RemoveTaTrace`編成`remove-trace`且保留canvas/row/trace/request/revision；accepted/no-frame與invalid action fail closed | PASS：Ptcs.Client `17/17`。 |
| DYN-T-622 | remove-trace Dynamic.Ptcs round-trip | server transient DTO/action decode/encode、traceId及accepted result round-trip | PASS：Dynamic.Ptcs `15/15`。 |
| DYN-T-623 | OverviewStripe temporal resolution | canonical axis position解析、missing/invalid item、同id marker precedence與stripe navigator保留 | PASS：Contracts `44/44`、Renderer `49/49`。 |
| DYN-T-624 | Browser presentation/lifecycle | dark hollow halo且semantic stroke不變；plot labels=0；data window wrap/no scroll；trace hide/show/remove；last trace收合；Reset復原；desktop/mobile console 0 | PASS：`verify-ta-generic-marker-playwright.fsx` 4,000 bars、48 paths、all=53ms、pointer p95=30.93ms/max71ms；新增composite candle回歸，hide單一trace不影響sibling，show後等待`.First`並恢復全部8條batched candle paths。 |
| DYN-T-625 | Exact graph/performance/consumer | 五包exact graph、bundle manifest、兩套4,000-bar gates；Daedalus真SPAA同步後才public push | PASS：focused `44/49/15/17/12`；renderer verifier 300 cursor transitions max117ms，正式phases over100=0；Daedalus真SPAA 3,563-point全綠。Consumer gate須以`ta-candle-*`選candlestick，batched locator以`.First.WaitForAsync()`後再驗count；五包public push、official signature／dependency／entry parity均通過。 |
## RFC-PTCS-DYNAMIC-0030 row control line gates

| ID | Scope | Acceptance |
| --- | --- | --- |
| DYN-T-626 | Renderer DOM ownership | OWNER PASS：7個authored rows各恰有一條`ta-row-control-line-{rowId}`；row/trace controls只存在於同row line；Marker/OverviewStripe不出現在controls。 |
| DYN-T-627 | Desktop/narrow geometry | OWNER PASS：1440px與390px line固定40px、同row controls同Y band、跨row Y band分離；390px MACD region nowrap且內容只在自身水平overflow。 |
| DYN-T-628 | Lifecycle/package regression | OWNER/RELEASE PASS：focused `49/12/17`、兩套F# Playwright、console/page errors=0、full WebSharper bundle、package verifier、official repository signature及exact dependency readback全綠。CONSUMER PENDING：Daedalus以官方graph執行真SPAA 8-row desktop/640px gate。 |

## RFC-PTCS-DYNAMIC-0031 page display time-zone gates

| ID | Scope | Acceptance |
| --- | --- | --- |
| DYN-T-629 | Contracts codec | OWNER PASS：四種typed zone stable id／label round-trip、unknown拒絕；legacy renderer API固定UTC。Contracts suite `45/45`。 |
| DYN-T-630 | Formatter | OWNER PASS：Summer／winter、DST start/end boundary、UTC+8跨日、invalid canonical timestamp fail-closed。Renderer suite `51/51`。 |
| DYN-T-631 | Renderer projection | OWNER PASS：Axis、row cursor、data window、metadata、Marker／Stripe／OFI tooltip由同一reactive selection切換；canonical attributes不變。 |
| DYN-T-632 | Snapshot→patch/lifecycle | OWNER PASS：4,000-bar browser fixture在CT patch後selection維持；切換不送action、不改loaded bars／viewport／canonical cursor，console/page error 0。 |
| DYN-T-633 | Package/consumer | PASS：exact graph五包已public release，official repository signature及排除`.signature.p7s`後local/official entries parity通過。Daedalus commit `02d3432e`真SPAA驗Backtest全頁time projection與no-fetch/no-run/no-revision/viewport/selection drift，輸出`SMA13X34_DMI_SPAA=GREEN`。 |

## Row barrier／visible cursor resize hotfix

| ID | Scope | Acceptance |
| --- | --- | --- |
| DYN-T-649 | Renderer cursor/row barrier | OWNER PASS／consumer pending：shared hover cursor visible後keyboard resize，7個row label及crosshair在reactive DOM settle 220ms後仍visible、displayed cursor index不變、chart render sequence不變；reader maps只在rows-complete barrier發布一次。舊official graph在真SPAA resize後hidden，須以新graph重跑。 |
| DYN-T-650 | Performance/package/consumer | OWNER/RELEASE PASS／consumer pending：Renderer/Interactive/Ptcs focused `55/15/17`；4,000-bar F# Playwright明確200→All 435.30ms，正式phases無>100ms，300 cursor p95=41ms/max120ms；Interactive package verifier與三包official signature/dependencies/entry parityPASS。Daedalus須以真SPAA 960 bars／8 rows重跑200→All `<=2,000ms`及visible cursor resize；舊official graph 2,065–2,075ms不得視為通過。 |

## 2026-09-29 Latest viewport／row Edit／overview OHLC

| Test ID | Scope | Required cases | Status |
| --- | --- | --- | --- |
| DYN-T-651 | Pending viewport latest-wins | 快速`200 -> All`時local state立即更新；remote最多in-flight＋latest queued；stale completion不覆蓋最後intent | OWNER PASS：state `655.91ms`、rows-ready `734.35ms`，callback只保留latest。 |
| DYN-T-652 | Row Edit geometry | desktop／768／375px長label；label ellipsis＋title；Edit完整位於line內且寬度>=51px | OWNER PASS：375px截圖與geometry gate通過。 |
| DYN-T-653 | Overview OHLC／stripe responsibility | bounded wick/up/down paths、sample<=280；沒有close-only line；只有authored Order／Fill stripes，Signal不進overview | OWNER PASS：BrowserDemo fixture與DOM/path gate通過；consumer真資料待Daedalus。 |
| DYN-T-654 | Cursor event-to-render | 300 transitions；以browser event request到shared render完成量測；host round-trip僅診斷；console/page error 0 | OWNER PASS：p95 `2.00ms`、max `26.00ms`，CDP max task `28.95ms`。 |
| DYN-T-655 | Navigator pointer ownership | `pointerdown`取得capture；SVG root外與iframe外的move/up仍保有preview並只完成一次action；`pointercancel`清draft且不dispatch；fallback不改bounds authority | OWNER PASS：focused F# Playwright四個同document情境為`0->2 / 0->1 / 0->2 / 1->2`；iframe x=80、navigator x=101、release x=68時callback `1->2`、outcome=`request-earlier`、console 0。 |
| DYN-T-656 | Exact graph／consumer closure | Renderer／Interactive／Ptcs focused、完整Renderer gate、package bundle、official signature/dependency/provenance/entry parity；真SPAA須先scroll並重算geometry，drag start hit-test命中overview後，root外release action必須唯一增量 | PASS：official `.104/.96/.105` signature有效、commit=`237236b7...`、exact deps／manifest／entries `7/10/7` diff=0。Focused `55/15/17`、package verifier及功能／幾何全回歸PASS。真SPAA action `3->4`且最終authorityLoaded=1000、visible=1-250、actions=5；先前RED是consumer以off-viewport stale raw coordinates點到空白，不是owner failure。 |

## RFC-PTCS-DYNAMIC-0033 DECIDE_ON overview／open-left gates

| ID | Scope | Acceptance |
| --- | --- | --- |
| DYN-T-657 | Contracts overview authority | 8,000 anchors接受、8,001拒絕；explicit OverviewAxisRef round-trip；coverage observation domain不受overview ordinal延長 | OWNER PASS：Contracts `49/49`。 |
| DYN-T-658 | Cross-axis renderer | 1K detail＋60K overview時selection／stripe依canonical event time；OHLC／OC up綠、down紅、flat neutral；DOM bounded | OWNER PASS：Renderer `58/58`與完整BrowserDemo gate通過。 |
| DYN-T-659 | Provider open-left | range-less Earlier不產生UnixEpoch；wire明列`provider-open-earlier`；非法方向／ordinal／非零寬拒絕，舊缺欄位解`explicit-bounds` | OWNER PASS：Contracts、Dynamic.Ptcs `15/15`、Ptcs.Client `17/17`。 |
| DYN-T-660 | Prepared-row barrier | ready row count未等於authored row count前toolbar／navigator不送boundary outcome；完成後恢復，pending latest-wins不退化 | OWNER PASS：pure `0/2/3/4` rows及browser progressive/document replacement通過。 |
| DYN-T-661 | Browser／performance | 8,000 bounded overview、500 coverage／250 detail、tiny selection、existing pointer/cursor/viewport regressions；正式phase無>100ms task | OWNER PASS：`verify-ta-renderer-playwright.fsx`，progressive max `45.37ms`、over100=`0`。 |
| DYN-T-662 | Exact graph／consumer | 五包source commit、Release build/push、official signature/dependency/manifest/SHA readback；Daedalus真SPAA與fresh DIB | OWNER/RELEASE PASS：commit `f0cd0d3`，五包official signature／exact dependency／entry parity通過；nuget.org-only fresh cache focused=`58/15/15/17`。Consumer真SPAA／fresh DIB pending。 |
| DYN-T-663 | Draft／commit responsiveness | single-step pointermove在250ms內更新preview與selection，callback不增加；pointerup使用最後pending draft並恰送一次action | OWNER PASS：完整F# Playwright通過，drag後`Viewing 4-51`；300 cursor transitions p95 `7ms`、max `26ms`。 |
| DYN-T-664 | Local viewport／overview axis／loaded View All | pending 200 action期間pan可用且立即`3801-4000 -> 3601-3800`；remote不平行送；overview至少三個event-time labels。另以loaded=3022、active=724重現，All title須為3022並以ordinal intent替換為1-3022／selection 100%。 | OWNER PASS：完整F# Playwright通過；3022 fixture的generation=`2`、active detail=`0+3022`、runtime axis/price=`3022`，正式phases over100=`0`。 |
| DYN-T-665 | Exact graph／consumer | 五包exact tests、full WebSharper/browser、official signature/dependency/provenance/entry parity及真SPAA | PASS：owner `49/58/15/15/17`、完整browser、official graph integrity及兩次max4000 persistent SPAA完整run全綠；consumer alpha47已push。 |
| DYN-T-666 | Adjacent cache empty scope | active=`0+724`、domain=`4724`且遠端零長segment=`4724`時，Later在無hit下必須Miss；true boundary empty仍KnownEmpty。IndexedDB seed後reload再readAdjacent亦須Miss。 | OWNER PASS：舊`.112`單元穩定RED為KnownEmpty；`.113` unit `15/15`，persistent BrowserCache Playwright=`ADJACENT:LATER:MISS`，既有large rehydrate/LRU gates全綠。 |

## RFC-PTCS-DYNAMIC-0034 Navigator overview integrity

| ID | Scope | Acceptance |
| --- | --- | --- |
| DYN-T-667 | Contiguous overview aggregation | 8,000 source points compact至<=280 contiguous buckets，source range無gap/overlap且首尾完整；bucket保留first O/max H/min L/last C與volume sum；單點spike不得消失。 | OWNER PASS：Renderer exact-package suite `63/63`。 |
| DYN-T-668 | OC/body-only authority | OC／scalar point與任何mixed-authority bucket的High/Low皆為None；SVG wick只含authoritative OHLC bucket，body polarity與Y-domain仍正確。 | OWNER PASS：pure aggregation及BrowserDemo驗99 authored wicks＋1 body-only anchor。 |
| DYN-T-669 | Pointer and edge interaction | left/right=`ew-resize`、selection=`grab`、active move=`grabbing`；pointerup一次commit。Start/Latest以既有typed intent切exact loaded head/tail，invalid projection不送action。 | OWNER PASS：完整F# Playwright驗cursor、single commit、Start/Latest及revision/generation逐intent單調前進。 |
| DYN-T-670 | Browser/performance/package/consumer | 完整WebSharper與F# Playwright無console/page error，正式phase無>100ms；Renderer/Interactive/Ptcs exact package與official readback通過，再由Daedalus真SPAA驗overview extrema、drag及edge controls。 | OWNER/RELEASE PASS／CONSUMER PENDING：source `97cb224`、official graph `.123/.115/.123`；repository signature、exact dependency、manifest、entry parity及nuget.org-only fresh-cache `63/15/17`通過。Daedalus真SPAA待完成。 |
| DYN-T-673 | Global loaded-domain draft | loaded=900、active=`700+200`時selection、pointer delta、preview皆使用900 domain；40px／12 steps於250ms內改變global draft，callback與chart render sequence不變；release只送一個existing typed intent。 | OWNER PASS：pure exact-package test與完整F# Playwright通過；500/250 fixture先出現changed Preview，再切至`1-250`。 |
| DYN-T-674 | Pending rebase／resync／iframe | 48與250寬global target在PollInFlight、PausedForResync、兩者交錯及iframe外release皆進single latest queue；settle後以最新revision/generation重建，callback不平行且最後為`1-48`或`1-250`。 | OWNER／RELEASE PASS：focused F# Playwright五情境callback=`0->2 / 0->1 / 0->2 / 0->1 / 0->1`，QueryGeneration=2，console 0；official `.127/.119/.128` signatures與NuGet.org-only `64/15/17`通過。真SPAA pending。 |
| DYN-T-675 | Rendered preview／release exact parity | loaded=900、active=200、40px/12-step move後，250ms settled畫面Preview的1-based start/count必須與唯一wire coverage ordinal/count及authoritative final完全相同；不得使用±N tolerance。明確跨loaded boundary仍須落到`1-count`，pending/resync/iframe single-action不退化。 | OWNER／RELEASE PASS；CONSUMER PENDING：official `.131/.123/.132`下Preview=`675`、wire ordinal=`674`、count=`200`、final=`675-874`。NuGet.org-only focused=`65/15/17`；pending/resync/iframe五情境與完整F# Playwright PASS，正式phase over100=0。`.130`因stale repository commit已retired；待真SPAA exact equality。 |
| DYN-T-681 | Semantic gesture ownership and reactive replacement | 4,500 resident／約1,000 visible的left/right/body mode在pointerdown後固定；reactive SVG replacement不得清除active gesture；left只改左界、right只改右界、body保寬。Resident draft沿用timestamp overview mapper；跨resident boundary仍送一個既有typed intent。 | OWNER／PACKAGE PASS，CONSUMER IN PROGRESS：source `10b3fe99ad729f6b1045d422e8e2dd624bde2315`；Renderer final-package Expecto `68/68`；owner BrowserDemo Expecto Playwright `1/1`，Preview=`431`、wire ordinal=`430`、count=`200`、actionCount=`1`、console error=`0`。Contracts `0.1.36`、Renderer／Interactive.Client `0.2.3` NuGet.org signature、commit、exact dependencies與DLL parity通過；Daedalus正式18884 gate已解除package等待並執行中。 |
| DYN-T-682 | 0.2 navigator compatibility-line PackageReference audit | 對仍釘Contracts 0.1.35或Renderer 0.1.x的Dynamic.Ptcs、Ptcs.Client與Tagging.Client逐一驗binary/API/wire相容、build與既有Expecto；只有全部通過才同步exact references。 | PENDING／最高優先：本輪只發布新的0.2 owner graph，不把legacy compatibility consumers盲升。 |
| DYN-T-683 | Authoritative frame preserves active and chained navigator gestures | active gesture：pointerdown與first preview move後送入相同coverage identity、較新revision/generation projection，left/right/body/boundary geometry、mode、固定邊或body width不漂移，pointerup只送一個intent。chained gesture：前一remote response仍pending時，下一pointerdown必須沿用使用者當下看到的draft window與selection ratios；前一response到達不得移動新手勢fixed edge或清mode。 | OWNER／RELEASE PASS；CONSUMER PENDING：正式`0.2.5`在Daedalus 18884失敗（left fixed-right error 57.36px、body width error 47.80px、16 stress failures）。本機`0.2.6`仍snap約300px、`0.2.7`剩SVG border換算誤差2.51px，均retired。`0.2.8` Renderer exact-package Expecto `69/69`，BrowserDemo focused Expecto Playwright `1/1`，Preview=`431`、wire ordinal=`430`、count=`200`、actionCount=`1`。Renderer official SHA=`FF033B564C6C3202D8C313B9D341DC9C88A4F21C4D710534112764B96A0820E1`；Client active-SDK local SHA=`6678E76D7B5C3590A698AE65AC19E26AA0F4B03A112CCACC418348091E5A8EE2`且NuGet remote已接受。待Daedalus正式重驗18884。 |
| DYN-T-684 | Global resident strip drop-only owner gate | GlobalOverview=None維持既有DOM；Some時global strip位於resident navigator上方，只允許move、無resize handles。pointermove只改local draft且callback count不變；pointerup恰送一個ResidentCoverageChanged，matching projection收斂draft；resident count與inner viewport width不變。 | OWNER PASS／FORMAL CONSUMER PENDING：Contracts51/51、Renderer71/71；Contracts0.1.38、Renderer/Client0.2.10 fresh local-first packages完成。BrowserDemo package-consumer build PASS；Expecto Playwright 1/1 PASS，既有resident preview/release actionCount=1，global relocation start=3571 count=1500。D哥18884 formal fixture尚未回報，不得標整體完成。 |

## DYN-T-676 / DYN-WBS-581
Baseline三producer0errors/umbrella既有WS9002warning；newexactSpa48 versions finalbuild/DLLsourcehash/localnuspec consumer/fullHost與officialsignature/readback未執行。Existingupstream typed6及realHTTP-PCSL/browser不是Dynamic新package PASS；維持Renderer131/Interactive123，不宣稱TA兩RED已修。

Active local package test/demo closure: Ptcs.Tests old[0.1.56]→[0.1.58], PtcsTaClient.Tests old[0.1.132]→[0.1.133], Ptcs.LiveDemo oldSpa46/Ptcs56/Client132→Spa48/Ptcs58/Client133. These known active clients must restore/build against final candidate before PASS; no source test expectations change. No Run of LiveDemo/automatic hostspawn asbuild gate. Legacy sourceACL/Login18 remain excluded.

DYN-T-676 更新：三finalproducer DLL versions58/133/27+292 PASS；三unsignedlocalnupkg metadata/DLLhash/exactSpa48 depsPASS（3/6/5），umbrella4JSassetentriesbytes一致。既有Expecto兩個runner實際15/15與17/17，0ignored/failed/errored，0.372346/0.364855s，sourcebin未使用，consumeroutputDLLhash證pack292及Spa48+83。LiveDemo修assetpath前baseline0errors/1既有WS9002/7.49s；修後build未執行。FormalHost/official/browser整合未宣稱PASS。

DYN-T-676 更新：LiveDemo asset path37→48修正後 fullWebSharper build6.93s/0errors/1existingWS9002 PASS；loadedSpa48+83/hashAF43及7buildassets rawSHA/copy PASS，report C:/Users/Administrator/AppData/Local/Temp/aster-ptcs-package-0.2.48/dynamic-livedemo48.asset-proof.json。未啟demoHost；local unsigned candidate的build/assetproof，不是正式部署/browser/TA semanticfix。

DYN-T-676 revision2 newSpa055 consumer proof：existing exactPtcs58/Ptcs.Client133 runners32/32（15+17，0ignored/errors/fails）＋LiveDemo full compiler/identity/7copiedassets PASS；all three loaded Spa0.2.48+055/DLLCBBE3。Evidence absoluteC:/Users/Administrator/AppData/Local/Temp/aster-ptcs-package-0.2.48/dynamic-consumers055/proof.json、perproject raw stage logs。Selector先猜build/net10.0/wwwroot paths遭guard拒絕；依actual ContentUpdate與既有copy proof修 selector至contentFiles/any/any/build→output/build，原7assets/hash oracle unchanged，successfultests/builds不重跑。RendererWIP／TA first200/SMA／formal/prod 未改。

### 2026-10-03T06:21:53.0300588+08:00 DYN-VFY-050 revision3 official／consumer closure
Dynamic0.1.27／Ptcs0.1.58／Ptcs.Client0.1.133 producer29279539806118e04f1b563d9cb881aadddf53db unchanged；3包HTTP201、officialsignature/source/payload/canonicalhash/網站Dependencies PASS。Existing packageconsumer15/15＋17/17、LiveDemo fullWebSharper/7assets/latestSpa055已驗；official normalrestore assets/contentHash/runtimeDLL再驗PASS，無需重編譯/重跑semantic tests。
MaincanonicalG:/PulseTrade.fs source2f7057／四exact builds／GW496／真三TestHostsSDK/resultrestart／active18signedcache/七consumerPASS。Raw C:\Users\Administrator\AppData\Local\Temp\aster-ptcs-package-0.2.48/official12/readback-20261002205627356/result.json、dynamic-consumers055/proof.json、official-active18-promotion/result.json；detail G:/PulseTrade.fs/Libs/PulseTrade.Comm/doc/GW/WBS.PTCS-PACK-REF-001.md。SDK401feed官方bytes，不稱NuGet-only。正式未部署；Renderer diagnostic WIP不compile/stage，無TAsemantic變更。

## DYN-T-677 / DYN-VFY-050r4
Spa49 metadata cascade：三 full builds → 固定 source 三 archive identity/dependencies/assets → SDK feed same-version collision guard → exact-package consumer 15+17 actual tests → LiveDemo full WebSharper、Spa49 runtime DLL與七資產 raw hash。Renderer WIP、歷史 log、正式服務及既有 archive 不變；新版本 gates 尚待實測，baseline不代替。

DYN-VFY-050r4 pre-checkpoint 新版三 producer full WebSharper/compiler PASS：Dynamic28 11.04s、Ptcs59 9.70s、Client134 8.71s。TEMP/aster-ptcs-package-0.2.48/dynamic-wallet49-baseline-source-r1 保存 865 raw inputs/舊 HEAD + 三 metadata delta proof；其餘 canonical/captured bytes 與 Renderer WIP 不變。兩份既有 umbrella JS/min 為真 compiler Spa49 輸出，依 generated delivery 例外原樣同步，未手改。此批尚未 localpack/consumer/publish。

DYN-T-677 / DYN-VFY-050r4 完成 local scope：三固定source full builds與archive identity PASS；exact-package actual15+17=32/32，0ignored/failed/errored；LiveDemo fullcompiler/三DLLidentity/七Spa assets集合與hash PASS。完整證據與限制見 `doc/Verification.md` revision4；未啟Host/未public publish，不代替上游服務/browser gates。


## DYN-T-678 / DYN-VFY-050r5
Spa50新圖必重做三producer fullcompiler、固定commit archive/DLL/exactdeps/assets、SDKfeed same-version collisionguard；兩個exact-package runner須actual15+17且0ignored/failed/errored；LiveDemo fullcompiler、newSpa50 runtimeDLL與七asset精確集合/bytes/hash相符。只copy真compiler輸出，canonical/captured原bytes與Renderer WIP不變。當前gate pending；Spa49歷史localPASS不代替此輪，Host/browser/正式部署另列。

DYN-VFY-050r5 precheckpoint PASS：短路徑 dyn50-pre-r2 三 full WebSharper 為 Dynamic29 9.453s、Ptcs60 8.270s、Client135 9.368s，0 errors；865 canonical/captured raw SHA與clean blob、Renderer hash不變。先前巢狀root r1 MSB3030（10路徑>260）保留，不修ACL/造假assets。兩份umbrella JS/min只從真compiler原bytes同步，固定source checkpoint/final archives/consumer仍pending。

DYN-WBS-583 / DYN-VFY-050r5 本機範圍完成：producer source `636c19b1bb62f9425591c2d2fdb543de1445a9e5`，三fixed fullWeb 10.865/8.550/8.839s；三archive/exactdeps/DLL/assets與SDK401feed逐hash相符。真15+17=32/32（0ignored/failed/errored），LiveDemo fullWeb8.309s、三consumer runtime Spa50 DLL87569E35…與core f217c898匹配、七asset exact集合/bytes/hash與七ownbundle齊全。公開發布、main Host/browser與正式部署仍由upstream另驗。 詳細來源见 `doc/Verification.md` revision5。

DYN-T-678 / DYN-VFY-050r5 公開收尾（2026-10-03 current）：三 Dynamic 包官方簽章、candidate payload、NuGet 網站 dependency group/ID/exact range 全通過，12 包 closure finalized、pending0。upstream GW525/525、canonical wallet22/22、RN/GW/SPA 隔離 MCP/PCSL restart及native cleanup PASS；本 repo32/32與LiveDemo既有證據不重跑。精確 signed/unsigned hashes及三份upstream結果見 `doc/Verification.md` 公開補充。production未變，producer636c19b不重包。

## DYN-T-679 / DYN-VFY-051r1
真 MSBuild marker unit 必須覆蓋每個專案 20 cases：CLI/VS defaults、明傳專屬 false、true、global false 優先（含 VS auto-true 與 explicit true）、global true 不取代專屬 opt-in、Debug/non-Windows/other Configuration 保持原限制。umbrella 無 VS default且保留 Release/Debug targets；其餘七個 VS auto-true 保留。原 target bodies 全部移除、fixture 無 imports/Exec，source before/after SHA 必須一致。r2 RED 為 8 projects/9 targets/160 executed/34 failed，positive explicit true 8/8 PASS；r1 harness RemoveProperties 失敗獨立保留，不作產品 RED。測試不讀 key/不發布/不建立實際 nupkg，不冒稱 E2E。

DYN-T-679 完成：真 MSBuild RED160/34failed → final PS7及PS5各160/160；9個target、來源hash不變、VS/defaults/Config/OS/Exec/version/reference保留。沒有真 publisher effect；完整證據與既有harness失敗見 DYN-VFY-051r1。

完整追溯：RFC-PTCS-DYNAMIC-0035 / DYN-WBS-584 / DYN-T-679 / DYN-VFY-051r1。


## DYN-T-680 / DYN-VFY-050r6 — DEP52

需求 oracle：7 檔恰 14 metadata 替換，反向替換等於原 bytes，BOM/其他 XML 保留；active exact graph 對齊 .30/.61/.136 → Spa [.52]、Registry [.4]，其他 packages/WIP 不變。新版本需 3 producers full WebSharper、exact PackageReference/DLL/RepositoryCommit/assets 與 immutable archive/feed hash；原 Ptcs.Tests/Client.Tests、新增受影響 umbrella suite 必須有實際 discovered/executed/passed/ignored，LiveDemo 須完整 compiler 與 7 個 core content assets exact bytes。零 tests、僅 exit0 或 source baseline 不等於新版 PASS。

現況：原 source a299348c 的單一 umbrella producer baseline restore 1.894s / build12.712s PASS，865 canonical/captured inputs 與 Renderer 不變。7 metadata 檔 inverse bytes/XML/UTF8/BOM PASS；目前未 build/test/pack 新版，待 root 固定52/4 package proof及候選執行 gate。證據與失敗詳見 Verification r6，沒有 host/production 效果。

### DYN-T-680 actual / local package and consumers

source5267：三producer full WebSharper、865 input freeze、2 generated bundle equality、3exact本機package/feed證據 PASS；umbrella24＋Ptcs15＋Client17=56 actual unit tests PASS，0ignored/failed/errored，LiveDemo full compiler及7coreassets exact bytes PASS。候選/package SourceRevision固定5267，後續文件commit不重包；Registry4維持compile-only/runtime exclusion。原harness失敗與raw parse Correction詳見DYN-VFY-050r6 actual。Public release、下游Host/runtime/production均不由這56unit/producer證據代替。

## REL-01／fixed package新驗收
本輪metadata3producer/10refs不宣稱任何新compile/unit/browserPASS。使用doc/Verification.md既有package/TAclient/browser gates及Main3Hostintegration；須fixedCore53/Registry6/Native401同閉包，actualdiscovered/executed/passed/failed/skipped，三producerWebSharperDLL/WSassetpayloadhash和Host有效載入identity。Renderer131 exactpublished消費，不捕獲其foreignsource，不刪或改assertions。Native/Registry/WS/IFS localartifact已驗不等於本DynamicREADY；publisher/prod不動。

## 2026-10-06 RF-09 package／unit evidence

Source70f5e22：Dynamic0.1.32／Ptcs0.1.63／Ptcs.Client0.1.138完整compiler、pack、nuspec RepositoryCommit、exact Core0.2.54依賴與主DLL hash均PASS；Dynamic bundle7個asset與fresh output逐檔相同。既有PTCS adapter15/15、TA client17/17，共32 executed/passed、0ignored/failed/errored；消費本次本機immutable packages。Core DLL DAF4EB482A15ACB44C237D430512B4229F544A3AE308FEBC4DCE50E365CA1BFE，Mainclosure共12套件仍未publicpublish。Host建置在G:0bytes時中止；RNlocalpublish完成，SPA copy失敗，GW尚未build，正式三Host沒有切換。Renderer WIP保持。使用者自行在https://my-ai.co.in:81/chat登入；工具無可連線visiblebrowser，UI未驗。Evidence：各producer bin/net10.0/agent.aster/core54-20261006/receipt.json、Main temp/agent.aster/20261006-refactor/package-closure-proof.json（28freshassets），主要追溯RF-09 Release.Closure.md。

## 20261006 RF-12 Actors 長路徑 UI slice

Status=InProgress，Progress=20%，開始202610060631，已耗4分鐘，尚需Dev20–35/Test20–30，ETA202610060800(+08)。已在Core54真Host重現1682px內容超出1600viewport；Dynamic五行style與0.1.32→0.1.33 metadata修改完成，尚未build/pack/public/Host驗收。沿Main doc/20261006.REFACTOR/SA.md、SD.md及hypothesis RF12，實測長/短actor地址、窄/寬viewport、controls/focus/scroll；不以source/style文字測試冒充UI。Renderer WIP保持；MainHost單獨Runtime404修正unit87/87，不算本DynamicbrowserPASS。
Core60 consumer32cases planned；此處尚未執行、不標PASS。證據由Main evidence/Core60彙整，來源沿既有兩測試專案。

RF20-Form61：InProgress source33/33PASS (含9新增)，開始202610071912，已耗12分，WS／package／真browser待驗。既有Core60 mode90062→90000 RED保留，測試新label修正empty/null重名。RAW temp/agent.aster/rf20-draft/{build-r2.log,tests-r2.xml}；ETA202610072025整體Main。

2026-10-07T20:02:53.2751699+08:00 RF20-Form61 package/isolated驗收PASS：c9626c5原33assertions消費正式0.1.40與Core0.2.61，33/33；兩consumers15＋17PASS。首次將DLL換入舊Core60testclosure的2ERROR保留temp/agent.aster/core61-consumer/Dynamic.FinalPackage.xml；完整PackageReference r2修正closure後通過，未改assertions。真269xx Form90064→Plain→Form90064、Plain90065→Form90065、native280→360px與模式回復360、820x900 Sendbottom872且真echo90066通過；invalid草稿保留且Form不顯示Send。Main evidence/Core61/Composer.Browser.json；正式切換執行中。

2026-10-07T20:09:16.8792902+08:00 RF20正式Done：PulseTrade.Comm.Spa0.2.61 producer6e05ac13 finalWS1681/1681；公開NuGet12依賴閉合、Host45、Dynamic65PASS。Main release20261007-core61-9c8d9743e三Host已上線；actualTab/echo、nativeText160->90/Form280->360、mode草稿、invalid保留、窄Sendbottom872/900(test)及741/769(public)通過。原task/ACL/protected不變；RF18非零清除與RF10mixed/Vault另追Main。Evidence G:/PulseTrade.fs/doc/20261006.REFACTOR/evidence/Core61/。
