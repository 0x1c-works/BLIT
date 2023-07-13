using Autofac;
using Autofac.Extensions.DependencyInjection;
using BLIT.Win.Pages.BannerIcons.Models;
using BLIT.Win.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BLIT.Win.Services;

public class AppServices
{
    public static IContainer Container { get; private set; }
    public static IServiceProvider Configure()
    {
        var builder = new ContainerBuilder();
        // Singleton services
        builder.RegisterType<FileDialogService>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ConfirmDialogService>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<NotificationService>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<LoadingService>().AsImplementedInterfaces().SingleInstance();

        // Singleton components
        builder.RegisterType<GlobalSettings>().SingleInstance();
        builder.Register((ctx) => BannerSettings.Load()).SingleInstance();
        RegisterProjectService<BannerIconsProject>(builder);

        // Scoped services
        builder.RegisterType<SettingsService>().AsImplementedInterfaces();

        // Scoped components
        builder.RegisterType<BannerIconsProject>().InstancePerLifetimeScope();
        builder.RegisterType<BannerGroupEntry>().InstancePerDependency();
        builder.RegisterType<BannerColorEntry>().InstancePerDependency();
        builder.RegisterType<BannerIconEntry>().InstancePerDependency();

        Container = builder.Build();
        return new AutofacServiceProvider(Container);
    }

    public static T Get<T>()
    {
        return App.Current.Services.GetService<T>();
    }

    static void RegisterProjectService<T>(ContainerBuilder builder) where T : IProject
    {
        builder.RegisterType<ProjectService<T>>()
            .As<IProjectService<T>>()
            .SingleInstance()
            .OnActivated(async (e) => await e.Instance.NewProject());
    }
}
