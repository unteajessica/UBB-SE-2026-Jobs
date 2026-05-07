using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats_App.Converters;

public class MessageImageSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is not Message message || message.Type != MessageType.Image || string.IsNullOrWhiteSpace(message.Content))
        {
            return null;
        }

        if (!Uri.TryCreate(message.Content, UriKind.Absolute, out var uri))
        {
            return null;
        }

        return new BitmapImage(uri);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}
