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

    private static readonly Dictionary<string, string> FileIcons = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".jpg", "🖼" }, { ".jpeg", "🖼" }, { ".png", "🖼" }, { ".gif", "🖼" },
        { ".webp", "🖼" }, { ".bmp", "🖼" }, { ".svg", "🖼" }, { ".ico", "🖼" },
        { ".mp4", "🎬" }, { ".mkv", "🎬" }, { ".avi", "🎬" }, { ".mov", "🎬" },
        { ".wmv", "🎬" }, { ".flv", "🎬" }, { ".webm", "🎬" },
        { ".mp3", "🎵" }, { ".flac", "🎵" }, { ".wav", "🎵" }, { ".aac", "🎵" },
        { ".ogg", "🎵" }, { ".m4a", "🎵" },
        { ".pdf", "PDF" }, { ".docx", "DOC" }, { ".doc", "DOC" }, { ".xlsx", "XLS" },
        { ".pptx", "PPT" }, { ".epub", "EPUB" },
        { ".cs", "{}" }, { ".py", "{}" }, { ".js", "{}" }, { ".ts", "{}" },
        { ".html", "<>" }, { ".css", "CSS" }, { ".json", "{}" }, { ".xml", "XML" },
        { ".zip", "ZIP" }, { ".rar", "ZIP" }, { ".7z", "ZIP" }, { ".tar", "ZIP" },
        { ".ttf", "Aa" }, { ".otf", "Aa" }, { ".woff", "Aa" },
        { ".txt", "TXT" }, { ".md", "MD" }, { ".log", "LOG" },
    };

    public PreviewWindow(string filePath, IViewer viewer, PluginManager pluginManager)
    {
        InitializeComponent();
        _pluginManager = pluginManager;
        CurrentFilePath = filePath;
        _currentViewer = viewer;
        UpdatePinButtonState();
        _ = LoadFileAsync(filePath, viewer);
    }

    private async Task LoadFileAsync(string filePath, IViewer viewer)
    {
        CurrentFilePath = filePath;
        _cts.Cancel();
        _cts.Dispose();
        _cts = new CancellationTokenSource();

        ViewerHost.Content = null;
        LoadingOverlay.Visibility = Visibility.Visible;
        ErrorOverlay.Visibility = Visibility.Collapsed;

        string fileName = Path.GetFileName(filePath);
        if (string.IsNullOrWhiteSpace(fileName))
            fileName = filePath;

        string ext = Path.GetExtension(filePath);
        bool isDirectory = Directory.Exists(filePath);

        TitleText.Text = fileName;
        FileTypeIcon.Text = isDirectory ? "📁" : FileIcons.TryGetValue(ext, out var icon) ? icon : "FILE";

        var context = new ContextObject
        {
            FilePath = filePath,
            FileType = isDirectory ? "Folder" : string.Empty,
        };

        if (!isDirectory)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                context.FileSize = ContextObject.FormatFileSize(fileInfo.Length);
            }
            catch
            {
            }
        }

        FileTypeText.Text = context.FileType;
        FileSizeText.Text = context.FileSize;

        try
        {
            LogService.Write($"Preparing viewer {viewer.GetType().FullName} for {filePath}");
            await viewer.PrepareAsync(filePath, context, _cts.Token);
            _cts.Token.ThrowIfCancellationRequested();
            ShowViewer(viewer, context);
            LogService.Write($"Viewer ready: {viewer.GetType().FullName}");
        }
        catch (OperationCanceledException)
        {
            LogService.Write($"Preview load canceled for {filePath}");
        }
        catch (PreviewNotSupportedException ex)
        {
            LogService.Write($"Preview not supported for {filePath}: {ex.Message}");
            ShowError(ex.Message);
        }
        catch (Exception ex)
        {
            LogService.Write($"Preview failed for {filePath}: {ex}");
            ShowError($"Preview failed: {ex.Message}");
        }
    }

    private void ShowViewer(IViewer viewer, ContextObject context)
    {
        LoadingOverlay.Visibility = Visibility.Collapsed;
        ViewerHost.Content = viewer.ViewerControl;

        if (!string.IsNullOrEmpty(context.FileType))
            FileTypeText.Text = context.FileType;

        FileSizeText.Text = context.FileSize;

        Width = Math.Max(MinWidth, Math.Min(context.PreferredSize.Width, SystemParameters.WorkArea.Width * 0.9));
        Height = Math.Max(MinHeight, Math.Min(context.PreferredSize.Height, SystemParameters.WorkArea.Height * 0.9));
        CenterOnScreen();
    }

    private void ShowError(string message)
    {
        LoadingOverlay.Visibility = Visibility.Collapsed;
        ErrorOverlay.Visibility = Visibility.Visible;
        ErrorText.Text = message;
    }

    public void NavigateTo(string filePath)
    {
        _currentViewer?.Cleanup();
        var viewer = _pluginManager.FindViewer(filePath);
        if (viewer is null)
        {
            ShowError("No plugin found for this file type.");
            return;
        }

        _currentViewer = viewer;
        _ = LoadFileAsync(filePath, viewer);
    }

    public void ClosePreview()
    {
        _cts.Cancel();
        _currentViewer?.Cleanup();
        Hide();
    }

    private void CenterOnScreen()
    {
        Left = (SystemParameters.WorkArea.Width - Width) / 2;
        Top = (SystemParameters.WorkArea.Height - Height) / 2;
    }

    private void UpdatePinButtonState()
    {
        PinButton.Content = Topmost ? "Unpin" : "Pin";
        PinButton.ToolTip = Topmost ? "Disable always-on-top" : "Keep this preview on top";
        PinButton.FontWeight = Topmost ? FontWeights.SemiBold : FontWeights.Normal;
    }

    private void TogglePinnedState()
    {
        Topmost = !Topmost;
        UpdatePinButtonState();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Escape or Key.Space)
        {
            ClosePreview();
            e.Handled = true;
        }
        else if (e.Key is Key.Enter or Key.Return)
        {
            OpenExternal_Click(sender, e);
            e.Handled = true;
        }
        else if (e.Key == Key.P)
        {
            TogglePinnedState();
            e.Handled = true;
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        else
            DragMove();
    }

    private void PinButton_Click(object sender, RoutedEventArgs e) => TogglePinnedState();

    private void OpenExternal_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(CurrentFilePath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => ClosePreview();

    protected override void OnClosed(EventArgs e)
    {
        _cts.Cancel();
        _currentViewer?.Cleanup();
        _cts.Dispose();
        base.OnClosed(e);
    }
}
