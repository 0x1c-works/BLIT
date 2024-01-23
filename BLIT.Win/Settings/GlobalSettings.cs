using BLIT.Win.Helpers;
using Windows.Storage;

namespace BLIT.Win.Settings;

public class GlobalSettings : BindableBase {
    private StorageFolder _gameRootFolder;

    public StorageFolder GameRootFolder {
        get => _gameRootFolder;
        set {
            SetProperty(ref _gameRootFolder, value);
            OnPropertyChanged(nameof(GameRootFolderPath));
        }
    }

    public string GameRootFolderPath => GameRootFolder?.Path ?? I18n.Current.GetString("NeedGameRootFolder");
}

