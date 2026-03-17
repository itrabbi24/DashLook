using System.IO;
using System.Windows;
using DashLook.Common;
using Markdig;

namespace DashLook.Plugin.MarkdownViewer;

[ViewerPlugin("Markdown Viewer", "Renders Markdown files (.md, .markdown) as styled HTML", "1.0.0")]
public sealed class MarkdownViewerPlugin : IViewer
{
    public int Priority => 15; // Higher than TextViewer so .md files come here first

    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".md", ".markdown", ".mdown", ".mkd", ".mdx",
    };

    private MarkdownViewerControl? _control;
    public UIElement? ViewerControl => _control;

    public bool CanHandle(string path) =>
        SupportedExtensions.Contains(Path.GetExtension(path));

    public async Task PrepareAsync(string path, ContextObject context, CancellationToken token)
    {
        context.FileType     = "Markdown Document";
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

    private static string BuildHtmlPage(string body) => $$"""
        <!DOCTYPE html>
        <html>
        <head>
        <meta charset="utf-8"/>
        <style>
          body { background:#181825; color:#cdd6f4; font-family:'Segoe UI',sans-serif;
                 font-size:15px; line-height:1.7; padding:24px 32px; max-width:820px; margin:0 auto; }
          h1,h2,h3,h4 { color:#cba6f7; border-bottom:1px solid #313244; padding-bottom:4px; }
          a { color:#89b4fa; }
          code { background:#1e1e2e; color:#a6e3a1; padding:2px 6px; border-radius:4px;
                 font-family:'Cascadia Code','Consolas',monospace; font-size:13px; }
          pre  { background:#1e1e2e; padding:16px; border-radius:8px; overflow-x:auto; }
          pre code { background:none; padding:0; }
          blockquote { border-left:3px solid #cba6f7; margin:0; padding:0 16px; color:#a6adc8; }
          table { border-collapse:collapse; width:100%; }
          th,td { border:1px solid #313244; padding:8px 12px; }
          th { background:#1e1e2e; color:#cba6f7; }
          img { max-width:100%; border-radius:6px; }
          hr { border:none; border-top:1px solid #313244; }
        </style>
        </head>
        <body>{{body}}</body>
        </html>
        """;
}
