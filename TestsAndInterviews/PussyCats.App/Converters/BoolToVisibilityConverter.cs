using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace PussyCats_App.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        bool isVisible = value is true;
        if (parameter is string parameterString && parameterString == "Inverse")
            isVisible = !isVisible;
        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        bool isVisible = value is Visibility visibility && visibility == Visibility.Visible;
        if (parameter is string parameterString && parameterString == "Inverse")
            isVisible = !isVisible;
        return isVisible;
    }
}
