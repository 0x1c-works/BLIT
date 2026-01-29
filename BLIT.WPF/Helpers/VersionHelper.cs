using System.Reflection;

namespace BLIT.WPF.Helpers;

/// <summary>
///     版本号辅助类，用于获取应用程序版本信息
/// </summary>
public static class VersionHelper {
    /// <summary>
    ///     获取应用程序的完整版本号（4段式：Major.Minor.Patch.Build）
    ///     例如：1.0.0.1
    /// </summary>
    public static string GetFullVersion() {
        try {
            var assembly = Assembly.GetExecutingAssembly();
            Version? version = assembly.GetName().Version;

            return version?.ToString() ?? "Unknown";
        } catch {
            return "Unknown";
        }
    }
}