using Autofac;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using BLIT.Helpers;
using BLIT.Services;
using BLIT.ViewModels.Banner;
using BLIT.ViewModels.Banner.Data;
using BLIT.Win.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using Serilog;
using Splat;
using Splat.Autofac;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace BLIT;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        Logging.Initialize();
        // Set a lifetime scope (either the root or any of the child ones) to Autofac resolver.
        // This is needed because Autofac became immutable since version 5+.
        // https://github.com/autofac/Autofac/issues/811
        AutofacDependencyResolver autofacResolver = Container.Resolve<AutofacDependencyResolver>();
        autofacResolver.SetLifetimeScope(Container);

        Log.Information("BLIT initialized");
    }

    public static IHost _host = Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration((builder) => { builder.SetBasePath(FileSystemHelper.AppConfigPath); })
        .UseServiceProviderFactory(new AutofacServiceProviderFactory())
        .ConfigureContainer<ContainerBuilder>(ConfigureContainer)
        .Build();

    public static IContainer Container { get => (IContainer)_host.Services.GetAutofacRoot(); }
    public static void ConfigureContainer(HostBuilderContext ctx, ContainerBuilder builder)
    {
        // Configurations
        var configModule = new ConfigurationModule(ctx.Configuration);
        builder.RegisterModule(configModule);

        // Singleton services
        //builder.RegisterType<FileDialogService>().AsImplementedInterfaces().SingleInstance();
        //builder.RegisterType<ConfirmDialogService>().AsImplementedInterfaces().SingleInstance();
        //builder.RegisterType<NotificationService>().AsImplementedInterfaces().SingleInstance();
        //builder.RegisterType<LoadingService>().AsImplementedInterfaces().SingleInstance();

        // Singleton components
        //builder.RegisterType<GlobalSettings>().SingleInstance();
        builder.Register((ctx) => BannerSettings.Load()).SingleInstance();
        //RegisterProjectService<BannerIconsProject>(builder);

        // Scoped services
        //builder.RegisterType<SettingsService>().AsImplementedInterfaces();

        // Injected view models 
        builder.RegisterType<BannerSettingsViewModel>();

        // Scoped components
        builder.RegisterType<BannerGroupEntry>().InstancePerDependency();
        builder.RegisterType<BannerColorEntry>().InstancePerDependency();
        builder.RegisterType<BannerIconEntry>().InstancePerDependency();

        // Register the Adapter to Splat for ReactiveUI.
        // Creates and sets the Autofac resolver as the Locator.
        AutofacDependencyResolver resolver = builder.UseAutofacDependencyResolver();
        // Register the resolver in Autofac so it can be later resolved.
        builder.RegisterInstance(resolver);
        // Initialize ReactiveUI components.
        resolver.InitializeReactiveUI();

        /* --------- Automatic Registrations -------- */
        var assembly = Assembly.GetExecutingAssembly();
        RegisterRoutableViewsAndModels(builder, assembly);
        RegisterProjectServices(builder, assembly);

        // Register BindingTypeConverters
        Locator.CurrentMutable.RegisterConstant(new BooleanToVisibilityConverter(), typeof(IBindingTypeConverter));
    }
    static void RegisterRoutableViewsAndModels(ContainerBuilder builder, Assembly assembly)
    {
        // Register IViewFor types.
        Locator.CurrentMutable.RegisterViewsForViewModels(assembly);

        // Register IRoutableViewModel types
        foreach (Type? routableVm in assembly.DefinedTypes
            .Where(ti => ti.ImplementedInterfaces.Contains(typeof(IRoutableViewModel)) && !ti.IsAbstract)
            .Select(ti => ti.AsType()))
        {
            if (routableVm != null) builder.RegisterType(routableVm);
        }
    }
    static void RegisterProjectServices(ContainerBuilder builder, Assembly assembly)
    {
        var x = assembly.DefinedTypes
            .Where(ti => ti.ImplementedInterfaces.Contains(typeof(IProject)) && !ti.IsAbstract)
            .Select(ti => ti.AsType()).ToList();
        Type genericPSType = typeof(ProjectService<>);
        Type genericPSIType = typeof(IProjectService<>);

        foreach (TypeInfo? pti in assembly.DefinedTypes
            .Where(ti => ti.ImplementedInterfaces.Contains(typeof(IProject)) && !ti.IsAbstract)
            )
        {
            if (pti == null) continue;
            Type psType = genericPSType.MakeGenericType(pti.AsType());
            Type psiType = genericPSIType.MakeGenericType(pti.AsType());
            MethodInfo? mi = psType.GetMethod("NewProject", BindingFlags.Instance | BindingFlags.Public);
            builder.RegisterType(pti.AsType()).InstancePerLifetimeScope(); ;
            builder.RegisterType(psType).As(psiType).SingleInstance().OnActivated(async (e) => {
                if (mi == null) return;
                await mi.InvokeAsync(e.Instance, new object?[] { null });
            });
        }
    }

    public static object? Get(Type type)
    {
        return _host.Services.GetService(type);
    }
    public static T? Get<T>() where T : class
    {
        return _host.Services.GetService(typeof(T)) as T;
    }
    public static object MustGet(Type type)
    {
        var result = Get(type);
        if (result is null) throw new RegistrationException(type);
        return result;
    }
    public static T MustGet<T>() where T : class
    {
        T? result = Get<T>();
        if (result is null) throw new RegistrationException(typeof(T));
        return result;
    }

    async void OnStartup(object sender, StartupEventArgs e)
    {
        await _host.RunAsync();
    }
    async void OnExit(object sender, ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
    }

    void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled exception");
    }
}

public class RegistrationException : Exception
{
    public RegistrationException(Type type) : base($"{type.Name} is not registered.") { }
}
