using PussyCats.Library.Domain.Enums;

namespace PussyCats.Library.Domain;

public class Message
{
    public int MessageId { get; set; }
    public int ChatId { get; set; }
    public int SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public MessageType Type { get; set; }
    public bool IsRead { get; set; }
    public bool ShowReadReceipt { get; set; }
    public string SenderInitials { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
}
