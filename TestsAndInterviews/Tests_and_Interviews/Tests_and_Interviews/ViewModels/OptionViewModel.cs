namespace Tests_and_Interviews.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Microsoft.UI.Xaml;
    using Tests_and_Interviews.Helpers;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Models.Enums;
    using Tests_and_Interviews.Services;
    using Tests_and_Interviews.Services.Interfaces;

    /// <summary>
    /// OptionViewModel represents a single answer option for a question. It implements INotifyPropertyChanged to support data binding in the UI.
    /// </summary>
    public partial class OptionViewModel : INotifyPropertyChanged
    {
        private bool isSelected;

        /// <summary>
        /// PropertyChanged event is raised whenever a property value changes. This allows the UI to automatically update when the underlying data changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the text of the option. This is the display value that will be shown to the user for this particular answer choice.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the group to which this option belongs. This is particularly useful for single-choice questions where multiple options are part of the same group,
        /// </summary>
        public string GroupName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the index of the option, which can be used to identify the option when processing the user's answer.
        /// For example, in a single-choice question, the index can represent the position of the option in the list of choices.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this option is currently selected by the user.
        /// When the value changes, it raises the PropertyChanged event to update the UI and also invokes the OnSelectionChanged action to notify any 
        /// listeners that the selection state has changed.
        /// </summary>
        public bool IsSelected
        {
            get => this.isSelected;
            set
            {
                this.isSelected = value;
                this.Notify();
                this.OnSelectionChanged?.Invoke();
            }
        }

        /// <summary>
        /// Gets or sets an action that will be called whenever the selection state of this option changes. 
        /// This allows the parent view model to react to changes in the user's selection,
        /// </summary>
        public Action? OnSelectionChanged { get; set; }

        /// <summary>
        /// Notify method is a helper function that raises the PropertyChanged event for a given property name.
        /// It uses the CallerMemberName attribute to automatically capture the name of the calling property, making it easier to call Notify() without
        /// explicitly specifying the property name.
        /// </summary>
        /// <param name="property">The name of the property that changed. This parameter is optional and will be automatically filled by the compiler if not provided.</param>
        private void Notify([CallerMemberName] string property = "") =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }
}