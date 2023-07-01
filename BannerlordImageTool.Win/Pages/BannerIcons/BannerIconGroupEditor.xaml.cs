// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BannerlordImageTool.Win.Common;
using BannerlordImageTool.Win.Services;
using BannerlordImageTool.Win.ViewModels.BannerIcons;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BannerlordImageTool.Win.Pages.BannerIcons;

public sealed partial class BannerIconGroupEditor : UserControl
{
    static readonly Guid GUID_TEXTURE_DIALOG = new Guid("8a8429ec-b674-40d8-82f0-ad42be0d6e8f");
    static readonly Guid GUID_SPRITE_DIALOG = new Guid("7fb7d0f4-e50d-4fa3-a890-ae0775bca3d8");

    public GroupViewModel ViewModel
    {
        get => GetValue(ViewModelProperty) as GroupViewModel;
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(GroupViewModel),
        typeof(BannerIconsEditor),
        new PropertyMetadata(default(GroupViewModel)));

    public BannerIconGroupEditor()
    {
        this.InitializeComponent();
    }

    private async void btnDeleteSelectedTextures_Click(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.HasSelection) return;
        var result = await AppService.Get<IConfirmDialogService>().ShowDanger(
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
        var files = await AppService.Get<IFileDialogService>().OpenFiles(GUID_TEXTURE_DIALOG, new[] { CommonFileTypes.Png });

        if (files.Count == 0) return;
        ViewModel.AddIcons(files);
    }

    private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var changed = false;
        foreach (var item in e.AddedItems.Where(item => item is IconViewModel).Cast<IconViewModel>())
        {
            item.IsSelected = true;
            changed = true;
        }
        foreach (var item in e.RemovedItems.Where(item => item is IconViewModel).Cast<IconViewModel>())
        {
            item.IsSelected = false;
            changed = true;
        }
        if (changed)
        {
            ViewModel.NotifySelectionChange();
        }
    }

    private async void btnSelectSprite_Click(object sender, RoutedEventArgs e)
    {
        var file = await AppService.Get<IFileDialogService>().OpenFile(GUID_SPRITE_DIALOG,
                                                                       ViewModel.SingleSelection.SpritePath,
                                                                       new[] { CommonFileTypes.Png });
        if (file is null || ViewModel.SingleSelection is null) return;
        ViewModel.SingleSelection.SpritePath = file.Path;
    }

    private async void btnSelectTexture_Click(object sender, RoutedEventArgs e)
    {
        var file = await AppService.Get<IFileDialogService>().OpenFile(GUID_TEXTURE_DIALOG,
                                                                       ViewModel.SingleSelection.TexturePath,
                                                                       new[] { CommonFileTypes.Png });
        if (file is null || ViewModel.SingleSelection is null) return;
        ViewModel.SingleSelection.TexturePath = file.Path;
    }

}
