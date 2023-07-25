using ReactiveUI;
using Splat;

namespace BLIT.ViewModels.Banner;

public class BannerViewModel : ReactiveObject, IRoutableViewModel
{
    public string? UrlPathSegment => "banner";

    public IScreen HostScreen { get; }

    public BannerViewModel(IScreen? screen = null)
    {
        HostScreen = screen ?? Locator.Current.GetService<IScreen>()!;
    }
}
