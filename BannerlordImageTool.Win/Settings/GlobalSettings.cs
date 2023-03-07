using BannerlordImageTool.Win.Common;
using System.Globalization;
using Windows.Storage;

namespace BannerlordImageTool.Win.Settings;

public class GlobalSettings : BindableBase
{
    private StorageFolder _gameRootFolder;
    public StorageFolder GameRootFolder
    {
        get => _gameRootFolder;
        set
        {
            SetProperty(ref _gameRootFolder, value);
            OnPropertyChanged(nameof(GameRootFolderPath));
        }
    }

    public string GameRootFolderPath
    {
        get => GameRootFolder?.Path ?? "(Please select the location of your installed game)";
    }
}
