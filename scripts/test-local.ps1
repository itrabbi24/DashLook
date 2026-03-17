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

Step "Build MSI installer" {
    dotnet build installer/DashLook.Installer.wixproj `
        --configuration Release `
        "-p:SolutionDir=$root\" `
        -p:Version=0.0.0 -v quiet
}

Write-Host ""
if ($failed) {
    Write-Host "One or more steps FAILED. Fix errors before pushing." -ForegroundColor Red
    exit 1
} else {
    Write-Host "All checks passed - safe to push." -ForegroundColor Green
    exit 0
}
