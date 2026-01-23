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

    // Dependency Property for GroupData (BannerGroupEntry data)
    public static readonly DependencyProperty GroupDataProperty = DependencyProperty.Register(
        nameof(GroupData),
        typeof(BannerGroupEntry),
        typeof(BannerIconGroupEditor),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, OnGroupDataChanged));

    public BannerGroupEntry? GroupData {
        get => (BannerGroupEntry?)GetValue(GroupDataProperty);
        set => SetValue(GroupDataProperty, value);
    }

    private static void OnGroupDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is BannerIconGroupEditor editor) {
            System.Diagnostics.Debug.WriteLine($"BannerIconGroupEditor.OnGroupDataChanged called, NewValue={e.NewValue}");
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
