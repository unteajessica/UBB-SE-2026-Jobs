using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats_App.Converters;

public class MessageTextVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is not Message message)
        {
            return Visibility.Collapsed;
        }

        return message.Type == MessageType.Text ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}
