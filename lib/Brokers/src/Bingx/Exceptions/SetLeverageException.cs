using System.Runtime.Serialization;

namespace Brokers.src.Bingx.Exceptions;

[Serializable]
public class SetLeverageException : BingxException
{
    public SetLeverageException() { }

    public SetLeverageException(string message) : base(message) { }

    public SetLeverageException(string message, Exception innerException) : base(message, innerException) { }

    protected SetLeverageException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
