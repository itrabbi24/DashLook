// Stub implementations for plugins that need native dependencies
// Full implementations planned for v1.1
using DashLook.Common.Cross;

namespace DashLook.Linux.Plugins;

public sealed class PdfPlugin : IPreviewPlugin
{
    public int Priority => 10;
    public bool CanHandle(string path) =>
        path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);

    public Task<PreviewResult> PrepareAsync(string path, CancellationToken token) =>
        Task.FromResult(new PreviewResult
        {
            Success = false,
            Error   = "PDF preview requires WebView2/Chromium — planned for v1.1.\nOpen the file instead.",
        });

    public void Cleanup() { }
}

public sealed class VideoPlugin : IPreviewPlugin
{
    public int Priority => 10;

    private static readonly HashSet<string> Ext = new(StringComparer.OrdinalIgnoreCase)
        { ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm", ".mp3", ".flac", ".wav", ".aac" };

    public bool CanHandle(string path) => Ext.Contains(Path.GetExtension(path));

    public Task<PreviewResult> PrepareAsync(string path, CancellationToken token) =>
        Task.FromResult(new PreviewResult
        {
            Success = false,
            Error   = "Video/audio preview via LibVLC is planned for v1.1.\nOpen the file instead.",
        });

    public void Cleanup() { }
}

public sealed class FontPlugin : IPreviewPlugin
{
    public int Priority => 10;

    public bool CanHandle(string path) =>
        Path.GetExtension(path) is ".ttf" or ".otf" or ".woff" or ".woff2";

    public Task<PreviewResult> PrepareAsync(string path, CancellationToken token) =>
        Task.FromResult(new PreviewResult
        {
            Success = false,
            Error   = "Font preview is planned for v1.1.",
        });

    public void Cleanup() { }
}

public sealed class HtmlPlugin : IPreviewPlugin
{
    public int Priority => 10;

    public bool CanHandle(string path) =>
        Path.GetExtension(path) is ".html" or ".htm" or ".xhtml";

    public Task<PreviewResult> PrepareAsync(string path, CancellationToken token) =>
        Task.FromResult(new PreviewResult
        {
            Success = false,
            Error   = "HTML preview via WebView2 is planned for v1.1.",
        });

    public void Cleanup() { }
}
