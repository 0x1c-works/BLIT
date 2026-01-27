namespace BLIT.Banner.Performance;

/// <summary>
/// Represents the timing of a single processing step
/// </summary>
public class StepTiming {
    public string StepName { get; set; } = string.Empty;
    public long ElapsedMilliseconds { get; set; }
}

/// <summary>
/// Represents the processing result of a single icon
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
