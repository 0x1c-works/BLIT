using MahApps.Metro.Controls;

namespace BLIT.Windows;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindowViewModel ViewModel { get; } = new MainWindowViewModel();
    public MainWindow()
    {
        InitializeComponent();
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
