using System.IO;
using Microsoft.UI.Xaml.Data;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats_App.Converters;

public class MessageContentDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is not Message message)
        {
            return string.Empty;
        }

        if (message.Type == MessageType.Text)
        {
            return message.Content;
        }

        var displayName = !string.IsNullOrWhiteSpace(message.OriginalFileName)
            ? message.OriginalFileName
            : Path.GetFileName(message.Content);

        if (string.IsNullOrWhiteSpace(displayName))
        {
            return message.Content;
        }

        return message.Type == MessageType.Image
            ? $"IMG {displayName}"
            : $"FILE {displayName}";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}
