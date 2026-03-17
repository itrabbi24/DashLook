using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DashLook.Common;

namespace DashLook.Plugin.ImageViewer;

[ViewerPlugin("Image Viewer", "Previews PNG, JPG, GIF, BMP, WebP, ICO, TIFF, SVG images", "1.0.0")]
public sealed class ImageViewerPlugin : IViewer
{
    public int Priority => 10;

    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif",
        ".ico", ".webp", ".wdp", ".hdp", ".jfif",
    };

    private ImageViewerControl? _control;
    public UIElement? ViewerControl => _control;

    public bool CanHandle(string path) =>
        SupportedExtensions.Contains(Path.GetExtension(path));

    public async Task PrepareAsync(string path, ContextObject context, CancellationToken token)
    {
        context.FileType = $"{Path.GetExtension(path).TrimStart('.').ToUpperInvariant()} Image";

        BitmapSource? bitmap = null;
        string? errorMsg = null;

        await Task.Run(() =>
        {
            try
            {
                var frame = BitmapFrame.Create(
                    new Uri(path, UriKind.Absolute),
                    BitmapCreateOptions.IgnoreImageCache,
                    BitmapCacheOption.OnLoad);
                frame.Freeze();
                bitmap = frame;

                context.PreferredSize = new Size(
                    Math.Max(400, Math.Min(frame.PixelWidth  + 48, 1600)),
                    Math.Max(300, Math.Min(frame.PixelHeight + 80, 1000)));

                context.FileType = $"{Path.GetExtension(path).TrimStart('.').ToUpper()} Image  " +
                                   $"{frame.PixelWidth} × {frame.PixelHeight}";
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }
        }, token);

        token.ThrowIfCancellationRequested();

        if (errorMsg is not null)
            throw new PreviewNotSupportedException(errorMsg);

        _control = new ImageViewerControl(bitmap!);
    }

    public void Cleanup()
    {
        _control = null;
    }
}
