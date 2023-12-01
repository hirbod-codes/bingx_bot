using System.Runtime.Serialization;

namespace bingx_test.Exceptions;

[Serializable]
public class OpenOrderException : Exception
{
    public OpenOrderException()
    {
    }

    public OpenOrderException(string? message) : base(message)
    {
    }

    public OpenOrderException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected OpenOrderException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
