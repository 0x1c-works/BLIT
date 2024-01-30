using BLIT.scripts.Common;
using BLIT.scripts.Models.BannerIcons;
using Godot;

public partial class IconDragPreview : Control {
    [Export] public Label? ID { get; set; }
    [Export] public Label? AtlasName { get; set; }

    public void SetData(BannerIconEntry icon) {
        if (Check.IsGodotSafe(ID)) {
            ID.Text = (icon.ID.ToString());
        }
        if (Check.IsGodotSafe(AtlasName)) {
            AtlasName.Text = (icon.AtlasName);
        }
    }
}
