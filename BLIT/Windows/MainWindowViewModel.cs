using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using ReactiveUI;
using System.Collections.Generic;

namespace BLIT.Windows;
public class MainWindowViewModel : ReactiveObject
{
    public List<IHamburgerMenuItem> Menu = new List<IHamburgerMenuItem> {
        new HamburgerMenuIconItem() { Icon = new PackIconUnicons{Kind = PackIconUniconsKind.Home }, Label="Home"}
    };
    public List<IHamburgerMenuItem> OptionMenu = new List<IHamburgerMenuItem> {
        new HamburgerMenuIconItem() { Icon = new PackIconUnicons{Kind = PackIconUniconsKind.Setting }, Label="Settings" }
    };

}
