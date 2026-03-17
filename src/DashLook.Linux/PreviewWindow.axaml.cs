using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace DashLook.Linux;

public partial class PreviewWindow : Window
{
    private readonly PluginManager _pluginManager;
    public string CurrentFilePath { get; private set; }
    private bool _pinned;

    private static readonly Dictionary<string, string> FileIcons = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".jpg", "🖼" }, { ".jpeg", "🖼" }, { ".png", "🖼" }, { ".gif", "🖼" }, { ".svg", "🖼" },
        { ".mp4", "🎬" }, { ".mkv", "🎬" }, { ".avi", "🎬" }, { ".mov", "🎬" },
        { ".mp3", "🎵" }, { ".flac", "🎵" }, { ".wav", "🎵" },
        { ".pdf", "📕" },
        { ".md",  "📝" }, { ".txt", "📄" },
        { ".zip", "📦" }, { ".tar", "📦" }, { ".gz", "📦" },
        { ".ttf", "🔤" }, { ".otf", "🔤" },
        { ".html","🌐" }, { ".htm", "🌐" },
    };

    public PreviewWindow(string filePath, PluginManager pluginManager)
    {
        InitializeComponent();
        CurrentFilePath = filePath;
        _pluginManager = pluginManager;
        LoadFile(filePath);

        KeyDown += Window_KeyDown;
    }

    private void LoadFile(string filePath)
    {
        CurrentFilePath = filePath;
        ViewerHost.Content = null;
        LoadingOverlay.IsVisible = true;
        ErrorOverlay.IsVisible   = false;

        string ext = Path.GetExtension(filePath);
        TitleText.Text     = Path.GetFileName(filePath);
        FileTypeIcon.Text  = FileIcons.TryGetValue(ext, out var ico) ? ico : "📄";

        try
        {
            var fi = new FileInfo(filePath);
            FileSizeText.Text = FormatSize(fi.Length);
        }
        catch { }

        var cts = new CancellationTokenSource();
        _ = Task.Run(async () =>
        {
            var plugin = _pluginManager.FindPlugin(filePath);
            if (plugin is null)
            {
                ShowError("No preview available for this file type.");
                return;
            }

            var result = await plugin.PrepareAsync(filePath, cts.Token);

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!result.Success)
                {
                    ShowError(result.Error ?? "Preview failed.");
                    return;
                }

                LoadingOverlay.IsVisible = false;
                ViewerHost.Content = result.Control;
                FileTypeText.Text  = result.FileType;
                if (!string.IsNullOrEmpty(result.FileSize))
                    FileSizeText.Text = result.FileSize;

                Width  = result.PreferredSize.Width;
                Height = result.PreferredSize.Height;
            });
        }, cts.Token);
    }

    private void ShowError(string msg)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            LoadingOverlay.IsVisible = false;
            ErrorOverlay.IsVisible   = true;
            ErrorText.Text = msg;
        });
    }

    private void Window_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Escape) Close();
        else if (e.Key is Key.Enter) OpenExternal();
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private void Pin_Click(object? sender, RoutedEventArgs e)
    {
        _pinned = !_pinned;
        Topmost = _pinned;
        PinBtn.Foreground = _pinned
            ? new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#CBA6F7"))
            : new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#A6ADC8"));
    }

    private void OpenExternal_Click(object? sender, RoutedEventArgs e) => OpenExternal();
    private void Close_Click(object? sender, RoutedEventArgs e)       => Close();

    private void OpenExternal()
    {
        try { Process.Start(new ProcessStartInfo("xdg-open", $"\"{CurrentFilePath}\"") { UseShellExecute = true }); }
        catch { }
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / 1024.0 / 1024:F1} MB";
    }
}
