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
    DataViewModel ViewModel { get; init; } = new();

    public BannerIconsEditor()
    {
        this.InitializeComponent();
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

        TextureMerger merger = new TextureMerger(GlobalSettings.Current.BannerTexOutputResolution);

        ViewModel.IsExporting = true;
        infoExport.IsOpen = false;
        await Task.WhenAll(ViewModel.GetExportingGroups().Select(g =>
            Task.Factory.StartNew(() => {
                merger.Merge(outFolder.Path, g.GroupID, g.Icons.Select(icon => icon.FilePath).ToArray());
            })
        ));
        ViewModel.ToBannerIconData().SaveToXml(outFolder.Path);
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

    private void btnAddGroup_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.AddGroup();
    }

    private void listGroups_ItemClick(object sender, ItemClickEventArgs e)
    {
        ViewModel.SelectedGroup = e.ClickedItem as GroupViewModel;
    }
}
