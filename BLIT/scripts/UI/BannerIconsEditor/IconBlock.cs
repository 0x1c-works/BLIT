using BLIT.scripts.Models.BannerIcons;
using Godot;
using Serilog;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public partial class IconBlock : PanelContainer {
    public event Action<IconBlock> Selected = delegate { };
    [Export] public TextureRect? Texture { get; set; }
    [Export] public Label? ID { get; set; }
    [Export] public Label? AtlasName { get; set; }
    [Export] public StyleBox? SelectedStyle { get; set; }
    [Export] public StyleBox? UnselectedStyle { get; set; }

    [Export] public PackedScene? DragPreview { get; set; }

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
    private CancellationTokenSource? _cancelLoading;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        FocusEntered += OnFocusEnter;
        FocusExited += OnFocusExit;
    }
    public override void _ExitTree() {
        base._ExitTree();
        if (Texture != null && IsInstanceValid(Texture) && Texture.Texture != null && IsInstanceValid(Texture.Texture)) {
            Texture?.Texture?.Dispose();
        }
        _cancelLoading?.Cancel();
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
        }
    }

    private async void UpdateTexture() {
        if (Texture != null) {
            Texture.Texture?.Dispose();
            ImageTexture? tex = null;
            try {
                tex = await LoadImage(Icon.TexturePath);
                if (IsInstanceValid(Texture)) {
                    Texture.Texture = tex;
                } else {
                    tex?.Dispose();
                }
            } catch (TaskCanceledException) {
                tex?.Dispose();
                // ignore
            } catch (Exception ex) {
                Log.Error(ex, "Failed to load image");
                tex?.Dispose();
            }
        }
    }
    private void UpdateID() {
        if (ID != null && IsInstanceValid(ID)) {
            ID.Text = $"#{Icon.ID}";
        }
    }
    private void UpdateAtlasName() {
        if (AtlasName != null && IsInstanceValid(AtlasName)) {
            AtlasName.Text = Icon.AtlasName;
        }
    }
    private Task<ImageTexture?> LoadImage(string path) {
        _cancelLoading?.Cancel();
        _cancelLoading = new();
        return Task.Factory.StartNew(() => {
            if (!File.Exists(path)) return null;

            using var img = Image.LoadFromFile(path);
            return ImageTexture.CreateFromImage(img);
        }, _cancelLoading.Token);
    }

    private void OnFocusEnter() {
        UpdateFocusStyle();
        Selected(this);
    }

    private void OnFocusExit() {
        UpdateFocusStyle();
    }
    private void UpdateFocusStyle() {
        AddThemeStyleboxOverride("panel", HasFocus() ? SelectedStyle : UnselectedStyle);
    }
}
