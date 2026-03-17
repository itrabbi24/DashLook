using System.IO;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;

namespace DashLook.Plugin.PdfViewer;

public partial class PdfViewerControl : UserControl
{
    private readonly string _pdfPath;

    public PdfViewerControl(string pdfPath)
    {
        InitializeComponent();
        _pdfPath = pdfPath;
        Loaded += async (_, _) => await InitWebViewAsync();
    }

    private async Task InitWebViewAsync()
    {
        await WebView.EnsureCoreWebView2Async();
        // Navigate to file:// URI — WebView2 renders PDFs natively via Chromium's built-in PDF viewer
        WebView.Source = new Uri(_pdfPath);
    }
}
