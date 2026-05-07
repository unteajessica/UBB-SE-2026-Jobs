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

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        LoadGrid();
    }

    private async void LoadGrid()
    {
        await listViewModel.LoadDocumentsAsync();
        var docs = listViewModel.GetDocuments();
        listViewDocuments.ItemsSource = docs;
        noDocumentsLabel.Visibility = docs.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnDocumentNameChanged(object sender, TextChangedEventArgs e)
        => uploadViewModel.SetDocumentName(txtDocumentName.Text);

    private async void OnBrowseClick(object sender, RoutedEventArgs e)
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

    private async void OnUploadClick(object sender, RoutedEventArgs e)
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
            uploadViewModel.SetSelectedFilePath(null);
            LoadGrid();
        }
        catch (Exception ex)
        {
            uploadErrorLabel.Text       = ex.Message;
            uploadErrorLabel.Visibility = Visibility.Visible;
        }
    }

    private async void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: Document doc }) return;

        var dialog = new ContentDialog
        {
            Title           = "Delete Document",
            Content         = $"Are you sure you want to delete \"{doc.DocumentName}\"?",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            XamlRoot        = XamlRoot,
        };

        if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;

        try
        {
            await listViewModel.DeleteDocumentAsync(doc.DocumentId);
            LoadGrid();
        }
        catch (Exception ex)
        {
            statusLabel.Text       = ex.Message;
            statusLabel.Visibility = Visibility.Visible;
        }
    }

    private async void OnViewClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: Document doc }) return;
        statusLabel.Visibility = Visibility.Collapsed;

        var fullPath = await listViewModel.GetResolvedFilePathAsync(doc.DocumentId) ?? string.Empty;
        var status   = listViewModel.GetStatusMessage();
        if (!string.IsNullOrEmpty(status))
        {
            statusLabel.Text       = status;
            statusLabel.Visibility = Visibility.Visible;
            return;
        }

        if (File.Exists(fullPath))
            Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
        else
        {
            statusLabel.Text       = $"\"{doc.DocumentName}\" could not be found on disk.";
            statusLabel.Visibility = Visibility.Visible;
        }
    }
}
