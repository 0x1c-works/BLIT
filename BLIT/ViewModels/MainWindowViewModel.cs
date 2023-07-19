using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace BLIT.ViewModels;
public partial class MainWindowViewModel : ObservableObject
{
    bool _isInitialized = false;

    [ObservableProperty]
    string _applicationTitle = string.Empty;

    [ObservableProperty]
    ObservableCollection<INavigationControl> _navigationItems = new();

    [ObservableProperty]
    ObservableCollection<INavigationControl> _navigationFooter = new();

    [ObservableProperty]
    ObservableCollection<MenuItem> _trayMenuItems = new();

    public MainWindowViewModel(INavigationService navigationService)
    {
        if (!_isInitialized)
            InitializeViewModel();
    }

    void InitializeViewModel()
    {
        ApplicationTitle = "WPF UI - BLIT";

        NavigationItems = new ObservableCollection<INavigationControl>
        {
            // FIXME: will add this back someday in the future
            //new NavigationItem()
            //{
            //    Content = "Home",
            //    PageTag = "dashboard",
            //    Icon = SymbolRegular.Home24,
            //    PageType = typeof(Views.Pages.DashboardPage)
            //},
            new NavigationItem()
            {
                Content = "Data",
                PageTag = "data",
                Icon = SymbolRegular.DataHistogram24,
                PageType = typeof(Views.Pages.DataPage)
            },
            new NavigationItem()
            {
                Content = "Banner Icons",
                PageTag = "banner-icons",
                Icon = SymbolRegular.Flag24,
                PageType = typeof(Views.Pages.BannerIconsPage)
            }
        };

        NavigationFooter = new ObservableCollection<INavigationControl>
        {
            new NavigationItem()
            {
                Content = "Settings",
                PageTag = "settings",
                Icon = SymbolRegular.Settings24,
                PageType = typeof(Views.Pages.SettingsPage)
            }
        };

        TrayMenuItems = new ObservableCollection<MenuItem>
        {
            new MenuItem
            {
                Header = "Home",
                Tag = "tray_home"
            }
        };

        _isInitialized = true;
    }
}
