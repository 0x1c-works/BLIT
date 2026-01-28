using BLIT.WPF.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BLIT.WPF.Settings;

public class GlobalSettings : ObservableObject {
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