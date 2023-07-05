using BannerlordImageTool.Win.Helpers;

namespace BannerlordImageTool.Win.Pages.Settings.ViewModels;

public class SettingsViewModel : BindableBase
{
    public BannerSettingsViewModel BannerSettings { get; } = new();
    public SettingsViewModel()
    {
    }
}
