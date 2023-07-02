using Serilog;
using System.IO;
using Windows.Storage;

namespace BannerlordImageTool.Win.Helpers;

public class Logging
{
    public static string Folder
    {
        get => Path.Combine(ApplicationData.Current.LocalFolder.Path, "logs");
    }
    public static void Initialize()
    {

        var logPath = Path.Combine(Folder, "log-.txt");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }
}
