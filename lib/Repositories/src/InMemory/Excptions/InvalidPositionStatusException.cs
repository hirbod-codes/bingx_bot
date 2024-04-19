using System.Runtime.Serialization;

namespace Data.src.InMemory.Exceptions;

[Serializable]
public class InvalidPositionStatusException : Exception
{
    public InvalidPositionStatusException() { }

    public InvalidPositionStatusException(string message) : base(message) { }

    public InvalidPositionStatusException(string message, Exception innerException) : base(message, innerException) { }

    protected InvalidPositionStatusException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
