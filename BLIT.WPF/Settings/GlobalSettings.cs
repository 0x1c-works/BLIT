using CommunityToolkit.Mvvm.ComponentModel;
using BLIT.WPF.Helpers;

namespace BLIT.WPF.Settings;

public partial class GlobalSettings : ObservableObject {
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
