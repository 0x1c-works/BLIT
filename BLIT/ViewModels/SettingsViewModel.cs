using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using Wpf.Ui.Common.Interfaces;

namespace BLIT.ViewModels;
public partial class SettingsViewModel : ObservableObject, INavigationAware
{
    bool _isInitialized = false;

    [ObservableProperty]
    string _appVersion = string.Empty;

    [ObservableProperty]
    Wpf.Ui.Appearance.ThemeType _currentTheme = Wpf.Ui.Appearance.ThemeType.Unknown;

    public void OnNavigatedTo()
    {
        if (!_isInitialized)
            InitializeViewModel();
    }

    public void OnNavigatedFrom()
    {
    }

    void InitializeViewModel()
    {
        CurrentTheme = Wpf.Ui.Appearance.Theme.GetAppTheme();
        AppVersion = $"BLIT - {GetAssemblyVersion()}";

        _isInitialized = true;
    }

    string GetAssemblyVersion()
    {
        return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? String.Empty;
    }

    [RelayCommand]
    void OnChangeTheme(string parameter)
    {
        switch (parameter)
        {
            case "theme_light":
                if (CurrentTheme == Wpf.Ui.Appearance.ThemeType.Light)
                    break;

                Wpf.Ui.Appearance.Theme.Apply(Wpf.Ui.Appearance.ThemeType.Light);
                CurrentTheme = Wpf.Ui.Appearance.ThemeType.Light;

                break;

            default:
                if (CurrentTheme == Wpf.Ui.Appearance.ThemeType.Dark)
                    break;

                Wpf.Ui.Appearance.Theme.Apply(Wpf.Ui.Appearance.ThemeType.Dark);
                CurrentTheme = Wpf.Ui.Appearance.ThemeType.Dark;

                break;
        }
    }
}
