using BLIT.WPF.Helpers;
using BLIT.WPF.Pages.BannerIcons.Models;
using BLIT.WPF.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BLIT.WPF.Pages.BannerIcons;

public partial class BannerIconGroupEditor : UserControl, INotifyPropertyChanged {
    public event PropertyChangedEventHandler? PropertyChanged;

    private static readonly Guid GUID_TEXTURE_DIALOG = new("8a8429ec-b674-40d8-82f0-ad42be0d6e8f");
    private static readonly Guid GUID_SPRITE_DIALOG = new("7fb7d0f4-e50d-4fa3-a890-ae0775bca3d8");

    // Dependency Property for ViewModel
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
            editor.OnPropertyChanged(nameof(ViewModel));
        }
    }

    // Properties for binding
    private IEnumerable<BannerIconEntry> SelectedIcons => listIcons.SelectedItems.Cast<BannerIconEntry>();
    
    public bool HasSelectedIcons {
        get => listIcons.SelectedItems.Count > 0;
    }
    
    public BannerIconEntry? FirstSelectedIcon => SelectedIcons.FirstOrDefault();
    
    public bool CanReimportSprite {
        get => HasSelectedIcons && FirstSelectedIcon != null && ImageHelper.IsValidImage(FirstSelectedIcon.SpritePath);
    }
    
    public bool CanReimportTexture {
        get => HasSelectedIcons && FirstSelectedIcon != null && ImageHelper.IsValidImage(FirstSelectedIcon.TexturePath);
    }

    public BannerIconGroupEditor() {
        InitializeComponent();
    }

    private void listIcons_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        OnPropertyChanged(nameof(HasSelectedIcons));
        OnPropertyChanged(nameof(FirstSelectedIcon));
        OnPropertyChanged(nameof(CanReimportSprite));
        OnPropertyChanged(nameof(CanReimportTexture));
    }

    private async void btnAddTextures_Click(object sender, RoutedEventArgs e) {
        var fileDialog = AppServices.Get<IFileDialogService>();
        if (fileDialog == null || ViewModel == null) return;

        var files = await fileDialog.OpenFiles(GUID_TEXTURE_DIALOG, new[] { CommonFileTypes.Png });
        if (files == null || files.Count == 0) {
            return;
        }

        ViewModel.AddIcons(files);
    }

    private async void btnDeleteTextures_Click(object sender, RoutedEventArgs e) {
        if (!HasSelectedIcons || ViewModel == null) {
            return;
        }

        var confirmDialog = AppServices.Get<IConfirmDialogService>();
        if (confirmDialog == null) return;

        var result = await confirmDialog.ShowDanger(
            I18n.Current.GetString("DialogDeleteBannerIcon/Title"),
            string.Format(I18n.Current.GetString("DialogDeleteBannerIcon/Content"), SelectedIcons.Count()));

        if (result == ContentDialogResult.Primary) {
            ViewModel.DeleteIcons(SelectedIcons);
        }
    }

    private async void btnChangeSprite_Click(object sender, RoutedEventArgs e) {
        if (FirstSelectedIcon == null) return;

        var fileDialog = AppServices.Get<IFileDialogService>();
        if (fileDialog == null) return;

        var file = await fileDialog.OpenFile(GUID_SPRITE_DIALOG, 
                                            FirstSelectedIcon.SpritePath,
                                            new[] { CommonFileTypes.Png });
        if (string.IsNullOrEmpty(file)) {
            return;
        }

        FirstSelectedIcon.SpritePath = file;
        OnPropertyChanged(nameof(CanReimportSprite));
    }

    private async void btnChangeTexture_Click(object sender, RoutedEventArgs e) {
        if (FirstSelectedIcon == null) return;

        var fileDialog = AppServices.Get<IFileDialogService>();
        if (fileDialog == null) return;

        var file = await fileDialog.OpenFile(GUID_TEXTURE_DIALOG, 
                                            FirstSelectedIcon.TexturePath,
                                            new[] { CommonFileTypes.Png });
        if (string.IsNullOrEmpty(file)) {
            return;
        }

        FirstSelectedIcon.TexturePath = file;
        OnPropertyChanged(nameof(CanReimportTexture));
    }

    private void btnReimportSprite_Click(object sender, RoutedEventArgs e) {
        FirstSelectedIcon?.ReloadSprite();
        OnPropertyChanged(nameof(CanReimportSprite));
    }

    private void btnReimportTexture_Click(object sender, RoutedEventArgs e) {
        FirstSelectedIcon?.ReloadTexture();
        OnPropertyChanged(nameof(CanReimportTexture));
    }

    protected void OnPropertyChanged(string propertyName) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
