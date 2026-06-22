open System.Reflection; typeof<PulseTrade.Comm.Spa.Client>.GetMethods() |> Seq.iter (fun m -> printfn "%s" (m.ToString()))
