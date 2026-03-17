using Avalonia.Controls;
using Avalonia.Media;
using DashLook.Common.Cross;

namespace DashLook.Linux.Plugins;

public sealed class TextPlugin : IPreviewPlugin
{
    public int Priority => 5;

    private static readonly HashSet<string> Ext = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".log", ".csv", ".ini", ".cfg", ".conf",
        ".cs", ".vb", ".py", ".js", ".ts", ".java", ".kt", ".go", ".rs",
        ".cpp", ".c", ".h", ".rb", ".php", ".lua", ".sh", ".bat", ".ps1",
        ".html", ".htm", ".xml", ".json", ".yaml", ".yml", ".toml",
        ".md", ".markdown", ".css", ".scss", ".sql",
    };

    public bool CanHandle(string path) => Ext.Contains(Path.GetExtension(path));

    public async Task<PreviewResult> PrepareAsync(string path, CancellationToken token)
    {
        string content = await File.ReadAllTextAsync(path, token);
        token.ThrowIfCancellationRequested();

        var tb = new TextBox
        {
            Text             = content,
            IsReadOnly       = true,
            FontFamily       = new FontFamily("Cascadia Code,Consolas,monospace"),
            FontSize         = 13,
            Foreground       = new SolidColorBrush(Color.Parse("#CDD6F4")),
            Background       = new SolidColorBrush(Color.Parse("#181825")),
            AcceptsReturn    = true,
            TextWrapping     = TextWrapping.NoWrap,
            BorderThickness  = new Avalonia.Thickness(0),
            Padding          = new Avalonia.Thickness(12),
        };

        var scroll = new ScrollViewer
        {
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility   = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Content = tb,
        };

        string ext = Path.GetExtension(path).TrimStart('.').ToUpperInvariant();
        return new PreviewResult
        {
            Success  = true,
            Control  = scroll,
            FileType = $"{ext} File",
        };
    }

    public void Cleanup() { }
}
