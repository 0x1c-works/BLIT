using Autofac;
using Autofac.Extensions.DependencyInjection;
using BannerlordImageTool.Win.Pages.BannerIcons.ViewModels;
using BannerlordImageTool.Win.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BannerlordImageTool.Win.Services;

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

        // Singleton components
        builder.RegisterType<GlobalSettings>().SingleInstance();
        builder.Register((ctx) => BannerSettings.Load()).SingleInstance();
        builder.RegisterGeneric(typeof(ProjectService<>)).As(typeof(IProjectService<>)).SingleInstance();

        // Scoped services
        builder.RegisterType<SettingsService>().AsImplementedInterfaces();

        // Scoped components
        builder.RegisterType<BannerIconsPageViewModel>().InstancePerLifetimeScope();
        builder.RegisterType<BannerGroupViewModel>().InstancePerLifetimeScope().WithParameter(new TypedParameter(typeof(int), -1));
        builder.RegisterType<BannerColorViewModel>().InstancePerLifetimeScope();
        builder.RegisterType<BannerIconViewModel>().InstancePerLifetimeScope();

        Container = builder.Build();
        return new AutofacServiceProvider(Container);
    }

    public static T Get<T>()
    {
        return App.Current.Services.GetService<T>();
    }
}
