using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using PussyCats.Library.Domain;

namespace PussyCats_App.Converters;

public class IsCurrentUserMessageConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is not Message message)
        {
            return Visibility.Collapsed;
        }

        var currentSenderId = ChatDisplayResolver.GetCurrentSenderId();
        return message.SenderId == currentSenderId ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}
