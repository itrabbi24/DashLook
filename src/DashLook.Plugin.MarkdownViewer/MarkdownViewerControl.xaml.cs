using System.Windows.Controls;

namespace DashLook.Plugin.MarkdownViewer;

public partial class MarkdownViewerControl : UserControl
{
    private readonly string _html;

    public MarkdownViewerControl(string html)
    {
        InitializeComponent();
        _html = html;
        Loaded += async (_, _) =>
        {
            await WebView.EnsureCoreWebView2Async();
            WebView.NavigateToString(_html);
        };
    }
}
