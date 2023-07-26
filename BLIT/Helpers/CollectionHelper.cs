using System;
using System.Collections.Generic;

namespace BLIT.Helpers;
public static class CollectionHelper
{
    /// <summary>
    /// Copied from
    /// https://github.com/CommunityToolkit/ColorCode-Universal/blob/34878acb21e7688d37b6dd8acaee29feb9fbfc85/ColorCode.Core/Common/ExtensionMethods.cs#L12
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="comparison"></param>
    public static void SortStable<T>(this IList<T> list,
                                     Comparison<T> comparison)
    {
        var count = list.Count;

        for (var j = 1; j < count; j++)
        {
            T key = list[j];

            var i = j - 1;
            for (; i >= 0 && comparison(list[i], key) > 0; i--)
            {
                list[i + 1] = list[i];
            }

            list[i + 1] = key;
        }
    }
}
