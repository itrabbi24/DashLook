using System.IO;
using System.Windows;
using DashLook.Common;

namespace DashLook.Plugin.FontViewer;

[ViewerPlugin("Font Viewer", "Previews TrueType and OpenType fonts with sample text", "1.0.0")]
public sealed class FontViewerPlugin : IViewer
{
    public int Priority => 10;

    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".ttf", ".otf", ".woff", ".woff2",
    };

    private FontViewerControl? _control;
    public UIElement? ViewerControl => _control;

    public bool CanHandle(string path) =>
        SupportedExtensions.Contains(Path.GetExtension(path));

    public async Task PrepareAsync(string path, ContextObject context, CancellationToken token)
    {
        context.FileType     = "Font File";
        context.PreferredSize = new Size(760, 560);

        await Task.CompletedTask;
        _control = new FontViewerControl(path);
    }

    public void Cleanup() => _control = null;
}
