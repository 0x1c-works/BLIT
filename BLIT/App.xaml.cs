using Autofac;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using BLIT.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
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
