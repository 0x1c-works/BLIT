// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BannerlordImageTool.Win.Common;
using BannerlordImageTool.Win.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
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
        ViewModel = new BannerTexMergerViewModel(textureCells);
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

    async void btnOpenImages_Click(object sender, RoutedEventArgs e)
    {
        var files = await FileHelper.PickMultipleFiles();

        if (files.Count == 0) return;
        ViewModel.AddCellTextures(files);
    }
}

public class IconTexture : BindableBase
{
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
        set => SetProperty(ref _atlasIndex, value);
    }

    public IconTexture(string filePath)
    {
        _filePath = filePath;
    }
}

public class BannerTexMergerViewModel : BindableBase
{

    private int _groupID;
    private CollectionViewSource _collectionViewSource;

    public int GroupID
    {
        get => _groupID;
        set => SetProperty(ref _groupID, value);
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

    private List<IconTexture> _icons = new();

    internal BannerTexMergerViewModel(CollectionViewSource viewSource)
    {
        _collectionViewSource = viewSource;
    }

    private void cvs_VectorChanged(Windows.Foundation.Collections.IObservableVector<object> sender, Windows.Foundation.Collections.IVectorChangedEventArgs e)
    {
        var s = $"CHANGE:{e.CollectionChange}, INDEX:{e.Index}";
        Console.WriteLine(s);
        RefreshCellIndex();
    }

    public void AddCellTextures(IEnumerable<StorageFile> files)
    {
        var newCells = files.Where(file => !_icons.Any(icon => icon.FilePath.Equals(file.Path, StringComparison.InvariantCultureIgnoreCase)))
            .Select(file => new IconTexture(file.Path));
        _icons.AddRange(newCells);
        UpdateCVS();
    }
    public void RefreshCellIndex()
    {
        for (int i = 0; i < _icons.Count; i++)
        {
            _icons[i].AtlasIndex = i % 16;
        }

        //_icons = new(_icons);
        UpdateCVS();
    }

    void UpdateCVS()
    {
        _collectionViewSource.Source = new List<IconTexture>(_icons);
        _collectionViewSource.View.VectorChanged += cvs_VectorChanged;
    }
}
