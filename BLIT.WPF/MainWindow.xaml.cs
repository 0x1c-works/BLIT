using BLIT.WPF.Helpers;
using Serilog;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;

namespace BLIT.WPF;

public partial class MainWindow {
    private ViewModel Model { get; } = new();

    public MainWindow() {
        InitializeComponent();
        DataContext = Model;
        
        Loaded += MainWindow_Loaded;
    }
    
    private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
        Log.Information($"MainWindow loaded. NavigationView items count: {MainNavigationView.MenuItems.Count}");
        
        // Auto-navigate to BannerIcons page on startup
        if (MainNavigationView.MenuItems.Count > 0) {
            MainNavigationView.Navigate(typeof(Pages.BannerIcons.BannerIconsPage));
            Log.Information("Auto-navigated to BannerIcons page on startup");
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





