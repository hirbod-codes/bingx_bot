using System.Runtime.Serialization;

namespace broker_api.Exceptions;

[Serializable]
public class LeverageSetException : BingxApiException
{
    public LeverageSetException()
    {
    }

    public LeverageSetException(string? message) : base(message)
    {
    }

    public LeverageSetException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected LeverageSetException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
