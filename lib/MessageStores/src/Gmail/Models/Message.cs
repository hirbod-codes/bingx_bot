using Abstractions.src.MessageStores;

namespace MessageStores.src.Gmail.Models;

public class Message : IMessage
{
    public string Id { get; set; } = null!;
    public string ETag { get; set; } = null!;
    public string To { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string From { get; set; } = null!;
    public string Body { get; set; } = null!;
    public DateTime SentAt { get; set; }
}
