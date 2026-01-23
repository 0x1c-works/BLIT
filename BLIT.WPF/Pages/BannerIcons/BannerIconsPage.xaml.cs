using BLIT.WPF.Pages.BannerIcons.Models;
using BLIT.WPF.Pages.BannerIcons.ViewModels;
using BLIT.WPF.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BLIT.WPF.Pages.BannerIcons;

/// <summary>
/// Banner Icons 编辑页面
/// </summary>
public partial class BannerIconsPage : Page {
    private readonly BannerIconsPageViewModel? _viewModel = new BannerIconsPageViewModel();

    public BannerIconsPage() {
        System.Diagnostics.Debug.WriteLine("BannerIconsPage: Constructor called");
        InitializeComponent();

        DataContext = _viewModel;
        Loaded += OnPageLoaded;
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e) {
        System.Diagnostics.Debug.WriteLine("BannerIconsPage: Page loaded");

        // 设置输出分辨率默认值
        if (_viewModel?.ViewModel != null) {
            var resolutionName = _viewModel.ViewModel.OutputResolutionName;
            if (resolutionName == "2K") {
                comboOutputResolution.SelectedIndex = 0;
            } else if (resolutionName == "4K") {
                comboOutputResolution.SelectedIndex = 1;
            }
        }

        System.Diagnostics.Debug.WriteLine($"BannerIconsPage: Initialization complete, ViewModel={_viewModel?.ViewModel != null}");
    }

    // 分组列表选择变化
    private void listViewGroups_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        _viewModel?.OnSelectionChanged(listViewGroups.SelectedItem as BannerGroupEntry);
    }

    // 输出分辨率选择
    private void comboOutputResolution_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (_viewModel?.ViewModel == null || comboOutputResolution.SelectedItem == null) return;

        var item = comboOutputResolution.SelectedItem as ComboBoxItem;
        var tag = item?.Tag as string;
        if (tag != null) {
            _viewModel.ViewModel.OutputResolutionName = tag;
        }
    }
}
