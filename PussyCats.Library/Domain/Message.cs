using PussyCats.Library.Domain.Enums;

namespace PussyCats.Library.Domain;

public class Message
{
    public int MessageId { get; set; }
    public Chat Chat { get; set; } = null!;
    public MessageSender Sender { get; set; } = new();
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public MessageType Type { get; set; }
    public bool IsRead { get; set; }
    public bool ShowReadReceipt { get; set; }
    public string SenderInitials { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
}
