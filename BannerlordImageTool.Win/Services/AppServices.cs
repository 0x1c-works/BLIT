using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BannerlordImageTool.Win.Services;

public class AppServices
{
    public static IServiceProvider Configure()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<FileDialogService>().AsImplementedInterfaces();
        builder.RegisterType<ConfirmDialogService>().AsImplementedInterfaces();
        builder.RegisterType<NotificationService>().AsImplementedInterfaces();
        return new AutofacServiceProvider(builder.Build());
    }

    public static T Get<T>() => App.Current.Services.GetService<T>();
}
