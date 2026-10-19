# PulseTrade.Comm.Spa.Dynamic.Tagging.Client

Transport-neutral PTCS Dynamic SDUI ExtraInfo editor Plug-In for F# / WebSharper hosts.

One editor instance owns one `SpaceId + TargetKind + TargetId` TARGET and displays
all ExtraInfo blocks for that target at the same time. Each ExtraInfo contains its
own tag set, summary, and time-frame list. Blocks save/delete independently; the
target header can save every dirty ExtraInfo with bounded sequential dispatch.

```fsharp
let handle = TaggingEditorPlugin.mount host TaggingEditorPlugin.defaultOptions
handle.Open binding
```

The host supplies `TaggingEditorBinding.Dispatch`. The Plug-In opens no HTTP or
WebSocket connection and knows no LINE, K-bar, indicator, SQL, account, OAuth,
or credential semantics. Desktop presentation is draggable and multi-target;
mobile presentation keeps one active bottom sheet with tabs for open targets.

The package depends only on Dynamic contracts and WebSharper. It intentionally
does not depend on the TA client or renderer packages.
