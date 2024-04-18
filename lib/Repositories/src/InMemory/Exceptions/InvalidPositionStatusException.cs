using System.Runtime.Serialization;
using Abstractions.src.Repository;

namespace Repositories.src.InMemory.Exceptions;

[Serializable]
public class InvalidPositionStatusException : DataException
{
    public InvalidPositionStatusException() { }

    public InvalidPositionStatusException(string message) : base(message) { }

    public InvalidPositionStatusException(string message, Exception innerException) : base(message, innerException) { }

    protected InvalidPositionStatusException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
