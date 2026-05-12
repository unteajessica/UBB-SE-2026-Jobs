using System.Text.Json.Serialization;

namespace PussyCats.Library.Domain;

public class Document
{
    public int DocumentId { get; set; }

    public User User { get; set; } = null!;

    public string DocumentName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
}
