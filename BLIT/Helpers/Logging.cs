using BLIT.Helpers;
using Serilog;
using Serilog.Sink.AppCenter;
using System.IO;

namespace BLIT.Win.Helpers;

public class Logging
{
    public static void Initialize()
    {

        var logPath = Path.Combine(FileSystemHelper.AppLogPath, "log-.txt");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
            .WriteTo.AppCenterSink(target: AppCenterTarget.ExceptionsAsCrashesAndEvents, appCenterSecret: AppCenterHelper.Secret)
            .CreateLogger();
    }
}
