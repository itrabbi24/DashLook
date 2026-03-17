using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using DashLook.Services;
using DashLook.Windows;
using static DashLook.Services.AppTheme;
using Hardcodet.Wpf.TaskbarNotification;

namespace DashLook;

public partial class App : Application
{
    private HotkeyManager?   _hotkeyManager;
    private PluginManager?   _pluginManager;
    private UpdateManager?   _updateManager;
    private PreviewWindow?   _previewWindow;
    private TaskbarIcon?     _trayIcon;
    private MenuItem?        _startupMenuItem;

    // ── Startup ───────────────────────────────────────────────────────────────

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Single-instance guard
        var mutex = new System.Threading.Mutex(true, "DashLook_SingleInstance_v1", out bool isNew);
        if (!isNew)
        {
            NativeMethods.BroadcastDashLookActivate();
            Shutdown();
            return;
        }

        // Load and apply saved theme (dark/light/system)
        ThemeManager.Initialize();

        // First-run setup (startup registry entry for portable)
        StartupManager.EnsureFirstRunSetup();

        // Plugins
        // Use AppData so plugins folder is writable even when installed to Program Files
        string pluginsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DashLook", "Plugins");
        Directory.CreateDirectory(pluginsDir);
        _pluginManager = new PluginManager(pluginsDir);
        _pluginManager.LoadPlugins();

        // Tray icon
        _trayIcon = (TaskbarIcon)FindResource("TrayIcon");
        BuildTrayMenu();

        // Keyboard hook
        _hotkeyManager = new HotkeyManager();
        _hotkeyManager.SpacePressed += OnSpacePressed;
        _hotkeyManager.Start();

        // Background update check (silent, on startup)
        _updateManager = new UpdateManager();
        _updateManager.UpdateAvailable += OnUpdateAvailable;
        _ = Task.Run(async () =>
        {
            await Task.Delay(8000); // wait 8s after launch before checking
            await _updateManager.CheckAsync(silent: true);
        });
    }

    // ── Tray menu — matches QuickLook's layout ────────────────────────────────

    private void BuildTrayMenu()
    {
        var menu = new ContextMenu();

        // Version label (grayed out, not clickable)
        string ver = Assembly.GetExecutingAssembly().GetName().Version is { } v
            ? $"v{v.Major}.{v.Minor}.{v.Build}"
            : "v1.0.0";

        var versionItem = new MenuItem
        {
            Header     = ver,
            IsEnabled  = false,
            FontWeight = System.Windows.FontWeights.SemiBold,
        };
        menu.Items.Add(versionItem);
        menu.Items.Add(new Separator());

        // Check for Updates…
        var updateItem = new MenuItem { Header = "Check for Updates…" };
        updateItem.Click += async (_, _) => await CheckForUpdatesManual();
        menu.Items.Add(updateItem);

        // Find new Plugins…
        var pluginsItem = new MenuItem { Header = "Find new Plugins…" };
        pluginsItem.Click += (_, _) =>
            Process.Start(new ProcessStartInfo("https://github.com/itrabbi24/DashLook/wiki/Plugins")
                { UseShellExecute = true });
        menu.Items.Add(pluginsItem);

        // Open Data Folder
        var dataItem = new MenuItem { Header = "Open Data Folder" };
        dataItem.Click += (_, _) =>
        {
            string appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DashLook");
            Directory.CreateDirectory(appData);
            Process.Start(new ProcessStartInfo("explorer.exe", appData) { UseShellExecute = true });
        };
        menu.Items.Add(dataItem);

        // Theme submenu
        var themeMenu = new MenuItem { Header = "Theme" };
        AddThemeItem(themeMenu, "Dark",   AppTheme.Dark);
        AddThemeItem(themeMenu, "Light",  AppTheme.Light);
        AddThemeItem(themeMenu, "System", AppTheme.System);
        menu.Items.Add(themeMenu);

        menu.Items.Add(new Separator());

        // Run at Startup (checkmark toggle)
        _startupMenuItem = new MenuItem
        {
            Header      = "Run at Startup",
            IsCheckable = true,
            IsChecked   = StartupManager.IsStartupEnabled(),
        };
        _startupMenuItem.Click += (_, _) =>
        {
            if (_startupMenuItem.IsChecked)
                StartupManager.EnableStartup();
            else
                StartupManager.DisableStartup();
        };
        menu.Items.Add(_startupMenuItem);

        // Restart
        var restartItem = new MenuItem { Header = "Restart" };
        restartItem.Click += (_, _) =>
        {
            string exe = Path.Combine(AppContext.BaseDirectory,
                AppDomain.CurrentDomain.FriendlyName + ".exe");
            Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true });
            ExitApp();
        };
        menu.Items.Add(restartItem);

        menu.Items.Add(new Separator());

        // Quit
        var quitItem = new MenuItem { Header = "Quit" };
        quitItem.Click += (_, _) => ExitApp();
        menu.Items.Add(quitItem);

        _trayIcon!.ContextMenu = menu;
        _trayIcon.TrayMouseDoubleClick += (_, _) => CheckForUpdatesManual();
    }

    private void AddThemeItem(MenuItem parent, string label, AppTheme theme)
    {
        var item = new MenuItem
        {
            Header      = label,
            IsCheckable = true,
            IsChecked   = ThemeManager.Current == theme,
        };
        item.Click += (_, _) =>
        {
            ThemeManager.Apply(theme);
            // Refresh checkmarks on all sibling items
            foreach (MenuItem sibling in parent.Items)
                sibling.IsChecked = sibling.Header?.ToString() == ThemeManager.Current.ToString();
        };
        parent.Items.Add(item);
    }

    // ── Update handling ───────────────────────────────────────────────────────

    private void OnUpdateAvailable(object? sender, UpdateAvailableEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            // Show balloon tip in tray
            _trayIcon?.ShowBalloonTip(
                "DashLook Update Available",
                $"Version {e.Info.LatestVersion} is ready. Click to install.",
                BalloonIcon.Info);

            _trayIcon!.TrayBalloonTipClicked += (_, _) => ShowUpdateDialog(e.Info);
        });
    }

    private async Task CheckForUpdatesManual()
    {
        if (_updateManager is null) return;

        var info = await _updateManager.CheckAsync(silent: false);
        Dispatcher.Invoke(() =>
        {
            if (info is null)
            {
                MessageBox.Show(
                    "You're already running the latest version of DashLook.",
                    "No Updates", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                ShowUpdateDialog(info);
            }
        });
    }

    private void ShowUpdateDialog(UpdateInfo info)
    {
        var dialog = new UpdateDialog(info, _updateManager!);
        dialog.Show();
    }

    // ── Space key handler ─────────────────────────────────────────────────────

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

    // ── Cleanup ───────────────────────────────────────────────────────────────

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
