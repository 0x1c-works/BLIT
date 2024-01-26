using Godot;
using Godot.Collections;
using System.Linq;

public partial class Workspace : Control {
    [Export] public Control? StartPage { get; set; }
    [Export] public ButtonGroup? RootNavButtonGroup { get; set; }
    [Export] public Dictionary<NodePath, NodePath> ButtonToPages { get; set; } = new();

    private Control? _activePage;
    public Control? ActivePage {
        get => _activePage;
        set {
            if (_activePage == value) return;
            if (_activePage != null) {
                _activePage.Visible = false;
            }
            _activePage = value;
            if (_activePage != null) {
                _activePage.Visible = true;
            }
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        RegisterNavButtons();
        foreach (Control page in GetChildren().Where(child => child is Control).Cast<Control>()) {
            page.Visible = page == StartPage;
            if (page.Visible) {
                NodePath relPath = GetPathTo(page);
                NodePath activeNavPath = ButtonToPages.FirstOrDefault(kv => kv.Value == relPath).Key;
                if (GetNode(activeNavPath) is BaseButton activeNav) {
                    activeNav.ButtonPressed = true;
                }
            }
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
    }

    private void RegisterNavButtons() {
        if (RootNavButtonGroup == null) return;
        RootNavButtonGroup.Pressed += OnNavigateTo;
    }
    private void OnNavigateTo(BaseButton pressedButton) {
        if (ButtonToPages.TryGetValue(GetPathTo(pressedButton), out NodePath? pageNodePath)) {
            if (pageNodePath != null && GetNode(pageNodePath) is Control page) {
                ActivePage = page;
            }
        }
    }
}
