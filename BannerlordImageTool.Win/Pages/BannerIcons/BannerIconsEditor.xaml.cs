// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BannerlordImageTool.Banner;
using BannerlordImageTool.Win.Common;
using BannerlordImageTool.Win.Services;
using BannerlordImageTool.Win.Settings;
using BannerlordImageTool.Win.ViewModels.BannerIcons;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BannerlordImageTool.Win.Pages.BannerIcons;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BannerIconsEditor : Page
{
    const string PROJECT_FILE_TYPE_NAME = "Banner Icons Project";
    const string PROJECT_FILE_EXT = ".bip";
    static readonly IDictionary<string, IList<string>> SAVE_FILE_TYPE = new Dictionary<string, IList<string>>() {
        {PROJECT_FILE_TYPE_NAME, new []{PROJECT_FILE_EXT} },
    };
    static readonly Guid GUID_EXPORT_DIALOG = new Guid("0c5f39f0-1a31-4d85-a9ee-7ad0cfd690b6");
    static readonly Guid GUID_PROJECT_DIALOG = new Guid("f86d402a-33de-4f62-8c2b-c5e75428c018");

    DataViewModel ViewModel { get => App.Current.BannerViewModel; }

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

    private async void btnExportAll_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.IsExporting) return;
        try
        {
            var outFolder = await FileDialogService.Current.OpenFolder(GUID_EXPORT_DIALOG);
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
            await ExportXML(outFolder);
            ViewModel.IsExporting = false;

            var btnGo = new Button() {
                Content = I18n.Current.GetString("ButtonOpenFolder/Content"),
            };
            btnGo.Click += (s, e) => Process.Start("explorer.exe", outFolder.Path);
            ShowSuccessInfo(
                string.Format(I18n.Current.GetString("ExportSuccess"), outFolder.Path),
                btnGo);
        }
        catch (Exception ex)
        {
            //await new MessageDialog(ex.Message, "Error").ShowAsync();
        }
    }
    private async void btnExportXML_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var outDir = await ExportXML(null);
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
        catch (Exception ex)
        {
            // TODO: error toast
        }
    }

    async Task<string> ExportXML(StorageFolder outFolder)
    {
        try
        {
            if (outFolder is null)
            {
                outFolder = await FileDialogService.Current.OpenFolder(GUID_EXPORT_DIALOG);
            }
            if (outFolder is not null)
            {
                ViewModel.ToBannerIconData().SaveToXml(outFolder.Path);
                SpriteOrganizer.GenerateConfigXML(outFolder.Path, ViewModel.ToIconSprites());
                return outFolder.Path;
            }
            return null;
        }
        catch (Exception ex)
        {
            // TODO: error toast
            return null;
        }
    }

    private async void btnDeleteGroup_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedGroup is null) return;
        var result = await DialogHelper.ShowDangerConfirmDialog(
            this,
            I18n.Current.GetString("DialogDeleteBannerGroup/Title"),
            string.Format(I18n.Current.GetString("DialogDeleteBannerGroup/Content"), ViewModel.SelectedGroup.GroupID));
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.DeleteGroup(ViewModel.SelectedGroup);
        }
    }

    private void btnNewProject_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Reset();
    }
    private async void btnSaveProject_Click(object sender, RoutedEventArgs e)
    {
        await SaveProject(false);
    }

    private async void btnOpenProject_Click(object sender, RoutedEventArgs e)
    {
        var file = await FileDialogService.Current.OpenFile(GUID_PROJECT_DIALOG, new[] { PROJECT_FILE_EXT });
        if (file is null) return;
        await ViewModel.Load(file);
    }

    private async void btnSaveProjectAs_Click(object sender, RoutedEventArgs e)
    {
        await SaveProject(true);
    }

    async Task SaveProject(bool force)
    {
        StorageFile file = ViewModel.CurrentFile;
        if (force || file is null)
        {
            file = await FileDialogService.Current.SaveFile(GUID_PROJECT_DIALOG, SAVE_FILE_TYPE, "banner_icons", file);
        }
        if (file is null) return;

        await ViewModel.Save(file);
    }
}
