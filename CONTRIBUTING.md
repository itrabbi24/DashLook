# Contributing to DashLook

Thanks for taking the time to contribute!

## Getting started

1. Fork the repo and clone it locally
2. Install [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
3. Open `DashLook.sln` in Visual Studio 2022 or VS Code
4. Run `dotnet build DashLook.sln` to verify everything compiles

## Reporting bugs

Use the [bug report template](.github/ISSUE_TEMPLATE/bug_report.md). Include:
- Your Windows version
- DashLook version
- The file type that failed
- What you expected vs what happened

## Adding a new plugin

The fastest way to contribute is adding support for a new file format:

1. Create a new project `src/DashLook.Plugin.XxxViewer/`
2. Reference `DashLook.Common`
3. Implement `IViewer` and decorate with `[ViewerPlugin]`
4. Add the project to `DashLook.sln`
5. Test with a few sample files
6. Open a PR

See any existing plugin (e.g. `ImageViewer`) for the full pattern.

## Code style

- Follow the existing C# style (no tabs, 4-space indent)
- Keep plugin DLLs self-contained — no shared state between plugins
- Plugins must not crash the host; catch all exceptions in `PrepareAsync`
- Prefer `async/await` over blocking calls

## Pull request checklist

- [ ] Builds without warnings (`dotnet build -c Release`)
- [ ] Tested on at least 3 sample files
- [ ] Plugin handles `CancellationToken` correctly
- [ ] `Cleanup()` releases all resources
