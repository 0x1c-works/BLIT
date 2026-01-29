using BLIT.Banner;
using BLIT.Banner.Progress;
using BLIT.WPF.Helpers;
using BLIT.WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using MessagePack;
using Serilog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace BLIT.WPF.Pages.BannerIcons.Models;

public partial class BannerIconsProject(
    ISettingsService settings,
    BannerGroupEntry.Factory bannerGroupFactory,
    BannerColorEntry.Factory colorFactory)
    : ObservableObject, IProject {
    /// <summary>
    ///     0 - 6 is occpuied by the native game
    /// </summary>
    private const int MinGroupId = 7;

    /// <summary>
    ///     0-193 is occupied by the native game
    /// </summary>
    private const int MinColorId = 194;


    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanExport))]
    private bool _isExporting;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanExport))]
    private bool _isSavingOrLoading;

    public ObservableCollection<BannerGroupEntry> Groups { get; } = new();
    public ObservableCollection<BannerColorEntry> Colors { get; } = new();
    public string? CurrentFilePath { get; set; }

    public string OutputResolutionName {
        get =>
            settings.Banner.TextureOutputResolution switch {
                OutputResolution.Res2K => "2K",
                OutputResolution.Res4K => "4K",
                _ => I18n.Current.GetString("PleaseSelect")
            };
        set {
            settings.Banner.TextureOutputResolution = Enum.TryParse(value, out OutputResolution enumValue)
                ? enumValue
                : OutputResolution.INVALID;
            OnPropertyChanged();
        }
    }

    public bool CanExport => !IsExporting && !IsSavingOrLoading && (Groups.Any(g => g.CanExport) || Colors.Count > 0);

    #region IProject Members

    public async Task Write(Stream s) {
        try {
            IsSavingOrLoading = true;
            await MessagePackSerializer.SerializeAsync(s, new SaveData(this));
        } catch (Exception ex) { Log.Error(ex, "error in saving the banner project"); } finally {
            IsSavingOrLoading = false;
        }
    }

    public async Task Read(Stream s) {
        try {
            IsSavingOrLoading = true;
            var data = await MessagePackSerializer.DeserializeAsync<SaveData>(s);
            Groups.Clear();
            Colors.Clear();
            foreach (BannerGroupEntry.SaveData groupData in data.Groups) {
                Groups.Add(groupData.Load(bannerGroupFactory));
            }

            foreach (BannerColorEntry.SaveData colorData in data.Colors) {
                Colors.Add(colorData.Load(colorFactory));
            }
        } catch (Exception ex) { Log.Error(ex, "error in loading the banner project"); } finally {
            IsSavingOrLoading = false;
        }
    }

    public void AfterLoaded() {
        OnPropertyChanged(nameof(CanExport));
    }

    #endregion

    public BannerIconData ToBannerIconData() {
        var data = new BannerIconData();
        foreach (BannerGroupEntry group in GetExportingGroups()) {
            data.IconGroups.Add(group.ToBannerIconGroup());
        }

        foreach (BannerColorEntry color in GetExportingColors()) {
            data.BannerColors.Add(color.ToBannerColor());
        }

        return data;
    }

    public IEnumerable<IconSprite> ToIconSprites() {
        return GetExportingGroups().Aggregate(new List<IconSprite>(), (icons, g) => {
            icons.AddRange(g.Icons.Where(icon => !string.IsNullOrWhiteSpace(icon.SpritePath))
                .Select(icon => icon.ToIconSprite()));
            return icons;
        });
    }

    public IEnumerable<BannerGroupEntry> GetExportingGroups() {
        return Groups.Where(g => g.CanExport).OrderBy(g => g.GroupID);
    }

    public IEnumerable<BannerColorEntry> GetExportingColors() {
        return Colors.Where(c => c.CanExport);
    }

    public void AddGroup() {
        BannerGroupEntry newGroup = bannerGroupFactory(GetNextGroupID());
        newGroup.PropertyChanged += OnGroupPropertyChanged;
        Groups.Add(newGroup);
        OnPropertyChanged(nameof(CanExport));
    }

    public void DeleteGroup(BannerGroupEntry? group) {
        if (group is null) {
            return;
        }

        var index = Groups.IndexOf(group);
        if (index < 0) {
            return;
        }

        group.PropertyChanged -= OnGroupPropertyChanged;
        Groups.Remove(group);
        OnPropertyChanged(nameof(CanExport));
    }

    public void AddColor() {
        Colors.Add(colorFactory(GetNextColorID()));
    }

    public void DeleteColors(IEnumerable<BannerColorEntry> colors) {
        BannerColorEntry[] deleting = colors.ToArray();
        foreach (BannerColorEntry color in deleting) {
            Colors.Remove(color);
        }
    }

    public void SortColors() {
        Colors.SortStable(BannerColorEntry.Compare);
    }

    public int GetNextGroupID() {
        return Groups.Count > 0 ? Groups.Max(g => g.GroupID) + 1 : settings.Banner.CustomGroupStartID;
    }

    public int GetNextColorID() {
        return Colors.Count > 0 ? Colors.Max(c => c.ID) + 1 : settings.Banner.CustomColorStartID;
    }

    public int ValidateGroupID(int oldID, int newID) {
        return ValidateID(oldID, newID, MinGroupId, id => Groups.Any(g => g.GroupID == id), GetNextGroupID);
    }

    public int ValidateColorID(int oldID, int newID) {
        return ValidateID(oldID, newID, MinColorId, id => Colors.Any(g => g.ID == id), GetNextColorID);
    }

    private int ValidateID(int oldID, int newID, int minValidID, Func<int, bool> isIDOccupied, Func<int> getNextID) {
        if (oldID == newID) {
            return newID;
        }

        var direction = newID - oldID > 0;
        while (isIDOccupied(newID)) {
            newID += direction ? 1 : -1;
        }

        if (newID < minValidID) { newID = getNextID(); }

        return newID;
    }

    private void OnGroupPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        OnPropertyChanged(nameof(CanExport));
        if (e.PropertyName == nameof(BannerGroupEntry.GroupID)) {
        }
    }

    public async Task<string?> ExportAll(string outFolderPath, IProgress<ExportProgressData>? progress = null) {
        List<IconSprite> iconsList = ToIconSprites().ToList();
        List<BannerGroupEntry> exportingGroups = GetExportingGroups().ToList();

        // Calculate texture counts per group
        var textureCount = exportingGroups.Select(group => group.Icons.Select(icon => icon.TexturePath).ToArray())
            .Select(icons => (icons.Length + 15) / 16).Sum();

        // Calculate total progress units:
        // - Texture generation: textureCount units
        // - Sprite processing: iconsList.Count units
        // - XML generation: 1 unit
        var totalProgress = textureCount + iconsList.Count + 1;
        var currentProgress = 0;
        var lockObj = new object();

        // Create unified progress handler
        var unifiedProgress = new Progress<ExportProgressData>(data => {
            lock (lockObj) {
                // Update based on stage
                if (data.CurrentStage == "Texture") {
                    // Texture progress: data.ProcessedCount is texture index
                    currentProgress = data.ProcessedCount;
                } else if (data.CurrentStage == "Sprite") {
                    // Sprite progress: data.ProcessedCount is icon count
                    currentProgress = textureCount + data.ProcessedCount;
                } else if (data.CurrentStage == "XML") {
                    // XML progress: already at final stage
                    currentProgress = textureCount + iconsList.Count;
                }

                progress?.Report(new ExportProgressData(currentProgress, totalProgress, data.CurrentStage));
            }
        });

        var merger = new TextureMerger(settings.Banner.TextureOutputResolution);

        // Merge textures from all exporting groups with proper progress tracking
        // Use a thread-safe counter to track completed textures across all groups
        var completedTextures = 0;
        var textureCountLock = new object();

        var textureMergeTasks = new List<Task>();

        foreach (BannerGroupEntry group in exportingGroups) {
            var groupID = group.GroupID;
            var textureFileNames = group.Icons.Select(icon => icon.TexturePath).ToArray();

            Task task = Task.Run(() => {
                // Create progress adapter that tracks completed texture count
                var groupProgress = new Progress<ExportProgressData>(data => {
                    if (data.CurrentStage == "Texture") {
                        // Each texture completion increments the global counter
                        lock (textureCountLock) {
                            completedTextures++;
                            ((IProgress<ExportProgressData>)unifiedProgress).Report(new ExportProgressData(
                                completedTextures,
                                textureCount,
                                "Texture"));
                        }
                    }
                });

                merger.Merge(outFolderPath, groupID, textureFileNames, groupProgress);
            });

            textureMergeTasks.Add(task);
        }

        await Task.WhenAll(textureMergeTasks);

        // Create logger adapter to pass WPF's Serilog configuration to BLIT.Banner
        ILogger serilogLogger = Log.ForContext<BannerIconsProject>();
        var logger = new SerilogLoggerAdapter(serilogLogger);

        // Collect sprites with progress tracking
        await SpriteOrganizer.CollectToSpriteParts(outFolderPath, iconsList, logger, unifiedProgress);

        // Report XML generation completion
        ((IProgress<ExportProgressData>)unifiedProgress).Report(new ExportProgressData(totalProgress, totalProgress,
            "XML"));

        return ExportXML(outFolderPath);
    }

    public string? ExportXML(string outFolderPath) {
        if (!string.IsNullOrWhiteSpace(outFolderPath)) {
            ToBannerIconData().SaveToXml(outFolderPath);
            SpriteOrganizer.GenerateConfigXML(outFolderPath, ToIconSprites());
            return outFolderPath;
        }

        return null;
    }

    #region Nested type: SaveData

    [MessagePackObject]
    public class SaveData {
        [Key(1)] public BannerColorEntry.SaveData[] Colors = new BannerColorEntry.SaveData[] { };

        [Key(0)] public BannerGroupEntry.SaveData[] Groups = new BannerGroupEntry.SaveData[] { };

        public SaveData(BannerIconsProject vm) {
            Groups = vm.Groups.Select(g => new BannerGroupEntry.SaveData(g)).ToArray();
            Colors = vm.Colors.Select(g => new BannerColorEntry.SaveData(g)).ToArray();
        }

        public SaveData() { }
    }

    #endregion
}