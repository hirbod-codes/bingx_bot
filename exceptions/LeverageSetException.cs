using System.Runtime.Serialization;

namespace bingx_test.Exceptions;

[Serializable]
internal class LeverageSetException : Exception
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
