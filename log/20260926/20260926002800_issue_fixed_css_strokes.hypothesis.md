# Hypothesis: fixed CSS stroke contracts

## 現象

- 一般line/SMA的線寬在viewport縮放或row resize後可能跟著SVG viewBox比例改變。
- Overview navigator的可視區間缺少固定2 CSS px的左右深綠邊界契約。

## 核心假設

1. Line path使用SVG user-unit `stroke-width`且未使用`vector-effect: non-scaling-stroke`，導致CSS thickness隨SVG scale改變。
2. Overview viewport selection沒有獨立左右boundary element，或既有boundary同樣受viewBox scale影響。

## 最小實驗

- source定位line path與overview viewport render function／DOM attributes。
- 在BrowserDemo量測resize前後path computed stroke與overview boundary bounding box；先建立失敗oracle。
- 若假設成立，優先以SVG `vector-effect="non-scaling-stroke"`與CSS-pixel overlay／獨立boundary DOM修正，不改data geometry。

## 預估

非型別宣告實作約20–45行，測試約25–50行。若超過90行，須回查是否誤把consumer layout或新framework帶入owner修正。

## Experiment

兩個核心假設均成立。一般line與overview close path缺少`non-scaling-stroke`；selection stroke與不透明handle沒有把可見線及操作命中區分開。最小修正未引入新framework：active／legacy line補fixed stroke，overview改為純fill＋兩條visual line＋既有transparent rect。Daedalus後續把testid定為`ta-overview-left/right-handle-visual`，既有handle保留；owner依該contract重建後通過完整browser gate。
