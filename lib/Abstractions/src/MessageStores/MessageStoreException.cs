namespace Abstractions.src.MessageStores;

[System.Serializable]
public class MessageStoreException : System.Exception
{
    public MessageStoreException() { }
    public MessageStoreException(string message) : base(message) { }
    public MessageStoreException(string message, System.Exception inner) : base(message, inner) { }
    protected MessageStoreException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
