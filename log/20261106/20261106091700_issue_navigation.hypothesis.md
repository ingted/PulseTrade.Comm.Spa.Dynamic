# Load03/04 navigation hypothesis

2026-11-06 09:17 +08:00; root E:/deletable/asteer-load05 branch20261106_003.asteer_load05 base96dc05896cf5ea729b75a03ecdf61fb00ed88ce3. Existing05 changes remain uncommitted in RendererModel/Renderer/Renderer.Tests.fsproj/TAResearchRendererTests; no unrelated revert.

Authority: canonical manual-task03 Questions.md Q03.1–3 answered; task04 accepted RFC-PTCS-DYNAMIC-0033 and RFC-TRADECORE-0031 DECIDE_ON overview contract. Assignment msg-fsi-925c61ba62084380b338b5635ebab4d0 / msg-fsi-1848578be85445e4bcbb1ed08e3ff21c.

## Facts and earliest deviations
- RendererModel.tryMainPlotRangeWindow always normalizes min/max of pointer bounds. Existing DYN-T-746 reverse cases assert that old behavior. This contradicts accepted reverse reset: mouse-down is center, restore up to1500 bars, constrained only by actual resident availability. Forward range remains min/max within captured viewport.
- Renderer uses overview anchors for candles/markers but activeReferenceTimeline for resident date axis and resident ordinal domain for mask/gesture. Audit3622c580 documents this source contradiction; actual overview Frames endpoints still requested from browser owner.
- 12-second delayed jump, scale930K→60K right-anchor and failed outer expansion remain separate hypotheses until actual gesture/state evidence; no shared-root claim yet.

## Slice03-A experiment
Before product patch, run baseline assembly through Expecto with independent example: resident4500, current Start1500 Count600, track1000, mouse-down500 then release100 => target Start1050 Count1500 (center candle1800). Old result is Start1560 Count240. Add parameterized boundaries/invalid/forward cases to existing Renderer.Tests; retire only reverse min/max assertions explicitly superseded by Q03. No source-string assertion used as behavioral proof.
Pseudocode: validate existing finite geometry and nonempty capture; if end<start map mouse-down to captured candle; choose min(maximumCount,resident length) and clamp centered start to resident edges; otherwise retain current forward span. Expected non-type product change <=15 lines. No remote/backend semantics or package version change.

## Compiler blocker
Baseline175/175PASS. Existing05 candidate found unsupported WebSharper String.Equals overload and was corrected. Subsequent compile cannot delete its generated log because shared wsfscservice PID48740 holds DELETE_PENDING handle; coreutils and authorized headless agy both confirmed, standalone retry also crashes before compiling. Owners notified; no shared process termination/ACL changes by asteer.
## Correction and RED evidence (2026-11-06 09:10:02 +08:00)
The header/file timestamp09:17 was an erroneous estimated time; filesystem creation is09:08:25 +08. MCP Expecto execution b81220880b134838ae06115c8838efd1 at09:09:05 ran1, passed0, failed1, ignored0: baseline returned Start1560 Count240 versus independently required Start1050 Count1500. Session asteer-load03-baseline was stopped immediately to release baseline DLLs. Earlier probe calls failed to compile due [] vs array and missing transactional #r context; no product result was claimed from those probes.

## 2026-11-06 11:32 +08:00 Y axis geometry regression
Observed real ES on18883 Renderer0.3.34: 10 quarter-tick labels but top100%/gridy250 for all values; candles still occupy correct price range. Source of truth is normalize(low,high,top,height,value) plus candle callers. Earliest deviation: tickY sends bottom as top and zero top as height. Exact one-line correction restores shared coordinate basis, no change to tick values, normalize, bounds or series. Before evidence: log/20261106/20261106084123.asteer_18883_reverse.png and sticky geometry JSON. Existing Renderer suite must remain green; actual browser label separation awaits next packaged candidate. Product change estimate1line. No host rebuild/restart/versions here.

## Experiment 2026-11-06 overview settlement scope
Nativeoverview fix also requires existingpending event-range settlement toacceptserverActiveDetail: anoverviewstartneednotbeanactivebarstart andResizecountmustbeserverderived. WireNavigatorMove givespreservedcountoracle; querygeneration andviewportgeneration remainexistinggates. This expandsinitial100lineestimate; no secondauthority/statecache added. Boundaryrefill retainsknownactiveedge ratherthanconvertcalendar/overviewdelta toactiveindices. Purecases useindependentfixtures; actualFrame/UI gate stillpending.
