using Microsoft.UI.Xaml.Data;
using PussyCats.Library.Domain;

namespace PussyCats_App.Converters;

public class ChatAvatarFgConverter : IValueConverter
{
    private const string DefaultAvatarForegroundColor = "#FF0F4FAD";
    private const string CompanyAvatarForegroundColor = "#FF374151";

    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is not Chat chat)
        {
            return DefaultAvatarForegroundColor;
        }

        return chat.CompanyId.HasValue ? CompanyAvatarForegroundColor : DefaultAvatarForegroundColor;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}
