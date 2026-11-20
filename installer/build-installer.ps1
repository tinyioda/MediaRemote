#Requires -Version 7
<#
.SYNOPSIS
    Publishes MediaRemote as a self-contained win-x64 build and compiles the
    Inno Setup installer around it.

    This is a standalone packaging step, not part of the normal build. Run it
    by hand (or from a release pipeline) when you actually want to produce
    MediaRemoteSetup.exe — `dotnet build`, `dotnet run`, and F5/Debug or
    Release in Visual Studio never call this script.

.PARAMETER Version
    Version number to stamp on the installer (e.g. "1.2.0"). Defaults to
    "1.0.0" if omitted.
#>
param(
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

$installerDir = $PSScriptRoot
$repoRoot = Split-Path $installerDir -Parent
$csproj = Join-Path $repoRoot "src\MediaRemote.csproj"

Write-Host "Publishing self-contained win-x64 build..." -ForegroundColor Cyan
dotnet publish $csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

$iscc = Get-Command ISCC.exe -ErrorAction SilentlyContinue
if (-not $iscc) {
    $candidates = @(
        "$env:ProgramFiles (x86)\Inno Setup 6\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
        "$env:LocalAppData\Programs\Inno Setup 6\ISCC.exe"
    )
    $found = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
    if ($found) {
        $iscc = $found
    }
}
if (-not $iscc) {
    throw "Inno Setup's ISCC.exe was not found. Install Inno Setup 6 from https://jrsoftware.org/isdl.php and re-run this script."
}

$isccPath = if ($iscc -is [System.Management.Automation.CommandInfo]) { $iscc.Source } else { $iscc }

Write-Host "Compiling installer (version $Version)..." -ForegroundColor Cyan
& $isccPath "/DMyAppVersion=$Version" (Join-Path $installerDir "setup.iss")
if ($LASTEXITCODE -ne 0) {
    throw "ISCC.exe failed with exit code $LASTEXITCODE"
}

Write-Host "Done. Installer is in installer\Output\MediaRemoteSetup.exe" -ForegroundColor Green
