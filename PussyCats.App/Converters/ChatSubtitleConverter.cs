using Microsoft.UI.Xaml.Data;
using PussyCats.Library.Domain;

namespace PussyCats_App.Converters;

public class ChatSubtitleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is not Chat chat)
        {
            return string.Empty;
        }

        return chat.IsBlocked ? "Blocked conversation" : string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}
