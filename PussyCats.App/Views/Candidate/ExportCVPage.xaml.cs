using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.ViewModels;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace PussyCats_App.Views.Candidate;

public sealed partial class ExportCVPage : Page
{
    private ExportCVViewModel viewModel = null!;

    public ExportCVPage()
    {
        InitializeComponent();
        Loaded += OnPageLoaded;
    }

    protected override void OnNavigatedTo(NavigationEventArgs eventArguments)
    {
        base.OnNavigatedTo(eventArguments);
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs eventArguments)
    {
        Loaded -= OnPageLoaded;
        viewModel = App.Services.GetRequiredService<ExportCVViewModel>();
        DataContext = viewModel;

        loadingRing.IsActive = true;
        statusText.Text = "Loading preview...";

        try
        {
            await CvWebView.EnsureCoreWebView2Async();
            var html = await viewModel.GetPreviewHtmlAsync();
            CvWebView.NavigateToString(html);
            statusText.Text = string.Empty;
        }
        catch (Exception exception)
        {
            statusText.Text = $"Preview failed: {exception.Message}";
        }
        finally
        {
            loadingRing.IsActive = false;
        }
    }

    private async void OnDownloadClick(object sender, RoutedEventArgs eventArguments)
    {
        if (viewModel is null) return;

        loadingRing.IsActive = true;
        statusText.Text = "Generating PDF...";

        try
        {
            var pdfBytes = await viewModel.GetPdfBytesAsync();

            var savePicker = new FileSavePicker { SuggestedFileName = "CV" };
            savePicker.FileTypeChoices.Add("PDF", new List<string> { ".pdf" });
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainAppWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            var file = await savePicker.PickSaveFileAsync();
            if (file is null)
            {
                statusText.Text = string.Empty;
                return;
            }

            await FileIO.WriteBytesAsync(file, pdfBytes);
            statusText.Text = "Downloaded successfully!";
        }
        catch (Exception exception)
        {
            statusText.Text = $"Export failed: {exception.Message}";
        }
        finally
        {
            loadingRing.IsActive = false;
        }
    }
}
