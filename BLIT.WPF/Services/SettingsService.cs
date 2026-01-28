using BLIT.WPF.Helpers;
using BLIT.WPF.Settings;

namespace BLIT.WPF.Services;

public interface ISettingsService {
    string Version { get; }
    GlobalSettings Global { get; }
    BannerSettings Banner { get; }
}

public class SettingsService(GlobalSettings global, BannerSettings banner) : ISettingsService {
    public GlobalSettings Global { get; } = global;
    public BannerSettings Banner { get; } = banner;

    public string Version => VersionHelper.GetFullVersion();
}