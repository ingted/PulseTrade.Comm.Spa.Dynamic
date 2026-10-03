# PulseTrade.Comm.Spa.Dynamic KM

## WebSharper library Release artifact

- `WebSharperProject=Library` 的 package 必須由完整 WebSharper compile 產生；一般 incremental `Release build` 可能因 `CoreCompile` 判定 up-to-date，而留下不含 `WebSharper.meta` 的 DLL。
- 建立不可變 Release candidate 時使用 `Release Rebuild`，且在 pack／交付下游前以 `Assembly.GetManifestResourceNames()` 確認 `WebSharper.meta` 存在。Contracts、Renderer、Interactive.Client 的 exact package graph應使用乾淨restore cache重建一次。
- 下游發生大量 `WS9001 Type not found in JavaScript compilation` 時，先檢查被參考package DLL的WebSharper metadata與同版號cache bytes；不可先改domain type、關閉WebSharper compiler或沿用舊bundle掩蓋artifact問題。
- Interactive.Client package另執行`scripts/verify-interactive-client-package.fsx`，驗bundle manifest、entry、minified entry與Runtime.js。該gate不取代Contracts／Renderer assembly metadata檢查。

關聯：`doc/RFC/RFC-PTCS-DYNAMIC-0027.dark-plot-histogram-style.md`、`doc/Verification.md`的`DYN-VFY-032`。

## Marker plot 與 cursor detail 分工

- 密集事件的長文字不可直接畫在K bar plot glyph旁；plot只保留shape/color與`<title>`，shared cursor detail放固定高度的row-local band，避免遮蔽、縮放碰撞與row跳動。
- Detail band從accepted prepared placements建立slot index，pointer hot path只更新預建DOM；不可重新decode wire、掃描全部markers、重建chart或送provider action。
- Wire/candidate hard limit與presentation budget分離：現行marker bucket最多64，OFI直接顯示4筆並以`+N`表達其餘；不得因DOM budget丟失stable identities。

關聯：`doc/RFC/RFC-PTCS-DYNAMIC-0028.default-viewport-marker-ofi-band.md`、`DYN-VFY-033`。

## WebSharper reactive list 與 authored display

- F# list computation中用條件式加入Doc時必須明確`yield`；只有expression而未yield可能產生`FS3221`並讓row/trace controls在WebSharper輸出中被丟棄，純model測試不一定能發現。
- 只設HTML `hidden`不足以隱藏帶inline `display`的renderer node。local visibility helper需保存authored display，hidden時設`display:none`，解除時恢復原值；不可一律恢復空字串破壞grid/flex layout。
- 這兩項必須由真BrowserDemo DOM gate驗 controls存在、點擊後geometry/visibility改變，再以Reset/identity replacement驗state lifecycle。

關聯：`doc/RFC/RFC-PTCS-DYNAMIC-0029.trace-lifecycle-cursor-events-marker-contrast.md`、`DYN-VFY-035`。

## 2026-10-03 Spa exact adapter cascade
ExactSpa version consumers must migrate producerpackages before formalHostrestore can be accepted. Metadataonly Ptcs58/Client133/umbrella27 plannedagainstSpa48 whileusingofficialRenderer131; do notcompile renderer diagnosticWIP intoapackage accidentally. DYN-WBS581/T676/VFY050 andsourcecheckpointlog trace finalsource/DLL/local/official distinctions.

Frozenproducer292 localpackages58/133/27與32typedunitconsumer已驗；NuGetRepoCommit/ProductVersion/embeddedDLLhash/actualconsumerDLL都要核對。WebSharper先刪projectdir/websharper.log，TimingLog/Standalone不繞過delete；原ACL受阻時可用有raw/Gitblobmanifest的immutablebuildartifact，不改canonicalsource或停sharedhelper。ExactPackageRef進版同步ContentUpdate/copyassets路徑（LiveDemo37→48）。回鏈DYN581/T676/VFY050及20261003005800log。

### 2026-10-03T06:21:53.0300588+08:00 Acquisition-only與consumer
GenericDynamic27沒有既有七consumer packageedge，acquisition-only exactrestore補cache不當功能測試。三包292與Spa055 functionalpayload／既有clientassets一致，signedraw/canonicalhash區分。DYN-VFY-050r3。

## Spa49 immutable build provenance
Git autocrlf 的 clean blob 與 working raw bytes可不同；capture 同時驗 HEAD clean blob與raw SHA，拒絕custom clean filter，不normalize canonical檔案。Compiler bundle輸出先另存，再只同步實際變動的tracked generated assets，最後固定sourcecommit重編譯/pack。回鏈 DYN-WBS-582 / DYN-VFY-050r4。
