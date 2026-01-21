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

    private void NavigationView_ItemInvoked(object sender, Wpf.Ui.Controls.NavigationViewItemInvokedEventArgs e) {
        Log.Information("NavigationView_ItemInvoked event fired");
        
        if (e.InvokedItem is not NavigationViewItem navItem) {
            Log.Information("InvokedItem is not NavigationViewItem");
            return;
        }

        var tag = navItem.Tag as string;
        Log.Information($"Invoked item tag: {tag}, content: {navItem.Content}");
        
        // Special handling for Help item - open in browser
        if (tag == "Help") {
            Log.Information("Help item invoked - opening browser");
            HandleHelpNavigation();
            return;
        }
        
        // For regular pages, get TargetPageType and navigate
        var targetPageType = (Type?)navItem.GetValue(Wpf.Ui.Controls.NavigationViewItem.TargetPageTypeProperty);
        
        if (targetPageType == null) {
            Log.Information($"TargetPageType is null for item: {tag}");
            return;
        }

        Log.Information($"Navigating to page type: {targetPageType.Name}");
        
        try {
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
            Log.Information("Opening help in browser");
            SentrySdk.AddBreadcrumb("Visit help", category: "ui.nav");
            
            string helpUrl = I18n.Current.GetString("LinkHelpWebsite");
            Log.Information($"Help URL: {helpUrl}");
            
            Process.Start(new ProcessStartInfo {
                FileName = helpUrl,
                UseShellExecute = true,
            });
            
            Log.Information("Help URL opened successfully");
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





