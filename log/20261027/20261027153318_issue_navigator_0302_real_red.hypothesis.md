# DYN-WBS-615 hypothesis — renderer-instance gesture Var仍未對齊persistent DOM lifetime

## 現象

Exact 0.3.2真B證stable optional `navigatorGestureFrame`已進package，但accepted middle後同一DOM上的long gesture仍產生bare action。Target0與preview正確，故pointer geometry／drag accumulation正常；缺失仍在release取得coverage snapshot之前。

## 已證實邊界

- Package ProductVersion與consumer canonical min.js皆為0.3.2／source `70f2d1a...`。
- Middle第二次前置move action有NavigatorWindow intent，backend/serializer/queue可保留field。
- Long gesture的pointerdown/start/delta/target preview完整，listener不是完全失效；wire absence對應coverage authority缺失。
- 0.3.2 source contract只證Var與assignment／read字串存在，未證同一Var跨accepted rerender仍是listener與mapper共同authority。

## 根因候選與區分實驗

1. Persistent DOM listener仍屬舊renderer instance：accepted document使outer renderer重建新的instance Var與mapper，但WebSharper diff保留舊element/listener；新mapper發布到新Var，舊listener只讀舊Var。比對generated JS closure ownership與renderer telemetry instance即可區分。
2. 同一renderer instance存在多個保留mapper subscription：較新的mapper發布Some後，較早prepared/default mapper再發布None覆寫stable Var。比對assignment次序／projection fingerprints與pointerdown讀值即可區分。

## 預計最小修復

未定；若候選1成立，authority須移到實際DOM element或跨instance且精確keyed的renderer-owned registry，舊listener於event invocation從element current key解析authority。若候選2成立，publication須帶monotonic render generation，只允許目前DOM generation更新authority。預估非型別宣告修改小於40行；超過80行先追加Experiment。

## 不變事項

不改wire、consumer/oracle/host、GLOBAL ordinal、2018 mapping、main plot；不從任意current/latest state拼裝gesture frame。

## Experiment / 根因

0.3.2 canonical generated JS直接證實候選2：stable Var在renderer instance層只有一份，listener於event invocation確實`Get()`；但每個保留的navigator `Map` subscription都以自己的outer `document_1`搭最新`currentPreparedData.RawData`計算projection並寫同一Var。`sameDocumentPresentation`／shell cache刻意正規化coverage/query counters，lexical document可存活；最後執行的舊mapper可用不成對document/data得到None並覆寫新mapper。

最小修正不是generation workaround，而是讓所有writers收斂：每個mapper從其收到的atomic `(renderedState, prepared)` pair選`renderedState.Document`，只有frame沒有document時才fallback cached shell；timeline、coverage decode、maximum-bars與published tuple全使用該currentDocument。DYN-T-729舊碼0/1 RED，DYN-T-729B鎖frame document優先。
