using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using DashLook.Services;

namespace DashLook.Windows;

public partial class UpdateDialog : Window
{
    private readonly UpdateInfo _info;
    private readonly UpdateManager _manager;
    private CancellationTokenSource _cts = new();
    private bool _isDownloading;

    public UpdateDialog(UpdateInfo info, UpdateManager manager)
    {
        InitializeComponent();
        _info = info;
        _manager = manager;

        CurrentVersionText.Text = $"v{info.CurrentVersion.Major}.{info.CurrentVersion.Minor}.{info.CurrentVersion.Build}";
        LatestVersionText.Text = $"v{info.LatestVersion.Major}.{info.LatestVersion.Minor}.{info.LatestVersion.Build}";
        ReleaseNotesText.Text = string.IsNullOrWhiteSpace(info.ReleaseNotes) ? "No release notes." : info.ReleaseNotes;
        PublishedText.Text = $"Released {info.PublishedAt:MMM d, yyyy}";

        if (!info.HasInstaller)
        {
            UpdateBtn.Content = "Open Release Page";
        }
    }

    private async void Update_Click(object sender, RoutedEventArgs e)
    {
        if (!_info.HasInstaller)
        {
            Process.Start(new ProcessStartInfo(_info.ReleaseUrl) { UseShellExecute = true });
            Close();
            return;
        }

        _isDownloading = true;
        UpdateBtn.IsEnabled = false;
        UpdateBtn.Content = "Downloading...";
        SkipBtn.IsEnabled = true;
        SkipBtn.Content = "Cancel download";
        ProgressArea.Visibility = Visibility.Visible;
        ProgressText.Text = "Downloading update...";

        try
        {
            await _manager.DownloadAndInstallAsync(_info,
                progress =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ProgressBar.Value = progress;
                        ProgressText.Text = progress < 100
                            ? $"Downloading... {progress}%"
                            : "Installing - DashLook will restart...";
                    });
                },
                _cts.Token);
        }
        catch (OperationCanceledException)
        {
            ResetActionState();
            ProgressArea.Visibility = Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Download failed: {ex.Message}\n\nYou can download manually from:\n{_info.ReleaseUrl}",
                "Update Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            ResetActionState();
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        if (_isDownloading)
        {
            _cts.Cancel();
            return;
        }

        Close();
    }

    private void ResetActionState()
    {
        _isDownloading = false;
        _cts.Dispose();
        _cts = new CancellationTokenSource();

        UpdateBtn.IsEnabled = true;
        UpdateBtn.Content = _info.HasInstaller ? "Download & Install" : "Open Release Page";
        SkipBtn.IsEnabled = true;
        SkipBtn.Content = "Skip this version";
    }

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) DragMove();
    }
}
