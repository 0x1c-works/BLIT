using BLIT.WPF.Helpers;
using BLIT.WPF.Pages.BannerIcons.Models;
using BLIT.WPF.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BLIT.WPF.Pages.BannerIcons;

public partial class BannerColorsEditor : UserControl, INotifyPropertyChanged {
    public event PropertyChangedEventHandler? PropertyChanged;
    
    private const int TITLE_MAX_COUNT = 3;

    // Dependency Property for ViewModel
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
            editor.OnPropertyChanged(nameof(ViewModel));
        }
    }

    // Selection Properties
    public IEnumerable<BannerColorEntry> SelectedColors => listViewColors.SelectedItems.Cast<BannerColorEntry>();
    public BannerColorEntry? FirstSelectedColor => SelectedColors.FirstOrDefault();
    public bool HasSelectedColor => SelectedColors.Any();
    public bool IsSingleSelected => listViewColors.SelectedItems.Count == 1;
    public bool IsMultipleSelection => SelectedColors.Count() > 1;

    // Multi-selection Flag Properties
    public bool IsForSigil {
        get => GetMultiSelectionFlag(c => c.IsForSigil);
        set => SetMultiSelectionFlag((c, v) => c.IsForSigil = v, value);
    }

    public bool IsForBackground {
        get => GetMultiSelectionFlag(c => c.IsForBackground);
        set => SetMultiSelectionFlag((c, v) => c.IsForBackground = v, value);
    }

    public string SelectedColorIDs {
        get => string.Join(", ", SelectedColors.Take(TITLE_MAX_COUNT).Select(c => c.ID));
    }

    public string MoreColorsText {
        get {
            var moreCount = SelectedColors.Count() - TITLE_MAX_COUNT;
            return moreCount > 0 ? string.Format(I18n.Current.GetString("AndMore"), moreCount) : string.Empty;
        }
    }

    public BannerColorsEditor() {
        InitializeComponent();
    }

    private void btnAdd_Click(object sender, RoutedEventArgs e) {
        if (ViewModel == null) return;
        ViewModel.AddColor();
        if (ViewModel.Colors.Count > 0) {
            listViewColors.SelectedIndex = ViewModel.Colors.Count - 1;
        }
    }

    private async void btnDelete_Click(object sender, RoutedEventArgs e) {
        if (!SelectedColors.Any() || ViewModel == null) {
            return;
        }

        var confirmDialog = AppServices.Get<IConfirmDialogService>();
        if (confirmDialog == null) return;

        var result = await confirmDialog.ShowDanger(
            I18n.Current.GetString("DialogDeleteColor/Title"),
            string.Format(I18n.Current.GetString("DialogDeleteColor/Content"), SelectedColors.Count()));

        if (result != ContentDialogResult.Primary) {
            return;
        }

        var index = listViewColors.SelectedIndex;
        ViewModel.DeleteColors(SelectedColors);
        var count = ViewModel.Colors.Count;
        listViewColors.SelectedIndex = count > 0 ? Math.Min(index, count - 1) : -1;
    }

    private void btnSort_Click(object sender, RoutedEventArgs e) {
        ViewModel?.SortColors();
    }

    private void listViewColors_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        OnPropertyChanged(nameof(HasSelectedColor));
        OnPropertyChanged(nameof(FirstSelectedColor));
        OnPropertyChanged(nameof(IsSingleSelected));
        OnPropertyChanged(nameof(IsMultipleSelection));
        OnPropertyChanged(nameof(SelectedColorIDs));
        OnPropertyChanged(nameof(MoreColorsText));
        OnPropertyChanged(nameof(IsForSigil));
        OnPropertyChanged(nameof(IsForBackground));
    }

    private void ColorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
        if (FirstSelectedColor == null) return;
        
        // Update the color from RGB sliders
        byte r = (byte)sliderR.Value;
        byte g = (byte)sliderG.Value;
        byte b = (byte)sliderB.Value;
        
        FirstSelectedColor.Color = Color.FromArgb(255, r, g, b);
    }

    private bool GetMultiSelectionFlag(Func<BannerColorEntry, bool> getter) {
        if (!HasSelectedColor) return false;
        return SelectedColors.All(c => getter?.Invoke(c) ?? false);
    }

    private void SetMultiSelectionFlag(Action<BannerColorEntry, bool> setter, bool value) {
        foreach (BannerColorEntry item in SelectedColors) {
            setter?.Invoke(item, value);
        }
        OnPropertyChanged(nameof(IsForSigil));
        OnPropertyChanged(nameof(IsForBackground));
    }

    protected void OnPropertyChanged(string propertyName) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
