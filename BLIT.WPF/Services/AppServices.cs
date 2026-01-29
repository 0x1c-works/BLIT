using Autofac;
using Autofac.Extensions.DependencyInjection;
using BLIT.WPF.Pages.BannerIcons.Models;
using BLIT.WPF.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace BLIT.WPF.Services;

public class AppServices {
    public static IContainer Container { get; private set; } = null!;

    public static IServiceProvider Configure() {
        var builder = new ContainerBuilder();

        // Singleton services
        builder.RegisterType<FileDialogService>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ConfirmDialogService>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<NotificationService>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<LoadingService>().AsImplementedInterfaces().SingleInstance();

        // Singleton components
        builder.RegisterType<GlobalSettings>().SingleInstance();
        builder.Register(ctx => BannerSettings.Load()).SingleInstance();
        RegisterProjectService<BannerIconsProject>(builder);

        // Scoped services
        builder.RegisterType<SettingsService>().AsImplementedInterfaces();

        // Scoped components
        builder.RegisterType<BannerIconsProject>().InstancePerLifetimeScope();
        builder.RegisterType<BannerGroupEntry>().InstancePerDependency();
        builder.RegisterType<BannerColorEntry>().InstancePerDependency();
        builder.RegisterType<BannerIconEntry>().InstancePerDependency();

        // Register factories
        builder.Register<BannerGroupEntry.Factory>(ctx => {
            var container = ctx.Resolve<ILifetimeScope>();
            return groupID => container.Resolve<BannerGroupEntry>(
                new TypedParameter(typeof(int), groupID)
            );
        }).InstancePerLifetimeScope();

        builder.Register<BannerColorEntry.Factory>(ctx => {
            var container = ctx.Resolve<ILifetimeScope>();
            return id => container.Resolve<BannerColorEntry>(
                new TypedParameter(typeof(int), id)
            );
        }).InstancePerLifetimeScope();

        builder.Register<BannerIconEntry.Factory>(ctx => {
            var container = ctx.Resolve<ILifetimeScope>();
            return (groupVm, texturePath) => container.Resolve<BannerIconEntry>(
                new TypedParameter(typeof(BannerGroupEntry), groupVm),
                new TypedParameter(typeof(string), texturePath)
            );
        }).InstancePerLifetimeScope();

        Container = builder.Build();
        return new AutofacServiceProvider(Container);
    }

    public static T? Get<T>() where T : class {
        if (App.Current?.Services == null) {
            return null;
        }

        return App.Current.Services.GetService<T>();
    }

    private static void RegisterProjectService<T>(ContainerBuilder builder) where T : IProject {
        builder.RegisterType<ProjectService<T>>()
            .As<IProjectService<T>>()
            .SingleInstance()
            .OnActivated(async e => await e.Instance.NewProject());
    }
}