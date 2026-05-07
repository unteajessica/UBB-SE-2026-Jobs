using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain;
using Windows.Storage.Pickers;

namespace PussyCats_App.Views.Candidate;

public sealed partial class DocumentsPage : Page
{
    private readonly DocumentListViewModel listViewModel;
    private readonly UploadDocumentViewModel uploadViewModel;

    public DocumentsPage()
    {
        InitializeComponent();
        listViewModel   = App.Services.GetRequiredService<DocumentListViewModel>();
        uploadViewModel = App.Services.GetRequiredService<UploadDocumentViewModel>();
    }

    protected override void OnNavigatedTo(NavigationEventArgs eventArguments)
    {
        base.OnNavigatedTo(eventArguments);
        LoadGrid();
    }

    private async void LoadGrid()
    {
        await listViewModel.LoadDocumentsAsync();
        var documents = listViewModel.GetDocuments();
        listViewDocuments.ItemsSource = documents;
        noDocumentsLabel.Visibility = documents.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnDocumentNameChanged(object sender, TextChangedEventArgs eventArguments)
        => uploadViewModel.SetDocumentName(txtDocumentName.Text);

    private async void OnBrowseClick(object sender, RoutedEventArgs eventArguments)
    {
        var picker = new FileOpenPicker();
        var handle = WinRT.Interop.WindowNative.GetWindowHandle(App.MainAppWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, handle);
        picker.ViewMode = PickerViewMode.List;
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.FileTypeFilter.Add(".pdf");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".png");

        var file = await picker.PickSingleFileAsync();
        if (file is null) return;

        uploadViewModel.SetSelectedFilePath(file.Path);
        selectedFileLabel.Text = file.Name;
    }

    private async void OnUploadClick(object sender, RoutedEventArgs eventArguments)
    {
        uploadErrorLabel.Visibility = Visibility.Collapsed;
        try
        {
            await uploadViewModel.UploadDocumentAsync();
            var error = uploadViewModel.GetErrorMessage();
            if (!string.IsNullOrEmpty(error))
            {
                uploadErrorLabel.Text       = error;
                uploadErrorLabel.Visibility = Visibility.Visible;
                return;
            }

            txtDocumentName.Text   = string.Empty;
            selectedFileLabel.Text = "No file selected";
            uploadViewModel.SetDocumentName(string.Empty);
            uploadViewModel.SetSelectedFilePath(string.Empty);
            LoadGrid();
        }
        catch (Exception ex)
        {
            uploadErrorLabel.Text       = ex.Message;
            uploadErrorLabel.Visibility = Visibility.Visible;
        }
    }

    private async void OnDeleteClick(object sender, RoutedEventArgs eventArguments)
    {
        if (sender is not Button { Tag: Document document }) return;

        var dialog = new ContentDialog
        {
            Title           = "Delete Document",
            Content         = $"Are you sure you want to delete \"{document.DocumentName}\"?",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            XamlRoot        = XamlRoot,
        };

        if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;

        try
        {
            await listViewModel.DeleteDocumentAsync(document.DocumentId);
            LoadGrid();
        }
        catch (Exception ex)
        {
            statusLabel.Text       = ex.Message;
            statusLabel.Visibility = Visibility.Visible;
        }
    }

    private async void OnViewClick(object sender, RoutedEventArgs eventArguments)
    {
        if (sender is not Button { Tag: Document document }) return;
        statusLabel.Visibility = Visibility.Collapsed;

        var fullPath = await listViewModel.GetResolvedFilePathAsync(document.DocumentId) ?? string.Empty;
        var status   = listViewModel.GetStatusMessage();
        if (!string.IsNullOrEmpty(status))
        {
            statusLabel.Text       = status;
            statusLabel.Visibility = Visibility.Visible;
            return;
        }

        if (Uri.TryCreate(fullPath, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
        }
        else if (File.Exists(fullPath))
        {
            Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
        }
        else
        {
            statusLabel.Text       = $"\"{document.DocumentName}\" could not be found on disk.";
            statusLabel.Visibility = Visibility.Visible;
        }
    }
}
