// <copyright file="DateTimeToStringConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.Helpers
{
    using System;
    using Microsoft.UI.Xaml.Data;

    /// <summary>
    /// Converts a <see cref="DateTime"/> value to a formatted string for use in XAML bindings.
    /// </summary>
    public class DateTimeToStringConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="DateTime"/> value to a localized formatted string.
        /// </summary>
        /// <param name="value">The <see cref="DateTime"/> value to convert.</param>
        /// <param name="targetType">The target type of the binding. Not used.</param>
        /// <param name="parameter">An optional parameter. Not used.</param>
        /// <param name="language">The language of the conversion. Not used.</param>
        /// <returns>A string formatted as "MMM dd, yyyy h:mm tt" in local time if the value is a <see cref="DateTime"/>; otherwise an empty string.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime dt)
            {
                return dt.ToLocalTime().ToString("MMM dd, yyyy h:mm tt");
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns the value unchanged. This converter does not support two-way binding.
        /// </summary>
        /// <param name="value">The value to return.</param>
        /// <param name="targetType">The target type of the binding. Not used.</param>
        /// <param name="parameter">An optional parameter. Not used.</param>
        /// <param name="language">The language of the conversion. Not used.</param>
        /// <returns>The original value unchanged.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
