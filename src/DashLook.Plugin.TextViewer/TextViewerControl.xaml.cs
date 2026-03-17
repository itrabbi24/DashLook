using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace DashLook.Plugin.TextViewer;

public partial class TextViewerControl : UserControl
{
    // Custom highlighting definitions bundled with this plugin
    private static readonly Dictionary<string, IHighlightingDefinition> _custom = new(StringComparer.OrdinalIgnoreCase);

    static TextViewerControl()
    {
        RegisterEmbedded(".bat", "DashLook.Plugin.TextViewer.Highlighting.Batch.xshd");
        RegisterEmbedded(".cmd", "DashLook.Plugin.TextViewer.Highlighting.Batch.xshd");
    }

    private static void RegisterEmbedded(string extension, string resourceName)
    {
        try
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (stream is null) return;
            using var reader = new XmlTextReader(stream);
            _custom[extension] = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }
        catch { /* silently ignore — file will still preview without highlighting */ }
    }

    public TextViewerControl(string content, string filePath)
    {
        InitializeComponent();

        string ext = Path.GetExtension(filePath);

        // Try custom definitions first, then fall back to AvalonEdit built-ins
        IHighlightingDefinition? highlighting =
            _custom.TryGetValue(ext, out var custom)
                ? custom
                : HighlightingManager.Instance.GetDefinitionByExtension(ext);

        if (highlighting is not null)
            Editor.SyntaxHighlighting = highlighting;

        Editor.Text = content;
    }
}
