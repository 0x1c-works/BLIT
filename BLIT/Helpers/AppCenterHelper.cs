using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Serilog;

namespace BLIT.Helpers;
static class AppCenterHelper
{
    public const string Secret = "##TO BE REPLACED AT BUILD TIME##";

    public static void Initialize()
    {
        if (AppCenter.Configured)
        {
            AppCenter.Start(typeof(Analytics));
            AppCenter.Start(typeof(Crashes));
            Log.Information("App center initialized.");
        }
        else
        {
            Log.Error("App center is not configured for initialization.");
        }
    }
}
