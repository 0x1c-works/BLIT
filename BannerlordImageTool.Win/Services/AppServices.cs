using Autofac;
using Autofac.Extensions.DependencyInjection;
using BannerlordImageTool.Win.Pages.BannerIcons.ViewModels;
using BannerlordImageTool.Win.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BannerlordImageTool.Win.Services;

public class AppServices
{
    public static IServiceProvider Configure()
    {
        var builder = new ContainerBuilder();
        // Singleton services
        _ = builder.RegisterType<FileDialogService>().AsImplementedInterfaces().SingleInstance();
        _ = builder.RegisterType<ConfirmDialogService>().AsImplementedInterfaces().SingleInstance();
        _ = builder.RegisterType<NotificationService>().AsImplementedInterfaces().SingleInstance();

        // Singleton data objects
        _ = builder.RegisterType<GlobalSettings>().AsSelf().SingleInstance();
        _ = builder.Register((ctx) => BannerSettings.Load()).AsSelf().SingleInstance();
        _ = builder.RegisterType<BannerIconsPageViewModel>().AsSelf().SingleInstance();

        // Scoped services
        _ = builder.RegisterType<SettingsService>().AsImplementedInterfaces();

        return new AutofacServiceProvider(builder.Build());
    }

    public static T Get<T>()
    {
        return App.Current.Services.GetService<T>();
    }
}
