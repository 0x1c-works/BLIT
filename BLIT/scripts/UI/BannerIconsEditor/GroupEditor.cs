using BLIT.scripts.Models.BannerIcons;
using Godot;
using Serilog;
using System.Collections.Specialized;
using System.Linq;

public partial class GroupEditor : Control {
    [Export] public Control? EmptyPage { get; set; }
    [Export] public SpinBox? GroupID { get; set; }
    [Export] public FlowItemList? IconGallery { get; set; }
    [Export] public PackedScene? IconBlockPrefab { get; set; }
    [Export] public IconDetailEditor? IconDetailEditor { get; set; }

    private BannerGroupEntry? _group;
    public BannerGroupEntry? Group {
        get => _group;
        set {
            if (_group == value) return;
            _group = value;
            UpdateUI();
        }
    }

    public override void _Ready() {
        if (IconGallery != null) {
            IconGallery.SelectionChanged += OnIconSelected;
            IconGallery.ChildMoved += OnChildMoved;
        }
        UpdateUI();
    }

    private void OnChildMoved(int oldIndex, int newIndex) {
        Group?.Icons.Move(oldIndex, newIndex);
        Group?.RefreshCellIndex();
    }

    public void OnGroupSelected(BannerGroupEntry? group) {
        _group = group;
        UpdateUI();
    }

    private void UpdateUI() {
        Visible = Group != null;
        if (EmptyPage != null) {
            EmptyPage.Visible = !Visible;
        }
        if (Group == null) {
            return;
        }
        if (GroupID != null) {
            GroupID.Value = Group.GroupID;
        }
        if (IconGallery != null) {
            Node[] children = IconGallery.GetChildren().ToArray();
            foreach (Node? item in children) {
                item.QueueFree();
            }
            foreach (BannerIconEntry icon in Group.Icons) {
                AddIconBlock(icon);
            }
        }
        Group.Icons.CollectionChanged += OnIconsCollectionChanged;
    }

    private void OnIconsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
    }

    private void OnGroupIDChanged(float value) {
        if (Group == null) return;
        Group.GroupID = (int)value;
        if (GroupID != null) {
            GroupID.Value = Group.GroupID;
        }
    }

    private void AddIconBlock(BannerIconEntry icon) {
        if (IconBlockPrefab == null) {
            Log.Error("IconBlockPrefab is not set");
            return;
        }
        if (IconGallery == null) {
            Log.Error("IconGallery is not set");
            return;
        }
        IconBlock block = IconBlockPrefab.Instantiate<IconBlock>();
        block.Icon = icon;
        IconGallery.AddChild(block);
    }
    private void OnIconSelected() {
        var selected = IconGallery?.SelectedItem as IconBlock;
        Log.Debug("selected: {ID} @ {Atlas}", selected?.Icon?.ID, selected?.Icon?.AtlasName);
        if (IconDetailEditor != null) {
            IconDetailEditor.IconBlock = selected;
        }
    }
}
