// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BannerlordImageTool.Win.ViewModels.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

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

    }

    private void btnEdit_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedSpriteScanFolder is null) return;

        ViewModel.SelectedSpriteScanFolder.IsEditing = true;
    }

    private void btnDelete_Click(object sender, RoutedEventArgs e)
    {

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
}
