namespace BLIT.Utils.Logging;

/// <summary>
/// Logger interface for shared utilities.
/// Implementations can be provided by different logging frameworks (Serilog, Console, etc.)
/// </summary>
public interface ILogger {
    void Debug(string message);
    void Information(string message);
    void Warning(string message);
    void Error(string message);
    void Error(Exception ex, string message);
}
