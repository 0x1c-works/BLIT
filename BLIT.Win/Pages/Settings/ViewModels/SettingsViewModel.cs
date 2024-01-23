using BLIT.Win.Helpers;

namespace BLIT.Win.Pages.Settings.ViewModels;

public class SettingsViewModel : BindableBase {
    public BannerSettingsViewModel BannerSettings { get; } = new();
    public SettingsViewModel() {
    }
}
