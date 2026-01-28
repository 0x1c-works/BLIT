namespace BLIT.Banner.Performance;

/// <summary>
///     Represents the timing of a single processing step
/// </summary>
public class StepTiming {
    public string StepName { get; set; } = string.Empty;
    public long ElapsedMilliseconds { get; set; }
}

/// <summary>
///     Represents comprehensive statistics for a processing step
///     Includes count, totals, and percentile metrics (avg, median, P95, P99)
/// </summary>
public class StepStatistics {
    public string StepName { get; set; } = string.Empty;
    public long Count { get; set; } // Number of times step was executed
    public long TotalMs { get; set; } // Total time across all executions
    public long AverageMs { get; set; } // Average time (Mean)
    public long MedianMs { get; set; } // Median time (50th percentile)
    public long MinMs { get; set; } // Minimum time
    public long MaxMs { get; set; } // Maximum time
    public long P95Ms { get; set; } // 95th percentile (shows anomalies)
    public long P99Ms { get; set; } // 99th percentile (shows extreme anomalies)
}

/// <summary>
///     Represents the processing result of a single icon
/// </summary>
public class IconProcessingResult {
    public string IconID { get; set; } = string.Empty;
    public int GroupID { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<StepTiming> StepTimings { get; set; } = new();
    public long TotalMilliseconds { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public override string ToString() {
        if (!Success) {
            return $"Icon {IconID} (GroupID: {GroupID}) - FAILED: {ErrorMessage}";
        }

        var stepsText = string.Join(", ", StepTimings.Select(s => $"{s.StepName}: {s.ElapsedMilliseconds}ms"));
        return $"Icon {IconID} (GroupID: {GroupID}) - {TotalMilliseconds}ms [{stepsText}]";
    }
}