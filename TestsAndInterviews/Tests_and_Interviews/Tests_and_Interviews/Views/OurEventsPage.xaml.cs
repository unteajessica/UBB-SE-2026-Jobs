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
using Tests_and_Interviews.Models;
using Tests_and_Interviews.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace Tests_and_Interviews.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class OurEventsPage : Page
{
    public OurEventsViewModel ViewModel { get; }

    /// <summary>
    /// Our events page constructor that initializes its view model
    /// </summary>
public OurEventsPage()
{
    var mainWindow = App.MainWindow;
    InitializeComponent();
    ViewModel = new OurEventsViewModel(mainWindow.EventsService, mainWindow.SessionService);
    this.DataContext = ViewModel;
}

protected override async void OnNavigatedTo(NavigationEventArgs e)
{
    base.OnNavigatedTo(e);
    await ViewModel.LoadEventsAsync();
}

private void NavigateBack_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = App.MainWindow;
        mainWindow.RootFrame.Navigate(typeof(ViewProfilePage));
    }

    /// <summary>
    /// Function that navigates the user to the "Create Event" page when clicking
    /// the button "Create Event"
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CreateEventButton_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = App.MainWindow;
        mainWindow.RootFrame.Navigate(typeof(CreateEventPage));
    }

    private void PastEventsButton_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = App.MainWindow;
        mainWindow.RootFrame.Navigate(typeof(PastEventsPage));
    }

    /// <summary>
    /// Function that navigates the user to the "Edit Event" page when clicking
    /// the 3-dot button next to the event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void EditEvent_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var selectedEvent = button.Tag as Event;

        var mainWindow = App.MainWindow;
        mainWindow.RootFrame.Navigate(typeof(EditEventPage), selectedEvent);
    }

    /// <summary>
    /// Function that navigates the user to the "Collaborators" page when clicking the
    /// "See Collaborators" button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SeeCollaboratorsButton_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = App.MainWindow;
        mainWindow.RootFrame.Navigate(typeof(CollaboratorsPage));
    }
}
