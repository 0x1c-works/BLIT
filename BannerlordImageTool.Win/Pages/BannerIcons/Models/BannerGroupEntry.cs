using BannerlordImageTool.Banner;
using BannerlordImageTool.Win.Helpers;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.Storage;

namespace BannerlordImageTool.Win.Pages.BannerIcons.Models;

public class BannerGroupEntry : BindableBase
{
    public delegate BannerGroupEntry Factory(int groupID);
    public ObservableCollection<BannerIconEntry> Icons { get; } = new();
    int _groupID = 7;
    readonly Lazy<BannerIconEntry.Factory> _iconFactory;

    public int GroupID
    {
        get => _groupID;
        set
        {
            SetProperty(ref _groupID, value);
            OnPropertyChanged(nameof(GroupName));
        }
    }
    public string GroupName => BannerUtils.GetGroupName(GroupID);

    public bool CanExport => Icons.Count > 0;

    public IEnumerable<BannerIconEntry> AllSelection => Icons.Where(icon => icon.IsSelected);
    public BannerIconEntry SingleSelection => Icons.Where(icon => icon.IsSelected).FirstOrDefault();
    public bool HasSelection => Icons.Any(icon => icon.IsSelected);

    public BannerGroupEntry(int groupID, Lazy<BannerIconEntry.Factory> iconFactory)
    {
        GroupID = groupID;
        _iconFactory = iconFactory;
        Icons.CollectionChanged += _icons_CollectionChanged;
    }

    void _icons_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
        IEnumerable<BannerIconEntry> icons = files
            .Where(file =>
                !Icons.Any(icon =>
                    icon.TexturePath.Equals(file.Path, StringComparison.InvariantCultureIgnoreCase)
                )
            )
            .Select(file => _iconFactory.Value(this, file.Path));
        foreach (BannerIconEntry icon in icons)
        {
            Icons.Add(icon);
            icon.AutoScanSprite();
        }
    }
    public void DeleteIcons(IEnumerable<BannerIconEntry> icons)
    {
        var queue = new Queue<BannerIconEntry>(icons);
        while (queue.Count > 0)
        {
            BannerIconEntry deleting = queue.Dequeue();
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
        for (var i = 0; i < Icons.Count; i++)
        {
            Icons[i].CellIndex = i;
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
        foreach (BannerIconEntry icon in Icons)
        {
            group.Icons.Add(icon.ToBannerIcon());
        }
        return group;
    }

    [MessagePackObject]
    public class SaveData
    {
        [Key(0)]
        public int GroupID;
        [Key(1)]
        public BannerIconEntry.SaveData[] Icons = new BannerIconEntry.SaveData[] { };

        public SaveData(BannerGroupEntry vm)
        {
            GroupID = vm.GroupID;
            Icons = vm.Icons.Select(icon => new BannerIconEntry.SaveData(icon)).ToArray();
        }
        public SaveData() { }

        public BannerGroupEntry Load(Factory factory)
        {
            BannerGroupEntry vm = factory(GroupID);
            foreach (BannerIconEntry.SaveData icon in Icons)
            {
                vm.Icons.Add(icon.Load(vm, vm._iconFactory.Value));
            }
            return vm;
        }
    }
}
