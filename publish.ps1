param(
    [switch]$FrameworkDependent = $false,
    [string]$OutputDir = ""
)

$ErrorActionPreference = "Stop"
$ProjectDir = Join-Path $PSScriptRoot "Fluxstrap"
$ProjectFile = Join-Path $ProjectDir "Fluxstrap.csproj"
$IsSelfContained = -not $FrameworkDependent

if ([string]::IsNullOrEmpty($OutputDir)) {
    $OutputDir = Join-Path $PSScriptRoot "Release"
}

Write-Host "=== Fluxstrap Publisher ===" -ForegroundColor Cyan
Write-Host "Project: $ProjectFile"
Write-Host "Output:  $OutputDir"
Write-Host "Mode:    $(if ($IsSelfContained) {'Self-contained (everything in one EXE)'} else {'Framework-dependent'})"
Write-Host ""

# Build arguments
$args = @(
    "publish", $ProjectFile,
    "-c", "Release",
    "-o", $OutputDir,
    "-p:PublishSingleFile=true",
    "-p:DebugType=none",
    "-p:DebugSymbols=false",
    "-p:IncludeNativeLibrariesForSelfExtract=true",
    "-p:IncludeAllContentForSelfExtract=true"
)

if ($IsSelfContained) {
    $args += "-r", "win-x64"
    $args += "-p:SelfContained=true"
}

Write-Host "Running: dotnet $($args -join ' ')" -ForegroundColor Yellow
Write-Host ""

& "dotnet" $args

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nPublish failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Verify output
$exe = Get-ChildItem -Path $OutputDir -Filter "Fluxstrap.exe" -Recurse | Select-Object -First 1
if ($exe) {
    $sizeInMB = [math]::Round($exe.Length / 1MB, 2)
    Write-Host "`n=== Success ===" -ForegroundColor Green
    Write-Host "Output: $($exe.FullName)"
    Write-Host "Size:   $sizeInMB MB"
    
    # List remaining files (should only be the EXE for single-file)
    $extra = Get-ChildItem -Path $OutputDir -Exclude "*.exe" -Recurse
    if ($extra) {
        Write-Host "`nAdditional files:" -ForegroundColor Yellow
        $extra | ForEach-Object { Write-Host "  $($_.Name) ($([math]::Round($_.Length/1KB,1)) KB)" }
    }
} else {
    Write-Host "`nWarning: Fluxstrap.exe not found in output!" -ForegroundColor Red
}
