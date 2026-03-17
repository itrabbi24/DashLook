using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LibVLCSharp.Shared;
using LibVLCSharp.WPF;

namespace DashLook.Plugin.VideoViewer;

public partial class VideoViewerControl : UserControl
{
    private LibVLC? _libVlc;
    private MediaPlayer? _player;
    private Media? _media;
    private bool _isDragging = false;
    private DispatcherTimer? _timer;

    public VideoViewerControl(string path, bool isAudio)
    {
        InitializeComponent();
        Loaded += (_, _) => InitVlc(path, isAudio);
    }

    private void InitVlc(string path, bool isAudio)
    {
        Core.Initialize();
        _libVlc = new LibVLC();
        _player = new MediaPlayer(_libVlc);
        VideoView.MediaPlayer = _player;

        _media = new Media(_libVlc, path, FromType.FromPath);
        _player.Media = _media;

        if (isAudio)
        {
            AudioPlaceholder.Visibility = Visibility.Visible;
            AudioFileName.Text = Path.GetFileNameWithoutExtension(path);
        }

        _player.Volume = (int)VolumeSlider.Value;
        _player.Play();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _timer.Tick += UpdateProgress;
        _timer.Start();
    }

    private void UpdateProgress(object? sender, EventArgs e)
    {
        if (_player is null || _isDragging)
            return;

        ProgressSlider.Value = _player.Position;

        long total = _player.Length / 1000;
        long current = (long)(_player.Position * total);
        TimeText.Text = $"{current / 60}:{current % 60:D2} / {total / 60}:{total % 60:D2}";
        PlayPauseBtn.Content = _player.IsPlaying ? "Pause" : "Play";
    }

    private void PlayPause_Click(object sender, RoutedEventArgs e)
    {
        if (_player?.IsPlaying == true)
            _player.Pause();
        else
            _player?.Play();
    }

    private void Progress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_isDragging)
            return;

        _player!.Position = (float)e.NewValue;
    }

    private void Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_player is not null)
            _player.Volume = (int)e.NewValue;
    }

    public void DisposeMedia()
    {
        _timer?.Stop();
        _player?.Stop();
        _player?.Dispose();
        _media?.Dispose();
        _libVlc?.Dispose();
    }
}

