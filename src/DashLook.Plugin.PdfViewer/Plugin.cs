using System.IO;
using System.Windows;
using DashLook.Common;

namespace DashLook.Plugin.PdfViewer;

[ViewerPlugin("PDF Viewer", "Renders PDF documents using WebView2", "1.0.0")]
public sealed class PdfViewerPlugin : IViewer
{
    public int Priority => 10;

    private PdfViewerControl? _control;
    public UIElement? ViewerControl => _control;

    public bool CanHandle(string path) =>
        Path.GetExtension(path).Equals(".pdf", StringComparison.OrdinalIgnoreCase);

    public async Task PrepareAsync(string path, ContextObject context, CancellationToken token)
    {
        context.FileType     = "PDF Document";
        context.PreferredSize = new Size(900, 720);
        context.CanResize    = true;

        await Task.CompletedTask;
        _control = new PdfViewerControl(path);
    }

    public void Cleanup() => _control = null;
}
