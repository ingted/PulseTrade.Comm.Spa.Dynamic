
open System.Net.Http

let client = new HttpClient()
let html = client.GetStringAsync("http://127.0.0.1:13387/chat").Result
let js = client.GetStringAsync("http://127.0.0.1:13387/ext/js/PulseTrade.Comm.Spa.Dynamic.js").Result

System.IO.File.WriteAllText("check_html.html", html)
printfn "HTML Length: %d" html.Length
printfn "JS Length: %d" js.Length

