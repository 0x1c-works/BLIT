using Godot;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class FlowItemList : HFlowContainer {
    private record struct DropInfo(int RefIndex, Rect2 RefRect, bool IsBeforeRef) {
        public readonly static DropInfo Invalid = new DropInfo(-1, default, false);
        public bool IsValid => RefIndex > -1;

        public override string ToString() {
            var at = IsBeforeRef ? "before" : "after";
            return $"Drop {at} child {RefIndex}";
        }
    }

    private record FocusEvents(Action OnFocus, Action OnUnfocus);
    private const int DROP_INDICATOR_WIDTH = 2;
    private const int BLOCK_GAP = DROP_INDICATOR_WIDTH * 3;

    [Signal] public delegate void SelectionChangeEventHandler();
    [Export] public Color DropIndicatorColor = Colors.Orange;

    private Control? _selected;
    public Control? SelectedItem {
        get => _selected;
        private set {
            if (_selected != value) {
                _selected = value;
                _shouldNotifyChange = true;
            }
        }
    }
    private Dictionary<ulong, FocusEvents> _itemEvents = new();
    private bool _shouldNotifyChange = false;

    private Rect2? _dropRect;
    private Rect2? DropRect {
        get => _dropRect;
        set {
            if (_dropRect == value) return;
            if (_dropRect.HasValue && value.HasValue && value.Value.IsEqualApprox(_dropRect.Value)) {
                return;
            }
            _dropRect = value;
            QueueRedraw();
        }
    }

    public override void _Ready() {
        ChildEnteredTree += OnChildEnteredTree;
        ChildExitingTree += OnChildExitingTree;
        AddThemeConstantOverride("h_separation", BLOCK_GAP);
        AddThemeConstantOverride("h_separation", BLOCK_GAP);
    }

    public override void _Process(double delta) {
        base._Process(delta);
        if (_shouldNotifyChange) {
            EmitSignal(SignalName.SelectionChange);
            _shouldNotifyChange = false;
        }
    }

    public override void _Draw() {
        base._Draw();
        if (DropRect.HasValue) {
            Log.Debug("drop rect: {DropRect}", DropRect);
            DrawRect(DropRect.Value, DropIndicatorColor);
        }
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data) {
        var isDroppable = data.Obj is IconBlock;
        Log.Debug("Can drop: {Can}; Pos: {atPosition}", isDroppable, atPosition);
        if (isDroppable) {
            DropInfo dropInfo = GetDropInfo(atPosition);
            Log.Debug("Drop check: {DropInfo}", dropInfo);
            if (!dropInfo.IsValid) {
                isDroppable = false;
            } else {
                if (GetChild(dropInfo.RefIndex) == null) {
                    throw new InvalidOperationException($"Invalid ref child for dropping at index {dropInfo.RefIndex}");
                }
                isDroppable = true;
                var x = dropInfo.IsBeforeRef ? dropInfo.RefRect.Position.X - DROP_INDICATOR_WIDTH * 2 : dropInfo.RefRect.End.X + DROP_INDICATOR_WIDTH;
                var y = dropInfo.RefRect.Position.Y;
                DropRect = new Rect2(Mathf.Max(0, x), y, DROP_INDICATOR_WIDTH, dropInfo.RefRect.Size.Y);
            }
        }
        if (!isDroppable) {
            DropRect = null;
        }
        return isDroppable;
    }
    public override void _DropData(Vector2 atPosition, Variant data) {
        base._DropData(atPosition, data);
        var icon = data.Obj as IconBlock;
        Log.Debug("{D} dropped", icon?.Icon.ID);
        DropRect = null;
    }

    private DropInfo GetDropInfo(Vector2 pos) {
        Control[] children = GetChildren().Cast<Control>().ToArray();
        for (var i = 0; i < children.Length; i++) {
            Rect2 rect = children[i].GetRect();
            if (pos.Y < rect.Position.Y) break; // ignores the rows below the dropping position
            if (pos.Y >= rect.Position.Y && pos.Y <= rect.End.Y + BLOCK_GAP) {
                // if the dropping position is on the child's row
                if (pos.X >= rect.Position.X && pos.X < rect.End.X + BLOCK_GAP) {
                    // if the dropping position is in the child's range (plus the cell gap to both right and bottom)
                    var isBeforeThisChild = pos.X <= rect.GetCenter().X;
                    return new DropInfo(i, rect, isBeforeThisChild);
                }
            }
        }
        return DropInfo.Invalid;
    }

    private void OnChildEnteredTree(Node node) {
        if (node is Control control) {
            var events = new FocusEvents(() => OnItemFocused(control), () => OnItemUnfocused(control));
            _itemEvents[control.GetInstanceId()] = events;
            control.FocusEntered += events.OnFocus;
            control.FocusExited += events.OnUnfocus;
        }
    }

    private void OnChildExitingTree(Node node) {
        if (node is Control control) {
            if (_itemEvents.TryGetValue(control.GetInstanceId(), out FocusEvents? events)) {
                control.FocusEntered -= events.OnFocus;
                control.FocusExited -= events.OnUnfocus;
                _itemEvents.Remove(control.GetInstanceId());
            }
        }
    }

    private void OnItemFocused(Control item) {
        SelectedItem = item;
    }

    private void OnItemUnfocused(Control item) {
        if (item == SelectedItem) SelectedItem = null;
    }
}
