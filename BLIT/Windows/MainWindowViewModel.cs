using BLIT.Views;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using ReactiveUI;
using System;
using System.Collections.Generic;

namespace BLIT.Windows;
public class MainWindowViewModel : ReactiveObject, IScreen
{
    public List<IHamburgerMenuItem> Menu = new List<IHamburgerMenuItem> {
        new NavMenuItem() { Icon = new PackIconUnicons{Kind = PackIconUniconsKind.Home }, Label="Home", TargetViewModel = typeof(WelcomePageViewModel)}
    };
    public List<IHamburgerMenuItem> OptionMenu = new List<IHamburgerMenuItem> {
        new NavMenuItem() { Icon = new PackIconUnicons{Kind = PackIconUniconsKind.Setting }, Label="Settings" }
    };
    public ReactiveCommand<Func<NavMenuItem>, IRoutableViewModel> Navigate { get; }

    public RoutingState Router { get; }

    public MainWindowViewModel()
    {
        Router = new RoutingState();
        Navigate = ReactiveCommand.CreateFromObservable<Func<NavMenuItem>, IRoutableViewModel>(getter => {
            Type? vmType = getter()?.TargetViewModel;
            if (vmType != null && App.Get(vmType) is IRoutableViewModel vm)
                return Router.Navigate.Execute(vm);
            throw new NavigationException(vmType);
        });
    }
}

public class NavMenuItem : HamburgerMenuIconItem
{
    public Type? TargetViewModel { get; set; }
}

public class NavigationException : Exception
{
    public NavigationException(Type? type) : base(type == null
        ? "Lack of target routable view model"
        : $"Cannot navigate to {type})")
    { }
}
