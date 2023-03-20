// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BannerlordImageTool.Banner;
using BannerlordImageTool.Win.Common;
using BannerlordImageTool.Win.Settings;
using BannerlordImageTool.Win.ViewModels.BannerIcons;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BannerlordImageTool.Win.Pages.BannerIcons;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BannerIconsEditor : Page
{
    DataViewModel ViewModel { get; init; } = new();

    public BannerIconsEditor()
    {
        this.InitializeComponent();
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.SelectedGroup))
        {
            listViewGroups.SelectedItem = ViewModel.SelectedGroup;
        }
    }

    void ResolutionOption_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem item)
        {
            return;
        }
        ViewModel.OutputResolutionName = item.Tag as string;
    }

    async void btnExport_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.IsExporting) return;

        var outFolder = await FileHelper.PickFolder($"BannerIconsExportDir", "bannerIconsExportTo");
        if (outFolder == null) return;

        TextureMerger merger = new TextureMerger(GlobalSettings.Current.Banner.TextureOutputResolution);

        ViewModel.IsExporting = true;
        infoExport.IsOpen = false;
        await Task.WhenAll(ViewModel.GetExportingGroups().Select(g =>
            Task.Factory.StartNew(() => {
                merger.Merge(outFolder.Path, g.GroupID, g.Icons.Select(icon => icon.TexturePath).ToArray());
            })
        ));
        await SpriteOrganizer.CollectToSpriteParts(outFolder.Path, ViewModel.ToIconSprites());
        await SaveXML(outFolder);
        ViewModel.IsExporting = false;

        var btnGo = new Button() {
            Content = I18n.Current.GetString("Open"),
        };
        btnGo.Click += (s, e) => Process.Start("explorer.exe", outFolder.Path);
        ShowSuccessInfo(
            string.Format(I18n.Current.GetString("ExportSuccess"), outFolder.Path),
            btnGo);
    }

    void ShowSuccessInfo(string message, Button actionButton)
    {
        infoExport.Message = message;
        infoExport.Severity = InfoBarSeverity.Success;
        infoExport.IsOpen = true;
        infoExport.ActionButton = actionButton;
    }

    void btnImport_Click(object sender, RoutedEventArgs e)
    {

    }

    private void btnAddGroup_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.AddGroup();
    }

    private void listViewGroups_ItemClick(object sender, ItemClickEventArgs e)
    {
        ViewModel.SelectedGroup = e.ClickedItem as GroupViewModel;
    }

    private async void btnSaveXML_Click(object sender, RoutedEventArgs e)
    {
        var outDir = await SaveXML(null);
        if (outDir is not null)
        {
            var btnGo = new Button() {
                Content = I18n.Current.GetString("Open"),
            };
            btnGo.Click += (s, e) => Process.Start("explorer.exe", outDir);
            ShowSuccessInfo(string.Format(I18n.Current.GetString("SaveXMLSuccess"),
                                          Path.Join(outDir, "banner_icons.xml")),
                            btnGo);
        }
    }

    async Task<string> SaveXML(StorageFolder outFolder)
    {
        if (outFolder is null)
        {
            outFolder = await FileHelper.PickFolder("BannerIconsSaveXMLDir", "bannerIconsSaveXMLTo");
        }
        if (outFolder is not null)
        {
            ViewModel.ToBannerIconData().SaveToXml(outFolder.Path);
            return outFolder.Path;
        }
        return null;
    }

    private async void btnDeleteGroup_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedGroup is null) return;
        var result = await DialogHelper.ShowDangerConfirmDialog(
            this,
            I18n.Current.GetString("DialogDeleteBannerGroupTitle"),
            string.Format(I18n.Current.GetString("DialogDeleteBannerGroupContent"), ViewModel.SelectedGroup.GroupID));
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.DeleteGroup(ViewModel.SelectedGroup);
        }
    }
}
