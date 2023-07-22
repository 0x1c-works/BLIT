using ReactiveUI;
using Splat;
using System.Diagnostics;
using System.Reactive;
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
            this.BindCommand(ViewModel, x => x.OpenWebsite, x => x.linkWebsite).DisposeWith(disposables);
        });
    }
}

public class WelcomePageViewModel : ReactiveObject, IRoutableViewModel
{
    public string WebisteUrl = "https://blit.0x1.best";
    public string Target = "World";

    public string? UrlPathSegment => "welcome";

    public IScreen HostScreen { get; }
    public ReactiveCommand<Unit, Unit> OpenWebsite;

    public WelcomePageViewModel(IScreen? screen = null)
    {
        HostScreen = screen ?? Locator.Current.GetService<IScreen>()!;
        OpenWebsite = ReactiveCommand.Create(() => {
            Process.Start(new ProcessStartInfo(WebisteUrl) { UseShellExecute = true });
        });
    }
}
