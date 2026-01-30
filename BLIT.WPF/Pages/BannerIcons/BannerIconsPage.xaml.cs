using BLIT.WPF.Pages.BannerIcons.Models;
using BLIT.WPF.Pages.BannerIcons.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Abstractions.Controls;

namespace BLIT.WPF.Pages.BannerIcons;

/// <summary>
///     Banner Icons 编辑页面
/// </summary>
public partial class BannerIconsPage : INavigableView<BannerIconsPageViewModel> {
    private readonly BannerIconsPageViewModel? _viewModel = new();

    public BannerIconsPage() {
        Debug.WriteLine("BannerIconsPage: Constructor called");
        InitializeComponent();

        DataContext = _viewModel;
        Loaded += OnPageLoaded;
    }

    #region INavigableView<BannerIconsPageViewModel> Members

    public BannerIconsPageViewModel ViewModel => _viewModel ?? throw new NullReferenceException("ViewModel is null");

    #endregion

    private void OnPageLoaded(object sender, RoutedEventArgs e) {
        Debug.WriteLine("BannerIconsPage: Page loaded");

        // 订阅 ViewModel 的属性变化，以便在 ViewModel 改变时更新 DataContext
        if (_viewModel != null) {
            _viewModel.PropertyChanged += (s, args) => {
                if (args.PropertyName == nameof(BannerIconsPageViewModel.ViewModel)) {
                    // 重新设置 DataContext 以确保绑定更新
                    DataContext = _viewModel;
                }
            };
            // 设置输出分辨率默认值
            if (_viewModel.ViewModel != null) {
                var resolutionName = _viewModel.ViewModel.OutputResolutionName;
                if (resolutionName == "2K") {
                    comboOutputResolution.SelectedIndex = 0;
                } else if (resolutionName == "4K") {
                    comboOutputResolution.SelectedIndex = 1;
                }
            }
            _viewModel.CheckScanFolders();
        }
    }

    // 分组列表选择变化
    private void listViewGroups_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        // SelectionChanged 事件处理器现在由 SelectedItem 绑定处理
        // 但保留此处理器以防需要其他副作用
    }

    // 输出分辨率选择
    private void comboOutputResolution_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (_viewModel?.ViewModel == null || comboOutputResolution.SelectedItem == null) {
            return;
        }

        var item = comboOutputResolution.SelectedItem as ComboBoxItem;
        if (item?.Tag is string tag) {
            _viewModel.ViewModel.OutputResolutionName = tag;
        }
    }
}