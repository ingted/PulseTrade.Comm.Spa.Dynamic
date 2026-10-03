# Hypothesis: missing global veto in two newly active producers

1. Actor.Registry PostBuildR only checks Release; GW PushGwReleasePackageToNuGet checks Release/Windows/dedicated true. Explicit PublishNuGetAfterPack=false is therefore ignored. Independent import-free MSBuild marker truth table should reproduce4 legacyActor and3 GW false-veto failures while previousmain8 staysgreen.
2. Shared verifier currently requiresone *PushNuGet flag and Push*Release target; Actor haszero flags/PostBuildR. Addonly exact filename plus resolvedPackageId=PulseTrade.Comm.Actor.Registry, onePostBuildR, noflag, and condition allowlist of original-or-approved-veto. Do not accept generic unflagged projects.

Pseudocode: classify exactlegacyActor before flag gate; validatepackage/target/condition; copy noflagdefaults; build marker-only targets; use independent Release && global!=false oracle forlegacyActor, existing oracle forothers. Add two condition suffixes raw-byte-safe. Estimate25–40 non-type lines verifier plus2 product lines; >80 requires Experiment.

No original publisher script or key file is read at runtime; source review only. No build/pack/publish/host mutation. Raw baseline/source XML invariants and real unitcounts required.
## Experiment results

Both hypotheses confirmed:200 matrix RED7false-veto cases before source change, PS5/PS7 each200PASS afterward; strictprofile rejection7/7each. Raw inverse productbytes/BOM/defaults/Exec/refs/versions preserved. TEMP PS5 wrapper array-shape failure was isolated and fixed without changing sourceoracles; originalfailure retained. Evidence C:\Users\Administrator\AppData\Local\Temp\aster-publish52-baa3850bf7b2408c87ee32b56370d7f4. Canonical verifier24+/7-, no estimate overrun. No build/restore/productPack/publish/host actions.
