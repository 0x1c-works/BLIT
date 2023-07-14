using Autofac;
using BLIT.Banner;
using BLIT.Win.Helpers;
using BLIT.Win.Services;
using MessagePack;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace BLIT.Win.Pages.BannerIcons.Models;
public class BannerIconsProject : BindableBase, IProject
{
    /// <summary>
    /// 0 - 6 is occpuied by the native game
    /// </summary>
    public const int MIN_GROUP_ID = 7;
    /// <summary>
    /// 0-193 is occupied by the native game
    /// </summary>
    public const int MIN_COLOR_ID = 194;

    public BannerIconsProject(
        ISettingsService settings,
        BannerGroupEntry.Factory bannerGroupFactory,
        BannerColorEntry.Factory colorFactory)
    {
        _settings = settings;
        _groupFactory = bannerGroupFactory;
        _colorFactory = colorFactory;
    }

    readonly ISettingsService _settings;
    readonly BannerGroupEntry.Factory _groupFactory;
    readonly BannerColorEntry.Factory _colorFactory;

    public ObservableCollection<BannerGroupEntry> Groups { get; } = new();
    public ObservableCollection<BannerColorEntry> Colors { get; } = new();
    public StorageFile CurrentFile { get; set; }

    public string OutputResolutionName
    {
        get => _settings.Banner.TextureOutputResolution switch {
            OutputResolution.Res2K => "2K",
            OutputResolution.Res4K => "4K",
            _ => I18n.Current.GetString("PleaseSelect"),
        };
        set
        {
            _settings.Banner.TextureOutputResolution = Enum.TryParse(value, out OutputResolution enumValue) ? enumValue : OutputResolution.INVALID;
            OnPropertyChanged();
        }
    }

    bool _isExporting = false;
    public bool IsExporting
    {
        get => _isExporting;
        set
        {
            SetProperty(ref _isExporting, value);
            OnPropertyChanged(nameof(CanExport));
        }
    }

    bool _isSavingOrLoading = false;
    public bool IsSavingOrLoading
    {
        get => _isSavingOrLoading;
        set
        {
            SetProperty(ref _isSavingOrLoading, value);
            OnPropertyChanged(nameof(CanExport));
        }
    }
    public bool CanExport => !_isExporting && !IsSavingOrLoading && (Groups.Any(g => g.CanExport) || Colors.Count > 0);

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

    public IOrderedEnumerable<BannerGroupEntry> GetExportingGroups()
    {
        return Groups.Where(g => g?.CanExport ?? false).OrderBy(g => g.GroupID);
    }
    public IOrderedEnumerable<BannerColorEntry> GetExportingColors()
    {
        return Colors.Where(c => c?.CanExport ?? false).OrderBy(c => c.ID);
    }

    public void AddGroup()
    {
        BannerGroupEntry newGroup = _groupFactory(GetNextGroupID());
        newGroup.PropertyChanged += OnGroupPropertyChanged;
        Groups.Add(newGroup);
        OnPropertyChanged(nameof(CanExport));
    }

    public void DeleteGroup(BannerGroupEntry group)
    {
        if (group is null)
        {
            return;
        }

        var index = Groups.IndexOf(group);
        if (index < 0)
        {
            return;
        }

        group.PropertyChanged -= OnGroupPropertyChanged;
        Groups.Remove(group);
        OnPropertyChanged(nameof(CanExport));
    }

    public void AddColor()
    {
        Colors.Add(_colorFactory(GetNextColorID()));
    }
    public void DeleteColors(IEnumerable<BannerColorEntry> colors)
    {
        BannerColorEntry[] deleting = colors.ToArray();
        foreach (BannerColorEntry color in deleting)
        {
            Colors.Remove(color);
        }
    }
    public int GetNextGroupID()
    {
        return Groups.Count > 0 ? Groups.Max(g => g.GroupID) + 1 : _settings.Banner.CustomGroupStartID;
    }

    public int GetNextColorID()
    {
        return Colors.Count > 0 ? Colors.Max(c => c.ID) + 1 : _settings.Banner.CustomColorStartID;
    }

    public int ValidateGroupID(int oldID, int newID)
    {
        return ValidateID(oldID, newID, MIN_GROUP_ID, (id) => Groups.Any(g => g.GroupID == id), GetNextGroupID);
    }
    public int ValidateColorID(int oldID, int newID)
    {
        return ValidateID(oldID, newID, MIN_COLOR_ID, (id) => Colors.Any(g => g.ID == id), GetNextColorID);
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

    void OnGroupPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(CanExport));
        if (e.PropertyName == nameof(BannerGroupEntry.GroupID))
        {

        }
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
            Groups.Clear();
            Colors.Clear();
            foreach (BannerGroupEntry.SaveData groupData in data.Groups)
            {
                Groups.Add(groupData.Load(_groupFactory));
            }
            foreach (BannerColorEntry.SaveData colorData in data.Colors)
            {
                Colors.Add(colorData.Load(_colorFactory));
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
        OnPropertyChanged(nameof(CanExport));
    }

    public async Task<string> ExportAll(StorageFolder outFolder)
    {
        var merger = new TextureMerger(_settings.Banner.TextureOutputResolution);
        await Task.WhenAll(GetExportingGroups().Select(g =>
            Task.Factory.StartNew(() => {
                merger.Merge(outFolder.Path, g.GroupID, g.Icons.Select(icon => icon.TexturePath).ToArray());
            })
        ));
        await SpriteOrganizer.CollectToSpriteParts(outFolder.Path, ToIconSprites());
        return ExportXML(outFolder);

    }
    public string ExportXML(StorageFolder outFolder)
    {
        if (outFolder is not null)
        {
            ToBannerIconData().SaveToXml(outFolder.Path);
            SpriteOrganizer.GenerateConfigXML(outFolder.Path, ToIconSprites());
            return outFolder.Path;
        }
        return null;
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
            Groups = vm.Groups.Select(g => new BannerGroupEntry.SaveData(g)).ToArray();
            Colors = vm.Colors.Select(g => new BannerColorEntry.SaveData(g)).ToArray();
        }
        public SaveData() { }
    }
}
