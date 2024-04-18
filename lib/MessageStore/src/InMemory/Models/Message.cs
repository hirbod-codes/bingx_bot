using Abstractions.src.Models;

namespace MessageStores.src.InMemory.Models;

public class Message : IMessage
{
    public string Id { get; set; } = null!;
    public string From { get; set; } = null!;
    public string Body { get; set; } = null!;
    public DateTime SentAt { get; set; }
}
