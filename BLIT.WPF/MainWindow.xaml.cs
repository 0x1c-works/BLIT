using BLIT.WPF.Helpers;
using BLIT.WPF.Pages.BannerIcons;
using BLIT.WPF.Services;
using CommunityToolkit.Mvvm.Input;
using Sentry;
using Serilog;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using Wpf.Ui.Controls;
using TextBlock = System.Windows.Controls.TextBlock;
using Timer = System.Timers.Timer;

namespace BLIT.WPF;

public partial class MainWindow {
    private const int LoadingDelayMs = 300; // Delay before showing loading overlay
    private Timer? _loadingDelayTimer;

    public MainWindow() {
        InitializeComponent();
        DataContext = Model;

        Loaded += MainWindow_Loaded;
    }

    private ViewModel Model { get; } = new();

    private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
        Log.Information($"MainWindow loaded. NavigationView items count: {MainNavigationView.MenuItems.Count}");

        // Subscribe to navigation events
        MainNavigationView.Navigating += MainNavigationView_Navigating;
        MainNavigationView.Navigated += MainNavigationView_Navigated;

        // Auto-navigate to BannerIcons page on startup
        if (MainNavigationView.MenuItems.Count > 0) {
            MainNavigationView.Navigate(typeof(BannerIconsPage));
            Log.Information("Auto-navigated to BannerIcons page on startup");
        }
    }

    private void MainNavigationView_Navigating(object sender, RoutedEventArgs args) {
        if (sender is NavigationView navView) {
            // Get the currently navigating page type from the selected item
            var selectedItem = navView.SelectedItem as NavigationViewItem;
            Type? pageType = selectedItem?.TargetPageType;

            Log.Information($"Navigation starting to: {pageType?.Name}");

            // Get the menu item name
            var menuItemName = GetMenuItemNameForPageType(pageType);
            if (menuItemName != null) {
                // Start a timer to show loading overlay after delay (to avoid flicker on fast loads)
                StartLoadingDelayTimer(menuItemName);
            }
        }
    }

    private void MainNavigationView_Navigated(object sender, RoutedEventArgs args) {
        if (sender is NavigationView navView) {
            var selectedItem = navView.SelectedItem as NavigationViewItem;
            Type? pageType = selectedItem?.TargetPageType;

            Log.Information($"Navigation completed to: {pageType?.Name}");
        }

        // Stop the timer and hide loading overlay
        StopLoadingDelayTimer();
        HideLoadingOverlay();
    }

    private string? GetMenuItemNameForPageType(Type? pageType) {
        if (pageType == null) {
            return null;
        }

        // Search in MenuItems
        foreach (var item in MainNavigationView.MenuItems) {
            if (item is NavigationViewItem navItem && navItem.TargetPageType == pageType) {
                return GetMenuItemDisplayText(navItem);
            }
        }

        // Search in FooterMenuItems
        foreach (var item in MainNavigationView.FooterMenuItems) {
            if (item is NavigationViewItem navItem && navItem.TargetPageType == pageType) {
                return GetMenuItemDisplayText(navItem);
            }
        }

        return null;
    }

    private string? GetMenuItemDisplayText(NavigationViewItem item) {
        // Try to get Content as string (should be already localized from I18n binding)
        if (item.Content is string contentStr) {
            return contentStr;
        }

        // If Content is a FrameworkElement, try to extract text
        if (item.Content is TextBlock textBlock) {
            return textBlock.Text;
        }

        return item.Content?.ToString();
    }

    private void StartLoadingDelayTimer(string menuItemName) {
        StopLoadingDelayTimer();

        _loadingDelayTimer = new Timer(LoadingDelayMs);
        _loadingDelayTimer.Elapsed += (s, e) => {
            StopLoadingDelayTimer();

            // Show loading overlay on the UI thread
            Dispatcher.Invoke(() => {
                ShowLoadingOverlay(menuItemName);
            });
        };
        _loadingDelayTimer.AutoReset = false;
        _loadingDelayTimer.Start();
    }

    private void StopLoadingDelayTimer() {
        if (_loadingDelayTimer != null) {
            _loadingDelayTimer.Stop();
            _loadingDelayTimer.Dispose();
            _loadingDelayTimer = null;
        }
    }

    private void ShowLoadingOverlay(string menuItemName) {
        try {
            var message = I18n.Current.GetString("PageLoading.Message");
            var formattedMessage = string.Format(message, menuItemName);

            var loadingService = AppServices.Get<ILoadingService>();
            loadingService?.Show(formattedMessage);

            Log.Information($"Loading overlay shown: {formattedMessage}");
        } catch (Exception ex) {
            Log.Error($"Error showing loading overlay: {ex.Message}");
        }
    }

    private void HideLoadingOverlay() {
        try {
            StopLoadingDelayTimer();

            var loadingService = AppServices.Get<ILoadingService>();
            loadingService?.Hide();

            Log.Information("Loading overlay hidden");
        } catch (Exception ex) {
            Log.Error($"Error hiding loading overlay: {ex.Message}");
        }
    }

    #region Nested type: ViewModel

    public partial class ViewModel : INotifyPropertyChanged {
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

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        private void OnPropertyChanged([CallerMemberName] string? prop = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        [RelayCommand]
        public void OpenHelp() {
            try {
                Log.Information("Opening help in browser");
                SentrySdk.AddBreadcrumb("Visit help", "ui.nav");

                var helpUrl = I18n.Current.GetString("LinkHelpWebsite");
                Log.Information($"Help URL: {helpUrl}");

                Process.Start(new ProcessStartInfo { FileName = helpUrl, UseShellExecute = true });

                Log.Information("Help URL opened successfully");
            } catch (Exception ex) {
                Log.Error($"Failed to open help URL: {ex.Message}");
            }
        }
    }

    #endregion
}

public record NavPageHeaderInfo(string Title, string? SubTitle = null, bool IsModified = false) {
    public bool HasSubTitle { get; set; } = true;

    public string SubTitle { get; init; } = string.IsNullOrWhiteSpace(SubTitle)
        ? I18n.Current.GetString("Placeholder.NewProject")
        : SubTitle;
}