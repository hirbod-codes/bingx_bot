namespace Abstractions.src.src;

[System.Serializable]
public class NetworkException : System.Exception
{
    public NetworkException() { }
    public NetworkException(string message) : base(message) { }
    public NetworkException(string message, System.Exception inner) : base(message, inner) { }
    protected NetworkException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
