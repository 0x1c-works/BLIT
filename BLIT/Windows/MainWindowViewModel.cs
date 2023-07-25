﻿using BLIT.ViewModels;
using BLIT.ViewModels.Banner;
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
        new NavMenuItem() {
            Icon = new PackIconBootstrapIcons{Kind = PackIconBootstrapIconsKind.House},
            Label="Home",
            TargetViewModel = typeof(HomeViewModel)
        },
        new NavMenuItem() {
            Icon = new PackIconBootstrapIcons{Kind = PackIconBootstrapIconsKind.Flag},
            Label="Banner",
            TargetViewModel = typeof(BannerViewModel)
        }
    };
    public List<IHamburgerMenuItem> OptionMenu = new List<IHamburgerMenuItem> {
        new NavMenuItem() {
            Icon = new PackIconBootstrapIcons{Kind = PackIconBootstrapIconsKind.Gear },
            Label="Settings",
            TargetViewModel=typeof(SettingsViewModel)
        }
    };
    public ReactiveCommand<NavMenuItem?, IRoutableViewModel> Navigate { get; }

    public RoutingState Router { get; }

    public MainWindowViewModel()
    {
        Router = new RoutingState();
        Navigate = ReactiveCommand.CreateFromObservable<NavMenuItem?, IRoutableViewModel>(menuItem => {
            Type? vmType = menuItem?.TargetViewModel;
            var vm = vmType != null ? App.Get(vmType) : null;
            if (vm is IRoutableViewModel rvm)
                return Router.Navigate.Execute(rvm);
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
        : $"View model {type.Name} is not registered")
    { }
}
