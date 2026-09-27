namespace PulseTrade.Comm.Spa.Dynamic.Renderer

open System
open PulseTrade.Comm.Spa.Dynamic.Contracts
open WebSharper

type TaDisplayTimestamp =
    { CanonicalUtc: string
      Zone: SduiDisplayTimeZone
      ZoneId: string
      ZoneAbbreviation: string
      FullText: string
      CompactText: string
      DateText: string
      ClockText: string }

[<JavaScript; RequireQualifiedAccess>]
module TaDisplayTimeFormatter =
    type UtcParts =
        { Year: int
          Month: int
          Day: int
          Hour: int
          Minute: int
          Second: int }

    let isLeapYear year =
        year % 400 = 0 || (year % 4 = 0 && year % 100 <> 0)

    let daysInMonth year month =
        match month with
        | 2 -> if isLeapYear year then 29 else 28
        | 4 | 6 | 9 | 11 -> 30
        | 1 | 3 | 5 | 7 | 8 | 10 | 12 -> 31
        | _ -> 0

    let digit (value: string) index =
        let character = value[index]
        if character >= '0' && character <= '9' then Some(int character - int '0') else None

    let number (value: string) start count =
        if start < 0 || count < 0 || start + count > value.Length then
            None
        else
            let mutable result = 0
            let mutable valid = true
            let mutable index = start
            while valid && index < start + count do
                match digit value index with
                | Some valueDigit -> result <- result * 10 + valueDigit
                | None -> valid <- false
                index <- index + 1
            if valid then Some result else None

    let tryParseCanonicalUtc (value: string) =
        let candidate = if isNull value then "" else value.Trim()
        let suffixStart =
            if candidate.EndsWith("Z") || candidate.EndsWith("z") then candidate.Length - 1
            elif candidate.EndsWith("+00:00") then candidate.Length - 6
            else -1
        let fixedShape =
            suffixStart >= 19
            && candidate.Length >= 20
            && candidate[4] = '-'
            && candidate[7] = '-'
            && (candidate[10] = 'T' || candidate[10] = 't' || candidate[10] = ' ')
            && candidate[13] = ':'
            && candidate[16] = ':'
        let fractionValid =
            if suffixStart = 19 then true
            elif suffixStart > 20 && candidate[19] = '.' then
                candidate.Substring(20, suffixStart - 20)
                |> Seq.forall (fun character -> character >= '0' && character <= '9')
            else false

        match fixedShape, fractionValid,
              number candidate 0 4, number candidate 5 2, number candidate 8 2,
              number candidate 11 2, number candidate 14 2, number candidate 17 2 with
        | true, true, Some year, Some month, Some day, Some hour, Some minute, Some second
            when year >= 1
                 && day >= 1
                 && day <= daysInMonth year month
                 && hour <= 23
                 && minute <= 59
                 && second <= 59 ->
            Some
                { Year = year
                  Month = month
                  Day = day
                  Hour = hour
                  Minute = minute
                  Second = second }
        | _ -> None

    let dayOfWeek year month day =
        let offsets = [| 0; 3; 2; 5; 0; 3; 5; 1; 4; 6; 2; 4 |]
        let adjustedYear = if month < 3 then year - 1 else year
        (adjustedYear + adjustedYear / 4 - adjustedYear / 100 + adjustedYear / 400 + offsets[month - 1] + day) % 7

    let nthSunday year month occurrence =
        let firstSunday = 1 + ((7 - dayOfWeek year month 1) % 7)
        firstSunday + 7 * (occurrence - 1)

    let instantKey month day hour minute second =
        (((month * 32 + day) * 24 + hour) * 60 + minute) * 60 + second

    let isModernUsDaylightTime zone parts =
        let startHourUtc, endHourUtc =
            match zone with
            | SduiDisplayTimeZone.AmericaChicago -> 8, 7
            | SduiDisplayTimeZone.AmericaNewYork -> 7, 6
            | _ -> 0, 0
        let startKey = instantKey 3 (nthSunday parts.Year 3 2) startHourUtc 0 0
        let endKey = instantKey 11 (nthSunday parts.Year 11 1) endHourUtc 0 0
        let valueKey = instantKey parts.Month parts.Day parts.Hour parts.Minute parts.Second
        valueKey >= startKey && valueKey < endKey

    let offsetAndAbbreviation zone parts =
        match zone with
        | SduiDisplayTimeZone.Utc -> 0, "UTC"
        | SduiDisplayTimeZone.FixedUtcPlus8 -> 8 * 60, "UTC+8"
        | SduiDisplayTimeZone.AmericaChicago ->
            if isModernUsDaylightTime zone parts then -5 * 60, "CDT" else -6 * 60, "CST"
        | SduiDisplayTimeZone.AmericaNewYork ->
            if isModernUsDaylightTime zone parts then -4 * 60, "EDT" else -5 * 60, "EST"

    let previousDay year month day =
        if day > 1 then year, month, day - 1
        elif month > 1 then
            let previousMonth = month - 1
            year, previousMonth, daysInMonth year previousMonth
        else
            year - 1, 12, 31

    let nextDay year month day =
        if day < daysInMonth year month then year, month, day + 1
        elif month < 12 then year, month + 1, 1
        else year + 1, 1, 1

    let applyOffset offsetMinutes parts =
        let totalMinutes = parts.Hour * 60 + parts.Minute + offsetMinutes
        let year, month, day, normalizedMinutes =
            if totalMinutes < 0 then
                let previousYear, previousMonth, previousDate = previousDay parts.Year parts.Month parts.Day
                previousYear, previousMonth, previousDate, totalMinutes + 24 * 60
            elif totalMinutes >= 24 * 60 then
                let nextYear, nextMonth, nextDate = nextDay parts.Year parts.Month parts.Day
                nextYear, nextMonth, nextDate, totalMinutes - 24 * 60
            else
                parts.Year, parts.Month, parts.Day, totalMinutes
        { parts with
            Year = year
            Month = month
            Day = day
            Hour = normalizedMinutes / 60
            Minute = normalizedMinutes % 60 }

    let pad2 value = if value < 10 then "0" + string value else string value
    let pad4 value =
        let text = string value
        if text.Length >= 4 then text else String.replicate (4 - text.Length) "0" + text

    let tryFormat zone canonicalUtc =
        tryParseCanonicalUtc canonicalUtc
        |> Option.map (fun utc ->
            let offsetMinutes, abbreviation = offsetAndAbbreviation zone utc
            let local = applyOffset offsetMinutes utc
            let dateText = pad4 local.Year + "-" + pad2 local.Month + "-" + pad2 local.Day
            let clockText = pad2 local.Hour + ":" + pad2 local.Minute + ":" + pad2 local.Second
            { CanonicalUtc = canonicalUtc
              Zone = zone
              ZoneId = SduiDisplayTimeZone.id zone
              ZoneAbbreviation = abbreviation
              FullText = dateText + " " + clockText + " " + abbreviation
              CompactText = pad2 local.Month + "-" + pad2 local.Day + " " + pad2 local.Hour + ":" + pad2 local.Minute + " " + abbreviation
              DateText = dateText
              ClockText = clockText + " " + abbreviation })

    let fullOrOriginal zone canonicalUtc =
        tryFormat zone canonicalUtc |> Option.map _.FullText |> Option.defaultValue canonicalUtc

    let compactOrOriginal zone canonicalUtc =
        tryFormat zone canonicalUtc |> Option.map _.CompactText |> Option.defaultValue canonicalUtc

    let dateAndClockOrUnavailable zone canonicalUtc =
        tryFormat zone canonicalUtc
        |> Option.map (fun value -> value.DateText, value.ClockText)
        |> Option.defaultValue ("Unavailable", "Unavailable")
