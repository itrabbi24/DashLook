using System.Windows.Controls;

namespace DashLook.Plugin.HtmlViewer;

public partial class HtmlViewerControl : UserControl
{
    private readonly string _filePath;

    public HtmlViewerControl(string filePath)
    {
        InitializeComponent();
        _filePath = filePath;
        Loaded += async (_, _) =>
        {
            await WebView.EnsureCoreWebView2Async();
            WebView.Source = new Uri(_filePath);
        };
    }
}
