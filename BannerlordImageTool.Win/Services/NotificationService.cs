using BannerlordImageTool.Win.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BannerlordImageTool.Win.Services;

public delegate void NotificationActionCallback(Toast toast, RoutedEventArgs e);
public record struct NotificationAction(string Text, NotificationActionCallback OnClick);
public record struct Notification(ToastVariant Variant,
                                  string Message,
                                  string Title = null,
                                  NotificationAction? Action = null,
                                  bool KeepOpen = false,
                                  double TimeoutSeconds = 10.0);
public delegate Toast NotifyHandler(Notification notification);

public interface INotificationService
{
    event NotifyHandler OnNotify;
    Toast Notify(Notification notification);
}

class NotificationService : INotificationService
{
    public event NotifyHandler OnNotify;
    public Toast Notify(Notification notification)
    {
        return OnNotify?.Invoke(notification);
    }
}
