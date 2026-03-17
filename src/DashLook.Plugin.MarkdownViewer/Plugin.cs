using System.IO;
using System.Windows;
using System.Windows.Media;
using DashLook.Common;
using Markdig;

namespace DashLook.Plugin.MarkdownViewer;

[ViewerPlugin("Markdown Viewer", "Renders Markdown files (.md, .markdown) as styled HTML", "1.0.0")]
public sealed class MarkdownViewerPlugin : IViewer
{
    public int Priority => 15;

    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".md", ".markdown", ".mdown", ".mkd", ".mdx"
    };

    private MarkdownViewerControl? _control;
    public UIElement? ViewerControl => _control;

    public bool CanHandle(string path) => SupportedExtensions.Contains(Path.GetExtension(path));

    public async Task PrepareAsync(string path, ContextObject context, CancellationToken token)
    {
        context.FileType = "Markdown Document";
        context.PreferredSize = new Size(860, 680);

        string markdown = await File.ReadAllTextAsync(path, token);
        token.ThrowIfCancellationRequested();

        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        string htmlBody = Markdown.ToHtml(markdown, pipeline);
        string fullHtml = BuildHtmlPage(htmlBody);

        _control = new MarkdownViewerControl(fullHtml);
    }

    public void Cleanup() => _control = null;

    private static string BuildHtmlPage(string body)
    {
        string page = BrushToHex("ContentBackground", "#181825");
        string card = BrushToHex("WindowBackground", "#1E1E2E");
        string border = BrushToHex("BorderColor", "#313244");
        string text = BrushToHex("TextPrimary", "#CDD6F4");
        string muted = BrushToHex("TextSecondary", "#A6ADC8");
        string accent = BrushToHex("AccentColor", "#CBA6F7");
        string accentLight = BrushToHex("AccentLight", "#89B4FA");
        string success = BrushToHex("SuccessColor", "#A6E3A1");

        return $$"""
        <!DOCTYPE html>
        <html>
        <head>
        <meta charset="utf-8"/>
        <style>
          body { background:{{page}}; color:{{text}}; font-family:'Segoe UI',sans-serif;
                 font-size:15px; line-height:1.7; padding:24px 32px; max-width:820px; margin:0 auto; }
          h1,h2,h3,h4 { color:{{accent}}; border-bottom:1px solid {{border}}; padding-bottom:4px; }
          a { color:{{accentLight}}; }
          code { background:{{card}}; color:{{success}}; padding:2px 6px; border-radius:4px;
                 font-family:'Cascadia Code','Consolas',monospace; font-size:13px; }
          pre  { background:{{card}}; padding:16px; border-radius:8px; overflow-x:auto; }
          pre code { background:none; padding:0; }
          blockquote { border-left:3px solid {{accent}}; margin:0; padding:0 16px; color:{{muted}}; }
          table { border-collapse:collapse; width:100%; }
          th,td { border:1px solid {{border}}; padding:8px 12px; }
          th { background:{{card}}; color:{{accent}}; }
          img { max-width:100%; border-radius:6px; }
          hr { border:none; border-top:1px solid {{border}}; }
        </style>
        </head>
        <body>{{body}}</body>
        </html>
        """;
    }

    private static string BrushToHex(string resourceKey, string fallback)
    {
        if (Application.Current?.Resources[resourceKey] is SolidColorBrush brush)
            return $"#{brush.Color.R:X2}{brush.Color.G:X2}{brush.Color.B:X2}";

        return fallback;
    }
}
