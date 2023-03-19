using BannerlordImageTool.Banner;
using BannerlordImageTool.Win.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.Storage;

namespace BannerlordImageTool.Win.ViewModels.BannerIcons;

public class GroupViewModel : BindableBase
{
    private ObservableCollection<IconViewModel> _icons = new();
    private int _groupID = 7;

    public ObservableCollection<IconViewModel> Icons { get => _icons; }

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

    public bool CanExport
    {
        get => Icons.Count > 0;
    }

    public IEnumerable<IconViewModel> AllSelection
    {
        get => Icons.Where(icon => icon.IsSelected);
    }
    public IconViewModel SingleSelection
    {
        get => Icons.Where(icon => icon.IsSelected).FirstOrDefault();
    }
    public bool HasSelection
    {
        get => Icons.Any(icon => icon.IsSelected);
    }

    internal GroupViewModel()
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
        var icons = files
            .Where(file =>
                !_icons.Any(icon =>
                    icon.TexturePath.Equals(file.Path, StringComparison.InvariantCultureIgnoreCase)
                )
            )
            .Select(file => new IconViewModel(this, file.Path));
        foreach (var icon in icons)
        {
            _icons.Add(icon);
            icon.AutoScanSprite();
        }
    }
    public void DeleteIcons(IEnumerable<IconViewModel> icons)
    {
        var queue = new Queue<IconViewModel>(icons);
        while (queue.Count > 0)
        {
            var deleting = queue.Dequeue();
            if (!Icons.Remove(deleting))
            {
                // a more expensive way to ensure the icon is deleted
                var index = Icons.Select(i => i.TexturePath).ToList().IndexOf(deleting.TexturePath);
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
        OnPropertyChanged(nameof(AllSelection));
        OnPropertyChanged(nameof(SingleSelection));
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
