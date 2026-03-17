namespace DashLook.Services;

/// <summary>
/// Detects the active File Explorer window and retrieves the path of
/// the currently selected file using the Shell Automation Object Model.
/// </summary>
public static class FileExplorerHelper
{
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
        // Use Shell Automation COM: Shell.Application.Windows()
        Type? shellType = Type.GetTypeFromProgID("Shell.Application");
        if (shellType is null) return null;

        dynamic shell = Activator.CreateInstance(shellType)!;
        dynamic windows = shell.Windows();

        IntPtr foreground = NativeMethods.GetForegroundWindow();
        IntPtr foregroundRoot = NativeMethods.GetAncestor(foreground, 2); // GA_ROOT

        for (int i = 0; i < windows.Count; i++)
        {
            dynamic? win = windows.Item(i);
            if (win is null) continue;

            try
            {
                IntPtr explorerHwnd = (IntPtr)win.HWND;
                IntPtr explorerRoot = NativeMethods.GetAncestor(explorerHwnd, 2); // GA_ROOT
                if (explorerHwnd != foreground && explorerRoot != foregroundRoot) continue;

                dynamic? doc = win.Document;
                if (doc is null) continue;

                // Try to get the focused item first, fall back to first selected item
                string? path = null;
                try { path = (string?)doc.FocusedItem?.Path; }
                catch { }

                if (path is null)
                {
                    dynamic? selected = doc.SelectedItems();
                    if (selected is not null && selected.Count > 0)
                        path = (string?)selected.Item(0)?.Path;
                }

                return path;
            }
            catch
            {
                continue;
            }
        }

        return null;
    }
}
