using System;
using System.Windows;
using Wpf.Ui.Appearance;

namespace BLIT.WPF.Helpers;

/// <summary>
/// WPF theme management helper using WPF UI library
/// </summary>
public static class ThemeHelper {
    private const string THEME_PREFERENCE_KEY = "theme";
    
    public enum Theme {
        Light,
        Dark,
        HighContrast
    }

    private static Window? _currentWindow;
    private static Theme _currentTheme = Theme.Dark;

    /// <summary>
    /// Gets the current theme
    /// </summary>
    public static Theme CurrentTheme => _currentTheme;

    /// <summary>
    /// Checks if current theme is dark
    /// </summary>
    public static bool IsDarkTheme => _currentTheme == Theme.Dark;

    public static void OnAppStart() {
        _currentTheme = GetSavedTheme();
    }

    public static void Initialize(Window window) {
        _currentWindow = window;
        ApplyTheme(_currentTheme);
    }

    public static void SetTheme(Theme theme) {
        _currentTheme = theme;
        SaveTheme(theme);
        ApplyTheme(theme);
    }

    private static void ApplyTheme(Theme theme) {
        if (Application.Current == null) return;

        // Convert to WPF UI ApplicationTheme
        var appTheme = theme switch {
            Theme.Light => ApplicationTheme.Light,
            Theme.Dark => ApplicationTheme.Dark,
            Theme.HighContrast => ApplicationTheme.HighContrast,
            _ => ApplicationTheme.Dark
        };

        ApplicationThemeManager.Apply(appTheme);
        System.Diagnostics.Debug.WriteLine($"Theme changed to: {theme}");
    }

    private static Theme GetSavedTheme() {
        try {
            var settingsPath = GetSettingsFilePath();
            if (System.IO.File.Exists(settingsPath)) {
                var themeStr = System.IO.File.ReadAllText(settingsPath);
                if (Enum.TryParse<Theme>(themeStr, out var theme)) {
                    return theme;
                }
            }
        } catch {
            // Ignore errors
        }
        return Theme.Dark;
    }

    private static void SaveTheme(Theme theme) {
        try {
            var settingsPath = GetSettingsFilePath();
            var dir = System.IO.Path.GetDirectoryName(settingsPath);
            if (!string.IsNullOrEmpty(dir)) {
                System.IO.Directory.CreateDirectory(dir);
            }
            System.IO.File.WriteAllText(settingsPath, theme.ToString());
        } catch {
            // Ignore errors
        }
    }

    private static string GetSettingsFilePath() {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return System.IO.Path.Combine(localAppData, "BLIT.WPF", THEME_PREFERENCE_KEY);
    }
}
