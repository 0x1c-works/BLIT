using BLIT.WPF.Helpers;
using BLIT.WPF.Pages.BannerIcons;
using BLIT.WPF.Pages.BannerIcons.Models;
using BLIT.WPF.Pages.Settings;
using BLIT.WPF.Services;
using Serilog;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace BLIT.WPF;

public partial class MainWindow : Window {
    private ViewModel Model { get; } = new ViewModel();

    public MainWindow() {
        InitializeComponent();
        DataContext = Model;
        
        Loaded += MainWindow_Loaded;
    }
    
    private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
        Log.Information($"MainWindow loaded. NavigationView items count: {MainNavigationView.MenuItems.Count}");
        
        // Manually wire up event handlers for NavigationViewItems
        // since they might not work from XAML
        if (MainNavigationView.MenuItems.Count > 0) {
            var bannerIconsItem = MainNavigationView.MenuItems[0] as NavigationViewItem;
            if (bannerIconsItem != null) {
                Log.Information($"Setting up event handlers for BannerIcons item");
                // Try different events to see which one works
                bannerIconsItem.PreviewMouseDown += (s, e) => {
                    Log.Information("BannerIcons - PreviewMouseDown triggered");
                    NavigateToPage(bannerIconsItem);
                };
            }
        }
        
        // Setup Help button
        if (MainNavigationView.FooterMenuItems.Count > 0) {
            var helpItem = MainNavigationView.FooterMenuItems[0] as NavigationViewItem;
            if (helpItem != null) {
                Log.Information($"Setting up event handlers for Help item");
                helpItem.PreviewMouseDown += (s, e) => {
                    Log.Information("Help - PreviewMouseDown triggered");
                    e.Handled = true;
                    HandleHelpNavigation();
                };
            }
        }
        
        Log.Information("MainWindow initialization complete");
    }

    private void NavigateToPage(NavigationViewItem item) {
        Log.Information($"NavigateToPage called for: {item.Content}");
        
        // Get the TargetPageType from the item
        var targetPageType = (Type?)item.GetValue(Wpf.Ui.Controls.NavigationViewItem.TargetPageTypeProperty);
        
        if (targetPageType == null) {
            Log.Information("TargetPageType is null");
            return;
        }

        Log.Information($"Navigating to page type: {targetPageType.Name}");
        
        try {
            // Create an instance of the target page
            if (Activator.CreateInstance(targetPageType) is Page pageInstance) {
                Log.Information($"Created page instance: {pageInstance.GetType().Name}");
                AppContent.Navigate(pageInstance);
                Log.Information("Navigation completed successfully");
            } else {
                Log.Error($"Failed to create page instance for {targetPageType.Name}");
            }
        } catch (Exception ex) {
            Log.Error($"Exception during navigation: {ex.Message}");
            Log.Error($"Stack trace: {ex.StackTrace}");
        }
    }

    public void NavigateToSettings() {
        Log.Information("Navigating to Settings page");
        AppContent.Navigate(new SettingsPage());
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




