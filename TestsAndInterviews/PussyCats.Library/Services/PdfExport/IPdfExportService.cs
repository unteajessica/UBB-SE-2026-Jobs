using PussyCats.Library.Domain;

namespace PussyCats.Library.Services.PdfExport;

public interface IPdfExportService
{
    Task<string> RenderHtmlAsync(User user);
    Task<byte[]> GeneratePdfAsync(User user);
}
