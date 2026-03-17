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

> Select any file · Press `Space` · See it instantly — no waiting, no loading screens

<br/>

</div>

---

## What is DashLook?

macOS users have had Quick Look for years — click a file, press Space, you instantly see what's inside. Windows and Linux never had this built-in. **DashLook** brings that exact experience to both platforms.

It sits silently in your system tray and watches what you select in File Explorer. The moment you press `Space`, a sleek preview window pops up — for images, videos, PDFs, code, archives, fonts, and more — without ever opening an app. Press `Space` again and it's gone.

---

## What's New — v1.0.1

> Released March 2026

- **Fixed release assets** — all download files now correctly attach to GitHub releases
- **Auto-update system** — DashLook now checks for new versions silently in the background at startup and notifies you via a tray balloon when one is available
- **One-click update** — click the balloon or open the tray menu → *Check for Updates* to download and install the new version automatically without leaving the app
- **Release workflow improvements** — Windows EXE, Windows portable ZIP, Linux x64, and Linux ARM64 builds all published on every release
- **Stability fixes** — resolved edge cases in the preview window when switching files rapidly

> Previous release: **v1.0.0** — initial public release with all core features

---

## Features

### Core Experience

- 🚀 **Instant preview** — press `Space` in File Explorer, preview appears in under a second
- 🔁 **Toggle to close** — same `Space` key closes the preview; or press `Esc`
- 📌 **Pin window** — keep the preview floating on top of any other app
- ⌨️ **Keyboard first** — navigate, zoom, and close entirely without touching the mouse
- 🌙 **Dark theme** — elegant Catppuccin-inspired dark UI, easy on the eyes at any hour
- ⚡ **Starts with Windows** — DashLook lives in the system tray and auto-starts on login
- 🪶 **Lightweight** — under 50 MB, tiny memory footprint, zero background file scanning
- 🔄 **Auto-update** — notified automatically when a new version is available, one click to install

### File Type Support

- 🖼️ **Image viewer** — PNG, JPG, GIF, WebP, BMP, ICO, TIFF with pinch/scroll zoom
- 🎬 **Video player** — MP4, MKV, AVI, MOV and many more with full playback controls
- 🎵 **Audio player** — MP3, FLAC, WAV, AAC, OGG with metadata display
- 📕 **PDF viewer** — full multi-page rendering, scroll, zoom, page navigation
- 💻 **Code viewer** — syntax highlighting for 100+ programming languages, line numbers
- 📝 **Markdown viewer** — renders `.md` files as beautiful styled HTML with dark theme
- 📦 **Archive viewer** — browse ZIP, RAR, 7Z, TAR contents and file sizes with live search
- 🔤 **Font previewer** — see the font rendered at sizes 14–48 pt with full alphabet and character set
- 🌐 **HTML / SVG viewer** — renders HTML and SVG in a full Chromium-based engine
- 🔌 **Plugin system** — add support for any file type by dropping a `.dll` into the Plugins folder
- 🐧 **Linux native** — built with Avalonia UI, runs natively on Ubuntu, Debian, Fedora, and ARM

---

## Supported File Types

| Type | Formats |
|------|---------|
| 🖼️ Images | JPG, JPEG, PNG, GIF, BMP, WebP, ICO, TIFF, SVG |
| 🎬 Video | MP4, MKV, AVI, MOV, WMV, FLV, WebM, M4V, TS |
| 🎵 Audio | MP3, FLAC, WAV, AAC, OGG, M4A, WMA, OPUS, APE |
| 📕 PDF | PDF documents — all pages, smooth scroll, zoom |
| 💻 Code | C#, Python, JavaScript, TypeScript, Go, Rust, C++, C, Java, Kotlin, Swift, PHP, Ruby, Lua, Bash, PowerShell, and 90+ more |
| 📝 Markdown | MD, MDX — rendered with syntax highlighting and dark theme |
| 🏷️ Markup & Data | HTML, XML, JSON, YAML, TOML, CSV, XAML |
| 📦 Archives | ZIP, RAR, 7Z, TAR, GZ, BZ2, XZ, CBZ, CBR |
| 🔤 Fonts | TTF, OTF, WOFF, WOFF2 |
| 🌐 Web | HTML, HTM, SVG rendered in Chromium engine |

---

## Download & Install

Go to **[Releases](https://github.com/itrabbi24/DashLook/releases/latest)** and pick the file for your system:

### 🪟 Windows

| File | What it is |
|------|-----------|
| `DashLook-*-Setup.msi` | **Recommended** — full installer, adds Start Menu shortcut, auto-start on login |
| `DashLook-*-win-x64.exe` | Single EXE — download and double-click, nothing to install |
| `DashLook-*-windows-x64-portable.zip` | Portable ZIP — extract anywhere, run from USB drive |

> **Requirements:** Windows 10 or Windows 11 (64-bit)

### 🐧 Linux

| File | What it is |
|------|-----------|
| `DashLook-*-linux-x64.tar.gz` | For x64 — Ubuntu, Debian, Fedora, and most distros |
| `DashLook-*-linux-arm64.tar.gz` | For ARM64 — Raspberry Pi 4/5, Apple Silicon (via Rosetta), etc. |

```bash
# Download, extract, and make executable
tar -xzf DashLook-*-linux-x64.tar.gz
chmod +x DashLook

# Preview a file
./DashLook /path/to/photo.jpg
./DashLook document.pdf
./DashLook archive.zip
```

> **Requirements:** Ubuntu 20.04+ / Debian 11+ / Fedora 36+ (x64 or ARM64)

**Optional — add to PATH so you can run it from anywhere:**
```bash
sudo mv DashLook /usr/local/bin/
dashlook /path/to/any/file
```

**Optional — right-click "Preview" in Nautilus file manager:**
```bash
mkdir -p ~/.local/share/nautilus/scripts
echo '#!/bin/bash
dashlook "$1"' > ~/.local/share/nautilus/scripts/"Preview with DashLook"
chmod +x ~/.local/share/nautilus/scripts/"Preview with DashLook"
```

---

## How to Use

### Basic usage

1. Open **File Explorer** (Windows) or your file manager (Linux)
2. Click on any file to select it
3. Press **`Space`** — the preview opens instantly
4. Press **`Space`** again (or `Esc`) to close it
5. Select a different file while the preview is open — it switches automatically

That's it. No right-clicking, no menus, no loading.

### Keyboard shortcuts

| Key | What it does |
|-----|-------------|
| `Space` | Open preview / close preview (toggle) |
| `Esc` | Close preview |
| `Enter` | Open the file in its default app |
| `Ctrl + C` | Copy the file path to clipboard |
| `Mouse wheel` | Zoom in / out (images and PDFs) |
| `Ctrl + Mouse wheel` | Zoom in / out (code viewer) |

### Preview window controls

| Button | What it does |
|--------|-------------|
| 📌 Pin | Keep the preview window always on top |
| ↗ Open | Open the file in its default application |
| ✕ Close | Close the preview |
| Drag title bar | Move the preview window anywhere on screen |

### System tray menu

Right-click the DashLook icon in the system tray to access:

| Option | What it does |
|--------|-------------|
| *version number* | Shows the currently installed version (grayed) |
| **Check for Updates…** | Manually check if a new version is available |
| **Find new Plugins…** | Opens the plugins wiki page on GitHub |
| **Open Data Folder** | Opens the DashLook app data folder in Explorer |
| **Run at Startup** | Toggle whether DashLook starts with Windows |
| **Restart** | Restart the app |
| **Quit** | Exit DashLook completely |

### Auto-update

DashLook checks for updates silently 8 seconds after it starts. When a new version is found:

1. A **balloon notification** appears from the system tray
2. Click the balloon — an update dialog opens showing current vs. latest version, release notes, and publish date
3. Click **Download & Install** — DashLook downloads the new version with a progress bar and launches it automatically
4. DashLook restarts into the new version — done

You can also manually check any time via **right-click tray icon → Check for Updates…**

---

## Plugin System

DashLook supports third-party viewer plugins. A plugin is a single `.dll` file you drop into the `Plugins` folder next to `DashLook.exe`.

**Finding plugins:**
- Right-click the tray icon → **Find new Plugins…** opens the [plugin wiki](https://github.com/itrabbi24/DashLook/wiki/Plugins)

**Installing a plugin:**
1. Download the plugin `.dll`
2. Place it in the `Plugins` folder (right-click tray → **Open Data Folder** to find it)
3. Restart DashLook

**Built-in viewers** (always included, no plugins needed):

| Viewer | Handles |
|--------|---------|
| Image Viewer | PNG, JPG, GIF, WebP, BMP, ICO, TIFF |
| Video Player | MP4, MKV, AVI, MOV, WMV, and more |
| Audio Player | MP3, FLAC, WAV, OGG, AAC, and more |
| PDF Viewer | PDF files, all pages |
| Code Viewer | 100+ programming and markup languages |
| Markdown Viewer | MD and MDX files |
| Archive Viewer | ZIP, RAR, 7Z, TAR and variants |
| Font Previewer | TTF, OTF, WOFF, WOFF2 |
| HTML Viewer | HTML, HTM, SVG files |

---

## How It Works

DashLook runs as a background process in your system tray. Here's what happens under the hood:

1. **Global keyboard hook** — DashLook registers a system-wide keyboard listener using the Windows API (`WH_KEYBOARD_LL`). It listens only for the `Space` key and only acts when File Explorer is the focused window.

2. **File detection** — when Space is pressed, DashLook asks Windows Shell (via COM automation) which file is selected in the currently active File Explorer window.

3. **Plugin matching** — DashLook checks each installed viewer plugin to see which one can handle the file extension, picks the best match by priority, and hands the file off.

4. **Preview window** — a lightweight borderless window appears, loads the viewer control, and renders the file. The window is resizable, draggable, and can be pinned on top.

5. **Toggle** — pressing Space again closes the window. If you select a different file and press Space, the same window navigates to the new file without reopening.

On **Linux**, DashLook works differently — you pass a file path as a command-line argument and the preview window opens directly (file manager integration via scripts or right-click menu).

---

## FAQ

**Does it slow down my PC?**
No. DashLook uses almost no CPU when idle. It wakes up only when you press Space and goes back to sleep after you close the preview.

**Does it index or scan my files?**
Never. DashLook only reads a file when you explicitly select it and press Space. No background scanning, no indexing, no telemetry.

**Can I use it without installing?**
Yes — download the portable `.exe` or `.zip` version. Just run it, no installer needed.

**How do I uninstall?**
Just delete the `.exe` file. DashLook stores settings in `%AppData%\DashLook` — delete that folder too for a clean removal.

**Why doesn't it preview [some file type]?**
It might not be supported yet, or there may be a plugin for it. Check the [plugin wiki](https://github.com/itrabbi24/DashLook/wiki/Plugins) or open an issue on GitHub.

---

<div align="center">

Made with care by **[ARG RABBI](https://itrabbi24.github.io/)**

*If DashLook saves you time, a ⭐ on GitHub means a lot — thank you!*

</div>
