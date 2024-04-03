namespace bot.src.Bots.DoubleUtBot;

public class BotOptions : IBotOptions
{
    public string Provider { get; set; } = null!;
    public int TimeFrame { get; set; }
    public int BrokerFailureRetryCount { get; set; } = 3;
    public int BrokerFailureRetryInterval { get; set; } = 1000;
    public bool ShouldTerminateAfterBrokerFailure { get; set; } = false;
    public int MessageStoreFailureRetryCount { get; set; } = 3;
    public int MessageStoreFailureRetryInterval { get; set; } = 1000;
    public bool ShouldTerminateAfterMessageStoreFailure { get; set; } = false;
}
