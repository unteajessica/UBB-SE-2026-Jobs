using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Configuration;
using PussyCats.App.Services;
using PussyCats.Library.Domain;

namespace PussyCats.App.ViewModels;

public class UploadDocumentViewModel : DispatchableObservableObject
{
    private readonly IDocumentService documentService;
    private readonly SessionContext session;
    private string documentName = string.Empty;
    private string selectedFilePath = string.Empty;
    private string errorMessage = string.Empty;

    public UploadDocumentViewModel(IDocumentService documentService, SessionContext session)
    {
        this.documentService = documentService;
        this.session = session;
    }

    public string DocumentName
    {
        get => documentName;
        set => SetProperty(ref documentName, value);
    }

    public string SelectedFilePath
    {
        get => selectedFilePath;
        set => SetProperty(ref selectedFilePath, value);
    }

    public string ErrorMessage
    {
        get => errorMessage;
        private set => SetProperty(ref errorMessage, value);
    }

    public void SetSelectedFilePath(string filePath) => SelectedFilePath = filePath;

    public bool ValidateDocumentInput()
    {
        if (string.IsNullOrWhiteSpace(DocumentName))
        {
            ErrorMessage = "Document name is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(SelectedFilePath))
        {
            ErrorMessage = "Select a file before uploading.";
            return false;
        }

        ErrorMessage = string.Empty;
        return true;
    }

    public async Task UploadDocumentAsync(CancellationToken cancellationToken = default)
    {
        if (!ValidateDocumentInput())
        {
            return;
        }

        var document = new Document
        {
            UserId = ViewModelSupport.ResolveUserId(session),
            DocumentName = DocumentName.Trim(),
        };

        await documentService.UploadDocumentAsync(document, SelectedFilePath, cancellationToken).ConfigureAwait(false);
    }

    public string GetDocumentName() => DocumentName;
    public void SetDocumentName(string documentName) => DocumentName = documentName;
    public string GetErrorMessage() => ErrorMessage;
    public string GetSelectedFilePath() => SelectedFilePath;
}
