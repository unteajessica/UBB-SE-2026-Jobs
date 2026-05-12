using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Configuration;
using PussyCats.App.Services;
using PussyCats.Library.Domain;

namespace PussyCats.App.ViewModels;

public class UploadDocumentViewModel : DispatchableObservableObject
{
    private readonly IDocumentService documentService;
    private readonly IUserService userService;
    private readonly SessionContext session;
    private string documentName = string.Empty;
    private string selectedFilePath = string.Empty;
    private string errorMessage = string.Empty;

    public UploadDocumentViewModel(IDocumentService documentService, IUserService userService, SessionContext session)
    {
        this.documentService = documentService;
        this.userService = userService;
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

        var userId = ViewModelSupport.ResolveUserId(session);
        var user = await userService.GetByIdAsync(userId, cancellationToken)
            ?? new User { UserId = userId };

        var document = new Document
        {
            User = user,
            DocumentName = DocumentName.Trim(),
        };

        await documentService.UploadDocumentAsync(document, SelectedFilePath, cancellationToken);
    }

    public string GetDocumentName() => DocumentName;
    public void SetDocumentName(string documentName) => DocumentName = documentName;
    public string GetErrorMessage() => ErrorMessage;
    public string GetSelectedFilePath() => SelectedFilePath;
}
