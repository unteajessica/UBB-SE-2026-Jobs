using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace PussyCats_App.Converters;

public class BoolToActiveVoteBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is true)
        {
            // "dislike" parameter → red; default → blue
            bool isDislike = parameter is string p && p.Equals("dislike", StringComparison.OrdinalIgnoreCase);
            return isDislike
                ? new SolidColorBrush(Color.FromArgb(0xFF, 0xB4, 0x23, 0x18))
                : new SolidColorBrush(Color.FromArgb(0xFF, 0x1F, 0x6F, 0xEB));
        }

        return new SolidColorBrush(Color.FromArgb(0xFF, 0x69, 0x71, 0x81));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}
