using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using ReactiveUI;
using System;
using System.Collections.Generic;

namespace BLIT.Windows;
public class MainWindowViewModel : ReactiveObject, IScreen
{
    public List<IHamburgerMenuItem> Menu = new List<IHamburgerMenuItem> {
        new NavMenuItem() { Icon = new PackIconUnicons{Kind = PackIconUniconsKind.Home }, Label="Home"}
    };
    public List<IHamburgerMenuItem> OptionMenu = new List<IHamburgerMenuItem> {
        new NavMenuItem() { Icon = new PackIconUnicons{Kind = PackIconUniconsKind.Setting }, Label="Settings" }
    };

    public RoutingState Router { get; }

    public MainWindowViewModel()
    {
        Router = new RoutingState();
    }
}

public class NavMenuItem : HamburgerMenuIconItem
{
    public Type? PageType { get; set; }
}
