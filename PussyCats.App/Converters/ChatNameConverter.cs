using System.Reflection;
using Microsoft.UI.Xaml.Data;
using PussyCats.Library.Domain;

namespace PussyCats_App.Converters;

public class ChatNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is null)
        {
            return string.Empty;
        }

        if (value is Chat chat)
        {
            return ChatDisplayResolver.ResolveChatName(chat);
        }

        if (value is User user)
        {
            return user.Name;
        }

        if (value is Company company)
        {
            return company.CompanyName;
        }

        var type = value.GetType();
        var nameProperty = type.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
        if (nameProperty?.GetValue(value) is string name && !string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        var companyNameProperty = type.GetProperty("CompanyName", BindingFlags.Public | BindingFlags.Instance);
        if (companyNameProperty?.GetValue(value) is string companyName && !string.IsNullOrWhiteSpace(companyName))
        {
            return companyName;
        }

        return value.ToString() ?? string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}
