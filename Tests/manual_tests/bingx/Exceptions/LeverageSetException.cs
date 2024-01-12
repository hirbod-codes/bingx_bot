using System.Runtime.Serialization;

namespace manual_tests.bingx.Exceptions;

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
