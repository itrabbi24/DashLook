<div align="center">

<img src="assets/logo.svg" width="112" height="112" alt="DashLook logo"/>

# DashLook

Windows-first instant file preview. Select a file in File Explorer or on the desktop, then press `Space`.

[![Release](https://img.shields.io/github/v/release/itrabbi24/DashLook?style=flat-square&label=latest)](https://github.com/itrabbi24/DashLook/releases/latest)
[![Build](https://img.shields.io/github/actions/workflow/status/itrabbi24/DashLook/build.yml?style=flat-square&label=build)](https://github.com/itrabbi24/DashLook/actions)
[![Platform](https://img.shields.io/badge/Windows-10%20%2F%2011-0078D4?style=flat-square&logo=windows&logoColor=white)](https://github.com/itrabbi24/DashLook/releases/latest)
[![License](https://img.shields.io/github/license/itrabbi24/DashLook?style=flat-square)](LICENSE)

<img src="assets/logo-banner.svg" alt="DashLook banner"/>

</div>

## What it does

DashLook is a Quick Look-style preview app for Windows.

- Select any supported file in File Explorer or on the desktop
- Press `Space` to open a preview window
- Press `Space` again to close it
- Keep navigating and DashLook switches to the newly selected file

## What's new in v1.0.20

- built-in viewer plugin DLLs are now shipped next to the Windows executable so installed builds can actually preview files
- captured shell context is now carried from the hotkey hook into preview resolution
- desktop fallback stays available through the native desktop list view path
- Linux projects and Linux release artifacts removed from the repo and CI
- refreshed logo and banner assets for a cleaner Windows-first identity

## Supported preview types

- images: PNG, JPG, GIF, WebP, BMP, ICO, TIFF, SVG
- text and code: TXT, MD, JSON, XML, YAML, source files
- PDF documents
- archives: ZIP, RAR, 7Z, TAR
- fonts: TTF, OTF, WOFF, WOFF2
- HTML and SVG content
- video and audio through the installed viewer plugins

## Download

Get the latest Windows build from [Releases](https://github.com/itrabbi24/DashLook/releases/latest).

Available release files:

- `DashLook-*-Setup.exe`: installer with Start Menu entry
- `DashLook-*-win-x64.exe`: single-file portable executable
- `DashLook-*-windows-x64-portable.zip`: extracted portable folder build

## How to use

1. Start DashLook.
2. Select a file in File Explorer or on the desktop.
3. Press `Space`.
4. Press `Esc` or `Space` again to close the preview.

## Development

Build the Windows app locally:

```powershell
dotnet build src\DashLook\DashLook.csproj -c Release
```

Publish a Windows test build locally:

```powershell
dotnet publish src\DashLook\DashLook.csproj -c Release -r win-x64 --self-contained true -o dist\hotfix-win -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

## Notes

DashLook is inspired by the Quick Look interaction model on macOS and by mature Windows preview tools such as QuickLook, but the implementation in this repository stays native to this codebase.



