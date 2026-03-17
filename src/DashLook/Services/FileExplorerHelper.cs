using System.Runtime.InteropServices;
using System.Text;

namespace DashLook.Services;

/// <summary>
/// Detects the active File Explorer window and retrieves the path of
/// the currently selected file using the Shell Automation Object Model.
/// </summary>
public static class FileExplorerHelper
{
    private static readonly HashSet<string> DesktopClasses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Progman",
        "WorkerW",
        "SHELLDLL_DefView",
        "SysListView32"
    };

    /// <summary>
    /// Returns the path of the item selected in the foreground File Explorer
    /// window, or null if nothing is selected / the window is not Explorer.
    /// </summary>
    public static string? GetSelectedFilePath()
    {
        TryGetSelectedFilePath(out var filePath);
        return filePath;
    }

    public static bool TryGetSelectedFilePath(out string? filePath)
    {
        try
        {
            filePath = GetSelectedPathViaShellAutomation();
            return !string.IsNullOrWhiteSpace(filePath);
        }
        catch
        {
            filePath = null;
            return false;
        }
    }

    private static string? GetSelectedPathViaShellAutomation()
    {
        Type? shellType = Type.GetTypeFromProgID("Shell.Application");
        if (shellType is null) return null;

        dynamic shell = Activator.CreateInstance(shellType)!;
        dynamic windows = shell.Windows();

        IntPtr foreground = NativeMethods.GetForegroundWindow();
        IntPtr foregroundRoot = NativeMethods.GetAncestor(foreground, 2); // GA_ROOT
        bool desktopFocused = IsDesktopWindow(foreground) || IsDesktopWindow(foregroundRoot);

        IntPtr desktopShell = NativeMethods.GetShellWindow();
        IntPtr desktopShellRoot = desktopShell != IntPtr.Zero
            ? NativeMethods.GetAncestor(desktopShell, 2)
            : IntPtr.Zero;

        for (int i = 0; i < windows.Count; i++)
        {
            dynamic? win = windows.Item(i);
            if (win is null) continue;

            try
            {
                IntPtr explorerHwnd = (IntPtr)win.HWND;
                IntPtr explorerRoot = NativeMethods.GetAncestor(explorerHwnd, 2); // GA_ROOT

                bool isForegroundMatch = explorerHwnd == foreground || explorerRoot == foregroundRoot;
                bool isDesktopMatch = desktopFocused &&
                                      (explorerHwnd == desktopShell || explorerRoot == desktopShellRoot);
                if (!isForegroundMatch && !isDesktopMatch) continue;

                dynamic? doc = win.Document;
                if (doc is null) continue;

                string? path = null;
                try { path = (string?)doc.FocusedItem?.Path; }
                catch { }

                if (string.IsNullOrWhiteSpace(path))
                {
                    dynamic? selected = doc.SelectedItems();
                    if (selected is not null && selected.Count > 0)
                        path = (string?)selected.Item(0)?.Path;
                }

                if (!string.IsNullOrWhiteSpace(path))
                    return path;
            }
            catch
            {
                continue;
            }
        }

        return null;
    }

    private static bool IsDesktopWindow(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero) return false;

        var className = new StringBuilder(256);
        NativeMethods.GetClassName(hwnd, className, className.Capacity);
        return DesktopClasses.Contains(className.ToString());
    }
}
