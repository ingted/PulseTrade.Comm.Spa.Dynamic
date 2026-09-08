namespace PulseTrade.Comm.Spa.Dynamic.Interactive.Client

open WebSharper

[<JavaScript>]
type InteractiveClientLifecycleOptions =
    { ReconnectBaseMs: int
      ReconnectMaximumMs: int }

[<JavaScript>]
type InteractiveClientLifecycleState =
    { Started: bool
      Connected: bool
      ReconnectAttempt: int
      ReconnectScheduled: bool
      Disposed: bool }

[<JavaScript; RequireQualifiedAccess>]
type InteractiveClientLifecycleEvent =
    | Start
    | TransportOpened of hasRuntimeIdentity: bool
    | SnapshotAccepted
    | TransportClosed
    | ReconnectDue
    | Dispose

[<JavaScript; RequireQualifiedAccess>]
type InteractiveClientLifecycleEffect =
    | OpenTransport
    | SendMounted
    | RequestFullSnapshot
    | SendUnmounted
    | ScheduleReconnect of delayMs: int
    | CancelReconnect
    | CloseTransport

/// Pure browser-application connection lifecycle. Socket identity/generation checks stay in the
/// browser adapter; this state machine owns idempotent start, bounded reconnect and terminal dispose.
[<JavaScript; RequireQualifiedAccess>]
module InteractiveClientLifecycle =
    let defaults =
        { ReconnectBaseMs = 1000
          ReconnectMaximumMs = 30000 }

    let initial =
        { Started = false
          Connected = false
          ReconnectAttempt = 0
          ReconnectScheduled = false
          Disposed = false }

    let reconnectDelay options attempt =
        let rec expand current remaining =
            if remaining <= 1 then current
            else expand (min options.ReconnectMaximumMs (current * 2)) (remaining - 1)

        expand options.ReconnectBaseMs (max 1 attempt)

    let transition options event state =
        if state.Disposed then
            state, [||]
        else
            match event with
            | InteractiveClientLifecycleEvent.Start when not state.Started ->
                { state with Started = true },
                [| InteractiveClientLifecycleEffect.OpenTransport |]
            | InteractiveClientLifecycleEvent.Start -> state, [||]
            | InteractiveClientLifecycleEvent.TransportOpened hasRuntimeIdentity when state.Started ->
                let synchronizationEffects =
                    if hasRuntimeIdentity then
                        [| InteractiveClientLifecycleEffect.SendMounted
                           InteractiveClientLifecycleEffect.RequestFullSnapshot |]
                    else
                        [||]

                { state with
                    Connected = true
                    ReconnectScheduled = false },
                Array.append [| InteractiveClientLifecycleEffect.CancelReconnect |] synchronizationEffects
            | InteractiveClientLifecycleEvent.SnapshotAccepted when state.Started && state.Connected ->
                { state with ReconnectAttempt = 0 }, [||]
            | InteractiveClientLifecycleEvent.SnapshotAccepted -> state, [||]
            | InteractiveClientLifecycleEvent.TransportClosed when state.Started && not state.ReconnectScheduled ->
                let attempt = state.ReconnectAttempt + 1

                { state with
                    Connected = false
                    ReconnectAttempt = attempt
                    ReconnectScheduled = true },
                [| InteractiveClientLifecycleEffect.ScheduleReconnect(reconnectDelay options attempt) |]
            | InteractiveClientLifecycleEvent.TransportClosed ->
                { state with Connected = false }, [||]
            | InteractiveClientLifecycleEvent.ReconnectDue when state.Started && state.ReconnectScheduled ->
                { state with ReconnectScheduled = false },
                [| InteractiveClientLifecycleEffect.OpenTransport |]
            | InteractiveClientLifecycleEvent.ReconnectDue -> state, [||]
            | InteractiveClientLifecycleEvent.Dispose ->
                { state with
                    Connected = false
                    ReconnectScheduled = false
                    Disposed = true },
                [| InteractiveClientLifecycleEffect.CancelReconnect
                   InteractiveClientLifecycleEffect.SendUnmounted
                   InteractiveClientLifecycleEffect.CloseTransport |]
            | _ -> state, [||]
