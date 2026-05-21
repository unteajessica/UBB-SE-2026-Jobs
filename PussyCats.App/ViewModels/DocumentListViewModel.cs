using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Configuration;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.Documents;

namespace PussyCats.App.ViewModels;

public class DocumentListViewModel : DispatchableObservableObject
{
    private readonly IDocumentService documentService;
    private readonly ILocalDocumentFileService localDocumentFileService;
    private readonly SessionContext session;
    private List<Document> documents = new();
    private string statusMessage = string.Empty;

    public DocumentListViewModel(
        IDocumentService documentService,
        ILocalDocumentFileService localDocumentFileService,
        SessionContext session)
    {
        this.documentService = documentService;
        this.localDocumentFileService = localDocumentFileService;
        this.session = session;
    }

    public List<Document> Documents
    {
        get => documents;
        private set => SetProperty(ref documents, value);
    }

    public string StatusMessage
    {
        get => statusMessage;
        private set => SetProperty(ref statusMessage, value);
    }

    public async Task LoadDocumentsAsync(CancellationToken cancellationToken = default)
    {
        Documents = (await documentService
            .GetDocumentsByUserIdAsync(ViewModelSupport.ResolveUserId(session), cancellationToken)
            ).ToList();
    }

    public List<Document> GetDocuments() => Documents;

    public async Task DeleteDocumentAsync(int documentId, CancellationToken cancellationToken = default)
    {
        await localDocumentFileService.DeleteDocumentAsync(documentId, cancellationToken);
        await LoadDocumentsAsync(cancellationToken);
    }

    public async Task<string?> GetResolvedFilePathAsync(int documentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = await localDocumentFileService.GetDocumentAbsolutePathAsync(documentId, cancellationToken);
            StatusMessage = string.Empty;
            return fullPath;
        }
        catch
        {
            StatusMessage = "The file could not be found on disk.";
            return null;
        }
    }

    public string GetStatusMessage() => StatusMessage;
}
