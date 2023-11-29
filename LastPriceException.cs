using System.Runtime.Serialization;

namespace bingx_test;

[Serializable]
internal class LastPriceException : Exception
{
    public LastPriceException()
    {
    }

    public LastPriceException(string? message) : base(message)
    {
    }

    public LastPriceException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected LastPriceException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}