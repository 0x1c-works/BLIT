using Serilog;
using Serilog.Events;
using System.IO;
using Windows.Storage;

namespace BLIT.Win.Helpers;

public class Logging {
    public static string Folder => Path.Combine(ApplicationData.Current.LocalFolder.Path, "logs");
    public static void Initialize() {

        var logPath = Path.Combine(Folder, "log-.txt");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
            .WriteTo.Sentry(o => {
                o.Dsn = "https://4e3afdb2d984e01e08d30e685b29b725@o4510494475878400.ingest.us.sentry.io/4510747015643136";
                // Enable logs to be sent to Sentry
                o.EnableLogs = true;
                // Debug and higher are stored as breadcrumbs (default is Information)
                o.MinimumBreadcrumbLevel = LogEventLevel.Debug;
                // Warning and higher is sent as event (default is Error)
                o.MinimumEventLevel = LogEventLevel.Warning;
            })
            .CreateLogger();
    }
}
