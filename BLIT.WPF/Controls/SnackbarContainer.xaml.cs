using System.Windows.Controls;
using BLIT.WPF.Services;
using Wpf.Ui;

namespace BLIT.WPF.Controls;

/// <summary>
/// Snackbar container that sets up the SnackbarPresenter for the SnackbarService.
/// </summary>
public partial class SnackbarContainer : UserControl {
    public SnackbarContainer() {
        InitializeComponent();

        // Set the SnackbarPresenter for the WPF-UI SnackbarService
        var notificationService = AppServices.Get<INotificationService>();
        if (notificationService != null) {
            notificationService.SetSnackbarPresenter(RootSnackbarPresenter);
        }
    }
}