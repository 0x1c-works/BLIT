using BLIT.scripts.Common;
using BLIT.scripts.Models.BannerIcons;
using BLIT.scripts.Services;
using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

public partial class ColorsPage : Control {
    [Export] public Tree? ColorList { get; set; }
    [Export] public SpinBox? IDEdit { get; set; }
    [Export] public CheckButton? SigilColorToggle { get; set; }
    [Export] public CheckButton? BackgroundColorToggle { get; set; }
    [Export] public ColorPicker? ColorPicker { get; set; }
    [Export] public Control? MultipleIDSection { get; set; }
    [Export] public Label? MultipleIDList { get; set; }
    [Export] public Label? MultipleIDMore { get; set; }
    [Export] public Control? ColorEditor { get; set; }
    [Export] public Control? EmptyPage { get; set; }

    public IProjectService<BannerIconsProject> ProjectService => AppService.Get<IProjectService<BannerIconsProject>>();
    public BannerIconsProject? Project => ProjectService.Current;

    private Texture2D? _sigilIcon;
    private Texture2D? _backgroundIcon;
    private List<BannerColorEntry> _selectedColors = [];

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _sigilIcon = ResourceLoader.Load<Texture2D>("res://icons/quill-pen-fill.svg");
        _backgroundIcon = ResourceLoader.Load<Texture2D>("res://icons/paint-fill.svg");

        ProjectService.PropertyChanged += OnProjectServicePropertyChanged;

        BuildList();
        UpdateEditor();
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
        item.SetMetadata(0, color.ID);
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

    private void OnCellsSelected(TreeItem item, int column, bool selected) {
        var colorID = item.GetMetadata(0).AsInt32();
        BannerColorEntry? color = Project?.GetColor(colorID);
        if (color == null) return;
        if (selected) {
            if (_selectedColors.Contains(color)) return;
            _selectedColors.Add(color);
        } else {
            _selectedColors.Remove(color);
        }
        UpdateEditor();
    }

    private void UpdateEditor() {
        if (_selectedColors == null || _selectedColors.Count == 0) {
            if (Check.IsGodotSafe(ColorEditor)) {
                ColorEditor.Visible = false;
            }
            if (Check.IsGodotSafe(EmptyPage)) {
                EmptyPage.Visible = true;
            }
            return;
        }

        if (Check.IsGodotSafe(ColorEditor)) {
            ColorEditor.Visible = true;
        }
        if (Check.IsGodotSafe(EmptyPage)) {
            EmptyPage.Visible = false;
        }

        if (_selectedColors.Count == 1) {
            BannerColorEntry color = _selectedColors[0];
            if (Check.IsGodotSafe(IDEdit)) {
                IDEdit.Visible = true;
                IDEdit.Value = color.ID;
            }
            if (Check.IsGodotSafe(MultipleIDSection)) {
                MultipleIDSection.Visible = false;
            }
            if (Check.IsGodotSafe(SigilColorToggle)) {
                SigilColorToggle.ButtonPressed = color.IsForSigil;
            }
            if (Check.IsGodotSafe(BackgroundColorToggle)) {
                BackgroundColorToggle.ButtonPressed = color.IsForBackground;
            }
            if (Check.IsGodotSafe(ColorPicker)) {
                ColorPicker.Visible = true;
                ColorPicker.Color = color.Color;
            }
        } else {
            if (Check.IsGodotSafe(IDEdit)) {
                IDEdit.Visible = false;
            }
            if (Check.IsGodotSafe(MultipleIDSection)) {
                MultipleIDSection.Visible = true;
            }
            var colorIDs = _selectedColors.Select(c => c.ID).Order().ToArray();
            if (Check.IsGodotSafe(MultipleIDList)) {
                MultipleIDList.Text = string.Join(", ", colorIDs.Take(3));
            }
            if (colorIDs.Length > 3 && Check.IsGodotSafe(MultipleIDMore)) {
                MultipleIDMore.Text = string.Format(Tr("AND_NUMBER_MORE"), colorIDs.Length - 3);
            }
            if (Check.IsGodotSafe(ColorPicker)) {
                ColorPicker.Visible = false;
            }
            if (Check.IsGodotSafe(SigilColorToggle)) {
                SigilColorToggle.ButtonPressed = _selectedColors.All(c => c.IsForSigil);
            }
            if (Check.IsGodotSafe(BackgroundColorToggle)) {
                BackgroundColorToggle.ButtonPressed = _selectedColors.All(c => c.IsForBackground);
            }
        }
    }
}
