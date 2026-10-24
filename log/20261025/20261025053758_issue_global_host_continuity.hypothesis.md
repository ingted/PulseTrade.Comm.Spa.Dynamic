# Hypothesis — GLOBAL bootstrap replacement detach gap

時間：2026-10-25 05:37:58 +08:00
Owner：agent.asteer

## 現象與證據

- Exact 0.2.32 package identity已由consumer核對；final no-1K authority正確。
- Bootstrap→authoritative期間，MutationObserver有5個callback batch找不到任何visible GLOBAL node；contract為0。
- 因此ready gate只保證「何時開始replace」，未保證DOM replace本身的連續可見性。

## 根因候選

1. 高信心：bootstrap與authoritative是不同Doc subtree；WebSharper replace順序先detach舊node再attach新node，observer可見中間空狀態。
2. 次要：selector只存在於被替換內層；即使outer shell仍在，observer的GLOBAL selector契約仍會短暫為0。

## 最小驗法與修復草圖

```text
mount stable GLOBAL host once
keep bootstrap GLOBAL node inside stable host
prepare authoritative GLOBAL content
handoff without removing the last selector-bearing node
after authoritative selector is present, retire bootstrap marker/content
assert every observable transition has globalNodeCount >= 1 and final count = 1
```

預估production非型別宣告變更20–45行、tests 20–60行；若超過2倍，先回此檔記錄原因與新切點。

## Result

- 根因候選1成立：validated-ready gate不等於DOM replacement continuity；0.2.32真consumer仍有5個零visible batch。
- Client以stable雙host、authoritative-first mount與non-zero geometry retirement gate根治detach/attach空窗；production diff 57 additions／8 deletions，未超過估算2倍。
- DYN-T-702修前RED／修後GREEN；0.2.33 exact-package Renderer 94/94、Client 19/19與package verifier全通。真18884相同MutationObserver gate由Daedilus重驗中。
