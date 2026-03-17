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

    public UpdateDialog(UpdateInfo info, UpdateManager manager)
    {
        InitializeComponent();
        _info    = info;
        _manager = manager;

        CurrentVersionText.Text  = $"v{info.CurrentVersion.Major}.{info.CurrentVersion.Minor}.{info.CurrentVersion.Build}";
        LatestVersionText.Text   = $"v{info.LatestVersion.Major}.{info.LatestVersion.Minor}.{info.LatestVersion.Build}";
        ReleaseNotesText.Text    = string.IsNullOrWhiteSpace(info.ReleaseNotes) ? "No release notes." : info.ReleaseNotes;
        PublishedText.Text       = $"Released {info.PublishedAt:MMM d, yyyy}";

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

        UpdateBtn.IsEnabled = false;
        SkipBtn.IsEnabled   = false;
        ProgressArea.Visibility = Visibility.Visible;
        ProgressText.Text = "Downloading update…";

        try
        {
            await _manager.DownloadAndInstallAsync(_info,
                progress =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ProgressBar.Value = progress;
                        ProgressText.Text = progress < 100
                            ? $"Downloading… {progress}%"
                            : "Installing — DashLook will restart…";
                    });
                },
                _cts.Token);
        }
        catch (OperationCanceledException)
        {
            ProgressArea.Visibility = Visibility.Collapsed;
            UpdateBtn.IsEnabled = true;
            SkipBtn.IsEnabled   = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Download failed: {ex.Message}\n\nYou can download manually from:\n{_info.ReleaseUrl}",
                "Update Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            UpdateBtn.IsEnabled = true;
            SkipBtn.IsEnabled   = true;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        _cts.Cancel();
        Close();
    }

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) DragMove();
    }
}
