using BLIT.Utils.Logging;

namespace BLIT.WPF.Helpers;

/// <summary>
///     Adapter that wraps Serilog.ILogger to implement BLIT.Utils.Logging.ILogger
///     Allows Serilog configuration in WPF to be used by BLIT.Banner
/// </summary>
public class SerilogLoggerAdapter : ILogger {
    private readonly Serilog.ILogger _logger;

    public SerilogLoggerAdapter(Serilog.ILogger logger) {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region ILogger Members

    public void Debug(string message) {
        _logger.Debug(message);
    }

    public void Information(string message) {
        _logger.Information(message);
    }

    public void Warning(string message) {
        _logger.Warning(message);
    }

    public void Error(string message) {
        _logger.Error(message);
    }

    public void Error(Exception ex, string message) {
        _logger.Error(ex, message);
    }

    #endregion
}