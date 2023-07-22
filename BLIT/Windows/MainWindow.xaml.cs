using BLIT.Views;
using MahApps.Metro.Controls;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;

namespace BLIT.Windows;
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
        });
    }
}

public abstract class WindowBase<TViewModel> : MetroWindow, IViewFor<TViewModel> where TViewModel : class
{
    /// <summary>
    /// The view model dependency property.
    /// </summary>
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
                                    "ViewModel",
                                    typeof(TViewModel),
                                    typeof(ReactiveWindow<TViewModel>),
                                    new PropertyMetadata(null));

    /// <summary>
    /// Gets the binding root view model.
    /// </summary>
    public TViewModel? BindingRoot => ViewModel;

    /// <inheritdoc/>
    public TViewModel? ViewModel
    {
        get => (TViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TViewModel?)value;
    }
}
public abstract class MainWindowBase : WindowBase<MainWindowViewModel> { }
