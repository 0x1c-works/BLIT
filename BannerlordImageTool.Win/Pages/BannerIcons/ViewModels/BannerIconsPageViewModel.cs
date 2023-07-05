using Autofac;
using BannerlordImageTool.Banner;
using BannerlordImageTool.Win.Helpers;
using BannerlordImageTool.Win.Services;
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

namespace BannerlordImageTool.Win.Pages.BannerIcons.ViewModels;
public class BannerIconsPageViewModel : BindableBase, IProject
{
    public BannerIconsPageViewModel(
        ISettingsService settings,
        BannerGroupViewModel.Factory bannerGroupFactory,
        BannerColorViewModel.Factory colorFactory)
    {
        _settings = settings;
        _groupFactory = bannerGroupFactory;
        _colorFactory = colorFactory;
    }

    readonly ISettingsService _settings;
    readonly BannerGroupViewModel.Factory _groupFactory;
    readonly BannerColorViewModel.Factory _colorFactory;

    public ObservableCollection<BannerGroupViewModel> Groups { get; } = new();
    public ObservableCollection<BannerColorViewModel> Colors { get; } = new();
    public StorageFile CurrentFile { get; set; }

    BannerGroupViewModel _selectedGroup;
    public BannerGroupViewModel SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            SetProperty(ref _selectedGroup, value);
            OnPropertyChanged(nameof(HasSelectedGroup));
            OnPropertyChanged(nameof(ShowEmptyHint));
        }
    }
    public bool HasSelectedGroup => SelectedGroup is not null;
    public bool ShowEmptyHint => !HasSelectedGroup;

    public string OutputResolutionName
    {
        get => _settings.Banner.TextureOutputResolution switch {
            OutputResolution.Res2K => "2K",
            OutputResolution.Res4K => "4K",
            _ => I18n.Current.GetString("PleaseSelect"),
        };
        set
        {
            _settings.Banner.TextureOutputResolution = Enum.TryParse<OutputResolution>(value, out OutputResolution enumValue) ? enumValue : OutputResolution.INVALID;
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
        foreach (BannerGroupViewModel group in GetExportingGroups())
        {
            data.IconGroups.Add(group.ToBannerIconGroup());
        }
        foreach (BannerColorViewModel color in GetExportingColors())
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

    public IOrderedEnumerable<BannerGroupViewModel> GetExportingGroups()
    {
        return Groups.Where(g => g?.CanExport ?? false).OrderBy(g => g.GroupID);
    }
    public IOrderedEnumerable<BannerColorViewModel> GetExportingColors()
    {
        return Colors.Where(c => c?.CanExport ?? false).OrderBy(c => c.ID);
    }

    public void AddGroup()
    {
        BannerGroupViewModel newGroup = _groupFactory(GetNextGroupID());
        newGroup.PropertyChanged += OnGroupPropertyChanged;
        Groups.Add(newGroup);
        SelectedGroup ??= Groups.Last();
        OnPropertyChanged(nameof(CanExport));
    }

    public void DeleteGroup(BannerGroupViewModel group)
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
        if (group == SelectedGroup)
        {
            SelectedGroup = Groups.Count > 0 ? Groups[Math.Max(0, index - 1)] : null;
        }
        OnPropertyChanged(nameof(CanExport));
    }

    public void AddColor()
    {
        Colors.Add(_colorFactory(GetNextColorID()));
    }
    public void DeleteColors(IEnumerable<BannerColorViewModel> colors)
    {
        BannerColorViewModel[] deleting = colors.ToArray();
        foreach (BannerColorViewModel color in deleting)
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

    void OnGroupPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(CanExport));
        if (e.PropertyName == nameof(BannerGroupViewModel.GroupID))
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
            foreach (BannerGroupViewModel.SaveData groupData in data.Groups)
            {
                Groups.Add(groupData.Load(_groupFactory));
            }
            foreach (BannerColorViewModel.SaveData colorData in data.Colors)
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
        // Update the selection if there is any
        SelectedGroup = HasSelectedGroup ? Groups.FirstOrDefault(g => g.GroupID == SelectedGroup.GroupID) : Groups.FirstOrDefault();
        OnPropertyChanged(nameof(CanExport));
    }

    [MessagePackObject]
    public class SaveData
    {
        [Key(0)]
        public BannerGroupViewModel.SaveData[] Groups = new BannerGroupViewModel.SaveData[] { };
        [Key(1)]
        public BannerColorViewModel.SaveData[] Colors = new BannerColorViewModel.SaveData[] { };

        public SaveData(BannerIconsPageViewModel vm)
        {
            Groups = vm.Groups.Select(g => new BannerGroupViewModel.SaveData(g)).ToArray();
            Colors = vm.Colors.Select(g => new BannerColorViewModel.SaveData(g)).ToArray();
        }
        public SaveData() { }
    }
}
