using BLIT.scripts.Common;
using BLIT.scripts.Models.BannerIcons;
using BLIT.scripts.Services;
using BLIT.scripts.UI.Control;
using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

public partial class ColorsPage : Control {
    [Export] public Button? DeleteButton { get; set; }
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

        ReloadList();
        UpdateEditor();

        if (Check.IsGodotSafe(IDEdit)) {
            IDEdit.ValueChanged += OnIDEditValueChanged;
        }
        if (Check.IsGodotSafe(SigilColorToggle)) {
            SigilColorToggle.Toggled += OnSigilColorToggleToggled;
        }
        if (Check.IsGodotSafe(BackgroundColorToggle)) {
            BackgroundColorToggle.Toggled += OnBackgroundColorToggleToggled;
        }
        if (Check.IsGodotSafe(ColorPicker)) {
            ColorPicker.ColorChanged += OnColorPickerColorChanged;
        }
    }

    private void OnProjectServicePropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(ProjectService.Current)) {
            ReloadList();
        }
    }

    private void ReloadList() {
        if (Project == null) return;
        if (!Check.IsGodotSafe(ColorList)) return;
        ColorList.Clear();
        ColorList.SetColumnExpand(1, true);
        ColorList.SetColumnCustomMinimumWidth(0, 20);
        TreeItem root = ColorList.CreateItem();
        foreach (BannerColorEntry color in Project.Colors) {
            CreateItem(color, root);
        }
        Project.Colors.CollectionChanged += OnColorsCollectionChanged;
    }

    private void OnColorsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        if (!Check.IsGodotSafe(ColorList)) return;
        TreeItem root = ColorList.GetRoot();
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null) {
            foreach (BannerColorEntry color in e.NewItems.Cast<BannerColorEntry>()) {
                CreateItem(color, root);
            }
        } else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null) {
            foreach (BannerColorEntry color in e.OldItems.Cast<BannerColorEntry>()) {
                TreeItem? item = ColorList.GetItemByID(color.ID);
                if (item != null) {
                    root.RemoveChild(item);
                }
            }
            //var selectedIndex = Mathf.Min(root.GetChildCount() - 1, Mathf.Max(0, _prevSelectedIndex));
            ColorList.SelectItemByIndex(-1);
        }
    }

    private TreeItem CreateItem(BannerColorEntry color, TreeItem parent) {
        if (!Check.IsGodotSafe(ColorList)) {
            throw new InvalidOperationException($"{nameof(ColorList)} is not set");
        }
        TreeItem item = ColorList.CreateItem(parent);
        item.SetMetadata(0, color.ID);
        item.SetCustomFontSize(2, 10);
        item.SetCustomFontSize(3, 10);
        RenderItem(item, color);
        color.PropertyChanged += (o, e) => OnColorPropertyChanged(item, color, e);
        return item;
    }

    private void RenderItem(TreeItem item, BannerColorEntry color) {
        item.SetCustomBgColor(0, color.Color);
        item.SetText(1, $"#{color.ID}");
        if (color.IsForSigil) {
            item.SetIcon(2, _sigilIcon);
            item.SetTooltipText(2, "SIGIL_COLOR");
        } else {
            item.SetIcon(2, null);
            item.SetTooltipText(2, null);
        }
        if (color.IsForBackground) {
            item.SetIcon(3, _backgroundIcon);
            item.SetTooltipText(3, "BACKGROUND_COLOR");
        } else {
            item.SetIcon(3, null);
            item.SetTooltipText(3, null);
        }
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
        if (DeleteButton != null) {
            DeleteButton.Disabled = _selectedColors.Count == 0;
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

    private void OnIDEditValueChanged(double value) {
        if (_selectedColors.Count != 1) return;
        BannerColorEntry color = _selectedColors[0];
        color.ID = (int)value;
        IDEdit!.Value = color.ID;
    }
    private void OnSigilColorToggleToggled(bool pressed) {
        foreach (BannerColorEntry color in _selectedColors) {
            color.IsForSigil = pressed;
        }
    }
    private void OnBackgroundColorToggleToggled(bool pressed) {
        foreach (BannerColorEntry color in _selectedColors) {
            color.IsForBackground = pressed;
        }
    }
    private void OnColorPickerColorChanged(Color color) {
        if (_selectedColors.Count != 1) return;
        _selectedColors[0].Color = color;
    }
    private void OnColorPropertyChanged(TreeItem item, BannerColorEntry color, PropertyChangedEventArgs e) {
        switch (e.PropertyName) {
            case nameof(BannerColorEntry.ID):
            case nameof(BannerColorEntry.Color):
            case nameof(BannerColorEntry.IsForSigil):
            case nameof(BannerColorEntry.IsForBackground):
                RenderItem(item, color);
                break;
        }
    }
    private void AddColor() {
        if (Project == null) return;
        Project.AddColor();
    }
    private void DeleteSelectedColors() {
        if (Project == null) return;
        Project.DeleteColors(_selectedColors);
    }
    private void SortColor() {
        if (Project == null) return;
        Project.Colors.CollectionChanged -= OnColorsCollectionChanged;
        Project.SortColors();
        ReloadList();
    }
}
