using BLIT.Banner;
using BLIT.Services;
using BLIT.ViewModels.Banner.Data;
using DynamicData;
using DynamicData.Binding;
using MessagePack;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Windows.Storage;

namespace BLIT.Win.Pages.BannerIcons.Models;

public class BannerGroupEntry : ReactiveObject, IDisposable
{
    public delegate BannerGroupEntry Factory(int groupID);

    readonly CompositeDisposable _disposables = new();
    readonly Lazy<BannerIconEntry.Factory> _iconFactory;
    BannerIconsProject _project;
    ObservableCollection<BannerIconEntry> _sourceIcons { get; } = new();
    ReadOnlyObservableCollection<BannerIconEntry> _icons;
    public ReadOnlyObservableCollection<BannerIconEntry> Icons => _icons;

    int _groupID = BannerSettings.MIN_CUSTOM_GROUP_ID;
    public int GroupID
    {
        get => _groupID;
        set
        {
            value = _project.ValidateGroupID(_groupID, value);
            this.RaiseAndSetIfChanged(ref _groupID, value);
        }
    }
    [ObservableAsProperty] public string GroupName { get; } = "";

    [ObservableAsProperty] public bool CanExport { get; }

    public BannerGroupEntry(BannerIconsProject project, int groupID, Lazy<BannerIconEntry.Factory> iconFactory)
    {
        _project = project;
        GroupID = groupID;
        _iconFactory = iconFactory;
        IConnectableObservable<DynamicData.IChangeSet<BannerIconEntry, int>> iconsChanges = _sourceIcons.ToObservableChangeSet(x => x.ID)
                                                                                                        .ObserveOn(RxApp.MainThreadScheduler)
                                                                                                        .Publish();
        iconsChanges.Bind(out _icons).Subscribe().DisposeWith(_disposables);

        this.WhenAnyValue(x => x.GroupID)
            .Select(x => BannerUtils.GetGroupName(x))
            .ToPropertyEx(this, x => x.GroupName)
            .DisposeWith(_disposables);
        iconsChanges
            .Do(_ => RefreshCellIndex())
            .Select(x => x.Any())
            .ToPropertyEx(this, x => x.CanExport)
            .DisposeWith(_disposables);
    }

    public void AddIcons(IEnumerable<StorageFile> files)
    {
        bool IsIconAdded(BannerIconEntry icon, StorageFile file) => icon.TexturePath.Equals(file.Path, StringComparison.InvariantCultureIgnoreCase);

        IEnumerable<BannerIconEntry> newIcons = files
            .Where(file => !Icons.Any(icon => IsIconAdded(icon, file)))
            .Select(file => _iconFactory.Value(this, file.Path));
        IEnumerable<BannerIconEntry> existingIcons = Icons.Where(icon => files.Any(file => IsIconAdded(icon, file)));
        foreach (BannerIconEntry icon in newIcons)
        {
            Icons.Add((IEnumerable<BannerIconEntry>)icon);
            icon.AutoScanSprite();
        }
        foreach (BannerIconEntry icon in existingIcons)
        {
            icon.ReloadSprite();
            icon.ReloadTexture();
        }
    }
    public void DeleteIcons(IEnumerable<BannerIconEntry> icons)
    {
        var queue = new Queue<BannerIconEntry>(icons);
        while (queue.Count > 0)
        {
            BannerIconEntry deleting = queue.Dequeue();
            int index;
            while ((index = Icons.Select(i => i.TexturePath).IndexOf(deleting.TexturePath)) > -1)
            {
                _sourceIcons.RemoveAt(index);
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

    public void Dispose()
    {
        _disposables.Dispose();
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
                vm._sourceIcons.Add((IEnumerable<BannerIconEntry>)icon.Load(vm, vm._iconFactory.Value));
            }
            return vm;
        }
    }
}
