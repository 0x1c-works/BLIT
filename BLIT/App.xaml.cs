using Autofac;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using BLIT.Helpers;
using BLIT.Views;
using BLIT.Windows;
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
        // Set a lifetime scope (either the root or any of the child ones) to Autofac resolver.
        // This is needed because Autofac became immutable since version 5+.
        // https://github.com/autofac/Autofac/issues/811
        AutofacDependencyResolver autofacResolver = Container.Resolve<AutofacDependencyResolver>();
        autofacResolver.SetLifetimeScope(Container);
    }

    void SetupDI()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<HomeView>().As<IViewFor<MainWindowViewModel>>();
        builder.RegisterType<WelcomePageViewModel>();

        AutofacDependencyResolver resolver = builder.UseAutofacDependencyResolver();
        builder.RegisterInstance(resolver);
        resolver.InitializeReactiveUI();


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

        // Pages & VMs
        builder.RegisterType<HomeView>();
        builder.RegisterType<WelcomePageViewModel>();

        // Register the Adapter to Splat for ReactiveUI.
        // Creates and sets the Autofac resolver as the Locator.
        AutofacDependencyResolver resolver = builder.UseAutofacDependencyResolver();
        // Register the resolver in Autofac so it can be later resolved.
        builder.RegisterInstance(resolver);
        // Initialize ReactiveUI components.
        resolver.InitializeReactiveUI();

        var ass = Assembly.GetCallingAssembly();
        var ass2 = Assembly.GetEntryAssembly();
        var ass3 = Assembly.GetExecutingAssembly();
        var l0 = Assembly.GetCallingAssembly().DefinedTypes.ToList();
        var l = Assembly.GetCallingAssembly().DefinedTypes
            .Where(ti => ti.ImplementedInterfaces.Contains(typeof(IViewFor)))
            .Select(ti => new { ti.Name, ti.ImplementedInterfaces, ti.IsAbstract })
            .ToList();


        Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());
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
