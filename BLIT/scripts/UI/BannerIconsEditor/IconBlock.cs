using BLIT.scripts.Common;
using BLIT.scripts.Models.BannerIcons;
using Godot;
using Serilog;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public delegate void Texture2DUpdate(string path, Texture2D? texture);

public partial class IconBlock : PanelContainer, ISelectableItem {
    public event Action<ISelectableItem> Selected = delegate { };
    public event Texture2DUpdate TextureUpdated = delegate { };
    public event Texture2DUpdate SpriteUpdated = delegate { };

    [Export] public TextureRect? Texture { get; set; }
    [Export] public Label? ID { get; set; }
    [Export] public Label? AtlasName { get; set; }
    [Export] public StyleBox? SelectedStyle { get; set; }
    [Export] public StyleBox? UnselectedStyle { get; set; }

    [Export] public PackedScene? DragPreview { get; set; }

    public bool IsSelected {
        get {
            if (GetParent() is FlowItemList list) {
                return list.SelectedItem == this;
            }
            return false;
        }
        set {
            if (value && GetParent() is FlowItemList list) {
                list.SelectedItem = this;
            }
            UpdateSelectedStyle();
        }
    }

    private BannerIconEntry? _icon;
    public BannerIconEntry Icon {
        get => _icon ?? throw new NullReferenceException("IconBlock has an empty icon");
        set {
            if (_icon == value) return;
            _icon = value;
            if (_icon != null) {
                _icon.PropertyChanged += OnIconPropertyChanged;
            }
            UpdateUI();
        }
    }
    private CancellationTokenSource? _cancelLoadingTexture = new();
    private CancellationTokenSource? _cancelLoadingSprite = new();
    public Texture2D? TextureAsset { get; private set; }
    public Texture2D? SpriteAsset { get; private set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        TextureUpdated += (path, asset) => {
            if (Check.IsGodotSafe(Texture)) {
                Texture.Texture = asset;
            }
        };
    }
    public override void _ExitTree() {
        base._ExitTree();
        if (Check.IsGodotSafe(Texture)) {
            Texture?.Texture?.Dispose();
        }
        _cancelLoadingTexture?.Cancel();
        _cancelLoadingSprite?.Cancel();
        TextureAsset?.Dispose();
        SpriteAsset?.Dispose();
    }
    public override Variant _GetDragData(Vector2 atPosition) {
        Control preview;
        if (DragPreview != null) {
            IconDragPreview iconPreview = DragPreview.Instantiate<IconDragPreview>();
            iconPreview.SetData(Icon);
            preview = iconPreview;
        } else {
            preview = new ColorPickerButton() {
                Color = Colors.Orange,
                Size = new Vector2(50, 50),
            };
        }
        preview.RotationDegrees = 10;
        SetDragPreview(preview);
        return this;
    }

    private void UpdateUI() {
        UpdateTexture();
        UpdateSprite();
        UpdateID();
        UpdateAtlasName();
    }

    private void OnIconPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(BannerIconEntry.ID)) {
            UpdateID();
        } else if (e.PropertyName == nameof(BannerIconEntry.AtlasName)) {
            UpdateAtlasName();
        } else if (e.PropertyName == nameof(BannerIconEntry.TexturePath)) {
            UpdateTexture();
        } else if (e.PropertyName == nameof(BannerIconEntry.SpritePath)) {
            UpdateSprite();
        }
    }

    public async void UpdateTexture() {
        TextureAsset?.Dispose();
        TextureAsset = await LoadImage(Icon.TexturePath, _cancelLoadingTexture);
        TextureUpdated(Icon.TexturePath, TextureAsset);
    }
    public async void UpdateSprite() {
        SpriteAsset?.Dispose();
        SpriteAsset = await LoadImage(Icon.SpritePath, _cancelLoadingSprite);
        SpriteUpdated(Icon.SpritePath, SpriteAsset);
    }

    private void UpdateID() {
        if (Check.IsGodotSafe(ID)) {
            ID.Text = $"#{Icon.ID}";
        }
    }
    private void UpdateAtlasName() {
        if (Check.IsGodotSafe(AtlasName)) {
            AtlasName.Text = Icon.AtlasName;
        }
    }
    private async Task<Texture2D?> LoadImage(string path, CancellationTokenSource? cancelSource) {
        Texture2D? tex = null;
        try {
            cancelSource?.Cancel();
            cancelSource = new();
            tex = await Task.Factory.StartNew(() => {
                if (!File.Exists(path)) return null;

                using var img = Image.LoadFromFile(path);
                return ImageTexture.CreateFromImage(img);
            }, cancelSource.Token);
        } catch (TaskCanceledException) {
            tex?.Dispose();
        } catch (Exception ex) {
            Log.Error(ex, "Failed to load image");
            tex?.Dispose();
        }
        return tex;
    }

    public void UpdateSelectedStyle() {
        AddThemeStyleboxOverride("panel", IsSelected ? SelectedStyle : UnselectedStyle);
    }
    private void OnClick() {
        Selected(this);
        UpdateSelectedStyle();
    }
}
