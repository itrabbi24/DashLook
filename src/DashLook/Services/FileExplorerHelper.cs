using System.Diagnostics;
using System.IO;
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

        IntPtr root = NativeMethods.GetAncestor(foreground, 2);

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
            filePath = GetSelectedPath();
            return !string.IsNullOrWhiteSpace(filePath);
        }
        catch
        {
            filePath = null;
            return false;
        }
    }

    public static bool TryGetSelectedFilePathWithRetry(out string? filePath, int attempts = 8, int delayMs = 35)
    {
        for (int i = 0; i < attempts; i++)
        {
            if (TryGetSelectedFilePath(out filePath)) return true;
            Thread.Sleep(delayMs);
        }

        filePath = null;
        return false;
    }

    private static string? GetSelectedPath()
    {
        IntPtr foreground = NativeMethods.GetForegroundWindow();
        IntPtr foregroundRoot = NativeMethods.GetAncestor(foreground, 2);
        bool desktopFocused = HasClass(foreground, DesktopClasses) || HasClass(foregroundRoot, DesktopClasses);

        string? path = GetSelectedPathViaShellAutomation(foreground, foregroundRoot, desktopFocused);
        if (!string.IsNullOrWhiteSpace(path))
            return path;

        if (desktopFocused)
            return GetSelectedDesktopPathViaListView();

        return null;
    }

    private static string? GetSelectedPathViaShellAutomation(IntPtr foreground, IntPtr foregroundRoot, bool desktopFocused)
    {
        Type? shellType = Type.GetTypeFromProgID("Shell.Application");
        if (shellType is null) return null;

        dynamic shell = Activator.CreateInstance(shellType)!;
        dynamic windows = shell.Windows();

        for (int i = 0; i < windows.Count; i++)
        {
            dynamic? win = windows.Item(i);
            if (win is null) continue;

            try
            {
                IntPtr explorerHwnd = (IntPtr)win.HWND;
                IntPtr explorerRoot = NativeMethods.GetAncestor(explorerHwnd, 2);

                bool isForegroundMatch = explorerHwnd == foreground || explorerRoot == foregroundRoot;
                bool isDesktopCandidate = desktopFocused;
                if (!isForegroundMatch && !isDesktopCandidate) continue;

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

    private static string? GetSelectedDesktopPathViaListView()
    {
        IntPtr listView = FindDesktopListView();
        if (listView == IntPtr.Zero) return null;

        int selectedIndex = (int)NativeMethods.SendMessage(
            listView,
            NativeMethods.LVM_GETNEXTITEM,
            new IntPtr(-1),
            new IntPtr(NativeMethods.LVNI_SELECTED));

        if (selectedIndex < 0) return null;

        string? itemName = GetListViewItemText(listView, selectedIndex);
        if (string.IsNullOrWhiteSpace(itemName)) return null;

        string userDesktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        string userPath = Path.Combine(userDesktop, itemName);
        if (File.Exists(userPath) || Directory.Exists(userPath))
            return userPath;

        string commonDesktop = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
        string commonPath = Path.Combine(commonDesktop, itemName);
        if (File.Exists(commonPath) || Directory.Exists(commonPath))
            return commonPath;

        return userPath;
    }

    private static IntPtr FindDesktopListView()
    {
        IntPtr shellWindow = NativeMethods.GetShellWindow();
        IntPtr defView = FindShellDefView(shellWindow);

        if (defView == IntPtr.Zero)
        {
            IntPtr progman = NativeMethods.FindWindow("Progman", null);
            defView = FindShellDefView(progman);
        }

        if (defView == IntPtr.Zero)
        {
            IntPtr worker = IntPtr.Zero;
            while ((worker = NativeMethods.FindWindowEx(IntPtr.Zero, worker, "WorkerW", null)) != IntPtr.Zero)
            {
                defView = FindShellDefView(worker);
                if (defView != IntPtr.Zero) break;
            }
        }

        if (defView == IntPtr.Zero) return IntPtr.Zero;

        return NativeMethods.FindWindowEx(defView, IntPtr.Zero, "SysListView32", "FolderView");
    }

    private static IntPtr FindShellDefView(IntPtr parent)
    {
        if (parent == IntPtr.Zero) return IntPtr.Zero;
        return NativeMethods.FindWindowEx(parent, IntPtr.Zero, "SHELLDLL_DefView", null);
    }

    private static string? GetListViewItemText(IntPtr listView, int itemIndex)
    {
        NativeMethods.GetWindowThreadProcessId(listView, out uint processId);
        if (processId == 0) return null;

        IntPtr process = NativeMethods.OpenProcess(
            NativeMethods.PROCESS_VM_OPERATION |
            NativeMethods.PROCESS_VM_READ |
            NativeMethods.PROCESS_VM_WRITE |
            NativeMethods.PROCESS_QUERY_INFORMATION,
            false,
            processId);

        if (process == IntPtr.Zero) return null;

        IntPtr remoteText = IntPtr.Zero;
        IntPtr remoteLvItem = IntPtr.Zero;

        try
        {
            const int maxChars = 520;
            int textBufferSize = maxChars * 2;
            int lvItemSize = Marshal.SizeOf<NativeMethods.LVITEM>();

            remoteText = NativeMethods.VirtualAllocEx(process, IntPtr.Zero, (UIntPtr)textBufferSize,
                NativeMethods.MEM_COMMIT | NativeMethods.MEM_RESERVE, NativeMethods.PAGE_READWRITE);
            remoteLvItem = NativeMethods.VirtualAllocEx(process, IntPtr.Zero, (UIntPtr)lvItemSize,
                NativeMethods.MEM_COMMIT | NativeMethods.MEM_RESERVE, NativeMethods.PAGE_READWRITE);

            if (remoteText == IntPtr.Zero || remoteLvItem == IntPtr.Zero)
                return null;

            var lvItem = new NativeMethods.LVITEM
            {
                mask = NativeMethods.LVIF_TEXT,
                iItem = itemIndex,
                iSubItem = 0,
                pszText = remoteText,
                cchTextMax = maxChars
            };

            byte[] lvItemBytes = StructureToBytes(lvItem);
            if (!NativeMethods.WriteProcessMemory(process, remoteLvItem, lvItemBytes, lvItemBytes.Length, out _))
                return null;

            NativeMethods.SendMessage(listView, NativeMethods.LVM_GETITEMTEXTW, new IntPtr(itemIndex), remoteLvItem);

            byte[] textBytes = new byte[textBufferSize];
            if (!NativeMethods.ReadProcessMemory(process, remoteText, textBytes, textBytes.Length, out _))
                return null;

            string text = Encoding.Unicode.GetString(textBytes);
            int nullIndex = text.IndexOf('\0');
            return nullIndex >= 0 ? text[..nullIndex] : text;
        }
        finally
        {
            if (remoteLvItem != IntPtr.Zero)
                NativeMethods.VirtualFreeEx(process, remoteLvItem, UIntPtr.Zero, NativeMethods.MEM_RELEASE);
            if (remoteText != IntPtr.Zero)
                NativeMethods.VirtualFreeEx(process, remoteText, UIntPtr.Zero, NativeMethods.MEM_RELEASE);
            NativeMethods.CloseHandle(process);
        }
    }

    private static byte[] StructureToBytes<T>(T value) where T : struct
    {
        int size = Marshal.SizeOf<T>();
        IntPtr buffer = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(value, buffer, false);
            byte[] bytes = new byte[size];
            Marshal.Copy(buffer, bytes, 0, size);
            return bytes;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
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


