using BLIT.WPF.Pages.BannerIcons.Models;
using BLIT.WPF.Pages.BannerIcons.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BLIT.WPF.Pages.BannerIcons;

/// <summary>
/// Banner Colors Editor User Control
/// </summary>
public partial class BannerColorsEditor : UserControl {
    private readonly BannerColorsEditorViewModel? _viewModel = new();
    private bool _isInitialized = false;

    public BannerColorsEditor() {
        InitializeComponent();
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
                _viewModel.ProjectData = pageViewModel.ViewModel;
                System.Diagnostics.Debug.WriteLine($"[BannerColorsEditor] Set ProjectData from parent ViewModel: {pageViewModel.ViewModel}");
                
                // 订阅父 ViewModel 的 PropertyChanged 事件
                pageViewModel.PropertyChanged += (ps, pe) => {
                    if (pe.PropertyName == nameof(BannerIconsPageViewModel.ViewModel)) {
                        _viewModel.ProjectData = pageViewModel.ViewModel;
                        System.Diagnostics.Debug.WriteLine($"[BannerColorsEditor] ProjectData updated: {pageViewModel.ViewModel}");
                    }
                };
            }
        }
    }

    private void listViewColors_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (_viewModel != null) {
            _viewModel.OnSelectionChanged(listViewColors.SelectedItems.Cast<BannerColorEntry>());
        }
    }
}

// Extension class to add R, G, B, HexColor properties to BannerColorEntry
public static class BannerColorEntryExtensions {
    public static byte GetR(this BannerColorEntry entry) => entry.Color.R;
    public static byte GetG(this BannerColorEntry entry) => entry.Color.G;
    public static byte GetB(this BannerColorEntry entry) => entry.Color.B;
    
    public static string GetHexColor(this BannerColorEntry entry) {
        return $"#{entry.Color.R:X2}{entry.Color.G:X2}{entry.Color.B:X2}";
    }
}
