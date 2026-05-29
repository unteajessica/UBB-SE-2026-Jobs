using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Tests_and_Interviews.ViewModels;

namespace Tests_and_Interviews.Views;

public sealed partial class CreateJobPage : Page
{
    private const string DialogTitleSuccess = "Success";
    private const string DialogTitleError = "Could not create job";
    private const string DialogButtonOk = "OK";

    public CreateJobViewModel ViewModel { get; }

    public CreateJobPage()
    {
        var mainWindow = App.MainWindow;
        ViewModel = new CreateJobViewModel(mainWindow.JobsService, mainWindow.SessionService);
        ViewModel.OnSaveCompleted = HandleSaveCompleted;

        InitializeComponent();
        DataContext = this;

        Loaded += this.CreateJobPage_Loaded;
    }

    private async void CreateJobPage_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.InitializeAsync();
    }

    private async void HandleSaveCompleted(bool isSaved, string message)
    {
        var resultDialog = new ContentDialog
        {
            Title = isSaved ? DialogTitleSuccess : DialogTitleError,
            Content = message,
            CloseButtonText = DialogButtonOk,
            XamlRoot = XamlRoot
        };
        await resultDialog.ShowAsync();

        if (isSaved && Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.SaveJobAsync();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private void NavigateBack_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }
}
