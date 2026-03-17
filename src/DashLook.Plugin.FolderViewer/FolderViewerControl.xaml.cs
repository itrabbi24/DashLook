using System.Windows.Controls;

namespace DashLook.Plugin.FolderViewer;

public partial class FolderViewerControl : UserControl
{
    public FolderViewerControl(FolderPreviewInfo info)
    {
        InitializeComponent();

        FolderNameText.Text = info.Name;
        ModifiedText.Text = $"Last modified at {info.LastModifiedText}";
        SummaryText.Text = info.Summary;
        FolderCountText.Text = info.FolderCount.ToString("N0");
        FileCountText.Text = info.FileCount.ToString("N0");
        SizeText.Text = info.SizeText;
    }
}
