using BLIT.WPF.Helpers;
using BLIT.WPF.Pages.BannerIcons;
using BLIT.WPF.Pages.BannerIcons.Models;
using BLIT.WPF.Pages.Settings;
using BLIT.WPF.Services;
using Sentry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BLIT.WPF;

public partial class MainWindow : Window {
    private ViewModel Model { get; } = new ViewModel();

    public MainWindow() {
        InitializeComponent();
        DataContext = Model;
        
        Loaded += MainWindow_Loaded;
    }
    
    private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
        // Select first menu item after window is loaded
        if (AppNav.Items.Count > 0) {
            AppNav.SelectedIndex = 0;
        }
    }

    private void AppNav_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        try {
            if (AppNav.SelectedItem is not ListBoxItem item) {
                return;
            }

            var tag = item.Tag as string;
            if (tag != null && TAGGED_PAGES.TryGetValue(tag, out NavPage? page)) {
                // Create page instance and navigate
                var pageInstance = Activator.CreateInstance(page.Type) as Page;
                if (pageInstance != null) {
                    AppContent.Navigate(pageInstance);
                    page.OnLoad?.Invoke(sender, item);
                } else {
                    System.Diagnostics.Debug.WriteLine($"Failed to create page instance for {page.Type.Name}");
                }
            }
        } catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            MessageBox.Show($"导航失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void NavigateToSettings() {
        // Navigate to settings page if needed
        AppContent.Navigate(new SettingsPage());
    }

    private record NavPage(Type Type, Action<object, ListBoxItem>? OnLoad);

    private static readonly Dictionary<string, NavPage> TAGGED_PAGES = new() {
        {"BannerIcons", new(typeof(BannerIconsPage), OnProjectPageLoad<BannerIconsProject>)},
    };

    private static void OnProjectPageLoad<T>(object sender, ListBoxItem item) where T : IProject {
        var project = AppServices.Get<IProjectService<T>>();
        if (project != null) {
            void UpdateHeader() {
                // Update UI header if needed
            }
            UpdateHeader();
            project.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(project.Name)) {
                    UpdateHeader();
                }
            };
        }
    }

    private void NavHelp_MouseDown(object sender, MouseButtonEventArgs e) {
        SentrySdk.AddBreadcrumb("Visit help", category: "ui.nav");
        Process.Start(new ProcessStartInfo {
            FileName = I18n.Current.GetString("LinkHelpWebsite"),
            UseShellExecute = true,
        });
    }

    public class ViewModel : INotifyPropertyChanged {
        private string? _rootFolder;
        public string? RootFolder {
            get => _rootFolder;
            set {
                if (_rootFolder == value) {
                    return;
                }

                _rootFolder = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? prop = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}

public record NavPageHeaderInfo(string Title, string? SubTitle = null, bool IsModified = false) {
    public bool HasSubTitle { get; set; } = true;
    public string SubTitle { get; init; } = string.IsNullOrWhiteSpace(SubTitle) 
        ? I18n.Current.GetString("Placeholder.NewProject") 
        : SubTitle;
}
