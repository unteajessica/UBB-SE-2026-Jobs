using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace PussyCats_App.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        bool isVisible = value is true;
        if (parameter is string p && p == "Inverse")
            isVisible = !isVisible;
        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        bool isVisible = value is Visibility v && v == Visibility.Visible;
        if (parameter is string p && p == "Inverse")
            isVisible = !isVisible;
        return isVisible;
    }
}
