using Serilog;
using System.IO;
using Windows.Storage;

namespace BLIT.Win.Helpers;

public class Logging
{
    public static string Folder => Path.Combine(ApplicationData.Current.LocalFolder.Path, "logs");
    public static void Initialize()
    {

        var logPath = Path.Combine(Folder, "log-.txt");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.Debug()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }
}
