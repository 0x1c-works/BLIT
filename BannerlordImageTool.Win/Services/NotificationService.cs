using BannerlordImageTool.Win.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BannerlordImageTool.Win.Services;

public delegate void NotificationActionCallback(Button sender, RoutedEventArgs e);
public record struct NotificationAction(string Text, NotificationActionCallback OnClick);
public record struct Notification(ToastVariant Variant, string Message, string Title = null, NotificationAction? Action = null);
public delegate void NotifyHandler(Notification notification);

public interface INotificationService
{
    event NotifyHandler OnNotify;
    void Notify(Notification notification);
}

class NotificationService : INotificationService
{
    public event NotifyHandler OnNotify;
    public void Notify(Notification notification)
    {
        OnNotify?.Invoke(notification);
    }
}
