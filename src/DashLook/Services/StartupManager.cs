using Microsoft.Win32;
using System.Reflection;

namespace DashLook.Services;

/// <summary>
/// Manages the Windows startup registry entry so DashLook launches on login.
/// Used by the portable version — the MSI installer handles this via WiX.
/// </summary>
public static class StartupManager
{
    private const string RunKey    = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName   = "DashLook";
    private const string DevKey    = @"Software\DashLook";

    /// <summary>Returns true when DashLook is registered to start with Windows.</summary>
    public static bool IsStartupEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, false);
        return key?.GetValue(AppName) is not null;
    }

    /// <summary>Register DashLook to auto-start on Windows login.</summary>
    public static void EnableStartup()
    {
        string exePath = Assembly.GetExecutingAssembly().Location
            .Replace(".dll", ".exe"); // handles .NET single-file publish

        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true)
                     ?? throw new InvalidOperationException("Cannot open Run registry key.");
        key.SetValue(AppName, $"\"{exePath}\"");

        // Write developer info
        WriteDevInfo();
    }

    /// <summary>Remove DashLook from Windows startup.</summary>
    public static void DisableStartup()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
        key?.DeleteValue(AppName, throwOnMissingValue: false);
    }

    /// <summary>
    /// Call on first run: auto-enables startup and writes developer info to registry.
    /// </summary>
    public static void EnsureFirstRunSetup()
    {
        using var devKey = Registry.CurrentUser.OpenSubKey(DevKey, false);
        bool alreadyRan = devKey?.GetValue("Installed") is not null;

        if (!alreadyRan)
        {
            EnableStartup();
            WriteDevInfo();
        }
    }

    private static void WriteDevInfo()
    {
        using var key = Registry.CurrentUser.CreateSubKey(DevKey);
        key.SetValue("Developer",  "ARG RABBI — https://itrabbi24.github.io/");
        key.SetValue("Website",    "https://itrabbi24.github.io/");
        key.SetValue("Version",    "1.0.0");
        key.SetValue("Installed",  1, RegistryValueKind.DWord);
    }
}
