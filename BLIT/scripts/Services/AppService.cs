using Autofac;
using Serilog;
using System;

namespace BLIT.scripts.Services;
public class AppService
{
    static IContainer? _container;
    public static IContainer Container => _container ?? throw new Exception("AppService not configured");
    public static void Configure()
    {
        if (_container != null)
        {
            Log.Warning("AppService already configured. The process is skipped.");
            return;
        }
        var builder = new ContainerBuilder();
        // Singleton services
        //builder.RegisterType<FileDialogService>().AsImplementedInterfaces().SingleInstance();
        //builder.RegisterType<ConfirmDialogService>().AsImplementedInterfaces().SingleInstance();
        //builder.RegisterType<NotificationService>().AsImplementedInterfaces().SingleInstance();
        //builder.RegisterType<LoadingService>().AsImplementedInterfaces().SingleInstance();

        // Singleton components
        //builder.RegisterType<GlobalSettings>().SingleInstance();
        //builder.Register((ctx) => BannerSettings.Load()).SingleInstance();
        //RegisterProjectService<BannerIconsProject>(builder);

        // Scoped services
        //builder.RegisterType<SettingsService>().AsImplementedInterfaces();

        // Scoped components
        //builder.RegisterType<BannerIconsProject>().InstancePerLifetimeScope();
        //builder.RegisterType<BannerGroupEntry>().InstancePerDependency();
        //builder.RegisterType<BannerColorEntry>().InstancePerDependency();
        //builder.RegisterType<BannerIconEntry>().InstancePerDependency();

        _container = builder.Build();
    }

    static void RegisterProjectService<T>(ContainerBuilder builder) where T : IProject
    {
        builder.RegisterType<ProjectService<T>>()
            .As<IProjectService<T>>()
            .SingleInstance()
            .OnActivated(async (e) => await e.Instance.NewProject());
    }

}
