using System;
using System.IO;

namespace BLIT.scripts.Common;
public static class FileSystemHelper {
    public static string LocalData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BLIT");
    public static string GetLocalDataPath(string path) {
        return Path.Combine(LocalData, path);
    }
}
