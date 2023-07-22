using ReactiveUI;
using Splat;
using System.Reactive.Disposables;

namespace BLIT.Views;

public abstract class HomeViewBase : ReactiveUserControl<WelcomePageViewModel> { }

/// <summary>
/// Interaction logic for WelcomePage.xaml
/// </summary>
public partial class HomeView : HomeViewBase
{
    public HomeView()
    {
        InitializeComponent();
        this.WhenActivated((disposables) => {
            this.OneWayBind(ViewModel, x => x.Target, x => x.txtTarget.Text).DisposeWith(disposables);
            this.OneWayBind(ViewModel, x => x.WebisteUrl, x => x.linkWebsite.NavigateUri).DisposeWith(disposables);
        });

    }
}

public class WelcomePageViewModel : ReactiveObject, IRoutableViewModel
{
    public string WebisteUrl = "https://blit.0x1.best";
    public string Target = "World";

    public string? UrlPathSegment => "welcome";

    public IScreen HostScreen { get; }

    public WelcomePageViewModel(IScreen? screen = null)
    {
        HostScreen = screen ?? Locator.Current.GetService<IScreen>()!;
    }
}
