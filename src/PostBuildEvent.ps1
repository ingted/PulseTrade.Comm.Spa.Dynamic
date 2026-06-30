param(
    $assembly = "PulseTrade.Comm.Spa.Dynamic"
)
function Get-VersionFromFileName {
    param ([string]$fileName)
    if ($fileName -match '\d+(\.\d+)+(-\w+(\.\d+)?)?') {
        return [version]($matches[0].Split('-')[0])
    } else {
        return [version]'0.0'
    }
}
function Get-NuGetApiKey {
    $paths = @("/Nuget/apikey.txt", "G:\Nuget\apikey.txt", "C:\Nuget\apikey.txt")
    foreach ($p in $paths) {
        if (Test-Path $p) {
            return (Get-Content -Path $p -Raw).Trim()
        }
    }
    return $null
}
function Get-LibPacksContent {
    $currentDir = Get-Location

    while ($currentDir -ne [System.IO.Path]::GetPathRoot($currentDir)) {
        $libPacksPath = Join-Path -Path $currentDir -ChildPath 'lib-packs.txt'
        if (Test-Path $libPacksPath) {
            return Get-Content -Path $libPacksPath -Raw
        }
        $currentDir = (Get-Item $currentDir).Parent.FullName
    }

    Write-Host "lib-packs.txt not found in any parent directory." -ForegroundColor Red
    return $null
}
function Get-ProjectPackageVersion {
    param([string]$assembly)

    $projectPath = Join-Path (Get-Location).Path "$assembly.fsproj"
    if (-not (Test-Path -LiteralPath $projectPath)) {
        return $null
    }

    $projectXml = [xml](Get-Content -LiteralPath $projectPath -Raw -Encoding UTF8)
    $versionNode = $projectXml.Project.PropertyGroup |
        ForEach-Object { $_.Version } |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        Select-Object -First 1

    if ([string]::IsNullOrWhiteSpace($versionNode)) {
        return $null
    }

    return [string]$versionNode
}
function Get-DefaultLibraryPacksPath {
    $sdkRoot = Join-Path ${env:ProgramW6432} "dotnet\sdk"
    if (-not (Test-Path -LiteralPath $sdkRoot)) {
        return $null
    }

    $sdk = Get-ChildItem -LiteralPath $sdkRoot -Directory |
        Sort-Object -Property { [version](($_.Name -replace '-.*$', '')) } -Descending |
        Select-Object -First 1

    if ($null -eq $sdk) {
        return $null
    }

    return Join-Path $sdk.FullName "FSharp\library-packs"
}
Write-Host ("[PostBuild] " + $assembly + ": Running in " + $PSVersionTable.OS)
$packageVersion = Get-ProjectPackageVersion $assembly
$searchRoots = @(
    (Join-Path (Get-Location).Path "bin"),
    (Join-Path (Get-Location).Path "bin/Release")
) | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -Unique

if ($searchRoots.Count -gt 0) {
    if ([string]::IsNullOrWhiteSpace($packageVersion)) {
        $packages = $searchRoots | ForEach-Object { Get-ChildItem -LiteralPath $_ -Filter "$($assembly)*.nupkg" }
    } else {
        $expectedPackageName = "$assembly.$packageVersion.nupkg"
        $packages = $searchRoots | ForEach-Object { Get-ChildItem -LiteralPath $_ -Filter $expectedPackageName }
    }

    $packages = $packages | Sort-Object -Property LastWriteTimeUtc -Descending
    if ($packages.Count -eq 0) {
        Write-Host "No .nupkg found for $assembly version $packageVersion" -ForegroundColor Yellow
        return
    }
    $pkg = $packages[0]
    $libraryPacksPath = Get-LibPacksContent
    if ([string]::IsNullOrWhiteSpace($libraryPacksPath)) {
        $libraryPacksPath = Get-DefaultLibraryPacksPath
    }

    if ([string]::IsNullOrWhiteSpace($libraryPacksPath)) {
        Write-Host "Library packs path not found. Skipping local package copy." -ForegroundColor Yellow
    } else {
        copy $pkg.FullName $libraryPacksPath -force
    }

    $apiKey = Get-NuGetApiKey
    if ($null -eq $apiKey) {
        Write-Host "CRITICAL: NuGet API Key file NOT found!" -ForegroundColor Red
        return
    }
    Write-Host "Pushing: $($pkg.Name) to nuget.org..."
    dotnet nuget push $pkg.FullName --api-key $apiKey --source https://api.nuget.org/v3/index.json --skip-duplicate
} else {
    Write-Host "Bin directory not found. Skipping push." -ForegroundColor Gray
}
