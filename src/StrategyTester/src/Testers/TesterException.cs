using System.Runtime.Serialization;

namespace StrategyTester.src.Testers;

[Serializable]
public class TesterException : Exception
{
    public TesterException() { }

    public TesterException(string message) : base(message) { }

    public TesterException(string message, Exception innerException) : base(message, innerException) { }

    protected TesterException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
