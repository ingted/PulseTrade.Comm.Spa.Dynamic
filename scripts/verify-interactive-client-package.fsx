#r "nuget: FAkka.Argu, 10.1.201"
#load "ParseLine.fsx"

open System
open System.IO
open System.IO.Compression
open System.Security.Cryptography
open System.Text
open System.Text.Json
open System.Xml.Linq
open Argu

[<CliPrefix(CliPrefix.DoubleDash)>]
type VerifyArgs =
    | [<CustomCommandLine("--package")>] Package of string
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Package _ -> "Interactive.Client nupkg path"

let repoRoot = Directory.GetParent(__SOURCE_DIRECTORY__).FullName
let projectPath =
    Path.Combine(
        repoRoot,
        "src",
        "PulseTrade.Comm.Spa.Dynamic.Interactive.Client",
        "PulseTrade.Comm.Spa.Dynamic.Interactive.Client.fsproj")

let project = XDocument.Load projectPath

let projectVersion =
    project.Descendants()
    |> Seq.tryFind (fun element -> element.Name.LocalName = "Version")
    |> Option.map (fun element -> element.Value)
    |> Option.filter (String.IsNullOrWhiteSpace >> not)
    |> Option.defaultWith (fun () -> failwith $"Project Version is missing: {projectPath}")

let projectDependencyVersion packageId =
    project.Descendants()
    |> Seq.filter (fun element -> element.Name.LocalName = "PackageReference")
    |> Seq.tryFind (fun element ->
        let includeAttribute = element.Attribute(XName.Get "Include")
        not (isNull includeAttribute) && includeAttribute.Value = packageId)
    |> Option.bind (fun element ->
        let versionAttribute = element.Attribute(XName.Get "Version")
        if isNull versionAttribute then None else Some versionAttribute.Value)
    |> Option.filter (String.IsNullOrWhiteSpace >> not)
    |> Option.defaultWith (fun () -> failwith $"Project dependency is missing: {packageId}")

let defaultPackagePath =
    Path.Combine(
        repoRoot,
        "src",
        "PulseTrade.Comm.Spa.Dynamic.Interactive.Client",
        "bin",
        "Release",
        $"PulseTrade.Comm.Spa.Dynamic.Interactive.Client.{projectVersion}.nupkg")

let defaultArgumentsText =
    sprintf
        "--package \"%s\""
        (defaultPackagePath.Replace('\\', '/'))

let parser = ArgumentParser.Create<VerifyArgs>(programName = "verify-interactive-client-package.fsx")
let parse text =
    let argv = PL.parseLine [| ' ' |] (Some '"') None true text
    parser.Parse(argv, raiseOnUsage = true)
let defaults = parse defaultArgumentsText

let automationArgs =
    fsi.CommandLineArgs
    |> Array.skip 1
    |> Array.skipWhile ((=) "--")

let automation = parser.Parse automationArgs

let packagePath =
    automation.TryGetResult(<@ Package @>)
    |> Option.orElseWith (fun () -> defaults.TryGetResult(<@ Package @>))
    |> Option.defaultWith (fun () -> failwith "--package is required")
    |> Path.GetFullPath

if not (File.Exists packagePath) then
    failwith $"Package does not exist: {packagePath}"

let prefix = "contentFiles/any/any/ptcs-dynamic-interactive/"
let requiredEntries =
    [ prefix + "bundle.manifest.json"
      prefix + "client.js"
      prefix + "client.min.js"
      prefix + "WebSharper.Core.JavaScript/Runtime.js" ]

let archive = ZipFile.OpenRead packagePath

let entryMap =
    archive.Entries
    |> Seq.map (fun entry -> entry.FullName.Replace('\\', '/'), entry)
    |> Map.ofSeq

requiredEntries
|> List.iter (fun name ->
    if not (Map.containsKey name entryMap) then
        failwith $"Missing package entry: {name}")

let readEntry name =
    use stream = entryMap[name].Open()
    use reader = new StreamReader(stream, Encoding.UTF8, true)
    reader.ReadToEnd()

let manifestText = readEntry (prefix + "bundle.manifest.json")
let nuspecEntry =
    archive.Entries
    |> Seq.tryFind (fun entry -> entry.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase))
    |> Option.defaultWith (fun () -> failwith "Package nuspec is missing.")

let nuspecText =
    use stream = nuspecEntry.Open()
    use reader = new StreamReader(stream, Encoding.UTF8, true)
    reader.ReadToEnd()

for relativePath in
    [ "any/any/ptcs-dynamic-interactive/bundle.manifest.json"
      "any/any/ptcs-dynamic-interactive/client.js"
      "any/any/ptcs-dynamic-interactive/client.min.js"
      "any/any/ptcs-dynamic-interactive/WebSharper.Core.JavaScript/Runtime.js" ] do
    if not (nuspecText.Contains($"include=\"{relativePath}\"")) || not (nuspecText.Contains("copyToOutput=\"true\"")) then
        failwith $"Nuspec contentFiles metadata is incomplete for {relativePath}."

let manifest = JsonDocument.Parse manifestText
let root = manifest.RootElement

let expectProperty (name: string) expected =
    let actual = root.GetProperty(name).GetString()
    if actual <> expected then
        failwith $"Manifest {name} mismatch: expected={expected}; actual={actual}"

expectProperty "schema" "ptcs-dynamic-interactive-bundle.v1"
expectProperty "packageId" "PulseTrade.Comm.Spa.Dynamic.Interactive.Client"
expectProperty "packageVersion" projectVersion

for dependency in
    [ "FSharp.Core"
      "PulseTrade.Comm.Spa.Dynamic.Contracts"
      "PulseTrade.Comm.Spa.Dynamic.Renderer" ] do
    let version = projectDependencyVersion dependency
    let packageId = dependency
    if not (nuspecText.Contains($"id=\"{packageId}\"")) || not (nuspecText.Contains($"version=\"{version}\"")) then
        failwith $"Nuspec dependency mismatch: {packageId} {version}."
expectProperty "protocol" "ptcs-dynamic-action.v1"
expectProperty "entry" "client.js"
expectProperty "runtime" "WebSharper.Core.JavaScript/Runtime.js"

if not (root.GetProperty("module").GetBoolean()) then
    failwith "Manifest module must be true."

let client = readEntry (prefix + "client.js")
let runtimeImport = "from \"./WebSharper.Core.JavaScript/Runtime.js\""

if not (client.Contains runtimeImport) then
    failwith "Client bundle does not import the packaged WebSharper runtime."

let unexpectedImports =
    client.Split([| '\r'; '\n' |], StringSplitOptions.RemoveEmptyEntries)
    |> Array.filter (fun line -> line.TrimStart().StartsWith("import ") && not (line.Contains runtimeImport))

if unexpectedImports.Length > 0 then
    let details = String.concat " | " unexpectedImports
    failwith $"Client bundle has unpackaged imports: {details}"

if not (client.Contains "ptcs-dynamic-action.v1") then
    failwith "Client bundle does not contain the action protocol marker."

for marker in
    [ "interactive-reconnect"
      "interactive-snapshot-timeout"
      "SnapshotAccepted"
      "RECONNECTING"
      "DISPOSED"
      "temporal-axis.v1"
      "temporal-series.v1" ] do
    if not (client.Contains marker) then
        failwith $"Client bundle does not contain lifecycle marker: {marker}."

let sha256 = SHA256.Create()
let packageStream = File.OpenRead packagePath
let hash = sha256.ComputeHash(packageStream) |> Convert.ToHexString |> fun value -> value.ToLowerInvariant()

packageStream.Dispose()
sha256.Dispose()
manifest.Dispose()
archive.Dispose()

printfn "PASS interactive-client-package"
printfn "package=%s" packagePath
printfn "entries=%d" requiredEntries.Length
printfn "sha256=%s" hash
