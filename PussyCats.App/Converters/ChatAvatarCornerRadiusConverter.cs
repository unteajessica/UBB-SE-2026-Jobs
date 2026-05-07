using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using PussyCats.Library.Domain;

namespace PussyCats_App.Converters;

public class ChatAvatarCornerRadiusConverter : IValueConverter
{
    private const double CompanyAvatarCornerRadius = 8;
    private const double CircularAvatarCornerRadius = 999;

    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is not Chat chat)
        {
            return new CornerRadius(CircularAvatarCornerRadius);
        }

        return chat.CompanyId.HasValue
            ? new CornerRadius(CompanyAvatarCornerRadius)
            : new CornerRadius(CircularAvatarCornerRadius);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}
