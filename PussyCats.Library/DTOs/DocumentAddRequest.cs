namespace PussyCats.Library.DTOs;

public class DocumentAddRequest
{
    public int UserId { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
}
