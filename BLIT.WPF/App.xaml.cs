using BLIT.WPF.Helpers;
using BLIT.WPF.Services;
using Sentry.Protocol;
using Serilog;
using System.Windows;

namespace BLIT.WPF;

public partial class App : Application {
    public new static App Current => (App)Application.Current;
    //public new Window? MainWindow { get; private set; }
    public IServiceProvider? Services { get; private set; }

    public App() {
        // Initialize Sentry
        SentrySdk.Init(o => {
            o.Dsn = "https://4e3afdb2d984e01e08d30e685b29b725@o4510494475878400.ingest.us.sentry.io/4510747015643136";
            o.Debug = true;
            o.SendDefaultPii = true;
            o.IsGlobalModeEnabled = true;
        });

        DispatcherUnhandledException += OnUnhandledException;

        // Initialize logging and services
        ThemeHelper.OnAppStart();
        Logging.Initialize();
        Services = AppServices.Configure();
        Log.Information("BLIT.WPF started.");
    }

    private void OnUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
        Exception exception = e.Exception;
        if (exception != null) {
            exception.Data[Mechanism.HandledKey] = false;
            exception.Data[Mechanism.MechanismKey] = "Application.DispatcherUnhandledException";
            SentrySdk.CaptureException(exception);
            SentrySdk.FlushAsync(TimeSpan.FromSeconds(2)).GetAwaiter().GetResult();
        }

        // Prevent app crash for now (you may want to change this)
        e.Handled = true;
    }

    protected override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);
        //MainWindow = new MainWindow();
        ThemeHelper.Initialize(MainWindow);
        //MainWindow.Show();
    }
}
