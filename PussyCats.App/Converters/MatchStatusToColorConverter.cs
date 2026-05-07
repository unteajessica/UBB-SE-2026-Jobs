using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using PussyCats.Library.Domain.Enums;
using Windows.UI;

namespace PussyCats_App.Converters;

public class MatchStatusToColorConverter : IValueConverter
{
    private static readonly Color AcceptedColor = Color.FromArgb(255, 76, 175, 80);
    private static readonly Color RejectedColor  = Color.FromArgb(255, 244, 67, 54);
    private static readonly Color DefaultColor   = Color.FromArgb(255, 33, 150, 243);

    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        var color = value is MatchStatus status
            ? status switch
            {
                MatchStatus.Accepted => AcceptedColor,
                MatchStatus.Rejected => RejectedColor,
                _                   => DefaultColor,
            }
            : DefaultColor;
        return new SolidColorBrush(color);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}
