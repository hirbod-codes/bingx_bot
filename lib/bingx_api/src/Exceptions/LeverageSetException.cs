using System.Runtime.Serialization;

namespace bingx_api.Exceptions;

[Serializable]
public class LeverageSetException : Exception
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
