// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BLIT.Win.Helpers;
using BLIT.Win.Services;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.UI.Xaml;
using Microsoft.Windows.ApplicationModel.Resources;
using Serilog;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BLIT.Win;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application {
    public static new App Current => Application.Current as App;
    public I18n I18n { get; } = new I18n(new ResourceLoader(), new ResourceManager());
    public Window MainWindow { get; private set; }
    public IServiceProvider Services { get; }

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App() {
        InitializeComponent();
        ThemeHelper.OnAppStart();
        Logging.Initialize();
        Services = AppServices.Configure();
        Log.Information("BLIT started.");

        if (AppCenter.Configured) {
            AppCenter.Start(typeof(Analytics));
            AppCenter.Start(typeof(Crashes));
            Log.Information("App center integrated.");
        }
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args) {
        MainWindow = new MainWindow();
        ThemeHelper.Initialize();
        MainWindow.Activate();
    }
}
