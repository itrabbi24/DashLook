# Changelog

All notable changes to DashLook will be documented here.

## [1.0.20] - 2026-03-17

### Fixed
- Preserved built-in viewer plugin DLLs next to the published Windows executable so the installed app can actually discover and load viewers.
- Added plugin loading diagnostics to confirm which viewers are available at startup and why a viewer load fails.
- Kept the captured Explorer/Desktop shell-context path for `Space` preview resolution.

### Changed
- Kept the repo Windows-only and retained the refreshed branding assets.

## [1.0.19] - 2026-03-17

### Fixed
- Captured the Explorer/Desktop shell context at the exact `Space` key press and reuse that snapshot while resolving the selected file path.
- Added stronger diagnostics around hotkey capture, selection resolution, and preview open failures.
- Kept the native desktop list-view fallback for desktop selection when shell automation does not return a focused item.

### Changed
- Removed Linux projects, Linux workflows, and Linux release artifacts from the repository.
- Refreshed the public logo and banner assets to a Windows-first branding direction.

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
- Space key trigger flow is now stable again by resolving selected file path on the app side instead of inside the low-level keyboard hook callback.
- Added explorer-context focus checks before handling Space to avoid missed trigger states.
- Improved update dialog button readability and interaction states.

## [1.0.14] - 2026-03-17

### Fixed
- Desktop file selection now works with Space preview.
- Added desktop shell window matching fallback for Progman, WorkerW, and SHELLDLL_DefView contexts.

## [1.0.13] - 2026-03-17

### Fixed
- Space key preview trigger now resolves selected file directly instead of relying on fragile Explorer class checks.
- Improved Explorer window matching using a root window fallback for newer Explorer UI contexts.
- Reduced duplicate file-resolution work in the hotkey flow by carrying selected file path in the hotkey event payload.

## [1.0.0] - 2024-03-17

### Added
- Initial release.
- Global Space key hook for File Explorer integration.
- Plugin system with runtime DLL loading.
- Image viewer, video viewer, PDF viewer, text/code viewer, markdown viewer, archive viewer, font previewer, and HTML/SVG viewer.
- System tray integration and single-instance enforcement.
