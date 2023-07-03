// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BannerlordImageTool.Win.Helpers;
using BannerlordImageTool.Win.Services;
using BannerlordImageTool.Win.Settings;
using Microsoft.UI.Xaml;
using Microsoft.Windows.ApplicationModel.Resources;
using Serilog;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BannerlordImageTool.Win
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static new App Current { get => Application.Current as App; }
        public GlobalSettings Settings { get; } = new GlobalSettings();
        public I18n I18n { get; } = new I18n(new ResourceLoader(), new ResourceManager());
        public Window MainWindow { get => m_window; }
        public IServiceProvider Services { get; }

        public ViewModels.BannerIcons.DataViewModel BannerViewModel { get; set; } = new();

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            Logging.Initialize();
            Services = AppServices.Configure();

            Log.Information("Bannerlord Image Tool started.");
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }

        private Window m_window;
    }
}
