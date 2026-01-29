namespace BLIT.Banner.Progress;

/// <summary>
///     Represents export progress data across all export stages
///     Used to track progress uniformly across texture, sprite, and XML generation stages
/// </summary>
public struct ExportProgressData {
    /// <summary>
    ///     Number of items already processed
    /// </summary>
    public int ProcessedCount { get; set; }

    /// <summary>
    ///     Total number of items to process
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    ///     Current export stage name for display
    /// </summary>
    public string CurrentStage { get; set; }

    /// <summary>
    ///     Calculates the progress percentage (0-100)
    /// </summary>
    public double Percentage => TotalCount > 0 ? ProcessedCount * 100.0 / TotalCount : 0;

    public ExportProgressData() {
        ProcessedCount = 0;
        TotalCount = 0;
        CurrentStage = "Exporting...";
    }

    public ExportProgressData(int processed, int total, string stage = "Exporting...") {
        ProcessedCount = processed;
        TotalCount = total;
        CurrentStage = stage;
    }
}