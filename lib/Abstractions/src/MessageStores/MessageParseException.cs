namespace Abstractions.src.MessageStores;

[System.Serializable]
public class MessageParseException : MessageStoreException
{
    public MessageParseException() { }
    public MessageParseException(string message) : base(message) { }
    public MessageParseException(string message, System.Exception inner) : base(message, inner) { }
    protected MessageParseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
