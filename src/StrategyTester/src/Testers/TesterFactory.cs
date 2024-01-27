using bot.src.Bots;
using bot.src.Brokers;
using bot.src.Data;
using bot.src.Strategies;
using StrategyTester.src.Utils;
using StrategyTester.src.Testers.General;

namespace StrategyTester.src.Testers;

public static class TesterFactory
{
    public static ITester CreateTester(string testerName, ICandleRepository candleRepository, ITime time, IStrategy strategy, IBroker broker, IBot bot) => testerName switch
    {
        TestersNames.GENERAL => new GeneralTester(candleRepository, time, strategy, broker, bot),
        _ => throw new Exception()
    };
}
