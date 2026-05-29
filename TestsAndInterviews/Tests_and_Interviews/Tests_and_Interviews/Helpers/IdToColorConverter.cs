// <copyright file="IdToColorConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.Helpers
{
    using System;
    using Microsoft.UI;
    using Microsoft.UI.Xaml.Data;
    using Microsoft.UI.Xaml.Media;
    using Tests_and_Interviews.Models.Enums;
    using Windows.UI;

    /// <summary>
    /// Converts a SlotStatus value to a SolidColorBrush for UI representation.
    /// </summary>
    public class IdToColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts a SlotStatus value to a SolidColorBrush for UI representation.
        /// </summary>
        /// <param name="value">The value to convert, expected to be a SlotStatus.</param>
        /// <param name="targetType">The type to convert to (not used).</param>
        /// <param name="parameter">An optional parameter for the converter (not used).</param>
        /// <param name="language">The language to use in the converter (not used).</param>
        /// <returns>A SolidColorBrush corresponding to the SlotStatus value, or a default color if the value is not a
        /// SlotStatus.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is SlotStatus status)
            {
                return status == SlotStatus.Occupied
                    ? new SolidColorBrush(Color.FromArgb(255, 99, 102, 255))
                    : new SolidColorBrush(Color.FromArgb(255, 206, 213, 255));
            }

            return new SolidColorBrush(Colors.LightGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => null;
    }
}