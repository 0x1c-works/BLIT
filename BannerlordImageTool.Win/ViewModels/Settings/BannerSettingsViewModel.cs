using BannerlordImageTool.Win.Common;
using BannerlordImageTool.Win.Settings;
using System.Collections.ObjectModel;
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

    public BannerSettingsViewModel()
    {
        foreach (var folderVM in GlobalSettings
                                    .Current
                                    .BannerSpriteScanFolders
                                    .Select(relPath => new BannerSpriteScanFolderViewModel(relPath)))
        {
            SpriteScanFolders.Add(folderVM);
        }
    }
}
