using BLIT.WPF.Helpers;

namespace BLIT.WPF.Settings;

public class GlobalSettings : BindableBase {
    private string? _gameRootFolder;

    public string? GameRootFolder {
        get => _gameRootFolder;
        set {
            SetProperty(ref _gameRootFolder, value);
            OnPropertyChanged(nameof(GameRootFolderPath));
        }
    }

    public string GameRootFolderPath => GameRootFolder ?? I18n.Current.GetString("NeedGameRootFolder");
}
