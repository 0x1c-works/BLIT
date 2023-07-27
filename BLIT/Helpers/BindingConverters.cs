using ReactiveUI;
using System;
using System.Windows;

namespace BLIT.Helpers;

public class BooleanToVisibilityConverter : IBindingTypeConverter
{
    public int GetAffinityForObjects(Type fromType, Type toType)
    {
        if (fromType == typeof(bool) && toType == typeof(bool))
        {
            return 100;
        }
        if (fromType == typeof(bool) && toType == typeof(Visibility))
        {
            return 2;
        }
        return 0;
    }

    public bool TryConvert(object? from, Type toType, object? conversionHint, out object? result)
    {
        result = Visibility.Collapsed;
        if (from is not bool b) return false;
        if (toType != typeof(Visibility)) return false;
        if (b)
        {
            result = Visibility.Visible;
        }
        else if (conversionHint is Visibility.Hidden)
        {
            result = Visibility.Hidden;
        }
        else
        {
            result = Visibility.Collapsed;
        }
        return true;
    }
}
