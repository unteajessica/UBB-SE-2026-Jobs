using PussyCats.Library.Domain;

namespace PussyCats_App.Services.PdfExportService;

public interface IPdfExportService
{
    Task RenderProfileAsync(User profile);
    Task DownloadPdfAsync();
}
