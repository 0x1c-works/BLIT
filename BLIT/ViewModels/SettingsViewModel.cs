using ReactiveUI;
using Splat;
using System;

namespace BLIT.ViewModels;
public class SettingsViewModel : ReactiveObject, IRoutableViewModel
{
    public string? UrlPathSegment => "settings";

    public IScreen HostScreen { get; }

    public SettingsViewModel(IScreen? screen = null)
    {
        HostScreen = screen ?? Locator.Current.GetService<IScreen>()!;
    }
}
