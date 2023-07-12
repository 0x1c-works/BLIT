// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BannerlordImageTool.Win.Helpers;
using BannerlordImageTool.Win.Pages.Settings.ViewModels;
using BannerlordImageTool.Win.Services;
using BannerlordImageTool.Win.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Linq;
using Windows.Globalization;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BannerlordImageTool.Win.Pages.Settings;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SettingsPage : Page
{
    public readonly Tuple<string, string>[] Languages = new[] {
        new Tuple<string, string>("English", "en"),
        new Tuple<string, string>("¼òÌåÖÐÎÄ", "zh"),
    };
    SettingsViewModel ViewModel { get; } = new();
    GlobalSettings _globalSettings = AppServices.Get<GlobalSettings>();

    public SettingsPage()
    {
        InitializeComponent();
    }

    void btnOpenLogFolder_Click(object sender, RoutedEventArgs e)
    {
        FileHelpers.OpenFolderInExplorer(Logging.Folder);
    }

    async void cboLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var old = (e.RemovedItems.FirstOrDefault() as Tuple<string, string>)?.Item2;
        var selected = (e.AddedItems.FirstOrDefault() as Tuple<string, string>)?.Item2;
        if (selected == CurrentLang || CurrentLang.StartsWith(selected)) { return; }
        ApplicationLanguages.PrimaryLanguageOverride = selected;
        ContentDialogResult result = await AppServices.Get<IConfirmDialogService>().ShowWarn(this,
            I18n.Current.GetString("DialogChangeLanguage/Title"),
            I18n.Current.GetString("DialogChangeLanguage/Content")
            );
        if (result == ContentDialogResult.Primary)
        {
            // FIXME: WinUI3 seems not able to change language at runtime. So I have to restart the app as a workaround.
            //        See: https://github.com/microsoft/microsoft-ui-xaml/issues/5940
            AppInstance.Restart("");
        }
    }

    void cboLanguage_Loaded(object sender, RoutedEventArgs e)
    {
        var current = CurrentLang;
        Tuple<string, string> item = Languages.FirstOrDefault(item => item.Item2 == current);
        if (item == null)
        {
            item = Languages.FirstOrDefault(item => current.StartsWith(item.Item2));
        }
        cboLanguage.SelectedItem = item;
    }

    string CurrentLang { get => Windows.ApplicationModel.Resources.Core.ResourceContext.GetForViewIndependentUse().Languages[0]; }

    void toggleTheme_Loaded(object sender, RoutedEventArgs e)
    {
        ((ToggleSwitch)sender).IsOn = !ThemeHelper.IsDarkTheme;
    }

    void toggleTheme_Toggled(object sender, RoutedEventArgs e)
    {
        ThemeHelper.RootTheme = ((ToggleSwitch)sender).IsOn ? ElementTheme.Light : ElementTheme.Dark;
    }
}
