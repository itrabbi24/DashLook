using System.Windows;

namespace DashLook.Common;

/// <summary>
/// Core interface every DashLook viewer plugin must implement.
/// Plugins are discovered at runtime from the Plugins\ directory.
/// </summary>
public interface IViewer
{
    /// <summary>
    /// Higher priority wins when multiple plugins claim the same extension.
    /// Built-in plugins use 0. Third-party plugins should use negative values.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Return true if this plugin can preview the given file path.
    /// Check the file extension (and optionally magic bytes) here.
    /// </summary>
    bool CanHandle(string path);

    /// <summary>
    /// Called before the preview window opens.
    /// Load data, decode the file, prepare the UI control here.
    /// Update context.Title, context.FileSize, etc.
    /// Throw PreviewNotSupportedException to fall through to the next plugin.
    /// </summary>
    Task PrepareAsync(string path, ContextObject context, CancellationToken token);

    /// <summary>
    /// The WPF control that renders the preview. Set after PrepareAsync completes.
    /// </summary>
    UIElement? ViewerControl { get; }

    /// <summary>
    /// Called when the preview window closes. Release resources here.
    /// </summary>
    void Cleanup();
}
