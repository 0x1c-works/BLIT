// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BLIT.Win.Pages.Settings.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BLIT.Win.Pages.Settings;

public sealed partial class BannerSpriteScanFolderItem : UserControl {
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(BannerSpriteScanFolderViewModel),
        typeof(BannerSpriteScanFolderItem),
        new PropertyMetadata(null));

    public BannerSpriteScanFolderItem() {
        InitializeComponent();
    }

    public BannerSpriteScanFolderViewModel ViewModel {
        get => GetValue(ViewModelProperty) as BannerSpriteScanFolderViewModel;
        set => SetValue(ViewModelProperty, value);
    }

    private void editPath_KeyDown(object sender, KeyRoutedEventArgs e) {
        if (e.Key == VirtualKey.Enter) {
            Accept();
        } else if (e.Key == VirtualKey.Escape) {
            Discard();
        }
    }

    private void Accept() {
        ViewModel.IsEditing = false;
        ViewModel.RelativePath = editPath.Text;
    }

    private void Discard() {
        ViewModel.IsEditing = false;
        editPath.Text = ViewModel.RelativePath;
    }

    private void btnAccept_Click(object sender, RoutedEventArgs e) {
        Accept();
    }

    private void btnDiscard_Click(object sender, RoutedEventArgs e) {
        Discard();
    }
}