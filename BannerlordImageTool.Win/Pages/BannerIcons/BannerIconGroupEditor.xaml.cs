// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BannerlordImageTool.Win.Common;
using BannerlordImageTool.Win.ViewModels.BannerIcons;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BannerlordImageTool.Win.Pages.BannerIcons;

public sealed partial class BannerIconGroupEditor : UserControl
{
    public GroupViewModel ViewModel
    {
        get => GetValue(ViewModelProperty) as GroupViewModel;
        set
        {
            SetValue(ViewModelProperty, value);
        }
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

    private void btnConfirmDelete_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.DeleteIcons(ViewModel.AllSelection);
        flyoutConfirmDelete.Hide();
    }

    async void btnOpenImages_Click(object sender, RoutedEventArgs e)
    {
        var files = await FileHelper.PickMultipleFiles(".png");

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
        var file = await FileHelper.PickSingleFile(".png");
        if (file is null || ViewModel.SingleSelection is null) return;
        ViewModel.SingleSelection.SpritePath = file.Path;
    }

    private async void btnSelectTexture_Click(object sender, RoutedEventArgs e)
    {
        var file = await FileHelper.PickSingleFile(".png");
        if (file is null || ViewModel.SingleSelection is null) return;
        ViewModel.SingleSelection.TexturePath = file.Path;
    }
}