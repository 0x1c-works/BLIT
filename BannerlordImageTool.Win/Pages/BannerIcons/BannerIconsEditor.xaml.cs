// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BannerlordImageTool.BannerTex;
using BannerlordImageTool.Win.Common;
using BannerlordImageTool.Win.Settings;
using BannerlordImageTool.Win.ViewModels.BannerIcons;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BannerlordImageTool.Win.Pages.BannerIcons;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BannerIconsEditor : Page
{
    public BannerGroupViewModel ViewModel { get; private set; }
    public BannerIconsEditor()
    {
        this.InitializeComponent();
        ViewModel = new BannerGroupViewModel();
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

        var outFolder = await FileHelper.PickFolder($"BannerIconsExportDir-{ViewModel.GroupID}",
                                                    "bannerIconsExportTo");
        if (outFolder == null) return;

        TextureMerger merger = new TextureMerger(GlobalSettings.Current.BannerTexOutputResolution);

        ViewModel.IsExporting = true;
        infoExport.IsOpen = false;
        await Task.Factory.StartNew(() => {
            merger.Merge(outFolder.Path, ViewModel.GroupID, ViewModel.Icons.Select(icon => icon.FilePath).ToArray());
        });
        var xmlData = new BannerIconData();
        xmlData.IconGroups.Add(ViewModel.ToBannerIconGroup());
        xmlData.SaveToXml(outFolder.Path);
        ViewModel.IsExporting = false;

        infoExport.Message = string.Format(I18n.Current.GetString("exportSuccess"), outFolder.Path);
        infoExport.Severity = InfoBarSeverity.Success;
        infoExport.IsOpen = true;
        var btnGo = new Button() {
            Content = "Open the folder",
        };
        btnGo.Click += (s, e) => Process.Start("explorer.exe", outFolder.Path);

        infoExport.ActionButton = btnGo;
    }

    void btnImport_Click(object sender, RoutedEventArgs e)
    {

    }

    private void btnConfirmDelete_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.DeleteIcons(ViewModel.Selection);
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
        foreach (var item in e.AddedItems.Where(item => item is BannerIconViewModel).Cast<BannerIconViewModel>())
        {
            item.IsSelected = true;
            changed = true;
        }
        foreach (var item in e.RemovedItems.Where(item => item is BannerIconViewModel).Cast<BannerIconViewModel>())
        {
            item.IsSelected = false;
            changed = true;
        }
        if (changed)
        {
            ViewModel.NotifySelectionChange();
        }
    }

}
