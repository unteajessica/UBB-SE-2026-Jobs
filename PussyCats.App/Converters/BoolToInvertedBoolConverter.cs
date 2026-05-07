using System;
using Microsoft.UI.Xaml.Data;

namespace PussyCats_App.Converters;

public class BoolToInvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
        => value is not true;

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => value is not true;
}
