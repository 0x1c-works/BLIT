using Godot;
using System;
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
    public static bool SelectItemByIndex(this Tree tree, int index, bool selectRow = false, int[]? selectColumns = null) {
        TreeItem? selection = GetItemAtIndex(tree, index);
        var columns = selectColumns ?? [0];
        if (selectRow) {
            columns = Enumerable.Range(0, tree.Columns).ToArray();
        }
        Array.ForEach(columns, i => tree.SetSelected(selection, i));
        return selection != null;
    }
    public static void ToggleItem(this Tree tree,
                                         TreeItem item,
                                         bool selected,
                                         bool entireRow = false,
                                         int[]? effectedColumns = null,
                                         int focusedColumn = 0) {
        var columns = effectedColumns ?? [focusedColumn];
        if (!columns.Contains(focusedColumn)) {
            columns = columns.Append(focusedColumn).ToArray();
        }
        if (entireRow) {
            columns = Enumerable.Range(0, tree.Columns).ToArray();
        }
        Array.ForEach(columns, i => {
            if (selected) {
                if (i == focusedColumn) {
                    tree.SetSelected(item, i);
                } else {
                    item.Select(i);
                }
            } else {
                item.Deselect(i);
            }
        });
    }
}
