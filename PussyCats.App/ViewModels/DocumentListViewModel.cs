using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Configuration;
using PussyCats.App.Services;
using PussyCats.Library.Domain;

namespace PussyCats.App.ViewModels;

public class DocumentListViewModel : ObservableObject
{
    private readonly IDocumentService documentService;
    private readonly SessionContext session;
    private List<Document> documents = new();
    private string statusMessage = string.Empty;

    public DocumentListViewModel(IDocumentService documentService, SessionContext session)
    {
        this.documentService = documentService;
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

    public async Task LoadDocumentsAsync(CancellationToken ct = default)
    {
        Documents = (await documentService
            .GetDocumentsByUserIdAsync(ViewModelSupport.ResolveUserId(session), ct)
            .ConfigureAwait(false)).ToList();
    }

    public List<Document> GetDocuments() => Documents;

    public async Task DeleteDocumentAsync(int documentId, CancellationToken ct = default)
    {
        await documentService.DeleteDocumentAsync(documentId, ct).ConfigureAwait(false);
        await LoadDocumentsAsync(ct).ConfigureAwait(false);
    }

    public async Task<string?> GetResolvedFilePathAsync(int documentId, CancellationToken ct = default)
    {
        try
        {
            var fullPath = await documentService.GetDocumentAbsolutePathAsync(documentId, ct).ConfigureAwait(false);
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
