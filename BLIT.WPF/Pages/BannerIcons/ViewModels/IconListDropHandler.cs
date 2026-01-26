using BLIT.WPF.Pages.BannerIcons.Models;
using GongSolutions.Wpf.DragDrop;
using System.Collections.ObjectModel;
using System.Windows;

namespace BLIT.WPF.Pages.BannerIcons.ViewModels;

/// <summary>
/// Drag and drop handler for icon list (within same group)
/// Supports reordering icons by dragging within the list
/// </summary>
public class IconListDropHandler : IDropTarget {
    public void DragOver(IDropInfo dropInfo) {
        // Only accept drops from BannerIconEntry within the same list
        if (dropInfo.Data is not BannerIconEntry sourceIcon) {
            dropInfo.Effects = DragDropEffects.None;
            return;
        }

        // Check if we're dropping on a valid target
        if (dropInfo.TargetItem is not BannerIconEntry targetIcon) {
            dropInfo.Effects = DragDropEffects.None;
            return;
        }

        // Allow move operation
        dropInfo.Effects = DragDropEffects.Move;
        
        // Show insert position indicator
        dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
    }

    public void Drop(IDropInfo dropInfo) {
        if (dropInfo.Data is not BannerIconEntry sourceIcon) {
            return;
        }

        if (dropInfo.TargetItem is not BannerIconEntry targetIcon) {
            return;
        }

        // Get the icons collection from the target
        if (dropInfo.TargetCollection is not ObservableCollection<BannerIconEntry> icons) {
            return;
        }

        int sourceIndex = icons.IndexOf(sourceIcon);
        int targetIndex = icons.IndexOf(targetIcon);

        // Don't move if source and target are the same
        if (sourceIndex == targetIndex) {
            return;
        }

        // Remove from source position
        icons.RemoveAt(sourceIndex);

        // Adjust target index if needed (when moving down, index shifts)
        if (sourceIndex < targetIndex) {
            targetIndex--;
        }

        // Insert at target position
        icons.Insert(targetIndex, sourceIcon);

        // Refresh cell indices for proper display
        if (dropInfo.TargetItem is BannerIconEntry targetEntry) {
            var group = GetGroupFromIcon(targetEntry, dropInfo);
            group?.RefreshCellIndex();
        }
    }

    /// <summary>
    /// Helper to get the group that contains the icon
    /// This is needed to refresh the cell indices
    /// </summary>
    private BannerGroupEntry? GetGroupFromIcon(BannerIconEntry icon, IDropInfo dropInfo) {
        // The drop target is part of a BannerGroupEntry
        // We can traverse up the visual tree or access through the data context
        // For now, we'll rely on the fact that the icon knows its position
        // and the icons collection parent will handle refreshing
        return null; // Will be handled by the collection change event
    }
}
