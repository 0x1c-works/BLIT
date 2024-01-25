using BLIT.scripts.Common;
using Godot;
using System.Threading;

public partial class IconDetailEditor : Control {
    [Export] public Control? EmptyPage { get; set; }
    [Export] public Button? ReimportSprite { get; set; }
    [Export] public Button? ReimportTexture { get; set; }
    [Export] public Label? Title { get; set; }
    [Export] public Label? SpritePath { get; set; }
    [Export] public Label? TexturePath { get; set; }
    [Export] public TextureRect? Sprite { get; set; }
    [Export] public TextureRect? Texture { get; set; }

    private CancellationTokenSource? _cancelSpriteLoading = new();

    private IconBlock? _icon;
    public IconBlock? IconBlock {
        get => _icon;
        set {
            if (_icon == value) return;
            Visible = value != null;
            if (EmptyPage != null) {
                EmptyPage.Visible = !Visible;
            }
            _icon = value;
            if (IconBlock != null) {
                UpdateUI();
            }
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
    }

    private void UpdateUI() {
        UpdateTitle();
        UpdateSprite();
        UpdateTexture();
    }

    private void UpdateTitle() {
        UpdateLabel(Title, IconBlock?.Icon?.AtlasName);
    }
    private async void UpdateSprite() {
        var spritePath = IconBlock?.Icon?.SpritePath;
        UpdateLabel(SpritePath, GetImageDisplayPath(spritePath));
        if (Sprite != null && IsInstanceValid(Sprite)) {
            ImageTexture? tex = ImageHelper.IsValidImage(spritePath) ? await ImageHelper.LoadImage(spritePath!, _cancelSpriteLoading) : null;
            Sprite.Texture.Dispose();
            Sprite.Texture = tex;
        }
    }
    private void UpdateTexture() {
        UpdateLabel(TexturePath, GetImageDisplayPath(IconBlock?.Icon?.TexturePath));
        if (Texture != null && IsInstanceValid(Texture) && IconBlock?.Texture != null) {
            Texture.Texture = IconBlock.Texture.Texture;
        }
    }
    private string GetImageDisplayPath(string? path) {
        return ImageHelper.IsValidImage(path) ? path! : "UNASSIGNED";
    }
    private void UpdateLabel(Label? label, string? value) {
        if (label != null && IsInstanceValid(label)) {
            label.Text = value ?? string.Empty;
        }
    }
}
