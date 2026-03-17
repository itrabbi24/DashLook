using System.Runtime.InteropServices;
using System.Text;

namespace DashLook.Services;

/// <summary>
/// Detects the active File Explorer window and retrieves the path of
/// the currently selected file using the Shell Automation Object Model.
/// </summary>
public static class FileExplorerHelper
{
    // Class names of windows we consider "file managers"
    private static readonly HashSet<string> ExplorerClasses = new(StringComparer.OrdinalIgnoreCase)
    {
        "CabinetWClass",        // File Explorer window
        "ExploreWClass",        // Legacy explorer
        "Progman",              // Desktop
        "WorkerW",              // Desktop (alternate)
    };

    /// <summary>
    /// Returns true when the foreground window is a File Explorer window.
    /// </summary>
    public static bool IsFileExplorerFocused()
    {
        var hwnd = NativeMethods.GetForegroundWindow();
        if (hwnd == IntPtr.Zero) return false;

        var sb = new StringBuilder(256);
        NativeMethods.GetClassName(hwnd, sb, sb.Capacity);
        return ExplorerClasses.Contains(sb.ToString());
    }

    /// <summary>
    /// Returns the path of the item selected in the foreground File Explorer
    /// window, or null if nothing is selected / the window is not Explorer.
    /// </summary>
    public static string? GetSelectedFilePath()
    {
        try
        {
            return GetSelectedPathViaShellAutomation();
        }
        catch
        {
            return null;
        }
    }

    private static string? GetSelectedPathViaShellAutomation()
    {
        // Use Shell Automation COM: Shell.Application.Windows()
        Type? shellType = Type.GetTypeFromProgID("Shell.Application");
        if (shellType is null) return null;

        dynamic shell   = Activator.CreateInstance(shellType)!;
        dynamic windows = shell.Windows();

        IntPtr foreground = NativeMethods.GetForegroundWindow();

        for (int i = 0; i < windows.Count; i++)
        {
            dynamic? win = windows.Item(i);
            if (win is null) continue;

            try
            {
                if ((IntPtr)win.HWND != foreground) continue;

                dynamic? doc = win.Document;
                if (doc is null) continue;

                // Try to get the focused item first, fall back to first selected item
                string? path = null;
                try   { path = (string?)doc.FocusedItem?.Path; }
                catch { /* no focused item */ }

                if (path is null)
                {
                    dynamic? selected = doc.SelectedItems();
                    if (selected is not null && selected.Count > 0)
                        path = (string?)selected.Item(0)?.Path;
                }

                return path;
            }
            catch { continue; }
        }

        return null;
    }
}
