using BLIT.scripts.Models.BannerIcons;
using BLIT.scripts.Services;
using Godot;
using Serilog;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

public partial class IconGroupList : Control {
    public event Action<BannerGroupEntry?>? SelectionChanged;
    [Export] public Button? AddButton { get; set; }
    [Export] public Button? DeleteButton { get; set; }
    [Export] public Tree? ItemList { get; set; }

    private IProjectService<BannerIconsProject> ProjectService => AppService.Get<IProjectService<BannerIconsProject>>();
    private BannerIconsProject? Project => ProjectService.Current;
    private int _prevSelectedID = -1;
    private int _prevSelectedIndex = -1;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        ItemList ??= GetNode<Tree>("ItemList");
        ItemList.ItemSelected += OnGroupSelected;

        if (AddButton != null) {
            AddButton.Pressed += CreateGroup;
        }
        if (DeleteButton != null) {
            DeleteButton.Pressed += DeleteSelectedGroup;
        }

        ProjectService.PropertyChanged += OnProjectServicePropertyChanged;
        Bind();
        UpdateList();
    }

    private void OnProjectServicePropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(ProjectService.Current)) {
            Bind();
            UpdateList();
        }
    }

    private void Bind() {
        if (Project == null) return;
        Project.Groups.CollectionChanged += OnGroupsCollectionChanged;
    }

    private void OnGroupsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        // FIXME: can be optimized
        //UpdateList();
        if (ItemList == null) return;
        TreeItem root = ItemList.GetRoot();
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null) {
            foreach (BannerGroupEntry group in e.NewItems.Cast<BannerGroupEntry>()) {
                CreateItem(group, root);
            }
        } else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null) {
            foreach (BannerGroupEntry group in e.OldItems.Cast<BannerGroupEntry>()) {
                TreeItem? item = GetItemByID(group.GroupID);
                if (item != null) {
                    root.RemoveChild(item);
                }
            }
            var selectedIndex = Mathf.Min(root.GetChildCount() - 1, Mathf.Max(0, _prevSelectedIndex));
            SelectItemByIndex(selectedIndex);
        }
    }
    private void UpdateList() {
        if (ItemList == null) return;
        _prevSelectedID = GetSelectedGroup()?.GroupID ?? -1;
        ItemList.Clear();
        if (Project == null) {
            return;
        }
        TreeItem root = ItemList.CreateItem();
        var hasSelected = false;
        foreach (BannerGroupEntry group in Project.Groups) {
            TreeItem? item = CreateItem(group, root);
            if (item == null) continue;
            if (_prevSelectedID == group.GroupID) {
                hasSelected = true;
                item.Select(0);
            }
        }
        if (!hasSelected) {
            SelectItemByIndex(_prevSelectedIndex);
        }
    }
    private TreeItem? CreateItem(BannerGroupEntry group, TreeItem parent) {
        if (ItemList == null) return null;
        TreeItem item = ItemList.CreateItem(parent);
        item.SetText(0, group.GroupID.ToString());
        item.SetText(1, $"({group.Icons.Count})");
        item.SetCustomColor(1, Colors.LightGray);
        item.SetTextAlignment(1, HorizontalAlignment.Right);
        item.SetMetadata(0, group.GroupID);
        group.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(group.GroupID)) {
                item.SetText(0, group.GroupID.ToString());
                item.SetMetadata(0, group.GroupID);
            }
        };
        group.Icons.CollectionChanged += (sender, e) => {
            item.SetText(1, group.Icons.Count.ToString());
        };
        return item;
    }
    private void OnGroupSelected() {
        if (ItemList == null) return;
        BannerGroupEntry? selected = GetSelectedGroup();
        if (DeleteButton != null) {
            DeleteButton.Disabled = selected == null;
        }
        if (selected != null) {
            Log.Information("Selected group {Group}", [selected]);
        }
        SelectionChanged?.Invoke(selected);
    }
    private void CreateGroup() {
        Project?.AddGroup();
    }
    private void DeleteSelectedGroup() {
        if (ItemList == null) return;
        BannerGroupEntry? selected = GetSelectedGroup();
        if (selected != null) {
            Project?.DeleteGroup(selected);
        }
    }
    private BannerGroupEntry? GetSelectedGroup() {
        TreeItem? selected = ItemList?.GetSelected();
        if (selected != null) {
            _prevSelectedIndex = selected.GetIndex();
            var id = selected.GetMetadata(0).AsInt32();
            BannerGroupEntry? group = Project?.GetGroup(id);
            return group;
        }
        return null;
    }
    private TreeItem? GetItemAtIndex(int index) {
        if (index < 0) return null;
        TreeItem? root = ItemList?.GetRoot();
        if (root == null) return null;
        if (index >= root.GetChildCount()) return null;
        return root.GetChild(index);
    }
    private TreeItem? GetItemByID(int id) {
        return ItemList?.GetRoot().GetChildren().FirstOrDefault(child => child.GetMetadata(0).AsInt32() == id);
    }
    private void SelectItemByIndex(int index) {
        TreeItem? selection = GetItemAtIndex(index);
        if (selection != null) {
            selection.Select(0);
        } else {
            // refresh the UI state on empty selection
            OnGroupSelected();
        }
    }
}
