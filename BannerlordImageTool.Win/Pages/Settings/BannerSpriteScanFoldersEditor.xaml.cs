// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BannerlordImageTool.Win.ViewModels.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BannerlordImageTool.Win.Pages.Settings;

public sealed partial class BannerSpriteScanFoldersEditor : UserControl
{
    public BannerSettingsViewModel ViewModel
    {
        get => GetValue(ViewModelProperty) as BannerSettingsViewModel;
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel), typeof(BannerSettingsViewModel), typeof(BannerSpriteScanFoldersEditor),
        new PropertyMetadata(null));

    public BannerSpriteScanFoldersEditor()
    {
        this.InitializeComponent();
    }

    private void btnAdd_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SpriteScanFolders.Add(new BannerSpriteScanFolderViewModel());
        ViewModel.SelectedSpriteScanFolderIndex = ViewModel.SpriteScanFolders.Count - 1;
    }

    private void btnEdit_Click(object sender, RoutedEventArgs e)
    {
        EditSelectedFolder();
    }

    private void btnDelete_Click(object sender, RoutedEventArgs e)
    {
        DeleteSelectedFolder();
    }

    private void listViewBannerSrpiteScanFolders_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0)
        {
            ViewModel.SelectedSpriteScanFolderIndex = -1;
        }
        else
        {
            ViewModel.SelectedSpriteScanFolderIndex = listViewBannerSrpiteScanFolders.SelectedIndex;
        }
    }

    private void listViewBannerSrpiteScanFolders_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        EditSelectedFolder();
    }

    void EditSelectedFolder()
    {
        if (ViewModel.SelectedSpriteScanFolder is null) return;
        ViewModel.SelectedSpriteScanFolder.IsEditing = true;
    }
    void DeleteSelectedFolder()
    {
        if (ViewModel.SelectedSpriteScanFolder is null) return;
        ViewModel.SpriteScanFolders.Remove(ViewModel.SelectedSpriteScanFolder);
    }
}
