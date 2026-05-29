using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Tests_and_Interviews.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace Tests_and_Interviews.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CollaboratorsPage : Page
    {
        public CollaboratorsViewModel ViewModel { get; }

        /// <summary>
        /// Collaborators page constructor that initializes its view model
        /// </summary>
        public CollaboratorsPage()
        {
            var mainWindow = App.MainWindow;
            InitializeComponent();
            ViewModel = new CollaboratorsViewModel(mainWindow.CollabsService, mainWindow.SessionService);
            this.DataContext = ViewModel;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.LoadCollaboratorsAsync();
        }

        /// <summary>
        /// Function that navigates back to "Our Events" page when pressing the button "Back"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavigateBack_Click(object sender, RoutedEventArgs e)
        {
            var mainW = App.MainWindow;
            mainW.RootFrame.Navigate(typeof(ViewProfilePage));
        }
    }
}
