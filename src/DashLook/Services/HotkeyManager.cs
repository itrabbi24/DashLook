using System.Runtime.InteropServices;

namespace DashLook.Services;

/// <summary>
/// Installs a low-level keyboard hook (WH_KEYBOARD_LL) to intercept the
/// Space key globally so DashLook can trigger previews from any window.
/// </summary>
public sealed class HotkeyManager : IDisposable
{
    public event EventHandler<SpacePressedEventArgs>? SpacePressed;

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int VK_SPACE = 0x20;

    private IntPtr _hookHandle = IntPtr.Zero;
    private NativeMethods.LowLevelKeyboardProc? _hookProc;

    public void Start()
    {
        _hookProc = HookCallback;
        _hookHandle = NativeMethods.SetWindowsHookEx(
            WH_KEYBOARD_LL,
            _hookProc,
            NativeMethods.GetModuleHandle(null),
            0);

        LogService.Write(_hookHandle == IntPtr.Zero
            ? $"Keyboard hook install failed: {Marshal.GetLastWin32Error()}"
            : "Keyboard hook installed.");
    }

    public void Stop()
    {
        if (_hookHandle != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(_hookHandle);
            _hookHandle = IntPtr.Zero;
            LogService.Write("Keyboard hook removed.");
        }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == WM_KEYDOWN)
        {
            var kbs = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
            if (kbs.vkCode == VK_SPACE && FileExplorerHelper.IsExplorerContextFocused())
            {
                LogService.Write("Space detected in Explorer context.");
                SpacePressed?.Invoke(this, SpacePressedEventArgs.Instance);
                return (IntPtr)1;
            }
        }

        return NativeMethods.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    public void Dispose() => Stop();
}

public sealed class SpacePressedEventArgs : EventArgs
{
    public static SpacePressedEventArgs Instance { get; } = new();
}
