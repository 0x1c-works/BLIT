using BLIT.Services;
using BLIT.ViewModels.Banner.Data;
using ReactiveUI;
using Splat;

namespace BLIT.ViewModels.Banner;

public class BannerViewModel : ReactiveObject, IRoutableViewModel
{
    public string? UrlPathSegment => "banner";

    public IScreen HostScreen { get; }
    public IProjectService<BannerIconsProject> _project;

    public BannerViewModel(IProjectService<BannerIconsProject> project, IScreen? screen = null)
    {
        HostScreen = screen ?? Locator.Current.GetService<IScreen>()!;
        _project = project;
    }
}
