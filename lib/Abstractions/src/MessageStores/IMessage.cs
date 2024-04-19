namespace Abstractions.src.MessageStores;

public interface IMessage
{
    public string Id { get; set; }
    public string From { get; set; }
    public string Body { get; set; }
    public DateTime SentAt { get; set; }
}
