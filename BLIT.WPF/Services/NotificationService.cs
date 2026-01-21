using System;

namespace BLIT.WPF.Services;

public enum ToastVariant {
    Success,
    Warning,
    Error,
    Info
}

public delegate void NotificationActionCallback(object sender, EventArgs e);

public record struct NotificationAction(string Text, NotificationActionCallback OnClick);

public record struct Notification(
    ToastVariant Variant,
    string Message,
    string? Title = null,
    NotificationAction? Action = null,
    bool KeepOpen = false,
    double TimeoutSeconds = 10.0,
    bool IsClosable = true
);

public delegate void NotifyHandler(Notification notification);

public interface INotificationService {
    event NotifyHandler? OnNotify;
    void Notify(Notification notification);
}

public class NotificationService : INotificationService {
    public event NotifyHandler? OnNotify;
    
    public void Notify(Notification notification) {
        OnNotify?.Invoke(notification);
    }
}
