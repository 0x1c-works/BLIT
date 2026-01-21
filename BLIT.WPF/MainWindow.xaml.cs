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
    }

    private void NavigationView_SelectionChanged(object sender, RoutedEventArgs e) {
        Log.Information("NavigationView_SelectionChanged event fired");
        
        if (sender is not NavigationView navView) {
            Log.Information("Sender is not NavigationView");
            return;
        }

        var selectedItem = navView.SelectedItem as NavigationViewItem;
        if (selectedItem == null) {
            Log.Information("SelectedItem is null");
            return;
        }

        Log.Information($"Selected item: {selectedItem.Content}");
        
        // Check if this is the Help item
        var tag = selectedItem.Tag as string;
        if (tag == "Help") {
            Log.Information("Help item selected");
            HandleHelpNavigation();
            return;
        }
        
        // Get the TargetPageType from the selected item
        var targetPageType = (Type?)selectedItem.GetValue(Wpf.Ui.Controls.NavigationViewItem.TargetPageTypeProperty);
        
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

    private void HandleHelpNavigation() {
        try {
            Log.Information("Handling help navigation");
            SentrySdk.AddBreadcrumb("Visit help", category: "ui.nav");
            
            string helpUrl = I18n.Current.GetString("LinkHelpWebsite");
            Log.Information($"Opening help URL: {helpUrl}");
            
            Process.Start(new ProcessStartInfo {
                FileName = helpUrl,
                UseShellExecute = true,
            });
        } catch (Exception ex) {
            Log.Error($"Failed to open help URL: {ex.Message}");
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




