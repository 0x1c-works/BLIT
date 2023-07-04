// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BannerlordImageTool.Win.Helpers;
using BannerlordImageTool.Win.Pages.BannerIcons.ViewModels;
using BannerlordImageTool.Win.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BannerlordImageTool.Win.Pages.BannerIcons;

public sealed partial class BannerIconGroupEditor : UserControl
{
    static readonly Guid GUID_TEXTURE_DIALOG = new("8a8429ec-b674-40d8-82f0-ad42be0d6e8f");
    static readonly Guid GUID_SPRITE_DIALOG = new("7fb7d0f4-e50d-4fa3-a890-ae0775bca3d8");

    public BannerGroupViewModel ViewModel
    {
        get => GetValue(ViewModelProperty) as BannerGroupViewModel;
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(BannerGroupViewModel),
        typeof(BannerIconsPage),
        new PropertyMetadata(default(BannerGroupViewModel)));

    public BannerIconGroupEditor()
    {
        InitializeComponent();
    }

    async void btnDeleteSelectedTextures_Click(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.HasSelection)
        {
            return;
        }

        ContentDialogResult result = await AppServices.Get<IConfirmDialogService>().ShowDanger(
            this,
            I18n.Current.GetString("DialogDeleteBannerIcon/Title"),
            string.Format(I18n.Current.GetString("DialogDeleteBannerIcon/Content"), ViewModel.AllSelection.Count()));
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.DeleteIcons(ViewModel.AllSelection);
        }
    }

    async void btnOpenTextures_Click(object sender, RoutedEventArgs e)
    {
        System.Collections.Generic.IReadOnlyList<Windows.Storage.StorageFile> files = await AppServices.Get<IFileDialogService>().OpenFiles(GUID_TEXTURE_DIALOG, new[] { CommonFileTypes.Png });

        if (files.Count == 0)
        {
            return;
        }

        ViewModel.AddIcons(files);
    }

    void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var changed = false;
        foreach (BannerIconViewModel item in e.AddedItems.Where(item => item is BannerIconViewModel).Cast<BannerIconViewModel>())
        {
            item.IsSelected = true;
            changed = true;
        }
        foreach (BannerIconViewModel item in e.RemovedItems.Where(item => item is BannerIconViewModel).Cast<BannerIconViewModel>())
        {
            item.IsSelected = false;
            changed = true;
        }
        if (changed)
        {
            ViewModel.NotifySelectionChange();
        }
    }

    async void btnSelectSprite_Click(object sender, RoutedEventArgs e)
    {
        Windows.Storage.StorageFile file = await AppServices.Get<IFileDialogService>().OpenFile(GUID_SPRITE_DIALOG,
                                                                       ViewModel.SingleSelection.SpritePath,
                                                                       new[] { CommonFileTypes.Png });
        if (file is null || ViewModel.SingleSelection is null)
        {
            return;
        }

        ViewModel.SingleSelection.SpritePath = file.Path;
    }

    async void btnSelectTexture_Click(object sender, RoutedEventArgs e)
    {
        Windows.Storage.StorageFile file = await AppServices.Get<IFileDialogService>().OpenFile(GUID_TEXTURE_DIALOG,
                                                                       ViewModel.SingleSelection.TexturePath,
                                                                       new[] { CommonFileTypes.Png });
        if (file is null || ViewModel.SingleSelection is null)
        {
            return;
        }

        ViewModel.SingleSelection.TexturePath = file.Path;
    }

}
