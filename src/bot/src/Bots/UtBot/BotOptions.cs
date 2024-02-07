namespace bot.src.Bots.UtBot;

public class BotOptions : IBotOptions
{
    public string Provider { get; set; } = null!;
    public int TimeFrame { get; set; }
    public int BrokerFailureRetryCount { get; set; } = 1;
    public int MessageStoreFailureRetryCount { get; set; } = 2;
}
