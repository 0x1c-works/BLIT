using System.Collections.ObjectModel;

namespace BLIT.WPF.Helpers;

public static class ObservableCollectionExtensions {
    /// <summary>
    ///     Performs a stable sort on an ObservableCollection using the specified comparison function.
    /// </summary>
    public static void SortStable<T>(this ObservableCollection<T> collection, Comparison<T> comparison) {
        List<T> sorted = collection.OrderBy(x => x, new ComparisonComparer<T>(comparison)).ToList();
        for (var i = 0; i < sorted.Count; i++) {
            collection.Move(collection.IndexOf(sorted[i]), i);
        }
    }

    #region Nested type: ComparisonComparer

    private class ComparisonComparer<T> : IComparer<T> {
        private readonly Comparison<T> _comparison;

        public ComparisonComparer(Comparison<T> comparison) {
            _comparison = comparison;
        }

        #region IComparer<T> Members

        public int Compare(T? x, T? y) {
            if (x == null && y == null) {
                return 0;
            }

            if (x == null) {
                return -1;
            }

            if (y == null) {
                return 1;
            }

            return _comparison(x, y);
        }

        #endregion
    }

    #endregion
}