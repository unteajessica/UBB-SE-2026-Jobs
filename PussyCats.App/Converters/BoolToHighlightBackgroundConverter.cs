using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace PussyCats_App.Converters;

public class BoolToHighlightBackgroundConverter : IValueConverter
{
    private static readonly SolidColorBrush HighlightBrush =
        new(Color.FromArgb(30, 98, 0, 238));
    private static readonly SolidColorBrush TransparentBrush =
        new(Color.FromArgb(0, 0, 0, 0));

    public object Convert(object? value, Type targetType, object? parameter, string language)
        => value is true ? HighlightBrush : TransparentBrush;

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}
