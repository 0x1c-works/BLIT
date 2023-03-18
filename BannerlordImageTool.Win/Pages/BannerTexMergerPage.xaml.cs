// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BannerlordImageTool.BannerTex;
using BannerlordImageTool.Win.Common;
using BannerlordImageTool.Win.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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

public class BannerTexMergerViewModel : BindableBase
{
    private ObservableCollection<BannerIconViewModel> _icons = new();
    private int _groupID = 7;
    private bool _isExporting = false;

    public ObservableCollection<BannerIconViewModel> Icons { get => _icons; }

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
        get => BannerUtils.GetGroupName(GroupID);
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
                case OutputResolution.Res2K: return "2K";
                case OutputResolution.Res4K: return "4K";
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

    public IEnumerable<BannerIconViewModel> Selection
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
            .Select(file => new BannerIconViewModel(this, file.Path));
        foreach (var cell in newCells)
        {
            _icons.Add(cell);
        }
    }
    public void DeleteIcons(IEnumerable<BannerIconViewModel> icons)
    {
        var queue = new Queue<BannerIconViewModel>(icons);
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
            _icons[i].CellIndex = i;
        }
    }
    public void NotifySelectionChange()
    {
        OnPropertyChanged(nameof(Selection));
        OnPropertyChanged(nameof(HasSelection));
    }

    public BannerIconGroup ToBannerIconGroup()
    {
        var group = new BannerIconGroup() {
            ID = GroupID,
            Name = GroupName,
            IsPattern = false,
        };
        foreach (var icon in Icons)
        {
            group.Icons.Add(icon.ToBannerIcon());
        }
        return group;
    }
}

public class BannerIconViewModel : BindableBase
{
    private BannerTexMergerViewModel _viewModel;
    private string _texturePath;
    private int _cellIndex;

    public string FilePath
    {
        get => _texturePath;
        set => SetProperty(ref _texturePath, value);
    }
    public int CellIndex
    {
        get => _cellIndex;
        set
        {
            if (value == _cellIndex) return;
            SetProperty(ref _cellIndex, value);
            OnPropertyChanged(nameof(ID));
            OnPropertyChanged(nameof(AtlasName));
        }
    }
    public int AtlasIndex
    {
        get => CellIndex / (TextureMerger.ROWS * TextureMerger.COLS);
    }

    public string AtlasName
    {
        get => BannerUtils.GetAtlasName(_viewModel.GroupID, AtlasIndex);
    }
    public int ID
    {
        get => BannerUtils.GetIconID(_viewModel.GroupID, CellIndex);
    }

    public bool IsSelected { get; set; }
    public bool IsValid { get => !string.IsNullOrEmpty(FilePath) && AtlasIndex >= 0; }

    public BannerIconViewModel(BannerTexMergerViewModel viewModel, string filePath)
    {
        _viewModel = viewModel;
        _texturePath = filePath;

        _viewModel.PropertyChanged += _viewModel_PropertyChanged;
    }

    private void _viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BannerTexMergerViewModel.GroupName))
        {
            OnPropertyChanged(nameof(AtlasName));
            OnPropertyChanged(nameof(ID));
        }
    }

    public BannerIcon ToBannerIcon()
    {
        return new BannerIcon() { ID = ID, MaterialName = AtlasName, TextureIndex = CellIndex };
    }
}
