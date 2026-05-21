using System.Text.Json;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using PussyCats.Library.Domain;
using Windows.Storage.Pickers;

namespace PussyCats_App.Services.PdfExportService;

public class PdfExportService : IPdfExportService
{
    private readonly WebView2 webView;
    private User? currentProfile;
    private const int RenderDelayMilliseconds = 500;
    private const string TemplateUrl = "http://assets.local/CVHtmlTemplate.html";

    public PdfExportService(WebView2 webView)
    {
        this.webView = webView;
    }

    public async Task RenderProfileAsync(User profile)
    {
        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile), "Profile cannot be null.");
        }

        currentProfile = profile;

        webView.Source = new Uri(TemplateUrl);
        await WaitForNavigationAsync();

        var json = JsonSerializer.Serialize(currentProfile, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await webView.ExecuteScriptAsync($"CVGenerator.generateProfile({json});");

        // Wait for DOM updates to settle
        await Task.Delay(RenderDelayMilliseconds);
    }

    public async Task DownloadPdfAsync()
    {
        if (currentProfile == null)
        {
            throw new InvalidOperationException("Profile has not been rendered yet.");
        }

        var savePicker = new FileSavePicker();
        savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        savePicker.SuggestedFileName = BuildFileName(currentProfile);
        savePicker.FileTypeChoices.Add("PDF Document", new[] { ".pdf" });

        var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(PussyCats_App.App.MainAppWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, windowHandle);

        var file = await savePicker.PickSaveFileAsync();
        if (file == null)
        {
            return;
        }

        // Print via Chromium's native renderer
        var success = await webView.CoreWebView2.PrintToPdfAsync(file.Path, null);
        if (!success)
        {
            throw new InvalidOperationException("PDF generation failed. Please try again.");
        }
    }

    private Task WaitForNavigationAsync()
    {
        var navigationTaskCompletionSource = new TaskCompletionSource<bool>();
        void Handler(WebView2 sender, CoreWebView2NavigationCompletedEventArgs eventArgs)
        {
            webView.NavigationCompleted -= Handler;
            navigationTaskCompletionSource.SetResult(true);
        }
        webView.NavigationCompleted += Handler;
        return navigationTaskCompletionSource.Task;
    }

    private string BuildFileName(User profile)
    {
        var firstName = string.IsNullOrWhiteSpace(profile.FirstName) ? "FirstName" : profile.FirstName;
        var lastName = string.IsNullOrWhiteSpace(profile.LastName) ? "LastName" : profile.LastName;
        return $"{firstName}_{lastName}_CV.pdf";
    }
}
