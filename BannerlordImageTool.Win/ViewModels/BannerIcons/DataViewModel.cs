using BannerlordImageTool.BannerTex;
using BannerlordImageTool.Win.Common;
using BannerlordImageTool.Win.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BannerlordImageTool.Win.ViewModels.BannerIcons;
public class DataViewModel : BindableBase
{
    public ObservableCollection<GroupViewModel> Groups { get; } = new();

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
            switch (GlobalSettings.Current.BannerTexOutputResolution)
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
                GlobalSettings.Current.BannerTexOutputResolution = enumValue;
            }
            else
            {
                GlobalSettings.Current.BannerTexOutputResolution = OutputResolution.INVALID;
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
        get => !_isExporting && Groups.Any(g => g.CanExport);
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

    public IOrderedEnumerable<GroupViewModel> GetExportingGroups()
    {
        return Groups.Where(g => g.CanExport).OrderBy(g => g.GroupID);
    }

    public void AddGroup()
    {
        int predictedId = Groups.OrderBy(g => g.GroupID).LastOrDefault()?.GroupID + 1 ?? 800;
        while (Groups.Any(g => g.GroupID == predictedId))
        {
            predictedId++;
        }
        var newGroup = new GroupViewModel() { GroupID = predictedId };
        newGroup.PropertyChanged += OnGroupPropertyChanged;
        Groups.Add(newGroup);
        OnPropertyChanged(nameof(CanExport));
    }

    private void OnGroupPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(CanExport));
    }
}
