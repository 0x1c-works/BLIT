using BLIT.scripts.Models.BannerIcons;
using Godot;

public partial class IconDragPreview : Control {
    [Export] public Label? ID { get; set; }
    [Export] public Label? AtlasName { get; set; }

    public void SetData(BannerIconEntry icon) {
        if (ID != null && IsInstanceValid(ID)) {
            ID.Text = (icon.ID.ToString());
        }
        if (AtlasName != null && IsInstanceValid(AtlasName)) {
            AtlasName.Text = (icon.AtlasName);
        }
    }
}
