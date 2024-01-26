using BLIT.scripts.Models.BannerIcons;
using BLIT.scripts.Services;
using Godot;
using System;
using System.Linq;

public partial class BannerIconsSettingsPage : Control {
    [Export] public TextEdit? ScanFoldersEdit { get; set; }
    [Export] public SpinBox? GroupStartIDEdit { get; set; }
    [Export] public SpinBox? ColorStartIDEdit { get; set; }

    private BannerSettings Settings => AppService.Get<BannerSettings>();
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        if (ScanFoldersEdit != null) {
            ScanFoldersEdit.Text = string.Join("\n", Settings.SpriteScanFolders);
            ScanFoldersEdit.TextChanged += OnScanFoldersChanged;
        }
        if (GroupStartIDEdit != null) {
            GroupStartIDEdit.Value = Settings.CustomGroupStartID;
            GroupStartIDEdit.ValueChanged += OnGroupStartIDChanged;
        }
        if (ColorStartIDEdit != null) {
            ColorStartIDEdit.Value = Settings.CustomColorStartID;
            ColorStartIDEdit.ValueChanged += OnColorStartIDChanged;
        }
    }

    private void OnScanFoldersChanged() {
        Settings.SpriteScanFolders = ScanFoldersEdit?.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList() ?? [];
        Settings.Save();
    }

    private void OnGroupStartIDChanged(double value) {
        Settings.CustomGroupStartID = (int)(GroupStartIDEdit?.Value ?? 0);
        Settings.Save();
    }
    private void OnColorStartIDChanged(double value) {
        Settings.CustomGroupStartID = (int)(GroupStartIDEdit?.Value ?? 0);
        Settings.Save();
    }
}
