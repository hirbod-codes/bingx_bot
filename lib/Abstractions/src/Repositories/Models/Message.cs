using Abstractions.src.MessageStores;

namespace Abstractions.src.Data.Models;

public class Message : IMessage
{
    public string Id { get; set; } = null!;
    public string From { get; set; } = null!;
    public string Body { get; set; } = null!;
    public DateTime SentAt { get; set; }
}
