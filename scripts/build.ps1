<#
.SYNOPSIS
    Full DashLook build script — compiles all projects and packages the release.
.EXAMPLE
    .\scripts\build.ps1
    .\scripts\build.ps1 -Configuration Release -Runtime win-x64
#>

param(
    [string]$Configuration = "Release",
    [string]$Runtime       = "win-x64",
    [string]$Version       = "1.0.0",
    [switch]$SkipInstaller
)

$ErrorActionPreference = "Stop"
$Root = Split-Path $PSScriptRoot -Parent

Write-Host "=== DashLook Build Script ===" -ForegroundColor Cyan
Write-Host "Config  : $Configuration"
Write-Host "Runtime : $Runtime"
Write-Host "Version : $Version"
Write-Host ""

# 1. Restore
Write-Host "[1/4] Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore "$Root\DashLook.sln"
if ($LASTEXITCODE -ne 0) { throw "Restore failed" }

# 2. Build solution
Write-Host "[2/4] Building solution..." -ForegroundColor Yellow
dotnet build "$Root\DashLook.sln" `
    --configuration $Configuration `
    --no-restore `
    -p:Version=$Version
if ($LASTEXITCODE -ne 0) { throw "Build failed" }

# 3. Publish
$OutputDir = "$Root\publish\DashLook-$Version"
Write-Host "[3/4] Publishing to $OutputDir..." -ForegroundColor Yellow

dotnet publish "$Root\src\DashLook\DashLook.csproj" `
    --configuration $Configuration `
    --runtime $Runtime `
    --self-contained true `
    --output $OutputDir `
    -p:PublishSingleFile=false `
    -p:Version=$Version
if ($LASTEXITCODE -ne 0) { throw "Publish failed" }

# Copy plugin DLLs into Plugins\
$PluginsOut = "$OutputDir\Plugins"
New-Item -ItemType Directory -Force -Path $PluginsOut | Out-Null

$PluginProjects = @(
    "DashLook.Plugin.ImageViewer",
    "DashLook.Plugin.TextViewer",
    "DashLook.Plugin.VideoViewer",
    "DashLook.Plugin.PdfViewer",
    "DashLook.Plugin.MarkdownViewer",
    "DashLook.Plugin.ArchiveViewer",
    "DashLook.Plugin.FontViewer",
    "DashLook.Plugin.HtmlViewer"
)

foreach ($plugin in $PluginProjects) {
    $dll = "$Root\src\$plugin\bin\$Configuration\net9.0-windows\$plugin.dll"
    if (Test-Path $dll) {
        Copy-Item $dll $PluginsOut -Force
        Write-Host "  Copied: $plugin.dll" -ForegroundColor Green
    }
}

# 4. ZIP
Write-Host "[4/4] Creating ZIP archive..." -ForegroundColor Yellow
$ZipPath = "$Root\publish\DashLook-$Version-portable-$Runtime.zip"
Compress-Archive -Path "$OutputDir\*" -DestinationPath $ZipPath -Force
Write-Host "  Created: $ZipPath" -ForegroundColor Green

Write-Host ""
Write-Host "=== Build complete! ===" -ForegroundColor Cyan
Write-Host "Output : $OutputDir"
Write-Host "ZIP    : $ZipPath"
