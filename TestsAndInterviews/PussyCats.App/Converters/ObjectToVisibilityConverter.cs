using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace PussyCats_App.Converters;

public class ObjectToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
        => value is not null and not "" ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}
