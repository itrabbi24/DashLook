using System.IO;
using System.Windows;
using DashLook.Common;
using ICSharpCode.SharpZipLib.Zip;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace DashLook.Plugin.ArchiveViewer;

[ViewerPlugin("Archive Viewer", "Lists contents of ZIP, RAR, 7Z, TAR, GZ archives", "1.0.0")]
public sealed class ArchiveViewerPlugin : IViewer
{
    public int Priority => 10;

    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".xz",
        ".tar.gz", ".tgz", ".tar.bz2", ".tar.xz", ".cbz", ".cbr",
    };

    private ArchiveViewerControl? _control;
    public UIElement? ViewerControl => _control;

    public bool CanHandle(string path) =>
        SupportedExtensions.Contains(Path.GetExtension(path));

    public async Task PrepareAsync(string path, ContextObject context, CancellationToken token)
    {
        string ext = Path.GetExtension(path).TrimStart('.').ToUpperInvariant();
        context.FileType     = $"{ext} Archive";
        context.PreferredSize = new Size(760, 560);

        var entries = await Task.Run(() => ReadEntries(path), token);
        token.ThrowIfCancellationRequested();

        _control = new ArchiveViewerControl(entries);
    }

    public void Cleanup() => _control = null;

    private static List<ArchiveEntry> ReadEntries(string path)
    {
        var result = new List<ArchiveEntry>();
        try
        {
            using var archive = ArchiveFactory.Open(path);
            foreach (var entry in archive.Entries)
            {
                result.Add(new ArchiveEntry
                {
                    Name         = entry.Key ?? "",
                    Size         = entry.Size,
                    CompressedSize = entry.CompressedSize,
                    IsDirectory  = entry.IsDirectory,
                    LastModified = entry.LastModifiedTime,
                });
            }
        }
        catch { /* return partial results */ }
        return result;
    }
}

public record ArchiveEntry
{
    public string   Name           { get; init; } = "";
    public long     Size           { get; init; }
    public long     CompressedSize { get; init; }
    public bool     IsDirectory    { get; init; }
    public DateTime? LastModified  { get; init; }
}
