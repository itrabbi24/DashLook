# DashLook local build test - run before pushing.
# Usage: powershell scripts/test-local.ps1

$ErrorActionPreference = 'Stop'

$root = Split-Path $PSScriptRoot -Parent
Set-Location $root

$failed = $false
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$version = (Select-String -Path 'src/DashLook/DashLook.csproj' -Pattern '<Version>([^<]+)</Version>').Matches[0].Groups[1].Value
$tempRoot = Join-Path $root ("dist/local-check-$timestamp")
$publishDir = Join-Path $tempRoot 'win-setup'
$relativePublishDir = '..\' + $publishDir.Substring($root.Length + 1)
$installerVersion = "$version-local"

function Step([string]$name, [scriptblock]$action) {
    Write-Host ""
    Write-Host "==> $name" -ForegroundColor Cyan

    $global:LASTEXITCODE = 0
    $succeeded = $true

    try {
        & $action
    }
    catch {
        Write-Host $_ -ForegroundColor Red
        $succeeded = $false
    }

    if (-not $succeeded -or $LASTEXITCODE -ne 0) {
        Write-Host "FAILED: $name" -ForegroundColor Red
        $script:failed = $true
    }
    else {
        Write-Host "OK: $name" -ForegroundColor Green
    }
}

Step 'Prepare staging paths' {
    Stop-Process -Name DashLook -Force -ErrorAction SilentlyContinue
    Start-Sleep -Milliseconds 350
    Remove-Item -Recurse -Force $tempRoot -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $publishDir -Force | Out-Null
}

Step 'Build Windows app' {
    dotnet build src/DashLook/DashLook.csproj --configuration Release --no-incremental --disable-build-servers -v quiet
}

Step 'Publish Windows single-file for installer' {
    dotnet publish src/DashLook/DashLook.csproj `
        --configuration Release `
        --runtime win-x64 `
        --self-contained true `
        --output $publishDir `
        --disable-build-servers `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:Version=$version `
        -v quiet
}

Step 'Build Inno Setup installer' {
    $iscc = @(
        'C:\Program Files (x86)\Inno Setup 6\ISCC.exe',
        'C:\Program Files\Inno Setup 6\ISCC.exe'
    ) | Where-Object { Test-Path $_ } | Select-Object -First 1

    if (-not $iscc) {
        Write-Host 'Inno Setup not installed - skipping (CI will build it)' -ForegroundColor Yellow
        $global:LASTEXITCODE = 0
        return
    }

    & $iscc installer/DashLook.iss /DAppVersion=$installerVersion "/DPublishDir=$relativePublishDir" /Q
}

Step 'Shutdown dotnet build servers' {
    dotnet build-server shutdown | Out-Null
}

Write-Host ""
if ($failed) {
    Write-Host 'One or more steps FAILED. Fix errors before pushing.' -ForegroundColor Red
    exit 1
}

Write-Host "Local build output: $publishDir" -ForegroundColor DarkGray
Write-Host 'All checks passed - safe to push.' -ForegroundColor Green
exit 0
