using bot.src.Bots;
using bot.src.Bots.SuperTrendV1;
using bot.src.Brokers;
using bot.src.Brokers.Bingx;
using bot.src.Indicators;
using bot.src.MessageStores;
using bot.src.RiskManagement;
using bot.src.RiskManagement.SuperTrendV1;
using bot.src.Runners;
using bot.src.Runners.SuperTrendV1;
using bot.src.Strategies;
using bot.src.Strategies.SuperTrendV1;
using bot.src.Indicators.SuperTrendV1;
using bot.src.MessageStores.InMemory.Models;

namespace bot.src.Models;

public class Options
{
    public int? TimeFrame { get; set; }
    public IMessageStoreOptions? MessageStoreOptions { get; set; }
    public IBrokerOptions? BrokerOptions { get; set; }
    public IRiskManagementOptions? RiskManagementOptions { get; set; }
    public IIndicatorOptions? IndicatorOptions { get; set; }
    public IStrategyOptions? StrategyOptions { get; set; }
    public IBotOptions? BotOptions { get; set; }
    public IRunnerOptions? RunnerOptions { get; set; }

    public static Options ApplyDefaults() => new()
    {
        BotOptions = new BotOptions() { TimeFrame = 180 },
        BrokerOptions = new BrokerOptions()
        {
            ApiKey = "",
            ApiSecret = "",
            BaseUrl = "open-api-vst.bingx.com",
            BrokerCommission = 0.001m,
            Symbol = "BTC-USDT",
            TimeFrame = 180
        },
        IndicatorOptions = new IndicatorOptions(),
        MessageStoreOptions = new MessageStoreOptions(),
        RiskManagementOptions = new RiskManagementOptions(),
        RunnerOptions = new RunnerOptions() { TimeFrame = 180 },
        StrategyOptions = new StrategyOptions(),
        TimeFrame = 180
    };
}
