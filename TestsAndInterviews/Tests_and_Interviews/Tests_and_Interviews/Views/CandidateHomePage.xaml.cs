// <copyright file="CandidateHomePage.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.Views
{
    using Microsoft.UI.Xaml.Controls;
    using Tests_and_Interviews.ViewModels;

    /// <summary>
    /// Represents the home page for candidates, providing the user interface and data context for candidate-related
    /// operations.
    /// </summary>
    public sealed partial class CandidateHomePage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CandidateHomePage"/> class and sets up the data context for data binding.
        /// </summary>
        public CandidateHomePage()
        {
            this.InitializeComponent();
            this.ViewModel = new CandidateViewModel();
            this.DataContext = this.ViewModel;
        }

        /// <summary>
        /// Gets the view model containing candidate data and logic for the associated view.
        /// </summary>
        public CandidateViewModel ViewModel { get; }

        private void BackButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            App.MainWindow.ReturnToMainMenu();
        }
    }
}