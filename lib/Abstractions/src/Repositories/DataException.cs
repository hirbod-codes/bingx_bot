using System.Runtime.Serialization;

namespace Abstractions.src.Repository;

[Serializable]
public class DataException : System.Exception
{
    public DataException() { }

    public DataException(string message) : base(message) { }

    public DataException(string message, System.Exception innerException) : base(message, innerException) { }

    protected DataException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
