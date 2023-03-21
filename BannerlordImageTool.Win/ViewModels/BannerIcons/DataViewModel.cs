using BannerlordImageTool.Banner;
using BannerlordImageTool.Win.Common;
using BannerlordImageTool.Win.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.UI;

namespace BannerlordImageTool.Win.ViewModels.BannerIcons;
public class DataViewModel : BindableBase
{
    public ObservableCollection<GroupViewModel> Groups { get; } = new();
    public ObservableCollection<ColorViewModel> Colors { get; } = new() {
            new ColorViewModel(){ID=123,Color=Color.FromArgb(255,255,0,0)},
            new ColorViewModel(){ID=234,Color=Color.FromArgb(255,0,255,0)},
    };

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
        get => !_isExporting && (Groups.Any(g => g.CanExport) || Colors.Count > 0);
    }

    public BannerIconData ToBannerIconData()
    {
        var data = new BannerIconData();
        foreach (var group in GetExportingGroups())
        {
            data.IconGroups.Add(group.ToBannerIconGroup());
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
        return Groups.Where(g => g.CanExport).OrderBy(g => g.GroupID);
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
}
