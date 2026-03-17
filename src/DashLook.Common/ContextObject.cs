using System.Windows;

namespace DashLook.Common;

/// <summary>
/// Passed to IViewer.PrepareAsync so plugins can communicate metadata
/// back to the host window and read user settings.
/// </summary>
public class ContextObject
{
    // ── File metadata (set by plugin) ────────────────────────────────────────

    /// <summary>Display title shown in the preview window title bar.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Human-readable file size string, e.g. "2.4 MB".</summary>
    public string FileSize { get; set; } = string.Empty;

    /// <summary>File type description, e.g. "PNG Image".</summary>
    public string FileType { get; set; } = string.Empty;

    /// <summary>True when the plugin has finished loading and the UI is ready.</summary>
    public bool IsReady { get; set; }

    // ── Sizing hints (set by plugin) ─────────────────────────────────────────

    /// <summary>Preferred window size. The host may clamp to screen bounds.</summary>
    public Size PreferredSize { get; set; } = new Size(900, 620);

    /// <summary>Whether the preview window can be resized by the user.</summary>
    public bool CanResize { get; set; } = true;

    // ── Host callbacks (provided by host) ────────────────────────────────────

    /// <summary>Call this when the viewer is fully loaded and ready to show.</summary>
    public Action? OnReady { get; set; }

    /// <summary>Call this to show an error message in the preview window.</summary>
    public Action<string>? OnError { get; set; }

    // ── Theme ────────────────────────────────────────────────────────────────

    /// <summary>True when Windows dark mode is active.</summary>
    public bool IsDarkTheme { get; set; } = true;

    // ── Raw path (read-only) ─────────────────────────────────────────────────

    public string FilePath { get; set; } = string.Empty;

    /// <summary>Helper: format bytes as KB / MB / GB string.</summary>
    public static string FormatFileSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024L * 1024 * 1024) return $"{bytes / 1024.0 / 1024:F1} MB";
        return $"{bytes / 1024.0 / 1024 / 1024:F2} GB";
    }
}
