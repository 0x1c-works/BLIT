using ImageMagick;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Xml;
using BLIT.Banner.Performance;
using BLIT.Utils.Logging;

namespace BLIT.Banner;

public record IconSprite(int GroupID, int IconID, string RawPath, bool AlwaysLoad = true);

public class SpriteOrganizer {
    const int MaxConcurrency = 10;
    private static readonly string SPRITE_SUB_FOLDER = Path.Join("GUI", "SpriteParts");

    public static async Task CollectToSpriteParts(
        string outDir, 
        IEnumerable<IconSprite> icons,
        ILogger? logger = null) {
        
        if (!string.IsNullOrEmpty(outDir)) {
            outDir = Directory.CreateDirectory(outDir).FullName;
        }

        // Initialize performance tracker
        var tracker = new SpritePerformanceTracker(logger ?? new NullLogger(), outDir) {
            IsEnabled = false,
        };
        tracker.Start();
        var iconList = icons.ToList();
        tracker.SetTotalCount(iconList.Count);

        // Process all icons in parallel with concurrency control
        var semaphore = new SemaphoreSlim(MaxConcurrency, MaxConcurrency);
        
        var tasks = iconList.Select(async icon => {
            var iconId = $"{icon.GroupID}_{icon.IconID}";
            tracker.StartIcon(iconId, icon.GroupID);
            
            await semaphore.WaitAsync();
            try {
                await ResizeAndSave(outDir, icon, tracker);
                tracker.CompleteIcon(iconId, true);
            } catch (Exception ex) {
                var errorMessage = ex.InnerException?.Message ?? ex.Message ?? "Unknown error";
                tracker.CompleteIcon(iconId, false, errorMessage);
            } finally {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
        
        // Generate config XML with performance tracking
        GenerateConfigXML(outDir, icons, tracker);
        
        // Print and save final report
        tracker.PrintFinalReport();
        tracker.SaveReportToFile(GetLogDirectory());
    }

    public static void GenerateConfigXML(string outDir, IEnumerable<IconSprite> icons, SpritePerformanceTracker? tracker = null) {
        var stopwatch = Stopwatch.StartNew();
        
        var doc = new XmlDocument();
        XmlElement root = doc.CreateElement("Config");
        doc.AppendChild(root);
        foreach (IconSprite? icon in icons.Where(icon => icon.AlwaysLoad).DistinctBy(icon => icon.GroupID)) {
            XmlElement node = doc.CreateElement("SpriteCategory");
            node.SetAttribute("Name", GetAtlasID(icon.GroupID));
            node.AppendChild(doc.CreateElement("AlwaysLoad"));
            root.AppendChild(node);
        }
        
        var configPath = Path.Join(EnsureSpriteFolder(outDir), "Config.xml");
        using var writer = XmlWriter.Create(
            configPath,
            new XmlWriterSettings() {
                Encoding = Encoding.UTF8,
                Indent = true,
            });
        doc.WriteTo(writer);
        
        stopwatch.Stop();
        tracker?.RecordStep("CONFIG_XML", "Generate Config XML", stopwatch.ElapsedMilliseconds);
    }

    private static string EnsureSpriteFolder(string outDir) {
        var dir = Path.Join(outDir, SPRITE_SUB_FOLDER);
        return Directory.CreateDirectory(dir).FullName;
    }

    private static string EnsureGroupFolder(string outDir, int groupID) {
        var dir = Path.Join(EnsureSpriteFolder(outDir), GetAtlasID(groupID));
        return Directory.CreateDirectory(dir).FullName;
    }


    private static async Task ResizeAndSave(string outDir, IconSprite icon, SpritePerformanceTracker tracker) {
        (var groupID, var iconID, var filePath, var _) = icon;
        var iconId = $"{groupID}_{iconID}";
        
        // Run ImageMagick operations on thread pool to avoid deadlocks
        await Task.Run(() => {
            var overallStopwatch = Stopwatch.StartNew();
            
            // Ensure folder exists and track time
            var folderStopwatch = Stopwatch.StartNew();
            var outPath = Path.Join(EnsureGroupFolder(outDir, groupID), $"{iconID}.png");
            folderStopwatch.Stop();
            tracker.RecordStep(iconId, "Ensure folder", folderStopwatch.ElapsedMilliseconds);

            // Load image
            var loadStopwatch = Stopwatch.StartNew();
            using var img = new MagickImage(filePath);
            loadStopwatch.Stop();
            tracker.RecordStep(iconId, "Load image", loadStopwatch.ElapsedMilliseconds);

            // Check if resize is needed
            var checkStopwatch = Stopwatch.StartNew();
            bool needsResize = img.Width != 512 || img.Height != 512;
            checkStopwatch.Stop();
            tracker.RecordStep(iconId, "Check resize needed", checkStopwatch.ElapsedMilliseconds);

            // Resize if needed
            if (needsResize) {
                var resizeStopwatch = Stopwatch.StartNew();
                img.Resize(new MagickGeometry(512));
                resizeStopwatch.Stop();
                tracker.RecordStep(iconId, $"Resize to 512x512", resizeStopwatch.ElapsedMilliseconds);
            }

            // Save file
            var saveStopwatch = Stopwatch.StartNew();
            img.Write(outPath);
            saveStopwatch.Stop();
            tracker.RecordStep(iconId, "Save file", saveStopwatch.ElapsedMilliseconds);

            overallStopwatch.Stop();
        });
    }

    private static string GetAtlasID(int groupID) {
        return $"ui_{groupID}";
    }

    private static string GetLogDirectory() {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BLIT.WPF",
            "logs"
        );
    }
}

/// <summary>
/// Null logger implementation for when no logger is provided
/// </summary>
internal class NullLogger : ILogger {
    public void Debug(string message) { }
    public void Information(string message) { }
    public void Warning(string message) { }
    public void Error(string message) { }
    public void Error(Exception ex, string message) { }
}
