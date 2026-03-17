using System.IO;
using System.Windows;
using DashLook.Common;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;

namespace DashLook.Plugin.TextViewer;

[ViewerPlugin("Text / Code Viewer", "Syntax-highlighted preview for source code and plain text files", "1.0.0")]
public sealed class TextViewerPlugin : IViewer
{
    public int Priority => 5;

    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        // Plain text
        ".txt", ".log", ".csv", ".tsv", ".ini", ".cfg", ".conf",
        // Code
        ".cs", ".vb", ".fs", ".py", ".js", ".ts", ".jsx", ".tsx",
        ".java", ".kt", ".swift", ".go", ".rs", ".cpp", ".c", ".h",
        ".hpp", ".rb", ".php", ".lua", ".r", ".m", ".sh", ".bat",
        ".ps1", ".psm1", ".psd1",
        // Markup / data
        ".html", ".htm", ".xml", ".xaml", ".json", ".yaml", ".yml",
        ".toml", ".md", ".markdown", ".rst", ".css", ".scss", ".less",
        ".sql", ".graphql", ".proto", ".dockerfile",
    };

    private TextViewerControl? _control;
    public UIElement? ViewerControl => _control;

    public bool CanHandle(string path) =>
        SupportedExtensions.Contains(Path.GetExtension(path));

    public async Task PrepareAsync(string path, ContextObject context, CancellationToken token)
    {
        string ext = Path.GetExtension(path).TrimStart('.').ToUpperInvariant();
        context.FileType = $"{ext} File";
        context.PreferredSize = new Size(900, 640);

        string content = await File.ReadAllTextAsync(path, token);
        token.ThrowIfCancellationRequested();

        _control = new TextViewerControl(content, path);
    }

    public void Cleanup() => _control = null;
}
