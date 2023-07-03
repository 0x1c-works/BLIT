using Microsoft.Extensions.DependencyInjection;
using System;

namespace BannerlordImageTool.Win.Services;

public class AppServices
{
    public static IServiceProvider Configure()
    {
        var service = new ServiceCollection();
        service.AddSingleton<IFileDialogService, FileDialogService>();
        service.AddSingleton<IConfirmDialogService, ConfirmDialogService>();
        service.AddSingleton<INotificationService, NotificationService>();
        return service.BuildServiceProvider();
    }

    public static T Get<T>() => App.Current.Services.GetService<T>();
}
