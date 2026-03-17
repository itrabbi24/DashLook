<div align="center">

<img src="assets/logo.svg" width="100" height="100" alt="DashLook Logo"/>

# DashLook

**Instant file preview for Windows — just press `Space`**

[![Release](https://img.shields.io/github/v/release/itrabbi24/DashLook?style=flat-square&color=cba6f7&label=latest)](https://github.com/itrabbi24/DashLook/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/itrabbi24/DashLook/total?style=flat-square&color=89b4fa)](https://github.com/itrabbi24/DashLook/releases)
[![Build](https://img.shields.io/github/actions/workflow/status/itrabbi24/DashLook/build.yml?style=flat-square&label=build)](https://github.com/itrabbi24/DashLook/actions)
[![License](https://img.shields.io/github/license/itrabbi24/DashLook?style=flat-square&color=a6e3a1)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-0078D4?style=flat-square&logo=windows)](https://github.com/itrabbi24/DashLook)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com)

<br/>

> Select any file in Windows Explorer · Press `Space` · See it instantly

<br/>

</div>

---

## What is DashLook?

macOS users have had Quick Look for years — select a file, press Space, preview it instantly. Windows never had this built-in. **DashLook** changes that.

It sits in your system tray, hooks into File Explorer, and gives you immediate previews of images, videos, PDFs, code files, archives, fonts, and more — without ever opening a separate app.

---

## Supported File Types

| Category | Formats | Icon |
|----------|---------|------|
| **Images** | JPG, PNG, GIF, BMP, WebP, ICO, TIFF, SVG | 🖼️ |
| **Video** | MP4, MKV, AVI, MOV, WMV, FLV, WebM | 🎬 |
| **Audio** | MP3, FLAC, WAV, AAC, OGG, M4A, WMA | 🎵 |
| **Documents** | PDF, EPUB | 📕 |
| **Office** | DOCX, XLSX, PPTX *(via plugin)* | 📝 |
| **Code** | C#, Python, JS, TS, Go, Rust, C++, 100+ | 💻 |
| **Markup** | HTML, XML, JSON, YAML, TOML, CSV | 🌐 |
| **Markdown** | MD, MDX, Markdown | 📝 |
| **Archives** | ZIP, RAR, 7Z, TAR, GZ, BZ2, CBZ | 📦 |
| **Fonts** | TTF, OTF, WOFF, WOFF2 | 🔤 |

---

## Install

### Option 1 — Portable (recommended for trying out)
1. Go to [Releases](https://github.com/itrabbi24/DashLook/releases/latest)
2. Download `DashLook-x.x.x-portable-win-x64.zip`
3. Extract, run `DashLook.exe`

### Option 2 — Installer
1. Download `DashLook-Setup.msi` from [Releases](https://github.com/itrabbi24/DashLook/releases/latest)
2. Run the installer
3. DashLook starts automatically on login

### Option 3 — Build from source
```powershell
git clone https://github.com/itrabbi24/DashLook
cd DashLook
.\scripts\build.ps1
```

---

## How to Use

| Action | Shortcut |
|--------|----------|
| Preview selected file | `Space` |
| Close preview | `Space` or `Esc` |
| Open with default app | `Enter` |
| Zoom in/out (images) | Mouse wheel |
| Play / Pause (video) | Click play button |
| Navigate | Mouse or keyboard |

---

## Plugin System

DashLook is built around a **plugin architecture** — every file type is handled by its own plugin DLL. Adding new format support is as simple as dropping a DLL into the `Plugins\` folder.

### Built-in plugins

| Plugin | Handles |
|--------|---------|
| `DashLook.Plugin.ImageViewer` | Images (PNG, JPG, GIF, WebP, BMP, ICO…) |
| `DashLook.Plugin.VideoViewer` | Video and audio (uses LibVLC) |
| `DashLook.Plugin.PdfViewer` | PDF documents (WebView2) |
| `DashLook.Plugin.TextViewer` | Source code + plain text (AvalonEdit) |
| `DashLook.Plugin.MarkdownViewer` | Markdown with syntax highlighting |
| `DashLook.Plugin.ArchiveViewer` | ZIP, RAR, 7Z, TAR file listing |
| `DashLook.Plugin.FontViewer` | Font preview with pangram samples |
| `DashLook.Plugin.HtmlViewer` | HTML/SVG files (WebView2) |

### Writing your own plugin

Implement `IViewer` from `DashLook.Common`, decorate with `[ViewerPlugin]`, compile to a DLL, and drop it in `Plugins\`. That's it.

```csharp
[ViewerPlugin("My Viewer", "Previews .xyz files")]
public class XyzViewer : IViewer
{
    public int Priority => 5;
    public bool CanHandle(string path) => path.EndsWith(".xyz");
    public UIElement? ViewerControl { get; private set; }

    public async Task PrepareAsync(string path, ContextObject ctx, CancellationToken ct)
    {
        ViewerControl = new XyzControl(path);
    }

    public void Cleanup() { /* release resources */ }
}
```

---

## Building

### Requirements
- Windows 10 / 11
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Visual Studio 2022 or VS Code with C# Dev Kit

### Quick build
```powershell
dotnet build DashLook.sln -c Release
```

### Full release build (includes portable ZIP)
```powershell
.\scripts\build.ps1 -Version 1.0.0
```

---

## Project Structure

```
DashLook/
├── src/
│   ├── DashLook/                    ← Main WPF application
│   ├── DashLook.Common/             ← Plugin interface (IViewer, ContextObject)
│   ├── DashLook.Plugin.ImageViewer/ ← Image preview
│   ├── DashLook.Plugin.TextViewer/  ← Code / text preview
│   ├── DashLook.Plugin.VideoViewer/ ← Video / audio player
│   ├── DashLook.Plugin.PdfViewer/   ← PDF viewer
│   ├── DashLook.Plugin.MarkdownViewer/
│   ├── DashLook.Plugin.ArchiveViewer/
│   ├── DashLook.Plugin.FontViewer/
│   └── DashLook.Plugin.HtmlViewer/
├── installer/                       ← WiX MSI installer
├── assets/                          ← Logo, icons, screenshots
├── scripts/                         ← Build & release scripts
└── .github/workflows/               ← CI/CD (GitHub Actions)
```

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| UI framework | WPF (.NET 9) |
| Video / audio | LibVLCSharp |
| PDF / HTML | WebView2 (Chromium) |
| Code syntax | AvalonEdit |
| Markdown | Markdig |
| Archives | SharpCompress |
| Installer | WiX Toolset v5 |
| CI/CD | GitHub Actions |

---

## Contributing

Pull requests are welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) first.

- For bugs: [Open an issue](https://github.com/itrabbi24/DashLook/issues/new?template=bug_report.md)
- For features: Start a discussion first
- For plugins: PRs adding new format support are always appreciated

---

## License

DashLook is released under the [GPL v3 License](LICENSE).

---

<div align="center">

Made with ❤️ by **[ARG RABBI](https://itrabbi24.github.io/)**

*If DashLook saves you time, consider starring the repo ⭐*

</div>
