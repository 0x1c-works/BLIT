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

    // Dependency Property for ViewModel (BannerIconsProject data)
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(BannerIconsProject),
        typeof(BannerColorsEditor),
        new PropertyMetadata(null, OnViewModelChanged));

    public BannerIconsProject? ViewModel {
        get => (BannerIconsProject?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is BannerColorsEditor editor) {
            // Update the internal ViewModel when the data context changes
            if (editor._viewModel != null) {
                editor._viewModel.ProjectData = e.NewValue as BannerIconsProject;
            }
        }
    }

    public BannerColorsEditor() {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void listViewColors_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (_viewModel != null) {
            _viewModel.OnSelectionChanged(listViewColors.SelectedItems.Cast<BannerColorEntry>());
        }
    }

    private void ColorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
        if (_viewModel?.FirstSelectedColor == null) return;
        
        // Update the color from RGB sliders
        byte r = (byte)sliderR.Value;
        byte g = (byte)sliderG.Value;
        byte b = (byte)sliderB.Value;
        
        _viewModel.FirstSelectedColor.Color = Color.FromArgb(255, r, g, b);
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
