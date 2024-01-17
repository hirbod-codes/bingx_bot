using System.Runtime.Serialization;

namespace providers.src.Providers;

[Serializable]
internal class InvalidIndicatorException : Exception
{
    public InvalidIndicatorException() { }

    public InvalidIndicatorException(string message) : base(message) { }

    public InvalidIndicatorException(string message, Exception innerException) : base(message, innerException) { }

    protected InvalidIndicatorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
