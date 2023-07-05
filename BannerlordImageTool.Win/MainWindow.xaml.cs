// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BannerlordImageTool.Win.Helpers;
using BannerlordImageTool.Win.Pages.BannerIcons;
using BannerlordImageTool.Win.Pages.BannerIcons.ViewModels;
using BannerlordImageTool.Win.Pages.Settings;
using BannerlordImageTool.Win.Services;
using BannerlordImageTool.Win.Theming;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BannerlordImageTool.Win;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : ThemedWindow
{
    ViewModel Model { get; } = new ViewModel();

    public MainWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppNav.SelectedItem = AppNav.MenuItems.First();

        Activated += MainWindow_Activated;
    }

    void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        AppTitleText.Foreground = args.WindowActivationState == WindowActivationState.Deactivated
            ? (SolidColorBrush)App.Current.Resources["WindowCaptionForegroundDisabled"]
            : (Brush)(SolidColorBrush)App.Current.Resources["WindowCaptionForeground"];
    }

    void AppNav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem item)
        {
            return;
        }

        AppNav.Header = new NavPageHeaderInfo(item.Content.ToString());

        if (args.IsSettingsSelected)
        {
            AppContent.Navigate(typeof(SettingsPage));
        }
        else
        {
            if (item is not null)
            {
                var tag = item?.Tag as string;
                if (TAGGED_PAGES.TryGetValue(tag, out NavPage page))
                {
                    AppContent.Navigate(page.Type);
                    page.OnLoad?.Invoke(AppNav, item);
                }
            }
        }
    }

    record NavPage(Type Type, Action<NavigationView, NavigationViewItem> OnLoad);
    static readonly Dictionary<string, NavPage> TAGGED_PAGES = new() {
        {"BannerIcons",new(typeof(BannerIconsPage), OnProjectPageLoad<BannerIconsPageViewModel>)},
    };
    static void OnProjectPageLoad<T>(NavigationView view, NavigationViewItem item) where T : IProject
    {
        IProjectService<T> project = AppServices.Get<IProjectService<T>>();
        void UpdateHeader()
        {
            view.Header = new NavPageHeaderInfo(item.Content.ToString(), project?.Name, false);
        }
        UpdateHeader();
        project.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(project.Name))
            {
                UpdateHeader();
            }
        };
    }


    public class ViewModel : INotifyPropertyChanged
    {
        StorageFolder _rootFolder;
        public StorageFolder RootFolder
        {
            get => _rootFolder;
            set
            {
                if (_rootFolder?.Path == value?.Path)
                {
                    return;
                }

                _rootFolder = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string prop = null)
        {
            PropertyChanged?.Invoke(this, new(prop));
        }
    }
}
public record NavPageHeaderInfo(string Title, string SubTitle = null, bool IsModified = false)
{
    public string SubTitle { get; init; } = string.IsNullOrWhiteSpace(SubTitle) ? I18n.Current.GetString("Placeholder/NewProject") : SubTitle;
}
