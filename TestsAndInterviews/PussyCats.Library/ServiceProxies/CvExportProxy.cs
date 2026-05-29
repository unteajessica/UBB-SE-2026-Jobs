namespace PussyCats.Library.ServiceProxies;

public class CvExportProxy
{
    private readonly HttpClient http;

    public CvExportProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<string> GetHtmlAsync(int userId, CancellationToken cancellationToken = default)
        => await http.GetStringAsync($"api/users/{userId}/cv/html", cancellationToken);

    public Task<byte[]> GetPdfBytesAsync(int userId, CancellationToken cancellationToken = default)
        => http.GetByteArrayAsync($"api/users/{userId}/cv/pdf", cancellationToken);
}
