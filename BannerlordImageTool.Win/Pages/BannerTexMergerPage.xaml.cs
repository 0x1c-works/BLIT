// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BannerlordImageTool.BannerTex;
using BannerlordImageTool.Win.Common;
using BannerlordImageTool.Win.Settings;
using ImageMagick;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BannerlordImageTool.Win.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BannerTexMergerPage : Page
{
    public BannerTexMergerViewModel ViewModel { get; private set; }
    public BannerTexMergerPage()
    {
        this.InitializeComponent();
        ViewModel = new BannerTexMergerViewModel();
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

        var outFolder = await FileHelper.PickFolder($"BannerTextureExportDir-{ViewModel.GroupID}",
                                                    "bannerExportTo");
        if (outFolder == null) return;

        TextureMerger merger = new TextureMerger(GlobalSettings.Current.BannerTexOutputResolution);
        var outBasePath = Path.Join(outFolder.Path, ViewModel.GroupName);

        ViewModel.IsExporting = true;
        infoExport.IsOpen = false;
        await Task.Factory.StartNew(() => {
            merger.Merge(outBasePath, ViewModel.Icons.Select(icon => icon.FilePath).ToArray());
        });
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
        foreach (var item in e.AddedItems.Where(item => item is IconTexture).Cast<IconTexture>())
        {
            item.IsSelected = true;
            changed = true;
        }
        foreach (var item in e.RemovedItems.Where(item => item is IconTexture).Cast<IconTexture>())
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

public class BannerTexMergerViewModel : BindableBase
{
    private ObservableCollection<IconTexture> _icons = new();
    private int _groupID = 7;
    private bool _isExporting = false;

    public ObservableCollection<IconTexture> Icons { get => _icons; }

    public int GroupID
    {
        get => _groupID;
        set
        {
            SetProperty(ref _groupID, value);
            OnPropertyChanged(nameof(GroupName));
        }
    }
    public string GroupName
    {
        get => $"banners_{GroupID}";
    }
    public bool IsExporting
    {
        get => _isExporting;
        set
        {
            SetProperty(ref _isExporting, value);
            OnPropertyChanged(nameof(CanExport));
        }
    }
    public bool CanExport
    {
        get => Icons.Count > 0 && !IsExporting;
    }
    public string OutputResolutionName
    {
        get
        {
            switch (GlobalSettings.Current.BannerTexOutputResolution)
            {
                case BannerTex.OutputResolution.Res2K: return "2K";
                case BannerTex.OutputResolution.Res4K: return "4K";
                default: return I18n.Current.GetString("PleaseSelect");
            }
        }
        set
        {
            if (Enum.TryParse<BannerTex.OutputResolution>(value, out var enumValue))
            {
                GlobalSettings.Current.BannerTexOutputResolution = enumValue;
            }
            else
            {
                GlobalSettings.Current.BannerTexOutputResolution = BannerTex.OutputResolution.INVALID;
            }
            OnPropertyChanged();
        }
    }

    public IEnumerable<IconTexture> Selection
    {
        get => Icons.Where(icon => icon.IsSelected);
    }
    public bool HasSelection
    {
        get => Icons.Any(icon => icon.IsSelected);
    }

    internal BannerTexMergerViewModel()
    {
        _icons.CollectionChanged += _icons_CollectionChanged;
    }

    private void _icons_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != NotifyCollectionChangedAction.Reset)
        {
            RefreshCellIndex();
        }
        OnPropertyChanged(nameof(Icons));
        OnPropertyChanged(nameof(CanExport));
    }

    public void AddIcons(IEnumerable<StorageFile> files)
    {
        var newCells = files.Where(file => !_icons.Any(icon => icon.FilePath.Equals(file.Path, StringComparison.InvariantCultureIgnoreCase)))
            .Select(file => new IconTexture(this, file.Path));
        foreach (var cell in newCells)
        {
            _icons.Add(cell);
        }
    }
    public void DeleteIcons(IEnumerable<IconTexture> icons)
    {
        var queue = new Queue<IconTexture>(icons);
        while (queue.Count > 0)
        {
            var deleting = queue.Dequeue();
            if (!Icons.Remove(deleting))
            {
                // a more expensive way to ensure the icon is deleted
                var index = Icons.Select(i => i.FilePath).ToList().IndexOf(deleting.FilePath);
                if (index > -1)
                {
                    Icons.RemoveAt(index);
                }
            }
        }
    }
    public void RefreshCellIndex()
    {
        for (int i = 0; i < _icons.Count; i++)
        {
            _icons[i].AtlasIndex = i / (TextureMerger.ROWS * TextureMerger.COLS);
        }
    }
    public void NotifySelectionChange()
    {
        OnPropertyChanged(nameof(Selection));
        OnPropertyChanged(nameof(HasSelection));
    }
}

public class IconTexture : BindableBase
{
    private BannerTexMergerViewModel _viewModel;
    private string _filePath;
    private int _atlasIndex;

    public string FilePath
    {
        get => _filePath;
        set => SetProperty(ref _filePath, value);
    }
    public int AtlasIndex
    {
        get => _atlasIndex;
        set
        {
            SetProperty(ref _atlasIndex, value);
            OnPropertyChanged(nameof(AtlasName));
        }
    }

    public string AtlasName
    {
        get => $"{_viewModel.GroupName}_{AtlasIndex + 1:d2}";
    }

    public bool IsSelected { get; set; }
    public bool IsValid { get => !string.IsNullOrEmpty(FilePath) && AtlasIndex >= 0; }

    public IconTexture(BannerTexMergerViewModel viewModel, string filePath)
    {
        _viewModel = viewModel;
        _filePath = filePath;

        _viewModel.PropertyChanged += _viewModel_PropertyChanged;
    }

    private void _viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BannerTexMergerViewModel.GroupName))
        {
            OnPropertyChanged(nameof(AtlasName));
        }
    }
}
