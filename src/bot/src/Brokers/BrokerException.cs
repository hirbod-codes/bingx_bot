namespace bot.src.Brokers;

[System.Serializable]
public class BrokerException : System.Exception
{
    public BrokerException() { }
    public BrokerException(string message) : base(message) { }
    public BrokerException(string message, System.Exception inner) : base(message, inner) { }
    protected BrokerException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
