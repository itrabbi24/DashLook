using System.Windows.Controls;
using DashLook.Common;

namespace DashLook.Plugin.ArchiveViewer;

public partial class ArchiveViewerControl : UserControl
{
    private readonly List<ArchiveEntryViewModel> _allItems;

    public ArchiveViewerControl(List<ArchiveEntry> entries)
    {
        InitializeComponent();
        _allItems = entries.Select(e => new ArchiveEntryViewModel(e)).ToList();
        FileList.ItemsSource = _allItems;
        FooterText.Text = $"{entries.Count(e => !e.IsDirectory)} files, {entries.Count(e => e.IsDirectory)} folders";
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string filter = SearchBox.Text.Trim();
        FileList.ItemsSource = string.IsNullOrEmpty(filter)
            ? _allItems
            : _allItems.Where(i => i.DisplayName.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}

public class ArchiveEntryViewModel
{
    private readonly ArchiveEntry _entry;

    public ArchiveEntryViewModel(ArchiveEntry entry) => _entry = entry;

    public string DisplayName => (_entry.IsDirectory ? "[Folder] " : "[File] ") + _entry.Name;
    public string SizeText => _entry.IsDirectory ? string.Empty : ContextObject.FormatFileSize(_entry.Size);
    public string CompressedText => _entry.IsDirectory ? string.Empty : ContextObject.FormatFileSize(_entry.CompressedSize);
    public string ModifiedText => _entry.LastModified?.ToString("yyyy-MM-dd HH:mm") ?? string.Empty;
}
