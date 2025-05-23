using System.Runtime.Serialization;

namespace Brokers.src.Bingx.Exceptions;

[Serializable]
public class AccountBalanceException : BingxException
{
    public AccountBalanceException() { }

    public AccountBalanceException(string message) : base(message) { }

    public AccountBalanceException(string message, Exception innerException) : base(message, innerException) { }

    protected AccountBalanceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
