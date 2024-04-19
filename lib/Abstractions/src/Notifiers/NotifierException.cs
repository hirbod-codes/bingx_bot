namespace Abstractions.src.Notifiers;

[System.Serializable]
public class NotifierException : System.Exception
{
    public NotifierException() { }
    public NotifierException(string message) : base(message) { }
    public NotifierException(string message, System.Exception inner) : base(message, inner) { }
    protected NotifierException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
