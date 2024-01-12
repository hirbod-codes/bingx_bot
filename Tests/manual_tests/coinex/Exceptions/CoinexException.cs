using System.Runtime.Serialization;

namespace manual_tests.coinex.Exceptions;

[Serializable]
internal class CoinexException : Exception
{
    public CoinexException()
    {
    }

    public CoinexException(string? message) : base(message)
    {
    }

    public CoinexException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected CoinexException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
