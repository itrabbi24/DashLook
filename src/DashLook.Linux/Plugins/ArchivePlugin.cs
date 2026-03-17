using Avalonia.Controls;
using Avalonia.Layout;
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

        // Header row
        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("*,100,80") };
        header.Children.Add(MakeCell("Name",       0, bold: true));
        header.Children.Add(MakeCell("Size",       1, bold: true));
        header.Children.Add(MakeCell("Type",       2, bold: true));
        var headerBorder = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#313244")),
            Padding    = new Avalonia.Thickness(8, 4),
            Child      = header,
        };

        // File rows via ListBox
        var items = entries.Select(e => new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,100,80"),
            Children =
            {
                MakeCell((e.IsDir ? "📁  " : "📄  ") + e.Name, 0),
                MakeCell(e.IsDir ? "" : FormatSize(e.Size), 1),
                MakeCell(e.IsDir ? "Folder" : Path.GetExtension(e.Name).TrimStart('.').ToUpperInvariant(), 2),
            },
        }).ToList();

        var listBox = new ListBox
        {
            Background       = new SolidColorBrush(Color.Parse("#181825")),
            Foreground       = new SolidColorBrush(Color.Parse("#CDD6F4")),
            BorderThickness  = new Avalonia.Thickness(0),
            ItemsSource      = items,
            ItemTemplate     = new Avalonia.Controls.Templates.FuncDataTemplate<Grid>((g, _) => g),
        };

        var footer = new TextBlock
        {
            Text       = $"{entries.Count(e => !e.IsDir)} files, {entries.Count(e => e.IsDir)} folders",
            Foreground = new SolidColorBrush(Color.Parse("#A6ADC8")),
            FontSize   = 11,
            Margin     = new Avalonia.Thickness(8, 4),
        };

        var layout = new DockPanel { LastChildFill = true };
        DockPanel.SetDock(headerBorder, Dock.Top);
        DockPanel.SetDock(footer,       Dock.Bottom);
        layout.Children.Add(headerBorder);
        layout.Children.Add(footer);
        layout.Children.Add(listBox);

        string ext = Path.GetExtension(path).TrimStart('.').ToUpperInvariant();
        return new PreviewResult
        {
            Success  = true,
            Control  = layout,
            FileType = $"{ext} Archive — {entries.Count(e => !e.IsDir)} files",
            PreferredSize = (760, 560),
        };
    }

    public void Cleanup() { }

    private static TextBlock MakeCell(string text, int column, bool bold = false)
    {
        var tb = new TextBlock
        {
            Text              = text,
            Foreground        = new SolidColorBrush(Color.Parse(bold ? "#CBA6F7" : "#CDD6F4")),
            FontSize          = 12,
            FontWeight        = bold ? FontWeight.SemiBold : FontWeight.Normal,
            TextTrimming      = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center,
            Padding           = new Avalonia.Thickness(4, 2),
        };
        Grid.SetColumn(tb, column);
        return tb;
    }

    private static string FormatSize(long b) =>
        b < 1024 ? $"{b} B" :
        b < 1024 * 1024 ? $"{b / 1024.0:F1} KB" :
        $"{b / 1024.0 / 1024:F1} MB";
}
