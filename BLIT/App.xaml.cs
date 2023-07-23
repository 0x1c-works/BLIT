using Autofac;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using BLIT.Helpers;
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
        //builder.Register((ctx) => BannerSettings.Load()).SingleInstance();
        //RegisterProjectService<BannerIconsProject>(builder);

        // Scoped services
        //builder.RegisterType<SettingsService>().AsImplementedInterfaces();

        // Scoped components
        //builder.RegisterType<BannerIconsProject>().InstancePerLifetimeScope();
        //builder.RegisterType<BannerGroupEntry>().InstancePerDependency();
        //builder.RegisterType<BannerColorEntry>().InstancePerDependency();
        //builder.RegisterType<BannerIconEntry>().InstancePerDependency();

        // Register the Adapter to Splat for ReactiveUI.
        // Creates and sets the Autofac resolver as the Locator.
        AutofacDependencyResolver resolver = builder.UseAutofacDependencyResolver();
        // Register the resolver in Autofac so it can be later resolved.
        builder.RegisterInstance(resolver);
        // Initialize ReactiveUI components.
        resolver.InitializeReactiveUI();

        /* --------- Automatic Registrations -------- */
        var assembly = Assembly.GetExecutingAssembly();
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

    public static object? Get(Type type)
    {
        return _host.Services.GetService(type);
    }
    public static T? Get<T>() where T : class
    {
        return _host.Services.GetService(typeof(T)) as T;
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


    //static void RegisterProjectService<T>(ContainerBuilder builder) where T : IProject
    //{
    //    builder.RegisterType<ProjectService<T>>()
    //        .As<IProjectService<T>>()
    //        .SingleInstance()
    //        .OnActivated(async (e) => await e.Instance.NewProject());
    //}
}
