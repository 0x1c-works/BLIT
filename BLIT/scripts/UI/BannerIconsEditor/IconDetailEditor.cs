using BLIT.Banner;
using BLIT.scripts.Common;
using Godot;
using System.Threading;
using static Godot.FileDialog;

public partial class IconDetailEditor : Control {
    [Export] public Control? Content { get; set; }
    [Export] public Control? EmptyPage { get; set; }
    [Export] public Button? ReimportSprite { get; set; }
    [Export] public Button? ReimportTexture { get; set; }
    [Export] public Label? Title { get; set; }
    [Export] public Label? SpritePath { get; set; }
    [Export] public Label? TexturePath { get; set; }
    [Export] public TextureRect? Sprite { get; set; }
    [Export] public TextureRect? Texture { get; set; }

    private CancellationTokenSource? _cancelSpriteLoading = new();

    private IconBlock? _iconBlock;
    public IconBlock? IconBlock {
        get => _iconBlock;
        set {
            if (_iconBlock == value) return;
            if (Check.IsGodotSafe(_iconBlock)) {
                UnbindIconBlock(_iconBlock);
            }
            _iconBlock = value;
            if (Check.IsGodotSafe(_iconBlock)) {
                BindIconBlock(_iconBlock);
            }
            UpdateUI();
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        // Set the initial UI state
        UpdateUI();
    }

    private void BindIconBlock(IconBlock block) {
        block.SpriteUpdated += OnSpriteUpdated;
        block.TextureUpdated += OnTextureUpdated;
    }
    private void UnbindIconBlock(IconBlock block) {
        block.SpriteUpdated -= OnSpriteUpdated;
        block.TextureUpdated -= OnTextureUpdated;
    }
    private void OnSpriteUpdated(string path, Texture2D? asset) {
        UpdateImageAsset(path, asset, SpritePath, Sprite);
    }
    private void OnTextureUpdated(string path, Texture2D? asset) {
        UpdateImageAsset(path, asset, TexturePath, Texture);
    }

    private void UpdateUI() {

        if (Content != null) {
            Content.Visible = IconBlock != null;
        }
        if (EmptyPage != null) {
            EmptyPage.Visible = IconBlock == null;
        }
        UpdateTitle();
        UpdateImageAsset(IconBlock?.Icon.TexturePath, IconBlock?.TextureAsset, TexturePath, Texture);
        UpdateImageAsset(IconBlock?.Icon.SpritePath, IconBlock?.SpriteAsset, SpritePath, Sprite);
    }

    private void UpdateTitle() {
        BLIT.scripts.Models.BannerIcons.BannerIconEntry? icon = IconBlock?.Icon;
        var title = string.Empty;
        if (icon != null) {
            title = $"{BannerUtils.GetGroupName(icon.Group.GroupID)} / #{icon.ID}";
        }
        UpdateLabel(Title, title);
    }
    private void UpdateImageAsset(string? path, Texture2D? asset, Label? pathLabel, TextureRect? preview) {
        UpdateLabel(pathLabel, GetImageDisplayPath(path));
        if (Check.IsGodotSafe(preview)) {
            preview.Texture = asset;
        }
    }
    private string GetImageDisplayPath(string? path) {
        return ImageHelper.IsValidImage(path) ? path! : "UNASSIGNED";
    }
    private void UpdateLabel(Label? label, string? value) {
        if (Check.IsGodotSafe(label)) {
            label.Text = value ?? string.Empty;
        }
    }

    private void OnReimportSprite() {
        IconBlock?.UpdateSprite();
    }
    private void OnReimportTexture() {
        IconBlock?.UpdateTexture();
    }
    private void ChangeSprite() {
        if (!Check.IsGodotSafe(IconBlock)) return;
        ChangeImage(IconBlock.Icon.SpritePath, (path) => {
            IconBlock.Icon.SpritePath = path;
            //IconBlock.UpdateSprite();
        });
    }
    private void ChangeTexture() {
        if (!Check.IsGodotSafe(IconBlock)) return;
        ChangeImage(IconBlock.Icon.TexturePath, (path) => {
            IconBlock.Icon.TexturePath = path;
            //IconBlock.UpdateTexture();
        });
    }

    private void ChangeImage(string? currentPath, FileSelectedEventHandler onFileSelected) {
        FileDialog dlg = FileDialogHelper.CreateNative([FileDialogHelper.SUPPORTED_IMAGES], currentPath);
        dlg.FileMode = FileModeEnum.OpenFile;
        dlg.FileSelected += onFileSelected;
        AddChild(dlg);
        dlg.PopupCentered();
    }
}
