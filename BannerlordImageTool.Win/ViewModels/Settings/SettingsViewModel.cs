using BannerlordImageTool.Win.Common;

namespace BannerlordImageTool.Win.ViewModels.Settings;

public class SettingsViewModel : BindableBase
{
    public BannerSettingsViewModel BannerSettings { get; } = new();
    public SettingsViewModel()
    {
    }
}
