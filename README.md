<div align="center">

<img src="assets/logo.svg" width="100" height="100" alt="DashLook Logo"/>

# DashLook

**Instant file preview for Windows & Linux — just press `Space`**

[![Release](https://img.shields.io/github/v/release/itrabbi24/DashLook?style=flat-square&color=cba6f7&label=latest)](https://github.com/itrabbi24/DashLook/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/itrabbi24/DashLook/total?style=flat-square&color=89b4fa)](https://github.com/itrabbi24/DashLook/releases)
[![Build](https://img.shields.io/github/actions/workflow/status/itrabbi24/DashLook/build.yml?style=flat-square&label=build)](https://github.com/itrabbi24/DashLook/actions)
[![Platform](https://img.shields.io/badge/Windows-0078D4?style=flat-square&logo=windows&logoColor=white)](https://github.com/itrabbi24/DashLook/releases/latest)
[![Platform](https://img.shields.io/badge/Linux-FCC624?style=flat-square&logo=linux&logoColor=black)](https://github.com/itrabbi24/DashLook/releases/latest)
[![License](https://img.shields.io/github/license/itrabbi24/DashLook?style=flat-square&color=a6e3a1)](LICENSE)

<br/>

> Select any file · Press `Space` · See it instantly

<br/>

</div>

---

## What is DashLook?

macOS users have had Quick Look for years — select a file, press Space, done. Windows and Linux never had this built-in. **DashLook** gives it to you on both platforms.

It sits quietly in your system tray and gives you instant previews of images, videos, PDFs, code files, archives, fonts and more — without opening any app.

---

## Features

- 🚀 **Instant preview** — press `Space` in File Explorer / file manager, appears in under a second
- 🖼️ **Image viewer** — PNG, JPG, GIF, WebP, BMP, ICO, TIFF with zoom support
- 🎬 **Video player** — MP4, MKV, AVI, MOV and more with full playback controls
- 🎵 **Audio player** — MP3, FLAC, WAV, AAC, OGG with waveform display
- 📕 **PDF viewer** — full multi-page rendering, scroll, zoom
- 💻 **Code viewer** — syntax highlighting for 100+ languages with line numbers
- 📝 **Markdown viewer** — renders MD files as styled HTML with dark theme
- 📦 **Archive viewer** — browse ZIP, RAR, 7Z, TAR contents with live search
- 🔤 **Font previewer** — see the font rendered at sizes 14–48 with full alphabet
- 🌐 **HTML viewer** — renders HTML and SVG files in a Chromium engine
- 📌 **Pin window** — keep the preview on top of other windows
- ⌨️ **Keyboard first** — everything controllable without mouse
- 🔁 **Toggle preview** — press Space or Esc to close, same key to open
- 🌙 **Dark theme** — elegant dark UI, easy on the eyes
- 🔌 **Plugin system** — support any new file type by dropping a DLL in the Plugins folder
- ⚡ **Starts with Windows** — lives in the system tray, auto-starts on login
- 🪶 **Lightweight** — tiny memory footprint, no background scanning
- 🐧 **Linux support** — native Linux app built with Avalonia UI

---

## Supported Files

| Type | Formats |
|------|---------|
| 🖼️ Images | JPG, JPEG, PNG, GIF, BMP, WebP, ICO, TIFF, SVG |
| 🎬 Video | MP4, MKV, AVI, MOV, WMV, FLV, WebM, M4V, TS |
| 🎵 Audio | MP3, FLAC, WAV, AAC, OGG, M4A, WMA, OPUS, APE |
| 📕 PDF | PDF documents (all pages, zoom) |
| 💻 Code | C#, Python, JavaScript, TypeScript, Go, Rust, C++, C, Java, Kotlin, Swift, PHP, Ruby, Lua, Bash, PowerShell and 90+ more |
| 📝 Markdown | MD, MDX — rendered with syntax highlighting and dark theme |
| 🏷️ Markup & Data | HTML, XML, JSON, YAML, TOML, CSV, XAML |
| 📦 Archives | ZIP, RAR, 7Z, TAR, GZ, BZ2, XZ, CBZ, CBR |
| 🔤 Fonts | TTF, OTF, WOFF, WOFF2 |
| 🌐 Web | HTML, HTM, SVG files rendered in browser engine |

---

## Download & Install

Go to **[Releases](https://github.com/itrabbi24/DashLook/releases/latest)**:

### 🪟 Windows

| File | Notes |
|------|-------|
| `DashLook.exe` | Single EXE — just run it, no install needed |
| `DashLook-*-windows-x64.zip` | Portable ZIP with all files |
| `DashLook-Setup.msi` | Full installer, auto-starts on login |

> **Requirements:** Windows 10 or Windows 11 (64-bit)

### 🐧 Linux

```bash
# Download and extract
tar -xzf DashLook-1.0.0-linux-x64.tar.gz
chmod +x DashLook

# Preview a file
./DashLook /path/to/photo.jpg
./DashLook document.pdf
```

> **Requirements:** Ubuntu 20.04+ / Debian 11+ / Fedora 36+ (x64 or ARM64)

**File manager integration (Nautilus):**
```bash
# Add right-click "Preview with DashLook" to Nautilus
mkdir -p ~/.local/share/nautilus/scripts
echo '#!/bin/bash
/path/to/DashLook "$1"' > ~/.local/share/nautilus/scripts/Preview\ with\ DashLook
chmod +x ~/.local/share/nautilus/scripts/Preview\ with\ DashLook
```

---

## How to Use

| Key / Action | What it does |
|---|---|
| `Space` | Open or close the preview |
| `Esc` | Close the preview |
| `Enter` | Open file with default app |
| `Ctrl+C` | Copy file path to clipboard |
| Mouse wheel | Zoom in / out (images) |
| 📌 button | Pin window on top |
| ↗ button | Open with default app |
| Drag window | Move the preview anywhere |

---

<div align="center">

Made with ❤️ by **[ARG RABBI](https://itrabbi24.github.io/)**

*Like DashLook? Drop a ⭐ — it helps a lot!*

</div>
