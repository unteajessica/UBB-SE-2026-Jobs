using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using PussyCats.App.ViewModels;
using PussyCats_App.Services.PdfExportService;

namespace PussyCats_App.Views.Candidate;

public sealed partial class ExportCVPage : Page
{
    private ExportCVViewModel? viewModel;

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
        loadingRing.IsActive = true;

        await CvWebView.EnsureCoreWebView2Async();

        var templateFolder = Path.Combine(AppContext.BaseDirectory, "resources");
        CvWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            "assets.local", templateFolder, CoreWebView2HostResourceAccessKind.Allow);

        var pdfService = new PdfExportService(CvWebView);

        viewModel = App.Services.GetRequiredService<ExportCVViewModel>();
        viewModel.AttachExportService(pdfService);
        DataContext = viewModel;

        statusText.SetBinding(TextBlock.TextProperty,
            new Microsoft.UI.Xaml.Data.Binding { Path = new Microsoft.UI.Xaml.PropertyPath("StatusText"), Source = viewModel });

        await viewModel.LoadAndRenderCVAsync();
        loadingRing.IsActive = false;
    }

    private async void OnDownloadClick(object sender, RoutedEventArgs eventArguments)
    {
        if (viewModel is null) return;
        loadingRing.IsActive = true;
        await viewModel.ExportCVCommand.ExecuteAsync(null);
        loadingRing.IsActive = false;
    }
}
