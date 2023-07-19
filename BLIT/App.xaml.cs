using Autofac;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using BLIT.Helpers;
using BLIT.Services;
using BLIT.ViewModels;
using BLIT.Views.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace BLIT;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    static readonly IHost _host = Host
        .CreateDefaultBuilder()
        .UseServiceProviderFactory(new AutofacServiceProviderFactory())
        .ConfigureAppConfiguration(c => {
            c.SetBasePath(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory));
        })
        .ConfigureContainer<ContainerBuilder>(ConfigureContainer)
        .Build();

    static void ConfigureContainer(HostBuilderContext ctx, ContainerBuilder builder)
    {
        builder.RegisterModule(new ConfigurationModule(ctx.Configuration));

        builder.RegisterType<ApplicationHostService>().As<IHostedService>().SingleInstance();
        builder.RegisterType<PageService>().As<IPageService>().SingleInstance();
        builder.RegisterType<ThemeService>().As<IThemeService>().SingleInstance();
        builder.RegisterType<TaskBarService>().As<ITaskBarService>().SingleInstance();
        builder.RegisterType<NavigationService>().As<INavigationService>().SingleInstance();

        builder.RegisterType<MainWindow>().As<INavigationWindow>().InstancePerLifetimeScope();
        builder.RegisterType<MainWindowViewModel>().InstancePerLifetimeScope();
        builder.RegisterType<Views.Pages.DashboardPage>();
        builder.RegisterType<DashboardViewModel>();
        builder.RegisterType<Views.Pages.DataPage>();
        builder.RegisterType<DataViewModel>();
        builder.RegisterType<Views.Pages.SettingsPage>();
        builder.RegisterType<SettingsViewModel>();
    }

    /// <summary>
    /// Gets registered service.
    /// </summary>
    /// <typeparam name="T">Type of the service to get.</typeparam>
    /// <returns>Instance of the service or <see langword="null"/>.</returns>
    public static T? GetService<T>()
        where T : class
    {
        return _host.Services.GetService(typeof(T)) as T;
    }

    /// <summary>
    /// Occurs when the application is loading.
    /// </summary>
    async void OnStartup(object sender, StartupEventArgs e)
    {
        Logging.Initialize();
        AppCenterHelper.Initialize();
        await _host.StartAsync();
    }

    /// <summary>
    /// Occurs when the application is closing.
    /// </summary>
    async void OnExit(object sender, ExitEventArgs e)
    {
        await _host.StopAsync();

        _host.Dispose();
    }

    /// <summary>
    /// Occurs when an exception is thrown by an application but not handled.
    /// </summary>
    void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // TODO: handle exception
        // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
    }
}