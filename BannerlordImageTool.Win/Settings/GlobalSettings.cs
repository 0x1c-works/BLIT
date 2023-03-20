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

    public BannerSettings Banner { get; private set; } = new();

    public GlobalSettings()
    {
        Banner = BannerSettings.Load();
    }


    static class Keys
    {
        public const string BannerSpriteScanFolders = "banner_sprite_scan_folders";
    }

}

