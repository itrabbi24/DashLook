using Avalonia.Controls;
using Avalonia.Media;
using DashLook.Common.Cross;
using SharpCompress.Archives;

namespace DashLook.Linux.Plugins;

public sealed class ArchivePlugin : IPreviewPlugin
{
    public int Priority => 10;

    private static readonly HashSet<string> Ext = new(StringComparer.OrdinalIgnoreCase)
        { ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".xz", ".tgz", ".cbz", ".cbr" };

    public bool CanHandle(string path) => Ext.Contains(Path.GetExtension(path));

    public async Task<PreviewResult> PrepareAsync(string path, CancellationToken token)
    {
        var entries = await Task.Run(() =>
        {
            var list = new List<(string Name, long Size, bool IsDir)>();
            try
            {
                using var archive = ArchiveFactory.Open(path);
                foreach (var e in archive.Entries)
                    list.Add((e.Key ?? "", e.Size, e.IsDirectory));
            }
            catch { }
            return list;
        }, token);

        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly          = true,
            Background          = new SolidColorBrush(Color.Parse("#181825")),
            Foreground          = new SolidColorBrush(Color.Parse("#CDD6F4")),
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            HorizontalGridLinesBrush = new SolidColorBrush(Color.Parse("#313244")),
        };

        grid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new Avalonia.Data.Binding("Name"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
        grid.Columns.Add(new DataGridTextColumn { Header = "Size", Binding = new Avalonia.Data.Binding("SizeText"), Width = new DataGridLength(100) });

        grid.ItemsSource = entries.Select(e => new
        {
            Name     = (e.IsDir ? "📁 " : "📄 ") + e.Name,
            SizeText = e.IsDir ? "" : FormatSize(e.Size),
        }).ToList();

        string ext = Path.GetExtension(path).TrimStart('.').ToUpperInvariant();
        return new PreviewResult
        {
            Success  = true,
            Control  = grid,
            FileType = $"{ext} Archive — {entries.Count(e => !e.IsDir)} files",
            PreferredSize = (760, 560),
        };
    }

    public void Cleanup() { }

    private static string FormatSize(long b) =>
        b < 1024 ? $"{b} B" :
        b < 1024 * 1024 ? $"{b / 1024.0:F1} KB" :
        $"{b / 1024.0 / 1024:F1} MB";
}
