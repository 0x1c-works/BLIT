using BLIT.Banner;
using BLIT.Win.Helpers;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.Storage;

namespace BLIT.Win.Pages.BannerIcons.Models;

public class BannerGroupEntry : BindableBase {
    public delegate BannerGroupEntry Factory(int groupID);

    private BannerIconsProject _project;
    public ObservableCollection<BannerIconEntry> Icons { get; } = new();

    private int _groupID = 7;
    private readonly Lazy<BannerIconEntry.Factory> _iconFactory;

    public int GroupID {
        get => _groupID;
        set {
            value = _project.ValidateGroupID(_groupID, value);
            SetProperty(ref _groupID, value);
            OnPropertyChanged(nameof(GroupName));
        }
    }
    public string GroupName => BannerUtils.GetGroupName(GroupID);

    public bool CanExport => Icons.Count > 0;

    public BannerGroupEntry(BannerIconsProject project, int groupID, Lazy<BannerIconEntry.Factory> iconFactory) {
        _project = project;
        GroupID = groupID;
        _iconFactory = iconFactory;
        Icons.CollectionChanged += _icons_CollectionChanged;
    }

    private void _icons_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        if (e.Action != NotifyCollectionChangedAction.Reset) {
            RefreshCellIndex();
        }
        OnPropertyChanged(nameof(Icons));
        OnPropertyChanged(nameof(CanExport));
    }

    public void AddIcons(IEnumerable<StorageFile> files) {
        bool IsIconAdded(BannerIconEntry icon, StorageFile file) => icon.TexturePath.Equals(file.Path, StringComparison.InvariantCultureIgnoreCase);

        IEnumerable<BannerIconEntry> newIcons = files
            .Where(file => !Icons.Any(icon => IsIconAdded(icon, file)))
            .Select(file => _iconFactory.Value(this, file.Path));
        IEnumerable<BannerIconEntry> existingIcons = Icons.Where(icon => files.Any(file => IsIconAdded(icon, file)));
        foreach (BannerIconEntry icon in newIcons) {
            Icons.Add(icon);
            icon.AutoScanSprite();
        }
        foreach (BannerIconEntry icon in existingIcons) {
            icon.ReloadSprite();
            icon.ReloadTexture();
        }
    }
    public void DeleteIcons(IEnumerable<BannerIconEntry> icons) {
        var queue = new Queue<BannerIconEntry>(icons);
        while (queue.Count > 0) {
            BannerIconEntry deleting = queue.Dequeue();
            if (!Icons.Remove(deleting)) {
                // a more expensive way to ensure the icon is deleted
                var index = Icons.Select(i => i.TexturePath).ToList().IndexOf(deleting.TexturePath);
                if (index > -1) {
                    Icons.RemoveAt(index);
                }
            }
        }
    }
    public void RefreshCellIndex() {
        for (var i = 0; i < Icons.Count; i++) {
            Icons[i].CellIndex = i;
        }
    }

    public BannerIconGroup ToBannerIconGroup() {
        var group = new BannerIconGroup() {
            ID = GroupID,
            Name = GroupName,
            IsPattern = false,
        };
        foreach (BannerIconEntry icon in Icons) {
            group.Icons.Add(icon.ToBannerIcon());
        }
        return group;
    }

    [MessagePackObject]
    public class SaveData {
        [Key(0)]
        public int GroupID;
        [Key(1)]
        public BannerIconEntry.SaveData[] Icons = new BannerIconEntry.SaveData[] { };

        public SaveData(BannerGroupEntry vm) {
            GroupID = vm.GroupID;
            Icons = vm.Icons.Select(icon => new BannerIconEntry.SaveData(icon)).ToArray();
        }
        public SaveData() { }

        public BannerGroupEntry Load(Factory factory) {
            BannerGroupEntry vm = factory(GroupID);
            foreach (BannerIconEntry.SaveData icon in Icons) {
                vm.Icons.Add(icon.Load(vm, vm._iconFactory.Value));
            }
            return vm;
        }
    }
}
