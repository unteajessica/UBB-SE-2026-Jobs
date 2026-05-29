using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Tests_and_Interviews.Services;
using Tests_and_Interviews.ViewModels;
using Windows.Storage.Streams;

namespace Tests_and_Interviews.Views;

public sealed partial class ViewProfilePage : Page
{
    private const int DefaultCompanyIdFallback = 1;
    private const int StreamSeekStartPosition = 0;

    public CompanyProfileViewModel ViewModel { get; }

    public ViewProfilePage()
    {
        var mainWindow = App.MainWindow;
        ViewModel = new CompanyProfileViewModel(mainWindow.CompanyService, new ProfileCompletionCalculator(), mainWindow.GameService, mainWindow.EventsService, mainWindow.SessionService, mainWindow.CollabsService, mainWindow.JobsService);

        ViewModel.OnProfileImageDecoded = SetupProfileImage;
        ViewModel.OnProfileImageCleared = ClearProfileImage;
        ViewModel.OnLogoDecoded = SetupLogoImage;
        ViewModel.OnLogoCleared = ClearLogoImage;

        this.InitializeComponent();
        DataContext = ViewModel;

        ViewModel.NavigateEditProfileRequested += (_, _) =>
        {
            App.MainWindow.RootFrame.Navigate(typeof(EditProfilePage), ViewModel.CompanyId);
        };
        ViewModel.NavigateAllEventsRequested += (_, _) =>
        {
            App.MainWindow.RootFrame.Navigate(typeof(OurEventsPage), ViewModel.CompanyId);
        };
        ViewModel.NavigateAllJobsRequested += (_, _) =>
        {
            App.MainWindow.RootFrame.Navigate(typeof(OurJobsPage), ViewModel.CompanyId);
        };
        ViewModel.NavigateAllCollaboratorRequested += (_, _) =>
        {
            App.MainWindow.RootFrame.Navigate(typeof(CollaboratorsPage), ViewModel.CompanyId);
        };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var id = e.Parameter is int companyId ? companyId : DefaultCompanyIdFallback;
        await ViewModel.Load(id);
    }

    private async void SetupProfileImage(byte[] bytes)
    {
        var bitmap = new BitmapImage();
        using (var mem = new InMemoryRandomAccessStream())
        {
            await mem.WriteAsync(bytes.AsBuffer());
            mem.Seek(StreamSeekStartPosition);
            bitmap.SetSource(mem);
        }
        ProfilePictureBrush.ImageSource = bitmap;
    }

    private void ClearProfileImage()
    {
        ProfilePictureBrush.ImageSource = null;
    }

    private async void SetupLogoImage(byte[] bytes)
    {
        var bitmap = new BitmapImage();
        using (var mem = new InMemoryRandomAccessStream())
        {
            await mem.WriteAsync(bytes.AsBuffer());
            mem.Seek(StreamSeekStartPosition);
            bitmap.SetSource(mem);
        }

        CompanyLogoBrush.ImageSource = bitmap;
    }

    private void ClearLogoImage()
    {
        CompanyLogoBrush.ImageSource = null;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        App.MainWindow.ReturnToMainMenu();
    }
}
