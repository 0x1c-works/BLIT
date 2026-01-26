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

        // The collection change event will trigger RefreshCellIndex
        // through BannerGroupEntry._icons_CollectionChanged
        // No need to call it manually here
    }
}
