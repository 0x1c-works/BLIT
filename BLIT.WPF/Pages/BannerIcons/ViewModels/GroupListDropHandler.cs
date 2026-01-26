using BLIT.WPF.Pages.BannerIcons.Models;
using GongSolutions.Wpf.DragDrop;
using System.Collections.ObjectModel;
using System.Windows;

namespace BLIT.WPF.Pages.BannerIcons.ViewModels;

/// <summary>
/// Drag and drop handler for group list
/// Supports reordering groups by dragging within the list
/// </summary>
public class GroupListDropHandler : IDropTarget {
    public void DragOver(IDropInfo dropInfo) {
        // Only accept drops from BannerGroupEntry within the same list
        if (dropInfo.Data is not BannerGroupEntry sourceGroup) {
            dropInfo.Effects = DragDropEffects.None;
            return;
        }

        // Check if we're dropping on a valid target
        if (dropInfo.TargetItem is not BannerGroupEntry targetGroup) {
            dropInfo.Effects = DragDropEffects.None;
            return;
        }

        // Allow move operation
        dropInfo.Effects = DragDropEffects.Move;
        
        // Show highlight adorner to indicate drop position
        dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
    }

    public void Drop(IDropInfo dropInfo) {
        if (dropInfo.Data is not BannerGroupEntry sourceGroup) {
            return;
        }

        if (dropInfo.TargetItem is not BannerGroupEntry targetGroup) {
            return;
        }

        // Get the groups collection from the target
        if (dropInfo.TargetCollection is not ObservableCollection<BannerGroupEntry> groups) {
            return;
        }

        int sourceIndex = groups.IndexOf(sourceGroup);
        int targetIndex = groups.IndexOf(targetGroup);

        // Don't move if source and target are the same
        if (sourceIndex == targetIndex) {
            return;
        }

        // Remove from source position
        groups.RemoveAt(sourceIndex);

        // Adjust target index if needed (when moving down, index shifts)
        if (sourceIndex < targetIndex) {
            targetIndex--;
        }

        // Insert at target position
        groups.Insert(targetIndex, sourceGroup);
    }
}
