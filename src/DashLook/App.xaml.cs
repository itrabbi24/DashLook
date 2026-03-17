using System.IO;
using System.Reflection;
using System.Windows;
using DashLook.Services;
using Hardcodet.Wpf.TaskbarNotification;

namespace DashLook;

public partial class App : Application
{
    private HotkeyManager? _hotkeyManager;
    private PluginManager? _pluginManager;
    private PreviewWindow? _previewWindow;
    private TaskbarIcon? _trayIcon;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Single-instance guard via named mutex
        var mutex = new System.Threading.Mutex(true, "DashLook_SingleInstance", out bool isNew);
        if (!isNew)
        {
            // Signal the existing instance and exit
            NativeMethods.BroadcastDashLookActivate();
            Shutdown();
            return;
        }

        // Register auto-start on first run (portable version)
        StartupManager.EnsureFirstRunSetup();

        // Ensure Plugins directory exists
        string pluginsDir = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Plugins");
        Directory.CreateDirectory(pluginsDir);

        // Bootstrap services
        _pluginManager = new PluginManager(pluginsDir);
        _pluginManager.LoadPlugins();

        _trayIcon = (TaskbarIcon)FindResource("TrayIcon");
        SetupTrayMenu();

        _hotkeyManager = new HotkeyManager();
        _hotkeyManager.SpacePressed += OnSpacePressed;
        _hotkeyManager.Start();
    }

    private void SetupTrayMenu()
    {
        var menu = new System.Windows.Controls.ContextMenu();

        var about = new System.Windows.Controls.MenuItem { Header = "About DashLook" };
        about.Click += (_, _) => ShowAbout();

        var exit = new System.Windows.Controls.MenuItem { Header = "Exit" };
        exit.Click += (_, _) => ExitApp();

        menu.Items.Add(about);
        menu.Items.Add(new System.Windows.Controls.Separator());
        menu.Items.Add(exit);

        _trayIcon!.ContextMenu = menu;
        _trayIcon.TrayMouseDoubleClick += (_, _) => ShowAbout();
    }

    private void OnSpacePressed(object? sender, SpacePressedEventArgs e)
    {
        string? filePath = FileExplorerHelper.GetSelectedFilePath();
        if (filePath is null) return;

        Dispatcher.Invoke(() => OpenPreview(filePath));
    }

    private void OpenPreview(string filePath)
    {
        if (_previewWindow is { IsVisible: true })
        {
            // Toggle: close if same file, navigate if different
            if (_previewWindow.CurrentFilePath == filePath)
            {
                _previewWindow.ClosePreview();
                return;
            }
            _previewWindow.NavigateTo(filePath);
            return;
        }

        var viewer = _pluginManager!.FindViewer(filePath);
        if (viewer is null) return;

        _previewWindow = new PreviewWindow(filePath, viewer, _pluginManager);
        _previewWindow.Show();
    }

    private static void ShowAbout()
    {
        MessageBox.Show(
            "DashLook v1.0\n\nPress Space in File Explorer to preview any file.\n\nhttps://github.com/RABBI-IT/DashLook",
            "About DashLook",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void ExitApp()
    {
        _hotkeyManager?.Stop();
        _trayIcon?.Dispose();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotkeyManager?.Stop();
        _trayIcon?.Dispose();
        base.OnExit(e);
    }
}
