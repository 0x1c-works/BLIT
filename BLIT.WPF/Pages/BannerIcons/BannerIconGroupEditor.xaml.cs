using BLIT.WPF.Pages.BannerIcons.Models;
using BLIT.WPF.Pages.BannerIcons.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace BLIT.WPF.Pages.BannerIcons;

/// <summary>
/// Banner Icon Group Editor User Control
/// </summary>
public partial class BannerIconGroupEditor : UserControl {
    private readonly BannerIconGroupEditorViewModel? _viewModel = new();
    private bool _isInitialized = false;

    public BannerIconGroupEditor() {
        InitializeComponent();
        // Set DataContext to the internal ViewModel so XAML bindings work
        DataContext = _viewModel;
        
        // 订阅 IsVisibleChanged 事件，延迟初始化直到需要显示
        this.IsVisibleChanged += (s, e) => {
            if (this.IsVisible && !_isInitialized) {
                InitializeBindings();
            }
        };
        
        // 同时订阅 Loaded 事件作为备选
        this.Loaded += (s, e) => {
            if (this.IsVisible && !_isInitialized) {
                InitializeBindings();
            }
        };
    }

    private void InitializeBindings() {
        if (_isInitialized) return;
        _isInitialized = true;

        if (this.Parent is FrameworkElement parent) {
            var pageDataContext = parent.DataContext;
            if (pageDataContext is BannerIconsPageViewModel pageViewModel && _viewModel != null) {
                _viewModel.GroupData = pageViewModel.SelectedGroup;
                System.Diagnostics.Debug.WriteLine($"[BannerIconGroupEditor] Set GroupData from parent ViewModel: {pageViewModel.SelectedGroup?.GroupID}");
                
                // 订阅父 ViewModel 的 PropertyChanged 事件
                pageViewModel.PropertyChanged += (ps, pe) => {
                    if (pe.PropertyName == nameof(BannerIconsPageViewModel.SelectedGroup)) {
                        _viewModel.GroupData = pageViewModel.SelectedGroup;
                        System.Diagnostics.Debug.WriteLine($"[BannerIconGroupEditor] GroupData updated: {pageViewModel.SelectedGroup?.GroupID}");
                    }
                };
            }
        }
    }

    private void listIcons_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (_viewModel != null) {
            _viewModel.OnSelectionChanged(listIcons.SelectedItems.Cast<BannerIconEntry>());
        }
    }
}
