// <copyright file="TestCardViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.ViewModels
{
    using Microsoft.UI.Xaml;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    /// <summary>
    /// TestCardViewModel represents the data and state for a single test card in the UI.
    /// It implements INotifyPropertyChanged to support data binding and UI updates when properties change.
    /// The view model includes properties for the test's ID, title, category, question type label, and 
    /// visual states for selection and hover effects.
    /// It also provides computed properties for the card's border thickness and brush based on its current state.
    /// </summary>
    public partial class TestCardViewModel : INotifyPropertyChanged
    {
        private bool isSelected;
        private bool isHovered;


        /// <summary>
        /// PropertyChanged event is raised whenever a property value changes, allowing the UI to update accordingly.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the unique identifier for the test.
        /// This property is used to identify the test associated with this view model instance.
        /// </summary>
        public int TestId { get; set; }

        /// <summary>
        /// Gets or sets the title of the test.
        /// This property is used to display the name of the test in the UI and can be updated to reflect changes in the test's title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the category of the test.
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the label for the question type associated with the test.
        /// </summary>
        public string QuestionTypeLabel { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether isSelected indicates whether the test card is currently selected in the UI.
        /// </summary>
        public bool IsSelected
        {
            get => this.isSelected;
            set
            {
                this.isSelected = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.CardBorderThickness));
                this.OnPropertyChanged(nameof(this.CardBorderBrush));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether isHovered indicates whether the mouse pointer is currently hovering over the test card in the UI.
        /// </summary>
        public bool IsHovered
        {
            get => this.isHovered;
            set
            {
                this.isHovered = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.CardBorderThickness));
                this.OnPropertyChanged(nameof(this.CardBorderBrush));
            }
        }

        /// <summary>
        /// Gets the appropriate border thickness for the test card based on its current state.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public Microsoft.UI.Xaml.Thickness CardBorderThickness =>
            this.IsSelected || this.IsHovered
                ? new Microsoft.UI.Xaml.Thickness(2.5)
                : new Microsoft.UI.Xaml.Thickness(1);

        /// <summary>
        /// Gets the appropriate border brush for the test card based on its current state, using different colors for selected, hovered, and default states.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public Microsoft.UI.Xaml.Media.SolidColorBrush CardBorderBrush =>
            this.IsSelected
                ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 132, 148, 255))
                : this.IsHovered
                    ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 30, 30, 30))
                    : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 232, 228, 255));

        /// <summary>
        /// OnPropertyChanged method is a helper method that raises the PropertyChanged event for a given property name.
        /// </summary>
        /// <param name="name">The name of the property that changed. This parameter is optional and defaults to the caller member name.</param>
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
