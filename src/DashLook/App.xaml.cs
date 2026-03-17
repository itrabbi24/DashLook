using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DashLook.Services;
using DashLook.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using static DashLook.Services.AppTheme;

namespace DashLook;

public partial class App : Application
{
    private const int MaxRecentPreviewCount = 8;

    private readonly List<string> _recentPreviewPaths = [];
    private HotkeyManager? _hotkeyManager;
    private PluginManager? _pluginManager;
    private UpdateManager? _updateManager;
    private PreviewWindow? _previewWindow;
    private StartupBannerWindow? _startupBanner;
    private TaskbarIcon? _trayIcon;
    private MenuItem? _startupMenuItem;
    private MenuItem? _themeMenu;
    private MenuItem? _recentMenu;
    private DispatcherTimer? _selectionMonitorTimer;
    private string? _lastObservedSelectionPath;
    private bool _selectionSwitchInProgress;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mutex = new System.Threading.Mutex(true, "DashLook_SingleInstance_v1", out bool isNew);
        if (!isNew)
        {
            NativeMethods.BroadcastDashLookActivate();
            Shutdown();
            return;
        }

        ThemeManager.Initialize();
        ThemeManager.ThemeChanged += (_, _) => RefreshThemeMenuChecks();
        ShowStartupBanner();
        StartupManager.EnsureFirstRunSetup();

        string pluginsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DashLook", "Plugins");
        Directory.CreateDirectory(pluginsDir);
        _pluginManager = new PluginManager(pluginsDir);
        _pluginManager.LoadPlugins();

        _trayIcon = (TaskbarIcon)FindResource("TrayIcon");
        BuildTrayMenu();
        InitializeSelectionMonitor();

        _hotkeyManager = new HotkeyManager();
        _hotkeyManager.SpacePressed += OnSpacePressed;
        _hotkeyManager.Start();

        _updateManager = new UpdateManager();
        _updateManager.UpdateAvailable += OnUpdateAvailable;
        _ = Task.Run(async () =>
        {
            await Task.Delay(8000);
            await _updateManager.CheckAsync(silent: true);
        });

        _ = CompleteStartupBannerAsync();
    }

    private void ShowStartupBanner()
    {
        _startupBanner = new StartupBannerWindow();
        _startupBanner.Show();
    }

    private async Task CompleteStartupBannerAsync()
    {
        if (_startupBanner is null)
            return;

        _startupBanner.SetReadyState();
        await _startupBanner.DismissAsync();
        _startupBanner = null;
    }

    private void InitializeSelectionMonitor()
    {
        _selectionMonitorTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };
        _selectionMonitorTimer.Tick += SelectionMonitorTimer_Tick;
        _selectionMonitorTimer.Start();
    }

    private void SelectionMonitorTimer_Tick(object? sender, EventArgs e)
    {
        if (_selectionSwitchInProgress || _previewWindow is not { IsVisible: true } || _pluginManager is null)
            return;

        if (!FileExplorerHelper.TryCaptureSelectionContext(out var context))
            return;

        if (!FileExplorerHelper.TryGetSelectedFilePath(context, out var selectedPath) || string.IsNullOrWhiteSpace(selectedPath))
            return;

        if (string.Equals(_lastObservedSelectionPath, selectedPath, StringComparison.OrdinalIgnoreCase))
            return;

        _lastObservedSelectionPath = selectedPath;

        if (string.Equals(_previewWindow.CurrentFilePath, selectedPath, StringComparison.OrdinalIgnoreCase))
            return;

        if (_pluginManager.FindViewer(selectedPath) is null)
        {
            LogService.Write($"Selection changed to unsupported file; keeping current preview: {selectedPath}");
            return;
        }

        try
        {
            _selectionSwitchInProgress = true;
            LogService.Write($"Selection changed while preview is open. Switching to: {selectedPath}");
            OpenPreview(selectedPath);
        }
        finally
        {
            _selectionSwitchInProgress = false;
        }
    }

    private void BuildTrayMenu()
    {
        var menu = new ContextMenu();

        string ver = Assembly.GetExecutingAssembly().GetName().Version is { } v
            ? $"v{v.Major}.{v.Minor}.{v.Build}"
            : "v1.0.0";

        menu.Items.Add(new MenuItem
        {
            Header = ver,
            IsEnabled = false,
            FontWeight = FontWeights.SemiBold,
        });
        menu.Items.Add(new Separator());

        var updateItem = new MenuItem { Header = "Check for Updates..." };
        updateItem.Click += async (_, _) => await CheckForUpdatesManual();
        menu.Items.Add(updateItem);

        _recentMenu = new MenuItem { Header = "Recent Previews" };
        menu.Items.Add(_recentMenu);
        RefreshRecentMenu();

        var dataItem = new MenuItem { Header = "Open Data Folder" };
        dataItem.Click += (_, _) =>
        {
            string appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DashLook");
            Directory.CreateDirectory(appData);
            Process.Start(new ProcessStartInfo("explorer.exe", appData) { UseShellExecute = true });
        };
        menu.Items.Add(dataItem);

        _themeMenu = new MenuItem { Header = "Theme" };
        AddThemeItem(_themeMenu, "Dark", Dark);
        AddThemeItem(_themeMenu, "Light", Light);
        AddThemeItem(_themeMenu, "System", AppTheme.System);
        RefreshThemeMenuChecks();
        menu.Items.Add(_themeMenu);

        menu.Items.Add(new Separator());

        _startupMenuItem = new MenuItem
        {
            Header = "Run at Startup",
            IsCheckable = true,
            IsChecked = StartupManager.IsStartupEnabled(),
        };
        _startupMenuItem.Click += (_, _) =>
        {
            if (_startupMenuItem.IsChecked)
                StartupManager.EnableStartup();
            else
                StartupManager.DisableStartup();
        };
        menu.Items.Add(_startupMenuItem);

        var restartItem = new MenuItem { Header = "Restart" };
        restartItem.Click += (_, _) =>
        {
            string? exe = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName;
            if (!string.IsNullOrWhiteSpace(exe))
                Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true });
            ExitApp();
        };
        menu.Items.Add(restartItem);

        menu.Items.Add(new Separator());

        var quitItem = new MenuItem { Header = "Quit" };
        quitItem.Click += (_, _) => ExitApp();
        menu.Items.Add(quitItem);

        _trayIcon!.ContextMenu = menu;
        _trayIcon.TrayMouseDoubleClick += async (_, _) => await CheckForUpdatesManual();
    }

    private void AddThemeItem(MenuItem parent, string label, AppTheme theme)
    {
        var item = new MenuItem
        {
            Header = label,
            Tag = theme,
            IsCheckable = true,
            IsChecked = ThemeManager.Current == theme,
        };
        item.Click += (_, _) => ThemeManager.Apply(theme);
        parent.Items.Add(item);
    }

    private void RefreshThemeMenuChecks()
    {
        if (_themeMenu is null)
            return;

        foreach (var entry in _themeMenu.Items.OfType<MenuItem>())
            entry.IsChecked = entry.Tag is AppTheme theme && theme == ThemeManager.Current;
    }

    private void RememberRecentPreview(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        int existingIndex = _recentPreviewPaths.FindIndex(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
        if (existingIndex >= 0)
            _recentPreviewPaths.RemoveAt(existingIndex);

        _recentPreviewPaths.Insert(0, path);

        if (_recentPreviewPaths.Count > MaxRecentPreviewCount)
            _recentPreviewPaths.RemoveRange(MaxRecentPreviewCount, _recentPreviewPaths.Count - MaxRecentPreviewCount);

        RefreshRecentMenu();
    }

    private void RefreshRecentMenu()
    {
        if (_recentMenu is null)
            return;

        _recentMenu.Items.Clear();

        if (_recentPreviewPaths.Count == 0)
        {
            _recentMenu.Items.Add(new MenuItem
            {
                Header = "No recent previews yet",
                IsEnabled = false,
            });
            return;
        }

        foreach (string path in _recentPreviewPaths)
        {
            string label = Path.GetFileName(path);
            if (string.IsNullOrWhiteSpace(label))
                label = path;

            var item = new MenuItem
            {
                Header = label,
                ToolTip = path,
                Tag = path,
            };
            item.Click += (_, _) => OpenRecentPreview(path);
            _recentMenu.Items.Add(item);
        }
    }

    private void OpenRecentPreview(string path)
    {
        if (!File.Exists(path) && !Directory.Exists(path))
        {
            LogService.Write($"Recent preview target no longer exists: {path}");
            _recentPreviewPaths.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
            RefreshRecentMenu();
            return;
        }

        Dispatcher.Invoke(() =>
        {
            LogService.Write($"Opening recent preview target: {path}");
            OpenPreview(path);
        });
    }

    private void OnUpdateAvailable(object? sender, UpdateAvailableEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            _trayIcon?.ShowBalloonTip(
                "DashLook Update Available",
                $"Version {e.Info.LatestVersion} is ready. Click to install.",
                BalloonIcon.Info);

            _trayIcon!.TrayBalloonTipClicked += (_, _) => ShowUpdateDialog(e.Info);
        });
    }

    private async Task CheckForUpdatesManual()
    {
        if (_updateManager is null)
            return;

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

    private void OnSpacePressed(object? sender, SpacePressedEventArgs e)
    {
        _ = Dispatcher.BeginInvoke(() =>
        {
            if (!FileExplorerHelper.TryGetSelectedFilePathWithRetry(e.Context, out var filePath, attempts: 10, delayMs: 40))
            {
                LogService.Write($"Preview open skipped because selection could not be resolved. {FileExplorerHelper.DescribeContext(e.Context)}");
                return;
            }

            LogService.Write($"Opening preview for: {filePath}");
            OpenPreview(filePath!);
        });
    }

    private void OpenPreview(string filePath)
    {
        _lastObservedSelectionPath = filePath;

        if (_previewWindow is { IsVisible: true })
        {
            if (string.Equals(_previewWindow.CurrentFilePath, filePath, StringComparison.OrdinalIgnoreCase))
            {
                _previewWindow.ClosePreview();
                return;
            }

            if (_pluginManager!.FindViewer(filePath) is null)
            {
                LogService.Write($"No viewer found for: {filePath}");
                return;
            }

            RememberRecentPreview(filePath);
            _previewWindow.NavigateTo(filePath);
            return;
        }

        var viewer = _pluginManager!.FindViewer(filePath);
        if (viewer is null)
        {
            LogService.Write($"No viewer found for: {filePath}");
            return;
        }

        RememberRecentPreview(filePath);
        _previewWindow = new PreviewWindow(filePath, viewer, _pluginManager);
        _previewWindow.Show();
    }

    private void ExitApp()
    {
        _selectionMonitorTimer?.Stop();
        _hotkeyManager?.Stop();
        _startupBanner?.Close();
        _trayIcon?.Dispose();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _selectionMonitorTimer?.Stop();
        _hotkeyManager?.Stop();
        _startupBanner?.Close();
        _trayIcon?.Dispose();
        base.OnExit(e);
    }
}
