using Godot;
using Serilog;
using System;
using System.IO;

namespace BLIT.scripts.Common;
public class AppConfig
{
    static readonly string _configPath = FileSystemHelper.GetLocalDataPath("user.config");
    public static ConfigFile Current { get; } = new();
    public static event Action<ConfigFile>? Loaded;

    public static void Load()
    {
        if (File.Exists(_configPath))
        {
            Error err = Current.Load(_configPath);
            if (err != Error.Ok)
            {
                Log.Error($"Failed to load app config file '{_configPath}': {err}");
            }
            else
            {
                Log.Information("App config saved.");
                Loaded?.Invoke(Current);
            }
        }
    }
    public static void Save()
    {
        Error err = Current.Save(_configPath);
        if (err != Error.Ok)
        {
            Log.Error($"Failed to save app config file '{_configPath}': {err}");
        }
        else
        {
            Log.Information("App config saved.");
        }
    }
}
