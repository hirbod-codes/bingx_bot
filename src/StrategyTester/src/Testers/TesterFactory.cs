using bot.src.Bots;
using bot.src.Brokers;
using bot.src.Strategies;
using StrategyTester.src.Utils;
using StrategyTester.src.Testers.General;
using Serilog;

namespace StrategyTester.src.Testers;

public static class TesterFactory
{
    public static ITester CreateTester(string testerName, ITesterOptions testerOptions, ITime time, IStrategy strategy, IBroker broker, IBot bot, ILogger logger) => testerName switch
    {
        TestersNames.GENERAL => new GeneralTester(testerOptions, time, strategy, broker, bot, logger),
        _ => throw new Exception()
    };
}
