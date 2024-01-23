using Autofac;
using BLIT.scripts.Common;
using BLIT.scripts.Models;
using BLIT.scripts.Models.BannerIcons;
using Serilog;
using System;
using System.Linq;
using System.Reflection;

namespace BLIT.scripts.Services;
public class AppService {
    private static IContainer? _container;
    public static IContainer Container => _container ?? throw new Exception("AppService not configured");
    public static void Configure() {
        if (_container != null) {
            Log.Warning("AppService already configured. The process is skipped.");
            return;
        }
        var builder = new ContainerBuilder();
        // Singletons

        //builder.RegisterType<FileDialogService>().AsImplementedInterfaces().SingleInstance();
        //builder.RegisterType<ConfirmDialogService>().AsImplementedInterfaces().SingleInstance();
        //builder.RegisterType<NotificationService>().AsImplementedInterfaces().SingleInstance();
        //builder.RegisterType<LoadingService>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<GlobalSettings>().SingleInstance();
        builder.Register((ctx) => BannerSettings.Load()).SingleInstance();
        var assembly = Assembly.GetExecutingAssembly();
        RegisterProjectServices(builder, assembly);

        // Scoped services
        builder.RegisterType<SettingsService>().AsImplementedInterfaces();

        // Scoped components
        builder.RegisterType<BannerIconsProject>().InstancePerLifetimeScope();
        builder.RegisterType<BannerGroupEntry>().InstancePerDependency();
        builder.RegisterType<BannerColorEntry>().InstancePerDependency();
        builder.RegisterType<BannerIconEntry>().InstancePerDependency();

        _container = builder.Build();
    }

    private static void RegisterProjectServices(ContainerBuilder builder, Assembly assembly) {
        var x = assembly.DefinedTypes
            .Where(ti => ti.ImplementedInterfaces.Contains(typeof(IProject)) && !ti.IsAbstract)
            .Select(ti => ti.AsType()).ToList();
        Type genericClass = typeof(ProjectService<>);
        Type genericInterface = typeof(IProjectService<>);

        foreach (TypeInfo? pti in assembly.DefinedTypes
            .Where(ti => ti.ImplementedInterfaces.Contains(typeof(IProject)) && !ti.IsAbstract)
            ) {
            if (pti == null) continue;
            Type serviceClass = genericClass.MakeGenericType(pti.AsType());
            Type serviceInterface = genericInterface.MakeGenericType(pti.AsType());
            MethodInfo? mi = serviceClass.GetMethod("NewProject", BindingFlags.Instance | BindingFlags.Public);
            builder.RegisterType(pti.AsType()).InstancePerLifetimeScope(); ;
            builder.RegisterType(serviceClass).As(serviceInterface).SingleInstance().OnActivated(async (e) => {
                if (mi == null) return;
                await mi.InvokeAsync(e.Instance, [null]);
            });
        }
    }

}
