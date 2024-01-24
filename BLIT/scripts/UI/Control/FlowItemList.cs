using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class FlowItemList : HFlowContainer {
    private record FocusEvents(Action OnFocus, Action OnUnfocus);

    [Signal]
    public delegate void SelectionChangeEventHandler();

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

    public override void _Ready() {
        ChildEnteredTree += OnChildEnteredTree;
        ChildExitingTree += OnChildExitingTree;
    }

    public override void _Process(double delta) {
        base._Process(delta);
        if (_shouldNotifyChange) {
            EmitSignal(SignalName.SelectionChange);
            _shouldNotifyChange = false;
        }
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
