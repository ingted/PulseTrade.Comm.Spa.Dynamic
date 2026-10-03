# Unit verification only: extracts publisher gates into an import-free MSBuild
# fixture. Original task bodies, Exec commands, imports and package builds never run.
[CmdletBinding()]
param(
    [string[]]$ProjectPath = @(),
    [string]$EvidenceRoot = '',
    [ValidateRange(5,300)][int]$TimeoutSeconds = 60
)
$ErrorActionPreference = 'Stop'
$utf8 = New-Object System.Text.UTF8Encoding($false, $true)
function HashFile([string]$Path) {
    $sha = [Security.Cryptography.SHA256]::Create()
    try { return [BitConverter]::ToString($sha.ComputeHash([IO.File]::ReadAllBytes($Path))).Replace('-','') }
    finally { $sha.Dispose() }
}
function WriteJson([string]$Path, $Value) {
    [IO.File]::WriteAllText($Path, (ConvertTo-Json -InputObject $Value -Depth 12), $utf8)
}
function AddElement($Document, $Parent, [string]$Name, $Attributes) {
    $element = $Document.CreateElement($Name)
    foreach ($key in $Attributes.Keys) { $element.SetAttribute($key, [string]$Attributes[$key]) }
    [void]$Parent.AppendChild($element)
    return $element
}
if ($ProjectPath.Count -eq 0) {
    $ProjectPath = @('src/PulseTrade.Comm.Spa.Dynamic.fsproj') + @(
        'ACL','Dynamic.Contracts','Dynamic.Interactive.Client','Dynamic.Ptcs',
        'Dynamic.Ptcs.Client','Dynamic.Renderer','Login'
    ) | ForEach-Object {
        if ($_ -like 'src/*') { Join-Path (Split-Path $PSScriptRoot) $_ }
        else { Join-Path (Split-Path $PSScriptRoot) "src/PulseTrade.Comm.Spa.$_/PulseTrade.Comm.Spa.$_.fsproj" }
    }
}
if ([string]::IsNullOrWhiteSpace($EvidenceRoot)) {
    $EvidenceRoot = Join-Path ([IO.Path]::GetTempPath()) ('ptc-publish-hook-unit-' + [Guid]::NewGuid().ToString('N'))
}
$EvidenceRoot = [IO.Path]::GetFullPath($EvidenceRoot)
if (Test-Path -LiteralPath $EvidenceRoot) { throw 'evidence-root-already-exists' }
[void][IO.Directory]::CreateDirectory($EvidenceRoot)
$cases = @(
    @{name='cli-default'; vs='false'; flag=''; global=''; config='Release'; os='Windows_NT'; allow=$false},
    @{name='vs-default'; vs='true'; flag=''; global=''; config='Release'; os='Windows_NT'; allow='vs'},
    @{name='cli-explicit-false'; vs='false'; flag='false'; global=''; config='Release'; os='Windows_NT'; allow=$false},
    @{name='vs-explicit-false'; vs='true'; flag='false'; global=''; config='Release'; os='Windows_NT'; allow=$false},
    @{name='cli-explicit-true'; vs='false'; flag='true'; global=''; config='Release'; os='Windows_NT'; allow=$true},
    @{name='vs-explicit-true'; vs='true'; flag='true'; global=''; config='Release'; os='Windows_NT'; allow=$true},
    @{name='cli-global-false'; vs='false'; flag=''; global='false'; config='Release'; os='Windows_NT'; allow=$false},
    @{name='vs-global-false'; vs='true'; flag=''; global='false'; config='Release'; os='Windows_NT'; allow=$false},
    @{name='cli-true-global-false'; vs='false'; flag='true'; global='false'; config='Release'; os='Windows_NT'; allow=$false},
    @{name='vs-true-global-false'; vs='true'; flag='true'; global='false'; config='Release'; os='Windows_NT'; allow=$false},
    @{name='cli-global-true-alone'; vs='false'; flag=''; global='true'; config='Release'; os='Windows_NT'; allow=$false},
    @{name='vs-global-true-default'; vs='true'; flag=''; global='true'; config='Release'; os='Windows_NT'; allow='vs'},
    @{name='cli-true-global-true'; vs='false'; flag='true'; global='true'; config='Release'; os='Windows_NT'; allow=$true},
    @{name='debug-true'; vs='false'; flag='true'; global=''; config='Debug'; os='Windows_NT'; allow=$true},
    @{name='nonwindows-release-true'; vs='false'; flag='true'; global=''; config='Release'; os='Unix'; allow=$true},
    @{name='nonwindows-debug-true'; vs='false'; flag='true'; global=''; config='Debug'; os='Unix'; allow=$true},
    @{name='other-config-true'; vs='false'; flag='true'; global=''; config='Other'; os='Windows_NT'; allow=$true},
    @{name='vs-debug-false'; vs='true'; flag='false'; global=''; config='Debug'; os='Windows_NT'; allow=$false},
    @{name='vs-debug-true-global-false'; vs='true'; flag='true'; global='false'; config='Debug'; os='Windows_NT'; allow=$false},
    @{name='cli-false-global-true'; vs='false'; flag='false'; global='true'; config='Release'; os='Windows_NT'; allow=$false}
)
$inputs = @(); $checks = @(); $targetsCount = 0; $outcomes = @(); $failure = $null
$driver = New-Object Xml.XmlDocument
$driverRoot = AddElement $driver $driver 'Project' @{}
$probe = AddElement $driver $driverRoot 'Target' @{Name='Probe'}
try {
    foreach ($path in $ProjectPath) {
        $path = (Get-Item -LiteralPath $path).FullName
        if ([IO.Path]::GetExtension($path) -ne '.fsproj') { throw 'project-extension-required' }
        $raw = [IO.File]::ReadAllText($path, $utf8)
        if ($raw -match '<!DOCTYPE') { throw 'source-doctype-forbidden' }
        $source = New-Object Xml.XmlDocument; $source.XmlResolver = $null; $source.LoadXml($raw)
        $flags = @([regex]::Matches($raw, '\$\((\w+PushNuGet)\)') | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
        $isActorLegacy = [IO.Path]::GetFileName($path) -ceq 'PulseTrade.Comm.Actor.Registry.fsproj'
        if ($isActorLegacy) {
            $packageIds = @($source.SelectNodes('/Project/PropertyGroup/PackageId'))
            $assemblyNames = @($source.SelectNodes('/Project/PropertyGroup/AssemblyName'))
            if ($packageIds.Count -gt 1 -or $assemblyNames.Count -ne 1 -or $flags.Count -ne 0) { throw 'actor-legacy-identity-required' }
            $identity = if ($packageIds.Count -eq 1) { $packageIds[0] } else { $assemblyNames[0] }
            if ($identity.HasAttribute('Condition') -or $identity.ParentNode.HasAttribute('Condition') -or $identity.InnerText -cne 'PulseTrade.Comm.Actor.Registry') { throw 'actor-legacy-identity-required' }
            $flag = ''
        } else {
            if ($flags.Count -ne 1) { throw 'one-publisher-flag-required' }
            $flag = $flags[0]
        }
        $isUmbrella = [IO.Path]::GetFileName($path) -eq 'PulseTrade.Comm.Spa.Dynamic.fsproj'
        $targets = @($source.SelectNodes('/Project/Target') | Where-Object { $_.GetAttribute('AfterTargets') -eq 'Pack' -and $_.SelectNodes('.//Exec').Count -gt 0 })
        $expectedNames = if ($isUmbrella) { @('PostBuildD','PostBuildR') } else { @($targets | ForEach-Object { $_.Name }) }
        if ($isActorLegacy) {
            if ($targets.Count -ne 1 -or $targets[0].Name -cne 'PostBuildR') { throw 'actor-legacy-target-required' }
            $condition = [regex]::Replace($targets[0].GetAttribute('Condition'), '\s+', ' ').Trim()
            $legacyCondition = "'`$(Configuration)' == 'Release'"
            $vetoCondition = $legacyCondition + " and '`$(PublishNuGetAfterPack)' != 'false'"
            if ($condition -cne $legacyCondition -and $condition -cne $vetoCondition) { throw 'actor-legacy-condition-required' }
        } elseif (($isUmbrella -and $targets.Count -ne 2) -or (!$isUmbrella -and ($targets.Count -ne 1 -or $targets[0].Name -notmatch '^Push\w+ReleasePackageToNuGet$'))) { throw 'publisher-target-shape-changed' }
        if ($isUmbrella -and (@($targets | ForEach-Object {$_.Name} | Sort-Object) -join '|') -ne ($expectedNames -join '|')) { throw 'umbrella-target-names-changed' }
        $index = $inputs.Count
        $backup = Join-Path $EvidenceRoot ("source-$index.fsproj")
        [IO.File]::Copy($path, $backup, $false)
        $inputs += [pscustomobject]@{path=$path; sha256=(HashFile $backup); flag=$flag; targets=@($targets | ForEach-Object {$_.Name}); version=[string]$source.SelectSingleNode('/Project/PropertyGroup/Version').InnerText}
        $targetsCount += $targets.Count
        foreach ($case in $cases) {
            $caseId = "$index-$($case.name)"
            $fixturePath = Join-Path $EvidenceRoot "$caseId.proj"
            $marker = Join-Path $EvidenceRoot "$caseId.marker"
            $fixture = New-Object Xml.XmlDocument
            $fixtureRoot = AddElement $fixture $fixture 'Project' @{}
            foreach ($group in $source.SelectNodes('/Project/PropertyGroup')) {
                $properties = @($group.ChildNodes | Where-Object { $_.NodeType -eq [Xml.XmlNodeType]::Element -and $_.Name -eq $flag })
                if ($properties.Count -gt 0) {
                    $copyGroup = AddElement $fixture $fixtureRoot 'PropertyGroup' @{}
                    if ($group.HasAttribute('Condition')) { $copyGroup.SetAttribute('Condition', $group.GetAttribute('Condition')) }
                    foreach ($property in $properties) { [void]$copyGroup.AppendChild($fixture.ImportNode($property, $true)) }
                }
            }
            [void](AddElement $fixture $fixtureRoot 'Target' @{Name='Pack'})
            foreach ($target in $targets) {
                $copyTarget = AddElement $fixture $fixtureRoot 'Target' @{Name=$target.Name; AfterTargets=$target.GetAttribute('AfterTargets'); Condition=$target.GetAttribute('Condition')}
                [void](AddElement $fixture $copyTarget 'WriteLinesToFile' @{File=$marker; Lines=$target.Name; Overwrite='false'; Encoding='UTF-8'})
            }
            if ($fixture.OuterXml -match '\$\(\[' -or $fixture.SelectNodes('//Exec|//Import|//UsingTask').Count -gt 0) { throw 'unsafe-fixture-shape' }
            [IO.File]::WriteAllText($fixturePath, $fixture.OuterXml, $utf8)
            $propertiesText = "Configuration=$($case.config);OS=$($case.os);BuildingInsideVisualStudio=$($case.vs);PublishNuGetAfterPack=$($case.global)"
            if ($flag -ne '' -and $case.flag -ne '') { $propertiesText += ";$flag=$($case.flag)" }
            [void](AddElement $driver $probe 'MSBuild' @{Projects=$fixturePath; Targets='Pack'; BuildInParallel='false'; Properties=$propertiesText})
            $allowed = if ($isActorLegacy) { $case.global -ne 'false' } elseif ($case.allow -is [string]) { !$isUmbrella } else { [bool]$case.allow }
            $expected = @()
            if ($allowed) {
                if ($isActorLegacy -and $case.config -eq 'Release') { $expected = @('PostBuildR') }
                elseif ($isUmbrella -and $case.config -eq 'Release') { $expected = @('PostBuildR') }
                elseif ($isUmbrella -and $case.config -eq 'Debug') { $expected = @('PostBuildD') }
                elseif (!$isUmbrella -and $case.config -eq 'Release' -and $case.os -eq 'Windows_NT') { $expected = @($targets[0].Name) }
            }
            $checks += [pscustomobject]@{project=$path; case=$case.name; marker=$marker; expected=$expected}
        }
    }
    $driverPath = Join-Path $EvidenceRoot 'driver.proj'
    [IO.File]::WriteAllText($driverPath, $driver.OuterXml, $utf8)
    WriteJson (Join-Path $EvidenceRoot 'source-before.json') $inputs
    $start = New-Object Diagnostics.ProcessStartInfo
    $start.FileName = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source
    $start.Arguments = 'msbuild "' + $driverPath + '" /t:Probe /nologo /v:minimal /nr:false /m:1'
    $start.UseShellExecute = $false; $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true; $start.RedirectStandardError = $true
    foreach ($input in $inputs) { if ($input.flag -ne '') { $start.EnvironmentVariables.Remove($input.flag) } }
    $process = New-Object Diagnostics.Process; $process.StartInfo = $start
    try {
        if (!$process.Start()) { throw 'msbuild-start-failed' }
        $stdout = $process.StandardOutput.ReadToEndAsync(); $stderr = $process.StandardError.ReadToEndAsync()
        if (!$process.WaitForExit($TimeoutSeconds * 1000)) { throw 'msbuild-timeout' }
        [IO.File]::WriteAllText((Join-Path $EvidenceRoot 'stdout.log'), $stdout.GetAwaiter().GetResult(), $utf8)
        [IO.File]::WriteAllText((Join-Path $EvidenceRoot 'stderr.log'), $stderr.GetAwaiter().GetResult(), $utf8)
        if ($process.ExitCode -ne 0) { throw 'msbuild-fixture-failed' }
    } finally {
        if ($null -ne $process -and !$process.HasExited) { $process.Kill(); [void]$process.WaitForExit(5000) }
        $process.Dispose()
    }
    foreach ($check in $checks) {
        $actual = if ([IO.File]::Exists($check.marker)) { @([IO.File]::ReadAllLines($check.marker, $utf8) | Where-Object { $_ -ne '' } | Sort-Object) } else { @() }
        $expected = @($check.expected | Sort-Object)
        $outcomes += [pscustomobject]@{project=$check.project; case=$check.case; expected=$expected; actual=@($actual); passed=(($actual -join '|') -ceq ($expected -join '|'))}
    }
} catch { $failure = [pscustomobject]@{code=$_.Exception.Message; type=$_.Exception.GetType().FullName} }
$after = @($inputs | ForEach-Object { [pscustomobject]@{path=$_.path; sha256=(HashFile $_.path); unchanged=((HashFile $_.path) -ceq $_.sha256)} })
$failed = @($outcomes | Where-Object {!$_.passed}).Count
$passed = $null -eq $failure -and $failed -eq 0 -and $outcomes.Count -eq $checks.Count -and $checks.Count -gt 0 -and @($after | Where-Object {!$_.unchanged}).Count -eq 0
$result = [pscustomobject]@{passed=$passed; finalized=$passed; kind='isolated-msbuild-unit'; projects=$inputs.Count; targets=$targetsCount; executed=$outcomes.Count; failed=$failed; sourceUnchanged=(@($after | Where-Object {!$_.unchanged}).Count -eq 0); failure=$failure; cases=$outcomes}
WriteJson (Join-Path $EvidenceRoot 'source-after.json') $after
WriteJson (Join-Path $EvidenceRoot 'result.json') $result
[pscustomobject]@{passed=$passed; projects=$inputs.Count; targets=$targetsCount; executed=$outcomes.Count; failed=$failed; evidence=$EvidenceRoot; failure=$failure} | ConvertTo-Json -Compress
if (!$passed) { exit 1 }
