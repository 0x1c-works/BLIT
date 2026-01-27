using BLIT.WPF.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Button = Wpf.Ui.Controls.Button;

namespace BLIT.WPF.Services;

public enum ToastVariant {
    Success,
    Warning,
    Error,
    Info
}

public record struct NotificationAction(string Text, RoutedEventHandler OnClick);

public record struct Notification(
    ToastVariant Variant,
    string Message,
    string? Title = null,
    NotificationAction? Action = null,
    bool KeepOpen = false,
    double TimeoutSeconds = 10.0,
    bool IsClosable = true
);

/// <summary>
/// Service that bridges the NotificationService with WPF-UI SnackbarService.
/// </summary>
public interface INotificationService {
    TimeSpan DefaultTimeOut { get; set; }

    /// <summary>
    /// Shows a notification using the SnackBar.
    /// </summary>
    void Notify(Notification notification);

    void SetSnackbarPresenter(SnackbarPresenter contentPresenter);
    SnackbarPresenter? GetSnackbarPresenter();
}

public class NotificationService : INotificationService {
    private SnackbarPresenter? _presenter;

    private Snackbar? _snackbar;

    public TimeSpan DefaultTimeOut { get; set; } = TimeSpan.FromSeconds(10);

    public void SetSnackbarPresenter(SnackbarPresenter contentPresenter) {
        _presenter = contentPresenter;
    }

    public SnackbarPresenter? GetSnackbarPresenter() {
        return _presenter;
    }

    public void Notify(Notification notification) {
        // Map ToastVariant to ControlAppearance
        var appearance = notification.Variant switch {
            ToastVariant.Success => ControlAppearance.Success,
            ToastVariant.Warning => ControlAppearance.Caution,
            ToastVariant.Error => ControlAppearance.Danger,
            ToastVariant.Info => ControlAppearance.Secondary,
            _ => ControlAppearance.Secondary
        };

        // Create icon based on appearance
        IconElement? icon = notification.Variant switch {
            ToastVariant.Success => new SymbolIcon(SymbolRegular.CheckmarkCircle24, fontSize:32D),
            ToastVariant.Warning => new SymbolIcon(SymbolRegular.Warning24, fontSize:32D),
            ToastVariant.Error => new SymbolIcon(SymbolRegular.ErrorCircle24, fontSize:32D),
            ToastVariant.Info => new SymbolIcon(SymbolRegular.Info24, fontSize:32D),
            _ => null
        };

        // Convert timeout to TimeSpan
        var timeout = notification.KeepOpen
            ? TimeSpan.Zero
            : TimeSpan.FromSeconds(notification.TimeoutSeconds);

        // Show snackbar
        var title = notification.Title ?? notification.Variant.ToString();


        if (_presenter is null) {
            throw new InvalidOperationException($"The SnackbarPresenter was never set");
        }

        Button? actionButton = null;
        if (notification.Action.HasValue) {
            actionButton = new Button() { Content = notification.Action.Value.Text, };
            actionButton.Click += notification.Action.Value.OnClick;
        }

        var content = new SnackbarContent() {
            Message = new ContentControl() { Content = notification.Message }, Action = actionButton,
        };

        _snackbar ??= new Snackbar(_presenter);

        _snackbar.SetCurrentValue(Snackbar.TitleProperty, title);
        _snackbar.SetCurrentValue(ContentControl.ContentProperty, content);
        _snackbar.SetCurrentValue(Snackbar.AppearanceProperty, appearance);
        _snackbar.SetCurrentValue(Snackbar.IconProperty, icon);
        _snackbar.SetCurrentValue(
            Snackbar.TimeoutProperty,
            timeout.TotalSeconds <= 0 ? DefaultTimeOut : timeout
        );

        _snackbar.Show(true);
    }
}