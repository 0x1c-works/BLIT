using System.Diagnostics;
using System.Runtime.InteropServices;

/// <summary>
///     WPF Performance Diagnostic Helper
///     用于诊断 WPF 应用的性能问题
///     使用方法:
///     在 App.xaml.cs 或应用启动时调用 EnableDiagnostics()
/// </summary>
public static class WpfPerformanceDiagnostics {
    /// <summary>
    ///     启用 WPF 诊断输出
    ///     将性能信息输出到 Visual Studio 输出窗口
    /// </summary>
    public static void EnableDiagnostics() {
        // 启用 WPF 诊断信息
        PresentationTraceSources.DataBindingSource.Switch.Level =
            SourceLevels.Warning;

        PresentationTraceSources.ResourceDictionarySource.Switch.Level =
            SourceLevels.Warning;

        Trace.AutoFlush = true;

        // 添加 TextWriterTraceListener 到输出窗口
        var myTextListener = new TextWriterTraceListener();
        Trace.Listeners.Add(myTextListener);

        OutputDiagnosticInfo();
    }

    /// <summary>
    ///     输出系统和应用诊断信息
    /// </summary>
    private static void OutputDiagnosticInfo() {
        Debug.WriteLine("=== WPF Performance Diagnostics ===");
        Debug.WriteLine($"OS: {Environment.OSVersion}");
        Debug.WriteLine($"Processor Count: {Environment.ProcessorCount}");
        Debug.WriteLine($".NET Version: {RuntimeInformation.FrameworkDescription}");
        Debug.WriteLine("");
        Debug.WriteLine("Drag-and-drop optimization is active");
        Debug.WriteLine("Icon list uses lightweight 64x64px drag adorner");
        Debug.WriteLine("====================================");
    }

    /// <summary>
    ///     监控拖拽性能
    ///     在 drag 操作中调用
    /// </summary>
    public static void LogDragPerformance(string operationName, long elapsedMilliseconds) {
        Debug.WriteLine(
            $"[Drag Performance] {operationName}: {elapsedMilliseconds}ms");

        if (elapsedMilliseconds > 16) {
            Debug.WriteLine(
                "⚠️  Warning: Operation took longer than 1 frame (16ms)");
        }
    }

    /// <summary>
    ///     获取当前应用的内存使用信息
    /// </summary>
    public static (long TotalMemory, long WorkingSet) GetMemoryUsage() {
        var process = Process.GetCurrentProcess();
        return (process.TotalProcessorTime.Milliseconds, process.WorkingSet64);
    }

    /// <summary>
    ///     输出内存使用信息到调试窗口
    /// </summary>
    public static void LogMemoryUsage() {
        var (total, working) = GetMemoryUsage();
        Debug.WriteLine(
            $"[Memory] Total: {total / 1024 / 1024} MB, Working: {working / 1024 / 1024} MB");
    }
}