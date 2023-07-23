﻿using BLIT.Views;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace BLIT.Windows;
public abstract class MainWindowBase : WindowBase<MainWindowViewModel> { }
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : MainWindowBase
{
    IObservable<Func<NavMenuItem?>> _selectedNavItem;
    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new MainWindowViewModel();

        // An observable that returns a function as the parameter of command Navigate
        // which returns the selected item from the NavMain control when called.
        _selectedNavItem = Observable.Create<Func<NavMenuItem?>>((o) => {
            o.OnNext(() => NavMain.SelectedItem as NavMenuItem);
            return () => { };
        });

        this.WhenActivated(disposables => {
            this.OneWayBind(ViewModel, x => x.Router, x => x.RoutedViewHost.Router).DisposeWith(disposables);
            this.OneWayBind(ViewModel, x => x.Menu, x => x.NavMain.ItemsSource).DisposeWith(disposables);
            this.OneWayBind(ViewModel, x => x.OptionMenu, x => x.NavMain.OptionsItemsSource).DisposeWith(disposables);

            this.BindCommand(
                ViewModel,
                x => x.Navigate,
                x => x.NavMain,
                _selectedNavItem,
                // The command will be called by the ItemInvoked event
                nameof(NavMain.ItemInvoked)).DisposeWith(disposables);

            // This is the initial view
            ViewModel.Router.Navigate.Execute(new WelcomePageViewModel());

            ViewModel.Navigate.ThrownExceptions.Subscribe(async ex => {
                await this.ShowMessageAsync("Navigation Error", ex.Message);
            }).DisposeWith(disposables);
        });
    }
}