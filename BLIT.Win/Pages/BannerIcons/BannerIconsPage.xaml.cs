// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BLIT.Win.Controls;
using BLIT.Win.Helpers;
using BLIT.Win.Pages.BannerIcons.Models;
using BLIT.Win.Services;
using Microsoft.AppCenter.Analytics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BLIT.Win.Pages.BannerIcons;

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
    readonly ILoadingService _loading = AppServices.Get<ILoadingService>();
    readonly INotificationService _notification = AppServices.Get<INotificationService>();

    BannerIconsProject ViewModel { get => _project.Current; }
    BannerGroupEntry SelectedGroup { get => listViewGroups.SelectedItem as BannerGroupEntry; }
    bool HasSelectedGroup { get => SelectedGroup is not null; }
    bool ShouldShowEmptyHint { get => !HasSelectedGroup; }

    public BannerIconsPage()
    {
        InitializeComponent();
        _project.PropertyChanged += OnProjectPropertyChanged;
        Loaded += OnPageLoaded;
    }

    void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        if (_settings.Banner.SpriteScanFolders.Count == 0)
        {
            Toast toast = null;
            toast = _notification.Notify(new Notification(
                ToastVariant.Warning,
                Message: I18n.Current.GetString("WarningNoSpriteScanFolders/Message"),
                TimeoutSeconds: 30,
                Action: new(
                    I18n.Current.GetString("ButtonToSettings/Content"),
                    (s, e) => {
                        Analytics.TrackEvent("open settings", new Dictionary<string, string> { { "source", "banner icon's hint" } });
                        if (toast != null) toast.IsOpen = false;
                        (App.Current.MainWindow as MainWindow)?.NavigateToSettings();
                    })
                ));
        }
    }

    void OnProjectPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_project.Current))
        {
            Bindings.Update();
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

    async void btnExportAll_Click(object sender, RoutedEventArgs e)
    {
        StorageFolder outFolder = await SelectOutFolder();
        if (outFolder == null) return;

        await DoExportAsync(async () => {
            var outDir = await ViewModel.ExportAll(outFolder);
            _notification.Notify(new(
                ToastVariant.Success,
                Message: string.Format(I18n.Current.GetString("ExportSuccess"), outDir),
                Action: new(
                    I18n.Current.GetString("ButtonOpenFolder/Content"),
                    (s, e) => FileHelpers.OpenFolderInExplorer(outDir))));
        });
    }
    async void btnExportXML_Click(object sender, RoutedEventArgs e)
    {
        StorageFolder outFolder = await SelectOutFolder();
        if (outFolder == null) return;

        await DoExportAsync(() => {
            var outDir = ViewModel.ExportXML(outFolder);
            _notification.Notify(new(
                ToastVariant.Success,
                Message: string.Format(I18n.Current.GetString("SaveXMLSuccess"), Path.Join(outDir, "banner_icons.xml")),
                Action: new(
                    I18n.Current.GetString("ButtonOpenFolder/Content"),
                    (s, e) => FileHelpers.OpenFolderInExplorer(outDir))));
            return Task.CompletedTask;
        });
    }

    async Task<StorageFolder> SelectOutFolder()
    {
        StorageFolder outFolder = null;
        try
        {
            outFolder = await AppServices.Get<IFileDialogService>().OpenFolder(GUID_EXPORT_DIALOG);
        }
        catch (NotFoundException ex)
        {
            _notification.Notify(new(ToastVariant.Error,
                                     string.Format(I18n.Current.GetString("TargetPathNotFound"), ex.FaultPath)));
        }
        return outFolder;
    }

    async Task DoExportAsync(Func<Task> work)
    {
        if (ViewModel.IsExporting)
        {
            return;
        }

        _loading.Show(I18n.Current.GetString("TextExporting/Text"));
        try
        {
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
            _loading.Hide();
            ViewModel.IsExporting = false;
        }
    }

    void btnAddGroup_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.AddGroup();
        listViewGroups.SelectedItem = ViewModel.Groups.Last();
    }

    async void btnDeleteGroup_Click(object sender, RoutedEventArgs e)
    {
        if (!HasSelectedGroup)
        {
            return;
        }

        ContentDialogResult result = await AppServices.Get<IConfirmDialogService>().ShowDanger(
            this,
            I18n.Current.GetString("DialogDeleteBannerGroup/Title"),
            string.Format(I18n.Current.GetString("DialogDeleteBannerGroup/Content"), SelectedGroup.GroupID));
        if (result != ContentDialogResult.Primary)
        {
            return;
        }
        var selectedIndex = listViewGroups.SelectedIndex;
        ViewModel.DeleteGroup(SelectedGroup);
        if (listViewGroups.Items.Count > 0)
        {
            listViewGroups.SelectedIndex = Math.Min(selectedIndex, listViewGroups.Items.Count - 1);
        }
        else
        {
            listViewGroups.SelectedItem = null;
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
        _loading.Show(I18n.Current.GetString("PleaseWait"));
        await _project.Load(openedFile);
        listViewGroups.SelectedItem = ViewModel.Groups.FirstOrDefault();
        // wait for the UI to update
        await Task.Delay(200);
        _loading.Hide();
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
        _loading.Show(I18n.Current.GetString("PleaseWait"));
        await _project.Save(filePath);
        // wait for the UI to update
        await Task.Delay(200);
        _loading.Hide();
    }
    void listViewGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Bindings.Update();
    }
}
