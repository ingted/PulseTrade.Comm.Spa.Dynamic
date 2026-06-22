import { isBlank, asText } from "./PulseTrade.Comm.Spa.Client.js"
import { Some } from "./Microsoft.FSharp.Core.FSharpOption`1.js"
export function tryJson(text){
  try {
    return isBlank(text)?null:Some(json(text));
  }
  catch(m){
    return null;
  }
}
export function jsonOrDefault(fallback, text){
  return isBlank(text)?fallback:json(text);
}
export function json(text){
  return JSON.parse(asText(text));
}
