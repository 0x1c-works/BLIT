using BannerlordImageTool.Win.Helpers;
using BannerlordImageTool.Win.Settings;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace BannerlordImageTool.Win.ViewModels.Settings;

public class BannerSettingsViewModel : BindableBase
{
    public ObservableCollection<BannerSpriteScanFolderViewModel> SpriteScanFolders { get; } = new();
    private int _selectedScanFolderIndex;
    public int SelectedSpriteScanFolderIndex
    {
        get => _selectedScanFolderIndex;
        set
        {
            SetProperty(ref _selectedScanFolderIndex, value);
        }
    }
    public BannerSpriteScanFolderViewModel SelectedSpriteScanFolder
    {
        get => SelectedSpriteScanFolderIndex >= 0
            && SelectedSpriteScanFolderIndex < SpriteScanFolders.Count
                ? SpriteScanFolders[SelectedSpriteScanFolderIndex]
                : null;
    }

    public int CustomGroupStartID
    {
        get => GlobalSettings.Current.Banner.CustomGroupStartID;
        set
        {
            GlobalSettings.Current.Banner.CustomGroupStartID = value;
            OnPropertyChanged();
        }
    }
    public int CustomColorStartID
    {
        get => GlobalSettings.Current.Banner.CustomColorStartID;
        set
        {
            GlobalSettings.Current.Banner.CustomColorStartID = value;
            OnPropertyChanged();
        }
    }

    public BannerSettingsViewModel()
    {
        foreach (var folderVM in GlobalSettings
                                    .Current
                                    .Banner.SpriteScanFolders
                                    .Select(relPath => new BannerSpriteScanFolderViewModel(relPath)))
        {
            SpriteScanFolders.Add(folderVM);
        }
        SpriteScanFolders.CollectionChanged += (s, e) => {
            if (e.Action != NotifyCollectionChangedAction.Move)
            {
                if (e.OldItems is not null)
                {
                    foreach (BannerSpriteScanFolderViewModel item in e.OldItems)
                    {
                        item.PropertyChanged -= OnScanFolderPropertyChanged;
                    }
                }
                if (e.NewItems is not null)
                {
                    foreach (BannerSpriteScanFolderViewModel item in e.NewItems)
                    {
                        item.PropertyChanged += OnScanFolderPropertyChanged;
                    }
                }
            }
            SaveSpriteScanFolders();
        };
    }

    private void OnScanFolderPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BannerSpriteScanFolderViewModel.RelativePath))
        {
            SaveSpriteScanFolders();
        }
    }

    public void SaveSpriteScanFolders()
    {
        GlobalSettings.Current.Banner.SaveSpriteScanFolders(SpriteScanFolders.Select(vm => vm.RelativePath));
    }

}
