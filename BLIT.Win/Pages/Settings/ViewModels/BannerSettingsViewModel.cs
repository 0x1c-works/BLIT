using BLIT.Win.Helpers;
using BLIT.Win.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace BLIT.Win.Pages.Settings.ViewModels;

public class BannerSettingsViewModel : BindableBase {
    private readonly ISettingsService _settings = AppServices.Get<ISettingsService>();
    public ObservableCollection<BannerSpriteScanFolderViewModel> SpriteScanFolders { get; } = new();

    private int _selectedScanFolderIndex;
    public int SelectedSpriteScanFolderIndex {
        get => _selectedScanFolderIndex;
        set => SetProperty(ref _selectedScanFolderIndex, value);
    }
    public BannerSpriteScanFolderViewModel SelectedSpriteScanFolder => SelectedSpriteScanFolderIndex >= 0
            && SelectedSpriteScanFolderIndex < SpriteScanFolders.Count
                ? SpriteScanFolders[SelectedSpriteScanFolderIndex]
                : null;

    public int CustomGroupStartID {
        get => _settings.Banner.CustomGroupStartID;
        set {
            _settings.Banner.CustomGroupStartID = value;
            OnPropertyChanged();
        }
    }
    public int CustomColorStartID {
        get => _settings.Banner.CustomColorStartID;
        set {
            _settings.Banner.CustomColorStartID = value;
            OnPropertyChanged();
        }
    }

    public BannerSettingsViewModel() {
        foreach (BannerSpriteScanFolderViewModel folderVM in _settings.Banner.SpriteScanFolders.Select(relPath => new BannerSpriteScanFolderViewModel(relPath))) {
            SpriteScanFolders.Add(folderVM);
        }
        SpriteScanFolders.CollectionChanged += (s, e) => {
            if (e.Action != NotifyCollectionChangedAction.Move) {
                if (e.OldItems is not null) {
                    foreach (BannerSpriteScanFolderViewModel item in e.OldItems) {
                        item.PropertyChanged -= OnScanFolderPropertyChanged;
                    }
                }
                if (e.NewItems is not null) {
                    foreach (BannerSpriteScanFolderViewModel item in e.NewItems) {
                        item.PropertyChanged += OnScanFolderPropertyChanged;
                    }
                }
            }
            SaveSpriteScanFolders();
        };
    }

    private void OnScanFolderPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(BannerSpriteScanFolderViewModel.RelativePath)) {
            SaveSpriteScanFolders();
        }
    }

    public void SaveSpriteScanFolders() {
        _settings.Banner.SaveSpriteScanFolders(SpriteScanFolders.Select(vm => vm.RelativePath));
    }

}
