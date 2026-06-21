#r "nuget: FAkka.FCell2, 10.1.301"
open FAkka.FCell2
let asm = typeof<FCell2View<string,string,string>>.Assembly
asm.GetTypes() |> Array.filter (fun t -> t.Name.Contains("fCell2")) |> Array.iter (fun t -> printfn "FullName: %s" t.FullName)
