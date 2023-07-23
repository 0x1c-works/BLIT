using ReactiveUI;
using Splat;

namespace BLIT.ViewModels;
public class BannerEditorViewModel : ReactiveObject, IRoutableViewModel
{
    public string? UrlPathSegment => "banner";

    public IScreen HostScreen { get; }

    public BannerEditorViewModel(IScreen screen)
    {
        HostScreen = screen ?? Locator.Current.GetService<IScreen>()!;
    }
}
