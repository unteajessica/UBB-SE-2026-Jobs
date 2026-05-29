using System;
using Microsoft.UI.Xaml.Data;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain.Enums;

namespace PussyCats_App.Converters;

public class JobRoleToDisplayNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
        => value is JobRole role ? ViewModelSupport.FormatJobRole(role) : value?.ToString() ?? string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}
