# DashLook local build test - run before pushing to catch errors early
# Usage: powershell scripts/test-local.ps1

$root = Split-Path $PSScriptRoot -Parent
Set-Location $root

$failed = $false

function Step([string]$name, [scriptblock]$action) {
    Write-Host ""
    Write-Host "==> $name" -ForegroundColor Cyan
    & $action
    if ($LASTEXITCODE -ne 0) {
        Write-Host "FAILED: $name" -ForegroundColor Red
        $script:failed = $true
    } else {
        Write-Host "OK: $name" -ForegroundColor Green
    }
}

Step "Build Windows app" {
    dotnet build src/DashLook/DashLook.csproj --configuration Release --no-incremental -v quiet
}

Step "Build Linux app" {
    dotnet build src/DashLook.Linux/DashLook.Linux.csproj --configuration Release --no-incremental -v quiet
}

Step "Publish Windows single-file for installer" {
    dotnet publish src/DashLook/DashLook.csproj `
        --configuration Release --runtime win-x64 --self-contained true `
        --output dist/win-setup -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true -p:Version=0.0.0 -v quiet
}

Step "Build Inno Setup installer" {
    $iscc = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
    if (-not (Test-Path $iscc)) {
        Write-Host "Inno Setup not installed locally - skipping installer build (CI will build it)" -ForegroundColor Yellow
        $global:LASTEXITCODE = 0
        return
    }
    & $iscc installer/DashLook.iss /DAppVersion=0.0.0 "/DPublishDir=..\dist\win-setup" /Q
}

Write-Host ""
if ($failed) {
    Write-Host "One or more steps FAILED. Fix errors before pushing." -ForegroundColor Red
    exit 1
} else {
    Write-Host "All checks passed - safe to push." -ForegroundColor Green
    exit 0
}
