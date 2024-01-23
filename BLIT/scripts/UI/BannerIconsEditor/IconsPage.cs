using Godot;

public partial class IconsPage : Control {
    [Export] public IconGroupList? IconGroupList { get; set; }
    [Export] public GroupEditor? GroupEditor { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        if (IconGroupList != null && GroupEditor != null) {
            IconGroupList.SelectionChanged += GroupEditor.OnGroupSelected;
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
    }
}
