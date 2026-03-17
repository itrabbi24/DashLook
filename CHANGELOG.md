# Changelog

All notable changes to DashLook will be documented here.

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


