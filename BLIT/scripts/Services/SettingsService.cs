using BLIT.scripts.Models;
using BLIT.scripts.Models.BannerIcons;

namespace BLIT.scripts.Services;

public interface ISettingsService {
    GlobalSettings Global { get; }
    BannerSettings Banner { get; }
}


public class SettingsService : ISettingsService {
    public GlobalSettings Global { get; }
    public BannerSettings Banner { get; }

    public SettingsService(GlobalSettings global, BannerSettings banner) {
        Global = global;
        Banner = banner;
    }
}
