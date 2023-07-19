using Serilog;
using Serilog.Sink.AppCenter;
using System.IO;

namespace BLIT.Helpers;

static class Logging
{
    public static void Initialize()
    {

        var logPath = Path.Combine(FileSystemHelper.LogsFolderPath, "log-.txt");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
            .WriteTo.AppCenterSink(target: AppCenterTarget.ExceptionsAsCrashesAndEvents, appCenterSecret: AppCenterHelper.Secret)
            .CreateLogger();
    }
}
