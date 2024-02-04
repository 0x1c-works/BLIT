using Godot;
using System.Linq;

namespace BLIT.scripts.UI.Control;
public static class TreeExtensions {
    public static TreeItem? GetItemByID(this Tree tree, int id) {
        return tree?.GetRoot().GetChildren().FirstOrDefault(child => child.GetMetadata(0).AsInt32() == id);
    }
    public static TreeItem? GetItemAtIndex(this Tree tree, int index) {
        if (index < 0) return null;
        TreeItem? root = tree?.GetRoot();
        if (root == null) return null;
        if (index >= root.GetChildCount()) return null;
        return root.GetChild(index);
    }
    public static bool SelectItemByIndex(this Tree tree, int index) {
        TreeItem? selection = GetItemAtIndex(tree, index);
        selection?.Select(0);
        return selection != null;
    }
}
