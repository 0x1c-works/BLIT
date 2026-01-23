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

    // Dependency Property for ViewModel (BannerGroupEntry data)
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(BannerGroupEntry),
        typeof(BannerIconGroupEditor),
        new PropertyMetadata(null, OnViewModelChanged));

    public BannerGroupEntry? ViewModel {
        get => (BannerGroupEntry?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is BannerIconGroupEditor editor) {
            // Update the internal ViewModel when the data context changes
            if (editor._viewModel != null) {
                editor._viewModel.GroupData = e.NewValue as BannerGroupEntry;
            }
        }
    }

    public BannerIconGroupEditor() {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void listIcons_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (_viewModel != null) {
            _viewModel.OnSelectionChanged(listIcons.SelectedItems.Cast<BannerIconEntry>());
        }
    }
}
