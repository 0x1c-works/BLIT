using Autofac;
using BLIT.Banner;
using BLIT.Helpers;
using BLIT.Services;
using DynamicData;
using DynamicData.Binding;
using MessagePack;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace BLIT.ViewModels.Banner.Data;
public class BannerIconsProject : ReactiveObject, IProject, IDisposable
{
    public BannerIconsProject(
        BannerSettings settings,
        BannerGroupEntry.Factory bannerGroupFactory,
        BannerColorEntry.Factory colorFactory)
    {
        _settings = settings;
        _groupFactory = bannerGroupFactory;
        _colorFactory = colorFactory;

        IObservable<IChangeSet<BannerGroupEntry, int>> groupChanges = _sourceGroups.ToObservableChangeSet(x => x.GroupID)
                                                                                   .ObserveOn(RxApp.MainThreadScheduler)
                                                                                   .Publish();
        IObservable<IChangeSet<BannerColorEntry, int>> colorChanges = _sourceIcons.ToObservableChangeSet(x => x.ID)
                                                                                   .Throttle(TimeSpan.FromMilliseconds(50))
                                                                                   .ObserveOn(RxApp.MainThreadScheduler)
                                                                                   .Publish();
        groupChanges.Bind(out _groups).Subscribe().DisposeWith(_disposables);
        colorChanges.Bind(out _colors).Subscribe().DisposeWith(_disposables);

        Observable.CombineLatest(this.WhenAnyValue(x => x.IsExporting),
                                 this.WhenAnyValue(x => x.IsSavingOrLoading),
                                 groupChanges.AutoRefresh(x => x.CanExport).ToCollection().Select(groups => groups.Any(g => g.CanExport)),
                                 colorChanges.AutoRefresh(x => x.CanExport).ToCollection().Select(colors => colors.Any(c => c.CanExport)),
                                 (isExporting, isSavingOrLoading, hasExportableGruops, hasExportableColors) =>
                                    !isExporting && !isSavingOrLoading && (hasExportableColors || hasExportableColors))
            .ToPropertyEx(this, x => x.CanExport)
            .DisposeWith(_disposables);
    }

    readonly CompositeDisposable _disposables = new();
    readonly BannerSettings _settings;
    readonly BannerGroupEntry.Factory _groupFactory;
    readonly BannerColorEntry.Factory _colorFactory;

    ObservableCollection<BannerGroupEntry> _sourceGroups { get; } = new();
    ObservableCollection<BannerColorEntry> _sourceIcons { get; } = new();
    ReadOnlyObservableCollection<BannerGroupEntry> _groups;
    ReadOnlyObservableCollection<BannerColorEntry> _colors;
    public ReadOnlyObservableCollection<BannerGroupEntry> Groups => _groups;
    public ReadOnlyObservableCollection<BannerColorEntry> Colors => _colors;

    public string OutputResolutionName
    {
        get => _settings.TextureOutputResolution switch {
            OutputResolution.Res2K => "2K",
            OutputResolution.Res4K => "4K",
            _ => I18n.GetString("PleaseSelect"),
        };
        set
        {
            _settings.TextureOutputResolution = Enum.TryParse(value, out OutputResolution enumValue) ? enumValue : OutputResolution.INVALID;
            this.RaisePropertyChanged(nameof(OutputResolutionName));
        }
    }

    [Reactive] public bool IsExporting { get; private set; }
    [Reactive] public bool IsSavingOrLoading { get; private set; }
    [ObservableAsProperty] public bool CanExport { get; }

    public BannerIconData ToBannerIconData()
    {
        var data = new BannerIconData();
        foreach (BannerGroupEntry group in GetExportingGroups())
        {
            data.IconGroups.Add(group.ToBannerIconGroup());
        }
        foreach (BannerColorEntry color in GetExportingColors())
        {
            data.BannerColors.Add(color.ToBannerColor());
        }
        return data;
    }
    public IEnumerable<IconSprite> ToIconSprites()
    {
        return GetExportingGroups().Aggregate(new List<IconSprite>(), (icons, g) => {
            icons.AddRange(g.Icons.Where(icon => !string.IsNullOrWhiteSpace(icon.SpritePath))
                                  .Select(icon => icon.ToIconSprite()));
            return icons;
        });
    }

    public IEnumerable<BannerGroupEntry> GetExportingGroups()
    {
        return _sourceGroups.Where(g => g?.CanExport ?? false).OrderBy(g => g.GroupID);
    }
    public IEnumerable<BannerColorEntry> GetExportingColors()
    {
        return _sourceIcons.Where(c => c?.CanExport ?? false);
    }

    public void AddGroup()
    {
        BannerGroupEntry newGroup = _groupFactory(GetNextGroupID());
        _sourceGroups.Add(newGroup);
    }

    public void DeleteGroup(BannerGroupEntry group)
    {
        if (group is null)
        {
            return;
        }

        var index = _sourceGroups.IndexOf(group);
        if (index < 0)
        {
            return;
        }

        _sourceGroups.Remove(group);
    }

    public void AddColor()
    {
        _sourceIcons.Add(_colorFactory(GetNextColorID()));
    }
    public void DeleteColors(IEnumerable<BannerColorEntry> colors)
    {
        BannerColorEntry[] deleting = colors.ToArray();
        foreach (BannerColorEntry color in deleting)
        {
            _sourceIcons.Remove(color);
        }
    }
    public void SortColors()
    {
        _sourceIcons.SortStable((x, y) => BLITColorHelper.Sort(x.Color, y.Color));
    }
    public int GetNextGroupID()
    {
        return _sourceGroups.Count > 0 ? _sourceGroups.Max(g => g.GroupID) + 1 : _settings.CustomGroupStartID;
    }

    public int GetNextColorID()
    {
        return _sourceIcons.Count > 0 ? _sourceIcons.Max(c => c.ID) + 1 : _settings.CustomColorStartID;
    }

    public int ValidateGroupID(int oldID, int newID)
    {
        return ValidateID(oldID, newID, BannerSettings.MIN_CUSTOM_GROUP_ID, (id) => _sourceGroups.Any(g => g.GroupID == id), GetNextGroupID);
    }
    public int ValidateColorID(int oldID, int newID)
    {
        return ValidateID(oldID, newID, BannerSettings.MIN_CUSTOM_COLOR_ID, (id) => _sourceIcons.Any(g => g.ID == id), GetNextColorID);
    }
    int ValidateID(int oldID, int newID, int minValidID, Func<int, bool> isIDOccupied, Func<int> getNextID)
    {
        if (oldID == newID) return newID;

        var direction = newID - oldID > 0;
        while (isIDOccupied(newID))
        {
            newID += direction ? 1 : -1;
        }
        if (newID < minValidID) { newID = getNextID(); }
        return newID;
    }

    public async Task Write(Stream s)
    {
        try
        {
            IsSavingOrLoading = true;
            await MessagePackSerializer.SerializeAsync(s, new SaveData(this));
        }
        catch (Exception ex) { Log.Error(ex, "error in saving the banner project"); }
        finally
        {
            IsSavingOrLoading = false;
        }
    }

    public async Task Read(Stream s)
    {
        try
        {
            IsSavingOrLoading = true;
            SaveData data = await MessagePackSerializer.DeserializeAsync<SaveData>(s);
            _sourceGroups.Clear();
            _sourceIcons.Clear();
            foreach (BannerGroupEntry.SaveData groupData in data.Groups)
            {
                _sourceGroups.Add(groupData.Load(_groupFactory));
            }
            foreach (BannerColorEntry.SaveData colorData in data.Colors)
            {
                _sourceIcons.Add(colorData.Load(_colorFactory));
            }
        }
        catch (Exception ex) { Log.Error(ex, "error in loading the banner project"); }
        finally
        {
            IsSavingOrLoading = false;
        }
    }

    public void AfterLoaded()
    {
    }

    public async Task<string?> ExportAll(StorageFolder outFolder)
    {
        var merger = new TextureMerger(_settings.TextureOutputResolution);
        await Task.WhenAll(GetExportingGroups().Select(g =>
            Task.Factory.StartNew(() => {
                merger.Merge(outFolder.Path, g.GroupID, g.Icons.Select(icon => icon.TexturePath).ToArray());
            })
        ));
        await SpriteOrganizer.CollectToSpriteParts(outFolder.Path, ToIconSprites());
        return ExportXML(outFolder);

    }
    public string? ExportXML(StorageFolder outFolder)
    {
        if (outFolder is not null)
        {
            ToBannerIconData().SaveToXml(outFolder.Path);
            SpriteOrganizer.GenerateConfigXML(outFolder.Path, ToIconSprites());
            return outFolder.Path;
        }
        return null;
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }

    [MessagePackObject]
    public class SaveData
    {
        [Key(0)]
        public BannerGroupEntry.SaveData[] Groups = new BannerGroupEntry.SaveData[] { };
        [Key(1)]
        public BannerColorEntry.SaveData[] Colors = new BannerColorEntry.SaveData[] { };

        public SaveData(BannerIconsProject vm)
        {
            Groups = vm._sourceGroups.Select(g => new BannerGroupEntry.SaveData(g)).ToArray();
            Colors = vm._sourceIcons.Select(g => new BannerColorEntry.SaveData(g)).ToArray();
        }
        public SaveData() { }
    }
}
