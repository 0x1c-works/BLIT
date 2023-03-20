// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BannerlordImageTool.Win.Pages;
using BannerlordImageTool.Win.Pages.BannerIcons;
using BannerlordImageTool.Win.Pages.Settings;
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

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        if (args.WindowActivationState == WindowActivationState.Deactivated)
        {
            AppTitleText.Foreground = (SolidColorBrush)App.Current.Resources["WindowCaptionForegroundDisabled"];
        }
        else
        {
            AppTitleText.Foreground = (SolidColorBrush)App.Current.Resources["WindowCaptionForeground"];
        }
    }

    private void AppNav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        var item = args.SelectedItem as NavigationViewItem;
        if (item is null) return;
        AppNav.Header = item.Content;

        if (args.IsSettingsSelected)
        {
            AppContent.Navigate(typeof(SettingsPage));
        }
        else
        {
            if (item is not null)
            {
                var tag = item?.Tag as string;
                if (TAGGED_PAGES.TryGetValue(tag, out var pageType))
                {
                    AppContent.Navigate(pageType);
                }
            }
        }
    }

    static readonly Dictionary<string, Type> TAGGED_PAGES = new() {
        {"BannerIcons",typeof(BannerIconsEditor) },
    };

    public class ViewModel : INotifyPropertyChanged
    {
        private StorageFolder _rootFolder;
        public StorageFolder RootFolder
        {
            get => _rootFolder;
            set
            {
                if (_rootFolder?.Path == value?.Path) return;
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
