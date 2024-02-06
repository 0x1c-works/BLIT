using BLIT.scripts.Common;
using BLIT.scripts.Models.BannerIcons;
using BLIT.scripts.Services;
using BLIT.scripts.UI.Control;
using Godot;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    private Dictionary<BannerColorEntry, PropertyChangedEventHandler> _propChangedHandlers = new();
    private List<(BannerColorEntry, HashSet<int>)> _colorsSelectedStates = [];
    public IList<BannerColorEntry> SelectedColors => _colorsSelectedStates.Where(v => v.Item2.Count > 0).Select(v => v.Item1).ToImmutableList();


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
        _colorsSelectedStates.Clear();
        ColorList.Clear();
        // Remove all color property changed handlers because the old items are no longer valid
        // otherwise, Godot will throw exceptions when color properties are changed, because it will
        // try to update the UI of the disposed TreeItem.
        foreach (KeyValuePair<BannerColorEntry, PropertyChangedEventHandler> kv in _propChangedHandlers) {
            kv.Key.PropertyChanged -= kv.Value;
        }
        _propChangedHandlers.Clear();

        ColorList.SetColumnExpand(1, true);
        ColorList.SetColumnCustomMinimumWidth(0, 20);
        TreeItem root = ColorList.CreateItem();
        foreach (BannerColorEntry color in Project.Colors) {
            CreateItem(color, root);
        }
        Project.Colors.CollectionChanged += OnColorsCollectionChanged;
        UpdateEditor();
    }

    private TreeItem CreateItem(BannerColorEntry color, TreeItem parent) {
        if (!Check.IsGodotSafe(ColorList)) {
            throw new InvalidOperationException($"{nameof(ColorList)} is not set");
        }
        TreeItem item = ColorList.CreateItem(parent);
        item.SetMetadata(0, color.ID);
        item.SetCustomFontSize(2, 10);
        item.SetCustomFontSize(3, 10);
        RenderColorItem(item, color);
        // Construct a new color property handler bound to the newly created TreeItem
        void handler(object? sender, PropertyChangedEventArgs e) {
            OnColorPropertyChanged(item, color, e);
        }
        if (_propChangedHandlers.TryGetValue(color, out PropertyChangedEventHandler? oldHandler)) {
            Log.Warning("color property handler for {ID} is overridden unexpectedly", color.ID);
            color.PropertyChanged -= oldHandler;
        }
        color.PropertyChanged += handler;
        // Register the handler for unregistering it when the TreeItem is removed
        _propChangedHandlers[color] = handler;
        return item;
    }
    private void OnColorPropertyChanged(TreeItem item, BannerColorEntry color, PropertyChangedEventArgs e) {
        switch (e.PropertyName) {
            case nameof(BannerColorEntry.ID):
            case nameof(BannerColorEntry.Color):
            case nameof(BannerColorEntry.IsForSigil):
            case nameof(BannerColorEntry.IsForBackground):
                RenderColorItem(item, color);
                break;
        }
    }
    private void RenderColorItem(TreeItem item, BannerColorEntry color) {
        item.SetCustomBgColor(0, color.Color);
        item.SetText(1, color.ID.ToString());
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

    private void OnColorsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        if (!Check.IsGodotSafe(ColorList)) return;
        TreeItem root = ColorList.GetRoot();
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null) {
            foreach (BannerColorEntry color in e.NewItems.Cast<BannerColorEntry>()) {
                CreateItem(color, root);
            }
        } else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null) {
            var prevIndex = e.OldStartingIndex;
            foreach (BannerColorEntry color in e.OldItems.Cast<BannerColorEntry>()) {
                TreeItem? item = ColorList.GetItemByID(color.ID);
                if (item != null) {
                    root.RemoveChild(item);
                }
                if (_propChangedHandlers.TryGetValue(color, out PropertyChangedEventHandler? handler)) {
                    // Remove the property handler of the deleted TreeItem to prevent from updating
                    // the disposed UI controls whenever the color properties are changed.
                    color.PropertyChanged -= handler;
                    _propChangedHandlers.Remove(color);
                }
                _colorsSelectedStates.RemoveAll(entry => entry.Item1 == color);
            }
            var newSelectedIndex = Mathf.Min(root.GetChildCount() - 1, Mathf.Max(0, prevIndex));
            ColorList.SelectItemByIndex(newSelectedIndex, selectRow: true);
        }
    }

    private void OnCellsSelected(TreeItem item, int column, bool selected) {
        var colorID = item.GetMetadata(0).AsInt32();
        BannerColorEntry? color = Project?.GetColor(colorID);
        if (color == null) return;
        (BannerColorEntry, HashSet<int>) selectedState = _colorsSelectedStates.FirstOrDefault(entry => entry.Item1 == color);
        if (selected) {
            if (selectedState.Item1 == null) {
                selectedState = (color, [column]);
                _colorsSelectedStates.Add(selectedState);
            } else {
                selectedState.Item2.Add(column);
            }
        } else
            if (selectedState.Item1 != null) {
            selectedState.Item2.Remove(column);
            if (selectedState.Item2.Count <= 0) {
                _colorsSelectedStates.Remove(selectedState);
            }
        }

        if (DeleteButton != null) {
            DeleteButton.Disabled = SelectedColors.Count == 0;
        }
        UpdateEditor();
    }
    private void SelectRow(TreeItem item, bool isSelected) {
        for (var i = 0; i < 4; i++) {
            if (isSelected) {
                item.Select(i);
            } else {
                item.Deselect(i);
            }
        }
    }
    private void OnColorListGUIInput(InputEvent e) {
        if (!Check.IsGodotSafe(ColorList)) return;
        if (e is InputEventKey keyEvent) {
            if (keyEvent.Keycode is Key.Space or Key.Enter && keyEvent.Pressed) {
                TreeItem focusedItem = ColorList.GetSelected();
                if (Check.IsGodotSafe(focusedItem)) {
                    var isItemSelected = IsColorItemSelected(focusedItem);
                    //SelectRow(focusedItem, !isItemSelected);
                    ColorList.ToggleItem(focusedItem, !isItemSelected, entireRow: true);
                }
            }
        }
    }
    private bool IsColorItemSelected(TreeItem item) {
        return Enumerable.Range(0, 4).Any(item.IsSelected);
    }

    private void AddColor() {
        if (Project == null) return;
        Project.AddColor();
    }
    private void DeleteSelectedColors() {
        if (Project == null) return;
        Project.DeleteColors(SelectedColors);
    }
    private void SortColor() {
        if (Project == null) return;
        Project.Colors.CollectionChanged -= OnColorsCollectionChanged;
        Project.SortColors();
        ReloadList();
    }
    private void UpdateEditor() {
        if (SelectedColors.Count == 0) {
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
        if (SelectedColors.Count == 1) {
            BannerColorEntry color = SelectedColors[0];
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
            var colorIDs = SelectedColors.Select(c => c.ID).Order().ToArray();
            if (Check.IsGodotSafe(MultipleIDList)) {
                MultipleIDList.Text = string.Join(", ", colorIDs.Take(3));
            }
            if (Check.IsGodotSafe(MultipleIDMore)) {
                if (colorIDs.Length > 3) {
                    MultipleIDMore.Visible = true;
                    MultipleIDMore.Text = string.Format(Tr("AND_NUMBER_MORE"), colorIDs.Length - 3);
                } else {
                    MultipleIDMore.Visible = false;
                }
            }
            if (Check.IsGodotSafe(ColorPicker)) {
                ColorPicker.Visible = false;
            }
            if (Check.IsGodotSafe(SigilColorToggle)) {
                SigilColorToggle.ButtonPressed = SelectedColors.All(c => c.IsForSigil);
            }
            if (Check.IsGodotSafe(BackgroundColorToggle)) {
                BackgroundColorToggle.ButtonPressed = SelectedColors.All(c => c.IsForBackground);
            }
        }
    }

    private void OnIDEditValueChanged(double value) {
        if (SelectedColors.Count != 1) return;
        BannerColorEntry color = SelectedColors[0];
        color.ID = (int)value;
        IDEdit!.Value = color.ID;
    }
    private void OnSigilColorToggleToggled(bool pressed) {
        foreach (BannerColorEntry color in SelectedColors) {
            color.IsForSigil = pressed;
        }
    }
    private void OnBackgroundColorToggleToggled(bool pressed) {
        foreach (BannerColorEntry color in SelectedColors) {
            color.IsForBackground = pressed;
        }
    }
    private void OnColorPickerColorChanged(Color color) {
        if (SelectedColors.Count != 1) return;
        SelectedColors[0].Color = color;
    }
}
