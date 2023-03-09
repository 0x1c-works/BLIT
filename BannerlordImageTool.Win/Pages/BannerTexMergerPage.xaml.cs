// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

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
        ViewModel.OutputResolution = item.Tag as string;
    }

    void btnExport_Click(object sender, RoutedEventArgs e)
    {

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
        var files = await FileHelper.PickMultipleFiles();

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
    public string OutputResolution
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
            _icons[i].AtlasIndex = i / 16;
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
        get => $"{_viewModel.GroupName}_{AtlasIndex}";
    }

    public bool IsSelected { get; set; }

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
