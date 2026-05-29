using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Tests_and_Interviews.ViewModels;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using WinRT.Interop;

namespace Tests_and_Interviews.Views;

public class WinUIImagePickerService : IImagePickerService
{
    private const int StreamSeekStartPosition = 0;
    private const string FileExtensionPng = ".png";
    private const string FileExtensionJpg = ".jpg";
    private const string FileExtensionJpeg = ".jpeg";
    private const string FileExtensionBmp = ".bmp";
    private const string FileExtensionGif = ".gif";

    public async Task<(string FileName, byte[] Bytes)?> PickImageAsync()
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.PicturesLibrary
        };

        picker.FileTypeFilter.Add(FileExtensionPng);
        picker.FileTypeFilter.Add(FileExtensionJpg);
        picker.FileTypeFilter.Add(FileExtensionJpeg);
        picker.FileTypeFilter.Add(FileExtensionBmp);
        picker.FileTypeFilter.Add(FileExtensionGif);

        IntPtr hwnd = WindowNative.GetWindowHandle(App.MainWindow);
        InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSingleFileAsync();
        if (file == null)
        {
            return null;
        }

        byte[] bytes;
        using (var input = await file.OpenReadAsync())
        using (var reader = new DataReader(input.GetInputStreamAt(StreamSeekStartPosition)))
        {
            await reader.LoadAsync((uint)input.Size);
            bytes = new byte[input.Size];
            reader.ReadBytes(bytes);
        }

        return (file.Name, bytes);
    }
}

public sealed partial class EditProfilePage : Page
{
    private const int DefaultCompanyIdFallback = 1;
    private const int StreamSeekStartPosition = 0;

    private const string DialogTitleSaveSuccess = "Profile saved";
    private const string DialogContentSaveSuccess = "Your changes were saved.";
    private const string DialogTitleSaveError = "Could not save";
    private const string DialogButtonOk = "OK";

    private const string DialogTitleCancel = "Discard changes?";
    private const string DialogContentCancel = "Are you sure you want to cancel the modifications?";
    private const string DialogButtonYes = "Yes";
    private const string DialogButtonNo = "No";

    public EditCompanyProfileViewModel ViewModel { get; }

    public EditProfilePage()
    {
        var mainWindow = App.MainWindow;
        var imagePickerService = new WinUIImagePickerService();

        ViewModel = new EditCompanyProfileViewModel(
            mainWindow.CompanyService,
            mainWindow.GameService,
            mainWindow.CompanyValidator,
            mainWindow.GameValidator,
            imagePickerService);

        ViewModel.OnProfilePreviewRequested = SetupProfilePreview;
        ViewModel.OnLogoPreviewRequested = SetupLogoPreview;

        InitializeComponent();
        DataContext = ViewModel;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var id = e.Parameter is int companyId ? companyId : DefaultCompanyIdFallback;
        await ViewModel.Load(id);
    }

    private void NavigateBack_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = App.MainWindow;
        if (mainWindow.RootFrame.CanGoBack)
        {
            mainWindow.RootFrame.GoBack();
        }
        else
        {
            mainWindow.RootFrame.Navigate(typeof(ViewProfilePage), ViewModel.CompanyId);
        }
    }

    private async void SetupProfilePreview(byte[] bytes)
    {
        var bitmapImage = new BitmapImage();
        using (var memStream = new InMemoryRandomAccessStream())
        {
            await memStream.WriteAsync(bytes.AsBuffer());
            memStream.Seek(StreamSeekStartPosition);
            bitmapImage.SetSource(memStream);
        }
        PhotoPreviewImage.Source = bitmapImage;
    }

    private async void SetupLogoPreview(byte[] bytes)
    {
        var bitmapImage = new BitmapImage();
        using (var memStream = new InMemoryRandomAccessStream())
        {
            await memStream.WriteAsync(bytes.AsBuffer());
            memStream.Seek(StreamSeekStartPosition);
            bitmapImage.SetSource(memStream);
        }
        LogoPreviewImage.Source = bitmapImage;
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        var err = await ViewModel.TrySave();
        if (err is null)
        {
            var successDialog = new ContentDialog
            {
                Title = DialogTitleSaveSuccess,
                Content = DialogContentSaveSuccess,
                CloseButtonText = DialogButtonOk,
                XamlRoot = XamlRoot
            };
            await successDialog.ShowAsync();
            App.MainWindow.RootFrame.Navigate(typeof(ViewProfilePage), ViewModel.CompanyId);
            return;
        }

        var errorDialog = new ContentDialog
        {
            Title = DialogTitleSaveError,
            Content = err,
            CloseButtonText = DialogButtonOk,
            XamlRoot = XamlRoot
        };
        await errorDialog.ShowAsync();
    }

    private async void Cancel_Click(object sender, RoutedEventArgs e)
    {
        var confirmDialog = new ContentDialog
        {
            Title = DialogTitleCancel,
            Content = DialogContentCancel,
            PrimaryButtonText = DialogButtonYes,
            CloseButtonText = DialogButtonNo,
            XamlRoot = XamlRoot
        };
        var result = await confirmDialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return;
        }
        NavigateBack_Click(sender, e);
    }
}
