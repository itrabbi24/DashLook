# Changelog

All notable changes to DashLook will be documented here.

## [1.0.18] - 2026-03-17

### Fixed
- Added a dedicated Windows desktop-selection fallback using the desktop list view when shell automation does not resolve the selected item.
- Kept Space key handling fast while improving selected-file retries for preview opening.

## [1.0.17] - 2026-03-17

### Fixed
- Improved Space key detection by prioritizing Explorer process context detection.
- Added selected-file resolution retry window to reduce missed preview opens.
- Windows build verified with win-x64 publish artifact.
## [1.0.15] - 2026-03-17

### Fixed
- Space key trigger flow is now stable again by resolving selected file path on the app side instead of inside low-level keyboard hook callback.
- Added explorer-context focus check before handling Space to avoid missed trigger states.
- Improved update dialog button readability and interaction states (hover/disabled/cancel-download).
## [1.0.14] - 2026-03-17

### Fixed
- Desktop file selection now works with Space preview.
- Added desktop shell window matching fallback for Progman/WorkerW/SHELLDLL_DefView contexts.
## [1.0.13] - 2026-03-17

### Fixed
- Space key preview trigger now resolves selected file directly instead of relying on fragile Explorer class checks.
- Improved Explorer window matching using root window fallback for newer Explorer UI/tab contexts.
- Reduced duplicate file-resolution work in hotkey flow by carrying selected file path in the hotkey event payload.
## [1.0.0] — 2024-03-17

### Added
- Initial release
- Global Space key hook for File Explorer integration
- Plugin system with runtime DLL loading
- Image viewer (PNG, JPG, GIF, BMP, WebP, ICO, TIFF)
- Video/audio player via LibVLC
- PDF viewer via WebView2
- Syntax-highlighted text/code viewer via AvalonEdit
- Markdown renderer with dark theme
- Archive viewer (ZIP, RAR, 7Z, TAR) with search
- Font previewer with pangram samples
- HTML/SVG viewer via WebView2
- Dark Catppuccin-inspired theme
- System tray integration
- Single-instance enforcement
- WiX MSI installer
- GitHub Actions CI/CD pipeline





