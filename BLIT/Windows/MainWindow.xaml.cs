using BLIT.Views;
using MahApps.Metro.Controls;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;

namespace BLIT.Windows;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : MainWindowBase
{
    //IAppNavService? _appNavService = App.Get<IAppNavService>();

    public MainWindow()
    {
        InitializeComponent();
        //if (_appNavService != null)
        //{
        //    _appNavService.Frame = MainFrame;
        //}
        //MainFrame.Navigate(new Uri("Pages/WelcomePage.xaml", UriKind.Relative));

        ViewModel = new MainWindowViewModel();

        this.WhenActivated(disposables => {
            //this.BindCommand(ViewModel, vm => vm.NavigateToSettings, v => v.SettingsButton);
            //this.BindCommand(ViewModel, vm => vm.NavigateToHome, v => v.HomeButton);
            this.OneWayBind(ViewModel, x => x.Router, x => x.RoutedViewHost.Router).DisposeWith(disposables);
            ViewModel.Router.Navigate.Execute(new WelcomePageViewModel());
        });
    }

    void HamburgerMenuControl_ItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs args)
    {
        NavMain.Content = args.InvokedItem;

        if (!args.IsItemOptions && NavMain.IsPaneOpen)
        {
            // You can close the menu if an item was selected
            // this.HamburgerMenuControl.SetCurrentValue(HamburgerMenuControl.IsPaneOpenProperty, false);
        }
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
