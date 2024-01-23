using Serilog;
using System.IO;

namespace BLIT.scripts.Common;
public class Logging {
    public static string Folder => FileSystemHelper.GetLocalDataPath("logs");
    public static void Initialize() {

        var logPath = Path.Combine(Folder, "log-.txt");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
            .WriteTo.Godot()
            .CreateLogger();
    }
}

