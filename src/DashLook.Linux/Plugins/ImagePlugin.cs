using Avalonia.Controls;
using Avalonia.Media.Imaging;
using DashLook.Common.Cross;

namespace DashLook.Linux.Plugins;

public sealed class ImagePlugin : IPreviewPlugin
{
    public int Priority => 10;

    private static readonly HashSet<string> Ext = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".ico", ".tiff", ".tif",
    };

    public bool CanHandle(string path) => Ext.Contains(Path.GetExtension(path));

    public async Task<PreviewResult> PrepareAsync(string path, CancellationToken token)
    {
        return await Task.Run(() =>
        {
            try
            {
                var bitmap = new Avalonia.Media.Imaging.Bitmap(path);
                var img    = new Image
                {
                    Source  = bitmap,
                    Stretch = Avalonia.Media.Stretch.Uniform,
                };

                var scroll = new ScrollViewer
                {
                    HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                    VerticalScrollBarVisibility   = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                    Content = img,
                };

                string ext = Path.GetExtension(path).TrimStart('.').ToUpperInvariant();
                return new PreviewResult
                {
                    Success  = true,
                    Control  = scroll,
                    FileType = $"{ext} Image — {bitmap.PixelSize.Width} × {bitmap.PixelSize.Height}",
                    PreferredSize = (
                        Math.Max(400, Math.Min(bitmap.PixelSize.Width  + 48, 1600)),
                        Math.Max(300, Math.Min(bitmap.PixelSize.Height + 80, 1000))
                    ),
                };
            }
            catch (Exception ex)
            {
                return new PreviewResult { Success = false, Error = ex.Message };
            }
        }, token);
    }

    public void Cleanup() { }
}
