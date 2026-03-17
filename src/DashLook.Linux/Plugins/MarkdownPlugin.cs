using Avalonia.Controls;
using DashLook.Common.Cross;
using Markdig;

namespace DashLook.Linux.Plugins;

public sealed class MarkdownPlugin : IPreviewPlugin
{
    public int Priority => 15;

    private static readonly HashSet<string> Ext = new(StringComparer.OrdinalIgnoreCase)
        { ".md", ".markdown", ".mdown", ".mkd", ".mdx" };

    public bool CanHandle(string path) => Ext.Contains(Path.GetExtension(path));

    public async Task<PreviewResult> PrepareAsync(string path, CancellationToken token)
    {
        string raw = await File.ReadAllTextAsync(path, token);
        token.ThrowIfCancellationRequested();

        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        string html  = Markdown.ToHtml(raw, pipeline);

        // Avalonia doesn't have a built-in WebView — show raw text with basic formatting
        // Full HTML rendering via a WebView package is a planned enhancement
        var tb = new TextBox
        {
            Text            = raw,
            IsReadOnly      = true,
            FontFamily      = new Avalonia.Media.FontFamily("Segoe UI,Ubuntu,sans-serif"),
            FontSize        = 14,
            Foreground      = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#CDD6F4")),
            Background      = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#181825")),
            AcceptsReturn   = true,
            TextWrapping    = Avalonia.Media.TextWrapping.Wrap,
            BorderThickness = new Avalonia.Thickness(0),
            Padding         = new Avalonia.Thickness(24),
        };

        var scroll = new ScrollViewer { Content = tb };

        return new PreviewResult
        {
            Success   = true,
            Control   = scroll,
            FileType  = "Markdown Document",
            PreferredSize = (860, 680),
        };
    }

    public void Cleanup() { }
}
