using Microsoft.UI.Xaml.Data;
using PussyCats.Library.Domain;

namespace PussyCats_App.Converters;

public class ChatAvatarBgConverter : IValueConverter
{
    private const string DefaultAvatarBackgroundColor = "#FFE8EEF8";
    private const string CompanyAvatarBackgroundColor = "#FFF3F4F6";

    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is not Chat chat)
        {
            return DefaultAvatarBackgroundColor;
        }

        return chat.Company!=null ? CompanyAvatarBackgroundColor : DefaultAvatarBackgroundColor;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}
