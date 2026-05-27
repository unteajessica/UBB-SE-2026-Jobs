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

    public async Task UpdateDocumentNameAsync(int documentId, string newName, CancellationToken cancellationToken = default)
    {
        var document = await documentService.GetByIdAsync(documentId, cancellationToken);
        if (document is null) return;

        document.DocumentName = newName;
        await documentService.UpdateAsync(document, cancellationToken);
        await LoadDocumentsAsync(cancellationToken);
    }
}
