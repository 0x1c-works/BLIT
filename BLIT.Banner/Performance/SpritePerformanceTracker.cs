using BLIT.Utils.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace BLIT.Banner.Performance;

/// <summary>
/// Tracks performance metrics for sprite collection and generation
/// Thread-safe implementation for parallel icon processing
/// </summary>
public class SpritePerformanceTracker {
    private readonly ILogger _logger;
    private readonly string _outDir;
    private readonly ConcurrentDictionary<string, IconProcessingResult> _results;
    private readonly Stopwatch _overallStopwatch;
    private int _totalCount = 0;
    private int _processedCount = 0;
    private readonly object _countLock = new();

    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }

    public SpritePerformanceTracker(ILogger logger, string outDir) {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _outDir = outDir;
        _results = new ConcurrentDictionary<string, IconProcessingResult>();
        _overallStopwatch = Stopwatch.StartNew();
        StartTime = DateTime.Now;

        _logger.Information("=== Sprite Collection Performance Tracking Started ===");
    }

    public void SetTotalCount(int count) {
        _totalCount = count;
        _logger.Information($"Total icons to process: {count}");
    }

    public void StartIcon(string iconId, int groupId) {
        var result = new IconProcessingResult {
            IconID = iconId,
            GroupID = groupId,
            StartTime = DateTime.Now,
            Success = true
        };
        _results.TryAdd(iconId, result);
    }

    public void RecordStep(string iconId, string stepName, long elapsedMs) {
        if (_results.TryGetValue(iconId, out var result)) {
            result.StepTimings.Add(new StepTiming {
                StepName = stepName,
                ElapsedMilliseconds = elapsedMs
            });
            
            // Update total time
            result.TotalMilliseconds += elapsedMs;
        }
    }

    public void CompleteIcon(string iconId, bool success, string? errorMessage = null) {
        if (_results.TryGetValue(iconId, out var result)) {
            result.Success = success;
            result.ErrorMessage = errorMessage;
            result.EndTime = DateTime.Now;
            
            // If TotalMilliseconds wasn't set by steps, calculate from start/end time
            if (result.TotalMilliseconds == 0) {
                result.TotalMilliseconds = (long)(result.EndTime - result.StartTime).TotalMilliseconds;
            }

            // Increment processed count and log
            lock (_countLock) {
                _processedCount++;
            }

            if (success) {
                var stepsInfo = result.StepTimings.Count > 0
                    ? $"\n    Steps: {string.Join(", ", result.StepTimings.Select(s => $"{s.StepName}: {s.ElapsedMilliseconds}ms"))}"
                    : "";
                _logger.Information($"[{_processedCount}/{_totalCount}] ✓ Icon {iconId} (GroupID: {result.GroupID}) - {result.TotalMilliseconds}ms{stepsInfo}");
            } else {
                _logger.Error($"[{_processedCount}/{_totalCount}] ✗ Icon {iconId} (GroupID: {result.GroupID}) - FAILED: {errorMessage}");
            }
        }
    }

    public void PrintFinalReport() {
        _overallStopwatch.Stop();
        EndTime = DateTime.Now;

        var reportText = GenerateReportText();
        _logger.Information(reportText);
    }

    public void SaveReportToFile(string logDir) {
        _overallStopwatch.Stop();
        EndTime = DateTime.Now;

        var reportText = GenerateReportText();
        
        // Ensure log directory exists
        Directory.CreateDirectory(logDir);

        // Generate filename with timestamp
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var reportPath = Path.Combine(logDir, $"performance_report_{timestamp}.txt");

        try {
            File.WriteAllText(reportPath, reportText);
            _logger.Information($"Performance report saved to: {reportPath}");
        } catch (Exception ex) {
            _logger.Error(ex, $"Failed to save performance report to {reportPath}");
        }
    }

    private string GenerateReportText() {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("╔═══════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║        Sprite Export Performance Report                       ║");
        sb.AppendLine("╚═══════════════════════════════════════════════════════════════╝");
        sb.AppendLine();

        // Overall timing
        var totalSeconds = _overallStopwatch.Elapsed.TotalSeconds;
        sb.AppendLine($"Start Time: {StartTime:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"End Time: {EndTime:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Total Duration: {_overallStopwatch.ElapsedMilliseconds}ms ({totalSeconds:F2}s)");
        sb.AppendLine();

        // Summary statistics
        var successCount = _results.Count(r => r.Value.Success);
        var failureCount = _results.Count(r => !r.Value.Success);
        var averageTimeMs = successCount > 0 
            ? _results.Where(r => r.Value.Success).Average(r => r.Value.TotalMilliseconds) 
            : 0;

        sb.AppendLine("─── Summary ───");
        sb.AppendLine($"✓ Successful: {successCount} icons");
        sb.AppendLine($"✗ Failed: {failureCount} icons");
        sb.AppendLine($"Average Time per Icon: {averageTimeMs:F2}ms");
        sb.AppendLine();

        // Top slowest icons
        var slowestIcons = GetTopSlowest(5);
        if (slowestIcons.Count > 0) {
            sb.AppendLine("─── Top 5 Slowest Icons ───");
            for (int i = 0; i < slowestIcons.Count; i++) {
                var icon = slowestIcons[i];
                sb.AppendLine($"{i + 1}. Icon {icon.IconID} (GroupID: {icon.GroupID}) - {icon.TotalMilliseconds}ms");
            }
            sb.AppendLine();
        }

        // Time distribution by step
        var stepDistribution = CalculateStepDistribution();
        if (stepDistribution.Count > 0) {
            sb.AppendLine("─── Time Distribution by Step ───");
            var totalTime = stepDistribution.Values.Sum();
            foreach (var step in stepDistribution.OrderByDescending(s => s.Value)) {
                var percentage = totalTime > 0 ? (step.Value * 100.0 / totalTime) : 0;
                var barLength = (int)(percentage / 5);
                var bar = new string('█', barLength);
                sb.AppendLine($"{step.Key}: {step.Value}ms ({percentage:F1}%) {bar}");
            }
            sb.AppendLine();
        }

        // Failed icons list
        if (failureCount > 0) {
            sb.AppendLine("─── Failed Icons ───");
            var failedIcons = _results.Where(r => !r.Value.Success).OrderBy(r => r.Key);
            foreach (var icon in failedIcons) {
                sb.AppendLine($"[✗] Icon {icon.Key} (GroupID: {icon.Value.GroupID}): {icon.Value.ErrorMessage}");
            }
            sb.AppendLine();
        }

        // Detailed icon processing list (all icons with detailed steps)
        sb.AppendLine("─── Detailed Processing Information ───");
        foreach (var icon in _results.OrderBy(r => r.Key)) {
            var result = icon.Value;
            if (result.Success) {
                sb.AppendLine($"Icon {result.IconID} (GroupID: {result.GroupID}): {result.TotalMilliseconds}ms");
                if (result.StepTimings.Count > 0) {
                    foreach (var step in result.StepTimings) {
                        sb.AppendLine($"  ├─ {step.StepName}: {step.ElapsedMilliseconds}ms");
                    }
                }
            } else {
                sb.AppendLine($"Icon {result.IconID} (GroupID: {result.GroupID}): FAILED - {result.ErrorMessage}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("═══════════════════════════════════════════════════════════════");

        return sb.ToString();
    }

    private List<IconProcessingResult> GetTopSlowest(int count) {
        return _results
            .Where(r => r.Value.Success)
            .OrderByDescending(r => r.Value.TotalMilliseconds)
            .Take(count)
            .Select(r => r.Value)
            .ToList();
    }

    private Dictionary<string, long> CalculateStepDistribution() {
        var distribution = new Dictionary<string, long>();
        
        foreach (var result in _results.Values) {
            foreach (var step in result.StepTimings) {
                if (distribution.ContainsKey(step.StepName)) {
                    distribution[step.StepName] += step.ElapsedMilliseconds;
                } else {
                    distribution[step.StepName] = step.ElapsedMilliseconds;
                }
            }
        }

        return distribution;
    }
}
