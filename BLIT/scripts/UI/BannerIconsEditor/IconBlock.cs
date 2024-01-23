using BLIT.scripts.Models.BannerIcons;
using Godot;
using System;
using System.ComponentModel;

public partial class IconBlock : Control {
    [Export] public TextureRect? Texture { get; set; }
    [Export] public Label? ID { get; set; }
    [Export] public Label? AtlasName { get; set; }

    private BannerIconEntry? _icon;
    public BannerIconEntry Icon {
        get => _icon ?? throw new NullReferenceException("IconBlock has an empty icon");
        set {
            if (_icon == value) return;
            _icon = value;
            UpdateUI();
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
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

    private void UpdateTexture() {
        if (Texture != null) {
            var img = Image.LoadFromFile(Icon.TexturePath);
            Texture.Texture = ImageTexture.CreateFromImage(img);
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
}
