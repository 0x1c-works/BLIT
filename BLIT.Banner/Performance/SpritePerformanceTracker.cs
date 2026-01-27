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
    private Stopwatch? _overallStopwatch;
    private int _totalCount = 0;
    private int _processedCount = 0;
    private readonly object _countLock = new();

    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public bool IsEnabled { get; set; }

    public SpritePerformanceTracker(ILogger logger, string outDir) {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _outDir = outDir;
        _results = new ConcurrentDictionary<string, IconProcessingResult>();
    }

    public void Start() {
        if (!IsEnabled) return;
        StartTime = DateTime.Now;
        _overallStopwatch = Stopwatch.StartNew();
        _logger.Information("=== Sprite Collection Performance Tracking Started ===");
    }

    public void SetTotalCount(int count) {
        if (!IsEnabled) return;
        _totalCount = count;
        _logger.Information($"Total icons to process: {count}");
    }

    public void StartIcon(string iconId, int groupId) {
        if (!IsEnabled) return;
        var result = new IconProcessingResult {
            IconID = iconId, GroupID = groupId, StartTime = DateTime.Now, Success = true
        };
        _results.TryAdd(iconId, result);
    }

    public void RecordStep(string iconId, string stepName, long elapsedMs) {
        if (!IsEnabled) return;
        if (_results.TryGetValue(iconId, out var result)) {
            result.StepTimings.Add(new StepTiming { StepName = stepName, ElapsedMilliseconds = elapsedMs });

            // Update total time
            result.TotalMilliseconds += elapsedMs;
        }
    }

    public void CompleteIcon(string iconId, bool success, string? errorMessage = null) {
        if (!IsEnabled) return;
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

                if (success) {
                    var stepsInfo = result.StepTimings.Count > 0
                        ? $"\n    Steps: {string.Join(", ", result.StepTimings.Select(s => $"{s.StepName}: {s.ElapsedMilliseconds}ms"))}"
                        : "";
                    _logger.Information(
                        $"[{_processedCount}/{_totalCount}] ✓ Icon {iconId} (GroupID: {result.GroupID}) - {result.TotalMilliseconds}ms{stepsInfo}");
                } else {
                    _logger.Error(
                        $"[{_processedCount}/{_totalCount}] ✗ Icon {iconId} (GroupID: {result.GroupID}) - FAILED: {errorMessage}");
                }
            }
        }
    }

    public void PrintFinalReport() {
        if (!IsEnabled) return;
        _overallStopwatch?.Stop();
        EndTime = DateTime.Now;

        var reportText = GenerateReportText();
        _logger.Information(reportText);
    }

    public void SaveReportToFile(string logDir) {
        if (!IsEnabled) return;
        _overallStopwatch?.Stop();
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

        // Time distribution by step with advanced statistics
        var stepStatistics = CalculateStepStatistics();
        if (stepStatistics.Count > 0) {
            sb.AppendLine("─── Step Statistics (Advanced) ───");
            sb.AppendLine("Format: Step | Count | Total | Avg | Median | Min | Max | P95 | P99");
            sb.AppendLine();

            var totalTime = stepStatistics.Values.Sum(s => s.TotalMs);
            foreach (var stat in stepStatistics.Values.OrderByDescending(s => s.TotalMs)) {
                var percentage = totalTime > 0 ? (stat.TotalMs * 100.0 / totalTime) : 0;
                sb.AppendLine($"{stat.StepName}");
                sb.AppendLine($"  Count: {stat.Count} | Total: {stat.TotalMs}ms ({percentage:F1}%)");
                sb.AppendLine(
                    $"  Avg: {stat.AverageMs}ms | Median: {stat.MedianMs}ms | Min: {stat.MinMs}ms | Max: {stat.MaxMs}ms");
                sb.AppendLine($"  P95: {stat.P95Ms}ms | P99: {stat.P99Ms}ms");
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

    /// <summary>
    /// Calculates comprehensive statistics for each processing step
    /// Including count, totals, average, median, min, max, and percentiles (P95, P99)
    /// </summary>
    private Dictionary<string, StepStatistics> CalculateStepStatistics() {
        var stepData = new Dictionary<string, List<long>>();

        // Collect all measurements per step
        foreach (var result in _results.Values) {
            foreach (var step in result.StepTimings) {
                if (!stepData.ContainsKey(step.StepName)) {
                    stepData[step.StepName] = new List<long>();
                }

                stepData[step.StepName].Add(step.ElapsedMilliseconds);
            }
        }

        var statistics = new Dictionary<string, StepStatistics>();

        foreach (var stepName in stepData.Keys) {
            var measurements = stepData[stepName];
            measurements.Sort(); // Sort for percentile calculations

            var stats = new StepStatistics {
                StepName = stepName,
                Count = measurements.Count,
                TotalMs = measurements.Sum(),
                AverageMs = (long)Math.Round(measurements.Average()),
                MedianMs = GetPercentile(measurements, 50),
                MinMs = measurements.First(),
                MaxMs = measurements.Last(),
                P95Ms = GetPercentile(measurements, 95),
                P99Ms = GetPercentile(measurements, 99)
            };

            statistics[stepName] = stats;
        }

        return statistics;
    }

    /// <summary>
    /// Calculates the percentile value from a sorted list of measurements
    /// </summary>
    private long GetPercentile(List<long> sortedMeasurements, int percentile) {
        if (sortedMeasurements.Count == 0) return 0;
        if (percentile <= 0) return sortedMeasurements.First();
        if (percentile >= 100) return sortedMeasurements.Last();

        // Linear interpolation method
        double index = (percentile / 100.0) * (sortedMeasurements.Count - 1);
        int lowerIndex = (int)Math.Floor(index);
        int upperIndex = (int)Math.Ceiling(index);

        if (lowerIndex == upperIndex) {
            return sortedMeasurements[lowerIndex];
        }

        double lowerValue = sortedMeasurements[lowerIndex];
        double upperValue = sortedMeasurements[upperIndex];
        double fraction = index - lowerIndex;

        return (long)Math.Round(lowerValue + (upperValue - lowerValue) * fraction);
    }
}