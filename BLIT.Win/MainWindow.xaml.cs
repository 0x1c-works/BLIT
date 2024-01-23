// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BLIT.Win.Helpers;
using BLIT.Win.Pages.BannerIcons;
using BLIT.Win.Pages.BannerIcons.Models;
using BLIT.Win.Pages.Settings;
using BLIT.Win.Services;
using BLIT.Win.Theming;
using Microsoft.AppCenter.Analytics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BLIT.Win;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : ThemedWindow {
    private ViewModel Model { get; } = new ViewModel();

    public MainWindow() {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppNav.SelectedItem = AppNav.MenuItems.First();

        Activated += MainWindow_Activated;
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args) {
        AppTitleText.Foreground = args.WindowActivationState == WindowActivationState.Deactivated
            ? (SolidColorBrush)App.Current.Resources["WindowCaptionForegroundDisabled"]
            : (Brush)(SolidColorBrush)App.Current.Resources["WindowCaptionForeground"];
    }

    private void AppNav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args) {
        if (args.SelectedItem is not NavigationViewItem item) {
            return;
        }

        AppNav.Header = new NavPageHeaderInfo(item.Content.ToString()) { HasSubTitle = false };

        if (args.IsSettingsSelected) {
            AppContent.Navigate(typeof(SettingsPage));
        } else {
            if (item is not null) {
                var tag = item?.Tag as string;
                if (TAGGED_PAGES.TryGetValue(tag, out NavPage page)) {
                    AppContent.Navigate(page.Type);
                    page.OnLoad?.Invoke(AppNav, item);
                }
            }
        }
    }

    public void NavigateToSettings() {
        AppNav.SelectedItem = AppNav.SettingsItem;
    }

    private record NavPage(Type Type, Action<NavigationView, NavigationViewItem> OnLoad);

    private static readonly Dictionary<string, NavPage> TAGGED_PAGES = new() {
        {"BannerIcons",new(typeof(BannerIconsPage), OnProjectPageLoad<BannerIconsProject>)},
    };

    private static void OnProjectPageLoad<T>(NavigationView view, NavigationViewItem item) where T : IProject {
        IProjectService<T> project = AppServices.Get<IProjectService<T>>();
        void UpdateHeader() {
            view.Header = new NavPageHeaderInfo(item.Content.ToString(), project?.Name, false);
        }
        UpdateHeader();
        project.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(project.Name)) {
                UpdateHeader();
            }
        };
    }

    private void navHelp_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e) {
        Analytics.TrackEvent("Visit help", new Dictionary<string, string> {
            {"source", "nav" }
        });
        Process.Start(new ProcessStartInfo {
            FileName = I18n.Current.GetString("LinkHelpWebsite"),
            UseShellExecute = true,
        });
    }

    public class ViewModel : INotifyPropertyChanged {
        private StorageFolder _rootFolder;
        public StorageFolder RootFolder {
            get => _rootFolder;
            set {
                if (_rootFolder?.Path == value?.Path) {
                    return;
                }

                _rootFolder = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string prop = null) {
            PropertyChanged?.Invoke(this, new(prop));
        }
    }
}
public record NavPageHeaderInfo(string Title, string SubTitle = null, bool IsModified = false) {
    public bool HasSubTitle { get; set; } = true;
    public string SubTitle { get; init; } = string.IsNullOrWhiteSpace(SubTitle) ? I18n.Current.GetString("Placeholder/NewProject") : SubTitle;
}
