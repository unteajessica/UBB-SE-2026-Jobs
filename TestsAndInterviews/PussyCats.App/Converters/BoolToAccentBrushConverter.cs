using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace PussyCats_App.Converters;

public class BoolToAccentBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush SelectedBrush =
        new(Color.FromArgb(255, 98, 0, 238));
    private static readonly SolidColorBrush DefaultBrush =
        new(Color.FromArgb(255, 200, 200, 200));

    public object Convert(object? value, Type targetType, object? parameter, string language)
        => value is true ? SelectedBrush : DefaultBrush;

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}
