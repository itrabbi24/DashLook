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

    private static readonly Uri DarkUri = new("pack://application:,,,/Resources/Themes/Dark.xaml");
    private static readonly Uri LightUri = new("pack://application:,,,/Resources/Themes/Light.xaml");
    private static bool _systemThemeHooked;

    public static AppTheme Current { get; private set; } = AppTheme.Dark;

    public static event EventHandler? ThemeChanged;

    public static void Initialize()
    {
        Current = LoadPreference();
        HookSystemThemeChanges();
        ApplyInternal(Current, save: false);
    }

    public static void Apply(AppTheme theme)
    {
        Current = theme;
        ApplyInternal(theme, save: true);
        ThemeChanged?.Invoke(null, EventArgs.Empty);
    }

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

    private static void ApplyInternal(AppTheme theme, bool save)
    {
        var effectiveTheme = theme == AppTheme.System ? GetSystemTheme() : theme;
        var uri = effectiveTheme == AppTheme.Light ? LightUri : DarkUri;

        var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
        var themeDictionaries = mergedDictionaries
            .Where(dictionary => IsThemeDictionary(dictionary.Source))
            .ToList();

        foreach (var dictionary in themeDictionaries)
            mergedDictionaries.Remove(dictionary);

        mergedDictionaries.Insert(0, new ResourceDictionary { Source = uri });

        if (save)
            SavePreference(theme);
    }

    private static bool IsThemeDictionary(Uri? source)
    {
        if (source is null)
            return false;

        string value = source.OriginalString.Replace('\\', '/');
        return value.EndsWith("/Resources/Themes/Dark.xaml", StringComparison.OrdinalIgnoreCase)
            || value.EndsWith("/Resources/Themes/Light.xaml", StringComparison.OrdinalIgnoreCase)
            || value.EndsWith("Resources/Themes/Dark.xaml", StringComparison.OrdinalIgnoreCase)
            || value.EndsWith("Resources/Themes/Light.xaml", StringComparison.OrdinalIgnoreCase);
    }

    private static void HookSystemThemeChanges()
    {
        if (_systemThemeHooked)
            return;

        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        _systemThemeHooked = true;
    }

    private static void OnUserPreferenceChanged(object? sender, UserPreferenceChangedEventArgs e)
    {
        if (Current != AppTheme.System)
            return;

        if (Application.Current?.Dispatcher is null)
            return;

        Application.Current.Dispatcher.Invoke(() =>
        {
            ApplyInternal(Current, save: false);
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        });
    }

    private static AppTheme LoadPreference()
    {
        try
        {
            if (!File.Exists(SettingsPath))
                return AppTheme.System;

            var json = File.ReadAllText(SettingsPath);
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("theme", out var element))
                return Enum.TryParse<AppTheme>(element.GetString(), out var theme)
                    ? theme
                    : AppTheme.System;
        }
        catch
        {
        }

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
        catch
        {
        }
    }
}
