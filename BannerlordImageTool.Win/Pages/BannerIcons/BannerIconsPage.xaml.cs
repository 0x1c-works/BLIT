// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BannerlordImageTool.Banner;
using BannerlordImageTool.Win.Controls;
using BannerlordImageTool.Win.Helpers;
using BannerlordImageTool.Win.Pages.BannerIcons.Models;
using BannerlordImageTool.Win.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
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
    static readonly Guid GUID_EXPORT_DIALOG = new("0c5f39f0-1a31-4d85-a9ee-7ad0cfd690b6");
    static readonly Guid GUID_PROJECT_DIALOG = new("f86d402a-33de-4f62-8c2b-c5e75428c018");

    readonly ISettingsService _settings = AppServices.Get<ISettingsService>();
    readonly IFileDialogService _fileDialog = AppServices.Get<IFileDialogService>();
    readonly IProjectService<BannerIconsProject> _project = AppServices.Get<IProjectService<BannerIconsProject>>();

    BannerIconsProject ViewModel { get => _project.Current; }

    public BannerIconsPage()
    {
        InitializeComponent();
        _project.PropertyChanged += OnProjectPropertyChanged;
    }

    void OnProjectPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_project.Current))
        {
            _project.Current.PropertyChanged += ViewModel_PropertyChanged;
            Bindings.Update();
        }
    }

    void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
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

    void btnAddGroup_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.AddGroup();
    }

    async void btnExportAll_Click(object sender, RoutedEventArgs e)
    {
        StorageFolder outFolder = await AppServices.Get<IFileDialogService>().OpenFolder(GUID_EXPORT_DIALOG);
        if (outFolder == null)
        {
            return;
        }

        await DoExportAsync(async () => {
            var merger = new TextureMerger(_settings.Banner.TextureOutputResolution);
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
    async void btnExportXML_Click(object sender, RoutedEventArgs e)
    {
        StorageFolder outFolder = await AppServices.Get<IFileDialogService>().OpenFolder(GUID_EXPORT_DIALOG);
        if (outFolder == null)
        {
            return;
        }

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
        if (ViewModel.IsExporting)
        {
            return;
        }

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
        outFolder ??= await AppServices.Get<IFileDialogService>().OpenFolder(GUID_EXPORT_DIALOG);
        if (outFolder is not null)
        {
            ViewModel.ToBannerIconData().SaveToXml(outFolder.Path);
            SpriteOrganizer.GenerateConfigXML(outFolder.Path, ViewModel.ToIconSprites());
            return outFolder.Path;
        }
        return null;
    }

    async void btnDeleteGroup_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedGroup is null)
        {
            return;
        }

        ContentDialogResult result = await AppServices.Get<IConfirmDialogService>().ShowDanger(
            this,
            I18n.Current.GetString("DialogDeleteBannerGroup/Title"),
            string.Format(I18n.Current.GetString("DialogDeleteBannerGroup/Content"), ViewModel.SelectedGroup.GroupID));
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.DeleteGroup(ViewModel.SelectedGroup);
        }
    }

    void btnNewProject_Click(object sender, RoutedEventArgs e)
    {
        _project.NewProject();
    }
    void btnSaveProject_Click(object sender, RoutedEventArgs e)
    {
        Save(false);
    }

    async void btnOpenProject_Click(object sender, RoutedEventArgs e)
    {
        StorageFile openedFile = await _fileDialog.OpenFile(GUID_PROJECT_DIALOG, new[] { CommonFileTypes.BannerIconsProject });
        if (openedFile is null)
        {
            return;
        }
        await _project.Load(openedFile);
    }

    void btnSaveProjectAs_Click(object sender, RoutedEventArgs e)
    {
        Save(true);
    }

    async void Save(bool force)
    {
        var filePath = _project.CurrentFile?.Path;
        if (force || string.IsNullOrEmpty(filePath))
        {
            filePath = _fileDialog.SaveFile(GUID_PROJECT_DIALOG,
                                            new[] { CommonFileTypes.BannerIconsProject },
                                            "banner_icons",
                                            filePath);
        }
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }
        await _project.Save(filePath);
    }
}
