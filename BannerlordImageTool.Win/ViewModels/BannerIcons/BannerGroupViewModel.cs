using BannerlordImageTool.BannerTex;
using BannerlordImageTool.Win.Common;
using BannerlordImageTool.Win.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.Storage;

namespace BannerlordImageTool.Win.ViewModels.BannerIcons;

public class BannerGroupViewModel : BindableBase
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

    internal BannerGroupViewModel()
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
