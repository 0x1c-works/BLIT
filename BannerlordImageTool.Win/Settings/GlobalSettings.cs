using BannerlordImageTool.Win.Common;
using Windows.Storage;

namespace BannerlordImageTool.Win.Settings;

public class GlobalSettings : BindableBase
{
    public static GlobalSettings Current
    {
        get => App.Current.Settings;
    }
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
        get => GameRootFolder?.Path ?? I18n.Current.GetString("NeedGameRootFolder");
    }
    public BannerTex.OutputResolution BannerTexOutputResolution { get; set; }
}
