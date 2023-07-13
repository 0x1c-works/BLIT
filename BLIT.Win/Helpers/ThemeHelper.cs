using Microsoft.UI.Xaml;
using System;
using Windows.Storage;

namespace BLIT.Win.Helpers;

/// <summary>
/// Modified from https://github.com/microsoft/WinUI-Gallery/blob/main/WinUIGallery/Helper/ThemeHelper.cs
/// </summary>
public static class ThemeHelper
{
    const string THEME_PREFERENCE_KEY = "theme";
#if !UNPACKAGED
    static Window CurrentApplicationWindow;
#endif

    /// <summary>
    /// Gets the current actual theme of the app based on the requested theme of the
    /// root element, or if that value is Default, the requested theme of the Application.
    /// </summary>
    public static ElementTheme ActualTheme
    {
        get
        {
            if (CurrentApplicationWindow?.Content is FrameworkElement rootElement)
            {
                if (rootElement.RequestedTheme != ElementTheme.Default)
                {
                    return rootElement.RequestedTheme;
                }
            }

            return Enum.Parse<ElementTheme>(App.Current.RequestedTheme.ToString());
        }
    }

    /// <summary>
    /// Gets or sets (with LocalSettings persistence) the RequestedTheme of the root element.
    /// </summary>
    public static ElementTheme RootTheme
    {
        get
        {
            if (CurrentApplicationWindow?.Content is FrameworkElement rootElement)
            {
                return rootElement.RequestedTheme;
            }

            return ElementTheme.Default;
        }
        set
        {
            if (CurrentApplicationWindow?.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = value;
            }

#if !UNPACKAGED
            ApplicationData.Current.LocalSettings.Values[THEME_PREFERENCE_KEY] = value.ToString();
#endif
        }
    }

    public static void Initialize()
    {
#if !UNPACKAGED
        // Save reference as this might be null when the user is in another app
        CurrentApplicationWindow = App.Current.MainWindow;
        var savedTheme = ApplicationData.Current.LocalSettings.Values[THEME_PREFERENCE_KEY]?.ToString();

        if (savedTheme != null)
        {
            RootTheme = Enum.Parse<ElementTheme>(savedTheme);
        }
        else
        {
            RootTheme = ElementTheme.Dark;
        }
#endif
    }

    public static bool IsDarkTheme
    {
        get
        {
            if (RootTheme == ElementTheme.Default)
            {
                return Application.Current.RequestedTheme == ApplicationTheme.Dark;
            }
            return RootTheme == ElementTheme.Dark;
        }
    }
}
