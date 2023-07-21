using System;
using System.Diagnostics;
using System.IO;

namespace BLIT.Helpers;

static class FileSystemHelper
{
    public static string ExePath { get; } = AppDomain.CurrentDomain.BaseDirectory;
    public static string LocalAppDataPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BLIT");
    public static string AppConfigPath { get; } = EnsureDirectory(Path.Combine(ExePath, "config"));
    public static string AppLogPath { get; } = EnsureDirectory(Path.Combine(LocalAppDataPath, "logs"));

    static bool IsDirectory(string path)
    {
        return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
    }
    static string? GetDirectory(string path)
    {
        return IsDirectory(path) ? path : Path.GetDirectoryName(path);
    }
    public static void OpenFolderInExplorer(string path)
    {
        path = GetDirectory(path) ?? "";
        if (string.IsNullOrEmpty(path)) return;
        Process.Start("explorer.exe", path);
    }
    public static string EnsureDirectory(string path)
    {
        path = Directory.CreateDirectory(path).FullName;
        return path;
    }
}
