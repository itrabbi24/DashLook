using System.IO;
using System.Windows;
using DashLook.Common;

namespace DashLook.Plugin.HtmlViewer;

[ViewerPlugin("HTML Viewer", "Renders HTML files using WebView2 (Chromium)", "1.0.0")]
public sealed class HtmlViewerPlugin : IViewer
{
    public int Priority => 10;

    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".html", ".htm", ".xhtml", ".svg",
    };

    private HtmlViewerControl? _control;
    public UIElement? ViewerControl => _control;

    public bool CanHandle(string path) =>
        SupportedExtensions.Contains(Path.GetExtension(path));

    public async Task PrepareAsync(string path, ContextObject context, CancellationToken token)
    {
        string ext = Path.GetExtension(path).TrimStart('.').ToUpperInvariant();
        context.FileType     = $"{ext} File";
        context.PreferredSize = new Size(1024, 720);

        await Task.CompletedTask;
        _control = new HtmlViewerControl(path);
    }

    public void Cleanup() => _control = null;
}
