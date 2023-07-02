using BannerlordImageTool.Banner;
using BannerlordImageTool.Win.Helpers;
using BannerlordImageTool.Win.Settings;
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

namespace BannerlordImageTool.Win.ViewModels.BannerIcons;
public class DataViewModel : BindableBase
{
    public ObservableCollection<GroupViewModel> Groups { get; } = new();
    public ObservableCollection<ColorViewModel> Colors { get; } = new();
    public StorageFile CurrentFile { get; set; }

    private GroupViewModel _selectedGroup;
    public GroupViewModel SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            SetProperty(ref _selectedGroup, value);
            OnPropertyChanged(nameof(HasSelectedGroup));
            OnPropertyChanged(nameof(ShowEmptyHint));
        }
    }
    public bool HasSelectedGroup
    {
        get => SelectedGroup is not null;
    }
    public bool ShowEmptyHint
    {
        get => !HasSelectedGroup;
    }

    public string OutputResolutionName
    {
        get
        {
            switch (GlobalSettings.Current.Banner.TextureOutputResolution)
            {
                case OutputResolution.Res2K: return "2K";
                case OutputResolution.Res4K: return "4K";
                default: return I18n.Current.GetString("PleaseSelect");
            }
        }
        set
        {
            if (Enum.TryParse<OutputResolution>(value, out var enumValue))
            {
                GlobalSettings.Current.Banner.TextureOutputResolution = enumValue;
            }
            else
            {
                GlobalSettings.Current.Banner.TextureOutputResolution = OutputResolution.INVALID;
            }
            OnPropertyChanged();
        }
    }

    private bool _isExporting = false;
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
        get => !_isExporting && !IsSavingOrLoading && (Groups.Any(g => g.CanExport) || Colors.Count > 0);
    }

    private bool _isSavingOrLoading = false;
    public bool IsSavingOrLoading
    {
        get => _isSavingOrLoading;
        set
        {
            SetProperty(ref _isSavingOrLoading, value);
            OnPropertyChanged(nameof(CanExport));
        }
    }

    public BannerIconData ToBannerIconData()
    {
        var data = new BannerIconData();
        foreach (var group in GetExportingGroups())
        {
            data.IconGroups.Add(group.ToBannerIconGroup());
        }
        foreach (var color in GetExportingColors())
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

    public IOrderedEnumerable<GroupViewModel> GetExportingGroups()
    {
        return Groups.Where(g => g?.CanExport ?? false).OrderBy(g => g.GroupID);
    }
    public IOrderedEnumerable<ColorViewModel> GetExportingColors()
    {
        return Colors.Where(c => c?.CanExport ?? false).OrderBy(c => c.ID);
    }

    public void AddGroup()
    {
        var newGroup = new GroupViewModel() { GroupID = GetNextGroupID() };
        newGroup.PropertyChanged += OnGroupPropertyChanged;
        Groups.Add(newGroup);
        if (SelectedGroup is null)
        {
            SelectedGroup = Groups.Last();
        }
        OnPropertyChanged(nameof(CanExport));
    }

    public void DeleteGroup(GroupViewModel group)
    {
        if (group is null) return;
        var index = Groups.IndexOf(group);
        if (index < 0) return;
        group.PropertyChanged -= OnGroupPropertyChanged;
        Groups.Remove(group);
        if (group == SelectedGroup)
        {
            if (Groups.Count > 0)
            {
                SelectedGroup = Groups[Math.Max(0, index - 1)];
            }
            else
            {
                SelectedGroup = null;
            }
        }
        OnPropertyChanged(nameof(CanExport));
    }

    public void AddColor()
    {
        Colors.Add(new ColorViewModel() { ID = GetNextColorID() });
    }
    public void DeleteColors(IEnumerable<ColorViewModel> colors)
    {
        var deleting = colors.ToArray();
        foreach (var color in deleting)
        {
            Colors.Remove(color);
        }
    }
    public int GetNextGroupID()
    {
        return Groups.Count > 0 ? Groups.Max(g => g.GroupID) + 1 : GlobalSettings.Current.Banner.CustomGroupStartID;
    }

    public int GetNextColorID()
    {
        return Colors.Count > 0 ? Colors.Max(c => c.ID) + 1 : GlobalSettings.Current.Banner.CustomColorStartID;
    }

    private void OnGroupPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(CanExport));
        if (e.PropertyName == nameof(GroupViewModel.GroupID))
        {

        }
    }

    public void Reset()
    {
        Groups.Clear();
        Colors.Clear();
        IsExporting = false;
        SelectedGroup = null;
        CurrentFile = null;
    }

    public async Task Save(string filePath)
    {
        IsSavingOrLoading = true;
        try
        {
            var data = new SaveData(this);
            using var file = File.OpenWrite(filePath);
            await MessagePackSerializer.SerializeAsync(file, data);
            CurrentFile = await StorageFile.GetFileFromPathAsync(filePath);
        }
        catch (Exception ex) { Log.Error(ex, "error in saving the banner project"); }
        finally
        {
            IsSavingOrLoading = false;
        }
    }
    public async Task Load(StorageFile openedFile)
    {
        try
        {
            IsSavingOrLoading = true;
            using var file = File.OpenRead(openedFile.Path);
            var data = await MessagePackSerializer.DeserializeAsync<SaveData>(file);
            CurrentFile = openedFile;
            Groups.Clear();
            Colors.Clear();
            foreach (var groupData in data.Groups)
            {
                Groups.Add(groupData.Load());
            }
            foreach (var colorData in data.Colors)
            {
                Colors.Add(colorData.Load());
            }
            // Update the selection if there was any
            if (HasSelectedGroup)
            {
                SelectedGroup = Groups.FirstOrDefault(g => g.GroupID == SelectedGroup.GroupID);
            }
            else
            {
                SelectedGroup = Groups.FirstOrDefault();
            }

            OnPropertyChanged(nameof(CanExport));
        }
        catch (Exception ex) { Log.Error(ex, "error in loading the banner project"); }
        finally
        {
            IsSavingOrLoading = false;
        }
    }

    [MessagePackObject]
    public class SaveData
    {
        [Key(0)]
        public GroupViewModel.SaveData[] Groups = new GroupViewModel.SaveData[] { };
        [Key(1)]
        public ColorViewModel.SaveData[] Colors = new ColorViewModel.SaveData[] { };

        public SaveData(DataViewModel vm)
        {
            Groups = vm.Groups.Select(g => new GroupViewModel.SaveData(g)).ToArray();
            Colors = vm.Colors.Select(g => new ColorViewModel.SaveData(g)).ToArray();
        }
        public SaveData() { }
    }
}
