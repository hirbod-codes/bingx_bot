using System.Runtime.Serialization;

namespace Abstractions.src.Runners;

[Serializable]
public class RunnerException : Exception
{
    public RunnerException() { }

    public RunnerException(string message) : base(message) { }

    public RunnerException(string message, Exception innerException) : base(message, innerException) { }

    protected RunnerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
