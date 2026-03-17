using System.IO;
using System.Windows;
using DashLook.Common;

namespace DashLook.Plugin.VideoViewer;

[ViewerPlugin("Video / Audio Viewer", "Plays video and audio files using LibVLC", "1.0.0")]
public sealed class VideoViewerPlugin : IViewer
{
    public int Priority => 10;

    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        // Video
        ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm",
        ".m4v", ".3gp", ".3g2", ".ts", ".mts", ".m2ts",
        // Audio
        ".mp3", ".flac", ".wav", ".aac", ".ogg", ".m4a", ".wma",
        ".opus", ".ape", ".alac",
    };

    private VideoViewerControl? _control;
    public UIElement? ViewerControl => _control;

    public bool CanHandle(string path) =>
        SupportedExtensions.Contains(Path.GetExtension(path));

    public async Task PrepareAsync(string path, ContextObject context, CancellationToken token)
    {
        bool isAudio = IsAudioExtension(Path.GetExtension(path));
        string ext = Path.GetExtension(path).TrimStart('.').ToUpperInvariant();
        context.FileType = isAudio ? $"{ext} Audio" : $"{ext} Video";
        context.PreferredSize = isAudio ? new Size(600, 200) : new Size(960, 580);

        await Task.CompletedTask; // LibVLC init happens on UI thread in the control
        _control = new VideoViewerControl(path, isAudio);
    }

    public void Cleanup()
    {
        _control?.DisposeMedia();
        _control = null;
    }

    private static bool IsAudioExtension(string ext) =>
        ext.ToLowerInvariant() is ".mp3" or ".flac" or ".wav" or ".aac"
            or ".ogg" or ".m4a" or ".wma" or ".opus" or ".ape" or ".alac";
}
