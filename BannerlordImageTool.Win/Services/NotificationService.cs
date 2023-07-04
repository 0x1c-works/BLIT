using BannerlordImageTool.Win.Controls;
using CommunityToolkit.WinUI.UI;
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
                                  double TimeoutSeconds = 10.0,
                                  bool IsClosable = true)
{
    public Toast CreateToast()
    {
        Button actionButton = null;
        if (Action.HasValue)
        {
            NotificationAction action = Action.Value;
            actionButton = new Button() {
                Content = action.Text,
            };
            actionButton.Click += (s, e) => {
                Toast toast = (s as Button).FindAscendant<Toast>();
                action.OnClick(toast, e);
            };
        }
        var toast = new Toast() {
            Message = Message,
            Title = Title,
            Variant = Variant,
            IsClosable = IsClosable,
            IsOpen = true,
            ActionButton = actionButton,
            TimeoutSeconds = KeepOpen ? 0 : TimeoutSeconds,
        };
        return toast;
    }
}
public delegate Toast NotifyHandler(Notification notification);

public interface INotificationService
{
    event NotifyHandler OnNotify;
    Toast Notify(Notification notification);
}

public class NotificationService : INotificationService
{
    public event NotifyHandler OnNotify;
    public Toast Notify(Notification notification)
    {
        return OnNotify?.Invoke(notification);
    }
}
