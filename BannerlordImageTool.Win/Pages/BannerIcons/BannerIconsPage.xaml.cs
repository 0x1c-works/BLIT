// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BannerlordImageTool.Banner;
using BannerlordImageTool.Win.Controls;
using BannerlordImageTool.Win.Helpers;
using BannerlordImageTool.Win.Pages.BannerIcons.ViewModels;
using BannerlordImageTool.Win.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
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
public sealed partial class BannerIconsPage : Page
{
    static readonly Guid GUID_EXPORT_DIALOG = new Guid("0c5f39f0-1a31-4d85-a9ee-7ad0cfd690b6");
    static readonly Guid GUID_PROJECT_DIALOG = new Guid("f86d402a-33de-4f62-8c2b-c5e75428c018");

    private readonly ISettingsService _settings = AppServices.Get<ISettingsService>();
    private BannerIconsPageViewModel ViewModel { get => AppServices.Get<BannerIconsPageViewModel>(); }

    public BannerIconsPage()
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

    void btnImport_Click(object sender, RoutedEventArgs e)
    {

    }

    private void btnAddGroup_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.AddGroup();
    }

    private async void btnExportAll_Click(object sender, RoutedEventArgs e)
    {
        var outFolder = await AppServices.Get<IFileDialogService>().OpenFolder(GUID_EXPORT_DIALOG);
        if (outFolder == null) return;
        await DoExportAsync(async () => {
            TextureMerger merger = new TextureMerger(_settings.Banner.TextureOutputResolution);
            await Task.WhenAll(ViewModel.GetExportingGroups().Select(g =>
                Task.Factory.StartNew(() => {
                    merger.Merge(outFolder.Path, g.GroupID, g.Icons.Select(icon => icon.TexturePath).ToArray());
                })
            ));
            await SpriteOrganizer.CollectToSpriteParts(outFolder.Path, ViewModel.ToIconSprites());
            var outDir = await ExportXML(outFolder);

            AppServices.Get<INotificationService>().Notify(new(
                ToastVariant.Success,
                Message: string.Format(I18n.Current.GetString("ExportSuccess"), outDir),
                Action: new(
                    I18n.Current.GetString("ButtonOpenFolder/Content"),
                    (s, e) => FileHelpers.OpenFolderInExplorer(outDir))));
        });
    }
    private async void btnExportXML_Click(object sender, RoutedEventArgs e)
    {
        var outFolder = await AppServices.Get<IFileDialogService>().OpenFolder(GUID_EXPORT_DIALOG);
        if (outFolder == null) return;
        await DoExportAsync(async () => {
            var outDir = await ExportXML(outFolder);
            AppServices.Get<INotificationService>().Notify(new(
                ToastVariant.Success,
                Message: string.Format(I18n.Current.GetString("SaveXMLSuccess"), Path.Join(outDir, "banner_icons.xml")),
                Action: new(
                    I18n.Current.GetString("ButtonOpenFolder/Content"),
                    (s, e) => FileHelpers.OpenFolderInExplorer(outDir))));
        });
    }

    async Task DoExportAsync(Func<Task> work)
    {
        if (ViewModel.IsExporting) return;
        Toast progressToast = null;
        try
        {
            progressToast = AppServices.Get<INotificationService>().Notify(new(
                ToastVariant.Progressing,
                Message: I18n.Current.GetString("TextExporting/Text"),
                KeepOpen: true,
                IsClosable: false
            ));
            ViewModel.IsExporting = true;
            await work();
        }
        catch (Exception ex)
        {
            AppServices.Get<INotificationService>().Notify(new(
                ToastVariant.Error,
                Message: ex.Message,
                Title: string.Format(
                    I18n.Current.GetString("ErrorWhen"),
                    I18n.Current.GetString("OperationExporting"))));
        }
        finally
        {
            ViewModel.IsExporting = false;
            if (progressToast != null)
            {
                progressToast.IsOpen = false;
            }
        }
    }

    async Task<string> ExportXML(StorageFolder outFolder)
    {
        if (outFolder is null)
        {
            outFolder = await AppServices.Get<IFileDialogService>().OpenFolder(GUID_EXPORT_DIALOG);
        }
        if (outFolder is not null)
        {
            ViewModel.ToBannerIconData().SaveToXml(outFolder.Path);
            SpriteOrganizer.GenerateConfigXML(outFolder.Path, ViewModel.ToIconSprites());
            return outFolder.Path;
        }
        return null;
    }

    private async void btnDeleteGroup_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedGroup is null) return;
        var result = await AppServices.Get<IConfirmDialogService>().ShowDanger(
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
        var file = await AppServices.Get<IFileDialogService>().OpenFile(GUID_PROJECT_DIALOG, new[] { CommonFileTypes.BannerIconsProject });
        if (file is null) return;
        await ViewModel.Load(file);
    }

    private async void btnSaveProjectAs_Click(object sender, RoutedEventArgs e)
    {
        await SaveProject(true);
    }

    async Task SaveProject(bool force)
    {
        string filePath = ViewModel.CurrentFile?.Path;
        if (force || string.IsNullOrEmpty(filePath))
        {
            filePath = AppServices.Get<IFileDialogService>().SaveFile(GUID_PROJECT_DIALOG,
                                                                     new[] { CommonFileTypes.BannerIconsProject },
                                                                     "banner_icons",
                                                                     filePath);
        }
        if (string.IsNullOrEmpty(filePath)) return;

        await ViewModel.Save(filePath);
    }
}
