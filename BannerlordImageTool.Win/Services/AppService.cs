using Microsoft.Extensions.DependencyInjection;
using System;

namespace BannerlordImageTool.Win.Services;

public class AppService
{
    public static IServiceProvider Configure()
    {
        var service = new ServiceCollection();
        service.AddSingleton<IFileDialogService, FileDialogService>();
        service.AddSingleton<IConfirmDialogService, ConfirmDialogService>();
        return service.BuildServiceProvider();
    }

    public static T Get<T>() => App.Current.Services.GetService<T>();
}
