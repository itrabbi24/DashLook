using System.Diagnostics;
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

    private static readonly HashSet<string> ExplorerClasses = new(StringComparer.OrdinalIgnoreCase)
    {
        "CabinetWClass",
        "ExploreWClass"
    };

    public static bool IsExplorerContextFocused()
    {
        IntPtr foreground = NativeMethods.GetForegroundWindow();
        if (foreground == IntPtr.Zero) return false;

        IntPtr root = NativeMethods.GetAncestor(foreground, 2); // GA_ROOT

        // Process-name based detection is more reliable across Explorer UI variants.
        if (IsExplorerProcessWindow(foreground) || IsExplorerProcessWindow(root))
            return true;

        if (HasClass(foreground, ExplorerClasses) || HasClass(foreground, DesktopClasses))
            return true;

        return HasClass(root, ExplorerClasses) || HasClass(root, DesktopClasses);
    }

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

    public static bool TryGetSelectedFilePathWithRetry(out string? filePath, int attempts = 6, int delayMs = 35)
    {
        for (int i = 0; i < attempts; i++)
        {
            if (TryGetSelectedFilePath(out filePath)) return true;
            Thread.Sleep(delayMs);
        }

        filePath = null;
        return false;
    }

    private static string? GetSelectedPathViaShellAutomation()
    {
        Type? shellType = Type.GetTypeFromProgID("Shell.Application");
        if (shellType is null) return null;

        dynamic shell = Activator.CreateInstance(shellType)!;
        dynamic windows = shell.Windows();

        IntPtr foreground = NativeMethods.GetForegroundWindow();
        IntPtr foregroundRoot = NativeMethods.GetAncestor(foreground, 2); // GA_ROOT
        bool desktopFocused = HasClass(foreground, DesktopClasses) || HasClass(foregroundRoot, DesktopClasses);

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

    private static bool IsExplorerProcessWindow(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero) return false;

        try
        {
            NativeMethods.GetWindowThreadProcessId(hwnd, out uint pid);
            if (pid == 0) return false;

            using Process process = Process.GetProcessById((int)pid);
            return process.ProcessName.Equals("explorer", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static bool HasClass(IntPtr hwnd, HashSet<string> classes)
    {
        if (hwnd == IntPtr.Zero) return false;

        var className = new StringBuilder(256);
        NativeMethods.GetClassName(hwnd, className, className.Capacity);
        return classes.Contains(className.ToString());
    }
}
