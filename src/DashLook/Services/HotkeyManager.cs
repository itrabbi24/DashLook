using System.Runtime.InteropServices;
using System.Windows.Input;

namespace DashLook.Services;

/// <summary>
/// Installs a low-level keyboard hook (WH_KEYBOARD_LL) to intercept the
/// Space key globally so DashLook can trigger previews from any window.
/// </summary>
public sealed class HotkeyManager : IDisposable
{
    public event EventHandler<SpacePressedEventArgs>? SpacePressed;

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN     = 0x0100;
    private const int VK_SPACE       = 0x20;

    private IntPtr _hookHandle = IntPtr.Zero;
    private NativeMethods.LowLevelKeyboardProc? _hookProc; // Keep alive!

    public void Start()
    {
        _hookProc = HookCallback;
        using var module = System.Diagnostics.Process.GetCurrentProcess().MainModule!;
        _hookHandle = NativeMethods.SetWindowsHookEx(
            WH_KEYBOARD_LL,
            _hookProc,
            NativeMethods.GetModuleHandle(module.ModuleName),
            0);
    }

    public void Stop()
    {
        if (_hookHandle != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(_hookHandle);
            _hookHandle = IntPtr.Zero;
        }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == WM_KEYDOWN)
        {
            var kbs = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
            if (kbs.vkCode == VK_SPACE)
            {
                // Only fire when File Explorer (or a file manager) is active
                // and no text-input control has focus
                if (FileExplorerHelper.IsFileExplorerFocused())
                {
                    SpacePressed?.Invoke(this, new SpacePressedEventArgs());
                    // Return 1 to suppress the Space key in File Explorer
                    return (IntPtr)1;
                }
            }
        }
        return NativeMethods.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    public void Dispose() => Stop();
}

public sealed class SpacePressedEventArgs : EventArgs { }
