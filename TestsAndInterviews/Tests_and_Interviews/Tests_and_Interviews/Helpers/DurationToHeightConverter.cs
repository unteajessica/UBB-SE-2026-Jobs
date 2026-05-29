// <copyright file="DurationToHeightConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.Helpers
{
    using System;
    using Microsoft.UI.Xaml.Data;

    /// <summary>
    /// Converts a duration in minutes to a pixel height value for use in XAML bindings.
    /// </summary>
    public class DurationToHeightConverter : IValueConverter
    {
        /// <summary>
        /// Converts a duration in minutes to a corresponding pixel height.
        /// </summary>
        /// <param name="value">The duration in minutes as an integer.</param>
        /// <param name="targetType">The target type of the binding. Not used.</param>
        /// <param name="parameter">An optional parameter. Not used.</param>
        /// <param name="language">The language of the conversion. Not used.</param>
        /// <returns>150 for a 90-minute duration, 100 for a 60-minute duration, or 50 for any other duration.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int duration = (int)value;

            if (duration == 90) return 150;
            if (duration == 60) return 100;

            return 50;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}