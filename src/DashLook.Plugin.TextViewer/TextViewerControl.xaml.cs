using System.IO;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit.Highlighting;

namespace DashLook.Plugin.TextViewer;

public partial class TextViewerControl : UserControl
{
    public TextViewerControl(string content, string filePath)
    {
        InitializeComponent();

        // Set syntax highlighting based on file extension
        string ext = Path.GetExtension(filePath).TrimStart('.');
        var highlighting = HighlightingManager.Instance.GetDefinitionByExtension("." + ext);
        if (highlighting is not null)
            Editor.SyntaxHighlighting = highlighting;

        Editor.Text = content;
    }
}
