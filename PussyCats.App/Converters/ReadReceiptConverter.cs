using Microsoft.UI.Xaml.Data;

namespace PussyCats_App.Converters;

public class ReadReceiptConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        return value is bool isRead ? (isRead ? "Seen" : "Delivered") : "Delivered";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}
