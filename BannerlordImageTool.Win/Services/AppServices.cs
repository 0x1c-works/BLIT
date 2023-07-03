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
        builder.RegisterType<FileDialogService>().AsImplementedInterfaces();
        builder.RegisterType<ConfirmDialogService>().AsImplementedInterfaces();
        builder.RegisterType<NotificationService>().AsImplementedInterfaces();

        builder.RegisterType<SettingsService>().AsImplementedInterfaces();

        // Singleton data objects
        builder.RegisterType<GlobalSettings>().AsSelf().SingleInstance();
        builder.Register((ctx) => BannerSettings.Load()).AsSelf().SingleInstance();
        builder.RegisterType<BannerIconsPageViewModel>().AsSelf().SingleInstance();

        return new AutofacServiceProvider(builder.Build());
    }

    public static T Get<T>() => App.Current.Services.GetService<T>();
}
