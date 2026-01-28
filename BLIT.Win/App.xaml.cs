// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BLIT.Win.Helpers;
using BLIT.Win.Services;
using Microsoft.UI.Xaml;
using Microsoft.Windows.ApplicationModel.Resources;
using Sentry;
using Sentry.Protocol;
using Serilog;
using System;
using System.Security;
using UnhandledExceptionEventArgs = Microsoft.UI.Xaml.UnhandledExceptionEventArgs;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BLIT.Win;

/// <summary>
///     Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application {
    /// <summary>
    ///     Initializes the singleton application object.  This is the first line of authored code
    ///     executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App() {
        SentrySdk.Init(o => {
            // Tells which project in Sentry to send events to:
            o.Dsn = "https://4e3afdb2d984e01e08d30e685b29b725@o4510494475878400.ingest.us.sentry.io/4510747015643136";
            // When configuring for the first time, to see what the SDK is doing:
            o.Debug = true;
            // Adds request URL and headers, IP and name for users, etc.
            o.SendDefaultPii = true;
            // Enable Global Mode since this is a client app.
            o.IsGlobalModeEnabled = true;
            // Disable Sentry's built in UnhandledException handler as this won't work with AOT compilation
            //o.DisableWinUiUnhandledExceptionIntegration();
            // TODO:Any other Sentry options you need go here.
        });
        UnhandledException += OnUnhandledException;

        InitializeComponent();
        ThemeHelper.OnAppStart();
        Logging.Initialize();
        Services = AppServices.Configure();
        Log.Information("BLIT started.");
    }

    public static new App Current => Application.Current as App;
    public I18n I18n { get; } = new(new ResourceLoader(), new ResourceManager());
    public Window MainWindow { get; private set; }
    public IServiceProvider Services { get; }

    // Use this attribute to ensure all types of exceptions are handled.
    [SecurityCritical]
    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) {
        // Get a reference to the exception, because the Exception property is cleared when accessed.
        Exception exception = e.Exception;
        if (exception != null) {
            // Tell Sentry this was an unhandled exception
            exception.Data[Mechanism.HandledKey] = false;
            exception.Data[Mechanism.MechanismKey] = "Application.UnhandledException";
            // Capture the exception
            SentrySdk.CaptureException(exception);
            // Flush the event immediately
            SentrySdk.FlushAsync(TimeSpan.FromSeconds(2)).GetAwaiter().GetResult();
        }
    }

    /// <summary>
    ///     Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args) {
        MainWindow = new MainWindow();
        ThemeHelper.Initialize();
        MainWindow.Activate();
    }
}