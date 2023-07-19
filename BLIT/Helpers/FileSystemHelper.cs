using System;
using System.IO;

namespace BLIT.Helpers;
static class FileSystemHelper
{
    public static readonly string DataFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BLIT");
    public static readonly string LogsFolderPath = Path.Combine(DataFolderPath, "logs");
}
