using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace PussyCats_App.Converters;

/// <summary>
/// Used by PersonalityTestPage RadioButtons: Convert(int, param) → bool (selected?),
/// ConvertBack(bool=true, param) → int (the answer value).
/// </summary>
public class ObjectToIntConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is int intValue && parameter is string paramStr && int.TryParse(paramStr, out int paramInt))
            return intValue == paramInt;
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        if (value is true && parameter is string paramStr && int.TryParse(paramStr, out int paramInt))
            return paramInt;
        return DependencyProperty.UnsetValue;
    }
}
