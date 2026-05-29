// <copyright file="StatusConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.Helpers
{
    using System;
    using Microsoft.UI.Xaml.Data;

    /// <summary>
    /// Converts empty or null application status strings to "Pending" for UI display.
    /// </summary>
    public class StatusConverter : IValueConverter
    {
        private const string FallbackPendingString = "Pending";

        /// <summary>
        /// Converts empty or null status strings to "Pending".
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type to convert to (not used).</param>
        /// <param name="parameter">An optional parameter for the converter (not used).</param>
        /// <param name="language">The language to use in the converter (not used).</param>
        /// <returns>The status string or "Pending" if empty/null.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var stringValue = value as string;
            return string.IsNullOrWhiteSpace(stringValue) ? FallbackPendingString : stringValue;
        }

        /// <summary>
        /// Not implemented for this converter.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
