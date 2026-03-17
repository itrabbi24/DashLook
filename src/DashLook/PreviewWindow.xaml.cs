using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using DashLook.Common;
using DashLook.Services;

namespace DashLook;

public partial class PreviewWindow : Window
{
    private IViewer? _currentViewer;
    private readonly PluginManager _pluginManager;
    private CancellationTokenSource _cts = new();

    public string CurrentFilePath { get; private set; }

    // File type → emoji icon mapping
    private static readonly Dictionary<string, string> FileIcons = new(StringComparer.OrdinalIgnoreCase)
    {
        // Images
        { ".jpg",  "🖼" }, { ".jpeg", "🖼" }, { ".png", "🖼" }, { ".gif", "🖼" },
        { ".webp", "🖼" }, { ".bmp",  "🖼" }, { ".svg", "🖼" }, { ".ico", "🖼" },
        // Video
        { ".mp4",  "🎬" }, { ".mkv", "🎬" }, { ".avi", "🎬" }, { ".mov", "🎬" },
        { ".wmv",  "🎬" }, { ".flv", "🎬" }, { ".webm","🎬" },
        // Audio
        { ".mp3",  "🎵" }, { ".flac","🎵" }, { ".wav", "🎵" }, { ".aac", "🎵" },
        { ".ogg",  "🎵" }, { ".m4a", "🎵" },
        // Documents
        { ".pdf",  "📕" }, { ".docx","📝" }, { ".doc", "📝" }, { ".xlsx","📊" },
        { ".pptx", "📋" }, { ".epub","📚" },
        // Code
        { ".cs",   "💻" }, { ".py",  "💻" }, { ".js",  "💻" }, { ".ts",  "💻" },
        { ".html", "🌐" }, { ".css", "🎨" }, { ".json","{ }" }, { ".xml", "🔖" },
        // Archives
        { ".zip",  "📦" }, { ".rar", "📦" }, { ".7z",  "📦" }, { ".tar", "📦" },
        // Fonts
        { ".ttf",  "🔤" }, { ".otf", "🔤" }, { ".woff","🔤" },
        // Text
        { ".txt",  "📄" }, { ".md",  "📝" }, { ".log", "📋" },
    };

    public PreviewWindow(string filePath, IViewer viewer, PluginManager pluginManager)
    {
        InitializeComponent();
        CurrentFilePath = filePath;
        _currentViewer = viewer;
        _pluginManager = pluginManager;
        LoadFile(filePath, viewer);
    }

    private void LoadFile(string filePath, IViewer viewer)
    {
        CurrentFilePath = filePath;
        _cts.Cancel();
        _cts = new CancellationTokenSource();

        // Reset UI
        ViewerHost.Content = null;
        LoadingOverlay.Visibility = Visibility.Visible;
        ErrorOverlay.Visibility = Visibility.Collapsed;

        string fileName = Path.GetFileName(filePath);
        string ext = Path.GetExtension(filePath);

        TitleText.Text = fileName;
        FileTypeIcon.Text = FileIcons.TryGetValue(ext, out var icon) ? icon : "📄";

        var context = new ContextObject();
        context.FilePath = filePath;
        context.OnReady  = () => Dispatcher.Invoke(() => ShowViewer(viewer, context));
        context.OnError  = (msg) => Dispatcher.Invoke(() => ShowError(msg));

        // Get file size
        try
        {
            var fi = new FileInfo(filePath);
            context.FileSize = ContextObject.FormatFileSize(fi.Length);
        }
        catch { /* ignore */ }

        // Update status bar
        FileSizeText.Text = context.FileSize;

        _ = Task.Run(async () =>
        {
            try
            {
                await viewer.PrepareAsync(filePath, context, _cts.Token);
                context.OnReady?.Invoke();
            }
            catch (OperationCanceledException) { }
            catch (PreviewNotSupportedException ex)
            {
                context.OnError?.Invoke(ex.Message);
            }
            catch (Exception ex)
            {
                context.OnError?.Invoke($"Preview failed: {ex.Message}");
            }
        }, _cts.Token);
    }

    private void ShowViewer(IViewer viewer, ContextObject context)
    {
        LoadingOverlay.Visibility = Visibility.Collapsed;
        ViewerHost.Content = viewer.ViewerControl;

        if (!string.IsNullOrEmpty(context.FileType))
            FileTypeText.Text = context.FileType;

        if (!string.IsNullOrEmpty(context.FileSize))
            FileSizeText.Text = context.FileSize;

        Width  = Math.Max(MinWidth,  Math.Min(context.PreferredSize.Width,  SystemParameters.WorkArea.Width  * 0.9));
        Height = Math.Max(MinHeight, Math.Min(context.PreferredSize.Height, SystemParameters.WorkArea.Height * 0.9));
        CenterOnScreen();
    }

    private void ShowError(string message)
    {
        LoadingOverlay.Visibility = Visibility.Collapsed;
        ErrorOverlay.Visibility   = Visibility.Visible;
        ErrorText.Text = message;
    }

    public void NavigateTo(string filePath)
    {
        _currentViewer?.Cleanup();
        var viewer = _pluginManager.FindViewer(filePath);
        if (viewer is null) { ShowError("No plugin found for this file type."); return; }
        _currentViewer = viewer;
        LoadFile(filePath, viewer);
    }

    public void ClosePreview()
    {
        _cts.Cancel();
        _currentViewer?.Cleanup();
        Hide();
    }

    private void CenterOnScreen()
    {
        Left = (SystemParameters.WorkArea.Width  - Width)  / 2;
        Top  = (SystemParameters.WorkArea.Height - Height) / 2;
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Escape or Key.Space)
        {
            ClosePreview();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter || e.Key == Key.Return)
        {
            OpenExternal_Click(sender, e);
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2) WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal : WindowState.Maximized;
        else DragMove();
    }

    private void OpenExternal_Click(object sender, RoutedEventArgs e)
    {
        try { Process.Start(new ProcessStartInfo(CurrentFilePath) { UseShellExecute = true }); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => ClosePreview();

    protected override void OnClosed(EventArgs e)
    {
        _cts.Cancel();
        _currentViewer?.Cleanup();
        base.OnClosed(e);
    }
}
