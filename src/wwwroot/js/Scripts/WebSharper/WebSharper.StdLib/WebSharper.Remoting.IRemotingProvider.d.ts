import AsyncBody from "../../../Content/WebSharper/WebSharper.StdLib/WebSharper.Concurrency.AsyncBody`1"
import Task from "../../../Content/WebSharper/WebSharper.StdLib/System.Threading.Tasks.Task`1"
export function isIRemotingProvider(x):x is IRemotingProvider
export default interface IRemotingProvider {
  Async(a:string, b:(any)[]):((a:AsyncBody<any>) => void)
  Send(a:string, b:(any)[]):void
  Sync(a:string, b:(any)[])
  Task(a:string, b:(any)[]):Task<any>
}
