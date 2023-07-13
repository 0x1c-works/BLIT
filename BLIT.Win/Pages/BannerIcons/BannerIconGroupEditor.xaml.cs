// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BLIT.Win.Helpers;
using BLIT.Win.Pages.BannerIcons.Models;
using BLIT.Win.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BLIT.Win.Pages.BannerIcons;

public sealed partial class BannerIconGroupEditor : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    static readonly Guid GUID_TEXTURE_DIALOG = new("8a8429ec-b674-40d8-82f0-ad42be0d6e8f");
    static readonly Guid GUID_SPRITE_DIALOG = new("7fb7d0f4-e50d-4fa3-a890-ae0775bca3d8");

    public BannerGroupEntry ViewModel
    {
        get => GetValue(ViewModelProperty) as BannerGroupEntry;
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(BannerGroupEntry),
        typeof(BannerIconsPage),
        new PropertyMetadata(default(BannerGroupEntry)));

    IEnumerable<BannerIconEntry> SelectedIcons { get => gridIcons.SelectedItems.Cast<BannerIconEntry>(); }
    bool HasSelectedIcons { get => gridIcons.SelectedItems.Any(); }
    BannerIconEntry FirstSelectedIcon { get => SelectedIcons.FirstOrDefault(); }
    bool CanReimportSprite { get => HasSelectedIcons && ImageHelper.IsValidImage(FirstSelectedIcon.SpritePath); }
    bool CanReimportTexture { get => HasSelectedIcons && ImageHelper.IsValidImage(FirstSelectedIcon.TexturePath); }

    public BannerIconGroupEditor()
    {
        InitializeComponent();
    }

    async void btnDeleteSelectedTextures_Click(object sender, RoutedEventArgs e)
    {
        if (!HasSelectedIcons)
        {
            return;
        }

        ContentDialogResult result = await AppServices.Get<IConfirmDialogService>().ShowDanger(
            this,
            I18n.Current.GetString("DialogDeleteBannerIcon/Title"),
            string.Format(I18n.Current.GetString("DialogDeleteBannerIcon/Content"), SelectedIcons.Count()));
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.DeleteIcons(SelectedIcons);
        }
    }

    async void btnOpenTextures_Click(object sender, RoutedEventArgs e)
    {
        IReadOnlyList<Windows.Storage.StorageFile> files = await AppServices.Get<IFileDialogService>().OpenFiles(GUID_TEXTURE_DIALOG, new[] { CommonFileTypes.Png });

        if (files.Count == 0)
        {
            return;
        }

        ViewModel.AddIcons(files);
    }

    void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var changed = e.AddedItems.Any() || e.RemovedItems.Any();
        if (changed)
        {
            Bindings.Update();
        }
    }

    async void btnSelectSprite_Click(object sender, RoutedEventArgs e)
    {
        Windows.Storage.StorageFile file = await AppServices.Get<IFileDialogService>().OpenFile(GUID_SPRITE_DIALOG,
                                                                       FirstSelectedIcon.SpritePath,
                                                                       new[] { CommonFileTypes.Png });
        if (file is null || FirstSelectedIcon is null)
        {
            return;
        }

        FirstSelectedIcon.SpritePath = file.Path;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanReimportSprite)));
    }
    async void btnSelectTexture_Click(object sender, RoutedEventArgs e)
    {
        Windows.Storage.StorageFile file = await AppServices.Get<IFileDialogService>().OpenFile(GUID_TEXTURE_DIALOG,
                                                                       FirstSelectedIcon.TexturePath,
                                                                       new[] { CommonFileTypes.Png });
        if (file is null || FirstSelectedIcon is null)
        {
            return;
        }
        FirstSelectedIcon.TexturePath = file.Path;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanReimportTexture)));
    }
    void btnReimportSprite_Click(object sender, RoutedEventArgs e)
    {
        FirstSelectedIcon.ReloadSprite();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanReimportSprite)));
    }
    void btnReimportTexture_Click(object sender, RoutedEventArgs e)
    {
        FirstSelectedIcon.ReloadTexture();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanReimportSprite)));
    }
}
