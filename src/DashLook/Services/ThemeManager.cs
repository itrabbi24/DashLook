using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace DashLook.Services;

public enum AppTheme { Dark, Light, System }

public static class ThemeManager
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "DashLook", "settings.json");

    private static readonly Uri DarkUri  = new("pack://application:,,,/Resources/Themes/Dark.xaml");
    private static readonly Uri LightUri = new("pack://application:,,,/Resources/Themes/Light.xaml");

    public static AppTheme Current { get; private set; } = AppTheme.Dark;

    public static event EventHandler? ThemeChanged;

    // ── Public API ────────────────────────────────────────────────────────

    public static void Initialize()
    {
        Current = LoadPreference();
        ApplyInternal(Current, save: false);
    }

    public static void Apply(AppTheme theme)
    {
        Current = theme;
        ApplyInternal(theme, save: true);
        ThemeChanged?.Invoke(null, EventArgs.Empty);
    }

    // ── Internals ─────────────────────────────────────────────────────────

    private static void ApplyInternal(AppTheme theme, bool save)
    {
        var effective = theme == AppTheme.System ? GetSystemTheme() : theme;
        var uri       = effective == AppTheme.Light ? LightUri : DarkUri;

        var merged = Application.Current.Resources.MergedDictionaries;

        // Remove the old theme dict (if any)
        var old = merged.FirstOrDefault(d =>
            d.Source == DarkUri || d.Source == LightUri);
        if (old != null) merged.Remove(old);

        // Insert new theme dict at index 0 (before Styles.xaml)
        merged.Insert(0, new ResourceDictionary { Source = uri });

        if (save) SavePreference(theme);
    }

    // ── System theme detection ────────────────────────────────────────────

    public static AppTheme GetSystemTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            return value is 1 ? AppTheme.Light : AppTheme.Dark;
        }
        catch
        {
            return AppTheme.Dark;
        }
    }

    // ── Persistence ───────────────────────────────────────────────────────

    private static AppTheme LoadPreference()
    {
        try
        {
            if (!File.Exists(SettingsPath)) return AppTheme.System;
            var json = File.ReadAllText(SettingsPath);
            var doc  = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("theme", out var el))
                return Enum.TryParse<AppTheme>(el.GetString(), out var t) ? t : AppTheme.System;
        }
        catch { /* ignore — fall back to System */ }
        return AppTheme.System;
    }

    private static void SavePreference(AppTheme theme)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            var json = JsonSerializer.Serialize(new { theme = theme.ToString() },
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
        catch { /* ignore */ }
    }
}
