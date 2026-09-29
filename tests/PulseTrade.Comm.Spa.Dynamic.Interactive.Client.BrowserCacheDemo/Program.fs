namespace PulseTrade.Comm.Spa.Dynamic.Interactive.Client.BrowserCacheDemo

open System
open System.Net
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open PulseTrade.Comm.Spa.Dynamic.Contracts

module Program =
    let document =
        { WorkspaceId = "browser-cache-fixture"
          Title = "Browser cache fixture"
          RowsRef = "rows"
          StatusRef = "status"
          SharedTimeAxis = false
          TemporalAxisRefs = [||]
          BaseRowId = None
          Rows = [||]
          EditorSchemas = [||]
          AllowedActions = [||]
          DefaultView = Map.empty }

    let entry index =
        let startUtc = DateTimeOffset(2026, 9, 1 + index, 0, 0, 0, TimeSpan.Zero)
        let axisRef = "browser-cache-axis-" + string index
        let axis =
            { AxisRef = axisRef
              Revision = int64 (index + 1)
              Points =
                [| { Position = 0L
                     SourceIntervalId = "browser-cache-source-" + string index
                     ScaleKey = "1K"
                     IntervalStartUtc = startUtc
                     IntervalEndUtc = startUtc.AddDays(1.0)
                     EventTimeUtc = Some(startUtc.AddDays(1.0))
                     ObservedThroughUtc = startUtc.AddDays(1.0)
                     AvailableAtUtc = Some(startUtc.AddDays(1.0))
                     Finality = PointFinality.Final
                     Projection = TemporalProjection.CandleSpan
                     Quality = Some "complete" }
                   { Position = 1L
                     SourceIntervalId = "browser-cache-preview-" + string index
                     ScaleKey = "1K"
                     IntervalStartUtc = startUtc.AddDays(1.0)
                     IntervalEndUtc = startUtc.AddDays(2.0)
                     EventTimeUtc = Some(startUtc.AddDays(1.5))
                     ObservedThroughUtc = startUtc.AddDays(1.5)
                     AvailableAtUtc = None
                     Finality = PointFinality.Preview
                     Projection = TemporalProjection.CandleSpan
                     Quality = Some "preview" } |] }
        let entryDocument = { document with TemporalAxisRefs = [| axisRef |] }

        { CacheIdentity =
            { OwnerFingerprint = "browser-cache-query-" + string index
              SchemaRevision = RuntimeCache.CurrentSchemaRevision }
          WorkspaceId = document.WorkspaceId
          Document = entryDocument
          Snapshot = { Data = Map [ axisRef, TemporalAxisCodec.encode axis ]; Freshness = TaFreshness.Stale(TimeSpan.Zero, "fixture") }
          DocumentRevision = 1L
          DataRevision = int64 (index + 1)
          Coverage =
            { StartEventTimeUtc = startUtc
              EndEventTimeExclusiveUtc = startUtc.AddDays(1.0) }
          CapturedAtUtc = startUtc }

    let largeEntry () =
        let pointCount = 3820
        let seriesCount = 28
        let axisRef = "browser-cache-large-axis"
        let startUtc = DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero)
        let axisPoints =
            Array.init pointCount (fun index ->
                let intervalStart = startUtc.AddMinutes(float index)
                let intervalEnd = intervalStart.AddMinutes(1.0)

                { Position = int64 index
                  SourceIntervalId = $"browser-cache-large:{index}"
                  ScaleKey = "1K"
                  IntervalStartUtc = intervalStart
                  IntervalEndUtc = intervalEnd
                  EventTimeUtc = Some intervalEnd
                  ObservedThroughUtc = intervalEnd
                  AvailableAtUtc = Some intervalEnd
                  Finality = PointFinality.Final
                  Projection = TemporalProjection.CandleSpan
                  Quality = Some "authoritative" })
        let traces =
            Array.init seriesCount (fun index ->
                { TraceId = $"browser-cache-large-trace-{index}"
                  Kind = TaTraceKind.Line
                  DataRef = $"browser-cache-large-series-{index}"
                  Label = $"Series {index}"
                  Color = ""
                  Width = 1.0
                  Visible = true
                  CandleDataRefs = None
                  Options = Map.empty })
        let row =
            { RowId = "browser-cache-large-row"
              Kind = TaRowKind.Sma
              DataRef = traces[0].DataRef
              HeightWeight = 1.0
              Visible = true
              Options = Map.empty
              Traces = traces }
        let largeDocument =
            { document with
                WorkspaceId = "browser-cache-large-fixture"
                Title = "Browser cache large fixture"
                SharedTimeAxis = true
                TemporalAxisRefs = [| axisRef |]
                BaseRowId = Some row.RowId
                Rows = [| row |] }
        let data =
            [ yield axisRef, TemporalAxisCodec.encode { AxisRef = axisRef; Revision = 1L; Points = axisPoints }

              for seriesIndex in 0 .. seriesCount - 1 do
                  let points =
                      Array.init pointCount (fun position ->
                          { Position = int64 position
                            Value = SduiValue.Number(float position + float seriesIndex) })

                  yield
                      $"browser-cache-large-series-{seriesIndex}",
                      TemporalSeriesCodec.encode
                          { AxisRef = axisRef
                            AxisRevision = 1L
                            Points = points } ]
            |> Map.ofList

        { CacheIdentity =
            { OwnerFingerprint = "browser-cache-large-query"
              SchemaRevision = RuntimeCache.CurrentSchemaRevision }
          WorkspaceId = largeDocument.WorkspaceId
          Document = largeDocument
          Snapshot =
            { Data = data
              Freshness = TaFreshness.Stale(TimeSpan.Zero, "large-fixture") }
          DocumentRevision = 1L
          DataRevision = 1L
          Coverage =
            { StartEventTimeUtc = startUtc
              EndEventTimeExclusiveUtc = startUtc.AddMinutes(float pointCount) }
          CapturedAtUtc = startUtc }

    let html () =
        let fixtures =
            [| 0..9 |]
            |> Array.map (fun index ->
                let encoded = entry index |> BrowserRuntimeCodec.encodeCacheEntry |> WebUtility.HtmlEncode
                $"<textarea hidden id=\"cache-entry-{index}\">{encoded}</textarea>")
            |> String.concat Environment.NewLine

        let largeFixture =
            largeEntry ()
            |> BrowserRuntimeCodec.encodeCacheEntry
            |> WebUtility.HtmlEncode

        $"""<!doctype html>
<html lang="en">
<head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Browser cache gate</title></head>
<body><main id="app"></main>{fixtures}<textarea hidden id="cache-large-entry">{largeFixture}</textarea><script type="module" src="/js/PulseTrade.Comm.Spa.Dynamic.Interactive.Client.BrowserCacheDemo.js"></script></body>
</html>"""

    [<EntryPoint>]
    let main args =
        let builder = WebApplication.CreateBuilder(args)
        let app = builder.Build()
        app.Urls.Add("http://127.0.0.1:18885")
        app.UseStaticFiles() |> ignore
        app.MapGet("/favicon.ico", Func<HttpContext, Threading.Tasks.Task>(fun context ->
            context.Response.StatusCode <- StatusCodes.Status204NoContent
            Threading.Tasks.Task.CompletedTask)) |> ignore
        app.MapGet("/", Func<HttpContext, Threading.Tasks.Task>(fun context ->
            context.Response.ContentType <- "text/html; charset=utf-8"
            context.Response.WriteAsync(html ()))) |> ignore
        printfn "Interactive.Client browser cache demo: http://127.0.0.1:18885/"
        app.Run()
        0
