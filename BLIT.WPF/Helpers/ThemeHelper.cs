using System.Diagnostics;
using System.IO;
using System.Windows;
using Wpf.Ui.Appearance;

namespace BLIT.WPF.Helpers;

/// <summary>
///     WPF theme management helper using WPF UI library
/// </summary>
public static class ThemeHelper {
    #region Theme enum

    public enum Theme {
        Light,
        Dark,
        HighContrast
    }

    #endregion

    private const string THEME_PREFERENCE_KEY = "theme";

    private static Window? _currentWindow;

    /// <summary>
    ///     Gets the current theme
    /// </summary>
    public static Theme CurrentTheme { get; private set; } = Theme.Dark;

    /// <summary>
    ///     Checks if current theme is dark
    /// </summary>
    public static bool IsDarkTheme => CurrentTheme == Theme.Dark;

    public static void OnAppStart() {
        CurrentTheme = GetSavedTheme();
    }

    public static void Initialize(Window window) {
        _currentWindow = window;
        ApplyTheme(CurrentTheme);
    }

    public static void SetTheme(Theme theme) {
        CurrentTheme = theme;
        SaveTheme(theme);
        ApplyTheme(theme);
    }

    private static void ApplyTheme(Theme theme) {
        if (Application.Current == null) {
            return;
        }

        // Convert to WPF UI ApplicationTheme
        ApplicationTheme appTheme = theme switch {
            Theme.Light => ApplicationTheme.Light,
            Theme.Dark => ApplicationTheme.Dark,
            Theme.HighContrast => ApplicationTheme.HighContrast,
            _ => ApplicationTheme.Dark
        };

        ApplicationThemeManager.Apply(appTheme);
        Debug.WriteLine($"Theme changed to: {theme}");
    }

    private static Theme GetSavedTheme() {
        try {
            var settingsPath = GetSettingsFilePath();
            if (File.Exists(settingsPath)) {
                var themeStr = File.ReadAllText(settingsPath);
                if (Enum.TryParse<Theme>(themeStr, out Theme theme)) {
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
            var dir = Path.GetDirectoryName(settingsPath);
            if (!string.IsNullOrEmpty(dir)) {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(settingsPath, theme.ToString());
        } catch {
            // Ignore errors
        }
    }

    private static string GetSettingsFilePath() {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(localAppData, "BLIT.WPF", THEME_PREFERENCE_KEY);
    }
}