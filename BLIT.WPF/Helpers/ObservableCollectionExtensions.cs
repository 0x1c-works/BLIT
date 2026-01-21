using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace BLIT.WPF.Helpers;

public static class ObservableCollectionExtensions {
    /// <summary>
    /// Performs a stable sort on an ObservableCollection using the specified comparison function.
    /// </summary>
    public static void SortStable<T>(this ObservableCollection<T> collection, Comparison<T> comparison) {
        var sorted = collection.OrderBy(x => x, new ComparisonComparer<T>(comparison)).ToList();
        for (int i = 0; i < sorted.Count; i++) {
            collection.Move(collection.IndexOf(sorted[i]), i);
        }
    }

    private class ComparisonComparer<T> : System.Collections.Generic.IComparer<T> {
        private readonly Comparison<T> _comparison;

        public ComparisonComparer(Comparison<T> comparison) {
            _comparison = comparison;
        }

        public int Compare(T? x, T? y) {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return _comparison(x, y);
        }
    }
}
