using BLIT.scripts.Models.BannerIcons;
using Godot;
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

    private BannerIconEntry? _icon;
    public BannerIconEntry Icon {
        get => _icon ?? throw new NullReferenceException("IconBlock has an empty icon");
        set {
            if (_icon == value) return;
            _icon = value;
            UpdateUI();
        }
    }
    private CancellationTokenSource? _cancelLoading;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
    }
    public override void _ExitTree() {
        base._ExitTree();
        _cancelLoading?.Cancel();
    }

    private void UpdateUI() {
        Icon.PropertyChanged += OnIconPropertyChanged;
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
            Texture.Texture = await LoadImage(Icon.TexturePath);
        }
    }
    private void UpdateID() {
        if (ID != null) {
            ID.Text = $"#{Icon.ID}";
        }
    }
    private void UpdateAtlasName() {
        if (AtlasName != null) {
            AtlasName.Text = Icon.AtlasName;
        }
    }
    private Task<ImageTexture?> LoadImage(string path) {
        _cancelLoading?.Cancel();
        _cancelLoading = new();
        return Task.Factory.StartNew(() => {
            if (!File.Exists(path)) return null;

            using var img = Image.LoadFromFile(Icon.TexturePath);
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
