using BannerlordImageTool.Win.Helpers;

namespace BannerlordImageTool.Win.ViewModels.Settings;

public class SettingsViewModel : BindableBase
{
    public BannerSettingsViewModel BannerSettings { get; } = new();
    public SettingsViewModel()
    {
    }
}
