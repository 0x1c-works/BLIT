using BLIT.Win.Settings;

namespace BLIT.Win.Services;

public interface ISettingsService {
    GlobalSettings Global { get; }
    BannerSettings Banner { get; }
}

public class SettingsService : ISettingsService {
    public SettingsService(GlobalSettings global, BannerSettings banner) {
        Global = global;
        Banner = banner;
    }

    #region ISettingsService Members

    public GlobalSettings Global { get; }
    public BannerSettings Banner { get; }

    #endregion
}