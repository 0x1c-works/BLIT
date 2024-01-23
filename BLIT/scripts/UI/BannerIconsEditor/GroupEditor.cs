using BLIT.scripts.Models.BannerIcons;
using Godot;
using Serilog;
using System.Linq;

public partial class GroupEditor : Control {
    [Export] public Control? EmptyPage { get; set; }
    [Export] public SpinBox? GroupID { get; set; }
    [Export] public Container? IconContainer { get; set; }

    [Export] public PackedScene? IconBlockPrefab { get; set; }

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
        UpdateUI();
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
        if (IconContainer != null) {
            Node[] children = IconContainer.GetChildren().ToArray();
            foreach (Node? item in children) {
                item.QueueFree();
            }
            foreach (BannerIconEntry icon in Group.Icons) {
                AddIconBlock(icon);
            }
        }
    }

    private void OnGroupIDChanged(float value) {
        if (Group == null) return;
        Group.GroupID = (int)value;
    }

    private void AddIconBlock(BannerIconEntry icon) {
        if (IconBlockPrefab == null) {
            Log.Error("IconBlockPrefab is not set");
            return;
        }
        if (IconContainer == null) {
            Log.Error("IconContainer is not set");
            return;
        }
        IconBlock block = IconBlockPrefab.Instantiate<IconBlock>();
        block.Icon = icon;
        IconContainer.AddChild(block);
    }
}
