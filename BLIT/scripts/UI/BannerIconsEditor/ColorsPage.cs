using BLIT.scripts.Common;
using BLIT.scripts.Models.BannerIcons;
using BLIT.scripts.Services;
using Godot;
using System;
using System.ComponentModel;

public partial class ColorsPage : Control {
    [Export] public Tree? ColorList { get; set; }

    public IProjectService<BannerIconsProject> ProjectService => AppService.Get<IProjectService<BannerIconsProject>>();
    public BannerIconsProject? Project => ProjectService.Current;

    private Texture2D? _sigilIcon;
    private Texture2D? _backgroundIcon;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _sigilIcon = ResourceLoader.Load<Texture2D>("res://icons/quill-pen-fill.svg");
        _backgroundIcon = ResourceLoader.Load<Texture2D>("res://icons/paint-fill.svg");

        ProjectService.PropertyChanged += OnProjectServicePropertyChanged;

        BuildList();
    }

    private void OnProjectServicePropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(ProjectService.Current)) {
            BuildList();
        }
    }

    private void BuildList() {
        if (Project == null) return;
        if (!Check.IsGodotSafe(ColorList)) return;
        ColorList.Clear();
        ColorList.SetColumnExpand(1, true);
        ColorList.SetColumnCustomMinimumWidth(0, 20);
        TreeItem root = ColorList.CreateItem();
        foreach (BannerColorEntry color in Project.Colors) {
            CreateItem(color, root);
        }
    }

    private TreeItem CreateItem(BannerColorEntry color, TreeItem parent) {
        if (!Check.IsGodotSafe(ColorList)) {
            throw new InvalidOperationException($"{nameof(ColorList)} is not set");
        }
        TreeItem item = ColorList.CreateItem(parent);
        item.SetCustomBgColor(0, color.Color);
        item.SetText(1, $"#{color.ID}");
        item.SetCustomFontSize(2, 10);
        item.SetCustomFontSize(3, 10);
        if (color.IsForSigil) {
            item.SetIcon(2, _sigilIcon);
            item.SetTooltipText(2, "SIGIL_COLOR");
        }
        if (color.IsForBackground) {
            item.SetIcon(3, _backgroundIcon);
            item.SetTooltipText(3, "BACKGROUND_COLOR");
        }
        return item;
    }
}
