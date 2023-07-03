using BannerlordImageTool.Banner;
using BannerlordImageTool.Win.Helpers;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.Storage;

namespace BannerlordImageTool.Win.Pages.BannerIcons.ViewModels;

public class BannerGroupViewModel : BindableBase
{
    private ObservableCollection<BannerIconViewModel> _icons = new();
    public ObservableCollection<BannerIconViewModel> Icons { get => _icons; }

    private int _groupID = 7;
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

    public IEnumerable<BannerIconViewModel> AllSelection
    {
        get => Icons.Where(icon => icon.IsSelected);
    }
    public BannerIconViewModel SingleSelection
    {
        get => Icons.Where(icon => icon.IsSelected).FirstOrDefault();
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
        var icons = files
            .Where(file =>
                !_icons.Any(icon =>
                    icon.TexturePath.Equals(file.Path, StringComparison.InvariantCultureIgnoreCase)
                )
            )
            .Select(file => new BannerIconViewModel(this, file.Path));
        foreach (var icon in icons)
        {
            _icons.Add(icon);
            icon.AutoScanSprite();
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

    [MessagePackObject]
    public class SaveData
    {
        [Key(0)]
        public int GroupID;
        [Key(1)]
        public BannerIconViewModel.SaveData[] Icons = new BannerIconViewModel.SaveData[] { };

        public SaveData(BannerGroupViewModel vm)
        {
            GroupID = vm.GroupID;
            Icons = vm.Icons.Select(icon => new BannerIconViewModel.SaveData(icon)).ToArray();
        }
        public SaveData() { }

        public BannerGroupViewModel Load()
        {
            var vm = new BannerGroupViewModel() { GroupID = GroupID };
            foreach (var icon in Icons)
            {
                vm.Icons.Add(icon.Load(vm));
            }
            return vm;
        }
    }
}
