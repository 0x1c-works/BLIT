using System;

namespace BLIT.Win.Helpers;
class AppCenterHelper
{
    const string APP_CENTER_SECRET = "BLIT_APP_CENTER_SECRET";
    public static string Secret
    {
        get => Environment.GetEnvironmentVariable(APP_CENTER_SECRET, EnvironmentVariableTarget.User);
    }
}
