using BannerlordImageTool.Win.Common;
using BannerlordImageTool.Win.Settings;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace BannerlordImageTool.Win.ViewModels.Settings;

public class SettingsViewModel : BindableBase
{
    public BannerSettingsViewModel BannerSettings { get; } = new();
    public SettingsViewModel()
    {
    }
}
